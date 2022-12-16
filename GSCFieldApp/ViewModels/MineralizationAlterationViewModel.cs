using GSCFieldApp.Models;
using GSCFieldApp.Services.DatabaseServices;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Template10.Mvvm;
using GSCFieldApp.Themes;
using Windows.UI.Xaml.Controls;
using System;

namespace GSCFieldApp.ViewModels
{
    public class MineralizationAlterationViewModel: ViewModelBase
    {
        #region INITIALIZATION

        //UI default values
        private string _mineralAltAlias = string.Empty;
        private string _mineralAltID = string.Empty;
        private string _mineralAltParentID = string.Empty;

        private string _mineralAltNote = string.Empty;
        private List<string> _mineralAlterations = new List<string>(); //Will contain a list of all mineral alterations related to current parent station. To catch duplicates

        //UI interaction
        public bool doMineralAltUpdate = false;

        private ObservableCollection<Themes.ComboBoxItem> _mineralAltMA = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedMineralAltMA = string.Empty;
        private ObservableCollection<Themes.ComboBoxItem> _mineralAltMinerals = new ObservableCollection<Themes.ComboBoxItem>();
        private ObservableCollection<Themes.ComboBoxItem> _mineralAltMineralsValues = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedMineralAltMineral = string.Empty;
        private ObservableCollection<Themes.ComboBoxItem> _mineralAltDist = new ObservableCollection<Themes.ComboBoxItem>();
        private ObservableCollection<Themes.ComboBoxItem> _mineralAltDistValues = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedMineralAltDist = string.Empty;
        private ObservableCollection<Themes.ComboBoxItem> _mineralAltUnit = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedMineralAltUnit = string.Empty;

        private ObservableCollection<Themes.ComboBoxItem> _mineralAltPhase = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedMineralAltPhase = string.Empty;
        private ObservableCollection<Themes.ComboBoxItem> _mineralAltTexture = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedMineralAltTexture = string.Empty;
        private ObservableCollection<Themes.ComboBoxItem> _mineralAltFacies = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedMineralAltFacies = string.Empty;

        //Model init
        private MineralAlteration mineralAltModel = new MineralAlteration();
        public DataIDCalculation mineralAltIDCalculator = new DataIDCalculation();
        public FieldNotes existingDataDetailMineralAlt;
        private Mineral mineralModel = new Mineral();
        DataAccess accessData = new DataAccess();

        //Events and delegate
        public delegate void mineralAltEditEventHandler(object sender); //A delegate for execution events
        public event mineralAltEditEventHandler newMineralAltEdit; //This event is triggered when a save has been done on station table. 

        #endregion

        #region PROPERTIES

        public MineralAlteration MineralAltModel { get { return mineralAltModel; } set { mineralAltModel = value; } }
        public string MineralAltAlias { get { return _mineralAltAlias; } set { _mineralAltAlias = value; } }
        public string MineralAltNote { get { return _mineralAltNote; } set { _mineralAltNote = value; } }
        public string MineralAltID { get { return _mineralAltID; } set { _mineralAltID = value; } }
        public string MineralAltParentID { get { return _mineralAltParentID; } set { _mineralAltParentID = value; } }

        public ObservableCollection<Themes.ComboBoxItem> MineralAltMA { get { return _mineralAltMA; } set { _mineralAltMA = value; } }
        public string SelectedMineralAltMA { get { return _selectedMineralAltMA; } set { _selectedMineralAltMA = value; } }
        public ObservableCollection<Themes.ComboBoxItem> MineralAltMinerals { get { return _mineralAltMinerals; } set { _mineralAltMinerals = value; } }
        public ObservableCollection<Themes.ComboBoxItem> MineralAltMineralsValues { get { return _mineralAltMineralsValues; } set { _mineralAltMineralsValues = value; } }
        public string SelectedMineralAltMineral { get { return _selectedMineralAltMineral; } set { _selectedMineralAltMineral = value; } }
        public ObservableCollection<Themes.ComboBoxItem> MineralAltDist { get { return _mineralAltDist; } set { _mineralAltDist = value; } }
        public ObservableCollection<Themes.ComboBoxItem> MineralAltDistValues { get { return _mineralAltDistValues; } set { _mineralAltDistValues = value; } }
        public string SelectedMineralAltDist { get { return _selectedMineralAltDist; } set { _selectedMineralAltDist = value; } }

        public ObservableCollection<Themes.ComboBoxItem> MineralAltPhase { get { return _mineralAltPhase; } set { _mineralAltPhase = value; } }
        public string SelectedMineralAltPhase { get { return _selectedMineralAltPhase; } set { _selectedMineralAltPhase = value; } }

        public ObservableCollection<Themes.ComboBoxItem> MineralAltTexture { get { return _mineralAltTexture; } set { _mineralAltTexture = value; } }
        public string SelectedMineralAltTexture { get { return _selectedMineralAltTexture; } set { _selectedMineralAltTexture = value; } }

        public ObservableCollection<Themes.ComboBoxItem> MineralAltFacies { get { return _mineralAltFacies; } set { _mineralAltFacies = value; } }
        public string SelectedMineralAltFacies { get { return _selectedMineralAltFacies; } set { _selectedMineralAltFacies = value; } }

        public ObservableCollection<Themes.ComboBoxItem> MineralAltUnit { get { return _mineralAltUnit; } set { _mineralAltUnit = value; } }
        public string SelectedMineralAltUnit { get { return _selectedMineralAltUnit; } set { _selectedMineralAltUnit = value; } }

        #endregion

        public MineralizationAlterationViewModel(FieldNotes inReportModel)
        {
            _mineralAltID = mineralAltIDCalculator.CalculateMineralAlterationID();
            _mineralAltParentID = inReportModel.GenericID;
            _mineralAltAlias = mineralAltIDCalculator.CalculateMineralAlterationAlias(_mineralAltParentID, inReportModel.station.StationAlias);

            existingDataDetailMineralAlt = inReportModel;

            //First order lists
            FillMineralAlterations();
            FillUnit();
            FillMinAltPhase();
            FillMinAltTexture();
            FillMinAltFacies();
            FillMinerals();
            FillDistribution();

        }

        /// <summary>
        /// Will fill the dialog with existing information coming from the database.
        /// </summary>
        /// <param name="incomingData">The model in which the existing information is stored.</param>
        public void AutoFillDialog(FieldNotes incomingData)
        {
            //Keep
            existingDataDetailMineralAlt = incomingData;

            //Set
            _mineralAltID = existingDataDetailMineralAlt.mineralAlteration.MAID;
            _mineralAltNote = existingDataDetailMineralAlt.mineralAlteration.MANotes;
            _mineralAltParentID = existingDataDetailMineralAlt.ParentID;
            _mineralAltAlias = existingDataDetailMineralAlt.mineralAlteration.MAName;

            _selectedMineralAltMA = existingDataDetailMineralAlt.mineralAlteration.MAMA;
            _selectedMineralAltUnit = existingDataDetailMineralAlt.mineralAlteration.MAUnit;
            _selectedMineralAltPhase = existingDataDetailMineralAlt.mineralAlteration.MAPhase;
            _selectedMineralAltTexture = existingDataDetailMineralAlt.mineralAlteration.MATexture;
            _selectedMineralAltFacies = existingDataDetailMineralAlt.mineralAlteration.MAFacies;



            //Update UI
            RaisePropertyChanged("MineralAltID");
            RaisePropertyChanged("MineralAltAlias");
            RaisePropertyChanged("MineralAltNote");
            RaisePropertyChanged("MineralAltParentID");
            RaisePropertyChanged("SelectedMineralAltMA");
            RaisePropertyChanged("SelectedMineralAltUnit");
            RaisePropertyChanged("SelectedMineralAltPhase");
            RaisePropertyChanged("SelectedMineralAltTexture");
            RaisePropertyChanged("SelectedMineralAltFacies");

            //Special case for minerals
            List<object> mineralTableRaw = accessData.ReadTable(mineralModel.GetType(), null);
            IEnumerable<Mineral> mineralTable = mineralTableRaw.Cast<Mineral>(); //Cast to proper list type
            IEnumerable<Mineral> mineralParentEarth = from e in mineralTable where e.MineralMAID == _mineralAltID select e;
            if (mineralParentEarth.Count() != 0 || mineralParentEarth != null)
            {
                foreach (Mineral mns in mineralParentEarth)
                {
                    AddAConcatenatedValue(mns.MineralName, null, Dictionaries.DatabaseLiterals.FieldMineral, false);
                }

            }

            //Update list view
            ConcatenatedCombobox ccBox = new ConcatenatedCombobox();
            foreach (string d in ccBox.UnpipeString(existingDataDetailMineralAlt.mineralAlteration.MADistribute))
            {
                AddADistribution(d);
            }

            RaisePropertyChanged("SelectedMineralAltDist");
            RaisePropertyChanged("MineralAltDistValues");

            doMineralAltUpdate = true;

        }

        /// <summary>
        /// On save event
        /// </summary>
        public void SaveDialogInfo()
        {
            //Get current class information and add to model
            mineralAltModel.MAID = _mineralAltID; //Prime key
            mineralAltModel.MAName = _mineralAltAlias;
            mineralAltModel.MANotes = _mineralAltNote;
            mineralAltModel.MAParentTable = Dictionaries.DatabaseLiterals.TableStation;
            mineralAltModel.MAParentID = _mineralAltParentID;

            if (SelectedMineralAltMA != null)
            {
                mineralAltModel.MAMA = SelectedMineralAltMA;
            }
            if (SelectedMineralAltUnit != null)
            {
                mineralAltModel.MAUnit = SelectedMineralAltUnit;
            }
            if (SelectedMineralAltPhase != null)
            {
                mineralAltModel.MAPhase = SelectedMineralAltPhase;
            }
            if (SelectedMineralAltTexture != null)
            {
                mineralAltModel.MATexture = SelectedMineralAltTexture;
            }
            if (SelectedMineralAltFacies != null)
            {
                mineralAltModel.MAFacies = SelectedMineralAltFacies;
            }

            //process list of values so they are concatenated.
            ConcatenatedCombobox ccBox = new ConcatenatedCombobox();
            mineralAltModel.MADistribute = ccBox.PipeValues(_mineralAltDistValues);

            //Special case for minerals
            if (MineralAltMineralsValues.Count != 0)
            {
                FieldNotes maModelToSave = new FieldNotes();
                maModelToSave.mineralAlteration = mineralAltModel;
                MineralViewModel minVM = new MineralViewModel(maModelToSave);
                List<string> listOfMinerals = new List<string>();

                foreach (Themes.ComboBoxItem mins in MineralAltMineralsValues)
                {
                    //Save only if the mineral was a new added one, prevent duplicates
                    if (mins.canRemoveItem == Windows.UI.Xaml.Visibility.Visible)
                    {
                        listOfMinerals.Add(mins.itemValue);
                    }

                }

                minVM.QuickMineralRecordOnly(existingDataDetailMineralAlt, listOfMinerals, Dictionaries.DatabaseLiterals.TableMineralAlteration);

            }

            //Save model class
            accessData.SaveFromSQLTableObject(mineralAltModel, doMineralAltUpdate);

            //Launch an event call for everyone that an min. alt. has been edited.
            if (newMineralAltEdit != null)
            {
                newMineralAltEdit(this);
            }
        }

        #region FILL

        /// <summary>
        /// Will fill the mineral alterations names combobox
        /// </summary>
        private void FillMinAltPhase()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldMineralAlterationPhase;
            string tableName = Dictionaries.DatabaseLiterals.TableMineralAlteration;
            foreach (var itemType in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedMineralAltPhase))
            {
                _mineralAltPhase.Add(itemType);
            }

            //Update UI
            RaisePropertyChanged("MineralAltPhase");
            RaisePropertyChanged("SelectedMineralAltPhase"); 
        }

        /// <summary>
        /// Will fill the mineral alterations names combobox
        /// </summary>
        private void FillMinAltTexture()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldMineralAlterationTexture;
            string tableName = Dictionaries.DatabaseLiterals.TableMineralAlteration;
            foreach (var itemType in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedMineralAltTexture))
            {
                _mineralAltTexture.Add(itemType);
            }

            //Update UI
            RaisePropertyChanged("MineralAltTexture");
            RaisePropertyChanged("SelectedMineralAltTexture");
        }


        /// <summary>
        /// Will fill the mineral alterations names combobox
        /// </summary>
        private void FillMinAltFacies()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldMineralAlterationFacies;
            string tableName = Dictionaries.DatabaseLiterals.TableMineralAlteration;
            foreach (var itemType in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedMineralAltFacies))
            {
                _mineralAltFacies.Add(itemType);
            }

            //Update UI
            RaisePropertyChanged("MineralAltFacies");
            RaisePropertyChanged("SelectedMineralAltFacies");
        }

        /// <summary>
        /// Will fill the mineral alterations names combobox
        /// </summary>
        private void FillMineralAlterations()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldMineralAlteration;
            string tableName = Dictionaries.DatabaseLiterals.TableMineralAlteration;
            foreach (var itemType in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedMineralAltMA))
            {
                _mineralAltMA.Add(itemType);
            }

            //Update UI
            RaisePropertyChanged("MineralAltMA");
            RaisePropertyChanged("SelectedMineralAltMA"); 
        }

        /// <summary>
        /// Will fill the mineral combobox
        /// </summary>
        private void FillMinerals()
        {
            //Reset
            _mineralAltMinerals.Clear();
            RaisePropertyChanged("MineralAltMinerals");

            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldMineral;
            string tableName = Dictionaries.DatabaseLiterals.TableMineral;

            //Fill in cbox
            foreach (var itemMinerals in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedMineralAltMineral))
            {
                _mineralAltMinerals.Add(itemMinerals);
            }

            //Update UI
            RaisePropertyChanged("MineralAltMinerals");
            //RaisePropertyChanged("SelectedMineralAltMineral");


        }

        /// <summary>
        /// Will fill the mineral alteration distribution combobox
        /// </summary>
        private void FillDistribution()
        {

            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldMineralAlterationDistrubute;
            string tableName = Dictionaries.DatabaseLiterals.TableMineralAlteration;

            //Fill in cbox
            foreach (var itemDist in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedMineralAltDist))
            {
                _mineralAltDist.Add(itemDist);
            }

            //Update UI
            RaisePropertyChanged("MineralAltDist");
            RaisePropertyChanged("SelectedMineralAltDist");
        }

        /// <summary>
        /// Will fill the mineral alteration unit combobox
        /// </summary>
        private void FillUnit()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldMineralAlterationUnit;
            string tableName = Dictionaries.DatabaseLiterals.TableMineralAlteration;
            foreach (var itemType in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedMineralAltUnit))
            {
                _mineralAltUnit.Add(itemType);
            }

            //Update UI
            RaisePropertyChanged("MineralAltUnit");
            RaisePropertyChanged("SelectedMineralAltUnit");
        }

        #endregion

        #region CONCATENATED FIELDS

        /// <summary>
        /// Will refresh the concatenated part of the distribution whenever a value is selected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MineralAlterationDistComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AddADistribution(SelectedMineralAltDist);
        }

        /// <summary>
        /// Will add to the list of purposes a selected purpose by the user.
        /// </summary>
        public void AddADistribution(string distToAdd)
        {

            Themes.ComboBoxItem newDist = new Themes.ComboBoxItem
            {
                itemValue = distToAdd
            };
            foreach (Themes.ComboBoxItem cb in MineralAltDist)
            {
                if (cb.itemValue == distToAdd)
                {
                    newDist.itemName = cb.itemName;
                    break;
                }
            }
            if (newDist.itemName != null && newDist.itemValue != string.Empty)
            {
                bool foundValue = false;
                foreach (Themes.ComboBoxItem existingItems in _mineralAltDistValues)
                {
                    if (distToAdd == existingItems.itemName)
                    {
                        foundValue = true;
                    }
                }
                if (!foundValue)
                {
                    _mineralAltDistValues.Add(newDist);
                }

            }

            RaisePropertyChanged("MineralAltDistValues");
        }

        /// <summary>
        /// Catch submission event to track user input and add it to listview with all terms.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void MAMineralAutoSuggest_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            AutoSuggestBox senderBox = sender as AutoSuggestBox;
            if (args.ChosenSuggestion != null && args.ChosenSuggestion.ToString() != "No results found")
            {
                Themes.ComboBoxItem selectedMineral = args.ChosenSuggestion as Themes.ComboBoxItem;
                AddAConcatenatedValue(selectedMineral.itemValue, senderBox.Name);
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

                if (NameToValidate.ToLower().Contains(Dictionaries.DatabaseLiterals.FieldMineral.ToLower()))
                {
                    parentCollection = MineralAltMinerals;
                    parentConcatCollection = _mineralAltMineralsValues;
                    parentProperty = "MineralAltMineralsValues";

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

        /// <summary>
        /// Will remove a purpose from purpose list
        /// </summary>
        /// <param name="inPurpose"></param>
        public void RemoveSelectedValue(object inPurpose, string parentListViewName)
        {

            Themes.ComboBoxItem oldValue = inPurpose as Themes.ComboBoxItem;

            if (parentListViewName.ToLower().Contains(Dictionaries.DatabaseLiterals.FieldMineralAlterationDistrubute.ToLower()))
            {
                _mineralAltDistValues.Remove(oldValue);
                RaisePropertyChanged("MineralAltDistValues");
            }
            if (parentListViewName.ToLower().Contains(Dictionaries.DatabaseLiterals.FieldMineral.ToLower()))
            {
                _mineralAltMineralsValues.Remove(oldValue);
                RaisePropertyChanged("MineralAltMineralsValues");
            }

        }

        #endregion

        #region EVENTS

        /// <summary>
        /// Whenever a selection occurs in mineralization alteration list, filter child lists.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MineralAlterationsNamesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FillDistribution();
        }

        #endregion
    }
}
