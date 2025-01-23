using GSCFieldApp.Models;
using GSCFieldApp.Services.DatabaseServices;
using GSCFieldApp.ViewModel;
using GSCFieldApp.Dictionaries;
using Microsoft.Maui.Controls;

namespace GSCFieldApp.Views;

public partial class FieldNotesPage : ContentPage
{
    //UI column management in the collection view for all notes
    //Only meant for WinUI3
    private int _fieldNotesColumns = 3;

    public int FieldNotesColumns 
    {
        get 
        {
            if ((int)(Width / 250) < 3)
            {
                return _fieldNotesColumns = 3;
            }
            else
            {
                return _fieldNotesColumns = (int)(Width / 250);
            }
            
        }
        set { _fieldNotesColumns = value; }
    }

    public FieldNotesPage(FieldNotesViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
        SizeChanged += FieldNotesPage_SizeChanged;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        //Force reset of theme header bar for hidden/show preferences that might be coming from config.
        FieldNotesViewModel vm2 = (FieldNotesViewModel)BindingContext;
        vm2.ThemeHeaderBarsRefresh();

        vm2.FillFieldNotesAsync().ConfigureAwait(false);

    }

    private void FieldNotesPage_SizeChanged(object sender, EventArgs e)
    {

        OnPropertyChanged(nameof(FieldNotesColumns));
    }
}