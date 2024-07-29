using CommunityToolkit.Mvvm.Input;
using DotSpatial.Projections.Transforms;
using Mapsui.UI.Maui.Extensions;
using System.Reflection;

namespace GSCFieldApp.Controls;

public partial class ExpandableFrame : ContentView
{
    public static readonly BindableProperty FrameColorProperty =
        BindableProperty.Create(nameof(FrameColor), typeof(Color), typeof(ExpandableFrame), Mapsui.Styles.Color.FromString("Grey").ToNative());

    public static readonly BindableProperty FrameContentVisibilityProperty =
        BindableProperty.Create(nameof(FrameContentVisibility), typeof(bool), typeof(ExpandableFrame), true);

    public static readonly BindableProperty FrameTitleProperty =
        BindableProperty.Create(nameof(FrameTitle), typeof(string), typeof(ExpandableFrame), "");

    public static readonly BindableProperty FrameWidthProperty =
    BindableProperty.Create(nameof(FrameWidth), typeof(string), typeof(ExpandableFrame), "");

    public Color FrameColor
    {
        get => (Color)GetValue(FrameColorProperty);
        set => SetValue(FrameColorProperty, value);
    }

    public string FrameTitle
    {
        get => (string)GetValue(FrameTitleProperty);
        set => SetValue(FrameTitleProperty, value);
    }

    public bool FrameContentVisibility
    {
        get => (bool)GetValue(FrameContentVisibilityProperty);
        set => SetValue(FrameContentVisibilityProperty, value);
    }

    public string FrameWidth
    {
        get => (string)GetValue(FrameWidthProperty);
        set => SetValue(FrameWidthProperty, value);
    }

    /// <summary>
    /// Hide command to hide group of controls
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task Hide()
    {
        // Reverse
        FrameContentVisibility = FrameContentVisibility ? false : true;

    }

    public ExpandableFrame()
	{
		InitializeComponent();
	}
}