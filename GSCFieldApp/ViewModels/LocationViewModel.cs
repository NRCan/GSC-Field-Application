using GSCFieldApp.Models;
using GSCFieldApp.Services.DatabaseServices;
using System;
using System.Collections.Generic;
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

        private string _locationLatitude = string.Empty;
        private string _locationLongitude = string.Empty; //Default
        private string _locationElevation = string.Empty;//Default

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
        public string LocationLatitude { get { return _locationLatitude; } set { _locationLatitude = value; } }
        public string LocationLongitude { get { return _locationLongitude; } set { _locationLongitude = value; } }
        public string LocationElevation { get { return _locationElevation; } set { _locationElevation = value; } }
        #endregion

        public LocationViewModel(FieldNotes inReport)
        {
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

    }
}
