﻿using GSCFieldApp.Dictionaries;
using GSCFieldApp.Models;
using GSCFieldApp.Services.DatabaseServices;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Template10.Common;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Windows.ApplicationModel;
using Windows.Storage;

namespace GSCFieldApp.ViewModels
{
    public class FieldBookDialogViewModel : ViewModelBase
    {
        #region INIT
        private Metadata model = new Metadata();
        public FieldBookDialogViewModel selectedUserInfo = null;
        public const string userTable = Dictionaries.DatabaseLiterals.TableMetadata;
        public FieldBooks existingUserDetail;

        //UI
        private ObservableCollection<Themes.ComboBoxItem> _projectType = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedProjectType = string.Empty;
        private string _startStationNumber = "1";
        private string _projectName = string.Empty;
        //private string _projectLeader = string.Empty;
        //private string _geologist = string.Empty;
        private string _projectLeaderFN = string.Empty;
        private string _projectLeaderMN = string.Empty;
        private string _projectLeaderLN = string.Empty;
        private string _geologistFN = string.Empty;
        private string _geologistMN = string.Empty;
        private string _geologistLN = string.Empty;
        private bool _enability = true;
        private string _geologistCode = string.Empty;
        private string _activityName = string.Empty;
        private string _notes = string.Empty;

        //Local settings
        readonly DataLocalSettings localSetting = new DataLocalSettings();

        //Events and delegate
        public delegate void projectEditEventHandler(object sender); //A delegate for execution events
        public event projectEditEventHandler projectEdit; //This event is triggered when a save has been done on station table.

        #endregion

        #region PROPERTIES
        public Metadata Model { get { return model; } set { model = value; } }

        public int Id
        {
            get
            {
                if (this.model == null)
                {
                    return -1;
                }

                return this.model.MetaID;
            }

            set
            {
                if (this.model != null)
                {
                    this.model.MetaID = value;
                }
            }
        }

        public string StartStationNumber
        {
            get
            {
                return _startStationNumber;
            }
            set
            {
                bool result = int.TryParse(value, out int index);

                if (result)
                {
                    if (index >= 0 && index <= 9999)
                    {
                        _startStationNumber = value;
                    }
                    else
                    {
                        _startStationNumber = value = "1";
                        RaisePropertyChanged("StartStationNumber");
                    }

                }
                else
                {
                    _startStationNumber = value = "1";
                    RaisePropertyChanged("StartStationNumber");
                }


            }
        }

        public string ProjectName { get { return _projectName; } set { _projectName = value; } }
        //public string ProjectLeader { get { return _projectLeader; } set { _projectLeader = value; } }
        //public string Geologist { get { return _geologist; } set { _geologist = value; } }
        public string ProjectLeaderFN { get { return _projectLeaderFN; } set { _projectLeaderFN = value; } }
        public string ProjectLeaderMN { get { return _projectLeaderMN; } set { _projectLeaderMN = value; } }
        public string ProjectLeaderLN { get { return _projectLeaderLN; } set { _projectLeaderLN = value; } }
        public string GeologistFN { get { return _geologistFN; } set { _geologistFN = value; } }
        public string GeologistMN { get { return _geologistMN; } set { _geologistMN = value; } }
        public string GeologistLN { get { return _geologistLN; } set { _geologistLN = value; } }

        public string GeologistCode { get { return _geologistCode; } set { _geologistCode = value; } }

        public string ActivityName { get { return _activityName; } set { _activityName = value; } }
        public string Notes { get { return _notes; } set { _notes = value; } }
        public ObservableCollection<Themes.ComboBoxItem> ProjectType { get { return _projectType; } set { _projectType = value; } }
        public string SelectedProjectType { get { return _selectedProjectType; } set { _selectedProjectType = value; } }
        public bool Enability { get { return _enability; } set { _enability = value; } }

        #endregion

        public FieldBookDialogViewModel()
        {
            FillProjectType();
        }

        #region METHODS

        /// <summary>
        /// Will fill the project type combobox
        /// </summary>
        private void FillProjectType()
        {
            DataAccess accessData = new DataAccess();

            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldUserInfoFWorkType;
            foreach (var pType in accessData.GetComboboxListWithVocab(userTable, fieldName, out _selectedProjectType))
            {
                _projectType.Add(pType);
            }

            //Update UI
            RaisePropertyChanged("ProjectType");
            RaisePropertyChanged("SelectedProjectType");

        }

        /// <summary>
        /// Saving method
        /// </summary>
        /// <param name="inUserInfo"></param>
        /// <param name="newUserInfo"></param>
        public async void SaveMetadata()
        {
            //Get data access service
            DataAccess ad = new DataAccess();

            //Detect update or new record
            bool doUserUpdate = false;
            if (existingUserDetail != null) //New Station
            {
                doUserUpdate = true;

            }

            //Save
            if (!doUserUpdate)
            {
                //Insert new row
                object modelObject = (object)Model;
                ad.SaveFromSQLTableObject(ref modelObject, false);
                Model = (Metadata)modelObject;
                //ad.SaveFromSQLTableObject(Model, false);

                //Update settings with new selected project
                ApplicationData.Current.SignalDataChanged();

                //Ask user for new data to load
                await LoadNewDataAsync();

            }
            else
            {
                //Build connection file
                SQLiteConnection selectedProjectConnection = new SQLiteConnection(existingUserDetail.ProjectDBPath);

                //Update existing
                object modelObject = (object)Model;
                ad.SaveSQLTableObjectFromDB(ref modelObject, doUserUpdate, selectedProjectConnection);
                Model = (Metadata)modelObject;
                //ad.SaveSQLTableObjectFromDB(Model, doUserUpdate, selectedProjectConnection);

            }

            if (projectEdit != null)
            {
                projectEdit(this);
            }
        }

        /// <summary>
        /// Will ask user if he wants to load new data for the newly added field book.
        /// </summary>
        /// <returns></returns>
        public async Task LoadNewDataAsync()
        {

            //When done navigate to map page.
            bool isForNewProject = true;
            INavigationService navService = BootStrapper.Current.NavigationService;
            navService.Navigate(typeof(Views.MapPage), isForNewProject);
            await Task.CompletedTask;

        }

        /// <summary>
        /// Will init UI with default values coming from database
        /// </summary>
        /// <param name="incomingData"></param>
        /// <param name="isWaypoint"></param>
        public void AutoFillDialog(FieldBooks incomingData)
        {
            //Set detail
            existingUserDetail = incomingData;

            //Disable some vital information controls
            _enability = false;

            //Set UI values
            _projectName = existingUserDetail.metadataForProject.ProjectName;
            _selectedProjectType = existingUserDetail.metadataForProject.FieldworkType;
            _startStationNumber = existingUserDetail.metadataForProject.StationStartNumber;
            _geologistCode = existingUserDetail.metadataForProject.UserCode;

            _projectLeaderFN = existingUserDetail.metadataForProject.ProjectLeader_FN;
            _projectLeaderMN = existingUserDetail.metadataForProject.ProjectLeader_MN;
            _projectLeaderLN = existingUserDetail.metadataForProject.ProjectLeader_LN;
            _geologistFN = existingUserDetail.metadataForProject.ProjectUser_FN;
            _geologistMN = existingUserDetail.metadataForProject.ProjectUser_MN;
            _geologistLN = existingUserDetail.metadataForProject.ProjectUser_LN;

            _notes = existingUserDetail.metadataForProject.MetadataNotes;
            _activityName = existingUserDetail.metadataForProject.MetadataActivity;

            //Refresh UI
            RaisePropertyChanged("ProjectName");
            RaisePropertyChanged("GeologistCode");
            RaisePropertyChanged("Enability");

            RaisePropertyChanged("ProjectLeaderFN");
            RaisePropertyChanged("ProjectLeaderMN");
            RaisePropertyChanged("ProjectLeaderLN");
            RaisePropertyChanged("GeologistFN");
            RaisePropertyChanged("GeologistMN");
            RaisePropertyChanged("GeologistLN");
            RaisePropertyChanged("Notes");
            RaisePropertyChanged("ActivityName");
            RaisePropertyChanged("SelectedProjectType"); 

        }

        /// <summary>
        /// Will set the metadata class with current information from UI and some other that are calculated
        /// </summary>
        public async Task SetModel()
        {
            //Save the new location only if the modal dialog wasn't pop for edition
            if (existingUserDetail == null) //New Station
            {
                //Set metadata ID
                Services.DatabaseServices.DataIDCalculation newIDMethod = new Services.DatabaseServices.DataIDCalculation();
                //Model.MetaID = newIDMethod.CalculateMetadataID();

                //Get current application version
                Package currentPack = Package.Current;
                PackageId currentPackID = currentPack.Id;
                PackageVersion currentPackVersion = currentPackID.Version;
                Model.Version = String.Format("{0}.{1}.{2}.{3}", currentPackVersion.Major, currentPackVersion.Minor, currentPackVersion.Build, currentPackVersion.Revision);
                Model.IsActive = 1;
                Model.StartDate = String.Format("{0:d}", DateTime.Today);
                Model.VersionSchema = DatabaseLiterals.DBVersion.ToString();

            }
            else
            {
                Model = existingUserDetail.metadataForProject;
            }

            Model.UserCode = GeologistCode;
            Model.FieldworkType = SelectedProjectType;
            Model.ProjectName = ProjectName;

            Model.ProjectLeader_FN = ProjectLeaderFN;
            Model.ProjectLeader_MN = ProjectLeaderMN;
            Model.ProjectLeader_LN = ProjectLeaderLN;
            Model.ProjectUser_FN = GeologistFN;
            Model.ProjectUser_MN = GeologistMN;
            Model.ProjectUser_LN = GeologistLN;
            Model.MetadataNotes = Notes;
            Model.MetadataActivity = ActivityName;

            Model.StationStartNumber = StartStationNumber;

            await Task.CompletedTask;
        }


        #endregion

        #region EVENTS

        /// <summary>
        /// When user wants to save the dialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public Task<bool> SaveDialogInfoAsync()
        {

            //await SetModel();

            // Insert code or run another method to verify required fields are not null.
            if (Model != null && Model.isValid)
            {

                SaveMetadata();

                //Save in the local settings 
                Services.DatabaseServices.DataLocalSettings dLocalSet = new Services.DatabaseServices.DataLocalSettings();
                dLocalSet.SaveUserInfoInSettings(Model);

            }

            return Task.FromResult(Model.isValid);
        }

        #endregion
    }
}
