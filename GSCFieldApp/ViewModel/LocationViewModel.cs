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
using Mapsui;
using Mapsui.Projections;
using ProjNet.CoordinateSystems;
using NTS = NetTopologySuite;

namespace GSCFieldApp.ViewModel
{
    [QueryProperty(nameof(FieldLocation), nameof(FieldLocation))]
    public partial class LocationViewModel: FieldAppPageHelper
    {
        #region INIT

        private FieldLocation _model = new FieldLocation();
        private GeopackageService _geopackageService = new GeopackageService();

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
            }

            //Exit
            await Shell.Current.GoToAsync($"//{nameof(FieldNotesPage)}");

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

            //Show message to take a station or a drill core.

            //Exit
            if (Model.isManualEntry)
            {
                await Shell.Current.GoToAsync($"//{nameof(MapPage)}");
            }
            else
            {
                await NavigateToFieldNotes(TableNames.location);
            }
            
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
                LocalizationResourceManager["DisplayAlertNotAllowedLocationContent"].ToString(),
                LocalizationResourceManager["GenericButtonOk"].ToString());

        }

        [RelayCommand]
        async Task SaveDelete()
        {
            if (_fieldLocation != null && _fieldLocation.LocationAlias != string.Empty && _model.LocationID != 0)
            {
                await commandServ.DeleteDatabaseItemCommand(TableNames.location, _fieldLocation.LocationAlias, _fieldLocation.LocationID);
            }

            //Exit on either map or field notes
            await Shell.Current.GoToAsync($"//{nameof(FieldNotesPage)}");

        }

        [RelayCommand]
        async Task AddStation()
        {
            if (_fieldLocation != null)
            {
                await SetAndSaveModelAsync();

                //Navigate to station page and keep locationmodel for relationnal link
                await Shell.Current.GoToAsync($"/{nameof(StationPage)}/",
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
            if (_fieldLocation != null)
            {
                await SetAndSaveModelAsync();

                //Navigate to station page and keep locationmodel for relationnal link
                await Shell.Current.GoToAsync($"/{nameof(DrillHolePage)}/",
                    new Dictionary<string, object>
                    {
                        [nameof(DrillHole)] = null,
                        [nameof(FieldLocation)] = _fieldLocation,
                    }
                );
            }


        }

        [RelayCommand]
        async Task ConvertToProjected()
        {
            //Transform
            if (_model.LocationLong != 0.0 && _model.LocationLat != 0.0)
            {
                //Bad system
                bool isSystemValid = false;

                if (_model.LocationDatum != null && _model.LocationDatum != string.Empty)
                {
                    //Detect a projected system (UTM or Yukon Alberts=3579)
                    int.TryParse(_model.LocationDatum, out int selectedEPGS);
                    if (selectedEPGS > 10000 || selectedEPGS == 3579)
                    {
                        //Build coordinate system for transformation
                        CoordinateSystem outSR = await SridReader.GetCSbyID(selectedEPGS);
                        CoordinateSystem inSR = await SridReader.GetCSbyID(KeywordEPSGDefault);

                        //Transform
                        NTS.Geometries.Point currentPoint = GeopackageService.defaultGeometryFactory.CreatePoint(new NetTopologySuite.Geometries.Coordinate(_model.LocationLong, _model.LocationLat));

                        if (currentPoint != null)
                        {
                            NTS.Geometries.Point transformedPoint = await GeopackageService.TransformPointCoordinates(currentPoint, inSR, outSR);

                            if (transformedPoint != null)
                            {
                                _model.LocationEasting = Math.Round(transformedPoint.X, 4);
                                _model.LocationNorthing = Math.Round(transformedPoint.Y, 4);

                                isSystemValid = true;
                                OnPropertyChanged(nameof(Model));
                            }
                        }


                    }

                }

                if (!isSystemValid && _model.LocationDatum != null && _model.LocationDatum != string.Empty)
                {
                    string projectionErrorTitle = LocalizationResourceManager["LocationPageGeodeticAlert"].ToString();
                    string projectionErrorContent = LocalizationResourceManager["LocationPageGeodeticAlertMessage"].ToString();
                    Shell.Current.DisplayAlert(projectionErrorTitle, projectionErrorContent, LocalizationResourceManager["GenericButtonOk"].ToString());
                }

            }
        }

        [RelayCommand]
        async Task ConvertToGeographic()
        {
            //Transform
            if (_model.LocationEasting != 0.0 && _model.LocationNorthing != 0.0)
            {
                //Bad system
                bool isSystemValid = false;

                if (_model.LocationDatum != null && _model.LocationDatum != string.Empty)
                {
                    //Build coordinate system for transformation
                    CoordinateSystem outSR = await SridReader.GetCSbyID(KeywordEPSGDefault);

                    //Detect a projected system (UTM or Yukon Alberts=3579)
                    int.TryParse(_model.LocationDatum, out int selectedEPGS);

                    //The database only has UTM NAD83 metric coordinate system, except Yukon albert
                    if (selectedEPGS > 10000 )
                    {
                        outSR = await SridReader.GetCSbyID(4617); //NAD83 geographic
                        CoordinateSystem inSR = await SridReader.GetCSbyID(selectedEPGS);

                        //Transform
                        NTS.Geometries.Point currentPoint = GeopackageService.defaultGeometryFactory.CreatePoint(
                            new NetTopologySuite.Geometries.Coordinate(_model.LocationEasting.Value, _model.LocationNorthing.Value));

                        if (currentPoint != null)
                        {
                            NTS.Geometries.Point transformedPoint = await GeopackageService.TransformPointCoordinates(currentPoint, inSR, outSR);

                            if (transformedPoint != null)
                            {
                                _model.LocationLong = Math.Round(transformedPoint.X,8);
                                _model.LocationLat = Math.Round(transformedPoint.Y,8);

                                isSystemValid = true;
                                OnPropertyChanged(nameof(Model));
                            }
                        }
                    }

                }

                if (!isSystemValid && _model.LocationDatum != null && _model.LocationDatum != string.Empty)
                {
                    string projectionErrorTitle = LocalizationResourceManager["LocationPageGeodeticAlert"].ToString();
                    string projectionErrorContent = LocalizationResourceManager["LocationPageGeodeticAlertMessage"].ToString();
                    await Shell.Current.DisplayAlert(projectionErrorTitle, projectionErrorContent, LocalizationResourceManager["GenericButtonOk"].ToString());
                }

            }
        }

        #endregion

        public LocationViewModel() { }

        #region METHODS

        public async Task SetAndSaveModelAsync()
        {
            //Make sure datum is properly set
            _model.LocationDatum = KeywordEPSGDefault.ToString();

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
