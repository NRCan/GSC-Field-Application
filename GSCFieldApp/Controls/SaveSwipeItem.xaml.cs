using Mapsui.UI.Maui.Extensions;

namespace GSCFieldApp.Controls;

public partial class SaveSwipeItem : ContentView
{
    /// <summary>
    /// Set a custom bindable property of color
    /// So the control gets whatever color is coming from the parent form
    /// </summary>
    public static readonly BindableProperty ButtonColorProperty =
        BindableProperty.Create(nameof(ButtonColor), typeof(Color), typeof(SaveSwipeItem), Mapsui.Styles.Color.FromString("Grey").ToNative());

    public static readonly BindableProperty ButtonLightColorProperty =
    BindableProperty.Create(nameof(ButtonLightColor), typeof(Color), typeof(SaveSwipeItem), Mapsui.Styles.Color.FromString("Grey").ToNative());

    public Color ButtonColor
    {
        get => (Color)GetValue(ButtonColorProperty);
        set => SetValue(ButtonColorProperty, value);
    }

    public Color ButtonLightColor
    {
        get => (Color)GetValue(ButtonLightColorProperty);
        set => SetValue(ButtonLightColorProperty, value);
    }

    public SaveSwipeItem()
	{
        InitializeComponent();
	}
}