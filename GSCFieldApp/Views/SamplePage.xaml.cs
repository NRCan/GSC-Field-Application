using GSCFieldApp.ViewModel;

namespace GSCFieldApp.Views;

public partial class SamplePage : ContentPage
{
	public SamplePage(SampleViewModel vm)
	{
        InitializeComponent();
        BindingContext = vm;
    }
}