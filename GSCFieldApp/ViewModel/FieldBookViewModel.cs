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
        private List<Themes.ComboBoxItem> _projectType = new List<Themes.ComboBoxItem>();
        private int _selectedProjectType = -1;

        #endregion

        #region PROPERTIES
        
        public Metadata Model { get { return model; } set { model = value; } }
        public List<Themes.ComboBoxItem> ProjectType { get { return _projectType; } set { _projectType = value; } }
        public int SelectedProjectType { get { return _selectedProjectType; } set { _selectedProjectType = value; } }

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
                da.PreferedDatabasePath = Path.Combine(FileSystem.Current.AppDataDirectory, Model.FieldBookFileName + DatabaseLiterals.DBTypeSqlite);

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
        async Task Back()
        {
            //Navigate backward (../.. will navigate two pages back)"
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

            if (_projectType != null && _projectType.Count > 0)
            {
                if (_projectType[0].defaultValue != string.Empty)
                {
                    _selectedProjectType = _projectType[0].defaultIndex;
                }
            }

            //Update UI
            OnPropertyChanged("ProjectType");
            OnPropertyChanged("SelectedProjectType");

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
                if (SelectedProjectType != -1)
                {
                    Model.FieldworkType = ProjectType[SelectedProjectType].itemValue;
                }
            }

        }

        #endregion
    }
}
