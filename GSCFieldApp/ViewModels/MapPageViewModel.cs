using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml.Controls.Maps;
using Windows.Storage.Streams;
using Windows.Foundation;
using Windows.UI.Popups;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Controls;
using Windows.Storage;
using Windows.UI.Core;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Symbology;
using GSCFieldApp.ViewModels;
using GSCFieldApp.Views;
using GSCFieldApp.Services.DatabaseServices;
using GSCFieldApp.Models;
using GSCFieldApp.Dictionaries;
using Template10.Common;
using Template10.Mvvm;
using Template10.Services.SettingsService;
using Template10.Controls;
using Template10.Services.NavigationService;
using Windows.ApplicationModel.Resources;
using Windows.System;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using SQLite.Net;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime;
using System.Threading;
using Esri.ArcGISRuntime.Location;
using System.Globalization;
using System.Windows.Input;
using Windows.Networking.Connectivity;
using Symbol = Windows.UI.Xaml.Controls.Symbol;
using Newtonsoft.Json;
using Esri.ArcGISRuntime.Portal;


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
        private DataAccess accessData = new DataAccess();
        private DataLocalSettings localSettings = new DataLocalSettings();
        public FieldLocation locationModel = new FieldLocation();

        //Other
        public bool addDataDialogPopedUp = false; //Will be used to stop pop-up launching everytime user navigates to map page.
        public ResourceLoader local = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
        DataLocalSettings localSetting = new DataLocalSettings();
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
        public double _currentAccuracy = 0.0;
        public bool initializingGPS = false;
        public bool _mapRingLabelAcquiringGPSVisibility = false;
        public Symbol _GPSModeSymbol = Symbol.Target;

        //Map Graphics
        private SimpleMarkerSymbol posSym = new SimpleMarkerSymbol();
        public GraphicsOverlay _OverlayStation;
        public GraphicsOverlay _OverlayStationLabel;
        public GraphicsOverlay _OverlayCurrentPosition;
        public Dictionary<string, Tuple<GraphicsOverlay, GraphicsOverlay>> _overlayContainerOther; //Will act as a "layer" container but for graphics, just like esriMap.Basemap.BaseLayers object

        //Delegates and events
        public static event EventHandler newDataLoaded; //This event is triggered when a new data has been loaded

        //Constants
        public string attributeID = "ID";
        public string attributeIDPosition = "Position";
        public string attributeIDAccuracy = "PositionAccuracy";

        public MapPageViewModel()
        {

            //Init
            lastTakenLocation = new Tuple<double, double>(0, 0);

            _OverlayStation = new GraphicsOverlay();
            _OverlayStationLabel = new GraphicsOverlay();
            _overlayContainerOther = new Dictionary<string, Tuple<GraphicsOverlay, GraphicsOverlay>>();
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

            //Set some configs
            SetQuickButtonEnable();

            //Fill vocab 
            FillLocationVocab();

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
        public double CurrentAccuracy { get { return _currentAccuracy; } set { _currentAccuracy = value; } }
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
            ResetLocationGraphic();

            var accessStatus = await Geolocator.RequestAccessAsync();

            switch (accessStatus)
            {
                case GeolocationAccessStatus.Allowed:

                    currentMapView.Tapped -= myMapView_AddByTap;

                    // If DesiredAccuracy or DesiredAccuracyInMeters are not set (or value is 0), DesiredAccuracy.Default is used.
                    _geolocator = new Geolocator { ReportInterval = 750 };

                    // Subscribe to the StatusChanged event to get updates of location status changes.
                    _geolocator.PositionChanged += OnPositionChanged;
                    _geolocator.StatusChanged += Geolocal_StatusChangedAsync;

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
                    ResetLocationGraphic();
                    await NoLocationFlightMode();
                    userHasTurnedGPSOff = true;
                    SetGPSModeIcon(Symbol.TouchPointer);

                    break;

                case PositionStatus.Disabled:
                    await Task.Delay(500);
                    // The permission to access location data is denied by the user or other policies.
                    ResetLocationGraphic();
                    userHasTurnedGPSOff = true;
                    SetGPSModeIcon(Symbol.TouchPointer);

                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High, async () =>
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


                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High, async () =>
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



                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High, async () =>
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


                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High, async () =>
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
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High, async () =>
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
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High, async () =>
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
        public void DisplayPointAndLabelsAsync(MapView inMapView)
        {

            #region Load from Default Database 

            // Get a count of the number of locations in the database
            FieldLocation locationModel = new FieldLocation();
            List<object> locationTableRows = accessData.ReadTable(locationModel.GetType(), string.Empty);
            int currentLocationCount = locationTableRows.Count();
            // If at least one location exists display it on the map
            if (currentLocationCount >= 1)
            {
                //Build a list of already loaded stations id on the map
                Dictionary<string, Graphic> loadedGraphicList = new Dictionary<string, Graphic>();
                foreach (Graphic gr in _OverlayStation.Graphics)
                {
                    loadedGraphicList[gr.Attributes[Dictionaries.DatabaseLiterals.FieldLocationID].ToString()] = gr;
                }

                //Reset main db station overlay
                inMapView.GraphicsOverlays.Remove(_OverlayStation);
                inMapView.GraphicsOverlays.Remove(_OverlayStationLabel);
                inMapView.GraphicsOverlays.Add(_OverlayStation);
                inMapView.GraphicsOverlays.Add(_OverlayStationLabel);

                // Load
                SQLiteConnection defaultConnection = accessData.GetConnectionFromPath(DataAccess.DbPath);
                using (defaultConnection)
                {
                    LoadFromGivenDB(locationTableRows, defaultConnection, loadedGraphicList, true);
                    defaultConnection.Close();
                }



                inMapView.UpdateLayout();
            }
            else
            {
                //Refresh graphics since the last location seems to have been deleted by the user
                if (currentMapView != null && currentMapView.GraphicsOverlays != null && currentMapView.GraphicsOverlays.Count > 0)
                {

                    currentMapView.GraphicsOverlays.Remove(_OverlayStation);
                    currentMapView.GraphicsOverlays.Remove(_OverlayStationLabel);
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
            PictureMarkerSymbol pointSym = new PictureMarkerSymbol(new Uri("ms-appx:///Assets/IC809393.png"));

            //Choose proper overlay
            GraphicsOverlay pointOverlay = new GraphicsOverlay();
            GraphicsOverlay pointLabelOverlay = new GraphicsOverlay();

            if (isDefaultDB)
            {
                pointOverlay = _OverlayStation;
                pointLabelOverlay = _OverlayStationLabel;
            }
            else
            {
                string dbFileName = Path.GetFileName(dbConnection.DatabasePath);
                if (_overlayContainerOther.ContainsKey(dbFileName))
                {
                    pointOverlay = _overlayContainerOther[dbFileName].Item1;
                    pointLabelOverlay = _overlayContainerOther[dbFileName].Item2;
                }

            }

            #region ADD
            // Get latitude, longitude and station id and add to graphics overlay
            foreach (object lcs in inLocationTableRows)
            {
                //Variables
                bool stationGraphicExists = false;

                #region POINT SYMBOL
                Models.FieldLocation currentLocation = lcs as Models.FieldLocation;
                var ptLatitude = currentLocation.LocationLat;
                var ptLongitude = currentLocation.LocationLong;
                var ptLocId = currentLocation.LocationID;

                List<object> stationTableRows = new List<object>();
                Station stations = new Station();
                string stationsSelectionQuery = "Select * from " + DatabaseLiterals.TableStation + " where " + DatabaseLiterals.FieldLocationID + " = '" + currentLocation.LocationID + "'";
                stationTableRows = accessData.ReadTableFromDBConnectionWithoutClosingConnection(stations.GetType(), stationsSelectionQuery, dbConnection);

                // should only be one station returned, this approach doesn't allow for multiple stations
                var ptStationId = string.Empty;
                var ptStationDate = string.Empty;
                var ptStationTime = string.Empty;
                var ptStationType = string.Empty;
                string ptStationLocationID = string.Empty;
                double ptStationLocationLat;
                double ptStationLocationLong;
                string ptStationLocationEPSG = string.Empty;
                foreach (object scs in stationTableRows)
                {
                    Models.Station currentStation = scs as Models.Station;
                    ptStationId = currentStation.StationAlias;
                    ptStationDate = currentStation.StationVisitDate;
                    ptStationTime = currentStation.StationVisitTime;
                    ptStationType = currentStation.StationObsType;
                    ptStationLocationID = currentLocation.LocationID;
                    ptStationLocationLat = currentLocation.LocationLat;
                    ptStationLocationLong = currentLocation.LocationLong;
                    ptStationLocationEPSG = currentLocation.LocationDatum;
                }

                //Find if station was already loaded
                if (graphicList.ContainsKey(currentLocation.LocationID))
                {
                    stationGraphicExists = true;
                    graphicList.Remove(currentLocation.LocationID);
                }

                //Add new graphic station and it's related label if needed
                if (!stationGraphicExists && ptStationId != null && ptStationId != string.Empty)
                {
                    //Create Map Point for graphic
                    MapPoint geoPoint = new MapPoint(ptLongitude, ptLatitude, SpatialReferences.Wgs84);

                    //Get if datum transformation is needed
                    int epsg = 0;
                    int.TryParse(ptStationLocationEPSG, out epsg);

                    if (epsg != 0 && epsg != 4326)
                    {
                        DatumTransformation datumTransfo = null;
                        SpatialReference outSR = null;

                        if ((epsg > 26900 && epsg < 27000))
                        {
                            outSR = new Esri.ArcGISRuntime.Geometry.SpatialReference(4617);
                            datumTransfo = TransformationCatalog.GetTransformation(outSR, SpatialReferences.Wgs84);
                        }


                        MapPoint proPoint = new MapPoint(ptLongitude, ptLatitude, outSR);

                        //Validate if transformation is needed.
                        if (datumTransfo != null)
                        {
                            //Replace geopoint
                            geoPoint = (MapPoint)Esri.ArcGISRuntime.Geometry.GeometryEngine.Project(proPoint, SpatialReferences.Wgs84, datumTransfo);
                        }

                    }

                    var graphic = new Graphic(geoPoint, pointSym);
                    graphic.Attributes.Add("Id", ptStationId.ToString());
                    graphic.Attributes.Add("Date", ptStationDate.ToString());
                    graphic.Attributes.Add("Time", ptStationTime.ToString());
                    graphic.Attributes.Add(Dictionaries.DatabaseLiterals.FieldLocationID, ptStationLocationID.ToString());
                    if (ptStationType != null)
                    {
                        graphic.Attributes.Add("Type", ptStationType.ToString());
                    }
                    else
                    {
                        graphic.Attributes.Add("Type", string.Empty);
                    }

                    graphic.Attributes.Add("Default", isDefaultDB);

                    pointOverlay.Graphics.Add(graphic);



                    #region LABEL SYMBOL
                    var textSym = new TextSymbol();
                    textSym.FontFamily = "Arial";
                    textSym.FontWeight = FontWeight.Bold;
                    textSym.Color = System.Drawing.Color.Black;
                    textSym.HaloColor = System.Drawing.Color.WhiteSmoke;
                    textSym.HaloWidth = 2;
                    textSym.Size = 16;
                    textSym.HorizontalAlignment = Esri.ArcGISRuntime.Symbology.HorizontalAlignment.Left;
                    textSym.VerticalAlignment = Esri.ArcGISRuntime.Symbology.VerticalAlignment.Baseline;
                    textSym.OffsetX = 7;
                    textSym.OffsetY = 7;
                    textSym.Text = ptStationId.ToString();
                    pointLabelOverlay.Graphics.Add(new Graphic(new MapPoint(ptLongitude, ptLatitude, SpatialReferences.Wgs84), textSym));
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
                                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High, async () =>
                                {


                                    ContentDialog acquiringLocationDialog = new ContentDialog()
                                    {
                                        Title = local.GetString("MapPageDialogLocationTitle"),
                                        Content = local.GetString("MapPageDialogLocationAcquiring"),
                                        PrimaryButtonText = local.GetString("GenericDialog_ButtonOK"),
                                    };
                                    acquiringLocationDialog.Style = (Style)Application.Current.Resources["WarningDialog"];
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

            if (clickedQuickButton != Dictionaries.DatabaseLiterals.KeywordLocation)
            {
                if (inLocation == null)
                {
                    inLocation = new FieldLocation();
                    inLocation.LocationElev = _currentMSGeoposition.Coordinate.Point.Position.Altitude;
                    inLocation.LocationLat = _currentMSGeoposition.Coordinate.Point.Position.Latitude;
                    inLocation.LocationLong = _currentMSGeoposition.Coordinate.Point.Position.Longitude;
                    inLocation.LocationErrorMeasure = _currentMSGeoposition.Coordinate.Accuracy;
                    inLocation.LocationElevMethod = vocabElevmethodGPS;
                    inLocation.LocationEntryType = _currentMSGeoposition.Coordinate.PositionSource.ToString();
                    inLocation.LocationErrorMeasureType = vocabErrorMeasureTypeMeter;
                    inLocation.LocationElevationAccuracy = _currentMSGeoposition.Coordinate.AltitudeAccuracy;
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
                FieldLocation manualLocation = new FieldLocation();
                manualLocation.LocationElev = 0.0;
                manualLocation.LocationLat = 0.0;
                manualLocation.LocationLong = 0.0;
                manualLocation.LocationEntryType = vocabEntryTypeManual;
                manualLocation.LocationID = idCalculator.CalculateLocationID(); //Calculate new value
                manualLocation.LocationAlias = idCalculator.CalculateLocationAlias(string.Empty); //Calculate new value
                manualLocation.MetaID = localSetting.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoID).ToString(); //Foreign key

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
            view.ViewModel.newStationEdit += NavigateToReport; //Detect when the add/edit request has finished.
            modal.IsModal = true;

            DataLocalSettings dLocalSettings = new DataLocalSettings();
            dLocalSettings.SetSettingValue("forceNoteRefresh", false);
        }

        /// <summary>
        /// Will pop the station dialog
        /// </summary>
        public void GotoLocationDataPart(FieldLocation locationEmptyEntry)
        {

            var modal = Window.Current.Content as ModalDialog;
            var view = modal.ModalContent as Views.LocationDialog;
            FieldNotes newLocationFieldNotes = new FieldNotes();
            newLocationFieldNotes.location = locationEmptyEntry;
            modal.ModalContent = view = new Views.LocationDialog(newLocationFieldNotes);
            view.locationVM.doLocationUpdate = false;
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


            ContentDialogResult cdr = await Services.ContentDialogMaker.CreateContentDialogAsync(noLocationDialog, true).Result;

            if (cdr == ContentDialogResult.Primary)
            {
                bool result = await Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-location"));
            }


        }


        /// <summary>
        /// When no location is available probably due to flight mode. Display this message.
        /// </summary>
        public async Task NoLocationFlightMode()
        {
            // Language localization using Resource.resw
            //var local = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High, async () =>
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
                    currentMapView.Tapped += myMapView_AddByTap;
                    userHasTurnedGPSOff = true;
                    SetGPSModeIcon(Symbol.TouchPointer);
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
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High, async () =>
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
            view.ViewModel.newSampleEdit += NavigateToReport; //Detect when the add/edit request has finished.
            modal.IsModal = true;

        }

        public void GotoStructureDialog(FieldLocation structureMapPoint)
        {
            //Create a quick earthmat
            EarthmatViewModel eVM = new EarthmatViewModel(null);
            FieldNotes quickEarthmat = eVM.QuickEarthmat(structureMapPoint);

            var modal = Window.Current.Content as ModalDialog;
            var view = modal.ModalContent as Views.StructureDialog;
            modal.ModalContent = view = new Views.StructureDialog(quickEarthmat, true);
            view.strucViewModel.newStructureEdit += NavigateToReport; //Detect when the add/edit request has finished.
            modal.IsModal = true;

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
            view.pflowModel.newPflowEdit += NavigateToReport; //Detect when the add/edit request has finished.
            modal.IsModal = true;

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

            var modal = Window.Current.Content as ModalDialog;
            var view = modal.ModalContent as Views.DocumentDialog;
            modal.ModalContent = view = new Views.DocumentDialog(quickStation, quickStation, true);
            view.DocViewModel.newDocumentEdit += NavigateToReport; //Detect when the add/edit request has finished.
            modal.IsModal = true;
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
            view.ViewModel.newStationEdit += RefreshMap; //Detect when the add/edit request has finished.
            modal.IsModal = true;

            DataLocalSettings dLocalSettings = new DataLocalSettings();
            dLocalSettings.SetSettingValue("forceNoteRefresh", true);
        }

        /// <summary>
        /// Mainly used to refresh the map, after some data entry, example waypoint addition.
        /// </summary>
        /// <param name="sender"></param>
        private void RefreshMap(object sender)
        {
            DisplayPointAndLabelsAsync(currentMapView);
        }

        /// <summary>
        /// Mainly used when user needs to navigate to the field note page after a certain steps has been taken
        /// </summary>
        /// <param name="sender"></param>
        private void NavigateToReport(object sender)
        {
            //Navigate to the report page.
            NavigationService.Navigate(typeof(Views.ReportPage), new[] { selectedStationID, selectedStationDate });
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

        async public void OnPositionChanged(Geolocator sender, PositionChangedEventArgs e)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
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

                    _currentAccuracy = in_position.Coordinate.Accuracy;
                    RaisePropertyChanged("CurrentAccuracy");
                    mapScale = currentMapView.MapScale;

                    //Only move if there is a coordinate
                    if (in_position.Coordinate.Point.Position.Longitude != 0 && in_position.Coordinate.Point.Position.Latitude != 0)
                    {
                        //Keep and update coordinates
                        _currentMSGeoposition = in_position;
                        RaisePropertyChanged("CurrentMSGeoposition");

                        _currentLongitude = in_position.Coordinate.Point.Position.Longitude;
                        RaisePropertyChanged("CurrentLongitude");
                        _currentLatitude = in_position.Coordinate.Point.Position.Latitude;
                        RaisePropertyChanged("CurrentLatitude");
                        _currentAltitude = in_position.Coordinate.Point.Position.Altitude;
                        RaisePropertyChanged("CurrentAltitude");

                        //Reset view on current location
                        bool settingViewPoint = await currentMapView.SetViewpointAsync(new Viewpoint(_currentLatitude, _currentLongitude, mapScale), TimeSpan.FromSeconds(1.5));
                    }
                    else
                    {
                        //Set non-sense accuracy to get a wide circle
                        _currentAccuracy = 0;
                        RaisePropertyChanged("CurrentAccuracy");
                    }

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

                    //Build current accuracy graphic
                    System.Drawing.Color accColor = new System.Drawing.Color();
                    Graphic accGraphic = GetAccuracyGraphic(_currentLongitude, _currentLatitude, _currentAccuracy, 36, out accColor);

                    //Build current position graphic
                    var posGraphic = new Graphic(new MapPoint(_currentLongitude, _currentLatitude, SpatialReferences.Wgs84), posSym);
                    posSym.Color = accColor;
                    posGraphic.Attributes.Add(attributeID, attributeIDPosition);

                    //Update graphic collection and UI
                    _OverlayCurrentPosition.Graphics.Add(posGraphic);
                    _OverlayCurrentPosition.Graphics.Add(accGraphic);
                    currentMapView.UpdateLayout();

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
        }

        private async void FieldBooksPageViewModel_newFieldBookSelectedAsync(object sender, string e)
        {
            ClearMapViewSettings();
            ClearLayers();
            DisplayPointAndLabelsAsync(currentMapView);
            try
            {
                await AddAllLayers();
            }
            catch (Exception)
            {

            }


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

        private Tuple<string, string, string> queryStation(string id)
        {
            Station stationModel = new Station();
            string stationId = "";
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
                    stationId = station.StationID.ToString();
                    stationDate = station.StationVisitDate.ToString();
                    stationTime = station.StationVisitTime.ToString();
                }
            }

            return new Tuple<string, string, string>(stationId, stationDate, stationTime);
        }

        private Tuple<string, int, List<string>> queryEarthmat(string id)
        {
            EarthMaterial earthmatModel = new EarthMaterial();
            string earthmatDetail = "";

            string earthmatQuerySelect = "SELECT * FROM " + Dictionaries.DatabaseLiterals.TableEarthMat;
            string earthmatQueryWhere = " WHERE STATIONID " + " = '" + id + "'";
            string earthmatQueryFinal = earthmatQuerySelect + earthmatQueryWhere;

            List<object> earthmatResults = accessData.ReadTable(earthmatModel.GetType(), earthmatQueryFinal);
            IEnumerable<EarthMaterial> earthmatTable = earthmatResults.Cast<EarthMaterial>();
            int earthmatCount = earthmatTable.Count();

            List<string> earthmatidList = new List<string>();
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

            return new Tuple<string, int, List<string>>(earthmatDetail, earthmatCount, earthmatidList);
        }

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
        /// Display information in a popup window when user clicks on feature location
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void myMapView_IdentifyFeature(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            //Get mouse position
            Point screenPoint = e.GetPosition(currentMapView);

            //Reset selections
            _OverlayStation.ClearSelection();
            _OverlayStationLabel.ClearSelection(); //Clear label selection, useless
            selectedStationDate = string.Empty;
            selectedStationID = string.Empty;

            // identify graphics overlay using the point tapped
            List<IdentifyGraphicsOverlayResult> overlays = new List<IdentifyGraphicsOverlayResult>(); //Container that will hold multiple graphic overlay identify query results (there might be multiple point on top of each others)
            IdentifyGraphicsOverlayResult resultGraphics = await currentMapView.IdentifyGraphicsOverlayAsync(_OverlayStation, screenPoint, 25, false);
            overlays.Add(resultGraphics);

            //make sure something is selected else try with other overlay
            if (overlays[0].Graphics.Count == 0)
            {
                foreach (KeyValuePair<string, Tuple<GraphicsOverlay, GraphicsOverlay>> go in _overlayContainerOther)
                {
                    resultGraphics = await currentMapView.IdentifyGraphicsOverlayAsync(go.Value.Item1, screenPoint, 25, false);
                    overlays.Add(resultGraphics);
                    go.Value.Item1.ClearSelection();
                    go.Value.Item2.ClearSelection();
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
                            // Get select station information
                            var tupleStation = queryStation(graphicId);
                            string stationId = tupleStation.Item1;
                            string stationDate = tupleStation.Item2;
                            string stationTime = tupleStation.Item3;

                            // Get select earthmat information
                            var tupleEarthmat = queryEarthmat(stationId);
                            string earthmatDetail = tupleEarthmat.Item1;
                            int earthmatCount = tupleEarthmat.Item2;
                            List<string> earthmatidList = tupleEarthmat.Item3;

                            // Get select ma information
                            MineralAlteration modelMineralAlteration = new MineralAlteration();
                            int maCount = CountChild(stationId, Dictionaries.DatabaseLiterals.FieldMineralAlterationRelID, Dictionaries.DatabaseLiterals.TableMineralAlteration, modelMineralAlteration);

                            // Get select document information
                            Document modelDocument = new Document();
                            int documentCount = CountChild(stationId, Dictionaries.DatabaseLiterals.FieldDocumentRelatedID, Dictionaries.DatabaseLiterals.TableDocument, modelDocument);

                            // getting the count for the children sample, mineral, fossil, structure 
                            int sampleTotalCount = 0;
                            foreach (string earthmatId in earthmatidList)
                            {
                                Sample modelSample = new Sample();
                                int sampleCount = CountChild(earthmatId, Dictionaries.DatabaseLiterals.FieldEarthMatID, Dictionaries.DatabaseLiterals.TableSample, modelSample);
                                sampleTotalCount += sampleCount;
                            }

                            int structureTotalCount = 0;
                            foreach (string earthmatId in earthmatidList)
                            {
                                Structure modelStructure = new Structure();
                                int structureCount = CountChild(earthmatId, Dictionaries.DatabaseLiterals.FieldEarthMatID, Dictionaries.DatabaseLiterals.TableStructure, modelStructure);
                                structureTotalCount += structureCount;
                            }

                            int mineralTotalCount = 0;
                            foreach (string earthmatId in earthmatidList)
                            {
                                Mineral modelMineral = new Mineral();
                                int mineralCount = CountChild(earthmatId, Dictionaries.DatabaseLiterals.FieldEarthMatID, Dictionaries.DatabaseLiterals.TableMineral, modelMineral);
                                mineralTotalCount += mineralCount;
                            }

                            int fossilTotalCount = 0;
                            foreach (string earthmatId in earthmatidList)
                            {
                                Fossil modelFossil = new Fossil();
                                int fossilCount = CountChild(earthmatId, Dictionaries.DatabaseLiterals.FieldEarthMatID, Dictionaries.DatabaseLiterals.TableFossil, modelFossil);
                                fossilTotalCount += fossilCount;
                            }

                            ContentDialog tapStationDialog = new ContentDialog()
                            {
                                Title = String.Format("Location Summary"),
                                Content = String.Format("{10}  {0}  {1}" +
                                "\nEarth material ({2}) {3}" +
                                "\nMineralization - Alteration ({4})" +
                                "\nDocuments ({5})" +
                                "\nSamples ({6})" +
                                "\nStructures ({7})" +
                                "\nMinerals ({8})" +
                                "\nFossils ({9})",
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
                                idGraphic.Attributes["Id"].ToString()),
                                PrimaryButtonText = local.GetString("MapPageDialogTextReport"),
                                SecondaryButtonText = local.GetString("MapPageDialogTextClose")
                            };

                            ContentDialogResult cdr = await Services.ContentDialogMaker.CreateContentDialogAsync(tapStationDialog, true).Result;
                            if (cdr == ContentDialogResult.Primary)
                            {
                                selectedStationID = idGraphic.Attributes["Id"].ToString();
                                selectedStationDate = idGraphic.Attributes["Date"].ToString();
                                NavigateToReport(sender);
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
        /// Allows user to enter location by tapping on the map
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void myMapView_AddByTap(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Point screenPoint = e.GetPosition(currentMapView);

            if (!currentMapView.LocationDisplay.IsEnabled)
            {
                MapPoint mapPoint = currentMapView.ScreenToLocation(screenPoint);

                //Convert projected to geographic if needed
                if (mapPoint.SpatialReference.IsProjected)
                {
                    SpatialReference geographicSpatialReference = new SpatialReference(4326);
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
                    FieldLocation tapLocation = new FieldLocation();
                    tapLocation.LocationElev = mapPoint.Z;
                    tapLocation.LocationLat = mapPoint.Y;
                    tapLocation.LocationLong = mapPoint.X;
                    tapLocation.LocationErrorMeasure = 9999;
                    tapLocation.LocationElevMethod = Dictionaries.DatabaseLiterals.DefaultNoData;
                    tapLocation.LocationEntryType = vocabEntryTypeTap;
                    tapLocation.LocationErrorMeasureType = Dictionaries.DatabaseLiterals.DefaultNoData;

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
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
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
            posSym.Color = System.Drawing.Color.FromArgb(0, 122, 194);
            posSym.Size = 23.0;
            SimpleLineSymbol posLineSym = new SimpleLineSymbol();
            posLineSym.Color = System.Drawing.Color.White;
            posLineSym.Width = 2;
            posSym.Outline = posLineSym;
        }

        /// <summary>
        /// Will set the position graphic so user can see where he is located.
        /// </summary>
        public Graphic GetAccuracyGraphic(double centerX, double centerY, double radiusInMeter, int numberOfVertices, out System.Drawing.Color accColor)
        {
            //Init symbols
            SimpleFillSymbol accSym = new SimpleFillSymbol();
            SimpleLineSymbol accLineSym = new SimpleLineSymbol();
            Windows.UI.Color posColor = (Windows.UI.Color)Application.Current.Resources["PositionColor"];
            int defaultAlpha = 30;

            //Parse accuracy to change color
            if (radiusInMeter > 20 && radiusInMeter <= 40)
            {
                posColor = (Windows.UI.Color)Application.Current.Resources["WarningColor"];
                defaultAlpha = 50;

            }
            else if (radiusInMeter > 40)
            {
                posColor = (Windows.UI.Color)Application.Current.Resources["ErrorColor"];
                defaultAlpha = 75;
            }
            else if (radiusInMeter == 0)
            {
                posColor = (Windows.UI.Color)Application.Current.Resources["ErrorColor"];
                defaultAlpha = 75;
                radiusInMeter = 1000; //Maximum accuracy for invalid position
            }

            //Finalize symbols
            accColor = System.Drawing.Color.FromArgb(posColor.R, posColor.G, posColor.B);
            accSym.Color = System.Drawing.Color.FromArgb(defaultAlpha, posColor.R, posColor.G, posColor.B);
            accLineSym.Color = accColor;
            accLineSym.Width = 2;
            accSym.Outline = accLineSym;

            //Set graphic
            Graphic accGraphic = new Graphic();
            accGraphic.Geometry = GetAccuracyPolygon(centerX, centerY, radiusInMeter, numberOfVertices);
            accGraphic.Symbol = accSym;
            accGraphic.Attributes.Add(attributeID, attributeIDAccuracy);

            return accGraphic;
        }

        /// <summary>
        /// Will set the position graphic so user can see where he is located.
        /// </summary>
        public Polygon GetAccuracyPolygon(double centerX, double centerY, double radiusInMeter, int numberOfVertices)
        {
            MapPoint centerPoint = new MapPoint(centerX, centerY, 0.0, SpatialReferences.Wgs84);
            MapPoint projectedPoint = (MapPoint)Esri.ArcGISRuntime.Geometry.GeometryEngine.Project(centerPoint, SpatialReferences.WebMercator);

            PolygonBuilder polyBuilder = new PolygonBuilder(SpatialReferences.WebMercator);
            List<MapPoint> polyPoints = new List<MapPoint>();
            int degreeInternalIteration = 0;
            int degreeInterval = Convert.ToInt16(Math.Floor(360.0 / numberOfVertices));
            for (int v = 0; v < numberOfVertices; v++)
            {
                double newX = projectedPoint.X + (radiusInMeter * Math.Cos(degreeInternalIteration * Math.PI / 180));
                double newY = projectedPoint.Y + (radiusInMeter * Math.Sin(degreeInternalIteration * Math.PI / 180));

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
        /// Will create a blanck polygon feature and add it to base map its extent gets initialized to new polygon extent instead of first entered tpk.
        /// A project will be force to match input spatial reference to work properly.
        /// </summary>
        /// <param name="inSP"></param>
        public void AddBlanckFeature(SpatialReference inSP)
        {
            if (inSP != null)
            {
                //Define extent by creating a new polygon
                SpatialReference wgs84 = SpatialReference.Create(4326);
                PolygonBuilder polyBuilder = new PolygonBuilder(wgs84);
                polyBuilder.AddPoint(new MapPoint(-143.6561, 41.0437, wgs84));
                polyBuilder.AddPoint(new MapPoint(-143.6561, 83.4417, wgs84));
                polyBuilder.AddPoint(new MapPoint(-56.7885, 83.4417, wgs84));
                polyBuilder.AddPoint(new MapPoint(-56.7885, 41.0437, wgs84));

                //Project
                Polygon blanckPoly = (Polygon)Esri.ArcGISRuntime.Geometry.GeometryEngine.Project(polyBuilder.ToGeometry(), inSP);

                // defines the schema for the geometry's attribute
                List<Field> polygonFields = new List<Field>();
                polygonFields.Add(Field.CreateString("Area", "Area Name", 50));

                FeatureCollectionTable polygonTable = new FeatureCollectionTable(polygonFields, GeometryType.Polygon, inSP);
                Dictionary<string, object> attributes = new Dictionary<string, object>();
                attributes[polygonFields[0].Name] = "Blanck area";
                Feature blanckFeature = polygonTable.CreateFeature(attributes, blanckPoly);
                polygonTable.AddFeatureAsync(blanckFeature);

                FeatureCollection fCollection = new FeatureCollection();
                fCollection.Tables.Add(polygonTable);
                FeatureCollectionLayer fCollectionLayer = new FeatureCollectionLayer(fCollection);
                fCollectionLayer.Name = "Blanck area";
                fCollectionLayer.IsVisible = false;

                esriMap.Basemap.BaseLayers.Add(fCollectionLayer);
            }

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
                if (fileName.Contains(".json"))
                {
                    jsonRenderingFile = sf;
                }
                else if (fileName.Contains(".tpk"))
                {
                    tpkList[sf.Name] = sf;
                }
                else if (fileName.Contains(".sqlite") && !fileName.Contains(DatabaseLiterals.DBName))
                {
                    sqliteList[sf.Name] = sf;
                }
                else if (fileName.Contains(".mmpk"))
                {
                    MobileMapPackage mobileMapPackage;
                    bool isDirectReadSupported = await MobileMapPackage.IsDirectReadSupportedAsync(sf.Path);
                    if (isDirectReadSupported)
                    {
                        mobileMapPackage = await MobileMapPackage.OpenAsync(sf.Path);
                    }
                    else
                    {
                        await MobileMapPackage.UnpackAsync(sf.Path, accessData.ProjectPath);
                        mobileMapPackage = await MobileMapPackage.OpenAsync(accessData.ProjectPath);
                    }

                    if (mobileMapPackage.Maps.Count > 0)
                    {
                        Map mymap = mobileMapPackage.Maps.First();
                        currentMapView.Map = mymap;
                        //currentMapView.UpdateLayout();

                        foreach (Layer item in currentMapView.Map.AllLayers)
                        {
                            MapPageLayers im = new MapPageLayers();
                            im.LayerName = item.Name;
                            MapPageLayerSetting mpls = new MapPageLayerSetting();
                            mpls.LayerOpacity = item.Opacity * 100;
                            mpls.LayerVisibility = item.IsVisible;
                            im.LayerSettings = mpls;
                            _filenameValues.Add(im);
                            RaisePropertyChanged("FilenameValues");
                        }
                    }
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
                            bool tpkVisibility = true;
                            bool.TryParse(configs.LayerSettings.LayerVisibility.ToString(), out tpkVisibility);
                            double tpkOpacity = 1;
                            Double.TryParse(configs.LayerSettings.LayerOpacity.ToString(), out tpkOpacity);
                            await AddDataTypeTPK(tpkList[configs.LayerName], tpkVisibility, tpkOpacity / 100);
                            tpkList.Remove(configs.LayerName);
                            foundLayers = true;
                        }

                    }
                    else if (configs.LayerName != null && configs.LayerName.Contains(".sqlite"))
                    {
                        if (sqliteList.ContainsKey(configs.LayerName))
                        {
                            bool sqlVisibility = true;
                            bool.TryParse(configs.LayerSettings.LayerVisibility.ToString(), out sqlVisibility);
                            double sqlOpacity = 1;
                            Double.TryParse(configs.LayerSettings.LayerOpacity.ToString(), out sqlOpacity);
                            AddDataTypeSQLite(sqliteList[configs.LayerName], sqlVisibility, sqlOpacity / 100);
                            sqliteList.Remove(configs.LayerName);
                            foundLayers = true;
                        }
                    }

                }
            }

            //Process possible missing layers in json
            if (tpkList.Count > 0)
            {
                foreach (KeyValuePair<string, StorageFile> remainingTpks in tpkList)
                {
                    await AddDataTypeTPK(remainingTpks.Value, true, 1);
                    MapPageLayers mpl = new MapPageLayers();
                    mpl.LayerName = remainingTpks.Key;
                    MapPageLayerSetting mpls = new MapPageLayerSetting();
                    mpls.LayerOpacity = 100;
                    mpls.LayerVisibility = true;
                    mpl.LayerSettings = mpls;
                    _filenameValues.Add(mpl);
                    RaisePropertyChanged("FilenameValues");
                    foundLayers = true;
                }
            }
            if (sqliteList.Count > 0)
            {
                foreach (KeyValuePair<string, StorageFile> remainingSqlite in sqliteList)
                {
                    AddDataTypeSQLite(remainingSqlite.Value, true, 1);
                    MapPageLayers mpl = new MapPageLayers();
                    mpl.LayerName = remainingSqlite.Key;
                    MapPageLayerSetting mpls = new MapPageLayerSetting();
                    mpls.LayerOpacity = 100;
                    mpls.LayerVisibility = true;
                    mpl.LayerSettings = mpls;
                    _filenameValues.Add(mpl);
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
                    }
                    if (esriMap.Basemap.BaseLayers.Count == 0 && _tileLayer.SpatialReference != null)
                    {
                        AddBlanckFeature(_tileLayer.SpatialReference);

                    }
                    else
                    {
                        SpatialReference sr = new SpatialReference(4326);
                        AddBlanckFeature(sr);
                    }


                    _tileLayer.IsVisible = isTPKVisible;
                    _tileLayer.Opacity = tpkOpacity;
                    esriMap.Basemap.BaseLayers.Add(_tileLayer);



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
        public void AddDataTypeSQLite(StorageFile inSQLite, bool isVisible = true, double opacity = 1)
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
                    foreach (Graphic gro in _overlayContainerOther[inSQLite.Name].Item1.Graphics)
                    {
                        loadedOtherGraphicList[gro.Attributes[Dictionaries.DatabaseLiterals.FieldLocationID].ToString()] = gro;
                    }
                }
                else
                {
                    Tuple<GraphicsOverlay, GraphicsOverlay> relatedGraphics = new Tuple<GraphicsOverlay, GraphicsOverlay>(new GraphicsOverlay(), new GraphicsOverlay());
                    _overlayContainerOther[inSQLite.Name] = relatedGraphics;
                }


                // Add graphics overlay to map view
                if (!currentMapView.GraphicsOverlays.Contains(_overlayContainerOther[inSQLite.Name].Item1))
                {
                    currentMapView.GraphicsOverlays.Add(_overlayContainerOther[inSQLite.Name].Item1);
                }
                if (!currentMapView.GraphicsOverlays.Contains(_overlayContainerOther[inSQLite.Name].Item2))
                {
                    currentMapView.GraphicsOverlays.Add(_overlayContainerOther[inSQLite.Name].Item2);
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
                    var sublayer = esriMap.AllLayers.First(x => x.Name.Contains(layerName.Split('.')[0]));
                    if (sublayer != null)
                    {
                        if (inSwitch != null)
                        {
                            sublayer.IsVisible = inSwitch.IsOn;
                        }
                        else if (inSlider != null)
                        {
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
                            subFile.LayerSettings.LayerOpacity = inSlider.Value / 100.0;
                        }

                    }
                    #endregion
                }

                if (esriMap != null && esriMap.AllLayers.Count > 0 && layerName.Contains(".sqlite"))
                {

                    #region OVERLAYS
                    // Find the layer from the map layers and change visibility
                    if (_overlayContainerOther.ContainsKey(layerName))
                    {
                        if (inSwitch != null)
                        {
                            _overlayContainerOther[layerName].Item1.IsVisible = inSwitch.IsOn;
                            _overlayContainerOther[layerName].Item2.IsVisible = inSwitch.IsOn;
                        }
                        else if (inSlider != null)
                        {
                            _overlayContainerOther[layerName].Item1.Opacity = inSlider.Value / 100.0;
                            _overlayContainerOther[layerName].Item2.Opacity = inSlider.Value / 100.0;
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
                            subFile.LayerSettings.LayerOpacity = inSlider.Value / 100.0;
                        }

                    }
                    #endregion

                }

                SaveLayerRendering();

            }


        }


        /// <summary>
        /// Will set the maps (layers) order in the map control from user choices.
        /// </summary>
        public async
        /// <summary>
        /// Will set the maps (layers) order in the map control from user choices.
        /// </summary>
        Task SetLayerOrderAsync()
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

                    bool firstIteration = true;
                    ObservableCollection<MapPageLayers> newFileList = new ObservableCollection<MapPageLayers>();
                    foreach (MapPageLayers orderedFiles in _filenameValues.Reverse()) //Reverse order while iteration because UI is reversed intentionnaly
                    {
                        if (orderedFiles.LayerName.Contains(".tpk"))
                        {
                            //Build path
                            string localFilePath = Path.Combine(accessData.ProjectPath, orderedFiles.LayerName);
                            Uri localUri = new Uri(localFilePath);

                            if (firstIteration)
                            {
                                Layer firstLayer = layerDico.First().Value;
                                AddBlanckFeature(firstLayer.SpatialReference);
                                firstIteration = false;
                            }

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


                                esriMap.Basemap.BaseLayers.Add(layerToAdd);
                            }




                        }

                    }

                    //Update UI
                    if (newFileList != null && newFileList.Count != 0 && newFileList.Count == _filenameValues.Count)
                    {
                        _filenameValues = newFileList;
                    }

                    RaisePropertyChanged("FilenameValues");

                    SaveLayerRendering();
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

            }
            currentMapView.UpdateLayout();

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
                var filesPicker = new Windows.Storage.Pickers.FileOpenPicker();
                filesPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
                filesPicker.FileTypeFilter.Add(".tpk");
                filesPicker.FileTypeFilter.Add(".sqlite");
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

                                    currentMapView.GraphicsOverlays.Remove(_overlayContainerOther[selectedFile.LayerName].Item1);
                                    currentMapView.GraphicsOverlays.Remove(_overlayContainerOther[selectedFile.LayerName].Item2);
                                    _overlayContainerOther.Remove(selectedFile.LayerName);

                                }

                            }

                        }
                    }
                }

            }
            else if (!fromSelection)
            {
                esriMap.Basemap.BaseLayers.Clear();

            }
        }

        #endregion

    }

}