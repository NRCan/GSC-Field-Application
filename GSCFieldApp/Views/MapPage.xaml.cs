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

namespace GSCFieldApp.Views;

public partial class MapPage : ContentPage
{
    private CancellationTokenSource? gpsCancelation;
    private bool _updateLocation = true;
    private MapControl mapControl = new Mapsui.UI.Maui.MapControl();

    public MapPage(MapViewModel vm)
	{
		InitializeComponent();

        //Initialize map control and GPS
        var tileLayer = Mapsui.Tiling.OpenStreetMap.CreateTileLayer();

        mapControl.Map.Layers.Add(tileLayer);
        mapControl.Map.Widgets.Add(new Mapsui.Widgets.ScaleBar.ScaleBarWidget(mapControl.Map) { TextAlignment = Mapsui.Widgets.Alignment.Center, HorizontalAlignment = Mapsui.Widgets.HorizontalAlignment.Left, VerticalAlignment = Mapsui.Widgets.VerticalAlignment.Bottom });

        mapView.Map = mapControl.Map;

        StartGPS();

        BindingContext = vm;

    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

    }

    #region METHODS
    /// <summary>
    /// Will start the GPS
    /// </summary>
    [Obsolete]
    public async void StartGPS()
    {
        try
        {
            this.gpsCancelation?.Dispose();
            this.gpsCancelation = new CancellationTokenSource();

            await Task.Run(async () =>
            {
                while (!gpsCancelation.IsCancellationRequested)
                {
                    var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
#if __MAUI__ // WORKAROUND for Preview 11 will be fixed in Preview 13 https://github.com/dotnet/maui/issues/3597
                    if (Application.Current == null)
                        return;

                    await Application.Current.Dispatcher.DispatchAsync(async () =>
                    {
#else
                    await Device.InvokeOnMainThreadAsync(async () =>
                    {
#endif
                        var location = await Geolocation.GetLocationAsync(request, this.gpsCancelation.Token)
                            .ConfigureAwait(false);
                        if (location != null)
                        {   
                            MyLocationPositionChanged(location);
                        }
                    }).ConfigureAwait(false);

                    await Task.Delay(200).ConfigureAwait(false);
                }
            }, gpsCancelation.Token).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Mapsui.Logging.Logger.Log(LogLevel.Error, e.Message, e);
        }
    }

    /// <summary>
    /// Will stop the GPS
    /// </summary>
    public void StopGPS()
    {
        this.gpsCancelation?.Cancel();
    }

    /// <summary>
    /// Will open a file picker dialog with custom extension set to mbtiles
    /// </summary>
    /// <returns></returns>
    public async Task<FileResult> PickLayer()
    {
        try
        {
            FilePickerFileType customFileType = new FilePickerFileType(
                new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                        {DevicePlatform.WinUI, new [] { ".mbtiles"} },
                        {DevicePlatform.Android, new [] { ".mbtiles"} },
                        {DevicePlatform.iOS, new [] { ".mbtiles"} },
                });

            PickOptions options = new PickOptions();
            options.PickerTitle = "Add Layer";
            options.FileTypes = customFileType;

            var result = await FilePicker.Default.PickAsync(options);
            if (result != null)
            {

            }

            return result;
        }
        catch (Exception ex)
        {
            // The user canceled or something went wrong
        }

        return null;
    }

    #endregion

    #region EVENTS

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

            await Application.Current?.Dispatcher?.DispatchAsync(() =>
            {
                MapViewModel vm = this.BindingContext as MapViewModel;
                vm.sensorLocation = e;

                mapView?.MyLocationLayer.UpdateMyLocation(new Position(e.Latitude, e.Longitude));
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
            Logger.Log(LogLevel.Error, ex.Message, ex);
        }
    }

    private async void AddLayerButton_Clicked(object sender, EventArgs e)
    {
        //Call a dialog for user to select a file
        FileResult fr = await PickLayer();
        if (fr != null) 
        {
            MbTilesTileSource mbtilesTilesource = new MbTilesTileSource(new SQLiteConnectionString(fr.FullPath, false));
            byte[] tileSource = await mbtilesTilesource.GetTileAsync(new TileInfo { Index = new TileIndex(0, 0, 0) });

            TileLayer newTileLayer = new TileLayer(mbtilesTilesource);
            mapControl.Map.Layers.Add(newTileLayer);


        }
    }


    #endregion

}