using GSCFieldApp.ViewModel;

namespace GSCFieldApp.Views;

public partial class AboutPage : ContentPage
{
    public AboutPage()
	{
        InitializeComponent();
		BindingContext = this;
    }
	public AboutPage(AboutPageViewModel vm): this()
	{
		InitializeComponent();
		
	}
}