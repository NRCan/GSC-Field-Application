using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using Template10.Controls;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Navigation;
using Windows.Networking.Connectivity;
using Windows.System;


namespace GSCFieldApp.Views
{
    public sealed partial class SettingsPage : Page
    {
        //Local setting
        //ApplicationDataContainer currentSettings = ApplicationData.Current.LocalSettings;

        readonly List<ToggleSwitch> commonSwitches = new List<ToggleSwitch>();
        readonly List<ToggleSwitch> bedrockSwitches = new List<ToggleSwitch>();
        readonly List<ToggleSwitch> surficialSwitches = new List<ToggleSwitch>();
        readonly Template10.Services.SerializationService.ISerializationService _SerializationService;

        public SettingsPage()
        {
            InitializeComponent();
            _SerializationService = Template10.Services.SerializationService.SerializationService.Json;
            //this.Loaded += SettingsPage_Loaded; ; //Set the toggle switches in the customize tab

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var index = int.Parse(_SerializationService.Deserialize(e.Parameter?.ToString()).ToString());
            ViewModel._selectedPivotIndex = index;

        }

        private void ThemeList_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Button senderButton = sender as Button;

            string picklistSelectedTheme = string.Empty;


            //TODO remove hardcoded names here
            if (senderButton.Name.ToLower().Contains(Dictionaries.DatabaseLiterals.KeywordStation))
            {
                picklistSelectedTheme = Dictionaries.DatabaseLiterals.KeywordStation;
            }
            else if (senderButton.Name.ToLower().Contains(Dictionaries.DatabaseLiterals.KeywordEarthmat))
            {
                picklistSelectedTheme = Dictionaries.DatabaseLiterals.KeywordEarthmat;
            }
            else if (senderButton.Name.ToLower().Contains(Dictionaries.DatabaseLiterals.KeywordSample))
            {
                picklistSelectedTheme = Dictionaries.DatabaseLiterals.KeywordSample;
            }
            else if (senderButton.Name.ToLower().Contains(Dictionaries.DatabaseLiterals.KeywordStructure))
            {
                picklistSelectedTheme = Dictionaries.DatabaseLiterals.KeywordStructure;
            }
            else if (senderButton.Name.ToLower().Contains(Dictionaries.DatabaseLiterals.KeywordDocument))
            {
                picklistSelectedTheme = Dictionaries.DatabaseLiterals.KeywordDocument;
            }
            else if (senderButton.Name.ToLower().Contains(Dictionaries.DatabaseLiterals.KeywordMA))
            {
                picklistSelectedTheme = Dictionaries.DatabaseLiterals.KeywordMA;
            }
            else if (senderButton.Name.ToLower().Contains(Dictionaries.DatabaseLiterals.KeywordMineral))
            {
                picklistSelectedTheme = Dictionaries.DatabaseLiterals.KeywordMineral;
            }
            else if (senderButton.Name.ToLower().Contains(Dictionaries.DatabaseLiterals.KeywordFossil))
            {
                picklistSelectedTheme = Dictionaries.DatabaseLiterals.KeywordFossil;
            }
            else if (senderButton.Name.ToLower().Contains(Dictionaries.DatabaseLiterals.KeywordPflow))
            {
                picklistSelectedTheme = Dictionaries.DatabaseLiterals.KeywordPflow;
            }
            else if (senderButton.Name.ToLower().Contains(Dictionaries.DatabaseLiterals.KeywordEnvironment))
            {
                picklistSelectedTheme = Dictionaries.DatabaseLiterals.KeywordEnvironment;
            }
            else if (senderButton.Name.ToLower().Contains(Dictionaries.DatabaseLiterals.KeywordDrill))
            {
                picklistSelectedTheme = Dictionaries.DatabaseLiterals.KeywordDrill;
            }
            OpenPicklistDialog(picklistSelectedTheme);

        }

        private void OpenPicklistDialog(string inTheme)
        {
            var modalD = Window.Current.Content as ModalDialog;
            var viewD = modalD.ModalContent as Views.PicklistDialog;
            viewD = new Views.PicklistDialog(inTheme);
            modalD.ModalContent = viewD;
            modalD.IsModal = true;
        }

        /// <summary>
        /// Easter egg that opens team picture
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TeamInfoTextBlock_Holding(object sender, Windows.UI.Xaml.Input.HoldingRoutedEventArgs e)
        {
            if (e.HoldingState == Windows.UI.Input.HoldingState.Completed)
            {
                this.AboutTeamPicture.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Close team picture.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AboutTeamPicture_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            this.AboutTeamPicture.Visibility = Visibility.Collapsed;
        }

        //Check to see if the user is using the latest app version
        private async void OnDownloadLinkClicked(Hyperlink sender, HyperlinkClickEventArgs args)
        {
            if (!IsInternetAvailable())
            {
                ShowMessage("No internet connection. Please check your network and try again.");
            }
            else
            {
                try
                {
                    string CurrentVersion = GetCurrentVersion();
                    string LatestVersion = await GetLatestVersionFromGitHubAsync();

                    if (string.IsNullOrWhiteSpace(LatestVersion))
                    {
                        ShowMessage("Unable to retrieve the latest version. Please try again later.");
                        return;
                    }

                    if (CurrentVersion == LatestVersion)
                    {
                        ShowMessage($"You have the latest version: {CurrentVersion}.");
                    }
                    else
                    {
                        ShowMessage($"A new version is available: {LatestVersion}. Your version: {CurrentVersion}.");

                        // Open the download page
                        var uri = new Uri("https://github.com/NRCan/GSC-Field-Application/releases");
                        await Launcher.LaunchUriAsync(uri);
                    }
                }
                catch (Exception ex)
                {
                    ShowMessage($"Error checking for updates: {ex.Message}");
                }
            }
        }
        

        private bool IsInternetAvailable()
        {
            var profile = NetworkInformation.GetInternetConnectionProfile();
            return profile != null && profile.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess;
        }

        private string GetCurrentVersion()
        {
            var version = Package.Current.Id.Version;
            return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }

        private async Task<string> GetLatestVersionFromGitHubAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; AppVersionChecker/1.0)");
                var response = await client.GetStringAsync("https://api.github.com/repos/NRCan/GSC-Field-Application/releases/latest");
                dynamic json = Newtonsoft.Json.JsonConvert.DeserializeObject(response); // Install Newtonsoft.Json
                return json?.tag_name; // Assuming 'tag_name' holds the release version, adjust if necessary
            }
        }

        private void ShowMessage(string message)
        {
            var dialog = new Windows.UI.Popups.MessageDialog(message);
            dialog.ShowAsync();
        }

    }
}
