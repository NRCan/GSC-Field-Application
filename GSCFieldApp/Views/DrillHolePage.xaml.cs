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
		vm2.FillPickers();
		vm2.InitModel();
		vm2.Load();
    }
}