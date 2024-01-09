using GSCFieldApp.ViewModel;
using Mapsui.Rendering.Skia;
using Mapsui.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mapsui.Logging;
using Mapsui.Styles;
using Mapsui.UI.Maui;
using Mapsui.Layers;
using Mapsui.Projections;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Devices.Sensors;
using Mapsui.Widgets;
using CommunityToolkit.Mvvm.Input;
using BruTile.Tms;
using BruTile;
using SQLite;
using BruTile.MbTiles;
using Mapsui.Tiling.Layers;
using Mapsui;
using GSCFieldApp.Services.DatabaseServices;
using GSCFieldApp.Models;
using System.Collections;
using GSCFieldApp.Dictionaries;
using System.IO;
using Color = Mapsui.Styles.Color;
using Brush = Mapsui.Styles.Brush;
using Mapsui.UI.Maui.Extensions;
using static GSCFieldApp.Models.GraphicPlacement;
using GSCFieldApp.Services;
using BruTile.Predefined;
using Mapsui.Extensions.Cache;
using BruTile.Web;
using BruTile.Wmsc;
using Mapsui.Animations;
using BruTile.Wms;

namespace GSCFieldApp.Views;

public partial class MapPage : ContentPage
{
    //private CancellationTokenSource? gpsCancelation;
    private CancellationTokenSource _cancelTokenSource;
    private bool _updateLocation = true;
    private MapControl mapControl = new Mapsui.UI.Maui.MapControl();
    private DataAccess da = new DataAccess();
    private int bitmapSymbolId = -1;
    private bool _isCheckingGeolocation = false;

    public LocalizationResourceManager LocalizationResourceManager
    => LocalizationResourceManager.Instance; // Will be used for in code dynamic local strings

    public MapPage(MapViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;

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

        mapView.Map = mapControl.Map;

        this.Loaded += MapPage_Loaded;
        this.mapControl.Map.Layers.LayerAdded += Layers_LayerAdded;
    }

    /// <summary>
    /// Will be triggered whenever a layer has been added. 
    /// This will make sure to close the waiting cursor
    /// </summary>
    /// <param name="layer"></param>
    private void Layers_LayerAdded(ILayer layer)
    {
        //Make sure to disable the waiting cursor
        MapViewModel mvm = this.BindingContext as MapViewModel;
        mvm.SetWaitingCursor(false);
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
            if (layer.Name.Contains("Drawables"))
            {
                int rightIndex = layerList.IndexOf(layer);
                mapView.Map.Layers.Insert(rightIndex, in_layer);
                break;
            }

        }
    }

    #region EVENTS

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        //In case user is coming from field notes
        //They might have deleted some stations, make sure to refresh

        foreach (var item in mapView.Map.Layers)
        {
            if (item.Name == "Stations")
            {
                mapView.Map.Layers.Remove(item);

                MemoryLayer ml = await CreatePointLayerAsync();
                mapView.Map.Layers.Add(ml);
                mapView.Map.RefreshData();

                //Zoom to extent of those poins
                SetExtent(ml);

                break;
            }
        }

        //Manage GPS
        if (!_isCheckingGeolocation)
        {
            await StartGPS();
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

        MapViewModel mvm = this.BindingContext as MapViewModel;
        mvm.SetWaitingCursor(true);

        //Manage symbol and layers
        await AddSymbolToRegistry();
        MemoryLayer ml = await CreatePointLayerAsync();
        mapView.Map.Layers.Add(ml);

        //Zoom to initial extent of the layer
        SetExtent(ml);

        mvm.SetWaitingCursor(false);

    }

    /// <summary>
    /// New informations from Geolocator arrived
    /// </summary>        
    /// <param name="e">Event arguments for new position</param>
    [SuppressMessage("Usage", "VSTHRD100:Avoid async void methods")]
    private async void MyLocationPositionChanged(Location e)
    {
        try
        {
            // check if I should update location
            if (!_updateLocation)
                return;

            await Application.Current?.Dispatcher?.DispatchAsync(async () =>
            {
                MapViewModel vm = this.BindingContext as MapViewModel;
                vm.RefreshCoordinates(e);

                await SetMapAccuracyColor(e.Accuracy);

                mapView?.MyLocationLayer.UpdateMyLocation(new Position(e.Latitude, e.Longitude));
                mapView.RefreshGraphics();

                if (e.Course != null)
                {
                    mapView?.MyLocationLayer.UpdateMyDirection(e.Course.Value, mapView?.Map.Navigator.Viewport.Rotation ?? 0);
                }

                if (e.Speed != null)
                {
                    mapView?.MyLocationLayer.UpdateMySpeed(e.Speed.Value);
                }

            })!;
        }
        catch (System.Exception ex)
        {
            await DisplayAlert("Alert", ex.Message, "OK");
            //Logger.Log(LogLevel.Error, ex.Message, ex);
        }
    }

    private void mapView_MapClicked(object sender, MapClickedEventArgs e)
    {
        //Make sure to disable map layer frame
        MapLayerFrame.IsVisible = false;
    }

    #region Buttons
    /// <summary>
    /// Will show a filer picker to add layers in the map
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void AddLayerButton_Clicked(object sender, EventArgs e)
    {
        MapViewModel mvm = this.BindingContext as MapViewModel;
        mvm.SetWaitingCursor(true);

        //Call a dialog for user to select a file
        FileResult fr = await PickLayer();
        if (fr != null)
        {
            MbTilesTileSource mbtilesTilesource = new MbTilesTileSource(new SQLiteConnectionString(fr.FullPath, false));
            byte[] tileSource = await mbtilesTilesource.GetTileAsync(new TileInfo { Index = new TileIndex(0, 0, 0) });

            TileLayer newTileLayer = new TileLayer(mbtilesTilesource);
            newTileLayer.Name = fr.FileName;

            //Insert at right location in collection
            InsertLayerAtRightPlace(newTileLayer);

        }

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
        MapViewModel vm = this.BindingContext as MapViewModel;
        vm.RefreshLayerCollection(mapView.Map.Layers);

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
            StartGPS();
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

        string wms_url = await DisplayPromptAsync(LocalizationResourceManager["MapPageAddWaypointDialogTitle"].ToString(),
            LocalizationResourceManager["MapPageAddWaypointDialogMessage"].ToString(),
            LocalizationResourceManager["GenericButtonOk"].ToString(),
            LocalizationResourceManager["GenericButtonCancel"].ToString(),
            LocalizationResourceManager["MapPageAddWaypointDialogPlaceholder"].ToString());

        if (wms_url != string.Empty)
        {
            //Insert
            await AddAWMSAsync(wms_url);
        }
    }

    #endregion

    #endregion

    #region METHODS

    /// <summary>
    /// Will zoom to the extent of the incoming memory layer
    /// </summary>
    /// <param name="inMemoryLayer"></param>
    public void SetExtent(MemoryLayer inMemoryLayer)
    {
        if (inMemoryLayer.Extent != null)
        {
            // Extend a bit more the rectangle by 2.5km
            var fieldDataExtent = new MRect(inMemoryLayer.Extent.MinX,
                inMemoryLayer.Extent.MinY,
                inMemoryLayer.Extent.MaxX,
                inMemoryLayer.Extent.MaxY).Grow(2500);

            mapView.Map.Navigator.ZoomToBox(box: fieldDataExtent, boxFit: MBoxFit.Fit);
        }
        else
        {
            // Fit to Canada with a 25km buffer
            var fieldDataExtent = new MRect(-139.649,45.144, -56.288, 72.378).Grow(25000);
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
            osmLayer.Name = "Open Street Map";
            mapControl.Map.Layers.Insert(0, osmLayer);
        }
        else
        {
            TileLayer osmLayer = Mapsui.Tiling.OpenStreetMap.CreateTileLayer(ApplicationLiterals.keywordWMS + "/3.0 Maui.net");
            osmLayer.Name = "Open Street Map";
            mapControl.Map.Layers.Insert(0, osmLayer);
        }

    }

    /// <summary>
    /// Will add a new WMS layer into the map page
    /// </summary>
    /// <param name="wmsURL"></param>
    /// <param name="layerIndex"></param>
    /// <param name="withCache"></param>
    public async Task AddAWMSAsync(string wmsURL, bool withCache = true)
    {
        MapViewModel vm = this.BindingContext as MapViewModel;
        vm.SetWaitingCursor(true);

        string fullURL = wmsURL;
        string[] splitURL = wmsURL.Split(ApplicationLiterals.keywordWMSLayers);
        string partialURL = splitURL[0];

        //Make sure a layer is added to the URL before continuing
        if (splitURL.Count() > 1 && splitURL[1].Contains("&"))
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

                //Insert at right location in collection
                InsertLayerAtRightPlace(tl);
            }
            else
            {

                HttpTileProvider provider = new HttpTileProvider(request);
                TileSource t = new TileSource(provider, schema);
                TileLayer tl = new TileLayer(t);
                tl.Name = layerNameFromURL;

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

            vm.SetWaitingCursor(false);
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
        //else
        //{
        //    if (App.Current.Resources.TryGetValue("White", out var errorColorvalue))
        //    {
        //        mapPageGrid.BackgroundColor = errorColorvalue as Microsoft.Maui.Graphics.Color;
        //    }

        //}

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
    /// Will start the GPS
    /// </summary>
    public async Task StartGPS()
    {
        _isCheckingGeolocation = true;

        try
        {

            GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));

            if (Application.Current == null)
                return;

            _cancelTokenSource = new CancellationTokenSource();

            var location = await Geolocation.GetLocationAsync(request, _cancelTokenSource.Token)
                .ConfigureAwait(false);
            if (location != null)
            {
                MyLocationPositionChanged(location);
            }


        }
        catch (System.Exception e)
        {

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                //Manage restricted access to other problems
                if (e.Message.ToLower().Contains("denied"))
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
                else
                {
                    //Generic error message regarding GPS
                    await Shell.Current.DisplayAlert(LocalizationResourceManager["DisplayAlertGPS"].ToString(), e.Message,
                                            LocalizationResourceManager["GenericButtonOk"].ToString());
                }

            });
        }
    }

    /// <summary>
    /// Will stop the GPS
    /// </summary>
    public async Task StopGPSAsync()
    {
        if (_isCheckingGeolocation && _cancelTokenSource != null && _cancelTokenSource.IsCancellationRequested == false)
        {
            await SetMapAccuracyColor(-99);
            _isCheckingGeolocation = false;
            _cancelTokenSource.Cancel();
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
            // The user canceled or something went wrong
        }

        return null;
    }

    /// <summary>
    /// Will create a new point layer for station location
    /// </summary>
    /// <returns></returns>
    private async Task<MemoryLayer> CreatePointLayerAsync()
    {
        return new MemoryLayer
        {
            Name = "Stations",
            IsMapInfoLayer = true,
            Features = await GetLocationsAsync(),
            Style = CreateBitmapStyle(),
        };
    }

    /// <summary>
    /// Will query the database to retrive some basic information about location
    /// and stations
    /// </summary>
    /// <returns></returns>
    private async Task<IEnumerable<IFeature>> GetLocationsAsync()
    {
        IEnumerable<IFeature> enumFeat = new IFeature[] { };

        if (da.PreferedDatabasePath != null && da.PreferedDatabasePath != string.Empty)
        {

            //Get an offset placement for labels
            GraphicPlacement gp = new GraphicPlacement();
            List<int> placementPool = Enumerable.Range(1, 8).ToList();
            Offset offset = new Offset();
            offset.X = gp.GetOffsetFromPlacementPriority(placementPool[2],32,32).Item1;
            offset.Y = gp.GetOffsetFromPlacementPriority(placementPool[2],32,32).Item2;

            SQLiteAsyncConnection currentConnection = new SQLiteAsyncConnection(da.PreferedDatabasePath);
            List<FieldLocation> fieldLoc = await currentConnection.QueryAsync<FieldLocation>
                ("SELECT LTRIM(SUBSTR(" + DatabaseLiterals.FieldLocationAlias + ", -6, 4), '0') as " + 
                DatabaseLiterals.FieldLocationAlias + ", " + DatabaseLiterals.FieldLocationLongitude + 
                ", " + DatabaseLiterals.FieldLocationLatitude + " FROM " + DatabaseLiterals.TableLocation);

            foreach (FieldLocation fl in fieldLoc)
            {
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


    #endregion

}