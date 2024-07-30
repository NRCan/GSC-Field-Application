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
    [QueryProperty(nameof(Paleoflow), nameof(Paleoflow))]
    [QueryProperty(nameof(Earthmaterial), nameof(Earthmaterial))]
    public partial class PaleoflowViewModel: FieldAppPageHelper
    {
        #region INIT
        //Database
        private Paleoflow _model = new Paleoflow();

        //UI
        private ComboBox _paleoflowClass = new ComboBox();
        private ComboBox _paleoflowFeature = new ComboBox();
        private ComboBox _paleoflowFeatureAll = new ComboBox();
        private ComboBox _paleoflowSense = new ComboBox();
        private ComboBox _paleoflowBedsurface = new ComboBox();
        private ComboBox _paleoflowConfidence = new ComboBox();
        private ComboBox _paleoflowQuality = new ComboBox();
        private ComboBox _paleoflowIndicators = new ComboBox();
        private ComboBox _paleoflowRelativeAge = new ComboBox();
        private ComboBox _paleoflowMethod = new ComboBox();
        private ComboBox _paleoflowRelation = new ComboBox();
        #endregion

        #region PROPERTIES
        [ObservableProperty]
        private Earthmaterial _earthmaterial;

        [ObservableProperty]
        private Paleoflow _paleoflow;

        public Paleoflow Model { get { return _model; } set { _model = value; } }

        public bool PaleoflowTypeVisibility
        {
            get { return Preferences.Get(nameof(PaleoflowTypeVisibility), true); }
            set { Preferences.Set(nameof(PaleoflowTypeVisibility), value); }
        }

        public bool PaleoflowMeasurementVisibility
        {
            get { return Preferences.Get(nameof(PaleoflowMeasurementVisibility), true); }
            set { Preferences.Set(nameof(PaleoflowMeasurementVisibility), value); }
        }

        public bool PaleoflowDescVisibility
        {
            get { return Preferences.Get(nameof(PaleoflowDescVisibility), true); }
            set { Preferences.Set(nameof(PaleoflowDescVisibility), value); }
        }

        public bool PaleoflowRelationVisibility
        {
            get { return Preferences.Get(nameof(PaleoflowRelationVisibility), true); }
            set { Preferences.Set(nameof(PaleoflowRelationVisibility), value); }
        }

        public bool PaleoflowGeneralVisibility
        {
            get { return Preferences.Get(nameof(PaleoflowGeneralVisibility), true); }
            set { Preferences.Set(nameof(PaleoflowGeneralVisibility), value); }
        }

        public ComboBox PaleoflowClass { get { return _paleoflowClass; } set { _paleoflowClass = value; } }
        public ComboBox PaleoflowFeature { get { return _paleoflowFeature; } set { _paleoflowFeature = value; } }
        public ComboBox PaleoflowSense { get { return _paleoflowSense; } set { _paleoflowSense = value; } }

        public ComboBox PaleoflowBedsurface { get { return _paleoflowBedsurface; } set { _paleoflowBedsurface = value; } }
        public ComboBox PaleoflowConfidence { get { return _paleoflowConfidence; } set { _paleoflowConfidence = value; } }
        public ComboBox PaleoflowQuality { get { return _paleoflowQuality; } set { _paleoflowQuality = value; } }
        public ComboBox PaleoflowIndicators { get { return _paleoflowIndicators; } set { _paleoflowIndicators = value; } }

        public ComboBox PaleoflowRelativeAge { get { return _paleoflowRelativeAge; } set { _paleoflowRelativeAge = value; } }
        public ComboBox PaleoflowMethod { get { return _paleoflowMethod; } set { _paleoflowMethod = value; } }
        public ComboBox PaleoflowRelation { get { return _paleoflowRelation; } set { _paleoflowRelation = value; } }

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
                await Shell.Current.GoToAsync("../");
            }
            else
            {
                await NavigateToFieldNotes(TableNames.pflow);
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
            if (_paleoflow != null && _paleoflow.ParentName != string.Empty && _model.PFlowID != 0)
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
                await NavigateToFieldNotes(TableNames.pflow);
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
            if (_paleoflow != null && _paleoflow.ParentName != string.Empty && _model.PFlowID != 0)
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
            if (_model.PFlowID != 0)
            {
                await commandServ.DeleteDatabaseItemCommand(TableNames.pflow, _model.PFlowName, _model.PFlowParentID);
            }

            //Exit
            await NavigateToFieldNotes(TableNames.pflow);

        }

        #endregion

        public PaleoflowViewModel() 
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
            _paleoflowClass = await FillAPicker(FieldPFlowClass);
            OnPropertyChanged(nameof(PaleoflowClass));

            _paleoflowSense = await FillAPicker(FieldPFlowSense);
            OnPropertyChanged(nameof(PaleoflowSense));

            _paleoflowBedsurface = await FillAPicker(FieldPFlowBedsurf);
            OnPropertyChanged(nameof(PaleoflowBedsurface));

            _paleoflowConfidence = await FillAPicker(FieldPFlowConfidence);
            OnPropertyChanged(nameof(PaleoflowConfidence));

            _paleoflowQuality = await FillAPicker(FieldPFlowDefinition);
            OnPropertyChanged(nameof(PaleoflowQuality));

            _paleoflowIndicators = await FillAPicker(FieldPFlowNumIndic);
            OnPropertyChanged(nameof(PaleoflowIndicators));

            _paleoflowRelation = await FillAPicker(FieldPFlowRelation);
            OnPropertyChanged(nameof(PaleoflowRelation));

            _paleoflowRelativeAge = await FillAPicker(FieldPFlowRelage);
            OnPropertyChanged(nameof(PaleoflowRelativeAge));

            _paleoflowMethod = await FillAPicker(FieldPFlowMethod);
            OnPropertyChanged(nameof(PaleoflowMethod));

            //Second order pickers
            _paleoflowFeatureAll = await FillAPicker(FieldPFlowFeature);

            await currentConnection.CloseAsync();
        }

        /// <summary>
        /// Will fill all picker controls that are dependant on the user selected
        /// pflow class/type
        /// </summary>
        /// <returns></returns>
        public async Task Fill2ndRoundPickers()
        {

            if (_model != null && _model.PFlowClass != null && _model.PFlowClass != string.Empty)
            {

                //Special case, needs to contain structure class and not structure class and type
                _paleoflowFeature.cboxItems = _paleoflowFeatureAll.cboxItems.Where(f => f.itemParent != null && f.itemParent.Contains(_model.PFlowClass.Split(KeywordConcatCharacter2nd)[0])).ToList();

                //Selected index parsing
                if (_paleoflowFeature.cboxItems.Count == 1)
                {
                    _paleoflowFeature.cboxDefaultItemIndex = 0;
                }

                OnPropertyChanged(nameof(PaleoflowFeature));

            }

        }


        /// <summary>
        /// Will fill the project type combobox
        /// </summary>
        private async Task<ComboBox> FillAPicker(string fieldName, string extraField = "")
        {
            //Make sure to user default database rather then the prefered one. This one will always be there.
            return await da.GetComboboxListWithVocabAsync(TablePFlow, fieldName, extraField);

        }

        /// <summary>
        /// Will initialize the model with needed calculated fields
        /// </summary>
        /// <returns></returns>
        public async Task InitModel()
        {
            if (Model != null && Model.PFlowID == 0 && _earthmaterial != null)
            {
                //Get current application version
                Model.PFlowParentID = _earthmaterial.EarthMatID;
                Model.PFlowName = await idCalculator.CalculatePflowAliasAsync(_earthmaterial.EarthMatID, _earthmaterial.EarthMatName);
                OnPropertyChanged(nameof(Model));

            }
        }

        /// <summary>
        /// Will refill the form with existing values for update/editing purposes
        /// </summary>
        /// <returns></returns>
        public async Task Load()
        {

            if (_paleoflow != null && _paleoflow.PFlowName != string.Empty)
            {
                //Set model like actual record
                _model = _paleoflow;

                //Refresh
                OnPropertyChanged(nameof(Model));


            }
            else
            {
                //Fill in second round of pickers
                await Fill2ndRoundPickers();
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
                Model.PFlowParentID = _earthmaterial.EarthMatID;
                Model.PFlowName = await idCalculator.CalculatePflowAliasAsync(_earthmaterial.EarthMatID, _earthmaterial.EarthMatName);
            }
            else if (Model.PFlowParentID != null)
            {
                // if coming from field notes on a record edit that needs to be saved as a new record with stay/save
                SQLiteAsyncConnection currentConnection = da.GetConnectionFromPath(da.PreferedDatabasePath);
                List<Earthmaterial> parentAlias = await currentConnection.Table<Earthmaterial>().Where(e => e.EarthMatID == Model.PFlowParentID).ToListAsync();
                await currentConnection.CloseAsync();
                Model.PFlowName = await idCalculator.CalculatePflowAliasAsync(Model.PFlowParentID, parentAlias.First().EarthMatName);
            }

            Model.PFlowID = 0;

        }

        #endregion
    }
}
