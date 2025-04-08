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

        private Mineral _model = new Mineral();

        //UI
        private ComboBox _mineralMode = new ComboBox();
        private ComboBox _mineralColour = new ComboBox();
        private ComboBox _mineralOccurence = new ComboBox();
        private ComboBox _mineralFormHabit = new ComboBox();

        private ComboBoxItem _selectedMineralFormHabit = new ComboBoxItem();

        private ObservableCollection<ComboBoxItem> _mineralFormHabitCollection = new ObservableCollection<ComboBoxItem>();

        //private bool _isMineralListVisible = false;
        private List<string> _mineralPageNameSearchResults = new List<string>();
        private ComboBox _mineralNames = new ComboBox();

        #endregion

        #region PROPERTIES

        [ObservableProperty]
        private Earthmaterial _earthmaterial;

        [ObservableProperty]
        private MineralAlteration _mineralAlteration;

        [ObservableProperty]
        private Mineral _mineral;

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

        //public bool IsMineralListVisible { get { return _isMineralListVisible; } set { _isMineralListVisible = value; } }
        public List<string> MineralPageNameSearchResults { get { return _mineralPageNameSearchResults; } set { _mineralPageNameSearchResults = value; } }
        public ComboBox MineralNames { get { return _mineralNames; } set { _mineralNames = value; } }

        #endregion

        public MineralViewModel()
        {
            //Init new field theme
            FieldThemes = new FieldThemes();

            //Fill search bar
            _ = FillSearchListAsync();
        }

        #region RELAYS

        [RelayCommand]
        public async Task Back()
        {
            await Shell.Current.GoToAsync("..");
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

                await commandServ.DeleteDatabaseItemCommand(TableNames.mineral, _model.MineralIDName, _model.MineralID);
       
            }

            //Exit
            await NavigateToFieldNotes(TableNames.mineral);

        }

        /// <summary>
        /// Special command to filter down all mineral names
        /// </summary>
        /// <param name="searchText"></param>
        /// <returns></returns>
        [RelayCommand]
        public async Task PerformNameSearch(string searchText)
        {

            var search_term = searchText.ToLower();
            var results = _mineralNames.cboxItems.Where(i => i.itemName.ToLower().Contains(search_term)).ToList();

            if (results.Count > 0)
            {
                _mineralPageNameSearchResults = new List<string>();
                foreach (ComboBoxItem tmp in results)
                {
                    if (!_mineralPageNameSearchResults.Contains(tmp.itemName.ToString()))
                    {
                        _mineralPageNameSearchResults.Add(tmp.itemName.ToString());
                    }
                }

                MineralPageNameSearchResults = _mineralPageNameSearchResults;
            }

            OnPropertyChanged(nameof(MineralPageNameSearchResults));
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
            //First order pickers
            _mineralMode = await FillAPicker(FieldMineralMode);
            OnPropertyChanged(nameof(MineralMode));

            _mineralColour = await FillAPicker(FieldMineralColour);
            OnPropertyChanged(nameof(MineralColour));

            _mineralOccurence = await FillAPicker(FieldMineralOccurence);
            OnPropertyChanged(nameof(MineralOccurence));

            _mineralFormHabit = await FillAPicker(FieldMineralFormHabit);
            OnPropertyChanged(nameof(MineralFormHabit));

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
                Model.MineralIDName = await idCalculator.CalculateMineralAlias(_earthmaterial.EarthMatID, _earthmaterial.EarthMatName, TableNames.earthmat);
                OnPropertyChanged(nameof(Model));

            }

            if (Model != null && Model.MineralID == 0 && _mineralAlteration != null)
            {
                //Get current application version
                Model.MineralMAID = _mineralAlteration.MAID;
                Model.MineralIDName = await idCalculator.CalculateMineralAlias(_mineralAlteration.MAID, _mineralAlteration.MAName, TableNames.mineralization);
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
                Model.MineralIDName = await idCalculator.CalculateMineralAlias(_earthmaterial.EarthMatID, _earthmaterial.EarthMatName, TableNames.earthmat);
            }
            else if (_mineralAlteration != null)
            {
                // if coming from ma notes, calculate new alias
                Model.MineralMAID = _mineralAlteration.MAID;
                Model.MineralIDName = await idCalculator.CalculateMineralAlias(_mineralAlteration.MAID, _mineralAlteration.MAName, TableNames.mineralization);
            }
            else if (Model.MineralEMID != null)
            {
                // if coming from field notes on a record edit that needs to be saved as a new record with stay/save
                List<Earthmaterial> parentAlias = await DataAccess.DbConnection.Table<Earthmaterial>().Where(e => e.EarthMatID == Model.MineralEMID).ToListAsync();
                Model.MineralIDName = await idCalculator.CalculateMineralAlias(Model.MineralEMID.Value, parentAlias.First().EarthMatName, TableNames.earthmat);
            }
            else if (Model.MineralMAID != null)
            {
                // if coming from field notes on a record edit that needs to be saved as a new record with stay/save
                List<MineralAlteration> parentAlias = await DataAccess.DbConnection.Table<MineralAlteration>().Where(e => e.MAID == Model.MineralMAID).ToListAsync();
                Model.MineralIDName = await idCalculator.CalculateMineralAlias(Model.MineralMAID.Value, parentAlias.First().MAName, TableNames.mineralization);
            }
            Model.MineralID = 0;

        }


        /// <summary>
        /// Will initialize some preset list of lithologies for type/group and details
        /// </summary>
        /// <returns></returns>
        private async Task FillSearchListAsync()
        {
            _mineralNames = await FillAPicker(FieldMineral);
        }

        #endregion

    }
}
