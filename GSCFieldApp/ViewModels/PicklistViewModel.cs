using GSCFieldApp.Models;
using GSCFieldApp.Services.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Template10.Mvvm;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using static GSCFieldApp.Dictionaries.DatabaseLiterals;

namespace GSCFieldApp.ViewModels
{
    public class PicklistViewModel : ViewModelBase
    {
        #region INIT

        readonly Vocabularies voc = new Vocabularies();
        readonly DataAccess accessData = new DataAccess();
        readonly DataLocalSettings localSettings = new DataLocalSettings();

        //Combobox
        private ObservableCollection<Themes.ComboBoxItem> _picklists = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedPicklist = string.Empty;
        private ObservableCollection<Themes.ComboBoxItem> _parentPicklist = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedParent = string.Empty;

        //List box
        private ObservableCollection<Vocabularies> _picklistValues = new ObservableCollection<Vocabularies>();
        private int _selectedPicklistValuesIndex = -1;
        private readonly List<string> _picklistValueCodes = new List<string>(); //Will be used to detect new added code without iterating inside the obs collection
        private readonly List<string> _picklistValueCodesNew = new List<string>(); //Will be used to track added term by the user.

        //Textbox
        private string _addModifyTerm = string.Empty;
        private Vocabularies _addModifyObject = new Vocabularies();
        private bool _picklistEditEnableDisable = true;
        //Other
        public string picklistTheme = string.Empty;
        public Visibility _parentVisibility = Visibility.Collapsed;

        public PicklistViewModel(string inPicklistTheme)
        {
            picklistTheme = inPicklistTheme;
            _picklistValues = new ObservableCollection<Vocabularies>();
            if (picklistTheme != string.Empty)
            {
                FillPicklistCombobox();
            }
        }

        #endregion

        #region Properties

        //Combobox 
        public ObservableCollection<Themes.ComboBoxItem> Picklists { get { return _picklists; } set { _picklists = value; } }
        public string SelectedPicklist { get { return _selectedPicklist; } set { _selectedPicklist = value; } }
        public ObservableCollection<Themes.ComboBoxItem> ParentPicklist { get { return _parentPicklist; } set { _parentPicklist = value; } }
        public string SelectedParent { get { return _selectedParent; } set { _selectedParent = value; } }

        //Listbox 
        public ObservableCollection<Vocabularies> PicklistValues { get { return _picklistValues; } set { _picklistValues = value; } }
        public int SelectedPicklistValueIndex { get { return _selectedPicklistValuesIndex; } set { _selectedPicklistValuesIndex = value; } }

        //Textbox
        public string AddModifyTerm { get { return _addModifyTerm; } set { _addModifyTerm = value; } }
        public object AddModifyObject { get { return _addModifyObject as object; } set { _addModifyObject = value as Vocabularies; } }
        public bool PicklistEditEnableDisable { get { return _picklistEditEnableDisable; } set { _picklistEditEnableDisable = value; } }
        //UI
        public Visibility ParentVisibility { get { return _parentVisibility; } set { _parentVisibility = value; } }

        #endregion

        #region FILL MANAGEMENT

        /// <summary>
        /// Will fill the combobox that shows available picklist for selected theme
        /// </summary>
        private void FillPicklistCombobox()
        {
            //Get the current project type
            string fieldworkType = string.Empty;
            if (localSettings.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoFWorkType) != null)
            {
                fieldworkType = localSettings.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoFWorkType).ToString();
            }

            //Retrieve list of picklist from user selected theme
            VocabularyManager vocManager = new VocabularyManager();
            List<object> vocabManagerRaw = accessData.ReadTable(vocManager.GetType(), null);
            IEnumerable<VocabularyManager> vocManagerTable = vocabManagerRaw.Cast<VocabularyManager>(); //Cast to proper list type
            IEnumerable<VocabularyManager> vocThemeID = from vm in vocManagerTable where vm.ThemeTable.ToLower().Contains(picklistTheme) && vm.ThemeEditable == Dictionaries.DatabaseLiterals.boolYes && (vm.ThemeProjectType == fieldworkType || vm.ThemeProjectType == "") select vm;

            //Special case for mineral table
            if (picklistTheme == Dictionaries.DatabaseLiterals.KeywordMineral)
            {
                vocThemeID = from vm in vocManagerTable where vm.ThemeTable.ToLower().Contains(picklistTheme) && !vm.ThemeTable.ToLower().Contains(Dictionaries.DatabaseLiterals.KeywordMA) && vm.ThemeEditable == Dictionaries.DatabaseLiterals.boolYes && (vm.ThemeProjectType == fieldworkType || vm.ThemeProjectType == "") select vm;
            }

            //Add theme list to dialog or a default NA value if nothing is available yet
            if (vocThemeID.Count() > 0)
            {
                foreach (VocabularyManager themeID in vocThemeID)
                {
                    Themes.ComboBoxItem picklistTheme = new Themes.ComboBoxItem
                    {
                        itemName = themeID.ThemeNameDesc,
                        itemValue = themeID.ThemeName
                    };
                    _picklists.Add(picklistTheme);

                }

            }
            else
            {
                ResourceLoader appResources = ResourceLoader.GetForCurrentView();
                string emptyTheme = appResources.GetString("PicklistDialogEmptyList/Text");

                Themes.ComboBoxItem emptyPicklist = new Themes.ComboBoxItem
                {
                    itemName = emptyTheme,
                    itemValue = string.Empty
                };
                _picklists.Add(emptyPicklist);
            }

            RaisePropertyChanged("Picklists");
        }

        /// <summary>
        /// Will update list box in dialog
        /// </summary>
        /// <param name="inSelectedItem">The combobox item user has selected</param>
        public void FillListBoxPicklistValues(string selectedItemID)
        {
            //Variables
            bool hasParent = false;

            //Retrieve list of values
            string picklistQuerySelectFrom = "SELECT * FROM " + Dictionaries.DatabaseLiterals.TableDictionary;
            string picklistQueryWhere = " WHERE " + Dictionaries.DatabaseLiterals.TableDictionary + "." + Dictionaries.DatabaseLiterals.FieldDictionaryCodedTheme + " = '" + selectedItemID + "'";
            string picklistQueryOrder = " ORDER BY " + Dictionaries.DatabaseLiterals.TableDictionary + "." + Dictionaries.DatabaseLiterals.FieldDictionaryOrder + " ASC";
            string picklistQueryFinal = picklistQuerySelectFrom + picklistQueryWhere + picklistQueryOrder;

            List<object> vocRaw = accessData.ReadTable(voc.GetType(), picklistQueryFinal);
            IEnumerable<Vocabularies> vocTable = vocRaw.Cast<Vocabularies>();

            //Add theme list to dialog or a default NA value if nothing is available yet
            _picklistValues.Clear();
            _picklistValueCodes.Clear();
            foreach (Vocabularies termIDs in vocTable)
            {
                _picklistValues.Add(termIDs);
                _picklistValueCodes.Add(termIDs.Code);

                if (termIDs.RelatedTo != string.Empty && termIDs.RelatedTo != null)
                {
                    hasParent = true;
                }
            }

            RaisePropertyChanged("PicklistValues");

            if (hasParent)
            {
                _parentVisibility = Visibility.Visible;
                RaisePropertyChanged("ParentVisibility");
                FillParentCombobox(selectedItemID);
            }
            else
            {
                _parentVisibility = Visibility.Collapsed;
                RaisePropertyChanged("ParentVisibility");

                //Empty list
                _parentPicklist.Clear();
                RaisePropertyChanged("PicklistParents");
            }

        }

        /// <summary>
        /// Will fill the parent combobox if any parent is found inside a list of terms.
        /// </summary>
        /// <param name="selectedChildID"></param>
        public void FillParentCombobox(string selectedChildID)
        {
            //Build query to retrieve unique parents
            //select * from M_DICTIONARY m WHERE m.CODETHEME in 
            string querySelect_1 = "select * from " + Dictionaries.DatabaseLiterals.TableDictionary + " m ";
            string queryWhere_1 = " WHERE m." + Dictionaries.DatabaseLiterals.FieldDictionaryCodedTheme + " in ";

            //(select m.CODETHEME from M_DICTIONARY m join M_DICTIONARY_MANAGER mdm on m.CODETHEME = mdm.CODETHEME WHERE m.CODE in 
            string querySelect_2 = "(select m." + Dictionaries.DatabaseLiterals.FieldDictionaryCodedTheme + " from " + Dictionaries.DatabaseLiterals.TableDictionary + " m ";
            string querySelect_2_join = "join " + Dictionaries.DatabaseLiterals.TableDictionaryManager + " mdm on m." + Dictionaries.DatabaseLiterals.FieldDictionaryCodedTheme + " = mdm." + Dictionaries.DatabaseLiterals.FieldDictionaryCodedTheme + " ";
            string queryWhere_2 = " WHERE m." + Dictionaries.DatabaseLiterals.FieldDictionaryCode + " in ";

            //(select distinct(m.RELATEDTO) from M_DICTIONARY m WHERE m.CODETHEME = 'MODTEXTURE' ORDER BY m.RELATEDTO ) and mdm.ASSIGNTABLE in 
            string querySelect_3 = "(select distinct(m." + Dictionaries.DatabaseLiterals.FieldDictionaryRelatedTo + ") from " + Dictionaries.DatabaseLiterals.TableDictionary + " m ";
            string queryWhere_3 = " WHERE m." + Dictionaries.DatabaseLiterals.FieldDictionaryCodedTheme + " = '" + selectedChildID + "'";
            string queryOrderBy_3 = " ORDER BY m." + Dictionaries.DatabaseLiterals.FieldDictionaryRelatedTo + " ) and mdm." + Dictionaries.DatabaseLiterals.FieldDictionaryManagerAssignTable + " in ";

            //(select mdm2.ASSIGNTABLE from M_DICTIONARY_MANAGER mdm2 where mdm2.CODETHEME = 'MODTEXTURE'))  AND m.VISIBLE = 'Y' ORDER BY m.DESCRIPTIONEN ASC
            string queryWhere_1_2 = "(select mdm2." + Dictionaries.DatabaseLiterals.FieldDictionaryManagerAssignTable +
                " from " + Dictionaries.DatabaseLiterals.TableDictionaryManager + " mdm2 where mdm2." + Dictionaries.DatabaseLiterals.FieldDictionaryCodedTheme +
                " = '" + selectedChildID + "'))";
            string queryWhere_1_3 = " AND m." + Dictionaries.DatabaseLiterals.FieldDictionaryVisible + " = '" + Dictionaries.DatabaseLiterals.boolYes + "'";
            string queryOrderby_1 = " ORDER BY m." + Dictionaries.DatabaseLiterals.FieldDictionaryDescription + " ASC";

            string queryFinal = querySelect_1 + queryWhere_1 + querySelect_2 + querySelect_2_join + queryWhere_2 + querySelect_3 + queryWhere_3 + queryOrderBy_3 + queryWhere_1_2 + queryWhere_1_3 + queryOrderby_1;

            //Get a unique list of all parents
            List<object> parentRaw = accessData.ReadTable(voc.GetType(), queryFinal);
            IEnumerable<Vocabularies> parentTable = parentRaw.Cast<Vocabularies>();

            //Add theme list to dialog or a default NA value if nothing is available yet
            _parentPicklist.Clear();
            foreach (Vocabularies parents in parentTable)
            {
                Themes.ComboBoxItem picklistParents = new Themes.ComboBoxItem
                {
                    itemName = parents.Description,
                    itemValue = parents.Code
                };
                _parentPicklist.Add(picklistParents);
            }

            RaisePropertyChanged("PicklistParents");

        }

        public void FillChildCombobox(string selectedParentID)
        {
            if (selectedParentID != null)
            {
                //Build query to retrieve unique parents
                string querySelect = "SELECT *";
                string queryFrom = " FROM " + Dictionaries.DatabaseLiterals.TableDictionary;
                string queryWhere = " WHERE " + Dictionaries.DatabaseLiterals.TableDictionary + "." + Dictionaries.DatabaseLiterals.FieldDictionaryRelatedTo + " = '" + selectedParentID + "'";
                string queryWhereAnd = " AND " + Dictionaries.DatabaseLiterals.TableDictionary + "." + Dictionaries.DatabaseLiterals.FieldDictionaryCodedTheme + " = '" + _selectedPicklist + "'";
                string queryOrderby = " ORDER BY " + Dictionaries.DatabaseLiterals.TableDictionary + "." + Dictionaries.DatabaseLiterals.FieldDictionaryOrder + " ASC";
                string queryFinal = querySelect + queryFrom + queryWhere + queryWhereAnd + queryOrderby;

                //Get a unique list of all parents
                List<object> childRaw = accessData.ReadTable(voc.GetType(), queryFinal);
                IEnumerable<Vocabularies> childTable = childRaw.Cast<Vocabularies>();

                //Add theme list to dialog or a default NA value if nothing is available yet
                _picklistValues.Clear();
                foreach (Vocabularies childs in childTable)
                {
                    _picklistValues.Add(childs);
                }

                RaisePropertyChanged("PicklistValues");
            }


        }
        #endregion

        #region SAVE/UPDATE METHODS
        /// <summary>
        /// On save event
        /// </summary>
        public void SaveDialogInfo()
        {
            if (_picklistValues.Count > 0)
            {
                //Get date
                DateTime currentTime = DateTime.Now;
                string currentStringTime = String.Format("{0:yyyy-MM-dd}", currentTime);

                //Get user metaid
                string userID = localSettings.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoID).ToString();

                //Set order
                double userOrder = 1.0;

                //Iterate through picklist
                foreach (Vocabularies finalValues in _picklistValues)
                {
                    //Variables
                    bool udpateTerm = true;

                    //Detect new terms
                    if (_picklistValueCodesNew.Contains(finalValues.Code))
                    {
                        udpateTerm = false; //Add, don't update

                        //Set creator
                        finalValues.Creator = userID;
                        finalValues.CreatorDate = currentStringTime;
                    }
                    else
                    {
                        //Set editor
                        finalValues.Editor = userID;
                        finalValues.EditorDate = currentStringTime;
                    }

                    //Detect parent
                    if ((_selectedParent != null || _selectedParent != string.Empty) && finalValues.Description == _addModifyTerm)
                    {
                        finalValues.RelatedTo = _selectedParent;
                    }

                    finalValues.Order = userOrder;

                    //Save model class
                    accessData.SaveFromSQLTableObject(finalValues, udpateTerm);

                    userOrder++;
                }

                //Update semantic zoom data for lithology if needed
                if (_selectedPicklist == Dictionaries.DatabaseLiterals.KeywordLithgrouptype || _selectedPicklist == Dictionaries.DatabaseLiterals.KeywordLithDetail)
                {
                    UpdateLithology();
                }
                else if (_selectedPicklist == Dictionaries.DatabaseLiterals.KeywordStrucClassType || _selectedPicklist == Dictionaries.DatabaseLiterals.KeywordStrucDetail)
                {
                    //Update semantic zoom data for structure type if needed
                    UpdateStructureType();
                }

            }
        }


        /// <summary>
        /// Will force a refresh of the structure type semantic zoom data
        /// </summary>
        public async void UpdateStructureType()
        {
            await UpdateSemanticZooms();

            //Rebuild semantic data
            Models.SemanticDataGenerator.GetGroupedData(true, Dictionaries.DatabaseLiterals.TableStructure, Dictionaries.DatabaseLiterals.FieldStructureClass, Dictionaries.DatabaseLiterals.FieldStructureDetail);
        }

        /// <summary>
        /// Will force a refresh of the lithology semantic zoom data
        /// </summary>
        public async void UpdateLithology()
        {
            await UpdateSemanticZooms();

            //Rebuild semantic data
            Models.SemanticDataGenerator.GetGroupedData(true, Dictionaries.DatabaseLiterals.TableEarthMat, Dictionaries.DatabaseLiterals.FieldEarthMatLithgroup, Dictionaries.DatabaseLiterals.FieldEarthMatLithdetail);
        }

        /// <summary>
        /// Will update parent/child type picklist in DB
        /// </summary>
        /// <returns></returns>
        public Task UpdateSemanticZooms()
        {
            Task updateTask = Task.CompletedTask;

            //Get date
            DateTime currentTime = DateTime.Now;
            string currentStringTime = String.Format("{0:yyyy-MM-dd}", currentTime);

            //Get user metaid
            string userID = localSettings.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoID).ToString();


            //Iterate through picklist
            foreach (Vocabularies finalValues in _picklistValues)
            {
                //Build query
                string queryUpdate = "UPDATE " + TableDictionary;
                string querySetVisibility = " SET " + FieldDictionaryVisible + " = '" + finalValues.Visibility + "', ";
                string querySetEditor = FieldDictionaryEditor + " = '" + userID + "', ";
                string querySetEditDate = FieldDictionaryEditorDate + " = '" + currentStringTime + "'";
                string queryWhere = " WHERE " + FieldDictionaryRelatedTo + " LIKE '" + finalValues.Code + "%'";

                //Save model class
                accessData.SaveFromQuery(queryUpdate + querySetVisibility + querySetEditor + querySetEditDate + queryWhere);
            }

            return updateTask;
        }

        #endregion

        #region EVENTS

        /// <summary>
        /// Will manager user term selection and process it in the textbox control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void picklistValues_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListViewBase senderBase = sender as ListViewBase;
            if (senderBase.SelectedItems.Count == 1)
            {
                Vocabularies selectedTerm = senderBase.SelectedItem as Vocabularies;

                if (selectedTerm.Editable == Dictionaries.DatabaseLiterals.boolYes || selectedTerm.Editable == null)
                {
                    _addModifyTerm = selectedTerm.Description;
                    _addModifyObject = selectedTerm;

                    RaisePropertyChanged("AddModifyObject");
                    RaisePropertyChanged("AddModifyTerm");
                    _picklistEditEnableDisable = true;
                    RaisePropertyChanged("PicklistEditEnableDisable");
                }
                else
                {
                    //Clear tag
                    _addModifyObject = new Vocabularies();
                    RaisePropertyChanged("AddModifyObject");
                    _picklistEditEnableDisable = false;
                    RaisePropertyChanged("PicklistEditEnableDisable");
                }

            }
            else if (senderBase.SelectedItems.Count == 0 || senderBase.SelectedItems.Count > 1)
            {
                //Clear tag
                _addModifyObject = new Vocabularies();
                RaisePropertyChanged("AddModifyObject");
                _picklistEditEnableDisable = true;
                RaisePropertyChanged("PicklistEditEnableDisable");
            }
        }

        public void picklistParent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Set parent inside term in textbox
            if (_addModifyObject != null)
            {
                _addModifyObject.RelatedTo = _selectedParent;
                RaisePropertyChanged("AddModifyObject");
            }

            FillChildCombobox(_selectedParent);
        }

        /// <summary>
        /// Will manager the textbox term when updated by the user.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void picklistAddTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //Cast
            TextBox senderBox = sender as TextBox;


            if (senderBox.Text != null && senderBox.Text != string.Empty && senderBox.Text != "")
            {
                //Set description
                _addModifyObject.Description = senderBox.Text;

                //Set code and id
                if (!_picklistValueCodes.Contains(_addModifyObject.Code))
                {
                    _addModifyObject.Code = senderBox.Text;
                    _addModifyObject.RelatedTo = _selectedParent;

                    //New id
                    DataIDCalculation idCalculator = new DataIDCalculation();
                    _addModifyObject.TermID = idCalculator.CalculateTermID();

                    //Set coded theme and visibility
                    if (_picklistValues.Count == 0)
                    {
                        //_picklistValues.Add(_addModifyObject);
                        //RaisePropertyChanged("PicklistValues");

                        _addModifyObject.CodedTheme = _selectedPicklist;
                    }
                    else
                    {
                        _addModifyObject.CodedTheme = _picklistValues[0].CodedTheme;
                    }

                    _addModifyObject.Visibility = Dictionaries.DatabaseLiterals.boolYes;
                    _addModifyObject.Editable = boolYes;

                    //Set an order
                    _addModifyObject.Order = 0.0;

                }

                //Select parent if there is one
                if (_addModifyObject.RelatedTo != string.Empty && _addModifyObject.RelatedTo != null)
                {
                    _selectedParent = _addModifyObject.RelatedTo;
                    RaisePropertyChanged("SelectedParent");
                }

                //Update
                RaisePropertyChanged("AddModifyObject");
            }


        }

        /// <summary>
        /// Will push back textbox values into the list view of picklist terms
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void PicklistValueAddIcon_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (_picklistEditEnableDisable)
            {
                //Get current vocab in text box
                Vocabularies currentVocab = _addModifyObject as Vocabularies;

                //Refresh list view with vocab 
                bool vocabInList = false;
                for (int i = 0; i < _picklistValues.Count; i++)
                {
                    //For existing vocab in list
                    if (_picklistValues[i].Code == currentVocab.Code)
                    {
                        _picklistValues[i] = currentVocab;
                        vocabInList = true;
                        break;
                    }
                }

                //If vocab is new
                if (!vocabInList)
                {

                    _picklistValues.Add(currentVocab);
                    _picklistValueCodes.Add(currentVocab.Code);
                    _picklistValueCodesNew.Add(currentVocab.Code);
                }

                _picklistValues.Sort(); //Sort using a custom extension
                RaisePropertyChanged("PicklistValues");

                //Refresh text box 
                _addModifyTerm = string.Empty;
                _addModifyObject = new Vocabularies();
                RaisePropertyChanged("AddModifyTerm");
                RaisePropertyChanged("AddModifyObject");
            }


        }

        /// <summary>
        /// Will changed the selected item background value and visibility property
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void PicklistValueDefaultIcon_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (_selectedPicklistValuesIndex != -1)
            {
                int selectedIndex = _selectedPicklistValuesIndex;
                List<Vocabularies> copy = _picklistValues.ToList();
                _picklistValues.Clear();
                foreach (Vocabularies vcs in copy)
                {
                    if (vcs.Code == copy[selectedIndex].Code)
                    {
                        //Detect current
                        if (vcs.DefaultValue == Dictionaries.DatabaseLiterals.boolYes)
                        {
                            vcs.DefaultValue = Dictionaries.DatabaseLiterals.boolNo;
                        }
                        else
                        {
                            vcs.DefaultValue = Dictionaries.DatabaseLiterals.boolYes;
                        }

                    }
                    else
                    {
                        vcs.DefaultValue = Dictionaries.DatabaseLiterals.boolNo;
                    }
                    _picklistValues.Add(vcs);
                    RaisePropertyChanged("PicklistValues");

                }


            }
        }

        #endregion


    }
}
