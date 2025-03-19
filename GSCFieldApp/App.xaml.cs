using GSCFieldApp.Services;

namespace GSCFieldApp;

public partial class App : Application
{
    public LocalizationResourceManager LocalizationResourceManager 
        => LocalizationResourceManager.Instance; // Will be used for in code dynamic local strings


    public App()
	{
        InitializeComponent();

#if WINDOWS10_0_19041_0_OR_GREATER
        Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping(nameof(IPicker.Title), (handler, view) =>
        {
            if (handler.PlatformView is not null && view is Picker pick && !String.IsNullOrWhiteSpace(pick.Title))
            {
                handler.PlatformView.HeaderTemplate = new Microsoft.UI.Xaml.DataTemplate();
                //handler.PlatformView.PlaceholderText = pick.Title;
                pick.Title = null;
            }
        });
#endif

        MainPage = new AppShell();

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
