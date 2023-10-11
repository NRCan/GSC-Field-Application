using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;
using GSCFieldApp.Models;
using GSCFieldApp.Services.DatabaseServices;
using System.Text.RegularExpressions;
using Windows.UI.Xaml.Controls;
using GSCFieldApp.Dictionaries;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml;
using GSCFieldApp.Themes;
using System.Collections.ObjectModel;

namespace GSCFieldApp.ViewModels
{
    public class DrillHoleViewModel: ViewModelBase
    {
        #region INITIALIZATION

        private DrillHole dhModel = new DrillHole();
        public FieldNotes existingDataDetail;
        readonly DataAccess accessData = new DataAccess();
        readonly DataLocalSettings localSetting = new DataLocalSettings();
        public bool doDrillHoleUpdate = false;
        public DataIDCalculation idCalculator = new DataIDCalculation();

        //UI
        private int _drillID = 0; //Meant for update purposes, not insert
        private string _notes = string.Empty;
        private string _name = string.Empty;
        private string _companyName = string.Empty;
        private string _drillAzim = string.Empty; 
        private string _drillDip = string.Empty;
        private string _drillDepth = string.Empty;
        private string _drillLogBy = string.Empty;
        private string _drillLogIntervals = string.Empty;
        private string _drillLogSummary = string.Empty;

        private DateTime _drillDate = DateTime.Today;

        private ObservableCollection<Themes.ComboBoxItem> _drillType = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedDrillType = string.Empty;

        private ObservableCollection<Themes.ComboBoxItem> _drillUnit = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedDrillUnit = string.Empty;

        private ObservableCollection<Themes.ComboBoxItem> _drillHoleSize = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedDrillHoleSize = string.Empty;

        private ObservableCollection<Themes.ComboBoxItem> _drillCoreSize = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedDrillCoreSize = string.Empty;

        private ObservableCollection<Themes.ComboBoxItem> _drillLogType = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedDrillLogType = string.Empty;

        public DrillHoleViewModel(FieldNotes inReportModel) 
        {
            existingDataDetail = inReportModel;

            //Fill controls
            FillDrillTypes();
            FillDrillUnits();
            FillDrillHoleSize();
            FillDrillCoreSize();
            FillDrillLogType();
            FillLogBy();

        }

        #endregion

        #region PROPERTIES
        public string Notes { get { return _notes; } set { _notes = value; } }
        public string Name { get { return _name; } set { _name = value; } }
        public string CompanyName { get { return _companyName; } set { _companyName = value; } }
        public string DrillLogBy { get { return _drillLogBy; } set { _drillLogBy = value; } }
        public string DrillLogIntervals { get { return _drillLogIntervals; } set { _drillLogIntervals = value; } }
        public string DrillLogSummary { get { return _drillLogSummary; } set { _drillLogSummary = value; } }

        public DateTime DrillDate { get { return _drillDate.Date; } set { _drillDate = value.Date; } }
        public ObservableCollection<Themes.ComboBoxItem> DrillType { get { return _drillType; } set { _drillType = value; } }
        public string SelectedDrillType { get { return _selectedDrillType; } set { _selectedDrillType = value; } }
        public ObservableCollection<Themes.ComboBoxItem> DrillUnit { get { return _drillUnit; } set { _drillUnit = value; } }
        public string SelectedDrillUnit { get { return _selectedDrillUnit; } set { _selectedDrillUnit = value; } }

        public ObservableCollection<Themes.ComboBoxItem> DrillHoleSize { get { return _drillHoleSize; } set { _drillHoleSize = value; } }
        public string SelectedDrillHoleSize { get { return _selectedDrillHoleSize; } set { _selectedDrillHoleSize = value; } }

        public ObservableCollection<Themes.ComboBoxItem> DrillCoreSize { get { return _drillCoreSize; } set { _drillCoreSize = value; } }
        public string SelectedDrillCoreSize { get { return _selectedDrillCoreSize; } set { _selectedDrillCoreSize = value; } }

        public ObservableCollection<Themes.ComboBoxItem> DrillLogType { get { return _drillLogType; } set { _drillLogType = value; } }
        public string SelectedDrillLogType { get { return _selectedDrillLogType; } set { _selectedDrillLogType = value; } }


        public string DrillDepth { get { return _drillDepth; } set { _drillDepth = value; } }


        public string DrillAzim
        {
            get
            {
                return _drillAzim;
            }
            set
            {
                bool result = int.TryParse(value, out int index);

                if (result)
                {
                    if (index >= 0 && index < 360)
                    {
                        _drillAzim = value;
                    }
                    else
                    {
                        _drillAzim = value = "0";
                        RaisePropertyChanged("DrillAzim");
                    }

                }
                else
                {
                    _drillAzim = value = "0";
                    RaisePropertyChanged("DrillAzim");
                }


            }
        }

        public string DrillDip
        {
            get
            {
                return _drillDip;
            }
            set
            {
                bool result = int.TryParse(value, out int index);

                if (result)
                {
                    if (index >= 0 && index < 90)
                    {
                        _drillDip = value;
                    }
                    else
                    {
                        _drillDip = value = "0";
                        RaisePropertyChanged("DrillDip");
                    }

                }
                else
                {
                    _drillDip = value = "0";
                    RaisePropertyChanged("DrillDip");
                }


            }
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Will fill the drill hole types combobox
        /// </summary>
        private void FillDrillTypes()
        {

            //Fill vocab list
            string fieldName = Dictionaries.DatabaseLiterals.FieldDrillType;
            string tableName = Dictionaries.DatabaseLiterals.TableDrillHoles;
            foreach (var itemST in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedDrillType))
            {
                _drillType.Add(itemST);
            }

            //Update UI
            RaisePropertyChanged("DrillType");
            RaisePropertyChanged("SelectedDrillType"); 

        }

        /// <summary>
        /// Will fill the drill hole units combobox
        /// </summary>
        private void FillDrillUnits()
        {

            //Fill vocab list
            string fieldName = Dictionaries.DatabaseLiterals.FieldDrillUnit;
            string tableName = Dictionaries.DatabaseLiterals.TableDrillHoles;
            foreach (var itemST in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedDrillUnit))
            {
                _drillUnit.Add(itemST);
            }

            //Update UI
            RaisePropertyChanged("DrillUnit");
            RaisePropertyChanged("SelectedDrillUnit");

        }

        /// <summary>
        /// Will fill the drill hole size combobox
        /// </summary>
        private void FillDrillHoleSize()
        {

            //Fill vocab list
            string fieldName = Dictionaries.DatabaseLiterals.FieldDrillHoleSize;
            string tableName = Dictionaries.DatabaseLiterals.TableDrillHoles;
            foreach (var itemST in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedDrillHoleSize))
            {
                _drillHoleSize.Add(itemST);
            }

            //Update UI
            RaisePropertyChanged("DrillHoleSize");
            RaisePropertyChanged("SelectedDrillHoleSize");

        }

        /// <summary>
        /// Will fill the drill core size combobox
        /// </summary>
        private void FillDrillCoreSize()
        {

            //Fill vocab list
            string fieldName = Dictionaries.DatabaseLiterals.FieldDrillCoreSize;
            string tableName = Dictionaries.DatabaseLiterals.TableDrillHoles;
            foreach (var itemST in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedDrillCoreSize))
            {
                _drillCoreSize.Add(itemST);
            }

            //Update UI
            RaisePropertyChanged("DrillCoreSize");
            RaisePropertyChanged("SelectedDrillCoreSize");

        }

        /// <summary>
        /// Will fill the drill relog type combobox
        /// </summary>
        private void FillDrillLogType()
        {

            //Fill vocab list
            string fieldName = Dictionaries.DatabaseLiterals.FieldDrillRelogType;
            string tableName = Dictionaries.DatabaseLiterals.TableDrillHoles;
            foreach (var itemST in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedDrillLogType))
            {
                _drillLogType.Add(itemST);
            }

            //Update UI
            RaisePropertyChanged("DrillLogType");
            RaisePropertyChanged("SelectedDrillLogType");

        }

        /// <summary>
        /// Will fill log by with default value of fieldbook geologist
        /// </summary>
        private void FillLogBy()
        {
            if (localSetting.GetSettingValue(DatabaseLiterals.FieldUserInfoID) != null)
            {
                int currentMetaID = int.Parse(localSetting.GetSettingValue(DatabaseLiterals.FieldUserInfoID).ToString());

                List<object> metadataTableRaw = accessData.ReadTable(existingDataDetail.metadata.GetType(), null);
                IEnumerable<Metadata> metadataTable = metadataTableRaw.Cast<Metadata>(); //Cast to proper list type
                IEnumerable<Metadata> metadatas = from m in metadataTable where m.MetaID == currentMetaID select m;
                List<Metadata> metadatasList = metadatas.ToList();

                _drillLogBy = metadatasList[0].Geologist;

                //Update UI
                RaisePropertyChanged("DrillLogBy");
            }
               
        }

        /// <summary>
        /// Force a cascade delete if user get's out of drill hole dialog while in manual XY mode
        /// </summary>
        /// <param name="inParentModel"></param>
        public void DeleteCascadeOnQuickDrillHole(FieldNotes inParentModel)
        {
            //Get the location id
            FieldLocation locModel = new FieldLocation();
            List<object> locTableLRaw = accessData.ReadTable(locModel.GetType(), null);
            IEnumerable<FieldLocation> locTable = locTableLRaw.Cast<FieldLocation>(); //Cast to proper list type
            IEnumerable<int> locs = from l in locTable where l.LocationID == inParentModel.location.LocationID select l.LocationID;
            List<int> locationFromDH = locs.ToList();

            //Delete location
            accessData.DeleteRecord(Dictionaries.DatabaseLiterals.TableLocation, Dictionaries.DatabaseLiterals.FieldLocationID, locationFromDH[0]);
        }


        /// <summary>
        /// On save event
        /// </summary>
        public void SaveDialogInfoAsync()
        {
            //Save the new station
            dhModel.DrillName = _name;
            dhModel.DrillCompany = _companyName;
            dhModel.DrillRelogBy = _drillLogBy;
            dhModel.DrillNotes = _notes;
            dhModel.DrillLog = _drillLogSummary;

            dhModel.DrillDate = idCalculator.FormatDate(_drillDate);
            dhModel.DrillID = _drillID; //Prime key
            dhModel.DrillLocationID = existingDataDetail.location.LocationID; //Foreign key

            if (_drillAzim != string.Empty)
            {
                dhModel.DrillAzim = double.Parse(_drillAzim);
            }
            if (_drillDip != string.Empty)
            {
                dhModel.DrillDip = double.Parse(_drillDip);
            }
            if (_drillDepth != string.Empty)
            {
                dhModel.DrillDepth = double.Parse(_drillDepth);
            }
            
            

            //Comboboxes
            if (SelectedDrillType != null)
            {
                dhModel.DrillType = SelectedDrillType;
            }

            if (SelectedDrillUnit != null)
            {
                dhModel.DrillUnit = SelectedDrillUnit;
            }

            if (SelectedDrillHoleSize != null)
            {
                dhModel.DrillHoleSize = SelectedDrillHoleSize;
            }
            if (SelectedDrillCoreSize != null)
            {
                dhModel.DrillCoreSize = SelectedDrillCoreSize;
            }
            if (SelectedDrillLogType != null)
            {
                dhModel.DrillRelogType = SelectedDrillLogType;
            }
            object drillObject = (object)dhModel;
            accessData.SaveFromSQLTableObject(ref drillObject, doDrillHoleUpdate);
            dhModel = (DrillHole)drillObject;

            //if (newEnvironmentEdit != null)
            //{
            //    newEnvironmentEdit(this);
            //}

        }

        /// <summary>
        /// Will format drill date
        /// </summary>
        /// <returns></returns>
        public string CalculateDrillDate()
        {
            return String.Format("{0:yyyy-MM-dd}", _drillDate);
        }

        #endregion
    }
}
