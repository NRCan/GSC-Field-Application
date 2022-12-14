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
        private string _stationid = string.Empty; //Default
        private string _locationid = string.Empty; //Default
        private string _locationAlias = string.Empty; //Default
        private string _airno = string.Empty; //Default
        private string _slsnotes = string.Empty; //Default
        private DateTimeOffset _dateGeneric = DateTime.Now; //Default
        private string _dateDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now); //Default
        private string _dateTime = String.Format("{0:HH:mm:ss t}", DateTime.Now); //Default
        private string _notes = string.Empty; //Default
        private string _stationOCSize = string.Empty;
        private string _stationTravNo = string.Empty;
        private bool _enability = true; //Default

        private ObservableCollection<Themes.ComboBoxItem> _stationTypes = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedStationTypes = string.Empty;

        private ObservableCollection<Themes.ComboBoxItem> _stationQuality = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedStationQuality = string.Empty;

        private ObservableCollection<Themes.ComboBoxItem> _stationPhysEnv = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedStationPhysEnv = string.Empty;

        public FieldNotes existingDataDetail;

        public DataIDCalculation idCalculator = new DataIDCalculation();

        private Visibility _bedrockVisibility = Visibility.Visible; //Visibility for extra fields

        //Events and delegate
        public delegate void stationEditEventHandler(object sender); //A delegate for execution events
        public event stationEditEventHandler newStationEdit; //This event is triggered when a save has been done on station table.

        readonly DataLocalSettings localSetting = new DataLocalSettings();
        readonly DataAccess accessData = new DataAccess();

        public StationViewModel(bool isWayPoint)
        {
            //On init for new stations calculate values so UI shows stuff.
            _stationid = idCalculator.CalculateStationID();

            //Fill controls
            FillStationType();

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
            }

            SetFieldVisibility(); //Will enable/disable some fields based on bedrock or surficial usage

        }

        #endregion

        #region PROPERTIES
        public Visibility BedrockVisibility { get { return _bedrockVisibility; } set { _bedrockVisibility = value; } }

        public string Latitude { get { return _latitude.ToString(); } set { _latitude = Convert.ToDouble(value); } }
        public string Longitude { get { return _longitude.ToString(); } set { _longitude = Convert.ToDouble(value); } }
        public string Elevation { get { return _elevation.ToString(); } set { _elevation = Convert.ToDouble(value); } }
        public DateTimeOffset DateGeneric { get { return _dateGeneric; } set { _dateGeneric = value; } }
        public string DDate { get { return _dateDate; } set { _dateDate = value; } }
        public string DTime { get { return _dateTime; } set { _dateTime = value; } }
        public Station StationModel { get { return model; } set { model = value; } }
        public FieldLocation Location { get { return _location; } set { _location = value; } }
        public string Alias { get { return _alias; } set { _alias = value; } }
        public string StationID { get { return _stationid; } set { _stationid = value; } }
        public string LocationID { get { return _locationid; } set { _locationid = value; } }
        public string Notes { get { return _notes; } set { _notes = value; } }
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
        public bool Enability { get { return _enability; } set { _enability = value; } }

        public ObservableCollection<Themes.ComboBoxItem> StationTypes {get { return _stationTypes; } set { _stationTypes = value; } }
        public string SelectedStationTypes{ get { return _selectedStationTypes; } set { _selectedStationTypes = value; } }

        public ObservableCollection<Themes.ComboBoxItem> StationQuality { get { return _stationQuality; } set { _stationQuality = value; } }
        public string SelectedStationQuality{ get { return _selectedStationQuality; } set {  _selectedStationQuality = value; } }

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

            //Save the new location only if the modal dialog wasn't pop for edition
            if (existingDataDetail == null) //New Station
            {
                //Init location if it doesn't already exist from a manual entry
                if (_location.LocationEntryType != Dictionaries.DatabaseLiterals.locationEntryTypeManual)
                {
                    QuickLocation(_location);
                }
                else
                {
                    _locationid = _location.LocationID;
                }

                //Calculate new values for station too
                _dateDate = CalculateStationDate(); //Calculate new value
                _dateTime = CalculateStationTime();//Calculate new value

            }
            else //Existing station
            {
                doStationUpdate = true;

                if (existingDataDetail.ParentID != null && existingDataDetail.ParentID != string.Empty)
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
            if (SelectedStationQuality != null)
            {
                StationModel.StationOCQuality = SelectedStationQuality;
            }
            if (SelectedStationPhysEnv != null)
            {
                StationModel.StationPhysEnv = SelectedStationPhysEnv;
            }

            accessData.SaveFromSQLTableObject(StationModel, doStationUpdate);

            if (newStationEdit!=null)
            {
                newStationEdit(this);
            }
            
        }

        public void AutoFillDialog(FieldNotes incomingData, bool isWaypoint)
        {
            existingDataDetail = incomingData;

            if (isWaypoint)
            {
                _enability = false;
                RaisePropertyChanged("Enability");
            }

            _latitude = Convert.ToDouble(existingDataDetail.location.LocationLat);
            _longitude = Convert.ToDouble(existingDataDetail.location.LocationLong);
            _elevation = Convert.ToDouble(existingDataDetail.location.LocationElev);
            _alias = existingDataDetail.station.StationAlias;
            _notes = existingDataDetail.station.StationNote;
            _airno = existingDataDetail.station.StationAirNo;
            _slsnotes = existingDataDetail.station.StationSLSNotes;
            _stationOCSize = existingDataDetail.station.StationOCSize;
            _stationTravNo = existingDataDetail.station.StationTravNo.ToString();
            _selectedStationTypes = existingDataDetail.station.StationObsType;
            _selectedStationQuality = existingDataDetail.station.StationOCQuality;
            _selectedStationPhysEnv = existingDataDetail.station.StationPhysEnv;
            
            RaisePropertyChanged("Notes");
            RaisePropertyChanged("Alias");
            RaisePropertyChanged("AirPhoto");
            RaisePropertyChanged("SlSNotes");
            RaisePropertyChanged("StationOCSize");
            RaisePropertyChanged("SelectedStationTypes");
            RaisePropertyChanged("SelectedStationQuality");
            RaisePropertyChanged("SelectedStationPhysEnv");
            RaisePropertyChanged("TraverseNo");

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
                    if ((bool)localSetting.GetSettingValue(ApplicationLiterals.KeywordStationTraverseNo))
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

        #region CALCULATIONS

        public string CalculateStationDate()
        {
            return String.Format("{0:yyyy-MM-dd}", _dateGeneric); ;
        }

        public string CalculateStationTime()
        {
            return String.Format("{0:HH:mm:ss t}", _dateGeneric); ;
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
            _enability = false;
            RaisePropertyChanged("Enability");
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
        public string QuickLocation(FieldLocation inLocation)
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

            Location.LocationID = _locationid = idCalculator.CalculateLocationID(); //Calculate new value
            Location.LocationAlias = _locationAlias = idCalculator.CalculateLocationAlias(_alias); //Calculate new value
            Location.MetaID = localSetting.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoID).ToString(); //Foreign key


            accessData.SaveFromSQLTableObject(Location, false);

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
            string quickLocID = QuickLocation(inPosition);

            //Calculate air and traverse #
            FillAirPhotoNo_TraverseNo();

            //Save the new station
            StationModel.StationID = _stationid; //Prime key
            StationModel.LocationID = quickLocID; //Foreign key
            StationModel.StationAlias = _alias;
            StationModel.StationVisitDate = _dateDate = CalculateStationDate(); //Calculate new value
            StationModel.StationVisitTime = _dateTime = CalculateStationTime();//Calculate new value
            StationModel.StationAirNo = _airno;
            if (_stationTravNo!=string.Empty)
            {
                StationModel.StationTravNo = Convert.ToInt32(_stationTravNo);
            }
            

            accessData.SaveFromSQLTableObject(StationModel, false);

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
    }
}
