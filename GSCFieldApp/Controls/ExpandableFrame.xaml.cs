using CommunityToolkit.Mvvm.Input;
using DotSpatial.Projections.Transforms;
using GSCFieldApp.Services;
using Mapsui.UI.Maui.Extensions;
using System.Reflection;

namespace GSCFieldApp.Controls;

public partial class ExpandableFrame : ContentView
{
    //Localize
    public LocalizationResourceManager LocalizationResourceManager
        => LocalizationResourceManager.Instance; // Will be used for in code dynamic local strings

    public static readonly BindableProperty FrameColorProperty =
        BindableProperty.Create(nameof(FrameColor), typeof(Color), typeof(ExpandableFrame), Color.FromArgb("#808080"));

    public static readonly BindableProperty FrameContentVisibilityProperty =
        BindableProperty.Create(nameof(FrameContentVisibility), typeof(bool), typeof(ExpandableFrame), true);

    public static readonly BindableProperty FrameTitleProperty =
        BindableProperty.Create(nameof(FrameTitle), typeof(string), typeof(ExpandableFrame), "");

    public static readonly BindableProperty InfoAlertVisibilityProperty =
        BindableProperty.Create(nameof(InfoAlertVisibility), typeof(bool), typeof(ExpandableFrame), false);

    public static readonly BindableProperty IInfoAlertParameterProperty =
        BindableProperty.Create(nameof(InfoAlertParameter), typeof(string), typeof(ExpandableFrame), "");

    public static readonly BindableProperty ChevronIconProperty =
    BindableProperty.Create(nameof(ChevronIcon), typeof(string), typeof(ExpandableFrame), "&#xF0140;");

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

    public bool InfoAlertVisibility
    {
        get => (bool)GetValue(InfoAlertVisibilityProperty);
        set => SetValue(InfoAlertVisibilityProperty, value);
    }

    public string InfoAlertParameter
    {
        get => (string)GetValue(IInfoAlertParameterProperty);
        set => SetValue(IInfoAlertParameterProperty, value);
    }

    public string ChevronIcon
    {
        get => (string)GetValue(ChevronIconProperty);
        set => SetValue(ChevronIconProperty, value);
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
        ChevronIcon = FrameContentVisibility ? "&#xF0140;" : "&#xF0143;";

    }

    /// <summary>
    /// InfoAlert pop-up command to show a "Did you know?" message
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task InfoAlert()
    {
        string title = LocalizationResourceManager["GenericInfoDidYouKnow"].ToString();
        await Shell.Current.DisplayAlert(title, InfoAlertParameter, LocalizationResourceManager["GenericButtonOk"].ToString());

    }

    public ExpandableFrame()
	{
		InitializeComponent();
	}
}