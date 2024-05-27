using CommunityToolkit.Mvvm.Input;
using GSCFieldApp.Themes;
using System.Collections.ObjectModel;

namespace GSCFieldApp.Controls;

public partial class ConcatenatedCollection : ContentView
{
    public static readonly BindableProperty ConcatCollectionProperty =
       BindableProperty.Create(nameof(ConcatCollection), typeof(Color), typeof(ConcatenatedCollection), new ObservableCollection<ComboBoxItem> { } );

    public ObservableCollection<ComboBoxItem> ConcatCollection
    {
        get => (ObservableCollection<ComboBoxItem>)GetValue(ConcatCollectionProperty);
        set => SetValue(ConcatCollectionProperty, value);
    }



    public ConcatenatedCollection()
	{
		InitializeComponent();
	}

}