using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GSCFieldApp.Dictionaries;
using GSCFieldApp.Models;
using GSCFieldApp.Services.DatabaseServices;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSCFieldApp.ViewModel
{
    public partial class FieldNotesViewModel : ObservableObject
    {

        DataAccess da = new DataAccess();

        #region PROPERTIES

        private bool _isStationVisible = true;
        public bool IsStationVisible { get { return _isStationVisible; } set { _isStationVisible = value; } }

        private bool _isEarthMatVisible = true;
        public bool IsEarthMatVisible { get { return _isEarthMatVisible; } set { _isEarthMatVisible = value; } }

        private bool _isSampleVisible = true;
        public bool IsSampleVisible { get { return _isSampleVisible; } set { _isSampleVisible = value; } }

        public List<FieldNoteGroup> FieldNotes { get; private set; } = new List<FieldNoteGroup>();

        private List<FieldNote> _stations = new List<FieldNote>();
        public List<FieldNote> Stations
        {
            get
            {
                IEnumerable<FieldNoteGroup> s = FieldNotes.Where(p => p.Name == "Station");
                if (s != null)
                {
                    return _stations = FieldNotes.Where(p => p.Name == "Station").ToList()[0];
                }
                else
                {
                    return _stations = new List<FieldNote>();
                }

            }
            set { _stations = value; }
        }
        private List<FieldNote> _earthmats = new List<FieldNote>();
        public List<FieldNote> EarthMats
        { 

            get
            {
                IEnumerable<FieldNoteGroup> s = FieldNotes.Where(p => p.Name == "Earth Material");
                if (s != null)
                {
                    return _earthmats = FieldNotes.Where(p => p.Name == "Earth Material").ToList()[0];
                }
                else
                {
                    return _earthmats = new List<FieldNote>();
                }

            }
            set { _earthmats = value; }
        }

        private List<FieldNote> _samples = new List<FieldNote>();
        public List<FieldNote> Samples
        {

            get
            {
                IEnumerable<FieldNoteGroup> s = FieldNotes.Where(p => p.Name == "Sample");
                if (s != null)
                {
                    return _earthmats = FieldNotes.Where(p => p.Name == "Sample").ToList()[0];
                }
                else
                {
                    return _earthmats = new List<FieldNote>();
                }

            }
            set { _earthmats = value; }
        }

        public Dictionary<string, ObservableCollection<FieldNote>> FieldNotes2 =  new Dictionary<string, ObservableCollection<FieldNote>>();
        private ObservableCollection<FieldNote> _stations2 = new ObservableCollection<FieldNote>();
        public ObservableCollection<FieldNote> Stations2
        {
            get
            {
                if (FieldNotes2.ContainsKey(DatabaseLiterals.TableStation))
                {
                    return _stations2 = FieldNotes2[DatabaseLiterals.TableStation] ;
                }
                else
                {
                    return _stations2 = new ObservableCollection<FieldNote>();
                }

            }
            set { _stations2 = value; }
        }

        #endregion

        public FieldNotesViewModel()
        {
            FieldNotes2.Add(DatabaseLiterals.TableStation, new ObservableCollection<FieldNote>());
            FieldNotes.Add(new FieldNoteGroup("Station", new List<FieldNote>
            {
                new FieldNote
                {
                    Display_text_1 = "Outcrop",
                    Display_text_2 = "Nice Paysage",

                },
                new FieldNote
                {
                    Display_text_1 = "Bosquet",
                    Display_text_2 = "With grey rocks",

                },

            }));

            FieldNotes.Add(new FieldNoteGroup("Earth Material", new List<FieldNote>
            {
                new FieldNote
                {
                    Display_text_1 = "Granite",
                    Display_text_2 = "Black",

                },
                new FieldNote
                {
                    Display_text_1 = "Quartz",
                    Display_text_2 = "Pink",

                },
                new FieldNote
                {
                    Display_text_1 = "Dolomite",
                    Display_text_2 = "More like Dynamite",

                },
            }));

            FieldNotes.Add(new FieldNoteGroup("Sample", new List<FieldNote> { }));

            if (da.PreferedDatabasePath != string.Empty)
            {
                FillFieldNotesAsync();
            }

        }

        #region RELAY

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

        #endregion

        #region METHODS

        public async Task FillFieldNotesAsync()
        {
            if (da.PreferedDatabasePath != null && da.PreferedDatabasePath != string.Empty  )
            {
                SQLiteAsyncConnection currentConnection = new SQLiteAsyncConnection(da.PreferedDatabasePath);

                await FillStationNotes(currentConnection);

                await currentConnection.CloseAsync();

                OnPropertyChanged(nameof(FieldNotes2));
                OnPropertyChanged(nameof(Stations2));
            }

        }

        public async Task FillStationNotes(SQLiteAsyncConnection inConnection)
        {
            //Init a station group
            if (!FieldNotes2.ContainsKey(DatabaseLiterals.TableStation))
            {
                FieldNotes2.Add(DatabaseLiterals.TableStation, new ObservableCollection<FieldNote>());
            }
            else 
            {
                //Clear whatever was in there first.
                FieldNotes2[DatabaseLiterals.TableStation].Clear();

            }

            //Get all stations from database
            List<Station> stations = await inConnection.QueryAsync<Station>("SELECT * FROM " + DatabaseLiterals.TableStation + ";");

            if (stations != null && stations.Count > 0)
            {
                

                foreach (Station st in stations)
                {
                    FieldNotes2[DatabaseLiterals.TableStation].Add(new FieldNote
                    {
                        Display_text_1 = st.StationAlias,
                        Display_text_2 = st.StationObsType,
                        Display_text_3 = st.StationNote
                    });
                }

            }

        }

        #endregion
    }
}
