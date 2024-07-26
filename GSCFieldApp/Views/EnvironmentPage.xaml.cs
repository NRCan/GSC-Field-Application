using GSCFieldApp.ViewModel;

namespace GSCFieldApp.Views;

public partial class EnvironmentPage : ContentPage
{
	public EnvironmentPage(EnvironmentViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;

	}

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        //After binding context is setup fill pickers
        EnvironmentViewModel vm2 = this.BindingContext as EnvironmentViewModel;
        await vm2.FillPickers();
        await vm2.InitModel();
        await vm2.Load(); //In case it is coming from an existing record in field notes
    }

}