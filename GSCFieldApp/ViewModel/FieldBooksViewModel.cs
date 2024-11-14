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
using GSCFieldApp.Controls;
using NetTopologySuite.Index.HPRtree;
using GSCFieldApp.Services;
using System.IO.Compression;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Maui.Alerts;
using static GSCFieldApp.Dictionaries.DatabaseLiterals;

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

        //Others
        private double _dbVersion = 0.0; //Will be used to track ugraded db versions
        private double _dbNextVersion = 0.0; //Will be used to track to which version the db will be ugraded to
        //Events
        public static EventHandler<bool> newFieldBookSelected; //This event is triggered when a different fb is selected so field notes and map pages forces a refresh.  

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
        Task FieldBookChanged(FieldBooks tappedFieldbook)
        {
            string preferedDBPath = Preferences.Get(ApplicationLiterals.preferenceDatabasePath, string.Empty);

            if (tappedFieldbook != null && tappedFieldbook.ProjectDBPath != preferedDBPath)
            {
                SetFieldBookAsPreferred(tappedFieldbook);
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
            await Shell.Current.GoToAsync($"/{nameof(FieldBookPage)}/");
        }

        [RelayCommand]
        async Task UploadFieldBook()
        {
            //Upload
            AppFileServices appFileServices = new AppFileServices();
            string uploadFB = await appFileServices.UploadFieldBook();

            //Refresh list
            if (uploadFB.Contains(DBTypeSqlite))
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
            SetWaitingCursor(true);

            //Validate if selected fieldbook needs upgrade or can be upgraded
            //New field books or empty ones shouldn't be upgraded
            bool canUpgrade = await CanUpgradeFieldBook();
            if (canUpgrade)
            {
                bool upgradeWorked = await DoUpgradeFieldBook();

                //Show toast of upgrade finished
                if (upgradeWorked)
                {
                    await Shell.Current.DisplayAlert(LocalizationResourceManager["FieldBooksUpgradeTitle"].ToString(),
                        LocalizationResourceManager["FieldBooksUpgradeContentDone"].ToString(),
                        LocalizationResourceManager["GenericButtonOk"].ToString());
                }
                else
                {
                    //State that something went wrong.
                    await Shell.Current.DisplayAlert(LocalizationResourceManager["FieldBooksUpgradeTitle"].ToString(),
                        LocalizationResourceManager["FieldBooksUpgradeContentError"].ToString(),
                        LocalizationResourceManager["GenericButtonOk"].ToString());
                }
            }
            else
            {
                await Shell.Current.DisplayAlert(LocalizationResourceManager["FieldBooksUpgradeTitle"].ToString(),
                    LocalizationResourceManager["FieldBooksUpgradeContentInvalid"].ToString(),
                    LocalizationResourceManager["GenericButtonOk"].ToString());
            }

            SetWaitingCursor(false);
        }

        [RelayCommand]
        public async Task SwipeGestureRecognizer(FieldBooks fieldBook)
        {
            await PrepareDeleteFieldBook(fieldBook);
        }

        [RelayCommand]
        public async Task EditFieldBook(FieldBooks tappedFieldbook)
        {
            FieldBooks fbToEdit = new FieldBooks();
            //Navigate to fieldbook page and send along the metadata
            if (tappedFieldbook == null && _selectedFieldBook != null)
            {
                fbToEdit = _selectedFieldBook;
            }
            else if (tappedFieldbook != null)
            {
                fbToEdit = tappedFieldbook;
            }

            // Keep in preferences
            SetFieldBookAsPreferred(fbToEdit);

            await Shell.Current.GoToAsync($"/{nameof(FieldBookPage)}/",
                new Dictionary<string, object>
                {
                    [nameof(Metadata)] = fbToEdit.metadataForProject,
                }
            );
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
                    if ((fi.Extension.Contains(DBTypeSqlite) || fi.Extension.Contains(DBTypeSqliteDeprecated))
                        && !fi.Name.Contains(DBName))
                    {

                        //Connect to found database and retrive some information from it
                        FieldBooks currentBook = new FieldBooks();
                        SQLiteAsyncConnection currentConnection = da.GetConnectionFromPath(fi.FullName);

                        //Get metadata records
                        Metadata metadataTableRows = await currentConnection.Table<Metadata>()?.FirstAsync();
                        if(metadataTableRows != null) 
                        {
                            //For metadata
                            currentBook.CreateDate = metadataTableRows.StartDate;
                            currentBook.GeologistGeolcode = metadataTableRows.Geologist + "[" + metadataTableRows.UserCode + "]";
                            currentBook.ProjectPath = FileSystem.Current.AppDataDirectory;
                            currentBook.ProjectDBPath = fi.FullName;
                            currentBook.metadataForProject = metadataTableRows;

                            //Manage to select last prefered fieldbook
                            string preferedDBPath = Preferences.Get(ApplicationLiterals.preferenceDatabasePath, string.Empty);
                            if (currentBook.ProjectDBPath == preferedDBPath)
                            {
                                currentBook.isSelected = true;
                                _selectedFieldBook = currentBook;
                                
                                OnPropertyChanged(nameof(SelectedFieldBook));
                            }
                            else
                            {
                                currentBook.isSelected = false;
                            }
                        }

                        //For stations
                        string stationQuerySelect = "SELECT *";
                        string stationQueryFrom = " FROM " + TableStation;
                        string stationQueryWhere = " WHERE " + TableStation + "." + FieldStationAlias + " NOT LIKE '%" + KeywordStationWaypoint + "%'";
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

            //Push prefered field book at first place
            if (_fieldbookCollection != null && _fieldbookCollection.Count() > 1)
            {
                FieldBooks prefFB = _fieldbookCollection.Where(x => x.isSelected).First();
                if (prefFB != null)
                {
                    int currentPreferedIndex = _fieldbookCollection.IndexOf(prefFB);
                    _fieldbookCollection.Move(currentPreferedIndex, 0);
                }
            }
            else if (_fieldbookCollection != null && _fieldbookCollection.Count() == 1)
            {
                //Else auto-select the only field book
                _selectedFieldBook = _fieldbookCollection.First();
                OnPropertyChanged(nameof(SelectedFieldBook));

                //Force this to be the prefered database.
                da.PreferedDatabasePath = _selectedFieldBook.ProjectDBPath;
            }
            else
            {
                //Default
                da.PreferedDatabasePath = da.DatabaseFilePath;
            }

            //Refresh UI
            OnPropertyChanged(nameof(FieldbookCollection));

            WatermarkValidation();
        }

        /// <summary>
        /// Will show a watermark if no field book are present in the app
        /// </summary>
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
        public async Task DeleteUserFieldBook()
        {
            string userLocalFolder = Path.GetDirectoryName(da.PreferedDatabasePath);
            string userDBName = Path.GetFileNameWithoutExtension(da.PreferedDatabasePath);

            //Make sure to close the connection before deleting
            await da.CloseConnectionAsync();

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

        /// <summary>
        /// Will keep in memory the desired field book and selected in the collection
        /// </summary>
        /// <param name="book"></param>
        public void SetFieldBookAsPreferred(FieldBooks book)
        {
            da.PreferedDatabasePath = book.ProjectDBPath;

            // Keep in pref project type for futur vocab use and other viewing purposes
            Preferences.Set(nameof(FieldUserInfoFWorkType), book.metadataForProject.FieldworkType);
            Preferences.Set(nameof(FieldUserInfoUCode), book.metadataForProject.UserCode);

            _selectedFieldBook = book;
            OnPropertyChanged(nameof(SelectedFieldBook));

            //Send call to refresh other pages
            EventHandler<bool> newFieldBookRequest = newFieldBookSelected;
            if (newFieldBookRequest != null)
            {
                newFieldBookRequest(this, true);
            }

        }

        /// <summary>
        /// Will take select fieldbook db path and will perform some checks to validate if 
        /// database can be upgraded or not. Older schema will return true.
        /// </summary>
        public async Task<bool> CanUpgradeFieldBook()
        {
            //Variables
            bool canUpgrade = false;

            //Check #1 Needs more then one location for it to be upgradable
            FieldLocation fieldLocationQuery = new FieldLocation();
            int locationCount = await da.GetTableCount(fieldLocationQuery.GetType());

            //Check #2 DB version must be older then current
            _dbVersion = await da.GetDBVersion();

            if (_dbVersion != DatabaseLiterals.DBVersion && _dbVersion != 0.0 && _dbVersion < DatabaseLiterals.DBVersion)
            {
                canUpgrade = true;
            }

            return canUpgrade;
        }

        /// <summary>
        /// Will upgrade a selected field book to the latest schema version
        /// </summary>
        /// <returns></returns>
        public async Task<bool> DoUpgradeFieldBook()
        { 
            bool upgradeFinished = false;

            //Upgrade until latest version
            while (_dbVersion < DatabaseLiterals.DBVersion)
            {

                //Get a new legacy database name
                string legacyDB = GetLegacyDBNamePath(_dbVersion);

                //Create a brand new legacy database
                await da.CreateDatabaseFromResource(legacyDB, legacyDB.Substring(legacyDB.IndexOf("_v")));

                //Last check on db version
                _dbVersion = _dbNextVersion;

                //TEMP
                upgradeFinished = true;

            }

            return upgradeFinished;
        }

        /// <summary>
        /// Will build a new database name and path to store upgraded result for a given version
        /// to the next one (e.g. 1.6 db named Salluit.gpkg will output Salluit_V170.gpkg)
        /// </summary>
        /// <returns></returns>
        public string GetLegacyDBNamePath(double dbVersion)
        {
            //output
            string legacyDBPath = string.Empty;

            //Keep current database path before creating the new one
            string legacyDepot = FileSystem.Current.AppDataDirectory;

            //Get current file name
            string legacyFileName = Path.GetFileNameWithoutExtension(da.PreferedDatabasePath);

            //Get next version number
            _dbNextVersion = GetNextLegacyDBVersion(dbVersion);

            //Build a new one
            if (_dbNextVersion < DatabaseLiterals.DBVersion150)
            {
                legacyFileName = legacyFileName + "_v" + _dbNextVersion.ToString().Replace(".", "") + DatabaseLiterals.DBTypeSqliteDeprecated;
            }
            else if (_dbNextVersion < DatabaseLiterals.DBVersion170 && _dbNextVersion >= DatabaseLiterals.DBVersion150)
            {
                legacyFileName = legacyFileName + "_v" + _dbNextVersion.ToString().Replace(".", "") + "0" + DatabaseLiterals.DBTypeSqliteDeprecated;
            }
            else if (_dbNextVersion < DatabaseLiterals.DBVersion && _dbNextVersion >= DatabaseLiterals.DBVersion170)
            {
                legacyFileName = legacyFileName + "_v" + _dbNextVersion.ToString().Replace(".", "") + "0" + DatabaseLiterals.DBTypeSqlite;
            }
            else 
            {
                //Current defaulting to 1.9
                legacyFileName = legacyFileName + DatabaseLiterals.DBTypeSqlite;
            }

            //Build path
            legacyDBPath = Path.Combine(legacyDepot, legacyFileName);

            return legacyDBPath;
        }


        /// <summary>
        /// Will output the next legacy version that follows a given one
        /// </summary>
        /// <param name="dbVersion"></param>
        /// <returns></returns>
        public double GetNextLegacyDBVersion(double dbVersion)
        {
            //output
            double nextVersion = dbVersion;

            if (dbVersion == 1.42)
            {
                nextVersion = DatabaseLiterals.DBVersion143;
            }
            else if (dbVersion == 1.43)
            {
                nextVersion = DatabaseLiterals.DBVersion144;
            }
            else if (dbVersion == 1.44)
            {
                nextVersion = DatabaseLiterals.DBVersion150;
            }
            else if (dbVersion == 1.5)
            {
                nextVersion = DatabaseLiterals.DBVersion160;
            }
            else if (dbVersion == 1.6)
            {
                nextVersion = DatabaseLiterals.DBVersion170;
            }
            else if (dbVersion == 1.7)
            {
                nextVersion = DatabaseLiterals.DBVersion170;

            }
            else if (dbVersion == 1.8)
            {
                nextVersion = DatabaseLiterals.DBVersion190;

            }

            return nextVersion;
        }

        #endregion

    }
}
