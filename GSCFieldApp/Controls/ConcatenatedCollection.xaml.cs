using CommunityToolkit.Mvvm.Input;
using GSCFieldApp.Themes;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace GSCFieldApp.Controls;

public partial class ConcatenatedCollection : ContentView
{
    /// <summary>
    /// Bind to the item source collection property
    /// </summary>
    public static readonly BindableProperty ConcatSourceProperty =
       BindableProperty.Create(nameof(ConcatSource), typeof(ObservableCollection<ComboBoxItem>), typeof(ConcatenatedCollection), defaultBindingMode: BindingMode.TwoWay);

    public ObservableCollection<ComboBoxItem> ConcatSource
    {
        get => (ObservableCollection<ComboBoxItem>)GetValue(ConcatSourceProperty);
        set => SetValue(ConcatSourceProperty, value);
    }

    /// <summary>
    /// Will delete a selected item in quality collection box.
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public void DeleteItem(ComboBoxItem item)
    {
        if (ConcatSource.Contains(item))
        {
            ConcatSource.Remove(item);
            OnPropertyChanged(nameof(ConcatSource));
        }

    }

    public ConcatenatedCollection()
	{
		InitializeComponent();
	}

}