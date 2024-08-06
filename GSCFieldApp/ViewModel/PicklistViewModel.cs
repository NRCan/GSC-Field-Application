using CommunityToolkit.Mvvm.ComponentModel;
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

namespace GSCFieldApp.ViewModel
{
    public partial class PicklistViewModel: FieldAppPageHelper
    {
        #region INIT
        private Picklist _model = new Picklist();
        private ComboBox _picklistTables = new ComboBox();
        private ComboBox _picklistFields = new ComboBox();

        private List<VocabularyManager> _vocabularyManagers = new List<VocabularyManager>();
        private ObservableCollection<Picklist> _picklistValues = new ObservableCollection<Picklist>();

        #endregion

        #region RELAYS
        #endregion

        #region PROPERTIES

        public Picklist Model { get { return _model; } set { _model = value; } }
        public ComboBox PicklistTables { get { return _picklistTables; } set { _picklistTables = value; } }
        public ComboBox PicklistFields { get { return _picklistFields; } set { _picklistFields = value; } }
        public ObservableCollection<Picklist> PicklistValues { get { return _picklistValues; } set { _picklistValues = value; } }
        #endregion

        public PicklistViewModel() 
        {
            _ = FillPickers();
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
            }

            //Connect
            SQLiteAsyncConnection currentConnection = da.GetConnectionFromPath(da.PreferedDatabasePath);
            _vocabularyManagers = await currentConnection.Table<VocabularyManager>().Where(e => e.ThemeEditable == boolYes && (e.ThemeProjectType == fieldworkType || e.ThemeProjectType == string.Empty)).ToListAsync();

            //Special fill for table names
            _picklistTables = await FillTablePicklist(currentConnection);
            OnPropertyChanged(nameof(PicklistTables));

            await currentConnection.CloseAsync();
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
                    if (!parsedVoc.Contains(vcms.ThemeTable))
                    {
                        //Spoof a vocab object and get localized table name
                        Vocabularies voc = new Vocabularies();
                        voc.Code = vcms.ThemeTable;
                        voc.Description = vcms.ThemeTable;

                        if (vcms.ThemeTable.ToLower().Contains(TableNames.environment.ToString()))
                        {
                            voc.Description = LocalizationResourceManager["FielNotesEnvironmentHeader"].ToString();
                        }

                        if (vcms.ThemeTable.ToLower().Contains(TableNames.document.ToString()))
                        {
                            voc.Description = LocalizationResourceManager["FielNotesPhotoHeader"].ToString();
                        }

                        if (vcms.ThemeTable.ToLower().Contains(TableNames.drill.ToString()))
                        {
                            voc.Description = LocalizationResourceManager["FielNotesDrillHolesHeader"].ToString();
                        }

                        if (vcms.ThemeTable.ToLower().Contains(KeywordEarthmat))
                        {
                            voc.Code = vcms.ThemeTable;
                            voc.Description = LocalizationResourceManager["FielNotesEMHeader"].ToString();
                        }

                        if (vcms.ThemeTable.ToLower().Contains(TableNames.fossil.ToString()))
                        {
                            voc.Description = LocalizationResourceManager["FielNotesFossilHeader"].ToString();
                        }

                        if (vcms.ThemeTable.ToLower().Contains(TableNames.location.ToString()))
                        {
                            voc.Description = LocalizationResourceManager["FielNotesLocationHeader"].ToString();
                        }

                        if (vcms.ThemeTable.ToLower().Contains(TableNames.meta.ToString()))
                        {
                            voc.Description = LocalizationResourceManager["FieldBookPageTitle"].ToString();
                        }
                        if (vcms.ThemeTable.ToLower().Contains(TableNames.mineral.ToString()))
                        {
                            voc.Description = LocalizationResourceManager["FielNotesMineralHeader"].ToString();
                        }
                        if (vcms.ThemeTable.ToLower().Contains(TableNames.mineralization.ToString()))
                        {
                            voc.Description = LocalizationResourceManager["FielNotesMineralizationHeader"].ToString();
                        }
                        if (vcms.ThemeTable.ToLower().Contains(KeywordPflow))
                        {
                            voc.Description = LocalizationResourceManager["FielNotesPaleoflowHeader"].ToString();
                        }
                        if (vcms.ThemeTable.ToLower().Contains(TableNames.sample.ToString()))
                        {
                            voc.Description = LocalizationResourceManager["FielNotesSampleHeader"].ToString();
                        }
                        if (vcms.ThemeTable.ToLower().Contains(TableNames.station.ToString()))
                        {
                            voc.Description = LocalizationResourceManager["FielNotesStationHeader"].ToString();
                        }
                        if (vcms.ThemeTable.ToLower().Contains(TableNames.structure.ToString()))
                        {
                            voc.Description = LocalizationResourceManager["FielNotesStructureHeader"].ToString();
                        }

                        //Prevent bs from beind added.
                        if (voc.Code != null && voc.Code != string.Empty)
                        {
                            vocTable.Add(voc);

                            parsedVoc.Add(vcms.ThemeTable);
                        }


                    }

                }
                
            }

            //Convert to custom picker
            ComboBox tableBox = da.GetComboboxListFromVocab(vocTable);

            //Sort based on localized value
            List<ComboBoxItem> sortedTableBox = tableBox.cboxItems.OrderBy(t=>t.itemName).ToList();
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
            //Build combobox object
            List<Vocabularies> vocTable = new List<Vocabularies>();
            List<string> parsedVoc = new List<string>();

            if (_vocabularyManagers != null && _vocabularyManagers.Count > 0 && _model.PicklistName != string.Empty)
            {
                List<VocabularyManager> subVocabList = _vocabularyManagers.Where(v => v.ThemeTable == _model.PicklistName).ToList();
                foreach (VocabularyManager vcms in subVocabList)
                {
                    if (!parsedVoc.Contains(vcms.ThemeField))
                    {
                        //Spoof a vocab object and get localized table name
                        Vocabularies voc = new Vocabularies();
                        voc.Code = vcms.ThemeField;
                        voc.Description = vcms.ThemeNameDesc;

                        //Prevent bs from beind added.
                        if (voc.Code != null && voc.Code != string.Empty)
                        {
                            vocTable.Add(voc);
                            parsedVoc.Add(vcms.ThemeField);
                        }


                    }

                }

            }

            //Convert to custom picker
            ComboBox fieldBox = da.GetComboboxListFromVocab(vocTable);

            //Sort based on localized value
            List<ComboBoxItem> sortedFieldBox = fieldBox.cboxItems.OrderBy(t => t.itemName).ToList();
            fieldBox.cboxItems = sortedFieldBox;

            _picklistFields = fieldBox;
            OnPropertyChanged(nameof(PicklistFields));

        }

        /// <summary>
        /// Will fill out the list view of all picklist values
        /// based on selected table and field 
        /// </summary>
        public async void FillFieldValuesPicklist()
        {
            //Get the values
            List<Vocabularies> incomingValues = await da.GetPicklistValuesAsync(_model.PicklistName, _model.PicklistField, "", false);

            if (incomingValues != null && incomingValues.Count > 0)
            {
                //Init
                _picklistValues.Clear();
                OnPropertyChanged(nameof(PicklistValues));

                //Convert for usage in xaml template
                foreach (Vocabularies v in incomingValues)
                {
                    Picklist vToPick = new Picklist();
                    vToPick.PicklistName = _model.PicklistName;
                    vToPick.PicklistField = _model.PicklistField;
                    vToPick.PicklistVisible = v.Visibility;
                    vToPick.PicklistDefault = v.DefaultValue;
                    vToPick.PicklistFieldValueCode = v.Code;
                    vToPick.PicklistFieldValueName = v.Description;
                    _picklistValues.Add(vToPick);
                }
            }

            OnPropertyChanged(nameof(PicklistValues));

        }
        #endregion
    }
}
