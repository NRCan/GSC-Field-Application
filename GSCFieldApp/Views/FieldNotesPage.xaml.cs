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

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        var stack = Application.Current.MainPage.Navigation.NavigationStack;
        int countStack = Application.Current.MainPage.Navigation.NavigationStack.Count;

        if (countStack > 1)
        {
            Page lastPage = stack[countStack - 2];
            if (lastPage != null)
            {
                FieldNotesViewModel vmi = (FieldNotesViewModel)BindingContext;
                if (lastPage.GetType() == typeof(EarthmatPage))
                {
                    vmi.UpdateTable = Dictionaries.DatabaseLiterals.TableNames.earthmat;
                }
                else if (lastPage.GetType() == typeof(SamplePage))
                {
                    vmi.UpdateTable = Dictionaries.DatabaseLiterals.TableNames.sample;
                }
                else if (lastPage.GetType() == typeof(StationPage))
                {
                    vmi.UpdateTable = Dictionaries.DatabaseLiterals.TableNames.station;
                }
                vmi.UpdateRecordList();
            }

        }

    }

}