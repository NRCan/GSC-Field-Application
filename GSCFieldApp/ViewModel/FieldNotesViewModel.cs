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

        DataAccess da = new DataAccess();

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
                if (FieldNotes.ContainsKey(DatabaseLiterals.TableEarthMat))
                {
                    return _earthmats = FieldNotes[DatabaseLiterals.TableEarthMat];
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
                if (FieldNotes.ContainsKey(DatabaseLiterals.TableSample))
                {
                    return _samples = FieldNotes[DatabaseLiterals.TableSample];
                }
                else
                {
                    return _samples = new ObservableCollection<FieldNote>();
                }

            }
            set { _samples = value; }
        }

        public Dictionary<string, ObservableCollection<FieldNote>> FieldNotes =  new Dictionary<string, ObservableCollection<FieldNote>>();
        private ObservableCollection<FieldNote> _stations = new ObservableCollection<FieldNote>();
        public ObservableCollection<FieldNote> Stations
        {
            get
            {
                if (FieldNotes.ContainsKey(DatabaseLiterals.TableStation))
                {
                    return _stations = FieldNotes[DatabaseLiterals.TableStation] ;
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
            FieldNotes.Add(DatabaseLiterals.TableStation, new ObservableCollection<FieldNote>());
            FieldNotes.Add(DatabaseLiterals.TableEarthMat, new ObservableCollection<FieldNote>());
            FieldNotes.Add(DatabaseLiterals.TableSample, new ObservableCollection<FieldNote>());
            _dates = new ObservableCollection<string>();

        }

        #region RELAY

        [RelayCommand]
        async Task TapDateGestureRecognizer(string incomingDate)
        {
            //Filter out record based on selected date
            _stations = FieldNotes[DatabaseLiterals.TableStation].Where(i => i.Date == incomingDate) as ObservableCollection<FieldNote>;
            OnPropertyChanged(nameof(Stations));
        }

        /// <summary>
        /// Will reverse whatever is set has visibility on the records after a tap on header
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        async Task Hide(string inComingName)
        {
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

            }

        }

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
                    await Shell.Current.GoToAsync($"{nameof(StationPage)}",
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
                    await Shell.Current.GoToAsync($"{nameof(EarthmatPage)}",
                        new Dictionary<string, object>
                        {
                            [nameof(Earthmaterial)] = (Earthmaterial)tappedEM[0],
                            [nameof(Station)] = null,
                        }
                    );
                }
            }
        }

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

        public async Task FillFieldNotesAsync()
        {
            if (da.PreferedDatabasePath != null && da.PreferedDatabasePath != string.Empty  )
            {
                SQLiteAsyncConnection currentConnection = new SQLiteAsyncConnection(da.PreferedDatabasePath);

                await FillTraverseDates(currentConnection);
                await FillStationNotes(currentConnection);
                await FillEMNotes(currentConnection);

                await currentConnection.CloseAsync();

                OnPropertyChanged(nameof(FieldNotes));
                OnPropertyChanged(nameof(Dates));
            }

        }

        public async Task FillTraverseDates(SQLiteAsyncConnection inConnection)
        {

            //Clear whatever was in there first.
            _dates.Clear();

            //Get all dates from key tables
            List<Station> stats = await inConnection.QueryAsync<Station>(string.Format("select distinct({0}) from {1}", 
                DatabaseLiterals.FieldStationVisitDate, DatabaseLiterals.TableStation));
            List<DrillHole> drills = await inConnection.QueryAsync<DrillHole>(string.Format("select distinct({0}) from {1}",
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

            _dates.Sort();

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
            if (!FieldNotes.ContainsKey(DatabaseLiterals.TableStation))
            {
                FieldNotes.Add(DatabaseLiterals.TableStation, new ObservableCollection<FieldNote>());
            }
            else 
            {
                //Clear whatever was in there first.
                FieldNotes[DatabaseLiterals.TableStation].Clear();

            }

            //Get all stations from database
            List<Station> stations = await inConnection.Table<Station>().OrderBy(s => s.StationAlias).ToListAsync();

            if (stations != null && stations.Count > 0)
            {
                

                foreach (Station st in stations)
                {
                    FieldNotes[DatabaseLiterals.TableStation].Add(new FieldNote
                    {
                        Display_text_1 = st.StationAliasLight,
                        Display_text_2 = st.StationObsType,
                        Display_text_3 = st.StationNote,
                        GenericTableName = DatabaseLiterals.TableStation,
                        GenericID = st.StationID,
                        ParentID = st.LocationID,
                        Date = st.StationVisitDate
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
            if (!FieldNotes.ContainsKey(DatabaseLiterals.TableEarthMat))
            {
                FieldNotes.Add(DatabaseLiterals.TableEarthMat, new ObservableCollection<FieldNote>());
            }
            else
            {
                //Clear whatever was in there first.
                FieldNotes[DatabaseLiterals.TableEarthMat].Clear();

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
                    FieldNotes[DatabaseLiterals.TableEarthMat].Add(new FieldNote
                    {
                        
                        Display_text_1 = st.EarthmatAliasLight,
                        Display_text_2 = st.EarthMatLithdetail,
                        Display_text_3 = st.EarthMatLithgroup,
                        GenericTableName = DatabaseLiterals.TableEarthMat,
                        GenericID = st.EarthMatID,
                        ParentID = parentID
                    });
                }

            }

        }


        #endregion

    }
}
