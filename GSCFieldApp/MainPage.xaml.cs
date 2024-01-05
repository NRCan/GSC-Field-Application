using GSCFieldApp.ViewModel;

namespace GSCFieldApp;

public partial class MainPage : ContentPage
{

	public MainPage(MainViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
    }

}

