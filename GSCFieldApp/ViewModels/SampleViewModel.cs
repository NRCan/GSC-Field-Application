using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;
using GSCFieldApp.Models;
using GSCFieldApp.Services.DatabaseServices;
using Windows.UI.Xaml.Controls;
using System.Collections.ObjectModel;

namespace GSCFieldApp.ViewModels
{
    public class SampleViewModel: ViewModelBase
    {
        #region INITIALIZATION

        //UI default values
        
        private string _sampleAlias = string.Empty;
        private string _sampleID = string.Empty;
        private string _sampleEartmatID = string.Empty;

        private string _sampleNote = string.Empty;
        private string _sampleAzim = string.Empty; //Default
        private string _sampleDip = string.Empty;//Default

        //UI interaction
        public bool doSampleUpdate = false;

        private ObservableCollection<Themes.ComboBoxItem> _sampleType = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedSampleType = string.Empty;

        private ObservableCollection<Themes.ComboBoxItem> _samplePurpose = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedSamplePurpose = string.Empty;

        private ObservableCollection<Themes.ComboBoxItem> _purposeValues = new ObservableCollection<Themes.ComboBoxItem>();

        private ObservableCollection<Themes.ComboBoxItem> _sampleFormat = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedSampleFormat = string.Empty;

        private ObservableCollection<Themes.ComboBoxItem> _sampleSurface = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedSampleSurface = string.Empty;

        private ObservableCollection<Themes.ComboBoxItem> _sampleQuality = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedSampleQuality = string.Empty;

        //Model init
        private Sample sampleModel = new Sample();
        public DataIDCalculation sampleIDCalculator = new DataIDCalculation();
        public FieldNotes existingDataDetailSample;
        DataAccess accessData = new DataAccess();

        //Events and delegate
        public delegate void sampleEditEventHandler(object sender); //A delegate for execution events
        public event sampleEditEventHandler newSampleEdit; //This event is triggered when a save has been done on station table. 

        #endregion

        #region PROPERTIES

        public Sample SampleModel { get { return sampleModel; } set { sampleModel = value; } }
        public string SampleAlias { get { return _sampleAlias; } set { _sampleAlias = value; } }

        public string SampleNote { get { return _sampleNote; } set { _sampleNote = value; } }
        public string SampleID { get { return _sampleID; } set { _sampleID = value; } }
        public string SampleEarthmatID { get { return _sampleEartmatID; } set { _sampleEartmatID = value; } }

        public ObservableCollection<Themes.ComboBoxItem> SampleType { get { return _sampleType; } set { _sampleType = value; } }
        public string SelectedSampleType { get { return _selectedSampleType; } set { _selectedSampleType = value; } }

        public ObservableCollection<Themes.ComboBoxItem> SamplePurpose { get { return _samplePurpose; } set { _samplePurpose = value; } }
        public string SelectedSamplePurpose { get { return _selectedSamplePurpose; } set { _selectedSamplePurpose = value; } }

        public ObservableCollection<Themes.ComboBoxItem> PurposeValues { get { return _purposeValues; } set { _purposeValues = value; } }

        public ObservableCollection<Themes.ComboBoxItem> SampleFormat { get { return _sampleFormat; } set { _sampleFormat = value; } }
        public string SelectedSampleFormat { get { return _selectedSampleFormat; } set { _selectedSampleFormat = value; } }

        public ObservableCollection<Themes.ComboBoxItem> SampleSurface { get { return _sampleSurface; } set { _sampleSurface = value; } }
        public string SelectedSampleSurface { get { return _selectedSampleSurface; } set { _selectedSampleSurface = value; } }

        public ObservableCollection<Themes.ComboBoxItem> SampleQuality { get { return _sampleQuality; } set { _sampleQuality = value; } }
        public string SelectedSampleQuality{ get { return _selectedSampleQuality; } set { _selectedSampleQuality = value; } }

        public string SampleAzim
        {
            get
            {
                return _sampleAzim;
            }
            set
            {
                int index;
                bool result = int.TryParse(value, out index);

                if (result)
                {
                    if (index >= 0 && index < 360)
                    {
                        _sampleAzim = value;
                    }
                    else
                    {
                        _sampleAzim = value = "0";
                        RaisePropertyChanged("SampleAzim");
                    }

                }
                else
                {
                    _sampleAzim = value = "0";
                    RaisePropertyChanged("SampleAzim");
                }


            }
        }

        public string SampleDip
        {
            get
            {
                return _sampleDip;
            }
            set
            {
                int index;
                bool result = int.TryParse(value, out index);

                if (result)
                {
                    if (index >= 0 && index <= 90)
                    {
                        _sampleDip = value;
                    }
                    else
                    {
                        _sampleDip = value = "0";
                        RaisePropertyChanged("SampleDip");
                    }

                }
                else
                {
                    _sampleDip = value = "0";
                    RaisePropertyChanged("SampleDip");
                }


            }
        }

        #endregion

        //Main
        public SampleViewModel(FieldNotes inDetailModel)
        {
            //On init for new samples calculates values for default UI form
            _sampleEartmatID = inDetailModel.GenericID;
            _sampleID = sampleIDCalculator.CalculateSampleID();
            _sampleAlias = sampleIDCalculator.CalculateSampleAlias(_sampleEartmatID, inDetailModel.earthmat.EarthMatName);

            FillSamplePurpose();
            FillSampleType();
            FillSurface();
            FillFormat();
            FillQuality();
        }

        /// <summary>
        /// Will fill the dialog with existing information coming from the database.
        /// </summary>
        /// <param name="incomingData">The model in which the existing information is stored.</param>
        public void AutoFillDialog(FieldNotes incomingData)
        {
            //Keep
            existingDataDetailSample = incomingData;

            //Set
            _sampleID = existingDataDetailSample.sample.SampleID;
            _sampleNote = existingDataDetailSample.sample.SampleNotes;
            _sampleEartmatID = existingDataDetailSample.ParentID;
            _sampleAlias = existingDataDetailSample.sample.SampleName;
            _selectedSampleType = existingDataDetailSample.sample.SampleType;
            _selectedSampleSurface = existingDataDetailSample.sample.SampleSurface;
            _selectedSampleQuality = existingDataDetailSample.sample.SampleQuality;
            _selectedSampleFormat = existingDataDetailSample.sample.SampleFormat;
            _sampleAzim = existingDataDetailSample.sample.SampleAzim;
            _sampleDip = existingDataDetailSample.sample.SampleDiplunge;

            //Update UI
            RaisePropertyChanged("SampleID");
            RaisePropertyChanged("SampleAlias");
            RaisePropertyChanged("SelectedSampleType");
            RaisePropertyChanged("SampleNote");
            RaisePropertyChanged("SampleDip");
            RaisePropertyChanged("SampleAzim");
            RaisePropertyChanged("SelectedSampleFormat");
            RaisePropertyChanged("SelectedSampleQuality");
            RaisePropertyChanged("SelectedSampleSurface"); 

            //Update list view
            UnPipePurposes(existingDataDetailSample.sample.SamplePurpose);

            doSampleUpdate = true;
        }

        /// <summary>
        /// On save event
        /// </summary>
        public void SaveDialogInfo()
        {
            //Get current class information and add to model
            sampleModel.SampleID = _sampleID; //Prime key
            sampleModel.SampleName = _sampleAlias; //Foreign key
            sampleModel.SampleNotes = _sampleNote;
            sampleModel.SampleEarthmatID = _sampleEartmatID;
            sampleModel.SamplePurpose = PipePurposes(); //process list of values so they are concatenated.
            SampleModel.SampleAzim = _sampleAzim;
            SampleModel.SampleDiplunge = _sampleDip;

            if (SelectedSampleType != null)
            {
                sampleModel.SampleType = SelectedSampleType;
            }
            if (SelectedSampleFormat != null)
            {
                sampleModel.SampleFormat = SelectedSampleFormat;
            }
            if (SelectedSampleSurface != null)
            {
                sampleModel.SampleSurface = SelectedSampleSurface;
            }
            if (SelectedSampleQuality != null)
            {
                sampleModel.SampleQuality = SelectedSampleQuality;
            }

            //Save model class
            accessData.SaveFromSQLTableObject(sampleModel, doSampleUpdate);

            //Launch an event call for everyone that an earthmat has been edited.
            if (newSampleEdit != null)
            {
                newSampleEdit(this);
            }
        }

        #region FILL

        /// <summary>
        /// Will fill the sample type combobox
        /// </summary>
        private void FillSampleType()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldSampleType;
            string tableName = Dictionaries.DatabaseLiterals.TableSample;
            foreach (var itemType in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedSampleType))
            {
                _sampleType.Add(itemType);
            }

            //Update UI
            RaisePropertyChanged("SampleType");
            RaisePropertyChanged("SelectedSampleType"); 
        }

        /// <summary>
        /// Will fill the sample type combobox
        /// </summary>
        private void FillSurface()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldSampleSurface;
            string tableName = Dictionaries.DatabaseLiterals.TableSample;
            foreach (var itemType in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedSampleSurface))
            {
                _sampleSurface.Add(itemType);
            }

            //Update UI
            RaisePropertyChanged("SampleSurface");
            RaisePropertyChanged("SelectedSampleSurface"); 
        }
        /// <summary>
        /// Will fill the sample type combobox
        /// </summary>
        private void FillFormat()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldSampleFormat;
            string tableName = Dictionaries.DatabaseLiterals.TableSample;
            foreach (var itemType in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedSampleFormat))
            {
                _sampleFormat.Add(itemType);
            }

            //Update UI
            RaisePropertyChanged("SampleFormat");
            RaisePropertyChanged("SelectedSampleFormat");
        }
        /// <summary>
        /// Will fill the sample type combobox
        /// </summary>
        private void FillQuality()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldSampleQuality;
            string tableName = Dictionaries.DatabaseLiterals.TableSample;
            foreach (var itemType in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedSampleQuality))
            {
                _sampleQuality.Add(itemType);
            }

            //Update UI
            RaisePropertyChanged("SampleQuality");
            RaisePropertyChanged("SelectedSampleQuality"); 
        }

        /// <summary>
        /// Will fill the sample purpose combobox
        /// </summary>
        private void FillSamplePurpose()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldSamplePurpose;
            string tableName = Dictionaries.DatabaseLiterals.TableSample;
            foreach (var itemPurpose in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedSamplePurpose))
            {
                _samplePurpose.Add(itemPurpose);
            }
            

            //Update UI
            RaisePropertyChanged("SamplePurpose");
            RaisePropertyChanged("SelectedSamplePurpose");

        }
        #endregion

        /// <summary>
        /// Will refresh the concatenated part of the purpose whenever a value is selected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SamplePurposeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AddAPurpose(SelectedSamplePurpose);
        }

        /// <summary>
        /// Force a cascade delete if user get's out of sample dialog while in quick sample mode.
        /// </summary>
        /// <param name="inParentModel"></param>
        public void DeleteCascadeOnQuickSample(FieldNotes inParentModel)
        {
            //Get the location id
            Station stationModel = new Station();
            List<object> stationTableLRaw = accessData.ReadTable(stationModel.GetType(), null);
            IEnumerable<Station> stationTable = stationTableLRaw.Cast<Station>(); //Cast to proper list type
            IEnumerable<string> stats = from s in stationTable where s.StationID == inParentModel.ParentID select s.LocationID;
            List<string> locationFromStat = stats.ToList();

            //Delete location
            accessData.DeleteRecord(Dictionaries.DatabaseLiterals.TableLocation, Dictionaries.DatabaseLiterals.FieldLocationID, locationFromStat[0]);
        }

        /// <summary>
        /// Will remove a purpose from purpose list
        /// </summary>
        /// <param name="inPurpose"></param>
        public void RemoveSelectedPurpose(object inPurpose)
        {

            Themes.ComboBoxItem oldPurp = inPurpose as Themes.ComboBoxItem;
            _purposeValues.Remove(oldPurp);

            RaisePropertyChanged("PurposeValues");
        }

        /// <summary>
        /// Will add to the list of purposes a selected purpose by the user.
        /// </summary>
        public void AddAPurpose(string purposeToAdd)
        {
            #region NEW METHOD
            Themes.ComboBoxItem newPurp = new Themes.ComboBoxItem();
            newPurp.itemValue = purposeToAdd;
            foreach (Themes.ComboBoxItem cb in SamplePurpose)
            {
                if (cb.itemValue == purposeToAdd)
                {
                    newPurp.itemName = cb.itemName;
                    break;
                }
            }
            if (newPurp.itemName != null && newPurp.itemValue != string.Empty)
            {
                bool foundValue = false;
                foreach (Themes.ComboBoxItem existingItems in _purposeValues)
                {
                    if (purposeToAdd == existingItems.itemName)
                    {
                        foundValue = true;
                    }
                }
                if (!foundValue)
                {
                    _purposeValues.Add(newPurp);
                }
                
            }
            #endregion
            RaisePropertyChanged("PurposeValues");
        }

        /// <summary>
        /// Will take all values from purpose value list and pipe them as a string
        /// to be able to save them all in the database
        /// </summary>
        /// <returns></returns>
        public string PipePurposes()
        {
            string _samplePurposeConcat = string.Empty;

            foreach (Themes.ComboBoxItem purposes in PurposeValues)
            {
                if (_samplePurposeConcat == string.Empty)
                {
                    _samplePurposeConcat = purposes.itemValue;
                }
                else
                {
                    _samplePurposeConcat = _samplePurposeConcat + Dictionaries.DatabaseLiterals.KeywordConcatCharacter + purposes.itemValue;
                }
            }

            return _samplePurposeConcat;
        }

        public void UnPipePurposes(string inPurpose)
        {
            List<string> purposesUnpiped = inPurpose.Split(Dictionaries.DatabaseLiterals.KeywordConcatCharacter.Trim().ToCharArray()).ToList();

            //Clean values first
            _purposeValues.Clear();

            foreach (string pu in purposesUnpiped)
            {
                AddAPurpose(pu.Trim());
            }
        }
    }
}
