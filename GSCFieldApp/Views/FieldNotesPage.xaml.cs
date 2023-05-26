using GSCFieldApp.ViewModel;

namespace GSCFieldApp.Views;

public partial class FieldNotesPage : ContentPage
{
	public FieldNotesPage(FieldNotesViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}