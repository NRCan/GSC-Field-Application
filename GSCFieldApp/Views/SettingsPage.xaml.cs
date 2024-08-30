using GSCFieldApp.ViewModel;

namespace GSCFieldApp.Views;

public partial class SettingsPage : ContentPage
{
	public SettingsPage(SettingsViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

		SettingsViewModel vm2 = (SettingsViewModel)BindingContext;
		vm2.SettingRefresh();
    }
}