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
    }
}