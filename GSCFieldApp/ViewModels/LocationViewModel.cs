﻿using Esri.ArcGISRuntime.Geometry;
using GSCFieldApp.Dictionaries;
using GSCFieldApp.Models;
using GSCFieldApp.Services.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Template10.Common;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Windows.ApplicationModel.Resources;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace GSCFieldApp.ViewModels
{
    public class LocationViewModel : ViewModelBase
    {
        #region INITIALIZATION

        //UI default values

        private string _locationAlias = string.Empty;
        private int _locationID = 0;

        private string _locationLatitude = "0";
        private string _locationLongitude = "0"; //Default
        private string _locationElevation = "0";//Default
        private string _locationAccuracy = "0";//Default
        private string _locationNTS = string.Empty;
        private bool _readonlyFields = true;//Default
        private string _locationNorthing = "0";
        private string _locationEasting = "0";
        private string _locationNotes = string.Empty;
        private ObservableCollection<Themes.ComboBoxItem> _locationDatums = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedLocationDatums = string.Empty;
        private string _locationtimestamp = string.Empty;

        //UI interaction
        private bool _doLocationUpdate = false;
        private bool _isDrillHoleFieldBook = false;
        public string entryType = null;


        //Model init
        public FieldLocation locationModel = new FieldLocation();
        public FieldNotes existingDataDetailLocation;
        readonly DataAccess accessData = new DataAccess();
        readonly DataLocalSettings localSetting = new DataLocalSettings();
        public DataIDCalculation idCalculator = new DataIDCalculation();
        public ResourceLoader local = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();

        //Events and delegate
        public delegate void LocationEditEventHandler(object sender); //A delegate for execution events
        public event LocationEditEventHandler newLocationEdit; //This event is triggered when a save has been done on station table. 
        public static event EventHandler LocationUpdateEventHandler; //This even is triggered when user has change coordinate so map page needs a refresh.

        #endregion

        #region PROPERTIES

        public string LocationAlias { get { return _locationAlias; } set { _locationAlias = value; } }
        public int LocationID { get { return _locationID; } set { _locationID = value; } }
        public string LocationLatitude
        {
            get { return _locationLatitude; }
            set
            {
                bool result = double.TryParse(value, out double lat);

                if (result)
                {
                    if (lat >= -90.0 && lat <= 90.0)
                    {
                        _locationLatitude = value;
                    }
                    else
                    {
                        _locationLatitude = value = "0";
                        RaisePropertyChanged("LocationLatitude");
                    }

                }
                else
                {
                    _locationLatitude = value = "0";
                    RaisePropertyChanged("LocationLatitude");
                }
            }
        }
        public string LocationLongitude
        {
            get { return _locationLongitude; }
            set
            {
                bool result = double.TryParse(value, out double longitude);

                if (result)
                {
                    if (longitude >= -180.0 && longitude <= 180.0)
                    {
                        _locationLongitude = value;
                    }
                    else
                    {
                        _locationLongitude = value = "0";
                        RaisePropertyChanged("LocationLongitude");
                    }

                }
                else
                {
                    _locationLongitude = value = "0";
                    RaisePropertyChanged("LocationLongitude");
                }
            }
        }
        public string LocationElevation { get { return _locationElevation; } set { _locationElevation = value; } }
        public string LocationNorthing { get { return _locationNorthing; } set { _locationNorthing = value; } }
        public string LocationEasting { get { return _locationEasting; } set { _locationEasting = value; } }
        public string LocationAccuracy { get { return _locationAccuracy; } set { _locationAccuracy = value; } }
        public string LocationNTS { get { return _locationNTS; } set { _locationNTS = value; } }
        public bool ReadOnlyFields { get { return _readonlyFields; } set { _readonlyFields = value; } }
        public string LocationNotes { get { return _locationNotes; } set { _locationNotes = value; } }
        public ObservableCollection<Themes.ComboBoxItem> LocationDatums { get { return _locationDatums; } set { _locationDatums = value; } }
        public string SelectedLocationDatums { get { return _selectedLocationDatums; } set { _selectedLocationDatums = value; } }

        public bool DoLocationUpdate { get { return _doLocationUpdate; } set { _doLocationUpdate = value; } }
        public bool IsDrillHoleFieldBook { get { return _isDrillHoleFieldBook; } set { _isDrillHoleFieldBook = value; } }
        public string LocationTimestamp { get { return _locationtimestamp; } set { _locationtimestamp = value; } }
        #endregion

        public LocationViewModel(FieldNotes inReport)
        {
            //On init for new stations calculate values so UI shows stuff.
            //_locationID = idCalculator.CalculateLocationID();
            _locationAlias = idCalculator.CalculateLocationAlias();

            FillDatum();

            //Check field book type to change new drill hole button
            CheckFieldBookType();
        }

        /// <summary>
        /// Method to detect field book type which will then set
        /// visible or not the add drill hole button in the header bar
        /// </summary>
        private void CheckFieldBookType()
        {
            string fieldworkType = string.Empty;
            if (localSetting.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoFWorkType) != null)
            {
                fieldworkType = localSetting.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoFWorkType).ToString();
            }

            if (fieldworkType != string.Empty && fieldworkType.Contains(DatabaseLiterals.KeywordDrill))
            {
                _isDrillHoleFieldBook = true;
            }
            else
            {
                _isDrillHoleFieldBook = false;
            }

            RaisePropertyChanged("IsDrillHoleFieldBook");
        }

        private void FillDatum()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldLocationDatum;
            string tableName = Dictionaries.DatabaseLiterals.TableLocation;
            foreach (var itemSQ in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedLocationDatums))
            {
                _locationDatums.Add(itemSQ);
            }


            //Update UI
            RaisePropertyChanged("LocationDatums");
            RaisePropertyChanged("SelectedLocationDatums");
        }

        /// <summary>
        /// Will fill the dialog with existing information coming from the database.
        /// </summary>
        /// <param name="incomingData">The model in which the existing information is stored.</param>
        public void AutoFillDialog(FieldNotes incomingData)
        {
            //Keep
            existingDataDetailLocation = incomingData;

            //Set
            _locationID = existingDataDetailLocation.location.LocationID;
            _locationAlias = existingDataDetailLocation.location.LocationAlias;
            _locationLatitude = existingDataDetailLocation.location.LocationLat.ToString();
            _locationLongitude = existingDataDetailLocation.location.LocationLong.ToString();
            _locationElevation = existingDataDetailLocation.location.LocationElev.ToString();
            _locationAccuracy = existingDataDetailLocation.location.LocationErrorMeasure.ToString();
            _locationNotes = existingDataDetailLocation.location.LocationNotes;
            _locationEasting = existingDataDetailLocation.location.LocationEasting.ToString();
            _locationNorthing = existingDataDetailLocation.location.LocationNorthing.ToString();
            _locationNTS = existingDataDetailLocation.location.locationNTS;
            _locationtimestamp = existingDataDetailLocation.location.LocationTimestamp.ToString();

            //Check for manual XY projections
            if (existingDataDetailLocation.location.LocationEPSGProj != null && existingDataDetailLocation.location.LocationEPSGProj != string.Empty)
            {
                _selectedLocationDatums = existingDataDetailLocation.location.LocationEPSGProj;
            }

            //Update UI
            RaisePropertyChanged("LocationID");
            RaisePropertyChanged("LocationAlias");
            RaisePropertyChanged("LocationLongitude");
            RaisePropertyChanged("LocationLatitude");
            RaisePropertyChanged("LocationElevation");
            RaisePropertyChanged("LocationAccuracy");
            RaisePropertyChanged("LocationNotes");
            RaisePropertyChanged("LocationEasting");
            RaisePropertyChanged("LocationNorthing");
            RaisePropertyChanged("LocationNTS");
            RaisePropertyChanged("SelectedLocationDatums");

            _doLocationUpdate = true;
            RaisePropertyChanged("DoLocationUpdate");
        }

        /// <summary>
        /// On save event
        /// </summary>
        public void SaveDialogInfo()
        {
            //Parse coordinate pairs
            double.TryParse(_locationLongitude, out double _long);
            double.TryParse(_locationLatitude, out double _lat);
            double.TryParse(_locationEasting, out double _easting);
            double.TryParse(_locationNorthing, out double _northing);
            double.TryParse(_locationAccuracy, out double _accu);

            //Detect a projected system
            int.TryParse(SelectedLocationDatums.ToString(), out int selectedEPGS);

            //Make sure that everything has been filled
            if ((_long == 0 || _lat == 0) && (_easting != 0 || _northing != 0))
            {

                //Detect Datum difference
                SpatialReference inSR = SpatialReference.Create(selectedEPGS);
                SpatialReference outSR = SpatialReferences.Wgs84; //Default
                if ((selectedEPGS > 26900 && selectedEPGS < 27000) || selectedEPGS == 4617)
                {
                    //Commented out, by default it'll always be wgs84 in lat/long #299
                    //outSR = SpatialReference.Create(4617);

                    locationModel.LocationEPSGProj = selectedEPGS.ToString();
                    locationModel.LocationDatum = DatabaseLiterals.KeywordEPSGDefault;
                }

                MapPoint geoSave = CalculateGeographicCoordinate(_easting, _northing, inSR, outSR);
                if (geoSave != null)
                {
                    LocationLongitude = geoSave.X.ToString();
                    LocationLatitude = geoSave.Y.ToString();
                    RaisePropertyChanged("LocationLongitude");
                    RaisePropertyChanged("LocationLatitude");
                }
            }

            //Get current class information and add to model
            locationModel.LocationID = LocationID; //Prime key
            locationModel.LocationAlias = LocationAlias;
            locationModel.LocationLat = _lat;
            locationModel.LocationLong = _long;
            locationModel.LocationElev = Double.Parse(LocationElevation);
            locationModel.MetaID = int.Parse(localSetting.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoID).ToString()); //Foreign key
            locationModel.LocationNotes = LocationNotes;
            locationModel.LocationErrorMeasure = _accu;
            locationModel.locationNTS = _locationNTS;

            // Timestamp only if not in update mode
            if (!_doLocationUpdate)
            {
                locationModel.LocationTimestamp = idCalculator.FormatFullDate(DateTime.Now);
            }
            else
            {
                locationModel.LocationTimestamp = _locationtimestamp;
            }

            
            if (SelectedLocationDatums != null)
            {
                if (selectedEPGS != 4326 && selectedEPGS != 4617)
                {
                    //Keep new projection EPSG
                    locationModel.LocationEPSGProj = selectedEPGS.ToString();
                    locationModel.LocationEasting = _easting;
                    locationModel.LocationNorthing = _northing;
                }

                locationModel.LocationDatum = DatabaseLiterals.KeywordEPSGDefault;
            }

            if (entryType != null)
            {
                locationModel.LocationEntryType = entryType;
            }


            //Save model class
            GeopackageService geoService = new GeopackageService();

            if (_doLocationUpdate)
            {
                object locObject = (object)locationModel;
                accessData.SaveFromSQLTableObject(ref locObject, _doLocationUpdate);
                locationModel = (FieldLocation)locObject;

                //Extra step to make sure geometry is good
                string updateQuery = accessData.GetGeopackageUpdateQuery(DatabaseLiterals.TableLocation);
                geoService.DoSpatialiteQueryInGeopackage(updateQuery, false);

            }
            else
            {
                string insertQuery = accessData.GetGeopackageInsertQuery(locationModel);
                locationModel.LocationID = geoService.DoSpatialiteQueryInGeopackage(insertQuery);
            }




            //Launch an event call for everyone that an earthmat has been edited.
            if (newLocationEdit != null)
            {
                newLocationEdit(this);
            }

            //Trigger event for map page
            EventHandler updateRequest = LocationUpdateEventHandler;
            if (updateRequest != null && _doLocationUpdate)
            {
                updateRequest(this, null);
            }

        }


        /// <summary>
        /// Will set read-only attribute of certain controls in the xaml by inversion it's value
        /// </summary>
        /// <param name="doSetTo"></param>
        public void SetReadOnlyFields(bool doSetTo = false)
        {
            if (_readonlyFields)
            {
                _readonlyFields = false;
            }
            else
            {
                _readonlyFields = true;
            }

            RaisePropertyChanged("ReadOnlyFields");
        }

        /// <summary>
        /// Will calculate a 
        /// </summary>
        /// <param name="inX"></param>
        /// <param name="inY"></param>
        /// <returns></returns>
        public MapPoint CalculateGeographicCoordinate(Double easting, Double northing, SpatialReference inSR, SpatialReference outSR)
        {
            //Variables
            MapPoint geoPoint = new MapPoint(0, 0, outSR);

            //Transform
            if (easting != 0.0 && northing != 0.0)
            {

                if (outSR != null)
                {

                    DatumTransformation datumTransfo = null;
                    if ((outSR.Wkid > 26900 && outSR.Wkid < 27000))
                    {
                        outSR = SpatialReference.Create(4617);
                    }
                    else
                    {
                        datumTransfo = TransformationCatalog.GetTransformation(inSR, outSR);
                    }

                    MapPoint proPoint = new MapPoint(easting, northing, inSR);

                    //Validate if transformation is needed.
                    if (datumTransfo != null)
                    {
                        geoPoint = (MapPoint)Esri.ArcGISRuntime.Geometry.GeometryEngine.Project(proPoint, outSR, datumTransfo);
                    }
                    else
                    {
                        geoPoint = (MapPoint)Esri.ArcGISRuntime.Geometry.GeometryEngine.Project(proPoint, outSR);
                    }

                }
            }

            return geoPoint;

        }

        /// <summary>
        /// Mainly used when user needs to navigate to the field note page after a certain steps has been taken
        /// </summary>
        /// <param name="sender"></param>
        public async void NavigateToReportAsync(object sender)
        {
            //Navigate to map page
            INavigationService navService = BootStrapper.Current.NavigationService;
            navService.Navigate(typeof(Views.FieldNotesPage));
            await Task.CompletedTask;
        }

        /// <summary>
        /// Will convert a given set of geographic coordinates into projected.
        /// Given the user has selected a proper projection
        /// </summary>
        public async void DisplayUTMCoordinatesAsync()
        {
            //XY
            double x_value = 0.0;
            double y_value = 0.0;
            if (_locationLongitude != string.Empty)
            {
                x_value = Double.Parse(_locationLongitude);
            }
            if (_locationLatitude != string.Empty)
            {
                y_value = Double.Parse(_locationLatitude);
            }

            //Transform
            if (x_value != 0.0 && y_value != 0.0)
            {
                //Bad system
                bool isSystemValid = false;

                if (_selectedLocationDatums != null)
                {
                    //Detect a projected system
                    int.TryParse(_selectedLocationDatums, out int selectedEPGS);
                    if (selectedEPGS > 10000)
                    {
                        //Detect Datum difference
                        SpatialReference inSR = SpatialReferences.Wgs84; //Default
                        if (selectedEPGS > 26900 && selectedEPGS < 27000)
                        {
                            inSR = SpatialReference.Create(4617);
                        }

                        MapPoint geoPoint = new MapPoint(x_value, y_value, inSR);
                        var outSpatialRef = SpatialReference.Create(selectedEPGS);
                        MapPoint projPoint = (MapPoint)Esri.ArcGISRuntime.Geometry.GeometryEngine.Project(geoPoint, outSpatialRef);

                        int y = (int)projPoint.Y;
                        int x = (int)projPoint.X;
                        _locationNorthing = y.ToString();
                        _locationEasting = x.ToString();



                        isSystemValid = true;
                    }

                }

                if (!isSystemValid && _selectedLocationDatums != null && _selectedLocationDatums != string.Empty)
                {
                    //Show warning to select something
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        ContentDialog defaultEventLocationDialog = new ContentDialog()
                        {
                            Title = local.GetString("LocationDialogDatumTitle"),
                            Content = local.GetString("LocationDialogDatumContent"),
                            CloseButtonText = local.GetString("GenericDialog_ButtonOK")
                        };
                        defaultEventLocationDialog.Style = (Style)Application.Current.Resources["WarningDialog"];
                        await Services.ContentDialogMaker.CreateContentDialogAsync(defaultEventLocationDialog, true);

                    }).AsTask();
                }

            }
            else
            {
                _locationNorthing = x_value.ToString();
                _locationEasting = y_value.ToString();
            }
            RaisePropertyChanged("LocationEasting");
            RaisePropertyChanged("LocationNorthing");
        }

        /// <summary>
        /// Will convert a given set of projected coordinates into geographic ones.
        /// </summary>
        public async void DisplayGeoCoordinatesAsync()
        {
            //XY
            Double x_value = 0;
            Double y_value = 0;
            if (_locationEasting != string.Empty)
            {
                x_value = Double.Parse(_locationEasting);
            }
            if (_locationNorthing != string.Empty)
            {
                y_value = Double.Parse(_locationNorthing);
            }

            //Transform
            if (x_value != 0.0 && y_value != 0.0)
            {
                //Bad system
                bool isSystemValid = false;

                if (_selectedLocationDatums != null)
                {

                    //Detect a projected system
                    int.TryParse(_selectedLocationDatums, out int selectedEPGS);

                    if (selectedEPGS > 10000)
                    {
                        //Detect Datum difference
                        SpatialReference inSR = SpatialReference.Create(selectedEPGS);
                        SpatialReference outSR = SpatialReferences.Wgs84; //Default
                        if ((selectedEPGS > 26900 && selectedEPGS < 27000) || selectedEPGS == 4617)
                        {
                            outSR = SpatialReference.Create(4617);
                        }

                        //Get geographic point
                        MapPoint geoPoint = CalculateGeographicCoordinate(x_value, y_value, inSR, outSR);

                        double y = geoPoint.Y;
                        double x = geoPoint.X;
                        _locationLatitude = y.ToString();
                        _locationLongitude = x.ToString();

                        isSystemValid = true;
                    }


                }

                if (!isSystemValid && _selectedLocationDatums != null && _selectedLocationDatums != string.Empty)
                {
                    //Show warning to select something
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        ContentDialog defaultEventLocationDialog = new ContentDialog()
                        {
                            Title = local.GetString("LocationDialogDatumTitle"),
                            Content = local.GetString("LocationDialogDatumContent"),
                            CloseButtonText = local.GetString("GenericDialog_ButtonOK")
                        };
                        defaultEventLocationDialog.Style = (Style)Application.Current.Resources["WarningDialog"];
                        await Services.ContentDialogMaker.CreateContentDialogAsync(defaultEventLocationDialog, true);

                    }).AsTask();
                }
            }
            else
            {
                _locationLongitude = x_value.ToString();
                _locationLatitude = y_value.ToString();
            }

            RaisePropertyChanged("LocationLatitude");
            RaisePropertyChanged("LocationLongitude");

        }
    }
}
