using GSCFieldApp.Models;
using GSCFieldApp.Services.DatabaseServices;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Template10.Mvvm;
using GSCFieldApp.Themes;
using Windows.UI.Xaml.Controls;

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
        private string _mineralAltMode = string.Empty;
        private string _mineralAltResidualText = string.Empty;
        private readonly Dictionary<string, int> _mineralAltResidualModes = new Dictionary<string, int>(); //Will contain mineral Id and it's mode, for residual mode calculation
        private readonly List<string> _mineralAlterations = new List<string>(); //Will contain a list of all mineral alterations related to current parent station. To catch duplicates

        //UI interaction
        public bool doMineralAltUpdate = false;

        private ObservableCollection<Themes.ComboBoxItem> _mineralAltMA = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedMineralAltMA = string.Empty;
        private ObservableCollection<Themes.ComboBoxItem> _mineralAltMinerals = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedMineralAltMineral = string.Empty;
        private ObservableCollection<Themes.ComboBoxItem> _mineralAltDist = new ObservableCollection<Themes.ComboBoxItem>();
        private ObservableCollection<Themes.ComboBoxItem> _mineralAltDistValues = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedMineralAltDist = string.Empty;
        private ObservableCollection<Themes.ComboBoxItem> _mineralAltUnit = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedMineralAltUnit = string.Empty;


        //Model init
        private MineralAlteration mineralAltModel = new MineralAlteration();
        public DataIDCalculation mineralAltIDCalculator = new DataIDCalculation();
        public FieldNotes existingDataDetailMineralAlt;
        readonly DataAccess accessData = new DataAccess();

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
        public string MineralAltResidualText { get { return _mineralAltResidualText; } set { _mineralAltResidualText = value; } }

        public string MineralAltMode
        {
            get
            {
                return _mineralAltMode;
            }
            set
            {
                bool result = int.TryParse(value, out int index);

                if (result)
                {
                    if (index >= 0 && index <= 100)
                    {
                        _mineralAltMode = value;
                    }
                    else
                    {
                        _mineralAltMode = value = "0";
                        RaisePropertyChanged("MineralAltMode");
                    }

                }
                else
                {
                    _mineralAltMode = value = "0";
                    RaisePropertyChanged("MineralAltMode");
                }


            }
        }

        public ObservableCollection<Themes.ComboBoxItem> MineralAltMA { get { return _mineralAltMA; } set { _mineralAltMA = value; } }
        public string SelectedMineralAltMA { get { return _selectedMineralAltMA; } set { _selectedMineralAltMA = value; } }
        public ObservableCollection<Themes.ComboBoxItem> MineralAltMinerals { get { return _mineralAltMinerals; } set { _mineralAltMinerals = value; } }
        public string SelectedMineralAltMineral { get { return _selectedMineralAltMineral; } set { _selectedMineralAltMineral = value; } }
        public ObservableCollection<Themes.ComboBoxItem> MineralAltDist { get { return _mineralAltDist; } set { _mineralAltDist = value; } }
        public ObservableCollection<Themes.ComboBoxItem> MineralAltDistValues { get { return _mineralAltDistValues; } set { _mineralAltDistValues = value; } }
        public string SelectedMineralAltDist { get { return _selectedMineralAltDist; } set { _selectedMineralAltDist = value; } }
        public ObservableCollection<Themes.ComboBoxItem> MineralAltUnit { get { return _mineralAltUnit; } set { _mineralAltUnit = value; } }
        public string SelectedMineralAltUnit { get { return _selectedMineralAltUnit; } set { _selectedMineralAltUnit = value; } }

        #endregion

        public MineralizationAlterationViewModel(FieldNotes inReportModel)
        {
            _mineralAltID = mineralAltIDCalculator.CalculateMineralAlterationID();
            _mineralAltParentID = inReportModel.GenericID;
            _mineralAltAlias = mineralAltIDCalculator.CalculateMineralAlterationAlias(_mineralAltParentID, inReportModel.station.StationAlias);

            existingDataDetailMineralAlt = inReportModel;

            if (existingDataDetailMineralAlt.GenericID != null)
            {
                CalculateResidual();
            }

            //First order lists
            FillMineralAlterations();
            FillUnit();

            //Fill second order comboboxes (dependant on selected litho type)
            //NOTE: needs at least to be initialized and filled at init, else re-selecting an item after init doesn't seem to work.
            FillDistribution();
            FillMinerals();

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
            if (existingDataDetailMineralAlt.mineralAlteration.MAMode != null)
            {
                _mineralAltMode = existingDataDetailMineralAlt.mineralAlteration.MAMode.ToString();
            }
            else
            {
                _mineralAltMode = 0.ToString();
            }

            _selectedMineralAltMA = existingDataDetailMineralAlt.mineralAlteration.MAMA;
            _selectedMineralAltUnit = existingDataDetailMineralAlt.mineralAlteration.MAUnit;



            //Update UI
            RaisePropertyChanged("MineralAltID");
            RaisePropertyChanged("MineralAltAlias");
            RaisePropertyChanged("MineralAltNote");
            RaisePropertyChanged("MineralAltParentID");
            RaisePropertyChanged("MineralAltMode");
            RaisePropertyChanged("SelectedMineralAltMA");
            RaisePropertyChanged("SelectedMineralAltUnit");

            AutoFillDialog2ndRound(incomingData);

            doMineralAltUpdate = true;

        }

        /// <summary>
        /// Will fill the dialog with existing information coming from the database, based user selected mineralization / alteration. This 
        /// round of filling is dependant on a value.
        /// </summary>
        /// <param name="incomingData">The model in which the existing information is stored.</param>
        public void AutoFillDialog2ndRound(FieldNotes incomingData)
        {
            //Refill some comboboxes
            FillDistribution();
            FillMinerals();


            //Keep
            existingDataDetailMineralAlt = incomingData;

            //Set
            _selectedMineralAltMineral = existingDataDetailMineralAlt.mineralAlteration.MAMineral;

            //Clean
            _mineralAltDistValues.Clear();

            //Update list view
            ConcatenatedCombobox ccBox = new ConcatenatedCombobox();
            foreach (string d in ccBox.UnpipeString(existingDataDetailMineralAlt.mineralAlteration.MADistribute))
            {
                AddADistribution(d);
            }

            RaisePropertyChanged("SelectedMineralAltDist");
            RaisePropertyChanged("SelectedMineralAltMineral");
            RaisePropertyChanged("MineralAltDistValues");
        }

        /// <summary>
        /// On save event
        /// </summary>
        public void SaveDialogInfo()
        {
            //Get current class information and add to model
            mineralAltModel.MAID = _mineralAltID; //Prime key
            mineralAltModel.MAName = _mineralAltAlias;
            mineralAltModel.MAMode = _mineralAltMode;
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
            if (SelectedMineralAltMineral != null)
            {
                mineralAltModel.MAMineral = SelectedMineralAltMineral;
            }

            //process list of values so they are concatenated.
            ConcatenatedCombobox ccBox = new ConcatenatedCombobox();
            mineralAltModel.MADistribute = ccBox.PipeValues(_mineralAltDistValues); 

            //Save model class
            accessData.SaveFromSQLTableObject(mineralAltModel, doMineralAltUpdate);

            //Launch an event call for everyone that an min. alt. has been edited.
            if (newMineralAltEdit != null)
            {
                newMineralAltEdit(this);
            }
        }

        #region CALCULATE
        public void CalculateResidual(string newMode = "")
        {
            // Language localization using Resource.resw
            var loadLocalization = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            string Prefix = loadLocalization.GetString("MineralDialogResidualPrefix");
            string MiddleFix = loadLocalization.GetString("MineralDialogResidualMiddlefix");
            string Suffix = loadLocalization.GetString("MineralDialogResidualSuffix");

            List<object> mineralAltTableRaw = accessData.ReadTable(mineralAltModel.GetType(), null);
            IEnumerable<MineralAlteration> mineralAltTable = mineralAltTableRaw.Cast<MineralAlteration>(); //Cast to proper list type

            //Get a list of related mineralization alteration from selected station
            string parentID = existingDataDetailMineralAlt.GenericID;

            //Find proper parent id (request could come from a min. alt.)
            if (existingDataDetailMineralAlt.ParentTableName == Dictionaries.DatabaseLiterals.TableStation)
            {
                parentID = existingDataDetailMineralAlt.ParentID;
            }

            //Find proper parent id (request could come from a mineral or an earthmat selection)
            IEnumerable<MineralAlteration> mineralAltParentEarth = from ma in mineralAltTable where ma.MAParentID == parentID select ma;

            if (_mineralAltResidualModes.Count == 0 && (mineralAltParentEarth.Count() != 0 || mineralAltParentEarth != null))
            {
                foreach (MineralAlteration mns in mineralAltParentEarth)
                {
                    _mineralAlterations.Add(mns.MAName);

                    bool currentModeParsed = int.TryParse(mns.MAMode, out int currentPercentage);

                    if (mns.MAID == existingDataDetailMineralAlt.GenericID)
                    {
                        if (newMode != string.Empty)
                        {
                            currentModeParsed = int.TryParse(newMode, out currentPercentage);
                        }

                        if (currentModeParsed)
                        {
                            _mineralAltResidualModes[mns.MAID] = currentPercentage;
                        }

                    }
                    else
                    {
                        if (currentModeParsed)
                        {
                            _mineralAltResidualModes[mns.MAID] = currentPercentage;
                        }

                    }

                }

                if (_mineralAltResidualModes.Count() == 0)
                {
                    bool currentModeParsed = int.TryParse(newMode, out int currentPercentage);
                    _mineralAltResidualModes[existingDataDetailMineralAlt.GenericID] = currentPercentage;
                }

            }
            else
            {
                bool currentModeParsed = int.TryParse(newMode, out int currentPercentage);
                _mineralAltResidualModes[existingDataDetailMineralAlt.GenericID] = currentPercentage;
            }


            //Calculate total percentage
            int _mineralResidualMode = 0;
            foreach (KeyValuePair<string, int> modes in _mineralAltResidualModes)
            {
                _mineralResidualMode = _mineralResidualMode + modes.Value;
            }
            _mineralAltResidualText = Prefix + _mineralResidualMode.ToString() + MiddleFix + _mineralAltResidualModes.Count().ToString() + Suffix;
            RaisePropertyChanged("MineralAltResidualText");

        }
        #endregion

        #region FILL

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
            string fieldName = Dictionaries.DatabaseLiterals.FieldMineralAlterationMineral;
            string tableName = Dictionaries.DatabaseLiterals.TableMineralAlteration;


            //Get min.alt. value
            List<Vocabularies> mineralsFromMA = new List<Vocabularies>();

            if (_selectedMineralAltMA != string.Empty && _selectedMineralAltMA != Dictionaries.DatabaseLiterals.DefaultNoData)
            {
                mineralsFromMA = accessData.GetPicklistValuesFromParent(tableName, fieldName, _selectedMineralAltMA, false).ToList();
            }


            //Fill in cbox
            foreach (var itemMinerals in accessData.GetComboboxListFromVocab(mineralsFromMA, out _selectedMineralAltMineral))
            {
                _mineralAltMinerals.Add(itemMinerals);
            }

            //Update UI
            RaisePropertyChanged("MineralAltMinerals");
            RaisePropertyChanged("SelectedMineralAltMineral");


        }

        /// <summary>
        /// Will fill the mineral alteration distribution combobox
        /// </summary>
        private void FillDistribution()
        {

            //Reset
            _mineralAltDist.Clear();
            _mineralAltDistValues.Clear();
            RaisePropertyChanged("MineralAltDist");
            RaisePropertyChanged("MineralAltDistValues");

            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldMineralAlterationDistrubute;
            string tableName = Dictionaries.DatabaseLiterals.TableMineralAlteration;

            //Get min.alt. value
            List<Vocabularies> distFromMA = new List<Vocabularies>();

            if (_selectedMineralAltMA != string.Empty && _selectedMineralAltMA != Dictionaries.DatabaseLiterals.DefaultNoData)
            {
                distFromMA = accessData.GetPicklistValuesFromParent(tableName, fieldName, _selectedMineralAltMA, false).ToList();
            }


            //Fill in cbox
            foreach (var itemDist in accessData.GetComboboxListFromVocab(distFromMA, out _selectedMineralAltDist))
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
        /// Will remove a distribution from distribution list
        /// </summary>
        /// <param name="inDist"></param>
        public void RemoveSelectedDistribution(object inDist)
        {

            Themes.ComboBoxItem oldDist = inDist as Themes.ComboBoxItem;
            _mineralAltDistValues.Remove(oldDist);

            RaisePropertyChanged("MineralAltDistValues");
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

        #endregion

        #region EVENTS

        /// <summary>
        /// Will initiate a new calculate on the mode dynamic text in the UI that will show on many % is left
        /// from user entered mode value.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MineralAltModeNumBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox senderBox = sender as TextBox;
            CalculateResidual(senderBox.Text);
        }

        /// <summary>
        /// Whenever a selection occurs in mineralization alteration list, filter child lists.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MineralAlterationsNamesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FillDistribution();
            FillMinerals();
        }

        #endregion
    }
}
