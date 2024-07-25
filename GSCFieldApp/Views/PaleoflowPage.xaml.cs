using GSCFieldApp.Services;
using GSCFieldApp.ViewModel;

namespace GSCFieldApp.Views;

public partial class PaleoflowPage : ContentPage
{
	public PaleoflowPage(PaleoflowViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        //After binding context is setup fill pickers
        PaleoflowViewModel vm2 = this.BindingContext as PaleoflowViewModel;
        await vm2.FillPickers();
        await vm2.InitModel();
        await vm2.Load(); //In case it is coming from an existing record in field notes
    }

    private void PaleoflowPageClassPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (true)
        {

        }
        PaleoflowViewModel vm3 = this.BindingContext as PaleoflowViewModel;
        vm3.Fill2ndRoundPickers();
    }
}