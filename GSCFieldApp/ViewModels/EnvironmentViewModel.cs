using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Template10.Mvvm;
using GSCFieldApp.Models;
using GSCFieldApp.Services.DatabaseServices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using GSCFieldApp.Dictionaries;
using System.Runtime.ConstrainedExecution;

namespace GSCFieldApp.ViewModels
{
    public class EnvironmentViewModel : ViewModelBase
    {
        #region INITIALIZATION
        private EnvironmentModel environmentModel = new EnvironmentModel();
        private string _environmentid = string.Empty;
        private string _environmentAlias = string.Empty;
        private string _envirommentParentID = string.Empty;
        public bool doEnvironmentUpdate = false;
        public string _notes = string.Empty;

        private ObservableCollection<Themes.ComboBoxItem> _environmentRelief = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedEnvironmentRelief = string.Empty;

        public FieldNotes existingDataDetail;

        public DataIDCalculation idCalculator = new DataIDCalculation();

        //Events and delegate
        public delegate void environmentEditEventHandler(object sender); //A delegate for execution events
        public event environmentEditEventHandler newEnvironmentEdit; //This event is triggered when a save has been done on station table.

        readonly DataLocalSettings localSetting = new DataLocalSettings();
        readonly DataAccess accessData = new DataAccess();

        public EnvironmentViewModel(FieldNotes inReportModel)
        {
            existingDataDetail = inReportModel;

            //On init for new stations calculate values so UI shows stuff.
            _environmentid = idCalculator.CalculateEnvironmentID();
            _envirommentParentID = inReportModel.station.StationID;

            //Fill controls
            FillRelief();

            _environmentAlias = idCalculator.CalculateEnvironmentAlias(inReportModel.station.StationID,inReportModel.station.StationAlias);

        }

        #endregion

        #region PROPERTIES
        public string EnvironmentID { get { return _environmentid; } set { _environmentid = value; } }
        public string Alias { get { return _environmentAlias; } set { _environmentAlias = value; } }
        public string Notes { get { return _notes; } set { _notes = value; } }
 
        public ObservableCollection<Themes.ComboBoxItem> EnvironmentRelief { get { return _environmentRelief; } set { _environmentRelief = value; } }
        public string SelectedEnvironmentRelief { get { return _selectedEnvironmentRelief; } set { _selectedEnvironmentRelief = value; } }

        #endregion

        #region METHODS

        public void SaveDialogInfo()
        {
            bool doEnvironmentUpdate = false;
            //Themes.ConcatenatedCombobox concat = new Themes.ConcatenatedCombobox();

            //Save the new station
            environmentModel.EnvID = _environmentid; //Prime key
            environmentModel.EnvStationID = _envirommentParentID; //Foreign key
            environmentModel.EnvName = _environmentAlias;
            environmentModel.EnvNotes = _notes;

            if (SelectedEnvironmentRelief != null)
            {
                environmentModel.EnvRelief = SelectedEnvironmentRelief;
            }

            accessData.SaveFromSQLTableObject(environmentModel, doEnvironmentUpdate);

            if (newEnvironmentEdit != null)
            {
                newEnvironmentEdit(this);
            }

        }

        public void AutoFillDialog(FieldNotes incomingData)
        {
            existingDataDetail = incomingData;


            _environmentAlias = existingDataDetail.environment.EnvName;
            _notes = existingDataDetail.environment.EnvNotes;
            _selectedEnvironmentRelief = existingDataDetail.environment.EnvRelief;

            RaisePropertyChanged("Notes");
            RaisePropertyChanged("Alias");

            RaisePropertyChanged("SelectedEnvironmentRelief");

            ////Concatenated box
            //Themes.ConcatenatedCombobox ccBox = new Themes.ConcatenatedCombobox();
            //foreach (string s in ccBox.UnpipeString(existingDataDetail.station.StationOCQuality))
            //{
            //    AddAConcatenatedValue(s, DatabaseLiterals.FieldStationOCQuality);
            //}

        }

        /// <summary>
        /// Will fill the station outcrop type combobox
        /// </summary>
        private void FillRelief()
        {

            //Fill vocab list
            string fieldName = Dictionaries.DatabaseLiterals.FieldEnvRelief;
            string tableName = Dictionaries.DatabaseLiterals.TableEnvironment;
            foreach (var itemST in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedEnvironmentRelief))
            {
                _environmentRelief.Add(itemST);
            }

            //Update UI
            RaisePropertyChanged("EnvironmentRelief");
            RaisePropertyChanged("SelectedEnvironmentRelief");

        }


        #endregion

    }
}
