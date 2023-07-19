using GSCFieldApp.ViewModel;

namespace GSCFieldApp.Views;

public partial class FieldBookPage : ContentPage
{
	public FieldBookPage(FieldBookViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}