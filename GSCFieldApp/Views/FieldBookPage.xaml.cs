using GSCFieldApp.ViewModel;
using System.Windows.Input;

namespace GSCFieldApp.Views;

public partial class FieldBookPage : ContentPage
{

    public FieldBookPage(FieldBookViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
    }

}