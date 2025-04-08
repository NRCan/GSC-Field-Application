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
    [QueryProperty(nameof(DrillHole), nameof(DrillHole))]
    [QueryProperty(nameof(FieldLocation), nameof(FieldLocation))]
    public partial class DrillHoleViewModel: FieldAppPageHelper
    {
        #region INIT

        private DrillHole _model = new DrillHole();
        private ComboBox _drillType = new ComboBox();
        private ComboBox _drillUnits = new ComboBox();
        private ComboBox _drillHoleSizes = new ComboBox();
        private ComboBox _drillHoleLogType = new ComboBox();
        private ComboBox _drillCoreSizes = new ComboBox();

        private ComboBox _drillCoreSizesAll = new ComboBox();

        private string _drillHoleLogFrom = string.Empty;
        private string _drillHoleLogTo = string.Empty;
        private ObservableCollection<ComboBoxItem> _drillHoleLogIntervalCollection = new ObservableCollection<ComboBoxItem>();

        #endregion

        #region PROPERTIES

        [ObservableProperty]
        private DrillHole _drillHole;

        [ObservableProperty]
        private FieldLocation _fieldLocation;

        public DrillHole Model { get { return _model; } set { _model = value; } }

        public bool DrillHoleContextVisibility
        {
            get { return Preferences.Get(nameof(DrillHoleContextVisibility), true); }
            set { Preferences.Set(nameof(DrillHoleContextVisibility), value); }
        }

        public bool DrillHoleMetricsVisibility
        {
            get { return Preferences.Get(nameof(DrillHoleMetricsVisibility), true); }
            set { Preferences.Set(nameof(DrillHoleMetricsVisibility), value); }
        }

        public bool DrillHoleLogVisibility
        {
            get { return Preferences.Get(nameof(DrillHoleLogVisibility), true); }
            set { Preferences.Set(nameof(DrillHoleLogVisibility), value); }
        }

        public bool DrillHoleGeneralVisibility
        {
            get { return Preferences.Get(nameof(DrillHoleGeneralVisibility), true); }
            set { Preferences.Set(nameof(DrillHoleGeneralVisibility), value); }
        }

        public ComboBox DrillType { get { return _drillType; } set { _drillType = value; } }
        public ComboBox DrillUnits { get { return _drillUnits; } set { _drillUnits = value; } }
        public ComboBox DrillHoleSizes { get { return _drillHoleSizes; } set { _drillHoleSizes = value; } }
        public ComboBox DrillHoleLogType { get { return _drillHoleLogType; } set { _drillHoleLogType = value; } }
        public ComboBox DrillCoreSizes { get { return _drillCoreSizes; } set { _drillCoreSizes = value; } }

        public string DrillHoleLogFrom { get { return _drillHoleLogFrom; } set { _drillHoleLogFrom = value; } }
        public string DrillHoleLogTo { get { return _drillHoleLogTo; } set { _drillHoleLogTo = value; } }
        public ObservableCollection<ComboBoxItem> DrillHoleLogIntervalCollection { get { return _drillHoleLogIntervalCollection; } set { _drillHoleLogIntervalCollection = value; OnPropertyChanged(nameof(DrillHoleLogIntervalCollection)); } }

        public bool EarthMaterialVisible
        {
            get { return Preferences.Get(nameof(EarthMaterialVisible), true); }
            set { Preferences.Set(nameof(EarthMaterialVisible), value); }
        }

        public bool DocumentVisible
        {
            get { return Preferences.Get(nameof(DocumentVisible), true); }
            set { Preferences.Set(nameof(DocumentVisible), value); }
        }
        #endregion

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
            //Save
            await SetAndSaveModelAsync();

            //Exit
            await NavigateToFieldNotes(TableNames.drill);
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
            if (_model.DrillID != 0)
            {
                await commandServ.DeleteDatabaseItemCommand(TableNames.drill, _model.DrillIDName, _model.DrillLocationID, false, _model.DrillID);
            }

            //Exit
            await NavigateToFieldNotes(TableNames.drill);

        }

        /// <summary>
        /// Special command to set contact relation with other records
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        public async Task SetLogInterval()
        {
            if (_drillHoleLogFrom != string.Empty && _drillHoleLogTo != string.Empty)
            {
                //Build string
                string logInterval = _drillHoleLogFrom + KeywordConcatCharacter2nd + _drillHoleLogTo;

                //Add 
                ComboBoxItem newInterval = new ComboBoxItem();
                newInterval.itemName = logInterval;
                newInterval.itemValue = logInterval;
                newInterval.canRemoveItem = true;
                _drillHoleLogIntervalCollection.Add(newInterval);
            }

            OnPropertyChanged(nameof(DrillHoleLogIntervalCollection));
        }

        [RelayCommand]
        async Task AddEarthmat()
        {
            //Save
            await SetAndSaveModelAsync();

            //Navigate to child
            await Shell.Current.GoToAsync($"/{nameof(EarthmatPage)}/",
                new Dictionary<string, object>
                {
                    [nameof(Earthmaterial)] = null,
                    [nameof(Station)] = null,
                    [nameof(DrillHole)] = Model
                }
            );
        }

        [RelayCommand]
        async Task AddDocument()
        {
            //Save
            await SetAndSaveModelAsync();

            //Navigate to child
            await Shell.Current.GoToAsync($"/{nameof(DocumentPage)}/",
                new Dictionary<string, object>
                {
                    [nameof(Station)] = null,
                    [nameof(Document)] = null,
                    [nameof(DrillHole)] = Model
                }
            );
        }

        #endregion

        #region METHODS


        public async Task SetAndSaveModelAsync()
        {
            //Fill out missing values in model
            await SetModelAsync();

            //Edge case: renaming parent location alias based on drill hole alias
            if (Model != null && Model.DrillLocationID > 0)
            {
                List<FieldLocation> parentLocation = await DataAccess.DbConnection.Table<FieldLocation>().Where(x => x.LocationID == Model.DrillLocationID).ToListAsync();

                if (parentLocation != null && parentLocation.Count > 0)
                {
                    DataIDCalculation iDCalculation = new DataIDCalculation();
                    parentLocation[0].LocationAlias = await iDCalculation.CalculateLocationAliasAsync(Model.DrillIDName);
                    await da.SaveItemAsync(parentLocation[0], true);
                }
            }

            //Validate if new entry or update
            if (_drillHole != null && _drillHole.DrillIDName != string.Empty && _model.DrillID != 0)
            {
                await da.SaveItemAsync(Model, true);
            }
            else
            {
                //Insert new record
                await da.SaveItemAsync(Model, false);
            }

        }

        /// <summary>
        /// Will fill out missing fields for model. Default auto-calculated values
        /// Done before actually saving
        /// </summary>
        private async Task SetModelAsync()
        {
            //Manage date pickers
            //They don't init with the current datetime value which ends in a null value ni the database
            //Manage initial value for datepickers
            if (_model != null)
            {
                if (_model.DrillDate is null || _model.DrillDate == string.Empty)
                {
                    _model.DrillDate = DateTime.Today.ToString(DateStringFormat);

                    //Refresh
                    OnPropertyChanged(nameof(Model));
                }

                if (_model.DrillRelogDate is null || _model.DrillRelogDate == string.Empty)
                {
                    _model.DrillRelogDate = DateTime.Today.ToString(DateStringFormat);

                    //Refresh
                    OnPropertyChanged(nameof(Model));
                }


            }

            #region Process pickers
            if (DrillHoleLogIntervalCollection.Count > 0)
            {
                Model.DrillRelogIntervals = ConcatenatedCombobox.PipeValues(DrillHoleLogIntervalCollection); //process list of values so they are concatenated.
            }
            #endregion
        }

        /// <summary>
        /// Will reset model fields to default just like it's a new record
        /// </summary>
        /// <returns></returns>
        private async Task ResetModelAsync()
        {

            //Reset model
            if (_fieldLocation != null)
            {
                // if coming from em notes, calculate new alias
                Model.DrillLocationID = _fieldLocation.LocationID;
                DateTime locationDate = DateTime.Parse(_fieldLocation.LocationTimestamp);
                Model.DrillIDName = await idCalculator.CalculateDrillAliasAsync(locationDate, _fieldLocation.LocationID);
            }

            else if (Model.DrillLocationID != null)
            {
                // if coming from field notes on a record edit that needs to be saved as a new record with stay/save
                List<FieldLocation> parent = await DataAccess.DbConnection.Table<FieldLocation>().Where(e => e.LocationID == Model.DrillLocationID).ToListAsync();
                DateTime locationDate = DateTime.Parse(parent.First().LocationTimestamp);
                Model.DrillIDName = await idCalculator.CalculateDrillAliasAsync(locationDate, parent.First().LocationID);
            }

            Model.DrillID = 0;

        }

        /// <summary>
        /// Will refill the form with existing values for update/editing purposes
        /// </summary>
        /// <returns></returns>
        public async Task Load()
        {
            if (_drillHole != null && _drillHole.DrillIDName != string.Empty)
            {
                //Set model like actual record
                _model = _drillHole;

                List<string> dris = ConcatenatedCombobox.UnpipeString(_drillHole.DrillRelogIntervals);
                _drillHoleLogIntervalCollection.Clear(); //Clear any possible values first
                foreach (string dri in dris)
                {
                    ComboBoxItem new_dri = new ComboBoxItem();
                    new_dri.itemValue = dri;
                    new_dri.itemName = dri;
                    new_dri.canRemoveItem = true;
                    _drillHoleLogIntervalCollection.Add(new_dri);
                }
                OnPropertyChanged(nameof(DrillHoleLogIntervalCollection));

                //Refresh
                OnPropertyChanged(nameof(Model));

            }

        }

        /// <summary>
        /// Will fill all picker controls
        /// TODO: make sure this whole thing doesn't slow too much form rendering
        /// </summary>
        /// <returns></returns>
        public async Task FillPickers()
        {

            _drillType = await FillAPicker(FieldDrillType);
            OnPropertyChanged(nameof(DrillType));

            _drillUnits = await FillAPicker(FieldDrillUnit);
            OnPropertyChanged(nameof(DrillUnits));

            _drillHoleSizes = await FillAPicker(FieldDrillHoleSize);
            OnPropertyChanged(nameof(DrillHoleSizes));

            _drillHoleLogType = await FillAPicker(FieldDrillRelogType);
            OnPropertyChanged(nameof(DrillHoleLogType));

            _drillCoreSizesAll = await FillAPicker(FieldDrillCoreSize);

            //Not a picker but still needs initialization
            await FillLogBy();
        }


        /// <summary>
        /// Generic method to fill a needed picker control with vocabulary
        /// </summary>
        private async Task<ComboBox> FillAPicker(string fieldName, string extraField = "")
        {
            //Make sure to user default database rather then the prefered one. This one will always be there.
            return await da.GetComboboxListWithVocabAsync(TableDrillHoles, fieldName, extraField);

        }

        /// <summary>
        /// Will initialize the model with needed calculated fields
        /// </summary>
        /// <returns></returns>
        public async Task InitModel()
        {
            if (Model != null && Model.DrillID == 0 && _fieldLocation != null)
            {
                //Get current application version
                Model.DrillLocationID = _fieldLocation.LocationID;
                DateTime parentDate = DateTime.Parse(_fieldLocation.LocationTimestamp);
                Model.DrillIDName = await idCalculator.CalculateDrillAliasAsync(parentDate, _fieldLocation.LocationID);
                OnPropertyChanged(nameof(Model));

            }

        }

        /// <summary>
        /// Will fill log by with default value of fieldbook geologist
        /// </summary>
        private async Task FillLogBy()
        {
            List<Metadata> mets = await DataAccess.DbConnection.QueryAsync<Metadata>(string.Format("select * from {0} limit 1", TableMetadata));

            if (mets != null && mets.Count == 1)
            {
                _model.DrillRelogBy = mets[0].Geologist;

                //Update UI
                OnPropertyChanged(nameof(Model));
            }

        }

        /// <summary>
        /// Will autofill the core size based on selected hole size.
        /// structure class/type
        /// </summary>
        /// <returns></returns>
        public async Task FillCoreSize()
        {
            //Push value to a text box. 
            if (_model != null && _model.DrillHoleSize != null && _model.DrillHoleSize != string.Empty && _drillCoreSizesAll != null && _drillCoreSizesAll.cboxItems.Count > 0)
            {
                ComboBoxItem getParent = _drillCoreSizesAll.cboxItems.Where(f => f.itemParent != null && f.itemParent.Contains(_model.DrillHoleSize)).ToList().FirstOrDefault();

                if (getParent != null)
                {
                    _model.DrillCoreSize = getParent.itemValue;

                    OnPropertyChanged(nameof(Model));
                }

            }

        }


        #endregion
    }
}
