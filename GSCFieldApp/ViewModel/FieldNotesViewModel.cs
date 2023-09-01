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
using Microsoft.Maui.Controls;
using GSCFieldApp.Views;

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

        #endregion

        public FieldNotesViewModel()
        {
            //Init notes
            FieldNotes.Add(DatabaseLiterals.TableStation, new ObservableCollection<FieldNote>());
            FieldNotes.Add(DatabaseLiterals.TableEarthMat, new ObservableCollection<FieldNote>());
            FieldNotes.Add(DatabaseLiterals.TableSample, new ObservableCollection<FieldNote>());

            if (da.PreferedDatabasePath != string.Empty)
            {
                _ = FillFieldNotesAsync();
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

        [RelayCommand]
        public async Task TapGestureRecognizer(FieldNote fieldNotes)
        {
            //Get tapped record
            SQLiteAsyncConnection currentConnection = da.GetConnectionFromPath(da.PreferedDatabasePath);

            //Get metadata records
            List<Station> tappedStation = await currentConnection.Table<Station>().Where(i => i.StationAlias == fieldNotes.Display_text_1).ToListAsync();

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

            //await Shell.Current.DisplayAlert("Alert", "You have tapped: " + fieldNotes.Display_text_1 , "OK");
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

                await FillStationNotes(currentConnection);

                await currentConnection.CloseAsync();

                OnPropertyChanged(nameof(FieldNotes));
                OnPropertyChanged(nameof(Stations));
            }

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
            List<Station> stations = await inConnection.QueryAsync<Station>("SELECT * FROM " + DatabaseLiterals.TableStation + ";");

            if (stations != null && stations.Count > 0)
            {
                

                foreach (Station st in stations)
                {
                    FieldNotes[DatabaseLiterals.TableStation].Add(new FieldNote
                    {
                        Display_text_1 = st.StationAlias,
                        Display_text_2 = st.StationObsType,
                        Display_text_3 = st.StationNote,
                        GenericTableName = DatabaseLiterals.TableStation,
                        GenericID = st.StationID,
                        ParentID = st.LocationID
                    });
                }

            }

        }

        #endregion

    }
}
