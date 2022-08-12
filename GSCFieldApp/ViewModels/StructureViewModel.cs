using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;
using Windows.UI.Xaml.Controls;
using GSCFieldApp.Services.DatabaseServices;
using GSCFieldApp.Models;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using GSCFieldApp.Dictionaries;

namespace GSCFieldApp.ViewModels
{
    public class StructureViewModel: ViewModelBase
    {
        #region INIT DECLARATIONS
        
        public bool doStructureUpdate = false;
        private string _groupTypeDetail = string.Empty;
        public string level1Sep = Dictionaries.ApplicationLiterals.parentChildLevel1Seperator;
        public string level2Sep = Dictionaries.ApplicationLiterals.parentChildLevel2Seperator;

        //UI
        private ObservableCollection<Themes.ComboBoxItem> _structFormat = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedStructFormat = string.Empty;
        private ObservableCollection<Themes.ComboBoxItem> _structMethod = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedStructMethod = string.Empty;
        private ObservableCollection<Themes.ComboBoxItem> _structAttitude = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedStructAttitude = string.Empty; 
        private ObservableCollection<Themes.ComboBoxItem> _structYoung = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedStructYoung = string.Empty;
        private ObservableCollection<Themes.ComboBoxItem> _structGen = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedStructGen = string.Empty;
        private ObservableCollection<Themes.ComboBoxItem> _structStrain = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedStructStrain = string.Empty;
        private ObservableCollection<Themes.ComboBoxItem> _structFlat = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedStructFlat = string.Empty;
        private ObservableCollection<Themes.ComboBoxItem> _structRel = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedStructRel = string.Empty;

        public string _structDetail = string.Empty;//Default
        private string _structAzim = string.Empty; //Default
        private string _structDip = string.Empty;//Default
        private string _structFabric = string.Empty; //Default
        private string _structSense = string.Empty; //Default
        private string _structNote = string.Empty; //Default
        private string _structID = string.Empty; //Default
        private string _structParentID = string.Empty; //Default
        private string _structName = string.Empty; //Default
        private string _structClass = string.Empty;
        private string _structType = string.Empty;
        private string _strucclasstypedetail = string.Empty;


        //Model init
        public Structure structureModel = new Structure();
        public DataIDCalculation structureCalculator = new DataIDCalculation();
        public FieldNotes existingDataDetailStructure;
        DataAccess accessData = new DataAccess();

        //Events and delegate
        public delegate void structureEditEventHandler(object sender); //A delegate for execution events
        public event structureEditEventHandler newStructureEdit; //This event is triggered when a save has been done on station table. 

        #endregion

        #region PROPERTIES

        public string StructDetail { get { return _structDetail; } set { _structDetail = value; } }
        public string StructFabric{ get { return _structFabric; } set { _structFabric = value; } }
        public string StructSense { get { return _structSense; } set { _structSense = value; } }
        public string StructNote { get { return _structNote; } set { _structNote = value; } }
        public string StructClass { get { return _structClass; } set { _structClass = value; } }
        public string StructType { get { return _structType; } set { _structType = value; } }
        public string StructID { get { return _structID; } set { _structID = value; } }
        public string StructName { get { return _structName; } set { _structName = value; } }
        public string StructParentID { get { return _structParentID; } set { _structParentID = value; } }
        public string StructClassTypeDetail { get { return _strucclasstypedetail; } set { _strucclasstypedetail = value; } }
        public ObservableCollection<Themes.ComboBoxItem> StructFormat { get { return _structFormat; } set { _structFormat = value; } }
        public string SelectedStructFormat { get { return _selectedStructFormat; } set { _selectedStructFormat = value; } }
        public ObservableCollection<Themes.ComboBoxItem> StructMethod { get { return _structMethod; } set { _structMethod = value; } }
        public string SelectedStructMethod { get { return _selectedStructMethod; } set { _selectedStructMethod = value; } }
        public ObservableCollection<Themes.ComboBoxItem> StructAttitude { get { return _structAttitude; } set { _structAttitude = value; } }
        public string SelectedStructAttitude { get { return _selectedStructAttitude; } set { _selectedStructAttitude = value; } }

        public ObservableCollection<Themes.ComboBoxItem> StructYoung { get { return _structYoung; } set { _structYoung = value; } }
        public string SelectedStructYoung { get { return _selectedStructYoung; } set { _selectedStructYoung = value; } }

        public ObservableCollection<Themes.ComboBoxItem> StructGen { get { return _structGen; } set { _structGen = value; } }
        public string SelectedStructGen { get { return _selectedStructGen; } set { _selectedStructGen = value; } }
        public ObservableCollection<Themes.ComboBoxItem> StructStrain { get { return _structStrain; } set { _structStrain = value; } }
        public string SelectedStructStrain { get { return _selectedStructStrain; } set { _selectedStructStrain = value; } }
        public ObservableCollection<Themes.ComboBoxItem> StructFlat { get { return _structFlat; } set { _structFlat = value; } }
        public string SelectedStructFlat{ get { return _selectedStructFlat; } set { _selectedStructFlat = value; } }
        public ObservableCollection<Themes.ComboBoxItem> StructRelated { get { return _structRel; } set { _structRel = value; } }
        public string SelectedStructRelated { get { return _selectedStructRel; } set { _selectedStructRel = value; } }
        public string StructAzim
        {
            get
            {
                return _structAzim;
            }
            set
            {
                int index;
                bool result = int.TryParse(value, out index);

                if (result)
                {
                    if (index >= 0 && index < 360)
                    {
                        _structAzim = value;
                    }
                    else
                    {
                        _structAzim = value = "0";
                        RaisePropertyChanged("StructAzim");
                    }

                }
                else
                {
                    _structAzim = value = "0";
                    RaisePropertyChanged("StructAzim");
                }


            }
        }

        public string StructDip
        {
            get
            {
                return _structDip;
            }
            set
            {
                int index;
                bool result = int.TryParse(value, out index);

                if (result)
                {
                    if (index >= 0 && index <= 90)
                    {
                        _structDip = value;
                    }
                    else
                    {
                        _structDip = value = "0";
                        RaisePropertyChanged("StructDip");
                    }

                }
                else
                {
                    _structDip = value = "0";
                    RaisePropertyChanged("StructDip");
                }


            }
        }

        #endregion

        public StructureViewModel(FieldNotes inReport)
        {
            //On init for new samples calculates values for default UI form
            _structParentID = inReport.GenericID;
            _structID = structureCalculator.CalculateStructureID();
            _structName = structureCalculator.CalculateStructureAlias(_structParentID, inReport.earthmat.EarthMatName);

            existingDataDetailStructure = inReport;

            // First order vocabs
            FillStructureFormat();
            FillStructureMethod();
            FillStructureStrain();
            FillStructureFlattening();
            FillStructureRelated();
        }

        #region FILL

        /// <summary>
        /// Will fill the related combobox with a list f all structures from the database.
        /// </summary>
        public void FillStructureRelated(string lithoClass = "")
        {
            //Init.
            if (lithoClass != string.Empty)
            {
                _structRel.Clear();
                RaisePropertyChanged("StructRelated");
            }
            else
            {
                //Else keep whatever has been passed by Field Note as an existing value
                if (existingDataDetailStructure.structure.StructureClass != null && existingDataDetailStructure.structure.StructureClass != string.Empty)
                {
                    lithoClass = existingDataDetailStructure.structure.StructureClass;
                }
                
            }


            string querySelect = "SELECT * FROM " + Dictionaries.DatabaseLiterals.TableStructure;
            string queryWhere = " WHERE " + Dictionaries.DatabaseLiterals.FieldStructureParentID;

            //For new structures or editing existing structures
            if (existingDataDetailStructure.GenericTableName == Dictionaries.DatabaseLiterals.TableEarthMat)
            {
                queryWhere = queryWhere + " = '" + existingDataDetailStructure.GenericID + "'";
            }
            else if (existingDataDetailStructure.GenericTableName == Dictionaries.DatabaseLiterals.TableStructure)
            {
                queryWhere = queryWhere + " = '" + existingDataDetailStructure.ParentID + "'";
            }

            //Extra where clause to select only counterpart and not same structure types
            if (lithoClass.Contains(DatabaseLiterals.KeywordPlanar))
            {
                queryWhere = queryWhere + " AND " + DatabaseLiterals.FieldStructureClass + " LIKE '%" + DatabaseLiterals.KeywordLinear + "%'";
            }
            else if (lithoClass.Contains(DatabaseLiterals.KeywordLinear))
            {
                queryWhere = queryWhere + " AND " + DatabaseLiterals.FieldStructureClass + " LIKE '%" + DatabaseLiterals.KeywordPlanar + "%'";
            }
            else
            { 
                //Do nothing which should select everything.
            }

            string finalQuery = querySelect + queryWhere;

            List<object> allStructures = accessData.ReadTable(structureModel.GetType(), finalQuery);
            IEnumerable<Structure> strucTable = allStructures.Cast<Structure>();
            if (strucTable.Count() != 0)
            {
                foreach (Structure st in strucTable)
                {
                    if (st.StructureID != _structID && st.StructureID != _structParentID)
                    {
                        Themes.ComboBoxItem structItem = new Themes.ComboBoxItem();
                        structItem.itemName = st.StructureName;
                        structItem.itemValue = st.StructureID;
                        _structRel.Add(structItem);
                    }
                }
            }

            //Add a dumb value if nothing is found
            if (_structRel.Count == 0)
            {
                Themes.ComboBoxItem structItem = new Themes.ComboBoxItem();
                structItem.itemName = Dictionaries.DatabaseLiterals.picklistNADescription;
                structItem.itemValue = Dictionaries.DatabaseLiterals.picklistNACode;
                _structRel.Add(structItem);

            }
            else
            {
                //Add default emtpy so user can unselect
                Themes.ComboBoxItem structItem = new Themes.ComboBoxItem();
                structItem.itemName = string.Empty;
                structItem.itemValue = string.Empty;
                _structRel.Insert(0, structItem);

                //Reselect user value
                _selectedStructRel = existingDataDetailStructure.structure.StructureRelated;
                RaisePropertyChanged("SelectedStructRelated");
            }

            //Update UI
            RaisePropertyChanged("StructRelated");
        }

        private void FillStructureFlattening()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldStructureFlattening;
            string tableName = Dictionaries.DatabaseLiterals.TableStructure;
            foreach (var flat in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedStructFlat))
            {
                _structFlat.Add(flat);
            }

            //Update UI
            RaisePropertyChanged("StructFlat");
            RaisePropertyChanged("SelectedStructFlat");

        }

        private void FillStructureStrain()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldStructureStrain;
            string tableName = Dictionaries.DatabaseLiterals.TableStructure;
            foreach (var strains in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedStructStrain))
            {
                _structStrain.Add(strains);
            }


            //Update UI
            RaisePropertyChanged("StructStrain");
            RaisePropertyChanged("SelectedStructStrain");
        }

        private void FillStructureGeneration()
        {
            //Init.
            _structGen.Clear();

            string fieldName = Dictionaries.DatabaseLiterals.FieldStructureGeneration;
            string tableName = Dictionaries.DatabaseLiterals.TableStructure;

            #region Earthmat structural modal

            //Get lithgroup value
            string sgen = Regex.Split(_strucclasstypedetail, level2Sep)[0];

            List<Vocabularies> genstruc = new List<Vocabularies>();

            if (sgen != string.Empty && sgen != " " && sgen != "")
            {
                genstruc = accessData.GetPicklistValuesFromParent(tableName, fieldName, sgen, false).ToList();
            }
            else
            {
                genstruc = accessData.GetPicklistValuesFromParent(tableName, fieldName, "X", false).ToList();
            }

            //Fill in cbox
            foreach (var item in accessData.GetComboboxListFromVocab(genstruc, out _selectedStructGen))
            {
                _structGen.Add(item);
            }


            //Update UI
            RaisePropertyChanged("StructGen");
            if (_selectedStructGen != string.Empty)
            {
                RaisePropertyChanged("SelectedStructGen");
            }
            

            #endregion

        }

        private void FillStructureYounging()
        {

            //Init.
            _structYoung.Clear();
            RaisePropertyChanged("StructYoung");


            string fieldName = Dictionaries.DatabaseLiterals.FieldStructureYoung;
            string tableName = Dictionaries.DatabaseLiterals.TableStructure;

            //Get lithgroup value
            string strucLeft = Regex.Split(_strucclasstypedetail, level2Sep)[0];

            List<Vocabularies> mStruc = new List<Vocabularies>();

            if (strucLeft != string.Empty && strucLeft != " " && strucLeft != "")
            {
                mStruc = accessData.GetPicklistValuesFromParent(tableName, fieldName, strucLeft, false).ToList();
            }
            else
            {
                mStruc = accessData.GetPicklistValuesFromParent(tableName, fieldName, "X", false).ToList();
            }

            //Fill in cbox
            foreach (var item in accessData.GetComboboxListFromVocab(mStruc, out _selectedStructYoung))
            {
                _structYoung.Add(item);
            }

            //Update UI
            RaisePropertyChanged("StructYoung");
            RaisePropertyChanged("SelectedStructYoung");

        }

        private void FillStructureAttitude()
        {

            //Init.
            _structAttitude.Clear();
            RaisePropertyChanged("StructAttitude");


            string fieldName = Dictionaries.DatabaseLiterals.FieldStructureAttitude;
            string tableName = Dictionaries.DatabaseLiterals.TableStructure;

            //Get lithgroup value
            string strucLeft = Regex.Split(_strucclasstypedetail, level2Sep)[0];

            List<Vocabularies> mStruc = new List<Vocabularies>();

            if (strucLeft != string.Empty && strucLeft != " " && strucLeft != "")
            {
                mStruc = accessData.GetPicklistValuesFromParent(tableName, fieldName, strucLeft, false).ToList();
            }
            else
            {
                mStruc = accessData.GetPicklistValuesFromParent(tableName, fieldName, "X", false).ToList();
            }

            //Fill in cbox
            foreach (var item in accessData.GetComboboxListFromVocab(mStruc, out _selectedStructAttitude))
            {
                _structAttitude.Add(item);
            }

            //Update UI
            RaisePropertyChanged("StructAttitude");
            RaisePropertyChanged("SelectedStructAttitude");


        }

        private void FillStructureMethod()
        {
            //Init.
            DataAccess relatedPicklistAccess = new DataAccess();
            string fieldName = Dictionaries.DatabaseLiterals.FieldStructureMethod;
            string tableName = Dictionaries.DatabaseLiterals.TableStructure;
            foreach (var met in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedStructMethod))
            {
                _structMethod.Add(met);
            }


            //Update UI
            RaisePropertyChanged("StructMethod");
            RaisePropertyChanged("SelectedStructMethod");
        }

        private void FillStructureFormat()
        {
            //Init.
            _structFormat.Clear();
            RaisePropertyChanged("StructFormat");
            string fieldName = Dictionaries.DatabaseLiterals.FieldStructureFormat;
            string tableName = Dictionaries.DatabaseLiterals.TableStructure;


            //Get lithgroup value
            string strucLeft = Regex.Split(_strucclasstypedetail, level2Sep)[0];
            string strucFarLeft = Regex.Split(strucLeft, level1Sep)[0];
            List<Vocabularies> fStruc = new List<Vocabularies>();

            if (strucFarLeft != string.Empty && strucFarLeft != " " && strucFarLeft != "")
            {
                fStruc = accessData.GetPicklistValuesFromParent(tableName, fieldName, strucFarLeft, false).ToList();
            }
            else
            {
                fStruc = accessData.GetPicklistValuesFromParent(tableName, fieldName, "X", false).ToList();
            }

            //Fill in cbox
            foreach (var formats in accessData.GetComboboxListFromVocab(fStruc, out _selectedStructFormat))
            {
                _structFormat.Add(formats);
            }


            //Update UI
            RaisePropertyChanged("StructFormat");
            RaisePropertyChanged("SelectedStructFormat");

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
            existingDataDetailStructure = incomingData;

            //Set
            _structID = existingDataDetailStructure.structure.StructureID;
            _structName = existingDataDetailStructure.structure.StructureName;
            _structParentID = existingDataDetailStructure.structure.StructureParentID;
            _structDetail = existingDataDetailStructure.structure.StructureDetail;
            _structAzim = existingDataDetailStructure.structure.StructureAzimuth.ToString();
            _structDip = existingDataDetailStructure.structure.StructureDipPlunge.ToString();
            _structFabric = existingDataDetailStructure.structure.StructureFabric;
            _structNote = existingDataDetailStructure.structure.StructureNotes;
            _structSense = existingDataDetailStructure.structure.StructureSense;


            if (existingDataDetailStructure.structure.StructureType != null)
            {
                _structType = existingDataDetailStructure.structure.StructureType.ToString();
            }
            if (existingDataDetailStructure.structure.StructureClass != null)
            {
                _structClass = existingDataDetailStructure.structure.StructureClass.ToString();
            }
            
            _strucclasstypedetail = existingDataDetailStructure.structure.getClassTypeDetail;

            _selectedStructFlat = existingDataDetailStructure.structure.StructureFlattening;
            _selectedStructMethod = existingDataDetailStructure.structure.StructureMethod;
            _selectedStructRel = existingDataDetailStructure.structure.StructureRelated;
            _selectedStructStrain = existingDataDetailStructure.structure.StructureStrain;

            //Update UI
            RaisePropertyChanged("StructID");
            RaisePropertyChanged("StructName"); 
            RaisePropertyChanged("StructDetail"); 
            RaisePropertyChanged("StructAzim"); 
            RaisePropertyChanged("StructDip");
            RaisePropertyChanged("StructFabric"); 
            RaisePropertyChanged("StructNote"); 
            RaisePropertyChanged("StructSense"); 
            RaisePropertyChanged("StructType");
            RaisePropertyChanged("StructParentID");
            RaisePropertyChanged("StructClassTypeDetail");

            RaisePropertyChanged("SelectedStructFlat"); 
            RaisePropertyChanged("SelectedStructMethod"); 
            RaisePropertyChanged("SelectedStructRelated"); 
            RaisePropertyChanged("SelectedStructStrain");

            //Take care of related structure
            if (true)
            {

            }

            AutoFillDialog2ndRound(incomingData); 

            doStructureUpdate = true;
        }

        /// <summary>
        /// Will fill the dialog with existing information coming from the database.
        /// </summary>
        /// <param name="incomingData">The model in which the existing information is stored.</param>

        public void InitFill2ndRound(string fullStructText)
        {
            _groupTypeDetail = fullStructText;
            RaisePropertyChanged("GroupTypeDetail");

            FillStructureAttitude();
            FillStructureYounging();
            FillStructureGeneration();
            FillStructureFormat();
        }


        public void AutoFillDialog2ndRound(FieldNotes incomingData)
        {
            // Refille second order vocab
            FillStructureAttitude();
            FillStructureYounging();
            FillStructureGeneration();
            FillStructureFormat();

            //Set
            _selectedStructFormat = existingDataDetailStructure.structure.StructureFormat;
            _selectedStructAttitude = incomingData.structure.StructureAttitude;
            _selectedStructGen = incomingData.structure.StructureGeneration;
            _selectedStructYoung = incomingData.structure.StructureYounging;

            RaisePropertyChanged("SelectedStructFormat");
            RaisePropertyChanged("SelectedStructAttitude");
            RaisePropertyChanged("SelectedStructGen");
            RaisePropertyChanged("SelectedStructYoung");

        }

        /// <summary>
        /// On save event
        /// </summary>
        public void SaveDialogInfoAsync()
        {
            //Build model
            BuildStructureObject();

            //Save model class
            if (structureModel.StructureID != null)
            {
                accessData.SaveFromSQLTableObject(structureModel, doStructureUpdate);

                //Launch an event call for everyone that an earthmat has been edited.
                if (newStructureEdit != null)
                {
                    newStructureEdit(this);
                }
            }

        }

        /// <summary>
        /// Force a cascade delete if user get's out of sample dialog while in quick sample mode.
        /// </summary>
        /// <param name="inParentModel"></param>
        public void DeleteCascadeOnQuickStructure(FieldNotes inParentModel)
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

        public void BuildStructureObject()
        {
            //Get current class information and add to model
            structureModel.StructureID = _structID; //Prime key
            structureModel.StructureName = _structName; //Foreign key
            structureModel.StructureNotes = _structNote;
            structureModel.StructureFabric = _structFabric;
            structureModel.StructureSense = _structSense;
            structureModel.StructureParentID = _structParentID;
            structureModel.StructureAzimuth = _structAzim;
            structureModel.StructureDipPlunge = _structDip;

            if (SelectedStructFormat != null)
            {
                structureModel.StructureFormat = SelectedStructFormat;
            }
            if (SelectedStructMethod != null)
            {
                structureModel.StructureMethod = SelectedStructMethod;
            }
            if (SelectedStructAttitude != null)
            {
                structureModel.StructureAttitude = SelectedStructAttitude;
            }
            if (SelectedStructYoung != null)
            {
                structureModel.StructureYounging = SelectedStructYoung;
            }
            if (SelectedStructGen != null)
            {
                structureModel.StructureGeneration = SelectedStructGen;
            }
            if (SelectedStructStrain != null)
            {
                structureModel.StructureStrain = SelectedStructStrain;
            }
            if (SelectedStructFlat != null)
            {
                structureModel.StructureFlattening = SelectedStructFlat;
            }
            if (SelectedStructRelated != null)
            {
                structureModel.StructureRelated = SelectedStructRelated;
            }

            if (_strucclasstypedetail != string.Empty)
            {
                //Get class
                string strucClassType = Regex.Split(_strucclasstypedetail, level2Sep)[0];
                string strucClass = Regex.Split(strucClassType, level1Sep)[0];
                structureModel.StructureClass = strucClass;

                //Get type if any
                if (strucClassType.Contains(level1Sep))
                {
                    structureModel.StructureType = Regex.Split(strucClassType, level1Sep)[1];
                }
                else
                {
                    structureModel.StructureType = string.Empty;
                }

                //Get detail
                if (_strucclasstypedetail.Contains(level2Sep))
                {
                    structureModel.StructureDetail = Regex.Split(_strucclasstypedetail, level2Sep)[1];
                }
                else
                {
                    structureModel.StructureDetail = string.Empty;
                }


            }

            //Define sym angle after format has been saved in the model.
            structureModel.StructureSymAng = structureModel.StructureSymAng;

        }

        #endregion

        #region EVENTS

        /// <summary>
        /// Will be triggered whenever the user has selected a value from the list
        /// Cast the sender and add it to a proper textbox to show selected value
        /// </summary>
        /// <param name="sender"></param>
        public void NewDialog_userHasSelectedAValue(object sender)
        {
            ListView inListView = sender as ListView;
            Models.SemanticData inSD = inListView.SelectedValue as Models.SemanticData;
            if (inSD!=null)
            {
                string userStrucClassTypeDetail = inSD.Title + level2Sep + inSD.Subtitle;
                NewSearch_userHasSelectedAValue(userStrucClassTypeDetail);

            }

        }

        /// <summary>
        /// Will be triggered whenever the struc box has been filled with something. It will update the whole structure class
        /// </summary>
        /// <param name="userStructClassTypeDetail"></param>
        public void NewSearch_userHasSelectedAValue(string userStructClassTypeDetail)
        {
            _strucclasstypedetail = userStructClassTypeDetail;
            RaisePropertyChanged("StructClassTypeDetail");

            // Second order vocab, need to be at least initialized.
            FillStructureAttitude();
            FillStructureYounging();
            FillStructureGeneration();
            FillStructureFormat();
            FillStructureRelated(_strucclasstypedetail);

        }

        #endregion
    }
}
