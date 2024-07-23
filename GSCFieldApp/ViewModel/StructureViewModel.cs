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
    public partial class StructureViewModel: ObservableObject
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

        #endregion

        public StructureViewModel()
        { 
        
        }

        #region RELAYS

        [RelayCommand]
        public async Task Back()
        {
            //Make sure to delete station and location records if user is coming from map page
            if (_earthmaterial != null && _earthmaterial.IsQuickEarthmat != null && _earthmaterial.IsQuickEarthmat.Value)
            {
                //Get parent record
                SQLiteAsyncConnection currentConnection = da.GetConnectionFromPath(da.PreferedDatabasePath);
                Station sRecord = await currentConnection.Table<Station>().Where(s => s.StationID == _earthmaterial.EarthMatStatID).FirstAsync();

                //Delete without forced pop-up warning and question
                await commandServ.DeleteDatabaseItemCommand(TableNames.location, sRecord.StationAlias, sRecord.LocationID, true);

            }

            //Android when navigating back, ham menu disapears if / isn't added to path
            await Shell.Current.GoToAsync($"{nameof(FieldNotesPage)}/");
        }

        /// <summary>
        /// Save button command
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        async Task Save()
        {
            //Fill out missing values in model
            //await SetModelAsync();

            //Validate if new entry or update
            if (_structure != null && _structure.StructureName != string.Empty && _model.SampleID != 0)
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
            await Shell.Current.GoToAsync($"{nameof(FieldNotesPage)}/",
                new Dictionary<string, object>
                {
                    ["UpdateTableID"] = RandomNumberGenerator.GetHexString(10, false),
                    ["UpdateTable"] = TableNames.structure,
                }
            );
        }

        /// <summary>
        /// Save button command
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        async Task SaveStay()
        {
            //Fill out missing values in model
            //await SetModelAsync();

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
            await Shell.Current.GoToAsync($"{nameof(FieldNotesPage)}/",
                new Dictionary<string, object>
                {
                    ["UpdateTableID"] = RandomNumberGenerator.GetHexString(10, false),
                    ["UpdateTable"] = TableNames.structure,
                }
            );

        }


        #endregion

        #region METHODS

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
                Model.StructureName = await idCalculator.CalculateSampleAliasAsync(_earthmaterial.EarthMatID, _earthmaterial.EarthMatName);
            }
            else if (Model.SampleEarthmatID != null)
            {
                // if coming from field notes on a record edit that needs to be saved as a new record with stay/save
                SQLiteAsyncConnection currentConnection = da.GetConnectionFromPath(da.PreferedDatabasePath);
                List<Earthmaterial> parentAlias = await currentConnection.Table<Earthmaterial>().Where(e => e.EarthMatID == Model.StructureEarthmatID).ToListAsync();
                await currentConnection.CloseAsync();
                Model.StructureName = await idCalculator.CalculateEarthmatAliasAsync(Model.StructureEarthmatID, parentAlias.First().EarthMatName);
            }

            Model.StructureID = 0;

        }

        #endregion
    }
}
