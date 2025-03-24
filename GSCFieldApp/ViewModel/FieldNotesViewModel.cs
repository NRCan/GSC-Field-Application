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
            _ = Task.Run(async () => await FillFieldNotesAsync());

            //Detect new field book selection, uprgrade, edit, ...
            FieldBooksViewModel.newFieldBookSelected += FieldBooksViewModel_newFieldBookSelectedAsync;
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
                    await FillFieldNotesAsync();
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
            SQLiteAsyncConnection currentConnection = da.GetConnectionFromPath(da.PreferedDatabasePath);

            //Get desire form on screen from tapped table name
            if (fieldNotes.GenericTableName == TableStation)
            {
                List<Station> tappedStation = await currentConnection.Table<Station>().Where(i => i.StationID == fieldNotes.GenericID).ToListAsync();

                await currentConnection.CloseAsync();

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
                List<Earthmaterial> tappedEM = await currentConnection.Table<Earthmaterial>().Where(i => i.EarthMatID == fieldNotes.GenericID).ToListAsync();

                await currentConnection.CloseAsync();

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
                List<Sample> tappedSample = await currentConnection.Table<Sample>().Where(i => i.SampleID == fieldNotes.GenericID).ToListAsync();

                await currentConnection.CloseAsync();

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
                List<Structure> tappedStructure = await currentConnection.Table<Structure>().Where(i => i.StructureID == fieldNotes.GenericID).ToListAsync();

                await currentConnection.CloseAsync();

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
                List<Paleoflow> tappedPflow = await currentConnection.Table<Paleoflow>().Where(i => i.PFlowID == fieldNotes.GenericID).ToListAsync();

                await currentConnection.CloseAsync();

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
                List<Fossil> tappedFossil= await currentConnection.Table<Fossil>().Where(i => i.FossilID == fieldNotes.GenericID).ToListAsync();

                await currentConnection.CloseAsync();

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
                List<EnvironmentModel> tappedEnv = await currentConnection.Table<EnvironmentModel>().Where(i => i.EnvID == fieldNotes.GenericID).ToListAsync();

                await currentConnection.CloseAsync();

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
                List<Document> tappedDoc = await currentConnection.Table<Document>().Where(i => i.DocumentID == fieldNotes.GenericID).ToListAsync();

                await currentConnection.CloseAsync();

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
                List<Mineral> tappedMineral = await currentConnection.Table<Mineral>().Where(i => i.MineralID == fieldNotes.GenericID).ToListAsync();

                await currentConnection.CloseAsync();

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
                List<MineralAlteration> tappedMineralization = await currentConnection.Table<MineralAlteration>().Where(i => i.MAID == fieldNotes.GenericID).ToListAsync();

                await currentConnection.CloseAsync();

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
                List<FieldLocation> tappedLocation = await currentConnection.Table<FieldLocation>().Where(i => i.LocationID == fieldNotes.GenericID).ToListAsync();

                await currentConnection.CloseAsync();

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
                List<DrillHole> tappedDrill= await currentConnection.Table<DrillHole>().Where(i => i.DrillID == fieldNotes.GenericID).ToListAsync();

                await currentConnection.CloseAsync();

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
                List<Linework> tappedLine = await currentConnection.Table<Linework>().Where(i => i.LineID == fieldNotes.GenericID).ToListAsync();

                await currentConnection.CloseAsync();

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
        /// Will initiate a fill of all records in the database
        /// </summary>
        /// <returns></returns>
        public async Task FillFieldNotesAsync()
        {
            if (da.PreferedDatabasePath != null && da.PreferedDatabasePath != string.Empty)
            {

                SQLiteAsyncConnection currentConnection = new SQLiteAsyncConnection(da.PreferedDatabasePath);

                List<Task> tasks = new List<Task>();
                tasks.Add(FillTraverseDates(currentConnection));
                tasks.Add(FillStationNotes(currentConnection));
                tasks.Add(FillEMNotes(currentConnection));
                tasks.Add(FillSampleNotes(currentConnection));
                tasks.Add(FillDocumentNotes(currentConnection));
                tasks.Add(FillStructureNotes(currentConnection));
                tasks.Add(FillPaleoflowNotes(currentConnection));
                tasks.Add(FillFossilNotes(currentConnection));
                tasks.Add(FillEnvironmentNotes(currentConnection));
                tasks.Add(FillMineralNotes(currentConnection));
                tasks.Add(FillMineralizationAlterationNotes(currentConnection));
                tasks.Add(FillLocationNotes(currentConnection));
                tasks.Add(FillDrillHoleNotes(currentConnection));
                tasks.Add(FillLineworkNotes(currentConnection));

                await Task.WhenAll(tasks).ConfigureAwait(false);

                await currentConnection.CloseAsync();

                //OnPropertyChanged(nameof(FieldNotes));
                OnPropertyChanged(nameof(Dates));

                //Make a copy in case user wants to refilter values
                FieldNotesAll = new Dictionary<TableNames, ObservableCollection<FieldNote>>(FieldNotes);

                //Filter out latest date
                //TODO uncomment if really needed
                if (Dates != null && Dates.Count > 0)
                {
                    await FilterRecordsOnDate(Dates.First());
                }


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
            _dates.Clear();

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
                    }

                    if (!_dates.Contains(dDate))
                    {
                        _dates.Add(dDate);
                    }

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
                    stationsFN.Add(new FieldNote
                    {
                        Display_text_1 = st.StationAliasLight,
                        Display_text_2 = st.StationObsType,
                        Display_text_3 = st.StationNote,
                        GenericTableName = TableStation,
                        GenericID = st.StationID,
                        ParentID = st.LocationID,
                        Date = st.StationVisitDate,
                        isValid = st.isValid
                    });
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
                    foreach (Earthmaterial st in ems)
                    {
                        int parentID = -1;
                        string parentLightAlias = string.Empty;
                        if (st.EarthMatStatID != null)
                        {
                            parentID = (int)st.EarthMatStatID;
                        }
                        if (st.EarthMatDrillHoleID != null)
                        {
                            parentID = (int)st.EarthMatDrillHoleID;
                        }
                        emsFN.Add(new FieldNote
                        {

                            Display_text_1 = st.EarthmatAliasLight,
                            Display_text_2 = st.EarthMatLithdetail,
                            Display_text_3 = st.EarthMatLithgroup,
                            GenericTableName = TableEarthMat,
                            GenericID = st.EarthMatID,
                            ParentID = parentID,
                            isValid = st.isValid
                        });
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

                    foreach (Sample st in samples)
                    {
                        samplesFN.Add(new FieldNote
                        {
                            Display_text_1 = st.SampleAliasLight,
                            Display_text_2 = st.SampleType,
                            Display_text_3 = st.SamplePurpose,
                            GenericTableName = TableSample,
                            GenericID = st.SampleID,
                            ParentID = st.SampleEarthmatID,
                            isValid = st.isValid
                        });
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
                        int? parentID = dc.StationID;

                        if (!parentID.HasValue && dc.DrillHoleID.HasValue)
                        {
                            parentID = dc.DrillHoleID;
                        }
                        docsFN.Add(new FieldNote
                        {
                            Display_text_1 = dc.DocumentAliasLight,
                            Display_text_2 = dc.Category,
                            Display_text_3 = dc.Description,
                            GenericTableName = TableDocument,
                            GenericID = dc.DocumentID,
                            ParentID = parentID.Value,
                            isValid = dc.isValid
                        });
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

                    foreach (Structure st in structures)
                    {
                        structuresFN.Add(new FieldNote
                        {
                            Display_text_1 = st.StructureAliasLight,
                            Display_text_2 = st.StructureClass,
                            Display_text_3 = st.StructureDetail,
                            GenericTableName = TableStructure,
                            GenericID = st.StructureID,
                            ParentID = st.StructureEarthmatID,
                            isValid = st.isValid
                        });
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

                    foreach (Paleoflow st in pflows)
                    {
                        pflowsFN.Add(new FieldNote
                        {
                            Display_text_1 = st.PflowAliasLight,
                            Display_text_2 = st.PFlowClass,
                            Display_text_3 = st.PFlowSense,
                            GenericTableName = TablePFlow,
                            GenericID = st.PFlowID,
                            ParentID = st.PFlowParentID,
                            isValid = st.isValid
                        });
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

                    foreach (Fossil st in fossils)
                    {
                        fossilsFN.Add(new FieldNote
                        {
                            Display_text_1 = st.FossilAliasLight,
                            Display_text_2 = st.FossilType,
                            Display_text_3 = st.FossilNote,
                            GenericTableName = TableFossil,
                            GenericID = st.FossilID,
                            ParentID = st.FossilParentID,
                            isValid = st.isValid
                        });
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

                    foreach (EnvironmentModel st in environments)
                    {
                        environmentsFN.Add(new FieldNote
                        {
                            Display_text_1 = st.EnvironmentAliasLight,
                            Display_text_2 = st.EnvRelief,
                            Display_text_3 = st.EnvNotes,
                            GenericTableName = TableEnvironment,
                            GenericID = st.EnvID,
                            ParentID = st.EnvStationID,
                            isValid = st.isValid
                        });
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

                    foreach (Mineral st in minerals)
                    {
                        //Find proper parent
                        int? parentID = st.MineralEMID;
                        if (parentID == null && st.MineralMAID != null)
                        {
                            parentID = st.MineralMAID;
                        }

                        mineralsFN.Add(new FieldNote
                        {
                            Display_text_1 = st.MineralAliasLight,
                            Display_text_2 = st.MineralName,
                            Display_text_3 = st.MineralMode,
                            GenericTableName = TableMineral,
                            GenericID = st.MineralID,
                            ParentID = parentID.Value,
                            isValid = st.isValid
                        });
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

                    foreach (MineralAlteration st in mineralizations)
                    {
                        //Find proper parent
                        int? parentID = st.MAStationID;
                        if (parentID == null && st.MAEarthmatID != null)
                        {
                            parentID = st.MAEarthmatID;
                        }

                        mineralizationsFN.Add(new FieldNote
                        {
                            Display_text_1 = st.MineralALterationAliasLight,
                            Display_text_2 = st.MAMA,
                            Display_text_3 = st.MAUnit,
                            GenericTableName = TableMineralAlteration,
                            GenericID = st.MAID,
                            ParentID = parentID.Value,
                            isValid = st.isValid
                        });
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

            if (LocationVisible)
            {
                //Get all stations from database
                List<FieldLocation> locations = await inConnection.Table<FieldLocation>().OrderBy(s => s.LocationID).ToListAsync();
                ObservableCollection<FieldNote> locationsFN = new ObservableCollection<FieldNote>();
                if (locations != null && locations.Count > 0)
                {

                    foreach (FieldLocation loc in locations)
                    {
                        string coordinatesFormat = string.Format("{0}° {1}°",
                            Math.Round(loc.LocationLat, 8), Math.Round(loc.LocationLong, 8));
                        locationsFN.Add(new FieldNote
                        {
                            Display_text_1 = loc.LocationAliasLight,
                            Display_text_2 = coordinatesFormat,
                            Display_text_3 = loc.locationNTS,
                            GenericTableName = TableLocation,
                            GenericID = loc.LocationID,
                            ParentID = loc.MetaID,
                            isValid = loc.isValid,
                            Date = loc.LocationTimestamp.Substring(0, 10)
                        });
                    }

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
                        drillsFN.Add(new FieldNote
                        {
                            Display_text_1 = dr.DrillAliasLight,
                            Display_text_2 = dr.DrillCompany,
                            Display_text_3 = dr.DrillType,
                            GenericTableName = TableDrillHoles,
                            GenericID = dr.DrillID,
                            ParentID = dr.DrillLocationID,
                            isValid = dr.isValid
                        });
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
                        lineworksFN.Add(new FieldNote
                        {
                            Display_text_1 = ln.LineAliasLight,
                            Display_text_2 = ln.LineType,
                            Display_text_3 = ln.LineNotes,
                            GenericTableName = TableLinework,
                            GenericID = ln.LineID,
                            ParentID = ln.LineMetaID,
                            isValid = ln.isValid
                        });
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
            if (inDate != string.Empty)
            {
                //Update selection on UI
                _selectedDate = inDate;

                //Clean first
                foreach (TableNames tn in FieldNotes.Keys)
                {
                    FieldNotes[tn] = new ObservableCollection<FieldNote>();
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

                OnPropertyChanged(nameof(SelectedDate));

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
            SQLiteAsyncConnection currentConnection = new SQLiteAsyncConnection(da.PreferedDatabasePath);
            List<Task> tasks = new List<Task>();

            switch (tableToUpdate)
            {
                case TableNames.meta:
                    //Special case, this will trigger a whole field note page refresh
                    //Best used when a delete cascade has been done and and child should be removed from page
                    tasks.Add(FillFieldNotesAsync());
                    break;
                case TableNames.location:
                    tasks.Add(FillLocationNotes(currentConnection));
                    break;
                case TableNames.station:
 
                    tasks.Add(FillLocationNotes(currentConnection));
                    tasks.Add(FillStationNotes(currentConnection));
                    
                    break;

                case TableNames.earthmat:
                    tasks.Add(FillLocationNotes(currentConnection));
                    tasks.Add(FillStationNotes(currentConnection));
                    tasks.Add(FillEMNotes(currentConnection));

                    break;
                case TableNames.sample:
                    tasks.Add(FillLocationNotes(currentConnection));
                    tasks.Add(FillStationNotes(currentConnection));
                    tasks.Add(FillEMNotes(currentConnection));
                    tasks.Add(FillSampleNotes(currentConnection));

                    break;
                case TableNames.mineralization:
                    tasks.Add(FillLocationNotes(currentConnection));
                    tasks.Add(FillStationNotes(currentConnection));
                    tasks.Add(FillMineralizationAlterationNotes(currentConnection));
                    break;
                case TableNames.mineral:
                    tasks.Add(FillLocationNotes(currentConnection));
                    tasks.Add(FillStationNotes(currentConnection));
                    tasks.Add(FillEMNotes(currentConnection));
                    tasks.Add(FillMineralizationAlterationNotes(currentConnection));
                    tasks.Add(FillMineralNotes(currentConnection));
                    break;
                case TableNames.document:
                    tasks.Add(FillLocationNotes(currentConnection));
                    tasks.Add(FillStationNotes(currentConnection));
                    tasks.Add(FillDocumentNotes(currentConnection));
                    break;
                case TableNames.structure:
                    tasks.Add(FillLocationNotes(currentConnection));
                    tasks.Add(FillStationNotes(currentConnection));
                    tasks.Add(FillEMNotes(currentConnection));
                    tasks.Add(FillStructureNotes(currentConnection));
                    break;
                case TableNames.fossil:
                    tasks.Add(FillLocationNotes(currentConnection));
                    tasks.Add(FillStationNotes(currentConnection));
                    tasks.Add(FillEMNotes(currentConnection));
                    tasks.Add(FillFossilNotes(currentConnection));
                    break;
                case TableNames.environment:
                    tasks.Add(FillLocationNotes(currentConnection));
                    tasks.Add(FillStationNotes(currentConnection));
                    tasks.Add(FillEnvironmentNotes(currentConnection));
                    break;
                case TableNames.pflow:
                    tasks.Add(FillLocationNotes(currentConnection));
                    tasks.Add(FillStationNotes(currentConnection));
                    tasks.Add(FillEMNotes(currentConnection));
                    tasks.Add(FillPaleoflowNotes(currentConnection));
                    break;
                case TableNames.drill:
                    tasks.Add(FillLocationNotes(currentConnection));
                    tasks.Add(FillDrillHoleNotes(currentConnection));
                    break;
                case TableNames.linework:
                    tasks.Add(FillLineworkNotes(currentConnection));
                    break;
                default:
                    tasks.Add(FillLocationNotes(currentConnection));
                    tasks.Add(FillStationNotes(currentConnection));
                    break;
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
            await currentConnection.CloseAsync(); 
            

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
                await FillFieldNotesAsync();
            }

        }

        #endregion

    }
}
