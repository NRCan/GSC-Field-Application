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
using System.Collections.Generic;


#if ANDROID
using Android.Content;
#elif IOS
using UIKit;
using Foundation;
#endif

namespace GSCFieldApp.Views;

public partial class MapPage : ContentPage
{
    private MapViewModel _vm;
    //private CancellationTokenSource? gpsCancelation;
    private CancellationTokenSource _cancelTokenSource;
    private MapControl mapControl = new Mapsui.UI.Maui.MapControl();
    private DataAccess da = new DataAccess();
    private GeopackageService geopackService = new GeopackageService();
    private int bitmapSymbolId = -1;
    private bool _isCheckingGeolocation = false;
    private bool _isTapMode = false;
    private bool _isDrawingLine = false;
    private enum defaultLayerList { Stations, Linework, Traverses }
    private Sensor.Location badLoc = new Sensor.Location() { Accuracy=-99, Longitude=double.NaN, Latitude=double.NaN, Altitude=double.NaN };
    private Drawable _drawable = new Drawable(); //Meant to be used for linework
    private GeometryFeature _drawableGeometry = null; //Meant to be used for linework
    public LocalizationResourceManager LocalizationResourceManager
        => LocalizationResourceManager.Instance; // Will be used for in code dynamic local strings

    public MapPage(MapViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        _vm = vm;

        //Initialize grid background
        mapPageGrid.BackgroundColor = Mapsui.Styles.Color.FromString("White").ToNative();
        mapControl.Map.Widgets.Add(new Mapsui.Widgets.ScaleBar.ScaleBarWidget(mapControl.Map)
        {
            TextAlignment = Mapsui.Widgets.Alignment.Center,
            HorizontalAlignment = Mapsui.Widgets.HorizontalAlignment.Left,
            VerticalAlignment = Mapsui.Widgets.VerticalAlignment.Bottom
        });

        //Setting map page background default data
        SetOpenStreetMap();

        //Set map and start listenning to layer events
        mapView.Map = mapControl.Map;

        this.Loaded += MapPage_Loaded;
        this.mapControl.Map.Layers.LayerAdded += Layers_LayerAdded;

        //Detect new field book selection, uprgrade, edit, ...
        FieldBooksViewModel.newFieldBookSelected += FieldBooksViewModel_newFieldBookSelectedAsync;

    }

    #region EVENTS

    /// <summary>
    /// Event triggered when user has changed field books.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void FieldBooksViewModel_newFieldBookSelectedAsync(object sender, bool hasChanged)
    {
        if (hasChanged && mapView != null)
        {
            //Reload user datasets
            await LoadPreferedLayers();
        }

    }

    /// <summary>
    /// Will be triggered whenever a layer has been added. 
    /// This will make sure to close the waiting cursor
    /// </summary>
    /// <param name="layer"></param>
    private void Layers_LayerAdded(ILayer layer)
    {
        //Make sure to disable the waiting cursor
        this.WaitingCursor.IsRunning = false;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {


        base.OnNavigatedTo(args);

        //In case user is coming from field notes
        //They might have deleted some stations, make sure to refresh

        foreach (var item in mapView.Map.Layers)
        {
            if (item.Name == ApplicationLiterals.aliasStations || item.Name == ApplicationLiterals.aliasLinework)
            {
                //Remove all the one with same name
                ILayer[] stations = mapView.Map.Layers.Where(x=>x.Name == item.Name).ToArray();
                mapView.Map.Layers.Remove(stations);

                List<MemoryLayer> memLayers = await CreateDefaultLayersAsync();
                if (memLayers != null && memLayers.Count > 0)
                {
                    //TODO check this, a loop of layer addigin within a loop of layers...
                    foreach (MemoryLayer ml in memLayers)
                    {
                        //Make sure it's not already in there
                        if (mapView.Map.Layers.Where(x=>x.Name == ml.Name).Count() == 0)
                        {
                            mapView.Map.Layers.Add(ml);
                        }
                        
                    }

                    mapView.Map.RefreshData();

                    //Zoom to extent of stations
                    SetExtent(memLayers[0]);
                }

                break;
            }
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

        //Manage symbol and layers
        await AddSymbolToRegistry();

        //Freshen up the default layers
        await RefreshDefaultFeatureLayer();

        //Reload user datasets for selected field book
        await LoadPreferedLayers();

        //Manage GPS
        if (!_isCheckingGeolocation && !_isTapMode)
        {

            await StartGPS();

        }
    }

    private async void mapView_MapClicked(object sender, MapClickedEventArgs e)
    {
        //Make sure to disable map layer frame
        MapLayerFrame.IsVisible = false;

        //NOT WORKING --> Get feature info
        //GetWMSFeatureInfo(e.Point);

        //Detect if in tap mode or drawing lines to show tapped coordinates on screen
        if (!_isCheckingGeolocation && !_isDrawingLine)
        {
            //Keep tap mode for future validation
            _isTapMode = true;

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
        else
        {
            _isTapMode = false;
        }

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
            await AddAMBTile(fr.FullPath);
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
        _vm.RefreshLayerCollection(mapView.Map.Layers);

        MapLayerFrame.IsVisible = !MapLayerFrame.IsVisible;
    }

    /// <summary>
    /// Will enable/disable GPS. When disabled user can tap on screen to create stations
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void GPSMode_Clicked(object sender, EventArgs e)
    {
        if (_isCheckingGeolocation)
        {
            StopGPSAsync();

            //TODO find a working way of changing button symbol
            //tried in map view model like in version 2 Couldn't make it work
            //this.GPSMode.Text = "&#xF023D;";
            //this.GPSMode.FontFamily = "MatDesign";
        }
        else
        {
            _ = StartGPS();
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
            LocalizationResourceManager["GenericButtonCancel"].ToString(),
            LocalizationResourceManager["MapPageAddWMSDialogPlaceholder"].ToString(),-1,null,
            LocalizationResourceManager["MapPageAddWMSDialogPlaceholder"].ToString());

        if (wms_url != string.Empty)
        {
            //Insert
            await AddAWMSAsync(wms_url);
        }
    }

    /// <summary>
    /// Will enable the user to draw an interpretation line on screen.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void DrawLine_Clicked(object sender, EventArgs e)
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
                if (_vm.LayerCollection != null && _vm.LayerCollection.Count() > 0)
                {
                    //Update layer collection for menu
                    _vm.RefreshLayerCollection(mapView.Map.Layers);

                    _vm.SaveLayerRendering();
                }
            }
        }
    }

    /// <summary>
    /// Track single tap on screen. If user has enable drawing mode, add tapped points to 
    /// linestring of lineworkedit temporary feature
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void mapView_SingleTap(object sender, Mapsui.UI.TappedEventArgs e)
    {
        if (e != null && e.ScreenPosition != null && _isDrawingLine)
        {
            (double, double) clickedPoint = mapView.Map.Navigator.Viewport.ScreenToWorldXY(e.ScreenPosition.X, e.ScreenPosition.Y);

            FillLinework(clickedPoint.Item1, clickedPoint.Item2);

        }
    }

    /// <summary>
    /// Track double tap on screen.
    /// If user has enable drawing mode, this will close the current lineworkedit linestring geometry
    /// and will popup the linework edit form to save the record.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void mapView_DoubleTap(object sender, Mapsui.UI.TappedEventArgs e)
    {
        FinalizeLinework(e);
    }

    /// <summary>
    /// Whenever user touches screen, add point to line graphic
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void mapView_TouchStarted(object sender, Mapsui.UI.TouchedEventArgs e)
    {
        //if (e != null && e.ScreenPoints != null && e.ScreenPoints.Count() > 0)
        //{
        //    (double, double) clickedPoint = mapView.Map.Navigator.Viewport.ScreenToWorldXY(e.ScreenPoints.First().X, e.ScreenPoints.First().Y);
        //    FillDrawableWithCoordinate(clickedPoint.Item1, clickedPoint.Item2);

        //}
    }

    /// <summary>
    /// Will finalize linework if in drawing mode
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void mapView_LongTap(object sender, Mapsui.UI.TappedEventArgs e)
    {
        FinalizeLinework(e);
    }

    #endregion

    #region METHODS

    /// <summary>
    /// Will force a quick refresh on the feature layers like station and traverses
    /// </summary>
    /// <returns></returns>
    private async Task RefreshDefaultFeatureLayer()
    {
        _vm.EmptyLayerCollections();

        List<MemoryLayer> mls = await CreateDefaultLayersAsync();
        if (mls != null && mls.Count() > 0)
        {
            foreach (MemoryLayer ml in mls)
            {
                //Verify if doesn't exist and add
                if (mapView.Map.Layers.Where(x=>x.Name == ml.Name).Count() == 0)
                {
                    mapView.Map.Layers.Add(ml);
                }
                
            }

            //Zoom to initial extent of the station layer
            SetExtent(mls[0]);

        }
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
    /// On init will reload user prefered layers from saved JSON file.
    /// It'll also do some clean up making sure only desired layers are loaded.
    /// </summary>
    /// <returns></returns>
    public async Task LoadPreferedLayers()
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
        Collection<MapPageLayer> prefLayers = await _vm.GetLayerRendering();

        if (prefLayers != null && mapView != null)
        {

            foreach (MapPageLayer mpl in prefLayers)
            {
                if (mpl.LayerName != ApplicationLiterals.aliasStations && mpl.LayerName != ApplicationLiterals.aliasLinework)
                {
                    if (mpl.LayerType == MapPageLayer.LayerTypes.mbtiles)
                    {
                        await AddAMBTile(mpl.LayerPathOrURL, mpl);
                    }
                    else if (mpl.LayerType == MapPageLayer.LayerTypes.wms)
                    {
                        await AddAWMSAsync(mpl.LayerPathOrURL, true, mpl);
                    }
                }

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
                await Shell.Current.DisplayAlert(LocalizationResourceManager["GenericErrorTitle"].ToString(),
                    e.Message,
                    LocalizationResourceManager["GenericButtonOk"].ToString());
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
        if (withCache)
        {
            var persistentCache = new SqlitePersistentCache(ApplicationLiterals.keywordWMS + "_OSM");
            HttpTileSource source = KnownTileSources.Create(KnownTileSource.OpenStreetMap, ApplicationLiterals.keywordWMS + "/3.0 Maui.net", persistentCache: persistentCache);
            TileLayer osmLayer = new TileLayer(source);
            osmLayer.Name = ApplicationLiterals.aliasOSM;
            mapControl.Map.Layers.Insert(0, osmLayer);
        }
        else
        {
            TileLayer osmLayer = Mapsui.Tiling.OpenStreetMap.CreateTileLayer(ApplicationLiterals.keywordWMS + "/3.0 Maui.net");
            osmLayer.Name = ApplicationLiterals.aliasOSM;
            mapControl.Map.Layers.Insert(0, osmLayer);
        }

    }

    /// <summary>
    /// Will add a new WMS layer into the map page
    /// </summary>
    /// <param name="wmsURL"></param>
    /// <param name="withCache"></param>
    public async Task AddAWMSAsync(string wmsURL, bool withCache = true, MapPageLayer pageLayer = null)
    {
        if (wmsURL != null && wmsURL != string.Empty)
        {
            this.WaitingCursor.IsRunning = true;

            string fullURL = wmsURL;
            string[] splitURL = wmsURL.Split(ApplicationLiterals.keywordWMSLayers);
            string partialURL = splitURL[0];

            //Make sure a layer is added to the URL before continuing
            if (splitURL.Count() > 1)
            {
                string layerNameFromURL = wmsURL.Split(ApplicationLiterals.keywordWMSLayers)[1].Split("&")[0]; //Make sure to only keep layer name
                GlobalSphericalMercator schema = new GlobalSphericalMercator { Format = "image/png" };
                WmscRequest request = new WmscRequest(new Uri(partialURL), schema, new[] { layerNameFromURL }.ToList(), Array.Empty<string>().ToList());

                if (withCache)
                {
                    SqlitePersistentCache wmsCache = new SqlitePersistentCache(ApplicationLiterals.keywordWMS + layerNameFromURL.Replace(':', '_'));
                    HttpTileProvider provider = new HttpTileProvider(request, wmsCache);
                    TileSource t = new TileSource(provider, schema);
                    TileLayer tl = new TileLayer(t);
                    tl.Name = layerNameFromURL;
                    tl.Tag = fullURL;

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

                    HttpTileProvider provider = new HttpTileProvider(request);
                    TileSource t = new TileSource(provider, schema);
                    TileLayer tl = new TileLayer(t);
                    tl.Name = layerNameFromURL;
                    tl.Tag = fullURL;

                    if (pageLayer != null)
                    {
                        tl.Opacity = pageLayer.LayerOpacity;
                        tl.Enabled = pageLayer.LayerVisibility;
                    }

                    //Insert at right location in collection
                    InsertLayerAtRightPlace(tl);
                }
            }
            else
            {
                //Tell user app doesn't have access to location
                await Shell.Current.DisplayAlert(LocalizationResourceManager["MapPageAddWMSFailTitle"].ToString(),
                    LocalizationResourceManager["MapPageAddWMSFailMessage"].ToString(),
                    LocalizationResourceManager["GenericButtonOk"].ToString());

                this.WaitingCursor.IsRunning = false;
            }
        }

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
        }

        //Parse accuracy to change color
        if (accuracy > 20.0 && accuracy <= 40.0)
        {
            if (App.Current.Resources.TryGetValue("WarningColor", out var warningColorvalue))
            {
                mapPageGrid.BackgroundColor = warningColorvalue as Microsoft.Maui.Graphics.Color;
            }


        }
        else if (accuracy > 40.0)
        {
            if (App.Current.Resources.TryGetValue("ErrorColor", out var errorColorvalue))
            {
                mapPageGrid.BackgroundColor = errorColorvalue as Microsoft.Maui.Graphics.Color;
            }

        }
        else if (accuracy == -99)
        {
            if (App.Current.Resources.TryGetValue("Gray400", out var errorColorvalue))
            {
                mapPageGrid.BackgroundColor = errorColorvalue as Microsoft.Maui.Graphics.Color;
            }
        }
    }

    /// <summary>
    /// Must add all image in bitmap registry for mapsui to use them as symbol styles
    /// </summary>
    /// <returns></returns>
    public async Task<int> AddSymbolToRegistry()
    {
        using Stream pointBitmap = await FileSystem.OpenAppPackageFileAsync(@"point.png");

        MemoryStream memoryStream = new MemoryStream();
        pointBitmap.CopyTo(memoryStream);
        return bitmapSymbolId = BitmapRegistry.Instance.Register(memoryStream);

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
                        {DevicePlatform.WinUI, new [] { DatabaseLiterals.LayerTypeMBTiles} },
                        {DevicePlatform.Android, new [] { "application/*"} },
                        {DevicePlatform.iOS, new [] { DatabaseLiterals.LayerTypeMBTiles } },
                });

            PickOptions options = new PickOptions();
            options.PickerTitle = "Add Layer";
            options.FileTypes = customFileType;

            var result = await FilePicker.Default.PickAsync(options);
            if (result != null)
            {
                if (result.FileName.Contains(DatabaseLiterals.LayerTypeMBTiles))
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
            //Make sure some features have records
            bool addOrNot = true; //Will be used to get traverses out of the way if empty
            IEnumerable<IFeature> dFeats = await GetLocationsAsync((defaultLayerList)i);

            //TODO: comment out when traverses will be totally implemented
            if (Enum.GetName(typeof(defaultLayerList), i) == ApplicationLiterals.aliasTraversePoint &&
                Enum.GetName(typeof(defaultLayerList), i) == ApplicationLiterals.aliasLineworkEdit)
            {
                if (dFeats.Count() == 0)
                {
                    addOrNot = false;
                }
            }

            if (addOrNot)
            {
                if (Enum.GetName(typeof(defaultLayerList), i) == ApplicationLiterals.aliasStations)
                {
                    defaultLayers.Add(new MemoryLayer
                    {
                        Name = Enum.GetName(typeof(defaultLayerList), i),
                        IsMapInfoLayer = true,
                        Features = dFeats,
                        Style = CreateBitmapStyle()
                    });
                }
                else
                {
                    defaultLayers.Add(new MemoryLayer
                    {
                        Name = Enum.GetName(typeof(defaultLayerList), i),
                        IsMapInfoLayer = true,
                        Features = dFeats
                    });
                }

            }

        }


        return defaultLayers;
    }

    /// <summary>
    /// Will query the database to retrive some basic information about location
    /// and stations
    /// </summary>
    /// <returns></returns>
    private async Task<IEnumerable<IFeature>> GetLocationsAsync(defaultLayerList inLayer)
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

                    enumFeat = await GetStationLocationsAsync(offset);
                    break;

                case defaultLayerList.Traverses:

                    enumFeat = await GetPointTraversesAsync(offset);
                    break;
                case defaultLayerList.Linework:
                    enumFeat = await GetLineworkAsync(offset);
                    break;
                default:
                    enumFeat = await GetStationLocationsAsync(offset);

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

            SQLiteAsyncConnection currentConnection = new SQLiteAsyncConnection(da.PreferedDatabasePath);
            List<FieldLocation> fieldLoc = await currentConnection.QueryAsync<FieldLocation>
                ("SELECT LTRIM(SUBSTR(" + DatabaseLiterals.FieldLocationAlias + ", -6, 4), '0') as " +
                DatabaseLiterals.FieldLocationAlias + ", " + DatabaseLiterals.FieldLocationLongitude +
                ", " + DatabaseLiterals.FieldLocationLatitude + " FROM " + DatabaseLiterals.TableLocation);

            foreach (FieldLocation fl in fieldLoc)
            {
                //Get coordinate as EPSG 3857 (Spherical mercator WGS84)
                IFeature feat = new PointFeature(SphericalMercator.FromLonLat(fl.LocationLong, fl.LocationLat).ToMPoint());
                feat["name"] = fl.LocationAlias;

                enumFeat = enumFeat.Append(feat);

                feat.Styles.Add(new LabelStyle
                {
                    Text = fl.LocationAlias,
                    BackColor = new Brush(Color.WhiteSmoke),
                    HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Right,
                    //CollisionDetection = true,
                    BorderThickness = 2,
                    Offset = offset
                });

            }

            await currentConnection.CloseAsync();

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

            SQLiteAsyncConnection currentConnection = new SQLiteAsyncConnection(da.PreferedDatabasePath);
            List<TraversePoint> fieldTravPoint = await currentConnection.Table<TraversePoint>().ToListAsync();

            //Prep
            GeopackageService geopackageService = new GeopackageService();

            if (fieldTravPoint != null && fieldTravPoint.Count > 0)
            {
                foreach (TraversePoint tp in fieldTravPoint)
                {

                    //Build geometry
                    NetTopologySuite.Geometries.Point travPointString = await geopackageService.GetGeometryPointFromByteAsync(tp.TravGeom);

                    if (travPointString != null)
                    {
                        //Build feature metadata
                        WKTReader wellKnownTextReader = new WKTReader();
                        IFeature feat = new GeometryFeature(wellKnownTextReader.Read(travPointString.AsText()));
                        feat["name"] = tp.TravLabel;

                        enumFeat = enumFeat.Append(feat);

                        feat.Styles.Add(new LabelStyle
                        {
                            Text = tp.TravLabel,
                            BackColor = new Brush(Color.LightGoldenRodYellow),
                            HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Right,
                            //CollisionDetection = true,
                            BorderThickness = 2,
                            Offset = offset
                        });
                    }


                }

            }


            await currentConnection.CloseAsync();

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
            GeopackageService geopackageService = new GeopackageService();

            //Connect and gather linework
            SQLiteAsyncConnection currentConnection = new SQLiteAsyncConnection(da.PreferedDatabasePath);
            List<Linework> fieldLinework = await currentConnection.Table<Linework>().ToListAsync();

            if (fieldLinework != null && fieldLinework.Count() > 0)
            {
                foreach (Linework lw in fieldLinework)
                {
                    //Build geometry
                    LineString inLineString = await geopackageService.GetGeometryLineFromByte(lw.LineGeom);

                    if (inLineString != null)
                    {
                        //Build feature metadata
                        WKTReader wellKnownTextReader = new WKTReader();
                        IFeature feat = new GeometryFeature(wellKnownTextReader.Read(inLineString.AsText()));

                        if (feat != null)
                        {
                            feat["name"] = lw.LineAliasLight;

                            //Add to list of features
                            enumFeat = enumFeat.Append(feat);

                            //Style line and label
                            feat.Styles.Add(new VectorStyle { Line = new Pen(Color.Violet, 5) });
                            feat.Styles.Add(new LabelStyle
                            {
                                Text = lw.LineAliasLight,
                                BackColor = new Brush(Color.WhiteSmoke),
                                HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Right,
                                BorderThickness = 1
                            });
                        }

                    }


                }
            }

            await currentConnection.CloseAsync();
        }

        return enumFeat;
    
    }

    /// <summary>
    /// Will set style of stations on map
    /// </summary>
    /// <returns></returns>
    private SymbolStyle CreateBitmapStyle()
    {
        // For this sample we get the bitmap from an embedded resouce
        // but you could get the data stream from the web or anywhere
        // else.

        return new SymbolStyle { BitmapId = bitmapSymbolId, SymbolScale = 0.75 };
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

        //Force it to drawable layer
        IEnumerable<ILayer> layers = mapView.Map.Layers.FindLayer(ApplicationLiterals.aliasLineworkEdit);
        if (layers != null && layers.First() != null)
        {

            WritableLayer writeLayer = layers.First() as WritableLayer;
            if (writeLayer != null)
            {
                GeometryFeature newFeat = await AddPointToLinework(xCoord, yCoord);

                // Keep only one geometry within this layer
                writeLayer.Clear();
                writeLayer.AddRange(new List<IFeature>() { newFeat });

                mapView.Map.RefreshGraphics();
            }
        }



    }

    /// <summary>
    /// Will add a coordinate to a geometry feature
    /// </summary>
    /// <param name="geomToAddPointTo"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    private async Task<GeometryFeature> AddPointToLinework(double x, double y)
    {
        if (_drawableGeometry != null)
        {
            _drawableGeometry.Geometry = await GeopackageService.AddPointToLineString(_drawableGeometry.Geometry as LineString, x, y);

        }
        else
        {
            _drawableGeometry = new GeometryFeature();
            _drawableGeometry.Geometry = new LineString(new[] { new Coordinate(x, y), new Coordinate(x, y) });

        }

        return _drawableGeometry;
    }

    /// <summary>
    /// Will initiate the linework for edit layer with styling inside a writablelayer
    /// </summary>
    /// <returns></returns>
    private async Task InitiateLinework()
    {

        //Init drawable layer
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
                    Line = { Color = Color.FromString("Black"), Width = 5 }
                },
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
        //Force it to drawable layer
        IEnumerable<ILayer> layers = mapView.Map.Layers.FindLayer(ApplicationLiterals.aliasLineworkEdit);
        if (layers != null && layers.First() != null)
        {

            WritableLayer writeLayer = layers.First() as WritableLayer;
            if (writeLayer != null)
            {
                // Clean layer of any drawn geometries
                writeLayer.Clear();
                mapView.Map.RefreshGraphics();

                _drawableGeometry = null;
            }
        }
    }

    /// <summary>
    /// Will save, show note page and refresh linework edit layer
    /// </summary>
    private async void FinalizeLinework(Mapsui.UI.TappedEventArgs tappedEventArgs)
    {
        if (tappedEventArgs != null && tappedEventArgs.ScreenPosition != null && _isDrawingLine)
        {
            //Add last point to line
            (double, double) clickedPoint = mapView.Map.Navigator.Viewport.ScreenToWorldXY(tappedEventArgs.ScreenPosition.X, tappedEventArgs.ScreenPosition.Y);
            FillLinework(clickedPoint.Item1, clickedPoint.Item2);

            //Ask user if a save process can be initiated
            bool canSave = await DisplayAlert(LocalizationResourceManager["MapPageAddLineworkDialogTitle"].ToString(),
                LocalizationResourceManager["MapPageAddLineDialogMessage"].ToString(),
                LocalizationResourceManager["GenericButtonSave"].ToString(),
                LocalizationResourceManager["GenericButtonContinue"].ToString());

            if (canSave)
            {
                //Create an actual and new linework record with previous drawn line
                if ((LineString)_drawableGeometry.Geometry != null)
                {
                    //Save linework as a new record and open edit form for user to finalize it
                    await _vm.AddLinework((LineString)_drawableGeometry.Geometry);

                    //Empty lineworkedit layer of any content
                    EmptyLinework();

                    //Force refresh of linework feature on the map
                    IEnumerable<IFeature> lineFeats = await GetLocationsAsync(defaultLayerList.Linework);
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
                    this.WaitingCursor.IsRunning = true;

                    //Listening to location changes
                    GeolocationListeningRequest request = new GeolocationListeningRequest(GeolocationAccuracy.Default, TimeSpan.FromSeconds(1));
                    CancellationTokenSource _cancelTokenSource = new CancellationTokenSource();

                    bool success = await Geolocation.StartListeningForegroundAsync(request);
                    string status = success
                        ? "Started listening for foreground location updates"
                        : "Couldn't start listening";

                    if (success)
                    {
                        
                        //Force location change event
                        await BackgroundTimer(TimeSpan.FromSeconds(1));

                        //Temp this isn't triggered
                        Geolocation.LocationChanged += Geolocation_LocationChanged;

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

                    DeactivateLocationVisuals();

                    new ErrorToLogFile(fneEx).WriteToFile();

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
                    DeactivateLocationVisuals();

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
            _vm.RefreshCoordinates(inLocation);

            await SetMapAccuracyColor(inLocation.Accuracy);

            mapView?.MyLocationLayer.UpdateMyLocation(new Mapsui.UI.Maui.Position(inLocation.Latitude, inLocation.Longitude));
            mapView.RefreshGraphics();
            //mapView.MyLocationFollow = true;

            if (inLocation.Course != null)
            {
                mapView?.MyLocationLayer.UpdateMyDirection(inLocation.Course.Value, mapView?.Map.Navigator.Viewport.Rotation ?? 0);
            }

            if (inLocation.Speed != null)
            {
                mapView?.MyLocationLayer.UpdateMySpeed(inLocation.Speed.Value);
            }

            if (inLocation.Accuracy <= 10)
            {
                this.WaitingCursor.IsRunning = false;
            }
            else
            {
                this.WaitingCursor.IsRunning = true;
            }
            
        }

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
        _vm.RefreshCoordinates(badLoc);

        this.WaitingCursor.IsRunning = false;
    }

    #endregion

}