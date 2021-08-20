using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using GSCFieldApp.ViewModels;
using Windows.UI.Xaml;
using Template10.Controls;
using Template10.Common;
using GSCFieldApp.Services.DatabaseServices;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Foundation;
using GSCFieldApp.Services.SettingsServices;
using Windows.ApplicationModel.Activation;
using System;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Media.Animation;

namespace GSCFieldApp.Views
{
    public sealed partial class FieldBooksPage : Page
    {

        public FieldBooksPageViewModel ProjectViewModel { get; set; }

        public FieldBooksPage()
        {
            InitializeComponent();
            //ProjectViewModel = new FieldBooksPageViewModel();


        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            FieldBooksPageViewModel thisViewModel = this.DataContext as FieldBooksPageViewModel;
            if (thisViewModel != null)
            {
                thisViewModel.SelectActiveProject();
            }

        }

    }
}
