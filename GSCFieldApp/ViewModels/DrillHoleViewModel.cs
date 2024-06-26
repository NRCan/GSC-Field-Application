﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;
using GSCFieldApp.Models;
using GSCFieldApp.Services.DatabaseServices;
using System.Text.RegularExpressions;
using Windows.UI.Xaml.Controls;
using GSCFieldApp.Dictionaries;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml;
using GSCFieldApp.Themes;
using System.Collections.ObjectModel;

namespace GSCFieldApp.ViewModels
{
    public class DrillHoleViewModel: ViewModelBase
    {
        #region INITIALIZATION

        private DrillHole dhModel = new DrillHole();
        public FieldNotes existingDataDetail;
        readonly DataAccess accessData = new DataAccess();
        readonly DataLocalSettings localSetting = new DataLocalSettings();
        public bool doDrillHoleUpdate = false;
        public DataIDCalculation idCalculator = new DataIDCalculation();
        public Themes.ConcatenatedCombobox concat = new Themes.ConcatenatedCombobox();

        //Events and delegate
        public delegate void drillEditEventHandler(object sender); //A delegate for execution events
        public event drillEditEventHandler newDrillEdit; //This event is triggered when a save has been done on station table.
        public static event EventHandler<string> DrillUpdateEventHandler; //This event is triggered when user has change coordinate so map page needs a refresh.

        //UI
        private int _drillID = 0; //Meant for update purposes, not insert
        private string _drillIDName = string.Empty;
        private string _notes = string.Empty;
        private string _relatedTo = string.Empty;
        private string _name = string.Empty;
        private string _companyName = string.Empty;
        private string _drillAzim = string.Empty; 
        private string _drillDip = string.Empty;
        private string _drillDepth = string.Empty;
        private string _drillLogBy = string.Empty;
        private string _drillReLogDate = string.Empty;
        private string _drillLogSummary = string.Empty;
        private string _drillDate = string.Empty;
        private string _drillIntervalFrom = string.Empty;
        private string _drillIntervalTo = string.Empty;  

        private ObservableCollection<Themes.ComboBoxItem> _drillType = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedDrillType = string.Empty;

        private ObservableCollection<Themes.ComboBoxItem> _drillUnit = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedDrillUnit = string.Empty;

        private ObservableCollection<Themes.ComboBoxItem> _drillHoleSize = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedDrillHoleSize = string.Empty;

        private ObservableCollection<Themes.ComboBoxItem> _drillCoreSize = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedDrillCoreSize = string.Empty;

        private ObservableCollection<Themes.ComboBoxItem> _drillLogType = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedDrillLogType = string.Empty;

        private ObservableCollection<Themes.ComboBoxItem> _drillLogIntervals = new ObservableCollection<Themes.ComboBoxItem>();

        #endregion

        #region PROPERTIES
        public string Notes { get { return _notes; } set { _notes = value; } }

        public string RelatedTo { get { return _relatedTo; } set { _relatedTo = value; } }

        public string OriginalName { get { return _name; } set { _name = value; } }
        public string DrillIDName { get { return _drillIDName; } set { _drillIDName = value; } }
        public string CompanyName { get { return _companyName; } set { _companyName = value; } }
        public string DrillRelogDate { get { return _drillReLogDate; } set { _drillReLogDate = value; } }
        public string DrillLogBy { get { return _drillLogBy; } set { _drillLogBy = value; } }
        public string DrillLogSummary { get { return _drillLogSummary; } set { _drillLogSummary = value; } }
        public string DrillIntervalFrom { get { return _drillIntervalFrom; } set { _drillIntervalFrom = value; } }
        public string DrillIntervalTo { get { return _drillIntervalTo; } set { _drillIntervalTo = value; } }

        public string DrillDate { get { return _drillDate; } set { _drillDate = value; } }
        public ObservableCollection<Themes.ComboBoxItem> DrillType { get { return _drillType; } set { _drillType = value; } }
        public string SelectedDrillType { get { return _selectedDrillType; } set { _selectedDrillType = value; } }
        public ObservableCollection<Themes.ComboBoxItem> DrillUnit { get { return _drillUnit; } set { _drillUnit = value; } }
        public string SelectedDrillUnit { get { return _selectedDrillUnit; } set { _selectedDrillUnit = value; } }

        public ObservableCollection<Themes.ComboBoxItem> DrillHoleSize { get { return _drillHoleSize; } set { _drillHoleSize = value; } }
        public string SelectedDrillHoleSize { get { return _selectedDrillHoleSize; } set { _selectedDrillHoleSize = value; } }

        public ObservableCollection<Themes.ComboBoxItem> DrillCoreSize { get { return _drillCoreSize; } set { _drillCoreSize = value; } }
        public string SelectedDrillCoreSize { get { return _selectedDrillCoreSize; } set { _selectedDrillCoreSize = value; } }

        public ObservableCollection<Themes.ComboBoxItem> DrillLogType { get { return _drillLogType; } set { _drillLogType = value; } }
        public string SelectedDrillLogType { get { return _selectedDrillLogType; } set { _selectedDrillLogType = value; } }

        public ObservableCollection<Themes.ComboBoxItem> DrillLogIntervals { get { return _drillLogIntervals; } set { _drillLogIntervals = value; } }

        public string DrillDepth { get { return _drillDepth; } set { _drillDepth = value; } }


        public string DrillAzim
        {
            get
            {
                return _drillAzim;
            }
            set
            {
                bool result = double.TryParse(value, out double index);

                if (result)
                {
                    if (index >= 0.0 && index < 360.0)
                    {
                        _drillAzim = value;
                    }
                    else
                    {
                        _drillAzim = value = "0";
                        RaisePropertyChanged("DrillAzim");
                    }

                }
                else
                {
                    _drillAzim = value = "0";
                    RaisePropertyChanged("DrillAzim");
                }


            }
        }

        public string DrillDip
        {
            get
            {
                return _drillDip;
            }
            set
            {
                bool result = double.TryParse(value, out double index);

                if (result)
                {
                    if (index >= -90 && index < 90)
                    {
                        _drillDip = value;
                    }
                    else
                    {
                        _drillDip = value = "0";
                        RaisePropertyChanged("DrillDip");
                    }

                }
                else
                {
                    _drillDip = value = "0";
                    RaisePropertyChanged("DrillDip");
                }


            }
        }

        #endregion

        public DrillHoleViewModel(FieldNotes inReportModel)
        {
            existingDataDetail = inReportModel;

            _drillIDName = idCalculator.CalculateDrillAlias(DateTime.Today);

            //Fill controls
            FillDrillTypes();
            FillDrillUnits();
            FillDrillHoleSize();
            FillDrillLogType();
            FillLogBy();

        }

        #region METHODS

        /// <summary>
        /// Will refill UI
        /// </summary>
        /// <param name="incomingData"></param>
        public void AutoFillDialog(FieldNotes incomingData)
        {
            existingDataDetail = incomingData;

            _name = existingDataDetail.drillHoles.DrillName;
            _drillIDName = existingDataDetail.drillHoles.DrillIDName;
            _companyName = existingDataDetail.drillHoles.DrillCompany;
            _drillLogBy = existingDataDetail.drillHoles.DrillRelogBy;
            _notes = existingDataDetail.drillHoles.DrillNotes;
            _drillLogSummary = existingDataDetail.drillHoles.DrillLog;
            _drillDate = existingDataDetail.drillHoles.DrillDate;
            _drillReLogDate = existingDataDetail.drillHoles.DrillRelogDate;
            _drillID = existingDataDetail.drillHoles.DrillID;
            _drillAzim = existingDataDetail.drillHoles.DrillAzim.ToString();
            _drillDip = existingDataDetail.drillHoles.DrillDip.ToString();
            _drillDepth = existingDataDetail.drillHoles.DrillDepth.ToString();
            _relatedTo = existingDataDetail.drillHoles.DrillRelatedTo;

            RaisePropertyChanged("OriginalName");
            RaisePropertyChanged("DrillIDName");
            RaisePropertyChanged("CompanyName"); 
            RaisePropertyChanged("DrillLogBy");
            RaisePropertyChanged("DrillRelogDate");
            RaisePropertyChanged("Notes");
            RaisePropertyChanged("DrillLogSummary");
            RaisePropertyChanged("DrillDate");
            RaisePropertyChanged("DrillAzim");
            RaisePropertyChanged("DrillDip");
            RaisePropertyChanged("DrillDepth");
            RaisePropertyChanged("RelatedTo");

            _selectedDrillCoreSize = existingDataDetail.drillHoles.DrillCoreSize;
            _selectedDrillHoleSize = existingDataDetail.drillHoles.DrillHoleSize;
            _selectedDrillUnit = existingDataDetail.drillHoles.DrillUnit;
            _selectedDrillType = existingDataDetail.drillHoles.DrillType;
            _selectedDrillLogType = existingDataDetail.drillHoles.DrillRelogType;

            RaisePropertyChanged("SelectedDrillCoreSize");
            RaisePropertyChanged("SelectedDrillHoleSize");
            RaisePropertyChanged("SelectedDrillUnit");
            RaisePropertyChanged("SelectedDrillType"); 
            RaisePropertyChanged("SelectedDrillLogType");

            //Update list view
            ConcatenatedCombobox ccBox = new ConcatenatedCombobox();
            foreach (string d in ccBox.UnpipeString(existingDataDetail.drillHoles.DrillRelogIntervals))
            {
                AddAConcatenatedInterval(d);
            }

            doDrillHoleUpdate = true;

        }


        /// <summary>
        /// Will fill the drill hole types combobox
        /// </summary>
        private void FillDrillTypes()
        {

            //Fill vocab list
            string fieldName = Dictionaries.DatabaseLiterals.FieldDrillType;
            string tableName = Dictionaries.DatabaseLiterals.TableDrillHoles;
            foreach (var itemST in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedDrillType))
            {
                _drillType.Add(itemST);
            }

            //Update UI
            RaisePropertyChanged("DrillType");
            RaisePropertyChanged("SelectedDrillType"); 

        }

        /// <summary>
        /// Will fill the drill hole units combobox
        /// </summary>
        private void FillDrillUnits()
        {

            //Fill vocab list
            string fieldName = Dictionaries.DatabaseLiterals.FieldDrillUnit;
            string tableName = Dictionaries.DatabaseLiterals.TableDrillHoles;
            foreach (var itemST in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedDrillUnit))
            {
                _drillUnit.Add(itemST);
            }

            //Update UI
            RaisePropertyChanged("DrillUnit");
            RaisePropertyChanged("SelectedDrillUnit");

        }

        /// <summary>
        /// Will fill the drill hole size combobox
        /// </summary>
        private void FillDrillHoleSize()
        {

            //Fill vocab list
            string fieldName = Dictionaries.DatabaseLiterals.FieldDrillHoleSize;
            string tableName = Dictionaries.DatabaseLiterals.TableDrillHoles;
            foreach (var itemST in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedDrillHoleSize))
            {
                _drillHoleSize.Add(itemST);
            }

            //Update UI
            RaisePropertyChanged("DrillHoleSize");
            RaisePropertyChanged("SelectedDrillHoleSize");

        }

        /// <summary>
        /// Will fill the drill core size combobox
        /// </summary>
        private void FillDrillCoreSize()
        {

            //Fill vocab list
            _drillCoreSize.Clear();
            string fieldName = Dictionaries.DatabaseLiterals.FieldDrillCoreSize;
            string tableName = Dictionaries.DatabaseLiterals.TableDrillHoles;
            //foreach (var itemST in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedDrillCoreSize))
            //{
            //    _drillCoreSize.Add(itemST);
            //}

            List<Vocabularies> drillCores = new List<Vocabularies>();

            if (_selectedDrillHoleSize != null && _selectedDrillHoleSize != string.Empty && _selectedDrillHoleSize != " " && _selectedDrillHoleSize != "")
            {
                drillCores = accessData.GetPicklistValuesFromParent(tableName, fieldName, _selectedDrillHoleSize, false).ToList();
            }
            else
            {
                drillCores = accessData.GetPicklistValuesFromParent(tableName, fieldName, "x", false).ToList(); //Make the query crash and return N.A. if nothing is available in lithotype
            }

            //Fill in cbox
            if (_selectedDrillCoreSize == null)
            {
                _selectedDrillCoreSize = string.Empty;
            }
            foreach (var itemFeature in accessData.GetComboboxListFromVocab(drillCores, out _selectedDrillCoreSize))
            {
                _drillCoreSize.Add(itemFeature);
            }

            //If something already exists (from autofill) keep it
            if (existingDataDetail != null && existingDataDetail.drillHoles.DrillCoreSize != null)
            {
                _selectedDrillCoreSize = existingDataDetail.drillHoles.DrillCoreSize;
            }

            //Update UI
            RaisePropertyChanged("DrillCoreSize");
            RaisePropertyChanged("SelectedDrillCoreSize");

        }

        /// <summary>
        /// Will fill the drill relog type combobox
        /// </summary>
        private void FillDrillLogType()
        {

            //Fill vocab list
            string fieldName = Dictionaries.DatabaseLiterals.FieldDrillRelogType;
            string tableName = Dictionaries.DatabaseLiterals.TableDrillHoles;
            foreach (var itemST in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedDrillLogType))
            {
                _drillLogType.Add(itemST);
            }

            //Update UI
            RaisePropertyChanged("DrillLogType");
            RaisePropertyChanged("SelectedDrillLogType");

        }

        /// <summary>
        /// Will fill log by with default value of fieldbook geologist
        /// </summary>
        private void FillLogBy()
        {
            if (localSetting.GetSettingValue(DatabaseLiterals.FieldUserInfoID) != null)
            {
                int currentMetaID = int.Parse(localSetting.GetSettingValue(DatabaseLiterals.FieldUserInfoID).ToString());

                List<object> metadataTableRaw = accessData.ReadTable(existingDataDetail.metadata.GetType(), null);
                IEnumerable<Metadata> metadataTable = metadataTableRaw.Cast<Metadata>(); //Cast to proper list type
                IEnumerable<Metadata> metadatas = from m in metadataTable where m.MetaID == currentMetaID select m;
                List<Metadata> metadatasList = metadatas.ToList();

                _drillLogBy = metadatasList[0].Geologist;

                //Update UI
                RaisePropertyChanged("DrillLogBy");
            }
               
        }

        /// <summary>
        /// Force a cascade delete if user get's out of drill hole dialog while in manual XY mode
        /// </summary>
        /// <param name="inParentModel"></param>
        public void DeleteCascadeOnQuickDrillHole(FieldNotes inParentModel)
        {
            //Get the location id
            FieldLocation locModel = new FieldLocation();
            List<object> locTableLRaw = accessData.ReadTable(locModel.GetType(), null);
            IEnumerable<FieldLocation> locTable = locTableLRaw.Cast<FieldLocation>(); //Cast to proper list type
            IEnumerable<int> locs = from l in locTable where l.LocationID == inParentModel.location.LocationID select l.LocationID;
            List<int> locationFromDH = locs.ToList();

            //Delete location
            accessData.DeleteRecord(Dictionaries.DatabaseLiterals.TableLocation, Dictionaries.DatabaseLiterals.FieldLocationID, locationFromDH[0]);
        }

        /// <summary>
        /// On save event
        /// </summary>
        public void SaveDialogInfoAsync()
        {

            //Save the new station
            dhModel.DrillName = _name;
            dhModel.DrillIDName = _drillIDName;
            dhModel.DrillCompany = _companyName;
            dhModel.DrillRelogBy = _drillLogBy;
            dhModel.DrillNotes = _notes;
            dhModel.DrillLog = _drillLogSummary;
            dhModel.DrillRelatedTo = _relatedTo;
            dhModel.DrillRelogDate = _drillReLogDate;

            dhModel.DrillDate = _drillDate;
            dhModel.DrillID = _drillID; //Prime key
            dhModel.DrillLocationID = existingDataDetail.location.LocationID; //Foreign key

            if (_drillAzim != string.Empty)
            {
                dhModel.DrillAzim = double.Parse(_drillAzim);
            }
            if (_drillDip != string.Empty)
            {
                dhModel.DrillDip = double.Parse(_drillDip);
            }
            if (_drillDepth != string.Empty)
            {
                dhModel.DrillDepth = double.Parse(_drillDepth);
            }
            
            //Comboboxes
            if (SelectedDrillType != null)
            {
                dhModel.DrillType = SelectedDrillType;
            }

            if (SelectedDrillUnit != null)
            {
                dhModel.DrillUnit = SelectedDrillUnit;
            }

            if (SelectedDrillHoleSize != null)
            {
                dhModel.DrillHoleSize = SelectedDrillHoleSize;
            }
            if (SelectedDrillCoreSize != null)
            {
                dhModel.DrillCoreSize = SelectedDrillCoreSize;
            }
            if (SelectedDrillLogType != null)
            {
                dhModel.DrillRelogType = SelectedDrillLogType;
            }

            //Concat
            if (_drillLogIntervals != null && _drillLogIntervals.Count > 0)
            {
                dhModel.DrillRelogIntervals = concat.PipeValues(_drillLogIntervals);
            }

            object drillObject = (object)dhModel;
            accessData.SaveFromSQLTableObject(ref drillObject, doDrillHoleUpdate);
            dhModel = (DrillHole)drillObject;

            if (newDrillEdit != null)
            {
                newDrillEdit(this);
            }

            //Trigger event for map page
            EventHandler<string> updateRequest = DrillUpdateEventHandler;
            if (updateRequest != null)
            {
                updateRequest(this, dhModel.DrillID.ToString()) ;
            }

        }

        /// <summary>
        /// Will add to the list of intervals.
        /// </summary>
        public void AddAConcatenatedInterval(string valueToAdd, bool canRemove = true)
        {
            if (valueToAdd != null && valueToAdd != String.Empty)
            {
                //Create new cbox item
                Themes.ComboBoxItem newValue = new Themes.ComboBoxItem();
                newValue.itemValue = valueToAdd;
                newValue.itemName = valueToAdd;

                //Set visibility
                if (canRemove)
                {
                    newValue.canRemoveItem = Windows.UI.Xaml.Visibility.Visible;
                }
                else
                {
                    newValue.canRemoveItem = Windows.UI.Xaml.Visibility.Collapsed;
                }


                //Update collection
                if (newValue.itemName != null && newValue.itemName != string.Empty && newValue.itemName != Dictionaries.DatabaseLiterals.picklistNADescription)
                {
                    bool foundValue = false;
                    foreach (Themes.ComboBoxItem existingItems in _drillLogIntervals)
                    {
                        if (valueToAdd == existingItems.itemName)
                        {
                            foundValue = true;
                        }
                    }
                    if (!foundValue)
                    {
                        _drillLogIntervals.Add(newValue);
                        RaisePropertyChanged("DrillLogIntervals");
                    }

                }
            }
        }

        /// <summary>
        /// Will remove a value from concat box
        /// </summary>
        /// <param name="inInterval"></param>
        public void RemoveSelectedValue(object inInterval, string parentListViewName)
        {
            Themes.ComboBoxItem oldValue = inInterval as Themes.ComboBoxItem;

            _drillLogIntervals.Remove(oldValue);
            RaisePropertyChanged("DrillLogIntervals");

        }

        #endregion

        #region EVENTS
        /// <summary>
        /// Will be triggered whenever user selects a new hole size value. This will
        /// refill the core size pickist
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DrillHoleSizeCBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FillDrillCoreSize();
        }

        /// <summary>
        /// Will add a new contact object into contact list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DrillIntervalsSelectionButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (_drillIntervalFrom != null && _drillIntervalFrom != String.Empty &&
                _drillIntervalTo != null && _drillIntervalTo != String.Empty)
            {
                string newInterval = _drillIntervalFrom + DatabaseLiterals.KeywordConcatCharacter2nd + _drillIntervalTo;

                AddAConcatenatedInterval(newInterval);
            }



        }

        #endregion
    }
}
