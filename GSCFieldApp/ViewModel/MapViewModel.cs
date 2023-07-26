using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSCFieldApp.Views;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using GSCFieldApp.Models;
using GSCFieldApp.Services.DatabaseServices;
using System.Diagnostics;

namespace GSCFieldApp.ViewModel
{
    public partial class MapViewModel: ObservableObject
    {
        private DataIDCalculation idCalc = new DataIDCalculation();
        private DataAccess dataAccess = new DataAccess();

        private FieldLocation locationModel = new FieldLocation();
        private Station stationModel = new Station();
        public Location sensorLocation { get; set; }  //This is coming from the view when new location event is triggered. 

        public MapViewModel()
        {

        }

        #region RELAY COMMANDS
        [RelayCommand]
        async Task AddStation()
        {
            await SetLocationModelAsync();
            //await Shell.Current.GoToAsync($"{nameof(StationPage)}", 
            //    new Dictionary<string, object> 
            //    {
            //        [nameof(FieldLocation)] = locationModel
            //    }
            //);
        }
        #endregion

        #region METHODS


        /// <summary>
        /// Will save the location model within prefered database
        /// </summary>
        /// <returns></returns>
        public async Task SetLocationModelAsync()
        {
            if (sensorLocation.Altitude.HasValue)
            {
                locationModel.LocationElev = (double)sensorLocation.Altitude;
            }

            locationModel.LocationLat = sensorLocation.Latitude;
            locationModel.LocationLong = sensorLocation.Longitude;
            if (sensorLocation.Accuracy.HasValue)
            {
                locationModel.LocationErrorMeasure = (double)sensorLocation.Accuracy;
            }
            //locationModel.LocationElevMethod = vocabElevmethodGPS,
            //locationModel.LocationEntryType = sensorLocation.po.PositionSource.ToString(),
            //locationModel.LocationErrorMeasureType = sensorLocation.,
            locationModel.LocationElevationAccuracy = sensorLocation.VerticalAccuracy;
            locationModel.LocationDatum = Dictionaries.DatabaseLiterals.KeywordEPSGDefault;

            locationModel.LocationAlias = await idCalc.CalculateLocationAliasAsync(); //Calculate new value
            locationModel.MetaID = 1; //Foreign key

            //Save location model
            locationModel = await dataAccess.SaveItemAsync(locationModel, false) as FieldLocation;

        }

        #endregion
    }
}
