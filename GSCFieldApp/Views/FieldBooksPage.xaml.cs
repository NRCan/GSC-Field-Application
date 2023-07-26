using CommunityToolkit.Maui.Core.Views;
using GSCFieldApp.ViewModel;

namespace GSCFieldApp.Views;

public partial class FieldBooksPage : ContentPage
{

    public FieldBooksPage(FieldBooksViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        await DisplayAlert("test", "test", "test");
    }
}