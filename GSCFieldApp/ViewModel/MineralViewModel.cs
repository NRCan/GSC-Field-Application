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
    [QueryProperty(nameof(Mineral), nameof(Mineral))]
    [QueryProperty(nameof(Earthmaterial), nameof(Earthmaterial))]
    [QueryProperty(nameof(MineralAlteration), nameof(MineralAlteration))]
    public partial class MineralViewModel: FieldAppPageHelper
    {
        #region INIT

        //Database
        DataAccess da = new DataAccess();
        SQLiteAsyncConnection currentConnection;
        public DataIDCalculation idCalculator = new DataIDCalculation();

        private Mineral _model = new Mineral();

        //UI
        private ComboBox _mineralMode = new ComboBox();
        private ComboBox _mineralColour = new ComboBox();
        private ComboBox _mineralOccurence = new ComboBox();
        private ComboBox _mineralFormHabit = new ComboBox();

        private ComboBoxItem _selectedMineralFormHabit = new ComboBoxItem();

        private ObservableCollection<ComboBoxItem> _mineralFormHabitCollection = new ObservableCollection<ComboBoxItem>();

        //Localize
        public LocalizationResourceManager LocalizationResourceManager
        => LocalizationResourceManager.Instance; // Will be used for in code dynamic local strings

        //Services
        public CommandService commandServ = new CommandService();

        #endregion

        #region PROPERTIES

        [ObservableProperty]
        private Earthmaterial _earthmaterial;

        [ObservableProperty]
        private MineralAlteration _mineralAlteration;

        [ObservableProperty]
        private Mineral _mineral;

        public FieldThemes FieldThemes { get; set; } //Enable/Disable certain controls based on work type

        public Mineral Model { get { return _model; } set { _model = value; } }

        public bool MineralTypeVisibility
        {
            get { return Preferences.Get(nameof(MineralTypeVisibility), true); }
            set { Preferences.Set(nameof(MineralTypeVisibility), value); }
        }

        public bool MineralDescVisibility
        {
            get { return Preferences.Get(nameof(MineralDescVisibility), true); }
            set { Preferences.Set(nameof(MineralDescVisibility), value); }
        }

        public bool MineralGeneralVisibility
        {
            get { return Preferences.Get(nameof(MineralGeneralVisibility), true); }
            set { Preferences.Set(nameof(MineralGeneralVisibility), value); }
        }

        public ComboBox MineralMode { get { return _mineralMode; } set { _mineralMode = value; } }
        public ComboBox MineralColour { get { return _mineralColour; } set { _mineralColour = value; } }
        public ComboBox MineralOccurence { get { return _mineralOccurence; } set { _mineralOccurence = value; } }
        public ComboBox MineralFormHabit { get { return _mineralFormHabit; } set { _mineralFormHabit = value; } }

        public ComboBoxItem SelectedMineralFormHabit
        {
            get
            {
                return _selectedMineralFormHabit;
            }
            set
            {
                if (_selectedMineralFormHabit != value)
                {
                    if (_mineralFormHabitCollection != null)
                    {
                        if (_mineralFormHabitCollection.Count > 0 && _mineralFormHabitCollection[0] == null)
                        {
                            _mineralFormHabitCollection.RemoveAt(0);
                        }
                        if (value != null && value.itemName != string.Empty)
                        {
                            _mineralFormHabitCollection.Add(value);
                            _selectedMineralFormHabit = value;
                            OnPropertyChanged(nameof(MineralFormHabitCollection));
                        }

                    }


                }

            }
        }
        public ObservableCollection<ComboBoxItem> MineralFormHabitCollection { get { return _mineralFormHabitCollection; } set { _mineralFormHabitCollection = value; OnPropertyChanged(nameof(MineralFormHabitCollection)); } }

        #endregion

        public MineralViewModel()
        {
            //Init new field theme
            FieldThemes = new FieldThemes();
        }

        #region RELAYS

        [RelayCommand]
        public async Task Back()
        {

            //Exit 
            await NavigateToFieldNotes(TableNames.mineral);
            
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
            if (_mineral != null && _mineral.MineralIDName != string.Empty && _model.MineralID != 0)
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

            //Exit
             await NavigateToFieldNotes(TableNames.mineral);
            
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
            if (_mineral != null && _mineral.MineralIDName != string.Empty && _model.MineralID != 0)
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
            if (_model.MineralID != 0)
            {
                if (_model.MineralEMID != null)
                {
                    await commandServ.DeleteDatabaseItemCommand(TableNames.mineral, _model.MineralIDName, _model.MineralEMID.Value);
                }
                else
                {
                    await commandServ.DeleteDatabaseItemCommand(TableNames.mineral, _model.MineralIDName, _model.MineralMAID.Value);
                }
                
            }

            //Exit
            await NavigateToFieldNotes(TableNames.mineral);

        }


        #endregion

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
            _mineralMode = await FillAPicker(FieldMineralMode);
            OnPropertyChanged(nameof(MineralMode));

            _mineralColour = await FillAPicker(FieldMineralColour);
            OnPropertyChanged(nameof(MineralColour));

            _mineralOccurence = await FillAPicker(FieldMineralOccurence);
            OnPropertyChanged(nameof(MineralOccurence));

            _mineralFormHabit = await FillAPicker(FieldMineralFormHabit);
            OnPropertyChanged(nameof(MineralFormHabit));

            //Second order pickers


            await currentConnection.CloseAsync();
        }


        /// <summary>
        /// Will fill the project type combobox
        /// </summary>
        private async Task<ComboBox> FillAPicker(string fieldName, string extraField = "")
        {
            //Make sure to user default database rather then the prefered one. This one will always be there.
            return await da.GetComboboxListWithVocabAsync(TableMineral, fieldName, extraField);

        }

        /// <summary>
        /// Will fill out missing fields for model. Default auto-calculated values
        /// Done before actually saving
        /// </summary>
        private async Task SetModelAsync()
        {
            if (MineralFormHabitCollection.Count > 0)
            {
                Model.MineralFormHabit = ConcatenatedCombobox.PipeValues(MineralFormHabitCollection); //process list of values so they are concatenated.
            }
        }

        /// <summary>
        /// Will initialize the model with needed calculated fields
        /// </summary>
        /// <returns></returns>
        public async Task InitModel()
        {
            if (Model != null && Model.MineralID == 0 && _earthmaterial != null)
            {
                //Get current application version
                Model.MineralEMID = _earthmaterial.EarthMatID;
                Model.MineralIDName = await idCalculator.CalculateMineralAlias(_earthmaterial.EarthMatID, _earthmaterial.EarthMatName);
                OnPropertyChanged(nameof(Model));

            }

            if (Model != null && Model.MineralID == 0 && _mineralAlteration != null)
            {
                //Get current application version
                Model.MineralMAID = _mineralAlteration.MAID;
                Model.MineralIDName = await idCalculator.CalculateMineralAlias(_mineralAlteration.MAID, _mineralAlteration.MAName);
                OnPropertyChanged(nameof(Model));

            }
        }

        /// <summary>
        /// Will refill the form with existing values for update/editing purposes
        /// </summary>
        /// <returns></returns>
        public async Task Load()
        {

            if (_mineral != null && _mineral.MineralIDName != string.Empty)
            {
                //Set model like actual record
                _model = _mineral;

                List<string> bts = ConcatenatedCombobox.UnpipeString(_mineral.MineralFormHabit);
                _mineralFormHabitCollection.Clear(); //Clear any possible values first
                foreach (ComboBoxItem cbox in MineralFormHabit.cboxItems)
                {
                    if (bts.Contains(cbox.itemValue) && !_mineralFormHabitCollection.Contains(cbox))
                    {
                        _mineralFormHabitCollection.Add(cbox);
                    }
                }
                OnPropertyChanged(nameof(MineralFormHabitCollection));

                //Refresh
                OnPropertyChanged(nameof(Model));


            }

        }


        /// <summary>
        /// Will reset model fields to default just like it's a new record
        /// </summary>
        /// <returns></returns>
        private async Task ResetModelAsync()
        {

            //Reset model
            if (_earthmaterial != null)
            {
                // if coming from em notes, calculate new alias
                Model.MineralEMID = _earthmaterial.EarthMatID;
                Model.MineralName = await idCalculator.CalculateMineralAlias(_earthmaterial.EarthMatID, _earthmaterial.EarthMatName);
            }
            else if (_mineralAlteration != null)
            {
                // if coming from ma notes, calculate new alias
                Model.MineralMAID = _mineralAlteration.MAID;
                Model.MineralName = await idCalculator.CalculateMineralAlias(_mineralAlteration.MAID, _mineralAlteration.MAName);
            }
            else if (Model.MineralEMID != null)
            {
                // if coming from field notes on a record edit that needs to be saved as a new record with stay/save
                SQLiteAsyncConnection currentConnection = da.GetConnectionFromPath(da.PreferedDatabasePath);
                List<Earthmaterial> parentAlias = await currentConnection.Table<Earthmaterial>().Where(e => e.EarthMatID == Model.MineralEMID).ToListAsync();
                await currentConnection.CloseAsync();
                Model.MineralIDName = await idCalculator.CalculateMineralAlias(Model.MineralEMID.Value, parentAlias.First().EarthMatName);
            }
            else if (Model.MineralMAID != null)
            {
                // if coming from field notes on a record edit that needs to be saved as a new record with stay/save
                SQLiteAsyncConnection currentConnection = da.GetConnectionFromPath(da.PreferedDatabasePath);
                List<MineralAlteration> parentAlias = await currentConnection.Table<MineralAlteration>().Where(e => e.MAID == Model.MineralMAID).ToListAsync();
                await currentConnection.CloseAsync();
                Model.MineralIDName = await idCalculator.CalculateMineralAlias(Model.MineralMAID.Value, parentAlias.First().MAName);
            }
            Model.MineralID = 0;

        }


        #endregion

    }
}
