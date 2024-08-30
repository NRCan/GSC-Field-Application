using GSCFieldApp.ViewModel;

namespace GSCFieldApp.Views;

public partial class MineralizationAlterationPage : ContentPage
{
	public MineralizationAlterationPage(MineralizationAlterationViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

		MineralizationAlterationViewModel vm2 = BindingContext as MineralizationAlterationViewModel;
		await vm2.FillPickers();
		await vm2.InitModel();
		await vm2.Load();
    }
}