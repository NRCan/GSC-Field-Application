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

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        //Force reset of theme header bar for hidden/show preferences that might be coming from config.
        FieldNotesViewModel vm2 = (FieldNotesViewModel)BindingContext;
        vm2.ThemeHeaderBarsRefresh();
    }
}