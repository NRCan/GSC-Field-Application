using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using GSCFieldApp.Services.DatabaseServices;
using GSCFieldApp.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.ObjectModel;
using GSCFieldApp.Models;
using Microsoft.Maui.Controls.PlatformConfiguration;
using SQLite;
using System.Resources;
using GSCFieldApp.Dictionaries;
using BruTile.Wmts.Generated;
using GSCFieldApp.Themes;
using NetTopologySuite.Index.HPRtree;

namespace GSCFieldApp.ViewModel
{
    public partial class FieldBooksViewModel: ObservableObject
    {
        //UI
        private bool _noFieldBookWatermark = false;
        public ObservableCollection<FieldBooks> _fieldbookCollection = new ObservableCollection<FieldBooks>();
        public FieldBooks _selectedFieldBook;

        //Data service
        private DataAccess da = new DataAccess();

        //Models
        readonly Station stationModel = new Station();
        readonly Metadata metadataModel = new Metadata();

        public bool NoFieldBookWatermark { get { return _noFieldBookWatermark; } set { _noFieldBookWatermark = value; } }
        public ObservableCollection<FieldBooks> FieldbookCollection { get { return _fieldbookCollection; } set{ _fieldbookCollection = value; } }
        public FieldBooks SelectedFieldBook { get { return _selectedFieldBook; } set { _selectedFieldBook = value; } }

        public FieldBooksViewModel() 
        {
            //Fill list view of projects
            FillBookCollectionAsync();

            //Detect new field book save
            FieldBookPage.newFieldBookSaved += FieldBookDialog_newFieldBookSaved;

        }

        #region EVENTS
        private void FieldBookDialog_newFieldBookSaved(object sender, EventArgs e)
        {
            FillBookCollectionAsync();

        }

        #endregion

        #region RELAY COMMANDS

        [RelayCommand]
        async Task AddFieldBook()
        {
            await Shell.Current.GoToAsync($"{nameof(FieldBookPage)}");
        }

        [RelayCommand]
        async Task UploadFieldBook()
        {
            await Shell.Current.DisplayAlert("Alert", "Not yet implemented", "OK");
        }

        [RelayCommand]
        async Task DownloadFieldBook()
        {
            await Shell.Current.DisplayAlert("Alert", "Not yet implemented", "OK");
        }

        [RelayCommand]
        async Task UpdateFieldBook()
        {
            await Shell.Current.DisplayAlert("Alert", "Not yet implemented", "OK");
        }

        #endregion

        #region METHODS


        /// <summary>
        /// Will fill the project collection with information related to it
        /// </summary>
        public async void FillBookCollectionAsync()
        {
            _fieldbookCollection.Clear();

            List<string> invalidFieldBookToDelete = new List<string>();

            //Iterate through local state folder
            string[] fileList = Directory.GetFiles(FileSystem.Current.AppDataDirectory);

            foreach (string sf in fileList)
            {
                FileInfo fi = new FileInfo(sf);

                //Check if file size is higher then 0
                if (fi.Length > 0) 
                {
                    //Get the databases but not the main default one
                    if ((sf.Contains(DatabaseLiterals.DBTypeSqlite) || sf.Contains(DatabaseLiterals.DBTypeSqliteDeprecated))
                        && !sf.Contains(DatabaseLiterals.DBName))
                    {

                        //Connect to found database and retrive some information from it
                        FieldBooks currentBook = new FieldBooks();
                        SQLiteAsyncConnection currentConnection = da.GetConnectionFromPath(sf);

                        //Get metadata records
                        List<Metadata> metadataTableRows = await currentConnection.Table<Metadata>()?.ToListAsync();
                        foreach (Metadata m in metadataTableRows)
                        {
                            //For metadata
                            Metadata met = m as Metadata;
                            currentBook.CreateDate = met.StartDate;
                            currentBook.GeologistGeolcode = met.Geologist + "[" + met.UserCode + "]";
                            currentBook.ProjectPath = FileSystem.Current.AppDataDirectory;
                            currentBook.ProjectDBPath = sf;
                            currentBook.metadataForProject = m as Metadata;

                            //Manage to select last prefered fieldbook
                            string preferedDBPath = Preferences.Get(ApplicationLiterals.preferenceDatabasePath, string.Empty);
                            if (currentBook.ProjectDBPath == preferedDBPath)
                            {
                                _selectedFieldBook = currentBook;
                                OnPropertyChanged(nameof(SelectedFieldBook));
                            }
                        }

                        //For stations
                        string stationQuerySelect = "SELECT *";
                        string stationQueryFrom = " FROM " + DatabaseLiterals.TableStation;
                        string stationQueryWhere = " WHERE " + DatabaseLiterals.TableStation + "." + DatabaseLiterals.FieldStationAlias + " NOT LIKE '%" + DatabaseLiterals.KeywordStationWaypoint + "%'";
                        string stationQueryFinal = stationQuerySelect + stationQueryFrom + stationQueryWhere;
                        List<Station> stationCountResult = await currentConnection.Table<Station>().ToListAsync();
                        if (stationCountResult != null && stationCountResult.Count > 0)
                        {
                            currentBook.StationNumber = stationCountResult.Count.ToString();
                        }
                        else if (stationCountResult != null && stationCountResult.Count == 0)
                        {
                            currentBook.StationNumber = "0";
                        }
                        else
                        {
                            currentBook.StationNumber = "?";
                        }
                        if (stationCountResult.Count != 0)
                        {
                            Station lastStation = (Station)stationCountResult[stationCountResult.Count - 1];
                            currentBook.StationLastEntered = lastStation.StationAlias;
                        }

                        _fieldbookCollection.Add(currentBook);
                        OnPropertyChanged(nameof(FieldbookCollection));
                        await currentConnection.CloseAsync();

                    }
                }
                else 
                {
                    //Make sure to remove empty file
                    //This could happen if something went wrong with a database save
                    fi.Delete();
                }

                
            }
            
            

            WatermarkValidation();
        }

        public void WatermarkValidation()
        {
            if (_fieldbookCollection.Count == 0)
            {
                _noFieldBookWatermark = true;
            }
            else
            {
                _noFieldBookWatermark = false;

            }

            OnPropertyChanged(nameof(NoFieldBookWatermark));
        }

        #endregion
    }
}
