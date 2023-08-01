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
        private Tuple<List<Themes.ComboBoxItem>, int> _stationType = Tuple.Create(new List<Themes.ComboBoxItem>(), -1);
        //private int _selectedStationType = -1;

        #endregion

        #region PROPERTIES

        [ObservableProperty]
        FieldLocation fieldLocation;

        [ObservableProperty]
        private Metadata metadata;

        public Station Model { get { return model; } set { model = value; } }
        public Tuple<List<Themes.ComboBoxItem>, int> StationType { get { return _stationType; } set { _stationType = value; } }
        //public int SelectedStationType { get { return _selectedStationType; } set { _selectedStationType = value; } }

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
                await FillStationType();
            }
            
        }

        /// <summary>
        /// Will fill the project type combobox
        /// </summary>
        private async Task FillStationType()
        {
            //Init.
            string fieldName = DatabaseLiterals.FieldStationObsType;

            //Make sure to user default database rather then the prefered one. This one will always be there.
            _stationType = await da.GetComboboxListWithVocabAsync(DatabaseLiterals.TableStation, fieldName, metadata.FieldworkType);

            //Update UI
            OnPropertyChanged("StationType");

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
                if (StationType.Item2 != -1)
                {
                    Model.StationObsType = StationType.Item1[StationType.Item2].itemValue;
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
    }
}
