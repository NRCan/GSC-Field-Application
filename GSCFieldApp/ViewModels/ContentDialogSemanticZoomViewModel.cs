using System.Collections.ObjectModel;
using Template10.Mvvm;
using GSCFieldApp.Models;
using Windows.UI.Xaml;

namespace GSCFieldApp.ViewModels
{
    public class ContentDialogSemanticZoomViewModel: ViewModelBase
    {
        private ObservableCollection<SemanticDataGroup> _Groups;

        public string inAssignTable { get; set; }
        public string inParentFieldName { get; set; }
        public string inChildFieldName { get; set; }

        public ElementTheme userTheme { get; set; }

        public ContentDialogSemanticZoomViewModel()
        {
            //Force application theme on dialog, else it doesn't synchronize with user changes
            Services.SettingsServices.SettingsService _settings = Services.SettingsServices.SettingsService.Instance;
            if (_settings.AppTheme == ApplicationTheme.Dark)
            {
                userTheme = ElementTheme.Dark;
            }
            else
            {
                userTheme = ElementTheme.Light;
            }
        }

        public ObservableCollection<SemanticDataGroup> Groups
        {
            get { return _Groups; }
            set { _Groups = value; }
        }

        /// <summary>
        /// Will build the group from data, if it has all been set up.
        /// </summary>
        public void MakeGroup()
        {


            //Build list
            if (inAssignTable!=null && inParentFieldName!=null && inChildFieldName!=null)
            {

                //On init for new earthmats calculate values so UI shows stuff.
                Groups = new ObservableCollection<SemanticDataGroup>(SemanticDataGenerator.GetGroupedData(false, inAssignTable, inParentFieldName, inChildFieldName));
                RaisePropertyChanged("Groups");


            }

        }

    }
}
