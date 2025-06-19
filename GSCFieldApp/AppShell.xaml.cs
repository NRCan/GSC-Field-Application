using BruTile.Wms;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Storage;
using GSCFieldApp.Dictionaries;
using GSCFieldApp.Services;
using GSCFieldApp.Services.DatabaseServices;
using GSCFieldApp.Views;
using Mapsui.UI.Maui.Extensions;
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

            AppFileServices fileServices = new AppFileServices();
            await fileServices.SaveBackupPhotos(CancellationToken.None);

        });

        #endregion

        #region FORMS NAVIGATION

        ///Make each detail routes for field books page or field notes page since they
        ///are not navigable from the shell
        Routing.RegisterRoute("FieldBookPage", typeof(FieldBookPage));
        Routing.RegisterRoute("StationPage", typeof(StationPage));
        Routing.RegisterRoute("EarthmatPage", typeof(EarthmatPage));
        Routing.RegisterRoute("SamplePage", typeof(SamplePage));
        Routing.RegisterRoute("DocumentPage", typeof(DocumentPage));
        Routing.RegisterRoute("StructurePage", typeof(StructurePage));
        Routing.RegisterRoute("PaleoflowPage", typeof(PaleoflowPage));
        Routing.RegisterRoute("FossilPage", typeof(FossilPage));
        Routing.RegisterRoute("EnvironmentPage", typeof(EnvironmentPage));
        Routing.RegisterRoute("MineralPage", typeof(MineralPage));
        Routing.RegisterRoute("MineralizationAlterationPage", typeof(MineralizationAlterationPage));
        Routing.RegisterRoute("LocationPage", typeof(LocationPage));
        Routing.RegisterRoute("DrillHolePage", typeof(DrillHolePage));
        Routing.RegisterRoute("LineworkPage", typeof(LineworkPage));

        #endregion

        BindingContext = this;

        #region HAMBURGER MENU STYLING

#if WINDOWS
        // Set the foreground color for the Shell on Windows to black
        // Issue #444
        AppTheme appTheme = Application.Current.RequestedTheme;
        if (appTheme == AppTheme.Light)
        {
            Shell.SetForegroundColor(this, Mapsui.Styles.Color.Black.ToNative());
        }
#endif

#endregion
    }

}
