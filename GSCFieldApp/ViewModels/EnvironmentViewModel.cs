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
using System.Runtime.ConstrainedExecution;

namespace GSCFieldApp.ViewModels
{
    public class EnvironmentViewModel : ViewModelBase
    {
        #region INITIALIZATION
        private EnvironmentModel environmentModel = new EnvironmentModel();
        private string _environmentid = string.Empty;
        private string _environmentAlias = string.Empty;
        public bool doEnvironmentUpdate = false;

        //private ObservableCollection<Themes.ComboBoxItem> _stationTypes = new ObservableCollection<Themes.ComboBoxItem>();
        //private string _selectedStationTypes = string.Empty;

        //private ObservableCollection<Themes.ComboBoxItem> _observationSource = new ObservableCollection<Themes.ComboBoxItem>();
        //private string _selectedObservationSource = string.Empty;

        //private ObservableCollection<Themes.ComboBoxItem> _stationQuality = new ObservableCollection<Themes.ComboBoxItem>();
        //private ObservableCollection<Themes.ComboBoxItem> _stationQualityValues = new ObservableCollection<Themes.ComboBoxItem>();
        //private string _selectedStationQuality = string.Empty;

        //private ObservableCollection<Themes.ComboBoxItem> _stationPhysEnv = new ObservableCollection<Themes.ComboBoxItem>();
        //private string _selectedStationPhysEnv = string.Empty;

        public FieldNotes existingDataDetail;

        public DataIDCalculation idCalculator = new DataIDCalculation();

        //Events and delegate
        public delegate void environmentEditEventHandler(object sender); //A delegate for execution events
        public event environmentEditEventHandler newEnvironmentEdit; //This event is triggered when a save has been done on station table.

        readonly DataLocalSettings localSetting = new DataLocalSettings();
        readonly DataAccess accessData = new DataAccess();

        public EnvironmentViewModel(FieldNotes inReportModel)
        {
            //On init for new stations calculate values so UI shows stuff.
            _environmentid = idCalculator.CalculateEnvironmentID();

            //Fill controls
            //FillStationType();

            _environmentAlias = idCalculator.CalculateEnvironmentAlias(inReportModel.station.StationID,inReportModel.station.StationAlias);

        }

        #endregion

        #region PROPERTIES

        //public string Latitude { get { return _latitude.ToString(); } set { _latitude = Convert.ToDouble(value); } }
        //public string Longitude { get { return _longitude.ToString(); } set { _longitude = Convert.ToDouble(value); } }
        //public string Elevation { get { return _elevation.ToString(); } set { _elevation = Convert.ToDouble(value); } }
        //public DateTimeOffset DateGeneric { get { return _dateGeneric; } set { _dateGeneric = value; } }
        //public string DDate { get { return _dateDate; } set { _dateDate = value; } }
        //public string DTime { get { return _dateTime; } set { _dateTime = value; } }
        //public Station StationModel { get { return model; } set { model = value; } }
        //public FieldLocation Location { get { return _location; } set { _location = value; } }
        public string Alias { get { return _environmentAlias; } set { _environmentAlias = value; } }
        //public string StationID { get { return _stationid; } set { _stationid = value; } }
        //public string LocationID { get { return _locationid; } set { _locationid = value; } }
        //public string Notes { get { return _notes; } set { _notes = value; } }
        //public string RelatedTo { get { return _stationRelatedTo; } set { _stationRelatedTo = value; } }
        //public string AirPhoto { get { return _airno; } set { _airno = value; } }
        //public string TraverseNo
        //{
        //    get
        //    {
        //        return _stationTravNo;
        //    }
        //    set
        //    {

        //        bool result = int.TryParse(value, out int index);

        //        if (result)
        //        {
        //            if (index >= 0 && index <= 999999)
        //            {
        //                _stationTravNo = value;
        //            }
        //            else
        //            {
        //                _stationTravNo = value = "0";
        //                RaisePropertyChanged("TraverseNo");
        //            }

        //        }
        //        else
        //        {
        //            _stationTravNo = value = "0";
        //            RaisePropertyChanged("TraverseNo");
        //        }

        //    }
        //}
        //public string SlSNotes { get { return _slsnotes; } set { _slsnotes = value; } }
        //public string StationOCSize { get { return _stationOCSize; } set { _stationOCSize = value; } }
        //public bool Enability { get { return _enability; } set { _enability = value; } }

        //public ObservableCollection<Themes.ComboBoxItem> StationTypes {get { return _stationTypes; } set { _stationTypes = value; } }
        //public string SelectedStationTypes{ get { return _selectedStationTypes; } set { _selectedStationTypes = value; } }

        //public ObservableCollection<Themes.ComboBoxItem> ObservationSource { get { return _observationSource; } set { _observationSource = value; } }
        //public string SelectedObservationSources { get { return _selectedObservationSource; } set { _selectedObservationSource = value; } }

        //public ObservableCollection<Themes.ComboBoxItem> StationQuality { get { return _stationQuality; } set { _stationQuality = value; } }
        //public string SelectedStationQuality{ get { return _selectedStationQuality; } set {  _selectedStationQuality = value; } }
        //public ObservableCollection<Themes.ComboBoxItem> StationQualityValues { get { return _stationQualityValues; } set { _stationQualityValues = value; } }
        //public ObservableCollection<Themes.ComboBoxItem> StationPhysEnv { get { return _stationPhysEnv; } set { _stationPhysEnv = value; } }
        //public string SelectedStationPhysEnv { get { return _selectedStationPhysEnv; } set { _selectedStationPhysEnv = value; } }

        #endregion

        #region METHODS

        public void SaveDialogInfo()
        {
            //bool doStationUpdate = false;
            //Themes.ConcatenatedCombobox concat = new Themes.ConcatenatedCombobox();

            ////Save the new location only if the modal dialog wasn't pop for edition
            //if (existingDataDetail == null) //New Station
            //{
            //    //Init location if it doesn't already exist from a manual entry
            //    if (_location.LocationEntryType != Dictionaries.DatabaseLiterals.locationEntryTypeManual)
            //    {
            //        QuickLocation(_location);
            //    }
            //    else
            //    {
            //        _locationid = _location.LocationID;
            //    }

            //    //Calculate new values for station too
            //    _dateDate = CalculateStationDate(); //Calculate new value
            //    _dateTime = CalculateStationTime();//Calculate new value

            //}
            //else //Existing station
            //{
            //    doStationUpdate = true;

            //    if (existingDataDetail.ParentID != null && existingDataDetail.ParentID != string.Empty)
            //    {
            //        _locationid = existingDataDetail.ParentID; //Get location id from parent
            //    }
            //    else
            //    {
            //        _locationid = existingDataDetail.GenericID; //Get location id from generic
            //    }

            //    //Synchronize with values that can't be changed by the user.
            //    _stationid = existingDataDetail.GenericID;
            //    _alias = existingDataDetail.station.StationAlias;
            //    _dateDate = existingDataDetail.station.StationVisitDate;
            //    _dateTime = existingDataDetail.station.StationVisitTime;
            //}

            ////Save the new station
            //StationModel.StationID = StationID; //Prime key
            //StationModel.LocationID = LocationID; //Foreign key
            //StationModel.StationAlias = Alias;
            //StationModel.StationVisitDate = DDate;
            //StationModel.StationVisitTime = DTime;
            //StationModel.StationNote = Notes;
            //StationModel.StationAirNo = AirPhoto;
            //StationModel.StationRelatedTo = RelatedTo;
            //StationModel.StationSLSNotes = SlSNotes;
            //StationModel.StationOCSize = StationOCSize;

            //if (TraverseNo != null && TraverseNo != string.Empty)
            //{
            //    StationModel.StationTravNo = Convert.ToInt16(TraverseNo);
            //}

            //if (SelectedStationTypes != null)
            //{
            //    StationModel.StationObsType = SelectedStationTypes;
            //}
            //if (_stationQualityValues != null)
            //{
            //    StationModel.StationOCQuality = concat.PipeValues(_stationQualityValues); //process list of values so they are concatenated.
            //}
            //if (SelectedStationPhysEnv != null)
            //{
            //    StationModel.StationPhysEnv = SelectedStationPhysEnv;
            //}
            //if (SelectedObservationSources != null)
            //{
            //    StationModel.StationObsSource = SelectedObservationSources;
            //}


            //accessData.SaveFromSQLTableObject(StationModel, doStationUpdate);

            //if (newStationEdit != null)
            //{
            //    newStationEdit(this);
            //}

        }

        public void AutoFillDialog(FieldNotes incomingData)
        {
            //existingDataDetail = incomingData;

            //if (isWaypoint)
            //{
            //    _enability = false;
            //    RaisePropertyChanged("Enability");
            //}

            //_latitude = Convert.ToDouble(existingDataDetail.location.LocationLat);
            //_longitude = Convert.ToDouble(existingDataDetail.location.LocationLong);
            //_elevation = Convert.ToDouble(existingDataDetail.location.LocationElev);
            //_alias = existingDataDetail.station.StationAlias;
            //_notes = existingDataDetail.station.StationNote;
            //_airno = existingDataDetail.station.StationAirNo;
            //_slsnotes = existingDataDetail.station.StationSLSNotes;
            //_stationRelatedTo = existingDataDetail.station.StationRelatedTo;
            //_stationOCSize = existingDataDetail.station.StationOCSize;
            //_stationTravNo = existingDataDetail.station.StationTravNo.ToString();
            //_selectedStationTypes = existingDataDetail.station.StationObsType;
            ////_selectedStationQuality = existingDataDetail.station.StationOCQuality;
            //_selectedStationPhysEnv = existingDataDetail.station.StationPhysEnv;
            //_selectedObservationSource = existingDataDetail.station.StationObsSource;

            //RaisePropertyChanged("Notes");
            //RaisePropertyChanged("Alias");
            //RaisePropertyChanged("AirPhoto");
            //RaisePropertyChanged("SlSNotes");
            //RaisePropertyChanged("StationOCSize");
            //RaisePropertyChanged("SelectedStationTypes");
            ////RaisePropertyChanged("SelectedStationQuality");
            //RaisePropertyChanged("SelectedStationPhysEnv");
            //RaisePropertyChanged("TraverseNo");
            //RaisePropertyChanged("RelatedTo");
            //RaisePropertyChanged("SelectedObservationSources");

            ////Concatenated box
            //Themes.ConcatenatedCombobox ccBox = new Themes.ConcatenatedCombobox();
            //foreach (string s in ccBox.UnpipeString(existingDataDetail.station.StationOCQuality))
            //{
            //    AddAConcatenatedValue(s, DatabaseLiterals.FieldStationOCQuality);
            //}

        }

        ///// <summary>
        ///// Will prefill air photo number and traverse number from the last entered values by user.
        ///// </summary>
        //private void FillAirPhotoNo_TraverseNo()
        //{
        //    //Special case for air photo and traverse numbers, get the last numbers
        //    string tableName = Dictionaries.DatabaseLiterals.TableStation;
        //    string querySelectFrom = "SELECT * FROM " + tableName;
        //    string queryOrder1 = " ORDER BY " + tableName + "." + Dictionaries.DatabaseLiterals.FieldStationVisitDate + " DESC";
        //    string queryOrder2 = ", " + tableName + "." + Dictionaries.DatabaseLiterals.FieldStationVisitTime + " DESC";
        //    string queryLimit = " LIMIT 1";
        //    string finaleQuery = querySelectFrom + queryOrder1 + queryOrder2 + queryLimit;

        //    List<object> stationTableRaw = accessData.ReadTable(StationModel.GetType(), finaleQuery);
        //    IEnumerable<Station> stationFiltered = stationTableRaw.Cast<Station>(); //Cast to proper list type
        //    if (stationFiltered.Count() != 0 || stationFiltered != null)
        //    {
        //        foreach (Station sts in stationFiltered)
        //        {
        //            _stationTravNo = sts.StationTravNo.ToString();

        //            //Make check on date if newer, increment traverse no. if wanted by user
        //            if ((bool)localSetting.GetSettingValue(ApplicationLiterals.KeywordStationTraverseNo))
        //            {
        //                string currentDate = DateTime.Now.ToShortDateString();
        //                DateTime lastStationDate = DateTime.Parse(sts.StationVisitDate);
        //                DateTime currentDateDT = DateTime.Parse(currentDate);
        //                if (lastStationDate!= null && currentDateDT != null)
        //                {
        //                    int dateComparisonResult = DateTime.Compare(lastStationDate, currentDateDT);
        //                    if (lastStationDate != null && dateComparisonResult < 0)
        //                    {
        //                        _stationTravNo = (sts.StationTravNo + 1).ToString();
        //                    }
        //                }

        //            }



        //            _airno = sts.StationAirNo;
        //        }

        //    }

        //    RaisePropertyChanged("TraverseNo");
        //    RaisePropertyChanged("AirPhoto");
        //}

        ///// <summary>
        ///// Will fill the station physical environment combobox
        ///// </summary>
        //private void FillStationPhysEnv()
        //{
        //    //Init.
        //    string fieldName = Dictionaries.DatabaseLiterals.FieldStationPhysEnv;
        //    string tableName = Dictionaries.DatabaseLiterals.TableStation;
        //    foreach (var itemPE in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedStationPhysEnv))
        //    {
        //        _stationPhysEnv.Add(itemPE);
        //    }


        //    //Update UI
        //    RaisePropertyChanged("StationPhysEnv");
        //    RaisePropertyChanged("SelectedStationPhysEnv");

        //}

        ///// <summary>
        ///// Will fill the station outcrop quality combobox
        ///// </summary>
        //private void FillStationQuality()
        //{
        //    //Init.
        //    string fieldName = Dictionaries.DatabaseLiterals.FieldStationOCQuality;
        //    string tableName = Dictionaries.DatabaseLiterals.TableStation;
        //    foreach (var itemSQ in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedStationQuality))
        //    {
        //        _stationQuality.Add(itemSQ);
        //    }


        //    //Update UI
        //    RaisePropertyChanged("StationQuality");
        //    RaisePropertyChanged("SelectedStationQuality");

        //}

        ///// <summary>
        ///// Will fill the station outcrop quality combobox
        ///// </summary>
        //private void FillObsSource()
        //{
        //    //Init.
        //    string fieldName = Dictionaries.DatabaseLiterals.FieldStationObsSource;
        //    string tableName = Dictionaries.DatabaseLiterals.TableStation;
        //    foreach (var itemSQ in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedObservationSource))
        //    {
        //        _observationSource.Add(itemSQ);
        //    }


        //    //Update UI
        //    RaisePropertyChanged("ObservationSource");
        //    RaisePropertyChanged("SelectedObservationSources");

        //}

        ///// <summary>
        ///// Will fill the station outcrop type combobox
        ///// </summary>
        //private void FillStationType()
        //{

        //    //Fill vocab list
        //    string fieldName = Dictionaries.DatabaseLiterals.FieldStationObsType;
        //    string tableName = Dictionaries.DatabaseLiterals.TableStation;
        //    foreach (var itemST in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedStationTypes))
        //    {
        //        _stationTypes.Add(itemST);
        //    }

        //    //Update UI
        //    RaisePropertyChanged("StationTypes");
        //    RaisePropertyChanged("SelectedStationTypes");

        //}


        #endregion

    }
}
