using System;
using System.Collections.Generic;
using System.Linq;
using Template10.Mvvm;
using GSCFieldApp.Models;
using GSCFieldApp.Services.DatabaseServices;
using Windows.UI.Xaml.Input;
using Windows.Storage;
using Windows.UI.Xaml;
using GSCFieldApp.Dictionaries;
using Windows.UI.Xaml.Controls;
using System.Globalization;
using System.Collections.ObjectModel;
using Windows.Media.Capture;
using System.Reflection;

namespace GSCFieldApp.ViewModels
{
    public class DocumentViewModel : ViewModelBase
    {
        #region INITIALIZATION

        //UI
        private Document documentModel = new Document();
        private readonly Station stationModel = new Station();
        private readonly EarthMaterial eartModel = new EarthMaterial();
        private readonly Sample smModel = new Sample();
        private readonly FieldLocation locationModel = new FieldLocation();
        private readonly Paleoflow pflowModel = new Paleoflow();
        private readonly Fossil fossilModel = new Fossil();
        private readonly Structure structureModel = new Structure();
        private readonly Mineral mineralModel = new Mineral();
        private readonly MineralAlteration maModel = new MineralAlteration();
        private readonly DataAccess dataAcess = new DataAccess();
        private string _description = string.Empty; //Default
        private int _documentID = 0;  //Default
        private string _documentName = string.Empty; //Default
        private string _direction = string.Empty; //Default
        private string _fileName = string.Empty; //Default
        private string _fileNumber = "1"; //Default
        private string _fileToNumber = string.Empty; //Default
        private ObservableCollection<Themes.ComboBoxItem> _docType = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedDocType = string.Empty;
        private ObservableCollection<Themes.ComboBoxItem> _category = new ObservableCollection<Themes.ComboBoxItem>();
        private ObservableCollection<Themes.ComboBoxItem> _categoryValues = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedCategory = string.Empty;
        private ObservableCollection<Themes.ComboBoxItem> _relatedTable = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedRelatedTable = string.Empty;
        private ObservableCollection<Themes.ComboBoxItem> _relatedIDs = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedRelatedID = string.Empty;
        private Visibility _documentModeVisibility = Visibility.Collapsed; //Visibility for extra fields
        private Visibility _documentUpdateVisibility = Visibility.Visible; //Visibility for fields that can't be edited when the form is poped as an edit of an existing record.
        private bool _fileNameReadOnly = true;
        private readonly string _documentPhoto = string.Empty;
        public string _documentPhotoPath = null;
        private bool _documentPhotoExists = false;
        private readonly List<string> _fileNumbers = new List<string>(); //All current file numbers in database
        private bool _fileNumberExists = false; //Will be used to track if number already exists, prevent save on dialog.

        //DB
        public bool doDocumentUpdate = false; //New records or record update
        public StorageFile _documentPhotoFile; //When user snaps a new photo
        public FieldNotes existingDataDetailDocument;
        public FieldNotes selectedStationSummaryDocument;
        public Document lastDocument;
        public DataIDCalculation idCalculatorDoc = new DataIDCalculation();
        readonly DataAccess accessData = new DataAccess();

        //Local settings
        readonly DataLocalSettings localSetting = new DataLocalSettings();

        //Events and delegate
        public delegate void documentEditEventHandler(object sender); //A delegate for execution events
        public event documentEditEventHandler newDocumentEdit; //This event is triggered when a save has been done on station table. 
        public static event EventHandler existingFileNumber; //This event is triggered when an entered file number is the same as one already in the database.

        //Process
        public bool hasInitialized = false;

        #endregion

        #region PROPERTIES

        public string Description { get { return _description; } set { _description = value; } }
        public int DocumentID { get { return _documentID; } set { _documentID = value; } }
        public string DocumentName { get { return _documentName; } set { _documentName = value; } }
        public string FileName { get { return _fileName; } set { _fileName = value; } }
        public string Direction
        {
            get
            {
                return _direction;
            }
            set
            {
                bool result = int.TryParse(value, out int index);

                if (result)
                {
                    if (index >= 0 && index < 360)
                    {
                        _direction = value;
                    }
                    else
                    {
                        _direction = value = "0";
                        RaisePropertyChanged("Direction");
                    }

                }
                else
                {
                    _direction = value = "0";
                    RaisePropertyChanged("Direction");
                }


            }
        }
        public string FileNumber
        {
            get
            {
                return _fileNumber;
            }
            set
            {

                bool result = int.TryParse(value, out int indexFrom);

                if (result)
                {

                    if (indexFrom > 0 && indexFrom <= 10000) //Some dudes have lots of photos/documents...
                    {
                        _fileNumber = value;
                    }
                    else
                    {
                        _fileNumber = value = "1";
                        RaisePropertyChanged("FileNumber");
                    }

                    if (_fileNumbers.Contains(indexFrom.ToString()))
                    {
                        _fileNumberExists = true;
                        if (existingFileNumber != null)
                        {
                            existingFileNumber(this, null);
                        }
                        while (_fileNumbers.Contains(indexFrom.ToString()))
                        {
                            indexFrom++;
                        }
                        _fileNumber = value = indexFrom.ToString();
                        RaisePropertyChanged("FileNumber");
                    }
                    else
                    {
                        _fileNumberExists = false;
                    }

                    //Make sure it's lower then File Number To value
                    bool resultTo = int.TryParse(_fileToNumber, out int indexTo);
                    if (resultTo)
                    {
                        if (indexFrom <= indexTo)
                        {
                            _fileNumber = value;
                        }
                        else
                        {
                            int newFromIndex = indexTo - 1;
                            _fileNumber = value = newFromIndex.ToString();
                            RaisePropertyChanged("FileNumber");
                        }
                    }


                }
                else
                {
                    _fileNumber = value = "1";
                    RaisePropertyChanged("FileNumber");
                }


            }
        }
        public string FileToNumber
        {
            get
            {
                return _fileToNumber;
            }
            set
            {
                bool result = int.TryParse(value, out int indexTo);
                bool fromResult = int.TryParse(_fileNumber, out int indexFrom);

                if (result && fromResult)
                {

                    //Make sure it's higher then file number from value
                    if ((indexTo > 1 && indexTo <= 10000) && (indexTo > indexFrom) && (Math.Abs(indexTo - indexFrom) <= 500)) //Some dudes have lots of photos/documents...
                    {
                        _fileToNumber = value;
                    }
                    else
                    {
                        _fileToNumber = value = string.Empty;
                        RaisePropertyChanged("FileToNumber");
                    }

                }
                else
                {
                    _fileToNumber = value = string.Empty;
                    RaisePropertyChanged("FileToNumber");
                }


            }
        }
        public Visibility DocumentModeVisibility { get { return _documentModeVisibility; } set { _documentModeVisibility = value; } }
        public Visibility DocumentUpdateVisibility { get { return _documentUpdateVisibility; } set { _documentUpdateVisibility = value; } }
        public bool FileNameReadOnly { get { return _fileNameReadOnly; } set { _fileNameReadOnly = value; } }
        public ObservableCollection<Themes.ComboBoxItem> DocType { get { return _docType; } set { _docType = value; } }
        public string SelectedDocType { get { return _selectedDocType; } set { _selectedDocType = value; } }

        public ObservableCollection<Themes.ComboBoxItem> Category { get { return _category; } set { _category = value; } }
        public ObservableCollection<Themes.ComboBoxItem> CategoryValues { get { return _categoryValues; } set { _categoryValues = value; } }
        public string SelectedCategory { get { return _selectedCategory; } set { _selectedCategory = value; } }
        public ObservableCollection<Themes.ComboBoxItem> RelatedTable { get { return _relatedTable; } set { _relatedTable = value; } }
        public string SelectedRelatedTable { get { return _selectedRelatedTable; } set { _selectedRelatedTable = value; } }
        public ObservableCollection<Themes.ComboBoxItem> RelatedIds { get { return _relatedIDs; } set { _relatedIDs = value; } }
        public string SelectedRelatedID { get { return _selectedRelatedID; } set { _selectedRelatedID = value; } }

        public bool DocumentPhotoExists { get { return _documentPhotoExists; } set { _documentPhotoExists = value; } }
        public string DocumentPhotoPath { get { return _documentPhotoPath; } set { _documentPhotoPath = value; } }

        #endregion

        #region METHODS
        /// <summary>
        /// Init.
        /// </summary>
        /// <param name="inDetailModel"></param>
        public DocumentViewModel(FieldNotes inDetailModel, FieldNotes stationSummaryID)
        {
            //Keep report detail
            existingDataDetailDocument = inDetailModel;
            selectedStationSummaryDocument = stationSummaryID;
            //_documentID = idCalculatorDoc.CalculateDocumentID(); //Calculate new document ID

            if (stationSummaryID.GenericID != 0)
            {
                _selectedRelatedID = stationSummaryID.GenericID.ToString(); //Init with what was selected by the user in the report
                _selectedRelatedTable = DatabaseLiterals.TableStation;

                //Calculate new document name if needed (on new document only) and set relation from report
                if (inDetailModel.GenericTableName != Dictionaries.DatabaseLiterals.TableDocument || doDocumentUpdate == false)
                {
                    _documentName = idCalculatorDoc.CalculateDocumentAlias(stationSummaryID.GenericID, stationSummaryID.GenericAliasName, 1);
                }
            }
            else if (stationSummaryID.station != null)
            {
                _selectedRelatedID = stationSummaryID.station.StationID.ToString(); //Init with what was selected by the user in the report
                _selectedRelatedTable = DatabaseLiterals.TableStation;

                //Calculate new document name if needed (on new document only) and set relation from report
                _documentName = idCalculatorDoc.CalculateDocumentAlias(stationSummaryID.station.StationID, stationSummaryID.station.StationAlias, 1);

            }

            //Init file number to last found in database (not necessarily the highest value)
            List<object> lastDocuments = GetLastDocument();
            if (lastDocuments.Count() == 1)
            {
                lastDocument = GetLastDocument()[0] as Document;

                _fileNumber = GetLastFileNumberPlusOne(lastDocument.DocumentType);
            }

            SetFieldVisibility(); //Will make visible or not some fields based on user option to see full document or photo style dialog

            //Fill comboboxes
            FillCategory();
            FillDocumentType();
            FillRelatedTable();
            FillRelatedIDs();

            RaisePropertyChanged("SelectedRelatedID");
            RaisePropertyChanged("SelectedRelatedTable");

            //Get some info for validation
            GetAllFileNumbers();
            existingFileNumber += DocumentViewModel_existingFileNumber;
        }

        /// <summary>
        /// Will show a dialog to user warning that entered file number already exists in the database and that 
        /// he should take another one.
        /// </summary>
        public async void ShowWarningExistingFileNumberAsync()
        {
            var loadLocalization = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();

            try
            {
                ContentDialog warningBookDialog = new ContentDialog()
                {
                    Title = loadLocalization.GetString("WarningTitleInvalid"),
                    Content = loadLocalization.GetString("WarningInvalidDocNumber"),
                    PrimaryButtonText = loadLocalization.GetString("GenericDialog_ButtonOK")
                };

                warningBookDialog.Style = (Style)Application.Current.Resources["WarningDialog"];
                ContentDialogResult cdr = await warningBookDialog.ShowAsync();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(warningBookDialog);
            }
            catch (Exception)
            {

            }


        }

        /// <summary>
        /// Will retrieve a list of all file numbers in the database.
        /// For validation purposes.
        /// </summary>
        public void GetAllFileNumbers()
        { 
            List<object> docTableLRaw = accessData.ReadTable(documentModel.GetType(), null);
            IEnumerable<Document> docTable = docTableLRaw.Cast<Document>(); //Cast to proper list type
            IEnumerable<string> docs = from d in docTable select d.FileNumber.ToString();
            foreach (string ds in docs)
            {
                _fileNumbers.Add(ds);
            }

        }

        /// <summary>
        /// Will set visibility based on document or photo mode
        /// </summary>
        private void SetFieldVisibility()
        {

            if (localSetting.GetSettingValue(ApplicationLiterals.KeywordDocumentMode) != null)
            {
                if ((bool)localSetting.GetSettingValue(ApplicationLiterals.KeywordDocumentMode))
                {
                    _documentModeVisibility = Visibility.Collapsed;
                    _fileNameReadOnly = true;
                }
                else
                {
                    _documentModeVisibility = Visibility.Visible;
                    _fileNameReadOnly = false;
                }

            }
            else
            {
                _documentModeVisibility = Visibility.Collapsed;
                _fileNameReadOnly = true;
            }
            RaisePropertyChanged("DocumentModeVisibility");
            RaisePropertyChanged("FileNameReadOnly");
        }

        /// <summary>
        /// Will save the current UI information inside document table
        /// </summary>
        public bool SaveDialogInfoAsync()
        {
            //Validate if the file number exists or not. If yes don't save anything.
            if (!_fileNumberExists)
            {
                Themes.ConcatenatedCombobox concat = new Themes.ConcatenatedCombobox();

                //Get current UI information and add to model class
                documentModel.DocumentID = _documentID;
                documentModel.DocumentName = _documentName;
                documentModel.Description = _description;
                
                documentModel.FileName = _fileName;
                

                //Variable parsing
                if (_direction != String.Empty)
                {
                    documentModel.Direction = int.Parse(_direction);
                }
                if (_fileNumber != String.Empty)
                {
                    documentModel.FileNumber = int.Parse(_fileNumber);
                }

                #region COMBOBOXES
                if (SelectedCategory != null)
                {
                    documentModel.Category = SelectedCategory;
                }

                if (SelectedCategory != null)
                {
                    documentModel.Category = concat.PipeValues(_categoryValues); //process list of values so they are concatenated.
                }

                if (SelectedDocType != null)
                {
                    documentModel.DocumentType = SelectedDocType;
                }

                if (SelectedRelatedTable != null && SelectedRelatedID != null && int.Parse(SelectedRelatedID) != 0)
                {
                    documentModel.RelatedTable = SelectedRelatedTable;
                    documentModel.RelatedID = int.Parse(SelectedRelatedID);
                }
                else
                {
                    documentModel.RelatedTable = DatabaseLiterals.TableStation;
                    documentModel.RelatedID = selectedStationSummaryDocument.station.StationID;
                }
                #endregion

                #region Photos

                if (_documentPhotoFile != null)
                {
                    documentModel.FileName = _documentPhotoFile.Name;
                    documentModel.DocumentType = DatabaseLiterals.documentTableFileSuffix; //Default value from embeded cameras.
                    //documentModel.FileNumber = string.Empty; //File number can be empty since embedded photo don't need renaming
                }

                #endregion

                #region FILE NUMBER
                if (_fileToNumber != string.Empty)
                {
                    bool fromResult = int.TryParse(_fileNumber, out int fromNumber);

                    bool toResult = int.TryParse(_fileToNumber, out int toNumber);

                    if (fromResult && toResult)
                    {
                        List<object> docList = new List<object>();
                        int totalIteration = fromNumber + (toNumber - fromNumber);
                        int iteratedFileNumber = fromNumber;
                        int currentIteration = 1;
                        while (iteratedFileNumber <= totalIteration)
                        {
                            _fileNumber = iteratedFileNumber.ToString();

                            Document newDoc = new Document
                            {
                                //DocumentID = _documentID = idCalculatorDoc.CalculateDocumentID(),
                                FileNumber = iteratedFileNumber,
                                FileName = _fileName = CalculateFileName(),
                                DocumentName = _documentName = idCalculatorDoc.CalculateDocumentAlias(selectedStationSummaryDocument.GenericID, selectedStationSummaryDocument.GenericAliasName, currentIteration),

                                Category = documentModel.Category,
                                Description = documentModel.Description,
                                Direction = documentModel.Direction,
                                DocumentType = documentModel.DocumentType,
                                RelatedID = documentModel.RelatedID,
                                RelatedTable = documentModel.RelatedTable
                            };

                            docList.Add(newDoc);
                            iteratedFileNumber++;

                            currentIteration++;
                        }

                        //Save model class
                        dataAcess.BatchSaveSQLTables(docList);
                    }
                    else
                    {
                        //Save model class
                        object docObject = (object)documentModel;
                        dataAcess.SaveFromSQLTableObject(ref docObject, doDocumentUpdate);
                        documentModel = (Document)docObject;
                        //dataAcess.SaveFromSQLTableObject(documentModel, doDocumentUpdate);
                    }
                }
                else
                {
                    //Save model class
                    object docObject = (object)documentModel;
                    dataAcess.SaveFromSQLTableObject(ref docObject, doDocumentUpdate);
                    documentModel = (Document)docObject;
                    //dataAcess.SaveFromSQLTableObject(documentModel, doDocumentUpdate);
                }

                #endregion

                //Launch an event call for everyone that an earthmat has been edited.
                if (newDocumentEdit != null)
                {
                    newDocumentEdit(this);
                }

                return true;
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// Will return a list of records being the last entered document, based on document number,
        /// which is incremential
        /// </summary>
        /// <returns></returns>
        public List<object> GetLastDocument()
        {
            string lastDocumentQueryFrom = "SELECT * FROM " + Dictionaries.DatabaseLiterals.TableDocument + " ";
            string lastDocumentQueryWhere = "WHERE " + DatabaseLiterals.TableDocument + "." + DatabaseLiterals.FieldDocumentType + " <> '' ";
            string lastDocumentQueryOrderBy = "ORDER BY " + Dictionaries.DatabaseLiterals.TableDocument + "." + Dictionaries.DatabaseLiterals.FieldDocumentName + " DESC LIMIT 1";

            string lastDocumentQuery = lastDocumentQueryFrom + lastDocumentQueryWhere + lastDocumentQueryOrderBy;
            return dataAcess.ReadTable(documentModel.GetType(), lastDocumentQuery);
        }

        /// <summary>
        /// Special procedure for olypmus camera type
        /// </summary>
        /// <param name="inPrefix"></param>
        /// <returns></returns>
        private string GetOlympusPrefix()
        {

            //Get station visit date
            string dateQuerySelect = "SELECT " + DatabaseLiterals.TableStation + "." + DatabaseLiterals.FieldStationVisitDate;
            string dateQueryFrom = " FROM " + DatabaseLiterals.TableStation;
            string dateQueryWhere = " WHERE " + DatabaseLiterals.TableStation + "." + DatabaseLiterals.FieldStationID + " = '";

            if (existingDataDetailDocument.ParentTableName == Dictionaries.DatabaseLiterals.TableStation)
            {
                dateQueryWhere = dateQueryWhere + existingDataDetailDocument.ParentID + "'";
            }
            else if (existingDataDetailDocument.GenericTableName == Dictionaries.DatabaseLiterals.TableStation)
            {
                dateQueryWhere = dateQueryWhere + existingDataDetailDocument.station.StationID + "'";
            }
            else
            {
                //TODO find station visite date in some sort of way....
            }
            Station relatedStation = dataAcess.ReadTable(stationModel.GetType(), dateQuerySelect + dateQueryFrom + dateQueryWhere)[0] as Station;
            bool parsed = DateTime.TryParseExact(relatedStation.StationVisitDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime outStationVisiteDate);
            string strMonth = "Error";
            string strDay = "Error";

            if (outStationVisiteDate != null && parsed)
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


            return strMonth + strDay;
        }

        /// <summary>
        /// Will calculate file name based on selected document type and entered file number (From)
        /// </summary>
        private string CalculateFileName()
        {
            _fileName = string.Empty;
            string _noOlympusFileNumber = string.Empty;
            RaisePropertyChanged("FileName");

            //Get suffix and prefix
            string[] splitedDoc = SelectedDocType.Split('.');

            #region PHOTO NUMBERING
            //Calculate a proper file number for photos
            if (SelectedDocType.ToLower().Contains("jpg"))
            {
                SetFileNumberAsString();

                //Special procedure for Olympus camera
                if (splitedDoc[0].ToLower().Contains("p"))
                {
                    _noOlympusFileNumber = GetOlympusPrefix() + _fileNumber;
                }
                else
                {
                    _noOlympusFileNumber = _fileNumber;
                }
            }
            #endregion

            //Calculate file name and update UI
            if (splitedDoc.Count() == 1)
            {
                _fileName = _noOlympusFileNumber + "." + splitedDoc[0];
            }
            else if (splitedDoc.Count() == 2)
            {
                _fileName = splitedDoc[0] + _noOlympusFileNumber + "." + splitedDoc[1];
            }

            RaisePropertyChanged("FileName");

            return _fileName;
        }

        /// <summary>
        /// Will reset file number as a string number with padded 0.
        /// </summary>
        /// <returns></returns>
        private void SetFileNumberAsString()
        {
            switch (_fileNumber.Length)
            {
                case 1:
                    _fileNumber = "000" + _fileNumber;
                    break;
                case 2:
                    _fileNumber = "00" + _fileNumber;
                    break;
                case 3:
                    _fileNumber = "0" + _fileNumber;
                    break;
                case 4:
                    _fileNumber = "" + _fileNumber;
                    break;
            }
        }

        /// <summary>
        /// Force a cascade delete if user get's out of sample dialog while in quick sample mode.
        /// </summary>
        /// <param name="inParentModel"></param>
        public void DeleteCascadeOnQuickPhoto(FieldNotes inParentModel)
        {
            //Get the location id
            Station stationModel = new Station();
            List<object> stationTableLRaw = accessData.ReadTable(stationModel.GetType(), null);
            IEnumerable<Station> stationTable = stationTableLRaw.Cast<Station>(); //Cast to proper list type
            IEnumerable<int> stats = from s in stationTable where s.StationID == inParentModel.ParentID select s.LocationID;
            List<int> locationFromStat = stats.ToList();

            //Delete location
            accessData.DeleteRecord(Dictionaries.DatabaseLiterals.TableLocation, Dictionaries.DatabaseLiterals.FieldLocationID, locationFromStat[0]);
        }

        /// <summary>
        /// Will delete the temp snapshot from the local state folder
        /// </summary>
        public async void DeleteCapturePhoto()
        {
            // Make sure not to delete a real picture, but the current snapshot only
            if (_documentPhotoFile!=null && !_documentPhotoPath.Contains(_documentPhotoFile.Path))
            {
                await _documentPhotoFile.DeleteAsync();
            }

        }

        #endregion

        #region FILL

        /// <summary>
        /// Will fill the document type combobox
        /// </summary>
        private void FillCategory()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldDocumentCategory;
            string tableName = Dictionaries.DatabaseLiterals.TableDocument;
            _category = new ObservableCollection<Themes.ComboBoxItem>(accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedCategory));

            //Update UI
            RaisePropertyChanged("Category");
            RaisePropertyChanged("SelectedCategory");

            
        }

        /// <summary>
        /// Will fill the document type combobox
        /// </summary>
        private void FillDocumentType()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldDocumentType;
            string tableName = Dictionaries.DatabaseLiterals.TableDocument;
            _docType = new ObservableCollection<Themes.ComboBoxItem>(accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedDocType));

            //Update UI
            RaisePropertyChanged("DocType");

            //Manage last document information
            if (lastDocument != null && !doDocumentUpdate)
            {
                _selectedDocType = lastDocument.DocumentType;
            }

            RaisePropertyChanged("SelectedDocType");

        }

        /// <summary>
        /// Will fill the document type combobox
        /// </summary>
        private void FillRelatedTable()
        {
            //Fill only if needed
            if (_documentModeVisibility == Visibility.Visible)
            {
                //Init.
                string fieldName = Dictionaries.DatabaseLiterals.FieldDocumentRelatedtable;
                string tableName = Dictionaries.DatabaseLiterals.TableDocument;
                _relatedTable = new ObservableCollection<Themes.ComboBoxItem>(accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedRelatedTable));

                //Update UI
                RaisePropertyChanged("RelatedTable");
                RaisePropertyChanged("SelectedRelatedTable");

            }


        }

        /// <summary>
        /// Will fill the document related ids combobox based on selected related table
        /// </summary>
        private void FillRelatedIDs()
        {
            //Fill only if needed
            if (_documentModeVisibility == Visibility.Visible)
            {

                _relatedIDs.Clear();
                RaisePropertyChanged("RelatedIds");


                //Get the right station id wheter it's coming from the report or the map page as quick photo
                int processedStationID = 0;
                if (selectedStationSummaryDocument.GenericTableName == DatabaseLiterals.TableStation || selectedStationSummaryDocument.GenericID != 0)
                {
                    processedStationID = selectedStationSummaryDocument.GenericID;
                }
                else if (selectedStationSummaryDocument.ParentTableName == DatabaseLiterals.TableStation || selectedStationSummaryDocument.ParentID != 0)
                {
                    processedStationID = selectedStationSummaryDocument.ParentID;
                }
                if (selectedStationSummaryDocument.station.StationID != 0 && selectedStationSummaryDocument.station.StationID != 0)
                {
                    processedStationID = selectedStationSummaryDocument.station.StationID;
                }

                if (_selectedRelatedTable != null && _selectedRelatedTable != string.Empty)
                {

                    if (_selectedRelatedTable == DatabaseLiterals.TableStation)
                    {
                        string filterStations = "Select * from " + DatabaseLiterals.TableStation + " where " + DatabaseLiterals.TableStation + "." + DatabaseLiterals.FieldStationID + " = " + processedStationID;
                        List<object> relatedStations = dataAcess.ReadTable(stationModel.GetType(), filterStations);
                        IEnumerable<Station> statTables = relatedStations.Cast<Station>();
                        foreach (Station sts in statTables)
                        {
                            Themes.ComboBoxItem newItem = new Themes.ComboBoxItem
                            {
                                itemValue = sts.StationID.ToString(),
                                itemName = sts.StationAlias
                            };
                            _relatedIDs.Add(newItem);
                        }
                        RaisePropertyChanged("RelatedIds");

                        //Select first entry
                        _selectedRelatedID = _relatedIDs.First().itemName;
                        RaisePropertyChanged("SelectedRelatedID");
                    }

                    if (_selectedRelatedTable == DatabaseLiterals.TableEarthMat)
                    {
                        string filterEarthmats = "Select * from " + DatabaseLiterals.TableEarthMat + " where " + DatabaseLiterals.TableEarthMat + "." + DatabaseLiterals.FieldEarthMatStatID + " = " + processedStationID ;
                        List<object> relatedEarths = dataAcess.ReadTable(eartModel.GetType(), filterEarthmats);
                        IEnumerable<EarthMaterial> earths = relatedEarths.Cast<EarthMaterial>();
                        foreach (EarthMaterial ea in earths)
                        {
                            Themes.ComboBoxItem newItem = new Themes.ComboBoxItem
                            {
                                itemValue = ea.EarthMatID.ToString(),
                                itemName = ea.EarthMatName
                            };
                            _relatedIDs.Add(newItem);
                        }
                        RaisePropertyChanged("RelatedIds");
                    }

                    if (_selectedRelatedTable == DatabaseLiterals.TableLocation)
                    {
                        string filterLocations = "Select * from " + DatabaseLiterals.TableLocation + " where " + DatabaseLiterals.TableLocation + "." + DatabaseLiterals.FieldLocationID + " = " + selectedStationSummaryDocument.ParentID;
                        List<object> relatedLocations = dataAcess.ReadTable(locationModel.GetType(), filterLocations);
                        IEnumerable<FieldLocation> locs = relatedLocations.Cast<FieldLocation>();
                        foreach (FieldLocation lc in locs)
                        {
                            Themes.ComboBoxItem newItem = new Themes.ComboBoxItem
                            {
                                itemValue = lc.LocationID.ToString(),
                                itemName = lc.LocationID.ToString() //Alias isn't filled.
                            };
                            _relatedIDs.Add(newItem);
                        }
                        RaisePropertyChanged("RelatedIds");
                    }

                    if (_selectedRelatedTable == DatabaseLiterals.TableSample)
                    {
                        string filterSamplesSelectJoin = "Select * from " + DatabaseLiterals.TableSample + " join " + DatabaseLiterals.TableEarthMat; 
                        string filterSamplesWhere =  " on " + DatabaseLiterals.TableSample + "." + DatabaseLiterals.FieldSampleEarthmatID + " = " + DatabaseLiterals.TableEarthMat + "." + DatabaseLiterals.FieldEarthMatID + " where " + DatabaseLiterals.TableEarthMat + "." + DatabaseLiterals.FieldEarthMatStatID + " = " + processedStationID;
                        List<object> relatedSamples = dataAcess.ReadTable(smModel.GetType(), filterSamplesSelectJoin + filterSamplesWhere);
                        IEnumerable<Sample> sms = relatedSamples.Cast<Sample>();
                        foreach (Sample sm in sms)
                        {
                            Themes.ComboBoxItem newItem = new Themes.ComboBoxItem
                            {
                                itemValue = sm.SampleID.ToString(),
                                itemName = sm.SampleName
                            };
                            _relatedIDs.Add(newItem);
                        }
                        RaisePropertyChanged("RelatedIds");
                    }

                    if (_selectedRelatedTable == DatabaseLiterals.TablePFlow)
                    {
                        string filterPflowSelectJoin = "Select * from " + DatabaseLiterals.TablePFlow + " join " + DatabaseLiterals.TableEarthMat;
                        string filterPflowWhere = " on " + DatabaseLiterals.TablePFlow + "." + DatabaseLiterals.FieldPFlowParentID + " = " + DatabaseLiterals.TableEarthMat + "." + DatabaseLiterals.FieldEarthMatID + " where " + DatabaseLiterals.TableEarthMat + "." + DatabaseLiterals.FieldEarthMatStatID + " = " + processedStationID ;
                        List<object> relatedPflow = dataAcess.ReadTable(pflowModel.GetType(), filterPflowSelectJoin + filterPflowWhere);
                        IEnumerable<Paleoflow> pfs = relatedPflow.Cast<Paleoflow>();
                        foreach (Paleoflow pf in pfs)
                        {
                            Themes.ComboBoxItem newItem = new Themes.ComboBoxItem
                            {
                                itemValue = pf.PFlowID.ToString(),
                                itemName = pf.PFlowName
                            };
                            _relatedIDs.Add(newItem);
                        }
                        RaisePropertyChanged("RelatedIds");
                    }
                    if (_selectedRelatedTable == DatabaseLiterals.TableFossil)
                    {
                        string filterFossilSelectJoin = "Select * from " + DatabaseLiterals.TableFossil + " join " + DatabaseLiterals.TableEarthMat;
                        string filterFossilWhere = " on " + DatabaseLiterals.TableFossil + "." + DatabaseLiterals.FieldFossilParentID + " = " + DatabaseLiterals.TableEarthMat + "." + DatabaseLiterals.FieldEarthMatID + " where " + DatabaseLiterals.TableEarthMat + "." + DatabaseLiterals.FieldEarthMatStatID + " = " + processedStationID ;
                        List<object> relatedFossil = dataAcess.ReadTable(fossilModel.GetType(), filterFossilSelectJoin + filterFossilWhere);
                        IEnumerable<Fossil> fss = relatedFossil.Cast<Fossil>();
                        foreach (Fossil fs in fss)
                        {
                            Themes.ComboBoxItem newItem = new Themes.ComboBoxItem
                            {
                                itemValue = fs.FossilID.ToString(),
                                itemName = fs.FossilIDName
                            };
                            _relatedIDs.Add(newItem);
                        }
                        RaisePropertyChanged("RelatedIds");
                    }
                    if (_selectedRelatedTable == DatabaseLiterals.TableStructure)
                    {
                        string filterStructureSelectJoin = "Select * from " + DatabaseLiterals.TableStructure + " join " + DatabaseLiterals.TableEarthMat;
                        string filterStructureWhere = " on " + DatabaseLiterals.TableStructure + "." + DatabaseLiterals.FieldStructureParentID + " = " + DatabaseLiterals.TableEarthMat + "." + DatabaseLiterals.FieldEarthMatID + " where " + DatabaseLiterals.TableEarthMat + "." + DatabaseLiterals.FieldEarthMatStatID + " = " + processedStationID ;
                        List<object> relatedFossil = dataAcess.ReadTable(structureModel.GetType(), filterStructureSelectJoin + filterStructureWhere);
                        IEnumerable<Structure> sts = relatedFossil.Cast<Structure>();
                        foreach (Structure st in sts)
                        {
                            Themes.ComboBoxItem newItem = new Themes.ComboBoxItem
                            {
                                itemValue = st.StructureID.ToString(),
                                itemName = st.StructureName
                            };
                            _relatedIDs.Add(newItem);
                        }
                        RaisePropertyChanged("RelatedIds");
                    }

                    if (_selectedRelatedTable == DatabaseLiterals.TableMineral)
                    {
                        string filterMineralSelectJoin = "Select * from " + DatabaseLiterals.TableMineral + " join " + DatabaseLiterals.TableEarthMat;
                        string filterMineralWhere = " on " + DatabaseLiterals.TableMineral + "." + DatabaseLiterals.FieldMineralEMID + " = " + DatabaseLiterals.TableEarthMat + "." + DatabaseLiterals.FieldEarthMatID + " where " + DatabaseLiterals.TableEarthMat + "." + DatabaseLiterals.FieldEarthMatStatID + " = " + processedStationID ;
                        List<object> relatedMineral = dataAcess.ReadTable(mineralModel.GetType(), filterMineralSelectJoin + filterMineralWhere);
                        IEnumerable<Mineral> minerals = relatedMineral.Cast<Mineral>();
                        foreach (Mineral ms in minerals)
                        {
                            Themes.ComboBoxItem newItem = new Themes.ComboBoxItem
                            {
                                itemValue = ms.MineralID.ToString(),
                                itemName = ms.MineralIDName
                            };
                            _relatedIDs.Add(newItem);
                        }
                        RaisePropertyChanged("RelatedIds");
                    }
                    if (_selectedRelatedTable == DatabaseLiterals.TableMineralAlteration)
                    {
                        string filterMASelectJoin = "Select * from " + DatabaseLiterals.TableMineralAlteration + " join " + DatabaseLiterals.TableStation;
                        string filterMAWhere = " on " + DatabaseLiterals.TableMineralAlteration + "." + DatabaseLiterals.FieldMineralAlterationRelID + " = " + DatabaseLiterals.TableStation + "." + DatabaseLiterals.FieldStationID + " where " + DatabaseLiterals.TableStation + "." + DatabaseLiterals.FieldStationID + " = " + processedStationID;
                        List<object> relatedMA = dataAcess.ReadTable(maModel.GetType(), filterMASelectJoin + filterMAWhere);
                        IEnumerable<MineralAlteration> mineralizationAlterations = relatedMA.Cast<MineralAlteration>();
                        foreach (MineralAlteration ma in mineralizationAlterations)
                        {
                            Themes.ComboBoxItem newItem = new Themes.ComboBoxItem
                            {
                                itemValue = ma.MAID.ToString(),
                                itemName = ma.MAName
                            };
                            _relatedIDs.Add(newItem);
                        }
                        RaisePropertyChanged("RelatedIds");
                    }
                }

                RaisePropertyChanged("SelectedRelatedID");

            }
        }

        /// <summary>
        /// Will fill the dialog with existing information coming from the database.
        /// </summary>
        /// <param name="incomingData">The model in which the existing information is stored.</param>
        public async void AutoFillDialogAsync(FieldNotes incomingData)
        {

            doDocumentUpdate = true;

            //Keep
            existingDataDetailDocument = incomingData;

            //Set
            _documentID = existingDataDetailDocument.document.DocumentID;
            _documentName = existingDataDetailDocument.document.DocumentName;
            _description = existingDataDetailDocument.document.Description;
            _direction = existingDataDetailDocument.document.Direction.ToString();
            _fileName = existingDataDetailDocument.document.FileName;
            _fileNumber = existingDataDetailDocument.document.FileNumber.ToString();
            _documentPhotoPath = existingDataDetailDocument.document.PhotoPath;

            _selectedRelatedID = existingDataDetailDocument.document.RelatedID.ToString();
            //_selectedCategory = existingDataDetailDocument.document.Category;
            _selectedDocType = existingDataDetailDocument.document.DocumentType;
            _selectedRelatedTable = existingDataDetailDocument.document.RelatedTable;

            //Create thumbnail if needed
            if (existingDataDetailDocument.document.PhotoFileExists)
            {
                //Create storage file of photo if needed
                Services.DatabaseServices.DataLocalSettings localSetting = new Services.DatabaseServices.DataLocalSettings();
                string _fieldbookPath = localSetting.GetSettingValue(Dictionaries.ApplicationLiterals.KeywordFieldProject).ToString();
                StorageFolder fieldBookFolder = await StorageFolder.GetFolderFromPathAsync(_fieldbookPath);
                _documentPhotoFile = await fieldBookFolder.GetFileAsync(DocumentName + ".jpg");
                _documentPhotoExists = true;
            }
            else
            {
                _documentPhotoExists = false;
                _documentPhotoPath = string.Empty;
            }

            //Concatenated box
            Themes.ConcatenatedCombobox ccBox = new Themes.ConcatenatedCombobox();
            foreach (string d in ccBox.UnpipeString(existingDataDetailDocument.document.Category))
            {
                AddAConcatenatedValue(d, "Category");
            }

            //Update UI
            RaisePropertyChanged("DocumentID");
            RaisePropertyChanged("DocumentName");
            RaisePropertyChanged("Description");
            RaisePropertyChanged("Direction");
            RaisePropertyChanged("DocumentType");
            RaisePropertyChanged("FileName");
            RaisePropertyChanged("FileNumber");
            RaisePropertyChanged("DocumentNumber");

            //RaisePropertyChanged("SelectedCategory");
            RaisePropertyChanged("SelectedDocType");

            if (_documentModeVisibility == Visibility.Visible)
            {
                RaisePropertyChanged("SelectedRelatedTable");
                RaisePropertyChanged("SelectedRelatedID");
            }


            RaisePropertyChanged("DocumentPhotoPath");
            RaisePropertyChanged("DocumentPhotoExists");

            //Disable "To" textbox so user doesn't enter anything there
            _documentUpdateVisibility = Visibility.Collapsed;
            RaisePropertyChanged("DocumentUpdateVisibility");
        }

        /// <summary>
        /// Will activate camera of the device so user can take a photo. Will then post a thumbnail
        /// inside dialog.
        /// </summary>
        public async void TakeSnapshotAsync()
        {

            //Validate existing picture and bitmap, reset if needed 
            if (_documentPhotoFile != null)
            {

                DeleteCapturePhoto();
                _documentPhotoPath = string.Empty;
                _documentPhotoExists = false;
                RaisePropertyChanged("DocumentPhotoPath");
                RaisePropertyChanged("DocumentPhotoExists");
            }

            //Capture a photo
            CameraCaptureUI captureUI = new CameraCaptureUI();
            captureUI.PhotoSettings.AllowCropping = false;
            captureUI.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
            StorageFile photo = await captureUI.CaptureFileAsync(CameraCaptureUIMode.Photo);

            if (photo == null)
            {
                return;
            }

            //Make a copy of the photo inside the field book folder only if it doesn't already exists
            if (localSetting.GetSettingValue(Dictionaries.ApplicationLiterals.KeywordFieldProject) != null)
            {
                string _fieldbookPath = localSetting.GetSettingValue(Dictionaries.ApplicationLiterals.KeywordFieldProject).ToString();
                StorageFolder fieldBookFolder = await StorageFolder.GetFolderFromPathAsync(_fieldbookPath);
                StorageFile fieldBookPhoto = await photo.CopyAsync(fieldBookFolder, DocumentName + ".jpg", NameCollisionOption.ReplaceExisting);

                //Update UI
                _documentPhotoFile = fieldBookPhoto;
                _documentPhotoPath = fieldBookPhoto.Path;
                _documentPhotoExists = true;
                RaisePropertyChanged("DocumentPhotoPath");
                RaisePropertyChanged("DocumentPhotoExists");
            }
            else
            {
                return;
            }

        }

        #endregion

        #region EVENTS
        /// <summary>
        /// Will be triggered when user wants to get the last entered caption from database
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void LoadPreviousCaptionButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //Get last entered row
            List<object> previousRecordRaw = GetLastDocument();
            IEnumerable<Document> previousRecord = previousRecordRaw.Cast<Document>();

            //Get caption
            foreach (Document docs in previousRecord)
            {
                _description = docs.Description;
                RaisePropertyChanged("Description");
                break;
            }
        }

        /// <summary>
        /// Whenever the user selects a new document type, recalculate the file name
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DocumentTypeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedDocType != null)
            {
                if (hasInitialized)
                {
                    ComboBox senderBox = sender as ComboBox;
                    Themes.ComboBoxItem senderSelectedItem = senderBox.SelectedItem as Themes.ComboBoxItem;

                    _fileNumber = GetLastFileNumberPlusOne(senderSelectedItem.itemValue);
                    
                    SetFileNumberAsString();
                    RaisePropertyChanged("FileNumber");
                }

                CalculateFileName();
            }
        }

        /// <summary>
        /// Whenever user enters a new file from number, recalculate the file name
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DocumentFromNumberTextbox_TextChanged(object sender, TextBoxTextChangingEventArgs e)
        {
            //Update file number
            TextBox senderBox = sender as TextBox;
            _fileNumber = senderBox.Text;
            if (_fileNumber != string.Empty && !doDocumentUpdate)
            {
                CalculateFileName();
            }
        }

        public void DocumentParentThemeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox senderCbox = sender as ComboBox;
            _selectedRelatedTable = senderCbox.SelectedValue.ToString();

            //Refresh ids.
            FillRelatedIDs();
        }

        /// <summary>
        /// An event thrown when a file number entered by user was already entered in the database.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DocumentViewModel_existingFileNumber(object sender, EventArgs e)
        {
            ShowWarningExistingFileNumberAsync();
        }

        public string GetLastFileNumberPlusOne(string documentTypeName)
        {
            //Variables
            string tempFileNumber = string.Empty;

            //Reset numbering system to last photo number
            string querySelect = "SELECT " + DatabaseLiterals.TableDocument + "." + DatabaseLiterals.FieldDocumentFileNo + " ";
            string queryFrom = "FROM " + DatabaseLiterals.TableDocument + " ";
            string queryWhere = "WHERE " + DatabaseLiterals.TableDocument + "." + DatabaseLiterals.FieldDocumentType + " = '" + documentTypeName + "' ";

            if (documentTypeName == string.Empty)
            {
                queryWhere = "WHERE " + DatabaseLiterals.TableDocument + "." + DatabaseLiterals.FieldDocumentType + " <> '' ";
            }
        
            string queryWhere2 = "AND " + DatabaseLiterals.TableDocument + "." + DatabaseLiterals.FieldDocumentFileNo + " <> '' ";
            string queryOrder = "ORDER BY " + DatabaseLiterals.TableDocument + "." + DatabaseLiterals.FieldDocumentFileNo + " DESC LIMIT 1";
            string lastFileNoquery = querySelect + queryFrom + queryWhere + queryWhere2 + queryOrder;

            List<object> lastFileNumbers = dataAcess.ReadTable(documentModel.GetType(), lastFileNoquery);
            if (lastFileNumbers.Count == 0)
            {
                tempFileNumber = "1";
            }
            else
            {
                IEnumerable<Document> docNumbers = lastFileNumbers.Cast<Document>();
                foreach (Document dNo in docNumbers)
                {
                    try
                    {
                        tempFileNumber = (Convert.ToInt32(dNo.FileNumber) + 1).ToString();
                    }
                    catch (Exception)
                    {
                        //Do nothing, it might be empty
                    }
                    
                }
            }

            return tempFileNumber;
        }

        #endregion

        #region CONCATENATED FIELDS

        /// <summary>
        /// Will remove a category
        /// </summary>
        /// <param name="inPurpose"></param>
        public void RemoveSelectedValue(object inPurpose, string parentListViewName)
        {

            Themes.ComboBoxItem oldValue = inPurpose as Themes.ComboBoxItem;

            if (parentListViewName.ToLower().Contains(Dictionaries.DatabaseLiterals.FieldDocumentCategory.ToLower()))
            {
                _categoryValues.Remove(oldValue);
                RaisePropertyChanged("CategoryValues");
            }

        }

        /// <summary>
        /// Will refresh the concatenated part of the purpose whenever a value is selected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ConcatComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox senderBox = sender as ComboBox;
            if (senderBox.SelectedValue != null)
            {
                AddAConcatenatedValue(senderBox.SelectedValue.ToString(), senderBox.Name);
            }

        }

        /// <summary>
        /// Will add to the list of purposes a selected purpose by the user.
        /// </summary>
        /// <param name="fieldName"> Optional, database table field name to know which collection to update</param>
        /// <param name="parentComboboxName">Optional, parent combobox name in which a selected value will be appended to the list</param>
        public void AddAConcatenatedValue(string valueToAdd, string parentComboboxName = null, string fieldName = null, bool canRemove = true)
        {
            if (valueToAdd != null && valueToAdd != String.Empty)
            {
                //Create new cbox item
                Themes.ComboBoxItem newValue = new Themes.ComboBoxItem();
                newValue.itemValue = valueToAdd;

                //Set visibility
                if (canRemove)
                {
                    newValue.canRemoveItem = Windows.UI.Xaml.Visibility.Visible;
                }
                else
                {
                    newValue.canRemoveItem = Windows.UI.Xaml.Visibility.Collapsed;
                }


                #region Find parent collection
                ObservableCollection<Themes.ComboBoxItem> parentCollection = new ObservableCollection<Themes.ComboBoxItem>();
                ObservableCollection<Themes.ComboBoxItem> parentConcatCollection = new ObservableCollection<Themes.ComboBoxItem>();
                List<Themes.ComboBoxItem> parentList = new List<Themes.ComboBoxItem>();

                string parentProperty = string.Empty;

                string NameToValidate = string.Empty;
                if (parentComboboxName != null)
                {
                    NameToValidate = parentComboboxName;
                }
                if (fieldName != null)
                {
                    NameToValidate = fieldName;
                }

                if (NameToValidate.ToLower().Contains(Dictionaries.DatabaseLiterals.FieldDocumentCategory.ToLower()))
                {
                    parentCollection = Category;
                    parentConcatCollection = _categoryValues;
                    parentProperty = "Category";

                }

                #endregion


                //Find itemName from itemValue in parent collection
                if (parentCollection != null)
                {
                    foreach (Themes.ComboBoxItem cb in parentCollection)
                    {
                        if (cb.itemValue == valueToAdd || cb.itemName == valueToAdd)
                        {
                            newValue.itemName = cb.itemName;
                            newValue.itemValue = cb.itemValue;
                            break;
                        }
                    }
                }

                //Update collection
                if (newValue.itemName != null && newValue.itemName != string.Empty && newValue.itemName != Dictionaries.DatabaseLiterals.picklistNADescription)
                {
                    bool foundValue = false;
                    foreach (Themes.ComboBoxItem existingItems in parentConcatCollection)
                    {
                        if (valueToAdd == existingItems.itemName)
                        {
                            foundValue = true;
                        }
                    }
                    if (!foundValue)
                    {
                        parentConcatCollection.Add(newValue);
                        RaisePropertyChanged(parentProperty);
                    }

                }
            }
        }


        #endregion

    }
}
