using GSCFieldApp.Models;
using GSCFieldApp.Services.DatabaseServices;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Template10.Mvvm;
using Windows.UI.Xaml.Controls;

namespace GSCFieldApp.ViewModels
{
    public class PaleoflowViewModel: ViewModelBase
    {
        #region INIT DECLARATIONS

        public bool doPflowUpdate = false;

        //UI
        private ObservableCollection<Themes.ComboBoxItem> _pflowClass = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedPflowClass = string.Empty;
        private ObservableCollection<Themes.ComboBoxItem> _pflowDir = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedPflowDir = string.Empty;
        private ObservableCollection<Themes.ComboBoxItem> _pflowFeature = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedPflowFeature = string.Empty;
        private ObservableCollection<Themes.ComboBoxItem> _pflowConfidence = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedPflowConfidence = string.Empty;
        private ObservableCollection<Themes.ComboBoxItem> _pflowBedSurface = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedPflowBedSurface = string.Empty;
        private ObservableCollection<Themes.ComboBoxItem> _pflowDefined = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedPflowDefined = string.Empty;
        private ObservableCollection<Themes.ComboBoxItem> _pflowNoIndicator = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedPflowNoIndicator = string.Empty;
        private ObservableCollection<Themes.ComboBoxItem> _pflowAge = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedPflowAge = string.Empty;
        private ObservableCollection<Themes.ComboBoxItem> _pflowMethod = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedPflowMethod = string.Empty;
        private ObservableCollection<Themes.ComboBoxItem> _pflowRelative = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedPflowRelative = string.Empty;

        public string _pflowNote= string.Empty;//Default
        public string _pflowDipPlunge = string.Empty;
        public string _pflowAzim = string.Empty;
        public bool? _pflowMainDirection = false;
        public int _pflowParentID = 0;
        public int _pflowID = 0;
        public string _pflowName = string.Empty;

        //Model init
        private Paleoflow pflowModel = new Paleoflow();
        public DataIDCalculation pflowCalculator = new DataIDCalculation();
        public FieldNotes existingDataDetailPflow;
        readonly DataAccess accessData = new DataAccess();

        //Events and delegate
        public delegate void pflowEditEventHandler(object sender); //A delegate for execution events
        public event pflowEditEventHandler newPflowEdit; //This event is triggered when a save has been done on station table. 

        #endregion

        #region PROPERTIES
        public string PflowNote { get { return _pflowNote; } set { _pflowNote = value; } }
        public bool? PflowMainDirection { get { return _pflowMainDirection; } set { _pflowMainDirection = value; } }
        public int PflowParentID { get { return _pflowParentID; } set { _pflowParentID = value; } }
        public int PflowID { get { return _pflowID; } set { _pflowID = value; } }
        public string PflowName { get { return _pflowName; } set { _pflowName = value; } }
        public ObservableCollection<Themes.ComboBoxItem> PflowClass { get { return _pflowClass; } set { _pflowClass = value; } }
        public string SelectedPflowClass { get { return _selectedPflowClass; } set { _selectedPflowClass = value; } }
        public ObservableCollection<Themes.ComboBoxItem> PflowDir { get { return _pflowDir; } set { _pflowDir = value; } }
        public string SelectedPflowDir { get { return _selectedPflowDir; } set { _selectedPflowDir = value; } }
        public ObservableCollection<Themes.ComboBoxItem> PflowFeature { get { return _pflowFeature; } set { _pflowFeature = value; } }
        public string SelectedPflowFeature { get { return _selectedPflowFeature; } set { _selectedPflowFeature = value; } }

        public ObservableCollection<Themes.ComboBoxItem> PflowConfidence { get { return _pflowConfidence; } set { _pflowConfidence = value; } }
        public string SelectedPflowConfidence { get { return _selectedPflowConfidence; } set { _selectedPflowConfidence = value; } }

        public ObservableCollection<Themes.ComboBoxItem> PflowBedSurface { get { return _pflowBedSurface; } set { _pflowBedSurface = value; } }
        public string SelectedPflowBedSurface { get { return _selectedPflowBedSurface; } set { _selectedPflowBedSurface = value; } }
        public ObservableCollection<Themes.ComboBoxItem> PflowDefined { get { return _pflowDefined; } set { _pflowDefined = value; } }
        public string SelectedPflowDefined { get { return _selectedPflowDefined; } set { _selectedPflowDefined = value; } }
        public ObservableCollection<Themes.ComboBoxItem> PflowNoIndicator { get { return _pflowNoIndicator; } set { _pflowNoIndicator = value; } }
        public string SelectedPflowNoIndicator { get { return _selectedPflowNoIndicator; } set { _selectedPflowNoIndicator = value; } }
        public ObservableCollection<Themes.ComboBoxItem> PflowAge { get { return _pflowAge; } set { _pflowAge = value; } }
        public string SelectedPflowage { get { return _selectedPflowAge; } set { _selectedPflowAge = value; } }
        public ObservableCollection<Themes.ComboBoxItem> PflowMethod { get { return _pflowMethod; } set { _pflowMethod = value; } }
        public string SelectedPflowMethod { get { return _selectedPflowMethod; } set { _selectedPflowMethod = value; } }
        public ObservableCollection<Themes.ComboBoxItem> PflowRelative { get { return _pflowRelative; } set { _pflowRelative = value; } }
        public string SelectedPflowRelative { get { return _selectedPflowRelative; } set { _selectedPflowRelative = value; } }
        public string PflowAzim
        {
            get
            {
                return _pflowAzim;
            }
            set
            {
                bool result = int.TryParse(value, out int index);

                if (result)
                {
                    if (index >= 0 && index < 360)
                    {
                        _pflowAzim = value;
                    }
                    else
                    {
                        _pflowAzim = value = "0";
                        RaisePropertyChanged("PflowAzim");
                    }

                }
                else
                {
                    _pflowAzim = value = "0";
                    RaisePropertyChanged("PflowAzim");
                }


            }
        }
        public string PflowDip
        {
            get
            {
                return _pflowDipPlunge;
            }
            set
            {
                bool result = int.TryParse(value, out int index);

                if (result)
                {
                    if (index >= 0 && index <= 90)
                    {
                        _pflowDipPlunge = value;
                    }
                    else
                    {
                        _pflowDipPlunge = value = "0";
                        RaisePropertyChanged("PflowDip");
                    }

                }
                else
                {
                    _pflowDipPlunge = value = "0";
                    RaisePropertyChanged("PflowDip");
                }


            }
        }

        #endregion

        public PaleoflowViewModel(FieldNotes inReportDetail)
        {
            //On init for new samples calculates values for default UI form
            _pflowParentID = inReportDetail.GenericID;
            //_pflowID = pflowCalculator.CalculatePFlowID();
            _pflowName = pflowCalculator.CalculatePflowAlias(_pflowParentID, inReportDetail.earthmat.EarthMatName);

            FillPflowClass();
            FillPflowDirection();
            FillPflowConfidence();
            FillPflowBedrockSurface();
            FillPflowDefition();
            FillPflowIndicatorNumber();
            FillPflowRelAge();
            FillPflowRelation();
            FillPflowMethod();

            FillPflowFeature();


        }

        #region FILL
        private void FillPflowClass()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldPFlowClass;
            string tableName = Dictionaries.DatabaseLiterals.TablePFlow;
            foreach (var itemClass in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedPflowClass))
            {
                _pflowClass.Add(itemClass);
            }
            

            //Update UI
            RaisePropertyChanged("PflowClass"); 
            RaisePropertyChanged("SelectedPflowClass");

        }

        private void FillPflowDirection()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldPFlowSense;
            string tableName = Dictionaries.DatabaseLiterals.TablePFlow;
            foreach (var itemDirection in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedPflowDir))
            {
                _pflowDir.Add(itemDirection);
            }
            

            //Update UI
            RaisePropertyChanged("PflowDir");
            RaisePropertyChanged("SelectedPflowDir");

        }

        private void FillPflowFeature()
        {


            //Init.
            _pflowFeature.Clear();
            string fieldName = Dictionaries.DatabaseLiterals.FieldPFlowFeature;
            string tableName = Dictionaries.DatabaseLiterals.TablePFlow;

            #region Pflow feature
            List<Vocabularies> pflowF = new List<Vocabularies>();

            if (_selectedPflowClass != string.Empty && _selectedPflowClass != null && _selectedPflowClass != " " && _selectedPflowClass != "")
            {
                pflowF = accessData.GetPicklistValuesFromParent(tableName, fieldName, _selectedPflowClass, false).ToList();
            }
            else
            {
                pflowF = accessData.GetPicklistValuesFromParent(tableName, fieldName, "x", false).ToList(); //Make the query crash and return N.A. if nothing is available in lithotype
            }
  
            //Fill in cbox
            if (_selectedPflowFeature == null)
            {
                _selectedPflowFeature = string.Empty;
            }
            foreach (var itemFeature in accessData.GetComboboxListFromVocab(pflowF, out _selectedPflowFeature))
            {
                _pflowFeature.Add(itemFeature);
            }

            //If something already exists (from autofill) keep it, else autofill will update pflowclass that will trigger this method to redo list and empties user database value
            if (existingDataDetailPflow != null && (existingDataDetailPflow.paleoflow.PFlowFeature != null || existingDataDetailPflow.paleoflow.PFlowFeature != string.Empty))
            {
                _selectedPflowFeature = existingDataDetailPflow.paleoflow.PFlowFeature;
            }


            //Update UI
            RaisePropertyChanged("PflowFeature");
            RaisePropertyChanged("SelectedPflowFeature");

            #endregion

        }

        private void FillPflowConfidence()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldPFlowConfidence;
            string tableName = Dictionaries.DatabaseLiterals.TablePFlow;
            foreach (var itemConfidence in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedPflowConfidence))
            {
                _pflowConfidence.Add(itemConfidence);
            }
            

            //Update UI
            RaisePropertyChanged("PflowConfidence");
            RaisePropertyChanged("SelectedPflowConfidence");
        }

        private void FillPflowBedrockSurface()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldPFlowBedsurf;
            string tableName = Dictionaries.DatabaseLiterals.TablePFlow;
            foreach (var itemBedrockSurface in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedPflowBedSurface))
            {
                _pflowBedSurface.Add(itemBedrockSurface);
            }
            

            //Update UI
            RaisePropertyChanged("PflowBedSurface");
            RaisePropertyChanged("SelectedPflowBedSurface"); 

        }

        private void FillPflowDefition()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldPFlowDefinition;
            string tableName = Dictionaries.DatabaseLiterals.TablePFlow;
            foreach (var itemDefinition in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedPflowDefined))
            {
                _pflowDefined.Add(itemDefinition);
            }
            

            //Update UI
            RaisePropertyChanged("PflowDefined");
            RaisePropertyChanged("SelectedPflowDefined");
        }

        private void FillPflowIndicatorNumber()
        {
            //Init.
            DataAccess relatedPicklistAccess = new DataAccess();
            string fieldName = Dictionaries.DatabaseLiterals.FieldPFlowNumIndic;
            string tableName = Dictionaries.DatabaseLiterals.TablePFlow;
            foreach (var itemIndiNumber in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedPflowNoIndicator))
            {
                _pflowNoIndicator.Add(itemIndiNumber);
            }
           

            //Update UI
            RaisePropertyChanged("PflowNoIndicator");
            RaisePropertyChanged("SelectedPflowNoIndicator");
        }

        private void FillPflowRelAge()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldPFlowRelage;
            string tableName = Dictionaries.DatabaseLiterals.TablePFlow;
            foreach (var itemRelativeAge in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedPflowAge))
            {
                _pflowAge.Add(itemRelativeAge);
            }
            

            //Update UI
            RaisePropertyChanged("PflowAge");
            RaisePropertyChanged("SelectedPflowage");

        }

        private void FillPflowMethod()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldPFlowMethod;
            string tableName = Dictionaries.DatabaseLiterals.TablePFlow;
            foreach (var itemMethod in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedPflowMethod))
            {
                _pflowMethod.Add(itemMethod);
            }
            

            //Update UI
            RaisePropertyChanged("PflowMethod");
            RaisePropertyChanged("SelectedPflowMethod");

        }

        private void FillPflowRelation()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldPFlowRelation;
            string tableName = Dictionaries.DatabaseLiterals.TablePFlow;
            foreach (var itemRelation in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedPflowRelative))
            {
                _pflowRelative.Add(itemRelation);
            }
            

            //Update UI
            RaisePropertyChanged("PflowRelative");
            RaisePropertyChanged("SelectedPflowRelative");

        }

        #endregion

        #region METHODS

        /// <summary>
        /// Will fill the dialog with existing information coming from the database.
        /// </summary>
        /// <param name="incomingData">The model in which the existing information is stored.</param>
        public void AutoFillDialog(FieldNotes incomingData)
        {
            //Keep
            existingDataDetailPflow = incomingData;

            //Set
            _pflowID = existingDataDetailPflow.paleoflow.PFlowID;
            _pflowName = existingDataDetailPflow.paleoflow.PFlowName;
            _pflowParentID = existingDataDetailPflow.paleoflow.PFlowParentID;
            _pflowNote = existingDataDetailPflow.paleoflow.PFlowNotes;
            _pflowDipPlunge = existingDataDetailPflow.paleoflow.PFlowDip.ToString();
            _pflowAzim = existingDataDetailPflow.paleoflow.PFlowAzimuth.ToString();

            if (existingDataDetailPflow.paleoflow.PFlowMainDir == Dictionaries.DatabaseLiterals.boolYes)
            {
                _pflowMainDirection = true;
            }
            else
            {
                _pflowMainDirection = false;
            }
            

            _selectedPflowClass = existingDataDetailPflow.paleoflow.PFlowClass;
            _selectedPflowDir = existingDataDetailPflow.paleoflow.PFlowSense;
            _selectedPflowConfidence = existingDataDetailPflow.paleoflow.PFlowConfidence;
            _selectedPflowBedSurface = existingDataDetailPflow.paleoflow.PFlowBedsurf;
            _selectedPflowDefined = existingDataDetailPflow.paleoflow.PFlowDefinition;
            _selectedPflowNoIndicator = existingDataDetailPflow.paleoflow.PFlowNumIndic;
            _selectedPflowAge = existingDataDetailPflow.paleoflow.PFlowRelAge.ToString();
            _selectedPflowMethod = existingDataDetailPflow.paleoflow.PFlowMethod;
            _selectedPflowRelative = existingDataDetailPflow.paleoflow.PFlowRelation;
            _selectedPflowFeature = existingDataDetailPflow.paleoflow.PFlowFeature;

            //Update UI
            RaisePropertyChanged("PflowID"); 
            RaisePropertyChanged("PflowAge"); 
            RaisePropertyChanged("PflowParentID"); 
            RaisePropertyChanged("PflowAzim"); 
            RaisePropertyChanged("PflowNote");
            RaisePropertyChanged("PflowDip");
            RaisePropertyChanged("PflowName");
            RaisePropertyChanged("PflowMainDirection");

            //TODO Main direction parsing

            RaisePropertyChanged("SelectedPflowClass");
            RaisePropertyChanged("SelectedPflowDir");
            RaisePropertyChanged("SelectedPflowFeature"); 
            RaisePropertyChanged("SelectedPflowConfidence"); 
            RaisePropertyChanged("SelectedPflowBedSurface"); 
            RaisePropertyChanged("SelectedPflowDefined"); 
            RaisePropertyChanged("SelectedPflowNoIndicator"); 
            RaisePropertyChanged("SelectedPflowage"); 
            RaisePropertyChanged("SelectedPflowMethod"); 
            RaisePropertyChanged("SelectedPflowRelative"); 


            doPflowUpdate = true;
        }

        /// <summary>
        /// On save event
        /// </summary>
        public void SaveDialogInfo()
        {
            //Save
            pflowModel.PFlowID = _pflowID;
            pflowModel.PFlowName = _pflowName;
            pflowModel.PFlowParentID = _pflowParentID;
            pflowModel.PFlowNotes = _pflowNote;
            pflowModel.PFlowDip = int.Parse(_pflowDipPlunge);
            pflowModel.PFlowAzimuth = int.Parse(_pflowAzim);

            if (PflowMainDirection.HasValue)
            {
                if (PflowMainDirection == true)
                {
                    pflowModel.PFlowMainDir = Dictionaries.DatabaseLiterals.boolYes;
                }
                else
                {
                    pflowModel.PFlowMainDir = Dictionaries.DatabaseLiterals.boolNo;
                }
                
            }
            else
            {
                pflowModel.PFlowMainDir = Dictionaries.DatabaseLiterals.boolNo;
            }

            if (SelectedPflowFeature != null)
            {
                pflowModel.PFlowFeature = SelectedPflowFeature;
            }
            if (SelectedPflowClass != null)
            {
                pflowModel.PFlowClass = SelectedPflowClass;
            }
            if (SelectedPflowDir != null)
            {
                pflowModel.PFlowSense = SelectedPflowDir;
            }
            if (SelectedPflowConfidence != null)
            {
                pflowModel.PFlowConfidence = SelectedPflowConfidence;
            }
            if (SelectedPflowBedSurface != null)
            {
                pflowModel.PFlowBedsurf = SelectedPflowBedSurface;
            }
            if (SelectedPflowDefined != null)
            {
                pflowModel.PFlowDefinition = SelectedPflowDefined;
            }
            if (SelectedPflowNoIndicator != null)
            {
                pflowModel.PFlowNumIndic = SelectedPflowNoIndicator;
            }
            if (SelectedPflowage != null)
            {
                pflowModel.PFlowRelAge = int.Parse(SelectedPflowage);
            }
            if (SelectedPflowMethod != null)
            {
                pflowModel.PFlowMethod = SelectedPflowMethod;
            }
            if (SelectedPflowRelative != null)
            {
                pflowModel.PFlowRelation = SelectedPflowRelative;
            }

            //Save model class
            object pflowObject = (object)pflowModel;
            accessData.SaveFromSQLTableObject(ref pflowObject, doPflowUpdate);
            pflowModel = (Paleoflow)pflowObject;
            //accessData.SaveFromSQLTableObject(pflowModel, doPflowUpdate);

            //Launch an event call for everyone that an earthmat has been edited.
            if (newPflowEdit != null)
            {
                newPflowEdit(this);
            }
        }

        #endregion

        public void PflowClassCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FillPflowFeature();
        }

    }
}
