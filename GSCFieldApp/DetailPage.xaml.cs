using GSCFieldApp.ViewModel;

namespace GSCFieldApp;

public partial class DetailPage : ContentPage
{
	public DetailPage(DetailVieModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}