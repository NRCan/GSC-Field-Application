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
using GSCFieldApp.Dictionaries;
using GSCFieldApp.Themes;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.Maui.ApplicationModel.Communication;
using System.Xml.Linq;

namespace GSCFieldApp.ViewModel
{

    [QueryProperty(nameof(FieldLocation), nameof(FieldLocation))]
    [QueryProperty(nameof(Metadata), nameof(Metadata))]
    [QueryProperty(nameof(Station), nameof(Station))]
    public partial class StationViewModel : ObservableObject
    {
        #region INIT

        DataAccess da = new DataAccess();
        ConcatenatedCombobox concat = new ConcatenatedCombobox(); //Use to concatenate values
        private Station _model = new Station();
        private DateTimeOffset _dateGeneric = DateTime.Now; //Default
        public DataIDCalculation idCalculator = new DataIDCalculation();
        private ComboBox _stationType = new ComboBox();
        private ComboBox _stationOutcropQuality = new ComboBox();
        private ComboBox _stationSource = new ComboBox();
        private ComboBox _stationPhysEnv = new ComboBox();

        //Concatenated
        private ComboBoxItem _selectedStationOutcropQuality = new ComboBoxItem();
        private ObservableCollection<ComboBoxItem> _qualityCollection = new ObservableCollection<ComboBoxItem>();

        //Themes
        private bool _bedrockVisibility = true; //Visibility for extra fields

        #endregion

        #region PROPERTIES

        [ObservableProperty]
        FieldLocation fieldLocation;

        [ObservableProperty]
        private Metadata metadata;

        [ObservableProperty]
        private Station _station;

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
                        _qualityCollection.Add(value);
                        _selectedStationOutcropQuality = value;
                        OnPropertyChanged(nameof(QualityCollection));
                    }


                }
                
            } 
        }
        public ObservableCollection<ComboBoxItem> QualityCollection { get { return _qualityCollection; } set { _qualityCollection = value; } }

        public bool BedrockVisibility { get { return _bedrockVisibility; } set { _bedrockVisibility = value; } }
        #endregion 

        public StationViewModel()
        {
            SetFieldVisibility(); //Will enable/disable some fields based on bedrock or surficial usage
        }

        #region RELAYS

        [RelayCommand]
        public async Task Back()
        {
            await Shell.Current.GoToAsync($"{nameof(MapPage)}");
        }

        /// <summary>
        /// Will delete a selected item in quality collection box.
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        async Task Delete(ComboBoxItem item)
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
            if (_station != null &&_station.StationAlias != string.Empty)
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

            //Exit
            //await Shell.Current.GoToAsync($"{nameof(MapPage)}");
            await Shell.Current.GoToAsync("..");

        }

        #endregion

        #region METHODS

        /// <summary>
        /// Will set visibility based on a bedrock or surficial field book
        /// </summary>
        public async Task SetFieldVisibility()
        {
            //Prefered theme should be saved on field book selected. Defaults to bedrock.
            string preferedTheme = Preferences.Get(nameof(DatabaseLiterals.FieldUserInfoPName), Dictionaries.ScienceLiterals.ApplicationThemeBedrock);
            if (preferedTheme == Dictionaries.ScienceLiterals.ApplicationThemeBedrock)
            {
                _bedrockVisibility = true;
            }
            else if (preferedTheme == Dictionaries.ScienceLiterals.ApplicationThemeSurficial)
            {
                _bedrockVisibility = false;
            }


            OnPropertyChanged(nameof(BedrockVisibility));
        }

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

                #region Pickers
                //Select values in pickers
                foreach (ComboBoxItem cbox in StationType.cboxItems)
                {
                    if (cbox.itemValue == _station.StationObsType)
                    {
                        StationType.cboxDefaultItemIndex = StationType.cboxItems.IndexOf(cbox);
                        break;
                    }
                }
                OnPropertyChanged(nameof(StationType));

                foreach (ComboBoxItem cbox in StationSource.cboxItems)
                {
                    if (cbox.itemValue == _station.StationObsSource)
                    {
                        StationSource.cboxDefaultItemIndex = StationSource.cboxItems.IndexOf(cbox);
                        break;
                    }
                }
                OnPropertyChanged(nameof(StationSource));

                foreach (ComboBoxItem cbox in StationPhysEnv.cboxItems)
                {
                    if (cbox.itemValue == _station.StationPhysEnv)
                    {
                        StationPhysEnv.cboxDefaultItemIndex = StationPhysEnv.cboxItems.IndexOf(cbox);
                        break;
                    }
                }
                OnPropertyChanged(nameof(StationPhysEnv));
                #endregion

                //Piped value field
                List<string> qualities = concat.UnpipeString(_station.StationOCQuality);
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

            _stationType = await FillAPicker(DatabaseLiterals.FieldStationObsType);
            _stationOutcropQuality = await FillAPicker(DatabaseLiterals.FieldStationOCQuality);
            _stationSource = await FillAPicker(DatabaseLiterals.FieldStationObsSource);
            _stationPhysEnv = await FillAPicker(DatabaseLiterals.FieldStationPhysEnv);

            OnPropertyChanged(nameof(StationType));
            OnPropertyChanged(nameof(StationOutcropQuality));
            OnPropertyChanged(nameof(StationSource));
            OnPropertyChanged(nameof(StationPhysEnv));

        }

        /// <summary>
        /// Will fill the project type combobox
        /// </summary>
        private async Task<ComboBox> FillAPicker(string fieldName)
        {
            //Make sure to user default database rather then the prefered one. This one will always be there.
            return await da.GetComboboxListWithVocabAsync(DatabaseLiterals.TableStation, fieldName);

        }

        /// <summary>
        /// Will fill out missing fields for model. Default auto-calculated values
        /// </summary>
        private async Task SetModelAsync()
        {
            //Make sure it's for a new field book
            if (Model.StationID == 0)
            {
                //Get current application version
                Model.LocationID = fieldLocation.LocationID;
                Model.StationAlias = await idCalculator.CalculateStationAliasAsync(DateTime.Now);
                Model.StationVisitDate = CalculateStationDate(); //Calculate new value
                Model.StationVisitTime = CalculateStationTime(); //Calculate new value
            }

            //Process pickers
            if (StationType.cboxDefaultItemIndex != -1)
            {
                Model.StationObsType = StationType.cboxItems[StationType.cboxDefaultItemIndex].itemValue;
            }
            if (StationOutcropQuality.cboxDefaultItemIndex != -1)
            {
                Model.StationOCQuality = concat.PipeValues(QualityCollection); //process list of values so they are concatenated.
            }
            if (StationSource.cboxDefaultItemIndex != -1)
            {
                Model.StationObsSource = StationSource.cboxItems[StationSource.cboxDefaultItemIndex].itemValue;
            }
            if (StationPhysEnv.cboxDefaultItemIndex != -1)
            {
                Model.StationPhysEnv = StationPhysEnv.cboxItems[StationPhysEnv.cboxDefaultItemIndex].itemValue;
            }

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

        #region EVENTS

        
        #endregion
    }
}
