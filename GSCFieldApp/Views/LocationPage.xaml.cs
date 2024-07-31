using GSCFieldApp.ViewModel;

namespace GSCFieldApp.Views;

public partial class LocationPage : ContentPage
{
	public LocationPage(LocationViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

		LocationViewModel vm2 = (LocationViewModel)BindingContext;
        await vm2.FillPickers();
        await vm2.Load(); 
    }
}