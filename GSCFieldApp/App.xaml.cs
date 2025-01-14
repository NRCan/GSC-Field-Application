using GSCFieldApp.Services;

#if WINDOWS
using WinUIEx;
#endif
namespace GSCFieldApp;

public partial class App : Application
{
    public LocalizationResourceManager LocalizationResourceManager 
        => LocalizationResourceManager.Instance; // Will be used for in code dynamic local strings


    //NOTE: Only working method to have a splash screen on windows for winui3 2025-01-13
    // https://dotmorten.github.io/WinUIEx/concepts/Splashscreen.html
    //Still not fixe after 4 years by MS https://github.com/microsoft/microsoft-ui-xaml/issues/4055
#if WINDOWS


    private SimpleSplashScreen fss { get; set; }

#endif

    public App()
	{
#if WINDOWS

        fss = SimpleSplashScreen.ShowDefaultSplashScreen();

#endif
        InitializeComponent();

		MainPage = new AppShell();

#if WINDOWS

        MainPage.Loaded += MainPage_Loaded;

#endif


    }

    /// <summary>
    /// Track loaded event and ask for desired permission before moving on.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void MainPage_Loaded(object sender, EventArgs e)
    {
        //await CheckAndRequestLocationPermission();

#if WINDOWS

        fss?.Hide();
        fss = null;

#endif

    }

    /// <summary>
    /// TODO Make sure to properly ask for permission
    /// https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/appmodel/permissions?view=net-maui-8.0&tabs=android
    /// For now this method doesn't show anything on screen that would help
    /// user to actually enable location in settings.
    /// </summary>
    /// <returns></returns>
    public async Task<PermissionStatus> CheckAndRequestLocationPermission()
    {

        PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

        if (status == PermissionStatus.Granted)
            return status;

        if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
        {
            // Prompt the user to turn on in settings
            // On iOS once a permission has been denied it may not be requested again from the application
            return status;
        }

        if (Permissions.ShouldShowRationale<Permissions.LocationWhenInUse>())
        {
            // Prompt the user with additional information as to why the permission is needed
            bool answer = await Shell.Current.DisplayAlert(LocalizationResourceManager["DisplayAlertGPSDenied"].ToString(),
                LocalizationResourceManager["DisplayAlertGPSMessage"].ToString(),
                LocalizationResourceManager["GenericButtonYes"].ToString(),
                LocalizationResourceManager["GenericButtonNo"].ToString());
        }

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

        });

        return status;
    }
}
