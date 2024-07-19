using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GSCFieldApp.Models;
using GSCFieldApp.Services;
using GSCFieldApp.Services.DatabaseServices;
using GSCFieldApp.Views;
using SQLite;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Xml.Linq;
using static GSCFieldApp.Dictionaries.DatabaseLiterals;

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
            //Make sure to run update only if a human wants it...
            //TODO: find out why this is triggered, at worst 3 times, in a row with all
            // the last values
            TimeSpan updateSpan = DateTime.Now - lastTimeTableUpdate;
            if (updateSpan.TotalSeconds > 2)
            {
                lastTimeTableUpdate = DateTime.Now;
                UpdateRecordList(_updateTable);
            }
        }

        private bool _isStationVisible = true;
        public bool IsStationVisible 
        {
            get { return Preferences.Get(nameof(IsStationVisible), true); }
            set { Preferences.Set(nameof(IsStationVisible), value); }
        }

        private bool _isEarthMatVisible = false;
        public bool IsEarthMatVisible 
        {
            get { return Preferences.Get(nameof(IsEarthMatVisible), false); }
            set { Preferences.Set(nameof(IsEarthMatVisible), value); }
        }

        private bool _isSampleVisible = false;
        public bool IsSampleVisible 
        {
            get { return Preferences.Get(nameof(IsSampleVisible), false); }
            set { Preferences.Set(nameof(IsSampleVisible), value); }
        }

        private bool _isDocumentVisible = false;
        public bool IsDocumentVisible
        {
            get { return Preferences.Get(nameof(IsDocumentVisible), false); }
            set { Preferences.Set(nameof(IsDocumentVisible), value); }
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


        private ObservableCollection<string> _dates = new ObservableCollection<string>();
        public ObservableCollection<string> Dates
        {
            get { return _dates; }
            set { _dates = value; }
        }

        #endregion

        public FieldNotesViewModel()
        {
            //Init notes
            FieldNotes.Add(TableNames.station, new ObservableCollection<FieldNote>());
            FieldNotes.Add(TableNames.earthmat, new ObservableCollection<FieldNote>());
            FieldNotes.Add(TableNames.sample, new ObservableCollection<FieldNote>());
            FieldNotes.Add(TableNames.document, new ObservableCollection<FieldNote>());

            _dates = new ObservableCollection<string>();

            //Init all records
            _ =  FillFieldNotesAsync();
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
                    IsStationVisible = !IsStationVisible; 
                    OnPropertyChanged(nameof(IsStationVisible));
                }

                if (inComingName.ToLower().Contains(KeywordEarthmat))
                {
                    IsEarthMatVisible = !IsEarthMatVisible;
                    OnPropertyChanged(nameof(IsEarthMatVisible));
                }

                if (inComingName.ToLower().Contains(KeywordSample))
                {
                    IsSampleVisible = !IsSampleVisible;
                    OnPropertyChanged(nameof(IsSampleVisible));
                }

                if (inComingName.ToLower().Contains(nameof(TableNames.document)))
                {
                    IsDocumentVisible = !IsDocumentVisible;
                    OnPropertyChanged(nameof(IsDocumentVisible));
                }

                if (inComingName.ToLower().Contains(KeywordDates))
                {
                    foreach (KeyValuePair<TableNames, ObservableCollection<FieldNote>> item in FieldNotesAll)
                    {
                        FieldNotes[item.Key] = item.Value;
                    }

                    OnPropertyChanged(nameof(Stations));
                    OnPropertyChanged(nameof(EarthMats));
                    OnPropertyChanged(nameof(Samples));
                    OnPropertyChanged(nameof(Documents));
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
                    await Shell.Current.GoToAsync($"{nameof(StationPage)}/",
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
                    await Shell.Current.GoToAsync($"{nameof(EarthmatPage)}/",
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
                    await Shell.Current.GoToAsync($"{nameof(SamplePage)}/",
                        new Dictionary<string, object>
                        {
                            [nameof(Sample)] = (Sample)tappedSample[0],
                            [nameof(Earthmaterial)] = null,
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
                    await Shell.Current.GoToAsync($"{nameof(DocumentPage)}/",
                        new Dictionary<string, object>
                        {
                            [nameof(Station)] = null,
                            [nameof(Document)] = (Document)tappedDoc[0],
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

                await FillTraverseDates(currentConnection);
                await FillStationNotes(currentConnection);
                await FillEMNotes(currentConnection);
                await FillSampleNotes(currentConnection);
                await FillDocumentNotes(currentConnection);

                await currentConnection.CloseAsync();

                OnPropertyChanged(nameof(FieldNotes));
                OnPropertyChanged(nameof(Dates));

                //Make a copy in case user wants to refilter values
                FieldNotesAll = new Dictionary<TableNames, ObservableCollection<FieldNote>>(FieldNotes);

                //Filter out latest date
                //TODO uncomment if really needed
                //if (Dates != null && Dates.Count > 0)
                //{
                //    await FilterRecordsOnDate(Dates.First());
                //}
                
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
            List<DrillHole> drills = await inConnection.QueryAsync<DrillHole>(string.Format("select distinct({0}) from {1} order by {0} desc",
                FieldDrillRelogDate, TableDrillHoles));

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

            if (drills != null && drills.Count > 0)
            {
                foreach (DrillHole dr in drills)
                {

                    string dDate = LocalizationResourceManager["FieldNotesEmptyDate"].ToString();

                    if (dr.DrillRelogDate != null && dr.DrillRelogDate != string.Empty)
                    {
                        dDate = dr.DrillRelogDate;
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

            if (stations != null && stations.Count > 0)
            {
                
                foreach (Station st in stations)
                {
                    FieldNotes[TableNames.station].Add(new FieldNote
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

            OnPropertyChanged(nameof(FieldNotes)); 

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
                OnPropertyChanged(nameof(FieldNotes));
            }

            //Get all em from database
            List<Earthmaterial> ems = await inConnection.Table<Earthmaterial>().OrderBy(x=>x.EarthMatName).ToListAsync();

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
                    FieldNotes[TableNames.earthmat].Add(new FieldNote
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

            OnPropertyChanged(nameof(FieldNotes));

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
                OnPropertyChanged(nameof(FieldNotes));

            }

            //Get all stations from database
            List<Sample> samples = await inConnection.Table<Sample>().OrderBy(s => s.SampleName).ToListAsync();

            if (samples != null && samples.Count > 0)
            {

                foreach (Sample st in samples)
                {
                    FieldNotes[TableNames.sample].Add(new FieldNote
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

            OnPropertyChanged(nameof(FieldNotes));

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
                OnPropertyChanged(nameof(FieldNotes));

            }

            //Get all documents from database
            List<Document> docs = await inConnection.Table<Document>().OrderBy(s => s.DocumentName).ToListAsync();

            if (docs != null && docs.Count > 0)
            {

                foreach (Document dc in docs)
                {
                    FieldNotes[TableNames.document].Add(new FieldNote
                    {
                        Display_text_1 = dc.DocumentAliasLight,
                        Display_text_2 = dc.Category,
                        Display_text_3 = dc.Description,
                        GenericTableName = TableDocument,
                        GenericID = dc.DocumentID,
                        ParentID = dc.StationID.Value,
                        isValid = dc.isValid
                    });
                }

            }

            OnPropertyChanged(nameof(FieldNotes));

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
                //Start with station
                if (FieldNotesAll.ContainsKey(TableNames.station))
                {
                    FieldNotes[TableNames.station] = new ObservableCollection<FieldNote>(FieldNotesAll[TableNames.station].Where(x => x.Date == inDate).ToList());
                    OnPropertyChanged(nameof(Stations));
                    //Children
                    if (FieldNotesAll.ContainsKey(TableNames.earthmat))
                    {
                        List<int> stationIds = new List<int>();
                        foreach (FieldNote sids in FieldNotes[TableNames.station])
                        {
                            stationIds.Add(sids.GenericID);
                        }

                        FieldNotes[TableNames.earthmat] = new ObservableCollection<FieldNote>(FieldNotesAll[TableNames.earthmat].Where(x => stationIds.Contains(x.ParentID)).ToList());
                        OnPropertyChanged(nameof(EarthMats));
                    }
                }
            }

        }

        /// <summary>
        /// Will refresh a desire table in the field notes
        /// This should be triggered when user is coming from a form, either
        /// adding items or editing them
        /// </summary>
        public async void UpdateRecordList(TableNames tableToUpdate)
        {
            //Detect if 
            if (tableToUpdate != TableNames.meta)
            {
                SQLiteAsyncConnection currentConnection = new SQLiteAsyncConnection(da.PreferedDatabasePath);
                
                switch (tableToUpdate)
                {
                    case TableNames.meta:
                        break;
                    case TableNames.location:
                        break;
                    case TableNames.station:
                        await FillStationNotes(currentConnection);
                        break;
                    case TableNames.earthmat:
                        await FillEMNotes(currentConnection);
                        break;
                    case TableNames.sample:
                        await FillSampleNotes(currentConnection);
                        break;
                    case TableNames.mineralization:
                        break;
                    case TableNames.mineral:
                        break;
                    case TableNames.document:
                        await FillDocumentNotes(currentConnection);
                        break;
                    case TableNames.structure:
                        break;
                    case TableNames.fossil:
                        break;
                    case TableNames.environment:
                        break;
                    case TableNames.pflow:
                        break;
                    case TableNames.drill:
                        break;
                    default:
                        await FillStationNotes(currentConnection);
                        break;
                }

                await currentConnection.CloseAsync(); 
            }

        }

        #endregion

    }
}
