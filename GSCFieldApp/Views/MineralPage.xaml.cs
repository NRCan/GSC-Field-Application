using GSCFieldApp.ViewModel;

namespace GSCFieldApp.Views;

public partial class MineralPage : ContentPage
{
	public MineralPage(MineralViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;

	}

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

		MineralViewModel viewModel = BindingContext as MineralViewModel;
		await viewModel.FillPickers();
        await viewModel.InitModel();
        await viewModel.Load();
    }
}