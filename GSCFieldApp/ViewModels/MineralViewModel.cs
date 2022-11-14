using GSCFieldApp.Dictionaries;
using GSCFieldApp.Models;
using GSCFieldApp.Services.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;
using Windows.Gaming.Input.ForceFeedback;
using Windows.UI.Xaml.Controls;

namespace GSCFieldApp.ViewModels
{
    public class MineralViewModel: ViewModelBase
    {
        #region INITIALIZATION

        private string _groupTypeDetail = string.Empty;

        //UI default values
        private string _mineralAlias = string.Empty;
        private string _mineralID = string.Empty;
        private string _mineralParentID = string.Empty;
        private string _mineralParentName = string.Empty;
        private string _mineralParentAlias = string.Empty;
        private string _mineralNote = string.Empty;
        private string _mineralSizeMin = string.Empty;
        private string _mineralSizeMax = string.Empty;
        private string _mineralMode = string.Empty;
        private string _mineralResidualText = string.Empty;
        private string _mineralName = string.Empty;

        private Dictionary<string, int> _mineralResidualModes = new Dictionary<string, int>(); //Will contain mineral Id and it's mode, for residual mode calculation
        private List<string> _minerals = new List<string>(); //Will contain a list of all minerals related to current parent earthmat. To catch duplicates
    
        //UI interaction
        public bool doMineralUpdate = false;

        private ObservableCollection<Themes.ComboBoxItem> _mineralColor = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedMineralColor = string.Empty;
        private ObservableCollection<Themes.ComboBoxItem> _mineralFormHabit = new ObservableCollection<Themes.ComboBoxItem>();
        private ObservableCollection<Themes.ComboBoxItem> _mineralFormHabitValues = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedMineralFormHabit = string.Empty;
        private ObservableCollection<Themes.ComboBoxItem> _mineralOccur = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedMineralOccur = string.Empty;
        private ObservableCollection<Themes.ComboBoxItem> _mineralModeText = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedMineralModeText = string.Empty;

        //Model init
        private Mineral mineralModel = new Mineral();
        public DataIDCalculation mineralIDCalculator = new DataIDCalculation();
        public FieldNotes existingDataDetailMineral;
        DataAccess accessData = new DataAccess();

        //Events and delegate
        public delegate void mineralEditEventHandler(object sender); //A delegate for execution events
        public event mineralEditEventHandler newMineralEdit; //This event is triggered when a save has been done on station table. 

        #endregion

        #region PROPERTIES

        public Mineral MineralModel { get { return mineralModel; } set { mineralModel = value; } }
        public string MineralAlias { get { return _mineralAlias; } set { _mineralAlias = value; } }
        public string MineralNote { get { return _mineralNote; } set { _mineralNote = value; } }
        public string MineralID { get { return _mineralID; } set { _mineralID = value; } }
        public string MineralParentID { get { return _mineralParentID; } set { _mineralParentID = value; } }
        public string MineralResidualText { get { return _mineralResidualText; } set { _mineralResidualText = value; } }
        public string MineralName { get { return _mineralName; } set { _mineralName = value; } }
        public string MineralSizeMin
        {
            get
            {
                return _mineralSizeMin;
            }
            set
            {
                int index;
                bool result = int.TryParse(value, out index);

                if (result)
                {
                    if (index >= 0 && index <= 9999)
                    {
                        _mineralSizeMin = value;
                    }
                    else
                    {
                        _mineralSizeMin = value = "0";
                        RaisePropertyChanged("MineralSizeMin");
                    }

                }
                else
                {
                    _mineralSizeMin = value = "0";
                    RaisePropertyChanged("MineralSizeMin");
                }


            }

        }
        public string MineralSizeMax
        {
            get
            {
                return _mineralSizeMax;
            }
            set
            {
                int index;
                bool result = int.TryParse(value, out index);

                if (result)
                {
                    if (index >= 0 && index <= 9999)
                    {
                        _mineralSizeMax = value;
                    }
                    else
                    {
                        _mineralSizeMax = value = "0";
                        RaisePropertyChanged("MineralSizeMax");
                    }

                }
                else
                {
                    _mineralSizeMax = value = "0";
                    RaisePropertyChanged("MineralSizeMax");
                }


            }
        }


        public ObservableCollection<Themes.ComboBoxItem> MineralColour { get { return _mineralColor; } set { _mineralColor = value; } }
        public string SelectedMineralColor { get { return _selectedMineralColor; } set { _selectedMineralColor = value; } }
        public ObservableCollection<Themes.ComboBoxItem> MineralFormHabit { get { return _mineralFormHabit; } set { _mineralFormHabit = value; } }
        public ObservableCollection<Themes.ComboBoxItem> MineralFormHabitValues { get { return _mineralFormHabitValues; } set { _mineralFormHabitValues = value; } }
        public string SelectedMineralFormHabit { get { return _selectedMineralFormHabit; } set { _selectedMineralFormHabit = value; } }
        public ObservableCollection<Themes.ComboBoxItem> MineralOccur{ get { return _mineralOccur; } set { _mineralOccur = value; } }
        public string SelectedMineralOccur { get { return _selectedMineralOccur; } set { _selectedMineralOccur = value; } }
        public ObservableCollection<Themes.ComboBoxItem> MineralModeText { get { return _mineralModeText; } set { _mineralModeText = value; } }
        public string SelectedMineralModeText { get { return _selectedMineralModeText; } set { _selectedMineralModeText = value; } }
        #endregion

        public MineralViewModel(FieldNotes inReportModel)
        {
            //On init for new samples calculates values for default UI form
            if (inReportModel.GenericTableName == Dictionaries.DatabaseLiterals.TableEarthMat || inReportModel.GenericTableName == Dictionaries.DatabaseLiterals.TableMineralAlteration)
            {
                _mineralParentID = inReportModel.GenericID;
                _mineralParentAlias = inReportModel.GenericAliasName;
                _mineralParentName = inReportModel.GenericTableName;
            }
            else if (inReportModel.earthmat.EarthMatID != null) //Case mineral is created from earthmat dialog
            {
                _mineralParentID = inReportModel.earthmat.EarthMatID;
                _mineralParentAlias = inReportModel.earthmat.EarthMatName;
                _mineralParentName = Dictionaries.DatabaseLiterals.TableEarthMat;
            }
            else if (inReportModel.mineralAlteration.MAID != null) //Case mineral is created from mineral alteration dialog
            {
                _mineralParentID = inReportModel.mineralAlteration.MAID;
                _mineralParentAlias = inReportModel.mineralAlteration.MAName;
                _mineralParentName = Dictionaries.DatabaseLiterals.TableMineralAlteration;
            }

            _mineralID = mineralIDCalculator.CalculateMineralID();
            _mineralAlias = mineralIDCalculator.CalculateMineralAlias(_mineralParentID, _mineralParentAlias);

            existingDataDetailMineral = inReportModel;

            if (existingDataDetailMineral.GenericID != null)
            {
                CalculateResidual();
            }
            
            FillColour();
            FillForm();
            FillOccur();
            FillMode();
        }

        public void InitFill2ndRound(string fullMineralText)
        {
            _groupTypeDetail = fullMineralText;
            RaisePropertyChanged("GroupTypeDetail");
        }

        /// <summary>
        /// Will fill the dialog with existing information coming from the database.
        /// </summary>
        /// <param name="incomingData">The model in which the existing information is stored.</param>
        public void AutoFillDialog(FieldNotes incomingData)
        {
            //Keep
            existingDataDetailMineral = incomingData;

            //Set
            _mineralID = existingDataDetailMineral.mineral.MineralID;
            _mineralNote = existingDataDetailMineral.mineral.MineralNote;
            if (existingDataDetailMineral.mineral.MineralEMID != null)
            {
                _mineralParentID = existingDataDetailMineral.mineral.MineralEMID;
            }
            else if (existingDataDetailMineral.mineral.MineralMAID != null)
            {
                _mineralParentID = existingDataDetailMineral.mineral.MineralMAID;
            }
            
            _mineralAlias = existingDataDetailMineral.mineral.MineralIDName;
            _mineralName = existingDataDetailMineral.mineral.MineralName;

            if (existingDataDetailMineral.mineral.MineralSizeMax != null)
            {
                _mineralSizeMax = existingDataDetailMineral.mineral.MineralSizeMax.ToString();
            }
            else
            {
                _mineralSizeMax = 0.ToString();
            }

            if (existingDataDetailMineral.mineral.MineralSizeMin != null)
            {
                _mineralSizeMin = existingDataDetailMineral.mineral.MineralSizeMin.ToString();
            }
            else
            {
                _mineralSizeMin = 0.ToString();
            }

            _selectedMineralModeText = existingDataDetailMineral.mineral.MineralMode;
            _selectedMineralColor = existingDataDetailMineral.mineral.MineralColour;
            _selectedMineralFormHabit = existingDataDetailMineral.mineral.MineralFormHabit;
            _selectedMineralOccur = existingDataDetailMineral.mineral.MineralOccur;

            //Concatenated box
            Themes.ConcatenatedCombobox ccBox = new Themes.ConcatenatedCombobox();
            foreach (string d in ccBox.UnpipeString(existingDataDetailMineral.mineral.MineralFormHabit))
            {
                AddAConcatenatedValue(d, "MineralFormHabit");
            }

            //Update UI
            RaisePropertyChanged("MineralID");
            RaisePropertyChanged("MineralAlias");
            RaisePropertyChanged("MineralNote");
            RaisePropertyChanged("MineralEarthmatID");
            RaisePropertyChanged("MineralMode");
            RaisePropertyChanged("MineralSizeMin");
            RaisePropertyChanged("MineralSizeMax");
            RaisePropertyChanged("MineralName");

            RaisePropertyChanged("SelectedMineralColor");
            RaisePropertyChanged("SelectedMineralFormHabit");
            RaisePropertyChanged("SelectedMineralOccur");

            RaisePropertyChanged("SelectedMineralModeText");

            doMineralUpdate = true;
        }

        /// <summary>
        /// On save event
        /// </summary>
        public void SaveDialogInfo()
        {
            //Get combobox contact
            Themes.ConcatenatedCombobox concat = new Themes.ConcatenatedCombobox();

            //Get current class information and add to model
            mineralModel.MineralID = _mineralID; //Prime key
            mineralModel.MineralNote = _mineralNote;
            mineralModel.MineralIDName = _mineralAlias;
            mineralModel.MineralSizeMax = _mineralSizeMax;
            mineralModel.MineralSizeMin = _mineralSizeMin;
            mineralModel.MineralName = _mineralName;

            if (_mineralParentName == DatabaseLiterals.TableEarthMat)
            {
                mineralModel.MineralEMID = _mineralParentID;
            }

            if (_mineralParentName == DatabaseLiterals.TableMineralAlteration)
            {
                mineralModel.MineralMAID = _mineralParentID;
            }

            if (SelectedMineralColor != null)
            {
                mineralModel.MineralColour = SelectedMineralColor;
            }
            if (SelectedMineralFormHabit != null)
            {
                mineralModel.MineralFormHabit = SelectedMineralFormHabit;
            }
            if (SelectedMineralOccur != null)
            {
                mineralModel.MineralOccur = SelectedMineralOccur;
            }
            if (SelectedMineralModeText != null)
            {
                mineralModel.MineralMode = SelectedMineralModeText;
            }

            if (SelectedMineralFormHabit != null)
            {
                mineralModel.MineralFormHabit = concat.PipeValues(_mineralFormHabitValues); //process list of values so they are concatenated.
            }

            //Save model class
            accessData.SaveFromSQLTableObject(mineralModel, doMineralUpdate);

            //Launch an event call for everyone that an earthmat has been edited.
            if (newMineralEdit != null)
            {
                newMineralEdit(this);
            }
        }

        #region FILL

        private void FillMode()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldMineralMode;
            string tableName = Dictionaries.DatabaseLiterals.TableMineral;
            foreach (var itemType in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedMineralModeText))
            {
                _mineralModeText.Add(itemType);
            }

            //Update UI
            RaisePropertyChanged("MineralMode");
            RaisePropertyChanged("SelectedMineralModeText");
        }


        /// <summary>
        /// Will fill the mineral colour type combobox
        /// </summary>
        private void FillColour()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldMineralColour;
            string tableName = Dictionaries.DatabaseLiterals.TableMineral;
            foreach (var itemType in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedMineralColor))
            {
                _mineralColor.Add(itemType);
            }

            //Update UI
            RaisePropertyChanged("MineralColour");
            RaisePropertyChanged("SelectedMineralColor");
        }

        /// <summary>
        /// Will fill the mineral forms type combobox
        /// </summary>
        private void FillForm()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldMineralFormHabit;
            string tableName = Dictionaries.DatabaseLiterals.TableMineral;
            foreach (var itemType in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedMineralFormHabit))
            {
                _mineralFormHabit.Add(itemType);
            }

            //Update UI
            RaisePropertyChanged("MineralFormHabit");
            RaisePropertyChanged("SelectedMineralFormHabit"); 
        }

        /// <summary>
        /// Will fill the mineral occurences type combobox
        /// </summary>
        private void FillOccur()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldMineralOccurence;
            string tableName = Dictionaries.DatabaseLiterals.TableMineral;
            foreach (var itemType in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedMineralOccur))
            {
                _mineralOccur.Add(itemType);
            }

            //Update UI
            RaisePropertyChanged("MineralOccur");
            RaisePropertyChanged("SelectedMineralOccur"); 
        }

        #endregion

        #region CALCULATE
        public void CalculateResidual(string newMode = "")
        {
            // Language localization using Resource.resw
            var loadLocalization = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            string Prefix = loadLocalization.GetString("MineralDialogResidualPrefix");
            string MiddleFix = loadLocalization.GetString("MineralDialogResidualMiddlefix");
            string Suffix = loadLocalization.GetString("MineralDialogResidualSuffix");

            List<object> mineralTableRaw = accessData.ReadTable(mineralModel.GetType(), null);
            IEnumerable<Mineral> mineralTable = mineralTableRaw.Cast<Mineral>(); //Cast to proper list type

            //Get a list of related mineral from selected earthmat
            string parentID = existingDataDetailMineral.ParentID;

            //Find proper parent id (request could come from a mineral or an earthmat selection or a minerlization alteration)
            if (existingDataDetailMineral.ParentTableName == Dictionaries.DatabaseLiterals.TableStation)
            {
                parentID = existingDataDetailMineral.GenericID;
            }
            else if (existingDataDetailMineral.ParentTableName == Dictionaries.DatabaseLiterals.TableMineralAlteration)
            {
                parentID = existingDataDetailMineral.mineral.MineralMAID;
            }
            IEnumerable<Mineral> mineralParentEarth = from e in mineralTable where e.MineralEMID == parentID || e.MineralMAID == parentID select e;

            if (_mineralResidualModes.Count == 0 && (mineralParentEarth.Count() != 0 || mineralParentEarth != null))
            {
                foreach (Mineral mns in mineralParentEarth)
                {
                    _minerals.Add(mns.MineralName);

                    int currentPercentage = 0;
                    bool currentModeParsed = int.TryParse(mns.MineralMode, out currentPercentage);

                    if (currentModeParsed)
                    {
                        if (mns.MineralID == existingDataDetailMineral.GenericID)
                        {
                            if (newMode != string.Empty)
                            {
                                currentModeParsed = int.TryParse(newMode, out currentPercentage);
                            }

                            if (currentModeParsed)
                            {
                                _mineralResidualModes[mns.MineralID] = currentPercentage;
                            }

                        }
                        else
                        {
                            if (currentModeParsed)
                            {
                                _mineralResidualModes[mns.MineralID] = currentPercentage;
                            }

                        }
                    }

                   
                }

                if (_mineralResidualModes.Count() == 0)
                {
                    int currentPercentage = 0;
                    bool currentModeParsed = int.TryParse(newMode, out currentPercentage);
                    _mineralResidualModes[existingDataDetailMineral.GenericID] = currentPercentage;
                }

            }
            else
            {
                int currentPercentage = 0;
                bool currentModeParsed = int.TryParse(newMode, out currentPercentage);
                _mineralResidualModes[existingDataDetailMineral.GenericID] = currentPercentage;
            }


            //Calculate total percentage
            int _mineralResidualMode = 0;
            foreach (KeyValuePair<string, int> modes in _mineralResidualModes)
            {
                _mineralResidualMode = _mineralResidualMode + modes.Value;
            }
            _mineralResidualText = Prefix + _mineralResidualMode.ToString() + MiddleFix + _mineralResidualModes.Count().ToString() + Suffix;
            RaisePropertyChanged("MineralResidualText");

        }
        #endregion

        #region QUICKIE

        /// <summary>
        /// Will create a quick mineral record inside mineral table, from a given parent report model and mineral name, no eartmat will be created in the process
        /// </summary>
        /// <param name="parentModel">Earhtmat parent model</param>
        /// <param name="inMineralName">The new mineral name to add inside table</param>
        /// <returns>A detail report class</returns>
        public void QuickMineralRecordOnly(FieldNotes parentModel, string inMineralName, string parentName)
        {
            if (!_minerals.Contains(inMineralName))
            {
                //Get current class information and add to model
                if (parentName == DatabaseLiterals.TableEarthMat)
                {
                    mineralModel.MineralEMID = _mineralParentID; //Foreigh key
                }
                else if (parentName == DatabaseLiterals.TableMineral)
                {
                    mineralModel.MineralMAID = _mineralParentID; //Foreigh key
                }
               
                mineralModel.MineralIDName = _mineralAlias;
                mineralModel.MineralID = _mineralID; //Prime key
                mineralModel.MineralName = inMineralName;
                mineralModel.MineralSizeMax = 0.ToString();
                mineralModel.MineralSizeMin = 0.ToString();

                //Save model class
                accessData.SaveFromSQLTableObject(mineralModel, false);
            }
        }

        #endregion

        #region EVENTS
        public void MineralModeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox senderBox = sender as ComboBox;
            if (senderBox.SelectedIndex != -1)
            {
                Themes.ComboBoxItem senderItem = senderBox.SelectedItem as Themes.ComboBoxItem;
                CalculateResidual(senderItem.itemValue);
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

        #endregion

        #region METHODS

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

                if (NameToValidate.ToLower().Contains(Dictionaries.DatabaseLiterals.FieldMineralFormDeprecated.ToLower()))
                {
                    parentCollection = MineralFormHabit;
                    parentConcatCollection = _mineralFormHabitValues;
                    parentProperty = "MineralFormHabit";

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
        /// Will remove a category
        /// </summary>
        /// <param name="inPurpose"></param>
        public void RemoveSelectedValue(object inPurpose, string parentListViewName)
        {

            Themes.ComboBoxItem oldValue = inPurpose as Themes.ComboBoxItem;

            if (parentListViewName.ToLower().Contains(Dictionaries.DatabaseLiterals.FieldMineralFormDeprecated.ToLower()))
            {
                _mineralFormHabitValues.Remove(oldValue);
                RaisePropertyChanged("MineralFormHabitValues");
            }

        }

        #endregion
    }
}
