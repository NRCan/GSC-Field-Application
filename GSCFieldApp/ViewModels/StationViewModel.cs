using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Template10.Mvvm;
using GSCFieldApp.Models;
using GSCFieldApp.Services.DatabaseServices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using GSCFieldApp.Dictionaries;
using GSCFieldApp.Themes;
using Windows.ApplicationModel.Contacts;

namespace GSCFieldApp.ViewModels
{
    public class StationViewModel : ViewModelBase
    {
        #region INITIALIZATION
        private Station model = new Station();
        private FieldLocation _location = new FieldLocation();
        private double _latitude = 0.0; //Default
        private double _longitude = 0.0; //Default
        private double _elevation = 0.0;//Default
        private string _alias = string.Empty; //Default
        private int _stationid = 0; //Default
        private int _locationid = 0; //Default
        private string _locationAlias = string.Empty; //Default
        private string _airno = string.Empty; //Default
        private string _slsnotes = string.Empty; //Default
        private DateTimeOffset _dateGeneric = DateTime.Now; //Default
        private string _dateDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now); //Default
        private string _dateTime = String.Format("{0:HH:mm:ss t}", DateTime.Now); //Default
        private string _notes = string.Empty; //Default
        private string _stationOCSize = string.Empty;
        private string _stationTravNo = string.Empty;
        private string _stationRelatedTo = string.Empty; //sqlite v 1.6


        private ObservableCollection<Themes.ComboBoxItem> _stationTypes = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedStationTypes = string.Empty;

        private ObservableCollection<Themes.ComboBoxItem> _observationSource = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedObservationSource = string.Empty;

        private ObservableCollection<Themes.ComboBoxItem> _stationQuality = new ObservableCollection<Themes.ComboBoxItem>();
        private ObservableCollection<Themes.ComboBoxItem> _stationQualityValues = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedStationQuality = string.Empty;

        private ObservableCollection<Themes.ComboBoxItem> _stationPhysEnv = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedStationPhysEnv = string.Empty;

        public FieldNotes existingDataDetail;

        public DataIDCalculation idCalculator = new DataIDCalculation();

        private Visibility _bedrockVisibility = Visibility.Visible; //Visibility for extra fields
        private Visibility _waypointVisibility = Visibility.Visible; //Visibility for waypoint fields only
        //Events and delegate
        public delegate void stationEditEventHandler(object sender); //A delegate for execution events
        public event stationEditEventHandler newStationEdit; //This event is triggered when a save has been done on station table.

        readonly DataLocalSettings localSetting = new DataLocalSettings();
        readonly DataAccess accessData = new DataAccess();

        public StationViewModel(bool isWayPoint)
        {

            //Fill controls
            FillStationType();
            
            SetFieldVisibility(); //Will enable/disable some fields based on bedrock or surficial usage

            //Treat station for themes.
            if (isWayPoint)
            {
                TransformToWaypointTheme();
            }
            else
            {
                _alias = idCalculator.CalculateStationAlias(DateTime.Now);
                FillStationQuality();
                FillStationPhysEnv();
                FillAirPhotoNo_TraverseNo();
                FillObsSource();
            }

        }

        #endregion

        #region PROPERTIES
        public Visibility BedrockVisibility { get { return _bedrockVisibility; } set { _bedrockVisibility = value; } }
        public Visibility WaypointVisibility { get { return _waypointVisibility; } set { _waypointVisibility = value; } }
        public string Latitude { get { return _latitude.ToString(); } set { _latitude = Convert.ToDouble(value); } }
        public string Longitude { get { return _longitude.ToString(); } set { _longitude = Convert.ToDouble(value); } }
        public string Elevation { get { return _elevation.ToString(); } set { _elevation = Convert.ToDouble(value); } }
        public DateTimeOffset DateGeneric { get { return _dateGeneric; } set { _dateGeneric = value; } }
        public string DDate { get { return _dateDate; } set { _dateDate = value; } }
        public string DTime { get { return _dateTime; } set { _dateTime = value; } }
        public Station StationModel { get { return model; } set { model = value; } }
        public FieldLocation Location { get { return _location; } set { _location = value; } }
        public string Alias { get { return _alias; } set { _alias = value; } }
        public int StationID { get { return _stationid; } set { _stationid = value; } }
        public int LocationID { get { return _locationid; } set { _locationid = value; } }
        public string Notes { get { return _notes; } set { _notes = value; } }
        public string RelatedTo { get { return _stationRelatedTo; } set { _stationRelatedTo = value; } }
        public string AirPhoto { get { return _airno; } set { _airno = value; } }
        public string TraverseNo
        {
            get
            {
                return _stationTravNo;
            }
            set
            {

                bool result = int.TryParse(value, out int index);

                if (result)
                {
                    if (index >= 0 && index <= 999999)
                    {
                        _stationTravNo = value;
                    }
                    else
                    {
                        _stationTravNo = value = "0";
                        RaisePropertyChanged("TraverseNo");
                    }

                }
                else
                {
                    _stationTravNo = value = "0";
                    RaisePropertyChanged("TraverseNo");
                }

            }
        }
        public string SlSNotes { get { return _slsnotes; } set { _slsnotes = value; } }
        public string StationOCSize { get { return _stationOCSize; } set { _stationOCSize = value; } }

        public ObservableCollection<Themes.ComboBoxItem> StationTypes {get { return _stationTypes; } set { _stationTypes = value; } }
        public string SelectedStationTypes{ get { return _selectedStationTypes; } set { _selectedStationTypes = value; } }

        public ObservableCollection<Themes.ComboBoxItem> ObservationSource { get { return _observationSource; } set { _observationSource = value; } }
        public string SelectedObservationSources { get { return _selectedObservationSource; } set { _selectedObservationSource = value; } }

        public ObservableCollection<Themes.ComboBoxItem> StationQuality { get { return _stationQuality; } set { _stationQuality = value; } }
        public string SelectedStationQuality{ get { return _selectedStationQuality; } set {  _selectedStationQuality = value; } }
        public ObservableCollection<Themes.ComboBoxItem> StationQualityValues { get { return _stationQualityValues; } set { _stationQualityValues = value; } }
        public ObservableCollection<Themes.ComboBoxItem> StationPhysEnv { get { return _stationPhysEnv; } set { _stationPhysEnv = value; } }
        public string SelectedStationPhysEnv { get { return _selectedStationPhysEnv; } set { _selectedStationPhysEnv = value; } }

        #endregion

        #region METHODS
        public void SetCurrentLocationInUI(FieldLocation inPosition)
        {
            _location = inPosition;
            _latitude = inPosition.LocationLat;
            _longitude = inPosition.LocationLong;
            _elevation = inPosition.LocationElev;
        }


        public void SaveDialogInfo()
        {
            bool doStationUpdate = false;
            Themes.ConcatenatedCombobox concat = new Themes.ConcatenatedCombobox();

            //Save the new location only if the modal dialog wasn't pop for edition
            if (existingDataDetail == null) //New Station
            {
                //Init location if it doesn't already exist from a manual entry
                if (_location.LocationEntryType != Dictionaries.DatabaseLiterals.locationEntryTypeManual)
                {
                    _locationid = QuickLocation(_location);
                }
                else
                {
                    _locationid = _location.LocationID;
                }

                //Calculate new values for station too
                _dateDate = idCalculator.FormatDate(_dateGeneric.DateTime); //Calculate new value
                _dateTime = idCalculator.FormatTime(_dateGeneric.DateTime);//Calculate new value

            }
            else //Existing station
            {
                doStationUpdate = true;

                if (existingDataDetail.ParentID != 0)
                {
                    _locationid = existingDataDetail.ParentID; //Get location id from parent
                }
                else
                {
                    _locationid = existingDataDetail.GenericID; //Get location id from generic
                }

                //Synchronize with values that can't be changed by the user.
                _stationid = existingDataDetail.GenericID;
                _alias = existingDataDetail.station.StationAlias;
                _dateDate = existingDataDetail.station.StationVisitDate;
                _dateTime = existingDataDetail.station.StationVisitTime;
            }

            //Save the new station
            StationModel.StationID = StationID; //Prime key
            StationModel.LocationID = LocationID; //Foreign key
            StationModel.StationAlias = Alias;
            StationModel.StationVisitDate = DDate;
            StationModel.StationVisitTime = DTime;
            StationModel.StationNote = Notes;
            StationModel.StationAirNo = AirPhoto;
            StationModel.StationRelatedTo = RelatedTo;
            StationModel.StationSLSNotes = SlSNotes;
            StationModel.StationOCSize = StationOCSize;

            if (TraverseNo != null && TraverseNo != string.Empty)
            {
                StationModel.StationTravNo = Convert.ToInt16(TraverseNo);
            }

            if (SelectedStationTypes != null)
            {
                StationModel.StationObsType = SelectedStationTypes;
            }
            if (_stationQualityValues != null)
            {
                StationModel.StationOCQuality = concat.PipeValues(_stationQualityValues); //process list of values so they are concatenated.
            }
            if (SelectedStationPhysEnv != null)
            {
                StationModel.StationPhysEnv = SelectedStationPhysEnv;
            }
            if (SelectedObservationSources != null)
            {
                StationModel.StationObsSource = SelectedObservationSources;
            }


            object stationObject = (object)StationModel;
            accessData.SaveFromSQLTableObject(ref stationObject, doStationUpdate);
            StationModel = (Station)stationObject;

            //accessData.SaveFromSQLTableObject(StationModel, doStationUpdate);

            if (newStationEdit!=null)
            {
                newStationEdit(this);
            }
            
        }

        public void AutoFillDialog(FieldNotes incomingData, bool isWaypoint)
        {
            existingDataDetail = incomingData;

            _latitude = Convert.ToDouble(existingDataDetail.location.LocationLat);
            _longitude = Convert.ToDouble(existingDataDetail.location.LocationLong);
            _elevation = Convert.ToDouble(existingDataDetail.location.LocationElev);
            _alias = existingDataDetail.station.StationAlias;
            _notes = existingDataDetail.station.StationNote;
            _airno = existingDataDetail.station.StationAirNo;
            _slsnotes = existingDataDetail.station.StationSLSNotes;
            _stationRelatedTo = existingDataDetail.station.StationRelatedTo;
            _stationOCSize = existingDataDetail.station.StationOCSize;
            _stationTravNo = existingDataDetail.station.StationTravNo.ToString();
            _selectedStationTypes = existingDataDetail.station.StationObsType;
            //_selectedStationQuality = existingDataDetail.station.StationOCQuality;
            _selectedStationPhysEnv = existingDataDetail.station.StationPhysEnv;
            _selectedObservationSource = existingDataDetail.station.StationObsSource;

            RaisePropertyChanged("Notes");
            RaisePropertyChanged("Alias");
            RaisePropertyChanged("AirPhoto");
            RaisePropertyChanged("SlSNotes");
            RaisePropertyChanged("StationOCSize");
            RaisePropertyChanged("SelectedStationTypes");
            //RaisePropertyChanged("SelectedStationQuality");
            RaisePropertyChanged("SelectedStationPhysEnv");
            RaisePropertyChanged("TraverseNo");
            RaisePropertyChanged("RelatedTo");
            RaisePropertyChanged("SelectedObservationSources");

            //Concatenated box
            Themes.ConcatenatedCombobox ccBox = new Themes.ConcatenatedCombobox();
            foreach (string s in ccBox.UnpipeString(existingDataDetail.station.StationOCQuality))
            {
                AddAConcatenatedValue(s, DatabaseLiterals.FieldStationOCQuality);
            }

            //Validate waypoint form
            if (_selectedStationTypes == DatabaseLiterals.KeywordStationWaypoint || isWaypoint)
            {
                _waypointVisibility = Visibility.Collapsed;
                RaisePropertyChanged("WaypointVisibility");
            }
            else 
            {
                _waypointVisibility = Visibility.Visible;
                RaisePropertyChanged("WaypointVisibility");
            }


        }

        /// <summary>
        /// Will prefill air photo number and traverse number from the last entered values by user.
        /// </summary>
        private void FillAirPhotoNo_TraverseNo()
        {
            //Special case for air photo and traverse numbers, get the last numbers
            string tableName = Dictionaries.DatabaseLiterals.TableStation;
            string querySelectFrom = "SELECT * FROM " + tableName;
            string queryOrder1 = " ORDER BY " + tableName + "." + Dictionaries.DatabaseLiterals.FieldStationVisitDate + " DESC";
            string queryOrder2 = ", " + tableName + "." + Dictionaries.DatabaseLiterals.FieldStationVisitTime + " DESC";
            string queryLimit = " LIMIT 1";
            string finaleQuery = querySelectFrom + queryOrder1 + queryOrder2 + queryLimit;

            List<object> stationTableRaw = accessData.ReadTable(StationModel.GetType(), finaleQuery);
            IEnumerable<Station> stationFiltered = stationTableRaw.Cast<Station>(); //Cast to proper list type
            if (stationFiltered.Count() != 0 || stationFiltered != null)
            {
                foreach (Station sts in stationFiltered)
                {
                    _stationTravNo = sts.StationTravNo.ToString();

                    //Make check on date if newer, increment traverse no. if wanted by user
                    if (localSetting.GetSettingValue(ApplicationLiterals.KeywordStationTraverseNo) != null && localSetting.GetBoolSettingValue(ApplicationLiterals.KeywordStationTraverseNo))
                    {
                        string currentDate = DateTime.Now.ToShortDateString();
                        DateTime lastStationDate = DateTime.Parse(sts.StationVisitDate);
                        DateTime currentDateDT = DateTime.Parse(currentDate);
                        if (lastStationDate!= null && currentDateDT != null)
                        {
                            int dateComparisonResult = DateTime.Compare(lastStationDate, currentDateDT);
                            if (lastStationDate != null && dateComparisonResult < 0)
                            {
                                _stationTravNo = (sts.StationTravNo + 1).ToString();
                            }
                        }

                    }



                    _airno = sts.StationAirNo;
                }

            }

            RaisePropertyChanged("TraverseNo");
            RaisePropertyChanged("AirPhoto");
        }

        /// <summary>
        /// Will fill the station physical environment combobox
        /// </summary>
        private void FillStationPhysEnv()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldStationPhysEnv;
            string tableName = Dictionaries.DatabaseLiterals.TableStation;
            foreach (var itemPE in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedStationPhysEnv))
            {
                _stationPhysEnv.Add(itemPE);
            }
            

            //Update UI
            RaisePropertyChanged("StationPhysEnv");
            RaisePropertyChanged("SelectedStationPhysEnv");

        }

        /// <summary>
        /// Will fill the station outcrop quality combobox
        /// </summary>
        private void FillStationQuality()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldStationOCQuality;
            string tableName = Dictionaries.DatabaseLiterals.TableStation;
            foreach (var itemSQ in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedStationQuality))
            {
                _stationQuality.Add(itemSQ);
            }
            

            //Update UI
            RaisePropertyChanged("StationQuality");
            RaisePropertyChanged("SelectedStationQuality");

        }

        /// <summary>
        /// Will fill the station outcrop quality combobox
        /// </summary>
        private void FillObsSource()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldStationObsSource;
            string tableName = Dictionaries.DatabaseLiterals.TableStation;
            foreach (var itemSQ in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedObservationSource))
            {
                _observationSource.Add(itemSQ);
            }


            //Update UI
            RaisePropertyChanged("ObservationSource");
            RaisePropertyChanged("SelectedObservationSources");

        }

        /// <summary>
        /// Will fill the station outcrop type combobox
        /// </summary>
        private void FillStationType()
        {

            //Fill vocab list
            string fieldName = Dictionaries.DatabaseLiterals.FieldStationObsType;
            string tableName = Dictionaries.DatabaseLiterals.TableStation;
            foreach (var itemST in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedStationTypes))
            {
                _stationTypes.Add(itemST);
            }

            //Update UI
            RaisePropertyChanged("StationTypes");
            RaisePropertyChanged("SelectedStationTypes");

        }


        #endregion

        #region THEMES

        public void TransformToWaypointTheme()
        {
            //Get waypoint picklists value
            string waypointCode = string.Empty;
            string waypointName = string.Empty;
            foreach (Themes.ComboBoxItem cbxI in _stationTypes)
            {
                if (cbxI.itemValue.Contains(Dictionaries.DatabaseLiterals.KeywordStationWaypoint))
                {
                    waypointCode = cbxI.itemValue;
                    waypointName = cbxI.itemName;
                }
            }

            //Calculate alias
            _alias = idCalculator.CalculateStationWaypointAlias(waypointCode, waypointName);

            //Select obs type
            foreach (Themes.ComboBoxItem sTypes in _stationTypes)
            {
                if (sTypes.itemValue == Dictionaries.DatabaseLiterals.KeywordStationWaypoint)
                {
                    _selectedStationTypes = sTypes.itemValue;
                }
            }
            RaisePropertyChanged("SelectedStationTypes");

            //Clear other 

            //Disable controls that can't be changed by user
            _waypointVisibility = Visibility.Collapsed;
            RaisePropertyChanged("WaypointVisibility");
        }

        /// <summary>
        /// Will set visibility based on a bedrock or surficial field book
        /// </summary>
        private void SetFieldVisibility()
        {
            if (localSetting.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoFWorkType) != null)
            {
                if (localSetting.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoFWorkType).ToString() == Dictionaries.ScienceLiterals.ApplicationThemeBedrock)
                {
                    _bedrockVisibility = Visibility.Visible;
                }
                else if (localSetting.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoFWorkType).ToString() == Dictionaries.ScienceLiterals.ApplicationThemeSurficial)
                {
                    _bedrockVisibility = Visibility.Collapsed;
                }
            }
            else
            {
                //Fallback
                _bedrockVisibility = Visibility.Visible;
            }

            RaisePropertyChanged("BedrockVisibility");
        }

        #endregion

        #region Quickie

        /// <summary>
        /// Will make quick location record in location table, from a given xy position
        /// </summary>
        /// <param name="inLocation"></param>
        /// <returns>Location ID</returns>
        public int QuickLocation(FieldLocation inLocation)
        {

            //Set location
            if (inLocation != null)
            {
                //SetCurrentLocationInUI(inLocation);
                Location.LocationElevMethod = inLocation.LocationElevMethod;
                Location.LocationEntryType = inLocation.LocationEntryType;
                Location.LocationErrorMeasure = inLocation.LocationErrorMeasure;
                Location.LocationErrorMeasureType = inLocation.LocationErrorMeasureType;
                Location.LocationElevationAccuracy = inLocation.LocationElevationAccuracy;
                Location.LocationPDOP = inLocation.LocationPDOP;
                Location.LocationLat = inLocation.LocationLat;
                Location.LocationLong = inLocation.LocationLong;
                Location.LocationElev = inLocation.LocationElev;
                Location.LocationDatum = Dictionaries.DatabaseLiterals.KeywordEPSGDefault;

            }
            else
            {
                //Init location
                Location.LocationLat = _latitude;
                Location.LocationLong = _longitude;
                Location.LocationElev = _elevation;
            }

            
            Location.LocationAlias = _locationAlias = idCalculator.CalculateLocationAlias(_alias); //Calculate new value
            //Location.LocationID = _locationid = idCalculator.CalculateLocationID(_locationAlias); //Calculate new value
            Location.MetaID = int.Parse(localSetting.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoID).ToString()); //Foreign key

            //Fill in the table location
            //accessData.SaveFromSQLTableObject(Location, false);

            //Fill in the feature location
            GeopackageService geoService = new GeopackageService();
            string insertQuery = accessData.GetGeopackageInsertQuery(Location);
            
            Location.LocationID = geoService.DoSpatialiteQueryInGeopackage(insertQuery);
            return Location.LocationID;
        }

        /// <summary>
        /// Will make a quick station record in station table, from a given xy position. 
        /// XY will be used to create a quick location first
        /// </summary>
        /// <param name="inPosition"></param>
        /// <returns>A detail report class</returns>
        public FieldNotes QuickStation(FieldLocation inPosition)
        {
            //Create a quick location first
            int quickLocID = QuickLocation(inPosition);

            //Calculate air and traverse #
            FillAirPhotoNo_TraverseNo();

            //Save the new station
            //StationModel.StationID = _stationid; //Prime key
            StationModel.LocationID = quickLocID; //Foreign key
            StationModel.StationAlias = _alias;
            StationModel.StationVisitDate = _dateDate = idCalculator.FormatDate(_dateGeneric.DateTime); //Calculate new value
            StationModel.StationVisitTime = _dateTime = idCalculator.FormatTime(_dateGeneric.DateTime);//Calculate new value
            StationModel.StationAirNo = _airno;
            if (_stationTravNo!=string.Empty)
            {
                StationModel.StationTravNo = Convert.ToInt32(_stationTravNo);
            }

            object stationObject = (object)StationModel;
            accessData.SaveFromSQLTableObject(ref stationObject, false);
            StationModel = (Station)stationObject;

            //accessData.SaveFromSQLTableObject(StationModel, false);

            FieldNotes outputStationReport = new FieldNotes
            {
                ParentID = quickLocID,
                GenericID = _stationid,
                GenericAliasName = _alias,
                GenericTableName = Dictionaries.DatabaseLiterals.TableStation,
                station = StationModel
            };

            return outputStationReport;
        }

        #endregion

        #region DELETE
        public async Task<bool> DeleteAssociatedLocationIfManualEntryAsync()
        {
            //Variables
            bool proceed = false;

            var loadLocalization = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            ContentDialog deleteRecordDialog = new ContentDialog()
            {
                Title = loadLocalization.GetString("DeleteDialogGenericTitle"),
                Content = loadLocalization.GetString("DeleteDialog_ManualEntryStationLocation"),
                PrimaryButtonText = loadLocalization.GetString("Generic_ButtonYes/Content"),
                SecondaryButtonText = loadLocalization.GetString("Generic_ButtonNo/Content")
            };
            deleteRecordDialog.Style = (Style)Application.Current.Resources["DeleteDialog"];

            ContentDialogResult drd = await deleteRecordDialog.ShowAsync();

            if (drd == ContentDialogResult.Primary)
            {
                //Delete location 
                accessData.DeleteRecord(Dictionaries.DatabaseLiterals.TableLocation, Dictionaries.DatabaseLiterals.FieldLocationID, _location.LocationID);

                proceed = true;
            }

            return proceed;
        }
        #endregion

        #region CONCATENATED VALUES

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
        /// Will remove a category
        /// </summary>
        /// <param name="inPurpose"></param>
        public void RemoveSelectedValue(object inPurpose, string parentListViewName)
        {

            Themes.ComboBoxItem oldValue = inPurpose as Themes.ComboBoxItem;

            if (parentListViewName.ToLower().Contains(Dictionaries.DatabaseLiterals.FieldStationOCQuality.ToLower()))
            {
                _stationQualityValues.Remove(oldValue);
                RaisePropertyChanged("StationQualityValues");
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

                if (NameToValidate.ToLower().Contains(Dictionaries.DatabaseLiterals.FieldStationOCQuality.ToLower()))
                {
                    parentCollection = StationQuality;
                    parentConcatCollection = _stationQualityValues;
                    parentProperty = "StationQuality";

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
