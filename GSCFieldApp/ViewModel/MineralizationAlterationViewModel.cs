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
    [QueryProperty(nameof(MineralAlteration), nameof(MineralAlteration))]
    [QueryProperty(nameof(Earthmaterial), nameof(Earthmaterial))]
    [QueryProperty(nameof(Station), nameof(Station))]
    public partial class MineralizationAlterationViewModel: FieldAppPageHelper
    {
        #region INIT
        //Model
        private MineralAlteration _model = new MineralAlteration();

        private ComboBox _mineralizationAlteration = new ComboBox();
        private ComboBox _mineralizationAlterationUnit = new ComboBox();
        private ComboBox _mineralizationAlterationDistribution = new ComboBox();
        private ComboBox _mineralizationAlterationPhase = new ComboBox();
        private ComboBox _mineralizationAlterationTexture = new ComboBox();
        private ComboBox _mineralizationAlterationFacies = new ComboBox();
        private ComboBoxItem _selectedMineralizationAlterationDistribution = new ComboBoxItem();
        private ObservableCollection<ComboBoxItem> _mineralizationAlterationDistributionCollection = new ObservableCollection<ComboBoxItem>();

        #endregion

        #region PROPERTIES
        [ObservableProperty]
        private Earthmaterial _earthmaterial;

        [ObservableProperty]
        private Station _station;

        [ObservableProperty]
        private MineralAlteration _mineralAlteration;

        public MineralAlteration Model { get { return _model; } set { _model = value; } }

        public bool MADescVisibility
        {
            get { return Preferences.Get(nameof(MADescVisibility), true); }
            set { Preferences.Set(nameof(MADescVisibility), value); }
        }
        public bool MATypeVisibility
        {
            get { return Preferences.Get(nameof(MATypeVisibility), true); }
            set { Preferences.Set(nameof(MATypeVisibility), value); }
        }
        public ComboBox MineralizationAlteration { get { return _mineralizationAlteration; } set { _mineralizationAlteration = value; } }
        public ComboBox MineralizationAlterationUnit { get { return _mineralizationAlterationUnit; } set { _mineralizationAlterationUnit = value; } }
        public ComboBox MineralizationAlterationDistribution { get { return _mineralizationAlterationDistribution; } set { _mineralizationAlterationDistribution = value; } }

        public ComboBox MineralizationAlterationPhase { get { return _mineralizationAlterationPhase; } set { _mineralizationAlterationPhase = value; } }
        public ComboBox MineralizationAlterationTexture { get { return _mineralizationAlterationTexture; } set { _mineralizationAlterationTexture = value; } }
        public ComboBox MineralizationAlterationFacies { get { return _mineralizationAlterationFacies; } set { _mineralizationAlterationFacies = value; } }

        public ComboBoxItem SelectedMineralizationAlterationDistribution
        {
            get
            {
                return _selectedMineralizationAlterationDistribution;
            }
            set
            {
                if (_selectedMineralizationAlterationDistribution != value)
                {
                    if (_mineralizationAlterationDistributionCollection != null)
                    {
                        if (_mineralizationAlterationDistributionCollection.Count > 0 && _mineralizationAlterationDistributionCollection[0] == null)
                        {
                            _mineralizationAlterationDistributionCollection.RemoveAt(0);
                        }
                        if (value != null && value.itemName != string.Empty)
                        {
                            _mineralizationAlterationDistributionCollection.Add(value);
                            _selectedMineralizationAlterationDistribution = value;
                            OnPropertyChanged(nameof(MineralizationAlterationDistributionCollection));
                        }

                    }


                }

            }
        }
        public ObservableCollection<ComboBoxItem> MineralizationAlterationDistributionCollection { get { return _mineralizationAlterationDistributionCollection; } set { _mineralizationAlterationDistributionCollection = value; OnPropertyChanged(nameof(MineralizationAlterationDistributionCollection)); } }

        #endregion

        #region RELAYS
        [RelayCommand]
        public async Task Back()
        {
            //Exit 
            await NavigateToFieldNotes(TableNames.mineralization);

        }

        /// <summary>
        /// Save button command
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        async Task Save()
        {
            //Save
            await SetAndSaveModelAsync();

            //Exit
            await NavigateToFieldNotes(TableNames.mineralization);
        }

        /// <summary>
        /// Save button command
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        async Task SaveStay()
        {
            //Save
            await SetAndSaveModelAsync();

            //Show saved message
            await Toast.Make(LocalizationResourceManager["ToastSaveRecord"].ToString()).Show(CancellationToken.None);

            //Reset
            await ResetModelAsync();
            OnPropertyChanged(nameof(Model));


        }

        [RelayCommand]
        async Task SaveDelete()
        {
            if (_model.MAID != 0)
            {
                await commandServ.DeleteDatabaseItemCommand(TableNames.mineralization, _model.MAName, _model.MAID);
            }

            //Exit
            await NavigateToFieldNotes(TableNames.mineralization);

        }

        [RelayCommand]
        public async Task AddMineral()
        {
            //Save
            await SetAndSaveModelAsync();

            //Navigate to pflow page 
            await Shell.Current.GoToAsync($"/{nameof(MineralPage)}/",
                new Dictionary<string, object>
                {
                    [nameof(MineralPage)] = null,
                    [nameof(Earthmaterial)] = null,
                    [nameof(MineralAlteration)] = Model,
                }
            );
        }

        #endregion

        public MineralizationAlterationViewModel() { }

        #region METHODS

        public async Task SetAndSaveModelAsync()
        {
            //Fill out missing values in model
            await SetModelAsync();

            //Validate if new entry or update
            if (_mineralAlteration != null && _mineralAlteration.MAName != string.Empty && _model.MAID != 0)
            {
                await da.SaveItemAsync(Model, true);
            }
            else
            {
                //Insert new record
                await da.SaveItemAsync(Model, false);
            }

            //Close to be sure
            await da.CloseConnectionAsync();
        }

        /// <summary>
        /// Will fill out missing fields for model. Default auto-calculated values
        /// Done before actually saving
        /// </summary>
        private async Task SetModelAsync()
        {

            #region Process pickers

            if (MineralizationAlterationDistributionCollection.Count > 0)
            {
                Model.MADistribute = ConcatenatedCombobox.PipeValues(MineralizationAlterationDistributionCollection); //process list of values so they are concatenated.
            }

            #endregion

            //Process foreign keys
            if (Model.MAEarthmatID != null)
            {
                //Force null in station id
                Model.MAStationID = null;
            }
            else if (Model.MAStationID != null)
            {
                //Force null in earthmat id
                Model.MAEarthmatID = null;
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
                Model.MAEarthmatID = _earthmaterial.EarthMatID;
                Model.MAName = await idCalculator.CalculateMineralAlterationAliasAsync(_earthmaterial.EarthMatID, _earthmaterial.EarthMatName);
            }
            else if (_station != null)
            {
                // if coming from ma notes, calculate new alias
                Model.MAStationID = _station.StationID;
                Model.MAName = await idCalculator.CalculateMineralAlterationAliasAsync(_station.StationID, _station.StationAlias);
            }
            else if (Model.MAEarthmatID != null)
            {
                // if coming from field notes on a record edit that needs to be saved as a new record with stay/save
                SQLiteAsyncConnection currentConnection = da.GetConnectionFromPath(da.PreferedDatabasePath);
                List<Earthmaterial> parentAlias = await currentConnection.Table<Earthmaterial>().Where(e => e.EarthMatID == Model.MAEarthmatID).ToListAsync();
                await currentConnection.CloseAsync();
                Model.MAName = await idCalculator.CalculateMineralAlterationAliasAsync(Model.MAEarthmatID.Value, parentAlias.First().EarthMatName);
            }
            else if (Model.MAStationID != null)
            {
                // if coming from field notes on a record edit that needs to be saved as a new record with stay/save
                SQLiteAsyncConnection currentConnection = da.GetConnectionFromPath(da.PreferedDatabasePath);
                List<Station> parentAlias = await currentConnection.Table<Station>().Where(e => e.StationID == Model.MAStationID).ToListAsync();
                await currentConnection.CloseAsync();
                Model.MAName = await idCalculator.CalculateMineralAlterationAliasAsync(Model.MAStationID.Value, parentAlias.First().StationAlias);
            }
            Model.MAID = 0;

        }

        /// <summary>
        /// Will refill the form with existing values for update/editing purposes
        /// </summary>
        /// <returns></returns>
        public async Task Load()
        {
            if (_mineralAlteration != null && _mineralAlteration.MAName != string.Empty)
            {
                //Set model like actual record
                _model = _mineralAlteration;

                //Refresh
                OnPropertyChanged(nameof(Model));

                #region Pickers


                List<string> maDis = ConcatenatedCombobox.UnpipeString(_mineralAlteration.MADistribute);
                _mineralizationAlterationDistributionCollection.Clear(); //Clear any possible values first
                foreach (ComboBoxItem cbox in MineralizationAlterationDistribution.cboxItems)
                {
                    if (maDis.Contains(cbox.itemValue) && !_mineralizationAlterationDistributionCollection.Contains(cbox))
                    {
                        _mineralizationAlterationDistributionCollection.Add(cbox);
                    }
                }
                OnPropertyChanged(nameof(MineralizationAlterationDistributionCollection));


                #endregion

            }
        }

        /// <summary>
        /// Will fill all picker controls
        /// TODO: make sure this whole thing doesn't slow too much form rendering
        /// </summary>
        /// <returns></returns>
        public async Task FillPickers()
        {

            _mineralizationAlteration = await FillAPicker(FieldMineralAlteration);
            OnPropertyChanged(nameof(MineralizationAlteration));

            _mineralizationAlterationUnit = await FillAPicker(FieldMineralAlterationUnit);
            OnPropertyChanged(nameof(MineralizationAlterationUnit));

            _mineralizationAlterationDistribution = await FillAPicker(FieldMineralAlterationDistrubute);
            OnPropertyChanged(nameof(MineralizationAlterationDistribution));

            _mineralizationAlterationPhase = await FillAPicker(FieldMineralAlterationPhase);
            OnPropertyChanged(nameof(MineralizationAlterationPhase));

            _mineralizationAlterationTexture = await FillAPicker(FieldMineralAlterationTexture);
            OnPropertyChanged(nameof(MineralizationAlterationTexture));

            _mineralizationAlterationFacies = await FillAPicker(FieldMineralAlterationFacies);
            OnPropertyChanged(nameof(MineralizationAlterationFacies));

        }


        /// <summary>
        /// Generic method to fill a needed picker control with vocabulary
        /// </summary>
        private async Task<ComboBox> FillAPicker(string fieldName, string extraField = "")
        {
            //Make sure to user default database rather then the prefered one. This one will always be there.
            return await da.GetComboboxListWithVocabAsync(TableMineralAlteration, fieldName, extraField);

        }

        /// <summary>
        /// Will initialize the model with needed calculated fields
        /// </summary>
        /// <returns></returns>
        public async Task InitModel()
        {
            if (Model != null && Model.MAID == 0 && _earthmaterial != null)
            {
                Model.MAEarthmatID = _earthmaterial.EarthMatID;
                Model.MAName = await idCalculator.CalculateMineralAlterationAliasAsync(_earthmaterial.EarthMatID, _earthmaterial.EarthMatName);
                OnPropertyChanged(nameof(Model));

            }

            if (Model != null && Model.MAID == 0 && _station != null)
            {
                Model.MAStationID = _station.StationID;
                Model.MAName = await idCalculator.CalculateMineralAlterationAliasAsync(_station.StationID, _station.StationAlias);
                OnPropertyChanged(nameof(Model));

            }
        }

        #endregion
    }
}
