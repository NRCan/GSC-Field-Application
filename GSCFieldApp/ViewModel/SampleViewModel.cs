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
using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Alerts;
using SQLite;
using System.Data;
using System.Security.Cryptography;

namespace GSCFieldApp.ViewModel
{
    [QueryProperty(nameof(Sample), nameof(Sample))]
    [QueryProperty(nameof(Earthmaterial), nameof(Earthmaterial))]
    public partial class SampleViewModel : FieldAppPageHelper
    {

        #region INIT
        private Sample _model = new Sample();
        private ComboBox _samplePurpose = new ComboBox();
        private ComboBox _sampleType = new ComboBox();
        private ComboBox _sampleCorePortion = new ComboBox();
        private ComboBox _sampleFormat = new ComboBox();
        private ComboBox _sampleSurface = new ComboBox();
        private ComboBox _sampleQuality = new ComboBox();
        private ComboBox _sampleState = new ComboBox();
        private ComboBox _sampleHorizon = new ComboBox();
        private bool _isSampleDuplicate = false;

        //Concatenated
        private ComboBoxItem _selectedSamplePurpose = new ComboBoxItem();
        private ObservableCollection<ComboBoxItem> _purposeCollection = new ObservableCollection<ComboBoxItem>();

        #endregion

        #region PROPERTIES

        [ObservableProperty]
        private Earthmaterial _earthmaterial;

        [ObservableProperty]
        private Sample _sample;

        public Sample Model { get { return _model; } set { _model = value; } }

        public bool SampleDescVisibility
        {
            get { return Preferences.Get(nameof(SampleDescVisibility), true); }
            set { Preferences.Set(nameof(SampleDescVisibility), value); }
        }

        public bool SampleCoreVisibility
        {
            get { return Preferences.Get(nameof(SampleCoreVisibility), true); }
            set { Preferences.Set(nameof(SampleCoreVisibility), value); }
        }

        public bool SampleOrientVisibility
        {
            get { return Preferences.Get(nameof(SampleOrientVisibility), true); }
            set { Preferences.Set(nameof(SampleOrientVisibility), value); }
        }

        public bool SampleStateVisibility
        {
            get { return Preferences.Get(nameof(SampleStateVisibility), true); }
            set { Preferences.Set(nameof(SampleStateVisibility), value); }
        }

        public bool SampleGeneralVisibility
        {
            get { return Preferences.Get(nameof(SampleGeneralVisibility), true); }
            set { Preferences.Set(nameof(SampleGeneralVisibility), value); }
        }

        public ComboBox SampleType { get { return _sampleType; } set { _sampleType = value; } }

        public ComboBox SamplePurpose { get { return _samplePurpose; } set { _samplePurpose = value; } }
        public ObservableCollection<ComboBoxItem> SamplePurposeCollection { get { return _purposeCollection; } set { _purposeCollection = value; OnPropertyChanged(nameof(SamplePurposeCollection)); } }
        public ComboBoxItem SelectedSamplePurpose
        {
            get
            {
                return _selectedSamplePurpose;
            }
            set
            {
                if (_selectedSamplePurpose != value)
                {
                    if (_purposeCollection != null)
                    {
                        if (_purposeCollection.Count > 0 && _purposeCollection[0] == null)
                        {
                            _purposeCollection.RemoveAt(0);
                        }
                        if (value != null && value.itemName != string.Empty)
                        {
                            _purposeCollection.Add(value);
                            _selectedSamplePurpose = value;
                            OnPropertyChanged(nameof(SelectedSamplePurpose));
                            
                        }

                    }


                }

            }
        }

        public bool IsSampleDuplicate 
        { 
            get 
            { 
                return _isSampleDuplicate; 
            } 
            set 
            { 
                _isSampleDuplicate = value;

                //Initialiaze with a calculated value duplicate name
                if (_isSampleDuplicate && Model != null && (Model.SampleDuplicateName == null || Model.SampleDuplicateName == string.Empty))
                {
                    Model.SampleDuplicateName = Model.SampleName; //Init with current sample name
                }
                else if (!_isSampleDuplicate && Model != null && (Model.SampleDuplicateName != string.Empty && Model.SampleDuplicateName == Model.SampleName))
                {
                    //If duplicate name hasn't been edited by user but checkbox has been set off, revert to empty string.
                    Model.SampleDuplicateName = string.Empty;
                }
                OnPropertyChanged(nameof(Model));
            } 
        }

        public ComboBox SampleCorePortion { get { return _sampleCorePortion; } set { _sampleCorePortion = value; } }
        public ComboBox SampleFormat { get { return _sampleFormat; } set { _sampleFormat = value; } }
        public ComboBox SampleSurface { get { return _sampleSurface; } set { _sampleSurface = value; } }
        public ComboBox SampleQuality { get { return _sampleQuality; } set { _sampleQuality = value; } }
        public ComboBox SampleState { get { return _sampleState; } set { _sampleState = value; } }
        public ComboBox SampleHorizon { get { return _sampleHorizon; } set { _sampleHorizon = value; } }

        private bool CustomSampleNameEnabled
        {
            get { return Preferences.Get(nameof(CustomSampleNameEnabled), false); }
            set { }
        }

        #endregion

        public SampleViewModel() 
        {
            //Init new field theme
            FieldThemes = new FieldThemes();
        }

        #region RELAYS
        /// <summary>
        /// Back button command
        /// </summary>
        /// <returns></returns>
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
            if (_sample != null && _sample.SampleName != string.Empty && _model.SampleID != 0)
            {

                await da.SaveItemAsync(Model, true);
            }
            else
            {
                //New entry coming from parent form
                //Insert new record
                await da.SaveItemAsync(Model, false);
            }

            //Exit or stay in map page if quick photo
            if (_earthmaterial != null && _earthmaterial.IsMapPageQuick)
            {
                await Shell.Current.GoToAsync($"//{nameof(MapPage)}/");
            }
            else
            {
                await NavigateToFieldNotes(TableNames.sample);
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
            if (_sample != null && _sample.SampleName != string.Empty && _model.SampleID != 0)
            {
                await da.SaveItemAsync(Model, true);
            }
            else
            {
                //Insert new record
                await da.SaveItemAsync(Model, false);

            }

            //Show saved message
            await Toast.Make(LocalizationResourceManager["ToastSaveRecord"].ToString()).Show(CancellationToken.None);

            //Reset
            await ResetModelAsync();
            OnPropertyChanged(nameof(Model));

            //Surficial reminder
            await DuplicateReminder();


        }

        [RelayCommand]
        async Task SaveDelete()
        {
            if (_model.SampleID != 0)
            {
                await commandServ.DeleteDatabaseItemCommand(TableNames.sample, _model.SampleName, _model.SampleID);
            }

            //Exit
            await NavigateToFieldNotes(TableNames.sample);

        }

        [RelayCommand]
        async Task SampleNameEdit(string sampleName)
        {
            if (CustomSampleNameEnabled)
            {
                string editedSampleName = await Shell.Current.DisplayPromptAsync(LocalizationResourceManager["SamplePageEditNameTitle"].ToString(),
                    LocalizationResourceManager["SamplePageEditNameMessage"].ToString(),
                    LocalizationResourceManager["GenericButtonOk"].ToString(),
                    LocalizationResourceManager["GenericButtonCancel"].ToString(), null, -1, null, Model.SampleName);

                if (editedSampleName != null && editedSampleName != string.Empty)
                {
                    Model.SampleName = editedSampleName;
                    OnPropertyChanged(nameof(Model));
                }

            }

        }

        /// <summary>
        /// Will calculate the Core To value based on entered Core From (in m) and Core length (in cm)
        /// It'll need to be adjusted since the units are different between from and length.
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        public async void SampleCoreCalculatTo()
        {
            await CalculateSampleCoreToValue();
        }
        #endregion

        #region METHODS

        /// <summary>
        /// Will calculate to value based on from and will also recalculate sample name if needed
        /// </summary>
        /// <returns></returns>
        public async Task CalculateSampleCoreToValue()
        {
            //Recalculate "To" value
            Model.SampleCoreTo = Model.SampleCoreFrom + Model.SampleCoreLength / 100;

            //Modify sample name if needed
            if (CustomSampleNameEnabled)
            {
                Model.SampleName = await idCalculator.CalculateSampleAliasAsync(_earthmaterial.EarthMatID, _earthmaterial.EarthMatName, Model.SampleCoreFrom.ToString());
            }

            OnPropertyChanged(nameof(Model));
        }

        /// <summary>
        /// Initialize all pickers. 
        /// To save loading time, process only those needed based on work type
        /// </summary>
        /// <returns></returns>
        public async Task FillPickers()
        {
            _sampleType = await FillAPicker(FieldSampleType);
            _samplePurpose = await FillAPicker(FieldSamplePurpose);
            OnPropertyChanged(nameof(SampleType));
            OnPropertyChanged(nameof(SamplePurpose));

            //Bedrock pickers
            if (Preferences.ContainsKey(nameof(FieldUserInfoFWorkType))
                && Preferences.Get(nameof(FieldUserInfoFWorkType), "").ToString().Contains(ApplicationThemeBedrock))
            {
                _sampleCorePortion = await FillAPicker(FieldSampleCoreSize);
                _sampleFormat = await FillAPicker(FieldSampleFormat);
                _sampleSurface = await FillAPicker(FieldSampleSurface);
                OnPropertyChanged(nameof(SampleCorePortion));
                OnPropertyChanged(nameof(SampleFormat));
                OnPropertyChanged(nameof(SampleSurface));
            }

            //Surficial pickers
            if (Preferences.ContainsKey(nameof(FieldUserInfoFWorkType))
                && Preferences.Get(nameof(FieldUserInfoFWorkType), "").ToString() == ApplicationThemeSurficial)
            {
                _sampleQuality = await FillAPicker(FieldSampleQuality);
                _sampleState = await FillAPicker(FieldSampleState);
                _sampleHorizon = await FillAPicker(FieldSampleHorizon);
                OnPropertyChanged(nameof(SampleQuality));
                OnPropertyChanged(nameof(SampleState));
                OnPropertyChanged(nameof(SampleHorizon));
            }

        }

        /// <summary>
        /// Will fill the project type combobox
        /// </summary>
        private async Task<ComboBox> FillAPicker(string fieldName)
        {
            //Make sure to user default database rather then the prefered one. This one will always be there.
            return await da.GetComboboxListWithVocabAsync(TableSample, fieldName);

        }

        /// <summary>
        /// Will fill out missing fields for model. Default auto-calculated values
        /// Done before actually saving
        /// </summary>
        private async Task SetModelAsync()
        {

            #region Process pickers
            if (SamplePurposeCollection != null && SamplePurposeCollection.Count > 0)
            {
                Model.SamplePurpose = ConcatenatedCombobox.PipeValues(SamplePurposeCollection); //process list of values so they are concatenated.
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
            if (_earthmaterial != null)
            {
                // if coming from station notes, calculate new alias
                Model.SampleEarthmatID = _earthmaterial.EarthMatID;
                Model.SampleName = await idCalculator.CalculateSampleAliasAsync(_earthmaterial.EarthMatID, _earthmaterial.EarthMatName);
            }
            else if (Model.SampleEarthmatID != null)
            {
                // if coming from field notes on a record edit that needs to be saved as a new record with stay/save
                List<Earthmaterial> parentAlias = await DataAccess.DbConnection.Table<Earthmaterial>().Where(e => e.EarthMatID == Model.SampleEarthmatID).ToListAsync();
                Model.SampleName = await idCalculator.CalculateSampleAliasAsync(Model.SampleEarthmatID, parentAlias.First().EarthMatName);
            }

            Model.SampleID = 0;

        }

        /// <summary>
        /// Will refill the form with existing values for update/editing purposes
        /// </summary>
        /// <returns></returns>
        public async Task Load()
        {

            if (_sample != null && _sample.SampleName != string.Empty)
            {
                //Set model like actual record
                _model = _sample;

                //Refresh
                OnPropertyChanged(nameof(Model));

                #region Pickers
                //Select values in pickers
                List<string> bfs = ConcatenatedCombobox.UnpipeString(_sample.SamplePurpose);
                _purposeCollection.Clear(); //Clear any possible values first
                foreach (ComboBoxItem cbox in SamplePurpose.cboxItems)
                {
                    if (bfs.Contains(cbox.itemValue) && !_purposeCollection.Contains(cbox))
                    {
                        _purposeCollection.Add(cbox);
                    }
                }
                OnPropertyChanged(nameof(SamplePurpose));

                #endregion

                //Validate paleomag controls visibility
                ValidateForPaleomagnetism();

            }
        }

        /// <summary>
        /// Will show a quick reminder to take a duplicate for
        /// surficial geologist every 15 samples.
        /// </summary>
        public async Task<bool> DuplicateReminder()
        {
            bool shouldShowReminder = false;

            if (Preferences.ContainsKey(nameof(FieldUserInfoFWorkType))
                && Preferences.Get(nameof(FieldUserInfoFWorkType), "").ToString() == ApplicationThemeSurficial)
            {
                Sample sampleModel = new Sample();
                int sampleCount = await da.GetTableCount(Model.GetType());

                if (sampleCount % 15 == 0 && sampleCount != 0)
                {
                    shouldShowReminder = true;
                }
            }

            if (shouldShowReminder)
            {
                await Shell.Current.DisplayAlert(LocalizationResourceManager["SamplePageDuplicateReminderTitle"].ToString(),
                        LocalizationResourceManager["SamplePageDuplicateReminderMessage"].ToString(),
                        LocalizationResourceManager["GenericButtonOk"].ToString());
            }

            return shouldShowReminder;

        }

        /// <summary>
        /// If user has selected sample type of oriented, with paleomagnetism purpose, 
        /// within a surficial field book. Then enable oriented fields from bedrock
        /// </summary>
        public void ValidateForPaleomagnetism(bool forceDeactivate = false)
        {
            #region validate paleomagnetism

            //Validate for oriented samples type and paleomagnetism. This should trigger view on Oriented set of inputs
            if (FieldThemes.SurficialVisibility 
                && SelectedSamplePurpose.itemValue == samplePurposePaleomag
                && Model.SampleType == sampleTypeOriented)
            {
                FieldThemes.BedrockOrientedSampleVisibility = true;
                OnPropertyChanged(nameof(FieldThemes));
            }
            else if (FieldThemes.SurficialVisibility
                && (SelectedSamplePurpose.itemValue != samplePurposePaleomag
                || Model.SampleType != sampleTypeOriented))
            {
                FieldThemes.BedrockOrientedSampleVisibility = false;
                OnPropertyChanged(nameof(FieldThemes));
            }

            //Validate within purposes list
            if (FieldThemes.SurficialVisibility
                && Model.SampleType == sampleTypeOriented
                && SelectedSamplePurpose.itemValue == String.Empty
                && SamplePurposeCollection.Count > 0)
            {
                foreach (Controls.ComboBoxItem cbi in SamplePurposeCollection)
                {
                    if (cbi.itemValue.Contains(samplePurposePaleomag))
                    {
                        FieldThemes.BedrockOrientedSampleVisibility = true;
                        OnPropertyChanged(nameof(FieldThemes));
                    }
                }
            }

            //If needed, force deactivation of the whole header.
            if (forceDeactivate)
            {
                FieldThemes.BedrockOrientedSampleVisibility = true;
                OnPropertyChanged(nameof(FieldThemes));
            }

            #endregion
        }

        /// <summary>
        /// Will initialize the model with needed calculated fields
        /// </summary>
        /// <returns></returns>
        public async Task InitModel()
        {
            if (Model != null && Model.SampleID == 0 && _earthmaterial != null)
            {
                //Get current application version
                Model.SampleEarthmatID = _earthmaterial.EarthMatID;
                Model.SampleName = await idCalculator.CalculateSampleAliasAsync(_earthmaterial.EarthMatID, _earthmaterial.EarthMatName);

                OnPropertyChanged(nameof(Model));
            }
        }

        #endregion
    }
}
