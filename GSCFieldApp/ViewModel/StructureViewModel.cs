using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GSCFieldApp.Models;
using GSCFieldApp.Services.DatabaseServices;
using GSCFieldApp.Themes;
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

namespace GSCFieldApp.ViewModel
{
    [QueryProperty(nameof(Structure), nameof(Structure))]
    [QueryProperty(nameof(Earthmaterial), nameof(Earthmaterial))]
    public partial class StructureViewModel: FieldAppPageHelper
    {
        #region INIT

        //Database
        DataAccess da = new DataAccess();
        SQLiteAsyncConnection currentConnection;
        public DataIDCalculation idCalculator = new DataIDCalculation();

        private Structure _model = new Structure();

        //UI
        private ComboBox _structureRelatedAlias = new ComboBox();
        private ComboBox _structureFormat = new ComboBox();
        private ComboBox _structureMethod = new ComboBox();
        private ComboBox _structureFlattening = new ComboBox();
        private ComboBox _structureStrain = new ComboBox();
        private ComboBox _structureAttitude = new ComboBox();
        private ComboBox _structureYounging = new ComboBox();
        private ComboBox _structureGeneration = new ComboBox();
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
        private Structure _structure;

        public FieldThemes FieldThemes { get; set; } //Enable/Disable certain controls based on work type

        public Structure Model { get { return _model; } set { _model = value; } }

        public bool StructureTypeVisibility
        {
            get { return Preferences.Get(nameof(StructureTypeVisibility), true); }
            set { Preferences.Set(nameof(StructureTypeVisibility), value); }
        }

        public bool StructureMeasurementVisibility
        {
            get { return Preferences.Get(nameof(StructureMeasurementVisibility), true); }
            set { Preferences.Set(nameof(StructureMeasurementVisibility), value); }
        }

        public bool StructureDescVisibility
        {
            get { return Preferences.Get(nameof(StructureDescVisibility), true); }
            set { Preferences.Set(nameof(StructureDescVisibility), value); }
        }

        public bool StructureNotesVisibility
        {
            get { return Preferences.Get(nameof(StructureNotesVisibility), true); }
            set { Preferences.Set(nameof(StructureNotesVisibility), value); }
        }

        public ComboBox StructureRelatedAlias { get { return _structureRelatedAlias; } set { _structureRelatedAlias = value; } }
        public ComboBox StructureFormat { get { return _structureFormat; } set { _structureFormat = value; } }
        public ComboBox StructureMethod { get { return _structureMethod; } set { _structureMethod = value; } }
        public ComboBox StructureStrain { get { return _structureStrain; } set { _structureStrain = value; } }
        public ComboBox StructureFlattening { get { return _structureFlattening; } set { _structureFlattening = value; } }
        public ComboBox StructureAttitude { get { return _structureAttitude; } set { _structureAttitude = value; } }
        public ComboBox StructureYounging { get { return _structureYounging; } set { _structureYounging = value; } }
        public ComboBox StructureGeneration { get { return _structureGeneration; } set { _structureGeneration = value; } }
        #endregion

        public StructureViewModel()
        {
            //Init new field theme
            FieldThemes = new FieldThemes();
        }

        #region RELAYS

        [RelayCommand]
        public async Task Back()
        {
            //Make sure to delete station and location records if user is coming from map page
            if (_earthmaterial != null && _earthmaterial.IsMapPageQuick)
            {
                //Get parent record
                SQLiteAsyncConnection currentConnection = da.GetConnectionFromPath(da.PreferedDatabasePath);
                Station sRecord = await currentConnection.Table<Station>().Where(s => s.StationID == _earthmaterial.EarthMatStatID).FirstAsync();

                //Delete without forced pop-up warning and question
                await commandServ.DeleteDatabaseItemCommand(TableNames.location, sRecord.StationAlias, sRecord.LocationID, true);

            }

            //Exit or stay in map page if quick photo
            if (_earthmaterial != null && _earthmaterial.IsMapPageQuick)
            {
                await Shell.Current.GoToAsync("../");
            }
            else
            {
                await NavigateToFieldNotes(TableNames.structure);
            }
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
            if (_structure != null && _structure.StructureName != string.Empty && _model.StructureID != 0)
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
            if (_earthmaterial != null && _earthmaterial.IsMapPageQuick)
            {
                await Shell.Current.GoToAsync("../");
            }
            else
            {
                await NavigateToFieldNotes(TableNames.structure);
            }
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
            if (_structure != null && _structure.StructureName != string.Empty && _model.StructureID != 0)
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
            if (_model.StructureID != 0)
            {
                await commandServ.DeleteDatabaseItemCommand(TableNames.structure, _model.StructureName, _model.StructureEarthmatID);
            }

            //Exit
            await NavigateToFieldNotes(TableNames.structure);

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
            _structureFormat = await FillAPicker(FieldStructureFormat);
            OnPropertyChanged(nameof(StructureFormat));

            _structureMethod = await FillAPicker(FieldStructureMethod);
            OnPropertyChanged(nameof(StructureMethod));

            _structureMethod = await FillAPicker(FieldStructureMethod);
            OnPropertyChanged(nameof(StructureMethod));

            _structureFlattening = await FillAPicker(FieldStructureFlattening);
            OnPropertyChanged(nameof(StructureFlattening));

            _structureStrain = await FillAPicker(FieldStructureStrain);
            OnPropertyChanged(nameof(StructureStrain));

            //There is one picker that needs all brotha's and sista's listing
            _structureRelatedAlias = await FillRelatedStructureAsync();
            OnPropertyChanged(nameof(StructureRelatedAlias));

            await currentConnection.CloseAsync();
        }

        /// <summary>
        /// Will fill the project type combobox
        /// </summary>
        private async Task<ComboBox> FillAPicker(string fieldName)
        {
            //Make sure to user default database rather then the prefered one. This one will always be there.
            return await da.GetComboboxListWithVocabAsync(TableStructure, fieldName);

        }

        /// <summary>
        /// Will fill out missing fields for model. Default auto-calculated values
        /// Done before actually saving
        /// </summary>
        private async Task SetModelAsync()
        {

            //#region Process concatenated pickers
            //if (DocumentCategoryCollection != null && DocumentCategoryCollection.Count > 0)
            //{
            //    Model.Category = ConcatenatedCombobox.PipeValues(DocumentCategoryCollection); //process list of values so they are concatenated.
            //}

            //#endregion

        }

        /// <summary>
        /// Will initialize the model with needed calculated fields
        /// </summary>
        /// <returns></returns>
        public async Task InitModel()
        {
            if (Model != null && Model.StructureID == 0 && _earthmaterial != null)
            {
                //Get current application version
                Model.StructureEarthmatID = _earthmaterial.EarthMatID;
                Model.StructureName = await idCalculator.CalculateStructureAliasAsync(_earthmaterial.EarthMatID, _earthmaterial.EarthMatName);
                OnPropertyChanged(nameof(Model));

            }
        }

        /// <summary>
        /// Will refill the form with existing values for update/editing purposes
        /// </summary>
        /// <returns></returns>
        public async Task Load()
        {

            if (_structure != null && _structure.StructureName != string.Empty)
            {
                //Set model like actual record
                _model = _structure;

                //Refresh
                OnPropertyChanged(nameof(Model));

                //#region Pickers
                ////Select values in pickers
                //List<string> bfs = ConcatenatedCombobox.UnpipeString(_document.Category);
                //_categoryCollection.Clear(); //Clear any possible values first
                //foreach (ComboBoxItem cbox in DocumentCategory.cboxItems)
                //{
                //    if (bfs.Contains(cbox.itemValue) && !_categoryCollection.Contains(cbox))
                //    {
                //        _categoryCollection.Add(cbox);
                //    }
                //}
                //OnPropertyChanged(nameof(DocumentCategory));

                //#endregion

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
                // if coming from station notes, calculate new alias
                Model.StructureEarthmatID = _earthmaterial.EarthMatID;
                Model.StructureName = await idCalculator.CalculateStructureAliasAsync(_earthmaterial.EarthMatID, _earthmaterial.EarthMatName);
            }
            else if (Model.StructureEarthmatID != null)
            {
                // if coming from field notes on a record edit that needs to be saved as a new record with stay/save
                SQLiteAsyncConnection currentConnection = da.GetConnectionFromPath(da.PreferedDatabasePath);
                List<Earthmaterial> parentAlias = await currentConnection.Table<Earthmaterial>().Where(e => e.EarthMatID == Model.StructureEarthmatID).ToListAsync();
                await currentConnection.CloseAsync();
                Model.StructureName = await idCalculator.CalculateStructureAliasAsync(Model.StructureEarthmatID, parentAlias.First().EarthMatName);
            }

            Model.StructureID = 0;

        }

        /// <summary>
        /// Will fill with all structures from other siblings
        /// </summary>
        public async Task<ComboBox> FillRelatedStructureAsync(string strucClass = "")
        {
            ComboBox relatedCbx = new ComboBox();
            relatedCbx.cboxDefaultItemIndex = -1;

            //Refresh picker if user has changed structure class/type
            if (strucClass != string.Empty)
            {
                _structureRelatedAlias.cboxItems.Clear();
                OnPropertyChanged(nameof(StructureRelatedAlias));
            }
            else
            {
                //Else keep whatever has been passed from field notes as an existing value
                if (_model != null && _model.StructureClass != string.Empty)
                {
                    strucClass = _model.StructureClass;
                }
                    
            }

            if (_earthmaterial != null || _structure != null)
            {
                //Find proper parent id either for new structure or ones in edit
                List<Structure> sts = new List<Structure>();
                if (_structure != null)
                {

                    sts = await currentConnection.Table<Structure>().Where(i => (i.StructureEarthmatID == _structure.StructureEarthmatID)).ToListAsync();

                }
                else
                {
                    if (_earthmaterial != null)
                    {
                        sts = await currentConnection.Table<Structure>().Where(i => (i.StructureEarthmatID == _earthmaterial.EarthMatID)).ToListAsync();
                    }
                }

                //Extra where clause to find only counterpart structure and not the same classes
                if (_model.StructureClass != null && _model.StructureClass.Contains(KeywordPlanar))
                {
                    sts = sts.Where(s => s.StructureClass.Contains(KeywordLinear)).ToList();
                }
                else if (_model.StructureClass != null && _model.StructureClass.Contains(KeywordLinear))
                {
                    sts = sts.Where(s => s.StructureClass.Contains(KeywordPlanar)).ToList();
                }


                if (sts != null && sts.Count > 0)
                {

                    foreach (Structure st in sts)
                    {
                        Themes.ComboBoxItem newItem = new Themes.ComboBoxItem();
                        newItem.itemValue = st.StructureID.ToString();
                        newItem.itemName = string.Format("{0} ({1})", st.StructureName, st.StructureClass);
                        relatedCbx.cboxItems.Add(newItem);
                    }

                    relatedCbx.cboxDefaultItemIndex = -1;
                }
            }

            return relatedCbx;

        }


        #endregion
    }
}
