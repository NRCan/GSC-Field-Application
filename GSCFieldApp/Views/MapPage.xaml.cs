using GSCFieldApp.ViewModel;
using Mapsui.Extensions;
using System.Diagnostics.CodeAnalysis;
using Mapsui.Styles;
using Mapsui.UI.Maui;
using Mapsui.Layers;
using Mapsui.Projections;
using BruTile;
using SQLite;
using BruTile.MbTiles;
using Mapsui.Tiling.Layers;
using Mapsui;
using GSCFieldApp.Services.DatabaseServices;
using GSCFieldApp.Models;
using GSCFieldApp.Dictionaries;
using Color = Mapsui.Styles.Color;
using Brush = Mapsui.Styles.Brush;
using Mapsui.UI.Maui.Extensions;
using GSCFieldApp.Services;
using BruTile.Predefined;
using Mapsui.Extensions.Cache;
using BruTile.Web;
using BruTile.Wmsc;
using System.Collections.ObjectModel;
using Mapsui.Providers.Wms;
using Sensor = Microsoft.Maui.Devices.Sensors;
using System.Diagnostics;
using System;
using NetTopologySuite.Operation.Distance;
using BruTile.Wms;
using Mapsui.Nts;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using GeoAPI.Geometries;
using Mapsui.UI.Objects;
using Point = NetTopologySuite.Geometries.Point;
using Coordinate = NetTopologySuite.Geometries.Coordinate;
using MultiPoint = NetTopologySuite.Geometries.MultiPoint;
using System.Collections.Generic;
using SkiaSharp.Views.Maui.Controls;
using System.Globalization;
using Microsoft.Maui.Storage;
using ProjNet.Geometries;



#if ANDROID
using Android.Content;
#elif IOS
using UIKit;
using Foundation;
#endif

namespace GSCFieldApp.Views;

public partial class MapPage : ContentPage
{

    private CancellationTokenSource _cancelTokenSource;
    private MapControl mapControl = new Mapsui.UI.Maui.MapControl();
    private DataAccess da = new DataAccess();
    private GeopackageService geopackService = new GeopackageService();
    private bool _isCheckingGeolocation = false;
    private bool _isTapMode = false;
    private bool _isDrawingLine = false;
    private double _viewportHeightRatio = 1; //Needed to calculate ratio difference between mapsui viewport box and skiasharp box on touch action events for line drawing
    private double _viewportWidthRatio = 1;
    private enum defaultLayerList { Stations, Linework, Traverses }
    private Sensor.Location badLoc = new Sensor.Location() { Accuracy=-99, Longitude=double.NaN, Latitude=double.NaN, Altitude=double.NaN };
    private Drawable _drawable = new Drawable(); //Meant to be used for linework
    private GeometryFeature _drawableLineGeometry = null; //Meant to be used for linework
    private GeometryFeature _drawableLineVectorGeometry = null; //Meant to be used for linework
    public LocalizationResourceManager LocalizationResourceManager
        => LocalizationResourceManager.Instance; // Will be used for in code dynamic local strings
    public ApplicationLiterals.SupportedWMSCRS _wmsCRS = ApplicationLiterals.SupportedWMSCRS.epsg3857;
    public Tuple<Point, Point> _wmsCRSExtent = null;
    private int _locationSettingEnabledAttempt = 0; //used to know when user has turned on location in the device setting
    private TimeSpan _refreshRate = TimeSpan.FromMilliseconds(1000); //Used for GPS refresh rate on location change event
    private bool _locationFollowEnabled = false; //Used to know if map should follow user location
    private bool _isInitialLoadingDone = false; //Used to know if initial loading is done, will prevent reloading all layers each time user comes back to map page
    private ParallelOptions _parallelOptions = new()
    {
        MaxDegreeOfParallelism = 10
    };
    private WKTReader _wellKnownTextReader = new WKTReader();
    private GeopackageService _geopackageService = new GeopackageService();

    //Symbols
    private int bitmapLocationSymbolId = -1; //Current localisation point symbol
    private int bitmapLineworkPointSymbolId = -1; //Linework vertices point symbol

    #region Properties

    private bool GPSLogEnabled
    {
        get { return Preferences.Get(nameof(GPSLogEnabled), false); }
        set { }
    }

    public string GPSLogFilePath
    {
        get { return Preferences.Get(nameof(GPSLogFilePath), ""); }
        set { }
    }

    public bool GPSHighRateEnabled
    {
        get { return Preferences.Get(nameof(GPSHighRateEnabled), false); }
        set { }
    }

    public bool LocationFollowEnabled
    {
        get { return Preferences.Get(nameof(LocationFollowEnabled), false); }
        set { }
    }

    #endregion

    public MapPage(MapViewModel vm)
    {
        try
        {
            InitializeComponent();

            BindingContext = vm;

            //Initialize grid background
            mapPageGrid.BackgroundColor = Mapsui.Styles.Color.FromString("White").ToNative();
            GPSMode.TextColor = Mapsui.Styles.Color.FromString("White").ToNative();
            mapControl.Map.Widgets.Add(new Mapsui.Widgets.ScaleBar.ScaleBarWidget(mapControl.Map)
            {
                TextAlignment = Mapsui.Widgets.Alignment.Center,
                HorizontalAlignment = Mapsui.Widgets.HorizontalAlignment.Left,
                VerticalAlignment = Mapsui.Widgets.VerticalAlignment.Bottom
            });

            //Set map and start listenning to layer events
            mapView.Map = mapControl.Map;
            mapView.Map.Info += Map_Info; //Get feature info event for loaded layers
            this.Loaded += MapPage_Loaded;
            this.mapControl.Map.Layers.LayerAdded += Layers_LayerAdded;

            //Detect new field book selection, uprgrade, edit, ...
            FieldBooksViewModel.newFieldBookSelected += FieldBooksViewModel_newFieldBookSelectedAsync;

            //Detect linework update (mainly for user color change)
            LineworkViewModel.lineworkHasUpdated += LineworkViewModel_lineworkHasUpdated;
        }
        catch (System.Exception e)
        {
            new ErrorToLogFile(e).WriteToFile();
        }

    }


    #region EVENTS

    /// <summary>
    /// An even that is triggered when user has updated a linework
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="updatedLineworkID">the int id of the updated linework</param>
    private void LineworkViewModel_lineworkHasUpdated(object sender, Tuple<int, string> updatedLineworkIDAndColor)
    {
        try
        {
            //Get layer
            ILayer lineLayer = mapView.Map.Layers.Where(x => x.Name == ApplicationLiterals.aliasLinework).FirstOrDefault();
            MemoryLayer memLineLayer = lineLayer as MemoryLayer;
            if (lineLayer != null && memLineLayer != null)
            {
                //Get updated feature
                IFeature lineFeat = memLineLayer.Features.Where(x => x.ToDisplayText().Contains(updatedLineworkIDAndColor.Item1.ToString())).FirstOrDefault();

                if (lineFeat != null)
                {
                    //Get style
                    IStyle lineStyle = lineFeat.Styles.FirstOrDefault(x => x is VectorStyle);
                    if (lineStyle != null)
                    {
                        //New color
                        Color newLineColor = GetColorFromString(updatedLineworkIDAndColor.Item2);

                        //Validate if color has changed
                        VectorStyle lineVectorStyle = lineStyle as VectorStyle;
                        Pen linePenStyle = lineVectorStyle.Line as Pen;

                        if (linePenStyle != null && !newLineColor.Equals(linePenStyle.Color))
                        {
                            linePenStyle.Color = newLineColor;
                            mapView.Map.RefreshData();
                        }

                    }
                }
            }
        }
        catch (System.Exception e)
        {
            new ErrorToLogFile(e).WriteToFile();
        }

    }

    private async void Map_Info(object sender, MapInfoEventArgs e)
    {

        //For easier interaction, close the info window if it's already opened
        //On smaller device it takes too much space and prevents user from querying elsewhere
        //easily
        if (!MapInfoResultsFrame.IsVisible && !_isTapMode)
        {
            //Retrieve clicked feature information
            ILayer l = e.MapInfo?.Layer;
            IFeature feature = e.MapInfo?.Feature;
            if (feature != null)
            {
                //Get feature id for sql query
                string featureValueText = feature.ToDisplayText();

                if (feature is GeometryFeature geometryFeature)
                {
                    //Get database path stashed in the tag
                    if (l.Tag != null && l.Tag.ToString() != string.Empty && featureValueText != string.Empty && featureValueText.Contains(":"))
                    {

                        SQLiteAsyncConnection infoConnection = new SQLiteAsyncConnection(l.Tag.ToString());

                        try
                        {
                            if (infoConnection != null)
                            {
                                //Build query to extract record
                                //Is using hex(geometry) field for user loaded geopackage
                                string featureIDFieldName = featureValueText.Split(":")[0];
                                string featureValue = featureValueText.Split(":")[1];
                                string gfiQuery = string.Format("SELECT * FROM {0} WHERE hex({1}) = '{2}';",
                                    l.Name,
                                    featureIDFieldName,
                                    featureValue);

                                //Parse field app default layer name for real feature name
                                if (l.Name == ApplicationLiterals.aliasTraversePoint)
                                {
                                    gfiQuery = string.Format("SELECT * FROM {0} WHERE {1} = {2} LIMIT 1;",
                                    DatabaseLiterals.TableTraversePoint,
                                    featureIDFieldName,
                                    featureValue);
                                }
                                else if (l.Name == ApplicationLiterals.aliasStations)
                                {
                                    gfiQuery = string.Format("SELECT * FROM {0} WHERE {1} = {2} LIMIT 1;",
                                    DatabaseLiterals.TableStation,
                                    featureIDFieldName,
                                    featureValue);
                                }
                                else if (l.Name == ApplicationLiterals.aliasLinework)
                                {
                                    gfiQuery = string.Format("SELECT * FROM {0} WHERE {1} = {2} LIMIT 1;",
                                    DatabaseLiterals.TableLinework,
                                    featureIDFieldName,
                                    featureValue);
                                }



                                //Get clicked feature record values
                                object?[][]? results = GeopackageService.RunGenericQuery(l.Tag.ToString(), gfiQuery);

                                if (results != null)
                                {
                                    //Show a pop-up with the result
                                    MapViewModel vm = BindingContext as MapViewModel;
                                    vm.FillMapInfoCollection(results);

                                    MapInfoResultsFrame.IsVisible = true;
                                }

                            }

                            await infoConnection.CloseAsync();
                        }
                        catch (System.Exception gfiError)
                        {
                            await infoConnection.CloseAsync();

                            await Shell.Current.DisplayAlert(
                                LocalizationResourceManager["GenericErrorTitle"].ToString(),
                                gfiError.Message,
                                LocalizationResourceManager["GenericButtonOk"].ToString());
                            new ErrorToLogFile(gfiError).WriteToFile();
                        }

                    }

                }
            }
        }
        else
        {
            MapInfoResultsFrame.IsVisible = false;
        }

    }

    /// <summary>
    /// Event triggered when user has changed field books.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void FieldBooksViewModel_newFieldBookSelectedAsync(object sender, bool hasChanged)
    {
        if (hasChanged && mapView != null)
        {
            _isInitialLoadingDone = false;

            //Reload user datasets
            await LoadPreferedLayers();
            await Task.Run(async () => await QuickRefreshDefaultFeatureLayer(false));

            _isInitialLoadingDone = true;
        }

    }

    /// <summary>
    /// Will be triggered whenever a layer has been added. 
    /// This will make sure to close the waiting cursor
    /// </summary>
    /// <param name="layer"></param>
    private void Layers_LayerAdded(ILayer layer)
    {
        if (layer != null)
        {
            try
            {
                //Make sure to disable the waiting cursor
                this.WaitingCursor.IsRunning = false;
            }
            catch (System.Exception e)
            {
                //Might crash because of thread
                new ErrorToLogFile(e).WriteToFile();
            }

        }

    }

    /// <summary>
    /// When user navigates or open the map page event
    /// </summary>
    /// <param name="args"></param>
    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        try
        {
            base.OnNavigatedTo(args);

            //Update some debug settings that user might have changed
            await SetGPSRefreshRate();
            await SetLocationFollow();

            //Freshen up the default layers
            await Task.Run(async () => await QuickRefreshDefaultFeatureLayer());

            //Close things that might be open
            MapInfoResultsFrame.IsVisible = false;

        }
        catch (System.Exception e)
        {
            new ErrorToLogFile(e).WriteToFile();
        }


    }

    /// <summary>
    /// Once the map is loaded, make a quick check on 
    /// field data locations, in case something has been deleted while
    /// user was away in the notes
    /// Make sure to zoom back to the field data location extent.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void MapPage_Loaded(object sender, EventArgs e)
    {
        try
        {
            //Setting map page background default data
            SetOpenStreetMap();

            //Manage symbol and layers
            await Task.Run(async () => await AddLocationSymbolToRegistry());
            await Task.Run(async () => await AddLineworkPointSymbolToRegistry());

            //Freshen up the default layers
            await Task.Run(async () => await RefreshDefaultFeatureLayer());

            //Reload user datasets for selected field book
            await Task.Run(async () => await LoadPreferedLayers());

            //Manage GPS
            if (!_isCheckingGeolocation && !_isTapMode)
            {
                await StartGPS();
            }

            //Keep in memory that initial loading is done
            //Will prevent some method to be launched each time loading has ended
            _isInitialLoadingDone = true;
        }
        catch (System.Exception exception)
        {
            new ErrorToLogFile(exception).WriteToFile();
        }
    }

    /// <summary>
    /// Clicked event made on the map
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void mapView_MapClicked(object sender, MapClickedEventArgs e)
    {

        //NOT WORKING --> Get feature info
        //GetWMSFeatureInfo(e.Point);
        //GetGpkgFeatureInfo(e.Point);

        //Detect if in tap mode or drawing lines to show tapped coordinates on screen
        if (_isTapMode && !_isDrawingLine && !MapLayerFrame.IsVisible && !MapAddGeopackageWMSFrame.IsVisible && !MapInfoResultsFrame.IsVisible)
        {
            //Convert incoming geographic coordinates and transform into DMS
            DD2DMS dmsLongitude = DD2DMS.FromDouble(e.Point.ToPoint().X);
            DD2DMS dmsLatitude = DD2DMS.FromDouble(e.Point.ToPoint().Y);

            //Build alert content with DMS values
            string content = string.Format("{0}\n{1}", 
                LocalizationResourceManager["MapPageTapCoordinateContent"].ToString(),
                e.Point.ToString()
                );

            //Show dialog
            bool answer = await Shell.Current.DisplayAlert(
                LocalizationResourceManager["MapPageTapCoordinateTitle"].ToString(),
                content,
                LocalizationResourceManager["GenericButtonYes"].ToString(),
                LocalizationResourceManager["GenericButtonNo"].ToString());

            //Pop station form
            if (answer)
            {
                //Refresh view with tap
                mapView?.MyLocationLayer.UpdateMyLocation(new Mapsui.UI.Maui.Position(e.Point.Latitude, e.Point.Longitude));
                mapView.RefreshGraphics();

                //Init record creation
                MapViewModel mvm = BindingContext as MapViewModel;
                mvm.RefreshCoordinatesFromTap(e.Point);
                mvm.AddStationCommand.Execute(mvm);
            }
        }

        //Make sure to disable map layer frame
        MapLayerFrame.IsVisible = false;

        //MapInfoResultsFrame.IsVisible = false;
    }

    /// <summary>
    /// Whenever a get feature info is finished on a WMS layer
    /// Show results on screen
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="featureInfo"></param>
    private void Gfi_IdentifyFinished(object sender, FeatureInfo featureInfo)
    {
        DisplayAlert("test", featureInfo.FeatureInfos.ToString(), "OK");
    }

    #region Buttons

    /// <summary>
    /// Will zoom to selected layer from layer menu
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void mapZoomToLayer_Clicked(object sender, EventArgs e)
    {
        Button zoomToButton = sender as Button;
        ILayer zoomToLayer = zoomToButton.BindingContext as ILayer;

        if (zoomToLayer != null)
        {
            string pathToLayer = string.Empty;
            if (zoomToLayer.Tag != null)
            {
                pathToLayer = zoomToLayer.Tag.ToString();
            }
            
            if (zoomToLayer.Name != ApplicationLiterals.aliasOSM && !pathToLayer.Contains("wms") && !pathToLayer.Contains("ows"))
            {
                SetExtent(zoomToLayer);
            }
            else
            {
                //Set to default extent, which is Canada
                SetExtent();
            }
            
        }
    }

    /// <summary>
    /// Will remove the selected layer from the map
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void mapDeleteLayer_Clicked(object sender, EventArgs e)
    {
        Button deleteButton = sender as Button;
        ILayer layerToDelete = deleteButton.BindingContext as ILayer;

        if (layerToDelete != null)
        {
            //Remove layer from map control, make it so and remove possible duplicates at the same time
            ILayer[] layersToDelete = mapView.Map.Layers.Where(x => x.Name == layerToDelete.Name).ToArray();
            mapView.Map.Layers.Remove(layerToDelete);

            MapViewModel _vm = BindingContext as MapViewModel;

            //Remove it also from the view model layer collection for menu
            _vm.RefreshLayerCollection(mapView.Map.Layers);

            //Save
            _vm.SaveLayerRendering();
        }

    }

    /// <summary>
    /// Will show a filer picker to add layers in the map
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void AddLayerButton_Clicked(object sender, EventArgs e)
    {
        this.WaitingCursor.IsRunning = true;

        //Call a dialog for user to select a file
        FileResult fr = await PickLayer();
        if (fr != null)
        {
            if (fr.FileName.Contains(DatabaseLiterals.LayerTypeMBTiles) )
            {
                await AddAMBTile(fr.FullPath);
            }
            else if (fr.FileName.Contains(DatabaseLiterals.LayerTypeGPKG))
            {
                await AddFromGPKG(fr.FullPath);
            }
            
        }

        this.WaitingCursor.IsRunning = false;

    }

    /// <summary>
    /// Whenever map page layer button is clicked, show or not show
    /// frame that list all layers properties
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ManageLayerButton_Clicked(object sender, EventArgs e)
    {
        //Refresh VM list of layers
        MapViewModel _vm = BindingContext as MapViewModel;
        _vm.RefreshLayerCollection(mapView.Map.Layers);

        MapLayerFrame.IsVisible = !MapLayerFrame.IsVisible;
    }

    /// <summary>
    /// Will disable GPS and activate tap entries
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void GPSMode_Clicked(object sender, EventArgs e)
    {
        ToggleGPS();
    }

    /// <summary>
    /// Will enable/disable tap entry, and will make sure to shut down line drawing if enabled
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TapMode_Clicked(object sender, EventArgs e)
    {
        ToggleTapEntry();

        if (_isDrawingLine)
        {
            ToggleDrawing();
        }
        
    }

    /// <summary>
    /// Will pop an entry dialog for user to enter
    /// an URL adress to add a WMS service
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void AddWMS_Clicked(object sender, EventArgs e)
    {

        string wms_url = await DisplayPromptAsync(LocalizationResourceManager["MapPageAddWMSDialogTitle"].ToString(),
            LocalizationResourceManager["MapPageAddWMSDialogMessage"].ToString(),
            LocalizationResourceManager["GenericButtonOk"].ToString(),
            LocalizationResourceManager["GenericButtonCancel"].ToString());

        if (wms_url != null && wms_url != string.Empty)
        {
            this.WaitingCursor.IsRunning = true;

            //Get a list of supported CRS before prompting to chose a layer
            WMSService wService = new WMSService();
            List<string> crs = await Task.Run(async () => await wService.GetListOfCRS(wms_url));
            bool supportedCRS = false;

            if (crs.Count() > 0)
            {
                //Check for default or canadian default
                if (crs.Contains(DatabaseLiterals.KeywordEPSG + DatabaseLiterals.KeywordEPSGMapsuiDefault))
                {
                    supportedCRS = true;
                }
                //if (!supportedCRS && crs.Contains(DatabaseLiterals.KeywordEPSG + DatabaseLiterals.KeywordEPSGAtlas))
                //{
                //    supportedCRS = true;
                //    _wmsCRS = ApplicationLiterals.SupportedWMSCRS.epsg3978;

                //    _wmsCRSExtent = await Task.Run(async () => await wService.GetCRSExtent(wms_url, DatabaseLiterals.KeywordEPSG + DatabaseLiterals.KeywordEPSGAtlas));

                //}
                //if (!supportedCRS && crs.Contains(DatabaseLiterals.KeywordEPSG + DatabaseLiterals.KeywordEPSGDefault))
                //{
                //    supportedCRS = true;
                //    _wmsCRS = ApplicationLiterals.SupportedWMSCRS.epsg4326;


                //    _wmsCRSExtent = await Task.Run(async () => await wService.GetCRSExtent(wms_url, DatabaseLiterals.KeywordEPSG + DatabaseLiterals.KeywordEPSGDefault));

                //}
            }

            if (supportedCRS)    
            {
                //Get list of WMS layers
                List<MapPageLayerSelection> mpl = await Task.Run(async () => await wService.GetListOfWMSLayers(wms_url));


                //Show list of layers available from url get cap os user can chose
                if (mpl != null && mpl.Count() > 0)
                {
                    MapViewModel vm = BindingContext as MapViewModel;
                    vm.FillWMSFeatureCollection(mpl);
                }
            }
            else
            {
                await DisplayAlert(LocalizationResourceManager["GenericErrorTitle"].ToString(),
                    LocalizationResourceManager["MapPageAddWMSDialogCRSMessage"].ToString(),
                    LocalizationResourceManager["GenericButtonOk"].ToString());
            }


            this.WaitingCursor.IsRunning = false;
        }
    }

    /// <summary>
    /// Will enable/disable the user to draw an interpretation line on screen and will
    /// disable tap entry if enabled
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void DrawLine_Clicked(object sender, EventArgs e)
    {
        ToggleDrawing();

        if (_isTapMode)
        {
            ToggleTapEntry();
        }

    }

    #endregion

    /// <summary>
    /// Will detect when layer menu is closed so that it can saves the layer rendering in the json
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MapLayerFrame_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "IsVisible")
        {
            Border sentFrame = sender as Border;

            //Condition on map view not being null to prevent it from being launch and map page load
            if (sentFrame != null && !sentFrame.IsVisible && this.mapView != null)
            {
                MapViewModel _vm = BindingContext as MapViewModel;

                if (_vm != null && _vm.LayerCollection != null && _vm.LayerCollection.Count() > 0)
                {
                    //Reorder layers (user might have changed it) - reverse to match mapsui ordering
                    List<ILayer> reverseLayers = new List<ILayer>();
                    reverseLayers.AddRange(_vm.LayerCollection.ToList());
                    reverseLayers.Reverse();
                    foreach (ILayer layer in reverseLayers)
                    {
                        if (mapView.Map.Layers.Contains(layer))
                        {
                            mapView.Map.Layers.Move(reverseLayers.IndexOf(layer), layer);
                        }
                    }
                    mapView.Map.Refresh();

                    //Update layer collection for menu
                    //_vm.RefreshLayerCollection(mapView.Map.Layers);

                    _vm.SaveLayerRendering();
                }
            }
        }
    }

    /// <summary>
    /// Track single tap on screen. 
    /// If drawing line is enabled it will start creating a new one
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void mapView_SingleTap(object sender, Mapsui.UI.TappedEventArgs e)
    {
        if (e != null && e.ScreenPosition != null && _isDrawingLine && !MapAddGeopackageWMSFrame.IsVisible && !MapInfoResultsFrame.IsVisible)
        {
            //Get screen ratio between skia and mapsui
            Tuple<double, double> screenRatios = CalculateViewportRatio(sender as SKCanvasView, mapView.Map.Navigator);

            //Convert screen pixel location to map location
            (double, double) clickedPoint = mapView.Map.Navigator.Viewport.ScreenToWorldXY((e.ScreenPosition.X) * screenRatios.Item1, (e.ScreenPosition.Y) * screenRatios.Item2);

            //Add to linework layer
            FillLinework(clickedPoint.Item1, clickedPoint.Item2);
        }
    }

    /// <summary>
    /// Will finalize linework if in drawing mode
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void mapView_LongTap(object sender, Mapsui.UI.TappedEventArgs e)
    {
        if (_isDrawingLine)
        {
            //Stop event tracking and finalize work
            FinalizeLinework(e);
        }

    }

    /// <summary>
    /// Make sure to stop waiting cursor if progress bar is visible
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MapPageProgressBar_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "IsVisible")
        {
            Border sentFrame = sender as Border;
            if (sentFrame != null && sentFrame.IsVisible && this.mapView != null)
            {
                this.WaitingCursor.IsRunning = false;
            }
        }
    }

    /// <summary>
    /// When the pop-up dialog to select a feature from inside a geopackage or a wms get cap gets visible event
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void MapAddGeopackageWMSFrame_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "IsVisible")
        {
            Border sentFrame = sender as Border;

            //Condition on map view not being null to prevent it from being launch and map page load
            if (sentFrame != null && !sentFrame.IsVisible && this.mapView != null)
            {
                //Start loading feature from selected
                MapViewModel _vm = BindingContext as MapViewModel;
                if (_vm.FeatureCollection != null && _vm.FeatureCollection.Count > 0)
                {
                    
                    string gpkgPath = string.Empty;
                    //Build list of names from selection
                    List<string> geopackageFeatureNames = new List<string>();
                    bool isGeopackage = true;

                    this.WaitingCursor.IsRunning = true;

                    foreach (MapPageLayerSelection mpls in _vm.FeatureCollection)
                    {
                        if (mpls.Selected)
                        {
                            geopackageFeatureNames.Add(mpls.Name);
                            gpkgPath = mpls.Path;

                            //In case it's a wms layer
                            if (mpls.URL != null && mpls.URL != string.Empty)
                            {
                                isGeopackage = false;
                                bool hasError = await AddAWMSAsync(mpls.URL, mpls.Name, mpls.ID);

                                if (hasError)
                                {
                                    //Someting fails during loading
                                    await Shell.Current.DisplayAlert(LocalizationResourceManager["MapPageAddWMSFailTitle"].ToString(),
                                        LocalizationResourceManager["MapPageAddWMSFailMessage"].ToString(),
                                        LocalizationResourceManager["GenericButtonOk"].ToString());
                                    new ErrorToLogFile(LocalizationResourceManager["MapPageAddWMSFailMessage"].ToString()).WriteToFile();
                                }
                            }

                        }
                    }

                    //Adding geopackage feature in bulk
                    if (isGeopackage)
                    {
                        await AddGPKG(geopackageFeatureNames, gpkgPath);
                    }

                    this.WaitingCursor.IsRunning = false;
                }
            }
        }
    }

    #endregion

    #region METHODS

    /// <summary>
    /// Will force a quick refresh on the feature layers like station and traverses
    /// </summary>
    /// <returns></returns>
    private async Task RefreshDefaultFeatureLayer()
    {
        if (!_isInitialLoadingDone)
        {
            MapViewModel _vm = BindingContext as MapViewModel;
            _vm.EmptyLayerCollections();

            List<MemoryLayer> mls = await CreateDefaultLayersAsync();
            if (mls != null && mls.Count() > 0)
            {
                foreach (MemoryLayer ml in mls)
                {
                    //Verify if doesn't exist and add
                    if (mapView.Map.Layers.Where(x => x.Name == ml.Name).Count() == 0)
                    {
                        mapView.Map.Layers.Add(ml);
                    }

                }

                //Zoom to initial extent of the station layer
                SetExtent(mls[0]);

            }
        }

    }

    /// <summary>
    /// Will force a quick refresh on the feature layers like station and traverses
    /// <paramref name="lightClean"> Will only add or remove different features, else clean slate option</paramref>
    /// </summary>
    /// <returns></returns>
    private async Task QuickRefreshDefaultFeatureLayer(bool lightClean = true)
    {
        //In case user is coming from field notes
        //They might have deleted some stations or linework, make sure to refresh
        foreach (var item in mapView.Map.Layers)
        {
            if (item.Name == ApplicationLiterals.aliasStations || item.Name == ApplicationLiterals.aliasLinework 
                || item.Name == ApplicationLiterals.aliasTraversePoint)
            {
                //Get map layer
                ILayer mapLayer = mapView.Map.Layers.Where(x => x.Name == item.Name).First();
                MemoryLayer mapMemoryLayer = mapLayer as MemoryLayer;

                //Get counts
                int databaseCount = 0;
                int mapLayerCount = mapMemoryLayer.Features.Count();
                defaultLayerList layerToReload = defaultLayerList.Stations;

                if (item.Name == ApplicationLiterals.aliasStations)
                {
                    databaseCount = await Task.Run(async () => await da.GetTableCount(typeof(FieldLocation)));  
                }
                else if (item.Name == ApplicationLiterals.aliasLinework)
                {
                    databaseCount = await Task.Run(async () => await da.GetTableCount(typeof(Linework)));  
                    layerToReload = defaultLayerList.Linework;
                }
                else if (true)
                {
                    databaseCount = await Task.Run(async () => await da.GetTableCount(typeof(TraversePoint)));
                    layerToReload = defaultLayerList.Traverses;
                }

                //Check with record count if diff add last or remove missing
                if (databaseCount != mapLayerCount)
                {
                    //Get latest features
                    MemoryLayer refreshLayer = await Task.Run(async () => await CreateDefaultLayerAsync(layerToReload));

                    //Detect missing features (user delete from field notes) and remove them
                    if (refreshLayer != null)
                    {
                        //Feature list to modify
                        List<IFeature> mapFeatures = mapMemoryLayer.Features.ToList();

                        //Light clean case
                        if (lightClean)
                        {
                            //Remove case
                            if (databaseCount < mapLayerCount)
                            {
                                foreach (IFeature feat in mapMemoryLayer.Features)
                                {
                                    if (refreshLayer.Features.Where(f => f.ToDisplayText() == feat.ToDisplayText()).Count() == 0)
                                    {
                                        mapFeatures.Remove(feat);
                                    }
                                }
                            }

                            //Add case
                            if (databaseCount > mapLayerCount)
                            {
                                foreach (IFeature rFeat in refreshLayer.Features)
                                {
                                    if (mapFeatures.Where(f => f.ToDisplayText() == rFeat.ToDisplayText()).Count() == 0)
                                    {
                                        mapFeatures.Add(rFeat);
                                    }
                                }
                            }

                        }
                        else
                        {
                            //Hard clean case
                            mapFeatures = refreshLayer.Features.ToList();
                        }

                        //Transform back
                        IEnumerable<IFeature> sourceEnumFeatures = mapFeatures.AsEnumerable();

                        //Reset
                        mapMemoryLayer.Tag = refreshLayer.Tag; //Keep tag, it contains db path, meant for map info
                        mapMemoryLayer.Features = sourceEnumFeatures;
                        mapView.Map.Layers.Remove(mapLayer);
                        mapView.Map.Layers.Add(mapMemoryLayer);

                    }
                }

                //Zoom to extent of stations
                if (layerToReload == defaultLayerList.Stations)
                {
                    SetExtent(mapMemoryLayer);
                }
            }
        }

        //Force redraw of all
        mapView.Map.RefreshData();
    }

    /// <summary>
    /// Will insert a given layer right before the drawable
    /// layers so it's always on top of previously added background map 
    /// data but always under the lines and points.
    /// </summary>
    /// <param name="in_layer"></param>
    private void InsertLayerAtRightPlace(ILayer in_layer)
    {
        //Insert at right location in collection
        List<ILayer> layerList = mapControl.Map.Layers.ToList();
        foreach (ILayer layer in layerList)
        {

            //Insert before the layer names drawables, WMS always should be beneath lines and points
            if (layer.Name.Contains(ApplicationLiterals.aliasMapsuiDrawables))
            {
                int rightIndex = layerList.IndexOf(layer);

                //make sure it's not already in there
                if (mapView.Map.Layers.Where(x => x.Name == in_layer.Name).Count() == 0)
                {
                    mapView.Map.Layers.Insert(rightIndex, in_layer);

                    MapViewModel _vm = BindingContext as MapViewModel;

                    //Update layer collection for menu
                    _vm.RefreshLayerCollection(mapView.Map.Layers);

                    //Save
                    _vm.SaveLayerRendering(in_layer);
                    break;
                }

            }

        }
    }

    /// <summary>
    /// Will try to do a get feature info from a WMS service
    /// TODO find why it doesn't work and why x and y are int rather then doubles
    /// </summary>
    public void GetWMSFeatureInfo(Mapsui.UI.Maui.Position inMouseClickPosition)
    {
        //Detect if top layer is wms
        List<ILayer> layerList = mapControl.Map.Layers.ToList();
        foreach (ILayer layer in layerList)
        {

            //Insert before the layer names drawables, WMS always should be beneath lines and points
            if (layer.Name.Contains(ApplicationLiterals.aliasMapsuiDrawables))
            {
                //Get top layer
                ILayer topLayer = layerList[layerList.IndexOf(layer) - 1];
                string topLayerTag = topLayer.Tag.ToString();
                if (topLayerTag.Contains("wms") && topLayerTag.Contains("version"))
                {
                    //Get wms version
                    string wmsVersion = topLayerTag.Split("wms?version=")[1].Split("&")[0];
                    if (wmsVersion != string.Empty)
                    { 

                        GetFeatureInfo gfi = new GetFeatureInfo();
                        gfi.Request(topLayerTag, wmsVersion, "image/png", mapControl.Map.CRS.ToString(), topLayer.Name, mapControl.Map.Extent.MinX,
                            mapControl.Map.Extent.MinY, mapControl.Map.Extent.MaxX, mapControl.Map.Extent.MaxY, (int)inMouseClickPosition.Longitude, (int)inMouseClickPosition.Latitude,
                            (int)mapControl.Width, (int)mapControl.Height);
                        gfi.IdentifyFinished += Gfi_IdentifyFinished;
                    }
                }
                break;
            }

        }
    }

    /// <summary>
    /// Will try to do a get feature info from a geopackage layer on screen
    /// </summary>
    /// <param name="inMouseClickPosition"></param>
    public void GetGpkgFeatureInfo(Mapsui.UI.Maui.Position inMouseClickPosition)
    {
        //Detect if top layer is wms
        List<ILayer> layerList = mapControl.Map.Layers.ToList();
        foreach (ILayer layer in layerList)
        {
            //Detect a valid layer (not field book, visible, not drawables)
            if (!layer.Name.Contains(ApplicationLiterals.aliasMapsuiDrawables) && layer.IsMapInfoLayer && layer.Enabled
                && layer.Tag != null && layer.Tag.ToString().Contains("gpkg") && !layer.Tag.ToString().Contains("version"))
            {

                ////Get wms version
                //string wmsVersion = topLayerTag.Split("wms?version=")[1].Split("&")[0];
                //if (wmsVersion != string.Empty)
                //{
                //    GetFeatureInfo gp = Mapsui.Providers.MemoryProvider.
                //    GetFeatureInfo gfi = new GetFeatureInfo();
                //    gfi.Request(topLayerTag, wmsVersion, "image/png", mapControl.Map.CRS.ToString(), topLayer.Name, mapControl.Map.Extent.MinX,
                //        mapControl.Map.Extent.MinY, mapControl.Map.Extent.MaxX, mapControl.Map.Extent.MaxY, (int)inMouseClickPosition.Longitude, (int)inMouseClickPosition.Latitude,
                //        (int)mapControl.Width, (int)mapControl.Height);
                //    gfi.IdentifyFinished += Gfi_IdentifyFinished;
                //}
                
                break;
            }

        }
    }

    /// <summary>
    /// On init will reload user prefered layers from saved JSON file.
    /// It'll also do some clean up making sure only desired layers are loaded.
    /// </summary>
    /// <returns></returns>
    public async Task LoadPreferedLayers()
    {
        if (!_isInitialLoadingDone)
        {
            try
            {
                //Clean before load
                //Must match layer name else it'll be removed.
                List<string> prefLayerNames = new List<string>();
                prefLayerNames.Add(ApplicationLiterals.aliasOSM);
                prefLayerNames.Add(ApplicationLiterals.aliasTraversePoint);
                prefLayerNames.Add(ApplicationLiterals.aliasStations);
                prefLayerNames.Add(ApplicationLiterals.aliasLinework);
                prefLayerNames.Add(ApplicationLiterals.aliasMapsuiDrawables);
                prefLayerNames.Add(ApplicationLiterals.aliasMapsuiLayer);
                prefLayerNames.Add(ApplicationLiterals.aliasMapsuiCallouts);
                prefLayerNames.Add(ApplicationLiterals.aliasMapsuiPins);

                ILayer[] layerToRemove = mapView.Map.Layers.Where(x => !prefLayerNames.Contains(x.Name)).ToArray();
                if (layerToRemove.Count() > 0)
                {
                    mapView.Map.Layers.Remove(layerToRemove);
                }

                //Get prefered layers and add them
                MapViewModel _vm = BindingContext as MapViewModel;
                Collection<MapPageLayer> prefLayers = await _vm.GetLayerRendering();

                if (prefLayers != null && mapView != null)
                {

                    foreach (MapPageLayer mpl in prefLayers)
                    {
                        if (mpl.LayerName != ApplicationLiterals.aliasStations 
                            && mpl.LayerName != ApplicationLiterals.aliasLinework
                            && mpl.LayerName != ApplicationLiterals.aliasTraversePoint)
                        {
                            if (mpl.LayerType == MapPageLayer.LayerTypes.mbtiles)
                            {
                                await AddAMBTile(mpl.LayerPathOrURL, mpl);
                            }
                            else if (mpl.LayerType == MapPageLayer.LayerTypes.wms)
                            {
                                await AddAWMSAsync(mpl.LayerPathOrURL, mpl.LayerName, mpl.LayerID, true, mpl);
                            }
                            else if (mpl.LayerType == MapPageLayer.LayerTypes.gpkg)
                            {
                                await AddGPKG(new List<string> { mpl.LayerName }, mpl.LayerPathOrURL, mpl);
                            }
                        }

                    }
                }
            }
            catch (System.Exception e)
            {
                new ErrorToLogFile(e).WriteToFile();
            }

        }

    }

    /// <summary>
    /// Will add an mbtile file to map page from incoming file path
    /// </summary>
    /// <param name="mbTilePath"></param>
    /// <returns></returns>
    public async Task AddAMBTile(string mbTilePath, MapPageLayer pageLayer = null)
    {
        if (mbTilePath != null && mbTilePath != string.Empty)
        {
            try
            {
                MbTilesTileSource mbtilesTilesource = new MbTilesTileSource(new SQLiteConnectionString(mbTilePath, false));
                byte[] tileSource = await mbtilesTilesource.GetTileAsync(new TileInfo { Index = new TileIndex(0, 0, 0) });

                TileLayer newTileLayer = new TileLayer(mbtilesTilesource);
                newTileLayer.Name = Path.GetFileNameWithoutExtension(mbTilePath);
                newTileLayer.Tag = mbTilePath;

                //If full object is passed, get extra info in
                if (pageLayer != null)
                {
                    newTileLayer.Opacity = pageLayer.LayerOpacity;
                    newTileLayer.Enabled = pageLayer.LayerVisibility;
                }

                //Insert at right location in collection
                InsertLayerAtRightPlace(newTileLayer);
            }
            catch (System.Exception e)
            {
                new ErrorToLogFile(e).WriteToFile();
            }

        }

    }

    /// <summary>
    /// Will add a geopackage file to map page from incoming file path
    /// </summary>
    /// <param name="gpkgPath"></param>
    /// <returns></returns>
    public async Task AddGPKG(List<string> featuresToAdd, string gpkgPath, MapPageLayer pageLayer = null)
    {
        if (gpkgPath != null && gpkgPath != string.Empty && featuresToAdd.Count > 0)
        {
            try
            {
                //Prep
                SQLiteAsyncConnection gpkgConnection = new SQLiteAsyncConnection(gpkgPath);
                GeopackageService gpkgService = new GeopackageService();
                bool hitError = false; //to break all loops in case of error
                WKTReader wellKnownTextReader = new WKTReader();
                Color defaultColor = GetColorFromString("Grey");

                foreach (string features in featuresToAdd)
                {

                    //Error handling
                    if (hitError)
                    {
                        break;
                    }

                    //Validate number of geometries before actually proceed
                    string getGeomCount = string.Format("SELECT COUNT(*) FROM {0};", features);
                    List<int> geomCount = await gpkgConnection.QueryScalarsAsync<int>(getGeomCount);
                    bool canContinue = true;
                    if (geomCount.Count > 0 && geomCount[0] > 1000)
                    {
                        try
                        {
                            canContinue = await DisplayAlert(LocalizationResourceManager["GenericWarningTitle"].ToString(),
                                LocalizationResourceManager["MapPageTooManyGeometriesMessage"].ToString(),
                                LocalizationResourceManager["GenericButtonYes"].ToString(),
                                LocalizationResourceManager["GenericButtonNo"].ToString());
                        }
                        catch (System.Exception)
                        {

                        }

                    }

                    if (canContinue) 
                    {
                        //Will hold the geometries assembled as a new feature
                        IEnumerable<IFeature> feats = new IFeature[] { };

                        //Get geometry column name
                        string getGeomColumnNameQuery = string.Format("SELECT {0} FROM {1} where {2} = '{3}'",
                            GeopackageService.GpkgFieldGeometryColumnName,
                            GeopackageService.GpkgTableGeometry,
                            GeopackageService.GpkgFieldTableName,
                            features);
                        List<string> geomName = await gpkgConnection.QueryScalarsAsync<string>(getGeomColumnNameQuery);

                        //Get geometry type and load each one
                        string getGeomTypeQuery = string.Format("SELECT {0} FROM {1} WHERE {2} = '{3}';",
                            GeopackageService.GpkgFieldGeometryType,
                            GeopackageService.GpkgTableGeometry,
                            GeopackageService.GpkgFieldTableName,
                            features);
                        List<string> typeGeometries = await gpkgConnection.QueryScalarsAsync<string>(getGeomTypeQuery);

                        if (typeGeometries != null && typeGeometries.Count() > 0)
                        {
                            //Keep track of loading progress if coming from button and not preloading prefered layers
                            if (pageLayer == null)
                            {
                                this.MapPageProgressBar.Progress = 0;
                                this.MapPageProgressBar.IsVisible = true;
                            }


                            foreach (string geomType in typeGeometries)
                            {

                                //Error handling
                                if (hitError)
                                {
                                    break;
                                }

                                //Get some styling in
                                string xmlStyle = await Task.Run(async () => await gpkgService.GetGeopackageStyleXMLString(gpkgConnection, features));
                                List<GeopackageLayerStyling> stylings = await Task.Run(async () => await gpkgService.GetGeopackageStyle(xmlStyle, features, geomType));

                                //Get geometry column name
                                string currentGeomName = GeopackageService.GpkgFieldGeometry;
                                if (geomName.Count() > 0)
                                {
                                    currentGeomName = geomName[0];
                                }

                                //Get geometry from bytes
                                string getGeomQuery_base = string.Format("SELECT {0} FROM {1} ;", currentGeomName, features);
                                string getGeomQuery = getGeomQuery_base;

                                foreach (GeopackageLayerStyling styling in stylings)
                                {
                                    if (styling.className != string.Empty && styling.classValue != string.Empty)
                                    {
                                        //Add a where clause
                                        getGeomQuery = getGeomQuery_base.Replace(";", "") + string.Format("WHERE {0} = '{1}' ;", styling.className, styling.classValue);
                                    }
                                    List<byte[]> featGeometries = await gpkgConnection.QueryScalarsAsync<byte[]>(getGeomQuery);

                                    if (featGeometries != null && featGeometries.Count() > 0)
                                    {
                                        foreach (byte[] geomBytes in featGeometries)
                                        {
                                            if (pageLayer == null)
                                            {
                                                this.MapPageProgressBar.Progress = (double)(featGeometries.IndexOf(geomBytes) + 1) / featGeometries.Count();
                                            }

                                            //Prepare mapsui feature
                                            IFeature feat = null;

                                            //From bytes get feature object
                                            if (geomType.ToLower() == Geometry.TypeNameMultiPolygon.ToLower())
                                            {
                                                //Run on another thread else progress bar gets jammed and won't update in the UI
                                                MultiPolygon multiPolygon = await Task.Run(async () => await gpkgService.GetGeometryPolygonFromByte(geomBytes));

                                                //Get feature 
                                                if (multiPolygon != null)
                                                {
                                                    //Build feature metadata
                                                    feat = new GeometryFeature(wellKnownTextReader.Read(multiPolygon.AsText()));
                                                }
                                                else
                                                {
                                                    //Stop everything
                                                    hitError = true;
                                                    break;
                                                }

                                                //Get default or user line style 
                                                feat.Styles.Add(styling.polyVectorStyle);

                                            }
                                            else if (geomType.ToLower() == Geometry.TypeNameLineString.ToLower())
                                            {
                                                //Run on another thread else progress bar gets jammed and won't update in the UI
                                                LineString lines = await Task.Run(async () => await gpkgService.GetGeometryLineFromByte(geomBytes));

                                                //Get feature 
                                                if (lines != null)
                                                {
                                                    //Build feature metadata
                                                    feat = new GeometryFeature(wellKnownTextReader.Read(lines.AsText()));
                                                }
                                                else
                                                {
                                                    //Stop everything
                                                    hitError = true;
                                                    break;
                                                }

                                                //Get default or user line style 
                                                feat.Styles.Add(styling.lineVectorStyle);

                                            }
                                            else if (geomType.ToLower() == Geometry.TypeNameMultiLineString.ToLower())
                                            {
                                                //Run on another thread else progress bar gets jammed and won't update in the UI
                                                MultiLineString lines = await Task.Run(async () => await gpkgService.GetGeometryMultiLineFromByte(geomBytes));

                                                //Get feature 
                                                if (lines != null)
                                                {
                                                    //Build feature metadata
                                                    feat = new GeometryFeature(wellKnownTextReader.Read(lines.AsText()));
                                                }
                                                else
                                                {
                                                    //Stop everything
                                                    hitError = true;
                                                    break;
                                                }

                                                //Get default or user line style 
                                                feat.Styles.Add(styling.lineVectorStyle);

                                            }
                                            else if (geomType.ToLower() == Geometry.TypeNamePoint.ToLower())
                                            {
                                                //Run on another thread else progress bar gets jammed and won't update in the UI
                                                Point pnts = await Task.Run(async () => await gpkgService.GetGeometryPointFromByteAsync(geomBytes));

                                                //Get feature 
                                                if (pnts != null)
                                                {
                                                    //Build feature metadata
                                                    feat = new GeometryFeature(wellKnownTextReader.Read(pnts.AsText()));

                                                }
                                                else
                                                {
                                                    //Stop everything
                                                    hitError = true;
                                                    break;
                                                }

                                                //Style point
                                                feat.Styles.Add(styling.pointVectorStyle);
                                            }

                                            //Keep hex of geometry as id for futur get feature info from user
                                            if (geomName != null && geomName.Count > 0)
                                            {
                                                feat[geomName[0]] = Convert.ToHexString(geomBytes);
                                            }

                                            //Add to list of features
                                            feats = feats.Append(feat);
                                        }
                                    }

                                }

                                if (pageLayer == null)
                                {
                                    this.MapPageProgressBar.IsVisible = false;
                                }
                            }
                        }


                        //Create layer
                        //Need to set style to null else points will come with a big white circle beneath them
                        MemoryLayer newMemLayer = new MemoryLayer()
                        {
                            Name = features,
                            IsMapInfoLayer = true,
                            Features = feats,
                            Style = null,
                            Tag = gpkgPath,
                        };

                        //If full object is passed, get extra info in
                        if (pageLayer != null)
                        {
                            newMemLayer.Opacity = pageLayer.LayerOpacity;
                            newMemLayer.Enabled = pageLayer.LayerVisibility;
                        }

                        //Add to map
                        InsertLayerAtRightPlace(newMemLayer);
                    }


                }

                await gpkgConnection.CloseAsync();
            }
            catch (System.Exception e)
            {
                try
                {
                    await Shell.Current.DisplayAlert(LocalizationResourceManager["GenericErrorTitle"].ToString(),
                        e.Message,
                        LocalizationResourceManager["GenericButtonOk"].ToString());
                }
                catch (System.Exception)
                {

                }


                new ErrorToLogFile(e).WriteToFile();
                this.MapPageProgressBar.IsVisible = false;
            }

        }

    }

    /// <summary>
    /// Will add a geopackage file to map page from incoming file path
    /// </summary>
    /// <param name="gpkgPath"></param>
    /// <returns></returns>
    public async Task AddFromGPKG(string gpkgPath, MapPageLayer pageLayer = null)
    {
        if (gpkgPath != null && gpkgPath != string.Empty)
        {
            try
            {

                //Get list of all features
                SQLiteAsyncConnection currentConnection = new SQLiteAsyncConnection(gpkgPath);
                string getFeaturesQuery = string.Format("SELECT {0} FROM {1};", GeopackageService.GpkgFieldTableName, GeopackageService.GpkgTableGeometry);
                List<string> gpkgFeatures = await currentConnection.QueryScalarsAsync<string>(getFeaturesQuery);

                if (gpkgFeatures != null && gpkgFeatures.Count() > 0)
                {
                    //Ask user which feature they want to load up
                    MapViewModel vm = BindingContext as MapViewModel;
                    vm.FillGeopackageFeatureCollection(gpkgFeatures, gpkgPath);
                    
                }

                await currentConnection.CloseAsync();

            }
            catch (System.Exception e)
            {
                await Shell.Current.DisplayAlert(LocalizationResourceManager["GenericErrorTitle"].ToString(),
                    e.Message,
                    LocalizationResourceManager["GenericButtonOk"].ToString());

                new ErrorToLogFile(e).WriteToFile();
            }

        }

    }

    /// <summary>
    /// Will zoom to the extent of the incoming memory layer
    /// </summary>
    /// <param name="inLayer"></param>
    public void SetExtent(ILayer inLayer = null)
    {
        if (inLayer != null && inLayer.Extent != null)
        {
            // Extend a bit more the rectangle by 2.5km
            var fieldDataExtent = new MRect(inLayer.Extent.MinX,
                inLayer.Extent.MinY,
                inLayer.Extent.MaxX,
                inLayer.Extent.MaxY).Grow(2500);
            double currentArea = mapView.Map.Navigator.Viewport.ToExtent().GetArea();
            double zoomingToArea = fieldDataExtent.GetArea();

            //Zoom to extent of all stations, unless current extent is already smaller
            if (inLayer.Name == ApplicationLiterals.aliasStations && currentArea > zoomingToArea)
            {
                mapView.Map.Navigator.ZoomToBox(box: fieldDataExtent, boxFit: MBoxFit.Fit);
            }
            else if (inLayer.Name != ApplicationLiterals.aliasStations)
            {
                mapView.Map.Navigator.ZoomToBox(box: fieldDataExtent, boxFit: MBoxFit.Fit);
            }

        }
        else
        {
            // Fit to Canada with a 25km buffer
            var fieldDataExtent = new MRect(-15658662, 5533994, -5231695, 11534073).Grow(25000);
            mapView.Map.Navigator.ZoomToBox(box: fieldDataExtent, boxFit: MBoxFit.Fit);
        }


    }

    /// <summary>
    /// Will call open street map tile layers
    /// And will also set up a persistant cache mode so user can
    /// go offline and still navigate through open street map
    /// </summary>
    /// <param name="withCache"></param>
    public void SetOpenStreetMap(bool withCache = true)
    {
        if (!_isInitialLoadingDone)
        {
            if (withCache)
            {
                var persistentCache = new SqlitePersistentCache(ApplicationLiterals.keywordWMS + "_OSM");
                HttpTileSource source = KnownTileSources.Create(KnownTileSource.OpenStreetMap, ApplicationLiterals.keywordWMS + "/3.0 Maui.net", persistentCache: persistentCache);
                TileLayer osmLayer = new TileLayer(source);
                osmLayer.Name = ApplicationLiterals.aliasOSM;

                //Prevent duplicates
                if (mapControl.Map.Layers.Where(n=>n.Name == ApplicationLiterals.aliasOSM).ToList().Count() == 0)
                {
                    mapControl.Map.Layers.Insert(0, osmLayer);
                }

            }
            else
            {
                TileLayer osmLayer = Mapsui.Tiling.OpenStreetMap.CreateTileLayer(ApplicationLiterals.keywordWMS + "/3.0 Maui.net");
                osmLayer.Name = ApplicationLiterals.aliasOSM;

                //Prevent duplicates
                if (mapControl.Map.Layers.Where(n => n.Name == ApplicationLiterals.aliasOSM).ToList().Count() == 0)
                {
                    mapControl.Map.Layers.Insert(0, osmLayer);
                }
            }
        }


    }

    /// <summary>
    /// Will add a new WMS layer into the map page
    /// </summary>
    /// <param name="wmsURL"></param>
    /// <param name="withCache"></param>
    public async Task<bool> AddAWMSAsync(string wmsURL, string layerName, string layerID, bool withCache = true, MapPageLayer pageLayer = null)
    {
        bool hasError = false;

        if (wmsURL != null && wmsURL != string.Empty)
        {
            //Get tiling scheme for tiles and their resolution set from csr
            TileSchema schema = new GlobalSphericalMercator { Format = "image/png"};

            //Special schema for canadian atlas
            if (_wmsCRS == ApplicationLiterals.SupportedWMSCRS.epsg3978)
            {
                TileSchema3978 tileSchema3978 = new TileSchema3978();

                //If wms crs extent was parsed correctly from get cap, set tileschema with it
                if (_wmsCRSExtent != null)
                {
                    tileSchema3978 = new TileSchema3978(_wmsCRSExtent);
                }
                
                await tileSchema3978.TransformTo(3857);
                schema = tileSchema3978;

            }
            else if (_wmsCRS == ApplicationLiterals.SupportedWMSCRS.epsg4326)
            {
                TileSchema4326 tileSchema4326 = new TileSchema4326();

                //If wms crs extent was parsed correctly from get cap, set tileschema with it
                if (_wmsCRSExtent != null)
                {
                    tileSchema4326 = new TileSchema4326(_wmsCRSExtent);
                }

                await tileSchema4326.TransformTo(3857);
                schema = tileSchema4326;
            }

            WmscRequest request = new WmscRequest(new Uri(wmsURL), schema, [layerID]);
            
            if (request != null)
            {
                SqlitePersistentCache wmsCache = null;
                if (withCache)
                {
                    wmsCache = new SqlitePersistentCache(ApplicationLiterals.keywordWMS + layerName.Replace(':', '_'));
                }

                HttpTileProvider provider = new HttpTileProvider(request, wmsCache);
                TileSource t = new TileSource(provider, schema);
                TileLayer tl = new TileLayer(t);
                tl.Name = layerName;
                tl.Tag = wmsURL;

                if (pageLayer != null)
                {
                    tl.Opacity = pageLayer.LayerOpacity;
                    tl.Enabled = pageLayer.LayerVisibility;
                }

                //Insert at right location in collection
                InsertLayerAtRightPlace(tl);

            }
            else
            {
                hasError = true;
            }

        }

        return hasError;

    }

    /// <summary>
    /// Method to change current location symbol.
    /// NOTE: not doable for now https://github.com/Mapsui/Mapsui/issues/618
    /// </summary>
    /// <param name="accuracy"></param>
    /// <returns></returns>
    public static async Task<IStyle> SetAccuracyAndLocationGraphic(double? accuracy)
    {

        //Init symbols
        Brush locationBrush = new Brush(Mapsui.Styles.Color.FromString("LightGreen"));
        if (App.Current.Resources.TryGetValue("PositionColor", out var colorvalue))
        {
            //Need to cast in right color object
            Microsoft.Maui.Graphics.Color posColor = colorvalue as Microsoft.Maui.Graphics.Color;
            locationBrush.Color = posColor.ToMapsui();
        }

        //Parse accuracy to change color
        if (accuracy > 20.0 && accuracy <= 40.0)
        {
            if (App.Current.Resources.TryGetValue("WarningColor", out var warningColorvalue))
            {
                //Need to cast in right color object
                Microsoft.Maui.Graphics.Color warnColor = warningColorvalue as Microsoft.Maui.Graphics.Color;
                locationBrush.Color = warnColor.ToMapsui();
            }

        }
        else if (accuracy > 40.0)
        {
            if (App.Current.Resources.TryGetValue("ErrorColor", out var errorColorvalue))
            {
                //Need to cast in right color object
                Microsoft.Maui.Graphics.Color erColor = errorColorvalue as Microsoft.Maui.Graphics.Color;
                locationBrush.Color = erColor.ToMapsui();

            }

        }
        else if (accuracy == 0.0)
        {
            if (App.Current.Resources.TryGetValue("ErrorColor", out var errorColorvalue))
            {
                //Need to cast in right color object
                Microsoft.Maui.Graphics.Color noPosColor = errorColorvalue as Microsoft.Maui.Graphics.Color;
                locationBrush.Color = noPosColor.ToMapsui();

            }

        }

        IStyle pointStyle = new SymbolStyle()
        {
            SymbolScale = 1.30,
            Fill = new Brush(locationBrush)
        };

        return pointStyle;
    }

    /// <summary>
    /// Method to change current location symbol.
    /// NOTE: not doable for now https://github.com/Mapsui/Mapsui/issues/618
    /// </summary>
    /// <param name="accuracy"></param>
    /// <returns></returns>
    public async Task SetMapAccuracyColor(double? accuracy)
    {

        //Init symbols
        if (App.Current.Resources.TryGetValue("PositionColor", out var colorvalue))
        {
            mapPageGrid.BackgroundColor = colorvalue as Microsoft.Maui.Graphics.Color;
            GPSMode.TextColor = colorvalue as Microsoft.Maui.Graphics.Color;
        }

        //Parse accuracy to change color
        if (accuracy > 20.0 && accuracy <= 40.0)
        {
            if (App.Current.Resources.TryGetValue("WarningColor", out var warningColorvalue))
            {
                mapPageGrid.BackgroundColor = warningColorvalue as Microsoft.Maui.Graphics.Color;
                GPSMode.TextColor = warningColorvalue as Microsoft.Maui.Graphics.Color;
            }


        }
        else if (accuracy > 40.0)
        {
            if (App.Current.Resources.TryGetValue("ErrorColor", out var errorColorvalue))
            {
                mapPageGrid.BackgroundColor = errorColorvalue as Microsoft.Maui.Graphics.Color;
                GPSMode.TextColor = errorColorvalue as Microsoft.Maui.Graphics.Color;
            }

        }
        else if (accuracy == -99)
        {
            if (App.Current.Resources.TryGetValue("Gray400", out var errorColorvalue))
            {
                mapPageGrid.BackgroundColor = errorColorvalue as Microsoft.Maui.Graphics.Color;
                GPSMode.TextColor = errorColorvalue as Microsoft.Maui.Graphics.Color;
            }
        }
    }

    /// <summary>
    /// Must add all image in bitmap registry for mapsui to use them as symbol styles
    /// </summary>
    /// <returns></returns>
    public async Task<int> AddLocationSymbolToRegistry()
    {
        //Make sure it's not already registered
        if (!BitmapRegistry.Instance.TryGetBitmapId(nameof(bitmapLocationSymbolId), out bitmapLocationSymbolId))
        {
            //Stream the pnt for the symbol
            await using (Stream pointBitmap = await FileSystem.OpenAppPackageFileAsync(@"point.png"))
            {
                MemoryStream memoryStream = new MemoryStream();
                pointBitmap.CopyTo(memoryStream);

                //Register
                bitmapLocationSymbolId = BitmapRegistry.Instance.Register(memoryStream, nameof(bitmapLocationSymbolId));

            }

            return bitmapLocationSymbolId;
        }
        else
        {
            return bitmapLocationSymbolId;
        }
    }

    /// <summary>
    /// Must add all image in bitmap registry for mapsui to use them as symbol styles
    /// </summary>
    /// <returns></returns>
    public async Task<int> AddLineworkPointSymbolToRegistry()
    {
        //Make sure it's not already registered
        if (!BitmapRegistry.Instance.TryGetBitmapId(nameof(bitmapLineworkPointSymbolId), out bitmapLineworkPointSymbolId))
        {
            //Stream the pnt for the symbol
            await using (Stream pointBitmap = await FileSystem.OpenAppPackageFileAsync(@"vector-point.png"))
            {
                MemoryStream memoryStream = new MemoryStream();
                pointBitmap.CopyTo(memoryStream);

                //Register
                bitmapLineworkPointSymbolId = BitmapRegistry.Instance.Register(memoryStream, nameof(bitmapLineworkPointSymbolId));

            }

            return bitmapLineworkPointSymbolId;
        }
        else
        {
            return bitmapLineworkPointSymbolId;
        }
    }

    /// <summary>
    /// Will open a file picker dialog with custom extension set to mbtiles
    /// </summary>
    /// <returns></returns>
    public async Task<FileResult> PickLayer()
    {

        
        try
        {
            // NOTE: for Android application/mbtiles doesn't work
            // we need to get all and filter out only mbtile selected files.

            FilePickerFileType customFileType = new FilePickerFileType(
                new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                        {DevicePlatform.WinUI, new [] { DatabaseLiterals.LayerTypeMBTiles, DatabaseLiterals.LayerTypeGPKG} },
                        {DevicePlatform.Android, new [] { "application/*"} },
                        {DevicePlatform.iOS, new [] { DatabaseLiterals.LayerTypeMBTiles, DatabaseLiterals.LayerTypeGPKG } },
                });

            PickOptions options = new PickOptions();
            options.PickerTitle = "Add Layer";
            options.FileTypes = customFileType;

            var result = await FilePicker.Default.PickAsync(options);
            if (result != null)
            {
                if (result.FileName.Contains(DatabaseLiterals.LayerTypeMBTiles) ||
                    result.FileName.Contains(DatabaseLiterals.LayerTypeGPKG))
                {
                    return result;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }

            
        }
        catch (System.Exception ex)
        {
            new ErrorToLogFile(ex).WriteToFile();
        }

        return null;
    }

    /// <summary>
    /// Will create a new point layer for station or traverse location
    /// </summary>
    /// <returns></returns>
    private async Task<List<MemoryLayer>> CreateDefaultLayersAsync()
    {
        List<MemoryLayer> defaultLayers = new List<MemoryLayer>();

        foreach (int i in Enum.GetValues(typeof(defaultLayerList)))
        {
            MemoryLayer dLayer = await CreateDefaultLayerAsync((defaultLayerList)i);

            if (dLayer != null)
            {
                dLayer.Tag = da.PreferedDatabasePath; //Keep database path for map info button and zoom to extent
                defaultLayers.Add(dLayer);
            }
        }

        return defaultLayers;
    }

    /// <summary>
    /// Will query the database to retrive some basic information about location
    /// and stations
    /// </summary>
    /// <returns></returns>
    private async Task<IEnumerable<IFeature>> GetGeometriesAsync(defaultLayerList inLayer)
    {
        IEnumerable<IFeature> enumFeat = new IFeature[] { };

        if (da.PreferedDatabasePath != null && da.PreferedDatabasePath != string.Empty && da.PreferedDatabasePath != da.DatabaseFilePath)
        {

            //Get an offset placement for labels
            GraphicPlacement gp = new GraphicPlacement();
            List<int> placementPool = Enumerable.Range(1, 8).ToList();
            Offset offset = new Offset();
            offset.X = gp.GetOffsetFromPlacementPriority(placementPool[2],32,32).Item1;
            offset.Y = gp.GetOffsetFromPlacementPriority(placementPool[2],32,32).Item2;

            switch(inLayer) 
            {
                case defaultLayerList.Stations:

                    enumFeat = await Task.Run(async () => await GetStationLocationsAsync(offset)); 
                    break;

                case defaultLayerList.Traverses:

                    enumFeat = await Task.Run(async () => await GetPointTraversesAsync(offset)); 
                    break;
                case defaultLayerList.Linework:
                    enumFeat = await Task.Run(async () => await GetLineworkAsync(offset)); 
                    break;
                default:
                    enumFeat = await Task.Run(async () => await GetStationLocationsAsync(offset)); 

                    break;

            }

        }

        return enumFeat;

    }

    /// <summary>
    /// Will query the database to retrive some basic information about location
    /// and stations
    /// </summary>
    /// <returns></returns>
    private async Task<IEnumerable<IFeature>> GetStationLocationsAsync(Offset offset)
    {
        IEnumerable<IFeature> enumFeat = new IFeature[] { };

        if (da.PreferedDatabasePath != null && da.PreferedDatabasePath != string.Empty)
        {
            //Prep
            List<FieldLocation> fieldLoc = await DataAccess.DbConnection.Table<FieldLocation>().ToListAsync();
            List<Station> fieldStat = await DataAccess.DbConnection.Table<Station>().ToListAsync();

            LabelStyle labelStyle = new LabelStyle
            {
                BackColor = new Brush(Color.WhiteSmoke),
                HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Right,
                BorderThickness = 2,
                Offset = offset
            };

            await Parallel.ForEachAsync(fieldLoc, _parallelOptions, async (fl, token) =>
            {

                //Get coordinate as EPSG 3857 (Spherical mercator WGS84)
                //Build geometry
                NetTopologySuite.Geometries.Point locationPoint = await _geopackageService.GetGeometryPointFromByteAsync(fl.LocationGeometry);

                if (locationPoint != null)
                {
                    //Get some station info for labelling
                    string label = fl.LocationAliasLight;
                    Station station = fieldStat.Where(n => n.LocationID == fl.LocationID).FirstOrDefault();
                    if (station != null)
                    {
                        label = station.StationAliasLight;
                    }

                    //Build feature 
                    Mapsui.Nts.GeometryFeature feat = new Mapsui.Nts.GeometryFeature(locationPoint);
                    feat[DatabaseLiterals.FieldStationObsID] = fl.LocationID;
                    LabelStyle lStyle = new LabelStyle
                    {
                        BackColor = labelStyle.BackColor,
                        HorizontalAlignment = labelStyle.HorizontalAlignment,
                        BorderThickness = labelStyle.BorderThickness,
                        Offset = offset,
                        Text = label
                    };

                    feat.Styles.Add(lStyle);

                    enumFeat = enumFeat.Append(feat);
                }
            });

        }

        return enumFeat;

    }

    /// <summary>
    /// Will query the database to retrive some basic information about location
    /// and stations
    /// </summary>
    /// <returns></returns>
    private async Task<IEnumerable<IFeature>> GetPointTraversesAsync(Offset offset)
    {
        IEnumerable<IFeature> enumFeat = new IFeature[] { };

        if (da.PreferedDatabasePath != null && da.PreferedDatabasePath != string.Empty)
        {
            //Prep
            List<TraversePoint> fieldTravPoint = await DataAccess.DbConnection.Table<TraversePoint>().ToListAsync();

            LabelStyle labelStyle = new LabelStyle
            {
                BackColor = new Brush(Color.LightGoldenRodYellow),
                HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Right,
                BorderThickness = 2,
                Offset = offset
            };

            if (fieldTravPoint != null && fieldTravPoint.Count > 0)
            {
                //Prep style
                string xmlStyle = await Task.Run(async () => await _geopackageService.GetGeopackageStyleXMLString(DataAccess.DbConnection, DatabaseLiterals.TableTraversePoint));
                List<GeopackageLayerStyling> stylings = await Task.Run(
                    async () => await _geopackageService.GetGeopackageStyle(
                        xmlStyle, 
                        DatabaseLiterals.TableTraversePoint, 
                        Geometry.TypeNamePoint.ToLower()));

                await Parallel.ForEachAsync(fieldTravPoint, _parallelOptions, async (tp, token) =>
                {
                    //Build geometry
                    NetTopologySuite.Geometries.Point travPointString = await Task.Run(async () => await _geopackageService.GetGeometryPointFromByteAsync(tp.TravGeom));

                    if (travPointString != null)
                    {
                        //Build feature metadata
                        Mapsui.Nts.GeometryFeature feat = new Mapsui.Nts.GeometryFeature(travPointString);
                        feat[DatabaseLiterals.FieldTravPointID] = tp.TravID;
                        enumFeat = enumFeat.Append(feat);

                        //Style geom and label
                        if (stylings.Count() > 0)
                        {
                            feat.Styles.Add(stylings[0].pointVectorStyle);
                        }

                        LabelStyle lStyle = new LabelStyle
                        {
                            BackColor = labelStyle.BackColor,
                            HorizontalAlignment = labelStyle.HorizontalAlignment,
                            BorderThickness = labelStyle.BorderThickness,
                            Offset = offset,
                            Text = tp.TravLabel,
                        };

                        feat.Styles.Add(lStyle);
                    }
                });

            }

        }

        return enumFeat;

    }

    /// <summary>
    /// Will query the database to retrive some basic information about linework feature
    /// </summary>
    /// <param name="offset"></param>
    /// <returns></returns>
    private async Task<IEnumerable<IFeature>> GetLineworkAsync(Offset offset)
    {
        IEnumerable<IFeature> enumFeat = new IFeature[] { };
        if (da.PreferedDatabasePath != null && da.PreferedDatabasePath != string.Empty)
        {
            //Prep
            List<Linework> fieldLinework = await DataAccess.DbConnection.Table<Linework>().ToListAsync();
            LabelStyle labelStyle = new LabelStyle
            {
                BackColor = new Brush(Color.WhiteSmoke),
                HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Right,
                BorderThickness = 1
            };

            if (fieldLinework != null && fieldLinework.Count() > 0)
            {
                foreach (Linework lw in fieldLinework)
                {
                    //Build geometry
                    LineString inLineString = await Task.Run(async () => await _geopackageService.GetGeometryLineFromByte(lw.LineGeom)); 

                    if (inLineString != null)
                    {
                        //Build feature metadata
                        Mapsui.Nts.GeometryFeature feat = new Mapsui.Nts.GeometryFeature(inLineString);
                        
                        if (feat != null)
                        {
                            feat[DatabaseLiterals.FieldLineworkID] = lw.LineID;

                            //Add to list of features
                            enumFeat = enumFeat.Append(feat);

                            //Get true line color
                            Color lineColor = GetColorFromString(lw.LineSymbol);
                            
                            //Style line and label
                            feat.Styles.Add(new VectorStyle { Line = new Pen(lineColor, 3) });
                            feat.Styles.Add(new LabelStyle
                            {
                                Text = lw.LineAliasLight,
                                BackColor = labelStyle.BackColor,
                                HorizontalAlignment = labelStyle.HorizontalAlignment,
                                BorderThickness = labelStyle.BorderThickness,
                            });
                        }

                    }


                }
            }
        }

        return enumFeat;
    
    }

    /// <summary>
    /// Will set style of stations on map
    /// </summary>
    /// <returns></returns>
    private SymbolStyle CreateLocationBitmapStyle()
    {
        // For this sample we get the bitmap from an embedded resouce
        // but you could get the data stream from the web or anywhere
        // else.

        return new SymbolStyle { BitmapId = bitmapLocationSymbolId, SymbolScale = 0.75 };
    }

    /// <summary>
    /// Will set style of stations on map
    /// </summary>
    /// <returns></returns>
    private SymbolStyle CreateVectorBitmapStyle()
    {
        // For this sample we get the bitmap from an embedded resouce
        // but you could get the data stream from the web or anywhere
        // else.

        return new SymbolStyle { BitmapId = bitmapLineworkPointSymbolId, SymbolScale = 1.5 };
    }

    /// <summary>
    /// Will init and fill a drawable feature that will then be converted to a F_LINE_WORK when done.
    /// </summary>
    /// <param name="xCoord"></param>
    /// <param name="yCoord"></param>
    private async void FillLinework(double xCoord, double yCoord)
    {
        //Make sure styling and layer are ready
        await InitiateLinework();

        Tuple<GeometryFeature, GeometryFeature> newFeat = await AddPointToLinework(xCoord, yCoord);

        //Force it to drawable line layer
        IEnumerable<ILayer> layers = mapView.Map.Layers.FindLayer(ApplicationLiterals.aliasLineworkEdit);
        if (layers != null && layers.First() != null && newFeat != null)
        {

            WritableLayer writeLayer = layers.First() as WritableLayer;
            if (writeLayer != null)
            {
                // Keep only one geometry within this layer
                writeLayer.Clear();
                writeLayer.AddRange(new List<IFeature>() { newFeat.Item1 });

                mapView.Map.RefreshGraphics();
            }
        }

        //Add it to linework temp vector points
        IEnumerable<ILayer> tLayers = mapView.Map.Layers.FindLayer(ApplicationLiterals.aliasLineworkVerticesEdit);
        if (tLayers != null && tLayers.First() != null)
        {

            WritableLayer tLayer = tLayers.First() as WritableLayer;
            if (tLayer != null)
            {
                // Keep only one geometry within this layer
                tLayer.Clear();
                tLayer.AddRange(new List<IFeature>() { newFeat.Item2 });

                mapView.Map.RefreshGraphics();
            }
        }


    }

    /// <summary>
    /// Will add a coordinate to a geometry feature for linework along with a vector point geometry for all vertices
    /// showing user what he's digitizing correctly.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    private async Task<Tuple<GeometryFeature, GeometryFeature>> AddPointToLinework(double x, double y)
    {
        if (_drawableLineGeometry != null)
        {
            _drawableLineGeometry.Geometry = await GeopackageService.AddPointToLineString(_drawableLineGeometry.Geometry as LineString, x, y);
            _drawableLineVectorGeometry.Geometry = await GeopackageService.AddPointToMultiPoint(_drawableLineVectorGeometry.Geometry as MultiPoint, x, y);
        }
        else
        {
            _drawableLineGeometry = new GeometryFeature();
            _drawableLineVectorGeometry = new GeometryFeature();

            _drawableLineGeometry.Geometry = new LineString(new[] { new Coordinate(x, y), new Coordinate(x, y) });
            Point[] initVectorPoint = new Point[1];
            initVectorPoint[0] = new Point(new Coordinate(x, y));
            _drawableLineVectorGeometry.Geometry = new MultiPoint(initVectorPoint);

        }

        return new Tuple<GeometryFeature, GeometryFeature>(_drawableLineGeometry, _drawableLineVectorGeometry );
    }

    /// <summary>
    /// Will initiate the linework for edit layer with styling inside a writablelayer
    /// </summary>
    /// <returns></returns>
    private async Task InitiateLinework()
    {

        //Init linework temporary drawable layer
        IEnumerable<ILayer> currentLayers = mapView.Map.Layers.FindLayer(ApplicationLiterals.aliasLineworkEdit);
        if (currentLayers == null || currentLayers.Count() == 0)
        {
            //Style it
            ILayer layerToAdd = new WritableLayer
            {
                Name = ApplicationLiterals.aliasLineworkEdit,
                Style = new Mapsui.Styles.VectorStyle()
                {
                    Fill = null,
                    Outline = null,
                    Line = { Color = Color.FromString("Black"), Width = 2 }
                },
                IsMapInfoLayer = true,

            };

            mapView.Map.Layers.Add(layerToAdd);
        }

        //Init linework points temporary drawable layer
        IEnumerable<ILayer> tLayers = mapView.Map.Layers.FindLayer(ApplicationLiterals.aliasLineworkVerticesEdit);
        if (tLayers == null || tLayers.Count() == 0)
        {
            //Style it
            ILayer layerToAdd = new WritableLayer
            {
                Name = ApplicationLiterals.aliasLineworkVerticesEdit,
                Style = CreateVectorBitmapStyle(),
                IsMapInfoLayer = true,

            };

            mapView.Map.Layers.Add(layerToAdd);
        }

    }

    /// <summary>
    /// Will empty the lineworkedit layer of any geometries
    /// </summary>
    private void EmptyLinework()
    {
        //Linework line
        IEnumerable<ILayer> layers = mapView.Map.Layers.FindLayer(ApplicationLiterals.aliasLineworkEdit);
        if (layers != null && layers.Count() > 0)
        {

            WritableLayer writeLayer = layers.First() as WritableLayer;
            if (writeLayer != null)
            {
                // Clean layer of any drawn geometries
                writeLayer.Clear();
                mapView.Map.RefreshGraphics();

                _drawableLineGeometry = null;
            }
        }

        //Linework points
        layers = mapView.Map.Layers.FindLayer(ApplicationLiterals.aliasLineworkVerticesEdit);
        if (layers != null && layers.Count() > 0)
        {

            WritableLayer writeLayer = layers.First() as WritableLayer;
            if (writeLayer != null)
            {
                // Clean layer of any drawn geometries
                writeLayer.Clear();
                mapView.Map.RefreshGraphics();

                _drawableLineGeometry = null;
            }
        }
    }

    /// <summary>
    /// Will save, show note page and refresh linework edit layer
    /// </summary>
    private async void FinalizeLinework(Mapsui.UI.TappedEventArgs tappedEventArgs)
    {
        //Ask user if a save process can be initiated
        bool canSave = await DisplayAlert(LocalizationResourceManager["MapPageAddLineworkDialogTitle"].ToString(),
            LocalizationResourceManager["MapPageAddLineDialogMessage"].ToString(),
            LocalizationResourceManager["GenericButtonSave"].ToString(),
            LocalizationResourceManager["GenericButtonContinue"].ToString());

        if (canSave)
        {
            //Create an actual and new linework record with previous drawn line
            if ((LineString)_drawableLineGeometry.Geometry != null)
            {
                //Save linework as a new record and open edit form for user to finalize it
                MapViewModel _vm = BindingContext as MapViewModel;
                await _vm.AddLinework((LineString)_drawableLineGeometry.Geometry);

                //Empty lineworkedit layer of any content
                EmptyLinework();

                //Force refresh of linework feature on the map
                IEnumerable<IFeature> lineFeats = await GetGeometriesAsync(defaultLayerList.Linework);
                IEnumerable<ILayer> mapViewLayers = mapView.Map.Layers.Where(x => x.Name == ApplicationLiterals.aliasLinework);
                if (mapViewLayers.Count() > 0)
                {
                    ILayer lineworkLayer = mapViewLayers.First();
                    MemoryLayer memoryLineLayer = lineworkLayer as MemoryLayer;

                    if (memoryLineLayer != null)
                    {
                        memoryLineLayer.Features = lineFeats;
                        mapView.Map.RefreshData();
                    }


                }


            }
        }
    }

    /// <summary>
    /// Will return a mapsui color for symbolization, from a givens string
    /// Defaults to grey if it fails to convert.
    /// </summary>
    /// <param name="inColor"></param>
    /// <returns></returns>
    private Color GetColorFromString(string inColor)
    {
        //Valid else default to grey
        if (inColor != null)
        {
            try
            {
                return Color.FromString(inColor);
            }
            catch (System.Exception e)
            {
                new ErrorToLogFile(e.Message).WriteToFile();

                return Color.Grey;
            }
        }
        else
        {
            return Color.Grey;
        }

    }

    /// <summary>
    /// Will create a memory layer for a given enum value
    /// </summary>
    /// <param name="defaultLayerName"></param>
    /// <returns></returns>
    private async Task<MemoryLayer> CreateDefaultLayerAsync(defaultLayerList defaultLayerName)
    {
        MemoryLayer defaultLayer = new MemoryLayer();

        //Make sure some features have records
        bool addOrNot = true; //Will be used to get traverses out of the way if empty
        IEnumerable<IFeature> dFeats = await Task.Run(async () => await GetGeometriesAsync(defaultLayerName));

        //TODO: comment out when traverses will be totally implemented
        if (Enum.GetName(defaultLayerName) == ApplicationLiterals.aliasTraversePoint &&
            Enum.GetName(defaultLayerName) == ApplicationLiterals.aliasLineworkEdit)
        {
            if (dFeats.Count() == 0)
            {
                addOrNot = false;
            }
        }

        if (addOrNot)
        {
            if (Enum.GetName(defaultLayerName) == ApplicationLiterals.aliasStations)
            {
                defaultLayer = new MemoryLayer()
                {
                    Name = Enum.GetName(defaultLayerName),
                    IsMapInfoLayer = true,
                    Features = dFeats,
                    Style = CreateLocationBitmapStyle(),
                    Tag = da.PreferedDatabasePath,
                };

            }
            else
            {
                defaultLayer = new MemoryLayer()
                {
                    Name = Enum.GetName(defaultLayerName),
                    IsMapInfoLayer = true,
                    Features = dFeats,
                    Style = null,
                    Tag = da.PreferedDatabasePath,
                };
            }

        }

        return defaultLayer;
    }

    /// <summary>
    /// Will enable/disable the user to draw an interpretation line on screen.
    /// </summary>
    private void ToggleDrawing()
    {
        //Revert current state of drawing lines
        _isDrawingLine = !_isDrawingLine;

        //Change font color to indicate it's activated
        if (_isDrawingLine && App.Current.Resources.TryGetValue("Secondary", out var colorValue))
        {
            var activeColor = (Microsoft.Maui.Graphics.Color)colorValue;

            this.DrawLine.TextColor = activeColor;
        }
        else
        {
            //Keep as default
            this.DrawLine.TextColor = Microsoft.Maui.Graphics.Colors.White;

            //Clear any drawn lines
            EmptyLinework();
        }

    }

    /// <summary>
    /// Will enable/disable the user to make a tap entry for a new station
    /// </summary>
    private void ToggleTapEntry()
    {
        //Revert current state of drawing lines
        _isTapMode = !_isTapMode;

        //Change font color to indicate it's activated
        if (_isTapMode && App.Current.Resources.TryGetValue("Secondary", out var colorValue))
        {
            var activeColor = (Microsoft.Maui.Graphics.Color)colorValue;

            this.TapMode.TextColor = activeColor;
        }
        else
        {
            //Keep as default
            this.TapMode.TextColor = Microsoft.Maui.Graphics.Colors.White;
        }
    }

    /// <summary>
    /// Need to caculate some screen ratio between skiasharp and mapsui, 
    /// else a drawnline will be offset baceuse skiasharpp uses a different
    /// viewport/canvas size then mapsui.
    /// </summary>
    private Tuple<double, double> CalculateViewportRatio(SKCanvasView skiaBox, Navigator mapsuiBox)
    {
        if (_viewportHeightRatio == 1 && _viewportWidthRatio == 1)
        {
            if (skiaBox != null && mapsuiBox != null)
            {
                _viewportHeightRatio = mapsuiBox.Viewport.Height / skiaBox.CanvasSize.Height;
                _viewportWidthRatio = mapsuiBox.Viewport.Width / skiaBox.CanvasSize.Width;
            }
        }

        return new Tuple<double, double>(_viewportWidthRatio, _viewportHeightRatio);
    }

    /// <summary>
    /// Will enable/disable GPS and disable/enable tap entry
    /// </summary>
    private void ToggleGPS()
    {
        if (_isCheckingGeolocation)
        {
            StopGPSAsync();
        }
        else
        {
            _ = StartGPS();
        }
    }

    #endregion

    #region GPS

    /// <summary>
    /// Will start the GPS
    /// </summary>
    public async Task StartGPS()
    {
        //Init 
        _isCheckingGeolocation = true;
        CancellationToken cancellationToken = CancellationToken.None;
        this.WaitingCursor.IsRunning = true;

        //Get permission from device first
        PermissionStatus permissionStatus = await Permissions.RequestAsync<Permissions.LocationAlways>();

        switch (permissionStatus)
        {
            case PermissionStatus.Granted:
                
                if (cancellationToken.IsCancellationRequested)
                {
                    DeactivateLocationVisuals();
                    return;
                }

                try
                {
                    //Timespan for refresh rate
                    await SetGPSRefreshRate();
                    await SetLocationFollow();

                    //Listening to location changes
                    GeolocationListeningRequest request = new GeolocationListeningRequest(GeolocationAccuracy.Default, _refreshRate);
                    CancellationTokenSource _cancelTokenSource = new CancellationTokenSource();

                    //Enforce foreground listening
                    bool success = await Geolocation.StartListeningForegroundAsync(request);
                    string status = success
                        ? "Started listening for foreground location updates"
                        : "Couldn't start listening";

                    if (success)
                    {
                        _locationSettingEnabledAttempt = 0; //Reset attempt

                        //Force location change event
                        await BackgroundTimer(_refreshRate);

                        //Temp this isn't triggered
                        Geolocation.LocationChanged += Geolocation_LocationChanged;

                        this.WaitingCursor.IsRunning = false;
                    }
                    else
                    {
                        this.WaitingCursor.IsRunning = false;
                    }
                }
                catch (FeatureNotSupportedException fnsEx)
                {

                    // Handle not supported on device exception
                    await Shell.Current.DisplayAlert(LocalizationResourceManager["DisplayAlertGPSDenied"].ToString(),
                        fnsEx.Message,
                        LocalizationResourceManager["GenericButtonOk"].ToString());
                    DeactivateLocationVisuals();

                    new ErrorToLogFile(fnsEx).WriteToFile();

                }
                catch (FeatureNotEnabledException fneEx)
                {
                    ///Ask to enable location in setting, only once then retry 10 times, else
                    ///keep deactivated

                    if (_locationSettingEnabledAttempt == 0)
                    {
                        // Handle not enabled on device exception
                        await Shell.Current.DisplayAlert(LocalizationResourceManager["DisplayAlertGPSNoEnabled"].ToString(),
                            fneEx.Message,
                            LocalizationResourceManager["GenericButtonOk"].ToString());

                        //Open location settings so user can toggle it on
#if ANDROID
                    var intent = new Intent(Android.Provider.Settings.ActionLocationSourceSettings);
                    intent.AddCategory(Intent.CategoryDefault);
                    intent.SetFlags(ActivityFlags.NewTask);
                    Platform.CurrentActivity.StartActivityForResult(intent, 0);
#elif IOS
                        UIApplication.SharedApplication.OpenUrl(new NSUrl("App-Prefs:Privacy&path=LOCATION"));
#endif
                    }

                    //Deactivate and retry
                    DeactivateLocationVisuals();

                    //If after 10 attemps it's still not enabled, stop trying
                    if (_locationSettingEnabledAttempt <= 10)
                    {
                        //Increment atempt
                        _locationSettingEnabledAttempt = _locationSettingEnabledAttempt + 1;

                        await Task.Delay(1000).ContinueWith(async antecedent => await StartGPS());

                        new ErrorToLogFile(fneEx).WriteToFile();
                    }

                }
                catch (PermissionException pEx)
                {

                    // Handle permission exception
                    await Shell.Current.DisplayAlert(LocalizationResourceManager["DisplayAlertGPSDenied"].ToString(),
                        pEx.Message,
                        LocalizationResourceManager["GenericButtonOk"].ToString());

                    DeactivateLocationVisuals();

                    new ErrorToLogFile(pEx).WriteToFile();

                }
                catch (System.Exception ex)
                {

                    // Unable to get location
                    await Shell.Current.DisplayAlert(LocalizationResourceManager["DisplayAlertGPSDenied"].ToString(),
                        ex.Message,
                        LocalizationResourceManager["GenericButtonOk"].ToString());
                    //DeactivateLocationVisuals();

                    new ErrorToLogFile(ex).WriteToFile();

                }

                break;

            default:

                await DisplayGPSNotGranted();
                DeactivateLocationVisuals();

                break;

        }

    }

    /// <summary>
    /// Will stop the GPS
    /// </summary>
    public async Task StopGPSAsync()
    {
        DeactivateLocationVisuals();

        try
        {
            Geolocation.LocationChanged -= Geolocation_LocationChanged;
            Geolocation.StopListeningForeground();

        }
        catch (System.Exception ex)
        {
            // Unable to stop listening for location changes
            new ErrorToLogFile(ex).WriteToFile();

        }

    }

    /// <summary>
    /// TEMP method to force refresh of location each seconds
    /// TODO: Make sure this is still needed
    /// </summary>
    /// <param name="timeSpan"></param>
    /// <returns></returns>
    public async Task BackgroundTimer(TimeSpan timeSpan)
    {
        var periodicTimer = new PeriodicTimer(timeSpan);
        while (await periodicTimer.WaitForNextTickAsync() && _isCheckingGeolocation)
        {
            Sensor.Location location = await Geolocation.GetLocationAsync();
            await UpdateLocationOnMap(location);
        }
    }

    /// <summary>
    /// An event occuring when a user locatin has changed
    /// TODO; this should be working but isn't for now 
    /// https://github.com/dotnet/maui/pull/21919
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Geolocation_LocationChanged(object sender, Sensor.GeolocationLocationChangedEventArgs e)
    {
        try
        {
            // check if it should update location
            if (_isCheckingGeolocation)
            {
                UpdateLocationOnMap(e.Location);
            }
            else
            {
                return;
            }

        }
        catch (System.Exception ex)
        {
            await DisplayAlert("Alert", ex.Message, "OK");

            new ErrorToLogFile(ex).WriteToFile();

        }
    }

    /// <summary>
    /// Method that will actually refresh the map object with latest location info
    /// </summary>
    /// <param name="inLocation"></param>
    /// <returns></returns>
    public async Task UpdateLocationOnMap(Sensor.Location inLocation)
    {
        if (_isCheckingGeolocation)
        {
            MapViewModel _vm = BindingContext as MapViewModel;
            if (_vm != null)
            {
                this.WaitingCursor.IsRunning = false; //Make sure it's closed down
                
                _vm.RefreshCoordinates(inLocation);

                await SetMapAccuracyColor(inLocation.Accuracy);

                mapView?.MyLocationLayer.UpdateMyLocation(new Mapsui.UI.Maui.Position(inLocation.Latitude, inLocation.Longitude));
                mapView.MyLocationEnabled = true;
                //mapView.RefreshGraphics();
                mapView.MyLocationFollow = _locationFollowEnabled;

                if (inLocation.Course != null && inLocation.Course.HasValue)
                {
                    mapView?.MyLocationLayer.UpdateMyDirection(inLocation.Course.Value, mapView?.Map.Navigator.Viewport.Rotation ?? 0, false);
                }
                else
                {
                    mapView?.MyLocationLayer.UpdateMyDirection(0, mapView?.Map.Navigator.Viewport.Rotation ?? 0, false);
                }

            }  
        }

        //Debug option to log GPS
        LogGPSChanges(DateTime.Now, inLocation);

    }

    /// <summary>
    /// When location permission isn't granted show warning and navigate to settings.
    /// </summary>
    public async Task DisplayGPSNotGranted()
    {
        //Tell user app doesn't have access to location
        bool answer = await Shell.Current.DisplayAlert(LocalizationResourceManager["DisplayAlertGPSDenied"].ToString(),
            LocalizationResourceManager["DisplayAlertGPSMessage"].ToString(),
            LocalizationResourceManager["GenericButtonYes"].ToString(),
            LocalizationResourceManager["GenericButtonNo"].ToString());

        if (answer == true)
        {
            AppInfo.Current.ShowSettingsUI();
        }

    }

    /// <summary>
    /// Will make sure to render proper deactivated location graphics in map page
    /// </summary>
    public void DeactivateLocationVisuals()
    {
        //Make sure to disable background timer for location event
        _isCheckingGeolocation = false;

        //Make sure to turn gray map border color based on accuracy
        _ = SetMapAccuracyColor(-99);

        //Make sure to show proper bad location coordinates labels
        MapViewModel _vm = BindingContext as MapViewModel;
        _vm.RefreshCoordinates(badLoc);

        this.WaitingCursor.IsRunning = false;

        //Turn off location blue point
        if (mapView != null && mapView.MyLocationLayer != null)
        {
            mapView.MyLocationEnabled = false;
            mapView.RefreshGraphics();
        }
        
    }

    /// <summary>
    /// Will save into a log file all GPS location changes if enabled by user in debug mode
    /// </summary>
    public void LogGPSChanges(DateTime inTime, Sensor.Location inLocation)
    {
        if (GPSLogFilePath == string.Empty)
        {
            GPSLogFilePath = Path.Combine(FileSystem.Current.AppDataDirectory, ApplicationLiterals.gpsLogFileNameExt);
        }

        if (GPSLogEnabled && GPSLogFilePath != null && GPSLogFilePath != string.Empty)
        {
            using (var writer = new StreamWriter(GPSLogFilePath, true))
            {
                string gpsLogs = inTime.ToString("yyyy-MM-dd HH:mm:ss.fff") + "," +
                inLocation.Longitude + "," +
                inLocation.Latitude + "," +
                inLocation.Accuracy + "(m)," +
                _refreshRate.TotalMilliseconds.ToString() + "(ms)";   

                writer.WriteLine(gpsLogs);
                writer.Close();
            }
        }

    }

    /// <summary>
    /// From the user settings, will change the gps refresh rate to either two options
    /// 1000ms or 350ms.
    /// Mainly used for helicopter surveying.
    /// </summary>
    public async Task SetGPSRefreshRate()
    {
        TimeSpan _previousSpan = _refreshRate;

        //Set
        if (GPSHighRateEnabled)
        {
            this.mapViewHighRateGPSIcon.IsVisible = true;
            _refreshRate = TimeSpan.FromMilliseconds(350);
        }
        else
        {
            this.mapViewHighRateGPSIcon.IsVisible = false;
            _refreshRate = TimeSpan.FromMilliseconds(1000);
        }

        //Force refresh of GPS location even only if value has changed
        if (_isCheckingGeolocation && !_isTapMode)
        {
            //use total milliseconds, else it returns 0 when it hits 1000 ms.
            if (_previousSpan.TotalMilliseconds != _refreshRate.TotalMilliseconds)
            {
                
                await StopGPSAsync().ContinueWith(async a => await StartGPS());
            }

        }
    }

    /// <summary>
    /// Will set property of location follow on map
    /// </summary>
    /// <returns></returns>
    public async Task<bool> SetLocationFollow()
    {
        _locationFollowEnabled = LocationFollowEnabled;

        return _locationFollowEnabled;
    }

    #endregion

}
