using GSCFieldApp.ViewModel;

namespace GSCFieldApp.Views;

public partial class DrillHolePage : ContentPage
{
	public DrillHolePage(DrillHoleViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);	

		DrillHoleViewModel vm2 = BindingContext as DrillHoleViewModel;
		_ = vm2.FillPickers();
        _ = vm2.InitModel();
        _ = vm2.Load();
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