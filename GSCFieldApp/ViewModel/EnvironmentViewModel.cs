using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GSCFieldApp.Models;
using GSCFieldApp.Services.DatabaseServices;
using GSCFieldApp.Controls;
using GSCFieldApp.Views;
using GSCFieldApp.Services;
using static GSCFieldApp.Dictionaries.DatabaseLiterals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using CommunityToolkit.Maui.Alerts;
using System.Security.Cryptography;
using System.Collections.ObjectModel;

namespace GSCFieldApp.ViewModel
{
    [QueryProperty(nameof(EnvironmentModel), nameof(EnvironmentModel))]
    [QueryProperty(nameof(Station), nameof(Station))]
    public partial class EnvironmentViewModel : FieldAppPageHelper
    {
        #region INIT

        //Database
        private EnvironmentModel _model = new EnvironmentModel();

        //UI
        private ComboBox _environmentRelief = new ComboBox();
        private ComboBox _environmentBoulder = new ComboBox();
        private ComboBox _environmentDrainage = new ComboBox();
        private ComboBox _environmentPermIndicator = new ComboBox();
        private ComboBox _environmentExposure = new ComboBox();
        private ComboBox _environmentPattern = new ComboBox();
        private ComboBox _environmentCover = new ComboBox();
        private ComboBox _environmentIce = new ComboBox();

        //UI Concatenated
        private ComboBoxItem _selectedEnvironmentPattern = new ComboBoxItem();
        private ObservableCollection<ComboBoxItem> _environmentPatternCollection = new ObservableCollection<ComboBoxItem>();

        #endregion

        #region PROPERTIES
        [ObservableProperty]
        private Station _station;

        [ObservableProperty]
        private EnvironmentModel _environmentModel;

        public EnvironmentModel Model { get { return _model; } set { _model = value; } }

        public bool EnvironmentLandscapeVisibility
        {
            get { return Preferences.Get(nameof(EnvironmentLandscapeVisibility), true); }
            set { Preferences.Set(nameof(EnvironmentLandscapeVisibility), value); }
        }

        public bool EnvironmentTerrainVisibility
        {
            get { return Preferences.Get(nameof(EnvironmentTerrainVisibility), true); }
            set { Preferences.Set(nameof(EnvironmentTerrainVisibility), value); }
        }

        public bool EnvironmentGroundVisibility
        {
            get { return Preferences.Get(nameof(EnvironmentGroundVisibility), true); }
            set { Preferences.Set(nameof(EnvironmentGroundVisibility), value); }
        }

        public bool EnvironmentGeneralVisibility
        {
            get { return Preferences.Get(nameof(EnvironmentGeneralVisibility), true); }
            set { Preferences.Set(nameof(EnvironmentGeneralVisibility), value); }
        }

        public ComboBox EnvironmentRelief { get { return _environmentRelief; } set { _environmentRelief = value; } }
        public ComboBox EnvironmentBoulder { get { return _environmentBoulder; } set { _environmentBoulder = value; } }
        public ComboBox EnvironmentDrainage { get { return _environmentDrainage; } set { _environmentDrainage = value; } }
        public ComboBox EnvironmentPermIndicator { get { return _environmentPermIndicator; } set { _environmentPermIndicator = value; } }
        public ComboBox EnvironmentExposure { get { return _environmentExposure; } set { _environmentExposure = value; } }
        public ComboBox EnvironmentPattern { get { return _environmentPattern; } set { _environmentPattern = value; } }
        public ComboBox EnvironmentCover { get { return _environmentCover; } set { _environmentCover = value; } }
        public ComboBox EnvironmentIce { get { return _environmentIce; } set { _environmentIce = value; } }

        public ComboBoxItem SelectedEnvironmentPattern
        {
            get
            {
                return _selectedEnvironmentPattern;
            }
            set
            {
                if (_selectedEnvironmentPattern != value)
                {
                    if (_environmentPatternCollection != null)
                    {
                        if (_environmentPatternCollection.Count > 0 && _environmentPatternCollection[0] == null)
                        {
                            _environmentPatternCollection.RemoveAt(0);
                        }

                        if (value != null && value.itemName != string.Empty)
                        {
                            _environmentPatternCollection.Add(value);
                            _selectedEnvironmentPattern = value;
                            OnPropertyChanged(nameof(EnvironmentPatternCollection));
                        }
                    }


                }

            }
        }
        public ObservableCollection<ComboBoxItem> EnvironmentPatternCollection { get { return _environmentPatternCollection; } set { _environmentPatternCollection = value; } }


        #endregion

        #region RELAYS

        [RelayCommand]
        public async Task Back()
        {
            //Exit
            await NavigateToFieldNotes(TableNames.environment, false);
        }

        /// <summary>
        /// Save button command
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        async Task Save()
        {
            //Fill out missing values in model
            await SetModelAsync();

            //Validate if new entry or update
            if (_environmentModel != null && _environmentModel.EnvName != string.Empty && _model.EnvID != 0)
            {

                await da.SaveItemAsync(Model, true);
            }
            else
            {
                //New entry coming from parent form
                //Insert new record
                await da.SaveItemAsync(Model, false);
            }

            //Close to be sure
            await da.CloseConnectionAsync();

            //Exit or stay in map page if quick photo
            await NavigateToFieldNotes(TableNames.environment);
            
        }

        /// <summary>
        /// Save button command
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        async Task SaveStay()
        {
            //Fill out missing values in model
            await SetModelAsync();

            //Validate if new entry or update
            if (_environmentModel != null && _environmentModel.EnvName != string.Empty && _model.EnvID != 0)
            {

                await da.SaveItemAsync(Model, true);
            }
            else
            {
                //New entry coming from parent form
                //Insert new record
                await da.SaveItemAsync(Model, false);
            }

            //Close to be sure
            await da.CloseConnectionAsync();

            //Show saved message
            await Toast.Make(LocalizationResourceManager["ToastSaveRecord"].ToString()).Show(CancellationToken.None);

            //Reset
            await ResetModelAsync();
            OnPropertyChanged(nameof(Model));


        }

        [RelayCommand]
        async Task SaveDelete()
        {
            if (_model.EnvID != 0)
            {
                await commandServ.DeleteDatabaseItemCommand(TableNames.environment, _model.EnvName, _model.EnvID);
            }

            //Exit
            await NavigateToFieldNotes(TableNames.environment);

        }


        #endregion

        public EnvironmentViewModel()
        {
            FieldThemes = new FieldThemes();
        }

        #region METHODS

        /// <summary>
        /// Initialize all pickers. 
        /// To save loading time, process only those needed based on work type
        /// </summary>
        /// <returns></returns>
        public async Task FillPickers()
        {
            //Connect to db
            currentConnection = da.GetConnectionFromPath(da.PreferedDatabasePath);

            //First order pickers
            _environmentRelief = await FillAPicker(FieldEnvRelief);
            OnPropertyChanged(nameof(EnvironmentRelief));

            _environmentBoulder = await FillAPicker(FieldEnvBoulder);
            OnPropertyChanged(nameof(EnvironmentBoulder));

            _environmentDrainage = await FillAPicker(FieldEnvDrainage);
            OnPropertyChanged(nameof(EnvironmentDrainage));

            _environmentPermIndicator = await FillAPicker(FieldEnvPermIndicator);
            OnPropertyChanged(nameof(EnvironmentPermIndicator));

            _environmentExposure = await FillAPicker(FieldEnvExposure);
            OnPropertyChanged(nameof(EnvironmentExposure));

            _environmentPattern = await FillAPicker(FieldEnvGroundPattern);
            OnPropertyChanged(nameof(EnvironmentPattern));

            _environmentCover = await FillAPicker(FieldEnvGroundCover);
            OnPropertyChanged(nameof(EnvironmentCover));

            _environmentIce = await FillAPicker(FieldEnvGroundIce);
            OnPropertyChanged(nameof(EnvironmentIce));

            await currentConnection.CloseAsync();
        }

        /// <summary>
        /// Will fill the project type combobox
        /// </summary>
        private async Task<ComboBox> FillAPicker(string fieldName, string extraField = "")
        {
            //Make sure to user default database rather then the prefered one. This one will always be there.
            return await da.GetComboboxListWithVocabAsync(TableEnvironment, fieldName, extraField);

        }

        /// <summary>
        /// Will initialize the model with needed calculated fields
        /// </summary>
        /// <returns></returns>
        public async Task InitModel()
        {
            if (Model != null && Model.EnvID == 0 && _station != null)
            {
                //Get current application version
                Model.EnvStationID = _station.StationID;
                Model.EnvName = await idCalculator.CalculateEnvironmentAliasAsync(_station.StationID, _station.StationAlias);
                OnPropertyChanged(nameof(Model));

            }
        }

        /// <summary>
        /// Will refill the form with existing values for update/editing purposes
        /// </summary>
        /// <returns></returns>
        public async Task Load()
        {

            if (_environmentModel != null && _environmentModel.EnvName != string.Empty)
            {
                //Set model like actual record
                _model = _environmentModel;

                //Refresh
                OnPropertyChanged(nameof(Model));

                //Piped value field
                List<string> patterns = ConcatenatedCombobox.UnpipeString(_environmentModel.EnvGroundPattern);
                _environmentPatternCollection.Clear(); //Clear any possible values first
                foreach (ComboBoxItem cbox in EnvironmentPattern.cboxItems)
                {
                    if (patterns.Contains(cbox.itemValue) && !_environmentPatternCollection.Contains(cbox))
                    {
                        _environmentPatternCollection.Add(cbox);
                    }
                }
                OnPropertyChanged(nameof(EnvironmentPatternCollection));

            }
        }

        /// <summary>
        /// Will reset model fields to default just like it's a new record
        /// </summary>
        /// <returns></returns>
        private async Task ResetModelAsync()
        {

            //Reset model
            if (_station != null)
            {
                // if coming from station notes, calculate new alias
                Model.EnvStationID = _station.StationID;
                Model.EnvName = await idCalculator.CalculateEnvironmentAliasAsync(_station.StationID, _station.ParentName);
            }
            else if (Model.EnvStationID != null)
            {
                // if coming from field notes on a record edit that needs to be saved as a new record with stay/save
                SQLiteAsyncConnection currentConnection = da.GetConnectionFromPath(da.PreferedDatabasePath);
                List<Station> parentAlias = await currentConnection.Table<Station>().Where(e => e.StationID == Model.EnvStationID).ToListAsync();
                await currentConnection.CloseAsync();
                Model.EnvName = await idCalculator.CalculateEnvironmentAliasAsync(Model.EnvStationID, parentAlias.First().StationAlias);
            }

            Model.EnvID = 0;

        }

        /// <summary>
        /// Will fill out missing fields for model. Default auto-calculated values
        /// </summary>
        private async Task SetModelAsync()
        {

            //Process concatenated pickers
            if (EnvironmentPattern.cboxDefaultItemIndex != -1 && EnvironmentPatternCollection != null && EnvironmentPattern.cboxItems.Count > 0)
            {
                Model.EnvGroundPattern = ConcatenatedCombobox.PipeValues(EnvironmentPatternCollection); //process list of values so they are concatenated.
            }

        }

        #endregion
    }
}
