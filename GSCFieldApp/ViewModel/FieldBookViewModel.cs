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
using Microsoft.Maui.ApplicationModel.Communication;
using GSCFieldApp.Services;

namespace GSCFieldApp.ViewModel
{
    [QueryProperty(nameof(Metadata), nameof(Metadata))]
    public partial class FieldBookViewModel: ObservableObject
    {
        #region INIT

        DataAccess da = new DataAccess();
        private Metadata _model = new Metadata();
        private ComboBox _projectType = new ComboBox();
        private bool _canWrite = true;

        //Localization
        public LocalizationResourceManager LocalizationResourceManager
            => LocalizationResourceManager.Instance; // Will be used for in code dynamic local strings

        #endregion

        #region PROPERTIES

        [ObservableProperty]
        private Metadata metadata;

        public Metadata Model { get { return _model; } set { _model = value; } }
        public ComboBox ProjectType { get { return _projectType; } set { _projectType = value; } }
        public bool CanWrite { get { return _canWrite; } set { _canWrite = value; } }
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
                if (_model.MetaID > 0)
                {
                    //Fill out missing values in model
                    SetModel();

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
                await Shell.Current.GoToAsync("../");
            }
            else
            {
                //Show error
                await Shell.Current.DisplayAlert(LocalizationResourceManager["FieldBookPageFailedToSaveTitle"].ToString(),
                    LocalizationResourceManager["FieldBookPageFailedToSave"].ToString(), 
                    LocalizationResourceManager["GenericButtonOk"].ToString());
            }


        }

        [RelayCommand]
        public async Task Back()
        {
            await Shell.Current.GoToAsync("../");
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Will refill the form with existing values for update/editing purposes
        /// </summary>
        /// <returns></returns>
        public async Task Load()
        {
            if (metadata != null && metadata.isValid)
            {
                //Disable some key fields
                if (metadata.IsActive == 1)
                {
                    //Prevents user from updating key info after starting a survey
                    _canWrite = false;
                    OnPropertyChanged(nameof(CanWrite));
                }

                //Set model like actual record
                _model = metadata;
                OnPropertyChanged(nameof(Model));

                //Select values in pickers
                foreach (ComboBoxItem cbox in ProjectType.cboxItems)
                {
                    if (cbox.itemValue == _model.FieldworkType)
                    {
                        ProjectType.cboxDefaultItemIndex = ProjectType.cboxItems.IndexOf(cbox);
                        break;
                    }
                }
                OnPropertyChanged(nameof(ProjectType));
            }
        }

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
                Model.IsActive = 0; //Default to inactive on new field books since they don't have any stations yet.
            }

            //Process pickers
            if (ProjectType.cboxDefaultItemIndex != -1)
            {
                Model.FieldworkType = ProjectType.cboxItems[ProjectType.cboxDefaultItemIndex].itemValue;
            }

        }

        #endregion
    }
}
