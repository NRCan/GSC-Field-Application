using GSCFieldApp.ViewModel;

namespace GSCFieldApp.Views;

public partial class MineralizationAlterationPage : ContentPage
{
	public MineralizationAlterationPage(MineralizationAlterationViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}