using GSCFieldApp.Views;
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
            await GoToAsync(nameof(SettingsPage));
        });

        //Will be used to navigate to field books page
        NavigateToFieldBooksCommand = new Command(async () => {
            await GoToAsync(nameof(FieldBooksPage));
        });

        //Will be used to navigate to field books page
        NavigateToFieldNotesCommand = new Command(async () => {

            await DisplayAlert("Alert", "Not yet implemented", "OK");
        });

        //Will be used to navigate to field books page
        NavigateToMapCommand = new Command(async () => {
            await GoToAsync(nameof(MapPage));
        });

        //Will be used to trigger a backup process
        DoBackupCommand = new Command(async () =>
        {
            await DisplayAlert("Alert", "Not yet implemented", "OK");
        });

        #endregion

        #region FORMS NAVIGATION

        Routing.RegisterRoute(nameof(StationPage), typeof(StationPage));

        //Will be used to navigate to field books page
        NavigateToFieldBooksCommand = new Command(async () => {
            await GoToAsync(nameof(StationPage));
        });
        #endregion

        BindingContext = this;
    }


}
