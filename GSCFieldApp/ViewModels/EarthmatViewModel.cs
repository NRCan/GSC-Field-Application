﻿using GSCFieldApp.Dictionaries;
using GSCFieldApp.Models;
using GSCFieldApp.Services.DatabaseServices;
using GSCFieldApp.Themes;
using GSCFieldApp.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using Template10.Mvvm;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace GSCFieldApp.ViewModels
{
    public class EarthmatViewModel : ViewModelBase
    {
        #region INITIALIZATION
        private EarthMaterial earthmodel = new EarthMaterial();
        private readonly Mineral mineralModel = new Mineral();
        private string _alias = string.Empty; //Default
        private int _earthmatid = 0; //Default
        private int? _stationid = null; //Detault
        private int? _drillID = null; //Detault
        private string _colourindex = "0";//Detault
        private int? _percent; //Default
        private string _contactNote = string.Empty;//Detault
        private string _interpretation = string.Empty;//Detault
        private double? _mag;//Detault
        private string _lithoType = string.Empty;//Detault
        private string _lithoGroup = string.Empty;
        private string _lithoDetail = string.Empty;
        private string _groupTypeDetail = string.Empty;
        private string _notes = string.Empty;
        public bool doEarthUpdate = false;
        public string level1Sep = Dictionaries.ApplicationLiterals.parentChildLevel1Seperator;
        public string level2Sep = Dictionaries.ApplicationLiterals.parentChildLevel2Seperator;
        public DataIDCalculation idCalculator = new DataIDCalculation();
        DataAccess accessData = new DataAccess();
        private string _earthResidualText = string.Empty;
        private Models.Colour _earthColourW = new Models.Colour();
        private Models.Colour _earthColourF = new Models.Colour();
        private Models.Contacts _earthContact = new Contacts();


        private Visibility _bedrockVisibility = Visibility.Visible; //Visibility for extra fields
        private Visibility _surficialVisibility = Visibility.Collapsed; //visibility for extra fields
        public string projectType = string.Empty; //Will hold project type for list parsing.
        readonly DataLocalSettings localSetting = new DataLocalSettings();

        private Dictionary<int, int> _earthResidualPercent = new Dictionary<int, int>(); //Will contain earth material Id and it's percent, for residual percent calculation

        public FieldNotes existingDataDetail;

        //UI Combobox depandant on lithology selection
        private ObservableCollection<Themes.ComboBoxItem> _earthmatModStruc = new ObservableCollection<Themes.ComboBoxItem>();
        private ObservableCollection<Themes.ComboBoxItem> _earthmatModStrucValues = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedEarthmatModStruc = string.Empty;

        private ObservableCollection<Themes.ComboBoxItem> _earthmatModTexture = new ObservableCollection<Themes.ComboBoxItem>();
        private ObservableCollection<Themes.ComboBoxItem> _earthmatModTextureValues = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedEarthmatModTexture = string.Empty;

        private ObservableCollection<Themes.ComboBoxItem> _earthmatModComp = new ObservableCollection<Themes.ComboBoxItem>();
        private ObservableCollection<Themes.ComboBoxItem> _earthmatModCompValues = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedEarthmatModComp = string.Empty;

        private ObservableCollection<Themes.ComboBoxItem> _earthmatGrSize = new ObservableCollection<Themes.ComboBoxItem>();
        private ObservableCollection<Themes.ComboBoxItem> _earthmatGrSizeValues = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedEarthmatGrSize = string.Empty;

        private ObservableCollection<Themes.ComboBoxItem> _earthmatOccurAs = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedEarthmatOccurAs = string.Empty;

        private ObservableCollection<Themes.ComboBoxItem> _earthmatMI = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedEarthmatMI = string.Empty;

        private ObservableCollection<Themes.ComboBoxItem> _earthmatMF = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedEarthmatMF = string.Empty;

        private ObservableCollection<Themes.ComboBoxItem> _earthmatMU = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedEarthmatMU = string.Empty;

        private ObservableCollection<Themes.ComboBoxItem> _earthmatContactTypes = new ObservableCollection<Themes.ComboBoxItem>();
        private ObservableCollection<Themes.ComboBoxItem> _earthmatContactNoteTypes = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedEarthmatContactTypes = string.Empty;

        private ObservableCollection<Themes.ComboBoxItem> _earthmatRelated = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedEarthmatRelated = string.Empty;

        private ObservableCollection<Themes.ComboBoxItem> _earthmatMineral = new ObservableCollection<Themes.ComboBoxItem>();
        private ObservableCollection<Themes.ComboBoxItem> _earthmatMineralValues = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedEarthmatMineral = string.Empty;

        //UI Combobox NOT dependant on lithology
        private ObservableCollection<Themes.ComboBoxItem> _earthmatDefFabric = new ObservableCollection<Themes.ComboBoxItem>();
        private ObservableCollection<Themes.ComboBoxItem> _earthmatDefFabricValues = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedEarthmatDefFabric = string.Empty;
        private ObservableCollection<Themes.ComboBoxItem> _earthmatBedthick = new ObservableCollection<Themes.ComboBoxItem>();
        private ObservableCollection<Themes.ComboBoxItem> _earthmatBedthickValues = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedEarthmatBedthick = string.Empty;

        private ObservableCollection<Themes.ComboBoxItem> _earthmatColourG = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedEarthmatColourG = string.Empty;
        private ObservableCollection<Themes.ComboBoxItem> _earthmatColourI = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedEarthmatColourI = string.Empty;
        private ObservableCollection<Themes.ComboBoxItem> _earthmatColourQ = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedEarthmatColourQ = string.Empty;

        private ObservableCollection<Themes.ComboBoxItem> _earthmatInterConfidence = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedEarthmatInterConfidence = string.Empty;

        private ObservableCollection<Themes.ComboBoxItem> _earthmatMagQualifier = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedEarthmatMagQualifier = string.Empty;

        private ObservableCollection<Themes.ComboBoxItem> _earthmatCU = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedEarthmatCU = string.Empty;

        private ObservableCollection<Themes.ComboBoxItem> _earthmatCL = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedEarthmatCL = string.Empty;

        //Surficial version 1.7 new fields
        private ObservableCollection<Themes.ComboBoxItem> _earthmatSorting = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedEarthmatSorting = string.Empty;
        private ObservableCollection<Themes.ComboBoxItem> _earthmatWater = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedEarthmatWater = string.Empty;
        private ObservableCollection<Themes.ComboBoxItem> _earthmatOxi = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedEarthmatOxi = string.Empty;
        private ObservableCollection<Themes.ComboBoxItem> _earthmatClast = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedEarthmatClast = string.Empty;
        private ObservableCollection<Themes.ComboBoxItem> _earthmatSurfLitho = new ObservableCollection<Themes.ComboBoxItem>();
        private string _selectedEarthmatSurfLitho = string.Empty;

        //Events and delegate
        public delegate void stationEditEventHandler(object sender); //A delegate for execution events
        public event stationEditEventHandler newEarthmatEdit; //This event is triggered when a save has been done on station table.

        #endregion

        #region PROPERTIES

        public Visibility BedrockVisibility { get { return _bedrockVisibility; } set { _bedrockVisibility = value; } }
        public Visibility SurficialVisibility { get { return _surficialVisibility; } set { _surficialVisibility = value; } }
        public EarthMaterial EarthModel { get { return earthmodel; } set { earthmodel = value; } }
        public string Alias { get { return _alias; } set { _alias = value; } }
        public int? StationID { get { return _stationid; } set { _stationid = value; } }
        public int? DrillID { get { return _drillID; } set { _drillID = value; } }
        public int EarthmatID { get { return _earthmatid; } set { _earthmatid = value; } }
        public double? MagSusceptibility
        {
            get
            {
                return _mag;
            }
            set
            {
                bool result = double.TryParse(value.ToString(), out double mag);

                if (result)
                {
                    if (mag >= -10 && mag <= 500)
                    {
                        _mag = value;
                    }
                    else
                    {
                        _mag = value = 0.0;
                        RaisePropertyChanged("MagSusceptibility");
                    }

                }
                else
                {
                    _mag = value = 0.0;
                    RaisePropertyChanged("MagSusceptibility");
                }


            }
        }
        public string ColourIndex
        {
            get
            {
                return _colourindex;
            }
            set
            {
                bool result = int.TryParse(value, out int index);

                if (result)
                {
                    if (index >= 0 && index <= 100)
                    {
                        _colourindex = value;
                    }
                    else
                    {
                        _colourindex = value = "0";
                        RaisePropertyChanged("ColourIndex");
                    }

                }
                else
                {
                    _colourindex = value = "0";
                    RaisePropertyChanged("ColourIndex");
                }


            }
        }
        public Models.Colour EarthColourF { get { return _earthColourF; } set { _earthColourF = value; } }
        public Models.Colour EarthColourW { get { return _earthColourW; } set { _earthColourW = value; } }
        public Models.Contacts EartContact { get { return _earthContact; } set { _earthContact = value; } }
        public ObservableCollection<Themes.ComboBoxItem> EarthmatCU { get { return _earthmatCU; } set { _earthmatCU = value; } }
        public string SelectedEarthmatCU { get { if (_selectedEarthmatCU == null) { return string.Empty; } else { return _selectedEarthmatCU; } } set { _selectedEarthmatCU = value; } }
        public ObservableCollection<Themes.ComboBoxItem> EarthmatCL { get { return _earthmatCL; } set { _earthmatCL = value; } }
        public string SelectedEarthmatCL { get { if (_selectedEarthmatCL == null) { return string.Empty; } else { return _selectedEarthmatCL; } } set { _selectedEarthmatCL = value; } }

        public int? Percent
        {
            get
            {
                return _percent;
            }
            set
            {
                int index;
                bool result = int.TryParse(value.ToString(), out index);

                if (result)
                {
                    if (index >= 0 && index <= 100)
                    {
                        _percent = value;
                    }
                    else
                    {
                        _percent = value = 0;
                        RaisePropertyChanged("Percent");
                    }

                }
                else
                {
                    _percent = value = 0;
                    RaisePropertyChanged("Percent");
                }


            }
        }
        public string EarthResidualText { get { return _earthResidualText; } set { _earthResidualText = value; } }
        public string ContactNote { get { return _contactNote; } set { _contactNote = value; } }
        public string InterpretationNote { get { return _interpretation; } set { _interpretation = value; } }
        public string Notes { get { return _notes; } set { _notes = value; } }
        public string LithoType { get { return _lithoType; } set { _lithoType = value; } }
        public string LithoDetail { get { return _lithoDetail; } set { _lithoDetail = value; } }
        public string LithoGroup { get { return _lithoGroup; } set { _lithoGroup = value; } }

        public string GroupTypeDetail { get { return _groupTypeDetail; } set { _groupTypeDetail = value; } }

        public ObservableCollection<Themes.ComboBoxItem> EarthmatModTexture { get { return _earthmatModTexture; } set { _earthmatModTexture = value; } }
        public ObservableCollection<Themes.ComboBoxItem> EarthmatModTextureValues { get { return _earthmatModTextureValues; } set { _earthmatModTextureValues = value; } }
        public string SelectedEarthmatModTexture { get { if (_selectedEarthmatModTexture == null) { return string.Empty; } else { return _selectedEarthmatModTexture; } } set { _selectedEarthmatModTexture = value; } }
        public ObservableCollection<Themes.ComboBoxItem> EarthmatModComp { get { return _earthmatModComp; } set { _earthmatModComp = value; } }
        public ObservableCollection<Themes.ComboBoxItem> EarthmatModCompValues { get { return _earthmatModCompValues; } set { _earthmatModCompValues = value; } }
        public string SelectedEarthmatModComp { get { if (_selectedEarthmatModComp == null) { return string.Empty; } else { return _selectedEarthmatModComp; } } set { _selectedEarthmatModComp = value; } }
        public ObservableCollection<Themes.ComboBoxItem> EarthmatGrSize { get { return _earthmatGrSize; } set { _earthmatGrSize = value; } }
        public ObservableCollection<Themes.ComboBoxItem> EarthmatGrSizeValues { get { return _earthmatGrSizeValues; } set { _earthmatGrSizeValues = value; } }
        public string SelectedEarthmatGrSize { get { if (_selectedEarthmatGrSize == null) { return string.Empty; } else { return _selectedEarthmatGrSize; } } set { _selectedEarthmatGrSize = value; } }
        public ObservableCollection<Themes.ComboBoxItem> EarthmatOccurAs { get { return _earthmatOccurAs; } set { _earthmatOccurAs = value; } }
        public string SelectedEarthmatOccurAs { get { if (_selectedEarthmatOccurAs == null) { return string.Empty; } else { return _selectedEarthmatOccurAs; } } set { _selectedEarthmatOccurAs = value; } }
        public ObservableCollection<Themes.ComboBoxItem> EarthmatDefFabric { get { return _earthmatDefFabric; } set { _earthmatDefFabric = value; } }
        public ObservableCollection<Themes.ComboBoxItem> EarthmatDefFabricValues { get { return _earthmatDefFabricValues; } set { _earthmatDefFabricValues = value; } }
        public string SelectedEarthmatDefFabric { get { if (_selectedEarthmatDefFabric == null) { return string.Empty; } else { return _selectedEarthmatDefFabric; } } set { _selectedEarthmatDefFabric = value; } }
        public ObservableCollection<Themes.ComboBoxItem> EarthmatBedthick { get { return _earthmatBedthick; } set { _earthmatBedthick = value; } }
        public ObservableCollection<Themes.ComboBoxItem> EarthmatBedthickValues { get { return _earthmatBedthickValues; } set { _earthmatBedthickValues = value; } }
        public string SelectedEarthmatBedthick { get { if (_selectedEarthmatBedthick == null) { return string.Empty; } else { return _selectedEarthmatBedthick; } } set { _selectedEarthmatBedthick = value; } }


        public ObservableCollection<Themes.ComboBoxItem> EarthmatColourGeneric { get { return _earthmatColourG; } set { _earthmatColourG = value; } }
        public string SelectedEarthmatColourGeneric { get { if (_selectedEarthmatColourG == null) { return string.Empty; } else { return _selectedEarthmatColourG; } } set { _selectedEarthmatColourG = value; } }

        public ObservableCollection<Themes.ComboBoxItem> EarthmatColourIntensity { get { return _earthmatColourI; } set { _earthmatColourI = value; } }
        public string SelectedEarthmatColourIntensity { get { if (_selectedEarthmatColourI == null) { return string.Empty; } else { return _selectedEarthmatColourI; } } set { _selectedEarthmatColourI = value; } }

        public ObservableCollection<Themes.ComboBoxItem> EarthmatColourQualifier { get { return _earthmatColourQ; } set { _earthmatColourQ = value; } }
        public string SelectedEarthmatColourQualifier { get { if (_selectedEarthmatColourQ == null) { return string.Empty; } else { return _selectedEarthmatColourQ; } } set { _selectedEarthmatColourQ = value; } }


        public ObservableCollection<Themes.ComboBoxItem> EarthmatMineral { get { return _earthmatMineral; } set { _earthmatMineral = value; } }
        public ObservableCollection<Themes.ComboBoxItem> EarthmatMineralValues { get { return _earthmatMineralValues; } set { _earthmatMineralValues = value; } }
        public string SelectedEarthmatMineral { get { if (_selectedEarthmatMineral == null) { return string.Empty; } else { return _selectedEarthmatMineral; } } set { _selectedEarthmatMineral = value; } }

        public ObservableCollection<Themes.ComboBoxItem> EarthmatMF { get { return _earthmatMF; } set { _earthmatMF = value; } }
        public string SelectedEarthmatMF { get { if (_selectedEarthmatMF == null) { return string.Empty; } else { return _selectedEarthmatMF; } } set { _selectedEarthmatMF = value; } }

        public ObservableCollection<Themes.ComboBoxItem> EarthmatMI { get { return _earthmatMI; } set { _earthmatMI = value; } }
        public string SelectedEarthmatMI { get { if (_selectedEarthmatMI == null) { return string.Empty; } else { return _selectedEarthmatMI; } } set { _selectedEarthmatMI = value; } }

        public ObservableCollection<Themes.ComboBoxItem> EarthmatMU { get { return _earthmatMU; } set { _earthmatMU = value; } }
        public string SelectedEarthmatMU { get { if (_selectedEarthmatMU == null) { return string.Empty; } else { return _selectedEarthmatMU; } } set { _selectedEarthmatMU = value; } }
        public ObservableCollection<Themes.ComboBoxItem> EarthmatContactTypes { get { return _earthmatContactTypes; } set { _earthmatContactTypes = value; } }
        public string SelectedEarthmatContactTypes { get { if (_selectedEarthmatContactTypes == null) { return string.Empty; } else { return _selectedEarthmatContactTypes; } } set { _selectedEarthmatContactTypes = value; } }

        public ObservableCollection<Themes.ComboBoxItem> EarthmatContactNoteTypes { get { return _earthmatContactNoteTypes; } set { _earthmatContactNoteTypes = value; } }

        public ObservableCollection<Themes.ComboBoxItem> EarthmatRelated { get { return _earthmatRelated; } set { _earthmatRelated = value; } }
        public string SelectedEarthmatRelated { get { if (_selectedEarthmatRelated == null) { return string.Empty; } else { return _selectedEarthmatRelated; } } set { _selectedEarthmatRelated = value; } }
        public ObservableCollection<Themes.ComboBoxItem> EarthmatInterConfidence { get { return _earthmatInterConfidence; } set { _earthmatInterConfidence = value; } }
        public string SelectedEarthmatInterConfidence { get { if (_selectedEarthmatInterConfidence == null) { return string.Empty; } else { return _selectedEarthmatInterConfidence; } } set { _selectedEarthmatInterConfidence = value; } }
        public ObservableCollection<Themes.ComboBoxItem> MagQualifier { get { return _earthmatMagQualifier; } set { _earthmatMagQualifier = value; } }
        public string SelectedMagQualifier { get { if (_selectedEarthmatMagQualifier == null) { return string.Empty; } else { return _selectedEarthmatMagQualifier; } } set { _selectedEarthmatMagQualifier = value; } }

        //Surficial version 1.7 new fields
        public ObservableCollection<Themes.ComboBoxItem> EarthmatSorting { get { return _earthmatSorting; } set { _earthmatSorting = value; } }
        public string SelectedEarthmatSorting { get { if (_selectedEarthmatSorting == null) { return string.Empty; } else { return _selectedEarthmatSorting; } } set { _selectedEarthmatSorting = value; } }
        public ObservableCollection<Themes.ComboBoxItem> EarthmatWater { get { return _earthmatWater; } set { _earthmatWater = value; } }
        public string SelectedEarthmatWater { get { if (_selectedEarthmatWater == null) { return string.Empty; } else { return _selectedEarthmatWater; } } set { _selectedEarthmatWater = value; } }
        public ObservableCollection<Themes.ComboBoxItem> EarthmatOxi { get { return _earthmatOxi; } set { _earthmatOxi = value; } }
        public string SelectedEarthmatOxi { get { if (_selectedEarthmatOxi == null) { return string.Empty; } else { return _selectedEarthmatOxi; } } set { _selectedEarthmatOxi = value; } }
        public ObservableCollection<Themes.ComboBoxItem> EarthmatClast { get { return _earthmatClast; } set { _earthmatClast = value; } }
        public string SelectedEarthmatClast { get { if (_selectedEarthmatClast == null) { return string.Empty; } else { return _selectedEarthmatClast; } } set { _selectedEarthmatClast = value; } }
        public ObservableCollection<Themes.ComboBoxItem> EarthmatSurfLihto { get { return _earthmatSurfLitho; } set { _earthmatSurfLitho = value; } }
        public string SelectedEarthmatSurfLitho { get { if (_selectedEarthmatSurfLitho == null) { return string.Empty; } else { return _selectedEarthmatSurfLitho; } } set { _selectedEarthmatSurfLitho = value; } }



        #endregion

        #region METHODS
        public EarthmatViewModel(FieldNotes inDetailModel)
        {
            existingDataDetail = inDetailModel;

            //On init for new earthmats calculate values so UI shows stuff.
            _earthmatid = idCalculator.CalculateEarthmatID();

            if (inDetailModel!= null && inDetailModel.GenericTableName == DatabaseLiterals.TableStation) //detail model could be null if a quick earthmat is asked
            {
                _stationid = inDetailModel.GenericID;
                _alias = idCalculator.CalculateEarthmatnAlias(_stationid, inDetailModel.GenericAliasName);
            }
            else if (inDetailModel != null && inDetailModel.GenericTableName == DatabaseLiterals.TableDrillHoles)
            {
                _drillID = inDetailModel.GenericID;
                _alias = idCalculator.CalculateEarthmatnAlias(_drillID, inDetailModel.GenericAliasName);
            }

            //Will enable/disable some fields based on bedrock or surficial usage
            SetFieldVisibility();

            if (_bedrockVisibility == Visibility.Visible)
            {
                //Fill some first order comboboxes
                FillDefFabric();
                FillBedthick();
                FillContactTypes();
                FillContactU();
                FillContactL();

                FillRelatedEarthmat();
                FillMineral();

                FillMagQualifier();
                FillMetaIntensity();
                FillMetaFacies();

                //Fill second order comboboxes (dependant on selected litho type)
                //NOTE: needs at least to be initialized and filled at init, else re-selecting an item after init doesn't seem to work.
                FillModComp();
                FillModTextureStructure();
                FillGrSize();
                FillOccur();
            }
            else if (_surficialVisibility == Visibility.Visible)
            {
                FillModTextureStructure(false);
                FillSurficialLitho();
                FillSorting();
                FillWater();
                FillOxi();
                FillClast();
            }

            FillMU();
            FillColourG();
            FillColourI();
            FillColourQ();
            FillInterConfidence();

            if (existingDataDetail != null && existingDataDetail.GenericID != 0)
            {
                CalculateResidual();
            }


        }

        /// <summary>
        /// Will set visibility based on a bedrock or surficial field book
        /// </summary>
        private void SetFieldVisibility()
        {
            if (localSetting.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoFWorkType) != null)
            {
                //Keep value in variables
                projectType = localSetting.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoFWorkType).ToString();

                if (localSetting.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoFWorkType).ToString().Contains(Dictionaries.DatabaseLiterals.ApplicationThemeBedrock))
                {
                    _bedrockVisibility = Visibility.Visible;
                    _surficialVisibility = Visibility.Collapsed;
                }
                else if (localSetting.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoFWorkType).ToString() == Dictionaries.DatabaseLiterals.ApplicationThemeSurficial)
                {
                    _bedrockVisibility = Visibility.Collapsed;
                    _surficialVisibility = Visibility.Visible;
                }
            }
            else
            {
                //Fallback
                _bedrockVisibility = Visibility.Visible;
                _surficialVisibility = Visibility.Collapsed;
                projectType = DatabaseLiterals.ApplicationThemeBedrock;
            }

            RaisePropertyChanged("BedrockVisibility");
            RaisePropertyChanged("SurficialVisibility");
        }

        /// <summary>
        /// Will fill the dialog with existing information coming from the database.
        /// </summary>
        /// <param name="incomingData">The model in which the existing information is stored.</param>
        public void AutoFillDialog(FieldNotes incomingData)
        {
            //Keep
            existingDataDetail = incomingData;

            //Set
            _earthmatid = existingDataDetail.earthmat.EarthMatID;
            
            _alias = existingDataDetail.earthmat.EarthMatName;
            _interpretation = existingDataDetail.earthmat.EarthMatInterp;
            _contactNote = existingDataDetail.earthmat.EarthMatContactNote;
            _colourindex = existingDataDetail.earthmat.EarthMatColourInd.ToString();
            _mag = existingDataDetail.earthmat.EarthMatMagSuscept;
            _groupTypeDetail = existingDataDetail.earthmat.getGroupTypeDetail;
            _lithoType = existingDataDetail.earthmat.EarthMatLithtype;
            _lithoDetail = existingDataDetail.earthmat.EarthMatLithdetail;
            _lithoGroup = existingDataDetail.earthmat.EarthMatLithgroup;
            _notes = existingDataDetail.earthmat.EarthMatNotes;
            _percent = existingDataDetail.earthmat.EarthMatPercent;

            _earthColourW = new Colour().fromString(existingDataDetail.earthmat.EarthMatColourW);
            _earthColourF = new Colour().fromString(existingDataDetail.earthmat.EarthMatColourF);
            //_earthContact = new Contacts().fromString(existingDataDetail.earthmat.EarthMatContact);

            //Deal with annoying parents
            if (existingDataDetail.ParentTableName == DatabaseLiterals.TableStation)
            {
                _stationid = existingDataDetail.ParentID;
            }
            else if (existingDataDetail.ParentTableName == DatabaseLiterals.TableDrillHoles)
            {
                _drillID = existingDataDetail.ParentID;
            }

            //Update list view
            UnPipeValues(existingDataDetail.earthmat.EarthMatDefabric, Dictionaries.DatabaseLiterals.FieldEarthMatDefabric);
            UnPipeValues(existingDataDetail.earthmat.EarthMatBedthick, Dictionaries.DatabaseLiterals.FieldEarthMatBedthick);
            UnPipeValues(existingDataDetail.earthmat.EarthMatContact, Dictionaries.DatabaseLiterals.FieldEarthMatContact);

            _selectedEarthmatMU = existingDataDetail.earthmat.EarthMatMapunit;
            _selectedEarthmatInterConfidence = existingDataDetail.earthmat.EarthMatInterpConf;
            _selectedEarthmatMagQualifier = existingDataDetail.earthmat.EarthMatMagQualifier;
            _selectedEarthmatMI = existingDataDetail.earthmat.EarthMatMetaIntensity;
            _selectedEarthmatMF = existingDataDetail.earthmat.EarthMatMetaIFacies;
            _selectedEarthmatCU = existingDataDetail.earthmat.EarthMatContactUp;
            _selectedEarthmatCL = existingDataDetail.earthmat.EarthMatContactLow;

            //Update UI
            RaisePropertyChanged("EarthmatID");
            RaisePropertyChanged("Alias");
            RaisePropertyChanged("InterpretationNote");
            RaisePropertyChanged("Notes");
            RaisePropertyChanged("ContactNote");
            RaisePropertyChanged("ColourIndex");
            RaisePropertyChanged("MagSusceptibility");
            RaisePropertyChanged("LithoType");
            RaisePropertyChanged("GroupTypeDetail");
            RaisePropertyChanged("Percent");
            RaisePropertyChanged("EarthColourF");
            RaisePropertyChanged("EarthColourW");

            if (_selectedEarthmatMU != null)
            {
                RaisePropertyChanged("SelectedEarthmatMU");
            }
            if (_selectedEarthmatInterConfidence != null)
            {
                RaisePropertyChanged("SelectedEarthmatInterConfidence");
            }
            if (_selectedEarthmatMagQualifier != null)
            {
                RaisePropertyChanged("SelectedMagQualifier");
            }
            if (_selectedEarthmatMI != null)
            {
                RaisePropertyChanged("SelectedEarthmatMI");
            }
            if (_selectedEarthmatMF != null)
            {
                RaisePropertyChanged("SelectedEarthmatMF");
            }
            if (_selectedEarthmatCL != null)
            {
                RaisePropertyChanged("SelectedEarthmatCL");
            }
            if (_selectedEarthmatCU != null)
            {
                RaisePropertyChanged("SelectedEarthmatCU");
            }
            //Surficial
            if (_surficialVisibility == Visibility.Visible)
            {
                _selectedEarthmatSurfLitho = existingDataDetail.earthmat.EarthMatLithdetail;
                _selectedEarthmatSorting = existingDataDetail.earthmat.EarthMatSorting;
                _selectedEarthmatWater = existingDataDetail.earthmat.EarthMatH2O;
                _selectedEarthmatOxi = existingDataDetail.earthmat.EarthMatOxidation;
                _selectedEarthmatClast = existingDataDetail.earthmat.EarthMatClastForm;

                RaisePropertyChanged("SelectedEarthmatSurfLitho");
                RaisePropertyChanged("SelectedEarthmatSorting");
                RaisePropertyChanged("SelectedEarthmatWater");
                RaisePropertyChanged("SelectedEarthmatOxi");
                RaisePropertyChanged("SelectedEarthmatClast");
            }

            //Special case for minerals
            List<object> mineralTableRaw = accessData.ReadTable(mineralModel.GetType(), null);
            IEnumerable<Mineral> mineralTable = mineralTableRaw.Cast<Mineral>(); //Cast to proper list type
            IEnumerable<Mineral> mineralParentEarth = from e in mineralTable where e.MineralEMID == _earthmatid select e;
            if (mineralParentEarth.Count() != 0 || mineralParentEarth != null)
            {
                foreach (Mineral mns in mineralParentEarth)
                {
                    AddAConcatenatedValue(mns.MineralName, null, Dictionaries.DatabaseLiterals.FieldMineral, false);
                }

            }

            AutoFillDialog2ndRound(incomingData);

            doEarthUpdate = true;

        }

        /// <summary>
        /// Will fill the dialog with existing information coming from the database, based user selected lithology. This 
        /// round of filling is dependant on a value.
        /// </summary>
        /// <param name="incomingData">The model in which the existing information is stored.</param>
        public void AutoFillDialog2ndRound(FieldNotes incomingData)
        {
            //Refill some comboboxes

            if (_surficialVisibility == Visibility.Visible)
            {
                FillModTextureStructure(false);
            }
            else
            {
                FillModTextureStructure();
            }

            FillModComp();
            FillGrSize();
            FillOccur();

            //Keep
            existingDataDetail = incomingData;

            //Set
            _selectedEarthmatModTexture = existingDataDetail.earthmat.EarthMatModTextStruc;
            _selectedEarthmatModComp = existingDataDetail.earthmat.EarthMatModComp;
            _selectedEarthmatGrSize = existingDataDetail.earthmat.EarthMatGrSize;

            //Update list view
            UnPipeValues(existingDataDetail.earthmat.EarthMatModTextStruc, Dictionaries.DatabaseLiterals.FieldEarthMatModTextStruc);
            UnPipeValues(existingDataDetail.earthmat.EarthMatModComp, Dictionaries.DatabaseLiterals.FieldEarthMatModComp);
            UnPipeValues(existingDataDetail.earthmat.EarthMatGrSize, Dictionaries.DatabaseLiterals.FieldEarthMatGrSize);

            _selectedEarthmatOccurAs = existingDataDetail.earthmat.EarthMatOccurs;

            if (_selectedEarthmatOccurAs != null)
            {
                RaisePropertyChanged("SelectedEarthmatOccurAs");
            }
            else
            {
                _selectedEarthmatOccurAs = string.Empty;
                RaisePropertyChanged("SelectedEarthmatOccurAs");
            }
        }

        /// <summary>
        /// On save event
        /// </summary>
        public void SaveDialogInfo()
        {
            //Get current class information and add to model
            earthmodel.EarthMatID = _earthmatid; //Prime key
            earthmodel.EarthMatStatID = _stationid; //Foreign key
            earthmodel.EarthMatDrillHoleID = _drillID; //Foreign key
            earthmodel.EarthMatName = _alias;
            earthmodel.EarthMatMagSuscept = _mag;
            earthmodel.EarthMatColourInd = int.Parse(_colourindex);
            earthmodel.EarthMatContactNote = _contactNote;
            earthmodel.EarthMatInterp = _interpretation;
            earthmodel.EarthMatNotes = _notes;
            earthmodel.EarthMatPercent = _percent;
            earthmodel.EarthMatColourF = _earthColourF.ToString();
            earthmodel.EarthMatColourW = _earthColourW.ToString();

            //process list of values so they are concatenated.
            ConcatenatedCombobox ccBox = new ConcatenatedCombobox();

            earthmodel.EarthMatModTextStruc = ccBox.PipeValues(_earthmatModTextureValues); //process list of values so they are concatenated.
            earthmodel.EarthMatModComp = ccBox.PipeValues(_earthmatModCompValues); //process list of values so they are concatenated.
            earthmodel.EarthMatGrSize = ccBox.PipeValues(_earthmatGrSizeValues); //process list of values so they are concatenated.
            earthmodel.EarthMatContact = ccBox.PipeValues(_earthmatContactNoteTypes); //process list of values so they are concatenated.
            earthmodel.EarthMatDefabric = ccBox.PipeValues(_earthmatDefFabricValues); //process list of values so they are concatenated.
            earthmodel.EarthMatBedthick = ccBox.PipeValues(_earthmatBedthickValues); //process list of values so they are concatenated.

            if (_selectedEarthmatOccurAs != null)
            {
                earthmodel.EarthMatOccurs = SelectedEarthmatOccurAs;
            }
            if (_selectedEarthmatMU != null)
            {
                earthmodel.EarthMatMapunit = _selectedEarthmatMU;
            }
            if (_selectedEarthmatInterConfidence != null)
            {
                earthmodel.EarthMatInterpConf = _selectedEarthmatInterConfidence;
            }
            if (_selectedEarthmatMI != null)
            {
                earthmodel.EarthMatMetaIntensity = _selectedEarthmatMI;
            }
            if (_selectedEarthmatMF != null)
            {
                earthmodel.EarthMatMetaIFacies = _selectedEarthmatMF;
            }
            if (_selectedEarthmatSorting != null)
            {
                earthmodel.EarthMatSorting = _selectedEarthmatSorting;
            }
            if (_selectedEarthmatWater != null)
            {
                earthmodel.EarthMatH2O = _selectedEarthmatWater;
            }
            if (_selectedEarthmatOxi != null)
            {
                earthmodel.EarthMatOxidation = _selectedEarthmatOxi;
            }
            if (_selectedEarthmatClast != null)
            {
                earthmodel.EarthMatClastForm = _selectedEarthmatClast;
            }
            if (SelectedEarthmatCL != null)
            {
                earthmodel.EarthMatContactLow = SelectedEarthmatCL;
            }
            if (SelectedEarthmatCU != null)
            {
                earthmodel.EarthMatContactUp = SelectedEarthmatCU;
            }
            if (_groupTypeDetail != string.Empty)
            {
                //Get group
                string litho_groupType = Regex.Split(_groupTypeDetail, level2Sep)[0];
                string litho_group = Regex.Split(litho_groupType, level1Sep)[0];
                earthmodel.EarthMatLithgroup = litho_group;

                //Get type if any
                if (litho_groupType.Contains(level1Sep))
                {
                    earthmodel.EarthMatLithtype = Regex.Split(litho_groupType, level1Sep)[1];
                }
                else
                {
                    earthmodel.EarthMatLithtype = string.Empty;
                }

                //Get detail
                if (_groupTypeDetail.Contains(level2Sep))
                {
                    earthmodel.EarthMatLithdetail = Regex.Split(_groupTypeDetail, level2Sep)[1];
                }
                else
                {
                    earthmodel.EarthMatLithdetail = string.Empty;
                }

            }
            if (_selectedEarthmatMagQualifier != null && _selectedEarthmatMagQualifier != string.Empty)
            {
                earthmodel.EarthMatMagQualifier = _selectedEarthmatMagQualifier;
            }

            if (_selectedEarthmatSurfLitho != String.Empty)
            {
                earthmodel.EarthMatLithdetail = _selectedEarthmatSurfLitho;
            }

            //Save model class
            //accessData.SaveFromSQLTableObject(ref earthmodel, doEarthUpdate);

            object earthObject = (object)earthmodel;
            accessData.SaveFromSQLTableObject(ref earthObject, doEarthUpdate);
            earthmodel = (EarthMaterial)earthObject;

            //Special case for minerals
            if (_earthmatMineralValues.Count != 0)
            {

                List<string> listOfMinerals = new List<string>();
                foreach (Themes.ComboBoxItem mins in EarthmatMineralValues)
                {
                    //Save only if the mineral was a new added one, prevent duplicates
                    if (mins.canRemoveItem == Windows.UI.Xaml.Visibility.Visible)
                    {
                        listOfMinerals.Add(mins.itemName);

                    }

                }

                if (listOfMinerals.Count > 0)
                {
                    FieldNotes earthModelToSave = new FieldNotes();
                    earthModelToSave.earthmat = earthmodel;
                    MineralViewModel minVM = new MineralViewModel(earthModelToSave, true);

                    minVM.QuickMineralRecordOnly(earthmodel.EarthMatID, listOfMinerals, Dictionaries.DatabaseLiterals.TableEarthMat);
                }


            }

            //Launch an event call for everyone that an earthmat has been edited.
            if (newEarthmatEdit != null)
            {
                newEarthmatEdit(this);
            }
        }

        #endregion

        #region FILL METHODS

        public void InitFill2ndRound(string fullLithoText)
        {
            _groupTypeDetail = fullLithoText;
            RaisePropertyChanged("GroupTypeDetail");

            FillModComp();
            FillModTextureStructure();
            FillGrSize();
            FillOccur();
        }

        /// <summary>
        /// Will fill the material contact combobox
        /// </summary>
        public void FillContactU()
        {

            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldEarthMatContactUp;
            string tableName = Dictionaries.DatabaseLiterals.TableEarthMat;
            foreach (var itemCU in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedEarthmatCU))
            {
                _earthmatCU.Add(itemCU);
            }


            //Update UI
            RaisePropertyChanged("EarthmatCU");
            RaisePropertyChanged("SelectedEarthmatCU");

        }
        /// <summary>
        /// Will fill the material contact combobox
        /// </summary>
        public void FillContactL()
        {

            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldEarthMatContactLow;
            string tableName = Dictionaries.DatabaseLiterals.TableEarthMat;
            foreach (var itemCL in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedEarthmatCL))
            {
                _earthmatCL.Add(itemCL);
            }


            //Update UI
            RaisePropertyChanged("EarthmatCL");
            RaisePropertyChanged("SelectedEarthmatCL");

        }

        private void FillMineral()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldMineral;
            string tableName = Dictionaries.DatabaseLiterals.TableMineral;
            foreach (var itemMU in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedEarthmatMineral))
            {
                _earthmatMineral.Add(itemMU);
            }


            //Update UI
            RaisePropertyChanged("EarthmatMineral");
            //RaisePropertyChanged("SelectedEarthmatMineral");
        }

        private void FillMU()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldEarthMatMapunit;
            string tableName = Dictionaries.DatabaseLiterals.TableEarthMat;
            foreach (var itemMU in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedEarthmatMU))
            {
                _earthmatMU.Add(itemMU);
            }


            //Update UI
            RaisePropertyChanged("EarthmatMU");
            RaisePropertyChanged("SelectedEarthmatMU");
        }

        /// <summary>
        /// Will fill the textural model combobox
        /// </summary>
        public void FillModTextureStructure(bool withParents = true)
        {
            _earthmatModTexture.Clear();
            RaisePropertyChanged("EarthmatModTexture");
            _earthmatModTextureValues.Clear();
            RaisePropertyChanged("EarthmatModTextureValues");

            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldEarthMatModTextStruc;
            string tableName = Dictionaries.DatabaseLiterals.TableEarthMat;

            #region Earthmat structural modal

            if (withParents)
            {
                //Get lithgroup value
                string eText = string.Empty;
                if (_groupTypeDetail.Contains(level1Sep))
                {
                    eText = Regex.Split(_groupTypeDetail, level1Sep)[0];
                }
                else if (_groupTypeDetail.Contains(level2Sep))
                {
                    eText = Regex.Split(_groupTypeDetail, level2Sep)[0];
                }
                List<Vocabularies> mTxture = new List<Vocabularies>();

                if (eText != string.Empty && eText != " " && eText != "")
                {
                    mTxture = accessData.GetPicklistValuesFromParent(tableName, fieldName, eText, false).ToList();
                }
                else
                {
                    mTxture = accessData.GetPicklistValuesFromParent(tableName, fieldName, "x", false).ToList();
                }

                //Fill in cbox
                foreach (var itemModText in accessData.GetComboboxListFromVocab(mTxture, out _selectedEarthmatModTexture))
                {
                    _earthmatModTexture.Add(itemModText);
                }
            }
            else
            {
                //Fill in cbox
                foreach (var itemMU in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedEarthmatModTexture))
                {
                    _earthmatModTexture.Add(itemMU);
                }
            }





            //Update UI
            RaisePropertyChanged("EarthmatModTexture");
            RaisePropertyChanged("SelectedEarthmatModTexture");

            #endregion


        }

        /// <summary>
        /// Will fill the compositional model combobox
        /// </summary>
        public void FillModComp()
        {
            _earthmatModComp.Clear();
            _earthmatModCompValues.Clear();
            RaisePropertyChanged("EarthmatModComp");
            RaisePropertyChanged("EarthmatModCompValues");

            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldEarthMatModComp;
            string tableName = Dictionaries.DatabaseLiterals.TableEarthMat;

            #region Earthmat structural modal

            //Get lithgroup value
            string eComp = string.Empty;
            if (_groupTypeDetail.Contains(level1Sep))
            {
                eComp = Regex.Split(_groupTypeDetail, level1Sep)[0];
            }
            else if (_groupTypeDetail.Contains(level2Sep))
            {
                eComp = Regex.Split(_groupTypeDetail, level2Sep)[0];
            }

            List<Vocabularies> mComp = new List<Vocabularies>();

            if (eComp != string.Empty && eComp != " " && eComp != "")
            {
                mComp = accessData.GetPicklistValuesFromParent(tableName, fieldName, eComp, false).ToList();
            }
            else
            {
                mComp = accessData.GetPicklistValuesFromParent(tableName, fieldName, "x", false).ToList(); //Make the query crash and return N.A. if nothing is available in lithotype
            }

            //Fill in cbox
            foreach (var itemModComp in accessData.GetComboboxListFromVocab(mComp, out _selectedEarthmatModComp))
            {
                _earthmatModComp.Add(itemModComp);
            }


            //Update UI
            RaisePropertyChanged("EarthmatModComp");
            RaisePropertyChanged("SelectedEarthmatModComp");

            #endregion


        }

        /// <summary>
        /// Will fill the grain size combobox
        /// </summary>
        public void FillGrSize()
        {
            _earthmatGrSize.Clear();
            RaisePropertyChanged("EarthmatGrSize");
            _earthmatGrSizeValues.Clear();
            RaisePropertyChanged("EarthmatGrSizeValues");

            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldEarthMatGrSize;
            string tableName = Dictionaries.DatabaseLiterals.TableEarthMat;

            #region Earthmat structural modal

            //Get lithgroup value
            string eSize = string.Empty;
            if (_groupTypeDetail.Contains(level1Sep))
            {
                eSize = Regex.Split(_groupTypeDetail, level1Sep)[0];
            }
            else if (_groupTypeDetail.Contains(level2Sep))
            {
                eSize = Regex.Split(_groupTypeDetail, level2Sep)[0];
            }
            List<Vocabularies> grSize = new List<Vocabularies>();

            if (eSize != string.Empty && eSize != " " && eSize != "")
            {
                grSize = accessData.GetPicklistValuesFromParent(tableName, fieldName, eSize, false).ToList();
            }
            else
            {
                grSize = accessData.GetPicklistValuesFromParent(tableName, fieldName, "x", false).ToList();
            }

            //Fill in cbox
            foreach (var itemGrSize in accessData.GetComboboxListFromVocab(grSize, out _selectedEarthmatGrSize))
            {
                _earthmatGrSize.Add(itemGrSize);
            }


            //Update UI
            RaisePropertyChanged("EarthmatGrSize");
            RaisePropertyChanged("SelectedEarthmatGrSize");

            #endregion


        }

        /// <summary>
        /// Will fill the occuring as combobox
        /// </summary>
        public void FillOccur()
        {
            _earthmatOccurAs.Clear();
            RaisePropertyChanged("EarthmatOccurAs");

            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldEarthMatOccurs;
            string tableName = Dictionaries.DatabaseLiterals.TableEarthMat;

            #region Earthmat structural modal

            //Get lithgroup value
            string eOccur = string.Empty;
            if (_groupTypeDetail.Contains(level1Sep))
            {
                eOccur = Regex.Split(_groupTypeDetail, level1Sep)[0];
            }
            else if (_groupTypeDetail.Contains(level2Sep))
            {
                eOccur = Regex.Split(_groupTypeDetail, level2Sep)[0];
            }
            List<Vocabularies> ocAs = new List<Vocabularies>();

            if (eOccur != string.Empty && eOccur != " " && eOccur != "")
            {
                ocAs = accessData.GetPicklistValuesFromParent(tableName, fieldName, eOccur, false).ToList();
            }
            else
            {
                ocAs = accessData.GetPicklistValuesFromParent(tableName, fieldName, "x", false).ToList();
            }

            //Fill in cbox
            foreach (var itemOccur in accessData.GetComboboxListFromVocab(ocAs, out _selectedEarthmatOccurAs))
            {
                _earthmatOccurAs.Add(itemOccur);
            }


            //Update UI
            RaisePropertyChanged("EarthmatOccurAs");
            RaisePropertyChanged("SelectedEarthmatOccurAs");

            #endregion


        }

        /// <summary>
        /// Will fill the def. fabric combobox
        /// </summary>
        public void FillDefFabric()
        {

            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldEarthMatDefabric;
            string tableName = Dictionaries.DatabaseLiterals.TableEarthMat;
            foreach (var itemFabric in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedEarthmatDefFabric))
            {
                _earthmatDefFabric.Add(itemFabric);
            }


            //Update UI
            RaisePropertyChanged("EarthmatDefFabric");
            RaisePropertyChanged("SelectedEarthmatDefFabric");

        }

        /// <summary>
        /// Will fill the bedding thickness combobox
        /// </summary>
        public void FillBedthick()
        {

            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldEarthMatBedthick;
            string tableName = Dictionaries.DatabaseLiterals.TableEarthMat;
            foreach (var itemBedthick in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedEarthmatBedthick))
            {
                _earthmatBedthick.Add(itemBedthick);
            }


            //Update UI
            RaisePropertyChanged("EarthmatBedthick");
            RaisePropertyChanged("SelectedEarthmatBedthick");

        }

        /// <summary>
        /// Will fill the material colour combobox
        /// </summary>
        public void FillColourG()
        {

            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.KeywordColourGeneric;
            string tableName = Dictionaries.DatabaseLiterals.TableEarthMat;
            foreach (var itemCF in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedEarthmatColourG))
            {
                _earthmatColourG.Add(itemCF);
            }


            //Update UI
            RaisePropertyChanged("EarthmatColourGeneric");
            RaisePropertyChanged("SelectedEarthmatColourGeneric");

        }

        /// <summary>
        /// Will fill the material colour combobox
        /// </summary>
        public void FillColourI()
        {

            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.KeywordColourIntensity;
            string tableName = Dictionaries.DatabaseLiterals.TableEarthMat;
            foreach (var itemCF in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedEarthmatColourI))
            {
                _earthmatColourI.Add(itemCF);
            }


            //Update UI
            RaisePropertyChanged("EarthmatColourIntensity");
            RaisePropertyChanged("SelectedEarthmatColourIntensity");

        }

        // <summary>
        /// Will fill the material colour combobox
        /// </summary>
        public void FillColourQ()
        {

            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.KeywordColourQualifier;
            string tableName = Dictionaries.DatabaseLiterals.TableEarthMat;
            foreach (var itemCF in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedEarthmatColourQ))
            {
                _earthmatColourQ.Add(itemCF);
            }


            //Update UI
            RaisePropertyChanged("EarthmatColourQualifier");
            RaisePropertyChanged("SelectedEarthmatColourQualifier");

        }

        /// <summary>
        /// Will fill the material contact combobox
        /// </summary>
        public void FillContactTypes()
        {

            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldEarthMatContactUp;
            string tableName = Dictionaries.DatabaseLiterals.TableEarthMat;
            foreach (var itemCU in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedEarthmatContactTypes))
            {
                _earthmatContactTypes.Add(itemCU);
            }


            //Update UI
            RaisePropertyChanged("EarthmatContactTypes ");
            RaisePropertyChanged("SelectedEarthmatContactTypes ");

        }
        /// <summary>
        /// Will fill with all other earthmaterial associated with parent station
        /// </summary>
        public void FillRelatedEarthmat()
        {
            if (existingDataDetail != null && existingDataDetail.station != null)
            {

                //Querying with Linq
                List<object> earthmatTableRaw = accessData.ReadTable(earthmodel.GetType(), null);
                IEnumerable<EarthMaterial> earthmatTable = earthmatTableRaw.Cast<EarthMaterial>(); //Cast to proper list type
                IEnumerable<EarthMaterial> earths = from e in earthmatTable where e.EarthMatStatID == existingDataDetail.GenericID select e;

                if (existingDataDetail.GenericTableName == DatabaseLiterals.TableDrillHoles)
                {
                    earths = from e in earthmatTable where e.EarthMatDrillHoleID == existingDataDetail.GenericID select e;
                }
                else if (existingDataDetail.GenericTableName == DatabaseLiterals.TableEarthMat)
                {
                    earths = from e in earthmatTable where (e.EarthMatStatID == existingDataDetail.ParentID) &&
                             (e.EarthMatID != existingDataDetail.GenericID) select e;
                }

                foreach (EarthMaterial ea in earths)
                {
                    Themes.ComboBoxItem newItem = new Themes.ComboBoxItem();
                    newItem.itemValue = ea.EarthMatName;
                    newItem.itemName = ea.EarthMatName;
                    _earthmatRelated.Add(newItem);
                }


                //Update UI
                RaisePropertyChanged("EarthmatRelated");
            }


        }
        public void FillInterConfidence()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldEarthMatInterpConf;
            string tableName = Dictionaries.DatabaseLiterals.TableEarthMat;
            foreach (var itemIC in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedEarthmatInterConfidence))
            {
                _earthmatInterConfidence.Add(itemIC);
            }


            //Update UI
            RaisePropertyChanged("EarthmatInterConfidence");
            RaisePropertyChanged("SelectedEarthmatInterConfidence");
        }

        /// <summary>
        /// will fill in the mag qualifier combobox
        /// </summary>
        public void FillMagQualifier()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldEarthMatMagQualifier;
            string tableName = Dictionaries.DatabaseLiterals.TableEarthMat;
            foreach (var itemIC in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedEarthmatMagQualifier))
            {
                _earthmatMagQualifier.Add(itemIC);
            }


            //Update UI
            RaisePropertyChanged("MagQualifier");
            RaisePropertyChanged("SelectedMagQualifier");
        }

        /// <summary>
        /// will fill in metamorphic facies intensity combobox
        /// </summary>
        public void FillMetaIntensity()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldEarthMatMetaIntensity;
            string tableName = Dictionaries.DatabaseLiterals.TableEarthMat;
            foreach (var itemIC in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedEarthmatMI))
            {
                _earthmatMI.Add(itemIC);
            }


            //Update UI
            RaisePropertyChanged("EarthmatMI");
            RaisePropertyChanged("SelectedEarthmatMI");
        }

        /// <summary>
        /// Will fill in metamorphic facies combobox
        /// </summary>
        public void FillMetaFacies()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldEarthMatMetaFacies;
            string tableName = Dictionaries.DatabaseLiterals.TableEarthMat;
            foreach (var itemIF in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedEarthmatMF))
            {
                _earthmatMF.Add(itemIF);
            }


            //Update UI
            RaisePropertyChanged("EarthmatMF");
            RaisePropertyChanged("SelectedEarthmatMF");
        }

        private void FillSurficialLitho()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldEarthMatLithdetail;
            string tableName = Dictionaries.DatabaseLiterals.TableEarthMat;
            foreach (var itemMU in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedEarthmatSurfLitho))
            {
                _earthmatSurfLitho.Add(itemMU);
            }


            //Update UI
            RaisePropertyChanged("EarthmatSurfLitho");
            RaisePropertyChanged("SelectedEarthmatSurfLitho");
        }
        private void FillSorting()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldEarthMatSorting;
            string tableName = Dictionaries.DatabaseLiterals.TableEarthMat;
            foreach (var itemMU in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedEarthmatSorting))
            {
                _earthmatSorting.Add(itemMU);
            }


            //Update UI
            RaisePropertyChanged("EarthmatSorting");
            RaisePropertyChanged("SelectedEarthmatSorting");
        }

        private void FillWater()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldEarthMatH2O;
            string tableName = Dictionaries.DatabaseLiterals.TableEarthMat;
            foreach (var itemMU in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedEarthmatWater))
            {
                _earthmatWater.Add(itemMU);
            }


            //Update UI
            RaisePropertyChanged("EarthmatWater");
            RaisePropertyChanged("SelectedEarthmatWater");
        }

        private void FillOxi()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldEarthMatOxidation;
            string tableName = Dictionaries.DatabaseLiterals.TableEarthMat;
            foreach (var itemMU in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedEarthmatOxi))
            {
                _earthmatOxi.Add(itemMU);
            }


            //Update UI
            RaisePropertyChanged("EarthmatOxi");
            RaisePropertyChanged("SelectedEarthmatOxi");
        }

        private void FillClast()
        {
            //Init.
            string fieldName = Dictionaries.DatabaseLiterals.FieldEarthMatClastForm;
            string tableName = Dictionaries.DatabaseLiterals.TableEarthMat;
            foreach (var itemMU in accessData.GetComboboxListWithVocab(tableName, fieldName, out _selectedEarthmatClast))
            {
                _earthmatClast.Add(itemMU);
            }


            //Update UI
            RaisePropertyChanged("EarthmatClast");
            RaisePropertyChanged("SelectedEarthmatClast");
        }
        #endregion

        #region QUICKIE

        /// <summary>
        /// Will create a quick earthmat record inside earthmat table, from a given map position.
        /// A quick station and location will be created also.
        /// </summary>
        /// <param name="inPosition"></param>
        /// <returns>A detail report class</returns>
        public FieldNotes QuickEarthmat(FieldLocation inPosition)
        {
            //Create a quick station
            ViewModels.StationViewModel quickStationVM = new StationViewModel(false);
            FieldNotes quickStationReport = quickStationVM.QuickStation(inPosition);

            //Get current class information and add to model
            earthmodel.EarthMatID = _earthmatid; //Prime key
            earthmodel.EarthMatName = _alias = idCalculator.CalculateEarthmatnAlias(quickStationReport.station.StationID, quickStationReport.station.StationAlias);
            earthmodel.EarthMatStatID = quickStationReport.station.StationID; //Foreign key

            //Save model class
            object earthObject = (object)earthmodel;
            accessData.SaveFromSQLTableObject(ref earthObject, false);
            earthmodel = (EarthMaterial)earthObject;

            FieldNotes outputEarthmatReport = new FieldNotes();
            outputEarthmatReport.earthmat = earthmodel;
            outputEarthmatReport.ParentID = quickStationReport.station.StationID;
            outputEarthmatReport.ParentTableName = Dictionaries.DatabaseLiterals.TableStation;
            outputEarthmatReport.GenericID = earthmodel.EarthMatID;

            return outputEarthmatReport;
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

            if (parentListViewName.ToLower().Contains(Dictionaries.DatabaseLiterals.FieldEarthMatModTextStruc.ToLower()))
            {
                _earthmatModTextureValues.Remove(oldValue);
                RaisePropertyChanged("EarthmatModTextureValues");
            }
            if (parentListViewName.ToLower().Contains(Dictionaries.DatabaseLiterals.FieldEarthMatModComp.ToLower()))
            {
                _earthmatModCompValues.Remove(oldValue);
                RaisePropertyChanged("EarthmatModCompValues");
            }
            if (parentListViewName.ToLower().Contains(Dictionaries.DatabaseLiterals.FieldEarthMatGrSize.ToLower()))
            {
                _earthmatGrSizeValues.Remove(oldValue);
                RaisePropertyChanged("EarthmatGrSizeValues");
            }
            if (parentListViewName.ToLower().Contains(Dictionaries.DatabaseLiterals.FieldEarthMatDefabric.ToLower()))
            {
                _earthmatDefFabricValues.Remove(oldValue);
                RaisePropertyChanged("EarthmatDefFabricValues");
            }
            if (parentListViewName.ToLower().Contains(Dictionaries.DatabaseLiterals.FieldEarthMatBedthick.ToLower()))
            {
                _earthmatBedthickValues.Remove(oldValue);
                RaisePropertyChanged("EarthmatBedthickValues");
            }
            if (parentListViewName.ToLower().Contains(Dictionaries.DatabaseLiterals.FieldMineral.ToLower()))
            {
                _earthmatMineralValues.Remove(oldValue);
                RaisePropertyChanged("EarthmatMineralValues");
            }
            if (parentListViewName.ToLower().Contains(Dictionaries.DatabaseLiterals.FieldEarthMatContactDeprecated.ToLower()))
            {
                _earthmatContactNoteTypes.Remove(oldValue);
                RaisePropertyChanged("EarthmatContactNoteTypes");
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
        /// Catch submission event to track user input and add it to listview with all terms.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void EarthMineralAutoSuggest_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            AutoSuggestBox senderBox = sender as AutoSuggestBox;
            if (args.ChosenSuggestion != null && args.ChosenSuggestion.ToString() != "No results found")
            {
                Themes.ComboBoxItem selectedMineral = args.ChosenSuggestion as Themes.ComboBoxItem;
                AddAConcatenatedValue(selectedMineral.itemValue, senderBox.Name);
                //EarthMineralConcat.Items.Add(selectedMineral.itemValue);
                senderBox.Text = string.Empty;

                // Close the suggestion list
                senderBox.IsSuggestionListOpen = false;
                senderBox.Focus(FocusState.Programmatic);
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

                if (NameToValidate.ToLower().Contains(Dictionaries.DatabaseLiterals.FieldEarthMatModTextStruc.ToLower()))
                {
                    parentCollection = EarthmatModTexture;
                    parentConcatCollection = _earthmatModTextureValues;
                    parentProperty = "EarthmatModTexture";
                }
                if (NameToValidate.ToLower().Contains(Dictionaries.DatabaseLiterals.FieldEarthMatModComp.ToLower()))
                {
                    parentCollection = EarthmatModComp;
                    parentConcatCollection = _earthmatModCompValues;
                    parentProperty = "EarthmatModComp";
                }
                if (NameToValidate.ToLower().Contains(Dictionaries.DatabaseLiterals.FieldEarthMatGrSize.ToLower()))
                {
                    parentCollection = EarthmatGrSize;
                    parentConcatCollection = _earthmatGrSizeValues;
                    parentProperty = "EarthmatGrSize";
                }
                if (NameToValidate.ToLower().Contains(Dictionaries.DatabaseLiterals.FieldEarthMatDefabric.ToLower()))
                {
                    parentCollection = EarthmatDefFabric;
                    parentConcatCollection = _earthmatDefFabricValues;
                    parentProperty = "EarthmatDefFabric";
                }
                if (NameToValidate.ToLower().Contains(Dictionaries.DatabaseLiterals.FieldEarthMatBedthick.ToLower()))
                {
                    parentCollection = EarthmatBedthick;
                    parentConcatCollection = _earthmatBedthickValues;
                    parentProperty = "EarthmatBedthick";
                }
                if (NameToValidate.ToLower().Contains(Dictionaries.DatabaseLiterals.FieldMineral.ToLower()))
                {
                    parentCollection = EarthmatMineral;
                    parentConcatCollection = _earthmatMineralValues;
                    parentProperty = "EarthmatMineral";

                }
                if (NameToValidate.ToLower().Contains(Dictionaries.DatabaseLiterals.FieldEarthMatContactDeprecated.ToLower()))
                {
                    parentCollection = EarthmatContactNoteTypes;
                    parentConcatCollection = _earthmatContactNoteTypes;
                    parentProperty = "EarthmatContactNoteTypes";

                    newValue.itemName = valueToAdd;

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

        /// <summary>
        /// Will take all values from concatenated list views and pipe them as a string
        /// to be able to save them all in the database
        /// </summary>
        /// <returns></returns>
        public string PipePurposes(ObservableCollection<Themes.ComboBoxItem> inCbox)
        {
            string _concatenatedValue = string.Empty;

            foreach (Themes.ComboBoxItem vals in inCbox)
            {
                if (_concatenatedValue == string.Empty)
                {
                    _concatenatedValue = vals.itemValue;
                }
                else
                {
                    _concatenatedValue = _concatenatedValue + Dictionaries.DatabaseLiterals.KeywordConcatCharacter + vals.itemValue;
                }
            }

            return _concatenatedValue;
        }

        /// <summary>
        /// Will take an input piped value and make it a list of strings.
        /// </summary>
        /// <param name="inValue"></param>
        public void UnPipeValues(string inValue, string databaseTableField)
        {
            List<string> unpipedValues = new List<string>();

            //Clean values first
            ObservableCollection<Themes.ComboBoxItem> collectionToClean = null;
            if (databaseTableField.ToLower().Contains(Dictionaries.DatabaseLiterals.FieldEarthMatModTextStruc.ToLower()))
            {
                collectionToClean = _earthmatModTextureValues;
            }
            if (databaseTableField.ToLower().Contains(Dictionaries.DatabaseLiterals.FieldEarthMatModComp.ToLower()))
            {
                collectionToClean = _earthmatModCompValues;
            }
            if (databaseTableField.ToLower().Contains(Dictionaries.DatabaseLiterals.FieldEarthMatGrSize.ToLower()))
            {
                collectionToClean = _earthmatGrSizeValues;
            }
            if (databaseTableField.ToLower().Contains(Dictionaries.DatabaseLiterals.FieldEarthMatDefabric.ToLower()))
            {
                collectionToClean = _earthmatDefFabricValues;
            }
            if (databaseTableField.ToLower().Contains(Dictionaries.DatabaseLiterals.FieldEarthMatBedthick.ToLower()))
            {
                collectionToClean = _earthmatBedthickValues;
            }
            if (databaseTableField.ToLower().Contains(Dictionaries.DatabaseLiterals.FieldEarthMatContact.ToLower()))
            {
                collectionToClean = _earthmatContactNoteTypes;
            }
            collectionToClean.Clear();

            if (inValue != null)
            {
                unpipedValues = inValue.Split(Dictionaries.DatabaseLiterals.KeywordConcatCharacter.Trim().ToCharArray()).ToList();
                foreach (string un in unpipedValues)
                {
                    AddAConcatenatedValue(un.Trim(), null, databaseTableField);
                }
            }

        }

        #endregion

        #region EVENTS

        public void EarthPercent_TextChanged(object sender, TextBoxTextChangingEventArgs e)
        {
            TextBox senderBox = sender as TextBox;
            if (senderBox != null && senderBox.Text != string.Empty)
            {
                CalculateResidual(senderBox.Text);
            }
        }

        /// <summary>
        /// Will set fresh color textbox with proper entries from 3 colour boxes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ColourSetFresh_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _earthColourF.generic = _selectedEarthmatColourG;
            _earthColourF.intensity = _selectedEarthmatColourI;
            _earthColourF.qualifier = _selectedEarthmatColourQ;

            RaisePropertyChanged("EarthColourF");
        }

        /// <summary>
        /// Will set weathered color textbox with proper entries from 3 colour boxes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ColourSetWeathered_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _earthColourW.generic = _selectedEarthmatColourG;
            _earthColourW.intensity = _selectedEarthmatColourI;
            _earthColourW.qualifier = _selectedEarthmatColourQ;

            RaisePropertyChanged("EarthColourW");

        }

        /// <summary>
        /// Will add a new contact object into contact list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void EarthContactSelectionButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (_selectedEarthmatRelated != null && _selectedEarthmatRelated != String.Empty)
            {
                _earthContact.type = _selectedEarthmatContactTypes;

                EarthMaterial em = new EarthMaterial();
                em.EarthMatName = _selectedEarthmatRelated;
                _earthContact.relatedEarthMaterialID = em.GetIDLetter;

                AddAConcatenatedValue(_earthContact.ToString(), "EarthmatContactNoteTypes");
            }



        }

        #endregion

        #region CALCULATE
        public void CalculateResidual(string newMode = "")
        {
            // Language localization using Resource.resw
            var loadLocalization = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            string Prefix = loadLocalization.GetString("EarthResidualLabelPrefix");
            string MiddleFix = loadLocalization.GetString("EarthResidualLabelMiddlefix");
            string Suffix = loadLocalization.GetString("EarthResidualLabelSuffix");

            List<object> earthmatTableRaw = accessData.ReadTable(earthmodel.GetType(), null);
            IEnumerable<EarthMaterial> earthTable = earthmatTableRaw.Cast<EarthMaterial>(); //Cast to proper list type

            //Get a list of earth material siblings
            int parentID = existingDataDetail.ParentID;

            //Filter with proper parents
            IEnumerable<EarthMaterial> earthParent = from e in earthTable where e.EarthMatStatID == parentID select e;
            if (existingDataDetail.ParentTableName == DatabaseLiterals.TableDrillHoles)
            {
                earthParent = from e in earthTable where e.EarthMatDrillHoleID == parentID select e;
            }
            else if (existingDataDetail.GenericTableName == DatabaseLiterals.TableDrillHoles)
            {
                earthParent = from e in earthTable where e.EarthMatDrillHoleID == existingDataDetail.GenericID select e;
            }
            else if (existingDataDetail.GenericTableName == DatabaseLiterals.TableStation)
            {
                earthParent = from e in earthTable where e.EarthMatStatID == existingDataDetail.GenericID select e;
            }
            

            if (_earthResidualPercent.Count == 0 && (earthParent.Count() != 0 || earthParent != null))
            {
                foreach (EarthMaterial ets in earthParent)
                {
                    // _minerals.Add(ets.EarthMatID);

                    int currentPercentage = 0;
                    if (ets.EarthMatPercent != null)
                    {
                        currentPercentage = (int)ets.EarthMatPercent;
                    }
                    ;
                    bool currentPercentParsed = true;
                    if (ets.EarthMatID == existingDataDetail.GenericID)
                    {
                        if (newMode != string.Empty)
                        {
                            currentPercentParsed = int.TryParse(newMode, out currentPercentage);
                        }

                        if (currentPercentParsed)
                        {
                            _earthResidualPercent[ets.EarthMatID] = currentPercentage;
                        }

                    }
                    else
                    {
                        if (currentPercentParsed)
                        {
                            _earthResidualPercent[ets.EarthMatID] = currentPercentage;
                        }

                    }



                }

                if (_earthResidualPercent.Count() == 0)
                {
                    int currentPercentage = 0;
                    bool currentModeParsed = int.TryParse(newMode, out currentPercentage);
                    _earthResidualPercent[existingDataDetail.GenericID] = currentPercentage;
                }

            }
            else
            {
                int currentPercentage = 0;
                bool currentModeParsed = int.TryParse(newMode, out currentPercentage);
                _earthResidualPercent[existingDataDetail.GenericID] = currentPercentage;
            }


            //Calculate total percentage
            int _earthResidual = 0;
            foreach (KeyValuePair<int, int> modes in _earthResidualPercent)
            {
                _earthResidual = _earthResidual + modes.Value;
            }
            _earthResidualText = Prefix + _earthResidual.ToString() + MiddleFix + _earthResidualPercent.Count().ToString() + Suffix;
            RaisePropertyChanged("EarthResidualText");

        }
        #endregion
    }


}
