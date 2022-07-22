using GSCFieldApp.Models;
using GSCFieldApp.Services.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;
using Windows.UI.Xaml.Controls;

namespace GSCFieldApp.ViewModels
{
    public class MineralViewModel: ViewModelBase
    {
        #region INITIALIZATION

        private string _groupTypeDetail = string.Empty;

        //UI default values
        private string _mineralAlias = string.Empty;
        private string _mineralID = string.Empty;
        private string _mineralEarthmatID = string.Empty;

        private string _mineralNote = string.Empty;
        private string _mineralSizeMin = string.Empty;
        private string _mineralSizeMax = string.Empty;
        private string _mineralMode = string.Empty;
        private string _mineralResidualText = string.Empty;
        private string _mineralName = string.Empty;

        private Dictionary<string, int> _mineralResidualModes = new Dictionary<string, int>(); //Will contain mineral Id and it's mode, for residual mode calculation
        private List<string> _minerals = new List<string>(); //Will contain a list of all minerals related to current parent earthmat. To catch duplicates
    
        //UI interaction
        public bool doMineralUpdate = false;

        private ObservableCollection<Themes.ComboBoxItem> _mineralColor = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedMineralColor = string.Empty;
        private ObservableCollection<Themes.ComboBoxItem> _mineralForm = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedMineralForm = string.Empty;
        private ObservableCollection<Themes.ComboBoxItem> _mineralHabit = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedMineralHabit = string.Empty;
        private ObservableCollection<Themes.ComboBoxItem> _mineralOccur = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedMineralOccur = string.Empty;
        private ObservableCollection<Themes.ComboBoxItem> _mineralModeText = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedMineralModeText = string.Empty;

        //Model init
        private Mineral mineralModel = new Mineral();
        public DataIDCalculation mineralIDCalculator = new DataIDCalculation();
        public FieldNotes existingDataDetailMineral;
        DataAccess accessData = new DataAccess();

        //Events and delegate
        public delegate void mineralEditEventHandler(object sender); //A delegate for execution events
        public event mineralEditEventHandler newMineralEdit; //This event is triggered when a save has been done on station table. 

        #endregion

        #region PROPERTIES

        public Mineral MineralModel { get { return mineralModel; } set { mineralModel = value; } }
        public string MineralAlias { get { return _mineralAlias; } set { _mineralAlias = value; } }
        public string MineralNote { get { return _mineralNote; } set { _mineralNote = value; } }
        public string MineralID { get { return _mineralID; } set { _mineralID = value; } }
        public string MineralEarthmatID { get { return _mineralEarthmatID; } set { _mineralEarthmatID = value; } }
        public string MineralResidualText { get { return _mineralResidualText; } set { _mineralResidualText = value; } }
        public string MineralName { get { return _mineralName; } set { _mineralName = value; } }
        public string MineralSizeMin
        {
            get
            {
                return _mineralSizeMin;
            }
            set
            {
                int index;
                bool result = int.TryParse(value, out index);

                if (result)
                {
                    if (index >= 0 && index <= 9999)
                    {
                        _mineralSizeMin = value;
                    }
                    else
                    {
                        _mineralSizeMin = value = "0";
                        RaisePropertyChanged("MineralSizeMin");
                    }

                }
                else
                {
                    _mineralSizeMin = value = "0";
                    RaisePropertyChanged("MineralSizeMin");
                }


            }

        }
        public string MineralSizeMax
        {
            get
            {
                return _mineralSizeMax;
            }
            set
            {
                int index;
                bool result = int.TryParse(value, out index);

                if (result)
                {
                    if (index >= 0 && index <= 9999)
                    {
                        _mineralSizeMax = value;
                    }
                    else
                    {
                        _mineralSizeMax = value = "0";
                        RaisePropertyChanged("MineralSizeMax");
                    }

                }
                else
                {
                    _mineralSizeMax = value = "0";
                    RaisePropertyChanged("MineralSizeMax");
                }


            }
        }


        public ObservableCollection<Themes.ComboBoxItem> MineralColour { get { return _mineralColor; } set { _mineralColor = value; } }
        public string SelectedMineralColor { get { return _selectedMineralColor; } set { _selectedMineralColor = value; } }
        public ObservableCollection<Themes.ComboBoxItem> MineralForm { get { return _mineralForm; } set { _mineralForm = value; } }
        public string SelectedMineralForm { get { return _selectedMineralForm; } set { _selectedMineralForm = value; } }
        public ObservableCollection<Themes.ComboBoxItem> MineralHabit{ get { return _mineralHabit; } set { _mineralHabit = value; } }
        public string SelectedMineralHabit{ get { return _selectedMineralHabit; } set { _selectedMineralHabit = value; } }
        public ObservableCollection<Themes.ComboBoxItem> MineralOccur{ get { return _mineralOccur; } set { _mineralOccur = value; } }
        public string SelectedMineralOccur { get { return _selectedMineralOccur; } set { _selectedMineralOccur = value; } }
        public ObservableCollection<Themes.ComboBoxItem> MineralModeText { get { return _mineralModeText; } set { _mineralModeText = value; } }
        public string SelectedMineralModeText { get { return _selectedMineralModeText; } set { _selectedMineralModeText = value; } }
        #endregion

        public MineralViewModel(FieldNotes inReportModel)
        {
            //On init for new samples calculates values for default UI form
            if (inReportModel.GenericTableName == Dictionaries.DatabaseLiterals.TableEarthMat)
            {
                _mineralEarthmatID = inReportModel.GenericID;
            }
            else if (inReportModel.earthmat.EarthMatID != null) //Case mineral is created from earthmat dialog
            {
                _mineralEarthmatID = inReportModel.earthmat.EarthMatID;
            }
            
            _mineralID = mineralIDCalculator.CalculateMineralID();
            _mineralAlias = mineralIDCalculator.CalculateMineralAlias(_mineralEarthmatID, inReportModel.earthmat.EarthMatName);

            existingDataDetailMineral = inReportModel;

            if (existingDataDetailMineral.GenericID != null)
            {
                CalculateResidual();
            }
            
            FillColour();
            FillForm();
            FillHabit();
            FillOccur();
            FillMode();
        }

        public void InitFill2ndRound(string fullMineralText)
        {
            _groupTypeDetail = fullMineralText;
            RaisePropertyChanged("GroupTypeDetail");
        }

        /// <summary>
        /// Will fill the dialog with existing information coming from the database.
        /// </summary>
        /// <param name="incomingData">The model in which the existing information is stored.</param>
        public void AutoFillDialog(FieldNotes incomingData)
        {
            //Keep
            existingDataDetailMineral = incomingData;

            //Set
            _mineralID = existingDataDetailMineral.mineral.MineralID;
            _mineralNote = existingDataDetailMineral.mineral.MineralNote;
            _mineralEarthmatID = existingDataDetailMineral.ParentID;
            _mineralAlias = existingDataDetailMineral.mineral.MineralIDName;
            _mineralName = existingDataDetailMineral.mineral.MineralName;

            if (existingDataDetailMineral.mineral.MineralSizeMax != null)
            {
                _mineralSizeMax = existingDataDetailMineral.mineral.MineralSizeMax.ToString();
            }
            else
            {
                _mineralSizeMax = 0.ToString();
            }

            if (existingDataDetailMineral.mineral.MineralSizeMin != null)
            {
                _mineralSizeMin = existingDataDetailMineral.mineral.MineralSizeMin.ToString();
            }
            else
            {
                _mineralSizeMin = 0.ToString();
            }

            _selectedMineralModeText = existingDataDetailMineral.mineral.MineralMode;
            _selectedMineralColor = existingDataDetailMineral.mineral.MineralColour;
            _selectedMineralForm = existingDataDetailMineral.mineral.MineralForm;
            _selectedMineralHabit = existingDataDetailMineral.mineral.MineralHabit;
            _selectedMineralOccur = existingDataDetailMineral.mineral.MineralOccur;

            //Update UI
            RaisePropertyChanged("MineralID");
            RaisePropertyChanged("MineralAlias");
            RaisePropertyChanged("MineralNote");
            RaisePropertyChanged("MineralEarthmatID");
            RaisePropertyChanged("MineralMode");
            RaisePropertyChanged("MineralSizeMin");
            RaisePropertyChanged("MineralSizeMax");
            RaisePropertyChanged("MineralName");

            RaisePropertyChanged("SelectedMineralColor");
            RaisePropertyChanged("SelectedMineralForm");
            RaisePropertyChanged("SelectedMineralHabit");
            RaisePropertyChanged("SelectedMineralOccur");

            RaisePropertyChanged("SelectedMineralModeText");

            doMineralUpdate = true;
        }

        /// <summary>
        /// On save event
        /// </summary>
        public void SaveDialogInfo()
        {
            //Get current class information and add to model
            mineralModel.MineralID = _mineralID; //Prime key
            mineralModel.MineralNote = _mineralNote;
            mineralModel.MineralParentID = _mineralEarthmatID;
            mineralModel.MineralIDName = _mineralAlias;
            mineralModel.MineralSizeMax = _mineralSizeMax;
            mineralModel.MineralSizeMin = _mineralSizeMin;
            mineralModel.MineralName = _mineralName;

            if (SelectedMineralColor != null)
            {
                mineralModel.MineralColour = SelectedMineralColor;
            }
            if (SelectedMineralForm != null)
            {
                mineralModel.MineralForm = SelectedMineralForm;
            }
            if (SelectedMineralHabit != null)
            {
                mineralModel.MineralHabit = SelectedMineralHabit;
            }
            if (SelectedMineralOccur != null)
            {
                mineralModel.MineralOccur = SelectedMineralOccur;
            }
            if (SelectedMineralModeText != null)
            {
                mineralModel.MineralMode = SelectedMineralModeText;
            }

            //Save model class
            accessData.SaveFromSQLTableObject(mineralModel, doMineralUpdate);

            //Launch an event call for everyone that an earthmat has been edited.
            if (newMineralEdit != null)
            {
                newMineralEdit(this);
            }
        }

        #region FILL

        private void FillMode()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldMineralMode;
            string tableName = Dictionaries.DatabaseLiterals.TableMineral;
            foreach (var itemType in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedMineralModeText))
            {
                _mineralModeText.Add(itemType);
            }

            //Update UI
            RaisePropertyChanged("MineralMode");
            RaisePropertyChanged("SelectedMineralModeText");
        }


        /// <summary>
        /// Will fill the mineral colour type combobox
        /// </summary>
        private void FillColour()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldMineralColour;
            string tableName = Dictionaries.DatabaseLiterals.TableMineral;
            foreach (var itemType in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedMineralColor))
            {
                _mineralColor.Add(itemType);
            }

            //Update UI
            RaisePropertyChanged("MineralColour");
            RaisePropertyChanged("SelectedMineralColor");
        }

        /// <summary>
        /// Will fill the mineral forms type combobox
        /// </summary>
        private void FillForm()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldMineralForm;
            string tableName = Dictionaries.DatabaseLiterals.TableMineral;
            foreach (var itemType in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedMineralForm))
            {
                _mineralForm.Add(itemType);
            }

            //Update UI
            RaisePropertyChanged("MineralForm");
            RaisePropertyChanged("SelectedMineralForm"); 
        }

        /// <summary>
        /// Will fill the mineral habit type combobox
        /// </summary>
        private void FillHabit()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldMineralHabit;
            string tableName = Dictionaries.DatabaseLiterals.TableMineral;
            foreach (var itemType in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedMineralHabit))
            {
                _mineralHabit.Add(itemType);
            }

            //Update UI
            RaisePropertyChanged("MineralHabit");
            RaisePropertyChanged("SelectedMineralHabit"); 
        }

        /// <summary>
        /// Will fill the mineral occurences type combobox
        /// </summary>
        private void FillOccur()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldMineralOccurence;
            string tableName = Dictionaries.DatabaseLiterals.TableMineral;
            foreach (var itemType in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedMineralOccur))
            {
                _mineralOccur.Add(itemType);
            }

            //Update UI
            RaisePropertyChanged("MineralOccur");
            RaisePropertyChanged("SelectedMineralOccur"); 
        }

        #endregion

        #region CALCULATE
        public void CalculateResidual(string newMode = "")
        {
            // Language localization using Resource.resw
            var loadLocalization = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            string Prefix = loadLocalization.GetString("MineralDialogResidualPrefix");
            string MiddleFix = loadLocalization.GetString("MineralDialogResidualMiddlefix");
            string Suffix = loadLocalization.GetString("MineralDialogResidualSuffix");

            List<object> mineralTableRaw = accessData.ReadTable(mineralModel.GetType(), null);
            IEnumerable<Mineral> mineralTable = mineralTableRaw.Cast<Mineral>(); //Cast to proper list type

            //Get a list of related mineral from selected earthmat
            string parentID = existingDataDetailMineral.ParentID;

            //Find proper parent id (request could come from a mineral or an earthmat selection)
            if (existingDataDetailMineral.ParentTableName == Dictionaries.DatabaseLiterals.TableStation)
            {
                parentID = existingDataDetailMineral.GenericID;
            }
            IEnumerable<Mineral> mineralParentEarth = from e in mineralTable where e.MineralParentID == parentID select e;

            if (_mineralResidualModes.Count == 0 && (mineralParentEarth.Count() != 0 || mineralParentEarth != null))
            {
                foreach (Mineral mns in mineralParentEarth)
                {
                    _minerals.Add(mns.MineralName);

                    int currentPercentage = 0;
                    bool currentModeParsed = int.TryParse(mns.MineralMode, out currentPercentage);

                    if (currentModeParsed)
                    {
                        if (mns.MineralID == existingDataDetailMineral.GenericID)
                        {
                            if (newMode != string.Empty)
                            {
                                currentModeParsed = int.TryParse(newMode, out currentPercentage);
                            }

                            if (currentModeParsed)
                            {
                                _mineralResidualModes[mns.MineralID] = currentPercentage;
                            }

                        }
                        else
                        {
                            if (currentModeParsed)
                            {
                                _mineralResidualModes[mns.MineralID] = currentPercentage;
                            }

                        }
                    }

                   
                }

                if (_mineralResidualModes.Count() == 0)
                {
                    int currentPercentage = 0;
                    bool currentModeParsed = int.TryParse(newMode, out currentPercentage);
                    _mineralResidualModes[existingDataDetailMineral.GenericID] = currentPercentage;
                }

            }
            else
            {
                int currentPercentage = 0;
                bool currentModeParsed = int.TryParse(newMode, out currentPercentage);
                _mineralResidualModes[existingDataDetailMineral.GenericID] = currentPercentage;
            }


            //Calculate total percentage
            int _mineralResidualMode = 0;
            foreach (KeyValuePair<string, int> modes in _mineralResidualModes)
            {
                _mineralResidualMode = _mineralResidualMode + modes.Value;
            }
            _mineralResidualText = Prefix + _mineralResidualMode.ToString() + MiddleFix + _mineralResidualModes.Count().ToString() + Suffix;
            RaisePropertyChanged("MineralResidualText");

        }
        #endregion

        #region QUICKIE

        /// <summary>
        /// Will create a quick mineral record inside mineral table, from a given parent report model and mineral name, no eartmat will be created in the process
        /// </summary>
        /// <param name="parentModel">Earhtmat parent model</param>
        /// <param name="inMineralName">The new mineral name to add inside table</param>
        /// <returns>A detail report class</returns>
        public void QuickMineralRecordOnly(FieldNotes parentModel, string inMineralName)
        {
            if (!_minerals.Contains(inMineralName))
            {
                //Get current class information and add to model
                mineralModel.MineralParentID = _mineralEarthmatID; //Foreigh key
                mineralModel.MineralIDName = _mineralAlias;
                mineralModel.MineralID = _mineralID; //Prime key
                mineralModel.MineralName = inMineralName;
                mineralModel.MineralSizeMax = 0.ToString();
                mineralModel.MineralSizeMin = 0.ToString();

                //Save model class
                accessData.SaveFromSQLTableObject(mineralModel, false);
            }
        }

        #endregion

        public void MineralModeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox senderBox = sender as ComboBox;
            if (senderBox.SelectedIndex != -1)
            {
                Themes.ComboBoxItem senderItem = senderBox.SelectedItem as Themes.ComboBoxItem;
                CalculateResidual(senderItem.itemValue);
            }
            
        }
    }
}
