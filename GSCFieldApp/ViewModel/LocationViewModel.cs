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
    [QueryProperty(nameof(FieldLocation), nameof(FieldLocation))]
    public partial class LocationViewModel: FieldAppPageHelper
    {
        #region INIT

        private FieldLocation _model = new FieldLocation();

        //UI
        private ComboBox _locationGeodeticSystem = new ComboBox();

        #endregion

        #region PROPERTIES

        [ObservableProperty]
        private FieldLocation _fieldLocation;

        public FieldLocation Model { get { return _model; } set { _model = value; } }

        public bool LocationDatumVisibility
        {
            get { return Preferences.Get(nameof(LocationDatumVisibility), true); }
            set { Preferences.Set(nameof(LocationDatumVisibility), value); }
        }

        public bool LocationGeographicVisibility
        {
            get { return Preferences.Get(nameof(LocationGeographicVisibility), true); }
            set { Preferences.Set(nameof(LocationGeographicVisibility), value); }
        }

        public bool LocationProjectedVisibility
        {
            get { return Preferences.Get(nameof(LocationProjectedVisibility), true); }
            set { Preferences.Set(nameof(LocationProjectedVisibility), value); }
        }

        public bool LocationGeneralVisibility
        {
            get { return Preferences.Get(nameof(LocationGeneralVisibility), true); }
            set { Preferences.Set(nameof(LocationGeneralVisibility), value); }
        }

        public ComboBox LocationGeodeticSystem { get { return _locationGeodeticSystem; } set { _locationGeodeticSystem = value; } }

        #endregion

        #region RELAYS

        [RelayCommand]
        public async Task Back()
        {
            //Make sure to delete station and location records if user is coming from map page
            if (_fieldLocation != null && _fieldLocation.LocationAlias != string.Empty && _model.LocationID != 0)
            {
                //Delete without forced pop-up warning and question
                await commandServ.DeleteDatabaseItemCommand(TableNames.location, _fieldLocation.LocationAlias, _fieldLocation.LocationID, true);

                //Exit on map
                await Shell.Current.GoToAsync("../");
            }
            else
            {
                //Exit in field notes
                await NavigateToFieldNotes(TableNames.location);
            }

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
            await NavigateToFieldNotes(TableNames.location);
        }


        /// <summary>
        /// Save button command
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        async Task SaveStay()
        {
            //Display a warning to user
            await Shell.Current.DisplayAlert(LocalizationResourceManager["DisplayAlertNotAllowed"].ToString(),
                LocalizationResourceManager["DisplayAlertNotAllowedContent"].ToString(),
                LocalizationResourceManager["GenericButtonOk"].ToString());

        }

        [RelayCommand]
        async Task SaveDelete()
        {
            if (_fieldLocation != null && _fieldLocation.LocationAlias != string.Empty && _model.LocationID != 0)
            {
                await commandServ.DeleteDatabaseItemCommand(TableNames.location, _fieldLocation.LocationAlias, _fieldLocation.LocationID);
            }

            //Exit
            await NavigateToFieldNotes(TableNames.location);

        }

        [RelayCommand]
        async Task AddStation()
        {
            if (_fieldLocation != null)
            {
                await SetAndSaveModelAsync();

                //Navigate to station page and keep locationmodel for relationnal link
                await Shell.Current.GoToAsync($"{nameof(StationPage)}/",
                    new Dictionary<string, object>
                    {
                        [nameof(FieldLocation)] = _fieldLocation,
                        [nameof(Metadata)] = null,
                        [nameof(Station)] = null,
                    }
                );
            }
        }


        [RelayCommand]
        async Task AddDrill()
        { 
        
        }
        #endregion

        public LocationViewModel() { }

        #region METHODS

        public async Task SetAndSaveModelAsync()
        {

            //Validate if new entry or update
            if (_fieldLocation != null && _fieldLocation.LocationAlias != string.Empty && _model.LocationID != 0)
            {
                await da.SaveItemAsync(Model, true);
            }

            //Close to be sure
            await da.CloseConnectionAsync();
        }

        /// <summary>
        /// Will refill the form with existing values for update/editing purposes
        /// </summary>
        /// <returns></returns>
        public async Task Load()
        {
            if (_fieldLocation != null && _fieldLocation.LocationTableName != string.Empty)
            {
                //Set model like actual record
                _model = _fieldLocation;

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

            _locationGeodeticSystem = await FillAPicker(FieldLocationDatum);
            OnPropertyChanged(nameof(LocationGeodeticSystem));

        }


        /// <summary>
        /// Generic method to fill a needed picker control with vocabulary
        /// </summary>
        private async Task<ComboBox> FillAPicker(string fieldName, string extraField = "")
        {
            //Make sure to user default database rather then the prefered one. This one will always be there.
            return await da.GetComboboxListWithVocabAsync(TableLocation, fieldName, extraField);

        }

        #endregion

    }
}
