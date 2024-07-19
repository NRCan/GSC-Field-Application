using GSCFieldApp.Models;
using GSCFieldApp.Services.DatabaseServices;
using GSCFieldApp.ViewModel;
using Microsoft.Maui.Controls;

namespace GSCFieldApp.Views;

public partial class FieldNotesPage : ContentPage
{
	public FieldNotesPage(FieldNotesViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
    }
}