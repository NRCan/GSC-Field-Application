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

    /// <summary>
    /// Bind to the command on the delete button
    /// </summary>
    public static readonly BindableProperty DeleteItemProperty =
            BindableProperty.Create(nameof(DeleteItem), typeof(ICommand), typeof(ConcatenatedCollection), defaultBindingMode: BindingMode.TwoWay);

    public ObservableCollection<ComboBoxItem> ConcatSource
    {
        get => (ObservableCollection<ComboBoxItem>)GetValue(ConcatSourceProperty);
        set => SetValue(ConcatSourceProperty, value);
    }

    public ICommand DeleteItem
    {
        get => (ICommand)GetValue(DeleteItemProperty);
        set => SetValue(DeleteItemProperty, value);
    }

    public ConcatenatedCollection()
	{
		InitializeComponent();
	}

}