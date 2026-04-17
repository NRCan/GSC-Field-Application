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

        public bool DrillHoleVisible
        {
            get { return Preferences.Get(nameof(DrillHoleVisible), true); }
            set { Preferences.Set(nameof(DrillHoleVisible), value); }
        }

        #endregion

        #region RELAYS

        [RelayCommand]
        public async Task Back()
        {

            if (_model != null)
            {
                await commandServ.DeleteDatabaseItemCommand(TableNames.location, _model.LocationAlias, _model.LocationID, true);
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
            //Save
            object savedModel = await SetAndSaveModelAsync();

            //Exit
            if (savedModel != null)
            {
                await NavigateAfterAction(TableNames.location);
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

            if (_model != null)
            {
                await commandServ.DeleteDatabaseItemCommand(TableNames.location, _model.LocationAlias, _model.LocationID);
            }


            //Exit
            await NavigateAfterAction(TableNames.location);

        }

        [RelayCommand]
        async Task AddStation()
        {
            if (_fieldLocation != null)
            {
                object savedModel = await SetAndSaveModelAsync();

                if (savedModel != null)
                {
                    //Navigate to station page and keep locationmodel for relationnal link
                    await Shell.Current.GoToAsync($"/{nameof(StationPage)}/",
                        new Dictionary<string, object>
                        {
                            [nameof(FieldLocation)] = Model,
                            [nameof(Metadata)] = null,
                            [nameof(Station)] = null
                        }
                    );
                }

            }
        }

        [RelayCommand]
        async Task AddDrill()
        {
            if (_fieldLocation != null)
            {
                object savedModel = await SetAndSaveModelAsync();

                if (savedModel != null)
                {
                    //Navigate to station page and keep locationmodel for relationnal link
                    await Shell.Current.GoToAsync($"/{nameof(DrillHolePage)}/",
                        new Dictionary<string, object>
                        {
                            [nameof(DrillHole)] = null,
                            [nameof(FieldLocation)] = Model
                        }
                    );
                }
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
                                _model.LocationEPSGProj = _model.LocationDatum;
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
                    _ = Shell.Current.DisplayAlert(projectionErrorTitle, projectionErrorContent, LocalizationResourceManager["GenericButtonOk"].ToString());
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

        public async Task<object> SetAndSaveModelAsync()
        {
            //Validation
            object savedObject = null;

            //Make sure any projected coordinates are also converted back to geographic else nullified
            if (_model.LocationEasting != null && _model.LocationNorthing != null && _model.LocationEasting != 0.0 && _model.LocationNorthing != 0.0)
            {
                //Keep original projected datum (geographic and projected being in the same picker)
                _model.LocationEPSGProj = _model.LocationDatum;

                await ConvertToGeographic();
            }
            else
            {
                _model.LocationEPSGProj = null;
                _model.LocationEasting = null;
                _model.LocationNorthing = null;
            }

            //Make sure datum is properly reset to default
            _model.LocationDatum = KeywordEPSGDefault.ToString();

            //Validate error measurement on manual entries
            if (_model.isManualEntry)
            {
                _model.LocationErrorMeasure = null;
            }

            //Make sure the geometry matches
            GeopackageService gs = new GeopackageService();
            _model.LocationGeometry = gs.CreateByteGeometryPoint(_model.LocationLong, _model.LocationLat);
            RefreshGeometry(TableNames.location, Model); //Send signal that there is an updated geometry

            //Update record
            if (Model.LocationAlias != null && _model.LocationID != 0)
            {
                //Quick validation if record has children or not, this will change the alias naming scheme
                string tempAlias = await idCalculator.CalculateTempLocationAliasAsync(_model.LocationID);
                if (tempAlias != string.Empty)
                {
                    _model.LocationAlias = tempAlias;
                }
                savedObject = await da.SaveItemAsync(Model, true);
                RefreshFieldNotes(TableNames.location, Model, refreshType.update);
            }

            return savedObject;
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

                //Use case - Datum picker manages 2 field - projected location needs to change datum picker, else it'll always default to geographic 
                if (_model.LocationEPSGProj != null && _model.LocationEPSGProj != string.Empty)
                {
                    List<ComboBoxItem> proj = _locationGeodeticSystem.cboxItems.Where(d => d.itemValue == _model.LocationEPSGProj).ToList();
                    if (proj != null && proj.Count() > 0)
                    {
                        int projectedDatumIndex = _locationGeodeticSystem.cboxItems.IndexOf(proj[0]);
                        _locationGeodeticSystem.cboxDefaultItemIndex = projectedDatumIndex;
                        OnPropertyChanged(nameof(LocationGeodeticSystem));
                    }
                    
                }

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
