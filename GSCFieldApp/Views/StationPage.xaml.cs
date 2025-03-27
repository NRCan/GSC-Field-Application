using GSCFieldApp.Models;
using GSCFieldApp.ViewModel;
using GSCFieldApp.Services;

namespace GSCFieldApp.Views;


public partial class StationPage : ContentPage
{
    //Localize
    public LocalizationResourceManager LocalizationResourceManager
    => LocalizationResourceManager.Instance; // Will be used for in code dynamic local strings

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
        await vm2.InitModel();
        await vm2.Load(); //In case it is coming from an existing record in field notes

        //Set the title if waypoint
        if (vm2.Station != null && vm2.Station.IsWaypoint)
        {
            this.Title = LocalizationResourceManager["StationPageWaypoint"].ToString();
        }

    }

}