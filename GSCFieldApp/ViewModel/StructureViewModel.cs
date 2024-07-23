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

        DataAccess da = new DataAccess();
        public DataIDCalculation idCalculator = new DataIDCalculation();
        private Structure _model = new Structure();


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
            //_documentCategory = await FillAPicker(FieldDocumentCategory);
            //_documentScale = await FillAPicker(FieldDocumentScaleDirection);
            //_documentFileType = await FillAPicker(FieldDocumentType);

            //OnPropertyChanged(nameof(DocumentCategory));
            //OnPropertyChanged(nameof(DocumentScale));
            //OnPropertyChanged(nameof(DocumentFileType));
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

        #endregion
    }
}
