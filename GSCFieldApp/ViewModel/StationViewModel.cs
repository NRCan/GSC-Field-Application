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

namespace GSCFieldApp.ViewModel
{

    [QueryProperty(nameof(FieldLocation), nameof(FieldLocation))]
    [QueryProperty(nameof(Metadata), nameof(Metadata))]
    public partial class StationViewModel : ObservableObject
    {
        #region INIT

        DataAccess da = new DataAccess();
        private Station model = new Station();
        private DateTimeOffset _dateGeneric = DateTime.Now; //Default
        public DataIDCalculation idCalculator = new DataIDCalculation();
        private ComboBox _stationType = new ComboBox();
        private ComboBox _stationOutcropQuality = new ComboBox();
        private ComboBox _stationSource = new ComboBox();
        private ComboBox _stationPhysEnv = new ComboBox();

        //Concatenated
        private ComboBoxItem _selectedStationOutcropQuality = new ComboBoxItem();
        private ComboBoxItem _selectedQualityCollection = new ComboBoxItem();
        private ObservableCollection<ComboBoxItem> _qualityCollection = new ObservableCollection<ComboBoxItem>();

        #endregion

        #region PROPERTIES

        [ObservableProperty]
        FieldLocation fieldLocation;

        [ObservableProperty]
        private Metadata metadata;

        public Station Model { get { return model; } set { model = value; } }
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
                        OnPropertyChanged("QualityCollection");
                    }


                }
                
            } 
        }
        public ComboBoxItem SelectedQualityCollection { get { return _selectedQualityCollection; } set { _selectedQualityCollection = value; } }
        public ObservableCollection<ComboBoxItem> QualityCollection { get { return _qualityCollection; } set { _qualityCollection = value; } }
        #endregion

        public StationViewModel()
        {

        }

        #region RELAYS
        [RelayCommand]
        async Task Save()
        {

            //Validate if new entry or update
            if (model.StationID > 0)
            {
                await da.SaveItemAsync(Model, true);
            }
            else
            {
                //Fill out missing values in model
                await SetModelAsync();

                //Insert new record
                await da.SaveItemAsync(Model, false);
            }

            //Close to be sure
            await da.CloseConnectionAsync();

            //Exit
            await Shell.Current.GoToAsync($"{nameof(MapPage)}");

        }

        #endregion

        #region METHODS

        public async Task FillPickers()
        {
            if (metadata != null)
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
            
        }

        /// <summary>
        /// Will fill the project type combobox
        /// </summary>
        private async Task<ComboBox> FillAPicker(string fieldName)
        {
            //Make sure to user default database rather then the prefered one. This one will always be there.
            return await da.GetComboboxListWithVocabAsync(DatabaseLiterals.TableStation, fieldName, metadata.FieldworkType);

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

                //Process pickers
                if (StationType.cboxDefaultItemIndex != -1)
                {
                    Model.StationObsType = StationType.cboxItems[StationType.cboxDefaultItemIndex].itemValue;
                }
                if (StationOutcropQuality.cboxDefaultItemIndex != -1)
                {
                    Model.StationOCQuality = StationOutcropQuality.cboxItems[StationOutcropQuality.cboxDefaultItemIndex].itemValue;
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
