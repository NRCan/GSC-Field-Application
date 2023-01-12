using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml;
using Template10.Controls;


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

    }
}
