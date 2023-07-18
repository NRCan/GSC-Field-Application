using GSCFieldApp.ViewModel;

namespace GSCFieldApp.Views;

public partial class FieldBook234Page : ContentView
{
	public FieldBook234Page(FieldBook234Page vm)
	{
		InitializeComponent();
        BindingContext = vm;
	}
}