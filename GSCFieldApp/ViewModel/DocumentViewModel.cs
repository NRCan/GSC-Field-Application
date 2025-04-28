using CommunityToolkit.Mvvm.ComponentModel;
using GSCFieldApp.Models;
using GSCFieldApp.Services.DatabaseServices;
using GSCFieldApp.Controls;
using GSCFieldApp.Views;
using GSCFieldApp.Services;
using static GSCFieldApp.Dictionaries.DatabaseLiterals;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Reflection;
using CommunityToolkit.Maui.Alerts;
using SQLite;
using System.Globalization;
using System.Security.Cryptography;
using GSCFieldApp.Dictionaries;
using Microsoft.Maui.Storage;
using System.IO;


namespace GSCFieldApp.ViewModel
{
    [QueryProperty(nameof(Station), nameof(Station))]
    [QueryProperty(nameof(Document), nameof(Document))]
    [QueryProperty(nameof(DrillHole), nameof(DrillHole))]
    public partial class DocumentViewModel: FieldAppPageHelper
    {
        //Embedded image source problems
        //https://github.com/dotnet/maui/issues/8787
        //Was fixed by using a stream instead of a file path
        //It also prevents the control from showing the previous snapshot

        #region INIT

        private Document _model = new Document();
        private ComboBox _documentCategory = new ComboBox();
        private ComboBox _documentScale = new ComboBox();
        private ComboBox _documentFileType = new ComboBox();
        private int _fileNumberTo = 0; //Will be used to calculate external camera ending numbering value
        public bool IsProcessingBatch = false; //Will be used to block picklist from being refilled when processed in abtch
        private ImageSource _snapshotSource = null;
        public bool imageTapped = false; //Will track user edits on snapshot

        //Concatenated
        private ComboBoxItem _selectedDocumentCategory = new ComboBoxItem();
        private ObservableCollection<ComboBoxItem> _categoryCollection = new ObservableCollection<ComboBoxItem>();


        #endregion

        #region PROPERTIES

        [ObservableProperty]
        private Station _station;

        [ObservableProperty]
        private Document _document;

        [ObservableProperty]
        private DrillHole _drillHole;

        public Document Model { get { return _model; } set { _model = value; } }

        public bool DocumentDescVisibility
        {
            get { return Preferences.Get(nameof(DocumentDescVisibility), true); }
            set { Preferences.Set(nameof(DocumentDescVisibility), value); }
        }

        public bool DocumentAdvancedVisibility
        {
            get { return Preferences.Get(nameof(DocumentAdvancedVisibility), true); }
            set { Preferences.Set(nameof(DocumentAdvancedVisibility), value); }
        }

        public bool DocumentDocVisibility
        {
            get { return Preferences.Get(nameof(DocumentDocVisibility), true); }
            set { Preferences.Set(nameof(DocumentDocVisibility), value); }
        }

        public bool DocumentExternalCamVisibility
        {
            get { return Preferences.Get(nameof(DocumentExternalCamVisibility), true); }
            set { Preferences.Set(nameof(DocumentExternalCamVisibility), value); }
        }

        public bool DocumentInternalCamVisibility
        {
            get { return Preferences.Get(nameof(DocumentInternalCamVisibility), true); }
            set { Preferences.Set(nameof(DocumentInternalCamVisibility), value); }
        }

        public ComboBox DocumentCategory { get { return _documentCategory; } set { _documentCategory = value; } }
        public ObservableCollection<ComboBoxItem> DocumentCategoryCollection { get { return _categoryCollection; } set { _categoryCollection = value; OnPropertyChanged(nameof(DocumentCategoryCollection)); } }
        public ComboBoxItem SelectedDocumentCategory
        {
            get
            {
                return _selectedDocumentCategory;
            }
            set
            {
                if (_selectedDocumentCategory != value)
                {
                    if (_categoryCollection != null)
                    {
                        if (_categoryCollection.Count > 0 && _categoryCollection[0] == null)
                        {
                            _categoryCollection.RemoveAt(0);
                        }
                        if (value != null && value.itemName != string.Empty)
                        {
                            if (!_categoryCollection.Contains(value))
                            {
                                _categoryCollection.Add(value);
                            }
                            
                            _selectedDocumentCategory = value;
                            OnPropertyChanged(nameof(SelectedDocumentCategory));

                        }

                    }


                }

            }
        }

        public ComboBox DocumentScale { get { return _documentScale; } set { _documentScale = value; } }
        public ComboBox DocumentFileType { get { return _documentFileType; } set { _documentFileType = value; } }

        public int FileNumberTo { get { return _fileNumberTo; } set { _fileNumberTo = value; } }

        public ImageSource SnapshotSource { get { return _snapshotSource; } set { _snapshotSource = value; } }

        private bool InternalCameraFirstEnabled
        {
            get { return Preferences.Get(nameof(InternalCameraFirstEnabled), true); }
            set { }
        }

        #endregion

        public DocumentViewModel()
        {

            //Init new field theme
            FieldThemes = new FieldThemes();

        }

        #region RELAYS

        [RelayCommand]
        public async Task Hide(string visibilityObjectName)
        {
            //Use reflection to parse incoming block to hide
            PropertyInfo? prop = typeof(DocumentViewModel).GetProperty(visibilityObjectName);

            if (prop != null)
            {
                bool propBool = (bool)prop.GetValue(this);

                // Reverse
                propBool = propBool ? false : true;

                prop.SetValue(this, propBool);
                OnPropertyChanged(visibilityObjectName);
            }

        }
        
        [RelayCommand]
        async Task Save()
        {
            //Fill out missing values in model
            await SetModelAsync();

            //Validate if new entry or update
            if (_document != null && _document.DocumentName != string.Empty && (_model.StationID != 0 || _model.DrillHoleID != 0))
            {
                await da.SaveItemAsync(Model, true);
            }
            else if (_model.DocumentID == 0 && _model.FileNumber == _fileNumberTo)
            {
                await da.SaveItemAsync(Model, false);

            }
            else
            {
                //Insert new records as batch if needed
                int photoTaken = await Task.Run(async () => await BatchCreatePhotos());
            }

            //Exit to field notes or stay in map page
            await NavigateAfterAction(TableNames.document);

        }

        /// <summary>
        /// Save button command
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        async Task SaveStay()
        {
            //Fill out missing values in model
            await SetModelAsync();

            //Validate if new entry or update
            if (_document != null && _document.DocumentName != string.Empty && _model.DocumentID != 0)
            {
                await da.SaveItemAsync(Model, true);
            }
            else if (_model.DocumentID == 0 && _model.FileNumber == _fileNumberTo)
            {
                await da.SaveItemAsync(Model, false);

            }
            else
            {
                //Insert new records as batch if needed
                await Task.Run(async () => await BatchCreatePhotos());
            }

            //Show saved message
            await Toast.Make(LocalizationResourceManager["ToastSaveRecord"].ToString()).Show(CancellationToken.None);

            //Reset
            await ResetModelAsync();
            OnPropertyChanged(nameof(Model));
        }

        [RelayCommand]
        async Task SaveDelete()
        {
            if (_document != null && _document.DocumentID != 0)
            {
                await commandServ.DeleteDatabaseItemCommand(TableNames.document, _document.DocumentName, _document.DocumentID);
            }

            //Exit
            await NavigateAfterAction(TableNames.document);

        }

        [RelayCommand]
        public async Task Back()
        {
            //Make sure to delete station and location records if user is coming from map page
            if (_station != null && _station.IsMapPageQuick && _model.DocumentID != null)
            {
                //Delete without forced pop-up warning and question
                await commandServ.DeleteDatabaseItemCommand(TableNames.station, _station.StationAlias, _station.LocationID, true);

                //Make sure to delete captured photo if there was one.
                DeleteSnapshot();
            }
            else if (_drillHole != null && _model.DocumentID != null)
            {
                //Make sure to delete captured photo if there was one.
                DeleteSnapshot();
            }

            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        public async Task AddSnapshot()
        {
            if (MediaPicker.Default.IsCaptureSupported)
            {
                bool newSnapshot = true; //Will be used to replace snapshot with new one

                //Detect existing embedded photo for further processing in case of replacement
                if (_document != null && _document.Hyperlink != null && _document.PhotoFileExists)
                {
                    //await ResetModelAsync();
                    newSnapshot = false;
                }

                FileResult snapshot = await MediaPicker.Default.CapturePhotoAsync();
               
                if (snapshot != null)
                {
                    snapshot.FileName = _model.DocumentName + documentTableFileSuffix;

                    //Name sub folder just like the database and the json for layers
                    AppFileServices afs = new AppFileServices();
                    string subFolderPath = afs.CreatePhotoSubFolder();
                    string localFilePath = Path.Combine(FileSystem.Current.AppDataDirectory, subFolderPath, snapshot.FileName);

                    //Manage file name and possible existing files
                    if (File.Exists(localFilePath))
                    {
                        //Delete
                        try
                        {
                            File.Delete(localFilePath);
                        }
                        catch (UnauthorizedAccessException ua)
                        {
                            new ErrorToLogFile(ua).WriteToFile();
                        }
                        catch (Exception e)
                        {
                            new ErrorToLogFile(e).WriteToFile();
                        }
                        
                    }

                    using (Stream sourceStream = await snapshot.OpenReadAsync())
                    {
                        using (FileStream fileStream = File.OpenWrite(localFilePath))
                        {
                            await sourceStream.CopyToAsync(fileStream);
                        }
                    }

                    //Set model
                    _model.Hyperlink = localFilePath;
                    _model.FileName = snapshot.FileName;
                    _model.FileNumber = 0;
                    _fileNumberTo = 0;

                    try
                    {
                        OnPropertyChanged(nameof(Model));
                    }
                    catch (Exception e)
                    {
                        new ErrorToLogFile(e).WriteToFile();
                    }

                    //Save current record
                    if (newSnapshot)
                    {
                        await SaveAsNewSnapshot();
                    }

                    //Update image source
                    UpdateThumbnail(localFilePath);

                }
            }
        }

        /// <summary>
        /// Will search and load for the last non empty caption and load it
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        async Task LoadPreviousCaption()
        {
            //Get last entered row
            SQLiteAsyncConnection currentConnection = da.GetConnectionFromPath(da.PreferedDatabasePath);
            Document previousRecord = await currentConnection.Table<Document>()
                .Where(e => e.Description != null && e.Description != "")
                .OrderByDescending(d => d.Description)
                .FirstAsync();

            //Get caption
            if (previousRecord != null)
            {
                _model.Description = previousRecord.Description;
                OnPropertyChanged(nameof(Model));
            }

        }

        /// <summary>
        /// Triggered when user taps an embedded picture
        /// Will open it full screen so user can edit it
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        async Task ImageTapped()
        {
            if (File.Exists(_model.Hyperlink))
            {
                await Launcher.Default.OpenAsync(new OpenFileRequest("popoverTitle", new ReadOnlyFile(_model.Hyperlink)));

                //Force refresh of image (might have been edited by user)
                OnPropertyChanged(nameof(Model));

                imageTapped = true;
            }
            
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Initialize all pickers. 
        /// To save loading time, process only those needed based on work type
        /// </summary>
        /// <returns></returns>
        public async Task FillPickers()
        {
            if (!IsProcessingBatch)
            {
                _documentCategory = await FillAPicker(FieldDocumentCategory);
                _documentScale = await FillAPicker(FieldDocumentScaleDirection);
                _documentFileType = await FillAPicker(FieldDocumentType);

                OnPropertyChanged(nameof(DocumentCategory));
                OnPropertyChanged(nameof(DocumentScale));
                OnPropertyChanged(nameof(DocumentFileType));
            }

        }

        /// <summary>
        /// Will fill the project type combobox
        /// </summary>
        private async Task<ComboBox> FillAPicker(string fieldName)
        {
            //Make sure to user default database rather then the prefered one. This one will always be there.
            return await da.GetComboboxListWithVocabAsync(TableDocument, fieldName);

        }

        /// <summary>
        /// Will fill out missing fields for model. Default auto-calculated values
        /// Done before actually saving
        /// </summary>
        private async Task SetModelAsync()
        {

            #region Process concatenated pickers
            if (DocumentCategoryCollection != null && DocumentCategoryCollection.Count > 0)
            {
                Model.Category = ConcatenatedCombobox.PipeValues(DocumentCategoryCollection); //process list of values so they are concatenated.
            }

            #endregion

            //Make sure file number to is not lower than file number from
            CalculateFileNumberTo();

        }

        /// <summary>
        /// Will initialize the model with needed calculated fields
        /// </summary>
        /// <returns></returns>
        public async Task InitModel()
        {
            if (Model != null && Model.DocumentID == 0 && _station != null)
            {
                //Get current application version
                Model.StationID = _station.StationID;
                Model.DocumentName = await idCalculator.CalculateDocumentAliasAsync(_station.StationID, _station.StationAlias);
                Model.FileName = CalculateFileName();
                Model.FileNumber = 1;
                _fileNumberTo = 1;
                OnPropertyChanged(nameof(Model));
                OnPropertyChanged(nameof(FileNumberTo));
            }

            if (Model != null && Model.DocumentID == 0 && _drillHole != null)
            {
                //Get current application version
                Model.DrillHoleID = _drillHole.DrillID;
                Model.DocumentName = await idCalculator.CalculateDocumentAliasAsync(_drillHole.DrillID, _drillHole.DrillIDName);
                Model.FileName = CalculateFileName();
                Model.FileNumber = 1;
                _fileNumberTo = 1;
                OnPropertyChanged(nameof(Model));
                OnPropertyChanged(nameof(FileNumberTo));
            }

            //Init file number to last found in database (not necessarily the highest value)
            Document lastDocument = await GetLastDocument();
            if (lastDocument != null)
            {
                Model.DocumentType = lastDocument.DocumentType;
                Model.FileNumber = lastDocument.FileNumber + 1;
                _fileNumberTo = Model.FileNumber;
                Model.FileName = CalculateFileName();

                OnPropertyChanged(nameof(Model));
                OnPropertyChanged(nameof(FileNumberTo));

            }

        }

        /// <summary>
        /// Will refill the form with existing values for update/editing purposes
        /// </summary>
        /// <returns></returns>
        public async Task Load()
        {

            if (_document != null && _document.DocumentName != string.Empty)
            {
                //Set model like actual record
                _model = _document;

                //Refresh
                OnPropertyChanged(nameof(Model));

                #region Pickers
                //Select values in pickers
                List<string> bfs = ConcatenatedCombobox.UnpipeString(_document.Category);
                _categoryCollection.Clear(); //Clear any possible values first
                OnPropertyChanged(nameof(DocumentCategoryCollection));
                foreach (ComboBoxItem cbox in DocumentCategory.cboxItems)
                {
                    if (bfs.Contains(cbox.itemValue) && !_categoryCollection.Contains(cbox))
                    {
                        _categoryCollection.Add(cbox);
                    }
                }
                //OnPropertyChanged(nameof(DocumentCategory));
                OnPropertyChanged(nameof(DocumentCategoryCollection));

                #endregion

                //Embedded picture
                if (_model.Hyperlink != null && _model.Hyperlink != string.Empty)
                {
                    FileStream stream = File.OpenRead(_model.Hyperlink);
                    _snapshotSource = ImageSource.FromStream(() => stream);
                    OnPropertyChanged(nameof(SnapshotSource));
                }
            }

            else
            {
                // Open snapshot first if set by user as internal camera first
                if (InternalCameraFirstEnabled)
                {
                    await AddSnapshot();
                }
            }
        }

        /// <summary>
        /// Will reset model fields to default just like it's a new record
        /// </summary>
        /// <returns></returns>
        private async Task ResetModelAsync()
        {
            // if coming from field notes on a record edit that needs to be saved as a new record with stay/save

            //Reset model
            if (_station != null)
            {
                // if coming from station notes, calculate new alias
                Model.StationID = _station.StationID;
                Model.DocumentName = await idCalculator.CalculateDocumentAliasAsync(_station.StationID, _station.StationAlias);

            }
            else if (Model.StationID != null)
            {
                
                List<Station> parentAlias = await DataAccess.DbConnection.Table<Station>().Where(e => e.StationID == Model.StationID).ToListAsync();
                Model.DocumentName = await idCalculator.CalculateDocumentAliasAsync(Model.StationID.Value, parentAlias.First().StationAlias);

            }
            else if (_drillHole != null)
            {
                // if coming from station notes, calculate new alias
                Model.DrillHoleID = _drillHole.DrillID;
                Model.DocumentName = await idCalculator.CalculateDocumentAliasAsync(_drillHole.DrillID, _drillHole.DrillIDName);

            }
            else if (Model.DrillHoleID != null)
            {
                List<DrillHole> parentAlias = await DataAccess.DbConnection.Table<DrillHole>().Where(e => e.DrillID == Model.DrillHoleID).ToListAsync();
                Model.DocumentName = await idCalculator.CalculateDocumentAliasAsync(Model.DrillHoleID.Value, parentAlias.First().DrillIDName);
            }
            Model.Hyperlink = null;
            Model.FileName = CalculateFileName();
            Model.DocumentID = 0;
            Model.FileNumber = Model.FileNumber + 1;
            CalculateFileNumberTo();

        }

        /// <summary>
        /// Will calculate file name based on selected document type and entered file number (From)
        /// </summary>
        public string CalculateFileName()
        {
            //make sure to not process embedded picture (they have an hyperlink value)
            if (_model != null 
                && _model.DocumentType != null 
                && _model.Hyperlink == null)
            {
                _model.FileName = string.Empty;
                string _noOlympusFileNumber = string.Empty;

                //Get suffix and prefix
                string[] splitedDoc = _model.DocumentType.Split('.');

                #region PHOTO NUMBERING
                //Calculate a proper file number for photos
                if (_model.DocumentType.ToLower().Contains("jpg"))
                {
                    string stringFileNumber = GetFileNumberAsString();

                    //Special procedure for Olympus camera
                    if (splitedDoc[0].ToLower().Contains("p"))
                    {
                        _noOlympusFileNumber = GetOlympusPrefix() + stringFileNumber;
                    }
                    else
                    {
                        _noOlympusFileNumber = stringFileNumber;
                    }
                }
                #endregion

                //Calculate file name and update UI
                if (splitedDoc.Count() == 1)
                {
                    _model.FileName = _noOlympusFileNumber + "." + splitedDoc[0];
                }
                else if (splitedDoc.Count() == 2)
                {
                    _model.FileName = splitedDoc[0] + _noOlympusFileNumber + "." + splitedDoc[1];
                }

                OnPropertyChanged(nameof(Model));
            }

            return _model.FileName;
        }

        /// <summary>
        /// Will output a string file number as a padded 0.
        /// </summary>
        /// <returns></returns>
        private string GetFileNumberAsString()
        {
            string outputFileNumberAsString = _model.FileNumber.ToString();

            switch (_model.FileNumber.ToString().Length)
            {
                case 1:
                    outputFileNumberAsString = "000" + _model.FileNumber.ToString();
                    break;
                case 2:
                    outputFileNumberAsString = "00" + _model.FileNumber.ToString();
                    break;
                case 3:
                    outputFileNumberAsString = "0" + _model.FileNumber.ToString();
                    break;
                case 4:
                    outputFileNumberAsString = "" + _model.FileNumber.ToString();
                    break;
            }

            return outputFileNumberAsString;
        }

        /// <summary>
        /// Special procedure for olypmus camera type
        /// </summary>
        /// <returns>Returns the proper olympus prefix</returns>
        private string GetOlympusPrefix()
        {
            string strMonth = string.Empty;
            string strDay = string.Empty;

            if (_station != null && _station.StationVisitDate != string.Empty)
            {
                //Get station visit date
                bool parsed = DateTime.TryParseExact(_station.StationVisitDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime outStationVisiteDate);

                if (parsed)
                {
                    strMonth = outStationVisiteDate.Month.ToString();
                    switch (strMonth)
                    {
                        case "10":
                            strMonth = "A";
                            break;
                        case "11":
                            strMonth = "B";
                            break;
                        case "12":
                            strMonth = "C";
                            break;
                        default:
                            break;
                    }
                    //Remove 0 from months
                    strMonth = strMonth.Replace("0", "");
                    strDay = string.Format("{0:dd}", outStationVisiteDate);
                }
            }

            return strMonth + strDay;
        }

        /// <summary>
        /// Will save a batch of photos based on To # entered by user
        /// </summary>
        private async Task<int> BatchCreatePhotos()
        {
            int currentIteration = _model.FileNumber;

            if (_fileNumberTo != 0 && _fileNumberTo >= _model.FileNumber)
            {
                IsProcessingBatch = true;

                while (currentIteration <= _fileNumberTo)
                {
                    //Calculate filenumber and file name if not from embeded pictures
                    if (_model.Hyperlink == null || _model.Hyperlink == string.Empty)
                    {
                        _model.FileNumber = currentIteration;
                        _model.FileName = CalculateFileName();
                    }

                    if (_station != null)
                    {
                        ///This method of calculating the alias for each record slows down the whole process
                        //_model.DocumentName = await Task.Run(async() => await idCalculator.CalculateDocumentAliasAsync(_station.StationID, _station.StationAlias, sq));
                    }


                    OnPropertyChanged(nameof(Model));
                    await Task.Run(async() => await da.SaveItemAsync(Model, false));

                    currentIteration++;
                }

                //Batch calculat alias
                string aliasQuery_base = "UPDATE {0} SET {1} = (SELECT s.{2} FROM {3} as s where s.{4} = {8}) || {5} || {6} WHERE {6} {7} and {4} = {8}";
                string aliasQuery00 = string.Format(aliasQuery_base,
                    TableDocument, FieldDocumentName, FieldStationAlias, TableStation, FieldStationID, "'P00'", FieldDocumentFileNo, "<10", _model.StationID.ToString());
                string aliasQuery0 = string.Format(aliasQuery_base,
                    TableDocument, FieldDocumentName, FieldStationAlias, TableStation, FieldStationID, "'P0'", FieldDocumentFileNo, ">=10", _model.StationID.ToString());
                string aliasQuery = string.Format(aliasQuery_base,
                    TableDocument, FieldDocumentName, FieldStationAlias, TableStation, FieldStationID, "'P'", FieldDocumentFileNo, ">=100", _model.StationID.ToString());
                await DataAccess.DbConnection.ExecuteAsync(aliasQuery00);
                await DataAccess.DbConnection.ExecuteAsync(aliasQuery0);
                await DataAccess.DbConnection.ExecuteAsync(aliasQuery);

                IsProcessingBatch = false;
            }

            return currentIteration;


        }

        /// <summary>
        /// Triggered when user takes a new snapshot from an existing record
        /// </summary>
        /// <returns></returns>
        public async Task SaveAsNewSnapshot()
        {
            //Get back original station record
            if (_station == null)
            {
                //Go get original record
                List<Station> pRecord = await DataAccess.DbConnection.Table<Station>().Where(e => e.StationID == Model.StationID).ToListAsync();

                if (pRecord != null && pRecord.Count() > 0)
                {
                    _station = pRecord.First();

                    _model.DocumentName = await idCalculator.CalculateDocumentAliasAsync(_station.StationID, _station.StationAlias);
                    OnPropertyChanged(nameof(Model));
                    await da.SaveItemAsync(Model, false);
                }

            }
            else if (_drillHole == null)
            {
                //Go get original record
                List<DrillHole> pRecord = await DataAccess.DbConnection.Table<DrillHole>().Where(e => e.DrillID == Model.DrillHoleID).ToListAsync();

                if (pRecord != null && pRecord.Count() > 0)
                {
                    _drillHole = pRecord.First();

                    _model.DocumentName = await idCalculator.CalculateDocumentAliasAsync(_drillHole.DrillID, _drillHole.DrillIDName);
                    OnPropertyChanged(nameof(Model));
                    await da.SaveItemAsync(Model, false);
                }
            }
        }

        /// <summary>
        /// Will make sure that file number is equal to Model.FileNumber by default
        /// </summary>
        public void CalculateFileNumberTo()
        {
            if (_fileNumberTo == 0 || _fileNumberTo < _model.FileNumber)
            {
                _fileNumberTo = _model.FileNumber;
                OnPropertyChanged(nameof(FileNumberTo));
            }
            
        }

        /// <summary>
        /// Will delete the temp snapshot from the local state folder
        /// </summary>
        public void DeleteSnapshot()
        {
            // Make sure not to delete a real picture, but the current snapshot only
            if (_model != null && _model.Hyperlink != string.Empty && File.Exists(_model.Hyperlink))
            {
                File.Delete(_model.Hyperlink);
            }

        }

        /// <summary>
        /// Will return the last taken document object
        /// which is incremential
        /// </summary>
        /// <param name="includingSnapshot">Last document will include or exclude snapshot photos with embeded device</param>
        /// <returns></returns>
        public async Task<Document> GetLastDocument(bool includingSnapshot = false)
        {
            SQLiteAsyncConnection currentConnection = da.GetConnectionFromPath(da.PreferedDatabasePath);
            List<Document> previousRecord = await currentConnection.Table<Document>()
                .OrderByDescending(d => d.FileNumber).ToListAsync();

            if (includingSnapshot)
            {
                previousRecord = await currentConnection.Table<Document>()
                .OrderByDescending(d => d.DocumentName).ToListAsync();
            }

            //Get caption
            if (previousRecord.Count() >= 1)
            {
                return previousRecord.First();
            }
            else
            {
                return null;
            }

        }

        /// <summary>
        /// Will update the snapshot thumbnail in the form
        /// </summary>
        /// <param name="filePath"></param>
        public void UpdateThumbnail(string filePath = "")
        {
            //Take what's in the model first
            if (filePath == string.Empty)
            {
                filePath = Model.Hyperlink;
            }

            FileStream snapStream = File.OpenRead(filePath);
            _snapshotSource = ImageSource.FromStream(() => snapStream);
            OnPropertyChanged(nameof(SnapshotSource));

            //Unset image tapped
            imageTapped = false;
        }

        #endregion
    }
}
