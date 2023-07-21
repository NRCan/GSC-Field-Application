using GSCFieldApp.ViewModel;

namespace GSCFieldApp.Views;

public partial class FieldBooksPage : ContentPage
{
    private FieldBooksViewModel _vm;

    public FieldBooksPage(FieldBooksViewModel vm)
	{
		InitializeComponent();
		BindingContext = _vm = vm;
	}

}