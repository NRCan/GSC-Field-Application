using GSCFieldApp.ViewModel;

namespace GSCFieldApp.Views;

public partial class AboutPage : ContentPage
{
    public AboutPage(AboutPageViewModel vm)
	{
        InitializeComponent();
		BindingContext = vm;
    }
}