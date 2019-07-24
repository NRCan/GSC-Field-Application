﻿using Esri.ArcGISRuntime.Geometry;
using GSCFieldApp.Models;
using GSCFieldApp.Services.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Common;
using Template10.Mvvm;
using Template10.Services.NavigationService;

namespace GSCFieldApp.ViewModels
{
    public class LocationViewModel: ViewModelBase
    {
        #region INITIALIZATION

        //UI default values

        private string _locationAlias = string.Empty;
        private string _locationID = string.Empty;

        private string _locationLatitude = "0";
        private string _locationLongitude = "0"; //Default
        private string _locationElevation = "0";//Default
        private bool _readonlyFields = true;//Default
        private string _locationNorthing = "0";
        private string _locationEasting = "0";
        private string _locationNotes = string.Empty;
        private ObservableCollection<Themes.ComboBoxItem> _locationDatums = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedLocationDatums = string.Empty;

        //UI interaction
        public bool doLocationUpdate = false;
        public string entryType = null;


        //Model init
        public FieldLocation locationModel = new FieldLocation();
        public FieldNotes existingDataDetailLocation;
        DataAccess accessData = new DataAccess();
        DataLocalSettings localSetting = new DataLocalSettings();
        public DataIDCalculation idCalculator = new DataIDCalculation();

        //Events and delegate
        public delegate void LocationEditEventHandler(object sender); //A delegate for execution events
        public event LocationEditEventHandler newLocationEdit; //This event is triggered when a save has been done on station table. 

        #endregion

        #region PROPERTIES

        public string LocationAlias { get { return _locationAlias; } set { _locationAlias = value; } }
        public string LocationID { get { return _locationID; } set { _locationID = value; } }
        public string LocationLatitude {
            get { return _locationLatitude; }
            set
            {
                double lat;
                bool result = double.TryParse(value, out lat);

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
                double longitude;
                bool result = double.TryParse(value, out longitude);

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

        public bool ReadOnlyFields { get { return _readonlyFields; } set { _readonlyFields = value; } }

        public string LocationNotes { get { return _locationNotes; } set { _locationNotes = value; } }

        public ObservableCollection<Themes.ComboBoxItem> LocationDatums { get { return _locationDatums; } set { _locationDatums = value; } }
        public string SelectedLocationDatums { get { return _selectedLocationDatums; } set { _selectedLocationDatums = value; } }
        #endregion

        public LocationViewModel(FieldNotes inReport)
        {
            //On init for new stations calculate values so UI shows stuff.
            _locationID = idCalculator.CalculateLocationID();
            _locationAlias = idCalculator.CalculateLocationAlias();

            FillDatum();
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
            _locationNotes = existingDataDetailLocation.location.LocationNotes;
            _locationEasting = existingDataDetailLocation.location.LocationEasting.ToString();
            _locationNotes = existingDataDetailLocation.location.LocationNorthing.ToString();
            _selectedLocationDatums = existingDataDetailLocation.location.LocationDatum;

            //Update UI
            RaisePropertyChanged("LocationID");
            RaisePropertyChanged("LocationAlias");
            RaisePropertyChanged("LocationLongitude");
            RaisePropertyChanged("LocationLatitude");
            RaisePropertyChanged("LocationElevation");
            RaisePropertyChanged("LocationNotes");
            RaisePropertyChanged("LocationEasting");
            RaisePropertyChanged("LocationNorthing");
            RaisePropertyChanged("LocationDatums"); 

            doLocationUpdate = true;
        }

        /// <summary>
        /// On save event
        /// </summary>
        public void SaveDialogInfo()
        {
            int x_value = 0;
            int y_value = 0;

            int.TryParse(LocationEasting, out x_value);
            int.TryParse(LocationNorthing, out y_value);

            //Make sure that geographic coordinates are filled in.
            if (x_value== 0 && y_value == 0)
            {

                //Detect a projected system
                int selectedEPGS = 0;
                int.TryParse(SelectedLocationDatums.ToString(), out selectedEPGS);

                //Detect Datum difference
                SpatialReference inSR = new Esri.ArcGISRuntime.Geometry.SpatialReference(selectedEPGS);
                SpatialReference outSR = SpatialReferences.Wgs84; //Default
                if ((selectedEPGS > 26900 && selectedEPGS < 27000) || selectedEPGS == 4617)
                {
                    outSR = new Esri.ArcGISRuntime.Geometry.SpatialReference(4617);
                }

                MapPoint geoSave = CalculateGeographicCoordinate(x_value, y_value, inSR, outSR);
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
            locationModel.LocationLat = Double.Parse(LocationLatitude); 
            locationModel.LocationLong = Double.Parse(LocationLongitude);
            locationModel.LocationElev = Double.Parse(LocationElevation);
            locationModel.MetaID = localSetting.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoID).ToString(); //Foreign key
            locationModel.LocationNotes = LocationNotes;
            locationModel.LocationEasting = Double.Parse(LocationEasting);
            locationModel.LocationNorthing = Double.Parse(LocationNorthing);
            if (SelectedLocationDatums != null)
            {
                locationModel.LocationDatum = SelectedLocationDatums;
            }

            if (entryType != null)
            {
                locationModel.LocationEntryType = entryType;
            }


            //Save model class
            accessData.SaveFromSQLTableObject(locationModel, doLocationUpdate);

            //Launch an event call for everyone that an earthmat has been edited.
            if (newLocationEdit != null)
            {
                newLocationEdit(this);
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
        public MapPoint CalculateGeographicCoordinate(int easting, int northing, SpatialReference inSR, SpatialReference outSR)
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
                        outSR = new Esri.ArcGISRuntime.Geometry.SpatialReference(4617);
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
            navService.Navigate(typeof(Views.ReportPage));
            await Task.CompletedTask;
        }

    }
}
