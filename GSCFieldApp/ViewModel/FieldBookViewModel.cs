using CommunityToolkit.Mvvm.Input;
using GSCFieldApp.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSCFieldApp.Services.DatabaseServices;
using GSCFieldApp.Models;
using GSCFieldApp.Dictionaries;
using GSCFieldApp.Themes;
using ShimSkiaSharp;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace GSCFieldApp.ViewModel
{
    public partial class FieldBookViewModel: ObservableObject
    {
        #region INIT

        DataAccess da = new DataAccess();
        private Metadata model = new Metadata();
        private ComboBox _projectType = new ComboBox();

        #endregion

        #region PROPERTIES
        
        public Metadata Model { get { return model; } set { model = value; } }
        public ComboBox ProjectType { get { return _projectType; } set { _projectType = value; } }

        #endregion

        public FieldBookViewModel()
        {
            //Make sure a database exist before moving on
            _ = ValidateDabaseExistence();
        }

        private async Task ValidateDabaseExistence()
        {
            bool validates = await da.CreateDatabaseFromResource(da.DatabaseFilePath);

            //When database exist, move on with filling some picker controls
            if (validates) { FillProjectType(); }
        }

        #region RELAY COMMANDS

        [RelayCommand]
        async Task ValidateAndSave()
        {
            //Validate if all mandatory entries have been filled.
            if (Model.isValid || !Model.isValid)
            {
                //Make sure current field book database exists
#if WINDOWS
                da.PreferedDatabasePath = Path.Combine(FileSystem.Current.AppDataDirectory, Model.FieldBookFileName + DatabaseLiterals.DBTypeSqlite);
#elif ANDROID
                da.PreferedDatabasePath = Path.Combine(FileSystem.Current.AppDataDirectory, Model.FieldBookFileName + DatabaseLiterals.DBTypeSqliteDeprecated);
#else
                da.PreferedDatabasePath = Path.Combine(FileSystem.Current.AppDataDirectory, Model.FieldBookFileName + DatabaseLiterals.DBTypeSqlite);
#endif
                //Validate if new entry or update
                if (model.MetaID > 0)
                {
                    await da.SaveItemAsync(Model, true);
                }
                else
                {
                    //Create new field book database
                    await da.CreateDatabaseFromResource(da.PreferedDatabasePath);

                    //Fill out missing values in model
                    SetModel();

                    //Insert new record in F_METADATA
                    await da.SaveItemAsync(Model, false);
                }

                //Close to be sure
                await da.CloseConnectionAsync();

                //Exit
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                //Show error
                await Shell.Current.DisplayAlert("Warning", "Some mandatory fields have not been filled.", "Ok");
            }


        }

        [RelayCommand]
        public async Task Back()
        {
            await Shell.Current.GoToAsync("..");
        }

#endregion

        #region METHODS

        /// <summary>
        /// Will fill the project type combobox
        /// </summary>
        private async void FillProjectType()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldUserInfoFWorkType;

            //Make sure to user default database rather then the prefered one. This one will always be there.
            _projectType = await da.GetComboboxListWithVocabAsync(DatabaseLiterals.TableMetadata, fieldName);


            //Update UI
            OnPropertyChanged("ProjectType");

        }

        /// <summary>
        /// Will fill out missing fields for model. Default auto-calculated values
        /// </summary>
        private void SetModel()
        {
            //Make sure it's for a new field book
            if (Model.MetaID == 0) 
            {
                //Get current application version
                Model.Version = AppInfo.Current.VersionString;
                Model.StartDate = String.Format("{0:d}", DateTime.Today);
                Model.VersionSchema = DatabaseLiterals.DBVersion.ToString();

                //Process pickers
                if (ProjectType.cboxDefaultItemIndex != -1)
                {
                    Model.FieldworkType = ProjectType.cboxItems[ProjectType.cboxDefaultItemIndex].itemValue;
                }
            }

        }

        #endregion
    }
}
