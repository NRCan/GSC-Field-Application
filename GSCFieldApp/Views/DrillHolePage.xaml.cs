using GSCFieldApp.ViewModel;

namespace GSCFieldApp.Views;

public partial class DrillHolePage : ContentPage
{
	public DrillHolePage(DrillHoleViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);	

		DrillHoleViewModel vm2 = BindingContext as DrillHoleViewModel;
        if (!vm2.IsLoaded)
        {
            await vm2.FillPickers();
            await vm2.InitModel();
            await vm2.Load();
        }

    }

    private void DrillHolePageHoleSizePicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        //Cast
        Picker picker = sender as Picker;
        if (picker != null && picker.SelectedItem != null)
        {
            DrillHoleViewModel vm3 = BindingContext as DrillHoleViewModel;
            _ = vm3.FillCoreSize();
        }

    }
}