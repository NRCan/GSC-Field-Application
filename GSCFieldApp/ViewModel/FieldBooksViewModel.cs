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
using GSCFieldApp.Services;
using System.IO.Compression;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Maui.Alerts;

namespace GSCFieldApp.ViewModel
{
    public partial class FieldBooksViewModel: ObservableObject
    {
        //UI
        private bool _noFieldBookWatermark = false;
        public ObservableCollection<FieldBooks> _fieldbookCollection = new ObservableCollection<FieldBooks>();
        public FieldBooks _selectedFieldBook;
        private bool _isWaiting = false; //Waiting indicator

        //Localization
        public LocalizationResourceManager LocalizationResourceManager
            => LocalizationResourceManager.Instance; // Will be used for in code dynamic local strings

        //Data service
        private DataAccess da = new DataAccess();

        //Models
        readonly Station stationModel = new Station();
        readonly Metadata metadataModel = new Metadata();

        #region Properties

        public bool NoFieldBookWatermark { get { return _noFieldBookWatermark; } set { _noFieldBookWatermark = value; } }
        public ObservableCollection<FieldBooks> FieldbookCollection { get { return _fieldbookCollection; } set{ _fieldbookCollection = value; } }
        public FieldBooks SelectedFieldBook { get { return _selectedFieldBook; } set { _selectedFieldBook = value; } }
        public bool IsWaiting { get { return _isWaiting; } set { _isWaiting = value; } }

        #endregion

        public FieldBooksViewModel() 
        {
            //Fill list view of projects
            FillBookCollectionAsync();

        }

        #region RELAY COMMANDS

        [RelayCommand]
        Task FieldBookChanged()
        {
            string preferedDBPath = Preferences.Get(ApplicationLiterals.preferenceDatabasePath, string.Empty);

            if (_selectedFieldBook != null && _selectedFieldBook.ProjectDBPath != preferedDBPath)
            {
                da.PreferedDatabasePath = _selectedFieldBook.ProjectDBPath;

                // Keep in pref project type for futur vocab use and other viewing purposes
                Preferences.Set(nameof(DatabaseLiterals.FieldUserInfoFWorkType), _selectedFieldBook.metadataForProject.FieldworkType);
            }

            return Task.CompletedTask;
        }

        [RelayCommand]
        async Task DeleteFieldBook()
        {
            await PrepareDeleteFieldBook(SelectedFieldBook);
        }

        [RelayCommand]
        async Task AddFieldBook()
        {
            await Shell.Current.GoToAsync($"{nameof(FieldBookPage)}/");
        }

        [RelayCommand]
        async Task UploadFieldBook()
        {
            //Upload
            AppFileServices appFileServices = new AppFileServices();
            string uploadFB = await appFileServices.UploadFieldBook();

            //Refresh list
            if (uploadFB.Contains(DatabaseLiterals.DBTypeSqlite))
            {
                FillBookCollectionAsync();
            }
        }

        [RelayCommand]
        async Task DownloadFieldBook()
        {
            //Backup
            AppFileServices appFileServices = new AppFileServices();
            await appFileServices.BackupFieldBook();
        }

        [RelayCommand]
        async Task UpdateFieldBook()
        {
            await Shell.Current.DisplayAlert("Alert", "Not yet implemented", "OK");
        }

        [RelayCommand]
        public async Task SwipeGestureRecognizer(FieldBooks fieldBook)
        {
            await PrepareDeleteFieldBook(fieldBook);
        }

        [RelayCommand]
        public async Task EditFieldBook()
        {
            //Navigate to fieldbook page and send along the metadata
            if (_selectedFieldBook != null)
            {
                await Shell.Current.GoToAsync($"{nameof(FieldBookPage)}/",
                    new Dictionary<string, object>
                    {
                        [nameof(Metadata)] = _selectedFieldBook.metadataForProject,
                    }
                );
            }
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Enable / disable waiting cursor on map page
        /// </summary>
        /// <param name="isRunning"></param>
        public void SetWaitingCursor(bool isRunning)
        {
            _isWaiting = isRunning;
            OnPropertyChanged(nameof(IsWaiting));
        }

        /// <summary>
        /// Main method to start deleting a field book by making user
        /// really wants to delete it then will ask if a backup is needed.
        /// </summary>
        /// <param name="fieldBook"></param>
        /// <returns></returns>
        public async Task PrepareDeleteFieldBook(FieldBooks fieldBook)
        {
            bool deleteDialogResult = await Shell.Current.DisplayAlert(LocalizationResourceManager["FieldbookPageDeleteTitle"].ToString(),
                LocalizationResourceManager["FieldbookPageDeleteMessage"].ToString(),
                LocalizationResourceManager["GenericButtonYes"].ToString(),
                LocalizationResourceManager["GenericButtonNo"].ToString());

            if (deleteDialogResult)
            {
                //Make extra validation if user wants to backup his data
                ValidateDeleteProject(fieldBook);
            }
        }

        /// <summary>
        /// Will ask user if a field book backup is needed then will proceed with deleting
        /// the selected field book.
        /// </summary>
        /// <param name="sender"></param>
        public async void ValidateDeleteProject(FieldBooks fieldBook)
        {

            bool backupDialogResult = await Shell.Current.DisplayAlert(LocalizationResourceManager["FieldbookPageBackupTitle"].ToString(),
            LocalizationResourceManager["FieldbookPageBackupMessage"].ToString(),
            LocalizationResourceManager["GenericButtonYes"].ToString(),
            LocalizationResourceManager["GenericButtonNo"].ToString());

            if (backupDialogResult)
            {
                //Set progress ring
                SetWaitingCursor(true);

                //Backup
                AppFileServices appFileServices = new AppFileServices();
                await appFileServices.BackupFieldBook();

                //Delete
                DeleteUserFieldBook();

                //Unset progress ring
                SetWaitingCursor(false);
            }
            else
            {
                //Delete
                DeleteUserFieldBook();
            }

        }

        /// <summary>
        /// Will fill the project collection with information related to it
        /// </summary>
        public async void FillBookCollectionAsync()
        {
            _fieldbookCollection.Clear();

            List<string> invalidFieldBookToDelete = new List<string>();

            //Iterate through local state folder
            DirectoryInfo di = new DirectoryInfo(FileSystem.Current.AppDataDirectory);
            FileInfo[] fileList = di.GetFiles().OrderByDescending(i=>i.CreationTime).ToArray();

            foreach (FileInfo fi in fileList)
            {

                //Check if file size is higher then 0
                if (fi.Length > 0) 
                {
                    //Get the databases but not the main default one
                    if ((fi.Extension.Contains(DatabaseLiterals.DBTypeSqlite) || fi.Extension.Contains(DatabaseLiterals.DBTypeSqliteDeprecated))
                        && !fi.Name.Contains(DatabaseLiterals.DBName))
                    {

                        //Connect to found database and retrive some information from it
                        FieldBooks currentBook = new FieldBooks();
                        SQLiteAsyncConnection currentConnection = da.GetConnectionFromPath(fi.FullName);

                        //Get metadata records
                        List<Metadata> metadataTableRows = await currentConnection.Table<Metadata>()?.ToListAsync();
                        foreach (Metadata m in metadataTableRows)
                        {
                            //For metadata
                            Metadata met = m as Metadata;
                            currentBook.CreateDate = met.StartDate;
                            currentBook.GeologistGeolcode = met.Geologist + "[" + met.UserCode + "]";
                            currentBook.ProjectPath = FileSystem.Current.AppDataDirectory;
                            currentBook.ProjectDBPath = fi.FullName;
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

                        if (!_fieldbookCollection.Contains(currentBook))
                        {
                            _fieldbookCollection.Add(currentBook);
                            
                        }

                        await currentConnection.CloseAsync();

                    }
                }
                else 
                {
                    //Make sure to remove empty file
                    //This could happen if something went wrong with a database save
                    try
                    {
                        fi.Delete();
                    }
                    catch (Exception e)
                    {
                        new ErrorToLogFile(e).WriteToFile();
                    }
                    
                }

                
            }

            //Refresh UI
            OnPropertyChanged(nameof(FieldbookCollection));

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

        /// <summary>
        /// Will delete prefered field book and associated files
        /// </summary>
        public void DeleteUserFieldBook()
        {
            string userLocalFolder = Path.GetDirectoryName(da.PreferedDatabasePath);
            string userDBName = Path.GetFileNameWithoutExtension(da.PreferedDatabasePath);

            string[] file = Directory.GetFiles(userLocalFolder, userDBName + "*");

            foreach (string f in file)
            {
                if (File.Exists(f))
                {
                    File.Delete(f); 
                }
            }

            //Refresh
            FillBookCollectionAsync();

            //Reset prefered database
            da.PreferedDatabasePath = da.DatabaseFilePath;
        }

        #endregion

    }
}
