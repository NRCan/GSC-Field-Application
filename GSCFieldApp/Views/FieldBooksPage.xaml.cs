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

}