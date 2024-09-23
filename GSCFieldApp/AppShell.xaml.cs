using BruTile.Wms;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Storage;
using GSCFieldApp.Dictionaries;
using GSCFieldApp.Services;
using GSCFieldApp.Services.DatabaseServices;
using GSCFieldApp.Views;
using Microsoft.Extensions.Localization;
using Microsoft.Maui.Controls.Xaml;
using System.Text;
using System.Windows.Input;

namespace GSCFieldApp;

public partial class AppShell : Shell
{
    public ICommand NavigateToSettingsCommand { get; private set; }
    public ICommand DoBackupCommand { get; private set; }
    public ICommand DoPhotoBackupCommand { get; private set; }

    public AppShell()
	{
		InitializeComponent();

        #region MENU ROUTING
        //Registering some routing to other pages from app shell 
        //Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
        //Routing.RegisterRoute(nameof(FieldBooksPage), typeof(FieldBooksPage));
        //Routing.RegisterRoute(nameof(FieldNotesPage), typeof(FieldNotesPage));
        //Routing.RegisterRoute(nameof(MapPage), typeof(MapPage));
        //Routing.RegisterRoute(nameof(PicklistPage), typeof(PicklistPage));

        //Will be used to trigger a backup process
        DoBackupCommand = new Command(async () =>
        {
            AppFileServices fileServices = new AppFileServices();
            await fileServices.SaveBackupDBFile(CancellationToken.None);

        });

        DoPhotoBackupCommand = new Command(async () => {
            await DisplayAlert("Alert", "Not yet implemented", "OK");

        });

        #endregion

        #region FORMS NAVIGATION

        Routing.RegisterRoute(nameof(FieldBookPage), typeof(FieldBookPage));
        Routing.RegisterRoute("FieldNotesPage/StationPage", typeof(StationPage));
        Routing.RegisterRoute("FieldNotesPage/EarthmatPage", typeof(EarthmatPage));
        Routing.RegisterRoute("FieldNotesPage/SamplePage", typeof(SamplePage));
        Routing.RegisterRoute(nameof(DocumentPage), typeof(DocumentPage));
        Routing.RegisterRoute(nameof(StructurePage), typeof(StructurePage));
        Routing.RegisterRoute(nameof(PaleoflowPage), typeof(PaleoflowPage));
        Routing.RegisterRoute(nameof(FossilPage), typeof(FossilPage));
        Routing.RegisterRoute(nameof(EnvironmentPage), typeof(EnvironmentPage));
        Routing.RegisterRoute(nameof(MineralPage), typeof(MineralPage));
        Routing.RegisterRoute(nameof(MineralizationAlterationPage), typeof(MineralizationAlterationPage));
        Routing.RegisterRoute("FieldNotesPage/LocationPage", typeof(LocationPage));
        Routing.RegisterRoute(nameof(DrillHolePage), typeof(DrillHolePage));

        #endregion

        BindingContext = this;
    }

}
