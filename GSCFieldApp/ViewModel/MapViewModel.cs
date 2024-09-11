using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using GSCFieldApp.Views;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using GSCFieldApp.Models;
using GSCFieldApp.Services.DatabaseServices;
using System.Diagnostics;
using GSCFieldApp.Dictionaries;
using GSCFieldApp.Services;
using SQLite;
using CommunityToolkit.Maui.Core.Extensions;
using Mapsui.Layers;
using static GSCFieldApp.Dictionaries.DatabaseLiterals;

namespace GSCFieldApp.ViewModel
{
    public partial class MapViewModel: ObservableObject
    {
        #region INIT
        private DataIDCalculation idCalc = new DataIDCalculation();
        private DataAccess dataAccess = new DataAccess();

        private FieldLocation locationModel = new FieldLocation();
        private Metadata metadataModel = new Metadata(); 
        private Station stationModel = new Station();
        public Location sensorLocation { get; set; }  //This is coming from the view when new location event is triggered. 
        public Mapsui.Map mapViewFallback = new Mapsui.Map();
        private ObservableCollection<ILayer> _layerCollection = new ObservableCollection<ILayer>();
        private Collection<MapPageLayer> _customLayerCollection = new Collection<MapPageLayer>(); //Will be used to save user preferences and layers
        private string _gpsModeButtonSymbol = ApplicationLiterals.gpsModeGPS;

        private FieldThemes _fieldThemes = new FieldThemes();

        //Localization
        public LocalizationResourceManager LocalizationResourceManager
            => LocalizationResourceManager.Instance; // Will be used for in code dynamic local strings

        #endregion

        #region PROPERTIES
        public ObservableCollection<ILayer> layerCollection { get { return _layerCollection; } set { _layerCollection = value; } }
        private Collection<MapPageLayer> CustomLayerCollection { get { return _customLayerCollection; } set { _customLayerCollection = value; } }
        public string GPSModeButtonSymbol { get { return _gpsModeButtonSymbol; } set { _gpsModeButtonSymbol = value; } }

        #endregion

        public MapViewModel()
        {
            //Get main metadata record
            _ = GetMetadataAsync();

            //Detect new field book selection, uprgrade, edit, ...
            FieldBooksViewModel.newFieldBookSelected -= FieldBooksViewModel_newFieldBookSelectedAsync;
            FieldBooksViewModel.newFieldBookSelected += FieldBooksViewModel_newFieldBookSelectedAsync;

        }

        #region RELAY COMMANDS
        [RelayCommand]
        async Task AddStation()
        {
            if (sensorLocation != null)
            {
                int locId = await SetLocationModelAsync();

                if (locId != -1)
                {
                    //Navigate to station page and keep locationmodel for relationnal link
                    await Shell.Current.GoToAsync($"{nameof(StationPage)}/",
                        new Dictionary<string, object>
                        {
                            [nameof(FieldLocation)] = locationModel,
                            [nameof(Metadata)] = metadataModel,
                            [nameof(Station)] = null,
                        }
                    );
                }
                else
                {
                    await ShowMissingFieldBookMesasge();
                }

            }

        }

        [RelayCommand]
        async Task AddSample()
        {
            if (sensorLocation != null)
            {
                //Create a location record
                int locationID = await SetLocationModelAsync();

                if (locationID != -1)
                {
                    //Create a quick earth material record
                    EarthmatViewModel earthmatViewModel = new EarthmatViewModel();
                    Earthmaterial quickEM = await earthmatViewModel.QuickEarthmat(locationID);

                    //Navigate to station page and keep locationmodel for relationnal link
                    await Shell.Current.GoToAsync($"{nameof(SamplePage)}/",
                        new Dictionary<string, object>
                        {
                            [nameof(Sample)] = null,
                            [nameof(Earthmaterial)] = quickEM,
                        }
                    );
                }
                else
                {
                    await ShowMissingFieldBookMesasge();
                }
            }

        }

        [RelayCommand]
        async Task AddDocument()
        {
            if (sensorLocation != null)
            {
                //Create a location record
                int locationID = await SetLocationModelAsync();

                if (locationID != -1)
                {
                    //Create a quick earth material record
                    StationViewModel statViewModel = new StationViewModel();
                    Station quickStat = await statViewModel.QuickStation(locationID);

                    //Navigate to station page and keep locationmodel for relationnal link
                    await Shell.Current.GoToAsync($"{nameof(DocumentPage)}/",
                        new Dictionary<string, object>
                        {
                            [nameof(Station)] = quickStat,
                            [nameof(Document)] = null,
                        }
                    );
                }
                else
                {
                    await ShowMissingFieldBookMesasge();
                }

            }

        }

        [RelayCommand]
        async Task AddStructure()
        {
            if (sensorLocation != null)
            {
                //Create a location record
                int locationID = await SetLocationModelAsync();

                if (locationID != -1)
                {
                    //Special case: this command can either pop structure for bedrock fieldbook
                    // or pop paleoflow for surficial ones

                    //Create a quick earth material record
                    EarthmatViewModel emViewModel = new EarthmatViewModel();
                    Earthmaterial quickEM = await emViewModel.QuickEarthmat(locationID);

                    string preferedTheme = Preferences.Get(nameof(DatabaseLiterals.FieldUserInfoFWorkType), DatabaseLiterals.ApplicationThemeBedrock);
                    if (preferedTheme == ApplicationThemeBedrock)
                    {

                        //Navigate to structure page 
                        await Shell.Current.GoToAsync($"{nameof(StructurePage)}/",
                            new Dictionary<string, object>
                            {
                                [nameof(Structure)] = null,
                                [nameof(Earthmaterial)] = quickEM,
                            }
                        );
                    }
                    else
                    {
                        //Navigate to pflow page 
                        await Shell.Current.GoToAsync($"{nameof(PaleoflowPage)}/",
                            new Dictionary<string, object>
                            {
                                [nameof(PaleoflowPage)] = null,
                                [nameof(Earthmaterial)] = quickEM,
                            }
                        );
                    }
                }
                else
                {
                    await ShowMissingFieldBookMesasge();
                }
            }
        }

        [RelayCommand]
        async Task AddLocation()
        {
            if (sensorLocation != null)
            {
                //Create a location record
                int locationID = await SetLocationModelAsync();

                //Change entry type for manual
                locationModel.LocationEntryType = locationEntryTypeManual;

                //Navigate to structure page 
                if (locationID > 0)
                {
                    await Shell.Current.GoToAsync($"{nameof(LocationPage)}/",
                        new Dictionary<string, object>
                        {
                            [nameof(FieldLocation)] = locationModel,
                        }
                    );
                }
                else
                {
                    await ShowMissingFieldBookMesasge();
                }

            }

        }

        #endregion

        #region METHODS

        /// <summary>
        /// Will save the current layer settings into a JSON file inside the local folder
        /// </summary>
        /// <param name="inLayer">Optiontal layer to add before saving to json</param>
        public async void SaveLayerRendering(ILayer inLayer = null)
        {
            //If layer is passed as arg, update layer collection before saving.
            if (inLayer != null)
            {
                MapPageLayerBuilder mplb = new MapPageLayerBuilder();

                //Make sure layer isn't already in collection
                bool foundLayer = false;
                foreach (MapPageLayer mpls in _customLayerCollection)
                {
                    if (mpls.LayerName == inLayer.Name)
                    {
                        foundLayer = true; break;
                    }
                }

                if (!foundLayer)
                {
                    _customLayerCollection.Add(mplb.GetMapPageLayer(inLayer));
                }
                
            }

            //Build path to json file that will have same name as currently used field book
            string JSONPath = GetPreferedLayerJsonPath();

            await using FileStream fStream = File.Create(JSONPath);
            await JsonSerializer.SerializeAsync(fStream, _customLayerCollection);

        }

        /// <summary>
        /// Will get current field book layer settings from a jSON file inside the local folder
        /// </summary>
        public async Task<Collection<MapPageLayer>> GetLayerRendering()
        {
            Collection<MapPageLayer>? preferedLayers = null;

            //Build path to json file that will have same name as currently used field book
            string JSONPath = GetPreferedLayerJsonPath();

            //Make sure to remove existing file
            if (File.Exists(JSONPath))
            {
                using (FileStream openStream = File.OpenRead(JSONPath))
                {
                    // Enable support
                    var options = new JsonSerializerOptions { IncludeFields = true };

                    preferedLayers = await JsonSerializer.DeserializeAsync<Collection<MapPageLayer>>(openStream, options);

                    openStream.Close();
                }

            }

            return preferedLayers;

        }

        /// <summary>
        /// Will return the json file path for storing prefered layers
        /// </summary>
        /// <returns></returns>
        public string GetPreferedLayerJsonPath()
        { 
            return Path.Combine(Path.GetDirectoryName(dataAccess.PreferedDatabasePath), Path.GetFileNameWithoutExtension(dataAccess.PreferedDatabasePath)) + ".json";
        }

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
            //Quick check if a valid field book is selected
            if (dataAccess.PreferedDatabasePath != dataAccess.DatabaseFilePath)
            {
                locationModel = new FieldLocation();
                if (sensorLocation.Altitude.HasValue)
                {
                    locationModel.LocationElev = Math.Round((double)sensorLocation.Altitude, 0);
                }

                locationModel.LocationLat = Math.Round(sensorLocation.Latitude, 8);
                locationModel.LocationLong = Math.Round(sensorLocation.Longitude, 8);
                if (sensorLocation.Accuracy.HasValue)
                {
                    locationModel.LocationErrorMeasure = Math.Round((double)sensorLocation.Accuracy, 0);
                }
                if (sensorLocation.VerticalAccuracy.HasValue)
                {
                    locationModel.LocationElevationAccuracy = Math.Round(sensorLocation.VerticalAccuracy.Value, 0);
                }

                locationModel.LocationDatum = Dictionaries.DatabaseLiterals.KeywordEPSGDefault.ToString();
                locationModel.LocationTimestamp = idCalc.FormatFullDate(DateTime.Now);
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
                    locationModel.LocationGeometry = geoService.CreateByteGeometry(locationModel.LocationLong, locationModel.LocationLat);

                }

                //Other info
                locationModel.LocationEntryType = locationEntryTypeSatellite;

                //Update metadata if needed
                if (metadataModel.IsActive == 0)
                {
                    //Since a first location is being recorded, set to active.
                    metadataModel.IsActive = 1;
                    await dataAccess.SaveItemAsync(metadataModel, true);
                }

                //Save
                locationModel = await dataAccess.SaveItemAsync(locationModel, false) as FieldLocation;

                //Return ID
                return locationModel.LocationID;
            }
            else
            {
                return -1;
            }

        }

        /// <summary>
        /// Refresh the coordinates that will be shown on the map page as labels
        /// </summary>
        /// <param name="inLocation"></param>
        public void RefreshCoordinates(Location inLocation)
        {
            sensorLocation = inLocation;
            OnPropertyChanged(nameof(sensorLocation));
        }

        /// <summary>
        /// Will refresh the layer collection that is bind to 
        /// layer button
        /// We had to do this, since we need to get some layers out and
        /// resort them based on map rendering ordering
        /// </summary>
        /// <param name="layers"></param>
        public void RefreshLayerCollection(LayerCollection layers)
        {
            //To prevent layer being inserted in the wrong place, clear it before adding anything
            Collection<ILayer> refreshCollection = new Collection<ILayer>();

            //Clear before reloading
            RemoveAllLayers();

            int index = 0;

            //Add only wanted layers
            foreach (ILayer layer in layers)
            {
                //Remove unused layers
                if (!layer.Name.Contains("Drawables") && !layer.Name.Contains("Callouts") &&
                    !layer.Name.Contains("Layer") && !layer.Name.Contains("Pins")) 
                {
                    if (!_layerCollection.Contains(layer))
                    {
                        _layerCollection.Add(layer);

                        if (layer.Name != ApplicationLiterals.aliasStations && layer.Name != ApplicationLiterals.aliasOSM &&
                            layer.Name != ApplicationLiterals.aliasTraversePoint)
                        {
                            MapPageLayerBuilder mplb = new MapPageLayerBuilder();
                            _customLayerCollection.Add(mplb.GetMapPageLayer(layer, index));
                        }
                        
                    }
                }

                index++;
            }

            //Reverse ordering to mimic layer ordering on the map
            _layerCollection = new ObservableCollection<ILayer>(ReverseObsCollection(_layerCollection));

            if (_layerCollection.Count() > 0)
            {
                OnPropertyChanged(nameof(layerCollection));
            }
            

        }

        /// <summary>
        /// Will wipe all layers from layer collection
        /// </summary>
        public void RemoveAllLayers()
        {
            _layerCollection.Clear();
            _customLayerCollection.Clear();
            OnPropertyChanged(nameof(layerCollection));
            OnPropertyChanged(nameof(CustomLayerCollection));

        }

        /// <summary>
        /// Will reverse an observable collection
        /// Will convert to list, reverse and send result.
        /// </summary>
        /// <param name="inCollection"></param>
        /// <returns></returns>
        public List<ILayer> ReverseObsCollection(ObservableCollection<ILayer> inCollection)
        {
            List<ILayer> outCollection = inCollection.ToList();

            outCollection.Reverse();

            return outCollection;
        }

        /// <summary>
        /// Will force a set on the GPS mode button symbol
        /// When GPS is on it will display the gps symbol,
        /// when off it will show the hand symbol meaning
        /// user can do tap entries.
        /// </summary>
        /// <param name="isGPSOn"></param>
        public void SetGPSButtonSymbol(bool isGPSOn)
        {
            if (isGPSOn)
            {
                _gpsModeButtonSymbol = ApplicationLiterals.gpsModeGPS;
            }
            else 
            {
                _gpsModeButtonSymbol = ApplicationLiterals.gpsModeTap;
            }
            OnPropertyChanged(nameof(GPSModeButtonSymbol));
        }

        /// <summary>
        /// Will inform user to actual create or select a field book before taking any data
        /// </summary>
        public async Task ShowMissingFieldBookMesasge()
        {
            await Shell.Current.DisplayAlert(LocalizationResourceManager["GenericErrorTitle"].ToString(),
            LocalizationResourceManager["MapPageAlertFieldBookMessage"].ToString(),
            LocalizationResourceManager["GenericButtonOk"].ToString());
        }
        #endregion

        #region EVENTS

        /// <summary>
        /// Event triggered when user has changed field books.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void FieldBooksViewModel_newFieldBookSelectedAsync(object sender, bool hasChanged)
        {
            if (hasChanged)
            {
                RemoveAllLayers();
            }
            
        }

        #endregion
    }
}
