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

        public FieldNotes existingDataDetail;
        readonly DataAccess accessData = new DataAccess();

        //UI
        private string _notes = string.Empty;
        private string _name = string.Empty;
        private string _companyName = string.Empty;
        private string _drillAzim = string.Empty; 
        private string _drillDip = string.Empty;
        private string _drillDepth = string.Empty;

        private ObservableCollection<Themes.ComboBoxItem> _drillType = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedDrillType = string.Empty;

        private ObservableCollection<Themes.ComboBoxItem> _drillUnit = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedDrillUnit = string.Empty;

        public DrillHoleViewModel(FieldNotes inReportModel) 
        {
            existingDataDetail = inReportModel;

            //Fill controls
            FillDrillTypes();
            FillDrillUnits();
        }

        #endregion

        #region PROPERTIES
        public string Notes { get { return _notes; } set { _notes = value; } }
        public string Name { get { return _name; } set { _name = value; } }
        public string CompanyName { get { return _companyName; } set { _companyName = value; } }
        public ObservableCollection<Themes.ComboBoxItem> DrillType { get { return _drillType; } set { _drillType = value; } }
        public string SelectedDrillType { get { return _selectedDrillType; } set { _selectedDrillType = value; } }
        public ObservableCollection<Themes.ComboBoxItem> DrillUnit { get { return _drillUnit; } set { _drillUnit = value; } }
        public string SelectedDrillUnit { get { return _selectedDrillUnit; } set { _selectedDrillUnit = value; } }

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


        #endregion
    }
}
