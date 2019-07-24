using GSCFieldApp.Models;
using GSCFieldApp.Services.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;

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
        private string _notes = string.Empty;
        private ObservableCollection<Themes.ComboBoxItem> _locationDatums = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedLocationDatums = string.Empty;

        //UI interaction
        public bool doLocationUpdate = false;


        //Model init
        private FieldLocation locationModel = new FieldLocation();
        public FieldNotes existingDataDetailLocation;
        DataAccess accessData = new DataAccess();
        DataLocalSettings localSetting = new DataLocalSettings();

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

        public string Notes { get { return _notes; } set { _notes = value; } }

        public ObservableCollection<Themes.ComboBoxItem> LocationDatums { get { return _locationDatums; } set { _locationDatums = value; } }
        public string SelectedLocationDatums { get { return _selectedLocationDatums; } set { _selectedLocationDatums = value; } }
        #endregion

        public LocationViewModel(FieldNotes inReport)
        {
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


            //Update UI
            RaisePropertyChanged("LocationID");
            RaisePropertyChanged("LocationAlias");
            RaisePropertyChanged("LocationLongitude");
            RaisePropertyChanged("LocationLatitude");
            RaisePropertyChanged("LocationElevation");

            doLocationUpdate = true;
        }

        /// <summary>
        /// On save event
        /// </summary>
        public void SaveDialogInfo()
        {
            //Get current class information and add to model
            locationModel.LocationID = _locationID; //Prime key
            locationModel.LocationAlias = _locationAlias;
            locationModel.LocationLat = Double.Parse(_locationLatitude); 
            locationModel.LocationLong = Double.Parse(_locationLongitude);
            locationModel.LocationElev = Double.Parse(_locationElevation);
            locationModel.MetaID = localSetting.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoID).ToString(); //Foreign key

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

    }
}
