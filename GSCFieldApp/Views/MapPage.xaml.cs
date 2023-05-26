using GSCFieldApp.ViewModel;

namespace GSCFieldApp.Views;

public partial class MapPage : ContentPage
{
	public MapPage(MapViewModel vm)
	{
		InitializeComponent();

		BindingContext = vm;
	}
}