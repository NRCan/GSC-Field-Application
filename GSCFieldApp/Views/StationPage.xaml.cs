using GSCFieldApp.Models;
using GSCFieldApp.ViewModel;

namespace GSCFieldApp.Views;


public partial class StationPage : ContentPage
{
	public StationPage(StationViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        //After binding context is setup fill pickers
        StationViewModel vm2 = this.BindingContext as StationViewModel;
        _ = vm2.FillPickers();
    }

}