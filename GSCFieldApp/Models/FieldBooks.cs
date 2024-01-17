using CommunityToolkit.Mvvm.Input;
using GSCFieldApp.Dictionaries;
using GSCFieldApp.Services.DatabaseServices;
using GSCFieldApp.Views;
using NetTopologySuite.IO;

namespace GSCFieldApp.Models
{
    public partial class FieldBooks
    {
        #region PROPERTIES

        //Define here are the properties that can be used for edit and delete operation
        public string GeologistGeolcode { get; set; } //Since geologist is not mandatory, a mix of geologist and geolcode will always give a value

        public string StationNumber { get; set; }
        public string StationLastEntered { get; set; }
        public string ProjectPath { get; set; }
        public string ProjectDBPath { get; set; }
        public string CreateDate { get; set; }
        public Metadata metadataForProject { get; set; }

        private DataAccess da = new DataAccess();

        public Microsoft.Maui.Graphics.Color FieldbookColor { get; set;}

        #endregion

        public FieldBooks()
        {
            metadataForProject = new Metadata();
        }

        /// <summary>
        /// On a field book item tap, keep path to database in preferences
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [RelayCommand]
        public void Tap()
        {
            //Keep path
            da.PreferedDatabasePath = ProjectDBPath;

            //Keep theme
            Preferences.Set(nameof(DatabaseLiterals.FieldUserInfoPName), metadataForProject.FieldworkType);
        }

        /// <summary>
        /// On a field book item tap, keep path to database in preferences
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [RelayCommand]
        public async Task DoubleTapAsync()
        {
            //Keep path
            da.PreferedDatabasePath = ProjectDBPath;

            //Keep theme
            Preferences.Set(nameof(DatabaseLiterals.FieldUserInfoPName), metadataForProject.FieldworkType);

            //Navigate to fieldbook page and send along the metadata
            if (metadataForProject != null)
            {
                await Shell.Current.GoToAsync($"{nameof(FieldBookPage)}",
                    new Dictionary<string, object>
                    {
                        [nameof(Metadata)] = metadataForProject,
                    }
                );
            }
        }
    }
}
