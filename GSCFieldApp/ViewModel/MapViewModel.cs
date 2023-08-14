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
using GSCFieldApp.Dictionaries;
using SQLite;
using CommunityToolkit.Maui.Core.Extensions;

namespace GSCFieldApp.ViewModel
{
    public partial class MapViewModel: ObservableObject
    {
        private DataIDCalculation idCalc = new DataIDCalculation();
        private DataAccess dataAccess = new DataAccess();

        private FieldLocation locationModel = new FieldLocation();
        private Metadata metadataModel = new Metadata(); 
        private Station stationModel = new Station();
        public Location sensorLocation { get; set; }  //This is coming from the view when new location event is triggered. 

        public MapViewModel()
        {
            //Get main metadata record
            _ = GetMetadataAsync();
        }

        #region RELAY COMMANDS
        [RelayCommand]
        async Task AddStation()
        {
            await SetLocationModelAsync();

            //Navigate to station page and keep locationmodel for relationnal link
            await Shell.Current.GoToAsync($"{nameof(StationPage)}",
                new Dictionary<string, object>
                {
                    [nameof(FieldLocation)] = locationModel,
                    [nameof(Metadata)] = metadataModel,
                }
            );
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Will retrieve first metadata record
        /// </summary>
        /// <returns></returns>
        private async Task GetMetadataAsync()
        {
            //Get metadata record
            SQLiteAsyncConnection currentConnection = dataAccess.GetConnectionFromPath(dataAccess.PreferedDatabasePath);
            List<Metadata> mets = await currentConnection.QueryAsync<Metadata>(string.Format("select * from {0} limit 1", DatabaseLiterals.TableMetadata));
            metadataModel = mets[0];
            await currentConnection.CloseAsync();

        }

        /// <summary>
        /// Will save the location model within prefered database
        /// </summary>
        /// <returns></returns>
        public async Task<int> SetLocationModelAsync()
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
            locationModel.LocationDatum = Dictionaries.DatabaseLiterals.KeywordEPSGDefault.ToString();

            //Foreign key
            if (metadataModel.MetaID > 0)
            {
                locationModel.MetaID = metadataModel.MetaID; 
            }
            else
            {
                locationModel.MetaID = 1; 
            }

            locationModel.LocationAlias = await idCalc.CalculateLocationAliasAsync("", metadataModel.UserCode); //Calculate new value

            //Fill in the feature location
            if (!locationModel.LocationLong.IsZeroOrNaN() && !locationModel.LocationLat.IsZeroOrNaN())
            {
                GeopackageService geoService = new GeopackageService();
                locationModel.LocationGeometry = geoService.GetGeometry(locationModel.LocationLong, locationModel.LocationLat);

            }

            //Save
            locationModel = await dataAccess.SaveItemAsync(locationModel, false) as FieldLocation;

            //Return ID
            return locationModel.LocationID;

        }

        public void RefreshCoordinates(Location inLocation)
        {
            sensorLocation = inLocation;
            OnPropertyChanged(nameof(sensorLocation));
        }

        #endregion
    }
}
