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

        //Initialize map control and GPS
        var tileLayer = Mapsui.Tiling.OpenStreetMap.CreateTileLayer("NRCan_GSCFieldApp/3.0 Maui.net");

        mapControl.Map.Layers.Add(tileLayer);
        mapControl.Map.Widgets.Add(new Mapsui.Widgets.ScaleBar.ScaleBarWidget(mapControl.Map) { TextAlignment = Mapsui.Widgets.Alignment.Center, HorizontalAlignment = Mapsui.Widgets.HorizontalAlignment.Left, VerticalAlignment = Mapsui.Widgets.VerticalAlignment.Bottom });

        mapView.Map = mapControl.Map;

        this.Loaded += MapPage_Loaded;
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
                break;
            }
        }

        //Manage GPS
        if (!_isCheckingGeolocation)
        {
            StartGPS();
        }
    }

    private async void MapPage_Loaded(object sender, EventArgs e)
    {

        //Manage symbol and layers
        await AddSymbolToRegistry();
        MemoryLayer ml = await CreatePointLayerAsync();
        mapView.Map.Layers.Add(ml);
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
        catch (Exception ex)
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
        //Call a dialog for user to select a file
        FileResult fr = await PickLayer();
        if (fr != null)
        {
            MbTilesTileSource mbtilesTilesource = new MbTilesTileSource(new SQLiteConnectionString(fr.FullPath, false));
            byte[] tileSource = await mbtilesTilesource.GetTileAsync(new TileInfo { Index = new TileIndex(0, 0, 0) });

            TileLayer newTileLayer = new TileLayer(mbtilesTilesource);
            newTileLayer.Name = fr.FileName;

            //Insert at index 1
            //Index 0 would be OSM previous 1 would be location icon.
            mapView.Map.Layers.Insert(1, newTileLayer);

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
        }
        else
        {
            StartGPS();
        }
    }

    #endregion

    #endregion

    #region METHODS

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
        catch (Exception e)
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
        //finally 
        //{
        //    _isCheckingGeolocation = false;
        //}
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
        catch (Exception ex)
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