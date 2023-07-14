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

namespace GSCFieldApp.Views;

public partial class MapPage : ContentPage
{

	public MapPage()
	{
		InitializeComponent();

        var mapControl = new Mapsui.UI.Maui.MapControl();

        var tileLayer = Mapsui.Tiling.OpenStreetMap.CreateTileLayer();

        mapControl.Map.Layers.Add(tileLayer);
        mapControl.Map.Widgets.Add(new Mapsui.Widgets.ScaleBar.ScaleBarWidget(mapControl.Map) { TextAlignment = Mapsui.Widgets.Alignment.Center, HorizontalAlignment = Mapsui.Widgets.HorizontalAlignment.Left, VerticalAlignment = Mapsui.Widgets.VerticalAlignment.Bottom });

        mapView.Map = mapControl.Map;

    }

}