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

namespace GSCFieldApp.ViewModel
{
    [QueryProperty(nameof(Fossil), nameof(Fossil))]
    [QueryProperty(nameof(Earthmaterial), nameof(Earthmaterial))]
    public partial class FossilViewModel: FieldAppPageHelper
    {
        #region INIT
        //Database
        private Fossil _model = new Fossil();

        //UI
        private ComboBox _fossilType = new ComboBox();

        #endregion

        #region PROPERTIES

        [ObservableProperty]
        private Earthmaterial _earthmaterial;

        [ObservableProperty]
        private Fossil _fossil;

        public Fossil Model { get { return _model; } set { _model = value; } }

        public ComboBox FossilType { get { return _fossilType; } set { _fossilType = value; } }

        public bool FossilGeneralVisibility
        {
            get { return Preferences.Get(nameof(FossilGeneralVisibility), true); }
            set { Preferences.Set(nameof(FossilGeneralVisibility), value); }
        }

        #endregion

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
                await Shell.Current.GoToAsync($"//{nameof(MapPage)}/");
            }
            else
            {
                await NavigateToFieldNotes(TableNames.fossil);
            }
        }

        /// <summary>
        /// Save button command
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        async Task Save()
        {

            //Validate if new entry or update
            if (_fossil != null && _fossil.FossilIDName != string.Empty && _model.FossilID != 0)
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
                await Shell.Current.GoToAsync($"//{nameof(MapPage)}/");
            }
            else
            {
                await NavigateToFieldNotes(TableNames.fossil);
            }
        }

        /// <summary>
        /// Save button command
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        async Task SaveStay()
        {

            //Validate if new entry or update
            if (_fossil != null && _fossil.FossilIDName != string.Empty && _model.FossilID != 0)
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
            if (_model.FossilID != 0)
            {
                await commandServ.DeleteDatabaseItemCommand(TableNames.fossil, _model.FossilIDName, _model.FossilID);
            }

            //Exit
            await NavigateToFieldNotes(TableNames.fossil);

        }

        #endregion

        public FossilViewModel() 
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
            _fossilType = await FillAPicker(FieldFossilType);
            OnPropertyChanged(nameof(FossilType));

            await currentConnection.CloseAsync();
        }

        /// <summary>
        /// Will fill the project type combobox
        /// </summary>
        private async Task<ComboBox> FillAPicker(string fieldName, string extraField = "")
        {
            //Make sure to user default database rather then the prefered one. This one will always be there.
            return await da.GetComboboxListWithVocabAsync(TableFossil, fieldName, extraField);

        }

        /// <summary>
        /// Will initialize the model with needed calculated fields
        /// </summary>
        /// <returns></returns>
        public async Task InitModel()
        {
            if (Model != null && Model.FossilID == 0 && _earthmaterial != null)
            {
                //Get current application version
                Model.FossilParentID = _earthmaterial.EarthMatID;
                Model.FossilIDName = await idCalculator.CalculateFossilAliasAsync(_earthmaterial.EarthMatID, _earthmaterial.EarthMatName);
                OnPropertyChanged(nameof(Model));

            }
        }

        /// <summary>
        /// Will refill the form with existing values for update/editing purposes
        /// </summary>
        /// <returns></returns>
        public async Task Load()
        {

            if (_fossil != null && _fossil.FossilIDName != string.Empty)
            {
                //Set model like actual record
                _model = _fossil;

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
                // if coming from station notes, calculate new alias
                Model.FossilParentID = _earthmaterial.EarthMatID;
                Model.FossilIDName = await idCalculator.CalculateFossilAliasAsync(_earthmaterial.EarthMatID, _earthmaterial.EarthMatName);
            }
            else if (Model.FossilParentID != null)
            {
                // if coming from field notes on a record edit that needs to be saved as a new record with stay/save
                SQLiteAsyncConnection currentConnection = da.GetConnectionFromPath(da.PreferedDatabasePath);
                List<Earthmaterial> parentAlias = await currentConnection.Table<Earthmaterial>().Where(e => e.EarthMatID == Model.FossilParentID).ToListAsync();
                await currentConnection.CloseAsync();
                Model.FossilIDName = await idCalculator.CalculateFossilAliasAsync(Model.FossilParentID, parentAlias.First().EarthMatName);
            }

            Model.FossilID = 0;

        }

        #endregion
    }
}
