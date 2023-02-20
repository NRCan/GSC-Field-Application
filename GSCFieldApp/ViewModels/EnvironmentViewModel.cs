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
using GSCFieldApp.Themes;

namespace GSCFieldApp.ViewModels
{
    public class EnvironmentViewModel : ViewModelBase
    {
        #region INITIALIZATION
        private EnvironmentModel environmentModel = new EnvironmentModel();
        private int _environmentid = 0;
        private string _environmentAlias = string.Empty;
        private string _envirommentParentID = string.Empty;
        public bool doEnvironmentUpdate = false;
        public string _notes = string.Empty;
        private string _slope = "0"; //Default
        private string _azim = "0"; //Default
        private string _depth = "0"; //Default

        private ObservableCollection<Themes.ComboBoxItem> _environmentRelief = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedEnvironmentRelief = string.Empty;

        private ObservableCollection<Themes.ComboBoxItem> _environmentBoulder = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedEnvironmentBoulder = string.Empty;

        private ObservableCollection<Themes.ComboBoxItem> _environmentDrainage = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedEnvironmentDrainage = string.Empty;

        private ObservableCollection<Themes.ComboBoxItem> _environmentPermafrost = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedEnvironmentPermafrost = string.Empty;

        private ObservableCollection<Themes.ComboBoxItem> _environmentExposureType = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedEnvironmentExposureType = string.Empty;

        private ObservableCollection<Themes.ComboBoxItem> _environmentGroundCover = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedEnvironmentGroundCover = string.Empty;

        private ObservableCollection<Themes.ComboBoxItem> _environmentGroundIce = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedEnvironmentGroundIce= string.Empty;

        private ObservableCollection<Themes.ComboBoxItem> _environmentGroundPattern = new ObservableCollection<Themes.ComboBoxItem>();
        private ObservableCollection<Themes.ComboBoxItem> _environmentGroundPatternValues = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedEnvironmentGroundPattern = string.Empty;

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
            _envirommentParentID = inReportModel.station.StationID.ToString();

            //Fill controls
            FillRelief();
            FillBoulder();
            FillDrainage();
            FillPermafrost();
            FillExposureType();
            FillGroundCover();
            FillGroundIce();
            FillGroundPattern();

            _environmentAlias = idCalculator.CalculateEnvironmentAlias(inReportModel.station.StationID,inReportModel.station.StationAlias);

        }

        #endregion

        #region PROPERTIES
        public int EnvironmentID { get { return _environmentid; } set { _environmentid = value; } }
        public string Alias { get { return _environmentAlias; } set { _environmentAlias = value; } }
        public string Notes { get { return _notes; } set { _notes = value; } }
        public string Slope 
        {
            get
            {
                return _slope;
            }
            set
            {
                bool result = int.TryParse(value, out int index);

                if (result)
                {
                    if (index >= 0 && index <= 90)
                    {
                        _slope = value;
                    }
                    else
                    {
                        _slope = value = "0";
                        RaisePropertyChanged("Slope");
                    }

                }
                else
                {
                    _slope = value = "0";
                    RaisePropertyChanged("Slope");
                }


            }
        }
        public string Azim
        {
            get
            {
                return _azim;
            }
            set
            {
                bool result = int.TryParse(value, out int index);

                if (result)
                {
                    if (index >= 0 && index <= 90)
                    {
                        _azim = value;
                    }
                    else
                    {
                        _azim = value = "0";
                        RaisePropertyChanged("Azim");
                    }

                }
                else
                {
                    _azim = value = "0";
                    RaisePropertyChanged("Azim");
                }


            }
        }
        public string Depth
        {
            get
            {
                return _depth;
            }
            set
            {
                bool result = double.TryParse(value, out double dep);

                if (result)
                {
                    
                    _depth = value;
                    RaisePropertyChanged("Depth");

                }
                else
                {
                    _depth = value = "0";
                    RaisePropertyChanged("Depth");
                }


            }
        }
        public ObservableCollection<Themes.ComboBoxItem> EnvironmentRelief { get { return _environmentRelief; } set { _environmentRelief = value; } }
        public string SelectedEnvironmentRelief { get { return _selectedEnvironmentRelief; } set { _selectedEnvironmentRelief = value; } }

        public ObservableCollection<Themes.ComboBoxItem> EnvironmentBoulder { get { return _environmentBoulder; } set { _environmentBoulder = value; } }
        public string SelectedEnvironmentBoulder { get { return _selectedEnvironmentBoulder; } set { _selectedEnvironmentBoulder = value; } }

        public ObservableCollection<Themes.ComboBoxItem> EnvironmentDrainage { get { return _environmentDrainage; } set { _environmentDrainage = value; } }
        public string SelectedEnvironmentDrainage { get { return _selectedEnvironmentDrainage; } set { _selectedEnvironmentDrainage = value; } }
        public ObservableCollection<Themes.ComboBoxItem> EnvironmentPermafrost{ get { return _environmentPermafrost; } set { _environmentPermafrost = value; } }
        public string SelectedEnvironmentPermafrost { get { return _selectedEnvironmentPermafrost; } set { _selectedEnvironmentPermafrost = value; } }
        public ObservableCollection<Themes.ComboBoxItem> EnvironmentExposureType { get { return _environmentExposureType; } set { _environmentExposureType = value; } }
        public string SelectedEnvironmentExposureType { get { return _selectedEnvironmentExposureType; } set { _selectedEnvironmentExposureType = value; } }
        public ObservableCollection<Themes.ComboBoxItem> EnvironmentGroundCover{ get { return _environmentGroundCover; } set { _environmentGroundCover = value; } }
        public string SelectedEnvironmentGroundCover { get { return _selectedEnvironmentGroundCover; } set { _selectedEnvironmentGroundCover = value; } }
        public ObservableCollection<Themes.ComboBoxItem> EnvironmentGroundIce { get { return _environmentGroundIce; } set { _environmentGroundIce = value; } }
        public string SelectedEnvironmentGroundIce { get { return _selectedEnvironmentGroundIce; } set { _selectedEnvironmentGroundIce = value; } }

        public ObservableCollection<Themes.ComboBoxItem> EnvironmentGroundPattern { get { return _environmentGroundPattern; } set { _environmentGroundPattern = value; } }
        public ObservableCollection<Themes.ComboBoxItem> EnvironmentGroundPatternValues { get { return _environmentGroundPatternValues; } set { _environmentGroundPatternValues = value; } }
        public string SelectedEnvironmentGroundPattern { get { return _selectedEnvironmentGroundPattern; } set { _selectedEnvironmentGroundPattern = value; } }

        #endregion

        #region METHODS

        public void SaveDialogInfo()
        {
            //Save the new station
            environmentModel.EnvID = _environmentid; //Prime key
            environmentModel.EnvStationID = int.Parse(_envirommentParentID); //Foreign key
            environmentModel.EnvName = _environmentAlias;
            environmentModel.EnvNotes = _notes;
            environmentModel.EnvSlope = int.Parse(_slope);
            environmentModel.EnvAzim = int.Parse(_azim);
            environmentModel.EnvActiveLayerDepth = double.Parse(_depth);

            if (SelectedEnvironmentRelief != null)
            {
                environmentModel.EnvRelief = SelectedEnvironmentRelief;
            }

            if (SelectedEnvironmentBoulder != null)
            {
                environmentModel.EnvBoulder = SelectedEnvironmentBoulder;
            }

            if (SelectedEnvironmentDrainage != null)
            {
                environmentModel.EnvDrainage = SelectedEnvironmentDrainage;
            }

            if (SelectedEnvironmentPermafrost != null)
            {
                environmentModel.EnvPermIndicator = SelectedEnvironmentPermafrost;
            }

            if (SelectedEnvironmentExposureType != null)
            {
                environmentModel.EnvExposure = SelectedEnvironmentExposureType;
            }
            if (SelectedEnvironmentGroundCover != null)
            {
                environmentModel.EnvGroundCover = SelectedEnvironmentGroundCover;
            }
            if (SelectedEnvironmentGroundIce != null)
            {
                environmentModel.EnvGroundIce = SelectedEnvironmentGroundIce;
            }

            //process list of values so they are concatenated.
            ConcatenatedCombobox ccBox = new ConcatenatedCombobox();
            environmentModel.EnvGroundPattern = ccBox.PipeValues(_environmentGroundPatternValues);

            accessData.SaveFromSQLTableObject(environmentModel, doEnvironmentUpdate);

            if (newEnvironmentEdit != null)
            {
                newEnvironmentEdit(this);
            }

        }
        #endregion

        #region FILL
        public void AutoFillDialog(FieldNotes incomingData)
        {
            existingDataDetail = incomingData;

            _environmentid = existingDataDetail.environment.EnvID;
            _environmentAlias = existingDataDetail.environment.EnvName;
            _notes = existingDataDetail.environment.EnvNotes;
            _slope = existingDataDetail.environment.EnvSlope.ToString();
            _azim = existingDataDetail.environment.EnvAzim.ToString();
            _depth = existingDataDetail.environment.EnvActiveLayerDepth.ToString();

            _selectedEnvironmentRelief = existingDataDetail.environment.EnvRelief;
            _selectedEnvironmentBoulder = existingDataDetail.environment.EnvBoulder;
            _selectedEnvironmentDrainage = existingDataDetail.environment.EnvDrainage;
            _selectedEnvironmentPermafrost = existingDataDetail.environment.EnvPermIndicator;
            _selectedEnvironmentExposureType = existingDataDetail.environment.EnvExposure;
            _selectedEnvironmentGroundCover = existingDataDetail.environment.EnvGroundCover;
            _selectedEnvironmentGroundIce = existingDataDetail.environment.EnvGroundIce;

            RaisePropertyChanged("Notes");
            RaisePropertyChanged("Alias");
            RaisePropertyChanged("Slope");
            RaisePropertyChanged("Azim");
            RaisePropertyChanged("Depth");

            RaisePropertyChanged("SelectedEnvironmentRelief");
            RaisePropertyChanged("SelectedEnvironmentBoulder");
            RaisePropertyChanged("SelectedEnvironmentDrainage");
            RaisePropertyChanged("SelectedEnvironmentPermafrost");
            RaisePropertyChanged("SelectedEnvironmentExposureType");
            RaisePropertyChanged("SelectedEnvironmentGroundCover");
            RaisePropertyChanged("SelectedEnvironmentGroundIce");

            //Update list view
            ConcatenatedCombobox ccBox = new ConcatenatedCombobox();
            foreach (string d in ccBox.UnpipeString(existingDataDetail.environment.EnvGroundPattern))
            {
                AddAConcatenatedValue(d, DatabaseLiterals.FieldEnvGroundPattern);
            }

            doEnvironmentUpdate = true;

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

        /// <summary>
        /// Will fill the station outcrop type combobox
        /// </summary>
        private void FillBoulder()
        {

            //Fill vocab list
            string fieldName = Dictionaries.DatabaseLiterals.FieldEnvBoulder;
            string tableName = Dictionaries.DatabaseLiterals.TableEnvironment;
            foreach (var itemST in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedEnvironmentBoulder))
            {
                _environmentBoulder.Add(itemST);
            }

            //Update UI
            RaisePropertyChanged("EnvironmentBoulder");
            RaisePropertyChanged("SelectedEnvironmentBoulder");

        }


        /// <summary>
        /// Will fill the station outcrop type combobox
        /// </summary>
        private void FillDrainage()
        {

            //Fill vocab list
            string fieldName = Dictionaries.DatabaseLiterals.FieldEnvDrainage;
            string tableName = Dictionaries.DatabaseLiterals.TableEnvironment;
            foreach (var itemST in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedEnvironmentDrainage))
            {
                _environmentDrainage.Add(itemST);
            }

            //Update UI
            RaisePropertyChanged("EnvironmentDrainage");
            RaisePropertyChanged("SelectedEnvironmentDrainage");

        }

        /// <summary>
        /// Will fill the station outcrop type combobox
        /// </summary>
        private void FillPermafrost()
        {

            //Fill vocab list
            string fieldName = Dictionaries.DatabaseLiterals.FieldEnvPermIndicator;
            string tableName = Dictionaries.DatabaseLiterals.TableEnvironment;
            foreach (var itemST in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedEnvironmentPermafrost))
            {
                _environmentPermafrost.Add(itemST);
            }

            //Update UI
            RaisePropertyChanged("EnvironmentPermafrost");
            RaisePropertyChanged("SelectedEnvironmentPermafrost");

        }

        /// <summary>
        /// Will fill the station outcrop type combobox
        /// </summary>
        private void FillExposureType()
        {

            //Fill vocab list
            string fieldName = Dictionaries.DatabaseLiterals.FieldEnvExposure;
            string tableName = Dictionaries.DatabaseLiterals.TableEnvironment;
            foreach (var itemST in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedEnvironmentExposureType))
            {
                _environmentExposureType.Add(itemST);
            }

            //Update UI
            RaisePropertyChanged("EnvironmentExposureType");
            RaisePropertyChanged("SelectedEnvironmentExposureType");

        }

        /// <summary>
        /// Will fill the station outcrop type combobox
        /// </summary>
        private void FillGroundCover()
        {

            //Fill vocab list
            string fieldName = Dictionaries.DatabaseLiterals.FieldEnvGroundCover;
            string tableName = Dictionaries.DatabaseLiterals.TableEnvironment;
            foreach (var itemST in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedEnvironmentGroundCover))
            {
                _environmentGroundCover.Add(itemST);
            }

            //Update UI
            RaisePropertyChanged("EnvironmentGroundCover");
            RaisePropertyChanged("SelectedEnvironmentGroundCover");

        }

        /// <summary>
        /// Will fill the station outcrop type combobox
        /// </summary>
        private void FillGroundIce()
        {

            //Fill vocab list
            string fieldName = Dictionaries.DatabaseLiterals.FieldEnvGroundIce;
            string tableName = Dictionaries.DatabaseLiterals.TableEnvironment;
            foreach (var itemST in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedEnvironmentGroundIce))
            {
                _environmentGroundIce.Add(itemST);
            }

            //Update UI
            RaisePropertyChanged("EnvironmentGroundIce");
            RaisePropertyChanged("SelectedEnvironmentGroundIce");

        }

        /// <summary>
        /// Will fill the station outcrop type combobox
        /// </summary>
        private void FillGroundPattern()
        {

            //Fill vocab list
            string fieldName = Dictionaries.DatabaseLiterals.FieldEnvGroundPattern;
            string tableName = Dictionaries.DatabaseLiterals.TableEnvironment;
            foreach (var itemST in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedEnvironmentGroundPattern))
            {
                _environmentGroundPattern.Add(itemST);
            }

            //Update UI
            RaisePropertyChanged("EnvironmentGroundPattern");
            RaisePropertyChanged("SelectedEnvironmentGroundPattern");

        }

        #endregion

        #region CONCATENATED FIELDS

        /// <summary>
        /// Will remove a purpose from purpose list
        /// </summary>
        /// <param name="inPurpose"></param>
        public void RemoveSelectedValue(object inPurpose, string parentListViewName)
        {

            Themes.ComboBoxItem oldValue = inPurpose as Themes.ComboBoxItem;

            if (parentListViewName.ToLower().Contains(Dictionaries.DatabaseLiterals.FieldEnvGroundPattern.ToLower()))
            {
                _environmentGroundPatternValues.Remove(oldValue);
                RaisePropertyChanged("EnvironmentGroundPatternValues");
            }

        }

        /// <summary>
        /// Will refresh the concatenated part of the purpose whenever a value is selected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ConcatComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox senderBox = sender as ComboBox;
            if (senderBox.SelectedValue != null)
            {
                AddAConcatenatedValue(senderBox.SelectedValue.ToString(), senderBox.Name);
            }

        }

        /// <summary>
        /// Will add to the list of purposes a selected purpose by the user.
        /// </summary>
        /// <param name="fieldName"> Optional, database table field name to know which collection to update</param>
        /// <param name="parentComboboxName">Optional, parent combobox name in which a selected value will be appended to the list</param>
        public void AddAConcatenatedValue(string valueToAdd, string parentComboboxName = null, string fieldName = null, bool canRemove = true)
        {
            if (valueToAdd != null && valueToAdd != String.Empty)
            {
                //Create new cbox item
                Themes.ComboBoxItem newValue = new Themes.ComboBoxItem
                {
                    itemValue = valueToAdd
                };

                //Set visibility
                if (canRemove)
                {
                    newValue.canRemoveItem = Windows.UI.Xaml.Visibility.Visible;
                }
                else
                {
                    newValue.canRemoveItem = Windows.UI.Xaml.Visibility.Collapsed;
                }


                #region Find parent collection
                ObservableCollection<Themes.ComboBoxItem> parentCollection = new ObservableCollection<Themes.ComboBoxItem>();
                ObservableCollection<Themes.ComboBoxItem> parentConcatCollection = new ObservableCollection<Themes.ComboBoxItem>();
                List<Themes.ComboBoxItem> parentList = new List<Themes.ComboBoxItem>();

                string parentProperty = string.Empty;

                string NameToValidate = string.Empty;
                if (parentComboboxName != null)
                {
                    NameToValidate = parentComboboxName;
                }
                if (fieldName != null)
                {
                    NameToValidate = fieldName;
                }

                if (NameToValidate.ToLower().Contains(Dictionaries.DatabaseLiterals.FieldEnvGroundPattern.ToLower()))
                {
                    parentCollection = EnvironmentGroundPattern;
                    parentConcatCollection = _environmentGroundPatternValues;
                    parentProperty = "EnvironmentGroundPattern";
                }

                #endregion


                //Find itemName from itemValue in parent collection
                if (parentCollection != null)
                {
                    foreach (Themes.ComboBoxItem cb in parentCollection)
                    {
                        if (cb.itemValue == valueToAdd || cb.itemName == valueToAdd)
                        {
                            newValue.itemName = cb.itemName;
                            newValue.itemValue = cb.itemValue;
                            break;
                        }
                    }
                }

                //Update collection
                if (newValue.itemName != null && newValue.itemName != string.Empty && newValue.itemName != Dictionaries.DatabaseLiterals.picklistNADescription)
                {
                    bool foundValue = false;
                    foreach (Themes.ComboBoxItem existingItems in parentConcatCollection)
                    {
                        if (valueToAdd == existingItems.itemName)
                        {
                            foundValue = true;
                        }
                    }
                    if (!foundValue)
                    {
                        parentConcatCollection.Add(newValue);
                        RaisePropertyChanged(parentProperty);
                    }

                }
            }
        }

        #endregion

    }
}
