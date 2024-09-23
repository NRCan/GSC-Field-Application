using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GSCFieldApp.Views;
using GSCFieldApp.Services.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSCFieldApp.Models;
using static GSCFieldApp.Dictionaries.DatabaseLiterals;
using GSCFieldApp.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.Maui.ApplicationModel.Communication;
using System.Xml.Linq;
using System.Reflection;
using Microsoft.Maui.Controls.PlatformConfiguration;
using CommunityToolkit.Maui.Alerts;
using SQLite;
using GSCFieldApp.Services;
using System.Security.Cryptography;

namespace GSCFieldApp.ViewModel
{

    [QueryProperty(nameof(FieldLocation), nameof(FieldLocation))]
    [QueryProperty(nameof(Metadata), nameof(Metadata))]
    [QueryProperty(nameof(Station), nameof(Station))]
    public partial class StationViewModel : FieldAppPageHelper
    {
        #region INIT
        
        private Station _model = new Station();
        private DateTimeOffset _dateGeneric = DateTime.Now; //Default

        private ComboBox _stationType = new ComboBox();
        private ComboBox _stationOutcropQuality = new ComboBox();
        private ComboBox _stationSource = new ComboBox();
        private ComboBox _stationPhysEnv = new ComboBox();

        //Concatenated
        private ComboBoxItem _selectedStationOutcropQuality = new ComboBoxItem();
        private ObservableCollection<ComboBoxItem> _qualityCollection = new ObservableCollection<ComboBoxItem>();

        #endregion

        #region PROPERTIES

        [ObservableProperty]
        FieldLocation fieldLocation;

        [ObservableProperty]
        private Metadata metadata;

        [ObservableProperty]
        private Station _station;


        public bool StationTypeVisibility 
        {
            get { return Preferences.Get(nameof(StationTypeVisibility), true); }
            set { Preferences.Set(nameof(StationTypeVisibility), value); }
        }
        public bool StationOutcropVisibility
        {
            get { return Preferences.Get(nameof(StationOutcropVisibility), true); }
            set { Preferences.Set(nameof(StationOutcropVisibility), value); }
        }
        public bool StationGeneralVisibility
        {
            get { return Preferences.Get(nameof(StationGeneralVisibility), true); }
            set { Preferences.Set(nameof(StationGeneralVisibility), value); }
        }
        public bool StationNotesVisibility
        {
            get { return Preferences.Get(nameof(StationNotesVisibility), true); }
            set { Preferences.Set(nameof(StationNotesVisibility), value); }
        }
        public Station Model { get { return _model; } set { _model = value; } }
        public ComboBox StationType { get { return _stationType; } set { _stationType = value; } }
        public ComboBox StationOutcropQuality { get { return _stationOutcropQuality; } set { _stationOutcropQuality = value; } }
        public ComboBox StationSource { get { return _stationSource; } set { _stationSource = value; } }
        public ComboBox StationPhysEnv { get { return _stationPhysEnv; } set { _stationPhysEnv = value; } }

        public ComboBoxItem SelectedStationOutcropQuality 
        { 
            get 
            { 
                return _selectedStationOutcropQuality; 
            } 
            set 
            {
                if (_selectedStationOutcropQuality != value)
                {
                    if (_qualityCollection != null)
                    {
                        if (_qualityCollection.Count > 0 && _qualityCollection[0] == null)
                        {
                            _qualityCollection.RemoveAt(0);
                        }

                        if (value != null && value.itemName != string.Empty)
                        {
                            _qualityCollection.Add(value);
                            _selectedStationOutcropQuality = value;
                            OnPropertyChanged(nameof(QualityCollection));
                        }
                    }


                }
                
            } 
        }
        public ObservableCollection<ComboBoxItem> QualityCollection { get { return _qualityCollection; } set { _qualityCollection = value; } }

        //public bool BedrockVisibility { get { return _bedrockVisibility; } set { _bedrockVisibility = value; } }
        #endregion 

        public StationViewModel()
        {
            FieldThemes = new FieldThemes();
        }

        #region RELAYS

        [RelayCommand]
        public async Task Hide(string visibilityObjectName )
        {
            //Use reflection to parse incoming block to hide
            PropertyInfo? prop = typeof(StationViewModel).GetProperty( visibilityObjectName );

            if (prop != null)
            {
                bool propBool = (bool)prop.GetValue(this);

                // Reverse
                propBool = propBool ? false : true;

                prop.SetValue(this, propBool);
                OnPropertyChanged(visibilityObjectName);
            }

        }

        [RelayCommand]
        public async Task Back()
        {
            //Make sure to delete station and location records if user is coming from map page
            if (_station != null && _station.StationAlias != string.Empty && _model.StationID != 0)
            {
                //Delete without forced pop-up warning and question
                await commandServ.DeleteDatabaseItemCommand(TableNames.station, _station.StationAlias, _station.LocationID, true);

                //Exit on map
                await Shell.Current.GoToAsync($"////{nameof(MapPage)}/");

            }
            else
            {
                //Exit in field notes
                await NavigateToFieldNotes(TableNames.station);
            }

        }

        /// <summary>
        /// Will delete a selected item in quality collection box.
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        public async Task DeleteItem(ComboBoxItem item)
        {
            if (_qualityCollection.Contains(item))
            {
                _qualityCollection.Remove(item);
                OnPropertyChanged(nameof(QualityCollection));
            }

        }

        [RelayCommand]
        async Task Save()
        {
            //Fill out missing values in model
            await SetModelAsync();

            //Validate if new entry or update
            if (_station != null &&_station.StationAlias != string.Empty && _model.StationID != 0)
            {
                await da.SaveItemAsync(Model, true);

                //Close to be sure
                await da.CloseConnectionAsync();

                //Exit
                await NavigateToFieldNotes(TableNames.station);

            }
            else
            {
                //Insert new record
                await da.SaveItemAsync(Model, false);

                //Close to be sure
                await da.CloseConnectionAsync();

                //Exit
                await Shell.Current.GoToAsync($"////{nameof(MapPage)}/");

            }

           
        }

        /// <summary>
        /// Save button command
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        async Task SaveStay()
        {
            //Display a warning to user
            await Shell.Current.DisplayAlert(LocalizationResourceManager["DisplayAlertNotAllowed"].ToString(),
                LocalizationResourceManager["DisplayAlertNotAllowedContent"].ToString(),
                LocalizationResourceManager["GenericButtonOk"].ToString());

        }

        [RelayCommand]
        async Task SaveDelete()
        {
            if (_station != null && _station.StationID != 0)
            {
                await commandServ.DeleteDatabaseItemCommand(TableNames.station, _station.StationAlias, _station.LocationID);
            }

            //Exit
            await NavigateToFieldNotes(TableNames.meta);

        }

        [RelayCommand]
        async Task AddEarthmat()
        {
            //Fill out missing values in model
            await SetModelAsync();

            //Validate if new entry or update
            if (_station != null && _station.StationAlias != string.Empty)
            {
                await da.SaveItemAsync(Model, true);
            }
            else
            {
                //Insert new record
                await da.SaveItemAsync(Model, false);
            }

            //Close to be sure
            await da.CloseConnectionAsync();

            //Navigate to child
            await Shell.Current.GoToAsync($"////{nameof(FieldNotesPage)}/{nameof(EarthmatPage)}/",
                new Dictionary<string, object>
                {
                    [nameof(Earthmaterial)] = null,
                    [nameof(Station)] = Model
                }
            );
        }

        [RelayCommand]
        async Task AddDocument()
        {
            //Fill out missing values in model
            await SetModelAsync();

            //Validate if new entry or update
            if (_station != null && _station.StationAlias != string.Empty)
            {
                await da.SaveItemAsync(Model, true);
            }
            else
            {
                //Insert new record
                await da.SaveItemAsync(Model, false);
            }

            //Close to be sure
            await da.CloseConnectionAsync();

            //Navigate to child
            await Shell.Current.GoToAsync($"////{nameof(FieldNotesPage)}/{nameof(DocumentPage)}/",
                new Dictionary<string, object>
                {
                    [nameof(Station)] = Model,
                    [nameof(Document)] = null
                }
            );
        }

        [RelayCommand]
        async Task AddEnvironment()
        {
            //Fill out missing values in model
            await SetModelAsync();

            //Validate if new entry or update
            if (_station != null && _station.StationAlias != string.Empty)
            {
                await da.SaveItemAsync(Model, true);
            }
            else
            {
                //Insert new record
                await da.SaveItemAsync(Model, false);
            }

            //Close to be sure
            await da.CloseConnectionAsync();

            //Navigate to child
            await Shell.Current.GoToAsync($"////{nameof(FieldNotesPage)}/{nameof(EnvironmentPage)}/",
                new Dictionary<string, object>
                {
                    [nameof(EnvironmentModel)] = null,
                    [nameof(Station)] = Model
                }
            );
        }

        [RelayCommand]
        async Task AddMineralization()
        {
            //Fill out missing values in model
            await SetModelAsync();

            //Validate if new entry or update
            if (_station != null && _station.StationAlias != string.Empty)
            {
                await da.SaveItemAsync(Model, true);
            }
            else
            {
                //Insert new record
                await da.SaveItemAsync(Model, false);
            }

            //Close to be sure
            await da.CloseConnectionAsync();

            //Navigate to pflow page 
            await Shell.Current.GoToAsync($"////{nameof(FieldNotesPage)}/{nameof(MineralizationAlterationPage)}/",
                new Dictionary<string, object>
                {
                    [nameof(MineralAlteration)] = null,
                    [nameof(Earthmaterial)] = null,
                    [nameof(Station)] = Model,
                }
            );
        }
        #endregion

        #region METHODS

        /// <summary>
        /// Will refill the form with existing values for update/editing purposes
        /// </summary>
        /// <returns></returns>
        public async Task Load()
        {

            if (_station != null && _station.StationAlias != string.Empty)
            {
                //Set model like actual record
                _model = _station;

                //Refresh
                OnPropertyChanged(nameof(Model));

                //Piped value field
                List<string> qualities = ConcatenatedCombobox.UnpipeString(_station.StationOCQuality);
                _qualityCollection.Clear(); //Clear any possible values first
                foreach (ComboBoxItem cbox in StationOutcropQuality.cboxItems)
                {
                    if (qualities.Contains(cbox.itemValue) && !_qualityCollection.Contains(cbox))
                    {
                        _qualityCollection.Add(cbox);
                    }
                }
                OnPropertyChanged(nameof(QualityCollection));
            }
        }

        public async Task FillPickers()
        {
            //Bedrock pickers
            if (Preferences.ContainsKey(nameof(FieldUserInfoFWorkType))
                && Preferences.Get(nameof(FieldUserInfoFWorkType), "").ToString().Contains(ApplicationThemeBedrock))
            {
                _stationOutcropQuality = await FillAPicker(FieldStationOCQuality);
                OnPropertyChanged(nameof(StationOutcropQuality));
            }

            _stationType = await FillAPicker(FieldStationObsType);
            _stationSource = await FillAPicker(FieldStationObsSource);
            _stationPhysEnv = await FillAPicker(FieldStationPhysEnv);

            OnPropertyChanged(nameof(StationType));
            OnPropertyChanged(nameof(StationSource));
            OnPropertyChanged(nameof(StationPhysEnv));

        }

        /// <summary>
        /// Will fill the project type combobox
        /// </summary>
        private async Task<ComboBox> FillAPicker(string fieldName)
        {
            //Make sure to user default database rather then the prefered one. This one will always be there.
            return await da.GetComboboxListWithVocabAsync(TableStation, fieldName);

        }

        /// <summary>
        /// Will fill out missing fields for model. Default auto-calculated values
        /// </summary>
        private async Task SetModelAsync()
        {

            //Process concatenated pickers
            if (StationOutcropQuality.cboxDefaultItemIndex != -1 && QualityCollection != null && StationOutcropQuality.cboxItems.Count > 0)
            {
                Model.StationOCQuality = ConcatenatedCombobox.PipeValues(QualityCollection); //process list of values so they are concatenated.
            }

            //Process Air Photo and Traverse numbers
            FillAirPhotoTraverseNo();
 
        }

        /// <summary>
        /// TODO: Make sure this feature is still relevant. Not sure many people are using it.
        /// Will prefill air photo number and traverse number from the last entered values by user.
        /// </summary>
        private void FillAirPhotoTraverseNo()
        {
            ////Special case for air photo and traverse numbers, get the last numbers
            //string tableName = Dictionaries.TableStation;
            //string querySelectFrom = "SELECT * FROM " + tableName;
            //string queryOrder1 = " ORDER BY " + tableName + "." + Dictionaries.FieldStationVisitDate + " DESC";
            //string queryOrder2 = ", " + tableName + "." + Dictionaries.FieldStationVisitTime + " DESC";
            //string queryLimit = " LIMIT 1";
            //string finaleQuery = querySelectFrom + queryOrder1 + queryOrder2 + queryLimit;

            //List<object> stationTableRaw = accessData.ReadTable(StationModel.GetType(), finaleQuery);
            //IEnumerable<Station> stationFiltered = stationTableRaw.Cast<Station>(); //Cast to proper list type
            //if (stationFiltered.Count() != 0 || stationFiltered != null)
            //{
            //    foreach (Station sts in stationFiltered)
            //    {
            //        _stationTravNo = sts.StationTravNo.ToString();

            //        //Make check on date if newer, increment traverse no. if wanted by user
            //        if (localSetting.GetSettingValue(ApplicationLiterals.KeywordStationTraverseNo) == null ||
            //            (localSetting.GetSettingValue(ApplicationLiterals.KeywordStationTraverseNo) != null &&
            //            localSetting.GetBoolSettingValue(ApplicationLiterals.KeywordStationTraverseNo)))
            //        {
            //            string currentDate = DateTime.Now.ToShortDateString();
            //            DateTime lastStationDate = DateTime.Parse(sts.StationVisitDate);
            //            DateTime currentDateDT = DateTime.Parse(currentDate);
            //            if (lastStationDate != null && currentDateDT != null)
            //            {
            //                int dateComparisonResult = DateTime.Compare(lastStationDate, currentDateDT);
            //                if (lastStationDate != null && dateComparisonResult < 0)
            //                {
            //                    _stationTravNo = (sts.StationTravNo + 1).ToString();
            //                }
            //            }

            //        }




            //        _airno = sts.StationAirNo;
            //    }

            //}

            //RaisePropertyChanged("TraverseNo");
            //RaisePropertyChanged("AirPhoto");
        }

        /// <summary>
        /// Will reset model fields to default just like it's a new record
        /// </summary>
        /// <returns></returns>
        private async Task ResetModelAsync()
        {

            //Reset model
            if (Model.StationID == 0)
            {
                //Get current application version
                Model.LocationID = fieldLocation.LocationID;
                Model.StationAlias = await idCalculator.CalculateStationAliasAsync(DateTime.Now);
                Model.StationVisitDate = idCalculator.GetDate(); //Calculate new value
                Model.StationVisitTime = idCalculator.GetTime(); //Calculate new value
            }
            else if (Model.LocationID != null)
            {
                // if coming from field notes on a record edit that needs to be saved as a new record with stay/save
                Model.StationAlias = await idCalculator.CalculateStationAliasAsync(DateTime.Now);
                Model.StationVisitDate = idCalculator.GetDate(); //Calculate new value
                Model.StationVisitTime = idCalculator.GetTime(); //Calculate new value
            }

            Model.StationID = 0;

        }

        /// <summary>
        /// Will make a quick station record in station table, from a given xy position. 
        /// XY will be used to create a quick location first
        /// </summary>
        /// <param name="fieldLocation"></param>
        /// <returns></returns>
        public async Task<Station> QuickStation(int locationID)
        {
            //Spoof a location object with only an ID so SetModelAsync works well
            FieldLocation fl = new FieldLocation();
            fl.LocationID = locationID;
            fieldLocation = fl;

            //Fill out model and save new record
            await InitModel();
            Station quickStation = await da.SaveItemAsync(Model, false) as Station;
            quickStation.IsMapPageQuick = true;

            return quickStation;
        }

        /// <summary>
        /// Will initialize the model with needed calculated fields
        /// </summary>
        /// <returns></returns>
        public async Task InitModel()
        {
            if (Model != null && Model.StationID == 0 && fieldLocation != null)
            {
                //Get current application version
                Model.LocationID = fieldLocation.LocationID;
                Model.StationAlias = await idCalculator.CalculateStationAliasAsync(DateTime.Now);
                Model.StationVisitDate = idCalculator.GetDate(); //Calculate new value
                Model.StationVisitTime = idCalculator.GetTime(); //Calculate new value

                OnPropertyChanged(nameof(Model));

            }
        }

        #endregion

        #region EVENTS

        
        #endregion
    }
}
