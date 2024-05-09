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

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        //After binding context is setup fill pickers
        StationViewModel vm2 = this.BindingContext as StationViewModel;
        await vm2.FillPickers();
        //await vm2.SetFieldVisibility();
        await vm2.Load(); //In case it is coming from an existing record in field notes

        ////Overide title 
        //if (vm2 != null && vm2.Station != null && vm2.Station.StationAlias != string.Empty)
        //{
        //    this.Title = vm2.Station.StationAlias;
        //}
    }

}