using GSCFieldApp.ViewModels;
using System;
using System.Threading.Tasks;
using Template10.Common;
using Template10.Controls;
using Template10.Services.NavigationService;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace GSCFieldApp.Views
{
    public sealed partial class Shell : Page
    {
        public static Shell Instance { get; set; }
        public static HamburgerMenu HamburgerMenu => Instance.MyHamburgerMenu;

        public ShellViewModel SViewModel { get; set; }

        public Shell()
        {
            Instance = this;
            InitializeComponent();
            this.SViewModel = new ShellViewModel();
            this.Loaded += Shell_LoadedAsync;
        }

        private async void Shell_LoadedAsync(object sender, RoutedEventArgs e)
        {
            try
            {
                // Navigate to map page
                INavigationService navService = BootStrapper.Current.NavigationService;
                navService.Navigate(typeof(Views.FieldBooksPage));
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                // Show an error message using MessageDialog
                var dialog = new MessageDialog("An error occurred: " + ex.Message, "Error");
                await dialog.ShowAsync();
            }
        }

        public Shell(INavigationService navigationService) : this()
        {
            SetNavigationService(navigationService);
        }

        public void SetNavigationService(INavigationService navigationService)
        {
            MyHamburgerMenu.NavigationService = navigationService;
        }

    }
}

