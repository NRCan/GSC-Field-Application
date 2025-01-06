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
using GSCFieldApp.Controls;
using ShimSkiaSharp;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using Microsoft.Maui.ApplicationModel.Communication;
using GSCFieldApp.Services;

namespace GSCFieldApp.ViewModel
{
    [QueryProperty(nameof(Metadata), nameof(Metadata))]
    public partial class FieldBookViewModel: FieldAppPageHelper
    {
        #region INIT
        private Metadata _model = new Metadata();
        private ComboBox _projectType = new ComboBox();
        private bool _canWrite = true;

        #endregion

        #region PROPERTIES

        [ObservableProperty]
        private Metadata metadata;

        public Metadata Model { get { return _model; } set { _model = value; } }
        public ComboBox ProjectType { get { return _projectType; } set { _projectType = value; } }
        public bool CanWrite { get { return _canWrite; } set { _canWrite = value; } }

        
        public bool FieldBookDescriptionExpand
        {
            get { return Preferences.Get(nameof(FieldBookDescriptionExpand), true); }
            set { Preferences.Set(nameof(FieldBookDescriptionExpand), value); }
        }

        public bool FieldBookGeologistExpand
        {
            get { return Preferences.Get(nameof(FieldBookGeologistExpand), true); }
            set { Preferences.Set(nameof(FieldBookGeologistExpand), value); }
        }

        public bool FieldBookProjectExpand
        {
            get { return Preferences.Get(nameof(FieldBookProjectExpand), true); }
            set { Preferences.Set(nameof(FieldBookProjectExpand), value); }
        }

        public bool FieldBookOtherExpand
        {
            get { return Preferences.Get(nameof(FieldBookOtherExpand), true); }
            set { Preferences.Set(nameof(FieldBookOtherExpand), value); }
        }

        public bool FieldBooksGenericExpand
        {
            get { return Preferences.Get(nameof(FieldBooksGenericExpand), true); }
            set { Preferences.Set(nameof(FieldBooksGenericExpand), value); }
        }

        #endregion

        public FieldBookViewModel()
        {
            //Make sure a database exist before moving on
            _ = ValidateDabaseExistence();
        }

        /// <summary>
        /// Will detect if default geopackage file already exists, else it will
        /// create it from the embedded resource. If it does exist, a check 
        /// will be performed wether or not it matches latest database version.
        /// If not, it'll be recreated from embedded resource.
        /// </summary>
        /// <returns></returns>
        private async Task ValidateDabaseExistence()
        {
            bool validates = false;

            //Detect default database else create from resource
            if (!File.Exists(da.DatabaseFilePath))
            {
                validates = await da.CreateDatabaseFromResource(da.DatabaseFilePath);
            }
            else
            {
                //Validate version
                da.PreferedDatabasePath = da.DatabaseFilePath;
                double dbVersion = await da.GetDBVersion();
                if (dbVersion != DatabaseLiterals.DBVersion)
                {
                    //Start an upgrade process
                    FieldBooksViewModel fieldBooksViewModel = new FieldBooksViewModel();
                    bool updated = await fieldBooksViewModel.DoUpgradeFieldBook();
                    if (updated) 
                    {
                        validates = true;
                    }
                }
                else 
                {
                    validates = true;
                }
            }

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
                //Project name and database name update validation
                //If user has a databse, but renamed the project, we need to rename the database
#if WINDOWS
                string desiredDatabaseName = Path.Combine(FileSystem.Current.AppDataDirectory, Model.FieldBookFileName + DatabaseLiterals.DBTypeSqlite);
#elif ANDROID
                string desiredDatabaseName = Path.Combine(FileSystem.Current.AppDataDirectory, Model.FieldBookFileName + DatabaseLiterals.DBTypeSqliteDeprecated);
#else
                string desiredDatabaseName = Path.Combine(FileSystem.Current.AppDataDirectory, Model.FieldBookFileName + DatabaseLiterals.DBTypeSqlite);
#endif

                //Validate if new entry or update
                if (_model.MetaID > 0)
                {
                    if (!Path.Exists(desiredDatabaseName) && Path.Exists(da.PreferedDatabasePath))
                    {
                        //Rename database
                        FileInfo originalFileInfo = new FileInfo(da.PreferedDatabasePath);
                        originalFileInfo.MoveTo(desiredDatabaseName);

                        //Set prefered database
                        da.PreferedDatabasePath = desiredDatabaseName;
                    }

                    //Fill out missing values in model
                    SetModel();

                    await da.SaveItemAsync(Model, true);
                }
                else
                {
                    //Set database path
                    da.PreferedDatabasePath = desiredDatabaseName;

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
                await Shell.Current.GoToAsync($"//{nameof(FieldBooksPage)}/");
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
            await Shell.Current.GoToAsync($"//{nameof(FieldBooksPage)}");
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
                // Keep in pref project type for futur vocab use and other viewing purposes
                Preferences.Set(nameof(DatabaseLiterals.FieldUserInfoFWorkType), Model.FieldworkType);
            }

        }

        #endregion
    }
}
