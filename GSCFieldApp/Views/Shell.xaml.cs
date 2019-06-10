using System;
using System.ComponentModel;
using System.Linq;
using Template10.Common;
using Template10.Controls;
using Template10.Services.NavigationService;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using GSCFieldApp.ViewModels;
using System.Threading.Tasks;
using Windows.UI.Popups;


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
            //Navigate to map page
            INavigationService navService = BootStrapper.Current.NavigationService;
            navService.Navigate(typeof(Views.FieldBooksPage));
            await Task.CompletedTask;
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

