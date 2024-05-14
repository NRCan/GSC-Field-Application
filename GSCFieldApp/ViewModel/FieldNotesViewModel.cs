using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GSCFieldApp.Dictionaries;
using GSCFieldApp.Models;
using GSCFieldApp.Services;
using GSCFieldApp.Services.DatabaseServices;
using GSCFieldApp.Views;
using SQLite;
using System.Collections.ObjectModel;

namespace GSCFieldApp.ViewModel
{
    public partial class FieldNotesViewModel : ObservableObject
    {
        //Localization
        public LocalizationResourceManager LocalizationResourceManager
            => LocalizationResourceManager.Instance; // Will be used for in code dynamic local strings

        //Database
        DataAccess da = new DataAccess();

        //Database schema
        private enum Tables { station, em, sample } //Will be used as dictionary key or other

        //Records
        private Dictionary<Tables, ObservableCollection<FieldNote>> FieldNotes = new Dictionary<Tables, ObservableCollection<FieldNote>>(); //Used to populate records in each headers
        private Dictionary<Tables, ObservableCollection<FieldNote>> FieldNotesAll = new Dictionary<Tables, ObservableCollection<FieldNote>>(); //Safe variable for refiltering datasets

        #region PROPERTIES

        private bool _isStationVisible = true;
        public bool IsStationVisible { get { return _isStationVisible; } set { _isStationVisible = value; } }

        private bool _isEarthMatVisible = true;
        public bool IsEarthMatVisible { get { return _isEarthMatVisible; } set { _isEarthMatVisible = value; } }

        private bool _isSampleVisible = true;
        public bool IsSampleVisible { get { return _isSampleVisible; } set { _isSampleVisible = value; } }

        private ObservableCollection<FieldNote> _earthmats = new ObservableCollection<FieldNote>();
        public ObservableCollection<FieldNote> EarthMats
        { 

            get
            {
                if (FieldNotes.ContainsKey(Tables.em))
                {
                    return _earthmats = FieldNotes[Tables.em];
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
                if (FieldNotes.ContainsKey(Tables.sample))
                {
                    return _samples = FieldNotes[Tables.sample];
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
                if (FieldNotes.ContainsKey(Tables.station))
                {
                    return _stations = FieldNotes[Tables.station] ;
                }
                else
                {
                    return _stations = new ObservableCollection<FieldNote>();
                }

            }
            set { _stations = value; }
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
            FieldNotes.Add(Tables.station, new ObservableCollection<FieldNote>());
            FieldNotes.Add(Tables.em, new ObservableCollection<FieldNote>());
            FieldNotes.Add(Tables.sample, new ObservableCollection<FieldNote>());
            _dates = new ObservableCollection<string>();

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
                if (inComingName.ToLower().Contains(DatabaseLiterals.KeywordStation))
                {
                    IsStationVisible = !IsStationVisible; 
                    OnPropertyChanged(nameof(IsStationVisible));
                }

                if (inComingName.ToLower().Contains(DatabaseLiterals.KeywordEarthmat))
                {
                    IsEarthMatVisible = !IsEarthMatVisible;
                    OnPropertyChanged(nameof(IsEarthMatVisible));
                }

                if (inComingName.ToLower().Contains(DatabaseLiterals.KeywordSample))
                {
                    IsSampleVisible = !IsSampleVisible;
                    OnPropertyChanged(nameof(IsSampleVisible));
                }

                if (inComingName.ToLower().Contains(DatabaseLiterals.KeywordDates))
                {
                    foreach (KeyValuePair<Tables, ObservableCollection<FieldNote>> item in FieldNotesAll)
                    {
                        FieldNotes[item.Key] = item.Value;
                    }

                    OnPropertyChanged(nameof(Stations));
                    OnPropertyChanged(nameof(EarthMats));
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
            if (fieldNotes.GenericTableName == DatabaseLiterals.TableStation)
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

            if (fieldNotes.GenericTableName == DatabaseLiterals.TableEarthMat)
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
        }

        /// <summary>
        /// Will initiate a delete sequence from the tapped field note card/item
        /// </summary>
        /// <param name="fieldNotes"></param>
        /// <returns></returns>
        [RelayCommand]
        public async Task SwipeGestureRecognizer(FieldNote fieldNotes)
        {
            //Display a prompt with an answer to prevent butt or fat finger deleting stations.
            string answer = await Shell.Current.DisplayPromptAsync("Delete " + fieldNotes.Display_text_1, "Enter last two digit of current year to delete" , "DELETE", "CANCEL");

            if (answer == DateTime.Now.Year.ToString().Substring(2)) 
            {
                if (fieldNotes.GenericTableName == DatabaseLiterals.TableStation)
                {
                    FieldLocation stationToDelete = new FieldLocation();
                    stationToDelete.LocationID = fieldNotes.ParentID;
                    int numberOfDeletedRows = await da.DeleteItemAsync(stationToDelete);

                    if (numberOfDeletedRows == 1)
                    {
                        //Refil note page
                        await FillFieldNotesAsync();

                        //Show final messag to user
                        await Shell.Current.DisplayAlert("Delete Completed", "Record " + fieldNotes.Display_text_1 + " has been deleted.", "OK");
                    }
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

                await currentConnection.CloseAsync();

                OnPropertyChanged(nameof(FieldNotes));
                OnPropertyChanged(nameof(Dates));

                //Make a copy in case user wants to refilter values
                FieldNotesAll = new Dictionary<Tables, ObservableCollection<FieldNote>>(FieldNotes);

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

            //Get all dates from key tables
            List<Station> stats = await inConnection.QueryAsync<Station>(string.Format("select distinct({0}) from {1} order by {0} desc", 
                DatabaseLiterals.FieldStationVisitDate, DatabaseLiterals.TableStation));
            List<DrillHole> drills = await inConnection.QueryAsync<DrillHole>(string.Format("select distinct({0}) from {1} order by {0} desc",
                DatabaseLiterals.FieldDrillRelogDate, DatabaseLiterals.TableDrillHoles));

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
            if (!FieldNotes.ContainsKey(Tables.station))
            {
                FieldNotes.Add(Tables.station, new ObservableCollection<FieldNote>());
            }
            else 
            {
                //Clear whatever was in there first.
                FieldNotes[Tables.station].Clear();

            }

            //Get all stations from database
            List<Station> stations = await inConnection.Table<Station>().OrderBy(s => s.StationAlias).ToListAsync();

            if (stations != null && stations.Count > 0)
            {
                

                foreach (Station st in stations)
                {
                    FieldNotes[Tables.station].Add(new FieldNote
                    {
                        Display_text_1 = st.StationAliasLight,
                        Display_text_2 = st.StationObsType,
                        Display_text_3 = st.StationNote,
                        GenericTableName = DatabaseLiterals.TableStation,
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
            if (!FieldNotes.ContainsKey(Tables.em))
            {
                FieldNotes.Add(Tables.em, new ObservableCollection<FieldNote>());
            }
            else
            {
                //Clear whatever was in there first.
                FieldNotes[Tables.em].Clear();

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
                    FieldNotes[Tables.em].Add(new FieldNote
                    {
                        
                        Display_text_1 = st.EarthmatAliasLight,
                        Display_text_2 = st.EarthMatLithdetail,
                        Display_text_3 = st.EarthMatLithgroup,
                        GenericTableName = DatabaseLiterals.TableEarthMat,
                        GenericID = st.EarthMatID,
                        ParentID = parentID,
                        isValid = st.isValid
                    });
                }

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
                //Start with station
                if (FieldNotesAll.ContainsKey(Tables.station))
                {
                    FieldNotes[Tables.station] = new ObservableCollection<FieldNote>(FieldNotesAll[Tables.station].Where(x => x.Date == inDate).ToList());
                    OnPropertyChanged(nameof(Stations));
                    //Children
                    if (FieldNotesAll.ContainsKey(Tables.em))
                    {
                        List<int> stationIds = new List<int>();
                        foreach (FieldNote sids in FieldNotes[Tables.station])
                        {
                            stationIds.Add(sids.GenericID);
                        }

                        FieldNotes[Tables.em] = new ObservableCollection<FieldNote>(FieldNotesAll[Tables.em].Where(x => stationIds.Contains(x.ParentID)).ToList());
                        OnPropertyChanged(nameof(EarthMats));
                    }
                }
            }

        }

        #endregion

    }
}
