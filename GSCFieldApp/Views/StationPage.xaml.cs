using GSCFieldApp.Models;
using GSCFieldApp.ViewModel;

namespace GSCFieldApp.Views;

[QueryProperty(nameof(FieldLocation), nameof(FieldLocation))]
public partial class StationPage : ContentPage
{
	public StationPage(StationViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }
}