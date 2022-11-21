using Windows.UI.Xaml;
using System.Threading.Tasks;
using GSCFieldApp.Services.SettingsServices;
using GSCFieldApp.Services.DatabaseServices;
using Windows.ApplicationModel.Activation;
using Template10.Controls;
using Template10.Common;
using System;
using System.Linq;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Controls;
using SQLite;
using System.IO;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.Foundation;

namespace GSCFieldApp
{
    /// Documentation on APIs used in this page:
    /// https://github.com/Windows-XAML/Template10/wiki

    [Bindable]
    sealed partial class App : Template10.Common.BootStrapper
    {
        DataLocalSettings localSetting = new DataLocalSettings();

        public App()
        {

            InitializeComponent();
            SplashFactory = (e) => new Views.Splash(e);

            #region App settings

            var _settings = SettingsService.Instance;
            RequestedTheme = _settings.AppTheme;
            CacheMaxDuration = _settings.CacheMaxDuration;
            ShowShellBackButton = _settings.UseShellBackButton;

            

            #endregion
            
            SetESRILicence();

        }

        public override Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Will init a modal dialog to use for user info view page if needed.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public override async Task OnInitializeAsync(IActivatedEventArgs args)
        {
            
            //Will enable smaller size window for desktop testing, compared to default 500x320.
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(200, 200));

            if (Window.Current.Content as ModalDialog == null)
            {
                // create a new frame 
                var nav = NavigationServiceFactory(BackButton.Attach, ExistingContent.Include);

                // create modal root
                Window.Current.Content = new ModalDialog
                {
                    DisableBackButtonWhenModal = false,
                    Content = new Views.Shell(nav),
                };
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// Licence app to use ESRI ArcGIS Runtime SDK in Lite mode
        /// The following lines need not be here, they just need to appear prior to any use of SDK functionality.
        /// </summary>
        public void SetESRILicence()
        {
            string licenseKey = "runtimelite,1000,rud6138198800,none,YYPJD4SZ8P4YTJT46103";
            Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.SetLicense(licenseKey);
        }

    }
}

