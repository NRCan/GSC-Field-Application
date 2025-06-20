using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GSCFieldApp.Models;
using GSCFieldApp.Services;
using GSCFieldApp.Services.DatabaseServices;
using GSCFieldApp.Views;
using Mapsui.UI.Maui;
using SQLite;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Xml.Linq;
using static GSCFieldApp.Dictionaries.DatabaseLiterals;
using static GSCFieldApp.Services.ObservableCollectionHelper;

namespace GSCFieldApp.ViewModel
{
    [QueryProperty("UpdateTable", "UpdateTable")]
    [QueryProperty("UpdateTableID", "UpdateTableID")]
    public partial class FieldNotesViewModel : ObservableObject
    {
        //Localization
        public LocalizationResourceManager LocalizationResourceManager
            => LocalizationResourceManager.Instance; // Will be used for in code dynamic local strings

        //Database
        DataAccess da = new DataAccess();

        //Project type
        private FieldThemes _fieldThemes = new FieldThemes();

        //Error management
        DateTime lastTimeTableUpdate = DateTime.MinValue; //For some reason observation property UpdateTable is triggered twice or thrice

        //Records
        private Dictionary<TableNames, ObservableCollection<FieldNote>> FieldNotes = new Dictionary<TableNames, ObservableCollection<FieldNote>>(); //Used to populate records in each headers
        private Dictionary<TableNames, ObservableCollection<FieldNote>> FieldNotesAll = new Dictionary<TableNames, ObservableCollection<FieldNote>>(); //Safe variable for refiltering datasets

        #region PROPERTIES

        /// <summary>
        /// Will track request for table update when navigating to field notes
        /// </summary>
        [ObservableProperty]
        private TableNames _updateTable;

        [ObservableProperty]
        private string _updateTableID;

        /// <summary>
        /// Need to use a random id else updating twice a given table records, won't trigger
        /// </summary>
        /// <param name="value"></param>
        partial void OnUpdateTableIDChanged(string value)
        {
            UpdateRecordList(_updateTable);
        }

        private bool _isStationExpanded = true;
        public bool IsStationExpanded
        {
            get { return Preferences.Get(nameof(IsStationExpanded), true); }
            set { Preferences.Set(nameof(IsStationExpanded), value); }
        }

        private bool _isEarthMatExpanded = false;
        public bool IsEarthMatExpanded
        {
            get { return Preferences.Get(nameof(IsEarthMatExpanded), false); }
            set { Preferences.Set(nameof(IsEarthMatExpanded), value); }
        }

        private bool _isSampleExpanded = false;
        public bool IsSampleExpanded
        {
            get { return Preferences.Get(nameof(IsSampleExpanded), false); }
            set { Preferences.Set(nameof(IsSampleExpanded), value); }
        }

        private bool _isDocumentExpanded = false;
        public bool IsDocumentExpanded
        {
            get { return Preferences.Get(nameof(IsDocumentExpanded), false); }
            set { Preferences.Set(nameof(IsDocumentExpanded), value); }
        }

        private bool _isStructureExpanded = false;
        public bool IsStructureExpanded
        {
            get { return Preferences.Get(nameof(IsStructureExpanded), false); }
            set { Preferences.Set(nameof(IsStructureExpanded), value); }
        }

        private bool _isPaleoflowExpanded = false;
        public bool IsPaleoflowExpanded
        {
            get { return Preferences.Get(nameof(IsPaleoflowExpanded), false); }
            set { Preferences.Set(nameof(IsPaleoflowExpanded), value); }
        }

        private bool _isFossilExpanded = false;
        public bool IsFossilExpanded
        {
            get { return Preferences.Get(nameof(IsFossilExpanded), false); }
            set { Preferences.Set(nameof(IsFossilExpanded), value); }
        }

        private bool _isEnvironmentExpanded = false;
        public bool IsEnvironmentExpanded
        {
            get { return Preferences.Get(nameof(IsEnvironmentExpanded), false); }
            set { Preferences.Set(nameof(IsEnvironmentExpanded), value); }
        }

        private bool _isMineralExpanded = false;
        public bool IsMineralExpanded
        {
            get { return Preferences.Get(nameof(IsMineralExpanded), false); }
            set { Preferences.Set(nameof(IsMineralExpanded), value); }
        }

        private bool _isMineralizationExpanded = false;
        public bool IsMineralizationExpanded
        {
            get { return Preferences.Get(nameof(IsMineralizationExpanded), false); }
            set { Preferences.Set(nameof(IsMineralizationExpanded), value); }
        }

        private bool _isLocationExpanded = false;
        public bool IsLocationExpanded
        {
            get { return Preferences.Get(nameof(IsLocationExpanded), false); }
            set { Preferences.Set(nameof(IsLocationExpanded), value); }
        }

        private bool _isDrillHoleExpanded = false;
        public bool IsDrillHoleExpanded
        {
            get { return Preferences.Get(nameof(IsDrillHoleExpanded), false); }
            set { Preferences.Set(nameof(IsDrillHoleExpanded), value); }
        }

        private bool _isLineworkExpanded = false;
        public bool IsLineworkExpanded
        {
            get { return Preferences.Get(nameof(IsLineworkExpanded), false); }
            set { Preferences.Set(nameof(IsLineworkExpanded), value); }
        }

        public bool EarthMaterialVisible
        {
            get { return Preferences.Get(nameof(EarthMaterialVisible), true); }
            set { }
        }

        public bool SampleVisible
        {
            get { return Preferences.Get(nameof(SampleVisible), true); }
            set { }
        }

        public bool FossilVisible
        {
            get { return Preferences.Get(nameof(FossilVisible), true); }
            set { }
        }

        public bool DocumentVisible
        {
            get { return Preferences.Get(nameof(DocumentVisible), true); }
            set { }
        }

        public bool PaleoflowVisible
        {
            get { return Preferences.Get(nameof(PaleoflowVisible), true); }
            set { }
        }

        public bool EnvironmentVisible
        {
            get { return Preferences.Get(nameof(EnvironmentVisible), true); }
            set { }
        }

        public bool StructureVisible
        {
            get { return Preferences.Get(nameof(StructureVisible), true); }
            set { }
        }

        public bool MineralVisible
        {
            get { return Preferences.Get(nameof(MineralVisible), true); }
            set { }
        }

        public bool MineralizationVisible
        {
            get { return Preferences.Get(nameof(MineralizationVisible), true); }
            set { }
        }

        public bool DrillHoleVisible
        {
            get { return Preferences.Get(nameof(DrillHoleVisible), true); }
            set { }
        }

        public bool LocationVisible
        {
            get { return Preferences.Get(nameof(LocationVisible), true); }
            set { }
        }

        public bool LineworkVisible
        {
            get { return Preferences.Get(nameof(LineworkVisible), true); }
            set { }
        }

        private ObservableCollection<FieldNote> _earthmats = new ObservableCollection<FieldNote>();
        public ObservableCollection<FieldNote> EarthMats
        { 

            get
            {
                if (FieldNotes.ContainsKey(TableNames.earthmat))
                {
                    return _earthmats = FieldNotes[TableNames.earthmat];
                }
                else
                {
                    return _earthmats = new ObservableCollection<FieldNote>();
                }

            }
            set { _earthmats = value; }
        }

        private ObservableCollection<FieldNote> _samples = new ObservableCollection<FieldNote>();
        public ObservableCollection<FieldNote> Samples
        {

            get
            {
                if (FieldNotes.ContainsKey(TableNames.sample))
                {
                    return _samples = FieldNotes[TableNames.sample];
                }
                else
                {
                    return _samples = new ObservableCollection<FieldNote>();
                }

            }
            set { _samples = value; }
        }

        private ObservableCollection<FieldNote> _stations = new ObservableCollection<FieldNote>();
        public ObservableCollection<FieldNote> Stations
        {
            get
            {
                if (FieldNotes.ContainsKey(TableNames.station))
                {
                    return _stations = FieldNotes[TableNames.station] ;
                    }
                else
                {
                    return _stations = new ObservableCollection<FieldNote>();
                }

            }
            set { _stations = value; }
        }

        private ObservableCollection<FieldNote> _documents = new ObservableCollection<FieldNote>();
        public ObservableCollection<FieldNote> Documents
        {

            get
            {
                if (FieldNotes.ContainsKey(TableNames.document))
                {
                    return _documents = FieldNotes[TableNames.document];
                }
                else
                {
                    return _documents = new ObservableCollection<FieldNote>();
                }

            }
            set { _documents = value; }
        }

        private ObservableCollection<FieldNote> _structures = new ObservableCollection<FieldNote>();
        public ObservableCollection<FieldNote> Structures
        {

            get
            {
                if (FieldNotes.ContainsKey(TableNames.structure))
                {
                    return _structures = FieldNotes[TableNames.structure];
                }
                else
                {
                    return _structures = new ObservableCollection<FieldNote>();
                }

            }
            set { _structures = value; }
        }

        private ObservableCollection<FieldNote> _paleoflows = new ObservableCollection<FieldNote>();
        public ObservableCollection<FieldNote> Paleoflows
        {

            get
            {
                if (FieldNotes.ContainsKey(TableNames.pflow))
                {
                    return _paleoflows = FieldNotes[TableNames.pflow];
                }
                else
                {
                    return _paleoflows = new ObservableCollection<FieldNote>();
                }

            }
            set { _paleoflows = value; }
        }

        private ObservableCollection<FieldNote> _fossils = new ObservableCollection<FieldNote>();
        public ObservableCollection<FieldNote> Fossils
        {

            get
            {
                if (FieldNotes.ContainsKey(TableNames.fossil))
                {
                    return _fossils = FieldNotes[TableNames.fossil];
                }
                else
                {
                    return _fossils = new ObservableCollection<FieldNote>();
                }

            }
            set { _fossils = value; }
        }

        private ObservableCollection<FieldNote> _environments = new ObservableCollection<FieldNote>();
        public ObservableCollection<FieldNote> Environments
        {

            get
            {
                if (FieldNotes.ContainsKey(TableNames.environment))
                {
                    return _environments = FieldNotes[TableNames.environment];
                }
                else
                {
                    return _environments = new ObservableCollection<FieldNote>();
                }

            }
            set { _environments = value; }
        }

        private ObservableCollection<FieldNote> _minerals = new ObservableCollection<FieldNote>();
        public ObservableCollection<FieldNote> Minerals
        {

            get
            {
                if (FieldNotes.ContainsKey(TableNames.mineral))
                {
                    return _minerals = FieldNotes[TableNames.mineral];
                }
                else
                {
                    return _minerals = new ObservableCollection<FieldNote>();
                }

            }
            set { _minerals = value; }
        }

        private ObservableCollection<FieldNote> _mineralizationAlterations = new ObservableCollection<FieldNote>();
        public ObservableCollection<FieldNote> MineralizationAlterations
        {

            get
            {
                if (FieldNotes.ContainsKey(TableNames.mineralization))
                {
                    return _mineralizationAlterations = FieldNotes[TableNames.mineralization];
                }
                else
                {
                    return _mineralizationAlterations = new ObservableCollection<FieldNote>();
                }

            }
            set { _mineralizationAlterations = value; }
        }

        private ObservableCollection<FieldNote> _locations = new ObservableCollection<FieldNote>();
        public ObservableCollection<FieldNote> Locations
        {

            get
            {
                if (FieldNotes.ContainsKey(TableNames.location))
                {
                    return _locations = FieldNotes[TableNames.location];
                }
                else
                {
                    return _locations = new ObservableCollection<FieldNote>();
                }

            }
            set { _locations = value; }
        }

        private ObservableCollection<FieldNote> _drillHoles = new ObservableCollection<FieldNote>();
        public ObservableCollection<FieldNote> DrillHoles
        {

            get
            {
                if (FieldNotes.ContainsKey(TableNames.drill))
                {
                    return _drillHoles = FieldNotes[TableNames.drill];
                }
                else
                {
                    return _drillHoles = new ObservableCollection<FieldNote>();
                }

            }
            set { _drillHoles = value; }
        }

        private ObservableCollection<FieldNote> _lineworks = new ObservableCollection<FieldNote>();
        public ObservableCollection<FieldNote> Lineworks
        {

            get
            {
                if (FieldNotes.ContainsKey(TableNames.linework))
                {
                    return _lineworks = FieldNotes[TableNames.linework];
                }
                else
                {
                    return _lineworks = new ObservableCollection<FieldNote>();
                }

            }
            set { _lineworks = value; }
        }

        private string _selectedDate = string.Empty;
        public string SelectedDate { get { return _selectedDate; } set { _selectedDate = value; } }

        private ObservableCollection<string> _dates = new ObservableCollection<string>();
        public ObservableCollection<string> Dates
        {
            get { return _dates; }
            set { _dates = value; }
        }

        private bool _isWaiting = false;
        public bool IsWaiting { get { return _isWaiting; }  set { _isWaiting = value; } }

        #endregion

        public FieldNotesViewModel()
        {
            //Init notes
            FieldNotes.Add(TableNames.station, new ObservableCollection<FieldNote>());
            FieldNotes.Add(TableNames.earthmat, new ObservableCollection<FieldNote>());
            FieldNotes.Add(TableNames.sample, new ObservableCollection<FieldNote>());
            FieldNotes.Add(TableNames.document, new ObservableCollection<FieldNote>());
            FieldNotes.Add(TableNames.structure, new ObservableCollection<FieldNote>());
            FieldNotes.Add(TableNames.pflow, new ObservableCollection<FieldNote>());
            FieldNotes.Add(TableNames.fossil, new ObservableCollection<FieldNote>());
            FieldNotes.Add(TableNames.environment, new ObservableCollection<FieldNote>());
            FieldNotes.Add(TableNames.mineral, new ObservableCollection<FieldNote>());
            FieldNotes.Add(TableNames.mineralization, new ObservableCollection<FieldNote>());
            FieldNotes.Add(TableNames.location, new ObservableCollection<FieldNote>());
            FieldNotes.Add(TableNames.drill, new ObservableCollection<FieldNote>());
            FieldNotes.Add(TableNames.linework, new ObservableCollection<FieldNote>());

            _dates = new ObservableCollection<string>();

            //Init all records
            _ = Task.Run(async () => await ValidateFillFieldNotesAsync());

            //Detect new field book selection, uprgrade, edit, ...
            FieldBooksViewModel.newFieldBookSelected += FieldBooksViewModel_newFieldBookSelectedAsync;
            FieldAppPageHelper.newRecord += FieldAppPageHelper_newRecordAsync;  
            FieldAppPageHelper.updateRecord += FieldAppPageHelper_updateRecordAsync;
            FieldAppPageHelper.deleteRecord += FieldAppPageHelper_deleteRecordAsync;
            MapViewModel.newMapRecord += FieldAppPageHelper_newRecordAsync;
        }

        #region RELAY

        /// <summary>
        /// Will filter down all records based on user selected date
        /// </summary>
        /// <param name="incomingDate"></param>
        /// <returns></returns>
        [RelayCommand]
        async Task TapDateGestureRecognizer(string incomingDate)
        {
            await FilterRecordsOnDate(incomingDate);
        }

        /// <summary>
        /// Will reverse whatever is set has visibility on the records after a tap on header
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        async Task Hide(string inComingName)
        {
            //Important note, tapping the header of dates will refill ALL records, kind of a refresh
            if (inComingName != null && inComingName != string.Empty)
            {
                if (inComingName.ToLower().Contains(KeywordStation))
                {
                    IsStationExpanded = !IsStationExpanded ; 
                    OnPropertyChanged(nameof(IsStationExpanded ));
                }

                if (inComingName.ToLower().Contains(KeywordEarthmat))
                {
                    IsEarthMatExpanded = !IsEarthMatExpanded;
                    OnPropertyChanged(nameof(IsEarthMatExpanded));
                }

                if (inComingName.ToLower().Contains(KeywordSample))
                {
                    IsSampleExpanded = !IsSampleExpanded;
                    OnPropertyChanged(nameof(IsSampleExpanded));
                }

                if (inComingName.ToLower().Contains(nameof(TableNames.document)))
                {
                    IsDocumentExpanded = !IsDocumentExpanded;
                    OnPropertyChanged(nameof(IsDocumentExpanded));
                }

                if (inComingName.ToLower().Contains(nameof(TableNames.structure)))
                {
                    IsStructureExpanded = !IsStructureExpanded;
                    OnPropertyChanged(nameof(IsStructureExpanded));
                }

                if (inComingName.ToLower().Contains(nameof(TableNames.pflow)))
                {
                    IsPaleoflowExpanded = !IsPaleoflowExpanded;
                    OnPropertyChanged(nameof(IsPaleoflowExpanded));
                }

                if (inComingName.ToLower().Contains(nameof(TableNames.fossil)))
                {
                    IsFossilExpanded = !IsFossilExpanded;
                    OnPropertyChanged(nameof(IsFossilExpanded));
                }

                if (inComingName.ToLower().Contains(nameof(TableNames.environment)))
                {
                    IsEnvironmentExpanded = !IsEnvironmentExpanded;
                    OnPropertyChanged(nameof(IsEnvironmentExpanded));
                }


                if (inComingName.ToLower() == (nameof(TableNames.mineral)))
                {
                    IsMineralExpanded = !IsMineralExpanded;
                    OnPropertyChanged(nameof(IsMineralExpanded));
                }

                if (inComingName.ToLower() == (nameof(TableNames.mineralization)))
                {
                    IsMineralizationExpanded = !IsMineralizationExpanded;
                    OnPropertyChanged(nameof(IsMineralizationExpanded));
                }

                if (inComingName.ToLower() == (nameof(TableNames.location)))
                {
                    IsLocationExpanded = !IsLocationExpanded;
                    OnPropertyChanged(nameof(IsLocationExpanded));
                }

                if (inComingName.ToLower() == (nameof(TableNames.drill)))
                {
                    IsDrillHoleExpanded = !IsDrillHoleExpanded;
                    OnPropertyChanged(nameof(IsDrillHoleExpanded));
                }

                if (inComingName.ToLower() == (nameof(TableNames.linework)))
                {
                    IsLineworkExpanded = !IsLineworkExpanded;
                    OnPropertyChanged(nameof(IsLineworkExpanded));
                }

                //Special case, removing filtering on date and refreshing all records.
                if (inComingName.ToLower().Contains(KeywordDates))
                {
                    //Force refresh of all
                    await ValidateFillFieldNotesAsync(true);
                }

            }

        }

        /// <summary>
        /// Will open an edit form of tapped field note card/item
        /// </summary>
        /// <param name="fieldNotes"></param>
        /// <returns></returns>
        [RelayCommand]
        public async Task TapGestureRecognizer(FieldNote fieldNotes)
        {
            //Get tapped record
            if (fieldNotes.GenericTableName == TableStation)
            {
                List<Station> tappedStation = await DataAccess.DbConnection.Table<Station>().Where(i => i.StationID == fieldNotes.GenericID).ToListAsync();
                //Navigate to station page and keep locationmodel for relationnal link
                if (tappedStation != null && tappedStation.Count() == 1)
                {
                    await Shell.Current.GoToAsync($"/{nameof(StationPage)}/",
                        new Dictionary<string, object>
                        {
                            [nameof(FieldLocation)] = null,
                            [nameof(Metadata)] = null,
                            [nameof(Station)] = tappedStation[0]
                        }
                    );
                }
            }

            if (fieldNotes.GenericTableName == TableEarthMat)
            {
                List<Earthmaterial> tappedEM = await DataAccess.DbConnection.Table<Earthmaterial>().Where(i => i.EarthMatID == fieldNotes.GenericID).ToListAsync();

                //Navigate to station page and keep locationmodel for relationnal link
                if (tappedEM != null && tappedEM.Count() == 1)
                {
                    await Shell.Current.GoToAsync($"/{nameof(EarthmatPage)}/",
                        new Dictionary<string, object>
                        {
                            [nameof(Earthmaterial)] = (Earthmaterial)tappedEM[0],
                            [nameof(Station)] = null,
                        }
                    );
                }
            }

            if (fieldNotes.GenericTableName == TableSample)
            {
                List<Sample> tappedSample = await DataAccess.DbConnection.Table<Sample>().Where(i => i.SampleID == fieldNotes.GenericID).ToListAsync();

                //Navigate to station page and keep locationmodel for relationnal link
                if (tappedSample != null && tappedSample.Count() == 1)
                {
                    await Shell.Current.GoToAsync($"/{nameof(SamplePage)}/",
                        new Dictionary<string, object>
                        {
                            [nameof(Sample)] = (Sample)tappedSample[0],
                            [nameof(Earthmaterial)] = null,
                        }
                    );
                }
            }

            if (fieldNotes.GenericTableName == TableStructure)
            {
                List<Structure> tappedStructure = await DataAccess.DbConnection.Table<Structure>().Where(i => i.StructureID == fieldNotes.GenericID).ToListAsync();

                //Navigate to station page and keep locationmodel for relationnal link
                if (tappedStructure != null && tappedStructure.Count() == 1)
                {
                    await Shell.Current.GoToAsync($"/{nameof(StructurePage)}/",
                        new Dictionary<string, object>
                        {
                            [nameof(Structure)] = (Structure)tappedStructure[0],
                            [nameof(Earthmaterial)] = null,
                        }
                    );
                }
            }

            if (fieldNotes.GenericTableName == TablePFlow)
            {
                List<Paleoflow> tappedPflow = await DataAccess.DbConnection.Table<Paleoflow>().Where(i => i.PFlowID == fieldNotes.GenericID).ToListAsync();

                //Navigate to station page and keep locationmodel for relationnal link
                if (tappedPflow != null && tappedPflow.Count() == 1)
                {
                    await Shell.Current.GoToAsync($"/{nameof(PaleoflowPage)}/",
                        new Dictionary<string, object>
                        {
                            [nameof(Paleoflow)] = (Paleoflow)tappedPflow[0],
                            [nameof(Earthmaterial)] = null,
                        }
                    );
                }
            }

            if (fieldNotes.GenericTableName == TableFossil)
            {
                List<Fossil> tappedFossil= await DataAccess.DbConnection.Table<Fossil>().Where(i => i.FossilID == fieldNotes.GenericID).ToListAsync();

                //Navigate to station page and keep locationmodel for relationnal link
                if (tappedFossil != null && tappedFossil.Count() == 1)
                {
                    await Shell.Current.GoToAsync($"/{nameof(FossilPage)}/",
                        new Dictionary<string, object>
                        {
                            [nameof(Fossil)] = (Fossil)tappedFossil[0],
                            [nameof(Earthmaterial)] = null,
                        }
                    );
                }
            }

            if (fieldNotes.GenericTableName == TableEnvironment)
            {
                List<EnvironmentModel> tappedEnv = await DataAccess.DbConnection.Table<EnvironmentModel>().Where(i => i.EnvID == fieldNotes.GenericID).ToListAsync();

                //Navigate to station page and keep locationmodel for relationnal link
                if (tappedEnv != null && tappedEnv.Count() == 1)
                {
                    await Shell.Current.GoToAsync($"/{nameof(EnvironmentPage)}/",
                        new Dictionary<string, object>
                        {
                            [nameof(EnvironmentModel)] = (EnvironmentModel)tappedEnv[0],
                            [nameof(Station)] = null,
                        }
                    );
                }
            }

            if (fieldNotes.GenericTableName == TableDocument)
            {
                List<Document> tappedDoc = await DataAccess.DbConnection.Table<Document>().Where(i => i.DocumentID == fieldNotes.GenericID).ToListAsync();

                //Navigate to station page and keep locationmodel for relationnal link
                if (tappedDoc != null && tappedDoc.Count() == 1)
                {
                    await Shell.Current.GoToAsync($"/{nameof(DocumentPage)}/",
                        new Dictionary<string, object>
                        {
                            [nameof(Station)] = null,
                            [nameof(Document)] = (Document)tappedDoc[0],
                        }
                    );
                }
            }

            if (fieldNotes.GenericTableName == TableMineral)
            {
                List<Mineral> tappedMineral = await DataAccess.DbConnection.Table<Mineral>().Where(i => i.MineralID == fieldNotes.GenericID).ToListAsync();

                //Navigate to station page and keep locationmodel for relationnal link
                if (tappedMineral != null && tappedMineral.Count() == 1)
                {
                    await Shell.Current.GoToAsync($"/{nameof(MineralPage)}/",
                        new Dictionary<string, object>
                        {
                            [nameof(Mineral)] = (Mineral)tappedMineral[0],
                            [nameof(Earthmaterial)] = null,
                            [nameof(MineralAlteration)] = null,
                        }
                    );
                }
            }

            if (fieldNotes.GenericTableName == TableMineralAlteration)
            {
                List<MineralAlteration> tappedMineralization = await DataAccess.DbConnection.Table<MineralAlteration>().Where(i => i.MAID == fieldNotes.GenericID).ToListAsync();

                //Navigate to station page and keep locationmodel for relationnal link
                if (tappedMineralization != null && tappedMineralization.Count() == 1)
                {
                    await Shell.Current.GoToAsync($"/{nameof(MineralizationAlterationPage)}/",
                        new Dictionary<string, object>
                        {
                            [nameof(MineralAlteration)] = (MineralAlteration)tappedMineralization[0],
                            [nameof(Earthmaterial)] = null,
                            [nameof(Station)] = null,
                        }
                    );
                }
            }

            if (fieldNotes.GenericTableName == TableLocation)
            {
                List<FieldLocation> tappedLocation = await DataAccess.DbConnection.Table<FieldLocation>().Where(i => i.LocationID == fieldNotes.GenericID).ToListAsync();

                //Navigate to station page and keep locationmodel for relationnal link
                if (tappedLocation != null && tappedLocation.Count() == 1)
                {
                    await Shell.Current.GoToAsync($"/{nameof(LocationPage)}/",
                        new Dictionary<string, object>
                        {
                            [nameof(FieldLocation)] = (FieldLocation)tappedLocation[0],
                        }
                    );
                }
            }

            if (fieldNotes.GenericTableName == TableDrillHoles)
            {
                List<DrillHole> tappedDrill= await DataAccess.DbConnection.Table<DrillHole>().Where(i => i.DrillID == fieldNotes.GenericID).ToListAsync();

                //Navigate to station page and keep locationmodel for relationnal link
                if (tappedDrill != null && tappedDrill.Count() == 1)
                {
                    await Shell.Current.GoToAsync($"/{nameof(DrillHolePage)}/",
                        new Dictionary<string, object>
                        {
                            [nameof(DrillHole)] = (DrillHole)tappedDrill[0],
                            [nameof(FieldLocation)] = null,
                        }
                    );
                }
            }

            if (fieldNotes.GenericTableName == TableLinework)
            {
                List<Linework> tappedLine = await DataAccess.DbConnection.Table<Linework>().Where(i => i.LineID == fieldNotes.GenericID).ToListAsync();

                //Navigate to linework page
                if (tappedLine != null && tappedLine.Count() == 1)
                {
                    await Shell.Current.GoToAsync($"/{nameof(LineworkPage)}/",
                        new Dictionary<string, object>
                        {
                            [nameof(Linework)] = (Linework)tappedLine[0],
                        }
                    );
                }
            }
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Will check if a full refresh of the whole page is necessary and 
        /// will initiate a fill of all records in the database
        /// </summary>
        /// <param name="enforceValidation">Will enforce or not validateion, setting to false will force a full refill of all tables</param>
        /// <returns></returns>
        public async Task ValidateFillFieldNotesAsync(bool forceRefresh = false)
        {
            try
            {
                if (da.PreferedDatabasePath != null && da.PreferedDatabasePath != string.Empty)
                {

                    //Check if number of location has changed, or if last location is different from last record
                    bool check1 = false;
                    bool check2 = false;
                    bool check3 = false;
                    bool check4 = forceRefresh; //Code needs to force a refresh
                    bool check5 = FieldAppPageHelper.NavFromMapPage; //Check if we are coming from map page, if so, force a refresh

                    //Check #1 - If no records in location, then refill all
                    if (FieldNotes[TableNames.location].Count() == 0)
                    {
                        check1 = true;
                    }

                    ////Check #2 - If location record count is different, then refill all
                    //List<double> countLocation = await DataAccess.DbConnection.QueryScalarsAsync<double>(string.Format("SELECT count({0}) FROM {1}", FieldLocationID, TableLocation));
                    //await da.CloseConnectionAsync();
                    //if (LocationVisible && countLocation != null && countLocation.Count() == 1 && countLocation[0].ToString() != FieldNotes[TableNames.location].Count().ToString())
                    //{
                    //    check2 = true;
                    //}

                    ////Check #3 - If last location is different from last record, then refill all
                    //List<double> lastLocation = await DataAccess.DbConnection.QueryScalarsAsync<double>(string.Format("SELECT max({0}) FROM {1} limit 1", FieldLocationID, TableLocation));
                    //await da.CloseConnectionAsync();
                    //if (LocationVisible && lastLocation != null && lastLocation.Count() == 1 && FieldNotes[TableNames.location].Count() > 0 && lastLocation[0].ToString() != FieldNotes[TableNames.location].Last().GenericID.ToString())
                    //{
                    //    check3 = true;
                    //}


                    //Check #5 - When updating the field notes from elsewhere then field note page, when navigating back to it for some reasons
                    // and under Android only, it duplicates every record that was seen at least once. Example, user opens app, navigates to field notes page,
                    // then navigates to map page, then back to field notes page, it will duplicate all records that were seen at least once.

                    if (check1 || check2 || check3 || check4 || check5)
                    {
                        //Refill all
                        await FillFieldNotesAsync(DataAccess.DbConnection);
                    }

                    await FillTraverseDates(DataAccess.DbConnection);
                }

            }
            catch (Exception fieldNoteFillException)
            {
                new ErrorToLogFile(fieldNoteFillException).WriteToFile();
            }

        }

        /// <summary>
        /// First run on filling all the notes from the database
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public async Task FillFieldNotesAsync(SQLiteAsyncConnection connection)
        {
            List<Task> tasks = new List<Task>();
            tasks.Add(FillTraverseDates(connection));
            tasks.Add(FillStationNotes(connection));
            tasks.Add(FillEMNotes(connection));
            tasks.Add(FillSampleNotes(connection));
            tasks.Add(FillDocumentNotes(connection));
            tasks.Add(FillStructureNotes(connection));
            tasks.Add(FillPaleoflowNotes(connection));
            tasks.Add(FillFossilNotes(connection));
            tasks.Add(FillEnvironmentNotes(connection));
            tasks.Add(FillMineralNotes(connection));
            tasks.Add(FillMineralizationAlterationNotes(connection));
            tasks.Add(FillLocationNotes(connection));
            tasks.Add(FillDrillHoleNotes(connection));
            tasks.Add(FillLineworkNotes(connection));

            await Task.WhenAll(tasks).ConfigureAwait(false);

            //OnPropertyChanged(nameof(FieldNotes));
            OnPropertyChanged(nameof(Dates));

            //Make a copy in case user wants to refilter values
            FieldNotesAll = new Dictionary<TableNames, ObservableCollection<FieldNote>>(FieldNotes);

            //Force a first select or refresh selected date or force last date if no selection
            await DateRefreshner();
        }

        /// <summary>
        /// Will refresh the date selection and collection
        /// </summary>
        public async Task DateRefreshner()
        {
            if ((Dates != null && Dates.Count == 1) || (_selectedDate != null && _selectedDate == string.Empty))
            {
                await FilterRecordsOnDate(Dates.First());

                _selectedDate = Dates.First();
                OnPropertyChanged(nameof(SelectedDate));
            }
            else if (_selectedDate != null && Dates.Contains(_selectedDate))
            {
                await FilterRecordsOnDate(_selectedDate);
            }
        }

        /// <summary>
        /// Will fill the traverse date section of the notes
        /// </summary>
        /// <param name="inConnection"></param>
        /// <returns></returns>
        public async Task FillTraverseDates(SQLiteAsyncConnection inConnection)
        {

            //Clear whatever was in there first.
            //_dates.Clear();

            //List to detect dates that disapears
            List<string> datesToRemove = new List<string>();
            List<string> datesTotalList = new List<string>();

            //Get all dates from key TableNames
            List<Station> stats = await inConnection.QueryAsync<Station>(string.Format("select distinct({0}) from {1} order by {0} desc", 
                FieldStationVisitDate, TableStation));
            List<FieldLocation> locs = new List<FieldLocation>();

            try
            {
                locs = await inConnection.QueryAsync<FieldLocation>(string.Format("select distinct(SUBSTRING({0}, 1, 10)) as {0} from {1} order by {0} desc",
                FieldLocationTimestamp, TableLocation));

            }
            catch (Exception e)
            {
                new ErrorToLogFile(e).WriteToFile();
            }


            //Get all dates from database
            if (stats != null && stats.Count > 0)
            {
                foreach (Station st in stats)
                {
                    string sDate = LocalizationResourceManager["FieldNotesEmptyDate"].ToString();

                    if (st.StationVisitDate != null && st.StationVisitDate != string.Empty)
                    {
                        sDate = st.StationVisitDate;
                        datesTotalList.Add(sDate);
                    }

                    if (!_dates.Contains(sDate))
                    {
                        _dates.Add(sDate);
                    }

                }
            }

            if (locs != null && locs.Count > 0)
            {
                foreach (FieldLocation dr in locs)
                {

                    string dDate = LocalizationResourceManager["FieldNotesEmptyDate"].ToString();

                    if (dr.LocationTimestamp != null && dr.LocationTimestamp != string.Empty)
                    {
                        dDate = dr.LocationTimestamp;
                        datesTotalList.Add(dDate);
                    }

                    if (!_dates.Contains(dDate))
                    {
                        _dates.Add(dDate);
                    }

                }
            }

            //Remove missing dates
            datesToRemove = _dates.Where(d => !datesTotalList.Contains(d)).ToList();
            if (datesToRemove != null && datesToRemove.Count() > 0)
            {
                foreach (string dr in datesToRemove)
                {
                    _dates.Remove(dr);
                }

            }

            OnPropertyChanged(nameof(Dates));
        }

        /// <summary>
        /// Will get all database stations to fill station cards
        /// </summary>
        /// <param name="inConnection"></param>
        /// <returns></returns>
        public async Task FillStationNotes(SQLiteAsyncConnection inConnection)
        {

            //Init a station group
            if (!FieldNotes.ContainsKey(TableNames.station))
            {
                FieldNotes.Add(TableNames.station, new ObservableCollection<FieldNote>());
            }
            else
            {
                //Clear whatever was in there first.
                FieldNotes[TableNames.station].Clear();
                OnPropertyChanged(nameof(FieldNotes));

            }

            //Get all stations from database
            List<Station> stations = await inConnection.Table<Station>().OrderBy(s => s.StationAlias).ToListAsync();
            ObservableCollection<FieldNote> stationsFN = new ObservableCollection<FieldNote>();
            if (stations != null && stations.Count > 0)
            {
                
                foreach (Station st in stations)
                {
                    stationsFN.Add(GetStatFieldNote(st));
                }

            }
            FieldNotes[TableNames.station] = stationsFN;
            OnPropertyChanged(nameof(Stations)); 

        }

        /// <summary>
        /// Will get all database earthats to fill earth mat cards
        /// </summary>
        /// <param name="inConnection"></param>
        /// <returns></returns>
        public async Task FillEMNotes(SQLiteAsyncConnection inConnection)
        {
            //Init a station group
            if (!FieldNotes.ContainsKey(TableNames.earthmat))
            {
                FieldNotes.Add(TableNames.earthmat, new ObservableCollection<FieldNote>());
            }
            else
            {
                //Clear whatever was in there first.
                FieldNotes[TableNames.earthmat].Clear();
                OnPropertyChanged(nameof(EarthMats));
            }

            if (EarthMaterialVisible)
            {
                //Get all em from database
                List<Earthmaterial> ems = await inConnection.Table<Earthmaterial>().OrderBy(x => x.EarthMatName).ToListAsync();
                ObservableCollection<FieldNote> emsFN = new ObservableCollection<FieldNote>();
                if (ems != null && ems.Count > 0)
                {
                    foreach (Earthmaterial em in ems)
                    {
                        emsFN.Add(GetEMFieldNote(em));
                    }

                }
                FieldNotes[TableNames.earthmat] = emsFN;
            }

            OnPropertyChanged(nameof(EarthMats));

        }

        /// <summary>
        /// Will get all database samples to fill sample cards
        /// </summary>
        /// <param name="inConnection"></param>
        /// <returns></returns>
        public async Task FillSampleNotes(SQLiteAsyncConnection inConnection)
        {
            //Init a station group
            if (!FieldNotes.ContainsKey(TableNames.sample))
            {
                FieldNotes.Add(TableNames.sample, new ObservableCollection<FieldNote>());
            }
            else
            {
                //Clear whatever was in there first.
                FieldNotes[TableNames.sample].Clear();
                OnPropertyChanged(nameof(Samples));

            }

            if (SampleVisible)
            {
                //Get all stations from database
                List<Sample> samples = await inConnection.Table<Sample>().OrderBy(s => s.SampleName).ToListAsync();
                ObservableCollection<FieldNote> samplesFN = new ObservableCollection<FieldNote>();
                if (samples != null && samples.Count > 0)
                {

                    foreach (Sample sam in samples)
                    {
                        samplesFN.Add(GetSampleFieldNote(sam));
                    }

                }
                FieldNotes[TableNames.sample] = samplesFN;
            }

            OnPropertyChanged(nameof(Samples));

        }

        /// <summary>
        /// Will get all database samples to fill sample cards
        /// </summary>
        /// <param name="inConnection"></param>
        /// <returns></returns>
        public async Task FillDocumentNotes(SQLiteAsyncConnection inConnection)
        {
            //Init a station group
            if (!FieldNotes.ContainsKey(TableNames.document))
            {
                FieldNotes.Add(TableNames.document, new ObservableCollection<FieldNote>());
            }
            else
            {
                //Clear whatever was in there first.
                FieldNotes[TableNames.document].Clear();
                OnPropertyChanged(nameof(Documents));

            }

            if (DocumentVisible)
            {
                //Get all documents from database
                List<Document> docs = await inConnection.Table<Document>().OrderBy(s => s.DocumentName).ToListAsync();
                ObservableCollection<FieldNote> docsFN = new ObservableCollection<FieldNote>();
                if (docs != null && docs.Count > 0)
                {
                    foreach (Document dc in docs)
                    {
                        docsFN.Add(GetDocumentFieldNote(dc));
                    }
                }
                FieldNotes[TableNames.document] = docsFN;
            }

            OnPropertyChanged(nameof(Documents));

        }

        /// <summary>
        /// Will get all database structures to fill structures cards
        /// </summary>
        /// <param name="inConnection"></param>
        /// <returns></returns>
        public async Task FillStructureNotes(SQLiteAsyncConnection inConnection)
        {
            //Init a station group
            if (!FieldNotes.ContainsKey(TableNames.structure))
            {
                FieldNotes.Add(TableNames.structure, new ObservableCollection<FieldNote>());
            }
            else
            {
                //Clear whatever was in there first.
                FieldNotes[TableNames.structure].Clear();
                OnPropertyChanged(nameof(Structures));

            }

            if (StructureVisible)
            {
                //Get all stations from database
                List<Structure> structures = await inConnection.Table<Structure>().OrderBy(s => s.StructureName).ToListAsync();
                ObservableCollection<FieldNote> structuresFN = new ObservableCollection<FieldNote>();
                if (structures != null && structures.Count > 0)
                {

                    foreach (Structure struc in structures)
                    {
                        structuresFN.Add(GetStructureFieldNote(struc));
                    }

                }
                FieldNotes[TableNames.structure] = structuresFN;
            }

            OnPropertyChanged(nameof(Structures));

        }

        /// <summary>
        /// Will get all database pflow to fill pflow cards
        /// </summary>
        /// <param name="inConnection"></param>
        /// <returns></returns>
        public async Task FillPaleoflowNotes(SQLiteAsyncConnection inConnection)
        {
            //Init a station group
            if (!FieldNotes.ContainsKey(TableNames.pflow))
            {
                FieldNotes.Add(TableNames.pflow, new ObservableCollection<FieldNote>());
            }
            else
            {
                //Clear whatever was in there first.
                FieldNotes[TableNames.pflow].Clear();
                OnPropertyChanged(nameof(Paleoflows));

            }

            if (PaleoflowVisible)
            {
                //Get all stations from database
                List<Paleoflow> pflows = await inConnection.Table<Paleoflow>().OrderBy(s => s.PFlowName).ToListAsync();
                ObservableCollection<FieldNote> pflowsFN = new ObservableCollection<FieldNote>();
                if (pflows != null && pflows.Count > 0)
                {

                    foreach (Paleoflow pf in pflows)
                    {
                        pflowsFN.Add(GetPflowFieldNote(pf));
                    }

                }
                FieldNotes[TableNames.pflow] = pflowsFN;
            }

            OnPropertyChanged(nameof(Paleoflows));

        }

        /// <summary>
        /// Will get all database fossils
        /// </summary>
        /// <param name="inConnection"></param>
        /// <returns></returns>
        public async Task FillFossilNotes(SQLiteAsyncConnection inConnection)
        {
            //Init a station group
            if (!FieldNotes.ContainsKey(TableNames.fossil))
            {
                FieldNotes.Add(TableNames.fossil, new ObservableCollection<FieldNote>());
            }
            else
            {
                //Clear whatever was in there first.
                FieldNotes[TableNames.fossil].Clear();
                OnPropertyChanged(nameof(Fossils));

            }

            if (FossilVisible)
            {
                //Get all stations from database
                List<Fossil> fossils = await inConnection.Table<Fossil>().OrderBy(s => s.FossilIDName).ToListAsync();
                ObservableCollection<FieldNote> fossilsFN = new ObservableCollection<FieldNote>();
                if (fossils != null && fossils.Count > 0)
                {

                    foreach (Fossil fs in fossils)
                    {
                        fossilsFN.Add(GetFossilFieldNote(fs));
                    }

                }
                FieldNotes[TableNames.fossil] = fossilsFN;
            }

            OnPropertyChanged(nameof(Fossils));

        }

        /// <summary>
        /// Will get all database environment records
        /// </summary>
        /// <param name="inConnection"></param>
        /// <returns></returns>
        public async Task FillEnvironmentNotes(SQLiteAsyncConnection inConnection)
        {
            //Init a station group
            if (!FieldNotes.ContainsKey(TableNames.environment))
            {
                FieldNotes.Add(TableNames.environment, new ObservableCollection<FieldNote>());
            }
            else
            {
                //Clear whatever was in there first.
                FieldNotes[TableNames.environment].Clear();
                OnPropertyChanged(nameof(Environments));

            }

            if (EnvironmentVisible)
            {
                //Get all stations from database
                List<EnvironmentModel> environments = await inConnection.Table<EnvironmentModel>().OrderBy(s => s.EnvID).ToListAsync();
                ObservableCollection<FieldNote> environmentsFN = new ObservableCollection<FieldNote>();
                if (environments != null && environments.Count > 0)
                {

                    foreach (EnvironmentModel env in environments)
                    {
                        environmentsFN.Add(GetEnvironmentFieldNote(env));
                    }

                }
                FieldNotes[TableNames.environment] = environmentsFN;
            }

            OnPropertyChanged(nameof(Environments));

        }

        /// <summary>
        /// Will get all database minerals
        /// </summary>
        /// <param name="inConnection"></param>
        /// <returns></returns>
        public async Task FillMineralNotes(SQLiteAsyncConnection inConnection)
        {
            //Init a station group
            if (!FieldNotes.ContainsKey(TableNames.mineral))
            {
                FieldNotes.Add(TableNames.mineral, new ObservableCollection<FieldNote>());
            }
            else
            {
                //Clear whatever was in there first.
                FieldNotes[TableNames.mineral].Clear();
                OnPropertyChanged(nameof(Minerals));

            }

            if (MineralVisible)
            {
                //Get all stations from database
                List<Mineral> minerals = await inConnection.Table<Mineral>().OrderBy(s => s.MineralID).ToListAsync();
                ObservableCollection<FieldNote> mineralsFN = new ObservableCollection<FieldNote>();
                if (minerals != null && minerals.Count > 0)
                {

                    foreach (Mineral min in minerals)
                    {
                        mineralsFN.Add(GetMineralFieldNote(min));
                    }

                }
                FieldNotes[TableNames.mineral] = mineralsFN;
            }

            OnPropertyChanged(nameof(Minerals));

        }

        /// <summary>
        /// Will get all database mineralization/alteration
        /// </summary>
        /// <param name="inConnection"></param>
        /// <returns></returns>
        public async Task FillMineralizationAlterationNotes(SQLiteAsyncConnection inConnection)
        {
            //Init a station group
            if (!FieldNotes.ContainsKey(TableNames.mineralization))
            {
                FieldNotes.Add(TableNames.mineralization, new ObservableCollection<FieldNote>());
            }
            else
            {
                //Clear whatever was in there first.
                FieldNotes[TableNames.mineralization].Clear();
                OnPropertyChanged(nameof(MineralizationAlterations));

            }

            if (MineralizationVisible)
            {
                //Get all stations from database
                List<MineralAlteration> mineralizations = await inConnection.Table<MineralAlteration>().OrderBy(s => s.MAID).ToListAsync();
                ObservableCollection<FieldNote> mineralizationsFN = new ObservableCollection<FieldNote>();
                if (mineralizations != null && mineralizations.Count > 0)
                {

                    foreach (MineralAlteration malt in mineralizations)
                    {
                        mineralizationsFN.Add(GetMAFieldNote(malt));
                    }

                }
                FieldNotes[TableNames.mineralization] = mineralizationsFN;
            }

            OnPropertyChanged(nameof(MineralizationAlterations));

        }

        /// <summary>
        /// Will get all database location 
        /// </summary>
        /// <param name="inConnection"></param>
        /// <returns></returns>
        public async Task FillLocationNotes(SQLiteAsyncConnection inConnection)
        {
            //Init a station group
            if (!FieldNotes.ContainsKey(TableNames.location))
            {
                FieldNotes.Add(TableNames.location, new ObservableCollection<FieldNote>());
            }
            else
            {
                //Clear whatever was in there first.
                FieldNotes[TableNames.location].Clear();
                OnPropertyChanged(nameof(Locations));

            }

            //Get all stations from database
            List<FieldLocation> locations = await inConnection.Table<FieldLocation>().OrderBy(s => s.LocationID).ToListAsync();
            ObservableCollection<FieldNote> locationsFN = new ObservableCollection<FieldNote>();
            if (locations != null && locations.Count > 0)
            {

                foreach (FieldLocation loc in locations)
                {
                    locationsFN.Add(GetLocationFieldNote(loc));
                }

                
                FieldNotes[TableNames.location] = locationsFN;
                OnPropertyChanged(nameof(Locations));
            }

        }

        /// <summary>
        /// Will get all database drill holes 
        /// </summary>
        /// <param name="inConnection"></param>
        /// <returns></returns>
        public async Task FillDrillHoleNotes(SQLiteAsyncConnection inConnection)
        {
            //Init a station group
            if (!FieldNotes.ContainsKey(TableNames.drill))
            {
                FieldNotes.Add(TableNames.drill, new ObservableCollection<FieldNote>());
            }
            else
            {
                //Clear whatever was in there first.
                FieldNotes[TableNames.drill].Clear();
                OnPropertyChanged(nameof(DrillHoles));

            }

            if (DrillHoleVisible)
            {
                //Get all stations from database
                List<DrillHole> drills = await inConnection.Table<DrillHole>().OrderBy(s => s.DrillID).ToListAsync();
                ObservableCollection<FieldNote> drillsFN = new ObservableCollection<FieldNote>();
                if (drills != null && drills.Count > 0)
                {

                    foreach (DrillHole dr in drills)
                    {
                        drillsFN.Add(GetDrillFieldNote(dr));
                    }

                }
                FieldNotes[TableNames.drill] = drillsFN;
            }

            OnPropertyChanged(nameof(DrillHoles));

        }

        /// <summary>
        /// Will get all database stations to fill station cards
        /// </summary>
        /// <param name="inConnection"></param>
        /// <returns></returns>
        public async Task FillLineworkNotes(SQLiteAsyncConnection inConnection)
        {
            //Init a station group
            if (!FieldNotes.ContainsKey(TableNames.linework))
            {
                FieldNotes.Add(TableNames.linework, new ObservableCollection<FieldNote>());
            }
            else
            {
                //Clear whatever was in there first.
                FieldNotes[TableNames.linework].Clear();
                OnPropertyChanged(nameof(FieldNotes));

            }

            if (LineworkVisible)
            {
                //Get all stations from database
                List<Linework> lineworks = await inConnection.Table<Linework>().OrderBy(s => s.LineIDName).ToListAsync();
                ObservableCollection<FieldNote> lineworksFN = new ObservableCollection<FieldNote>();
                if (lineworks != null && lineworks.Count > 0)
                {

                    foreach (Linework ln in lineworks)
                    {
                        lineworksFN.Add(GetLineworkFieldNote(ln));
                    }

                }
                FieldNotes[TableNames.linework] = lineworksFN;
                OnPropertyChanged(nameof(Lineworks));
            }

        }

        /// <summary>
        /// A method that will filter out all records in field note page
        /// based on a desire date
        /// </summary>
        /// <param name="inDate"></param>
        /// <returns></returns>
        public async Task FilterRecordsOnDate(string inDate)
        {
            if (inDate != _selectedDate)
            {
                //Update selection on UI
                _selectedDate = inDate;
                OnPropertyChanged(nameof(SelectedDate));
            }

            if (inDate != string.Empty && _selectedDate != null)
            {
                //Clean first
                foreach (TableNames tn in FieldNotes.Keys)
                {
                    //Everything except linework that doesn't have any date
                    if (tn != TableNames.linework)
                    {
                        FieldNotes[tn] = new ObservableCollection<FieldNote>();
                    }
                    
                }

                //Start with station
                if (FieldNotesAll.ContainsKey(TableNames.station))
                {
                    //Keep stations from desired date
                    ObservableCollectionHelper.AddRange(FieldNotes[TableNames.station], FieldNotesAll[TableNames.station].Where(x => x.Date == inDate).OrderBy(x => x.GenericAliasName));
                    OnPropertyChanged(nameof(Stations));

                    //Resulting ids as a list for 
                    List<int> stationIds = new List<int>();
                    foreach (FieldNote sids in FieldNotes[TableNames.station])
                    {
                        stationIds.Add(sids.GenericID);
                    }

                    #region Station - First order children
                    ObservableCollectionHelper.AddRange(FieldNotes[TableNames.earthmat], FieldNotesAll[TableNames.earthmat].Where(x => stationIds.Contains(x.ParentID)).OrderBy(x => x.GenericAliasName));
                    OnPropertyChanged(nameof(EarthMats));

                    ObservableCollectionHelper.AddRange(FieldNotes[TableNames.document], FieldNotesAll[TableNames.document].Where(x => stationIds.Contains(x.ParentID)).OrderBy(x => x.GenericAliasName)); 
                    OnPropertyChanged(nameof(Documents));

                    ObservableCollectionHelper.AddRange(FieldNotes[TableNames.environment], FieldNotesAll[TableNames.environment].Where(x => stationIds.Contains(x.ParentID)).OrderBy(x => x.GenericAliasName)); 
                    OnPropertyChanged(nameof(Environments));

                    ObservableCollectionHelper.AddRange(FieldNotes[TableNames.mineralization], FieldNotesAll[TableNames.mineralization].Where(x => stationIds.Contains(x.ParentID)).OrderBy(x => x.GenericAliasName)); 
                    OnPropertyChanged(nameof(MineralizationAlterations));
                    #endregion

                    #region Earth Mat - Second order children

                    SetEarthMatSecondOrderChildren();

                    #endregion

                    #region Mineralization - Third order childrens

                    SetMineralizationThirdOrderChildren();

                    #endregion

                }

                //Continue with other children of location
                if (FieldNotesAll.ContainsKey(TableNames.location))
                {
                    ObservableCollectionHelper.AddRange(FieldNotes[TableNames.location], FieldNotesAll[TableNames.location].Where(x => x.Date == inDate).OrderBy(x => x.GenericAliasName).ToList());
                    OnPropertyChanged(nameof(Locations));

                    //Children
                    List<int> locIds = new List<int>();
                    foreach (FieldNote lids in FieldNotes[TableNames.location])
                    {
                        locIds.Add(lids.GenericID);
                    }

                    ObservableCollectionHelper.AddRange(FieldNotes[TableNames.drill], FieldNotesAll[TableNames.drill].Where(x => locIds.Contains(x.ParentID)).OrderBy(x => x.GenericAliasName).ToList());
                    OnPropertyChanged(nameof(DrillHoles));


                    #region Drill Holes - First order children

                    List<int> drillIds = new List<int>();
                    foreach (FieldNote ds in FieldNotes[TableNames.drill])
                    {
                        drillIds.Add(ds.GenericID);
                    }

                    ObservableCollectionHelper.AddRange(FieldNotes[TableNames.earthmat], FieldNotesAll[TableNames.earthmat].Where(x => drillIds.Contains(x.ParentID)).OrderBy(x => x.GenericAliasName));
                    OnPropertyChanged(nameof(EarthMats));

                    ObservableCollectionHelper.AddRange(FieldNotes[TableNames.document], FieldNotesAll[TableNames.document].Where(x => drillIds.Contains(x.ParentID)).OrderBy(x => x.GenericAliasName));
                    OnPropertyChanged(nameof(Documents));

                    #endregion

                    #region Earth Mat - Second order children

                    SetEarthMatSecondOrderChildren();

                    #endregion

                    #region Mineralization - Third order childrens

                    SetMineralizationThirdOrderChildren();

                    #endregion


                }

                //Finish with lineworks
                if (FieldNotesAll.ContainsKey(TableNames.linework))
                {
                    //Keep stations from desired date
                    ObservableCollectionHelper.AddRange(FieldNotes[TableNames.linework], FieldNotesAll[TableNames.linework].Where(x => x.Date == inDate).OrderBy(x => x.GenericAliasName));
                    OnPropertyChanged(nameof(Lineworks));
                }

            }

        }

        /// <summary>
        /// Will set the second order children of earth material based on earth material list
        /// </summary>
        private void SetEarthMatSecondOrderChildren()
        {
            List<int> emIds = new List<int>();

            foreach (FieldNote fn in FieldNotes[TableNames.earthmat])
            {
                emIds.Add(fn.GenericID);
            }

            ObservableCollectionHelper.AddRange(FieldNotes[TableNames.sample], FieldNotesAll[TableNames.sample].Where(x => emIds.Contains(x.ParentID)).OrderBy(x => x.GenericAliasName));
            ObservableCollectionHelper.AddRange(FieldNotes[TableNames.structure], FieldNotesAll[TableNames.structure].Where(x => emIds.Contains(x.ParentID)).OrderBy(x => x.GenericAliasName));
            ObservableCollectionHelper.AddRange(FieldNotes[TableNames.pflow], FieldNotesAll[TableNames.pflow].Where(x => emIds.Contains(x.ParentID)).OrderBy(x => x.GenericAliasName));
            ObservableCollectionHelper.AddRange(FieldNotes[TableNames.fossil], FieldNotesAll[TableNames.fossil].Where(x => emIds.Contains(x.ParentID)).OrderBy(x => x.GenericAliasName));
            ObservableCollectionHelper.AddRange(FieldNotes[TableNames.mineral], FieldNotesAll[TableNames.mineral].Where(x => emIds.Contains(x.ParentID)).OrderBy(x => x.GenericAliasName));
            ObservableCollectionHelper.AddRange(FieldNotes[TableNames.mineralization], FieldNotesAll[TableNames.mineralization].Where(x => emIds.Contains(x.ParentID)).OrderBy(x => x.GenericAliasName));

            OnPropertyChanged(nameof(Samples));
            OnPropertyChanged(nameof(Structures));
            OnPropertyChanged(nameof(Paleoflows));
            OnPropertyChanged(nameof(Fossils));
            OnPropertyChanged(nameof(Minerals));
            OnPropertyChanged(nameof(MineralizationAlterations));
        }

        /// <summary>
        /// Will set the third order children from mineralization based on min. list
        /// </summary>
        private void SetMineralizationThirdOrderChildren()
        {
            List<int> maIds = new List<int>();
            foreach (FieldNote fn in FieldNotes[TableNames.mineralization])
            {
                maIds.Add(fn.GenericID);
            }

            ObservableCollectionHelper.AddRange(FieldNotes[TableNames.mineral], FieldNotesAll[TableNames.mineral].Where(x => maIds.Contains(x.ParentID)).OrderBy(x => x.GenericAliasName).ToList());
            OnPropertyChanged(nameof(Minerals));
        }

        /// <summary>
        /// Will refresh a desire table in the field notes
        /// This should be triggered when user is coming from a form, either
        /// adding items or editing them or coming from map page.
        /// We need to make sure all parents are also refreshed
        /// </summary>
        public async void UpdateRecordList(TableNames tableToUpdate)
        {
            //Detect if 
            List<Task> tasks = new List<Task>();

            switch (tableToUpdate)
            {
                case TableNames.meta:
                    //Special case, this will trigger a whole field note page refresh
                    //Best used when a delete cascade has been done and and child should be removed from page
                    tasks.Add(ValidateFillFieldNotesAsync(true));
                    break;
                default:
                    break;
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);            

        }

        /// <summary>
        /// Simple method to hide/show whole field theme header bars, coming from setting page.
        /// </summary>
        public void ThemeHeaderBarsRefresh()
        {
            OnPropertyChanged(nameof(EarthMaterialVisible));
            OnPropertyChanged(nameof(SampleVisible));
            OnPropertyChanged(nameof(StructureVisible));
            OnPropertyChanged(nameof(DocumentVisible));
            OnPropertyChanged(nameof(PaleoflowVisible));
            OnPropertyChanged(nameof(EnvironmentVisible));
            OnPropertyChanged(nameof(FossilVisible));
            OnPropertyChanged(nameof(MineralVisible));
            OnPropertyChanged(nameof(MineralizationVisible));
            OnPropertyChanged(nameof(DrillHoleVisible));
            OnPropertyChanged(nameof(LocationVisible));
            OnPropertyChanged(nameof(LineworkVisible));
        }

        /// <summary>
        /// Will return a field note object with the information from the given station.
        /// This information will be displayed in the field note page
        /// </summary>
        /// <param name="st"></param>
        /// <returns></returns>
        public FieldNote GetStatFieldNote(Station st)
        {
            FieldNote stationsFN = new FieldNote
            {
                Display_text_1 = st.StationAliasLight,
                Display_text_2 = st.StationObsType,
                Display_text_3 = st.StationNote,
                GenericTableName = TableStation,
                GenericID = st.StationID,
                ParentID = st.LocationID,
                Date = st.StationVisitDate,
                isValid = st.isValid,
                ParentTableName = TableLocation
            };

            return stationsFN;
        }

        /// <summary>
        /// Will return a field note object with the information from the given earth material.
        /// This information will be displayed in the field note page
        /// </summary>
        /// <param name="earthmaterial"></param>
        /// <returns></returns>
        public FieldNote GetEMFieldNote(Earthmaterial earthmaterial)
        {
            int parentID = -1;
            string parentLightAlias = string.Empty;
            if (earthmaterial.EarthMatStatID != null)
            {
                parentID = (int)earthmaterial.EarthMatStatID;
            }
            if (earthmaterial.EarthMatDrillHoleID != null)
            {
                parentID = (int)earthmaterial.EarthMatDrillHoleID;
            }

            FieldNote emsFN = new FieldNote()
            {
                Display_text_1 = earthmaterial.EarthmatAliasLight,
                Display_text_2 = earthmaterial.EarthMatLithdetail,
                Display_text_3 = earthmaterial.EarthMatLithgroup,
                GenericTableName = TableEarthMat,
                GenericID = earthmaterial.EarthMatID,
                ParentID = parentID,
                isValid = earthmaterial.isValid,
                ParentTableName = earthmaterial.EarthMatDrillHoleID.HasValue ? TableDrillHoles : TableStation,

            };

            return emsFN;

        }

        /// <summary>
        /// Will return a field note object with the information from the given sample.
        /// This information will be displayed in the field note page
        /// </summary>
        /// <param name="sam"></param>
        /// <returns></returns>
        public FieldNote GetSampleFieldNote(Sample sam)
        {
            FieldNote samFN = new FieldNote
            {
                Display_text_1 = sam.SampleAliasLight,
                Display_text_2 = sam.SampleType,
                Display_text_3 = sam.SamplePurpose,
                GenericTableName = TableSample,
                GenericID = sam.SampleID,
                ParentID = sam.SampleEarthmatID,
                isValid = sam.isValid,
                ParentTableName = TableEarthMat
            };

            return samFN;
        }

        /// <summary>
        /// Will return a field note object with the information from the given document/photo.
        /// This information will be displayed in the field note page
        /// </summary>
        /// <param name="dc"></param>
        /// <returns></returns>
        public FieldNote GetDocumentFieldNote(Document dc)
        {
            int? parentID = dc.StationID;

            if (!parentID.HasValue && dc.DrillHoleID.HasValue)
            {
                parentID = dc.DrillHoleID;
            }

            FieldNote dcFN = new FieldNote
            {
                Display_text_1 = dc.DocumentAliasLight,
                Display_text_2 = dc.Category,
                Display_text_3 = dc.Description,
                GenericTableName = TableDocument,
                GenericID = dc.DocumentID,
                ParentID = parentID.HasValue ? parentID.Value : -1, // Use a default value like -1 or handle appropriately
                isValid = dc.isValid,
                ParentTableName = dc.DrillHoleID.HasValue ? TableDrillHoles : TableStation
            };

            return dcFN;
        }

        /// <summary>
        /// Will return a field note object with the information from the given structure.
        /// This information will be displayed in the field note page
        /// </summary>
        /// <param name="struc"></param>
        /// <returns></returns>
        public FieldNote GetStructureFieldNote(Structure struc)
        {

            FieldNote structureFN = new FieldNote
            {
                Display_text_1 = struc.StructureAliasLight,
                Display_text_2 = struc.StructureClass,
                Display_text_3 = struc.StructureDetail,
                GenericTableName = TableStructure,
                GenericID = struc.StructureID,
                ParentID = struc.StructureEarthmatID,
                isValid = struc.isValid,
                ParentTableName = TableEarthMat
            };

            return structureFN;
        }

        /// <summary>
        /// Will return a field note object with the information from the given paleoflow.
        /// This information will be displayed in the field note page
        /// </summary>
        /// <param name="pf"></param>
        /// <returns></returns>
        public FieldNote GetPflowFieldNote(Paleoflow pf)
        {

            FieldNote pfFN = new FieldNote
            {
                Display_text_1 = pf.PflowAliasLight,
                Display_text_2 = pf.PFlowClass,
                Display_text_3 = pf.PFlowSense,
                GenericTableName = TablePFlow,
                GenericID = pf.PFlowID,
                ParentID = pf.PFlowParentID,
                isValid = pf.isValid,
                ParentTableName = TableEarthMat
            };

            return pfFN;
        }

        /// <summary>
        /// Will return a field note object with the information from the given fossil.
        /// This information will be displayed in the field note page
        /// </summary>
        /// <param name="foss"></param>
        /// <returns></returns>
        public FieldNote GetFossilFieldNote(Fossil foss)
        {

            FieldNote fossFN = new FieldNote
            {
                Display_text_1 = foss.FossilAliasLight,
                Display_text_2 = foss.FossilType,
                Display_text_3 = foss.FossilNote,
                GenericTableName = TableFossil,
                GenericID = foss.FossilID,
                ParentID = foss.FossilParentID,
                isValid = foss.isValid,
                ParentTableName = TableEarthMat
            };

            return fossFN;
        }

        /// <summary>
        /// Will return a field note object with the information from the given environment.
        /// This information will be displayed in the field note page
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        public FieldNote GetEnvironmentFieldNote(EnvironmentModel env)
        {

            FieldNote envFN = new FieldNote
            {
                Display_text_1 = env.EnvironmentAliasLight,
                Display_text_2 = env.EnvRelief,
                Display_text_3 = env.EnvNotes,
                GenericTableName = TableEnvironment,
                GenericID = env.EnvID,
                ParentID = env.EnvStationID,
                isValid = env.isValid,
                ParentTableName = TableStation
            };

            return envFN;
        }

        /// <summary>
        /// Will return a field note object with the information from the given mienrals.
        /// This information will be displayed in the field note page
        /// </summary>
        /// <param name="min"></param>
        /// <returns></returns>
        public FieldNote GetMineralFieldNote(Mineral min)
        {
            //Find proper parent
            int? parentID = min.MineralEMID;
            if (parentID == null && min.MineralMAID != null)
            {
                parentID = min.MineralMAID;
            }

            FieldNote minFN = new FieldNote
            {
                Display_text_1 = min.MineralAliasLight,
                Display_text_2 = min.MineralName,
                Display_text_3 = min.MineralMode,
                GenericTableName = TableMineral,
                GenericID = min.MineralID,
                ParentID = parentID.Value,
                isValid = min.isValid,
                ParentTableName = min.MineralMAID.HasValue ? TableMineralAlteration : TableEarthMat
            };

            return minFN;
        }

        /// <summary>
        /// Will return a field note object with the information from the given mineralization.
        /// This information will be displayed in the field note page
        /// </summary>
        /// <param name="malt"></param>
        /// <returns></returns>
        public FieldNote GetMAFieldNote(MineralAlteration malt)
        {
            //Find proper parent
            int? parentID = malt.MAStationID;
            if (parentID == null && malt.MAEarthmatID != null)
            {
                parentID = malt.MAEarthmatID;
            }

            FieldNote malFN = new FieldNote
            {
                Display_text_1 = malt.MineralALterationAliasLight,
                Display_text_2 = malt.MAMA,
                Display_text_3 = malt.MAUnit,
                GenericTableName = TableMineralAlteration,
                GenericID = malt.MAID,
                ParentID = parentID.Value,
                isValid = malt.isValid,
                ParentTableName = malt.MAEarthmatID.HasValue ? TableEarthMat : TableStation
            };

            return malFN;
        }

        /// <summary>
        /// Will return a field note object with the information from the given location.
        /// This information will be displayed in the field note page
        /// </summary>
        /// <param name="loc"></param>
        /// <returns></returns>
        public FieldNote GetLocationFieldNote(FieldLocation loc)
        {
            string coordinatesFormat = string.Format("{0}° {1}°",
                Math.Round(loc.LocationLat, 8), Math.Round(loc.LocationLong, 8));

            //Date formating
            //Note this could be different than the date in the station table
            //Resulting in location and station related records being on different dates in the field notes
            string locDate = DateTime.MinValue.ToString("yyyy-MM-dd");
            if (loc.LocationTimestamp != null && loc.LocationTimestamp != string.Empty)
            {
                locDate = loc.LocationTimestamp.Substring(0, 10);
            }

            FieldNote locFN = new FieldNote
            {
                Display_text_1 = loc.LocationAliasLight,
                Display_text_2 = coordinatesFormat,
                Display_text_3 = loc.locationNTS,
                GenericTableName = TableLocation,
                GenericID = loc.LocationID,
                ParentID = loc.MetaID,
                isValid = loc.isValid,
                Date = locDate,
                ParentTableName = TableMetadata
            };

            return locFN;
        }

        /// <summary>
        /// Will return a field note object with the information from the given drill holes.
        /// This information will be displayed in the field note page
        /// </summary>
        /// <param name="dh"></param>
        /// <returns></returns>
        public FieldNote GetDrillFieldNote(DrillHole dh)
        {
            FieldNote dhFN = new FieldNote
            {
                Display_text_1 = dh.DrillAliasLight,
                Display_text_2 = dh.DrillCompany,
                Display_text_3 = dh.DrillType,
                GenericTableName = TableDrillHoles,
                GenericID = dh.DrillID,
                ParentID = dh.DrillLocationID,
                isValid = dh.isValid,
                ParentTableName = TableLocation
            };

            return dhFN;
        }

        /// <summary>
        /// Will return a field note object with the information from the given drill holes.
        /// This information will be displayed in the field note page
        /// </summary>
        /// <param name="dh"></param>
        /// <returns></returns>
        public FieldNote GetLineworkFieldNote(Linework lw)
        {
            FieldNote lwFN = new FieldNote
            {
                Display_text_1 = lw.LineAliasLight,
                Display_text_2 = lw.LineType,
                Display_text_3 = lw.LineNotes,
                GenericTableName = TableLinework,
                GenericID = lw.LineID,
                ParentID = lw.LineMetaID,
                isValid = lw.isValid,
                ParentTableName = TableMetadata,

            };

            return lwFN;
        }

        /// <summary>
        /// Will replace a field note in the collection with the one passed in.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="fnToUpdate"></param>
        private async Task<FieldNote> ReplaceFieldNote(TableNames table, FieldNote fnToUpdate)
        {

            // Find the existing FieldNote in the collection
            FieldNote existingFN= FieldNotes[table].Where(x => x.GenericID == fnToUpdate.GenericID).FirstOrDefault(fnToUpdate);

            if (existingFN != null && existingFN != fnToUpdate)
            {
                //For current selected dates
                int indexOfStation = FieldNotes[table].IndexOf(existingFN);
                if (indexOfStation > -1)
                {
                    FieldNotes[table].RemoveAt(indexOfStation);
                    FieldNotes[table].Insert(indexOfStation, fnToUpdate);
                }

                //For all dates
                int indexOfStationAll = FieldNotesAll[table].IndexOf(existingFN);
                if (indexOfStationAll > -1)
                {
                    FieldNotesAll[table].RemoveAt(indexOfStationAll);
                    FieldNotesAll[table].Insert(indexOfStationAll, fnToUpdate);
                }
            }

            return existingFN;
        }

        /// <summary>
        /// Will replace a field note in the collection with the one passed in.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="fnToUpdate"></param>
        private void AddFieldNote(TableNames table, FieldNote fnToUpdate)
        {
            ObservableCollection<FieldNote> addFNList = new ObservableCollection<FieldNote>() { fnToUpdate };

            //Adding a new record will force a new date selection
            if (Dates.Count() > 0 && fnToUpdate.Date != null && Dates.Contains(fnToUpdate.Date))
            {
                _selectedDate = fnToUpdate.Date;
                OnPropertyChanged(nameof(SelectedDate));

                // Add the new FieldNote to the collection
                ObservableCollectionHelper.AddRange(FieldNotes[table], addFNList);
            }

            //Add to all notes collection (for date filtering mainly)
            ObservableCollectionHelper.AddRange(FieldNotesAll[table], addFNList);

        }

        /// <summary>
        /// Will remove a field note from the collection based on the table and id.
        /// </summary>
        /// <param name="tableToDeleteFrom"></param>
        /// <param name="id"></param>
        /// <param name="genericOrParent">True if using generic id, false if using parent id</param>
        /// <returns></returns>
        private List<int> RemoveFieldNote(TableNames tableToDeleteFrom, int id, bool genericOrParent = true, string parentTableName = "")
        {
            List<int> removedIds = new List<int>();
            List<FieldNote> existingNote = null;

            //From parent ids
            if (genericOrParent)
            {
                existingNote = FieldNotes[tableToDeleteFrom].Where(x => x.GenericID == id).ToList();
            }
            else
            {
                existingNote = FieldNotes[tableToDeleteFrom].Where(x => x.ParentID == id && (x.ParentTableName == parentTableName || x.ParentTableName == "")).ToList();
            }

            if (existingNote != null && existingNote.Count() > 0)
            {
                foreach (FieldNote note in existingNote)
                {
                    //Remove from the collection
                    try
                    {
                        FieldNotes[tableToDeleteFrom].Remove(note);
                        FieldNotesAll[tableToDeleteFrom].Remove(note);
                    }
                    catch (Exception e)
                    {
                        new ErrorToLogFile(e).WriteToFile();
                    }

                    removedIds.Add(note.GenericID);
                }
            }

            return removedIds;
        }

        /// <summary>
        /// Will remove all of earth material child field notes
        /// </summary>
        /// <param name="tableToDeleteFrom"></param>
        /// <param name="id"></param>
        private void RemoveEarthmatChildFieldNotes(int id)
        {

            //Remove childs
            List<int> removedSamples = RemoveFieldNote(TableNames.sample, id, false, TableEarthMat);

            List<int> removedStructures = RemoveFieldNote(TableNames.structure, id, false, TableEarthMat);

            List<int> removedPflow = RemoveFieldNote(TableNames.pflow, id, false, TableEarthMat);

            List<int> removedFossil = RemoveFieldNote(TableNames.fossil, id, false, TableEarthMat);

            List<int> removedMineral = RemoveFieldNote(TableNames.mineral, id, false, TableEarthMat);


            List<int> removedMineralizations = RemoveFieldNote(TableNames.mineralization, id, false, TableEarthMat);
            foreach (int maID in removedMineralizations)
            {
                RemoveMineralizationChildFieldNotes(maID);
            }

            try
            {
                OnPropertyChanged(nameof(Samples));
                OnPropertyChanged(nameof(Structures));
                OnPropertyChanged(nameof(Paleoflows));
                OnPropertyChanged(nameof(Fossils));
                OnPropertyChanged(nameof(Minerals));
                OnPropertyChanged(nameof(MineralizationAlterations));
            }
            catch (Exception except)
            {
                new ErrorToLogFile(except).WriteToFile();
            }
        }

        /// <summary>
        /// Will remove all of station child field notes
        /// </summary>
        /// <param name="tableToDeleteFrom"></param>
        /// <param name="id"></param>
        private void RemoveStationChildFieldNotes(int id)
        {

            List<int> emREM = RemoveFieldNote(TableNames.earthmat, id, false, TableStation);
            foreach (int remEM in emREM)
            {
                RemoveEarthmatChildFieldNotes(remEM);
            }
                
            List<int> remMineralization = RemoveFieldNote(TableNames.mineralization, id, false, TableStation);
            foreach (int remMz in remMineralization)
            {
                RemoveMineralizationChildFieldNotes(remMz);
            }
                
            RemoveFieldNote(TableNames.environment, id, false, TableStation);

            RemoveFieldNote(TableNames.document, id, false, TableStation);

            try
            {

                OnPropertyChanged(nameof(Environments));
                OnPropertyChanged(nameof(Documents));
                OnPropertyChanged(nameof(MineralizationAlterations));
            }
            catch (Exception except)
            {
                new ErrorToLogFile(except).WriteToFile();
            }
        }

        /// <summary>
        /// Will remove all of station child field notes
        /// </summary>
        /// <param name="tableToDeleteFrom"></param>
        /// <param name="id"></param>
        private void RemoveMineralizationChildFieldNotes(int id)
        {

            List<int> removedMinerals = RemoveFieldNote(TableNames.mineral, id, false, TableMineralAlteration);

            try
            {
                OnPropertyChanged(nameof(Minerals));
            }
            catch (Exception except)
            {
                new ErrorToLogFile(except).WriteToFile();
            }
        }

        /// <summary>
        /// Will remove all of drill hole child field notes
        /// </summary>
        /// <param name="tableToDeleteFrom"></param>
        /// <param name="id"></param>
        private void RemoveDrillHoleChildFieldNotes(int id)
        {

            List<int> removedEM = RemoveFieldNote(TableNames.earthmat, id, false, TableDrillHoles);
            foreach (int rEM in removedEM)
            {
                RemoveEarthmatChildFieldNotes(rEM);
            }

            RemoveFieldNote(TableNames.document, id, false, TableDrillHoles);

            try
            {
                OnPropertyChanged(nameof(Documents));
                OnPropertyChanged(nameof(EarthMats));

            }
            catch (Exception except)
            {
                new ErrorToLogFile(except).WriteToFile();
            }
        }
        #endregion

        #region EVENTS

        /// <summary>
        /// Event triggered when user has changed field books.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void FieldBooksViewModel_newFieldBookSelectedAsync(object sender, bool hasChanged)
        {
            if (hasChanged)
            {
                //Reload all notes
                await ValidateFillFieldNotesAsync(true);
            }

        }

        /// <summary>
        /// Event based method to add a new record in field note page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void FieldAppPageHelper_newRecordAsync(object sender, Tuple<TableNames, object> e)
        {
            if (e != null)
            {
                Tuple<TableNames, object> newRec = (Tuple<TableNames, object>)e;

                if (newRec != null)
                {
                    switch (newRec.Item1)
                    {
                        case TableNames.station:
                            Station station = (Station)newRec.Item2;
                            if (station != null)
                            {
                                FieldNote stationsFN = GetStatFieldNote(station);
                                AddFieldNote(TableNames.station, stationsFN);

                                try
                                {
                                    OnPropertyChanged(nameof(Stations));
                                }
                                catch (Exception except)
                                {
                                    new ErrorToLogFile(except).WriteToFile();
                                }
                            }
                            break;

                        case TableNames.earthmat:
                            Earthmaterial earthmaterial = (Earthmaterial)newRec.Item2;

                            if (earthmaterial != null)
                            {
                                FieldNote emsFN = GetEMFieldNote(earthmaterial);
                                AddFieldNote(TableNames.earthmat, emsFN);

                                try
                                {
                                    OnPropertyChanged(nameof(EarthMats));
                                }
                                catch (Exception except)
                                {
                                    new ErrorToLogFile(except).WriteToFile();
                                }

                            }

                            break;

                        case TableNames.sample:
                            Sample sample = (Sample)newRec.Item2;
                            if (sample != null)
                            {
                                FieldNote samFN = GetSampleFieldNote(sample);
                                AddFieldNote(TableNames.sample, samFN);
                                try
                                {
                                    OnPropertyChanged(nameof(Samples));
                                }
                                catch (Exception except)
                                {
                                    new ErrorToLogFile(except).WriteToFile();
                                }
                            }
                            break;
                        case TableNames.document:
                            Document document = (Document)newRec.Item2;
                            if (document != null)
                            {
                                FieldNote dcFN = GetDocumentFieldNote(document);
                                AddFieldNote(TableNames.document, dcFN);
                                try
                                {
                                    OnPropertyChanged(nameof(Documents));
                                }
                                catch (Exception except)
                                {
                                    new ErrorToLogFile(except).WriteToFile();
                                }
                            }
                            break;
                        case TableNames.structure:
                            Structure structure = (Structure)newRec.Item2;
                            if (structure != null)
                            {
                                FieldNote structureFN = GetStructureFieldNote(structure);
                                AddFieldNote(TableNames.structure, structureFN);
                                try
                                {
                                    OnPropertyChanged(nameof(Structures));
                                }
                                catch (Exception except)
                                {
                                    new ErrorToLogFile(except).WriteToFile();
                                }
                            }
                            break;
                        case TableNames.pflow:
                            Paleoflow pflow = (Paleoflow)newRec.Item2;
                            if (pflow != null)
                            {
                                FieldNote pfFN = GetPflowFieldNote(pflow);
                                AddFieldNote(TableNames.pflow, pfFN);
                                try
                                {
                                    OnPropertyChanged(nameof(Paleoflows));
                                }
                                catch (Exception except)
                                {
                                    new ErrorToLogFile(except).WriteToFile();
                                }
                            }
                            break;
                        case TableNames.fossil:
                            Fossil fossil = (Fossil)newRec.Item2;
                            if (fossil != null)
                            {
                                FieldNote fossFN = GetFossilFieldNote(fossil);
                                AddFieldNote(TableNames.fossil, fossFN);
                                try
                                {
                                    OnPropertyChanged(nameof(Fossils));
                                }
                                catch (Exception except)
                                {
                                    new ErrorToLogFile(except).WriteToFile();
                                }
                            }
                            break;
                        case TableNames.environment:
                            EnvironmentModel environment = (EnvironmentModel)newRec.Item2;
                            if (environment != null)
                            {
                                FieldNote envFN = GetEnvironmentFieldNote(environment);
                                AddFieldNote(TableNames.environment, envFN);
                                try
                                {
                                    OnPropertyChanged(nameof(Environments));
                                }
                                catch (Exception except)
                                {
                                    new ErrorToLogFile(except).WriteToFile();
                                }
                            }
                            break;
                        case TableNames.mineral:
                            Mineral mineral = (Mineral)newRec.Item2;
                            if (mineral != null)
                            {
                                FieldNote minFN = GetMineralFieldNote(mineral);
                                AddFieldNote(TableNames.mineral, minFN);
                                try
                                {
                                    OnPropertyChanged(nameof(Minerals));
                                }
                                catch (Exception except)
                                {
                                    new ErrorToLogFile(except).WriteToFile();
                                }
                            }
                            break;
                        case TableNames.mineralization:
                            MineralAlteration mineralAlteration = (MineralAlteration)newRec.Item2;
                            if (mineralAlteration != null)
                            {
                                FieldNote malFN = GetMAFieldNote(mineralAlteration);
                                AddFieldNote(TableNames.mineralization, malFN);
                                try
                                {
                                    OnPropertyChanged(nameof(MineralizationAlterations));
                                }
                                catch (Exception except)
                                {
                                    new ErrorToLogFile(except).WriteToFile();
                                }
                            }
                            break;
                        case TableNames.drill:
                            DrillHole drillHole = (DrillHole)newRec.Item2;
                            if (drillHole != null)
                            {
                                FieldNote dhFN = GetDrillFieldNote(drillHole);
                                AddFieldNote(TableNames.drill, dhFN);
                                try
                                {
                                    OnPropertyChanged(nameof(DrillHoles));
                                }
                                catch (Exception except)
                                {
                                    new ErrorToLogFile(except).WriteToFile();
                                }
                            }
                            break;
                        case TableNames.location:
                            FieldLocation location = (FieldLocation)newRec.Item2;
                            if (location != null)
                            {
                                FieldNote locFN = GetLocationFieldNote(location);
                                AddFieldNote(TableNames.location, locFN);
                                try
                                {
                                    OnPropertyChanged(nameof(Locations));
                                }
                                catch (Exception except)
                                {
                                    new ErrorToLogFile(except).WriteToFile();
                                }
                            }
                            break;
                        case TableNames.linework:
                            Linework linework = (Linework)newRec.Item2;
                            if (linework != null)
                            {
                                FieldNote lwFN = GetLineworkFieldNote(linework);
                                AddFieldNote(TableNames.linework, lwFN);
                                try
                                {
                                    OnPropertyChanged(nameof(Lineworks));
                                }
                                catch (Exception except)
                                {
                                    new ErrorToLogFile(except).WriteToFile();
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }

            }
        }

        /// <summary>
        /// Event based method to update an existing record in the field note page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private async void FieldAppPageHelper_updateRecordAsync(object sender, Tuple<TableNames, object> e)
        {
            if (e != null)
            {
                Tuple<TableNames, object> newRec = (Tuple<TableNames, object>)e;

                if (newRec != null)
                {
                    switch (newRec.Item1)
                    {
                        case TableNames.station:
                            Station station = (Station)newRec.Item2;
                            if (station != null)
                            {
                                FieldNote upStationsFN = GetStatFieldNote(station);

                                FieldNote replacedNote = await ReplaceFieldNote(newRec.Item1, upStationsFN);

                                try
                                {
                                    OnPropertyChanged(nameof(Stations));
                                }
                                catch (Exception except)
                                {
                                    new ErrorToLogFile(except).WriteToFile();
                                }
   
                            }
                            break;

                        case TableNames.earthmat:
                            Earthmaterial earthmaterial = (Earthmaterial)newRec.Item2;

                            if (earthmaterial != null)
                            {
                                FieldNote updateEMFN = GetEMFieldNote(earthmaterial);

                                FieldNote replacedNote = await ReplaceFieldNote(newRec.Item1, updateEMFN);

                                try
                                {
                                    OnPropertyChanged(nameof(EarthMats));
                                }
                                catch (Exception except)
                                {
                                    new ErrorToLogFile(except).WriteToFile();
                                }

                            }

                            break;

                        case TableNames.sample:
                            Sample sample = (Sample)newRec.Item2;
                            if (sample != null)
                            {
                                FieldNote samFN = GetSampleFieldNote(sample);
                                FieldNote replacedNote = await ReplaceFieldNote(newRec.Item1, samFN);

                                try
                                {
                                    OnPropertyChanged(nameof(Samples));
                                }
                                catch (Exception except)
                                {
                                    new ErrorToLogFile(except).WriteToFile();
                                }
                            }
                            break;
                        case TableNames.document:
                            Document document = (Document)newRec.Item2;
                            if (document != null)
                            {
                                FieldNote dcFN = GetDocumentFieldNote(document);
                                FieldNote replacedNote = await ReplaceFieldNote(newRec.Item1, dcFN);

                                try
                                {
                                    OnPropertyChanged(nameof(Documents));
                                }
                                catch (Exception except)
                                {
                                    new ErrorToLogFile(except).WriteToFile();
                                }
                            }
                            break;
                        case TableNames.structure:
                            Structure structure = (Structure)newRec.Item2;
                            if (structure != null)
                            {
                                FieldNote structureFN = GetStructureFieldNote(structure);
                                FieldNote replacedNote = await ReplaceFieldNote(newRec.Item1, structureFN);

                                try
                                {
                                    OnPropertyChanged(nameof(Structures));
                                }
                                catch (Exception except)
                                {
                                    new ErrorToLogFile(except).WriteToFile();
                                }
                            }
                            break;
                        case TableNames.pflow:
                            Paleoflow pflow = (Paleoflow)newRec.Item2;
                            if (pflow != null)
                            {
                                FieldNote pfFN = GetPflowFieldNote(pflow);
                                FieldNote replacedNote = await ReplaceFieldNote(newRec.Item1, pfFN);

                                try
                                {
                                    OnPropertyChanged(nameof(Paleoflows));
                                }
                                catch (Exception except)
                                {
                                    new ErrorToLogFile(except).WriteToFile();
                                }
                            }
                            break;
                        case TableNames.fossil:
                            Fossil fossil = (Fossil)newRec.Item2;
                            if (fossil != null)
                            {
                                FieldNote fossFN = GetFossilFieldNote(fossil);
                                FieldNote replacedNote = await ReplaceFieldNote(newRec.Item1, fossFN);

                                try
                                {
                                    OnPropertyChanged(nameof(Fossils));
                                }
                                catch (Exception except)
                                {
                                    new ErrorToLogFile(except).WriteToFile();
                                }
                            }
                            break;
                        case TableNames.environment:
                            EnvironmentModel environment = (EnvironmentModel)newRec.Item2;
                            if (environment != null)
                            {
                                FieldNote envFN = GetEnvironmentFieldNote(environment);
                                FieldNote replacedNote = await ReplaceFieldNote(newRec.Item1, envFN);

                                try
                                {
                                    OnPropertyChanged(nameof(Environments));
                                }
                                catch (Exception except)
                                {
                                    new ErrorToLogFile(except).WriteToFile();
                                }
                            }
                            break;
                        case TableNames.mineral:
                            Mineral mineral = (Mineral)newRec.Item2;
                            if (mineral != null)
                            {
                                FieldNote minFN = GetMineralFieldNote(mineral);
                                FieldNote replacedNote = await ReplaceFieldNote(newRec.Item1, minFN);

                                try
                                {
                                    OnPropertyChanged(nameof(Minerals));
                                }
                                catch (Exception except)
                                {
                                    new ErrorToLogFile(except).WriteToFile();
                                }
                            }
                            break;
                        case TableNames.mineralization:
                            MineralAlteration mineralAlteration = (MineralAlteration)newRec.Item2;
                            if (mineralAlteration != null)
                            {
                                FieldNote malFN = GetMAFieldNote(mineralAlteration);
                                FieldNote replacedNote = await ReplaceFieldNote(newRec.Item1, malFN);

                                try
                                {
                                    OnPropertyChanged(nameof(MineralizationAlterations));
                                }
                                catch (Exception except)
                                {
                                    new ErrorToLogFile(except).WriteToFile();
                                }
                            }
                            break;
                        case TableNames.drill:
                            DrillHole drillHole = (DrillHole)newRec.Item2;
                            if (drillHole != null)
                            {
                                FieldNote dhFN = GetDrillFieldNote(drillHole);
                                FieldNote replacedNote = await ReplaceFieldNote(newRec.Item1, dhFN);

                                try
                                {
                                    OnPropertyChanged(nameof(DrillHoles));
                                }
                                catch (Exception except)
                                {
                                    new ErrorToLogFile(except).WriteToFile();
                                }
                            }
                            break;
                        case TableNames.location:
                            FieldLocation location = (FieldLocation)newRec.Item2;
                            if (location != null)
                            {
                                FieldNote locFN = GetLocationFieldNote(location);
                                FieldNote replacedNote = await ReplaceFieldNote(newRec.Item1, locFN);

                                try
                                {
                                    OnPropertyChanged(nameof(Locations));
                                }
                                catch (Exception except)
                                {
                                    new ErrorToLogFile(except).WriteToFile();
                                }
                            }
                            break;
                        case TableNames.linework:
                            Linework linework = (Linework)newRec.Item2;
                            if (linework != null)
                            {
                                FieldNote lwFN = GetLineworkFieldNote(linework);
                                FieldNote replacedNote = await ReplaceFieldNote(newRec.Item1, lwFN);

                                try
                                {
                                    OnPropertyChanged(nameof(Lineworks));
                                }
                                catch (Exception except)
                                {
                                    new ErrorToLogFile(except).WriteToFile();
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Event based method to delete a record in the field note page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private async void FieldAppPageHelper_deleteRecordAsync(object sender, Tuple<TableNames, int> e)
        {
            if (e != null)
            {
                Tuple<TableNames, int> delRec = (Tuple<TableNames, int>)e;

                if (delRec != null)
                {
                    FieldNote emptyNote = null;

                    switch (delRec.Item1)
                    {
                        case TableNames.station:

                            List<int> stationRemoved = RemoveFieldNote(delRec.Item1, delRec.Item2);
                            foreach (var st in stationRemoved)
                            {
                                RemoveStationChildFieldNotes(st);
                            }

                            try
                            {
                                OnPropertyChanged(nameof(Stations));
                            }
                            catch (Exception except)
                            {
                                new ErrorToLogFile(except).WriteToFile();
                            }

                            break;

                        case TableNames.earthmat:

                            List<int> earthmatsRemoved = RemoveFieldNote(delRec.Item1, delRec.Item2);
                            foreach (var ems in earthmatsRemoved)
                            {
                                RemoveEarthmatChildFieldNotes(ems);
                            }

                            try
                            {
                                OnPropertyChanged(nameof(EarthMats));
                            }
                            catch (Exception except)
                            {
                                new ErrorToLogFile(except).WriteToFile();
                            }


                            break;

                        case TableNames.sample:

                            RemoveFieldNote(delRec.Item1, delRec.Item2);

                            try
                            {
                                OnPropertyChanged(nameof(Samples));
                            }
                            catch (Exception except)
                            {
                                new ErrorToLogFile(except).WriteToFile();
                            }

                            break;
                        case TableNames.document:
                            RemoveFieldNote(delRec.Item1, delRec.Item2);

                            try
                            {
                                OnPropertyChanged(nameof(Documents));
                            }
                            catch (Exception except)
                            {
                                new ErrorToLogFile(except).WriteToFile();
                            }
                            break;
                        case TableNames.structure:

                            RemoveFieldNote(delRec.Item1, delRec.Item2);

                            try
                            {
                                OnPropertyChanged(nameof(Structures));
                            }
                            catch (Exception except)
                            {
                                new ErrorToLogFile(except).WriteToFile();
                            }

                            break;
                        case TableNames.pflow:
                            RemoveFieldNote(delRec.Item1, delRec.Item2);

                            try
                            {
                                OnPropertyChanged(nameof(Paleoflows));
                            }
                            catch (Exception except)
                            {
                                new ErrorToLogFile(except).WriteToFile();
                            }
                            break;
                        case TableNames.fossil:
                            RemoveFieldNote(delRec.Item1, delRec.Item2);

                            try
                            {
                                OnPropertyChanged(nameof(Fossils));
                            }
                            catch (Exception except)
                            {
                                new ErrorToLogFile(except).WriteToFile();
                            }
                            break;
                        case TableNames.environment:
                            RemoveFieldNote(delRec.Item1, delRec.Item2);

                            try
                            {
                                OnPropertyChanged(nameof(Environments));
                            }
                            catch (Exception except)
                            {
                                new ErrorToLogFile(except).WriteToFile();
                            }
                            break;
                        case TableNames.mineral:
                            RemoveFieldNote(delRec.Item1, delRec.Item2);

                            try
                            {
                                OnPropertyChanged(nameof(Minerals));
                            }
                            catch (Exception except)
                            {
                                new ErrorToLogFile(except).WriteToFile();
                            }
                            break;
                        case TableNames.mineralization:

                            List<int> mineralizationRemoved = RemoveFieldNote(delRec.Item1, delRec.Item2);
                            foreach (var mzs in mineralizationRemoved)
                            {
                                RemoveMineralizationChildFieldNotes(mzs);
                            }

                            try
                            {
                                OnPropertyChanged(nameof(MineralizationAlterations));
                            }
                            catch (Exception except)
                            {
                                new ErrorToLogFile(except).WriteToFile();
                            }
                            break;

                        case TableNames.drill:

                            List<int> drillsRemoved = RemoveFieldNote(delRec.Item1, delRec.Item2);
                            foreach (var dsRem in drillsRemoved)
                            {
                                RemoveDrillHoleChildFieldNotes(dsRem);
                            }
                            try
                            {
                                OnPropertyChanged(nameof(DrillHoles));
                            }
                            catch (Exception except)
                            {
                                new ErrorToLogFile(except).WriteToFile();
                            }

                            break;
                        case TableNames.location:

                            RemoveFieldNote(delRec.Item1, delRec.Item2);

                            List<int> locationDrillsRemoved = RemoveFieldNote(TableNames.drill, delRec.Item2, false, TableLocation);
                            foreach (int dIds in locationDrillsRemoved)
                            {
                                RemoveDrillHoleChildFieldNotes(dIds);
                            }

                            List<int> locationStationsRemoved = RemoveFieldNote(TableNames.station, delRec.Item2, false, TableLocation);
                            foreach (int sIds in locationStationsRemoved)
                            {
                                RemoveStationChildFieldNotes(sIds);
                            }


                            try
                            {
                                OnPropertyChanged(nameof(Locations));
                                OnPropertyChanged(nameof(Stations));
                                OnPropertyChanged(nameof(DrillHoles));
                            }
                            catch (Exception except)
                            {
                                new ErrorToLogFile(except).WriteToFile();
                            }


                            break;
                        case TableNames.linework:

                            RemoveFieldNote(delRec.Item1, delRec.Item2);

                            try
                            {
                                OnPropertyChanged(nameof(Lineworks));
                            }
                            catch (Exception except)
                            {
                                new ErrorToLogFile(except).WriteToFile();
                            }
                            break;
                        default:
                            break;
                    }

                }
            }
        }
        #endregion

    }
}
