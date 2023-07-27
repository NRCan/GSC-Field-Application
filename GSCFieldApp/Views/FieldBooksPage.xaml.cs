using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using GSCFieldApp.ViewModels;

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

        private void OnGridViewItemClicked(object sender, ItemClickEventArgs e)
        {

        }
    }
}
