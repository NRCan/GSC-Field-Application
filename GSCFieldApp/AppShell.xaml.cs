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
    public ICommand NavigateToFieldBooksCommand { get; private set; }
    public ICommand NavigateToFieldNotesCommand { get; private set; }
    public ICommand NavigateToMapCommand { get; private set; }
    public ICommand DoBackupCommand { get; private set; }


    public AppShell()
	{
		InitializeComponent();

        #region MENU ROUTING
        //Registering some routing to other pages from app shell 
        Routing.RegisterRoute(nameof(DetailPage), typeof(DetailPage));
        Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
        Routing.RegisterRoute(nameof(FieldBooksPage), typeof(FieldBooksPage));
        Routing.RegisterRoute(nameof(FieldNotesPage), typeof(FieldNotesPage));
        Routing.RegisterRoute(nameof(MapPage), typeof(MapPage));


        //Will be used to navigate to setting page
        NavigateToSettingsCommand = new Command(async () => {
            //await GoToAsync(nameof(SettingsPage));
            await DisplayAlert("Alert", "Not yet implemented", "OK");
        });

        //Will be used to navigate to field books page
        NavigateToFieldBooksCommand = new Command(async () => {
            await GoToAsync("///" + nameof(FieldBooksPage));
        });

        //Will be used to navigate to field books page
        NavigateToFieldNotesCommand = new Command(async () => {

            await DisplayAlert("Alert", "Not yet implemented", "OK");
        });

        //Will be used to navigate to field books page
        NavigateToMapCommand = new Command(async () => {
            await GoToAsync("/" + nameof(MapPage));
        });

        //Will be used to trigger a backup process
        DoBackupCommand = new Command(async () =>
        {
            await SaveBackupDBFile(CancellationToken.None);
        });

        #endregion

        #region FORMS NAVIGATION

        Routing.RegisterRoute(nameof(FieldBookPage), typeof(FieldBookPage));
        Routing.RegisterRoute(nameof(StationPage), typeof(StationPage));

        #endregion

        BindingContext = this;
    }

    /// <summary>
    /// Will save prefered database
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    async Task SaveBackupDBFile(CancellationToken cancellationToken)
    {
        
        //Open desired file
        DataAccess da = new DataAccess();
        using Stream stream = System.IO.File.OpenRead(da.PreferedDatabasePath);

        //Get output name
        string outputFileName = Path.GetFileName(da.PreferedDatabasePath).Replace(DatabaseLiterals.DBTypeSqliteDeprecated, DatabaseLiterals.DBTypeSqlite);

        //Open save dialog
        var fileSaverResult = await FileSaver.Default.SaveAsync(outputFileName, stream, cancellationToken);

        //TODO: localize here
        if (fileSaverResult.IsSuccessful)
        {
            await Toast.Make($"The file was saved successfully to location: {fileSaverResult.FilePath}").Show(cancellationToken);
        }
        else
        {
            await Toast.Make($"The file was not saved successfully with error: {fileSaverResult.Exception.Message}").Show(cancellationToken);
        }
    }


}
