using GSCFieldApp.ViewModel;

namespace GSCFieldApp.Views;

public partial class StationPage : ContentPage
{
	public StationPage(StationViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }
}