using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Template10.Mvvm;
using GSCFieldApp.Models;
using GSCFieldApp.Dictionaries;
using GSCFieldApp.Services.DatabaseServices;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using Template10.Controls;
using Windows.UI;

namespace GSCFieldApp.ViewModels
{
    public class FieldNotesViewModel : ViewModelBase //ViewModelBase already contains INotifyPropertyChanged that is used to update the listview control
    {
        #region INIT.

        #region DECLARE

        //App
        readonly DataAccess dAccess = new DataAccess();
        readonly DataLocalSettings localSetting = new DataLocalSettings();
        public DataIDCalculation idCalculator = new DataIDCalculation();

        /// <summary>
        /// A collection of observation class to display on screen in the report page.
        /// </summary>
        private readonly ObservableCollection<FieldNotes> _reportSummaryDateItems = new ObservableCollection<FieldNotes>();
        public ObservableCollection<FieldNotes> _reportDetailedStation = new ObservableCollection<FieldNotes>(); //Will be use for right panel.
        private readonly ObservableCollection<FieldNotes> _reportDetailedEarthmat = new ObservableCollection<FieldNotes>(); //Will be use for right panel.
        private readonly ObservableCollection<FieldNotes> _reportDetailSample = new ObservableCollection<FieldNotes>(); //Will be use for right panel.
        private readonly ObservableCollection<FieldNotes> _reportDetailedDocument = new ObservableCollection<FieldNotes>(); //Will be use for right panel.
        private readonly ObservableCollection<FieldNotes> _reportDetailedStructure = new ObservableCollection<FieldNotes>(); //Will be use for right panel.
        private readonly ObservableCollection<FieldNotes> _reportDetailedPflow = new ObservableCollection<FieldNotes>(); //Will be use for right panel.
        private readonly ObservableCollection<FieldNotes> _reportDetailedFossil = new ObservableCollection<FieldNotes>(); //Will be use for right panel.
        private readonly ObservableCollection<FieldNotes> _reportDetailedLocation = new ObservableCollection<FieldNotes>(); //Will be use for right panel.
        private readonly ObservableCollection<FieldNotes> _reportDetailedMinerals = new ObservableCollection<FieldNotes>(); //Will be use for right panel.
        private readonly ObservableCollection<FieldNotes> _reportDetailedMineralAlt = new ObservableCollection<FieldNotes>(); //Will be use for right panel.
        private readonly ObservableCollection<FieldNotes> _reportDetailEnvironment = new ObservableCollection<FieldNotes>(); //WIll be used for right panel

        public FieldNotes selectedSummaryDateItem = new FieldNotes();
        public int _reportDateIndex = -1;
        public int _reportStationIndex = -1;
        public int _reportEarthmatIndex = -1;
        public int _reportSampleIndex = -1;
        public int _reportDocumentIndex = -1;
        public int _reportStructureIndex = -1;
        public int _reportPflowIndex = -1;
        public int _reportFossilIndex = -1;
        public int _reportLocationIndex = -1;
        public int _reportMineralIndex = -1;
        public int _reportMineralizationAlterationIndex = -1;
        public int _reportEnvironmentIndex = -1;

        //Some model inits
        private readonly Station stationModel = new Station();
        private readonly EarthMaterial earthModel = new EarthMaterial();
        private readonly Sample sampleModel = new Sample();
        private readonly Document documentModel = new Document();
        private readonly Structure structureModel = new Structure();
        private readonly Paleoflow pflowModel = new Paleoflow();
        private readonly Fossil fossilModel = new Fossil();
        private readonly FieldLocation locationModel = new FieldLocation();
        private readonly Mineral mineralModel = new Mineral();
        private readonly MineralAlteration mineralAltModel = new MineralAlteration();
        private readonly EnvironmentModel environmentModel = new EnvironmentModel();  

        //UI interaction
        private string deleteRequestFromTable = string.Empty;

        //UI
        private bool _locationHeaderExpansion = true;
        private bool _earthmatHeaderExpansion = true;
        private bool _sampleHeaderExpansion = true;
        private bool _stationHeaderExpansion = true;
        private bool _documentHeaderExpansion = true;
        private bool _structureHeaderExpansion = true;
        private bool _pflowHeaderExpansion = true;
        private bool _fossilHeaderExpansion = true;
        private bool _mineralHeaderExpansion = true;
        private bool _mineralAltHeaderExpansion = true;
        private bool _environmentHeaderExpansion = true;

        private Visibility _samplePanelVisibility = Visibility.Visible;
        private Visibility _earthmatPanelVisibility = Visibility.Visible;
        private Visibility _mineralPanelVisibility = Visibility.Visible;
        private Visibility _mineralAltPanelVisibility = Visibility.Visible;
        private Visibility _environmentPanelVisibility = Visibility.Visible;
        private Visibility _documentPanelVisibility = Visibility.Visible;
        private Visibility _structurePanelVisibility = Visibility.Visible;
        private Visibility _pflowPanelVisibility = Visibility.Visible;
        private Visibility _fossilPanelVisibility = Visibility.Visible;


        private const double disableOpacity = 0.25;
        private const double enableOpacity = 1;
        private const double fullDisableOpacity = 0;
        private int earthmatRecordCount = 0;
        private int sampleRecordCount = 0;
        private int stationLocationRowCount = 0;
        private int documentRecordCount = 0;
        private int structureRecordCount = 0;
        private int pflowRecordCount = 0;
        private int fossilRecordCount = 0;
        private int mineralRecordCount = 0;
        private int mineralAltRecordCount = 0;
        private int environmentRecordCound = 0;


        //UI Text
        private string _headerDocumenText = ApplicationLiterals.KeywordDocumentHeaderFalse;

        //UI icons
        private double _stationIconOpacity = disableOpacity;
        private double _earthmatIconOpacity = disableOpacity;
        private double _sampleIconOpacity = disableOpacity;
        private double _mineralIconOpacity = disableOpacity;
        private double _mineralAddIconOpacity = disableOpacity;
        private double _documentIconOpacity = disableOpacity;
        private double _documentAddIconOpacity = disableOpacity;
        private double _mineralAltIconOpacity = disableOpacity;
        private double _locationAddIconOpacity = disableOpacity;
        private double _mineralAltAddIconOpacity = disableOpacity;
        private double _sampleAddIconOpacity = disableOpacity;
        private double _locationIconOpacity = disableOpacity;
        private double _earthmatAddIconOpacity = disableOpacity;
        private double _structureIconOpacity = disableOpacity;
        private double _structureAddIconOpacity = disableOpacity;
        private double _pflowIconOpacity = disableOpacity;
        private double _pflowAddIconOpacity = disableOpacity;
        private double _fossilIconOpacity = disableOpacity;
        private double _fossilAddIconOpacity = disableOpacity;
        private double _environmentIconOpacity = disableOpacity;
        private double _environmentAddIconOpacity = disableOpacity;

        //UI headers vertical colored bar
        private double _earthmatIconColorOpacity = enableOpacity;
        private double _sampleIconColorOpacity = enableOpacity;
        private double _stationIconColorOpacity = fullDisableOpacity;
        private double _documentIconColorOpacity = enableOpacity;
        private double _structureIconColorOpacity = enableOpacity;
        private double _pflowIconColorOpacity = enableOpacity;
        private double _fossilIconColorOpacity = enableOpacity;
        private double _mineralIconColorOpacity = enableOpacity;
        private double _locationIconColorOpacity = enableOpacity;
        private double _mineralAltIconColorOpacity = enableOpacity;
        private double _environmentIconColorOpacity = enableOpacity;

        //UI headers enable/disable colors
        private readonly string resourceNameDisableColor = "DisableColor";
        private readonly string resourcenameFieldEarthmatColor = "FieldEarthMaterialColor";
        private readonly string resourcenameFieldSampleColor = "FieldSampleColor";
        private readonly string resourcenameFieldDocumentColor = "FieldPhotoColor";
        private readonly string resourcenameFieldStructColor = "FieldStrucColor";
        private readonly string resourcenameFieldMineralColor = "FieldMineralColor";
        private readonly string resourcenameFieldPflowColor = "FieldPflowColor";
        private readonly string resourcenameFieldFossilColor = "FieldFossilColor";
        private readonly string resourcenameFieldStationColor = "FieldStationColor";
        private readonly string resourcenameFieldLocationColor = "FieldObservationColor";
        private readonly string resourcenameFieldMineralAltColor = "FieldMineralAlterationColor";
        private readonly string resourcenameFieldEnvironmentColor = "FieldEnvironmentColor";

        private SolidColorBrush _earthmatColor = new SolidColorBrush();
        private SolidColorBrush _sampleColor = new SolidColorBrush();
        private SolidColorBrush _structColor = new SolidColorBrush();
        private SolidColorBrush _fossilColor = new SolidColorBrush();
        private SolidColorBrush _paleoflowColor = new SolidColorBrush();
        private SolidColorBrush _mineralColor = new SolidColorBrush();
        private SolidColorBrush _documentColor = new SolidColorBrush();
        private SolidColorBrush _stationColor = new SolidColorBrush();
        private SolidColorBrush _locationColor = new SolidColorBrush();
        private SolidColorBrush _mineralAltColor = new SolidColorBrush();
        private SolidColorBrush _envColor = new SolidColorBrush();

        private SolidColorBrush _locationAddIconColor = new SolidColorBrush();
        private SolidColorBrush _earthmatAddIconColor = new SolidColorBrush();
        private SolidColorBrush _documentAddIconColor = new SolidColorBrush();
        private SolidColorBrush _sampleAddIconColor = new SolidColorBrush();
        private SolidColorBrush _mineralAddIconColor = new SolidColorBrush();
        private SolidColorBrush _structureAddIconColor = new SolidColorBrush();
        private SolidColorBrush _pflowAddIconColor = new SolidColorBrush();
        private SolidColorBrush _fossilAddIconColor = new SolidColorBrush();
        private SolidColorBrush _mineralAltIconColor = new SolidColorBrush();
        private SolidColorBrush _mineralAltAddIconColor = new SolidColorBrush();
        private SolidColorBrush _environmentIconColor = new SolidColorBrush();
        private SolidColorBrush _environmentAddIconColor = new SolidColorBrush();

        //Map page
        public string userSelectedStationID = string.Empty;
        public string userSelectedStationDate = string.Empty;

        //Events and delegate
        public delegate void summaryFinishLoadedEventHandler(object sender); //A delegate for execution events
        public event summaryFinishLoadedEventHandler summaryDone; //This event is triggered when favorite attribute has to change in UI
        public bool pageLoading = false;

        //Validity check
        public Dictionary<string, List<string>> summaryReportValidity = new Dictionary<string, List<string>>();

        #endregion

        #region PROPERTIES

        #region Will enable or disable expansion for tables that are below stations

        public bool LocationHeaderExpansion { get { return _locationHeaderExpansion; } set { _locationHeaderExpansion = value; localSetting.SetSettingValue(Dictionaries.ApplicationLiterals.KeyworkdExpandLocation,value); } }
        public bool SampleHeaderExpansion { get { return _sampleHeaderExpansion; } set { _sampleHeaderExpansion = value; localSetting.SetSettingValue(Dictionaries.ApplicationLiterals.KeyworkdExpandSample, value); } }
        public bool StationHeaderExpansion { get { return _stationHeaderExpansion; } set { _stationHeaderExpansion = value; localSetting.SetSettingValue(Dictionaries.ApplicationLiterals.KeyworkdExpandStation, value); } }
        public bool EarthmatHeaderExpansion { get { return _earthmatHeaderExpansion; } set { _earthmatHeaderExpansion = value; localSetting.SetSettingValue(Dictionaries.ApplicationLiterals.KeyworkdExpandEarthmat, value); } }
        public bool DocumentHeaderExpand { get { return _documentHeaderExpansion; } set { _documentHeaderExpansion = value; localSetting.SetSettingValue(Dictionaries.ApplicationLiterals.KeywordExpandDocument, value); } }
        public bool StructureHeaderExpansion { get { return _structureHeaderExpansion; } set { _structureHeaderExpansion = value; localSetting.SetSettingValue(Dictionaries.ApplicationLiterals.KeywordExpandStructure, value); } }
        public bool PflowHeaderExpansion { get { return _pflowHeaderExpansion; } set { _pflowHeaderExpansion = value; localSetting.SetSettingValue(Dictionaries.ApplicationLiterals.KeywordExpandPflow, value); } }
        public bool FossilHeaderExpansion { get { return _fossilHeaderExpansion; } set { _fossilHeaderExpansion = value; localSetting.SetSettingValue(Dictionaries.ApplicationLiterals.KeywordExpandFossil, value); } }
        public bool MineralHeaderExpansion { get { return _mineralHeaderExpansion; } set { _mineralHeaderExpansion = value; localSetting.SetSettingValue(Dictionaries.ApplicationLiterals.KeywordExpandMineral, value); } }
        public bool MineralAltHeaderExpansion { get { return _mineralAltHeaderExpansion; } set { _mineralAltHeaderExpansion = value; localSetting.SetSettingValue(Dictionaries.ApplicationLiterals.KeywordExpandMineralAlt, value); } }
        public bool EnvironmentHeaderExpander { get { return _earthmatHeaderExpansion; } set { _earthmatHeaderExpansion = value; localSetting.SetSettingValue(Dictionaries.ApplicationLiterals.KeywordExpandMineralAlt, value); } }

        #endregion

        #region Observable collections

        public ObservableCollection<FieldNotes> ReportDateItems
        {
            get
            {
                return _reportSummaryDateItems;
            }

        }

        /// <summary>
        /// A property that will hold all the selected report item details.
        /// </summary>
        public ObservableCollection<FieldNotes> ReportDetailedStation
        {
            get
            {
                return _reportDetailedStation;
            }
        }

        /// <summary>
        /// A property that will hold all the selected report item details.
        /// </summary>
        public ObservableCollection<FieldNotes> ReportDetailedEarthmat
        {
            get
            {
                return _reportDetailedEarthmat;
            }
        }

        /// <summary>
        /// A property that will hold all the selected report item details.
        /// </summary>
        public ObservableCollection<FieldNotes> ReportDetailedSample
        {
            get
            {
                return _reportDetailSample;
            }
        }

        /// <summary>
        /// A property that will hold all the selected report item details
        /// </summary>
        public ObservableCollection<FieldNotes> ReportDetailedDocument
        {
            get { return _reportDetailedDocument; }
        }

        /// <summary>
        /// A property that will hold all the selected report item details for structure table.
        /// </summary>
        public ObservableCollection<FieldNotes> ReportDetailedStructure
        {
            get { return _reportDetailedStructure; }
        }
        /// <summary>
        /// A property that will hold all the selected report item details for pflow table.
        /// </summary>
        public ObservableCollection<FieldNotes> ReportDetailedPflow
        {
            get { return _reportDetailedPflow; }
        }
        /// <summary>
        /// A property that will hold all the selected report item details for fossil table.
        /// </summary>
        public ObservableCollection<FieldNotes> ReportDetailedFossil
        {
            get { return _reportDetailedFossil; }
        }
        /// <summary>
        /// A property that will hold all the selected report item details for location table.
        /// </summary>
        public ObservableCollection<FieldNotes> ReportDetailedLocation
        {
            get { return _reportDetailedLocation; }
        }
        /// <summary>
        /// A property that will hold all the selected report item details for mineral table.
        /// </summary>
        public ObservableCollection<FieldNotes> ReportDetailedMineral
        {
            get { return _reportDetailedMinerals; }
        }
        /// <summary>
        /// A property that will hold all the selected report item details for mineral table.
        /// </summary>
        public ObservableCollection<FieldNotes> ReportDetailedMineralAlt
        {
            get { return _reportDetailedMineralAlt; }
        }

        /// <summary>
        /// A property that will hold all the selected report item details for mineral table.
        /// </summary>
        public ObservableCollection<FieldNotes> ReportDetailEnvironment
        {
            get { return _reportDetailEnvironment; }
        }
        #endregion

        #region Indexes
        /// <summary>
        /// A property to hold the report list selected index. For initialize purposes. At init, when the list is build the first
        /// item should be selected and show it's associated detail item in the right panel.
        /// </summary>
        public int ReportListViewDateIndex
        {
            get { return _reportDateIndex; }
            set { _reportDateIndex = value; RaisePropertyChanged("ReportListViewIndex"); }
        }
        public int ReportStationListIndex
        {
            get { return _reportStationIndex; }
            set { _reportStationIndex = value;  }
        } //Same thing as ReportListViewIndex, but for station list view refresh
        public int ReportEarhtmatIndex
        {
            get { return _reportEarthmatIndex; }
            set { _reportEarthmatIndex = value; }
        } //Same thing as ReportListViewIndex, but for station list view refresh
        public int ReportSampleIndex
        {
            get { return _reportSampleIndex; }
            set { _reportSampleIndex = value; }
        } //Same thing as ReportListViewIndex, but for station list view refresh
        public int ReportFossilIndex
        {
            get { return _reportFossilIndex; }
            set { _reportFossilIndex = value; }
        } //Same thing as ReportListViewIndex, but for station list view refresh
        public int ReportPflowIndex
        {
            get { return _reportPflowIndex; }
            set { _reportPflowIndex = value; }
        } //Same thing as ReportListViewIndex, but for station list view refresh
        public int ReportMineralsIndex
        {
            get { return _reportMineralIndex; }
            set { _reportMineralIndex = value; }
        } //Same thing as ReportListViewIndex, but for station list view refresh
        public int ReportMineralizationAlterationIndex
        {
            get { return _reportMineralizationAlterationIndex; }
            set { _reportMineralizationAlterationIndex = value; }
        } //Same thing as ReportListViewIndex, but for station list view refresh
        public int ReportDocumentIndex
        {
            get { return _reportDocumentIndex; }
            set { _reportDocumentIndex = value; }
        } //Same thing as ReportListViewIndex, but for station list view refresh
        public int ReportLocationIndex
        {
            get { return _reportLocationIndex; }
            set { _reportLocationIndex = value; }
        } //Same thing as ReportListViewIndex, but for station list view refresh
        public int ReportStructureIndex
        {
            get { return _reportStructureIndex; }
            set { _reportStructureIndex = value; }
        } //Same thing as ReportListViewIndex, but for station list view refresh
        public int ReportEnvironmentIndex
        {
            get { return _reportEnvironmentIndex; }
            set { _reportEnvironmentIndex = value; }
        } //Same thing as ReportListViewIndex, but for station list view refresh


        #endregion

        #region Visibility
        public Visibility SamplePanelVisibility { get { return _samplePanelVisibility; } set { _samplePanelVisibility = value; } }
        public Visibility EarthmatPanelVisibility { get { return _earthmatPanelVisibility; } set { _earthmatPanelVisibility = value; } }
        public Visibility MineralPanelVisibility { get { return _mineralPanelVisibility; } set { _mineralPanelVisibility = value; } }
        public Visibility MineralAltPanelVisibility { get { return _mineralAltPanelVisibility; } set { _mineralAltPanelVisibility = value; } }
        public Visibility EnvironmentPanelVisibility { get { return _environmentPanelVisibility; } set { _environmentPanelVisibility = value; } }
        public Visibility PhotoPanelVisibility { get { return _documentPanelVisibility; } set { _documentPanelVisibility = value; } }
        public Visibility StructurePanelVisibility { get { return _structurePanelVisibility; } set { _structurePanelVisibility = value; } }
        public Visibility PFlowPanelVisibility { get { return _pflowPanelVisibility; } set { _pflowPanelVisibility = value; } }
        public Visibility FossilPanelVisibility { get { return _fossilPanelVisibility; } set { _fossilPanelVisibility = value; } }
        #endregion

        #region Opacity/Color (icons)
        public double StationIconOpacity { get { return _stationIconOpacity; } set { _stationIconOpacity = value; } }
        public double StationIconColorOpacity { get { return _stationIconColorOpacity; } set { _stationIconColorOpacity = value; } }

        public double EarthmatIconOpacity { get { return _earthmatIconOpacity; } set { _earthmatIconOpacity = value; } }
        public double EarthmatAddIconOpacity { get { return _earthmatAddIconOpacity; } set { _earthmatAddIconOpacity = value; } }
        public double EarthmatIconColorOpacity { get { return _earthmatIconColorOpacity; } set { _earthmatIconColorOpacity = value; } }

        public double SampleIconOpacity { get { return _sampleIconOpacity; } set { _sampleIconOpacity = value; } }
        public double SampleAddIconOpacity { get { return _sampleAddIconOpacity; } set { _sampleAddIconOpacity = value; } }
        public double SampleIconColorOpacity { get { return _sampleIconColorOpacity; } set { _sampleIconColorOpacity = value; } }

        public double MineralIconOpacity { get { return _mineralIconOpacity; } set { _mineralIconOpacity = value; } }
        public double MineralAddIconOpacity { get { return _mineralAddIconOpacity; } set { _mineralAddIconOpacity = value; } }
        public double MineralIconColorOpacity { get { return _mineralIconColorOpacity; } set { _mineralIconColorOpacity = value; } }

        public double MineralAltIconOpacity { get { return _mineralAltIconOpacity; } set { _mineralAltIconOpacity = value; } }
        public double MineralAltAddIconOpacity { get { return _mineralAltAddIconOpacity; } set { _mineralAltAddIconOpacity = value; } }
        public double MineralAltIconColorOpacity { get { return _mineralAltIconColorOpacity; } set { _mineralAltIconColorOpacity = value; } }

        public double LocationAddIconOpacity { get { return _locationAddIconOpacity; } set { _locationAddIconOpacity = value; } }
        public double LocationIconOpacity { get { return _locationIconOpacity; } set { _locationIconOpacity = value; } }
        public double LocationIconColorOpacity { get { return _locationIconColorOpacity; } set { _locationIconColorOpacity = value; } }

        public double DocumentAddIconOpacity { get { return _documentAddIconOpacity; } set { _documentAddIconOpacity = value; } }
        public double DocumentIconOpacity { get { return _documentIconOpacity; } set { _documentIconOpacity = value; }}
        public double DocumentIconColorOpacity { get { return _documentIconColorOpacity; } set { _documentIconColorOpacity = value; } }

        public double StructureIconOpacity { get { return _structureIconOpacity; } set { _structureIconOpacity = value; } }
        public double StructureAddIconOpacity { get { return _structureAddIconOpacity; } set { _structureAddIconOpacity = value; } }
        public double StructureIconColorOpacity { get { return _structureIconColorOpacity; } set { _structureIconColorOpacity = value; } }

        public double PflowIconOpacity { get { return _pflowIconOpacity; } set { _pflowIconOpacity = value; } }
        public double PflowAddIconOpacity { get { return _pflowAddIconOpacity; } set { _pflowAddIconOpacity = value; } }
        public double PflowIconColorOpacity { get { return _pflowIconColorOpacity; } set { _pflowIconColorOpacity = value; } }

        public double FossilIconOpacity { get { return _fossilIconOpacity; } set { _fossilIconOpacity = value; } }
        public double FossilAddIconOpacity { get { return _fossilAddIconOpacity; } set { _fossilAddIconOpacity = value; } }
        public double FossilIconColorOpacity { get { return _fossilIconColorOpacity; } set { _fossilIconColorOpacity = value; } }

        public double EnvironmentIconOpacity { get { return _environmentIconOpacity; } set { _environmentIconOpacity = value; } }
        public double EnvironmentAddIconOpacity { get { return _environmentAddIconOpacity; } set { _environmentAddIconOpacity = value; } }
        public double EnvironmentIconColorOpacity { get { return _environmentIconColorOpacity; } set { _environmentIconColorOpacity = value; } }


        #endregion

        #region Header colors
        public SolidColorBrush EarthmatColor { get { return _earthmatColor; } set { _earthmatColor = value; } }
        public SolidColorBrush EarthmatAddIconColor { get { return _earthmatAddIconColor; } set { _earthmatAddIconColor = value; } }
        public SolidColorBrush SampleColor { get { return _sampleColor; } set { _sampleColor = value; } }
        public SolidColorBrush SampleAddIconColor { get { return _sampleAddIconColor; } set { _sampleAddIconColor = value; } }
        public SolidColorBrush StructColor { get { return _structColor; } set { _structColor = value; } }
        public SolidColorBrush StructAddIconColor { get { return _structureAddIconColor; } set { _structureAddIconColor = value; } }
        public SolidColorBrush PaleoflowColor { get { return _paleoflowColor; } set { _paleoflowColor = value; } }
        public SolidColorBrush PaleoflowAddIconColor { get { return _pflowAddIconColor; } set { _pflowAddIconColor = value; } }
        public SolidColorBrush MineralColor { get { return _mineralColor; } set { _mineralColor = value; } }
        public SolidColorBrush MineralAddIconColor { get { return _mineralAddIconColor; } set { _mineralAddIconColor = value; } }
        public SolidColorBrush DocumentColor { get { return _documentColor; } set { _documentColor = value; } }
        public SolidColorBrush DocumentAddIconColor { get { return _documentAddIconColor; } set { _documentAddIconColor = value; } }
        public SolidColorBrush FossilColor { get { return _fossilColor; } set { _fossilColor = value; } }
        public SolidColorBrush FossilAddIconColor { get { return _fossilAddIconColor; } set { _fossilAddIconColor = value; } }
        public SolidColorBrush StationColor { get { return _stationColor; } set { _stationColor = value; } }
        public SolidColorBrush LocationColor { get { return _locationColor; } set { _locationColor = value; } }
        public SolidColorBrush LocationAddIconColor { get { return _locationAddIconColor; } set { _locationAddIconColor = value; } }
        public SolidColorBrush MineralAltColor { get { return _mineralAltColor; } set { _mineralAltColor = value; } }
        public SolidColorBrush MineralAltAddIconColor { get { return _mineralAltAddIconColor; } set { _mineralAltAddIconColor = value; } }
        public SolidColorBrush EnvironmentColor { get { return _envColor; } set { _envColor = value; } }
        public SolidColorBrush EnvironmentAddIconColor { get { return _environmentAddIconColor; } set { _environmentAddIconColor = value; } }

        #endregion

        /// <summary>
        /// A property to set document header whether user has selected photo or document mode in the settings
        /// </summary>
        public string HeaderDocument
        {
            get
            {
                if (localSetting.GetSettingValue(Dictionaries.ApplicationLiterals.KeywordDocumentMode) != null)
                {
                    if ((bool)localSetting.GetSettingValue(Dictionaries.ApplicationLiterals.KeywordDocumentMode))
                    {
                        return ApplicationLiterals.KeywordDocumentHeaderFalse;
                    }
                    else
                    {
                        return ApplicationLiterals.KeywordDocumentHeaderTrue;
                    }
                }
                else
                {
                    return ApplicationLiterals.KeywordDocumentHeaderFalse;
                }

            }
            set
            {
                _headerDocumenText = value;
            }
        }
        #endregion

        /// <summary>
        /// Initialize the constructor
        /// </summary>
        public FieldNotesViewModel()
        {
            //Init a new collection of observation
            //_reportSummaryItems = new ObservableCollection<ReportDetailItemViewModel>(); //For split view pane
            _reportSummaryDateItems = new ObservableCollection<FieldNotes>(); //For split view pane
            _reportDetailedStation = new ObservableCollection<FieldNotes>(); //For split view content
            _reportDetailedEarthmat = new ObservableCollection<FieldNotes>();
            _reportDetailSample = new ObservableCollection<FieldNotes>(); //For split view content
            _reportDetailedDocument = new ObservableCollection<FieldNotes>(); //For split view content
            _reportDetailedStructure = new ObservableCollection<FieldNotes>();
            _reportDetailedFossil = new ObservableCollection<FieldNotes>();
            _reportDetailedPflow = new ObservableCollection<FieldNotes>();
            _reportDetailedMinerals = new ObservableCollection<FieldNotes>();
            _reportDetailedLocation = new ObservableCollection<FieldNotes>();
            _reportDetailedMineralAlt = new ObservableCollection<FieldNotes>();
            _reportDetailEnvironment = new ObservableCollection<FieldNotes>();

            //ReportListViewIndex = -1;
            //ReportStationListIndex = -1;
            _reportDateIndex = -1;
            _reportStationIndex = -1;
            _reportEarthmatIndex = -1;
            _reportSampleIndex = -1;
            _reportDocumentIndex = -1;
            _reportStructureIndex = -1;
            _reportPflowIndex = -1;
            _reportFossilIndex = -1;
            _reportLocationIndex = -1;
            _reportMineralIndex = -1;
            _reportMineralizationAlterationIndex = -1;
            _reportEnvironmentIndex = -1;


            SetHeaderColorOpacity(DatabaseLiterals.TableStation);
            SetHeaderColorOpacity(DatabaseLiterals.TableEarthMat);
            SetHeaderColorOpacity(DatabaseLiterals.TableSample);
            SetHeaderColorOpacity(DatabaseLiterals.TablePFlow);
            SetHeaderColorOpacity(DatabaseLiterals.TableFossil);
            SetHeaderColorOpacity(DatabaseLiterals.TableMineral);
            SetHeaderColorOpacity(DatabaseLiterals.TableMineralAlteration);
            SetHeaderColorOpacity(DatabaseLiterals.TableLocation);
            SetHeaderColorOpacity(DatabaseLiterals.TableDocument);
            SetHeaderColorOpacity(DatabaseLiterals.TableStructure);
            SetHeaderColorOpacity(DatabaseLiterals.TableEnvironment);

            //Detect new field book selection
            FieldBooksPageViewModel.newFieldBookSelected += FieldBooksPageViewModel_newFieldBookSelected;
        }



        #endregion

        #region FILL MANAGEMENT

        /// <summary>
        /// Will fill child content if a unselect has occured on a parent
        /// </summary>
        /// <param name="inParentReport"></param>
        public void FillParentContent(FieldNotes inParentReport)
        {
            if (inParentReport != null)
            {
                //If earthmat table is unselected, refill it and everything beneath it so all records for given station are showed.
                if (inParentReport.GenericTableName == DatabaseLiterals.TableEarthMat)
                {
                    FillEarthmatFromStation();
                }

            }
        }

        public void FillSummaryReportDateItems()
        {
            //Get a list of all observations inside fieldwork database
            Station statDate = new Station();
            //ReportDateItems.Clear();
            string visitDateSelectFrom = "SELECT DISTINCT(" + DatabaseLiterals.TableStation + "." + DatabaseLiterals.FieldStationVisitDate + ") FROM " + DatabaseLiterals.TableStation + " ";
            string visitDateWhere = string.Empty;
            //string visitDateOrderBy = "ORDER BY " + DatabaseLiterals.FieldStationAlias + " DESC";
            string visitDateOrderBy = "ORDER BY " + DatabaseLiterals.FieldStationVisitDate + " DESC";
            //string visitDateQueryAll = visitDateSelectFrom + visitDateWhere + visitDateOrderBy; //Will be used for extra validation
            string visitDateQuery = visitDateSelectFrom + visitDateWhere + visitDateOrderBy;

            if (_reportSummaryDateItems.Count > 0)
            {
                //If elements already have been added, build where clause to not retrieve them from query
                bool init = true;

                foreach (FieldNotes existingDates in _reportSummaryDateItems)
                {
                    string where_raw = DatabaseLiterals.FieldStationVisitDate + " <> '" + existingDates.station.StationVisitDate + "' ";

                    if (init)
                    {
                        visitDateWhere = "WHERE " + where_raw;
                        init = false;
                    }
                    else
                    {
                        visitDateWhere = visitDateWhere + "AND " + where_raw;
                    }
                    
                }

                visitDateQuery = visitDateSelectFrom + visitDateWhere + visitDateOrderBy;
            }

            List<object> stationDateTableRows = dAccess.ReadTable(statDate.GetType(), visitDateQuery);

            if (stationDateTableRows != null && stationDateTableRows.Count != 0)
            {
                int addingSequence = 0;
                foreach (object objs in stationDateTableRows)
                {
                    //Cast
                    Station currentStation = objs as Station;

                    //Read from the database the wanted informations and fill observation attributes
                    FieldNotes currentDateItems = new FieldNotes
                    {
                        station = currentStation
                    };

                    _reportSummaryDateItems.Insert(addingSequence, currentDateItems);

                    addingSequence++;

                }

                RaisePropertyChanged("ReportDateItems");

                //Set selected index
                if (ReportDateItems.Count > 0)
                {

                    ReportListViewDateIndex = 0;
                    RaisePropertyChanged("ReportListViewDateIndex");
                }

            }
            else
            {

                //Force reset of data selected index in case user has selected one, took a new station and navigates back to the notes.
                if (ReportDateItems.Count > 0)
                {

                    ReportListViewDateIndex = 0;
                    RaisePropertyChanged("ReportListViewDateIndex");
                }

                //Refills stations
                FillStationFromList();
            }
            ////To validate if a reset is needed because it's a new field book
            //else if (stationDateTableRows != null && stationDateTableRows.Count == 0 && )
            //{

            //}

            if (summaryDone!=null)
            {
                summaryDone(this);
            }

            //Set header text
            if (localSetting.GetSettingValue(Dictionaries.ApplicationLiterals.KeywordDocumentMode) != null)
            {
                if ((bool)localSetting.GetSettingValue(Dictionaries.ApplicationLiterals.KeywordDocumentMode))
                {
                    _headerDocumenText = ApplicationLiterals.KeywordDocumentHeaderTrue;
                }
                else
                {
                    _headerDocumenText = ApplicationLiterals.KeywordDocumentHeaderFalse;
                }
            }
            else
            {
                _headerDocumenText = ApplicationLiterals.KeywordDocumentHeaderFalse;
            }

            RaisePropertyChanged("HeaderDocument");
        }

        /// <summary>
        /// Will fill the report detail item property with the current selected detail item in memory
        /// </summary>
        public void FillStationFromList()
        {
            #region Conditional to user having selected a station
            
            if (_reportDateIndex != -1)
            {
                //Variables
                int userSelectedStationIndex = -1;

                //Get a list of related station from selected traverse date.
                Station stations = new Station();
                string stationSelectionQuery = "SELECT * FROM " + DatabaseLiterals.TableStation + " WHERE " + DatabaseLiterals.FieldStationVisitDate + " = '" + _reportSummaryDateItems[_reportDateIndex].station.StationVisitDate + "'";
                //string stationSelectionQuery = "SELECT * FROM " + DatabaseLiterals.TableStation + " WHERE " + DatabaseLiterals.FieldStationVisitDate + " = '" + _reportSummaryDateItems[_reportDateIndex].station.StationVisitDate + "' ORDER BY " + DatabaseLiterals.FieldStationAlias;
                List<object> stationTableRows = dAccess.ReadTable(stations.GetType(), stationSelectionQuery);


                _reportDetailedStation.Clear();

                if (stationTableRows.Count != 0 || stationTableRows != null)
                {
                    foreach (object sts in stationTableRows)
                    {
                        //Cast
                        Station currentStation = sts as Station;

                        //Fill the report item detail
                        FieldNotes currentDetailReport = new FieldNotes
                        {
                            station = currentStation,

                            GenericID = currentStation.StationID.ToString(),
                            GenericTableName = DatabaseLiterals.TableStation,
                            GenericFieldID = DatabaseLiterals.FieldStationID,
                            GenericAliasName = currentStation.StationAlias,

                            ParentID = currentStation.LocationID.ToString(), //TO keep the link with location table
                            ParentTableName = DatabaseLiterals.TableLocation, //To keep the link with location table.

                            MainID = currentStation.LocationID.ToString()
                        };

                        //Fill with location
                        FieldLocation locs = new FieldLocation();
                        string locationSelectionQuery = "SELECT * FROM " + DatabaseLiterals.TableLocation + " WHERE " + DatabaseLiterals.FieldLocationID + " = '" + currentStation.LocationID + "'";
                        List<object> locationTableRows = dAccess.ReadTable(locs.GetType(), locationSelectionQuery);

                        foreach (object lcs in locationTableRows)
                        {
                            FieldLocation currentLocation = lcs as FieldLocation;

                            currentDetailReport.location = currentLocation;
                        }

                        ReportDetailedStation.Add(currentDetailReport);

                        //Refresh summary
                        ValidateCheck(currentStation.isValid, currentStation.StationID.ToString(), currentDetailReport.MainID);

                        //Keep index if station id is the same as the one selected from map page by user
                        if (currentStation.StationAlias == userSelectedStationID)
                        {
                            userSelectedStationIndex = stationTableRows.IndexOf(sts);
                        }

                    }
                } 

                //Manager header opacity
                stationLocationRowCount = stationTableRows.Count;
                SetHeaderColorOpacity(Dictionaries.DatabaseLiterals.TableStation);
                SetHeaderColorOpacity(Dictionaries.DatabaseLiterals.TableLocation);

                //Set selected index
                if ((_reportStationIndex != stationTableRows.Count - 1) || (userSelectedStationIndex != -1))
                {
                    if (userSelectedStationIndex == -1)
                    {
                        _reportStationIndex = stationTableRows.Count - 1;
                    }
                    else
                    {
                        _reportStationIndex = userSelectedStationIndex;
                    }
                    RaisePropertyChanged("ReportStationListIndex");

                }

                SetHeaderColorOpacity(DatabaseLiterals.TableStation);


            }
            else
            {
                stationLocationRowCount = 0;
                EmptyAll();
            }
            #endregion
        }

        /// <summary>
        /// Will reset all headers to default.
        /// </summary>
        public void EmptyAll()
        {
            //Clear date from headers
            _reportDetailedLocation.Clear();
            _reportDetailedDocument.Clear();
            _reportDetailedEarthmat.Clear();
            _reportDetailedFossil.Clear();
            _reportDetailedMinerals.Clear();
            _reportDetailedPflow.Clear();
            _reportDetailedStation.Clear();
            _reportDetailedStructure.Clear();
            _reportDetailSample.Clear();
            _reportDetailedMineralAlt.Clear();
            _reportDetailEnvironment.Clear();
            RaisePropertyChanged("ReportDetailedLocation");
            RaisePropertyChanged("ReportDetailedDocument"); 
            RaisePropertyChanged("ReportDetailedEarthmat");
            RaisePropertyChanged("ReportDetailedFossil");
            RaisePropertyChanged("ReportDetailedMineral");
            RaisePropertyChanged("ReportDetailedPflow");
            RaisePropertyChanged("ReportDetailedSample");
            RaisePropertyChanged("ReportDetailedStation");
            RaisePropertyChanged("ReportDetailedStructure");
            RaisePropertyChanged("ReportDetailedMineralAlt");
            RaisePropertyChanged("ReportDetailEnvironment");

            //Reset opacity of header
            SetHeaderColorOpacity(Dictionaries.DatabaseLiterals.TableEarthMat);
            SetHeaderColorOpacity(Dictionaries.DatabaseLiterals.TableSample);
            SetHeaderColorOpacity(Dictionaries.DatabaseLiterals.TableStructure);
            SetHeaderColorOpacity(Dictionaries.DatabaseLiterals.TablePFlow);
            SetHeaderColorOpacity(Dictionaries.DatabaseLiterals.TableMineral);
            SetHeaderColorOpacity(Dictionaries.DatabaseLiterals.TableFossil);
            SetHeaderColorOpacity(Dictionaries.DatabaseLiterals.TableDocument);
            SetHeaderColorOpacity(Dictionaries.DatabaseLiterals.TableMineralAlteration);
            SetHeaderColorOpacity(Dictionaries.DatabaseLiterals.TableLocation);
            SetHeaderColorOpacity(Dictionaries.DatabaseLiterals.TableStation);
            SetHeaderColorOpacity(Dictionaries.DatabaseLiterals.TableEnvironment);
        }

        /// <summary>
        /// Will reset all headers to default for station childs
        /// </summary>
        public void EmptyStationChilds()
        {
            FillSummaryReportDateItems();
        }

        /// <summary>
        /// Will reset all headers to default for earthmat table childs.
        /// </summary>
        public void EmptyEarthmatChilds()
        {

            //Clear date from headers
            _reportDetailedFossil.Clear();
            _reportDetailedMinerals.Clear();
            _reportDetailedPflow.Clear();
            _reportDetailedStructure.Clear();
            _reportDetailSample.Clear();

            RaisePropertyChanged("ReportDetailedFossil");
            RaisePropertyChanged("ReportDetailedMineral");
            RaisePropertyChanged("ReportDetailedPflow");
            RaisePropertyChanged("ReportDetailedSample");
            RaisePropertyChanged("ReportDetailedStructure");

            //Reset opacity of header
            SetHeaderColorOpacity(Dictionaries.DatabaseLiterals.TableSample);
            SetHeaderColorOpacity(Dictionaries.DatabaseLiterals.TableStructure);
            SetHeaderColorOpacity(Dictionaries.DatabaseLiterals.TablePFlow);
            SetHeaderColorOpacity(Dictionaries.DatabaseLiterals.TableMineral);
            SetHeaderColorOpacity(Dictionaries.DatabaseLiterals.TableFossil);

        }

        /// <summary>
        /// Will reset all headers to default for earthmat table childs.
        /// </summary>
        public void EmptyMineralizationAlterationChilds()
        {

            //Clear date from headers
            _reportDetailedMinerals.Clear();

            RaisePropertyChanged("ReportDetailedMineral");

            //Reset opacity of header
            SetHeaderColorOpacity(Dictionaries.DatabaseLiterals.TableMineral);

        }

        /// <summary>
        /// Will fill the report detail item property with the current selected station item 
        /// </summary>
        public void FillEarthmatFromStation()
        {
            //Making sure to reset properly
            EmptyEarthmatChilds();

            #region Conditional to user having selected a station

            if(_reportStationIndex != -1)
            {
                if (_reportDetailedEarthmat.Count() > 0)
                {
                    _reportDetailedEarthmat.Clear();
                }

                //Keep selection
                FieldNotes currentReport = _reportDetailedStation[_reportStationIndex];

                //Get parent id
                int statID = int.Parse(currentReport.GenericID);

                //Get a list of related earthmat from selected station
                //Querying with Linq
                List<object> earthmatTableRaw = dAccess.ReadTable(earthModel.GetType(), null);
                IEnumerable<EarthMaterial> earthmatTable = earthmatTableRaw.Cast<EarthMaterial>(); //Cast to proper list type
                IEnumerable<EarthMaterial> eartmatParentStations = from e in earthmatTable where e.EarthMatStatID == statID select e;

                if (eartmatParentStations.Count() != 0 || eartmatParentStations != null)
                {
                    foreach (EarthMaterial earths in eartmatParentStations)
                    {
                        //Cast
                        EarthMaterial currentEarth = earths as EarthMaterial;

                        //Fill the report item detail
                        FieldNotes currentDetailReport = new FieldNotes
                        {
                            station = currentReport.station,

                            earthmat = currentEarth,

                            GenericID = currentEarth.EarthMatID.ToString(),
                            GenericTableName = DatabaseLiterals.TableEarthMat,
                            GenericFieldID = DatabaseLiterals.FieldEarthMatID,
                            GenericAliasName = currentEarth.EarthMatName,

                            ParentID = currentEarth.EarthMatStatID.ToString(), //TO keep the link with location table
                            ParentTableName = DatabaseLiterals.TableStation, //To keep the link with location table.

                            MainID = currentReport.ParentID
                        };

                        _reportDetailedEarthmat.Add(currentDetailReport);

                        //Refresh summary
                        ValidateCheck(currentEarth.isValid, currentEarth.EarthMatID, currentDetailReport.MainID);

                    }
                }

                RaisePropertyChanged("ReportDetailedEarthmat");

                //Manager header color opacity (transparent if no items)
                earthmatRecordCount = eartmatParentStations.Count();
                SetHeaderColorOpacity(Dictionaries.DatabaseLiterals.TableEarthMat);

            }
            else
            {
                if (_reportDetailedEarthmat.Count() > 0)
                {
                    _reportDetailedEarthmat.Clear();
                    SetHeaderColorOpacity(Dictionaries.DatabaseLiterals.TableEarthMat);
                    RaisePropertyChanged("ReportDetailedEarthmat");
                }
            }

            #endregion



        }

        /// <summary>
        /// Will fill the report detail item property with the current selected station item 
        /// </summary>
        public void FillMineralAltFromStation()
        {
            _reportDetailedMineralAlt.Clear();

            #region Conditional to user having selected a station

            if (_reportStationIndex != -1)
            {

                //Keep selection
                FieldNotes currentReport = _reportDetailedStation[_reportStationIndex];

                //Get parent id
                string statID = currentReport.GenericID;

                //Get a list of related mienral alterations from selected station
                //Querying with Linq
                List<object> maTableRaw = dAccess.ReadTable(mineralAltModel.GetType(), null);
                IEnumerable<MineralAlteration> maTable = maTableRaw.Cast<MineralAlteration>(); //Cast to proper list type
                IEnumerable<MineralAlteration> maParentStations = from ma in maTable where ma.MAParentID == int.Parse(statID) select ma;

                if (maParentStations.Count() != 0 || maParentStations != null)
                {


                    foreach (MineralAlteration mas in maParentStations)
                    {
                        //Cast
                        MineralAlteration currentMinealAlt = mas as MineralAlteration;

                        //Fill the report item detail
                        FieldNotes currentDetailReport = new FieldNotes
                        {
                            station = currentReport.station,

                            mineralAlteration = currentMinealAlt,

                            GenericID = currentMinealAlt.MAID.ToString(),
                            GenericTableName = DatabaseLiterals.TableMineralAlteration,
                            GenericFieldID = DatabaseLiterals.FieldMineralAlterationID,
                            GenericAliasName = currentMinealAlt.MAName,

                            ParentID = currentMinealAlt.MAParentID.ToString(), //TO keep the link with location table
                            ParentTableName = DatabaseLiterals.TableStation, //To keep the link with location table.

                            MainID = currentReport.ParentID
                        };

                        _reportDetailedMineralAlt.Add(currentDetailReport);

                        //Refresh summary
                        ValidateCheck(currentMinealAlt.isValid, currentMinealAlt.MAID, currentDetailReport.MainID);

                    }



                }

                //Manager header color opacity (transparent if no items)
                mineralAltRecordCount = maParentStations.Count();
                SetHeaderColorOpacity(Dictionaries.DatabaseLiterals.TableMineralAlteration);

            }

            #endregion
            SetHeaderColorOpacity(Dictionaries.DatabaseLiterals.TableMineralAlteration);
            RaisePropertyChanged("ReportDetailedMineralAlt");

        }

        /// <summary>
        /// Will fill the report detail item property with the current selected station item for environment
        /// </summary>
        public void FillEnvironmentFromStation()
        {
            _reportDetailEnvironment.Clear();

            #region Conditional to user having selected a station

            if (_reportStationIndex != -1)
            {

                //Keep selection
                FieldNotes currentReport = _reportDetailedStation[_reportStationIndex];

                //Get parent id
                int statID = int.Parse(currentReport.GenericID);

                //Get a list of related environment from selected station
                //Querying with Linq
                List<object> envTableRaw = dAccess.ReadTable(environmentModel.GetType(), null);
                IEnumerable<EnvironmentModel> envTable = envTableRaw.Cast<EnvironmentModel>(); //Cast to proper list type
                IEnumerable<EnvironmentModel> envParentStations = from env in envTable where env.EnvStationID == statID select env;

                if (envParentStations.Count() != 0 || envParentStations != null)
                {


                    foreach (EnvironmentModel envs in envParentStations)
                    {
                        //Cast
                        EnvironmentModel currentEnv = envs as EnvironmentModel;

                        //Fill the report item detail
                        FieldNotes currentDetailReport = new FieldNotes
                        {
                            station = currentReport.station,

                            environment = currentEnv,

                            GenericID = currentEnv.EnvID.ToString(),
                            GenericTableName = DatabaseLiterals.TableEnvironment,
                            GenericFieldID = DatabaseLiterals.FieldEnvID,
                            GenericAliasName = currentEnv.EnvName,

                            ParentID = currentEnv.EnvStationID.ToString(), //TO keep the link with location table
                            ParentTableName = DatabaseLiterals.TableStation, //To keep the link with location table.

                            MainID = currentReport.ParentID
                        };

                        _reportDetailEnvironment.Add(currentDetailReport);

                        //Refresh summary
                        ValidateCheck(currentEnv.isValid, currentEnv.EnvID.ToString(), currentDetailReport.MainID);

                    }



                }

                //Manager header color opacity (transparent if no items)
                environmentRecordCound = envParentStations.Count();
                SetHeaderColorOpacity(Dictionaries.DatabaseLiterals.TableEnvironment);

            }

            #endregion
            SetHeaderColorOpacity(Dictionaries.DatabaseLiterals.TableEnvironment);
            RaisePropertyChanged("ReportDetailEnvironment");
        }


        /// <summary>
        /// Will fill the report detail item property with the current selected station item 
        /// </summary>
        public void FillLocation()
        {
            _reportDetailedLocation.Clear();

            #region Conditional to user having selected a station

            if (_reportStationIndex != -1)
            {

                //Keep selection
                FieldNotes currentReport = _reportDetailedStation[_reportStationIndex];

                //Get parent id
                int locID = currentReport.station.LocationID;

                //Get a list of related earthmat from selected station
                //Querying with Linq
                List<object> locationTableRaw = dAccess.ReadTable(locationModel.GetType(), null);
                IEnumerable<FieldLocation> locationTable = locationTableRaw.Cast<FieldLocation>(); //Cast to proper list type
                IEnumerable<FieldLocation> locationResult = from l in locationTable where l.LocationID == locID select l;

                if (locationResult.Count() != 0 || locationResult != null)
                {


                    foreach (FieldLocation locs in locationResult)
                    {
                        //Cast
                        FieldLocation currentLocation = locs as FieldLocation;

                        //Fill the report item detail
                        FieldNotes currentDetailReport = new FieldNotes
                        {
                            location = currentLocation,

                            GenericID = currentLocation.LocationID.ToString(),
                            GenericTableName = DatabaseLiterals.TableLocation,
                            GenericFieldID = DatabaseLiterals.FieldLocationID,
                            GenericAliasName = currentLocation.LocationAlias,

                            MainID = currentLocation.LocationID.ToString()
                        };

                        _reportDetailedLocation.Add(currentDetailReport);

                    }

                }

            }

            #endregion

            RaisePropertyChanged("ReportDetailedLocation");

        }

        /// <summary>
        /// Will fill the report detail item property with the current selected station item
        /// </summary>
        public void FillDocument()
        {
            _reportDetailedDocument.Clear();

            #region Documents can be filled without selected stations since it can be related to pretty much any records in the database.

            //Variables
            List<object> docTableRaw = dAccess.ReadTable(documentModel.GetType(), null);
            IEnumerable<Document> docTable = docTableRaw.Cast<Document>(); //Cast to proper list type
                                                                           //IEnumerable<Document> docParent = from d in docTable where d.RelatedID == selectedSummaryItem.GenericID select d; //Get all documents attached to selected station
                                                                           //bool noSelectedStation = true;
            List<FieldNotes> collectionOfSelectedItems = new List<FieldNotes>();
            #region Build query to get a list of documents
            string queryFrom = "SELECT * FROM " + DatabaseLiterals.TableDocument;
            string whereDefault = string.Empty;

            if (_reportEarthmatIndex != -1)
            {
                collectionOfSelectedItems.Add(_reportDetailedEarthmat[_reportEarthmatIndex]);
            }
            if (_reportSampleIndex != -1)
            {
                collectionOfSelectedItems.Add(_reportDetailSample[_reportSampleIndex]);
            }
            if (_reportDocumentIndex != -1)
            {
                collectionOfSelectedItems.Add(_reportDetailedDocument[_reportDocumentIndex]);
            }
            if (_reportStructureIndex != -1)
            {
                collectionOfSelectedItems.Add(_reportDetailedStructure[_reportStructureIndex]);
            }
            if (_reportPflowIndex != -1)
            {
                collectionOfSelectedItems.Add(_reportDetailedPflow[_reportPflowIndex]);
            }
            if (_reportFossilIndex != -1)
            {
                collectionOfSelectedItems.Add(_reportDetailedFossil[_reportFossilIndex]);
            }
            if (_reportMineralIndex != -1)
            {
                collectionOfSelectedItems.Add(_reportDetailedMinerals[_reportMineralIndex]);
            }
            if (_reportMineralizationAlterationIndex != -1)
            {
                collectionOfSelectedItems.Add(_reportDetailedMineralAlt[_reportMineralizationAlterationIndex]);
            }
            if (_reportStationIndex != -1)
            {
                collectionOfSelectedItems.Add(_reportDetailedStation[_reportStationIndex]);
            }

            foreach (FieldNotes reportCollection in collectionOfSelectedItems)
            {

                if (whereDefault!=string.Empty)
                {
                    whereDefault = whereDefault + " OR " + DatabaseLiterals.TableDocument + "." + DatabaseLiterals.FieldDocumentName + " LIKE '" + reportCollection.GenericAliasName + "%'";
                }
                else
                {
                    whereDefault = " WHERE " + DatabaseLiterals.TableDocument + "." + DatabaseLiterals.FieldDocumentName + " LIKE '" + reportCollection.GenericAliasName + "%'";
                }


            }

            string finalQuery = queryFrom;
            if (whereDefault != string.Empty)
            {
                finalQuery = finalQuery + whereDefault;
            }
            #endregion

            //Get documents
            IEnumerable<Document> documents = dAccess.ReadTable(documentModel.GetType(), finalQuery).Cast<Document>();

            if (documents != null || documents.Count() != 0)
            {
                if (_reportStationIndex != -1)
                {
                    foreach (Document docs in documents)
                    {

                        //Cast
                        Document currentDocuments = docs as Document;

                        //Fill the report item detail
                        FieldNotes currentDetailReport = new FieldNotes
                        {
                            station = _reportDetailedStation[_reportStationIndex].station,

                            document = currentDocuments,

                            GenericID = currentDocuments.DocumentID.ToString(),
                            GenericTableName = DatabaseLiterals.TableDocument,
                            GenericFieldID = DatabaseLiterals.FieldDocumentID,
                            GenericAliasName = currentDocuments.DocumentName,

                            ParentID = currentDocuments.RelatedID.ToString(), //TO keep the link with location table
                            ParentTableName = currentDocuments.RelatedTable, //To keep the link with location table.

                            MainID = _reportDetailedStation[_reportStationIndex].station.LocationID.ToString()
                        };

                        _reportDetailedDocument.Add(currentDetailReport);

                        //Refresh summary
                        ValidateCheck(currentDocuments.isValid, currentDocuments.DocumentID, currentDetailReport.MainID);

                        //Stop possible overflow
                        if (_reportDetailedDocument.Count > 500)
                        {
                            //Add a dummy item
                            var loadLocalization = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
                            FieldNotes maxReached = new FieldNotes();
                            maxReached.document.DocumentName = loadLocalization.GetString("WarningMaxReached");
                            _reportDetailedDocument.Add(maxReached);
                            break;
                        }
                    }
                }

            }

            //Manager header color opacity (transparent if no items)
            documentRecordCount = documents.Count();
            SetHeaderColorOpacity(Dictionaries.DatabaseLiterals.TableDocument);

            RaisePropertyChanged("ReportDetailedDocument");



            #endregion
        }

        /// <summary>
        /// Will fill the report detail item property with the current selected earthmat item
        /// </summary>
        public void FillSample()
        {
            _reportDetailSample.Clear();

            //Variables
            List<object> sampleTableRaw = dAccess.ReadTable(sampleModel.GetType(), null);
            IEnumerable<Sample> sampleTable = sampleTableRaw.Cast<Sample>(); //Cast to proper list type

            //For other cases
            bool foundParentOrSibling = false;

            if (_reportEarthmatIndex != -1 && _reportStationIndex != -1)
            {
                #region Conditional to have a selected earthmat
                foundParentOrSibling = true;
                    

                //Get a list of related samples from selected earthmat
                //Querying with Linq
                IEnumerable<Sample> sampleParentEarth = from e in sampleTable where e.SampleEarthmatID == _reportDetailedEarthmat[_reportEarthmatIndex].GenericID select e;
                if (sampleParentEarth.Count() != 0 || sampleParentEarth != null)
                {
                    foreach (Sample spl in sampleParentEarth)
                    {

                        //Fill the report item detail
                        FieldNotes currentDetailReport = new FieldNotes
                        {
                            sample = spl,

                            GenericID = spl.SampleID.ToString(),
                            GenericTableName = DatabaseLiterals.TableSample,
                            GenericFieldID = DatabaseLiterals.FieldSampleID,
                            GenericAliasName = spl.SampleName,

                            ParentID = _reportDetailedEarthmat[_reportEarthmatIndex].earthmat.EarthMatID, //TO keep the link with earthmat table
                            ParentTableName = DatabaseLiterals.TableEarthMat, //To keep the link with location table.

                            MainID = _reportDetailedStation[_reportStationIndex].station.LocationID.ToString()
                        };

                        ReportDetailedSample.Add(currentDetailReport);

                        //Refresh summary
                        ValidateCheck(spl.isValid, spl.SampleID, currentDetailReport.MainID);

                    }


                    //Manage header opacity
                    sampleRecordCount = sampleParentEarth.Count();

                }

                #endregion

                //Manager header color opacity (transparent if no items)
                SetHeaderColorOpacity(Dictionaries.DatabaseLiterals.TableSample);

                RaisePropertyChanged("ReportDetailedSample");

            }
            //If the selection is already a sample, coming form an edit
            if (_reportSampleIndex != -1)
            {
                #region Conditional to have a selected sample
                foundParentOrSibling = true;

                //Get a list of related samples from selected earthmat
                //Querying with Linq
                IEnumerable<Sample> sampleParent = from e in sampleTable where e.SampleEarthmatID == _reportDetailSample[_reportSampleIndex].ParentID select e;

                if (sampleParent.Count() != 0 || sampleParent != null)
                {
                    foreach (Sample spl in sampleParent)
                    {
                        //Fill the report item detail
                        FieldNotes currentDetailReport = new FieldNotes
                        {
                            sample = spl,

                            GenericID = spl.SampleID.ToString(),
                            GenericTableName = DatabaseLiterals.TableSample,
                            GenericFieldID = DatabaseLiterals.FieldSampleID,
                            GenericAliasName = spl.SampleName,

                            ParentID = _reportDetailSample[_reportSampleIndex].earthmat.EarthMatID, //TO keep the link with earthmat table
                            ParentTableName = DatabaseLiterals.TableEarthMat, //To keep the link with location table.

                            MainID = _reportDetailedStation[ReportStationListIndex].station.LocationID.ToString()
                        };

                        ReportDetailedSample.Add(currentDetailReport);

                        //Refresh summary
                        ValidateCheck(spl.isValid, spl.SampleID, currentDetailReport.MainID);
                    }
                }

                //Manage header opacity
                sampleRecordCount = sampleParent.Count();

                #endregion

                //Manager header color opacity (transparent if no items)
                SetHeaderColorOpacity(Dictionaries.DatabaseLiterals.TableSample);

                RaisePropertyChanged("ReportDetailedSample");

            }

            if (!foundParentOrSibling && _reportStationIndex != -1)
            {

                #region For other situations

                List<object> stationTableRaw = dAccess.ReadTable(stationModel.GetType(), null);
                IEnumerable<Station> stationTable = stationTableRaw.Cast<Station>(); //Cast to proper list type

                List<object> earthTableRaw = dAccess.ReadTable(earthModel.GetType(), null);
                IEnumerable<EarthMaterial> earthTable = earthTableRaw.Cast<EarthMaterial>(); //Cast to proper list type

                //Get a list of all samples from selected station
                //Get a list of earthmat ids from selected stationID
                IEnumerable<string> earthFromStation = from e in earthTable join stat in stationTable on e.EarthMatStatID equals stat.StationID where stat.StationID == int.Parse(_reportDetailedStation[_reportStationIndex].GenericID) select e.EarthMatID;

                //Get resulting sample class from previous list of earthmat ids
                IEnumerable<Sample> sampleParent_ = from smp in sampleTable join e2 in earthTable on smp.SampleEarthmatID equals e2.EarthMatID where earthFromStation.Contains(e2.EarthMatID) select smp;

                if (sampleParent_.Count() != 0 || sampleParent_ != null)
                {
                    foreach (Sample spl in sampleParent_)
                    {
                        //Fill the report item detail
                        FieldNotes currentDetailReport = new FieldNotes
                        {
                            sample = spl,

                            GenericID = spl.SampleID.ToString(),
                            GenericTableName = DatabaseLiterals.TableSample,
                            GenericFieldID = DatabaseLiterals.FieldSampleID,
                            GenericAliasName = spl.SampleName,

                            ParentID = spl.SampleEarthmatID, //TO keep the link with earthmat table
                            ParentTableName = DatabaseLiterals.TableEarthMat, //To keep the link with location table.

                            MainID = _reportDetailedStation[ReportStationListIndex].station.LocationID.ToString()
                        };

                        _reportDetailSample.Add(currentDetailReport);

                        //Refresh summary
                        ValidateCheck(spl.isValid, spl.SampleID, currentDetailReport.MainID);

                    }
                }

                //Manage header opacity
                sampleRecordCount = sampleParent_.Count();

                #endregion

                //Manager header color opacity (transparent if no items)
                SetHeaderColorOpacity(Dictionaries.DatabaseLiterals.TableSample);

            }

            if (sampleRecordCount == 0)
            {
                _reportDetailSample.Clear();
            }

            RaisePropertyChanged("ReportDetailedSample");
            SetHeaderIconOpacity();

        }

        /// <summary>
        /// Will fill the report detail item property 
        /// </summary>
        public void FillStructure()
        {
            _reportDetailedStructure.Clear();

            //Variables
            List<object> structTableRaw = dAccess.ReadTable(structureModel.GetType(), null);
            IEnumerable<Structure> structureTable = structTableRaw.Cast<Structure>(); //Cast to proper list type

            //For other cases
            bool foundParentOrSibling = false;

            if (_reportEarthmatIndex != -1 && _reportStationIndex != -1)
            {
                #region Conditional to have a selected earthmat
                foundParentOrSibling = true;


                //Get a list of related samples from selected earthmat
                //Querying with Linq
                IEnumerable<Structure> structParentEarth = from e in structureTable where e.StructureParentID == _reportDetailedEarthmat[_reportEarthmatIndex].GenericID select e;
                if (structParentEarth.Count() != 0 || structParentEarth != null)
                {
                    foreach (Structure spl in structParentEarth)
                    {

                        //Fill the report item detail
                        FieldNotes currentDetailReport = new FieldNotes
                        {
                            structure = spl,

                            GenericID = spl.StructureID.ToString(),
                            GenericTableName = DatabaseLiterals.TableStructure,
                            GenericFieldID = DatabaseLiterals.FieldStructureID,
                            GenericAliasName = spl.StructureName,

                            ParentID = _reportDetailedEarthmat[_reportEarthmatIndex].earthmat.EarthMatID, //TO keep the link with earthmat table
                            ParentTableName = DatabaseLiterals.TableEarthMat, //To keep the link with location table.

                            MainID = _reportDetailedStation[ReportStationListIndex].station.LocationID.ToString()
                        };

                        _reportDetailedStructure.Add(currentDetailReport);

                        //Refresh summary
                        ValidateCheck(spl.isValid, spl.StructureID, currentDetailReport.MainID);
                    }


                    //Manage header opacity
                    structureRecordCount = structParentEarth.Count();

                }

                #endregion

                //Manager header color opacity (transparent if no items)
                SetHeaderColorOpacity(Dictionaries.DatabaseLiterals.TableStructure);

                RaisePropertyChanged("ReportDetailedStructure"); 

            }
            //If the selection is already a sample, coming form an edit
            if (_reportStructureIndex != -1)
            {
                #region Conditional to have a selected sample
                foundParentOrSibling = true;

                //Get a list of related samples from selected earthmat
                //Querying with Linq
                IEnumerable<Structure> strucParent = from e in structureTable where e.StructureParentID == _reportDetailedStructure[_reportStructureIndex].ParentID select e;

                if (strucParent.Count() != 0 || strucParent != null)
                {
                    foreach (Structure spl in strucParent)
                    {
                        //Fill the report item detail
                        FieldNotes currentDetailReport = new FieldNotes
                        {
                            structure = spl,

                            GenericID = spl.StructureID.ToString(),
                            GenericTableName = DatabaseLiterals.TableStructure,
                            GenericFieldID = DatabaseLiterals.FieldStructureID,
                            GenericAliasName = spl.StructureName,

                            ParentID = _reportDetailedStructure[_reportStructureIndex].earthmat.EarthMatID, //TO keep the link with earthmat table
                            ParentTableName = DatabaseLiterals.TableEarthMat, //To keep the link with location table.

                            MainID = _reportDetailedStation[ReportStationListIndex].station.LocationID.ToString()
                        };

                        _reportDetailedStructure.Add(currentDetailReport);

                        //Refresh summary
                        ValidateCheck(spl.isValid, spl.StructureID, currentDetailReport.MainID);
                    }
                }

                //Manage header opacity
                structureRecordCount = strucParent.Count();

                #endregion

                //Manager header color opacity (transparent if no items)
                SetHeaderColorOpacity(Dictionaries.DatabaseLiterals.TableStructure);

                RaisePropertyChanged("ReportDetailedStructure");

            }
            

            if (!foundParentOrSibling && _reportStationIndex != -1)
            {

                #region For other situations

                List<object> stationTableRaw = dAccess.ReadTable(stationModel.GetType(), null);
                IEnumerable<Station> stationTable = stationTableRaw.Cast<Station>(); //Cast to proper list type

                List<object> earthTableRaw = dAccess.ReadTable(earthModel.GetType(), null);
                IEnumerable<EarthMaterial> earthTable = earthTableRaw.Cast<EarthMaterial>(); //Cast to proper list type

                //Get a list of all samples from selected station
                //Get a list of earthmat ids from selected stationID
                IEnumerable<string> earthFromStation = from e in earthTable join stat in stationTable on e.EarthMatStatID equals stat.StationID where stat.StationID == int.Parse(_reportDetailedStation[_reportStationIndex].GenericID) select e.EarthMatID;

                //Get resulting sample class from previous list of earthmat ids
                IEnumerable<Structure> stParent = from smp in structureTable join e2 in earthTable on smp.StructureParentID equals e2.EarthMatID where earthFromStation.Contains(e2.EarthMatID) select smp;

                if (stParent.Count() != 0 || stParent != null)
                {
                    foreach (Structure spl in stParent)
                    {
                        //Fill the report item detail
                        FieldNotes currentDetailReport = new FieldNotes
                        {
                            structure = spl,

                            GenericID = spl.StructureID.ToString(),
                            GenericTableName = DatabaseLiterals.TableStructure,
                            GenericFieldID = DatabaseLiterals.FieldStructureID,
                            GenericAliasName = spl.StructureName,

                            ParentID = spl.StructureParentID, //TO keep the link with earthmat table
                            ParentTableName = DatabaseLiterals.TableEarthMat, //To keep the link with location table.

                            MainID = _reportDetailedStation[ReportStationListIndex].station.LocationID.ToString()
                        };

                        _reportDetailedStructure.Add(currentDetailReport);

                        //Refresh summary
                        ValidateCheck(spl.isValid, spl.StructureID, currentDetailReport.MainID);
                    }
                }

                //Manage header opacity
                structureRecordCount = stParent.Count();

                #endregion

                //Manager header color opacity (transparent if no items)
                SetHeaderColorOpacity(Dictionaries.DatabaseLiterals.TableStructure);

                RaisePropertyChanged("ReportDetailedStructure"); 


            }

            SetHeaderIconOpacity();
        }

        /// <summary>
        /// Will fill the report detail item for fossils
        /// </summary>
        private void FillFossil()
        {
            _reportDetailedFossil.Clear();

            //Variables
            List<object> fossilTableRaw = dAccess.ReadTable(fossilModel.GetType(), null);
            IEnumerable<Fossil> fossilTable = fossilTableRaw.Cast<Fossil>(); //Cast to proper list type

            //For other cases
            bool foundParentOrSibling = false;

            if (_reportEarthmatIndex != -1 && _reportStationIndex != -1)
            {
                #region Conditional to have a selected earthmat
                foundParentOrSibling = true;


                //Get a list of related samples from selected earthmat
                //Querying with Linq
                IEnumerable<Fossil> fossilParentEarth = from e in fossilTable where e.FossilParentID == _reportDetailedEarthmat[_reportEarthmatIndex].GenericID select e;
                if (fossilParentEarth.Count() != 0 || fossilParentEarth != null)
                {
                    foreach (Fossil fss in fossilParentEarth)
                    {

                        //Fill the report item detail
                        FieldNotes currentDetailReport = new FieldNotes
                        {
                            fossil = fss,

                            GenericID = fss.FossilID.ToString(),
                            GenericTableName = DatabaseLiterals.TableFossil,
                            GenericFieldID = DatabaseLiterals.FieldFossilID,
                            GenericAliasName = fss.FossilIDName,

                            ParentID = _reportDetailedEarthmat[_reportEarthmatIndex].earthmat.EarthMatID, //TO keep the link with earthmat table
                            ParentTableName = DatabaseLiterals.TableEarthMat, //To keep the link with location table.

                            MainID = _reportDetailedStation[ReportStationListIndex].station.LocationID.ToString()
                        };

                        _reportDetailedFossil.Add(currentDetailReport);

                        //Refresh summary
                        ValidateCheck(fss.isValid, fss.FossilID, currentDetailReport.MainID);
                    }


                    //Manage header opacity
                    fossilRecordCount = fossilParentEarth.Count();

                }

                #endregion

                //Manager header color opacity (transparent if no items)
                SetHeaderColorOpacity(Dictionaries.DatabaseLiterals.TableFossil);

                RaisePropertyChanged("ReportDetailedFossil");

            }
            //If the selection is already a sample, coming form an edit
            if (_reportFossilIndex != -1)
            {
                #region Conditional to have a selected sample
                foundParentOrSibling = true;

                //Get a list of related samples from selected earthmat
                //Querying with Linq
                IEnumerable<Fossil> fossilParent = from e in fossilTable where e.FossilParentID == _reportDetailedFossil[_reportFossilIndex].ParentID select e;

                if (fossilParent.Count() != 0 || fossilParent != null)
                {
                    foreach (Fossil fss in fossilParent)
                    {
                        //Fill the report item detail
                        FieldNotes currentDetailReport = new FieldNotes
                        {
                            fossil = fss,

                            GenericID = fss.FossilID.ToString(),
                            GenericTableName = DatabaseLiterals.TableFossil,
                            GenericFieldID = DatabaseLiterals.FieldFossilID,
                            GenericAliasName = fss.FossilIDName,

                            ParentID = _reportDetailedFossil[_reportFossilIndex].earthmat.EarthMatID, //TO keep the link with earthmat table
                            ParentTableName = DatabaseLiterals.TableEarthMat, //To keep the link with location table.

                            MainID = _reportDetailedStation[ReportStationListIndex].station.LocationID.ToString()
                        };

                        _reportDetailedFossil.Add(currentDetailReport);

                        //Refresh summary
                        ValidateCheck(fss.isValid, fss.FossilID, currentDetailReport.MainID);
                    }
                }

                //Manage header opacity
                fossilRecordCount = fossilParent.Count();

                #endregion

                //Manager header color opacity (transparent if no items)
                SetHeaderColorOpacity(Dictionaries.DatabaseLiterals.TableFossil);

                RaisePropertyChanged("ReportDetailedFossil");
            }
            

            if (!foundParentOrSibling && _reportStationIndex != -1)
            {

                #region For other situations

                List<object> stationTableRaw = dAccess.ReadTable(stationModel.GetType(), null);
                IEnumerable<Station> stationTable = stationTableRaw.Cast<Station>(); //Cast to proper list type

                List<object> earthTableRaw = dAccess.ReadTable(earthModel.GetType(), null);
                IEnumerable<EarthMaterial> earthTable = earthTableRaw.Cast<EarthMaterial>(); //Cast to proper list type

                //Get a list of all samples from selected station
                //Get a list of earthmat ids from selected stationID
                IEnumerable<string> earthFromStation = from e in earthTable join stat in stationTable on e.EarthMatStatID equals stat.StationID where stat.StationID == int.Parse(_reportDetailedStation[_reportStationIndex].GenericID) select e.EarthMatID;

                //Get resulting sample class from previous list of earthmat ids
                IEnumerable<Fossil> stParent = from smp in fossilTable join e2 in earthTable on smp.FossilParentID equals e2.EarthMatID where earthFromStation.Contains(e2.EarthMatID) select smp;

                if (stParent.Count() != 0 || stParent != null)
                {
                    foreach (Fossil fss in stParent)
                    {
                        //Fill the report item detail
                        FieldNotes currentDetailReport = new FieldNotes
                        {
                            fossil = fss,

                            GenericID = fss.FossilID.ToString(),
                            GenericTableName = DatabaseLiterals.TableFossil,
                            GenericFieldID = DatabaseLiterals.FieldFossilID,
                            GenericAliasName = fss.FossilIDName,

                            ParentID = fss.FossilParentID, //TO keep the link with earthmat table
                            ParentTableName = DatabaseLiterals.TableEarthMat, //To keep the link with location table.

                            MainID = _reportDetailedStation[ReportStationListIndex].station.LocationID.ToString()
                        };

                        _reportDetailedFossil.Add(currentDetailReport);

                        //Refresh summary
                        ValidateCheck(fss.isValid, fss.FossilID, currentDetailReport.MainID);
                    }
                }

                //Manage header opacity
                fossilRecordCount = stParent.Count();

                #endregion

                //Manager header color opacity (transparent if no items)
                SetHeaderColorOpacity(Dictionaries.DatabaseLiterals.TableFossil);

                RaisePropertyChanged("ReportDetailedFossil");


            }

            SetHeaderIconOpacity();
        }

        /// <summary>
        /// Will fill the report detail item for paleoflows
        /// </summary>
        private void FillPflow()
        {
            _reportDetailedPflow.Clear();

            //Variables
            List<object> pflowTableRaw = dAccess.ReadTable(pflowModel.GetType(), null);
            IEnumerable<Paleoflow> pflowTable = pflowTableRaw.Cast<Paleoflow>(); //Cast to proper list type

            //For other cases
            bool foundParentOrSibling = false;

            if (_reportEarthmatIndex != -1 && _reportStationIndex != -1)
            {
                #region Conditional to have a selected earthmat
                foundParentOrSibling = true;

                //Get a list of related samples from selected earthmat
                //Querying with Linq
                IEnumerable<Paleoflow> pflowParentEarth = from e in pflowTable where e.PFlowParentID == _reportDetailedEarthmat[_reportEarthmatIndex].GenericID select e;
                if (pflowParentEarth.Count() != 0 || pflowParentEarth != null)
                {
                    foreach (Paleoflow pf in pflowParentEarth)
                    {

                        //Fill the report item detail
                        FieldNotes currentDetailReport = new FieldNotes
                        {
                            paleoflow = pf,

                            GenericID = pf.PFlowID.ToString(),
                            GenericTableName = DatabaseLiterals.TablePFlow,
                            GenericFieldID = DatabaseLiterals.FieldPFlowID,
                            GenericAliasName = pf.PFlowName,

                            ParentID = _reportDetailedEarthmat[_reportEarthmatIndex].earthmat.EarthMatID, //TO keep the link with earthmat table
                            ParentTableName = DatabaseLiterals.TableEarthMat, //To keep the link with location table.

                            MainID = _reportDetailedStation[ReportStationListIndex].station.LocationID.ToString()
                        };
                        _reportDetailedPflow.Add(currentDetailReport);

                        //Refresh summary
                        ValidateCheck(pf.isValid, pf.PFlowID, currentDetailReport.MainID);
                    }


                    //Manage header opacity
                    pflowRecordCount = pflowParentEarth.Count();

                }

                #endregion

                //Manager header color opacity (transparent if no items)
                SetHeaderColorOpacity(Dictionaries.DatabaseLiterals.TablePFlow);

                RaisePropertyChanged("ReportDetailedPflow");
            }
            //If the selection is already a sample, coming form an edit
            if (_reportPflowIndex != -1)
            {
                #region Conditional to have a selected sample
                foundParentOrSibling = true;

                //Get a list of related samples from selected earthmat
                //Querying with Linq
                IEnumerable<Paleoflow> pflowParent = from e in pflowTable where e.PFlowParentID == _reportDetailedPflow[_reportPflowIndex].ParentID select e;

                if (pflowParent.Count() != 0 || pflowParent != null)
                {
                    foreach (Paleoflow pf in pflowParent)
                    {
                        //Fill the report item detail
                        FieldNotes currentDetailReport = new FieldNotes
                        {
                            paleoflow = pf,

                            GenericID = pf.PFlowID.ToString(),
                            GenericTableName = DatabaseLiterals.TablePFlow,
                            GenericFieldID = DatabaseLiterals.FieldPFlowID,
                            GenericAliasName = pf.PFlowName,

                            ParentID = _reportDetailedPflow[_reportPflowIndex].earthmat.EarthMatID, //TO keep the link with earthmat table
                            ParentTableName = DatabaseLiterals.TableEarthMat, //To keep the link with location table.
                            MainID = _reportDetailedStation[ReportStationListIndex].station.LocationID.ToString()
                        };

                        _reportDetailedPflow.Add(currentDetailReport);

                        //Refresh summary
                        ValidateCheck(pf.isValid, pf.PFlowID, currentDetailReport.MainID);
                    }
                }

                //Manage header opacity
                pflowRecordCount = pflowParent.Count();

                #endregion

                //Manager header color opacity (transparent if no items)
                SetHeaderColorOpacity(Dictionaries.DatabaseLiterals.TablePFlow);

                RaisePropertyChanged("ReportDetailedPflow");

            }
            

            if (!foundParentOrSibling && _reportStationIndex != -1)
            {

                #region For other situations

                List<object> stationTableRaw = dAccess.ReadTable(stationModel.GetType(), null);
                IEnumerable<Station> stationTable = stationTableRaw.Cast<Station>(); //Cast to proper list type

                List<object> earthTableRaw = dAccess.ReadTable(earthModel.GetType(), null);
                IEnumerable<EarthMaterial> earthTable = earthTableRaw.Cast<EarthMaterial>(); //Cast to proper list type

                //Get a list of all samples from selected station
                //Get a list of earthmat ids from selected stationID
                IEnumerable<string> earthFromStation = from e in earthTable join stat in stationTable on e.EarthMatStatID equals stat.StationID where stat.StationID == int.Parse(_reportDetailedStation[_reportStationIndex].GenericID) select e.EarthMatID;

                //Get resulting sample class from previous list of earthmat ids
                IEnumerable<Paleoflow> pfParent = from smp in pflowTable join e2 in earthTable on smp.PFlowParentID equals e2.EarthMatID where earthFromStation.Contains(e2.EarthMatID) select smp;

                if (pfParent.Count() != 0 || pfParent != null)
                {
                    foreach (Paleoflow pf in pfParent)
                    {
                        //Fill the report item detail
                        FieldNotes currentDetailReport = new FieldNotes
                        {
                            paleoflow = pf,

                            GenericID = pf.PFlowID.ToString(),
                            GenericTableName = DatabaseLiterals.TablePFlow,
                            GenericFieldID = DatabaseLiterals.FieldPFlowID,
                            GenericAliasName = pf.PFlowName,

                            ParentID = pf.PFlowParentID, //TO keep the link with earthmat table
                            ParentTableName = DatabaseLiterals.TableEarthMat, //To keep the link with location table.
                            MainID = _reportDetailedStation[ReportStationListIndex].station.LocationID.ToString()
                        };

                        _reportDetailedPflow.Add(currentDetailReport);

                        //Refresh summary
                        ValidateCheck(pf.isValid, pf.PFlowID, currentDetailReport.MainID);
                    }
                }

                //Manage header opacity
                pflowRecordCount = pfParent.Count();

                #endregion

                //Manager header color opacity (transparent if no items)
                SetHeaderColorOpacity(Dictionaries.DatabaseLiterals.TablePFlow);

                RaisePropertyChanged("ReportDetailedPflow");


            }

            SetHeaderIconOpacity();
        }

        /// <summary>
        /// Will fill the report detail item for minerals
        /// </summary>
        private void FillMineral()
        {
            _reportDetailedMinerals.Clear();

            //Variables
            List<object> mineralTableRaw = dAccess.ReadTable(mineralModel.GetType(), null);
            IEnumerable<Mineral> mineralTable = mineralTableRaw.Cast<Mineral>(); //Cast to proper list type

            //For other cases
            //bool foundParentOrSibling = false;

            if (_reportEarthmatIndex != -1 && _reportStationIndex != -1)
            {
                #region Conditional to have a selected earthmat
                //foundParentOrSibling = true;

                //Get a list of related samples from selected earthmat
                //Querying with Linq
                IEnumerable<Mineral> mineralParentEarth = from e in mineralTable where e.MineralEMID == _reportDetailedEarthmat[_reportEarthmatIndex].GenericID select e;
                if (mineralParentEarth.Count() != 0 || mineralParentEarth != null)
                {
                    FillMineralFromParent(mineralParentEarth, _reportDetailedEarthmat[_reportEarthmatIndex].earthmat.EarthMatID);

                    //Manage header opacity
                    mineralRecordCount = mineralParentEarth.Count();

                }

                #endregion

                //Manager header color opacity (transparent if no items)
                SetHeaderColorOpacity(Dictionaries.DatabaseLiterals.TableMineral);

                RaisePropertyChanged("ReportDetailedMineral");

            }

            if (_reportMineralizationAlterationIndex != -1 && _reportStationIndex != -1)
            {
                #region Conditional to have a selected mineralization alteration
                //foundParentOrSibling = true;

                //Get a list of related samples from selected earthmat
                //Querying with Linq
                IEnumerable<Mineral> mineralParentEarth = from e in mineralTable where e.MineralMAID == _reportDetailedMineralAlt[_reportMineralizationAlterationIndex].GenericID select e;
                if (mineralParentEarth.Count() != 0 || mineralParentEarth != null)
                {
                    FillMineralFromParent(mineralParentEarth, _reportDetailedMineralAlt[_reportMineralizationAlterationIndex].mineralAlteration.MAID);

                    //Manage header opacity
                    mineralRecordCount = mineralParentEarth.Count();

                }

                #endregion

                //Manager header color opacity (transparent if no items)
                SetHeaderColorOpacity(Dictionaries.DatabaseLiterals.TableMineral);

                RaisePropertyChanged("ReportDetailedMineral");

            }

            if (_reportStationIndex != -1 && _reportMineralizationAlterationIndex == -1 && _reportEarthmatIndex == -1)
            {

                #region For other situations

                List<object> stationTableRaw = dAccess.ReadTable(stationModel.GetType(), null);
                IEnumerable<Station> stationTable = stationTableRaw.Cast<Station>(); //Cast to proper list type

                List<object> earthTableRaw = dAccess.ReadTable(earthModel.GetType(), null);
                IEnumerable<EarthMaterial> earthTable = earthTableRaw.Cast<EarthMaterial>(); //Cast to proper list type

                List<object> maTableRaw = dAccess.ReadTable(mineralAltModel.GetType(), null);
                IEnumerable<MineralAlteration> maTable = maTableRaw.Cast<MineralAlteration>(); //Cast to proper list type

                //Get a list of minerals from parent
                IEnumerable<string> earthFromStation = from e in earthTable join stat in stationTable on e.EarthMatStatID equals stat.StationID where stat.StationID == int.Parse(_reportDetailedStation[_reportStationIndex].GenericID) select e.EarthMatID;
                IEnumerable<string> mineralizationAlterationFromStation = from ma in maTable join stat in stationTable on ma.MAParentID equals stat.StationID where stat.StationID == int.Parse(_reportDetailedStation[_reportStationIndex].GenericID) select ma.MAID;

                //Get resulting sample class from previous list of earthmat ids
                IEnumerable<Mineral> minParent = from smp in mineralTable join e2 in earthTable on smp.MineralEMID equals e2.EarthMatID where earthFromStation.Contains(e2.EarthMatID) select smp;
                IEnumerable<Mineral> minParentMA = from min in mineralTable join ma in maTable on min.MineralMAID equals ma.MAID where mineralizationAlterationFromStation.Contains(ma.MAID) select min;

                if (minParent.Count() != 0 || minParent != null || minParentMA.Count() != 0 || minParentMA!= null)
                {
                    minParent = minParent.Concat<Mineral>(minParentMA);
                    FillMineralFromParent(minParent, _reportDetailedStation[_reportStationIndex].GenericID);
                }

                //Manage header opacity
                mineralRecordCount = minParent.Count();

                #endregion

                //Manager header color opacity (transparent if no items)
                SetHeaderColorOpacity(Dictionaries.DatabaseLiterals.TableMineral);

                RaisePropertyChanged("ReportDetailedMineral");

            }

            SetHeaderIconOpacity();
        }

        /// <summary>
        /// Will iterate through a collection of parents and add items to report page.
        /// </summary>
        /// <param name="parentCollection"></param>
        /// <param name="parentID"></param>
        private void FillMineralFromParent(IEnumerable<Mineral> parentCollection, string parentID)
        {
            foreach (Mineral m in parentCollection)
            {
                //Fill the report item detail
                FieldNotes currentDetailReport = new FieldNotes
                {
                    mineral = m,

                    GenericID = m.MineralID.ToString(),
                    GenericTableName = DatabaseLiterals.TableMineral,
                    GenericFieldID = DatabaseLiterals.FieldMineralID,
                    GenericAliasName = m.MineralIDName
                };

                if (parentID == string.Empty && m.MineralEMID != null)
                {
                    currentDetailReport.ParentID = m.MineralEMID.ToString(); //TO keep the link with earthmat table
                    currentDetailReport.ParentTableName = DatabaseLiterals.TableEarthMat; //To keep the link with location table.
                }
                else if (parentID == string.Empty && m.MineralMAID != null)
                {
                    currentDetailReport.ParentID = m.MineralMAID.ToString(); //TO keep the link with earthmat table
                    currentDetailReport.ParentTableName = DatabaseLiterals.TableMineralAlteration; //To keep the link with location table.
                }
                else
                {
                    currentDetailReport.ParentID = parentID; //TO keep the link with earthmat table
                }
                
                
                currentDetailReport.MainID = _reportDetailedStation[ReportStationListIndex].station.LocationID.ToString();

                _reportDetailedMinerals.Add(currentDetailReport);

                //Refresh summary
                ValidateCheck(m.isValid, m.MineralID, currentDetailReport.MainID);
            }

        }

        #endregion

        #region DELETE MANAGEMENT
        public async void DeleteIcon_TappedAsync(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            FieldNotes noteToDelete = null;
            int indexToPop = -1;
            ObservableCollection<FieldNotes> collectionToUpdate = new ObservableCollection<FieldNotes>();
            string collectionNameToRefresh = string.Empty;

            //Get incoming table name
            SymbolIcon senderIcon = sender as SymbolIcon;
            string senderName = senderIcon.Name.ToLower().Replace("deleteicon", "");

            if (DatabaseLiterals.TableEarthMat.ToLower().Contains(senderName) && _reportEarthmatIndex != -1)
            {
                noteToDelete = _reportDetailedEarthmat[_reportEarthmatIndex];
                deleteRequestFromTable = DatabaseLiterals.TableEarthMat;

                //Keep info
                collectionToUpdate = _reportDetailedEarthmat;
                indexToPop = _reportEarthmatIndex;
                collectionNameToRefresh = "ReportDetailedEarthmat";

            }
            if (DatabaseLiterals.TableSample.ToLower().Contains(senderName) && _reportSampleIndex != -1)
            {
                noteToDelete = _reportDetailSample[_reportSampleIndex];
                deleteRequestFromTable = DatabaseLiterals.TableSample;

                //Keep info
                collectionToUpdate = _reportDetailSample;
                indexToPop = _reportSampleIndex;
                collectionNameToRefresh = "ReportDetailedSample";
            }
            if (DatabaseLiterals.TableDocument.ToLower().Contains(senderName) && _reportDocumentIndex != -1)
            {
                noteToDelete = _reportDetailedDocument[_reportDocumentIndex];
                deleteRequestFromTable = DatabaseLiterals.TableDocument;

                //Keep info
                collectionToUpdate = _reportDetailedDocument;
                indexToPop = _reportDocumentIndex;
                collectionNameToRefresh = "ReportDetailedDocument";
            }
            if (DatabaseLiterals.TableStructure.ToLower().Contains(senderName) && _reportStructureIndex != -1)
            {
                noteToDelete = _reportDetailedStructure[_reportStructureIndex];
                deleteRequestFromTable = DatabaseLiterals.TableStructure;

                //Keep info
                collectionToUpdate = _reportDetailedStructure;
                indexToPop = _reportStructureIndex;
                collectionNameToRefresh = "ReportDetailedStructure";
            }
            if (DatabaseLiterals.TablePFlow.ToLower().Contains(senderName) && _reportPflowIndex != -1)
            {
                noteToDelete = _reportDetailedPflow[_reportPflowIndex];
                deleteRequestFromTable = DatabaseLiterals.TablePFlow;

                //Keep info
                collectionToUpdate = _reportDetailedPflow;
                indexToPop = _reportPflowIndex;
                collectionNameToRefresh = "ReportDetailedPflow";
            }
            if (DatabaseLiterals.TableFossil.ToLower().Contains(senderName) && _reportFossilIndex != -1)
            {
                noteToDelete = _reportDetailedFossil[_reportFossilIndex];
                deleteRequestFromTable = DatabaseLiterals.TableFossil;

                //Keep info
                collectionToUpdate = _reportDetailedFossil;
                indexToPop = _reportFossilIndex;
                collectionNameToRefresh = "ReportDetailedFossil";
            }
            if (DatabaseLiterals.TableMineral.ToLower().Contains(senderName) && _reportMineralIndex != -1)
            {
                noteToDelete = _reportDetailedMinerals[_reportMineralIndex];
                deleteRequestFromTable = DatabaseLiterals.TableMineral;

                //Keep info
                collectionToUpdate = _reportDetailedMinerals;
                indexToPop = _reportMineralIndex;
                collectionNameToRefresh = "ReportDetailedMineral"; 
            }
            if (DatabaseLiterals.TableMineralAlteration.ToLower().Contains(senderName) && _reportMineralizationAlterationIndex != -1)
            {
                noteToDelete = _reportDetailedMineralAlt[_reportMineralizationAlterationIndex];
                deleteRequestFromTable = DatabaseLiterals.TableMineralAlteration;

                //Keep info
                collectionToUpdate = _reportDetailedMineralAlt;
                indexToPop = _reportMineralizationAlterationIndex;
                collectionNameToRefresh = "ReportDetailedMineralAlt";
            }
            if (DatabaseLiterals.TableEnvironment.ToLower().Contains(senderName) && _reportEnvironmentIndex != -1)
            {
                noteToDelete = _reportDetailEnvironment[_reportEnvironmentIndex];
                deleteRequestFromTable = DatabaseLiterals.TableEnvironment;

                //Keep info
                collectionToUpdate = _reportDetailEnvironment;
                indexToPop = _reportEnvironmentIndex;
                collectionNameToRefresh = "ReportDetailEnvironment";
            }
            if (DatabaseLiterals.TableStation.ToLower().Contains(senderName) && _reportStationIndex != -1)
            {
                noteToDelete = _reportDetailedStation[_reportStationIndex];
                deleteRequestFromTable = DatabaseLiterals.TableStation;

                //Keep info
                collectionToUpdate = _reportDetailedStation;
                indexToPop = _reportStationIndex;
                collectionNameToRefresh = "ReportDetailedStation";

            }

            if (noteToDelete != null)
            {
                var loadLocalization = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
                ContentDialog deleteRecordDialog = new ContentDialog()
                {
                    Title = loadLocalization.GetString("DeleteDialogGenericTitle"),
                    Content = loadLocalization.GetString("DeleteDialog_textBox/Text") + " (" + deleteRequestFromTable + ").",
                    PrimaryButtonText = loadLocalization.GetString("Generic_ButtonYes/Content"),
                    SecondaryButtonText = loadLocalization.GetString("Generic_ButtonNo/Content")
                };
                deleteRecordDialog.Style = (Style)Application.Current.Resources["DeleteDialog"];

                ContentDialogResult drd = await deleteRecordDialog.ShowAsync();

                if (drd == ContentDialogResult.Primary)
                {

                    //Get current
                    string deleteTableName = noteToDelete.GenericTableName;
                    string deleteFieldName = noteToDelete.GenericFieldID;
                    string deleteFieldValue = noteToDelete.GenericID;

                    //Delete
                    dAccess.DeleteRecord(deleteTableName, deleteFieldName, deleteFieldValue);

                    if (deleteTableName == DatabaseLiterals.TableStation)
                    {
                        //Delete location too
                        dAccess.DeleteRecord(Dictionaries.DatabaseLiterals.TableLocation, Dictionaries.DatabaseLiterals.FieldLocationID, noteToDelete.station.LocationID.ToString(), true);
                    }

                    if (deleteTableName == DatabaseLiterals.TableDocument)
                    {
                        //Delete associated photo, if it exists too
                        if (noteToDelete.document.PhotoFileExists)
                        {
                            StorageFile photoToDelete = await StorageFile.GetFileFromPathAsync(noteToDelete.document.PhotoPath);
                            await photoToDelete.DeleteAsync();
                        }
                        
                    }

                    //Delete any related photos
                    dAccess.DeleteRecord(Dictionaries.DatabaseLiterals.TableDocument, Dictionaries.DatabaseLiterals.FieldDocumentRelatedID, deleteFieldValue);

                    //Refresh item pane (left)
                    collectionToUpdate.RemoveAt(indexToPop);
                    RaisePropertyChanged(collectionNameToRefresh);
                    if (collectionNameToRefresh == "ReportDetailedEarthmat")
                    {
                        EmptyEarthmatChilds();
                    }
                    else if (collectionNameToRefresh == "ReportDetailedStation")
                    {
                        EmptyStationChilds();
                    }
                    else if (collectionNameToRefresh == "ReportDetailedMineralAlt")
                    {
                        EmptyMineralizationAlterationChilds();
                    }


                }
            }
        }

        #endregion

        #region EXPAND MANAGEMENT

        public void SetHeaderExpansion()
        {
            if (localSetting.GetSettingValue(Dictionaries.ApplicationLiterals.KeyworkdExpandLocation)!=null)
            {
                _locationHeaderExpansion = (bool)localSetting.GetSettingValue(Dictionaries.ApplicationLiterals.KeyworkdExpandLocation);
            }
            if (localSetting.GetSettingValue(Dictionaries.ApplicationLiterals.KeyworkdExpandEarthmat) != null)
            {
                _earthmatHeaderExpansion = (bool)localSetting.GetSettingValue(Dictionaries.ApplicationLiterals.KeyworkdExpandEarthmat);
            }
            if (localSetting.GetSettingValue(Dictionaries.ApplicationLiterals.KeyworkdExpandSample) != null)
            {
                _sampleHeaderExpansion = (bool)localSetting.GetSettingValue(Dictionaries.ApplicationLiterals.KeyworkdExpandSample);
            }
            if (localSetting.GetSettingValue(Dictionaries.ApplicationLiterals.KeyworkdExpandStation) != null)
            {
                _stationHeaderExpansion = (bool)localSetting.GetSettingValue(Dictionaries.ApplicationLiterals.KeyworkdExpandStation);
            }
            if (localSetting.GetSettingValue(Dictionaries.ApplicationLiterals.KeywordExpandDocument) != null)
            {
                _documentHeaderExpansion = (bool)localSetting.GetSettingValue(Dictionaries.ApplicationLiterals.KeywordExpandDocument);
            }
            if (localSetting.GetSettingValue(Dictionaries.ApplicationLiterals.KeywordExpandStructure) != null)
            {
                _structureHeaderExpansion = (bool)localSetting.GetSettingValue(Dictionaries.ApplicationLiterals.KeywordExpandStructure); 
            }
            if (localSetting.GetSettingValue(Dictionaries.ApplicationLiterals.KeywordExpandPflow) != null)
            {
                _pflowHeaderExpansion = (bool)localSetting.GetSettingValue(Dictionaries.ApplicationLiterals.KeywordExpandPflow);
            }
            if (localSetting.GetSettingValue(Dictionaries.ApplicationLiterals.KeywordExpandFossil) != null)
            {
                _fossilHeaderExpansion = (bool)localSetting.GetSettingValue(Dictionaries.ApplicationLiterals.KeywordExpandFossil);
            }
            if (localSetting.GetSettingValue(Dictionaries.ApplicationLiterals.KeywordExpandMineral) != null)
            {
                _mineralHeaderExpansion = (bool)localSetting.GetSettingValue(Dictionaries.ApplicationLiterals.KeywordExpandMineral);
            }
            if (localSetting.GetSettingValue(Dictionaries.ApplicationLiterals.KeywordExpandMineralAlt) != null)
            {
                _mineralAltHeaderExpansion = (bool)localSetting.GetSettingValue(Dictionaries.ApplicationLiterals.KeywordExpandMineralAlt);
            }
            if (localSetting.GetSettingValue(Dictionaries.ApplicationLiterals.KeywordExpandEnv) != null)
            {
                _environmentHeaderExpansion = (bool)localSetting.GetSettingValue(Dictionaries.ApplicationLiterals.KeywordExpandEnv);
            }
        }

        /// <summary>
        /// Fill earthmat detail report if it is expanded else, remove sample panel
        /// </summary>
        public void EarthMatExpandPanel_ExpandedChanged()
        {
            if (_earthmatHeaderExpansion && pageLoading)
            {
                FillEarthmatFromStation();
            }
            else
            {
                _reportDetailSample.Clear();
            }
        }

        /// <summary>
        /// Fill sample detail report if it is expanded
        /// </summary>
        public void SampleExpandPanel_ExpandedChanged()
        {
            if (_sampleHeaderExpansion && pageLoading)
            {
                FillSample();
            }

        }

        /// <summary>
        /// Fill sample detail report if it is expanded
        /// </summary>
        public void StructureExpandPanel_ExpandedChanged()
        {
            if (_structureHeaderExpansion && pageLoading)
            {
                FillStructure();
            }

        }

        //Will fill the document report if it is expanded
        public void DocumentExpandPanel_ExpandedChanged()
        {
            if (_documentHeaderExpansion && pageLoading && _reportDetailedStation.Count > 0)
            {
                FillDocument();
            }
        }

        /// <summary>
        /// Will fill the pflow report if it is expanded
        /// </summary>
        public void PflowExpandPanel_ExpandedChanged()
        {
            if (_pflowHeaderExpansion && pageLoading)
            {
                FillPflow();
            }
        }

        /// <summary>
        /// Will fill the fossil report if it is expanded
        /// </summary>
        public void FossilExpandPanel_ExpandedChanged()
        {
            if (_fossilHeaderExpansion && pageLoading)
            {
                FillFossil();
            }
        }

        /// <summary>
        /// Will fill the fossil report if it is expanded
        /// </summary>
        public void MineralExpandPanel_ExpandedChanged()
        {
            if (_mineralHeaderExpansion && pageLoading)
            {
                FillMineral();
            }
        }
        /// <summary>
        /// Will fill the fossil report if it is expanded
        /// </summary>
        public void MineralAltExpandPanel_ExpandedChanged()
        {
            if (_mineralAltHeaderExpansion && pageLoading)
            {
                FillMineralAltFromStation();
            }
        }

        /// <summary>
        /// Will fill the fossil report if it is expanded
        /// </summary>
        public void EnvironmentExpandPanel_ExpandedChanged()
        {
            if (_environmentHeaderExpansion && pageLoading)
            {
                FillEnvironmentFromStation();
            }
        }

        #endregion

        #region VISIBILITY MANAGEMENT

        /// <summary>
        /// Will set header visibility for all needed forms
        /// </summary>
        public void SetHeadersVisibility()
        {

            List<Tuple<string, Visibility, string>> tableAsHeaders = new List<Tuple<string, Visibility, string>>() {
                new Tuple<string, Visibility, string>(DatabaseLiterals.TableEarthMat, _earthmatPanelVisibility, "EarthmatPanelVisibility"),
                new Tuple<string, Visibility, string>(DatabaseLiterals.TableSample, _samplePanelVisibility, "SamplePanelVisibility"),
                new Tuple<string, Visibility, string>(DatabaseLiterals.TableDocument, _documentPanelVisibility, "PhotoPanelVisibility"),
                new Tuple<string, Visibility, string>(DatabaseLiterals.TableStructure, _structurePanelVisibility, "StructurePanelVisibility"),
                new Tuple<string, Visibility, string>(DatabaseLiterals.TableFossil, _fossilPanelVisibility, "FossilPanelVisibility"),
                new Tuple<string, Visibility, string>(DatabaseLiterals.TablePFlow, _pflowPanelVisibility, "PFlowPanelVisibility"),
                new Tuple<string, Visibility, string>(DatabaseLiterals.TableEnvironment, _environmentPanelVisibility, "EnvironmentPanelVisibility"),
                new Tuple<string, Visibility, string>(DatabaseLiterals.TableMineral, _mineralPanelVisibility, "MineralPanelVisibility"),
                new Tuple<string, Visibility, string>(DatabaseLiterals.TableMineralAlteration, _mineralAltPanelVisibility, "MineralAltPanelVisibility"),};

            foreach (Tuple<string, Visibility, string> tables in tableAsHeaders)
            {
                SetHeaderVisibility(tables.Item1, tables.Item2, tables.Item3);
            }

        }


        /// <summary>
        /// Will validate and instantiate default for header visibility in field note page
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="inTableVisibility"></param>
        /// <param name="propertyName"></param>
        public void SetHeaderVisibility(string tableName, Visibility inTableVisibility, string propertyName )
        {
            if (localSetting.GetSettingValue(tableName) != null) 
            {
                if ((bool)localSetting.GetSettingValue(tableName))
                {
                    inTableVisibility = Visibility.Visible;
                }
                else
                {
                    inTableVisibility = Visibility.Collapsed;
                } 
                
            }
            else
            {
                inTableVisibility = Visibility.Visible;
            }

            RaisePropertyChanged(propertyName);
        }

        #endregion

        #region OPACITY MANAGEMENT

        /// <summary>
        /// Will set the different icons opacity on the header
        /// based right of usage (if parent is selected can add, if no can't add, if record selected can edit, else no)
        /// </summary>
        public void SetHeaderIconOpacity()
        {
            //Variables
            bool hasEarthmat = false;
            bool hasSamples = false;
            bool hasDocument = false;
            bool hasStructure = false;
            bool hasPflow = false;
            bool hasFossil = false;
            bool hasMineral = false;
            bool hasMineralAlt = false;
            bool hasStationLocation = false;
            bool hasEnvironment = false;
            bool isWaypoint = false;

            //Default
            _stationIconOpacity = enableOpacity;

            _earthmatAddIconOpacity = enableOpacity; //Enable child of station
            _earthmatAddIconColor.Color = GetTableColor(DatabaseLiterals.TableEarthMat);

            _documentAddIconOpacity = enableOpacity; //Enable child of station
            _documentAddIconColor.Color = GetTableColor(DatabaseLiterals.TableDocument);

            _mineralAltAddIconOpacity = enableOpacity; //Enable child of station
            _mineralAltAddIconColor.Color = GetTableColor(DatabaseLiterals.TableMineralAlteration);

            _locationAddIconOpacity = enableOpacity; //Enable child of station
            _locationAddIconColor.Color = GetTableColor(DatabaseLiterals.TableLocation);

            _sampleAddIconOpacity = disableOpacity; //Disable child of childs
            _sampleAddIconColor.Color = GetTableColor(string.Empty);

            _structureAddIconOpacity = disableOpacity; //Disable child of childs
            _structureAddIconColor.Color = GetTableColor(string.Empty);

            _pflowAddIconOpacity = disableOpacity; //Disable child of childs
            _pflowAddIconColor.Color = GetTableColor(string.Empty);

            _fossilAddIconOpacity = disableOpacity; //Disable child of childs
            _fossilAddIconColor.Color = GetTableColor(string.Empty);

            _mineralAddIconOpacity = disableOpacity; //Disable child of childs 
            _mineralAddIconColor.Color = GetTableColor(string.Empty);

            _environmentAddIconOpacity = enableOpacity; //Enable child of station
            _environmentAddIconColor.Color = GetTableColor(DatabaseLiterals.TableEnvironment);

            //Enable add icon for children 
            if (_reportEarthmatIndex != -1)
            {
                _earthmatIconOpacity = enableOpacity;
                _earthmatAddIconOpacity = enableOpacity;

                _sampleAddIconOpacity = enableOpacity;
                _sampleAddIconColor.Color = GetTableColor(DatabaseLiterals.TableSample);

                _structureAddIconOpacity = enableOpacity;
                _structureAddIconColor.Color = GetTableColor(DatabaseLiterals.TableStructure);

                _pflowAddIconOpacity = enableOpacity;
                _pflowAddIconColor.Color = GetTableColor(DatabaseLiterals.TablePFlow);

                _fossilAddIconOpacity = enableOpacity;
                _fossilAddIconColor.Color = GetTableColor(DatabaseLiterals.TableFossil);

                _mineralAddIconOpacity = enableOpacity;
                _mineralAddIconColor.Color = GetTableColor(DatabaseLiterals.TableMineral);


                hasEarthmat = true;
            }
            if (_reportSampleIndex != -1)
            {
                _sampleIconOpacity = enableOpacity;
                hasSamples = true;
            }
            if (_reportDocumentIndex != -1)
            {
                _documentIconOpacity = enableOpacity;
                hasDocument = true;
            }
            if (_reportStructureIndex != -1)
            {
                _structureIconOpacity = enableOpacity;
                hasStructure = true;
            }
            if (_reportPflowIndex != -1)
            {
                _pflowIconOpacity = enableOpacity;
                hasPflow = true;
            }
            if (_reportFossilIndex != -1)
            {
                _fossilIconOpacity = enableOpacity;
                hasFossil = true;
            }
            if (_reportMineralIndex != -1)
            {
                _mineralIconOpacity = enableOpacity;
                hasMineral = true;
            }
            if (_reportMineralizationAlterationIndex != -1)
            {
                _mineralAltIconOpacity = enableOpacity;
                _mineralAltAddIconOpacity = enableOpacity;

                _mineralAddIconOpacity = enableOpacity;
                _mineralAddIconColor.Color = GetTableColor(DatabaseLiterals.TableMineral);

                hasMineralAlt = true;
            }
            if (_reportStationIndex != -1)
            {
                if (_reportDetailedStation.Count > 0)
                {
                    _stationIconOpacity = enableOpacity;
                    _earthmatAddIconOpacity = enableOpacity;
                    _mineralAltAddIconOpacity = enableOpacity;
                    _documentAddIconOpacity = enableOpacity;
                    _environmentAddIconOpacity = enableOpacity;
                    hasStationLocation = true;

                    if (_reportDetailedStation[_reportStationIndex].GenericAliasName != null && _reportDetailedStation[_reportStationIndex].GenericAliasName.Contains(Dictionaries.DatabaseLiterals.KeywordStationWaypoint))
                    {
                        isWaypoint = true;
                    }
                    else
                    {
                        isWaypoint = false;
                    }
                }

            }
            if (_reportEnvironmentIndex != -1)
            {
                _environmentIconOpacity = enableOpacity;
                _environmentAddIconOpacity = enableOpacity;

                hasEnvironment = true;
            }

            // Enable add icons for children, special case here if there is only one station
            if (_reportDetailedStation.Count > 0 && _reportDetailedStation.Count == 1)
            {
                _stationIconOpacity = enableOpacity;
                _earthmatAddIconOpacity = enableOpacity;
                _mineralAltAddIconOpacity = enableOpacity;
                _documentAddIconOpacity = enableOpacity;
                _environmentAddIconOpacity = enableOpacity;

                hasStationLocation = true;

                if (_reportDetailedStation[0].GenericAliasName.Contains(Dictionaries.DatabaseLiterals.KeywordStationWaypoint))
                {
                    isWaypoint = true;
                }
                else
                {
                    isWaypoint = false;
                }
            }

            #region Disable icons if no record are found or parent is gone missing
            if (!hasSamples)
            {
                _sampleIconOpacity = disableOpacity;
            }
            if (!hasStructure)
            {
                _structureIconOpacity = disableOpacity;
            }
            if (!hasFossil)
            {
                _fossilIconOpacity = disableOpacity;
            }
            if (!hasPflow)
            {
                _pflowIconOpacity = disableOpacity;
            }
            if (!hasMineral)
            {
                _mineralIconOpacity = disableOpacity;
            }
            if (!hasEarthmat)
            {
                _earthmatIconOpacity = disableOpacity;

                _sampleAddIconOpacity = disableOpacity; //Missing parent
                _sampleAddIconColor.Color = _sampleColor.Color; // Header color (could be activated or not)

                _structureAddIconOpacity = disableOpacity; //Missing parent
                _structureAddIconColor.Color = _structColor.Color; // Header color (could be activated or not)

                _pflowAddIconOpacity = disableOpacity;//Missing parent
                _pflowAddIconColor.Color = _paleoflowColor.Color; // Header color (could be activated or not)

                _fossilAddIconOpacity = disableOpacity;//Missing parent
                _fossilAddIconColor.Color = _fossilColor.Color; // Header color (could be activated or not)



            }
            if (!hasDocument)
            {
                _documentIconOpacity = disableOpacity;
            }
            if (!hasStationLocation || _reportDetailedStation.Count == 0)
            {
                _stationIconOpacity = disableOpacity;
                _earthmatAddIconOpacity = disableOpacity;
                _mineralAltAddIconOpacity = disableOpacity;
                _documentAddIconOpacity = disableOpacity;
                _environmentAddIconOpacity = disableOpacity;
            }
            if (!hasMineralAlt)
            {
                _mineralAltIconOpacity = disableOpacity;
            }
            if (!hasEnvironment)
            {
                _environmentIconOpacity = disableOpacity;
            }
            if (isWaypoint)
            {
                _earthmatAddIconOpacity = disableOpacity;
                _mineralAltAddIconOpacity = disableOpacity;
            }
            if (!hasEarthmat && !hasMineralAlt)
            {
                _mineralAddIconOpacity = disableOpacity;//Missing parent
                _mineralAddIconColor.Color = _mineralColor.Color; // Header color (could be activated or not)
            }

            #endregion

            #region UPDATE UI
            RaisePropertyChanged("StationIconOpacity");
            RaisePropertyChanged("LocationIconOpacity");

            RaisePropertyChanged("EarthmatIconOpacity");
            RaisePropertyChanged("EarthmatAddIconOpacity");
            RaisePropertyChanged("EarthmatAddIconColor");

            RaisePropertyChanged("SampleIconOpacity");
            RaisePropertyChanged("SampleAddIconOpacity");
            RaisePropertyChanged("SampleAddIconColor");

            RaisePropertyChanged("DocumentIconOpacity");
            RaisePropertyChanged("DocumentAddIconOpacity");
            RaisePropertyChanged("DocumentAddIconColor"); 

            RaisePropertyChanged("StructureIconOpacity"); 
            RaisePropertyChanged("StructureAddIconOpacity");
            RaisePropertyChanged("StructAddIconColor");

            RaisePropertyChanged("PflowAddIconOpacity");
            RaisePropertyChanged("PflowIconOpacity");
            RaisePropertyChanged("PaleoflowAddIconColor");

            RaisePropertyChanged("FossilAddIconOpacity");
            RaisePropertyChanged("FossilIconOpacity");
            RaisePropertyChanged("FossilAddIconColor");

            RaisePropertyChanged("MineralAddIconOpacity");
            RaisePropertyChanged("MineralIconOpacity");
            RaisePropertyChanged("MineralAddIconColor");

            RaisePropertyChanged("MineralAltAddIconOpacity");
            RaisePropertyChanged("MineralAltIconOpacity");
            RaisePropertyChanged("MineralAltAddIconColor");

            RaisePropertyChanged("EnvironmentAddIconOpacity");
            RaisePropertyChanged("EnvironmentIconOpacity");
            RaisePropertyChanged("EnvironmentAddIconColor");

            #endregion
        }

        /// <summary>
        /// Will set opapcity the vertical rectangle on the left of the header based on the 
        /// number of records in the database. No record no rectangle, >1 records, full opacity.
        /// Will also set as grey all controls of the header if there is no records, else
        /// header will be set to its related color based on table 
        /// </summary>
        /// <param name="tableToUpdateHeader"></param>
        public void SetHeaderColorOpacity(string tableToUpdateHeader)
        {
            //Create disable color brush
            Color dc = new Color();
            if (Application.Current.Resources[resourceNameDisableColor] != null)
            {
                dc = (Color)Application.Current.Resources[resourceNameDisableColor];
            }

            //For station records
            if (tableToUpdateHeader == Dictionaries.DatabaseLiterals.TableStation)
            {

                if (_reportDetailedStation.Count != 0)
                {
                    
                    _stationIconColorOpacity = enableOpacity;

                    Color stc = new Color();
                    if (Application.Current.Resources[resourcenameFieldStationColor] != null)
                    {
                        stc = (Color)Application.Current.Resources[resourcenameFieldStationColor];
                    }

                    _stationColor.Color = stc;
                }
                else
                {
                    _stationIconColorOpacity = fullDisableOpacity;
                    _stationColor.Color = dc;
                }
                RaisePropertyChanged("StationIconColorOpacity");
                RaisePropertyChanged("StationColor");
            }

            //For location records
            if (tableToUpdateHeader == Dictionaries.DatabaseLiterals.TableLocation)
            {

                if (_reportDetailedStation.Count != 0)
                {

                    _locationIconColorOpacity = enableOpacity;

                    Color lc = new Color();
                    if (Application.Current.Resources[resourcenameFieldLocationColor] != null)
                    {
                        lc = (Color)Application.Current.Resources[resourcenameFieldLocationColor];
                    }

                    _locationColor.Color = lc;
                }
                else
                {
                    _locationIconColorOpacity = fullDisableOpacity;
                    _locationColor.Color = dc;
                }
                RaisePropertyChanged("LocationIconColorOpacity"); 
                RaisePropertyChanged("LocationColor");
            }

            //For earthmat records
            if (tableToUpdateHeader == Dictionaries.DatabaseLiterals.TableEarthMat)
            {

                if (_reportDetailedEarthmat.Count != 0)
                {
                    _earthmatIconColorOpacity = enableOpacity;


                    Color ec = new Color();
                    if (Application.Current.Resources[resourcenameFieldEarthmatColor] != null)
                    {
                        ec = (Color)Application.Current.Resources[resourcenameFieldEarthmatColor];
                    }

                    _earthmatColor.Color = ec;
                }
                else
                {
                    _earthmatIconColorOpacity = fullDisableOpacity;
                    _earthmatColor.Color = dc;
                }
                RaisePropertyChanged("EarthmatIconColorOpacity");
                RaisePropertyChanged("EarthmatColor");
            }

            //For sample records
            if (tableToUpdateHeader == Dictionaries.DatabaseLiterals.TableSample)
            {

                if (_reportDetailSample.Count != 0)
                {
                    _sampleIconColorOpacity = enableOpacity;

                    Color sc = new Color();
                    if (Application.Current.Resources[resourcenameFieldSampleColor] != null)
                    {
                        sc = (Color)Application.Current.Resources[resourcenameFieldSampleColor];
                    }

                    _sampleColor.Color = sc;
                }
                else
                {
                    _sampleIconColorOpacity = fullDisableOpacity;
                    _sampleColor.Color = dc;
                }
                RaisePropertyChanged("SampleIconColorOpacity");
                RaisePropertyChanged("SampleColor"); 
            }

            //Document records
            if (tableToUpdateHeader == Dictionaries.DatabaseLiterals.TableDocument)
            {
                if (_reportDetailedDocument.Count != 0)
                {
                    _documentIconColorOpacity = enableOpacity;
                    Color docC = new Color();
                    if (Application.Current.Resources[resourcenameFieldDocumentColor] != null)
                    {
                        docC = (Color)Application.Current.Resources[resourcenameFieldDocumentColor];
                    }

                    _documentColor.Color = docC;
                }
                else
                {
                    _documentIconColorOpacity = fullDisableOpacity;
                    _documentColor.Color = dc;
                }
                RaisePropertyChanged("DocumentIconColorOpacity");
                RaisePropertyChanged("DocumentColor");
            }

            //Structure records
            if (tableToUpdateHeader == Dictionaries.DatabaseLiterals.TableStructure)
            {
                if (_reportDetailedStructure.Count != 0)
                {
                    _structureIconColorOpacity = enableOpacity;
                    Color strucC = new Color();
                    if (Application.Current.Resources[resourcenameFieldStructColor] != null)
                    {
                        strucC = (Color)Application.Current.Resources[resourcenameFieldStructColor];
                    }

                    _structColor.Color = strucC;
                }
                else
                {
                    _structureIconColorOpacity = fullDisableOpacity;
                    _structColor.Color = dc;
                }
                RaisePropertyChanged("StructureIconColorOpacity");
                RaisePropertyChanged("StructColor");
            }

            //paleoflow records
            if (tableToUpdateHeader == Dictionaries.DatabaseLiterals.TablePFlow)
            {
                if (_reportDetailedPflow.Count != 0)
                {
                    _pflowIconColorOpacity = enableOpacity;
                    Color pflowC = new Color();
                    if (Application.Current.Resources[resourcenameFieldPflowColor] != null)
                    {
                        pflowC = (Color)Application.Current.Resources[resourcenameFieldPflowColor];
                    }

                    _paleoflowColor.Color = pflowC;
                }
                else
                {
                    _pflowIconColorOpacity = fullDisableOpacity;
                    _paleoflowColor.Color = dc;
                }
                RaisePropertyChanged("PflowIconColorOpacity");
                RaisePropertyChanged("PaleoflowColor");
            }

            //fossil records
            if (tableToUpdateHeader == Dictionaries.DatabaseLiterals.TableFossil)
            {
                if (_reportDetailedFossil.Count != 0)
                {
                    _fossilIconColorOpacity = enableOpacity;
                    Color fc = new Color();
                    if (Application.Current.Resources[resourcenameFieldFossilColor] != null)
                    {
                        fc = (Color)Application.Current.Resources[resourcenameFieldFossilColor];
                    }

                    _fossilColor.Color = fc;
                }
                else
                {
                    _fossilIconColorOpacity = fullDisableOpacity;
                    _fossilColor.Color = dc;
                }
                RaisePropertyChanged("FossilIconColorOpacity");
                RaisePropertyChanged("FossilColor");
            }

            //mineral records
            if (tableToUpdateHeader == Dictionaries.DatabaseLiterals.TableMineral)
            {
                if (_reportDetailedMinerals.Count != 0)
                {
                    _mineralIconColorOpacity = enableOpacity;
                    Color mc = new Color();
                    if (Application.Current.Resources[resourcenameFieldMineralColor] != null)
                    {
                        mc = (Color)Application.Current.Resources[resourcenameFieldMineralColor];
                    }

                    _mineralColor.Color = mc;
                }
                else
                {
                    _mineralIconColorOpacity = fullDisableOpacity;
                    _mineralColor.Color = dc;
                }
                RaisePropertyChanged("MineralIconColorOpacity");
                RaisePropertyChanged("MineralColor");
            }

            //mineral atleration records
            if (tableToUpdateHeader == Dictionaries.DatabaseLiterals.TableMineralAlteration)
            {
                if (_reportDetailedMineralAlt.Count != 0)
                {
                    _mineralAltIconColorOpacity = enableOpacity;
                    Color mc = new Color();
                    if (Application.Current.Resources[resourcenameFieldMineralAltColor] != null)
                    {
                        mc = (Color)Application.Current.Resources[resourcenameFieldMineralAltColor];
                    }

                    _mineralAltColor.Color = mc;
                }
                else
                {
                    _mineralAltIconColorOpacity = fullDisableOpacity;
                    _mineralAltColor.Color = dc;
                }
                RaisePropertyChanged("MineralAltIconColorOpacity");
                RaisePropertyChanged("MineralAltColor");
            }

            //environment records
            if (tableToUpdateHeader == Dictionaries.DatabaseLiterals.TableEnvironment)
            {
                if (_reportDetailEnvironment.Count != 0)
                {
                    _environmentIconColorOpacity = enableOpacity;
                    Color mc = new Color();
                    if (Application.Current.Resources[resourcenameFieldEnvironmentColor] != null)
                    {
                        mc = (Color)Application.Current.Resources[resourcenameFieldEnvironmentColor];
                    }

                    _envColor.Color = mc;
                }
                else
                {
                    _environmentIconColorOpacity = fullDisableOpacity;
                    _envColor.Color = dc;
                }
                RaisePropertyChanged("EnvironmentIconColorOpacity");
                RaisePropertyChanged("EnvironmentColor");
            }

        }

        #endregion

        #region COLOR MANAGEMENT

        /// <summary>
        /// From a given table name, will return a color from custom resources file.
        /// </summary>
        /// <param name="tableName">An empty string value will return a disable color</param>
        /// <returns></returns>
        public Color GetTableColor(string tableName)
        {
            switch (tableName)
            {
                case DatabaseLiterals.TableStation:
                    Color stc = new Color();
                    if (Application.Current.Resources[resourcenameFieldStationColor] != null)
                    {
                        stc = (Color)Application.Current.Resources[resourcenameFieldStationColor];
                    }
                    return stc;
                case Dictionaries.DatabaseLiterals.TableEarthMat:
                    Color ec = new Color();
                    if (Application.Current.Resources[resourcenameFieldEarthmatColor] != null)
                    {
                        ec = (Color)Application.Current.Resources[resourcenameFieldEarthmatColor];
                    }
                    return ec;
                case DatabaseLiterals.TableLocation:
                    Color lc = new Color();
                    if (Application.Current.Resources[resourcenameFieldLocationColor] != null)
                    {
                        lc = (Color)Application.Current.Resources[resourcenameFieldLocationColor];
                    }
                    return lc;
                case DatabaseLiterals.TableSample:
                    Color sc = new Color();
                    if (Application.Current.Resources[resourcenameFieldSampleColor] != null)
                    {
                        sc = (Color)Application.Current.Resources[resourcenameFieldSampleColor];
                    }
                    return sc;
                case DatabaseLiterals.TableStructure:
                    Color strucC = new Color();
                    if (Application.Current.Resources[resourcenameFieldStructColor] != null)
                    {
                        strucC = (Color)Application.Current.Resources[resourcenameFieldStructColor];
                    }
                    return strucC;

                case DatabaseLiterals.TableMineral:
                    Color mc = new Color();
                    if (Application.Current.Resources[resourcenameFieldMineralColor] != null)
                    {
                        mc = (Color)Application.Current.Resources[resourcenameFieldMineralColor];
                    }
                    return mc;
                case DatabaseLiterals.TablePFlow:
                    Color pflowC = new Color();
                    if (Application.Current.Resources[resourcenameFieldPflowColor] != null)
                    {
                        pflowC = (Color)Application.Current.Resources[resourcenameFieldPflowColor];
                    }
                    return pflowC;
                case DatabaseLiterals.TableFossil:
                    Color fc = new Color();
                    if (Application.Current.Resources[resourcenameFieldFossilColor] != null)
                    {
                        fc = (Color)Application.Current.Resources[resourcenameFieldFossilColor];
                    }
                    return fc;
                case DatabaseLiterals.TableDocument:
                    Color docC = new Color();
                    if (Application.Current.Resources[resourcenameFieldDocumentColor] != null)
                    {
                        docC = (Color)Application.Current.Resources[resourcenameFieldDocumentColor];
                    }
                    return docC;
                case DatabaseLiterals.TableMineralAlteration:
                    Color maC = new Color();
                    if (Application.Current.Resources[resourcenameFieldMineralAltColor] != null)
                    {
                        maC = (Color)Application.Current.Resources[resourcenameFieldMineralAltColor];
                    }
                    return maC;
                case DatabaseLiterals.TableEnvironment:
                    Color envC = new Color();
                    if (Application.Current.Resources[resourcenameFieldEnvironmentColor] != null)
                    {
                        envC = (Color)Application.Current.Resources[resourcenameFieldEnvironmentColor];
                    }
                    return envC;
                default:
                    //Create disable color brush
                    Color dc = new Color();
                    if (Application.Current.Resources[resourceNameDisableColor] != null)
                    {
                        dc = (Color)Application.Current.Resources[resourceNameDisableColor];
                    }
                    return dc;
            }
        }

        #endregion

        #region LOCATION EVENTS
        public void LocationAddIcon_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Models.FieldLocation emptyFieldLocation = new FieldLocation
            {
                LocationElev = 0.0,
                LocationLat = 0.0,
                LocationLong = 0.0,
                LocationEntryType = Dictionaries.DatabaseLiterals.locationEntryTypeManual,
                LocationID = idCalculator.CalculateLocationID(), //Calculate new value
                LocationAlias = idCalculator.CalculateLocationAlias(string.Empty), //Calculate new value
                MetaID = int.Parse(localSetting.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoID).ToString()) //Foreign key
            };

            FieldNotes emptyFieldNotes = new FieldNotes
            {
                location = emptyFieldLocation
            };

            PopLocation(emptyFieldNotes, false);
        }

        public void LocationEditIcon_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            EditLocation();
        }
        public void ReportLocationList_DoubleTapped(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            EditLocation();
        }
        /// <summary>
        /// Will pop and fill station dialog with existing values
        /// </summary>
        public void EditLocation()
        {
            PopLocation(_reportDetailedStation[ReportStationListIndex], true);
        }

        public void ViewModel_newLocationEdit(object sender)
        {
            FillStationFromList();
        }

        #endregion

        #region STATION EVENTS
        public void StationEditIcon_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            EditStation();
        }

        /// <summary>
        /// Whenever a station have been edited or added, this event will be triggered and will force a refresh on stations
        /// </summary>
        /// <param name="sender"></param>
        public void ViewModel_newStationEdit(object sender)
        {
            FillStationFromList();
        }

        public void ReportStationList_DoubleTapped(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            EditStation();
        }

        /// <summary>
        /// Will pop and fill station dialog with existing values
        /// </summary>
        public void EditStation()
        {
            
            if (_reportStationIndex != -1)
            {
                PopStation(_reportDetailedStation[_reportStationIndex]);
            }
            
        }

        #endregion

        #region EARTHMAT EVENTS
        public void EarthMatAddIcon_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (!_reportDetailedStation[_reportStationIndex].GenericAliasName.Contains(Dictionaries.DatabaseLiterals.KeywordStationWaypoint))
            {
                PopEarthmat(_reportDetailedStation[_reportStationIndex], false);
            }
            
        }

        /// <summary>
        /// Will launch whenever a new earthmat or an existing one has been edited, and will force a refresh on the earthmats
        /// </summary>
        /// <param name="sender"></param>
        public void ViewModel_newEarthmatEdit(object sender)
        {
            FillEarthmatFromStation();

            //Select last
            try
            {
                _reportEarthmatIndex = _reportDetailedEarthmat.Count() - 1;
                RaisePropertyChanged("ReportEarhtmatIndex");
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// An event to detect is a user wants to edit a selected earthmat
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void EarthMatEditIcon_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            EditEarthmat();
        }

        public void ReportEarthmatList_DoubleTapped(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            EditEarthmat();
        }

        /// <summary>
        /// Will pop and fill earthmat dialog with existing values
        /// </summary>
        public void EditEarthmat()
        {

            if (_reportEarthmatIndex != -1)
            {
                PopEarthmat(_reportDetailedEarthmat[_reportEarthmatIndex], true);
            }
        }

        #endregion

        #region SAMPLE EVENTS
        public void textBlock_FieldSample_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //Cast
            TextBlock currentTextBlock = sender as TextBlock;
            currentTextBlock.Text = currentTextBlock.Text + " ";
        }
        public void SampleAddIcon_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {

            if (_reportEarthmatIndex != -1)
            {
                PopSample(_reportDetailedEarthmat[_reportEarthmatIndex], false);
            }
        }

        public void ViewModel_newSampleEdit(object sender)
        {
            FillSample();
        }

        public void SampleEditIcon_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            EditSample();
        }

        public void ReportSampleList_DoubleTapped(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            EditSample();
        }

        /// <summary>
        /// Will pop and fill sample dialog with existing values
        /// </summary>
        public void EditSample()
        {

            if (_reportSampleIndex != -1)
            {
                PopSample(_reportDetailSample[_reportSampleIndex], true);
            }
        }
        #endregion

        #region DOCUMENT/PHOTO EVENTS
        public void DocumentAddIcon_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            PopDocument(_reportDetailedStation[_reportStationIndex], false);
        }

        public void ViewModel_newDocumentEdit(object sender)
        {
            FillDocument();
        }

        public void DocumentEditIcon_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            EditDocument();
        }

        public void ReportDocumentList_DoubleTapped(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            EditDocument();
        }

        /// <summary>
        /// Will pop and fill document dialog with existing values
        /// </summary>
        public void EditDocument()
        {
            if (_reportDocumentIndex != -1)
            {
                PopDocument(_reportDetailedDocument[_reportDocumentIndex], true);
            }
        }

        #endregion

        #region STRUCTURE EVENTS
        public void textBlock_FieldStructure_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //Cast
            TextBlock currentTextBlock = sender as TextBlock;
            currentTextBlock.Text = currentTextBlock.Text + " ";
        }

        public void StructureAddIcon_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {

            if (_reportEarthmatIndex != -1)
            {
                PopStructure(_reportDetailedEarthmat[_reportEarthmatIndex], false);
            }

        }

        public void StructureEditIcon_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            EditStructure();
        }

        private void StrucViewModel_newStructureEdit(object sender)
        {
            FillStructure();
        }

        public void ReportStructureList_DoubleTapped(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            EditStructure();
        }

        /// <summary>
        /// Will pop structure dialog and fill it with existing values
        /// </summary>
        public void EditStructure()
        {

            if (_reportStructureIndex != -1)
            {
                PopStructure(_reportDetailedStructure[_reportStructureIndex], true);
            }
        }

        #endregion

        #region PFLOW EVENTS
        public void textBlock_FieldPflow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //Cast
            TextBlock currentTextBlock = sender as TextBlock;
            currentTextBlock.Text = currentTextBlock.Text + " ";
        }

        public void PflowAddIcon_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (_reportEarthmatIndex != -1)
            {
                PopPaleoflow(_reportDetailedEarthmat[_reportEarthmatIndex], false);
            }

        }

        public void PflowEditIcon_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            EditPflow();
        }

        private void PFlowViewModel_newPflowEdit(object sender)
        {
            FillPflow();
        }

        public void ReportPflowList_DoubleTapped(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            EditPflow();
        }

        /// <summary>
        /// Will pop and fill paleoflow dialog with existing values
        /// </summary>
        public void EditPflow()
        {
            if (_reportEarthmatIndex != -1)
            {
                PopPaleoflow(_reportDetailedPflow[_reportEarthmatIndex], true);
            }
        }

        #endregion

        #region FOSSIL EVENTS
        public void textBlock_FieldFossil_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //Cast
            TextBlock currentTextBlock = sender as TextBlock;
            currentTextBlock.Text = currentTextBlock.Text + " ";
        }

        public void FossilAddIcon_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (_reportEarthmatIndex != -1)
            {
                PopFossil(_reportDetailedEarthmat[_reportEarthmatIndex], false);
            }
        }

        public void FossilEditIcon_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            EditFossil();
        }

        private void FossilViewModel_newFossilEdit(object sender)
        {
            FillFossil();
        }

        public void ReportFossilList_DoubleTapped(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            EditFossil();
        }

        /// <summary>
        /// Will pop and fill and fossil dialog with existing values
        /// </summary>
        public void EditFossil()
        {
            if (_reportFossilIndex != -1)
            {
                PopFossil(_reportDetailedFossil[_reportFossilIndex], true);
            }
        }

        #endregion

        #region MINERAL EVENTS
        public void textBlock_FieldMineral_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //Cast
            TextBlock currentTextBlock = sender as TextBlock;
            currentTextBlock.Text = currentTextBlock.Text + " ";
        }

        public void MineralAddIcon_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (_reportEarthmatIndex != -1)
            {
                PopMineral(_reportDetailedEarthmat[_reportEarthmatIndex], false);
            }

            if (_reportMineralizationAlterationIndex != -1)
            {
                PopMineral(_reportDetailedMineralAlt[_reportMineralizationAlterationIndex], false);
            }

        }

        public void MineralEditIcon_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            EditMineral();
        }

        private void MineralViewModel_newMineralEdit(object sender)
        {
            FillMineral();
        }

        public void ReportMineralList_DoubleTapped(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            EditMineral();
        }

        /// <summary>
        /// Will pop and fill paleoflow dialog with existing values
        /// </summary>
        public void EditMineral()
        {
            if (_reportMineralIndex != -1)
            {
                PopMineral(_reportDetailedMinerals[_reportMineralIndex], true);
            }

        }

        #endregion

        #region MINERAL ALTERATIONS EVENTS
        public void textBlock_FieldMineralAlt_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //Cast
            TextBlock currentTextBlock = sender as TextBlock;
            currentTextBlock.Text = currentTextBlock.Text + " ";
        }

        public void MineralAltAddIcon_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (!_reportDetailedStation[_reportStationIndex].GenericAliasName.Contains(Dictionaries.DatabaseLiterals.KeywordStationWaypoint))
            {
                PopMineralAlt(_reportDetailedStation[_reportStationIndex], false);
            }
            

        }

        public void MineralAltEditIcon_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            EditMineralAlt();
        }

        private void MineralAltViewModel_newMineralEdit(object sender)
        {
            FillMineralAltFromStation();
        }

        public void ReportMineralAltList_DoubleTapped(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            EditMineralAlt();
        }

        /// <summary>
        /// Will pop and fill paleoflow dialog with existing values
        /// </summary>
        public void EditMineralAlt()
        {
            if (_reportMineralizationAlterationIndex != -1)
            {
                PopMineralAlt(_reportDetailedMineralAlt[_reportMineralizationAlterationIndex], true);
            }
        }
        #endregion

        #region ENVIRONMENT EVENTS
        public void textBlock_FieldEnvironment_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //Cast
            TextBlock currentTextBlock = sender as TextBlock;
            currentTextBlock.Text = currentTextBlock.Text + " ";
        }

        public void EnvironmentAddIcon_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (!_reportDetailedStation[_reportStationIndex].GenericAliasName.Contains(Dictionaries.DatabaseLiterals.KeywordStationWaypoint))
            {
                PopEnvironment(_reportDetailedStation[_reportStationIndex], false);
            }


        }

        public void EnvironmentEditIcon_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            EditEnvironment();
        }

        /// <summary>
        /// Will pop and fill paleoflow dialog with existing values
        /// </summary>
        public void EditEnvironment()
        {
            if (_reportEnvironmentIndex != -1)
            {
                PopEnvironment(_reportDetailEnvironment[_reportEnvironmentIndex], true);
            }
        }

        public void ReportEnvironment_DoubleTapped(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            EditEnvironment();
        }

        private void EnvironmentViewModel_newMineralEdit(object sender)
        {
            FillEnvironmentFromStation();
        }

        #endregion

        #region DIALOG POP-UPS

        /// <summary>
        /// From a given report item, will open a sample dialog.
        /// </summary>
        /// <param name="sampleReport"></param>
        public void PopSample(FieldNotes sampleReport, bool doUpdate)
        {
            var modalSample = Window.Current.Content as ModalDialog;
            var viewSample = modalSample.ModalContent as Views.SampleDialog;
            viewSample = new Views.SampleDialog(sampleReport, false);
            viewSample.ViewModel.doSampleUpdate = doUpdate;
            viewSample.ViewModel.newSampleEdit += ViewModel_newSampleEdit;
            modalSample.ModalContent = viewSample;
            modalSample.IsModal = true;

        }

        /// <summary>
        /// From a given report item, will open a sample dialog.
        /// </summary>
        /// <param name="sampleReport"></param>
        public void PopLocation(FieldNotes locationReport, bool doUpdate)
        {
            var modalLocation = Window.Current.Content as ModalDialog;
            var viewLocation = modalLocation.ModalContent as Views.LocationDialog;
            viewLocation = new Views.LocationDialog(locationReport);
            viewLocation.locationVM.doLocationUpdate = doUpdate;
            viewLocation.locationVM.newLocationEdit += ViewModel_newLocationEdit;
            modalLocation.ModalContent = viewLocation;
            modalLocation.IsModal = true;

        }

        private void PopStructure(FieldNotes structureReport, bool doStructureUpdate)
        {
            var modelStructure = Window.Current.Content as ModalDialog;
            var viewStructure = modelStructure.ModalContent as Views.StructureDialog;
            viewStructure = new Views.StructureDialog(structureReport, false);
            viewStructure.strucViewModel.doStructureUpdate = doStructureUpdate;
            viewStructure.strucViewModel.newStructureEdit += StrucViewModel_newStructureEdit;
            modelStructure.ModalContent = viewStructure;
            modelStructure.IsModal = true;
        }

        private void PopPaleoflow(FieldNotes pflowReport, bool doPflowUpdate)
        {
            var modelPflow = Window.Current.Content as ModalDialog;
            var viewPflow = modelPflow.ModalContent as Views.PaleoflowDialog;
            viewPflow = new Views.PaleoflowDialog(pflowReport);
            viewPflow.pflowModel.doPflowUpdate = doPflowUpdate;
            viewPflow.pflowModel.newPflowEdit += PFlowViewModel_newPflowEdit;
            modelPflow.ModalContent = viewPflow;
            modelPflow.IsModal = true;
        }

        private void PopMineral(FieldNotes mineralReport, bool doMineralUpdate)
        {
            var modelMineral = Window.Current.Content as ModalDialog;
            var viewMineral = modelMineral.ModalContent as Views.MineralDialog;
            viewMineral = new Views.MineralDialog(mineralReport);
            viewMineral.MineralVM.doMineralUpdate = doMineralUpdate;
            viewMineral.MineralVM.newMineralEdit += MineralViewModel_newMineralEdit;
            modelMineral.ModalContent = viewMineral;
            modelMineral.IsModal = true;
        }

        private void PopFossil(FieldNotes fossilReport, bool doFossilUpdate)
        {
            var modelFossil = Window.Current.Content as ModalDialog;
            var viewFossil = modelFossil.ModalContent as Views.FossilDialog;
            viewFossil = new Views.FossilDialog(fossilReport);
            viewFossil.fossilModel.doFossilUpdate = doFossilUpdate;
            viewFossil.fossilModel.newFossilEdit += FossilViewModel_newFossilEdit;
            modelFossil.ModalContent = viewFossil;
            modelFossil.IsModal = true;
        }

        /// <summary>
        /// From a given report item, will open an earthmat dialog.
        /// </summary>
        /// <param name="earthmatReport"></param>
        /// <param name="doUpdate"></param>
        public void PopEarthmat(FieldNotes earthmatReport, bool doUpdate)
        {
            var modal = Window.Current.Content as ModalDialog;
            var view = modal.ModalContent as Views.EarthmatDialog;
            view = new Views.EarthmatDialog(earthmatReport);
            view.ViewModel.doEarthUpdate = doUpdate;
            view.ViewModel.newEarthmatEdit += ViewModel_newEarthmatEdit; //Detect whenever a save is commited on the database.
            modal.ModalContent = view;
            modal.IsModal = true;
        }

        /// <summary>
        /// From a given report, will open a station dialog
        /// </summary>
        /// <param name="stationReport"></param>
        public void PopStation(FieldNotes stationReport)
        {
            var modal = Window.Current.Content as ModalDialog;
            var view = modal.ModalContent as Views.StationDataPart;
            view = new Views.StationDataPart(stationReport, false);
            view.ViewModel.newStationEdit += ViewModel_newStationEdit; //Detect whenever a save is commited on the database.
            //view.Tag = ReportDetailedStation;
            modal.ModalContent = view;
            modal.IsModal = true;
        }

        /// <summary>
        /// From a given report item, will open a document dialog.
        /// </summary>
        /// <param name="sampleReport"></param>
        public void PopDocument(FieldNotes locationReport, bool doUpdate)
        {
            var modalDocument = Window.Current.Content as ModalDialog;
            var viewDocument = modalDocument.ModalContent as Views.DocumentDialog;
            viewDocument = new Views.DocumentDialog(locationReport, _reportDetailedStation[ReportStationListIndex], false);
            viewDocument.DocViewModel.doDocumentUpdate = doUpdate;
            viewDocument.DocViewModel.newDocumentEdit += ViewModel_newDocumentEdit; //Detect whenever a save is commited on the database
            modalDocument.ModalContent = viewDocument;
            modalDocument.IsModal = true;

        }

        /// <summary>
        /// From a given report item, will open a mineral alteration dialog
        /// </summary>
        /// <param name="mineralAltReport"></param>
        /// <param name="doMineralAltUpdate"></param>
        private void PopMineralAlt(FieldNotes mineralAltReport, bool doMineralAltUpdate)
        {
            var modelMineralAlt = Window.Current.Content as ModalDialog;
            var viewMineralAlt = modelMineralAlt.ModalContent as Views.MineralizationAlterationDialog;
            viewMineralAlt = new Views.MineralizationAlterationDialog(mineralAltReport);
            viewMineralAlt.MAViewModel.doMineralAltUpdate = doMineralAltUpdate;
            viewMineralAlt.MAViewModel.newMineralAltEdit += MineralAltViewModel_newMineralEdit;
            modelMineralAlt.ModalContent = viewMineralAlt;
            modelMineralAlt.IsModal = true;
        }

        /// <summary>
        /// From a given report item, will open an environment dialog
        /// </summary>
        /// <param name="envReport"></param>
        /// <param name="doEnvironmentUpdate"></param>
        private void PopEnvironment(FieldNotes envReport, bool doEnvironmentUpdate)
        {
            var modelEnvironment = Window.Current.Content as ModalDialog;
            var viewMEnvironment = modelEnvironment.ModalContent as Views.EnvironmentDialog;
            viewMEnvironment = new Views.EnvironmentDialog(envReport);
            viewMEnvironment.EnvViewModel.doEnvironmentUpdate = doEnvironmentUpdate;
            viewMEnvironment.EnvViewModel.newEnvironmentEdit += EnvironmentViewModel_newMineralEdit;
            modelEnvironment.ModalContent = viewMEnvironment;
            modelEnvironment.IsModal = true;
        }

        #endregion

        #region OTHER EVENTS

        /// <summary>
        /// If a different field books is selected refresh whole field note page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FieldBooksPageViewModel_newFieldBookSelected(object sender, string e)
        {
            _reportSummaryDateItems.Clear();
            RaisePropertyChanged("ReportDateItems");
            EmptyAll();
            FillSummaryReportDateItems();
        }

        /// <summary>
        /// Will be used to set some UI before loading has ended. Like header visibility, 
        /// icons opacity and header expansion from previous user selection.
        /// </summary>
        public void pageBeingLoaded()
        {
            SetHeadersVisibility();
            SetHeaderIconOpacity();
            SetHeaderExpansion();
        }

        /// <summary>
        /// When a item is selected in the summary (left panel)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ReportSummaryDate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Cast and set
            GridView inDetailListViewDate = sender as GridView;
            if (inDetailListViewDate != null)
            {
                if (inDetailListViewDate.SelectedIndex != -1)
                {
                    FieldNotes selectedReport = inDetailListViewDate.SelectedItem as FieldNotes;
                    FillStationFromList(); //Initiate date stations list

                    // Force grid view control to scroll to selected item else it doesn't work by default.
                    inDetailListViewDate.ScrollIntoView(inDetailListViewDate.SelectedItem, ScrollIntoViewAlignment.Leading);
                    inDetailListViewDate.UpdateLayout();
                }



            }
        }

        /// <summary>
        /// Triggered whenever a list item is selected in the right panel of the report
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ReportDetail_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            //Set the selected item in view model
            GridView currentSender = sender as GridView;
            if (currentSender.SelectedIndex != -1)
            {
                FieldNotes selectedReport = currentSender.SelectedItem as FieldNotes;

                if (selectedReport.GenericTableName == DatabaseLiterals.TableStation)
                {
                    FillEarthmatFromStation();
                    EmptyEarthmatChilds();      
                    FillLocation();
                    FillDocument();
                    FillMineralAltFromStation();
                    EmptyMineralizationAlterationChilds();
                    FillMineral();
                    FillEnvironmentFromStation();
                    
                }

                if (selectedReport.GenericTableName == DatabaseLiterals.TableEarthMat)
                {
                    FillSample();
                    FillMineral();
                    FillPflow();
                    FillStructure();
                    FillFossil();
                }

                if (selectedReport.GenericTableName == DatabaseLiterals.TableMineralAlteration)
                {
                    FillMineral();
                }

                


            }
            else
            {
                //Else empty children on each unselect
                if (currentSender.Name.ToLower().Contains(DatabaseLiterals.KeywordEarthmat))
                {
                    EmptyEarthmatChilds();
                }
                if (currentSender.Name.ToLower().Contains(DatabaseLiterals.KeywordMA))
                {
                    EmptyMineralizationAlterationChilds();
                }
            }

            SetHeaderIconOpacity();
        }

        /// <summary>
        /// Will unselect an already selected report and remove it from pool
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void Report_ItemClick(object sender, ItemClickEventArgs e)
        {
            //Set the selected item in view model
            GridView currentSender = sender as GridView;
            FieldNotes currentClickedReport = e.ClickedItem as FieldNotes;

            //Detect already selected item
            if (e.ClickedItem == currentSender.SelectedItem && !currentSender.Name.ToLower().Contains(DatabaseLiterals.KeywordStation))
            {
                await Task.Delay(100);
                currentSender.SelectedItem = null;

                SetHeaderIconOpacity();
            }


        }

        #endregion

        #region VALIDITY CHECK
        public void ValidateCheck(bool isValid, string inItemID, string originID)
        {

            if (!isValid)
            {
                if (!summaryReportValidity.ContainsKey(originID))
                {
                    summaryReportValidity[originID] = new List<string>();
                    summaryReportValidity[originID].Add(inItemID);
                }
                else
                {
                    summaryReportValidity[originID].Add(inItemID);
                }

                foreach (FieldNotes rps in ReportDetailedStation)
                {
                    if (rps.MainID == originID)
                    {
                        ReportDetailedStation[ReportDetailedStation.IndexOf(rps)].isValid = false;
                        RaisePropertyChanged("ReportItems");
                        break;
                    }
                }

            }
            else
            {
                if (summaryReportValidity.ContainsKey(originID))
                {
                    summaryReportValidity[originID].Remove(inItemID);
                    if (summaryReportValidity[originID].Count == 0)
                    {
                        foreach (FieldNotes rps in ReportDetailedStation)
                        {
                            if (rps.MainID == originID)
                            {
                                ReportDetailedStation[ReportDetailedStation.IndexOf(rps)].isValid = true;
                                summaryReportValidity.Remove(originID);
                                RaisePropertyChanged("ReportItems");
                                break;

                            }
                        }


                    }
                }
            }
        }
        #endregion

        #region NAVIGATION

        /// <summary>
        /// Will select wanted date and station in report. 
        /// Information can come from a navigate event, from the map page when a user
        /// clicks a on station and wants to see the related reports informations.
        /// </summary>
        /// <param name="userStationID"></param>
        /// <param name="userStationDate"></param>
        public void SetSelectedStationFromMapPage()
        {
            if (userSelectedStationDate!= string.Empty && userSelectedStationID != string.Empty)
            {
                //Select proper date
                foreach (FieldNotes dates in _reportSummaryDateItems)
                {
                    if (dates.station.StationVisitDate == userSelectedStationDate)
                    {
                        _reportDateIndex = _reportSummaryDateItems.IndexOf(dates);
                        
                        RaisePropertyChanged("ReportListViewDateIndex");
                        FillStationFromList(); //Refill station based on new selected date
                    }
                }

                //Set station ID, waiting to be selected when station header is filled with records
                foreach (FieldNotes stations in _reportDetailedStation)
                {
                    if (stations.station.StationAlias == userSelectedStationID)
                    {
                        _reportStationIndex = _reportDetailedStation.IndexOf(stations);

                        RaisePropertyChanged("ReportStationListIndex");
                    }
                }

            }
            else
            {
                //Reset selection to last item
                 _reportStationIndex = _reportDetailedStation.Count - 1;

                RaisePropertyChanged("ReportStationListIndex");

            }
        }

        #endregion

    }
}
