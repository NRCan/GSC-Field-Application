﻿using GSCFieldApp.Dictionaries;
using GSCFieldApp.Models;
using GSCFieldApp.Services.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Template10.Mvvm;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using GSCFieldApp.Dictionaries;
using Newtonsoft.Json.Linq;

namespace GSCFieldApp.ViewModels
{
    public class SampleViewModel : ViewModelBase
    {
        #region INITIALIZATION

        //UI default values

        private string _sampleAlias = string.Empty;
        private int _sampleID = 0;
        private int _sampleEartmatID = 0;

        private string _sampleNote = string.Empty;
        private string _sampleAzim = string.Empty; //Default
        private string _sampleDip = string.Empty;//Default
        private string _sampleDepthMin = string.Empty; //Default
        private string _sampleDepthMax = string.Empty; //Default
        private string _sampleDuplicateName = string.Empty; //Default
        private string _sampleCoreLength = string.Empty; //Default
        private string _sampledBy = string.Empty; //Default
        private string _sampleCoreFrom = string.Empty; //Default
        private string _sampleCoreTo = string.Empty; //Default

        private Visibility _surficialVisibility = Visibility.Collapsed; //Visibility for extra fields
        private Visibility _bedrockVisibility = Visibility.Visible; //Visibility for extra fields
        private Visibility _drillholeVisibility = Visibility.Collapsed; //Visibility for extra fields
        private bool _isSampleDuplicate = false; //Whether checkbox is checked (true) or uncheck (false)
        private bool _isSampleBlank = false; //Whether checkbox is checked or uncheck
        private bool _isSampleDuplicateEnabled = false; //Whether duplicate name box is enabled or disabled

        //Local settings
        readonly DataLocalSettings localSetting = new DataLocalSettings();

        //UI interaction
        public bool doSampleUpdate = false;

        private ObservableCollection<Themes.ComboBoxItem> _sampleType = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedSampleType = string.Empty;

        private ObservableCollection<Themes.ComboBoxItem> _samplePurpose = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedSamplePurpose = string.Empty;

        private ObservableCollection<Themes.ComboBoxItem> _purposeValues = new ObservableCollection<Themes.ComboBoxItem>();

        private ObservableCollection<Themes.ComboBoxItem> _sampleFormat = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedSampleFormat = string.Empty;

        private ObservableCollection<Themes.ComboBoxItem> _sampleSurface = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedSampleSurface = string.Empty;

        private ObservableCollection<Themes.ComboBoxItem> _sampleQuality = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedSampleQuality = string.Empty;

        private ObservableCollection<Themes.ComboBoxItem> _sampleState = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedSampleState = string.Empty;

        private ObservableCollection<Themes.ComboBoxItem> _sampleHorizon = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedSampleHorizon = string.Empty;

        private ObservableCollection<Themes.ComboBoxItem> _sampleCoreSize = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedSampleCoreSize = string.Empty;

        //Model init
        private Sample sampleModel = new Sample();
        public DataIDCalculation sampleIDCalculator = new DataIDCalculation();
        public FieldNotes existingDataDetailSample;
        readonly DataAccess accessData = new DataAccess();

        //Events and delegate
        public delegate void sampleEditEventHandler(object sender); //A delegate for execution events
        public event sampleEditEventHandler newSampleEdit; //This event is triggered when a save has been done on station table. 

        #endregion

        #region PROPERTIES

        public Sample SampleModel { get { return sampleModel; } set { sampleModel = value; } }
        public string SampleAlias { get { return _sampleAlias; } set { _sampleAlias = value; } }

        public string SampleNote { get { return _sampleNote; } set { _sampleNote = value; } }
        public int SampleID { get { return _sampleID; } set { _sampleID = value; } }
        public int SampleEarthmatID { get { return _sampleEartmatID; } set { _sampleEartmatID = value; } }
        public string SampleDuplicateName { get { return _sampleDuplicateName; } set { _sampleDuplicateName = value; } }
        public string SampledBy { get { return _sampledBy; } set { _sampledBy = value; } }
        public string SampleCoreFrom 
        {

            get
            {
                return _sampleCoreFrom;
            }
            set
            {
                bool result = double.TryParse(value, out double index);

                if (result)
                {
                    _sampleCoreFrom = value;
                }
                else
                {
                    _sampleCoreFrom = value = "0";
                    RaisePropertyChanged("SampleCoreFrom");
                }


            }
        }
        public string SampleCoreLength 
        {
            get
            {
                return _sampleCoreLength;
            }
            set
            {
                bool result = double.TryParse(value, out double index);

                if (result)
                {
                    _sampleCoreLength = value;
                }
                else
                {
                    _sampleCoreLength = value = "0";
                    RaisePropertyChanged("SampleCoreLength");
                }


            }
        }
        public string SampleCoreTo 
        {
            get
            {
                return _sampleCoreTo;
            }
            set
            {
                bool result = double.TryParse(value, out double index);

                if (result)
                {
                    _sampleCoreTo = value;
                }
                else
                {
                    _sampleCoreTo = value = "0";
                    RaisePropertyChanged("SampleCoreTo");
                }


            }
        }

        public bool IsSampleDuplicate { get { return _isSampleDuplicate; } set { _isSampleDuplicate = value; } }
        public bool IsSampleBlank { get { return _isSampleBlank; } set { _isSampleBlank = value; } }
        public bool IsSampleDuplicateEnabled { get { return _isSampleDuplicateEnabled; } set { _isSampleDuplicateEnabled = value; } }
        public Visibility SurficialVisibility { get { return _surficialVisibility; } set { _surficialVisibility = value; } }
        public Visibility BedrockVisibility { get { return _bedrockVisibility; } set { _bedrockVisibility = value; } }
        public Visibility DrillholeVisibility { get { return _drillholeVisibility; } set { _drillholeVisibility = value; } }
        public ObservableCollection<Themes.ComboBoxItem> SampleType { get { return _sampleType; } set { _sampleType = value; } }
        public string SelectedSampleType { get { return _selectedSampleType; } set { _selectedSampleType = value; } }

        public ObservableCollection<Themes.ComboBoxItem> SamplePurpose { get { return _samplePurpose; } set { _samplePurpose = value; } }
        public string SelectedSamplePurpose { get { return _selectedSamplePurpose; } set { _selectedSamplePurpose = value; } }

        public ObservableCollection<Themes.ComboBoxItem> PurposeValues { get { return _purposeValues; } set { _purposeValues = value; } }

        public ObservableCollection<Themes.ComboBoxItem> SampleFormat { get { return _sampleFormat; } set { _sampleFormat = value; } }
        public string SelectedSampleFormat { get { return _selectedSampleFormat; } set { _selectedSampleFormat = value; } }

        public ObservableCollection<Themes.ComboBoxItem> SampleSurface { get { return _sampleSurface; } set { _sampleSurface = value; } }
        public string SelectedSampleSurface { get { return _selectedSampleSurface; } set { _selectedSampleSurface = value; } }

        public ObservableCollection<Themes.ComboBoxItem> SampleQuality { get { return _sampleQuality; } set { _sampleQuality = value; } }
        public string SelectedSampleQuality { get { return _selectedSampleQuality; } set { _selectedSampleQuality = value; } }

        public ObservableCollection<Themes.ComboBoxItem> SampleState { get { return _sampleState; } set { _sampleState = value; } }
        public string SelectedSampleState { get { return _selectedSampleState; } set { _selectedSampleState = value; } }
        public ObservableCollection<Themes.ComboBoxItem> SampleHorizon { get { return _sampleHorizon; } set { _sampleHorizon = value; } }
        public string SelectedSampleHorizon { get { return _selectedSampleHorizon; } set { _selectedSampleHorizon = value; } }

        public ObservableCollection<Themes.ComboBoxItem> SampleCoreSize { get { return _sampleCoreSize; } set { _sampleCoreSize = value; } }
        public string SelectedSampleCoreSize { get { return _selectedSampleCoreSize; } set { _selectedSampleCoreSize = value; } }

        public string SampleAzim
        {
            get
            {
                return _sampleAzim;
            }
            set
            {
                bool result = int.TryParse(value, out int index);

                if (result)
                {
                    if (index >= 0 && index < 360)
                    {
                        _sampleAzim = value;
                    }
                    else
                    {
                        _sampleAzim = value = "0";
                        RaisePropertyChanged("SampleAzim");
                    }

                }
                else
                {
                    _sampleAzim = value = "0";
                    RaisePropertyChanged("SampleAzim");
                }


            }
        }

        public string SampleDip
        {
            get
            {
                return _sampleDip;
            }
            set
            {
                bool result = int.TryParse(value, out int index);

                if (result)
                {
                    if (index >= 0 && index <= 90)
                    {
                        _sampleDip = value;
                    }
                    else
                    {
                        _sampleDip = value = "0";
                        RaisePropertyChanged("SampleDip");
                    }

                }
                else
                {
                    _sampleDip = value = "0";
                    RaisePropertyChanged("SampleDip");
                }


            }
        }

        public string SampleDepthMin
        {
            get
            {
                return _sampleDepthMin;
            }
            set
            {
                bool result = float.TryParse(value, out float index);

                if (result)
                {
                    if (index >= 0 && index <= 999)
                    {
                        _sampleDepthMin = value;
                    }
                    else
                    {
                        _sampleDepthMin = value = "0.0";
                        RaisePropertyChanged("SampleDepthMin");
                    }

                }
                else
                {
                    _sampleDepthMin = value = "0.0";
                    RaisePropertyChanged("SampleDepthMin");
                }


            }
        }

        public string SampleDepthMax
        {
            get
            {
                return _sampleDepthMax;
            }
            set
            {
                bool result = float.TryParse(value, out float index);

                if (result)
                {
                    if (index >= 0 && index <= 999)
                    {
                        _sampleDepthMax = value;
                    }
                    else
                    {
                        _sampleDepthMax = value = "0.0";
                        RaisePropertyChanged("SampleDepthMax");
                    }

                }
                else
                {
                    _sampleDepthMax = value = "0.0";
                    RaisePropertyChanged("SampleDepthMax");
                }
            }
        }
        #endregion

        //Main
        public SampleViewModel(FieldNotes inDetailModel)
        {
            //On init for new samples calculates values for default UI form
            _sampleEartmatID = inDetailModel.GenericID;
            //_sampleID = sampleIDCalculator.CalculateSampleID();
            _sampleAlias = sampleIDCalculator.CalculateSampleAlias(_sampleEartmatID, inDetailModel.earthmat.EarthMatName);

            FillSamplePurpose();
            FillSampleType();
            FillSurface();
            FillFormat();
            FillQuality();
            FillState();
            FillHorizon();
            FillCoreSizes();

            SetFieldVisibility(); //Will enable/disable some fields based on bedrock or surficial usage
        }

        /// <summary>
        /// Will fill the dialog with existing information coming from the database.
        /// </summary>
        /// <param name="incomingData">The model in which the existing information is stored.</param>
        public void AutoFillDialog(FieldNotes incomingData)
        {
            //Keep
            existingDataDetailSample = incomingData;

            //Set
            _sampleID = existingDataDetailSample.sample.SampleID;
            _sampleNote = existingDataDetailSample.sample.SampleNotes;
            _sampleEartmatID = existingDataDetailSample.ParentID;
            _sampleAlias = existingDataDetailSample.sample.SampleName;
            _selectedSampleType = existingDataDetailSample.sample.SampleType;
            _selectedSampleSurface = existingDataDetailSample.sample.SampleSurface;
            _selectedSampleQuality = existingDataDetailSample.sample.SampleQuality;
            _selectedSampleFormat = existingDataDetailSample.sample.SampleFormat;
            _sampleAzim = existingDataDetailSample.sample.SampleAzim.ToString();
            _sampleDip = existingDataDetailSample.sample.SampleDiplunge.ToString();
            _selectedSampleState = existingDataDetailSample.sample.SampleState;
            _selectedSampleHorizon = existingDataDetailSample.sample.SampleHorizon;
            _sampleDepthMin = existingDataDetailSample.sample.SampleDepthMin.ToString();
            _sampleDepthMax = existingDataDetailSample.sample.SampleDepthMax.ToString();
            _sampleDuplicateName = existingDataDetailSample.sample.SampleDuplicateName;
            _sampledBy = existingDataDetailSample.sample.SampleBy;
            _sampleCoreFrom = existingDataDetailSample.sample.SampleCoreFrom.ToString();
            _sampleCoreLength = existingDataDetailSample.sample.SampleCoreLength.ToString();
            _sampleCoreTo = existingDataDetailSample.sample.SampleCoreTo.ToString();
            _selectedSampleCoreSize = existingDataDetailSample.sample.SampleCoreSize;

            if (_sampleDuplicateName != String.Empty)
            {
                _isSampleDuplicate = true;
                RaisePropertyChanged("IsSampleDuplicate");
            }

            if (existingDataDetailSample.sample.SampleBlank != String.Empty)
            {
                if (existingDataDetailSample.sample.SampleBlank == DatabaseLiterals.boolYes)
                {
                    _isSampleBlank = true;
                    RaisePropertyChanged("IsSampleBlank");
                }
                else
                {
                    _isSampleBlank = false;
                    RaisePropertyChanged("IsSampleBlank");
                }
            }

            //Update UI
            RaisePropertyChanged("SampleID");
            RaisePropertyChanged("SampleAlias");
            RaisePropertyChanged("SelectedSampleType");
            RaisePropertyChanged("SampleNote");
            RaisePropertyChanged("SampleDip");
            RaisePropertyChanged("SampleAzim");
            RaisePropertyChanged("SelectedSampleFormat");
            RaisePropertyChanged("SelectedSampleQuality");
            RaisePropertyChanged("SelectedSampleSurface");
            RaisePropertyChanged("SelectedSampleState");
            RaisePropertyChanged("SelectedSampleHorizon");
            RaisePropertyChanged("SampleDepthMin");
            RaisePropertyChanged("SampleDepthMax");
            RaisePropertyChanged("SampleDuplicateName");

            RaisePropertyChanged("SelectedSampleCoreSize");
            RaisePropertyChanged("SampledBy"); 
            RaisePropertyChanged("SampleCoreFrom"); 
            RaisePropertyChanged("SampleCoreLength"); 
            RaisePropertyChanged("SampleCoreTo"); 

            //Update list view
            UnPipePurposes(existingDataDetailSample.sample.SamplePurpose);

            doSampleUpdate = true;

            //Validate paleomag controls visibility
            ValidateForPaleomagnetism();
        }

        /// <summary>
        /// On save event
        /// </summary>
        public void SaveDialogInfo()
        {
            //Get current class information and add to model
            sampleModel.SampleID = _sampleID; //Prime key
            sampleModel.SampleName = _sampleAlias; //Foreign key
            sampleModel.SampleNotes = _sampleNote;
            sampleModel.SampleEarthmatID = _sampleEartmatID;
            sampleModel.SampleBy = _sampledBy;

            if (_sampleCoreFrom != string.Empty)
            {
                sampleModel.SampleCoreFrom = double.Parse(_sampleCoreFrom);
            }
            if (_sampleCoreLength != string.Empty)
            {
                sampleModel.SampleCoreLength = double.Parse(_sampleCoreLength);
            }
            if (_sampleCoreTo != string.Empty)
            {
                sampleModel.SampleCoreTo = double.Parse(_sampleCoreTo);
            }

            sampleModel.SamplePurpose = PipePurposes(); //process list of values so they are concatenated.
            if (_sampleAzim != string.Empty)
            {
                sampleModel.SampleAzim = int.Parse(_sampleAzim);
            }
            if (_sampleDip != string.Empty)
            {
                sampleModel.SampleDiplunge = int.Parse(_sampleDip);
            }
            if (_sampleDepthMin != string.Empty)
            {
                sampleModel.SampleDepthMin = double.Parse(_sampleDepthMin);
            }
            if (_sampleDepthMax != string.Empty)
            {
                sampleModel.SampleDepthMax = double.Parse(_sampleDepthMax);
            }

            sampleModel.SampleDuplicateName = _sampleDuplicateName;

            if (IsSampleBlank)
            {
                sampleModel.SampleBlank = DatabaseLiterals.boolYes;
            }
            else if (!IsSampleBlank)
            {
                sampleModel.SampleBlank = DatabaseLiterals.boolNo;
            }

            if (SelectedSampleType != null)
            {
                sampleModel.SampleType = SelectedSampleType;
            }
            if (SelectedSampleFormat != null)
            {
                sampleModel.SampleFormat = SelectedSampleFormat;
            }
            if (SelectedSampleSurface != null)
            {
                sampleModel.SampleSurface = SelectedSampleSurface;
            }
            if (SelectedSampleQuality != null)
            {
                sampleModel.SampleQuality = SelectedSampleQuality;
            }
            if (SelectedSampleState != null)
            {
                sampleModel.SampleState = SelectedSampleState;
            }
            if (SelectedSampleState != null)
            {
                sampleModel.SampleHorizon = SelectedSampleHorizon;
            }
            if (SelectedSampleCoreSize != null)
            {
                sampleModel.SampleCoreSize = SelectedSampleCoreSize;
            }
            //Save model class
            object sampleObject = (object)sampleModel;
            accessData.SaveFromSQLTableObject(ref sampleObject, doSampleUpdate);
            sampleModel = (Sample)sampleObject;
            //accessData.SaveFromSQLTableObject(sampleModel, doSampleUpdate);

            //Launch an event call for everyone that an earthmat has been edited.
            if (newSampleEdit != null)
            {
                newSampleEdit(this);
            }
        }

        #region FILL

        /// <summary>
        /// Will fill the sample type combobox
        /// </summary>
        private void FillCoreSizes()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldSampleCoreSize;
            string tableName = Dictionaries.DatabaseLiterals.TableSample;
            foreach (var itemType in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedSampleCoreSize))
            {
                _sampleCoreSize.Add(itemType);
            }

            //Update UI
            RaisePropertyChanged("SampleCoreSize");
            RaisePropertyChanged("SelectedSampleCoreSize");
        }

        /// <summary>
        /// Will fill the sample type combobox
        /// </summary>
        private void FillSampleType()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldSampleType;
            string tableName = Dictionaries.DatabaseLiterals.TableSample;
            foreach (var itemType in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedSampleType))
            {
                _sampleType.Add(itemType);
            }

            //Update UI
            RaisePropertyChanged("SampleType");
            RaisePropertyChanged("SelectedSampleType");
        }

        /// <summary>
        /// Will fill the sample type combobox
        /// </summary>
        private void FillSurface()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldSampleSurface;
            string tableName = Dictionaries.DatabaseLiterals.TableSample;
            foreach (var itemType in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedSampleSurface))
            {
                _sampleSurface.Add(itemType);
            }

            //Update UI
            RaisePropertyChanged("SampleSurface");
            RaisePropertyChanged("SelectedSampleSurface");
        }
        /// <summary>
        /// Will fill the sample type combobox
        /// </summary>
        private void FillFormat()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldSampleFormat;
            string tableName = Dictionaries.DatabaseLiterals.TableSample;
            foreach (var itemType in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedSampleFormat))
            {
                _sampleFormat.Add(itemType);
            }

            //Update UI
            RaisePropertyChanged("SampleFormat");
            RaisePropertyChanged("SelectedSampleFormat");
        }
        /// <summary>
        /// Will fill the sample type combobox
        /// </summary>
        private void FillQuality()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldSampleQuality;
            string tableName = Dictionaries.DatabaseLiterals.TableSample;
            foreach (var itemType in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedSampleQuality))
            {
                _sampleQuality.Add(itemType);
            }

            //Update UI
            RaisePropertyChanged("SampleQuality");
            RaisePropertyChanged("SelectedSampleQuality");
        }
        /// <summary>
        /// Will fill the sample state combobox
        /// </summary>
        private void FillState()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldSampleState;
            string tableName = Dictionaries.DatabaseLiterals.TableSample;
            foreach (var itemType in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedSampleState))
            {
                _sampleState.Add(itemType);
            }

            //Update UI
            RaisePropertyChanged("SampleState");
            RaisePropertyChanged("SelectedSampleState");
        }

        /// <summary>
        /// Will fill the sample state combobox
        /// </summary>
        private void FillHorizon()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldSampleHorizon;
            string tableName = Dictionaries.DatabaseLiterals.TableSample;
            foreach (var itemType in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedSampleHorizon))
            {
                _sampleHorizon.Add(itemType);
            }

            //Update UI
            RaisePropertyChanged("SampleHorizon");
            RaisePropertyChanged("SelectedSampleHorizon");
        }

        /// <summary>
        /// Will fill the sample purpose combobox
        /// </summary>
        private void FillSamplePurpose()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldSamplePurpose;
            string tableName = Dictionaries.DatabaseLiterals.TableSample;
            foreach (var itemPurpose in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedSamplePurpose))
            {
                _samplePurpose.Add(itemPurpose);
            }


            //Update UI
            RaisePropertyChanged("SamplePurpose");
            RaisePropertyChanged("SelectedSamplePurpose");

        }
        #endregion

        #region METHODS

        /// <summary>
        /// Will show a quick reminder to take a duplicate for
        /// surficial geologist every 15 samples.
        /// </summary>
        public bool DuplicateReminder()
        {
            bool shouldShowReminder = false;

            if (_surficialVisibility == Visibility.Visible)
            {
                Sample sampleModel = new Sample();
                int sampleCount = accessData.GetTableCount(sampleModel.GetType());

                if (sampleCount % 15 == 0 && sampleCount != 0)
                {
                    shouldShowReminder = true;
                }
            }

            return shouldShowReminder;

        }

        /// <summary>
        /// Will set visibility based on a bedrock or surficial field book
        /// </summary>
        private void SetFieldVisibility()
        {
            if (localSetting.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoFWorkType) != null)
            {
                if (localSetting.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoFWorkType).ToString().Contains(Dictionaries.DatabaseLiterals.ApplicationThemeBedrock))
                {
                    _bedrockVisibility = Visibility.Visible;
                    _surficialVisibility = Visibility.Collapsed;
                }
                else if (localSetting.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoFWorkType).ToString() == Dictionaries.DatabaseLiterals.ApplicationThemeSurficial)
                {
                    _bedrockVisibility = Visibility.Collapsed;
                    _surficialVisibility = Visibility.Visible;
                }
                if (localSetting.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoFWorkType).ToString() == Dictionaries.DatabaseLiterals.ApplicationThemeDrillHole)
                {
                    _drillholeVisibility = Visibility.Visible;
                }
                else
                {
                    _drillholeVisibility = Visibility.Collapsed;
                }
            }
            else
            {
                //Fallback
                _bedrockVisibility = Visibility.Visible;
                _surficialVisibility = Visibility.Collapsed;
                _drillholeVisibility = Visibility.Collapsed;
            }

            RaisePropertyChanged("BedrockVisibility");
            RaisePropertyChanged("SurficialVisibility");
            RaisePropertyChanged("DrillholeVisibility");
        }

        /// <summary>
        /// Force a cascade delete if user get's out of sample dialog while in quick sample mode.
        /// </summary>
        /// <param name="inParentModel"></param>
        public void DeleteCascadeOnQuickSample(FieldNotes inParentModel)
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
        /// Will remove a purpose from purpose list
        /// </summary>
        /// <param name="inPurpose"></param>
        public void RemoveSelectedPurpose(object inPurpose)
        {

            Themes.ComboBoxItem oldPurp = inPurpose as Themes.ComboBoxItem;
            _purposeValues.Remove(oldPurp);

            if (oldPurp.itemName == DatabaseLiterals.samplePurposePaleomag)
            {
                ValidateForPaleomagnetism(true);
            }

            RaisePropertyChanged("PurposeValues");
        }

        /// <summary>
        /// Will add to the list of purposes a selected purpose by the user.
        /// </summary>
        public void AddAPurpose(string purposeToAdd)
        {
            #region NEW METHOD
            Themes.ComboBoxItem newPurp = new Themes.ComboBoxItem
            {
                itemValue = purposeToAdd
            };
            foreach (Themes.ComboBoxItem cb in SamplePurpose)
            {
                if (cb.itemValue == purposeToAdd)
                {
                    newPurp.itemName = cb.itemName;
                    break;
                }
            }
            if (newPurp.itemName != null && newPurp.itemValue != string.Empty)
            {
                bool foundValue = false;
                foreach (Themes.ComboBoxItem existingItems in _purposeValues)
                {
                    if (purposeToAdd == existingItems.itemName)
                    {
                        foundValue = true;
                    }

                }
                if (!foundValue)
                {
                    _purposeValues.Add(newPurp);
                }

            }
            #endregion
            RaisePropertyChanged("PurposeValues");

        }

        /// <summary>
        /// Will take all values from purpose value list and pipe them as a string
        /// to be able to save them all in the database
        /// </summary>
        /// <returns></returns>
        public string PipePurposes()
        {
            string _samplePurposeConcat = string.Empty;

            foreach (Themes.ComboBoxItem purposes in PurposeValues)
            {
                if (_samplePurposeConcat == string.Empty)
                {
                    _samplePurposeConcat = purposes.itemValue;
                }
                else
                {
                    _samplePurposeConcat = _samplePurposeConcat + Dictionaries.DatabaseLiterals.KeywordConcatCharacter + purposes.itemValue;
                }
            }

            return _samplePurposeConcat;
        }

        public void UnPipePurposes(string inPurpose)
        {
            List<string> purposesUnpiped = inPurpose.Split(Dictionaries.DatabaseLiterals.KeywordConcatCharacter.Trim().ToCharArray()).ToList();

            //Clean values first
            _purposeValues.Clear();

            foreach (string pu in purposesUnpiped)
            {
                AddAPurpose(pu.Trim());
            }
        }

        /// <summary>
        /// If user has selected sample type of oriented, with paleomagnetism purpose, within a surficial field book. Then enable oriented fields from bedrock
        /// </summary>
        public void ValidateForPaleomagnetism(bool forceDeactivate = false)
        {
            #region validate paleomagnetism

            //Validate for oriented samplem type and paleomagnetism. This should trigger view on Oriented set of inputs
            if (_surficialVisibility == Visibility.Visible
                && SelectedSamplePurpose == DatabaseLiterals.samplePurposePaleomag
                && SelectedSampleType == DatabaseLiterals.sampleTypeOriented)
            {
                _bedrockVisibility = Visibility.Visible;
                RaisePropertyChanged("BedrockVisibility");
            }
            else if (_surficialVisibility == Visibility.Visible
                && (SelectedSamplePurpose != DatabaseLiterals.samplePurposePaleomag
                || SelectedSampleType != DatabaseLiterals.sampleTypeOriented))
            {
                _bedrockVisibility = Visibility.Collapsed;
                RaisePropertyChanged("BedrockVisibility");
            }

            //Validate within purposes list
            if (_surficialVisibility == Visibility.Visible
                && SelectedSampleType == DatabaseLiterals.sampleTypeOriented
                && SelectedSamplePurpose == String.Empty
                && PurposeValues.Count > 0)
            {
                foreach (Themes.ComboBoxItem cbi in PurposeValues)
                {
                    if (cbi.itemValue.Contains(DatabaseLiterals.samplePurposePaleomag))
                    {
                        _bedrockVisibility = Visibility.Visible;
                        RaisePropertyChanged("BedrockVisibility");
                    }
                }
            }

            //If needed, force deactivation of the whole header.
            if (forceDeactivate)
            {
                _bedrockVisibility = Visibility.Collapsed;
                RaisePropertyChanged("BedrockVisibility");
            }

            #endregion
        }

        #endregion

        #region EVENTS

        public void SampleTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ValidateForPaleomagnetism();
        }

        public void SamplePurposeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AddAPurpose(SelectedSamplePurpose);

            //Special case for paleomagnetism for oriented sample types
            ValidateForPaleomagnetism();

        }

        /// <summary>
        /// Checkbox to enable/disable the duplicate name
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void isSampleDuplicate_Checked(object sender, RoutedEventArgs e)
        {

            if (_sampleDuplicateName == string.Empty)
            {
                //Init
                _sampleDuplicateName = _sampleAlias;
                _isSampleDuplicateEnabled = true;
            }
            else if (_sampleDuplicateName != string.Empty)
            {
                _isSampleDuplicateEnabled = true;
            }

            RaisePropertyChanged("SampleDuplicateName");
            RaisePropertyChanged("IsSampleDuplicateEnabled");
        }

        /// <summary>
        /// Checkbox to enable/disable the duplicate name
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void isSampleDuplicate_Unchecked(object sender, RoutedEventArgs e)
        {

            //Reset
            _sampleDuplicateName = string.Empty;
            _isSampleDuplicateEnabled = false;

            RaisePropertyChanged("SampleDuplicateName");
            RaisePropertyChanged("IsSampleDuplicateEnabled");

        }
        #endregion


    }
}
