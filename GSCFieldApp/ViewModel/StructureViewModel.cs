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
    [QueryProperty(nameof(Structure), nameof(Structure))]
    [QueryProperty(nameof(Earthmaterial), nameof(Earthmaterial))]
    public partial class StructureViewModel: FieldAppPageHelper
    {
        #region INIT
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
        private ComboBox _structureClass = new ComboBox();
        private ComboBox _structureDetail = new ComboBox();

        private ComboBox _structureFormatAll = new ComboBox();
        private ComboBox _structureDetailAll = new ComboBox();
        private ComboBox _structureAttitudeAll = new ComboBox();
        private ComboBox _structureYoungingAll = new ComboBox();
        private ComboBox _structureGenerationAll = new ComboBox();

        #endregion

        #region PROPERTIES

        [ObservableProperty]
        private Earthmaterial _earthmaterial;

        [ObservableProperty]
        private Structure _structure;

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
        public ComboBox StructureClass { get { return _structureClass; } set { _structureClass = value; } }
        public ComboBox StructureDetail{ get { return _structureDetail; } set { _structureDetail = value; } }

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
                await Shell.Current.GoToAsync($"//{nameof(MapPage)}/");
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
                await Shell.Current.GoToAsync($"//{nameof(MapPage)}/");
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
                await commandServ.DeleteDatabaseItemCommand(TableNames.structure, _model.StructureName, _model.StructureID);
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
            _structureClass = await FillAPicker(FieldStructureClass);
            OnPropertyChanged(nameof(StructureClass));

            _structureMethod = await FillAPicker(FieldStructureMethod);
            OnPropertyChanged(nameof(StructureMethod));

            _structureFlattening = await FillAPicker(FieldStructureFlattening);
            OnPropertyChanged(nameof(StructureFlattening));

            _structureStrain = await FillAPicker(FieldStructureStrain);
            OnPropertyChanged(nameof(StructureStrain));

            //Second order pickers
            _structureDetailAll = await FillAPicker(FieldStructureDetail);
            _structureAttitudeAll = await FillAPicker(FieldStructureAttitude);
            _structureYoungingAll = await FillAPicker(FieldStructureYoung);
            _structureGenerationAll = await FillAPicker(FieldStructureGeneration);

            //There is one picker who doesn't have parents in surficial
            if (FieldThemes.SurficialVisibility)
            {
                _structureFormat = await FillAPicker(FieldStructureFormat);
                OnPropertyChanged(nameof(StructureFormat));
            }
            else
            {
                _structureFormatAll = await FillAPicker(FieldStructureFormat);
            }

            //There is one picker that needs all brotha's and sista's listing
            _structureRelatedAlias = await FillRelatedStructureAsync();
            OnPropertyChanged(nameof(StructureRelatedAlias));

            await currentConnection.CloseAsync();
        }

        /// <summary>
        /// Will fill all picker controls that are dependant on the user selected
        /// structure class/type
        /// </summary>
        /// <returns></returns>
        public async Task Fill2ndRoundPickers()
        {

            if (_model != null && _model.StructureClass != null && _model.StructureClass != string.Empty)
            {
                //One picklist can only be filtered down if coming from bedrock project
                if (FieldThemes.BedrockVisibility)
                {
                    //Special case, needs to contain structure class and not structure class and type
                    _structureFormat.cboxItems = _structureFormatAll.cboxItems.Where(f => f.itemParent != null && f.itemParent.Contains(_model.StructureClass.Split(KeywordConcatCharacter2nd)[0])).ToList();

                    //Selected index parsing
                    if (_structureFormat.cboxItems.Count == 1)
                    {
                        _structureFormat.cboxDefaultItemIndex = 0;
                    }

                    OnPropertyChanged(nameof(StructureFormat));
                }

                _structureDetail.cboxItems = _structureDetailAll.cboxItems.Where(f => f.itemParent != null && f.itemParent.Contains(_model.StructureClass)).ToList();
                //Selected index parsing
                if (_structureDetail.cboxItems.Count == 1)
                {
                    _structureDetail.cboxDefaultItemIndex = 0;
                }
                OnPropertyChanged(nameof(StructureDetail));

                _structureAttitude.cboxItems = _structureAttitudeAll.cboxItems.Where(f => f.itemParent != null && f.itemParent.Contains(_model.StructureClass)).ToList();
                //Selected index parsing
                if (_structureAttitude.cboxItems.Count == 1)
                {
                    _structureAttitude.cboxDefaultItemIndex = 0;
                }
                OnPropertyChanged(nameof(StructureAttitude));

                _structureYounging.cboxItems = _structureYoungingAll.cboxItems.Where(f => f.itemParent != null && f.itemParent.Contains(_model.StructureClass)).ToList();
                //Selected index parsing
                if (_structureYounging.cboxItems.Count == 1)
                {
                    _structureYounging.cboxDefaultItemIndex = 0;
                }
                OnPropertyChanged(nameof(StructureYounging));

                _structureGeneration.cboxItems = _structureGenerationAll.cboxItems.Where(f => f.itemParent != null && f.itemParent.Contains(_model.StructureClass)).ToList();
                //Selected index parsing
                if (_structureGeneration.cboxItems.Count == 1)
                {
                    _structureGeneration.cboxDefaultItemIndex = 0;
                }
                OnPropertyChanged(nameof(StructureGeneration));
            }

        }


        /// <summary>
        /// Will fill the project type combobox
        /// </summary>
        private async Task<ComboBox> FillAPicker(string fieldName, string extraField = "")
        {
            //Make sure to user default database rather then the prefered one. This one will always be there.
            return await da.GetComboboxListWithVocabAsync(TableStructure, fieldName, extraField);

        }

        /// <summary>
        /// Will fill out missing fields for model. Default auto-calculated values
        /// Done before actually saving
        /// </summary>
        private async Task SetModelAsync()
        {

            //Make sure to split class picker into two fields (class and type)
            if (_model != null && _model.StructureClass != null && _model.StructureClass != string.Empty)
            {
                string[] splitStructure = _model.StructureClass.Split(KeywordConcatCharacter2nd);
                _model.StructureClass = splitStructure[0];
                if (splitStructure.Count() > 1)
                {
                    _model.StructureType = splitStructure[1];
                }
            }

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

                #region Pickers

                //Make sure to concatenate class picker from two fields (class and type)
                if (_model != null && _model.GetClassType != null && _model.GetClassType != string.Empty)
                {
                    _model.StructureClass = _model.GetClassType;
                }

                #endregion

                //Refresh
                OnPropertyChanged(nameof(Model));

                //Fill in second round of pickers
                await Fill2ndRoundPickers().ContinueWith(async antecedent => await Load2ndRound());

            }

        }

        /// <summary>
        /// Will refill the form with existing values for update/editing purposes
        /// </summary>
        /// <returns></returns>
        public async Task Load2ndRound()
        {

            if (_structure != null && _structure.StructureName != string.Empty && _structure.StructureClass != null && _structure.StructureClass != String.Empty)
            {
                //Special case picker
                //Doesn't interact well with xaml converter, need to keep it here

                foreach (ComboBoxItem cbox in StructureAttitude.cboxItems)
                {
                    if (cbox.itemValue == _structure.StructureAttitude)
                    {
                        StructureAttitude.cboxDefaultItemIndex = StructureAttitude.cboxItems.IndexOf(cbox); break;
                    }
                }
                OnPropertyChanged(nameof(StructureAttitude));

                foreach (ComboBoxItem cbox in StructureYounging.cboxItems)
                {
                    if (cbox.itemValue == _structure.StructureYounging)
                    {
                        StructureYounging.cboxDefaultItemIndex = StructureYounging.cboxItems.IndexOf(cbox); break;
                    }
                }
                OnPropertyChanged(nameof(StructureYounging));

                foreach (ComboBoxItem cbox in StructureGeneration.cboxItems)
                {
                    if (cbox.itemValue == _structure.StructureGeneration)
                    {
                        StructureGeneration.cboxDefaultItemIndex = StructureGeneration.cboxItems.IndexOf(cbox); break;
                    }
                }
                OnPropertyChanged(nameof(StructureGeneration));

                //foreach (ComboBoxItem cbox in StructureFormat.cboxItems)
                //{
                //    if (cbox.itemValue == _structure.StructureFormat)
                //    {
                //        StructureFormat.cboxDefaultItemIndex = StructureFormat.cboxItems.IndexOf(cbox); break;
                //    }
                //}
                //OnPropertyChanged(nameof(StructureFormat));

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
                        //Exclude self from list
                        if (st.StructureName != _model.StructureName)
                        {
                            Controls.ComboBoxItem newItem = new Controls.ComboBoxItem();
                            newItem.itemValue = st.StructureID.ToString();
                            newItem.itemName = string.Format("{0} ({1})", st.StructureName, st.StructureClass);
                            relatedCbx.cboxItems.Add(newItem);
                        }

                    }

                    relatedCbx.cboxDefaultItemIndex = -1;
                }
            }

            return relatedCbx;

        }


        #endregion
    }
}
