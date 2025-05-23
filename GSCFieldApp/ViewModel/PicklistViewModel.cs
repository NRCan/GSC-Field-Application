﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GSCFieldApp.Models;
using GSCFieldApp.Services.DatabaseServices;
using GSCFieldApp.Controls;
using GSCFieldApp.Views;
using GSCFieldApp.Services;
using static GSCFieldApp.Dictionaries.DatabaseLiterals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using CommunityToolkit.Maui.Alerts;
using System.IO;
using System.Collections.ObjectModel;
using NetTopologySuite.Index.HPRtree;
using GSCFieldApp.Dictionaries;

namespace GSCFieldApp.ViewModel
{
    public partial class PicklistViewModel : FieldAppPageHelper
    {
        #region INIT
        private Vocabularies _model = new Vocabularies();
        private Picklist _modelPicklist = new Picklist();
        private ComboBox _picklistTables = new ComboBox();
        private ComboBox _picklistFields = new ComboBox();
        private ComboBox _picklistParents = new ComboBox();
        private List<VocabularyManager> _vocabularyManagers = new List<VocabularyManager>();
        private ObservableCollection<Vocabularies> _picklistValues = new ObservableCollection<Vocabularies>();
        private bool _isWaiting = false;

        //Issue #404
        private bool _isDoubleTap;
        private bool _ignoreNextTap;

        #endregion

        #region RELAYS
        /// <summary>
        /// Will add a new term within the database
        /// </summary>
        [RelayCommand]
        async void AddNewTerm()
        {

            string popUpTitle = LocalizationResourceManager["PicklistPageAddNewTermTitle"].ToString();
            string popUpContent = LocalizationResourceManager["PicklistPageAddNewTermContent"].ToString();
            string result = await Shell.Current.DisplayPromptAsync(popUpTitle, popUpContent);

            if (result != null && result != string.Empty)
            {
                //Trim
                result = result.Trim();

                //Set
                Vocabularies newVocab = new Vocabularies();
                newVocab.Code = result;
                newVocab.Description = result;
                newVocab.DescriptionFR = result;
                newVocab.TermID = idCalculator.CalculateGUID();
                newVocab.CodedTheme = _picklistValues.First().CodedTheme; //Steal from first item
                newVocab.Visibility = boolYes;
                newVocab.Editable = boolYes;
                newVocab.Order = 0.0; //Make sure to add in first place
                newVocab.Creator = Preferences.Get(nameof(FieldUserInfoUCode), AppInfo.Current.Name);
                newVocab.CreatorDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now);

                //Manage parent
                if (_modelPicklist.PicklistParent != null && _modelPicklist.PicklistParent != string.Empty)
                {
                    newVocab.RelatedTo = _modelPicklist.PicklistParent;
                }

                //Add
                _picklistValues.Insert(0, newVocab);
                OnPropertyChanged(nameof(PicklistValues));

                //Save
                SQLiteAsyncConnection defaultConnection = da.GetConnectionFromPath(da.DatabaseFilePath);
                await defaultConnection.InsertAsync(newVocab);
                await defaultConnection.CloseAsync();

            }

        }

        /// <summary>
        /// Will mod a term (single tap event)
        /// </summary>
        [RelayCommand]
        async void ModifyTerm(Vocabularies vocabToEdit)
        {
            if (vocabToEdit != null)
            {
#if WINDOWS
                var delay = Task.Delay(555);
                await delay;

                if (_ignoreNextTap)
                {
                    _ignoreNextTap = false;
                    
                    return;
                }

                if (_isDoubleTap)
                {
                    _isDoubleTap = false;
                    _ignoreNextTap = true;
                    SetADefaultTerm(vocabToEdit);
                }
                else
	            {
                    ModifyATerm(vocabToEdit);
	            }
#else
                ModifyATerm(vocabToEdit);
#endif


            }


        }

        /// <summary>
        /// Will force a ascending alphabetical order sort on the picklist values
        /// </summary>
        [RelayCommand]
        async void SortTerm()
        {
            if (_picklistValues != null && _picklistValues.Count > 0)
            {
                List<Vocabularies> sortedVocab = _picklistValues.OrderBy(v => v.Description).ToList();
                _picklistValues.Clear();
                double newOrder = 1.0;
                foreach (Vocabularies vs in sortedVocab)
                {
                    vs.Order = newOrder;
                    _picklistValues.Add(vs);
                    newOrder++;
                }

                OnPropertyChanged(nameof(PicklistValues));

            }

        }

        /// <summary>
        /// Will set a term to be a default selection for a picklist
        /// (Double tap event)
        /// </summary>
        /// <param name="vocabToEdit"></param>
        [RelayCommand]
        async void SetDefaultTerm(Vocabularies vocabToEdit)
        {
#if WINDOWS
            //Track double event
            _isDoubleTap = true;
#else
            SetADefaultTerm(vocabToEdit);
#endif  
        }

        [RelayCommand]
        async void Save()
        {
            if (_picklistValues != null && _picklistValues.Count() > 0)
            {
                //Get date
                string currentTime = String.Format("{0:yyyy-MM-dd}", DateTime.Now);

                //Get user geolcode
                string userCode = Preferences.Get(nameof(FieldUserInfoUCode), AppInfo.Current.Name);

                //Set order
                double iterativeOrder = 1.0;

                //Show waiting cursor
                _isWaiting = true;
                OnPropertyChanged(nameof(IsWaiting));

                //Iterate through picklist values
                SQLiteAsyncConnection saveConnection = da.GetConnectionFromPath(da.DatabaseFilePath);
                foreach (Vocabularies vocs in _picklistValues)
                {
                    //Keep some knowledge about who has done this
                    vocs.Editor = userCode;
                    vocs.EditorDate = currentTime;

                    //New order
                    vocs.Order = iterativeOrder;
                    iterativeOrder++;
                }

                await saveConnection.UpdateAllAsync(_picklistValues, true);

                OnPropertyChanged(nameof(PicklistValues));
 
                await saveConnection.CloseAsync();

                //Show saved message
                _isWaiting = false;
                OnPropertyChanged(nameof(IsWaiting));
                await Toast.Make(LocalizationResourceManager["ToastSaveRecord"].ToString()).Show(CancellationToken.None);
            }
        }

        [RelayCommand]
        async void Import()
        {
            //Make user select a geopackage to load picklist from
            FileResult geopackageLoadFrom = await PickVocabGeopackage();

            if (geopackageLoadFrom != null)
            {
                bool merged = await da.DoMergeVocab(geopackageLoadFrom.FullPath, da.DatabaseFilePath, true);

                if (merged)
                {
                    //Clean UI
                    ResetPage();

                    await Toast.Make(LocalizationResourceManager["ToastImportRecord"].ToString()).Show(CancellationToken.None);
                }
                else
                {
                    await Shell.Current.DisplayAlert(LocalizationResourceManager["GenericErrorTitle"].ToString(),
                        LocalizationResourceManager["PicklistPageImportError"].ToString(),
                        LocalizationResourceManager["GenericButtonOk"].ToString());
                }
            }
            
        }

        [RelayCommand]
        async void PicklistPageTablesPickerChanged()
        {
            //Cast and make sure something valid is selected
            if (_picklistTables.cboxDefaultItemIndex >= 0)
            {
                FillFieldsPicklist();
            }
        }

        [RelayCommand]
        async void PicklistPageFieldsPickerChanged()
        {
            //Cast and make sure something valid is selected
            if (_picklistFields.cboxDefaultItemIndex >= 0)
            {
                bool doesHaveParents = await FillFieldParentValuesPicklist();
                if (!doesHaveParents)
                {
                    //Instead fill field values
                    FillFieldValuesPicklist();
                }
            }
        }

        [RelayCommand]
        async void PicklistPageParentPickerChanged()
        {
            //Cast and make sure something valid is selected
            if (_picklistParents.cboxDefaultItemIndex >= 0)
            {
                FillFieldValuesPicklist();
            }
        }
#endregion

        #region PROPERTIES

        public Vocabularies Model { get { return _model; } set { _model = value; } }
        public Picklist ModelPicklist { get { return _modelPicklist; } set { _modelPicklist = value; } }
        public ComboBox PicklistTables { get { return _picklistTables; } set { _picklistTables = value; } }
        public ComboBox PicklistFields { get { return _picklistFields; } set { _picklistFields = value; } }
        public ComboBox PicklistParents { get { return _picklistParents; } set { _picklistParents = value; } }
        public ObservableCollection<Vocabularies> PicklistValues { get { return _picklistValues; } set { _picklistValues = value; } }
        public bool IsWaiting { get { return _isWaiting; } set { _isWaiting = value; } }
        #endregion

        public PicklistViewModel()
        {
            _ = FillPickers();

            //Detect new field book selection, uprgrade, edit, ...
            FieldBooksViewModel.newFieldBookSelected += FieldBooksViewModel_newFieldBookSelectedAsync;
        }

        #region METHODS

        /// <summary>
        /// Will fill all picker controls
        /// TODO: make sure this whole thing doesn't slow too much form rendering
        /// </summary>
        /// <returns></returns>
        public async Task FillPickers()
        {
            //Get the current project type
            string fieldworkType = ApplicationThemeBedrock; //Default

            if (Preferences.ContainsKey(nameof(FieldUserInfoFWorkType)))
            {
                //This should be set whenever user selects a different field book
                fieldworkType = Preferences.Get(nameof(FieldUserInfoFWorkType), fieldworkType);

                //Something could have happened and nothing was selected, enforce bedrock
                if (fieldworkType == string.Empty)
                {
                    fieldworkType = ApplicationThemeBedrock; 
                }

                //Use case with old 'Bedrock - drill core' field work type
                if (fieldworkType.ToLower() == ApplicationThemeDrillHole)
                {
                    fieldworkType = ApplicationThemeBedrock;
                }
            }

            //Connect to default, not user database
            SQLiteAsyncConnection pickersConnection = da.GetConnectionFromPath(da.DatabaseFilePath);
            _vocabularyManagers = await pickersConnection.Table<VocabularyManager>().Where(e => e.ThemeEditable == boolYes && (e.ThemeSpecificTo.Contains(fieldworkType) || e.ThemeSpecificTo == string.Empty || e.ThemeSpecificTo == null)).ToListAsync();

            //Special fill for table names
            _picklistTables = await FillTablePicklist(pickersConnection);
            OnPropertyChanged(nameof(PicklistTables));

            await pickersConnection.CloseAsync();
        }

        /// <summary>
        /// Will go through the list of table names from M_DICTIONARY_MANAGER
        /// instead of the default vocab list from M_DICTIONNARY
        /// </summary>
        /// <returns></returns>
        private async Task<ComboBox> FillTablePicklist(SQLiteAsyncConnection inConnection)
        {
            //Build combobox object
            List<Vocabularies> vocTable = new List<Vocabularies>();
            List<string> parsedVoc = new List<string>();

            if (_vocabularyManagers != null && _vocabularyManagers.Count > 0)
            {

                foreach (VocabularyManager vcms in _vocabularyManagers)
                {
                    if (!parsedVoc.Contains(vcms.ThemeAssignTable))
                    {
                        //Spoof a vocab object and get localized table name
                        Vocabularies voc = new Vocabularies();
                        voc.Code = vcms.ThemeAssignTable;
                        voc.Description = vcms.ThemeAssignTable;

                        if (vcms.ThemeAssignTable.ToLower().Contains(TableNames.environment.ToString()))
                        {
                            voc.Description = LocalizationResourceManager["FielNotesEnvironmentHeader"].ToString();
                        }

                        if (vcms.ThemeAssignTable.ToLower().Contains(TableNames.document.ToString()))
                        {
                            voc.Description = LocalizationResourceManager["FielNotesPhotoHeader"].ToString();
                        }

                        if (vcms.ThemeAssignTable.ToLower().Contains(TableNames.drill.ToString()))
                        {
                            voc.Description = LocalizationResourceManager["FielNotesDrillHolesHeader"].ToString();
                        }

                        if (vcms.ThemeAssignTable.ToLower().Contains(KeywordEarthmat))
                        {
                            voc.Code = vcms.ThemeAssignTable;
                            voc.Description = LocalizationResourceManager["FielNotesEMHeader"].ToString();
                        }

                        if (vcms.ThemeAssignTable.ToLower().Contains(TableNames.fossil.ToString()))
                        {
                            voc.Description = LocalizationResourceManager["FielNotesFossilHeader"].ToString();
                        }

                        if (vcms.ThemeAssignTable.ToLower().Contains(TableNames.location.ToString()))
                        {
                            voc.Description = LocalizationResourceManager["FielNotesLocationHeader"].ToString();
                        }

                        if (vcms.ThemeAssignTable.ToLower().Contains(TableNames.meta.ToString()))
                        {
                            voc.Description = LocalizationResourceManager["FieldBookPageTitle"].ToString();
                        }
                        if (vcms.ThemeAssignTable.ToLower().Contains(TableNames.mineral.ToString()))
                        {
                            voc.Description = LocalizationResourceManager["FielNotesMineralHeader"].ToString();
                        }
                        if (vcms.ThemeAssignTable.ToLower().Contains(TableNames.mineralization.ToString()))
                        {
                            voc.Description = LocalizationResourceManager["FielNotesMineralizationHeader"].ToString();
                        }
                        if (vcms.ThemeAssignTable.ToLower().Contains(KeywordPflow))
                        {
                            voc.Description = LocalizationResourceManager["FielNotesPaleoflowHeader"].ToString();
                        }
                        if (vcms.ThemeAssignTable.ToLower().Contains(TableNames.sample.ToString()))
                        {
                            voc.Description = LocalizationResourceManager["FielNotesSampleHeader"].ToString();
                        }
                        if (vcms.ThemeAssignTable.ToLower().Contains(TableNames.station.ToString()))
                        {
                            voc.Description = LocalizationResourceManager["FielNotesStationHeader"].ToString();
                        }
                        if (vcms.ThemeAssignTable.ToLower().Contains(TableNames.structure.ToString()))
                        {
                            voc.Description = LocalizationResourceManager["FielNotesStructureHeader"].ToString();
                        }
                        if (vcms.ThemeAssignTable.ToLower().Contains(KeywordLinework))
                        {
                            voc.Description = LocalizationResourceManager["FielNotesLineworkHeader"].ToString();
                        }
                        //Prevent bs from beind added.
                        if (voc.Code != null && voc.Code != string.Empty)
                        {
                            vocTable.Add(voc);

                            parsedVoc.Add(vcms.ThemeAssignTable);
                        }


                    }

                }

            }

            //Convert to custom picker
            ComboBox tableBox = da.GetComboboxListFromVocab(vocTable);

            //Sort based on localized value
            List<ComboBoxItem> sortedTableBox = tableBox.cboxItems.OrderBy(t => t.itemName).ToList();
            tableBox.cboxItems = sortedTableBox;

            return tableBox;

        }

        /// <summary>
        /// Will go through the list of field names from M_DICTIONARY_MANAGER
        /// instead of the default vocab list from M_DICTIONNARY
        /// </summary>
        /// <returns></returns>
        public void FillFieldsPicklist()
        {
            //Reset other controls
            ResetPage(false);

            //Build combobox object
            List<Vocabularies> vocTable = new List<Vocabularies>();
            List<string> parsedVoc = new List<string>();

            if (_vocabularyManagers != null && _vocabularyManagers.Count > 0 && ModelPicklist.PicklistName != string.Empty)
            {

                foreach (VocabularyManager v in _vocabularyManagers)
                {
                    if (v.ThemeEditable == boolYes && v.ThemeAssignTable == ModelPicklist.PicklistName)
                    {
                        if (!parsedVoc.Contains(v.ThemeAssignField))
                        {

                            //Spoof a vocab object and get localized table name
                            Vocabularies voc = new Vocabularies();
                            voc.Code = v.ThemeCodedTheme;
                            voc.Description = v.ThemeCodeThemeDesc;

                            //Prevent bs from being added.
                            if (voc.Code != null && voc.Code != string.Empty)
                            {
                                vocTable.Add(voc);
                                parsedVoc.Add(v.ThemeAssignField);
                            }
                        }
                    }
                }
            }

            //Convert to custom picker
            ComboBox fieldBox = da.GetComboboxListFromVocab(vocTable);

            //Sort based on localized value
            List<ComboBoxItem> sortedFieldBox = fieldBox.cboxItems.OrderBy(t => t.itemName).ToList();
            fieldBox.cboxItems = sortedFieldBox;

            _picklistFields.cboxItems.Clear();
            OnPropertyChanged(nameof(PicklistFields));

            _picklistFields = fieldBox;
            OnPropertyChanged(nameof(PicklistFields));

        }

        /// <summary>
        /// Will fill out the list view of all picklist values
        /// based on selected table and field 
        /// </summary>
        public async void FillFieldValuesPicklist()
        {
            //Reset
            _picklistValues.Clear();
            OnPropertyChanged(nameof(PicklistValues));

            //Get the values
            List<Vocabularies> incomingValues = new List<Vocabularies>();
            List<VocabularyManager> assignToFields = _vocabularyManagers.Where(v => v.ThemeCodedTheme == _modelPicklist.PicklistField).ToList();
            if (_modelPicklist.PicklistParent != null && _modelPicklist.PicklistParent != string.Empty)
            {
                //Get db AssignTo field from selection
                if (assignToFields != null && assignToFields.Count() > 0)
                {
                    incomingValues = await da.GetPicklistValuesAsync(_modelPicklist.PicklistName, assignToFields[0].ThemeAssignField, _modelPicklist.PicklistParent, true, da.DatabaseFilePath);
                }

            }
            else
            {
                if (assignToFields != null && assignToFields.Count() > 0)
                {
                    incomingValues = await da.GetPicklistValuesAsync(_modelPicklist.PicklistName, assignToFields[0].ThemeAssignField, "", true, da.DatabaseFilePath);
                }

            }

            if (incomingValues != null && incomingValues.Count > 0)
            {
                //Convert for usage in xaml template
                foreach (Vocabularies v in incomingValues)
                {
                    if (v.Code != null && v.Code != string.Empty)
                    {
                        _picklistValues.Add(v);
                    }
                }
            }

            OnPropertyChanged(nameof(PicklistValues));

        }

        /// <summary>
        /// Will fill out the picker of parent values based on user selected field.
        /// </summary>
        public async Task<bool> FillFieldParentValuesPicklist()
        {
            bool doesHaveParents = false;

            if (_modelPicklist.PicklistField != null && _modelPicklist.PicklistField != string.Empty)
            {
                //Reset
                _picklistParents.cboxItems.Clear();
                OnPropertyChanged(nameof(PicklistParents));

                //Get the values
                string query = string.Format("SELECT DISTINCT(m2.{0}) FROM {1} as m2 WHERE m2.{2} = '{3}'",
                    FieldDictionaryRelatedTo, TableDictionary, FieldDictionaryCodedTheme, _modelPicklist.PicklistField);

                SQLiteAsyncConnection parentConnection = da.GetConnectionFromPath(da.DatabaseFilePath);
                List<string> parentVocabs = await parentConnection.QueryScalarsAsync<string>(query);

                if (parentVocabs != null && parentVocabs.Count() > 0 && parentVocabs[0] != null)
                {
                    //Convert to custom picker
                    _picklistParents = da.GetComboboxListFromStrings(parentVocabs);
                    OnPropertyChanged(nameof(PicklistParents));
                    doesHaveParents = true;
                }

                await parentConnection.CloseAsync();

            }

            return doesHaveParents;
        }

        /// <summary>
        /// Will show a file picker to user to chose a geopackage to load
        /// picklist from into the app
        /// </summary>
        /// <returns></returns>
        public async Task<FileResult> PickVocabGeopackage()
        {

            try
            {
                // NOTE: for Android application/mbtiles doesn't work
                // we need to get all and filter out only mbtile selected files.

                FilePickerFileType customFileType = new FilePickerFileType(
                    new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        {DevicePlatform.WinUI, new [] { DatabaseLiterals.LayerTypeGPKG} },
                        {DevicePlatform.Android, new [] { "application/*"} },
                        {DevicePlatform.iOS, new [] { DatabaseLiterals.LayerTypeGPKG } },
                    });

                PickOptions options = new PickOptions();
                options.PickerTitle = LocalizationResourceManager["PicklistPageImport"].ToString();
                options.FileTypes = customFileType;

                var result = await FilePicker.Default.PickAsync(options);
                if (result != null)
                {
                    if (result.FileName.Contains(DatabaseLiterals.LayerTypeGPKG))
                    {
                        return result;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }


            }
            catch (System.Exception ex)
            {
                new ErrorToLogFile(ex).WriteToFile();
            }

            return null;
        }

        /// <summary>
        /// Will clean the page by removing all list and selected values
        /// </summary>
        public void ResetPage(bool withTables = true)
        {
            if (withTables)
            {
                _picklistTables.cboxDefaultItemIndex = -1;
                OnPropertyChanged(nameof(PicklistTables));
            }

            _picklistFields.cboxItems.Clear();
            _picklistFields.cboxDefaultItemIndex = -1;
            OnPropertyChanged(nameof(PicklistFields));

            _picklistParents.cboxItems.Clear();
            _picklistParents.cboxDefaultItemIndex = -1;
            OnPropertyChanged(nameof(PicklistParents));

            _picklistValues.Clear();
            OnPropertyChanged(nameof(PicklistValues));

        }

        /// <summary>
        /// Will mod a term
        /// </summary>
        /// <param name="vocabToEdit"></param>
        public async void ModifyATerm(Vocabularies vocabToEdit)
        {
            string popUpTitle = LocalizationResourceManager["PicklistPageModifyTermTitle"].ToString();
            string popUpContent = LocalizationResourceManager["PicklistPageModifyTermContent"].ToString();
            string popUpButtonOK = LocalizationResourceManager["GenericButtonOk"].ToString();
            string popUpButtonCancel = LocalizationResourceManager["GenericButtonCancel"].ToString();
            string result = await Shell.Current.DisplayPromptAsync(popUpTitle, popUpContent, popUpButtonOK, popUpButtonCancel, null, -1, null, vocabToEdit.Description);

            if (result != null && result != string.Empty)
            {
                //Trim
                result = result.Trim();

                //Set
                vocabToEdit.Description = result;
                vocabToEdit.DescriptionFR = result;
                vocabToEdit.Editor = Preferences.Get(nameof(FieldUserInfoUCode), AppInfo.Current.Name);
                vocabToEdit.EditorDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now);

                //Replace 
                int vocabIndex = -1;
                foreach (Vocabularies voc in _picklistValues)
                {
                    if (voc.TermID == vocabToEdit.TermID)
                    {
                        vocabIndex = _picklistValues.IndexOf(voc);
                    }
                }
                _picklistValues.RemoveAt(vocabIndex);
                _picklistValues.Insert(vocabIndex, vocabToEdit);
                OnPropertyChanged(nameof(PicklistValues));

            }
        }

        /// <summary>
        /// Will set a term to be a default selection for a picklist
        /// </summary>
        /// <param name="vocabToEdit"></param>
        public async void SetADefaultTerm(Vocabularies vocabToEdit)
        {
            if (vocabToEdit != null)
            {

                // Get vocab that needs to be set as default
                Vocabularies newDefault = _picklistValues.Where(v => v.TermID == vocabToEdit.TermID).ToList().FirstOrDefault();

                // Get previous vocab that was set as default
                Vocabularies previousDefault = _picklistValues.Where(d => d.DefaultValue == boolYes).ToList().FirstOrDefault();

                //Validation and update of new default
                if (newDefault != null)
                {
                    newDefault.DefaultValue = boolYes;
                    int newDefaultIndex = _picklistValues.IndexOf(newDefault);

                    if (newDefaultIndex > -1)
                    {
                        _picklistValues.RemoveAt(newDefaultIndex);
                        _picklistValues.Insert(newDefaultIndex, newDefault);
                    }

                }

                // Validate and update of old default value
                if (previousDefault != null)
                {
                    previousDefault.DefaultValue = boolNo;
                    int previousDefaultIndex = _picklistValues.IndexOf(previousDefault);

                    if (previousDefaultIndex > -1)
                    {
                        _picklistValues.RemoveAt(previousDefaultIndex);
                        _picklistValues.Insert(previousDefaultIndex, previousDefault);
                    }
                }

                OnPropertyChanged(nameof(PicklistValues));

            }
        }
        #endregion

        #region EVENTS

        /// <summary>
        /// Whenever a user changes field book a forced refresh will occurs on picklist editor
        /// In case user goes from surficial to bedrock or the other way around
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="hasChanged"></param>
        private async void FieldBooksViewModel_newFieldBookSelectedAsync(object sender, bool hasChanged)
        {
            if (hasChanged)
            {
                //Reload all notes
                ResetPage();

                await FillPickers();
            }
        }

        #endregion

    }
}
