using GSCFieldApp.ViewModel;

namespace GSCFieldApp.Views;

public partial class FieldBooksPage : ContentPage
{
	public FieldBooksPage()
	{
		InitializeComponent();
		BindingContext = this;
	}

    public FieldBooksPage(FieldBooksViewModel vm) : this()
    {
        //InitializeComponent();

    }
}