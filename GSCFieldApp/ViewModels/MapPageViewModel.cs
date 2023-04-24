using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml.Controls;
using Windows.Storage;
using Windows.UI.Core;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Data;
using GSCFieldApp.Services.DatabaseServices;
using GSCFieldApp.Models;
using GSCFieldApp.Dictionaries;
using Template10.Mvvm;
using Template10.Controls;
using Windows.ApplicationModel.Resources;
using Windows.System;
using Windows.UI.Xaml.Input;
using System.Globalization;
using Symbol = Windows.UI.Xaml.Controls.Symbol;
using Newtonsoft.Json;
using SQLite;
using GSCFieldApp.Services;

//Added by jamel
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using ProjNet.Converters.WellKnownText;
using ProjNet;
using GeoAPI.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;
using System.Diagnostics;

namespace GSCFieldApp.ViewModels
{

    public class MapPageViewModel : ViewModelBase
    {
        #region INIT

        //Map
        public MapView currentMapView { get; set; }
        public Map esriMap;
        public double viewRotation = 0; //Default
        private bool _noMapsWatermark = false;
        private double mapScale = ApplicationLiterals.defaultMapScale;

        //Layers
        public ArcGISTiledLayer _basemapLayer;
        public ArcGISTiledLayer _bLayer;
        public string filepathname;
        public Visibility canDeleteLayer = Visibility.Visible; //Default
        public ArcGISTiledLayer defaultLayer;
        private ObservableCollection<MapPageLayers> _filenameValues = new ObservableCollection<MapPageLayers>();
        private object _selectedLayer;
        public object selectedStationID = string.Empty; //Will be used to show report page on user identified station.
        public object selectedStationDate = string.Empty;  //Will be used to show report page on user identified station.


        //Model and strings
        private readonly DataAccess accessData = new DataAccess();
        private readonly DataLocalSettings localSettings = new DataLocalSettings();
        public FieldLocation locationModel = new FieldLocation();

        //Other
        public bool addDataDialogPopedUp = false; //Will be used to stop pop-up launching everytime user navigates to map page.
        public ResourceLoader local = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
        readonly DataLocalSettings localSetting = new DataLocalSettings();
        public DataIDCalculation idCalculator = new DataIDCalculation();

        //Quick buttons
        private bool _mapPageQuickSampleEnable = true;
        private bool _mapPageQuickPhotoEnable = true;
        private bool _mapPageQuickMeasurementEnable = true;
        public string clickedQuickButton = string.Empty; //Will be used to track what user wants to tap option

        //Progress ring
        private bool _progressRingActive = false;
        private bool _progressRingVisibility = false;

        //GPS 
        public Geolocator _geolocator = null;
        public Geoposition _currentMSGeoposition;
        public bool userHasTurnedGPSOff = false;
        public double _currentLongitude = 0.0;
        public double _currentLatitude = 0.0;
        public double _currentAltitude = 0.0;
        public double _currentEasting = 0.0;
        public double _currentNorthing = 0.0;
        public double _currentAccuracy = 0.0;
        public string _currentProjection = string.Empty; //Added by jamel to get projection info
        public bool initializingGPS = false;
        public bool _mapRingLabelAcquiringGPSVisibility = false;
        public Symbol _GPSModeSymbol = Symbol.Target;

        //Map Graphics
        private readonly SimpleMarkerSymbol _posSym = new SimpleMarkerSymbol();
        public GraphicsOverlay _OverlayStation;
        public GraphicsOverlay _OverlayStationLabel;
        public GraphicsOverlay _OverlayCurrentPosition;
        public GraphicsOverlay _OverlayStructure;
        public Dictionary<string, List<GraphicsOverlay>> _overlayContainerOther; //Will act as a "layer" container but for graphics, just like esriMap.Basemap.BaseLayers object
        private System.Drawing.Color _accuracyColor = new System.Drawing.Color();
        private SimpleFillSymbol _accSym = new SimpleFillSymbol();
        private SimpleLineSymbol _accLineSym = new SimpleLineSymbol();
        //private Graphic _accGraphic = null;
        private MapPoint _centerPoint = new MapPoint(0, 0, 0.0, SpatialReferences.Wgs84);
        private MapPoint _projectedCenterPoint = null;
        //private Graphic _posGraphic = null;
        public bool pauseGraphicRefresh = false;


        //Delegates and events
        public static event EventHandler newDataLoaded; //This event is triggered when a new data has been loaded

        //Constants
        public string attributeID = "ID";
        public string attributeIDPosition = "Position";
        public string attributeIDAccuracy = "PositionAccuracy";

        //Testing
        private bool initMap = false;

        public MapPageViewModel()
        {

            //Init
            lastTakenLocation = new Tuple<double, double>(0, 0);

            _OverlayStation = new GraphicsOverlay();
            _OverlayStationLabel = new GraphicsOverlay();
            _overlayContainerOther = new Dictionary<string, List<GraphicsOverlay>>();
            _OverlayStructure = new GraphicsOverlay();
            //_OverlayCurrentPosition = new GraphicsOverlay();

            CreatePositionGraphic();
            //SetAccuracyGraphic();

            //Detect addition of any new layers
            newDataLoaded += ShellViewModel_newDataLoaded;
            SettingsPageViewModel.settingDeleteAllLayers += SettingsPageViewModel_deleteAllLayers;
            FieldBooksPageViewModel.deleteAllLayers += SettingsPageViewModel_deleteAllLayers;

            //Detect new field book selection, uprgrade, edit, ...
            FieldBooksPageViewModel.newFieldBookSelected -= FieldBooksPageViewModel_newFieldBookSelectedAsync;
            FieldBooksPageViewModel.newFieldBookSelected += FieldBooksPageViewModel_newFieldBookSelectedAsync;

            //Detect other setting events
            SettingsPartViewModel.settingUseStructureSymbols += SettingsPartViewModel_settingUseStructureSymbols;

            //Detect location edits
            LocationViewModel.LocationUpdateEventHandler += LocationViewModel_LocationUpdateEventHandler;

            //Set some configs
            SetQuickButtonEnable();

            //Fill vocab 
            FillLocationVocab();

        }

        private void EsriMap_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!initMap)
            {
                initMap = true;
            }
            
        }

        #endregion

        #region PROPERTIES

        public Dictionary<string, Dictionary<string, string>> _layerRendering { get; set; }
        //public Dictionary<string, Tuple<string, bool, double>> _layerRenderingConfiguration { get; set; } //Will be used to show layer in proper order, visibility and opacity based on previous setting on app opening.

        public Symbol GPSModeSymbol { get { return _GPSModeSymbol; } set { _GPSModeSymbol = value; } }

        public bool NoMapsWatermark { get { return _noMapsWatermark; } set { _noMapsWatermark = value; } }

        public ObservableCollection<MapPageLayers> FilenameValues { get { return _filenameValues; } set { _filenameValues = value; } }
        public object SelectedLayer
        {
            get { return _selectedLayer; }
            set
            {
                if (value != null)
                {
                    _selectedLayer = value;
                }

            }
        }

        public Geoposition CurrentMSGeoposition { get { return _currentMSGeoposition; } set { _currentMSGeoposition = value; } }

        public double CurrentLongitude { get { return _currentLongitude; } set { _currentLongitude = value; } }
        public double CurrentLatitude { get { return _currentLatitude; } set { _currentLatitude = value; } }
        public double CurrentAltitude { get { return _currentAltitude; } set { _currentAltitude = value; } }
        public double CurrentEasting { get { return _currentEasting; } set { _currentEasting = value; } }
        public double CurrentNorthing { get { return _currentNorthing; } set { _currentNorthing = value; } }
        public double CurrentAccuracy { get { return _currentAccuracy; } set { _currentAccuracy = value; } }
        public string CurrentProjection { get { return _currentProjection; } set { _currentProjection = value; } } //Added by Jamel
        public Tuple<double, double> lastTakenLocation { get; set; }
        public bool MapRingActive
        {
            get { return _progressRingActive; }
            set { _progressRingActive = value; }
        }
        public bool MapRingVisibility
        {
            get { return _progressRingVisibility; }
            set { _progressRingVisibility = value; }
        }

        public bool MapRingLabelAcquiringGPSVisibility
        {
            get { return _mapRingLabelAcquiringGPSVisibility; }
            set { _mapRingLabelAcquiringGPSVisibility = value; }
        }
        //public bool GPSSignalReceptionVisibility { get { return _GPSSignalReceptionVisibility; } set { _GPSSignalReceptionVisibility = value; } }

        public bool MapPageQuickMeasurementEnable { get { return _mapPageQuickMeasurementEnable; } set { _mapPageQuickMeasurementEnable = value; } }
        public bool MapPageQuickPhotoEnable { get { return _mapPageQuickPhotoEnable; } set { _mapPageQuickPhotoEnable = value; } }
        public bool MapPageQuickSampleEnable { get { return _mapPageQuickSampleEnable; } set { _mapPageQuickSampleEnable = value; } }

        //Dictionary values
        public string vocabEntryTypeTap { get; set; }
        public string vocabEntryTypeGPS { get; set; }
        public string vocabElevmethodGPS { get; set; }
        public string vocabErrorMeasureTypeMeter { get; set; }
        public string vocabEntryTypeManual { get; set; }

        #endregion

        #region MAP INTERACTION

        /// <summary>
        /// Will set layers, GPS and navigate to current location
        /// </summary>
        /// <param name="inMapView"></param>
        /// <returns></returns>
        public async Task SetMapView(MapView inMapView)
        {

            //PART 1. Load Map
            // Create variable for use elsewhere in this class, maybe better way
            if (currentMapView == null)
            {
                currentMapView = inMapView;
            }


            //Create new map if ncessary
            if (esriMap == null)
            {

                #region Add layers
                Task loadAll = AddAllLayers();
                await loadAll;

                #endregion

            }

            // PART 2. Deal with GPS
            // spw2017
            if (_currentMSGeoposition == null)
            {

                Task setGPSTask = SetGPS();
                await setGPSTask;
            }
            //Set some configs
            SetQuickButtonEnable();

        }


        /// <summary>
        /// Will initialized the GPS 
        /// </summary>
        /// <returns></returns>
        public async Task SetGPS()
        {
            //// Location platform is attempting to acquire a fix.
            //ResetLocationGraphic();

            var accessStatus = await Geolocator.RequestAccessAsync();

            switch (accessStatus)
            {
                case GeolocationAccessStatus.Allowed:

                    currentMapView.Tapped -= myMapView_AddByTap;

                    // If DesiredAccuracy or DesiredAccuracyInMeters are not set (or value is 0), DesiredAccuracy.Default is used.
                    _geolocator = new Geolocator { ReportInterval = 750 };

                    // Subscribe to the StatusChanged event to get updates of location status changes.
                    _geolocator.PositionChanged -= OnPositionChanged;
                    _geolocator.PositionChanged += OnPositionChanged;
                    _geolocator.StatusChanged -= Geolocal_StatusChangedAsync;
                    _geolocator.StatusChanged += Geolocal_StatusChangedAsync;

                    _geolocator.DesiredAccuracy = Windows.Devices.Geolocation.PositionAccuracy.Default;
                    //_geolocator.DesiredAccuracy = Windows.Devices.Geolocation.PositionAccuracy.High;
                       
                    break;

                case GeolocationAccessStatus.Denied:
                    //ResetLocationGraphic();
                    await NoLocationRoutine();
                    break;
                case GeolocationAccessStatus.Unspecified:
                    //ResetLocationGraphic();
                    await NoLocationRoutine();
                    break;
            }


        }

        public async void Geolocal_StatusChangedAsync(Geolocator sender, Windows.Devices.Geolocation.StatusChangedEventArgs args)
        {


            switch (args.Status)
            {
                case PositionStatus.Ready:

                    StopLocationRing();

                    break;

                case PositionStatus.Initializing:

                    //await Task.Delay(500);

                    if (FilenameValues.Count != 0) //This will prevent pop-up with new field book
                    {
                        StartLocationRing();
                    }

                    break;

                case PositionStatus.NoData:
                    //// Location platform could not obtain location data.

                    if (!_progressRingActive)
                    {
                        StartLocationRing();
                        ResetLocationGraphic();
                    }
                    
                    
                    //await Task.Delay(3000); //Let enough time to pass so GPS actually gets a proper fix
                    //await NoLocationFlightMode();

                    //try
                    //{
                    //    await SetGPS();
                    //}
                    //catch (Exception)
                    //{

                    //}


                    break;

                case PositionStatus.Disabled:
                    await Task.Delay(500);
                    // The permission to access location data is denied by the user or other policies.
                    ResetLocationGraphic();
                    userHasTurnedGPSOff = true;
                    SetGPSModeIcon(Symbol.TouchPointer);

                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        await NoLocationRoutine();
                        StopLocationRing();
                    }).AsTask();

                    //StopLocationRing();

                    break;

                case PositionStatus.NotInitialized:
                    StartLocationRing();
                    await Task.Delay(500);
                    // The location platform is not initialized. This indicates that the application
                    //// has not made a request for location data.

                    //Clear current graphics
                    //ResetLocationGraphic();
                    try
                    {
                        await SetGPS();
                    }
                    catch (Exception)
                    {

                    }


                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        ContentDialog notInitLocationDialog = new ContentDialog()
                        {
                            Title = local.GetString("MapPageDialogLocationTitle"),
                            Content = local.GetString("MapPageDialogLocationDidNotInit"),
                            CloseButtonText = local.GetString("GenericDialog_ButtonOK")
                        };
                        notInitLocationDialog.Style = (Style)Application.Current.Resources["WarningDialog"];
                        await Services.ContentDialogMaker.CreateContentDialogAsync(notInitLocationDialog, true);
                        StopLocationRing();
                    }).AsTask();
                    break;

                case PositionStatus.NotAvailable:
                    await Task.Delay(500);
                    //// The location platform is not available on this version of the OS.
                    //Clear current graphics
                    ResetLocationGraphic();
                    userHasTurnedGPSOff = true;
                    SetGPSModeIcon(Symbol.TouchPointer);

                    try
                    {
                        await SetGPS();
                    }
                    catch (Exception)
                    {

                        throw;
                    }



                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        ContentDialog NALocationDialog = new ContentDialog()
                        {
                            Title = local.GetString("MapPageDialogLocationTitle"),
                            Content = local.GetString("MapPageDialogLocationNAOnOS"),
                            CloseButtonText = local.GetString("GenericDialog_ButtonOK")
                        };
                        NALocationDialog.Style = (Style)Application.Current.Resources["WarningDialog"];
                        await Services.ContentDialogMaker.CreateContentDialogAsync(NALocationDialog, true);
                        StopLocationRing();
                    }).AsTask();
                    break;

                default:
                    await Task.Delay(500);
                    ResetLocationGraphic();
                    userHasTurnedGPSOff = true;
                    SetGPSModeIcon(Symbol.TouchPointer);
                    try
                    {
                        await SetGPS();
                    }
                    catch (Exception)
                    {

                        throw;
                    }


                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        ContentDialog defaultEventLocationDialog = new ContentDialog()
                        {
                            Title = local.GetString("MapPageDialogLocationTitle"),
                            Content = local.GetString("MapPageDialogLocationUnknownError"),
                            CloseButtonText = local.GetString("GenericDialog_ButtonOK")
                        };
                        defaultEventLocationDialog.Style = (Style)Application.Current.Resources["WarningDialog"];
                        await Services.ContentDialogMaker.CreateContentDialogAsync(defaultEventLocationDialog, true);
                        StopLocationRing();
                    }).AsTask();

                    break;
            }

        }

        /// <summary>
        /// Will make a quick verification whether user has still the right to get a location
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ValidateGeolocationAccess()
        {

            var accessStatus = await Geolocator.RequestAccessAsync();

            bool canAccess = true;

            switch (accessStatus)
            {
                case GeolocationAccessStatus.Allowed:
                    currentMapView.Tapped -= myMapView_AddByTap;
                    canAccess = true;
                    break;

                case GeolocationAccessStatus.Denied:
                    canAccess = false;

                    //Force call on UI thread, else it could crash the app if async call is made another thread.
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        ContentDialog tapModeDialog = new ContentDialog()
                        {
                            Title = local.GetString("MapPageDialogLocationTitle"),
                            Content = local.GetString("MapPageDialogLocationRestricted"),
                            CloseButtonText = local.GetString("GenericDialog_ButtonOK")
                        };
                        tapModeDialog.Style = (Style)Application.Current.Resources["WarningDialog"];
                        await Services.ContentDialogMaker.CreateContentDialogAsync(tapModeDialog, true);


                    }).AsTask();

                    break;

                case GeolocationAccessStatus.Unspecified:
                    canAccess = false;

                    //Force call on UI thread, else it could crash the app if async call is made another thread.
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        ContentDialog tapModeDialog = new ContentDialog()
                        {
                            Title = local.GetString("MapPageDialogLocationTitle"),
                            Content = local.GetString("MapPageDialogLocationRestricted"),
                            CloseButtonText = local.GetString("GenericDialog_ButtonOK")
                        };
                        tapModeDialog.Style = (Style)Application.Current.Resources["WarningDialog"];
                        await Services.ContentDialogMaker.CreateContentDialogAsync(tapModeDialog, true);


                    }).AsTask();

                    break;
            }

            return canAccess;
        }

        /// <summary>
        /// Will display database stations and display them on the map
        /// </summary>
        /// <param name="inMapView"></param>
        /// <param name="inLocationTableRows"></param>
        public void DisplayPointAndLabelsAsync(MapView inMapView, bool forceRefresh = false)
        {

            #region Load from Default Database 

            //Build a list of already loaded stations id on the map
            Dictionary<string, Graphic> loadedGraphicList = new Dictionary<string, Graphic>();
            if (forceRefresh)
            {
                foreach (Graphic gr in _OverlayStation.Graphics)
                {
                    loadedGraphicList[gr.Attributes[Dictionaries.DatabaseLiterals.FieldLocationID].ToString()] = gr;
                }

            }

            // If at least one location exists display it on the map
            if (!forceRefresh && inMapView != null)
            {

                //Reset main db station overlay
                inMapView.GraphicsOverlays.Remove(_OverlayStation);
                inMapView.GraphicsOverlays.Remove(_OverlayStationLabel);
                inMapView.GraphicsOverlays.Remove(_OverlayStructure);
                inMapView.GraphicsOverlays.Add(_OverlayStation);
                inMapView.GraphicsOverlays.Add(_OverlayStationLabel);
                inMapView.GraphicsOverlays.Add(_OverlayStructure);

                // Load
                LoadFromGivenDBTest(loadedGraphicList, true);
                //inMapView.UpdateLayout();
            }
            else
            {
                //Refresh graphics since the last location seems to have been deleted by the user
                if (currentMapView != null && currentMapView.GraphicsOverlays != null && currentMapView.GraphicsOverlays.Count > 0)
                {
                    _OverlayStation.Graphics.Clear();
                    _OverlayStationLabel.Graphics.Clear();
                    _OverlayStructure.Graphics.Clear();

                    currentMapView.GraphicsOverlays.Remove(_OverlayStation);
                    currentMapView.GraphicsOverlays.Remove(_OverlayStationLabel);
                    currentMapView.GraphicsOverlays.Remove(_OverlayStructure);

                    RaisePropertyChanged("_OverlayStation");
                    RaisePropertyChanged("_OverlayStationLabel");
                    RaisePropertyChanged("_OverlayStructure");
                }

            }

            #endregion

        }

        /// <summary>
        /// From a given database connection , will add point and labels of stations on the map page
        /// Information will be loaded if the schema is the same between all databases and the version with which
        /// this application version has been coded.
        /// </summary>
        /// <param name="inLocationTableRows"></param>
        public void LoadFromGivenDB(List<object> inLocationTableRows, SQLiteConnection dbConnection, Dictionary<string, Graphic> graphicList, bool isDefaultDB)
        {
            //PictureMarkerSymbol pointSym = new PictureMarkerSymbol(new Uri("ms-appx:///Assets/IC809393.png"));

            //Uri planarPath = new Uri("ms-appx:///Assets/Images/theme-light/struc_planar.png");
            //Uri linearPath = new Uri("ms-appx:///Assets/Images/theme-light/struc_linear.png");

            //PictureMarkerSymbol StrucPlaneSym = new PictureMarkerSymbol(planarPath);
            //PictureMarkerSymbol StrucLinearSym = new PictureMarkerSymbol(linearPath);

            ////Choose proper overlay
            //GraphicsOverlay pointOverlay = new GraphicsOverlay();
            //GraphicsOverlay pointLabelOverlay = new GraphicsOverlay();
            //GraphicsOverlay structureOverlay = new GraphicsOverlay();

            ////Set some rendering defaults
            //Renderer graphRenderer = new SimpleRenderer
            //{
            //    RotationType = RotationType.Geographic
            //};
            //pointOverlay.Renderer = graphRenderer;

            //if (isDefaultDB)
            //{
            //    pointOverlay = _OverlayStation;
            //    pointLabelOverlay = _OverlayStationLabel;
            //    structureOverlay = _OverlayStructure;
            //}
            //else
            //{
            //    string dbFileName = Path.GetFileName(dbConnection.DatabasePath);
            //    if (_overlayContainerOther.ContainsKey(dbFileName))
            //    {
            //        pointOverlay = _overlayContainerOther[dbFileName][0];
            //        pointLabelOverlay = _overlayContainerOther[dbFileName][1];
            //        structureOverlay = _overlayContainerOther[dbFileName][2];
            //    }

            //}

            //#region ADD
            //// Get latitude, longitude and station id and add to graphics overlay
            //foreach (object lcs in inLocationTableRows)
            //{
            //    //Variables
            //    bool stationGraphicExists = false;
            //    bool structureGraphicExists = false;

            //    #region POINT SYMBOL
            //    Models.FieldLocation currentLocation = lcs as Models.FieldLocation;
            //    var ptLatitude = currentLocation.LocationLat;
            //    var ptLongitude = currentLocation.LocationLong;
            //    var ptLocId = currentLocation.LocationID;

            //    //Get related station
            //    List<object> stationTableRows = new List<object>();
            //    Station stations = new Station();
            //    string stationsSelectionQuery = "Select * from " + DatabaseLiterals.TableStation + " where " + DatabaseLiterals.FieldLocationID + " = '" + currentLocation.LocationID + "'";
            //    stationTableRows = accessData.ReadTableFromDBConnectionWithoutClosingConnection(stations.GetType(), stationsSelectionQuery, dbConnection);

            //    // should only be one station returned, this approach doesn't allow for multiple stations
            //    var ptStationId = string.Empty;
            //    var ptStationDate = string.Empty;
            //    var ptStationTime = string.Empty;
            //    var ptStationType = string.Empty;
            //    string ptStationLocationID = string.Empty;
            //    double ptStationLocationLat;
            //    double ptStationLocationLong;
            //    string ptStationLocationEPSG = string.Empty;
            //    foreach (object scs in stationTableRows)
            //    {
            //        Models.Station currentStation = scs as Models.Station;
            //        ptStationId = currentStation.StationAlias;
            //        ptStationDate = currentStation.StationVisitDate;
            //        ptStationTime = currentStation.StationVisitTime;
            //        ptStationType = currentStation.StationObsType;
            //        ptStationLocationID = currentLocation.LocationID;
            //        ptStationLocationLat = currentLocation.LocationLat;
            //        ptStationLocationLong = currentLocation.LocationLong;
            //        ptStationLocationEPSG = currentLocation.LocationDatum;
            //    }

            //    //Find if station was already loaded
            //    if (graphicList.ContainsKey(currentLocation.LocationID))
            //    {
            //        stationGraphicExists = true;
            //        graphicList.Remove(currentLocation.LocationID);
            //    }

            //    //Add new graphic station and it's related label if needed
            //    if (!stationGraphicExists && ptStationId != null && ptStationId != string.Empty)
            //    {
            //        //Tracking available offset placement 
            //        List<int> placementPool = Enumerable.Range(1, 8).ToList();

            //        #region MAIN POINT
            //        //Create Map Point for graphic
            //        MapPoint geoPoint = new MapPoint(ptLongitude, ptLatitude, SpatialReferences.Wgs84);

            //        //Get if datum transformation is needed
            //        int.TryParse(ptStationLocationEPSG, out int epsg);

            //        if (epsg != 0 && epsg != 4326)
            //        {
            //            DatumTransformation datumTransfo = null;
            //            SpatialReference outSR = null;

            //            if ((epsg > 26900 && epsg < 27000))
            //            {
            //                outSR = SpatialReference.Create(4326);
            //                datumTransfo = TransformationCatalog.GetTransformation(outSR, SpatialReferences.Wgs84);
            //            }


            //            MapPoint proPoint = new MapPoint(ptLongitude, ptLatitude, outSR);

            //            //Validate if transformation is needed.
            //            if (datumTransfo != null)
            //            {
            //                //Replace geopoint
            //                geoPoint = (MapPoint)Esri.ArcGISRuntime.Geometry.GeometryEngine.Project(proPoint, SpatialReferences.Wgs84, datumTransfo);
            //            }

            //        }

            //        var StationGraphic = new Graphic(geoPoint, pointSym);
            //        StationGraphic.Attributes.Add("Id", ptStationId.ToString());
            //        StationGraphic.Attributes.Add("Date", ptStationDate.ToString());
            //        StationGraphic.Attributes.Add("Time", ptStationTime.ToString());
            //        StationGraphic.Attributes.Add("tableType", DatabaseLiterals.TableStation);
            //        StationGraphic.Attributes.Add(Dictionaries.DatabaseLiterals.FieldLocationID, ptStationLocationID.ToString());
            //        if (ptStationType != null)
            //        {
            //            StationGraphic.Attributes.Add("Type", ptStationType.ToString());
            //        }
            //        else
            //        {
            //            StationGraphic.Attributes.Add("Type", string.Empty);
            //        }

            //        StationGraphic.Attributes.Add("Default", isDefaultDB);

            //        pointOverlay.Graphics.Add(StationGraphic);

            //        #endregion

            //        #region LABEL SYMBOL
            //        GraphicPlacement placements = new GraphicPlacement();
            //        var textSym = new TextSymbol
            //        {
            //            FontFamily = "Arial",
            //            FontWeight = FontWeight.Bold,
            //            Color = System.Drawing.Color.Black,
            //            HaloColor = System.Drawing.Color.WhiteSmoke,
            //            HaloWidth = 2,
            //            Size = 16,
            //            HorizontalAlignment = Esri.ArcGISRuntime.Symbology.HorizontalAlignment.Left,
            //            VerticalAlignment = Esri.ArcGISRuntime.Symbology.VerticalAlignment.Baseline,
            //            OffsetX = placements.GetOffsetFromPlacementPriority(placementPool[0]).Item1,
            //            OffsetY = placements.GetOffsetFromPlacementPriority(placementPool[0]).Item2
            //        };
            //        placementPool.RemoveAt(0); //Remove taken placement from pool
            //        textSym.Text = ptStationId.ToString();
            //        pointLabelOverlay.Graphics.Add(new Graphic(new MapPoint(ptLongitude, ptLatitude, SpatialReferences.Wgs84), textSym));
            //        #endregion

            //        #region STRUCTURES

            //        ///For structure symboles (planar and linear) make sure they are wanted by user but that it's within a bedrock field notebook also

            //        if ((bool)localSettings.GetSettingValue(ApplicationLiterals.KeyworkStructureSymbols) && _mapPageQuickMeasurementEnable)
            //        {
            //            //Get related structures, if any
            //            List<object> strucTableRows = new List<object>();
            //            Structure structs = new Structure();
            //            string structSelectionQuery = "SELECT s.* FROM " + DatabaseLiterals.TableStructure + " s" +
            //                " JOIN " + DatabaseLiterals.TableEarthMat + " e on e." + DatabaseLiterals.FieldStructureParentID + " = s." + DatabaseLiterals.FieldEarthMatID +
            //                " JOIN " + DatabaseLiterals.TableStation + " st on st." + DatabaseLiterals.FieldStationID + " = e." + DatabaseLiterals.FieldEarthMatStatID +
            //                " WHERE st." + DatabaseLiterals.FieldStationAlias + " = '" + ptStationId + "';";
            //            strucTableRows = accessData.ReadTableFromDBConnectionWithoutClosingConnection(structs.GetType(), structSelectionQuery, dbConnection);

            //            //Variables
            //            if (!structureGraphicExists && strucTableRows.Count() > 0)
            //            {
            //                //Structure pairs tracking
            //                //Key = record ID, Value = priority number for placement
            //                Dictionary<string, int> strucPairs = new Dictionary<string, int>();
            //                int iteration = 1;
            //                foreach (Structure sts in strucTableRows)
            //                {
            //                    //Manage pair tracking for pool placement 
            //                    if (!strucPairs.ContainsKey(sts.StructureID))
            //                    {
            //                        if (sts.StructureRelated != null && sts.StructureRelated != String.Empty)
            //                        {
            //                            //Get related struc placement priority
            //                            strucPairs[sts.StructureID] = strucPairs[sts.StructureRelated];
            //                        }
            //                        else
            //                        {
            //                            //Assign new priority and remove it from the pool
            //                            strucPairs[sts.StructureID] = iteration;
            //                            iteration = iteration + 1;
            //                        }

            //                    }


            //                    //Set proper symbol
            //                    PictureMarkerSymbol strucSym = StrucPlaneSym.Clone() as PictureMarkerSymbol;
            //                    if (sts.StructureClass == DatabaseLiterals.KeywordLinear)
            //                    {
            //                        strucSym = StrucLinearSym.Clone() as PictureMarkerSymbol;
            //                    }

            //                    //Set azim
            //                    double.TryParse(sts.StructureSymAng, out double azimAngle);
            //                    if (azimAngle != 0.0)
            //                    {
            //                        strucSym.Angle = azimAngle;
            //                    }
            //                    strucSym.AngleAlignment = SymbolAngleAlignment.Map; //Set to map else symbol will keep same direction or mapview is rotated

            //                    //Set offset
            //                    Tuple<double, double> symOffset = placements.GetPositionOffsetFromPlacementPriority(strucPairs[sts.StructureID], ptLongitude, ptLatitude, 100.0);

            //                    //Create Map Point for graphic
            //                    MapPoint geoStructPoint = new MapPoint(symOffset.Item1, symOffset.Item2, SpatialReferences.Wgs84);

            //                    //Get if datum transformation is needed
            //                    if (epsg != 0 && epsg != 4326)
            //                    {
            //                        DatumTransformation datumTransfo = null;
            //                        SpatialReference outSR = null;

            //                        if ((epsg > 26900 && epsg < 27000))
            //                        {
            //                            outSR = SpatialReference.Create(4617);
            //                            datumTransfo = TransformationCatalog.GetTransformation(outSR, SpatialReferences.Wgs84);
            //                        }

            //                        MapPoint proPoint = new MapPoint(ptLongitude, ptLatitude, outSR);

            //                        //Validate if transformation is needed.
            //                        if (datumTransfo != null)
            //                        {
            //                            //Replace geopoint
            //                            geoStructPoint = (MapPoint)Esri.ArcGISRuntime.Geometry.GeometryEngine.Project(proPoint, SpatialReferences.Wgs84, datumTransfo);
            //                        }

            //                    }

            //                    //TODO make up for a different way to measure azim (not right hand rule)
            //                    var Sgraphic = new Graphic(geoStructPoint, strucSym);
            //                    Sgraphic.Attributes.Add("Id", sts.StructureName.ToString());
            //                    Sgraphic.Attributes.Add("Date", ptStationDate.ToString());
            //                    Sgraphic.Attributes.Add("ParentID", sts.StructureParentID);
            //                    Sgraphic.Attributes.Add("StructureClass", sts.getClassTypeDetail);
            //                    Sgraphic.Attributes.Add("Azim", sts.StructureAzimuth.ToString());
            //                    Sgraphic.Attributes.Add("Dip", sts.StructureDipPlunge.ToString());
            //                    Sgraphic.Attributes.Add("Default", isDefaultDB);
            //                    Sgraphic.Attributes.Add("tableType", DatabaseLiterals.TableStructure);
            //                    Sgraphic.Attributes.Add(Dictionaries.DatabaseLiterals.FieldLocationID, ptStationLocationID.ToString());
            //                    structureOverlay.Graphics.Add(Sgraphic);

            //                    //Set station symbol to transparent so we clearly see the structures instead
            //                    //StationGraphic.IsVisible = false;

            //                }


            //            }
            //            else
            //            {

            //            }
            //        }
            //        else
            //        {
            //            //Set station symbol to transparent so we clearly see the structures instead
            //            StationGraphic.IsVisible = true;
            //            structureOverlay.Graphics.Clear();
            //        }

            //        #endregion
            //    }

            //    #endregion
            //}
            //#endregion

            //#region REMOVE
            ////For remaining loc in loadedGraphicList
            //foreach (KeyValuePair<string, Graphic> grr in graphicList)
            //{
            //    int indexOfGraphic = pointOverlay.Graphics.IndexOf(grr.Value);
            //    pointOverlay.Graphics.RemoveAt(indexOfGraphic);
            //    pointLabelOverlay.Graphics.RemoveAt(indexOfGraphic);
            //}

            //#endregion

        }


        /// <summary>
        /// From a given database connection , will add point and labels of stations on the map page
        /// Information will be loaded if the schema is the same between all databases and the version with which
        /// this application version has been coded.
        /// </summary>
        /// <param name="inLocationTableRows"></param>
        public void LoadFromGivenDBTest(Dictionary<string, Graphic> graphicList, bool isDefaultDB)
        {
            PictureMarkerSymbol pointSym = new PictureMarkerSymbol(new Uri("ms-appx:///Assets/IC809393.png"));

            Uri planarPath = new Uri("ms-appx:///Assets/Images/theme-light/struc_planar.png");
            Uri linearPath = new Uri("ms-appx:///Assets/Images/theme-light/struc_linear.png");

            PictureMarkerSymbol StrucPlaneSym = new PictureMarkerSymbol(planarPath);
            PictureMarkerSymbol StrucLinearSym = new PictureMarkerSymbol(linearPath);

            //Choose proper overlay
            GraphicsOverlay pointOverlay = new GraphicsOverlay();
            GraphicsOverlay pointLabelOverlay = new GraphicsOverlay();
            GraphicsOverlay structureOverlay = new GraphicsOverlay();

            //Set some rendering defaults
            Renderer graphRenderer = new SimpleRenderer
            {
                RotationType = RotationType.Geographic
            };
            pointOverlay.Renderer = graphRenderer;

            if (isDefaultDB)
            {
                pointOverlay = _OverlayStation;
                pointLabelOverlay = _OverlayStationLabel;
                structureOverlay = _OverlayStructure;
            }

            #region ADD

            string selectMetadata = "SELECT * FROM " + DatabaseLiterals.TableMetadata + " fm ";
            string joinLocation = "JOIN " + DatabaseLiterals.TableLocation + " fl on fm." + DatabaseLiterals.FieldUserInfoID + " = fl." + DatabaseLiterals.FieldLocationMetaID + " ";
            string joinStation = "JOIN " + DatabaseLiterals.TableStation + " fs on fl." + DatabaseLiterals.FieldLocationID + " = fs." + DatabaseLiterals.FieldStationObsID + " ";
            string whereMetadata = string.Empty;
            if (localSetting.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoID) != null)
            {
                whereMetadata = " WHERE fm." + DatabaseLiterals.FieldUserInfoID + " = '" + localSetting.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoID).ToString() + "'";
            }
            MapPageStation mps = new MapPageStation();
            List<object> mpsRows = new List<object>();
            mpsRows = accessData.ReadTable(mps.GetType(), selectMetadata + joinLocation + joinStation + whereMetadata);

            // Get latitude, longitude and station id and add to graphics overlay
            foreach (object m in mpsRows)
            {
                //Variables
                bool stationGraphicExists = false;
                bool structureGraphicExists = false;

                #region POINT SYMBOL
                // should only be one station returned, this approach doesn't allow for multiple stations
                MapPageStation currentStationLocation = m as MapPageStation;
                var ptStationId = currentStationLocation.StationAlias;
                var ptStationDate = currentStationLocation.StationVisitDate;
                var ptStationTime = currentStationLocation.StationVisitTime;
                var ptStationType = currentStationLocation.StationObsType;
                var ptStationLocationID = currentStationLocation.LocationID;
                var ptStationLocationLat = currentStationLocation.LocationLat;
                var ptStationLocationLong = currentStationLocation.LocationLong;
                var ptStationLocationEPSG = currentStationLocation.LocationDatum;


                //Find if station was already loaded
                if (graphicList.ContainsKey(currentStationLocation.LocationID))
                {
                    stationGraphicExists = true;
                    graphicList.Remove(currentStationLocation.LocationID);
                }

                //Add new graphic station and it's related label if needed
                if (!stationGraphicExists && ptStationId != null && ptStationId != string.Empty)
                {
                    //Tracking available offset placement 
                    List<int> placementPool = Enumerable.Range(1, 8).ToList();

                    #region MAIN POINT
                    //Create Map Point for graphic
                    MapPoint geoPoint = new MapPoint(ptStationLocationLong, ptStationLocationLat, SpatialReferences.Wgs84);

                    //Get if datum transformation is needed
                    int.TryParse(ptStationLocationEPSG, out int epsg);

                    if (epsg != 0 && epsg != 4326)
                    {
                        DatumTransformation datumTransfo = null;
                        SpatialReference outSR = null;

                        if ((epsg > 26900 && epsg < 27000))
                        {
                            outSR = SpatialReference.Create(4326);
                            datumTransfo = TransformationCatalog.GetTransformation(outSR, SpatialReferences.Wgs84);
                        }


                        MapPoint proPoint = new MapPoint(ptStationLocationLong, ptStationLocationLat, outSR);

                        //Validate if transformation is needed.
                        if (datumTransfo != null)
                        {
                            //Replace geopoint
                            geoPoint = (MapPoint)Esri.ArcGISRuntime.Geometry.GeometryEngine.Project(proPoint, SpatialReferences.Wgs84, datumTransfo);
                        }

                    }

                    var StationGraphic = new Graphic(geoPoint, pointSym);
                    StationGraphic.Attributes.Add("Id", ptStationId.ToString());
                    StationGraphic.Attributes.Add("Date", ptStationDate.ToString());
                    StationGraphic.Attributes.Add("Time", ptStationTime.ToString());
                    StationGraphic.Attributes.Add("tableType", DatabaseLiterals.TableStation);
                    StationGraphic.Attributes.Add(Dictionaries.DatabaseLiterals.FieldLocationID, ptStationLocationID.ToString());
                    if (ptStationType != null)
                    {
                        StationGraphic.Attributes.Add("Type", ptStationType.ToString());
                    }
                    else
                    {
                        StationGraphic.Attributes.Add("Type", string.Empty);
                    }

                    StationGraphic.Attributes.Add("Default", isDefaultDB);

                    pointOverlay.Graphics.Add(StationGraphic);

                    #endregion

                    #region LABEL SYMBOL
                    GraphicPlacement placements = new GraphicPlacement();
                    var textSym = new TextSymbol
                    {
                        FontFamily = "Arial",
                        FontWeight = FontWeight.Bold,
                        Color = System.Drawing.Color.Black,
                        HaloColor = System.Drawing.Color.WhiteSmoke,
                        HaloWidth = 2,
                        Size = 16,
                        HorizontalAlignment = Esri.ArcGISRuntime.Symbology.HorizontalAlignment.Left,
                        VerticalAlignment = Esri.ArcGISRuntime.Symbology.VerticalAlignment.Baseline,
                        OffsetX = placements.GetOffsetFromPlacementPriority(placementPool[0]).Item1,
                        OffsetY = placements.GetOffsetFromPlacementPriority(placementPool[0]).Item2
                    };
                    placementPool.RemoveAt(0); //Remove taken placement from pool
                    textSym.Text = ptStationId.ToString();
                    pointLabelOverlay.Graphics.Add(new Graphic(new MapPoint(ptStationLocationLong, ptStationLocationLat, SpatialReferences.Wgs84), textSym));
                    #endregion

                    #region STRUCTURES

                    ///For structure symboles (planar and linear) make sure they are wanted by user but that it's within a bedrock field notebook also
                    if (localSettings.GetSettingValue(ApplicationLiterals.KeyworkStructureSymbols) != null && (bool)localSettings.GetSettingValue(ApplicationLiterals.KeyworkStructureSymbols) && _mapPageQuickMeasurementEnable)
                    {
                        //Get related structures, if any
                        List<object> strucTableRows = new List<object>();
                        Structure structs = new Structure();
                        string structSelectionQuery = "SELECT s.* FROM " + DatabaseLiterals.TableStructure + " s" +
                            " JOIN " + DatabaseLiterals.TableEarthMat + " e on e." + DatabaseLiterals.FieldStructureParentID + " = s." + DatabaseLiterals.FieldEarthMatID +
                            " JOIN " + DatabaseLiterals.TableStation + " st on st." + DatabaseLiterals.FieldStationID + " = e." + DatabaseLiterals.FieldEarthMatStatID +
                            " WHERE st." + DatabaseLiterals.FieldStationAlias + " = '" + ptStationId + "';";
                        strucTableRows = accessData.ReadTable(structs.GetType(), structSelectionQuery);

                        //Variables
                        if (!structureGraphicExists && strucTableRows.Count() > 0)
                        {
                            //Structure pairs tracking
                            //Key = record ID, Value = priority number for placement
                            Dictionary<int, int> strucPairs = new Dictionary<int, int>();
                            int iteration = 1;
                            foreach (Structure sts in strucTableRows)
                            {
                                //Manage pair tracking for pool placement 
                                if (!strucPairs.ContainsKey(sts.StructureID))
                                {
                                    if (sts.StructureRelated != null)
                                    {
                                        int relatedStruc = sts.StructureRelated ?? default(int);
                                        //Get related struc placement priority
                                        strucPairs[sts.StructureID] = strucPairs[relatedStruc];
                                    }
                                    else
                                    {
                                        //Assign new priority and remove it from the pool
                                        strucPairs[sts.StructureID] = iteration;
                                        iteration = iteration + 1;
                                    }

                                }


                                //Set proper symbol
                                PictureMarkerSymbol strucSym = StrucPlaneSym.Clone() as PictureMarkerSymbol;
                                if (sts.StructureClass == DatabaseLiterals.KeywordLinear)
                                {
                                    strucSym = StrucLinearSym.Clone() as PictureMarkerSymbol;
                                }

                                //Set azim
                                double azimAngle = sts.StructureSymAng;
                                if (azimAngle != 0.0)
                                {
                                    strucSym.Angle = azimAngle;
                                }
                                strucSym.AngleAlignment = SymbolAngleAlignment.Map; //Set to map else symbol will keep same direction or mapview is rotated

                                //Set offset
                                Tuple<double, double> symOffset = placements.GetPositionOffsetFromPlacementPriority(strucPairs[sts.StructureID], ptStationLocationLong, ptStationLocationLat, 100.0);

                                //Create Map Point for graphic
                                MapPoint geoStructPoint = new MapPoint(symOffset.Item1, symOffset.Item2, SpatialReferences.Wgs84);

                                //Get if datum transformation is needed
                                if (epsg != 0 && epsg != 4326)
                                {
                                    DatumTransformation datumTransfo = null;
                                    SpatialReference outSR = null;

                                    if ((epsg > 26900 && epsg < 27000))
                                    {
                                        outSR = SpatialReference.Create(4617);
                                        datumTransfo = TransformationCatalog.GetTransformation(outSR, SpatialReferences.Wgs84);
                                    }

                                    MapPoint proPoint = new MapPoint(ptStationLocationLong, ptStationLocationLat, outSR);

                                    //Validate if transformation is needed.
                                    if (datumTransfo != null)
                                    {
                                        //Replace geopoint
                                        geoStructPoint = (MapPoint)Esri.ArcGISRuntime.Geometry.GeometryEngine.Project(proPoint, SpatialReferences.Wgs84, datumTransfo);
                                    }

                                }

                                //TODO make up for a different way to measure azim (not right hand rule)
                                var Sgraphic = new Graphic(geoStructPoint, strucSym);
                                Sgraphic.Attributes.Add("Id", sts.StructureName.ToString());
                                Sgraphic.Attributes.Add("Date", ptStationDate.ToString());
                                Sgraphic.Attributes.Add("ParentID", sts.StructureParentID);
                                Sgraphic.Attributes.Add("StructureClass", sts.getClassTypeDetail);
                                Sgraphic.Attributes.Add("Azim", sts.StructureAzimuth.ToString());
                                Sgraphic.Attributes.Add("Dip", sts.StructureDipPlunge.ToString());
                                Sgraphic.Attributes.Add("Default", isDefaultDB);
                                Sgraphic.Attributes.Add("tableType", DatabaseLiterals.TableStructure);
                                Sgraphic.Attributes.Add(Dictionaries.DatabaseLiterals.FieldLocationID, ptStationLocationID.ToString());
                                structureOverlay.Graphics.Add(Sgraphic);

                                //Set station symbol to transparent so we clearly see the structures instead
                                //StationGraphic.IsVisible = false;

                            }


                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        //Set station symbol to transparent so we clearly see the structures instead
                        StationGraphic.IsVisible = true;
                        structureOverlay.Graphics.Clear();
                    }

                    #endregion
                }

                #endregion
            }
            #endregion

            #region REMOVE
            //For remaining loc in loadedGraphicList
            foreach (KeyValuePair<string, Graphic> grr in graphicList)
            {
                int indexOfGraphic = pointOverlay.Graphics.IndexOf(grr.Value);
                pointOverlay.Graphics.RemoveAt(indexOfGraphic);
                pointLabelOverlay.Graphics.RemoveAt(indexOfGraphic);
            }

            #endregion

        }

        /// <summary>
        /// Whenever a quick button is hit. Make some validation first about location
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void MapPageQuickButtons_Tapped(object sender, TappedRoutedEventArgs e)
        {
            AppBarButton senderButton = sender as AppBarButton;

            // Language localization using Resource.resw
            //var local = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();

            //Find which quick button was clicked
            if (!senderButton.Name.ToLower().Contains(Dictionaries.DatabaseLiterals.KeywordLocation))
            {
                if (senderButton.Name.ToLower().Contains(Dictionaries.DatabaseLiterals.KeywordStation))
                {
                    clickedQuickButton = Dictionaries.DatabaseLiterals.KeywordStation;
                }
                if (senderButton.Name.ToLower().Contains(Dictionaries.DatabaseLiterals.KeywordSample))
                {
                    clickedQuickButton = Dictionaries.DatabaseLiterals.KeywordSample;
                }
                if (senderButton.Name.ToLower().Contains(Dictionaries.DatabaseLiterals.KeywordPhoto))
                {
                    clickedQuickButton = Dictionaries.DatabaseLiterals.KeywordPhoto;
                }
                if (senderButton.Name.ToLower().Contains(Dictionaries.DatabaseLiterals.KeywordStructure))
                {
                    clickedQuickButton = Dictionaries.DatabaseLiterals.KeywordStructure;
                }
                if (senderButton.Name.ToLower().Contains(Dictionaries.DatabaseLiterals.KeywordStationWaypoint))
                {
                    clickedQuickButton = Dictionaries.DatabaseLiterals.KeywordStationWaypoint;
                }

                //Validate accessibility
                Task<bool> canAccess = ValidateGeolocationAccess();
                await canAccess;
                // Check that GPS is on, should also be a check that coordinates are being received
                if (canAccess.Result && !userHasTurnedGPSOff && _currentMSGeoposition != null && _OverlayCurrentPosition != null && _OverlayCurrentPosition.Graphics.Count > 0)
                {
                    try
                    {
                        _currentMSGeoposition = await _geolocator.GetGeopositionAsync();
                    }
                    catch (Exception)
                    {

                    }

                    // Check that horizontal accuracy is better then 30 m, arbitrary number
                    if (_currentMSGeoposition.Coordinate.Accuracy <= 20.0 && _currentMSGeoposition.Coordinate.Accuracy != 0.0 && _currentMSGeoposition.Coordinate.Point.Position.Longitude != 0 && _currentMSGeoposition.Coordinate.Point.Position.Latitude != 0)
                    {

                        GotoQuickDialog(null);

                    }
                    else
                    {

                        PoorLocationRoutineTap();

                    }
                }
                else
                {
                    if (!userHasTurnedGPSOff)
                    {
                        //Case user has also turned location off
                        if (!canAccess.Result)
                        {
                            await NoLocationRoutine();
                        }
                        else
                        {
                            if (initializingGPS)
                            {
                                //Force call on UI thread, else it could crash the app if async call is made another thread.
                                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                                {


                                    ContentDialog acquiringLocationDialog = new ContentDialog()
                                    {
                                        Title = local.GetString("MapPageDialogLocationTitle"),
                                        Content = local.GetString("MapPageDialogLocationAcquiring"),
                                        PrimaryButtonText = local.GetString("GenericDialog_ButtonOK"),
                                    };
                                    acquiringLocationDialog.Style = (Style)Application.Current.Resources["WarningDialog"];

                                    await Services.ContentDialogMaker.CreateContentDialogAsync(acquiringLocationDialog, true);
                                    ResetLocationGraphic();

                                }).AsTask();
                            }
                            else
                            {
                                PoorLocationRoutineTap();
                            }

                        }
                    }
                    else
                    {
                        NoLocationRoutineTapOnly();
                    }

                }
            }
            else
            {
                if (senderButton.Name.ToLower().Contains(Dictionaries.DatabaseLiterals.KeywordLocation))
                {
                    clickedQuickButton = Dictionaries.DatabaseLiterals.KeywordLocation;
                    GotoQuickDialog(null);
                }
            }

        }
        #endregion

        #region VIEW NAVIGATION

        /// <summary>
        /// From user selected option, will open the right dialog. If a map point is given, dialog will init this else it will take current position
        /// </summary>
        /// <param name="inLocation">The map point to force dialog to use, else current location will be used</param>
        public void GotoQuickDialog(FieldLocation inLocation)
        {
            ClearSelection();

            pauseGraphicRefresh = true; //Make sure the location graphic is paused and doesn't interact with UI.

            if (clickedQuickButton != Dictionaries.DatabaseLiterals.KeywordLocation)
            {
                if (inLocation == null)
                {
                    inLocation = new FieldLocation
                    {
                        LocationElev = _currentMSGeoposition.Coordinate.Point.Position.Altitude,
                        LocationLat = _currentMSGeoposition.Coordinate.Point.Position.Latitude,
                        LocationLong = _currentMSGeoposition.Coordinate.Point.Position.Longitude,
                        LocationErrorMeasure = _currentMSGeoposition.Coordinate.Accuracy,
                        LocationElevMethod = vocabElevmethodGPS,
                        LocationEntryType = _currentMSGeoposition.Coordinate.PositionSource.ToString(),
                        LocationErrorMeasureType = vocabErrorMeasureTypeMeter,
                        LocationElevationAccuracy = _currentMSGeoposition.Coordinate.AltitudeAccuracy
                    };
                }
                if (clickedQuickButton == Dictionaries.DatabaseLiterals.KeywordStation)
                {
                    GotoStationDataPart(inLocation);
                }
                if (clickedQuickButton == Dictionaries.DatabaseLiterals.KeywordSample)
                {
                    GotoSampleDialog(inLocation);
                }
                if (clickedQuickButton == Dictionaries.DatabaseLiterals.KeywordPhoto)
                {
                    GotoPhotoDialog(inLocation);
                }
                if (clickedQuickButton == Dictionaries.DatabaseLiterals.KeywordStructure)
                {
                    string projectType = localSettings.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoFWorkType).ToString();
                    if (projectType != null)
                    {
                        if (projectType == Dictionaries.ScienceLiterals.ApplicationThemeSurficial)
                        {
                            GotoPflowDialog(inLocation);
                        }
                        else
                        {
                            GotoStructureDialog(inLocation);
                        }

                    }
                    else
                    {
                        GotoStructureDialog(inLocation);
                    }

                }
                if (clickedQuickButton == Dictionaries.DatabaseLiterals.KeywordStationWaypoint)
                {
                    AddStationWaypoint(inLocation);
                }

                lastTakenLocation = new Tuple<double, double>(inLocation.LocationLong, inLocation.LocationLat);
            }
            else
            {
                FieldLocation manualLocation = new FieldLocation
                {
                    LocationElev = 0.0,
                    LocationLat = 0.0,
                    LocationLong = 0.0,
                    LocationEntryType = vocabEntryTypeManual,
                    //LocationID = idCalculator.CalculateLocationID(), //Calculate new value
                    LocationAlias = idCalculator.CalculateLocationAlias(string.Empty), //Calculate new value
                    MetaID = int.Parse(localSetting.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoID).ToString()) //Foreign key
                };

                GotoLocationDataPart(manualLocation);
            }

        }

        /// <summary>
        /// Will pop the station dialog
        /// </summary>
        public void GotoStationDataPart(FieldLocation stationMapPoint)
        {

            var modal = Window.Current.Content as ModalDialog;
            var view = modal.ModalContent as Views.StationDataPart;
            modal.ModalContent = view = new Views.StationDataPart(null, false);
            view.mapPosition = stationMapPoint;
            view.ViewModel.newStationEdit -= NavigateToReport;
            view.ViewModel.newStationEdit += NavigateToReport; //Detect when the add/edit request has finished.
            modal.IsModal = true;
            view.stationClosed -= modalDialogClosed;
            view.stationClosed += modalDialogClosed;
            DataLocalSettings dLocalSettings = new DataLocalSettings();
            dLocalSettings.SetSettingValue("forceNoteRefresh", false);
        }

        private void modalDialogClosed(object sender)
        {
            //Unpaused graphic refresh 
            pauseGraphicRefresh = false;
        }

        /// <summary>
        /// Will pop the station dialog
        /// </summary>
        public void GotoLocationDataPart(FieldLocation locationEmptyEntry)
        {

            var modal = Window.Current.Content as ModalDialog;
            var view = modal.ModalContent as Views.LocationDialog;
            FieldNotes newLocationFieldNotes = new FieldNotes
            {
                location = locationEmptyEntry
            };
            modal.ModalContent = view = new Views.LocationDialog(newLocationFieldNotes);
            view.locationVM.doLocationUpdate = false;
            view.locationVM.newLocationEdit -= NavigationToStationDialog;
            view.locationVM.newLocationEdit += NavigationToStationDialog; //Detect when the add/edit request has finished.
            modal.IsModal = true;
            DataLocalSettings dLocalSettings = new DataLocalSettings();
            dLocalSettings.SetSettingValue("forceNoteRefresh", true);
        }

        /// <summary>
        /// When no location is available. Display this message.
        /// </summary>
        public async Task NoLocationRoutine()
        {
            //// Language localization using Resource.resw
            //var local = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();

            ContentDialog noLocationDialog = new ContentDialog()
            {
                Title = local.GetString("MapPageDialogLocationTitle"),
                Content = local.GetString("MapPageDialogTextLocationStatus"),
                PrimaryButtonText = local.GetString("MapPageDialogTextYes"),
                SecondaryButtonText = local.GetString("MapPageDialogTextNo"),
            };

            noLocationDialog.Style = (Style)Application.Current.Resources["WarningDialog"];

            try
            {
                ContentDialogResult cdr = await Services.ContentDialogMaker.CreateContentDialogAsync(noLocationDialog, true).Result;

                if (cdr == ContentDialogResult.Primary)
                {
                    bool result = await Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-location"));
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("Warning dialog for location allocation failed.");
            }



        }

        /// <summary>
        /// When no location is available probably due to flight mode. Display this message.
        /// </summary>
        public async Task NoLocationFlightMode()
        {
            // Language localization using Resource.resw
            //var local = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                ContentDialog noLocationFlightModeDialog = new ContentDialog()
                {
                    Title = local.GetString("MapPageDialogLocationTitle"),
                    Content = local.GetString("MapPageDialogLocationFlightMode"),
                    PrimaryButtonText = local.GetString("GenericDialog_ButtonOK"),
                };

                noLocationFlightModeDialog.Style = (Style)Application.Current.Resources["WarningDialog"];
                ContentDialogResult flightModeResult = await Services.ContentDialogMaker.CreateContentDialogAsync(noLocationFlightModeDialog, true).Result;
                if (flightModeResult == ContentDialogResult.Primary)
                {

                }
            }).AsTask();
        }

        /// <summary>
        /// Pop this message when the gps is turned off and user wants to take a quick record.
        /// </summary>
        public async void PoorLocationRoutineTap()
        {
            //Remove any location ring
            StopLocationRing();

            // Language localization using Resource.resw
            //var local = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();

            ContentDialog noPoorHorizontalAccuracyDialog = new ContentDialog()
            {
                Title = local.GetString("MapPageDialogLocationTitle"),
                Content = local.GetString("MapPageDialogTextHorizontalAccuracy"),
                SecondaryButtonText = local.GetString("MapPageDialogTextYes"),
                CloseButtonText = local.GetString("MapPageDialogTextNo"),
                PrimaryButtonText = local.GetString("MapPageDialogTextTap")
            };
            noPoorHorizontalAccuracyDialog.Style = (Style)Application.Current.Resources["WarningDialog"];
            ContentDialogResult cdr = await Services.ContentDialogMaker.CreateContentDialogAsync(noPoorHorizontalAccuracyDialog, true).Result;

            if (cdr == ContentDialogResult.Secondary)
            {
                currentMapView.Tapped -= myMapView_AddByTap;
                userHasTurnedGPSOff = false;
                SetGPSModeIcon();
                await Task.Delay(1000);
                GotoQuickDialog(null);

            }
            else if (cdr == ContentDialogResult.Primary)
            {
                // SPW 2019
                ResetLocationGraphic();
                currentMapView.Tapped += myMapView_AddByTap;
                userHasTurnedGPSOff = true;
                SetGPSModeIcon(Symbol.TouchPointer);
                RaisePropertyChanged("GPSModeSymbol");
            }


        }

        /// <summary>
        /// Pop this message when the gps is turned off and user wants to take a quick record.
        /// </summary>
        public async void NoLocationRoutineTapOnly()
        {
            // Language localization using Resource.resw
            //var local = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {

                ContentDialog noPoorHorizontalAccuracyDialog = new ContentDialog()
                {
                    Title = local.GetString("MapPageDialogLocationTitle"),
                    Content = local.GetString("MapPageDialogLocationTapMode"),
                    PrimaryButtonText = local.GetString("MapPageDialogTextGPS"),
                    SecondaryButtonText = local.GetString("GenericDialog_ButtonYes"),
                    CloseButtonText = local.GetString("GenericDialog_ButtonNo")
                };
                noPoorHorizontalAccuracyDialog.Style = (Style)Application.Current.Resources["WarningDialog"];
                ContentDialogResult cdr = await Services.ContentDialogMaker.CreateContentDialogAsync(noPoorHorizontalAccuracyDialog, true).Result;

                if (cdr == ContentDialogResult.Primary)
                {

                    currentMapView.Tapped -= myMapView_AddByTap;
                    userHasTurnedGPSOff = false;
                    SetGPSModeIcon();
                    await SetGPS();
                }
                else if (cdr == ContentDialogResult.Secondary)
                {
                    currentMapView.Tapped += myMapView_AddByTap;
                    userHasTurnedGPSOff = true;
                    SetGPSModeIcon(Symbol.TouchPointer);
                }
            }).AsTask();
        }

        /// <summary>
        /// Will show a new dialog for sample, but first a 
        /// quick earthmat record will be added, in which a cascade will happen
        /// to create a new station and also a new location.
        /// </summary>
        public void GotoSampleDialog(FieldLocation sampleMapPoint)
        {
            //Create a quick earthmat
            EarthmatViewModel eVM = new EarthmatViewModel(null);
            FieldNotes quickEarthmat = eVM.QuickEarthmat(sampleMapPoint);

            var modal = Window.Current.Content as ModalDialog;
            var view = modal.ModalContent as Views.SampleDialog;
            modal.ModalContent = view = new Views.SampleDialog(quickEarthmat, true);
            view.ViewModel.newSampleEdit -= NavigateToReport;
            view.ViewModel.newSampleEdit += NavigateToReport; //Detect when the add/edit request has finished.
            modal.IsModal = true;

            view.sampClosed -= modalDialogClosed;
            view.sampClosed += modalDialogClosed;

        }

        public void GotoStructureDialog(FieldLocation structureMapPoint)
        {
            //Create a quick earthmat
            EarthmatViewModel eVM = new EarthmatViewModel(null);
            FieldNotes quickEarthmat = eVM.QuickEarthmat(structureMapPoint);

            var modal = Window.Current.Content as ModalDialog;
            var view = modal.ModalContent as Views.StructureDialog;
            modal.ModalContent = view = new Views.StructureDialog(quickEarthmat, true);
            view.strucViewModel.newStructureEdit -= NavigateToReport;
            view.strucViewModel.newStructureEdit += NavigateToReport; //Detect when the add/edit request has finished.
            modal.IsModal = true;
            view.strucClosed -= modalDialogClosed;
            view.strucClosed += modalDialogClosed;


        }

        /// <summary>
        /// Will show a new dialog for paleo flow dialog
        /// </summary>
        /// <param name="structureMapPoint"></param>
        public void GotoPflowDialog(FieldLocation structureMapPoint)
        {
            //Create a quick earthmat
            EarthmatViewModel eVM = new EarthmatViewModel(null);
            FieldNotes quickEarthmat = eVM.QuickEarthmat(structureMapPoint);

            var modal = Window.Current.Content as ModalDialog;
            var view = modal.ModalContent as Views.PaleoflowDialog;
            modal.ModalContent = view = new Views.PaleoflowDialog(quickEarthmat);
            view.pflowModel.newPflowEdit -= NavigateToReport;
            view.pflowModel.newPflowEdit += NavigateToReport; //Detect when the add/edit request has finished.
            modal.IsModal = true;

            view.pflowClosed -= modalDialogClosed;
            view.pflowClosed += modalDialogClosed;

        }

        /// <summary>
        /// Will show a new dialog for photos, but first a quick photo
        /// record will be added in which a cascade will happen to create
        /// a new station and also a new location.
        /// </summary>
        public void GotoPhotoDialog(FieldLocation documentMapPoint)
        {
            //Create a quick earthmat
            StationViewModel svm = new StationViewModel(false);
            FieldNotes quickStation = svm.QuickStation(documentMapPoint);

            ModalDialog modal = Window.Current.Content as ModalDialog;
            var view = modal.ModalContent as Views.DocumentDialog;
            modal.ModalContent = view = new Views.DocumentDialog(quickStation, quickStation, true);
            view.DocViewModel.newDocumentEdit -= NavigateToReport;
            view.DocViewModel.newDocumentEdit += NavigateToReport; //Detect when the add/edit request has finished.
            modal.IsModal = true;

            view.documentClosed -= modalDialogClosed;
            view.documentClosed += modalDialogClosed;

        }

        /// <summary>
        /// Will pop the station dialog
        /// </summary>
        public void AddStationWaypoint(FieldLocation stationWaypointMapPoint)
        {

            var modal = Window.Current.Content as ModalDialog;
            var view = modal.ModalContent as Views.StationDataPart;
            modal.ModalContent = view = new Views.StationDataPart(null, true);
            view.mapPosition = stationWaypointMapPoint;
            view.ViewModel.newStationEdit -= ViewModel_newStationEdit;
            view.ViewModel.newStationEdit += ViewModel_newStationEdit;
            modal.IsModal = true;

            DataLocalSettings dLocalSettings = new DataLocalSettings();
            dLocalSettings.SetSettingValue("forceNoteRefresh", true);
        }

        /// <summary>
        /// Mainly used to refresh the map, after some data entry, example waypoint addition.
        /// </summary>
        /// <param name="sender"></param>
        private void RefreshMap(bool forceRefresh = false)
        {
            if (forceRefresh)
            {
                ClearMapViewSettings();
            }
            SetMapView(currentMapView);
            DisplayPointAndLabelsAsync(currentMapView, forceRefresh);
        }

        /// <summary>
        /// Mainly used when user needs to navigate to the field note page after a certain steps has been taken
        /// </summary>
        /// <param name="sender"></param>
        private void NavigateToReport(object sender)
        {
            //Navigate to the report page.
            NavigationService.Navigate(typeof(Views.FieldNotesPage), new[] { selectedStationID, selectedStationDate });
        }

        /// <summary>
        /// Mainly used to show the station dialog when following location manual xy entry.
        /// </summary>
        /// <param name="sender"></param>
        private void NavigationToStationDialog(object sender)
        {
            GotoStationDataPart(null);
        }

        #endregion

        #region EVENTS

        /// <summary>
        /// Event triggered when user has edited locaation coordinate. Needs a force refresh on points.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LocationViewModel_LocationUpdateEventHandler(object sender, EventArgs e)
        {
            //Force a redraw of all locations
            RefreshMap(true);
        }

        private void ViewModel_newStationEdit(object sender)
        {
            RefreshMap(); //Detect when the add/edit request has finished.
        }

        async public void OnPositionChanged(Geolocator sender, PositionChangedEventArgs e)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Geolocal_Update(e.Position);
            });
        }

        /// <summary>
        /// Whenever user location changes update UI with graphics
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public async void Geolocal_Update(Geoposition in_position)
        {
            try
            {
                if (!userHasTurnedGPSOff)
                {
                    if (_progressRingActive)
                    {
                        StopLocationRing();
                    }
                    _currentAccuracy = in_position.Coordinate.Accuracy;
                    RaisePropertyChanged("CurrentAccuracy");

                    if (!Double.IsNaN(currentMapView.MapScale))
                    {
                        mapScale = currentMapView.MapScale;
                    }
                    else
                    {
                        mapScale = ApplicationLiterals.defaultMapScale;
                    }
                    

                    //Only move if there is a coordinate
                    if (in_position.Coordinate.Point.Position.Longitude != 0 && in_position.Coordinate.Point.Position.Latitude != 0 && !pauseGraphicRefresh)
                    {
                        //Keep and update coordinates
                        _currentMSGeoposition = in_position;
                        RaisePropertyChanged("CurrentMSGeoposition");

                        _currentLongitude = in_position.Coordinate.Point.Position.Longitude;
                        RaisePropertyChanged("CurrentLongitude");
                        _currentLatitude = in_position.Coordinate.Point.Position.Latitude;
                        //_currentEasting = in_position.Coordinate.Point.Position.Latitude;  //CoordinateFormatter.ToUtm(in_position.Coordinate.Point.Position.Latitude, UtmConversionMode.NorthSouthIndicators, true);
                        //_currentNorthing = in_position.Coordinate.Point.Position.Latitude;
                        RaisePropertyChanged("CurrentLatitude");
                        _currentAltitude = in_position.Coordinate.Point.Position.Altitude;
                        RaisePropertyChanged("CurrentAltitude");

                        //added by jamel
                        double[] eastingNorthing = ConvertLatLongToEastingNorthing(_currentLatitude, _currentLongitude);
                        _currentEasting = eastingNorthing[0];
                        _currentNorthing = eastingNorthing[1];
                        RaisePropertyChanged("CurrentNorthing");
                        RaisePropertyChanged("CurrentEasting");

                        //Reset view on current location
                        //bool settingViewPoint = await currentMapView.SetViewpointAsync(new Viewpoint(_currentLatitude, _currentLongitude, mapScale), TimeSpan.FromSeconds(0.75));
                    }
                    //else
                    //{
                    //    //Set non-sense accuracy to get a wide circle
                    //    _currentAccuracy = 0;
                    //    RaisePropertyChanged("CurrentAccuracy");
                    //}

                    if (!pauseGraphicRefresh)
                    {
                        //Clear current graphics
                        if (_OverlayCurrentPosition == null)
                        {
                            _OverlayCurrentPosition = new GraphicsOverlay();
                        }
                        else
                        {
                            _OverlayCurrentPosition.Graphics.Clear();
                        }

                        if (!currentMapView.GraphicsOverlays.Contains(_OverlayCurrentPosition))
                        {
                            currentMapView.GraphicsOverlays.Add(_OverlayCurrentPosition);
                        }

                        _centerPoint = new MapPoint(_currentLongitude, _currentLatitude, _currentAccuracy, SpatialReferences.Wgs84); //Can't just update geometry

                        //Build current accuracy graphic
                        Graphic _accGraphic = GetAccuracyGraphic();

                        //Build current position graphic
                        _posSym.Color = _accuracyColor;
                        var _posGraphic = new Graphic(
                            _centerPoint,
                            _posSym);
                        _posGraphic.Attributes.Add(attributeID, attributeIDPosition);

                        //Update graphic collection and UI
                        _OverlayCurrentPosition.Graphics.Add(_posGraphic);
                        _OverlayCurrentPosition.Graphics.Add(_accGraphic);
                        //currentMapView.Focus(FocusState.Unfocused);
                        //currentMapView.UpdateLayout();
                    }



                }
                else
                {
                    ResetLocationGraphic();
                }
            }
            catch (Exception ex)
            {
                //If anything happens to the event while being called
                string exMessage = ex.Message;
                ResetLocationGraphic();
            }
        }

        /// <summary>
        /// Event to delete all layers
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingsPageViewModel_deleteAllLayers(object sender, EventArgs e)
        {
            DeleteLayersAsync(false);
            RefreshMap(true);
         
        }

        /// <summary>
        /// Events to toggle map page structure symbols
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void SettingsPartViewModel_settingUseStructureSymbols(object sender, EventArgs e)
        {
            //Force refresh of all graphics
            RefreshMap(true);
        }

        public void MapRecenter_Tapped(object sender, TappedRoutedEventArgs e)
        {
            currentMapView.SetViewpointAsync(new Viewpoint(_currentLatitude, _currentLongitude, currentMapView.MapScale), TimeSpan.FromSeconds(0.75));
        }

        private async void FieldBooksPageViewModel_newFieldBookSelectedAsync(object sender, string e)
        {
            DeleteLayersAsync(false);
            RefreshMap(true);
        }

        /// <summary>
        /// Will reload all layers when new one are detected as added.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ShellViewModel_newDataLoaded(object sender, EventArgs e)
        {

            //Add tpks
            Task loadingUserLayers = AddAllLayers();
            await loadingUserLayers;
        }

        /// <summary>
        /// Display information in a popup window when user clicks on feature location
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void myMapView_IdentifyFeature(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            //Get mouse position
            Windows.Foundation.Point screenPoint = e.GetPosition(currentMapView);

            //Reset selections
            _OverlayStation.ClearSelection();
            _OverlayStationLabel.ClearSelection(); //Clear label selection, useless
            _OverlayStructure.ClearSelection();
            selectedStationDate = string.Empty;
            selectedStationID = string.Empty;

            // identify graphics overlay using the point tapped
            List<IdentifyGraphicsOverlayResult> overlays = new List<IdentifyGraphicsOverlayResult>(); //Container that will hold multiple graphic overlay identify query results (there might be multiple point on top of each others)
            IdentifyGraphicsOverlayResult resultGraphics = await currentMapView.IdentifyGraphicsOverlayAsync(_OverlayStation, screenPoint, 25, false);
            overlays.Add(resultGraphics);

            //make sure something is selected else try with other overlay
            if (overlays[0].Graphics.Count == 0)
            {
                //Try with structure
                IdentifyGraphicsOverlayResult resultStructGraphics = await currentMapView.IdentifyGraphicsOverlayAsync(_OverlayStructure, screenPoint, 25, false);
                overlays.Add(resultStructGraphics);

                //If still nothing try with another one
                if (overlays[0].Graphics.Count == 0)
                {
                    foreach (KeyValuePair<string, List<GraphicsOverlay>> go in _overlayContainerOther)
                    {
                        foreach (GraphicsOverlay gogo in go.Value)
                        {
                            resultGraphics = await currentMapView.IdentifyGraphicsOverlayAsync(gogo, screenPoint, 25, false);
                            overlays.Add(resultGraphics);
                            gogo.ClearSelection();
                        }

                    }
                }


            }

            // show attribute information for the first graphic identified (if any)
            foreach (IdentifyGraphicsOverlayResult io in overlays)
            {
                //Sample sampleModel = new Sample();

                if (io != null && io.Graphics.Count > 0)
                {
                    var idGraphic = io.Graphics.FirstOrDefault();
                    idGraphic.IsSelected = true;

                    if (idGraphic.Attributes.Keys.Count > 0)
                    {
                        //ResourceLoader local = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();

                        string graphicId = idGraphic.Attributes["Id"].ToString();

                        //Show dialog with possibility to navigate to report if it is from default/working database
                        if (Convert.ToBoolean(idGraphic.Attributes["Default"].ToString()))
                        {

                            //Detect station or structure identify
                            if (idGraphic.Attributes["tableType"].ToString() == DatabaseLiterals.TableStation)
                            {
                                ShowIdentifyStationDialog(idGraphic.Attributes["Id"].ToString(), idGraphic.Attributes["Date"].ToString(), sender);
                            }

                            if (idGraphic.Attributes["tableType"].ToString() == DatabaseLiterals.TableStructure)
                            {
                                ShowIdentifyStructureDialog(idGraphic.Attributes["Id"].ToString(), idGraphic.Attributes["Date"].ToString(), idGraphic.Attributes["Azim"].ToString(),
                                    idGraphic.Attributes["Dip"].ToString(), idGraphic.Attributes["StructureClass"].ToString(), idGraphic.Attributes["ParentID"].ToString(), sender);
                            }
 
                        }
                        else
                        {
                            ContentDialog tapStationDialog = new ContentDialog()
                            {
                                Title = String.Format("Station {0}", idGraphic.Attributes["Id"].ToString()),
                                Content = String.Format("Date {0} \nTime {1} \nType {2}", idGraphic.Attributes["Date"].ToString(), idGraphic.Attributes["Time"].ToString(), idGraphic.Attributes["Type"].ToString()),
                                PrimaryButtonText = local.GetString("MapPageDialogTextClose")
                            };

                            ContentDialogResult cdr = await Services.ContentDialogMaker.CreateContentDialogAsync(tapStationDialog, true).Result;

                        }

                    }
                }

            }


        }

        /// <summary>
        /// Pop-up dialog to show identify attributes on structure symbols points
        /// </summary>
        /// <param name="graphicID"></param>
        /// <param name="graphicDate"></param>
        /// <param name="sender"></param>
        public async void ShowIdentifyStructureDialog(string graphicID, string graphicDate, 
            string azim, string dipPlunge, string stationId, string graphicClass, object sender)
        {
            ContentDialog tapStationDialog = new ContentDialog()
            {
                Title = local.GetString("MapPageIdentifyStructureDialogTitle"),
                Content = String.Format("{0}  {1} " +
                "\n" + local.GetString("PflowDialogClass/Header") + "({2})" + 
                "\n" + local.GetString("StructureDialogAzim/Header") + "({3})" +
                "\n" + local.GetString("StructureDialogDip/Header") + "({4})", 
                graphicID,
                graphicDate,
                graphicClass,
                azim,
                dipPlunge
                ),
                PrimaryButtonText = local.GetString("MapPageDialogTextReport"),
                SecondaryButtonText = local.GetString("MapPageDialogTextClose")
            };

            ContentDialogResult cdr = await Services.ContentDialogMaker.CreateContentDialogAsync(tapStationDialog, true).Result;
            if (cdr == ContentDialogResult.Primary)
            {
                selectedStationID = stationId;
                selectedStationDate = graphicDate;
                NavigateToReport(sender);
            }
        }

        /// <summary>
        /// Pop-up dialog to show identify attributes on station points
        /// </summary>
        /// <param name="graphicID"></param>
        /// <param name="graphicDate"></param>
        /// <param name="sender"></param>
        public async void ShowIdentifyStationDialog(string graphicID, string graphicDate, object sender)
        {
            // Get select station information
            var tupleStation = queryStation(graphicID);
            int stationId = tupleStation.Item1;
            string stationDate = tupleStation.Item2;
            string stationTime = tupleStation.Item3;

            // Get select earthmat information
            var tupleEarthmat = queryEarthmat(stationId);
            string earthmatDetail = tupleEarthmat.Item1;
            int earthmatCount = tupleEarthmat.Item2;
            List<int> earthmatidList = tupleEarthmat.Item3;

            // Get select ma information
            MineralAlteration modelMineralAlteration = new MineralAlteration();
            int maCount = CountChild(stationId.ToString(), Dictionaries.DatabaseLiterals.FieldMineralAlterationRelID, Dictionaries.DatabaseLiterals.TableMineralAlteration, modelMineralAlteration);

            // Get select document information
            Document modelDocument = new Document();
            int documentCount = CountChild(stationId.ToString(), Dictionaries.DatabaseLiterals.FieldDocumentRelatedID, Dictionaries.DatabaseLiterals.TableDocument, modelDocument);

            // getting the count for the children sample, mineral, fossil, structure 
            int sampleTotalCount = 0;
            foreach (int earthmatId in earthmatidList)
            {
                Sample modelSample = new Sample();
                int sampleCount = CountChild(earthmatId.ToString(), Dictionaries.DatabaseLiterals.FieldEarthMatID, Dictionaries.DatabaseLiterals.TableSample, modelSample);
                sampleTotalCount += sampleCount;
            }

            int structureTotalCount = 0;
            foreach (int earthmatId in earthmatidList)
            {
                Structure modelStructure = new Structure();
                int structureCount = CountChild(earthmatId.ToString(), Dictionaries.DatabaseLiterals.FieldEarthMatID, Dictionaries.DatabaseLiterals.TableStructure, modelStructure);
                structureTotalCount += structureCount;
            }

            int mineralTotalCount = 0;
            foreach (int earthmatId in earthmatidList)
            {
                Mineral modelMineral = new Mineral();
                int mineralCount = CountChild(earthmatId.ToString(), Dictionaries.DatabaseLiterals.FieldEarthMatID, Dictionaries.DatabaseLiterals.TableMineral, modelMineral);
                mineralTotalCount += mineralCount;
            }

            int fossilTotalCount = 0;
            foreach (int earthmatId in earthmatidList)
            {
                Fossil modelFossil = new Fossil();
                int fossilCount = CountChild(earthmatId.ToString(), Dictionaries.DatabaseLiterals.FieldEarthMatID, Dictionaries.DatabaseLiterals.TableFossil, modelFossil);
                fossilTotalCount += fossilCount;
            }

            ContentDialog tapStationDialog = new ContentDialog()
            {
                Title = local.GetString("MapPageIdentifyStationDialogTitle"),
                Content = String.Format("{10}  {0}  {1}" +
                "\n" + local.GetString("EarthDialogHeader/Text") + "({2}) {3}" +
                "\n" + local.GetString("ReportPageMineralAltNameHeader/Text") + "({4})" +
                "\n" + local.GetString("MapPagePhotoCommand/Label") + "({5})" +
                "\n" + local.GetString("FieldworkTableSample/Text") + "({6})" +
                "\n" + local.GetString("FieldworkTableStructure/Text") + "({7})" +
                "\n" + local.GetString("FieldworkTableMineral/Text") + "({8})" +
                "\n" + local.GetString("FieldworkTableFossil/Text") + "({9})",
                stationDate,
                stationTime,
                earthmatCount.ToString(),
                earthmatDetail,
                maCount.ToString(),
                documentCount.ToString(),
                sampleTotalCount.ToString(),
                structureTotalCount.ToString(),
                mineralTotalCount.ToString(),
                fossilTotalCount.ToString(),
                graphicID),
                PrimaryButtonText = local.GetString("MapPageDialogTextReport"),
                SecondaryButtonText = local.GetString("MapPageDialogTextClose")
            };

            ContentDialogResult cdr = await Services.ContentDialogMaker.CreateContentDialogAsync(tapStationDialog, true).Result;
            if (cdr == ContentDialogResult.Primary)
            {
                selectedStationID = graphicID;
                selectedStationDate = graphicDate;
                NavigateToReport(sender);
            }
        }

        /// <summary>
        /// Allows user to enter location by tapping on the map
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void myMapView_AddByTap(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Windows.Foundation.Point screenPoint = e.GetPosition(currentMapView);

            if (!currentMapView.LocationDisplay.IsEnabled)
            {
                MapPoint mapPoint = currentMapView.ScreenToLocation(screenPoint);

                //Convert projected to geographic if needed
                if (mapPoint.SpatialReference.IsProjected)
                {
                    SpatialReference geographicSpatialReference = SpatialReference.Create(4326);
                    mapPoint = (MapPoint)GeometryEngine.Project(mapPoint, geographicSpatialReference);
                }

                DD2DMS dmsLongitude = DD2DMS.FromDouble(mapPoint.X);
                DD2DMS dmsLatitude = DD2DMS.FromDouble(mapPoint.Y);

                //ResourceLoader local = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
                ContentDialog tapDialog = new ContentDialog()
                {
                    Title = local.GetString("MapPageDialogTapCoordinateTitle"),
                    Content = string.Format("{0} {1}  ({2})\n{3} {4}  ({5}) \n\n {6}", "Longitude:", dmsLongitude.ToString("WE"), mapPoint.X.ToString(), "Latitude:", dmsLatitude.ToString("NS"), mapPoint.Y.ToString(), local.GetString("MapPageDialogTextTapForNewStation")),
                    PrimaryButtonText = local.GetString("MapPageDialogTextYes"),
                    SecondaryButtonText = local.GetString("MapPageDialogTextNo"),
                };

                ContentDialogResult cdr = await Services.ContentDialogMaker.CreateContentDialogAsync(tapDialog, true).Result;

                if (cdr == ContentDialogResult.Primary)
                {
                    FieldLocation tapLocation = new FieldLocation
                    {
                        LocationElev = mapPoint.Z,
                        LocationLat = mapPoint.Y,
                        LocationLong = mapPoint.X,
                        LocationErrorMeasure = 9999,
                        LocationElevMethod = Dictionaries.DatabaseLiterals.DefaultNoData,
                        LocationEntryType = vocabEntryTypeTap,
                        LocationErrorMeasureType = Dictionaries.DatabaseLiterals.DefaultNoData
                    };

                    GotoQuickDialog(tapLocation);
                }

            }
            // spw2017
            currentMapView.Tapped -= myMapView_AddByTap;

        }

        /// <summary>
        /// Will finalize the flyout closing by saving order of layers
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void LayerFlyout_ClosedAsync(object sender, object e)
        {
            await SetLayerOrderAsync();
        }

        #endregion

        #region METHODS

        //Added by Jamel
        public static double[] ConvertLatLongToEastingNorthing(double lat, double lng)
        {

            //Determine zone
            int utmzone = (int)((lng - -186.0) / 6.0);

            // Define the source and target coordinate systems
            ICoordinateSystem epsg4326 = GeographicCoordinateSystem.WGS84;
            ICoordinateSystem epsg27700 = ProjectedCoordinateSystem.WGS84_UTM(utmzone, true);

            // Create the coordinate transformation
            CoordinateTransformationFactory ctFact = new CoordinateTransformationFactory();
            ICoordinateTransformation transformation = ctFact.CreateFromCoordinateSystems(epsg4326, epsg27700);

            // Create the source and target coordinate points
            double[] srcPoint = new double[] { lng, lat };
            double[] targetPoint = transformation.MathTransform.Transform(srcPoint);

            return targetPoint;
        }
        private Tuple<int, string, string> queryStation(string id)
        {
            Station stationModel = new Station();
            int stationId = 0;
            string stationDate = "";
            string stationTime = "";

            string stationQuerySelect = "SELECT * FROM " + Dictionaries.DatabaseLiterals.TableStation;
            string stationQueryWhere = " WHERE STATIONNAME " + " = '" + id + "'";
            string stationFinalQuery = stationQuerySelect + stationQueryWhere;

            List<object> stationResults = accessData.ReadTable(stationModel.GetType(), stationFinalQuery);
            IEnumerable<Station> stationTable = stationResults.Cast<Station>();
            if (stationTable.Count() != 0)
            {
                foreach (Station station in stationTable)
                {
                    stationId = station.StationID;
                    stationDate = station.StationVisitDate.ToString();
                    stationTime = station.StationVisitTime.ToString();
                }
            }

            return new Tuple<int, string, string>(stationId, stationDate, stationTime);
        }

        private Tuple<string, int, List<int>> queryEarthmat(int id)
        {
            EarthMaterial earthmatModel = new EarthMaterial();
            string earthmatDetail = "";

            string earthmatQuerySelect = "SELECT * FROM " + Dictionaries.DatabaseLiterals.TableEarthMat;
            string earthmatQueryWhere = " WHERE STATIONID " + " = " + id.ToString();
            string earthmatQueryFinal = earthmatQuerySelect + earthmatQueryWhere;

            List<object> earthmatResults = accessData.ReadTable(earthmatModel.GetType(), earthmatQueryFinal);
            IEnumerable<EarthMaterial> earthmatTable = earthmatResults.Cast<EarthMaterial>();
            int earthmatCount = earthmatTable.Count();

            List<int> earthmatidList = new List<int>();
            string tempMaterial = "";
            if (earthmatTable.Count() != 0)
            {
                foreach (EarthMaterial earth in earthmatTable)
                {
                    earthmatidList.Add(earth.EarthMatID);
                    if (earth.EarthMatLithdetail == null)
                    {
                        tempMaterial = "tbd";
                    }
                    else
                    {
                        tempMaterial = earth.EarthMatLithdetail.ToString();
                    }

                    earthmatDetail += tempMaterial + " ";
                }
            }

            return new Tuple<string, int, List<int>>(earthmatDetail, earthmatCount, earthmatidList);
        }

        //TODO 215
        private int CountChild(string idValue, string fieldName, string tableName, object modelType)
        {
            string querySelect = "SELECT * FROM " + tableName;
            string queryWhere = " WHERE " + fieldName + " = '" + idValue + "'";
            string queryFinal = querySelect + queryWhere;

            List<object> results = accessData.ReadTable(modelType.GetType(), queryFinal);
            int count = results.Count();

            return count;
        }


        /// <summary>
        /// Will save the current layer settings into a JSON file inside the local folder
        /// </summary>
        /// <returns></returns>
        public void SaveLayerRendering()
        {
            //Before saving, clean _filenameValues
            List<int> indexToRemove = new List<int>();
            foreach (MapPageLayers mpl in _filenameValues)
            {
                if (mpl.LayerName is null || mpl.LayerName == "")
                {
                    indexToRemove.Add(_filenameValues.IndexOf(mpl));
                }
            }
            foreach (int ids in indexToRemove)
            {
                _filenameValues.RemoveAt(ids);
            }

            string JSONResult = JsonConvert.SerializeObject(_filenameValues);
            string JSONPath = Path.Combine(accessData.ProjectPath, "mapPageLayer.json");
            if (File.Exists(JSONPath))
            {
                try
                {
                    File.Delete(JSONPath);
                }
                catch (Exception)
                {
                    Debug.WriteLine("Could not delete mapPageLayer.json");
                }
                
            }

            using (var jayson = new StreamWriter(JSONPath, true))
            {
                jayson.WriteLine(JSONResult.ToString());
                jayson.Close();
            }


        }

        /// <summary>
        /// Will save current map view object into a serialized json
        /// </summary>
        public void SaveMapViewObjectToJSON(object inObject)
        {
            var resolver = new IgnorePropertiesResolver(new[] { "AllLayers" });


            //string JSONResult = JsonConvert.SerializeObject(esriMap.ToJson());
            string JSONResult = esriMap.ToJson();
            string JSONPath = Path.Combine(accessData.ProjectPath, "currentMapView_" + String.Format("{0:yyyyMMdd_HH'h'mm}", DateTime.Now) + ".json");
            if (File.Exists(JSONPath))
            {
                File.Delete(JSONPath);
            }

            using (var jayson = new StreamWriter(JSONPath, true))
            {
                jayson.WriteLine(JSONResult.ToString());
                jayson.Close();
            }
        }


        /// <summary>
        /// Will set the gps mode icon from tap to gps activated symbols
        /// </summary>
        /// <param name="inSymbol"></param>
        public void SetGPSModeIcon(Symbol inSymbol = Symbol.Target)
        {
            _GPSModeSymbol = inSymbol;
            RaisePropertyChanged("GPSModeSymbol");

        }

        public async void ResetLocationGraphic()
        {
            try
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    //Clear map overlay
                    if (_OverlayCurrentPosition != null)
                    {
                        _OverlayCurrentPosition.Graphics.Clear();

                        _currentLongitude = 0.0;
                        RaisePropertyChanged("CurrentLongitude");
                        _currentLatitude = 0.0;
                        RaisePropertyChanged("CurrentLatitude");
                        _currentAltitude = 0.0;
                        RaisePropertyChanged("CurrentAltitude");
                        _currentAccuracy = 0.0;
                        RaisePropertyChanged("CurrentAccuracy");
                        currentMapView.UpdateLayout();
                    }


                }).AsTask();

            }
            catch (Exception)
            {

            }

        }

        /// <summary>
        /// Will set the position graphic so user can see where he is located.
        /// </summary>
        public void CreatePositionGraphic()
        {
            //SimpleMarkerSymbol posSym = new SimpleMarkerSymbol();
            _posSym.Color = System.Drawing.Color.FromArgb(0, 122, 194);
            _posSym.Size = 23.0;
            SimpleLineSymbol posLineSym = new SimpleLineSymbol
            {
                Color = System.Drawing.Color.White,
                Width = 2
            };
            _posSym.Outline = posLineSym;
        }

        /// <summary>
        /// Will set the position graphic so user can see where he is located.
        /// </summary>
        public Graphic GetAccuracyGraphic(int numberOfVertices = 36)
        {
            //Init symbols
            Windows.UI.Color posColor = (Windows.UI.Color)Application.Current.Resources["PositionColor"];
            int defaultAlpha = 30;

            //Parse accuracy to change color
            if (_currentAccuracy > 20 && _currentAccuracy <= 40)
            {
                posColor = (Windows.UI.Color)Application.Current.Resources["WarningColor"];
                defaultAlpha = 50;

            }
            else if (_currentAccuracy > 40)
            {
                posColor = (Windows.UI.Color)Application.Current.Resources["ErrorColor"];
                defaultAlpha = 75;
            }
            else if (_currentAccuracy == 0)
            {
                posColor = (Windows.UI.Color)Application.Current.Resources["ErrorColor"];
                defaultAlpha = 75;
                _currentAccuracy = 1000; //Maximum accuracy for invalid position
            }

            //Finalize symbols
            _accuracyColor = System.Drawing.Color.FromArgb(posColor.R, posColor.G, posColor.B);
            _accSym.Color = System.Drawing.Color.FromArgb(defaultAlpha, posColor.R, posColor.G, posColor.B);
            _accLineSym.Color = _accuracyColor;
            _accLineSym.Width = 2;
            _accSym.Outline = _accLineSym;

            //Set graphic
            Graphic _accGraphic = new Graphic()
            {
                Geometry = GetAccuracyPolygon(numberOfVertices),
                Symbol = _accSym
            };
            _accGraphic.Attributes.Add(attributeID, attributeIDAccuracy);

            return _accGraphic;
        }

        /// <summary>
        /// Will set the position graphic so user can see where he is located.
        /// </summary>
        public Polygon GetAccuracyPolygon(int numberOfVertices)
        {
            _projectedCenterPoint = (MapPoint)Esri.ArcGISRuntime.Geometry.GeometryEngine.Project(_centerPoint, SpatialReferences.WebMercator);

            PolygonBuilder polyBuilder = new PolygonBuilder(SpatialReferences.WebMercator);

            List<MapPoint> polyPoints = new List<MapPoint>();
            int degreeInternalIteration = 0;
            int degreeInterval = Convert.ToInt16(Math.Floor(360.0 / numberOfVertices));
            for (int v = 0; v < numberOfVertices; v++)
            {
                double newX = _projectedCenterPoint.X + (_currentAccuracy * Math.Cos(degreeInternalIteration * Math.PI / 180));
                double newY = _projectedCenterPoint.Y + (_currentAccuracy * Math.Sin(degreeInternalIteration * Math.PI / 180));

                polyPoints.Add(new MapPoint(newX, newY));

                degreeInternalIteration = degreeInternalIteration + degreeInterval;
            }
            polyBuilder.AddPart(polyPoints);

            Polygon projectedPoly = (Polygon)Esri.ArcGISRuntime.Geometry.GeometryEngine.Project(polyBuilder.ToGeometry(), SpatialReferences.Wgs84);

            return projectedPoly;
        }

        /// <summary>
        /// Will convert an hex color to a rgb one.
        /// </summary>
        /// <param name="hexColor"></param>
        /// <returns></returns>
        public System.Drawing.Color HexToColor(string hexColor)
        {
            //Remove # if present
            if (hexColor.IndexOf('#') != -1)
                hexColor = hexColor.Replace("#", "");

            int red = 0;
            int green = 0;
            int blue = 0;

            if (hexColor.Length == 6)
            {
                //#RRGGBB
                red = int.Parse(hexColor.Substring(0, 2), NumberStyles.AllowHexSpecifier);
                green = int.Parse(hexColor.Substring(2, 2), NumberStyles.AllowHexSpecifier);
                blue = int.Parse(hexColor.Substring(4, 2), NumberStyles.AllowHexSpecifier);
            }
            else if (hexColor.Length == 3)
            {
                //#RGB
                red = int.Parse(hexColor[0].ToString() + hexColor[0].ToString(), NumberStyles.AllowHexSpecifier);
                green = int.Parse(hexColor[1].ToString() + hexColor[1].ToString(), NumberStyles.AllowHexSpecifier);
                blue = int.Parse(hexColor[2].ToString() + hexColor[2].ToString(), NumberStyles.AllowHexSpecifier);
            }

            return System.Drawing.Color.FromArgb(red, green, blue);
        }

        /// <summary>
        /// Will clear saved settings. To be used when creating and switching field books.
        /// </summary>
        public void ClearMapViewSettings()
        {
            _filenameValues.Clear();
            RaisePropertyChanged("FilenameValues");
            localSettings.WipeUserMapSettings();
        }

        /// <summary>
        /// Will load all layers, embedded and user loaded one to the map view
        /// Will also set some delete properties for the layers
        /// Will do a reset of the file list. Intended for app start-up
        /// </summary>
        /// <returns></returns>
        public async Task AddAllLayers()
        {

            _filenameValues.Clear(); //Reset flyout menu list of files

            bool foundAnyFiles = false;

            //Check for user layers loaded tpks
            Task<bool> loadUserLayers = AddUserLayers();
            if (await loadUserLayers)
            {
                foundAnyFiles = true;
                //SaveMapViewSettings();
            }

            //If nothing is found ask user to load data
            if (!foundAnyFiles)
            {
                _noMapsWatermark = true;
                RaisePropertyChanged("NoMapsWatermark");

                if (!addDataDialogPopedUp)
                {
                    await LoadingData();
                }

            }
            else
            {
                _noMapsWatermark = false;
                RaisePropertyChanged("NoMapsWatermark");
            }

            await SetLayerOrderAsync();
        }

        /// <summary>
        /// Will load all or given file path for user layers, which were added to the 
        /// local state folder
        /// </summary>
        /// <param name="filePath"></param>
        public async Task<bool> AddUserLayers()
        {
            bool foundLayers = false;

            #region  Get list of layer names to prevent from adding multiple of the same one.
            List<string> mapLayers = new List<string>();

            if (esriMap != null)
            {
                foreach (var layer in esriMap.AllLayers)
                {
                    if (!mapLayers.Contains(layer.Name))
                    {
                        mapLayers.Add(layer.Name);
                        foundLayers = true;
                    }


                }
            }
            else
            {
                foundLayers = false;
            }


            #endregion

            #region Get list of all files from local folder and filter them by needed extensions
            StorageFile jsonRenderingFile = null; //Will hold rendering configs
            Dictionary<string, StorageFile> tpkList = new Dictionary<string, StorageFile>(); //Will hold tpks list
            Dictionary<string, StorageFile> sqliteList = new Dictionary<string, StorageFile>(); //Will hold sqlite liste
            StorageFolder localFolder = await StorageFolder.GetFolderFromPathAsync(accessData.ProjectPath);
            IReadOnlyList<StorageFile> _readOnlyfileList = await localFolder.GetFilesAsync();
            //List<StorageFile> fileList = new List<StorageFile>();
            foreach (StorageFile sf in _readOnlyfileList)
            {
                string fileName = sf.Name;
                if (fileName.Contains(".json") && !fileName.Contains(".bak"))
                {
                    jsonRenderingFile = sf;
                }
                else if (fileName.Contains(".tpk"))
                {
                    tpkList[sf.Name] = sf;
                }
                else if (fileName.Contains(DatabaseLiterals.DBTypeSqlite) && !fileName.Contains(DatabaseLiterals.DBName))
                {
                    sqliteList[sf.Name] = sf;
                }
                else if (fileName.Contains(DatabaseLiterals.DBTypeSqliteDeprecated) && !fileName.Contains(DatabaseLiterals.DBName))
                {
                    sqliteList[sf.Name] = sf;
                }
                else if (fileName.Contains(".mmpk"))
                {
                    ///Deprecated MobileMapPackage.IsDirectReadSupportedAsync by ESRI, commented out for now
                    //MobileMapPackage mobileMapPackage;
                    //bool isDirectReadSupported = await MobileMapPackage.IsDirectReadSupportedAsync(sf.Path);
                    //if (isDirectReadSupported)
                    //{
                    //    mobileMapPackage = await MobileMapPackage.OpenAsync(sf.Path);
                    //}
                    //else
                    //{
                    //    await MobileMapPackage.UnpackAsync(sf.Path, accessData.ProjectPath);
                    //    mobileMapPackage = await MobileMapPackage.OpenAsync(accessData.ProjectPath);
                    //}

                    //if (mobileMapPackage.Maps.Count > 0)
                    //{
                    //    Map mymap = mobileMapPackage.Maps.First();
                    //    currentMapView.Map = mymap;
                    //    //currentMapView.UpdateLayout();

                    //    foreach (Layer item in currentMapView.Map.AllLayers)
                    //    {
                    //        MapPageLayers im = new MapPageLayers();
                    //        im.LayerName = item.Name;
                    //        MapPageLayerSetting mpls = new MapPageLayerSetting();
                    //        mpls.LayerOpacity = item.Opacity * 100;
                    //        mpls.LayerVisibility = item.IsVisible;
                    //        im.LayerSettings = mpls;
                    //        _filenameValues.Add(im);
                    //        RaisePropertyChanged("FilenameValues");
                    //    }
                    //}
                }

            }

            #endregion

            //Load given layer or load all
            if (jsonRenderingFile != null)
            {
                //Prevent null values pooching deserialization
                string jsonRenderingFileString = await Windows.Storage.FileIO.ReadTextAsync(jsonRenderingFile);
                if (!jsonRenderingFileString.Contains("null"))
                {
                    //Deserialize JSON rendering config file.
                    _filenameValues = JsonConvert.DeserializeObject<ObservableCollection<MapPageLayers>>(jsonRenderingFileString);
                    RaisePropertyChanged("FilenameValues");
                }

            }

            //Process saved config in json
            if (_filenameValues != null && _filenameValues.Count > 0)
            {
                foreach (MapPageLayers configs in _filenameValues)
                {
                    if (configs.LayerName != null && configs.LayerName.Contains(".tpk"))
                    {
                        if (tpkList.ContainsKey(configs.LayerName))
                        {
                            bool.TryParse(configs.LayerSettings.LayerVisibility.ToString(), out bool tpkVisibility);
                            Double.TryParse(configs.LayerSettings.LayerOpacity.ToString(), out double tpkSliderSettingOpacity);
                            await AddDataTypeTPK(tpkList[configs.LayerName], tpkVisibility, tpkSliderSettingOpacity / 100.0);
                            tpkList.Remove(configs.LayerName);
                            foundLayers = true;
                        }

                    }
                    else if (configs.LayerName != null && (configs.LayerName.Contains(DatabaseLiterals.DBTypeSqlite)  || configs.LayerName.Contains(DatabaseLiterals.DBTypeSqliteDeprecated)))
                    {
                        if (sqliteList.ContainsKey(configs.LayerName))
                        {
                            bool.TryParse(configs.LayerSettings.LayerVisibility.ToString(), out bool sqlVisibility);
                            Double.TryParse(configs.LayerSettings.LayerOpacity.ToString(), out double sqlSliderSettingOpacity);
                            AddDataTypeSQLite(sqliteList[configs.LayerName], sqlVisibility, sqlSliderSettingOpacity / 100.0);
                            sqliteList.Remove(configs.LayerName);
                            foundLayers = true;
                        }
                    }

                }
            }

            //Init collection of mapPagerLayers if deserialized has failed and thrown a null in there.
            if (_filenameValues == null)
            {
                _filenameValues = new ObservableCollection<MapPageLayers>();
            }
            //Process possible missing layers in json
            if (tpkList.Count > 0)
            {
                foreach (KeyValuePair<string, StorageFile> remainingTpks in tpkList)
                {
                    await AddDataTypeTPK(remainingTpks.Value, true, 1);
                    MapPageLayers mpl = new MapPageLayers
                    {
                        LayerName = remainingTpks.Key
                    };
                    MapPageLayerSetting mpls = new MapPageLayerSetting
                    {
                        LayerOpacity = 100,
                        LayerVisibility = true
                    };
                    mpl.LayerSettings = mpls;
                    _filenameValues.Insert(0,mpl);
                    RaisePropertyChanged("FilenameValues");
                    foundLayers = true;
                }
            }
            if (sqliteList.Count > 0)
            {
                foreach (KeyValuePair<string, StorageFile> remainingSqlite in sqliteList)
                {
                    AddDataTypeSQLite(remainingSqlite.Value, true, 1);
                    MapPageLayers mpl = new MapPageLayers
                    {
                        LayerName = remainingSqlite.Key
                    };
                    MapPageLayerSetting mpls = new MapPageLayerSetting
                    {
                        LayerOpacity = 100,
                        LayerVisibility = true
                    };
                    mpl.LayerSettings = mpls;
                    _filenameValues.Insert(0, mpl);
                    RaisePropertyChanged("FilenameValues");
                    foundLayers = true;
                }
            }


            //If nothing is found ask user to load data
            if (!foundLayers)
            {
                _noMapsWatermark = true;
                RaisePropertyChanged("NoMapsWatermark");

                if (!addDataDialogPopedUp)
                {
                    await LoadingData();
                }
            }
            else
            {
                _noMapsWatermark = false;
                RaisePropertyChanged("NoMapsWatermark");
            }

            //currentMapView.UpdateLayout();
            return foundLayers;

        }

        /// <summary>
        /// Will add a tpk data type to current map content
        /// </summary>
        public async Task AddDataTypeTPK(StorageFile inTPK, bool isTPKVisible = true, double tpkOpacity = 1.0)
        {
            //Get path
            var localUri = new Uri("file:\\" + inTPK.Path);

            //Build tile layer object and load
            ArcGISTiledLayer _tileLayer = new ArcGISTiledLayer(localUri);

            await _tileLayer.LoadAsync();

            //When loaded check if it was already set as visible or not
            if (_tileLayer.LoadStatus == Esri.ArcGISRuntime.LoadStatus.Loaded)
            {
                try
                {

                    if (esriMap == null)
                    {
                        esriMap = new Map(_tileLayer.SpatialReference);
                        esriMap.PropertyChanged += EsriMap_PropertyChanged;
                    }

                    _tileLayer.IsVisible = isTPKVisible;
                    _tileLayer.Opacity = tpkOpacity;
                    esriMap.Basemap.BaseLayers.Add(_tileLayer);

                    //Shows Peojection in map view
                    var txtProjection = _tileLayer.SpatialReference.WkText;
                    var txtDatum = txtProjection.IndexOf("DATUM") + 9;
                    var txtSpheroid = txtProjection.IndexOf("SPHEROID") - 2;
                    _currentProjection = txtProjection.Substring(txtDatum, txtSpheroid - txtDatum);
                    RaisePropertyChanged("CurrentProjection");
                }
                catch (Exception)
                {
                }

                currentMapView.Map = esriMap;
                RaisePropertyChanged("currentMapView");

            }

        }

        /// <summary>
        /// Will add an sqlite database to current map content
        /// </summary>
        /// <returns></returns>
        public void AddDataTypeSQLite(StorageFile inSQLite, bool isVisible = true, double sqlOpacity = 1)
        {
            SQLiteConnection currentConnection = accessData.GetConnectionFromPath(inSQLite.Path);
            List<object> otherLocationTableRows = accessData.ReadTableFromDBConnectionWithoutClosingConnection(locationModel.GetType(), string.Empty, currentConnection);
            if (otherLocationTableRows.Count >= 1)
            {
                #region MANAGER LAYERS

                //Build a list of already loaded stations id on the map
                Dictionary<string, Graphic> loadedOtherGraphicList = new Dictionary<string, Graphic>();
                if (_overlayContainerOther.ContainsKey(inSQLite.Name))
                {
                    foreach (Graphic gro in _overlayContainerOther[inSQLite.Name][0].Graphics)
                    {
                        loadedOtherGraphicList[gro.Attributes[Dictionaries.DatabaseLiterals.FieldLocationID].ToString()] = gro;
                    }
                }
                else
                {
                    List<GraphicsOverlay> relatedGraphics = new List<GraphicsOverlay>();
                    _overlayContainerOther[inSQLite.Name] = relatedGraphics;
                }


                // Add graphics overlay to map view
                for (int i = 0; i < _overlayContainerOther[inSQLite.Name].Count; i++)
                {
                    if (!currentMapView.GraphicsOverlays.Contains(_overlayContainerOther[inSQLite.Name][i]))
                    {
                        _overlayContainerOther[inSQLite.Name][i].Opacity = sqlOpacity;
                        currentMapView.GraphicsOverlays.Add(_overlayContainerOther[inSQLite.Name][i]);
                    }
                }
                    
                LoadFromGivenDB(otherLocationTableRows, currentConnection, loadedOtherGraphicList, false);

                #endregion
            }

            currentConnection.Close();
        }

        /// <summary>
        /// Will set the toggle switch visibility inside the layer and keep it in the current list of files
        /// </summary>
        public void SetLayerVisibility(ToggleSwitch inSwitch)
        {
            if (inSwitch.Header != null)
            {
                SetLayerVisibilityOrOpacity(inSwitch, inSwitch.Header.ToString());

                SaveLayerRendering();
            }


        }

        /// <summary>
        /// Will set the toggle switch visibility inside the layer and keep it in the current list of files
        /// </summary>
        public void SetLayerOpacity(Slider inSlider)
        {
            if (inSlider.Tag != null)
            {
                SetLayerVisibilityOrOpacity(inSlider, inSlider.Tag.ToString());
            }


        }

        /// <summary>
        /// Will set the toggle switch visibility inside the layer and keep it in the current list of files
        /// </summary>
        public void SetLayerVisibilityOrOpacity(object inControl, string layerName)
        {
            if (inControl != null)
            {
                //Cast to toggle for visibility or slider for opacity, either one will be non-null
                ToggleSwitch inSwitch = inControl as ToggleSwitch;
                Slider inSlider = inControl as Slider;

                if (esriMap != null && esriMap.AllLayers.Count > 0 && layerName.Contains(".tpk"))
                {
                    #region TPKs

                    // Find the layer from the map layers and change visibility
                    Layer sublayer = null;
                    try
                    {
                        sublayer = esriMap.AllLayers.First(x => x.Name.Contains(layerName.Split('.')[0]));
                    }
                    catch (Exception)
                    {
                        Debug.WriteLine("TPK not found within json");
                    }

                    if (sublayer != null)
                    {
                        if (inSwitch != null)
                        {
                            sublayer.IsVisible = inSwitch.IsOn;
                        }
                        else if (inSlider != null)
                        {
                            //Convert slider bar values to real opacity ([0,1])
                            sublayer.Opacity = inSlider.Value / 100.0;
                        }

                    }

                    // Find the layer from list of available layers and keep new value
                    MapPageLayers subFile = _filenameValues.First(x => x.LayerName == layerName);
                    if (subFile != null)
                    {

                        if (inSwitch != null)
                        {
                            subFile.LayerSettings.LayerVisibility = inSwitch.IsOn;
                        }
                        else if (inSlider != null)
                        {
                            subFile.LayerSettings.LayerOpacity = inSlider.Value;
                        }

                    }
                    #endregion
                }

                if (esriMap != null && esriMap.AllLayers.Count > 0 && (layerName.Contains(DatabaseLiterals.DBTypeSqlite) || layerName.Contains(DatabaseLiterals.DBTypeSqliteDeprecated)))
                {

                    #region OVERLAYS
                    // Find the layer from the map layers and change visibility
                    if (_overlayContainerOther.ContainsKey(layerName))
                    {
                        if (inSwitch != null)
                        {
                            _overlayContainerOther[layerName][0].IsVisible = inSwitch.IsOn;
                            _overlayContainerOther[layerName][1].IsVisible = inSwitch.IsOn;
                        }
                        else if (inSlider != null)
                        {
                            //Convert slider bar values to real opacity ([0,1])
                            _overlayContainerOther[layerName][0].Opacity = inSlider.Value / 100.0;
                            _overlayContainerOther[layerName][1].Opacity = inSlider.Value / 100.0;
                        }

                    }

                    // Find the layer from list of available layers and keep new value
                    MapPageLayers subFile = _filenameValues.First(x => x.LayerName == layerName);
                    if (subFile != null)
                    {

                        if (inSwitch != null)
                        {
                            subFile.LayerSettings.LayerVisibility = inSwitch.IsOn;
                        }
                        else if (inSlider != null)
                        {
                            subFile.LayerSettings.LayerOpacity = inSlider.Value;
                        }

                    }
                    #endregion

                }

            }


        }

        /// <summary>
        /// Will set the maps (layers) order in the map control from user choices.
        /// </summary>
        public async Task SetLayerOrderAsync()
        {
            try
            {
                //Change order if it has changed only
                List<MapPageLayers> fileCopy = _filenameValues.ToList();
                fileCopy.Reverse();

                if (esriMap != null)
                {

                    //Keep original layer collection
                    LayerCollection currentLayerCollection = esriMap.Basemap.BaseLayers;

                    Dictionary<string, Layer> layerDico = new Dictionary<string, Layer>();
                    foreach (Layer cl in currentLayerCollection)
                    {
                        layerDico[cl.Name] = cl;
                    }

                    esriMap.Basemap.BaseLayers.Clear();

                    //bool firstIteration = true;
                    ObservableCollection<MapPageLayers> newFileList = new ObservableCollection<MapPageLayers>();
                    foreach (MapPageLayers orderedFiles in _filenameValues.Reverse()) //Reverse order while iteration because UI is reversed intentionnaly
                    {
                        if (orderedFiles.LayerName.Contains(".tpk") || (orderedFiles.LayerName.Contains(DatabaseLiterals.DBTypeSqlite) && !orderedFiles.LayerName.Contains(DatabaseLiterals.DBName)) || (orderedFiles.LayerName.Contains(DatabaseLiterals.DBTypeSqliteDeprecated) && !orderedFiles.LayerName.Contains(DatabaseLiterals.DBName)) )
                        {
                            //Build path
                            string localFilePath = Path.Combine(accessData.ProjectPath, orderedFiles.LayerName);
                            Uri localUri = new Uri(localFilePath); 

                            //if (firstIteration)
                            //{
                            //    Layer firstLayer = layerDico.First().Value;
                            //    AddBlanckFeature(firstLayer.SpatialReference);
                            //    firstIteration = false;
                            //}

                            if (layerDico.ContainsKey(orderedFiles.LayerName.Split('.')[0]))
                            {
                                Layer layerToAdd = layerDico[orderedFiles.LayerName.Split('.')[0]];

                                try
                                {
                                    orderedFiles.LayerSettings.LayerVisibility = layerToAdd.IsVisible;
                                    orderedFiles.LayerSettings.LayerOpacity = layerToAdd.Opacity * 100;

                                    //Make sure to push the change to the UI in case this is coming from a first app opening
                                    newFileList.Insert(0, orderedFiles); //Save in new list because can't change something being looped


                                }
                                catch (Exception)
                                {
                                }

                                if (orderedFiles.LayerName.Contains(".tpk"))
                                {
                                    esriMap.Basemap.BaseLayers.Add(layerToAdd);
                                }
                                
                            }




                        }

                    }

                    //Update UI
                    if (newFileList != null && newFileList.Count != 0 && newFileList.Count == _filenameValues.Count)
                    {
                        _filenameValues = newFileList;
                    }

                    RaisePropertyChanged("FilenameValues");

                }
            }
            catch (Exception e)
            {
                ContentDialog tapDialog = new ContentDialog()
                {
                    Title = "Error",
                    Content = e.Message + e.StackTrace,
                    PrimaryButtonText = "Yes",
                    SecondaryButtonText = "No"
                };
                tapDialog.Style = (Style)Application.Current.Resources["DeleteDialog"];
                ContentDialogResult cdr = await Services.ContentDialogMaker.CreateContentDialogAsync(tapDialog, true).Result;
            }


        }

        /// <summary>
        /// Will clear all layers, tpks and sqlite (loaded as overlays)
        /// </summary>
        public void ClearLayers()
        {
            //Clear map layers
            if (currentMapView != null && currentMapView.Map != null && currentMapView.Map.Basemap != null)
            {
                esriMap.Basemap.BaseLayers.Clear();


                //Clear map overlay
                _overlayContainerOther.Clear();
                currentMapView.GraphicsOverlays.Remove(_OverlayStation);
                currentMapView.GraphicsOverlays.Remove(_OverlayStationLabel);
                currentMapView.GraphicsOverlays.Remove(_OverlayStructure);

                currentMapView.UpdateLayout();
            }
            

        }

        /// <summary>
        /// Will reset select variables
        /// </summary>
        public void ClearSelection()
        {
            //Reset selections
            _OverlayStation.ClearSelection();
            _OverlayStationLabel.ClearSelection(); //Clear label selection, useless
            selectedStationDate = string.Empty;
            selectedStationID = string.Empty;

        }

        /// <summary>
        /// Will ask user to choose from device a tpk file and will add it to the map view.
        /// </summary>
        /// <returns></returns>
        public async Task LoadingData()
        {
            //ResourceLoader local = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            ContentDialog tapDialog = new ContentDialog()
            {
                Title = local.GetString("UserDialogNewDataTitle"),
                Content = local.GetString("UserDialogNewDataContent"),
                PrimaryButtonText = local.GetString("UserDialogNewDataTitleYes"),
                SecondaryButtonText = local.GetString("UserDialogNewDataTitleNo"),
            };

            ContentDialogResult cdr = await Services.ContentDialogMaker.CreateContentDialogAsync(tapDialog, true).Result;

            addDataDialogPopedUp = true;

            if (cdr == ContentDialogResult.Primary)
            {

                StartProgressRing();

                //Get local storage folder
                StorageFolder localFolder = await StorageFolder.GetFolderFromPathAsync(accessData.ProjectPath);

                //Create a file picker for sqlite and tpk datas
                var filesPicker = new Windows.Storage.Pickers.FileOpenPicker
                {
                    SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop
                };
                filesPicker.FileTypeFilter.Add(".tpk");
                filesPicker.FileTypeFilter.Add(DatabaseLiterals.DBTypeSqlite);
                filesPicker.FileTypeFilter.Add(DatabaseLiterals.DBTypeSqliteDeprecated);
                filesPicker.FileTypeFilter.Add(".mmpk");
                //Get users selected files
                IReadOnlyList<StorageFile> files = await filesPicker.PickMultipleFilesAsync();
                if (files.Count > 0)
                {
                    foreach (StorageFile f in files)
                    {
                        await f.CopyAsync(localFolder);
                    }

                    //Show end message
                    ContentDialog addedLayerDialog = new ContentDialog()
                    {
                        Title = local.GetString("LoadDataButtonProcessEndMessageTitle"),
                        Content = local.GetString("LoadDataButtonProcessEndMessage/Text"),
                        PrimaryButtonText = local.GetString("LoadDataButtonProcessEndMessageOk")
                    };

                    ContentDialogResult addDialog = await Services.ContentDialogMaker.CreateContentDialogAsync(addedLayerDialog, true).Result;

                    if (addDialog == ContentDialogResult.Primary)
                    {

                        await AddUserLayers();
                    }



                }

                StopProgressRing();

            }

        }

        /// <summary>
        /// Will start the progress ring - This is a generic method
        /// </summary>
        public void StartProgressRing()
        {
            //Set progress ring
            _progressRingActive = true;
            _progressRingVisibility = true;
            RaisePropertyChanged("MapRingActive");
            RaisePropertyChanged("MapRingVisibility");
        }

        /// <summary>
        /// Will stop the progress ring - This is a generic method
        /// </summary>
        public void StopProgressRing()
        {
            //Set progress ring
            _progressRingActive = false;
            _progressRingVisibility = false;
            RaisePropertyChanged("MapRingActive");
            RaisePropertyChanged("MapRingVisibility");
        }

        /// <summary>
        /// Will start the progress ring and a acquiring location message
        /// </summary>
        public void StartLocationRing()
        {
            if (!_mapRingLabelAcquiringGPSVisibility || !_progressRingVisibility || !_progressRingActive)
            {
                _mapRingLabelAcquiringGPSVisibility = true;
                _progressRingVisibility = true;
                _progressRingActive = true;
                RaisePropertyChanged("MapRingLabelAcquiringGPSVisibility");
                RaisePropertyChanged("MapRingVisibility");
                RaisePropertyChanged("MapRingActive");
                //ResetLocationGraphic();
            }

            initializingGPS = true;
        }

        /// <summary>
        /// Will stop the progress rind and hide the acquiring location message
        /// </summary>
        public void StopLocationRing()
        {
            initializingGPS = false;
            _mapRingLabelAcquiringGPSVisibility = false;
            _progressRingVisibility = false;
            _progressRingActive = false;
            RaisePropertyChanged("MapRingLabelAcquiringGPSVisibility");
            RaisePropertyChanged("MapRingVisibility");
            RaisePropertyChanged("MapRingActive");
        }

        public void SetQuickButtonEnable()
        {
            if (localSettings.GetSettingValue(DatabaseLiterals.TableSample) != null)
            {
                if ((bool)localSettings.GetSettingValue(DatabaseLiterals.TableSample))
                {
                    _mapPageQuickSampleEnable = true;
                }
                else
                {
                    _mapPageQuickSampleEnable = false;
                }

            }

            if (localSettings.GetSettingValue(DatabaseLiterals.TableDocument) != null)
            {
                if ((bool)localSettings.GetSettingValue(DatabaseLiterals.TableDocument))
                {
                    _mapPageQuickPhotoEnable = true;
                }
                else
                {
                    _mapPageQuickPhotoEnable = false;
                }

            }


            //Based on project type
            if (localSettings.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoFWorkType) != null)
            {
                if (localSettings.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoFWorkType).ToString() == Dictionaries.ScienceLiterals.ApplicationThemeBedrock)
                {
                    if (localSettings.GetSettingValue(DatabaseLiterals.TableStructure) != null)
                    {
                        if ((bool)localSettings.GetSettingValue(DatabaseLiterals.TableStructure))
                        {
                            _mapPageQuickMeasurementEnable = true;
                        }
                        else
                        {
                            _mapPageQuickMeasurementEnable = false;
                        }
                    }

                }
                else if (localSettings.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoFWorkType).ToString() == Dictionaries.ScienceLiterals.ApplicationThemeSurficial)
                {
                    if (localSettings.GetSettingValue(DatabaseLiterals.TablePFlow) != null)
                    {
                        if ((bool)localSettings.GetSettingValue(DatabaseLiterals.TablePFlow))
                        {
                            _mapPageQuickMeasurementEnable = true;
                        }
                        else
                        {
                            _mapPageQuickMeasurementEnable = false;
                        }
                    }
                }
            }

        }

        /// <summary>
        /// Will build some vocab list for specific work
        /// </summary>
        /// <param name="withManualLocationEntry"></param>
        public void FillLocationVocab(bool withManualLocationEntry = false)
        {
            string querySelect = "SELECT " + Dictionaries.DatabaseLiterals.FieldDictionaryCode + " ";
            string queryFrom = "FROM " + Dictionaries.DatabaseLiterals.TableDictionary + " ";
            string queryWhere = "WHERE " + Dictionaries.DatabaseLiterals.FieldDictionaryTermID + " = '";

            string queryWhereElevMethodGPS = Dictionaries.DatabaseLiterals.termIDElevmethod_GPS + "'";
            string queryWhereErrorTypeMeter = Dictionaries.DatabaseLiterals.termIDErrorTypeMeasure_Meter + "'";
            string queryWhereEntryTap = Dictionaries.DatabaseLiterals.termIDEntryType_Tap + "'";
            string queryWhereEntryManual = Dictionaries.DatabaseLiterals.termIDEntryType_Manual + "'";

            Vocabularies vocaModel = new Vocabularies();

            try
            {
                string fullQuery = querySelect + queryFrom + queryWhere + queryWhereEntryTap;
                Vocabularies vocabEntryTypeTapVocab = accessData.ReadTable(vocaModel.GetType(), fullQuery)[0] as Vocabularies;
                vocabEntryTypeTap = vocabEntryTypeTapVocab.Code.ToString();

            }
            catch (Exception)
            {
                //TODO in case dictionnary has been updated.
                vocabEntryTypeTap = "tap";

            }

            List<object> elevObjects = accessData.ReadTable(vocaModel.GetType(), querySelect + queryFrom + queryWhere + queryWhereElevMethodGPS);
            if (elevObjects.Count > 0)
            {
                Vocabularies elevGPS = elevObjects[0] as Vocabularies;
                vocabElevmethodGPS = elevGPS.Code.ToString();
            }

            List<object> errorTypeObjects = accessData.ReadTable(vocaModel.GetType(), querySelect + queryFrom + queryWhere + queryWhereErrorTypeMeter);
            if (errorTypeObjects.Count > 0)
            {
                Vocabularies errorType = errorTypeObjects[0] as Vocabularies;
                vocabErrorMeasureTypeMeter = errorType.Code.ToString();
            }

            List<object> manObjects = accessData.ReadTable(vocaModel.GetType(), querySelect + queryFrom + queryWhere + queryWhereEntryManual);
            if (manObjects.Count > 0)
            {
                Vocabularies manualType = manObjects[0] as Vocabularies;
                vocabEntryTypeManual = manualType.Code.ToString();
            }

        }

        #endregion

        #region DELETE
        /// <summary>
        /// Will remove a selected layer from the map but also delete the original file from the local state folder.
        /// Else, will clear the layers from the map so they can be deleted in batch.
        /// </summary>
        public async void DeleteLayersAsync(bool fromSelection)
        {
            if (esriMap != null && _selectedLayer != null && fromSelection)
            {
                // Get selected layer
                MapPageLayers subFile = (MapPageLayers)_selectedLayer;

                if (!subFile.LayerName.Contains("ms-appx"))
                {

                    var local = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
                    ContentDialog deleteLayerDialog = new ContentDialog()
                    {
                        Title = local.GetString("DeleteDialogGenericTitle"),
                        Content = local.GetString("DeleteDialog_MapLayer/Text") + " (" + subFile.LayerName + ").",
                        PrimaryButtonText = local.GetString("Generic_ButtonYes/Content"),
                        SecondaryButtonText = local.GetString("Generic_ButtonNo/Content")
                    };
                    deleteLayerDialog.Style = (Style)Application.Current.Resources["DeleteDialog"];
                    ContentDialogResult dld = await Services.ContentDialogMaker.CreateContentDialogAsync(deleteLayerDialog, true).Result;

                    if (dld == ContentDialogResult.Primary)
                    {
                        if (_selectedLayer != null && _selectedLayer.ToString() != string.Empty)
                        {
                            // Get selected layer
                            MapPageLayers selectedFile = (MapPageLayers)_selectedLayer;
                            string localLayerPath = Path.Combine(accessData.ProjectPath, selectedFile.LayerName);

                            //For tpks
                            if (!selectedFile.LayerName.Contains("sql"))
                            {
                                Layer foundLayer = null;

                                // Find the layer from the image layer
                                foreach (Layer l in esriMap.AllLayers)
                                {
                                    if (l.Name == selectedFile.LayerName.Split('.')[0])
                                    {
                                        foundLayer = l;
                                        break;
                                    }
                                }

                                if (foundLayer != null)
                                {
                                    //Delete file
                                    Services.FileServices.FileServices deleteLayerFile = new Services.FileServices.FileServices();
                                    deleteLayerFile.DeleteLocalStateFile(localLayerPath);

                                    //Remove from map
                                    esriMap.Basemap.BaseLayers.Remove(foundLayer);

                                }

                                //Reset layer and layer flyout
                                _filenameValues.Remove(selectedFile);
                                _selectedLayer = string.Empty;
                                await SetLayerOrderAsync();
                            }

                            else
                            {
                                //Delete file
                                Services.FileServices.FileServices deleteLayerFile = new Services.FileServices.FileServices();
                                deleteLayerFile.DeleteLocalStateFile(localLayerPath);

                                //Reset layer and layer flyout
                                _filenameValues.Remove(selectedFile);
                                _selectedLayer = string.Empty;
                                await SetLayerOrderAsync();

                                //Reset overlays
                                if (_overlayContainerOther.ContainsKey(selectedFile.LayerName))
                                {

                                    currentMapView.GraphicsOverlays.Remove(_overlayContainerOther[selectedFile.LayerName][0]);
                                    currentMapView.GraphicsOverlays.Remove(_overlayContainerOther[selectedFile.LayerName][1]);
                                    _overlayContainerOther.Remove(selectedFile.LayerName);

                                }

                            }

                        }
                    }
                }

            }
            else if (!fromSelection)
            {
                try
                {
                    if (esriMap != null)
                    {
                        
                        esriMap.Basemap.BaseLayers.Clear();
                        esriMap = null;
                    }
                    
                }
                catch (Exception)
                {

                }
            }
        }

        #endregion

        #region ZOOM
        public void ZoomToLayer(bool fromSelection)
        {
            if (esriMap != null && _selectedLayer != null && fromSelection)
            {
                // Get selected layer
                if (_selectedLayer.ToString() == "")
                {
                    //Added to make sure the app does not crash if the user clicks the same zoom button twice
                }
                else
                {

                    MapPageLayers subFile = (MapPageLayers)_selectedLayer;
                    string localLayerPath = Path.Combine(accessData.ProjectPath, subFile.LayerName);

                    Layer foundLayer = null;

                    //For tpks
                    if (!subFile.LayerName.Contains("sql"))
                    {
                        //Layer foundLayer = null;

                        // Find the layer from the image layer
                        foreach (Layer l in esriMap.AllLayers)
                        {
                            if (l.Name == subFile.LayerName.Split('.')[0])
                            {
                                foundLayer = l;
                                break;
                            }
                        }

                        if (foundLayer != null)
                        {

                            //Zoom Extent
                            currentMapView.SetViewpoint(new Viewpoint(foundLayer.FullExtent));
                        }


                    }

                    else
                    {
                        if (_overlayContainerOther.ContainsKey(subFile.LayerName))
                        {
                            //Zoom extent of points within overlay 
                            Envelope sqliteExtent = _overlayContainerOther[subFile.LayerName][0].Extent;

                            if (sqliteExtent != null)
                            {
                                currentMapView.SetViewpoint(new Viewpoint(sqliteExtent));
                            }
                        }

                    }


                }
            }
        }

        #endregion
    }

}