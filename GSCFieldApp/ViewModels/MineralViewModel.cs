﻿using GSCFieldApp.Dictionaries;
using GSCFieldApp.Models;
using GSCFieldApp.Services.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Template10.Mvvm;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace GSCFieldApp.ViewModels
{
    public class MineralViewModel : ViewModelBase
    {
        #region INITIALIZATION

        private string _groupTypeDetail = string.Empty;

        //UI default values
        private string _mineralAlias = string.Empty;
        private int _mineralID = 0;
        private int? _mineralParentID = null;
        private string _mineralParentName = string.Empty;
        private string _mineralParentAlias = string.Empty;
        private string _mineralNote = string.Empty;
        private string _mineralSizeMin = string.Empty;
        private string _mineralSizeMax = string.Empty;
        private readonly string _mineralMode = string.Empty;
        private string _mineralResidualText = string.Empty;
        private string _mineralName = string.Empty;
        private int? _mineralMAID = null;
        private int? _mineralEMID = null;

        private readonly List<int> _mineralResidualModes = new List<int>(); //Will contain mineral Id and it's mode, for residual mode calculation
        private readonly List<string> _minerals = new List<string>(); //Will contain a list of all minerals related to current parent earthmat. To catch duplicates
        private readonly string resourcenameErrorColor = "ErrorColor";
        private readonly string resourcenameBlackColor = "DefaultForegroundColor";

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
        private SolidColorBrush _residualTextForeground = new SolidColorBrush();
        //Model init
        private Mineral mineralModel = new Mineral();
        public DataIDCalculation mineralIDCalculator = new DataIDCalculation();
        public FieldNotes existingDataDetailMineral;
        readonly DataAccess accessData = new DataAccess();

        //Events and delegate
        public delegate void mineralEditEventHandler(object sender); //A delegate for execution events
        public event mineralEditEventHandler newMineralEdit; //This event is triggered when a save has been done on station table. 

        #endregion

        #region PROPERTIES
        public SolidColorBrush ResidualTextForeground { get { return _residualTextForeground; } set { _residualTextForeground = value; } }
        public Mineral MineralModel { get { return mineralModel; } set { mineralModel = value; } }
        public string MineralAlias { get { return _mineralAlias; } set { _mineralAlias = value; } }
        public string MineralNote { get { return _mineralNote; } set { _mineralNote = value; } }
        public int MineralID { get { return _mineralID; } set { _mineralID = value; } }
        public int? MineralParentID { get { return _mineralParentID; } set { _mineralParentID = value; } }
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
                bool result = int.TryParse(value, out int index);

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
                bool result = int.TryParse(value, out int index);

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

        public string MineralParentName { get { return _mineralParentName; } set { _mineralParentName = value; } }
        public int? MineralEMID { get { return _mineralEMID; } set { _mineralEMID = value; } }
        public int? MineralMAID { get { return _mineralMAID; } set { _mineralMAID = value; } }
        public ObservableCollection<Themes.ComboBoxItem> MineralColour { get { return _mineralColor; } set { _mineralColor = value; } }
        public string SelectedMineralColor { get { return _selectedMineralColor; } set { _selectedMineralColor = value; } }
        public ObservableCollection<Themes.ComboBoxItem> MineralFormHabit { get { return _mineralFormHabit; } set { _mineralFormHabit = value; } }
        public ObservableCollection<Themes.ComboBoxItem> MineralFormHabitValues { get { return _mineralFormHabitValues; } set { _mineralFormHabitValues = value; } }
        public string SelectedMineralFormHabit { get { return _selectedMineralFormHabit; } set { _selectedMineralFormHabit = value; } }
        public ObservableCollection<Themes.ComboBoxItem> MineralOccur { get { return _mineralOccur; } set { _mineralOccur = value; } }
        public string SelectedMineralOccur { get { return _selectedMineralOccur; } set { _selectedMineralOccur = value; } }
        public ObservableCollection<Themes.ComboBoxItem> MineralModeText { get { return _mineralModeText; } set { _mineralModeText = value; } }
        public string SelectedMineralModeText { get { return _selectedMineralModeText; } set { _selectedMineralModeText = value; } }
        #endregion

        public MineralViewModel(FieldNotes inReportModel, bool forQuick = false)
        {
            Color stc = new Color();

            if (Application.Current.Resources[resourcenameBlackColor] != null)
            {
                stc = (Color)Application.Current.Resources[resourcenameBlackColor];
            }

            _residualTextForeground.Color = stc;
            RaisePropertyChanged("ResidualTextForeground"); 

            //On init for new samples calculates values for default UI form
            if (inReportModel.GenericTableName == Dictionaries.DatabaseLiterals.TableEarthMat || inReportModel.GenericTableName == Dictionaries.DatabaseLiterals.TableMineralAlteration)
            {
                _mineralParentID = inReportModel.GenericID;
                _mineralParentAlias = inReportModel.GenericAliasName;
                _mineralParentName = inReportModel.GenericTableName;

                if (inReportModel.GenericTableName == Dictionaries.DatabaseLiterals.TableEarthMat)
                {
                    _mineralEMID = inReportModel.earthmat.EarthMatID;
                }
                else if (inReportModel.GenericTableName == Dictionaries.DatabaseLiterals.TableMineralAlteration)
                {
                    _mineralMAID = inReportModel.mineralAlteration.MAID;
                }
            }
            else if (inReportModel.earthmat.EarthMatID != 0) //Case mineral is created from earthmat dialog
            {
                _mineralParentID = inReportModel.earthmat.EarthMatID;
                _mineralParentAlias = inReportModel.earthmat.EarthMatName;
                _mineralParentName = Dictionaries.DatabaseLiterals.TableEarthMat;
                _mineralEMID = inReportModel.earthmat.EarthMatID;
            }
            else if (inReportModel.mineralAlteration.MAID != 0) //Case mineral is created from mineral alteration dialog
            {
                _mineralParentID = inReportModel.mineralAlteration.MAID;
                _mineralParentAlias = inReportModel.mineralAlteration.MAName;
                _mineralParentName = Dictionaries.DatabaseLiterals.TableMineralAlteration;
                _mineralMAID = inReportModel.mineralAlteration.MAID;
            }

            if (!forQuick)
            {
                //_mineralID = mineralIDCalculator.CalculateMineralID();
                _mineralAlias = mineralIDCalculator.CalculateMineralAlias(_mineralParentID, _mineralParentAlias);

                existingDataDetailMineral = inReportModel;

                if (existingDataDetailMineral.GenericID != 0)
                {
                    CalculateResidual();
                }

                FillColour();
                FillForm();
                FillOccur();
                FillMode();
            }

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

            //Set parent
            if (existingDataDetailMineral.mineral.MineralEMID != null)
            {
                _mineralParentID = existingDataDetailMineral.mineral.MineralEMID;
                _mineralParentName = DatabaseLiterals.TableEarthMat;
                _mineralEMID = existingDataDetailMineral.mineral.MineralEMID;
            }
            else if (existingDataDetailMineral.mineral.MineralMAID != 0)
            {
                _mineralParentID = existingDataDetailMineral.mineral.MineralMAID;
                _mineralParentName = DatabaseLiterals.TableMineralAlteration;
                _mineralMAID = existingDataDetailMineral.mineral.MineralMAID;
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
            RaisePropertyChanged("MineralParentID");
            RaisePropertyChanged("MineralParentName");
            RaisePropertyChanged("MineralMAID");
            RaisePropertyChanged("MineralEMID");

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

            mineralModel.MineralIDName = _mineralAlias;

            if (_mineralNote != null && _mineralNote != String.Empty)
            {
                mineralModel.MineralNote = _mineralNote;
            }

            mineralModel.MineralName = _mineralName;

            if (_mineralEMID != 0)
            {
                mineralModel.MineralEMID = _mineralEMID;
            }
            if (_mineralMAID != 0)
            {
                mineralModel.MineralMAID = _mineralMAID;
            }

            //integer parsing
            if (_mineralSizeMax != null && _mineralSizeMax != String.Empty)
            {
                mineralModel.MineralSizeMax = _mineralSizeMax;
            }
            if (_mineralSizeMin != null && _mineralSizeMin != String.Empty)
            {
                mineralModel.MineralSizeMin = _mineralSizeMin;
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
            object mineralOject = (object)mineralModel;
            accessData.SaveFromSQLTableObject(ref mineralOject, doMineralUpdate);
            mineralModel = (Mineral)mineralOject;

            //accessData.SaveFromSQLTableObject(mineralModel, doMineralUpdate);

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
            _mineralResidualModes.Clear();

            // Language localization using Resource.resw
            var loadLocalization = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            string Prefix = loadLocalization.GetString("MineralDialogResidualPrefix");
            string MiddleFix = loadLocalization.GetString("MineralDialogResidualMiddlefix");
            string Suffix = loadLocalization.GetString("MineralDialogResidualSuffix");

            List<object> mineralTableRaw = accessData.ReadTable(mineralModel.GetType(), null);
            IEnumerable<Mineral> mineralTable = mineralTableRaw.Cast<Mineral>(); //Cast to proper list type

            //Get a list of related mineral from selected earthmat
            int parentID = existingDataDetailMineral.ParentID;


            //Filter with proper parents
            IEnumerable<Mineral> mineralParent = from e in mineralTable where e.MineralEMID == parentID select e;
            if (existingDataDetailMineral.ParentTableName == DatabaseLiterals.TableMineralAlteration)
            {
                mineralParent = from e in mineralTable where e.MineralMAID == parentID select e;
            }
            else if (existingDataDetailMineral.GenericTableName == DatabaseLiterals.TableMineralAlteration)
            {
                mineralParent = from e in mineralTable where e.MineralMAID == existingDataDetailMineral.GenericID select e;
            }
            else if (existingDataDetailMineral.GenericTableName == DatabaseLiterals.TableEarthMat)
            {
                mineralParent = from e in mineralTable where e.MineralEMID == existingDataDetailMineral.GenericID select e;
            }

            if (_mineralResidualModes.Count == 0 && (mineralParent.Count() != 0 || mineralParent != null))
            {
                foreach (Mineral mns in mineralParent)
                {
                    // _minerals.Add(ets.EarthMatID);

                    int currentPercentage = 0;
                    if (mns.MineralMode != null)
                    {
                        int.TryParse(mns.MineralMode, out currentPercentage);
                    }
                    ;
                    bool currentPercentParsed = true;
                    if (mns.MineralID == existingDataDetailMineral.GenericID)
                    {
                        if (newMode != string.Empty)
                        {
                            currentPercentParsed = int.TryParse(newMode, out currentPercentage);
                        }

                        if (currentPercentParsed && currentPercentage != 0)
                        {
                            _mineralResidualModes.Add(currentPercentage);
                        }

                    }
                    else
                    {
                        if (currentPercentParsed && currentPercentage != 0)
                        {
                            _mineralResidualModes.Add(currentPercentage);
                        }
                    }
                }

                //if (_mineralResidualModes.Count() == 0)
                //{
                //    int currentPercentage = 0;
                //    bool currentModeParsed = int.TryParse(newMode, out currentPercentage);
                //    _mineralResidualModes.Add(currentPercentage);
                //}

            }
            else
            {
                int currentPercentage = 0;
                bool currentModeParsed = int.TryParse(newMode, out currentPercentage);
                _mineralResidualModes.Add(currentPercentage);
            }

            if (newMode != string.Empty)
            {
                int currentPercentage = 0;
                bool currentModeParsed = int.TryParse(newMode, out currentPercentage);
                _mineralResidualModes.Add(currentPercentage);
            }

            //Calculate total percentage
            int _mineralResidualMode = 0;
            foreach (int modes in _mineralResidualModes)
            {
                _mineralResidualMode = _mineralResidualMode + modes;
            }
            _mineralResidualText = Prefix + _mineralResidualMode.ToString() + MiddleFix + _mineralResidualModes.Count().ToString() + Suffix;
            RaisePropertyChanged("MineralResidualText");

            //Validate over percentage
            Color stc = new Color();
            if (_mineralResidualMode > 100)
            {
                
                if (Application.Current.Resources[resourcenameErrorColor] != null)
                {
                    stc = (Color)Application.Current.Resources[resourcenameErrorColor];
                }

                _residualTextForeground.Color = stc;
            }
            else
            {
                if (Application.Current.Resources[resourcenameBlackColor] != null)
                {
                    stc = (Color)Application.Current.Resources[resourcenameBlackColor];
                }

                _residualTextForeground.Color = stc;
            }

            RaisePropertyChanged("ResidualTextForeground");

        }
        #endregion

        #region QUICKIE

        /// <summary>
        /// Will create a quick mineral record inside mineral table, from a given parent report model and mineral name, no eartmat will be created in the process
        /// </summary>
        /// <param name="parentModel">Earhtmat parent model</param>
        /// <param name="inMineralName">The new mineral name to add inside table</param>
        /// <returns>A detail report class</returns>
        public void QuickMineralRecordOnly(int parentID, List<string> inMineralNames, string parentName)
        {
            if (inMineralNames.Count() > 0)
            {
                List<object> mineralObjects = new List<object>();

                foreach (string inMinName in inMineralNames)
                {
                    Mineral newMineral = new Mineral();
                    //Get current class information and add to model
                    _mineralParentID = parentID;
                    if (parentName == DatabaseLiterals.TableEarthMat)
                    {
                        newMineral.MineralEMID = _mineralParentID; //Foreigh key
                    }
                    else if (parentName == DatabaseLiterals.TableMineralAlteration)
                    {
                        newMineral.MineralMAID = _mineralParentID;  //Foreigh key
                    }

                    newMineral.MineralIDName = mineralIDCalculator.CalculateMineralAlias(_mineralParentID, _mineralParentAlias, inMineralNames.IndexOf(inMinName));
                    //newMineral.MineralID = mineralIDCalculator.CalculateMineralID(); //Prime key
                    newMineral.MineralName = inMinName;
                    //newMineral.MineralSizeMax = 0.ToString();
                    //newMineral.MineralSizeMin = 0.ToString();

                    mineralObjects.Add(newMineral);
                }



                //Save model class
                accessData.BatchSaveSQLTables(mineralObjects);
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
