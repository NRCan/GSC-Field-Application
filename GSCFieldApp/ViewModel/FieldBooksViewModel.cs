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
using NTS = NetTopologySuite;
using NetTopologySuite.Geometries;
using ProjNet.CoordinateSystems;
using System.Data;

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
            if (uploadFB.Contains(DBTypeSqlite) || uploadFB.Contains(DBTypeSqliteDeprecated))
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

                    //Refresh
                    FillBookCollectionAsync();
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
                    //Get the databases but not the main default one or the legacy ones
                    if ((fi.Extension.Contains(DBTypeSqlite) || fi.Extension.Contains(DBTypeSqliteDeprecated))
                        && !fi.Name.Contains(DBName) && !fi.Name.Contains("_v"))
                    {

                        //Connect to database and retrieve some information from it
                        FieldBooks currentBook = new FieldBooks();
                        SQLiteAsyncConnection currentConnection = da.GetConnectionFromPath(fi.FullName);

                        //Get metadata records
                        List<Metadata> metadataTableRows = await currentConnection.Table<Metadata>().ToListAsync();
                        if (metadataTableRows != null && metadataTableRows.Count() > 0)
                        {
                            Metadata validMetadata = metadataTableRows.First();

                            if (validMetadata != null)
                            {
                                //For metadata
                                currentBook.CreateDate = validMetadata.StartDate;
                                currentBook.GeologistGeolcode = validMetadata.Geologist + "[" + validMetadata.UserCode + "]";
                                currentBook.ProjectPath = FileSystem.Current.AppDataDirectory;
                                currentBook.ProjectDBPath = fi.FullName;
                                currentBook.metadataForProject = validMetadata;

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
                List<FieldBooks> prefFB = _fieldbookCollection.Where(x => x.isSelected).ToList();
                if (prefFB != null && prefFB.Count > 0)
                {
                    int currentPreferedIndex = _fieldbookCollection.IndexOf(prefFB.First());
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

            if (_dbVersion != DatabaseLiterals.DBVersion 
                && _dbVersion != 0.0 
                && _dbVersion < DatabaseLiterals.DBVersion)
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
            bool upgradeWorked = true;

            //Rename selected field book with version number
            string legacyDBFrom = GetLegacyDBNamePath(_dbVersion, false);
            if (!File.Exists(legacyDBFrom) )
            {
                System.IO.File.Move(da.PreferedDatabasePath, legacyDBFrom);
                da.PreferedDatabasePath = legacyDBFrom;
            }

            //Upgrade until latest version
            while (_dbVersion < DatabaseLiterals.DBVersion)
            {

                //Get a new legacy database name
                string legacyDBTo = GetLegacyDBNamePath(_dbVersion);

                //Build ressource file name
                string legacyResource = string.Empty;
                if (legacyDBTo.Contains("_v"))
                {
                    legacyResource = DatabaseLiterals.DBName + legacyDBTo.Substring(legacyDBTo.IndexOf("_v"));
                }

                //Create a brand new legacy database 
                bool createdLegacyDB = await da.CreateDatabaseFromResource(legacyDBTo, legacyResource);

                if (createdLegacyDB)
                {
                    //Connect to the new working database
                    SQLiteAsyncConnection upgradeDBConnection = da.GetConnectionFromPath(legacyDBTo);

                    //Keep user vocab dictionaries
                    bool upgradeVocabWorked = await da.GetLatestVocab(legacyDBFrom, upgradeDBConnection, _dbVersion, true);

                    //Upgrade other tables
                    bool upgradeTableWorked = await UpgradeTables(legacyDBFrom, upgradeDBConnection, _dbVersion, _dbNextVersion, true);

                    //Upgrade geometries
                    bool upgradeGeometriesWorked = await UpgradeGeometries(legacyDBFrom, upgradeDBConnection, _dbVersion, _dbNextVersion, true);

                    //Manage errors
                    if (!upgradeVocabWorked || !upgradeTableWorked || !upgradeGeometriesWorked)
                    {
                        upgradeWorked = false;
                    }

                    //Close
                    await upgradeDBConnection.CloseAsync();
                }

                //Iterate version and legacy database path
                _dbVersion = _dbNextVersion;
                legacyDBFrom = legacyDBTo;



            }

            return upgradeWorked;
        }

        /// <summary>
        /// Will build a new database name and path to store upgraded result for a given version
        /// to the next one (e.g. 1.6 db named Salluit.gpkg will output Salluit_V170.gpkg)
        /// </summary>
        /// <returns></returns>
        public string GetLegacyDBNamePath(double dbVersion, bool withNextVersion = true)
        {
            //output
            string legacyDBPath = string.Empty;

            //Keep current database path before creating the new one
            string legacyDepot = FileSystem.Current.AppDataDirectory;

            //Get current file name
            string legacyFileName = Path.GetFileNameWithoutExtension(da.PreferedDatabasePath);

            //Get next version number
            if (withNextVersion)
            {
                _dbNextVersion = GetNextLegacyDBVersion(dbVersion);
            }
            else
            {
                _dbNextVersion = dbVersion;
            }

            //Build a new one
            if (legacyFileName.Contains("_v"))
            {
                //Make sure to remove iterated versions from name
                legacyFileName = legacyFileName.Substring(0,legacyFileName.IndexOf("_v")); 
            }
            
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
                nextVersion = DatabaseLiterals.DBVersion180;

            }
            else if (dbVersion == 1.8)
            {
                nextVersion = DatabaseLiterals.DBVersion190;

            }

            return nextVersion;
        }

        /// <summary>
        /// Will take an input database path and will upgrade it to current version
        /// </summary>
        /// <param name="inDBPath"></param>
        /// <param name="outToDBConnection"></param>
        /// <param name="inDBVersion"></param>
        /// <param name="closeConnection"></param>
        /// <returns></returns>
        public async Task<bool> UpgradeTables(string fromDBPath, SQLiteAsyncConnection toDBConnection, double fromDBVersion, double toDBVersion, bool closeConnection = true)
        {
            //output
            bool upgradeTableWorked = false;

            //Variables
            string attachDBName = "dbUpgrade";
            List<Exception> exceptionList = new List<Exception>();

            //Tables list as string to build insert query strings
            List<string> basicInsertQueriesTables = new List<string>()
            {
                TableLocation, TableMetadata, TableEarthMat, TableSample, 
                TableStation, TableDocument, TableStructure, TableFossil, 
                TableMineral, TableMineralAlteration, TablePFlow, TableEnvironment
            };

            //List of queries to send as a batch
            List<string> queryList = new List<string>();

            //Build attach db query
            string attachQuery = "ATTACH '" + fromDBPath + "' AS " + attachDBName + "; ";
            queryList.Add(attachQuery);
            
            //Shut down foreign keys constraints, else some loading might throws errors
            string shutDownForeignConstraints = "PRAGMA foreign_keys = off";
            queryList.Add(shutDownForeignConstraints);

            //Open foreign keys contraints, else delete won't happen properly
            string openForeignConstraints = "PRAGMA foreign_keys = on";

            #region Version specific insert queries

            //Tables field inserts must be in same order and same number as db table
            if (fromDBVersion <= 1.42)
            {
                queryList.Add(GetUpgradeQueryVersion1_42(attachDBName));

                //Remove table that needs a special update query
                basicInsertQueriesTables.Remove(TableEarthMat);

                //Add deprecated
                basicInsertQueriesTables.Remove(TableEnvironment);
                basicInsertQueriesTables.Add(TableTraverseLineDeprecated);
                basicInsertQueriesTables.Add(TableTraversePointDeprecated);
            }
            if (fromDBVersion == 1.43)
            {
                queryList.AddRange(GetUpgradeQueryVersion1_44(attachDBName));

                //Remove table that needs a special update query
                basicInsertQueriesTables.Remove(TableLocation);
                basicInsertQueriesTables.Remove(TableMetadata);

                //Add deprecated
                basicInsertQueriesTables.Remove(TableEnvironment);
                basicInsertQueriesTables.Add(TableTraverseLineDeprecated);
                basicInsertQueriesTables.Add(TableTraversePointDeprecated);

            }
            if (fromDBVersion < 1.5 && fromDBVersion >= 1.44)
            {
                queryList.AddRange(GetUpgradeQueryVersion1_5(attachDBName));

                //Remove table that needs a special update query
                basicInsertQueriesTables.Remove(TableLocation);
                basicInsertQueriesTables.Remove(TableSample);
                basicInsertQueriesTables.Remove(TableStation);
                basicInsertQueriesTables.Remove(TableStructure);
                basicInsertQueriesTables.Remove(TableEarthMat);
                basicInsertQueriesTables.Remove(TableMetadata);
                basicInsertQueriesTables.Remove(TableDocument);

                //Add deprecated
                basicInsertQueriesTables.Remove(TableEnvironment);
                basicInsertQueriesTables.Add(TableTraverseLineDeprecated);
                basicInsertQueriesTables.Add(TableTraversePointDeprecated);
            }

            if (fromDBVersion == 1.5)
            {
                queryList.AddRange(GetUpgradeQueryVersion1_6(attachDBName));

                //Remove table that needs a special update query
                basicInsertQueriesTables.Remove(TableStation);
                basicInsertQueriesTables.Remove(TableEarthMat);
                basicInsertQueriesTables.Remove(TableMineral);
                basicInsertQueriesTables.Remove(TableMineralAlteration);
                basicInsertQueriesTables.Remove(TableLocation);

                //Tables that are either not in are can't have any data in this version
                basicInsertQueriesTables.Remove(TableEnvironment);
                basicInsertQueriesTables.Add(TableTraverseLineDeprecated);
                basicInsertQueriesTables.Add(TableTraversePointDeprecated);

            }

            if (fromDBVersion == DBVersion160)
            {
                queryList.AddRange(GetUpgradeQueryVersion1_7(attachDBName));

                //Remove table that needs a special update query
                basicInsertQueriesTables.Clear();
                queryList.Add(openForeignConstraints); //Open at last, foreign keys so last query of delete works
            }

            if (fromDBVersion == DBVersion170)
            {
                queryList.AddRange(GetUpgradeQueryVersion1_8(attachDBName));

                //Remove table that needs a special update query
                basicInsertQueriesTables.Remove(TableSample);
                basicInsertQueriesTables.Remove(TableMineralAlteration);
                basicInsertQueriesTables.Remove(TableLocation);
                basicInsertQueriesTables.Remove(TableDocument);
                basicInsertQueriesTables.Remove(TableEarthMat);
                basicInsertQueriesTables.Remove(TableTraverseLine);
                basicInsertQueriesTables.Remove(TableTraversePoint);
            }

            if (fromDBVersion == DBVersion180)
            {
                queryList.AddRange(GetUpgradeQueryVersion1_9(attachDBName));

                //Remove table that needs a special update query
                basicInsertQueriesTables.Remove(TableSample);
                basicInsertQueriesTables.Remove(TableEarthMat);
                basicInsertQueriesTables.Remove(TableStructure);
            }
            #endregion

            //Insert remaining tables
            foreach (string t in basicInsertQueriesTables)
            {
                //Build insert queries
                string insertQuery = "INSERT INTO " + t + " SELECT * FROM dbUpgrade." + t + ";";
                queryList.Add(insertQuery);
            }

            //Coin upgraded db version in metadata
            string coinNewVersion = "UPDATE " + DatabaseLiterals.TableMetadata + " SET " + DatabaseLiterals.FieldUserInfoVersionSchema + " = " + toDBVersion.ToString() + ";";
            queryList.Add(coinNewVersion);

            //Build detach query
            string detachQuery = "DETACH DATABASE " + attachDBName + "; ";

            //Update database
            foreach (string q in queryList)
            {
                try
                {
                    await toDBConnection.ExecuteAsync(q);
                }
                catch (Exception e)
                {
                    exceptionList.Add(e);
                }

            }

            //Close if needed
            if (closeConnection)
            {

                await toDBConnection.CloseAsync();
            }

            //Process exceptions
            if (exceptionList.Count > 0)
            {
                string wholeStack = string.Empty;

                foreach (Exception es in exceptionList)
                {
                    wholeStack = wholeStack + "; " + es.Message + "; " + es.StackTrace;
                }

                foreach (string q in queryList)
                {
                    wholeStack = wholeStack + "\n " + q;
                }

                //Log
                new ErrorToLogFile(wholeStack + "\n DBVersion:" + fromDBVersion).WriteToFile();

            }
            else
            {
                upgradeTableWorked = true;
            }

            return upgradeTableWorked;
        }

        /// <summary>
        /// For certain versions, some geometry upgrade is needed (e.g. sqlite to gpkg, and uniform wgs84)
        /// </summary>
        /// <param name="fromDBPath"></param>
        /// <param name="toDBConnection"></param>
        /// <param name="fromDBVersion"></param>
        /// <param name="toDBVersion"></param>
        /// <param name="closeConnection"></param>
        /// <returns></returns>
        public async Task<bool> UpgradeGeometries(string fromDBPath, SQLiteAsyncConnection toDBConnection, double fromDBVersion, double toDBVersion, bool closeConnection = true)
        {
            //output
            bool upgradeGeometriesWorked = true;

            //Converting sqlite to geopackage geometries
            if (toDBVersion == 1.7)
            {
                GeopackageService packService = new GeopackageService();

                //For all records.
                List<FieldLocation> fieldLocations = await toDBConnection.Table<FieldLocation>().ToListAsync();
                foreach (FieldLocation fl in fieldLocations)
                {
                    if (fl.LocationLong != 0 && fl.LocationLat != 0)
                    {
                        try
                        {
                            //Get bytes from coordinate fields
                            byte[] geom = packService.CreateByteGeometryPoint(fl.LocationLong, fl.LocationLat);
                            fl.LocationGeometry = geom;

                            //Save
                            string upQuery = string.Format("UPDATE {0} SET {1} = ? WHERE {2} = {3};", TableLocation, FieldGenericGeometry, FieldLocationID, fl.LocationID);
                            object[] arg = new object[] { geom };
                            await toDBConnection.ExecuteAsync(upQuery, arg);


                        }
                        catch (Exception e)
                        {
                            new ErrorToLogFile(e.Message).WriteToFile();
                            upgradeGeometriesWorked = false;
                        }
                        
                    }
                    
                }
                
            }

            //Make sure they're all in WGS84, version 1.7 wasn't consistent 
            if (toDBVersion == 1.8)
            {
                GeopackageService packService = new GeopackageService();

                //For all records.
                List<FieldLocation> fieldLocations = await toDBConnection.Table<FieldLocation>().ToListAsync();
                foreach (FieldLocation fls in fieldLocations)
                {
                    if (fls.LocationEPSGProj != null && fls.LocationEPSGProj != "4326" && fls.LocationEPSGProj != string.Empty
                        && fls.LocationEasting.HasValue && fls.LocationNorthing.HasValue)
                    {
                        try
                        {
                            //Get point object
                            Coordinate coord = new Coordinate((double)fls.LocationEasting, (double)fls.LocationNorthing);
                            NTS.Geometries.Point pnt = new NTS.Geometries.Point(coord);

                            //Create spatial references
                            CoordinateSystem incomingProjection = await SridReader.GetCSbyID(Convert.ToInt16(fls.LocationEPSGProj));
                            CoordinateSystem outgoingProjection = await SridReader.GetCSbyID(4326);

                            //Transform
                            NTS.Geometries.Point transformedPoint = await GeopackageService.TransformPointCoordinates(pnt, incomingProjection, outgoingProjection);

                            //Get as byte 
                            byte[] pntByte = packService.CreateByteGeometryPoint(transformedPoint.X, transformedPoint.Y);

                            //Save
                            string upQuery = string.Format("UPDATE {0} SET {1} = ? WHERE {2} = {3};", TableLocation, FieldGenericGeometry, FieldLocationID, fls.LocationID);
                            object[] arg = new object[] { pntByte };
                            await toDBConnection.ExecuteAsync(upQuery, arg);

                            //Make sure geographic coordinates are also right
                            string upQueryGeo = string.Format("UPDATE {0} SET {1} = ?, {2} = ? WHERE {3} = {4};", TableLocation, FieldLocationLongitude, FieldLocationLatitude, FieldLocationID, fls.LocationID);
                            object[] argGeo = new object[] { transformedPoint.X, transformedPoint.Y };
                            await toDBConnection.ExecuteAsync(upQueryGeo, argGeo);
                        }
                        catch (Exception e)
                        {
                            new ErrorToLogFile(e.Message).WriteToFile();
                            upgradeGeometriesWorked = false;
                        }
                    }
                }

            }

            return upgradeGeometriesWorked;
        }

        /// <summary>
        /// Will output queries to update database to version 1.42
        /// </summary>
        /// <returns></returns>
        public string GetUpgradeQueryVersion1_42(string attachedDBName)
        {
            ///Schema v 1.42 -- New note field in earthmat, will make earthmat dialog crash 
            ///INSERT INTO F_EARTH_MATERIAL SELECT *, CASE WHEN EXISTS (SELECT sql from db2.sqlite_master where sql LIKE '%F_EARTH_MATERIAL%NOTES%') THEN ("") ELSE NULL END as NOTES FROM db2.F_EARTH_MATERIAL
            Earthmaterial modelEM = new Earthmaterial();
            List<string> earthmatFieldList = modelEM.getFieldList[1.42];
            string earthmat_querySelect = string.Empty;

            foreach (string earthmatFields in earthmatFieldList)
            {
                //Get all fields except notes

                if (earthmatFields != earthmatFieldList.First())
                {
                    if (earthmatFields == DatabaseLiterals.FieldEarthMatNotes)
                    {
                        //Set notes to empty
                        earthmat_querySelect = earthmat_querySelect + ", '' as " + earthmatFields;
                    }
                    else
                    {
                        earthmat_querySelect = earthmat_querySelect + ", " + earthmatFields;
                    }


                }
                else
                {
                    earthmat_querySelect = earthmatFields;
                }

            }
            earthmat_querySelect = earthmat_querySelect.Replace(", ,", "");

            string insertQueryEarthmat_142 = "INSERT INTO " + DatabaseLiterals.TableEarthMat;
            insertQueryEarthmat_142 = insertQueryEarthmat_142 + " SELECT " + earthmat_querySelect + " FROM " + attachedDBName + "." + DatabaseLiterals.TableEarthMat;

            return insertQueryEarthmat_142;
        }

        /// <summary>
        /// Will output a queyr to update database to version 1.44
        /// </summary>
        /// <returns></returns>
        public List<string> GetUpgradeQueryVersion1_44(string attachedDBName)
        {
            ///Schema v 1.44 -- New EPSG field in F_LOCATION, will make Location dialog crash, field is coming from F_METADATA
            ///INSERT INTO F_LOCATION SELECT *, CASE WHEN EXISTS (SELECT sql from db2.sqlite_master where sql LIKE '%F_LOCATION%EPSG%') THEN ("") ELSE NULL END as EPSG FROM db2.F_LOCATION
            #region F_LOCATION
            FieldLocation modelLocation = new FieldLocation();
            List<string> locationFieldList = modelLocation.getFieldList[DBVersion144];

            string location_querySelect = string.Empty;
            List<string> insertQuery_144 = new List<string>();

            foreach (string locationFields in locationFieldList)
            {
                //Get all fields except notes

                if (locationFields != locationFieldList.First())
                {
                    if (locationFields == DatabaseLiterals.FieldLocationDatum)
                    {

                        ///Take EPSG from F_METADATA
                        ///Note that this query takes into account possible database coming from Ganfeld FGDB conversion into SQLite.
                        ///I took the projection lut picklist from Ganfeld to build this query.
                        //INSERT INTO F_LOCATION
                        //SELECT l.LOCATIONID as LOCATIONID, l.LOCATIONNAME as LOCATIONNAME, l.EASTING as EASTING, l.NORTHING as NORTHING, l.LATITUDE as LATITUDE, l.LONGITUDE as LONGITUDE, 
                        //CASE WHEN EXISTS(SELECT sql from db2.sqlite_master where sql LIKE '%F_METADATA%EPSG%') THEN(
                        //CASE WHEN(m.EPSG LIKE '%84%') THEN('4326') ELSE(
                        ///* From Ganfeld Project LUT file */
                        //CASE WHEN(m.EPSG LIKE 'NAD_1983_Zone_7N') THEN('26907') ELSE(
                        //CASE WHEN(m.EPSG LIKE 'NAD_1983_Zone_8N') THEN('26908') ELSE(
                        //CASE WHEN(m.EPSG LIKE 'NAD_1983_Zone_9N') THEN('26909') ELSE(
                        //CASE WHEN(m.EPSG LIKE 'NAD_1983_Zone_%') THEN('269' || SUBSTR(m.EPSG, 15, 2)) ELSE(
                        //CASE WHEN(m.EPSG LIKE 'Albers_Yukon') THEN('3579') ELSE(
                        //CASE WHEN(m.EPSG LIKE '%North_American_1983') THEN('4617') ELSE(
                        //CASE WHEN(m.EPSG LIKE 'Albers_BC') THEN('3153') ELSE(m.EPSG)  END) END) END) END) END) END) END) END)
                        //ELSE('') END as EPSG, l.ELEVATION as ELEVATION, l.ELEVMETHOD as ELEVMETHOD, l.ELEVACCURACY as ELEACCURACY, l.ENTRYTYPE as ENTRYTYPE, l.PDOP as PDOP, l.ERRORMEASURE as ERRORMEASURE, l.ERRORTYPEMEASURE as ERRORTYPEMEASURE, l.NOTES as NOTES, l.REPORT_LINK as REPORT_LINK, l.METAID as METAID
                        //FROM db2.F_METADATA as m LEFT OUTER JOIN db2.F_LOCATION as l on l.METAID = m.METAID
                        location_querySelect = location_querySelect + ", CASE WHEN EXISTS (SELECT sql from " + attachedDBName + ".sqlite_master where sql LIKE '%" + DatabaseLiterals.TableMetadata + "%" + DatabaseLiterals.FieldLocationDatum + "%') THEN (CASE WHEN(m." + DatabaseLiterals.FieldLocationDatum + " LIKE '%84%') THEN('4326') ELSE( /* From Ganfeld Project LUT file */ CASE WHEN(m." + DatabaseLiterals.FieldLocationDatum + " LIKE 'NAD_1983_Zone_7N') THEN('26907') ELSE( CASE WHEN(m." + DatabaseLiterals.FieldLocationDatum + " LIKE 'NAD_1983_Zone_8N') THEN('26908') ELSE( CASE WHEN(m." + DatabaseLiterals.FieldLocationDatum + " LIKE 'NAD_1983_Zone_9N') THEN('26909') ELSE( CASE WHEN(m." + DatabaseLiterals.FieldLocationDatum + " LIKE 'NAD_1983_Zone_%') THEN('269' || SUBSTR(m." + DatabaseLiterals.FieldLocationDatum + ", 15, 2)) ELSE( CASE WHEN(m." + DatabaseLiterals.FieldLocationDatum + " LIKE 'Albers_Yukon') THEN('3579') ELSE( CASE WHEN(m." + DatabaseLiterals.FieldLocationDatum + " LIKE '%North_American_1983') THEN('4617') ELSE( CASE WHEN(m." + DatabaseLiterals.FieldLocationDatum + " LIKE 'Albers_BC') THEN('3153') ELSE(m." + DatabaseLiterals.FieldLocationDatum + ")  END) END) END) END) END) END) END) END) ELSE ('') END as " + DatabaseLiterals.FieldLocationDatum;
                    }

                    else
                    {
                        location_querySelect = location_querySelect + ", l." + locationFields + " as " + locationFields;
                    }
                }
                else
                {
                    location_querySelect = "l." + locationFields + " as " + locationFields;
                }


            }
            location_querySelect = location_querySelect.Replace(", ,", "");

            string insertQuery_144_Location = "INSERT INTO " + DatabaseLiterals.TableLocation + " SELECT " + location_querySelect;
            insertQuery_144_Location = insertQuery_144_Location + " FROM " + attachedDBName + "." + DatabaseLiterals.TableMetadata + " as m";
            insertQuery_144_Location = insertQuery_144_Location + " LEFT OUTER JOIN " + attachedDBName + "." + DatabaseLiterals.TableLocation + " as l ON " + "l." + DatabaseLiterals.FieldLocationMetaID + " = m." + DatabaseLiterals.FieldUserInfoID;
            insertQuery_144.Add(insertQuery_144_Location);
            #endregion

            #region F_METADATA
            Metadata modelMetadata = new Metadata();
            List<string> metadataFieldList = modelMetadata.getFieldList[DBVersion144];
            string metadata_querySelect = string.Empty;
            List<string> insertQueryMetadata_144 = new List<string>();

            //Get rid of deleted fields
            metadataFieldList.Remove(DatabaseLiterals.FieldUserInfoEPSG);
            metadataFieldList.Remove(DatabaseLiterals.FieldUserInfoActivityName);

            foreach (string metadataFields in metadataFieldList)
            {
                //Get all fields except notes
                if (metadataFields != metadataFieldList.First())
                {
                    metadata_querySelect = metadata_querySelect + ", " + metadataFields;
                }
                else if (metadataFields == metadataFieldList.Last())
                {
                    metadata_querySelect = metadata_querySelect + ", " + metadataFields;
                }
                else
                {
                    metadata_querySelect = metadataFields;
                }

            }
            metadata_querySelect = metadata_querySelect.Replace(", ,", "");

            insertQuery_144.Add("INSERT INTO " + DatabaseLiterals.TableMetadata + " SELECT " + metadata_querySelect + " FROM " + attachedDBName + "." + DatabaseLiterals.TableMetadata);
            #endregion

            return insertQuery_144;
        }

        /// <summary>
        /// Will output a query to update database to version 1.5
        /// </summary>
        /// <returns></returns>
        public List<string> GetUpgradeQueryVersion1_5(string attachedDBName)
        {
            ///Schema v 1.5: 
            ///https://github.com/NRCan/GSC-Field-Application/issues/105 New alias field names for some key tables
            ///https://github.com/NRCan/GSC-Field-Application/issues/67 New mandatory field to replace project name
            ///insert into F_LOCATION 
            //SELECT CASE WHEN EXISTS(SELECT sql from db2.sqlite_master where sql LIKE '%F_LOCATION%LOCATIONNAME%') THEN(l.LOCATIONNAME) ELSE NULL END as LOCATIONIDNAME from db2.F_LOCATION as l
            List<string> insertQuery_15 = new List<string>();

            #region F_LOCATION

            FieldLocation modelLocation = new FieldLocation();
            List<string> locationFieldList = modelLocation.getFieldList[DBVersion150];
            string location_querySelect = string.Empty;

            foreach (string locationFields in locationFieldList)
            {
                //Get all fields except alias

                if (locationFields != locationFieldList.First())
                {
                    if (locationFields == DatabaseLiterals.FieldLocationAlias)
                    {

                        location_querySelect = location_querySelect +
                            ", CASE WHEN EXISTS (SELECT sql from " + attachedDBName + ".sqlite_master where sql LIKE '%" + DatabaseLiterals.TableLocation + "%" + DatabaseLiterals.FieldLocationAliasDeprecated +
                            "%') THEN (l." + DatabaseLiterals.FieldLocationAliasDeprecated + ") ELSE NULL END as " + DatabaseLiterals.FieldLocationAlias;
                    }
                    else
                    {
                        location_querySelect = location_querySelect + ", l." + locationFields + " as " + locationFields;
                    }

                }
                else
                {
                    location_querySelect = " l." + locationFields + " as " + locationFields;
                }

            }
            location_querySelect = location_querySelect.Replace(", ,", "");

            string insertQuery_15_Location = "INSERT INTO " + DatabaseLiterals.TableLocation + " SELECT " + location_querySelect;
            insertQuery_15_Location = insertQuery_15_Location + " FROM " + attachedDBName + "." + DatabaseLiterals.TableLocation + " as l";
            insertQuery_15.Add(insertQuery_15_Location);

            #endregion

            #region F_STRUCTURE

            Structure modelStructure = new Structure();
            List<string> structureFieldList = modelStructure.getFieldList[DBVersion150];
            string structure_querySelect = string.Empty;

            foreach (string structureFields in structureFieldList)
            {
                //Get all fields except alias

                if (structureFields != structureFieldList.First())
                {
                    if (structureFields == DatabaseLiterals.FieldStructureName)
                    {

                        structure_querySelect = structure_querySelect +
                            ", CASE WHEN EXISTS (SELECT sql from " + attachedDBName + ".sqlite_master where sql LIKE '%" + DatabaseLiterals.TableStructure + "%" + DatabaseLiterals.FieldStructureNameDeprecated +
                            "%') THEN (s." + DatabaseLiterals.FieldStructureNameDeprecated + ") ELSE NULL END as " + DatabaseLiterals.FieldStructureName;
                    }
                    else
                    {
                        structure_querySelect = structure_querySelect + ", s." + structureFields + " as " + structureFields;
                    }

                }
                else
                {
                    structure_querySelect = " s." + structureFields + " as " + structureFields;
                }

            }
            structure_querySelect = structure_querySelect.Replace(", ,", "");

            string insertQuery_15_structure = "INSERT INTO " + DatabaseLiterals.TableStructure + " SELECT " + structure_querySelect;
            insertQuery_15_structure = insertQuery_15_structure + " FROM " + attachedDBName + "." + DatabaseLiterals.TableStructure + " as s";
            insertQuery_15.Add(insertQuery_15_structure);

            #endregion

            #region F_EARTHMAT

            Earthmaterial modelEarthmat = new Earthmaterial();
            List<string> earthmatFieldList = modelEarthmat.getFieldList[DBVersion150];
            string earthmat_querySelect = string.Empty;

            foreach (string earthmatFields in earthmatFieldList)
            {
                //Get all fields except alias

                if (earthmatFields != earthmatFieldList.First())
                {
                    if (earthmatFields == DatabaseLiterals.FieldEarthMatName)
                    {

                        earthmat_querySelect = earthmat_querySelect +
                            ", CASE WHEN EXISTS (SELECT sql from " + attachedDBName + ".sqlite_master where sql LIKE '%" + DatabaseLiterals.TableEarthMat + "%" + DatabaseLiterals.FieldEarthMatNameDeprecated +
                            "%') THEN (e." + DatabaseLiterals.FieldEarthMatNameDeprecated + ") ELSE NULL END as " + DatabaseLiterals.FieldEarthMatName;
                    }
                    else
                    {
                        earthmat_querySelect = earthmat_querySelect + ", e." + earthmatFields + " as " + earthmatFields;
                    }

                }
                else
                {
                    earthmat_querySelect = " e." + earthmatFields + " as " + earthmatFields;
                }

            }
            earthmat_querySelect = earthmat_querySelect.Replace(", ,", "");

            string insertQuery_15_earthmat = "INSERT INTO " + DatabaseLiterals.TableEarthMat + " SELECT " + earthmat_querySelect;
            insertQuery_15_earthmat = insertQuery_15_earthmat + " FROM " + attachedDBName + "." + DatabaseLiterals.TableEarthMat + " as e";
            insertQuery_15.Add(insertQuery_15_earthmat);

            #endregion

            #region F_SAMPLE

            Sample modelSample = new Sample();
            List<string> sampleFieldList = modelSample.getFieldList[DBVersion150];
            string sample_querySelect = string.Empty;

            foreach (string sampleFields in sampleFieldList)
            {
                //Get all fields except alias

                if (sampleFields != sampleFieldList.First())
                {
                    if (sampleFields == DatabaseLiterals.FieldSampleName)
                    {

                        sample_querySelect = sample_querySelect +
                            ", CASE WHEN EXISTS (SELECT sql from " + attachedDBName + ".sqlite_master where sql LIKE '%" + DatabaseLiterals.TableSample + "%" + DatabaseLiterals.FieldSampleNameDeprecated +
                            "%') THEN (sm." + DatabaseLiterals.FieldSampleNameDeprecated + ") ELSE NULL END as " + DatabaseLiterals.FieldSampleName;
                    }
                    else if (sampleFields == DatabaseLiterals.FieldSampleHorizon)
                    {
                        sample_querySelect = sample_querySelect +
                            ", NULL as " + DatabaseLiterals.FieldSampleHorizon;
                    }
                    else if (sampleFields == DatabaseLiterals.FieldSampleDepthMax)
                    {
                        sample_querySelect = sample_querySelect +
                            ", NULL as " + DatabaseLiterals.FieldSampleDepthMax;
                    }
                    else if (sampleFields == DatabaseLiterals.FieldSampleDepthMin)
                    {
                        sample_querySelect = sample_querySelect +
                            ", NULL as " + DatabaseLiterals.FieldSampleDepthMin;
                    }
                    else if (sampleFields == DatabaseLiterals.FieldSampleDuplicate)
                    {
                        sample_querySelect = sample_querySelect +
                            ", NULL as " + DatabaseLiterals.FieldSampleDuplicate;
                    }
                    else if (sampleFields == DatabaseLiterals.FieldSampleDuplicateName)
                    {
                        sample_querySelect = sample_querySelect +
                            ", NULL as " + DatabaseLiterals.FieldSampleDuplicateName;
                    }
                    else if (sampleFields == DatabaseLiterals.FieldSampleState)
                    {
                        sample_querySelect = sample_querySelect +
                            ", NULL as " + DatabaseLiterals.FieldSampleState;
                    }
                    else
                    {
                        sample_querySelect = sample_querySelect + ", sm." + sampleFields + " as " + sampleFields;
                    }

                }
                else
                {
                    sample_querySelect = " sm." + sampleFields + " as " + sampleFields;
                }

            }
            sample_querySelect = sample_querySelect.Replace(", ,", "");

            string insertQuery_15_sample = "INSERT INTO " + DatabaseLiterals.TableSample + " SELECT " + sample_querySelect;
            insertQuery_15_sample = insertQuery_15_sample + " FROM " + attachedDBName + "." + DatabaseLiterals.TableSample + " as sm";
            insertQuery_15.Add(insertQuery_15_sample);

            #endregion

            #region F_STATION

            Station modelStation = new Station();
            List<string> stationFieldList = modelStation.getFieldList[DBVersion150];
            string station_querySelect = string.Empty;

            foreach (string stationFields in stationFieldList)
            {
                //Get all fields except alias

                if (stationFields != stationFieldList.First())
                {
                    if (stationFields == DatabaseLiterals.FieldStationAlias)
                    {

                        station_querySelect = station_querySelect +
                            ", CASE WHEN EXISTS (SELECT sql from " + attachedDBName + ".sqlite_master where sql LIKE '%" + DatabaseLiterals.TableStation + "%" + DatabaseLiterals.FieldStationAliasDeprecated +
                            "%') THEN (st." + DatabaseLiterals.FieldStationAliasDeprecated + ") ELSE NULL END as " + DatabaseLiterals.FieldStationAlias;
                    }
                    else
                    {
                        station_querySelect = station_querySelect + ", st." + stationFields + " as " + stationFields;
                    }

                }
                else
                {
                    station_querySelect = " st." + stationFields + " as " + stationFields;
                }

            }
            station_querySelect = station_querySelect.Replace(", ,", "");

            string insertQuery_15_station = "INSERT INTO " + DatabaseLiterals.TableStation + " SELECT " + station_querySelect;
            insertQuery_15_station = insertQuery_15_station + " FROM " + attachedDBName + "." + DatabaseLiterals.TableStation + " as st";
            insertQuery_15.Add(insertQuery_15_station);

            #endregion

            #region F_DOCUMENT

            Document modelDocument = new Document();
            List<string> documentFieldList = modelDocument.getFieldList[DBVersion150];
            string document_querySelect = string.Empty;

            foreach (string docFields in documentFieldList)
            {

                if (docFields != documentFieldList.First())
                {
                    if (docFields == DatabaseLiterals.FieldDocumentName)
                    {

                        document_querySelect = document_querySelect +
                            ", CASE WHEN EXISTS (SELECT sql from " + attachedDBName + ".sqlite_master where sql LIKE '%" + DatabaseLiterals.TableDocument + "%" + DatabaseLiterals.FieldDocumentNameDeprecated +
                            "%') THEN (d." + DatabaseLiterals.FieldDocumentNameDeprecated + ") ELSE NULL END as " + DatabaseLiterals.FieldDocumentName;
                    }
                    else
                    {
                        document_querySelect = document_querySelect + ", d." + docFields + " as " + docFields;
                    }

                }
                else
                {
                    document_querySelect = " d." + docFields + " as " + docFields;
                }

            }
            document_querySelect = document_querySelect.Replace(", ,", "");

            string insertQuery_15_doc = "INSERT INTO " + DatabaseLiterals.TableDocument + " SELECT " + document_querySelect;
            insertQuery_15_doc = insertQuery_15_doc + " FROM " + attachedDBName + "." + DatabaseLiterals.TableDocument + " as d";
            insertQuery_15.Add(insertQuery_15_doc);

            #endregion

            #region F_METADATA

            Metadata modelMetadata = new Metadata();
            List<string> metadataFieldList = modelMetadata.getFieldList[DBVersion150];
            string metadata_querySelect = string.Empty;

            foreach (string metFields in metadataFieldList)
            {
                //Get all fields except alias
                if (metFields != metadataFieldList.First())
                {
                    if (metFields == DatabaseLiterals.FieldUserInfoActivityName)
                    {
                        //Duplicate project name in activity name
                        metadata_querySelect = metadata_querySelect +
                            ", iif(NOT EXISTS (SELECT sql from " + attachedDBName + ".sqlite_master where sql LIKE '%" + DatabaseLiterals.TableMetadata + "%" + DatabaseLiterals.FieldUserInfoActivityName +
                            "%'),m." + DatabaseLiterals.FieldUserInfoPName + ",NULL) as " + DatabaseLiterals.FieldUserInfoActivityName;

                    }
                    else if (metFields == DatabaseLiterals.FieldUserInfoNotes)
                    {
                        metadata_querySelect = metadata_querySelect +
                            ", NULL as " + DatabaseLiterals.FieldUserInfoNotes;
                    }
                    else
                    {
                        metadata_querySelect = metadata_querySelect + ", m." + metFields + " as " + metFields;
                    }
                }
                else
                {
                    metadata_querySelect = " m." + metFields + " as " + metFields;
                }


            }
            metadata_querySelect = metadata_querySelect.Replace(", ,", "");

            string insertQuery_15_met = "INSERT INTO " + DatabaseLiterals.TableMetadata + " SELECT " + metadata_querySelect;
            insertQuery_15_met = insertQuery_15_met + " FROM " + attachedDBName + "." + DatabaseLiterals.TableMetadata + " as m";
            insertQuery_15.Add(insertQuery_15_met);

            #endregion

            #region M_DICTIONARY/M_DICTIONARY_MANAGER


            #endregion

            return insertQuery_15;
        }

        /// <summary>
        /// Will output a query to update database to version 1.6
        /// </summary>
        /// <returns></returns>
        public List<string> GetUpgradeQueryVersion1_6(string attachedDBName)
        {
            ///Schema v 1.6: 
            ///https://github.com/NRCan/GSC-Field-Application/milestone/6
            List<string> insertQuery_16 = new List<string>();

            #region F_STATION

            Station modelStation = new Station();
            List<string> stationFieldList = modelStation.getFieldList[DBVersion160];
            string station_querySelect = string.Empty;

            foreach (string stationFields in stationFieldList)
            {
                //Get all fields except alias

                if (stationFields != stationFieldList.First())
                {
                    if (stationFields == DatabaseLiterals.FieldStationRelatedTo)
                    {

                        station_querySelect = station_querySelect +
                            ", NULL as " + DatabaseLiterals.FieldStationRelatedTo;
                    }
                    else if (stationFields == DatabaseLiterals.FieldStationObsSource)
                    {

                        station_querySelect = station_querySelect +
                            ", NULL as " + DatabaseLiterals.FieldStationObsSource;
                    }
                    else
                    {
                        station_querySelect = station_querySelect + ", st." + stationFields + " as " + stationFields;
                    }

                }
                else
                {
                    station_querySelect = " st." + stationFields + " as " + stationFields;
                }

            }
            station_querySelect = station_querySelect.Replace(", ,", "");

            string insertQuery_16_station = "INSERT INTO " + DatabaseLiterals.TableStation + " SELECT " + station_querySelect;
            insertQuery_16_station = insertQuery_16_station + " FROM " + attachedDBName + "." + DatabaseLiterals.TableStation + " as st";
            insertQuery_16.Add(insertQuery_16_station);

            #endregion

            #region F_EARTH_MATERIAL
            Earthmaterial modelEarth = new Earthmaterial();
            List<string> earthFieldList = modelEarth.getFieldList[DBVersion160];
            string earth_querySelect = string.Empty;

            foreach (string earthFields in earthFieldList)
            {
                //Get all fields except alias

                if (earthFields != earthFieldList.First())
                {
                    if (earthFields == DatabaseLiterals.FieldEarthMatPercent)
                    {

                        earth_querySelect = earth_querySelect +
                            ", NULL as " + DatabaseLiterals.FieldEarthMatPercent;
                    }
                    else if (earthFields == DatabaseLiterals.FieldEarthMatMetaFacies)
                    {
                        earth_querySelect = earth_querySelect +
                            ", NULL as " + DatabaseLiterals.FieldEarthMatMetaFacies;
                    }
                    else if (earthFields == DatabaseLiterals.FieldEarthMatMetaIntensity)
                    {
                        earth_querySelect = earth_querySelect +
                            ", NULL as " + DatabaseLiterals.FieldEarthMatMetaIntensity;
                    }
                    else if (earthFields == DatabaseLiterals.FieldEarthMatMagQualifier)
                    {
                        earth_querySelect = earth_querySelect +
                            ", NULL as " + DatabaseLiterals.FieldEarthMatMagQualifier;
                    }
                    else if (earthFields == DatabaseLiterals.FieldEarthMatModComp)
                    {
                        earth_querySelect = earth_querySelect +
                            ", et." + DatabaseLiterals.FieldEarthMatModCompDeprecated + " as " + DatabaseLiterals.FieldEarthMatModComp;
                    }
                    else if (earthFields == DatabaseLiterals.FieldEarthMatContact)
                    {
                        earth_querySelect = earth_querySelect +
                            ", et." + DatabaseLiterals.FieldEarthMatContactDeprecated + " as " + DatabaseLiterals.FieldEarthMatContact;
                    }
                    else if (earthFields == DatabaseLiterals.FieldEarthMatModTextStruc)
                    {
                        earth_querySelect = earth_querySelect +
                            ", et." + DatabaseLiterals.FieldEarthMatModStrucDeprecated + " || ' | ' || et." + DatabaseLiterals.FieldEarthMatModTextureDeprecated + " as " + DatabaseLiterals.FieldEarthMatModTextStruc;
                    }
                    else
                    {
                        earth_querySelect = earth_querySelect + ", et." + earthFields + " as " + earthFields;
                    }

                }
                else
                {
                    earth_querySelect = " et." + earthFields + " as " + earthFields;
                }

            }
            earth_querySelect = earth_querySelect.Replace(", ,", "");

            string insertQuery_16_earth = "INSERT INTO " + DatabaseLiterals.TableEarthMat + " SELECT " + earth_querySelect;
            insertQuery_16_earth = insertQuery_16_earth + " FROM " + attachedDBName + "." + DatabaseLiterals.TableEarthMat + " as et";
            insertQuery_16.Add(insertQuery_16_earth);

            //Add some update queries so the textstruc field looks a bit nicer. The | character gets inserted no matter if there is a value or not.
            string updateQuery_16_earth_pipe = "UPDATE " + DatabaseLiterals.TableEarthMat + " SET " + DatabaseLiterals.FieldEarthMatModTextStruc + " = NULL" +
                " WHERE " + DatabaseLiterals.FieldEarthMatModTextStruc + " = ' | ';";

            string updateQuery_16_earth_pipe2 = "UPDATE " + DatabaseLiterals.TableEarthMat + " SET " + DatabaseLiterals.FieldEarthMatModTextStruc +
                " = replace(" + DatabaseLiterals.FieldEarthMatModTextStruc + ", ' | ', '')" + " WHERE " + DatabaseLiterals.FieldEarthMatModTextStruc +
                " LIKE '% | ' OR " + DatabaseLiterals.FieldEarthMatModTextStruc + " LIKE ' | %';";

            #endregion

            #region F_MINERAL
            Mineral modelMineral = new Mineral();
            List<string> mineralFieldList = modelMineral.getFieldList[DBVersion160];
            string mineral_querySelect = string.Empty;

            foreach (string minFields in mineralFieldList)
            {
                //Get all fields except alias

                if (minFields != mineralFieldList.First())
                {
                    if (minFields == DatabaseLiterals.FieldMineralFormHabit)
                    {

                        mineral_querySelect = mineral_querySelect +
                            ", m." + DatabaseLiterals.FieldMineralFormDeprecated + " || '" + KeywordConcatCharacter + "' || m." + DatabaseLiterals.FieldMineralHabitDeprecated + " as " + DatabaseLiterals.FieldMineralFormHabit;
                    }
                    else if (minFields == DatabaseLiterals.FieldMineralMAID)
                    {
                        mineral_querySelect = mineral_querySelect +
                            ", NULL as " + DatabaseLiterals.FieldMineralMAID;
                    }
                    else
                    {
                        mineral_querySelect = mineral_querySelect + ", m." + minFields + " as " + minFields;
                    }

                }
                else
                {
                    mineral_querySelect = " m." + minFields + " as " + minFields;
                }

            }
            mineral_querySelect = mineral_querySelect.Replace(", ,", "");

            string insertQuery_16_mineral = "INSERT INTO " + DatabaseLiterals.TableMineral + " SELECT " + mineral_querySelect;
            insertQuery_16_mineral = insertQuery_16_mineral + " FROM " + attachedDBName + "." + DatabaseLiterals.TableMineral + " as m";
            insertQuery_16.Add(insertQuery_16_mineral);
            #endregion

            #region F_MINERAL COMING FROM F_MINERALIZATION_ALTERATION
            DataIDCalculation idCal = new DataIDCalculation();

            string mineral2_querySelect = string.Empty;

            foreach (string minFields2 in mineralFieldList)
            {
                //Get all fields except alias

                if (minFields2 != mineralFieldList.First())
                {
                    if (minFields2 == DatabaseLiterals.FieldMineralIDName)
                    {

                        mineral2_querySelect = mineral2_querySelect +
                            ", m." + DatabaseLiterals.FieldMineralAlterationName + " || '-mineral'" + " as " + DatabaseLiterals.FieldMineralIDName;
                    }
                    else if (minFields2 == DatabaseLiterals.FieldMineral)
                    {
                        mineral2_querySelect = mineral2_querySelect +
                            ", m." + DatabaseLiterals.FieldMineralAlterationMineralDeprecated + " as " + DatabaseLiterals.FieldMineral;
                    }
                    else if (minFields2 == DatabaseLiterals.FieldMineralMode)
                    {
                        mineral2_querySelect = mineral2_querySelect +
                            ", m." + DatabaseLiterals.FieldMineralAlterationModeDeprecated + " as " + DatabaseLiterals.FieldMineralMode;
                    }
                    else if (minFields2 == DatabaseLiterals.FieldMineralMAID)
                    {
                        mineral2_querySelect = mineral2_querySelect +
                            ", m." + minFields2 + " as " + minFields2;
                    }
                    else
                    {
                        mineral2_querySelect = mineral2_querySelect + ", NULL as " + minFields2;
                    }

                }
                else
                {
                    //Get a proper new GUID
                    mineral2_querySelect = "m." + DatabaseLiterals.FieldMineralAlterationName + " as " + minFields2;
                }

            }
            mineral2_querySelect = mineral2_querySelect.Replace(", ,", "");

            string insertQuery_16_mineral2 = "INSERT INTO " + DatabaseLiterals.TableMineral + " SELECT " + mineral2_querySelect;
            insertQuery_16_mineral2 = insertQuery_16_mineral2 + " FROM " + attachedDBName + "." + DatabaseLiterals.TableMineralAlteration + " as m";
            insertQuery_16.Add(insertQuery_16_mineral2);


            #endregion

            #region F_LOCATION

            FieldLocation modelLocation = new FieldLocation();
            List<string> locationFieldList = modelLocation.getFieldList[DBVersion160];
            string location_querySelect = string.Empty;

            foreach (string locationFields in locationFieldList)
            {
                //Get all fields except alias

                if (locationFields != locationFieldList.First())
                {
                    if (locationFields == DatabaseLiterals.FieldLocationNTS)
                    {

                        location_querySelect = location_querySelect +
                            ", NULL as " + DatabaseLiterals.FieldLocationNTS;
                    }
                    else
                    {
                        location_querySelect = location_querySelect + ", l." + locationFields + " as " + locationFields;
                    }

                }
                else
                {
                    location_querySelect = " l." + locationFields + " as " + locationFields;
                }

            }
            location_querySelect = location_querySelect.Replace(", ,", "");

            string insertQuery_16_location = "INSERT INTO " + DatabaseLiterals.TableLocation + " SELECT " + location_querySelect;
            insertQuery_16_location = insertQuery_16_location + " FROM " + attachedDBName + "." + DatabaseLiterals.TableLocation + " as l";
            insertQuery_16.Add(insertQuery_16_location);

            #endregion

            #region F_MINERALIZATION_ALTERATION

            MineralAlteration modelMA = new MineralAlteration();
            List<string> maFieldList = modelMA.getFieldList[DBVersion160];
            string ma_querySelect = string.Empty;

            foreach (string malocationFields in maFieldList)
            {
                //Get all fields except alias

                if (malocationFields != maFieldList.First())
                {
                    if (malocationFields == DatabaseLiterals.FieldMineralAlterationPhase)
                    {

                        ma_querySelect = ma_querySelect +
                            ", NULL as " + DatabaseLiterals.FieldMineralAlterationPhase;
                    }
                    else if (malocationFields == DatabaseLiterals.FieldMineralAlterationTexture)
                    {

                        ma_querySelect = ma_querySelect +
                            ", NULL as " + DatabaseLiterals.FieldMineralAlterationTexture;
                    }
                    else if (malocationFields == DatabaseLiterals.FieldMineralAlterationFacies)
                    {

                        ma_querySelect = ma_querySelect +
                            ", NULL as " + DatabaseLiterals.FieldMineralAlterationFacies;
                    }
                    else
                    {
                        ma_querySelect = ma_querySelect + ", ma." + malocationFields + " as " + malocationFields;
                    }

                }
                else
                {
                    ma_querySelect = " ma." + malocationFields + " as " + malocationFields;
                }

            }
            ma_querySelect = ma_querySelect.Replace(", ,", "");

            string insertQuery_16_ma = "INSERT INTO " + DatabaseLiterals.TableMineralAlteration + " SELECT " + ma_querySelect;
            insertQuery_16_ma = insertQuery_16_ma + " FROM " + attachedDBName + "." + DatabaseLiterals.TableMineralAlteration + " as ma";
            insertQuery_16.Add(insertQuery_16_ma);

            #endregion

            insertQuery_16.Add(updateQuery_16_earth_pipe);
            insertQuery_16.Add(updateQuery_16_earth_pipe2);

            return insertQuery_16;
        }

        /// <summary>
        /// Will output a query to update database to version 1.7
        /// </summary>
        /// <returns></returns>
        public List<string> GetUpgradeQueryVersion1_7(string attachedDBName)
        {

            ///Schema v 1.7: 
            ///https://github.com/NRCan/GSC-Field-Application/milestone/8
            List<string> insertQuery_17 = new List<string>();
            List<string> nullFieldList = new List<string>() { DatabaseLiterals.FieldGenericRowID , FieldGenericGeometry,
            FieldSampleBucketTray, FieldSampleWarehouseLocation, FieldEarthMatClastForm, FieldEarthMatH2O,
            FieldEarthMatOxidation, FieldEarthMatSorting};

            #region F_METADATA

            Metadata modelMetadata = new Metadata();
            List<string> metadataFieldList = modelMetadata.getFieldList[DBVersion];

            //Get view creation queries to mitigate GUID ids to integer ids.
            string metaView = ViewPrefix + TableMetadata;
            insertQuery_17.Add(GenerateLegacyFormatViews(attachedDBName, TableMetadata, FieldUserInfoID));

            //Get insert query 
            Tuple<string, string> primeMetadata = new Tuple<string, string>(FieldUserInfoID, ViewGenericLegacyPrimeKey);
            insertQuery_17.Add(GenerateInsertQueriesFromModel(metadataFieldList, nullFieldList, TableMetadata, primeMetadata, null, attachedDBName, metaView));

            #endregion

            #region F_LOCATION

            FieldLocation modelLocation = new FieldLocation();
            List<string> locationFieldList = modelLocation.getFieldList[DBVersion170];

            //Get view creation queries to mitigate GUID ids to integer ids.
            string locationView = ViewPrefix + TableLocation;
            insertQuery_17.Add(GenerateLegacyFormatViews(attachedDBName, TableLocation, FieldLocationID,
                FieldLocationMetaID, metaView, FieldUserInfoID));

            //Get insert query 
            Tuple<string, string> primeLocation = new Tuple<string, string>(FieldLocationID, ViewGenericLegacyPrimeKey);
            Tuple<string, string> foreignLocation = new Tuple<string, string>(FieldUserInfoID, ViewGenericLegacyForeignKey);

            insertQuery_17.Add(GenerateInsertQueriesFromModel(locationFieldList, nullFieldList, TableLocation,
                primeLocation, foreignLocation, attachedDBName, locationView));

            #endregion

            #region F_STATION

            Station modelStation = new Station();
            List<string> stationFieldList = modelStation.getFieldList[DBVersion];

            //Get view creation queries to mitigate GUID ids to integer ids.
            insertQuery_17.Add(GenerateLegacyFormatViews(attachedDBName, TableStation, FieldStationID,
                FieldStationObsID, locationView, FieldStationObsID));

            //Get insert query 
            Tuple<string, string> primeStat = new Tuple<string, string>(FieldStationID, ViewGenericLegacyPrimeKey);
            Tuple<string, string> foreignStat = new Tuple<string, string>(FieldStationObsID, ViewGenericLegacyForeignKey);
            string statView = ViewPrefix + TableStation;
            insertQuery_17.Add(GenerateInsertQueriesFromModel(stationFieldList, nullFieldList, TableStation,
                primeStat, foreignStat, attachedDBName, statView));

            #endregion

            #region F_EARTHMAT

            Earthmaterial modelEM = new Earthmaterial();
            List<string> earthmatFieldList = modelEM.getFieldList[DBVersion170];

            //Get view creation queries to mitigate GUID ids to integer ids.
            insertQuery_17.Add(GenerateLegacyFormatViews(attachedDBName, TableEarthMat, FieldEarthMatID,
                FieldEarthMatStatID, statView, FieldEarthMatStatID));

            //Get insert query 
            Tuple<string, string> primeEarth = new Tuple<string, string>(FieldEarthMatID, ViewGenericLegacyPrimeKey);
            Tuple<string, string> foreignEarth = new Tuple<string, string>(FieldEarthMatStatID, ViewGenericLegacyForeignKey);
            string earthView = ViewPrefix + TableEarthMat;
            insertQuery_17.Add(GenerateInsertQueriesFromModel(earthmatFieldList, nullFieldList, TableEarthMat,
                primeEarth, foreignEarth, attachedDBName, earthView));

            #endregion

            #region F_SAMPLE

            Sample modelSample = new Sample();
            List<string> sampleFieldList = modelSample.getFieldList[DBVersion170];

            //Get view creation queries to mitigate GUID ids to integer ids.
            insertQuery_17.Add(GenerateLegacyFormatViews(attachedDBName, TableSample, FieldSampleID,
                FieldSampleEarthmatID, earthView, FieldSampleEarthmatID));

            //Get insert query 
            Tuple<string, string> primeSample = new Tuple<string, string>(FieldSampleID, ViewGenericLegacyPrimeKey);
            Tuple<string, string> foreignSample = new Tuple<string, string>(FieldSampleEarthmatID, ViewGenericLegacyForeignKey);
            string sampleView = ViewPrefix + TableSample;
            insertQuery_17.Add(GenerateInsertQueriesFromModel(sampleFieldList, nullFieldList, TableSample,
                primeSample, foreignSample, attachedDBName, sampleView));

            #endregion

            #region F_STRUCTURE

            Structure modelStructure = new Structure();
            List<string> structureFieldList = modelStructure.getFieldList[DBVersion170];

            //Get view creation queries to mitigate GUID ids to integer ids.
            insertQuery_17.Add(GenerateLegacyFormatViews(attachedDBName, TableStructure, FieldStructureID,
                FieldStructureParentID, earthView, FieldStructureParentID));

            //Get insert query 
            Tuple<string, string> primeStructure = new Tuple<string, string>(FieldStructureID, ViewGenericLegacyPrimeKey);
            Tuple<string, string> foreignStructure = new Tuple<string, string>(FieldStructureParentID, ViewGenericLegacyForeignKey);
            string strucView = ViewPrefix + TableStructure;
            insertQuery_17.Add(GenerateInsertQueriesFromModel(structureFieldList, nullFieldList, TableStructure,
                primeStructure, foreignStructure, attachedDBName, strucView));

            #endregion

            #region F_PALEO_FLOW

            Paleoflow modelPFlow = new Paleoflow();
            List<string> pflowFieldList = modelPFlow.getFieldList[DBVersion];

            //Get view creation queries to mitigate GUID ids to integer ids.
            insertQuery_17.Add(GenerateLegacyFormatViews(attachedDBName, TablePFlow, FieldPFlowID,
                FieldPFlowParentID, earthView, FieldPFlowParentID));

            //Get insert query 
            Tuple<string, string> primePflow = new Tuple<string, string>(FieldPFlowID, ViewGenericLegacyPrimeKey);
            Tuple<string, string> foreignPflow = new Tuple<string, string>(FieldPFlowParentID, ViewGenericLegacyForeignKey);
            string pflowView = ViewPrefix + TablePFlow;
            insertQuery_17.Add(GenerateInsertQueriesFromModel(pflowFieldList, nullFieldList, TablePFlow,
                primePflow, foreignPflow, attachedDBName, pflowView));

            #endregion

            #region F_ENVIRONMENT

            EnvironmentModel modelEnv = new EnvironmentModel();
            List<string> envFieldList = modelEnv.getFieldList[DBVersion];

            //Get view creation queries to mitigate GUID ids to integer ids.
            insertQuery_17.Add(GenerateLegacyFormatViews(attachedDBName, TableEnvironment, FieldEnvID,
                FieldEnvStationID, statView, FieldEnvStationID));

            //Get insert query 
            Tuple<string, string> primeEnv = new Tuple<string, string>(FieldEnvID, ViewGenericLegacyPrimeKey);
            Tuple<string, string> foreignEnv = new Tuple<string, string>(FieldEnvStationID, ViewGenericLegacyForeignKey);
            string envView = ViewPrefix + TableEnvironment;

            insertQuery_17.Add(GenerateInsertQueriesFromModel(envFieldList, nullFieldList, TableEnvironment,
                primeEnv, foreignEnv, attachedDBName, envView));

            #endregion

            #region F_MINERALIZATION_ALTERAION

            ///Warning: We assumed that by default records will be linked with station

            MineralAlteration modelMA = new MineralAlteration();
            List<string> maFieldList = modelMA.getFieldList[DBVersion170];

            //Get view creation queries to mitigate GUID ids to integer ids.
            insertQuery_17.Add(GenerateLegacyFormatViews(attachedDBName, TableMineralAlteration, FieldMineralAlterationID,
                FieldMineralAlterationRelIDDeprecated, statView, FieldStationID));

            //Get insert query 
            Tuple<string, string> primeMA = new Tuple<string, string>(FieldMineralAlterationID, ViewGenericLegacyPrimeKey);
            Tuple<string, string> foreignMA = new Tuple<string, string>(FieldMineralAlterationRelIDDeprecated, ViewGenericLegacyForeignKey);
            string MAView = ViewPrefix + TableMineralAlteration;

            insertQuery_17.Add(GenerateInsertQueriesFromModel(maFieldList, nullFieldList, TableMineralAlteration,
                primeMA, foreignMA, attachedDBName, MAView));

            #endregion

            #region F_MINERAL

            Mineral modelMineral = new Mineral();
            List<string> mineralFieldList = modelMineral.getFieldList[DBVersion];

            //Get view creation queries to mitigate GUID ids to integer ids.
            string MineralView = ViewPrefix + TableMineral;

            insertQuery_17.Add(GenerateLegacyFormatViews(attachedDBName, TableMineral, FieldMineralID,
                FieldMineralMAID, MAView, FieldMineralAlterationID));
            insertQuery_17.Add(GenerateLegacyFormatViews(attachedDBName, TableMineral, FieldMineralID,
                FieldMineralEMID, earthView, FieldEarthMatID, MineralView + "_2"));

            //Get insert query 
            Tuple<string, string> primeMineral = new Tuple<string, string>(FieldMineralID, ViewGenericLegacyPrimeKey);
            Tuple<string, string> foreignMineral = new Tuple<string, string>(FieldMineralMAID, ViewGenericLegacyForeignKey);
            Tuple<string, string> foreignMineral2 = new Tuple<string, string>(FieldEarthMatID, ViewGenericLegacyForeignKey);


            insertQuery_17.Add(GenerateInsertQueriesFromModel(mineralFieldList, nullFieldList, TableMineral,
                primeMineral, foreignMineral, attachedDBName, MineralView));
            insertQuery_17.Add(GenerateInsertQueriesFromModel(mineralFieldList, nullFieldList, TableMineral,
                primeMineral, foreignMineral2, attachedDBName, MineralView + "_2"));

            #endregion

            #region F_FOSSIL

            Fossil modelFossil = new Fossil();
            List<string> fossilFieldList = modelFossil.getFieldList[DBVersion];

            //Get view creation queries to mitigate GUID ids to integer ids.
            insertQuery_17.Add(GenerateLegacyFormatViews(attachedDBName, TableFossil, FieldFossilID,
                FieldFossilParentID, earthView, FieldFossilParentID));

            //Get insert query 
            Tuple<string, string> primeFossil = new Tuple<string, string>(FieldFossilID, ViewGenericLegacyPrimeKey);
            Tuple<string, string> foreignFossil = new Tuple<string, string>(FieldFossilParentID, ViewGenericLegacyForeignKey);
            string fossilView = ViewPrefix + TableFossil;

            insertQuery_17.Add(GenerateInsertQueriesFromModel(fossilFieldList, nullFieldList, TableFossil,
                primeFossil, foreignFossil, attachedDBName, fossilView));

            #endregion

            #region F_DOCUMENT

            ///Warning: We assumed that by default records will be linked with station

            Document modelDocument = new Document();
            List<string> documentFieldList = modelDocument.getFieldList[DBVersion170];

            //Get view creation queries to mitigate GUID ids to integer ids.
            insertQuery_17.Add(GenerateLegacyFormatViews(attachedDBName, TableDocument, FieldDocumentID,
                FieldDocumentRelatedIDDeprecated, statView, FieldStationID));

            //Get insert query 
            Tuple<string, string> primeDoc = new Tuple<string, string>(FieldDocumentID, ViewGenericLegacyPrimeKey);
            Tuple<string, string> foreignDoc = new Tuple<string, string>(FieldDocumentRelatedIDDeprecated, ViewGenericLegacyForeignKey);
            string docView = ViewPrefix + TableDocument;

            insertQuery_17.Add(GenerateInsertQueriesFromModel(documentFieldList, nullFieldList, TableDocument,
                primeDoc, foreignDoc, attachedDBName, docView));

            #endregion

            #region CLEANING

            //Since inserts have been applied in new database, there'll be some duplicate rows
            //make sure to remove those old rows before continuing.
            string deletQuery = "DELETE FROM " + DatabaseLiterals.TableMetadata + " as fm WHERE length(fm." + FieldUserInfoID + ") = 36;";
            insertQuery_17.Add(deletQuery);

            #endregion

            return insertQuery_17;
        }

        /// <summary>
        /// Will generate a query to create a temp view that will help
        /// create integer based prime and foreign keys. Used for legacy data format (sqlite to geopackage)
        /// </summary>
        /// <param name="tableName">Name of the table to create a view from</param>
        /// <param name="primeKey">Field name of the prime key </param>
        /// <param name="foreignKey">Field name of the foreign key (optional)</param>
        /// <param name="relatedView">View name of for relationship to get foreign key(optional)</param>
        /// <param name="relatedPrimeKey">Field name of the original foreign key in related view (optional)</param>
        /// <returns></returns>
        public string GenerateLegacyFormatViews(string attachedDBName, string tableName, string primeKey, string foreignKey = "", string relatedView = "", string relatedPrimeKey = "", string viewName = "")
        {
            //Top parent view meant to look like
            ///create view view_metadata as
            ///select f.*, ROW_NUMBER() over(order by f.METAID) as PRIME from F_METADATA as f ;

            //Child table views meant to look like
            ///create view view_location as
            ///select f.*, ROW_NUMBER() over(order by f.LOCATIONID) as PRIME, v.PRIME as FORE from F_LOCATION as f
            ///join view_metadata as v where f.METAID = v.METAID ;

            string tableAlias = "f";
            string viewAlias = "v";

            string outputQueryView = "CREATE VIEW " + attachedDBName + "." + ViewPrefix + tableName + " as SELECT " + tableAlias + ".*, ROW_NUMBER() OVER(ORDER BY f." +
                primeKey + ") as " + ViewGenericLegacyPrimeKey;

            //Special case for location that gets data from station also
            if (tableName == TableLocation)
            {
                outputQueryView = outputQueryView + ", tst." + FieldStationReportLinkDeprecated + " as " + FieldLocationReportLink;
            }

            //Rare case when a second view is needed for multiple relationship
            if (viewName != String.Empty)
            {
                outputQueryView = "CREATE VIEW " + attachedDBName + "." + viewName + " as SELECT " + tableAlias + ".*, (ROW_NUMBER() OVER(ORDER BY f." +
                primeKey + ")) + (SELECT COUNT(*) FROM " + ViewPrefix + tableName + ") as " + ViewGenericLegacyPrimeKey;
            }

            if (relatedView != string.Empty)
            {
                if (tableName == TableLocation)
                {
                    //Join to another view
                    outputQueryView = outputQueryView + ", " + viewAlias + "." + ViewGenericLegacyPrimeKey + " as " + ViewGenericLegacyForeignKey + " FROM " + tableName + " as " + tableAlias +
                    " JOIN " + relatedView + " as " + viewAlias + " ON " + tableAlias + "." + foreignKey + " = " + viewAlias + "." + relatedPrimeKey +
                    " JOIN " + TableStation + " as tst ON " + "tst." + FieldStationObsID + " = " + tableAlias + "." + primeKey + ";";
                }
                else
                {
                    //Join to another view
                    outputQueryView = outputQueryView + ", " + viewAlias + "." + ViewGenericLegacyPrimeKey + " as " + ViewGenericLegacyForeignKey + " FROM " + tableName + " as " + tableAlias +
                    " JOIN " + relatedView + " as " + viewAlias + " ON " + tableAlias + "." + foreignKey + " = " + viewAlias + "." + relatedPrimeKey + ";";
                }

            }
            else
            {

                //Close top parent query
                outputQueryView = outputQueryView + " FROM " + tableName + " as " + tableAlias + ";";

            }


            return outputQueryView;
        }

        /// <summary>
        /// From a given list of fields, it'll produce an insert query
        /// Mainly used for database schema upgrade
        /// </summary>
        /// <param name="fieldList">All field names that needs to be added to insert query</param>
        /// <param name="fieldNullList">Field names that needs to be null since they aren't in the new database</param>
        /// <param name="tableName">Table name to insert into</param>
        /// <param name="attachedDBName">An attached database name if needed</param>
        /// <param name="differentSourceTable">Specify a new table name to insert from, if not like original</param>
        /// <param name="foreignKeyMapping">Specify the original foreign key field and the new field name of if insert doesn't come from same table</param>
        /// <param name="primeKeyMapping">Specify the original prime key field and the new field name of if insert doesn't come from the same table</param>
        /// <returns></returns>
        public string GenerateInsertQueriesFromModel(List<string> fieldList, List<string> fieldNullList, string tableName,
            Tuple<string, string> primeKeyMapping, Tuple<string, string> foreignKeyMapping,
            string attachedDBName = "", string differentSourceTable = "")
        {

            //Variables
            DataIDCalculation did = new DataIDCalculation();
            Random ran = new Random();

            string query_select = string.Empty;
            string alias = did.CalculateAlphabeticID(true, ran.Next(1, 50));

            // Skip possible restricted keyword
            if (alias == "AS")
            {
                alias = did.CalculateAlphabeticID(true, ran.Next(1, 50));
            }
            string incrementChar = string.Empty;

            //Iterate through field name list
            foreach (string fields in fieldList)
            {
                //Get all fields except alias
                if (fields != fieldList.First() && incrementChar == string.Empty)
                {
                    incrementChar = ",";
                }

                //Manage fields that needs to be set as null
                if (fieldNullList.Contains(fields))
                {
                    query_select = query_select + incrementChar + " NULL as " + fields;
                }
                else
                {
                    ///Deal with recalculated prime keys for legacy data format (GUID to INT)

                    //If hits on prime key
                    if (primeKeyMapping != null && primeKeyMapping.Item1 == fields)
                    {
                        query_select = query_select + incrementChar + " " + alias + "." + primeKeyMapping.Item2 + " as " + fields;
                    }
                    //If hits on foreign key
                    else if (foreignKeyMapping != null && foreignKeyMapping.Item1 == fields)
                    {
                        query_select = query_select + incrementChar + " " + alias + "." + foreignKeyMapping.Item2 + " as " + fields;
                    }
                    else
                    {
                        query_select = query_select + incrementChar + " " + alias + "." + fields + " as " + fields;

                    }

                }



            }

            //Remove empty values
            query_select = query_select.Replace(", ,", "");

            //Build full query
            string insert_query = "INSERT INTO " + tableName + " SELECT " + query_select;

            if (attachedDBName != string.Empty)
            {
                if (differentSourceTable != string.Empty)
                {
                    insert_query = insert_query + " FROM " + attachedDBName + "." + differentSourceTable + " as " + alias;
                }
                else
                {
                    insert_query = insert_query + " FROM " + attachedDBName + "." + tableName + " as " + alias;
                }

            }
            else
            {
                if (differentSourceTable != string.Empty)
                {
                    insert_query = insert_query + " FROM " + differentSourceTable + " as " + alias;
                }
                else
                {
                    insert_query = insert_query + " FROM " + tableName + " as " + alias;
                }

            }


            return insert_query;
        }

        /// <summary>
        /// Will output a query to update database to version 1.8
        /// </summary>
        /// <param name="attachedDBName"></param>
        /// <returns></returns>
        public List<string> GetUpgradeQueryVersion1_8(string attachedDBName)
        {
            ///Schema v 1.8: 
            ///https://github.com/NRCan/GSC-Field-Application/milestone/10
            List<string> insertQuery_18 = new List<string>();

            #region F_SAMPLE

            Sample modelSample = new Sample();
            List<string> sampleFieldList = modelSample.getFieldList[DBVersion180];
            string sample_querySelect = string.Empty;

            foreach (string sampleFields in sampleFieldList)
            {
                //Get all fields except alias

                if (sampleFields != sampleFieldList.First())
                {
                    if (sampleFields == DatabaseLiterals.FieldSampleIsBlank)
                    {
                        sample_querySelect = sample_querySelect +
                            ", NULL as " + DatabaseLiterals.FieldSampleIsBlank;
                    }
                    else if (sampleFields == DatabaseLiterals.FieldSampleCoreFrom)
                    {
                        sample_querySelect = sample_querySelect +
                            ", NULL as " + DatabaseLiterals.FieldSampleCoreFrom;
                    }
                    else if (sampleFields == DatabaseLiterals.FieldSampleCoreTo)
                    {
                        sample_querySelect = sample_querySelect +
                            ", NULL as " + DatabaseLiterals.FieldSampleCoreTo;
                    }
                    else if (sampleFields == DatabaseLiterals.FieldSampleCoreLength)
                    {
                        sample_querySelect = sample_querySelect +
                            ", NULL as " + DatabaseLiterals.FieldSampleCoreLength;
                    }
                    else if (sampleFields == DatabaseLiterals.FieldSampleCoreSize)
                    {
                        sample_querySelect = sample_querySelect +
                            ", NULL as " + DatabaseLiterals.FieldSampleCoreSize;
                    }
                    else if (sampleFields == DatabaseLiterals.FieldSampledBy)
                    {
                        sample_querySelect = sample_querySelect +
                            ", NULL as " + DatabaseLiterals.FieldSampledBy;
                    }
                    else
                    {
                        sample_querySelect = sample_querySelect + ", sm." + sampleFields + " as " + sampleFields;
                    }

                }
                else
                {
                    sample_querySelect = " sm." + sampleFields + " as " + sampleFields;
                }

            }
            sample_querySelect = sample_querySelect.Replace(", ,", "");

            string insertQuery_18_sample = "INSERT INTO " + DatabaseLiterals.TableSample + " SELECT " + sample_querySelect;
            insertQuery_18_sample = insertQuery_18_sample + " FROM " + attachedDBName + "." + DatabaseLiterals.TableSample + " as sm";
            insertQuery_18.Add(insertQuery_18_sample);

            #endregion

            #region F_MINERALIZATION_ALTERATION

            MineralAlteration modelMA = new MineralAlteration();
            List<string> maFieldList = modelMA.getFieldList[DBVersion];
            string ma_querySelect = string.Empty;

            foreach (string maFields in maFieldList)
            {
                //Get all fields except alias

                if (maFields != maFieldList.First())
                {
                    if (maFields == DatabaseLiterals.FieldMineralAlterationEarthmatID)
                    {
                        ma_querySelect = ma_querySelect +
                            ", NULL as " + DatabaseLiterals.FieldMineralAlterationEarthmatID;
                    }
                    else if (maFields == DatabaseLiterals.FieldMineralAlterationStationID)
                    {
                        ma_querySelect = ma_querySelect +
                            ", " + DatabaseLiterals.FieldMineralAlterationRelIDDeprecated + " as " + DatabaseLiterals.FieldMineralAlterationStationID;
                    }
                    else
                    {
                        ma_querySelect = ma_querySelect + ", ma." + maFields + " as " + maFields;
                    }

                }
                else
                {
                    ma_querySelect = " ma." + maFields + " as " + maFields;
                }

            }
            ma_querySelect = ma_querySelect.Replace(", ,", "");

            string insertQuery_18_ma = "INSERT INTO " + DatabaseLiterals.TableMineralAlteration + " SELECT " + ma_querySelect;
            insertQuery_18_ma = insertQuery_18_ma + " FROM " + attachedDBName + "." + DatabaseLiterals.TableMineralAlteration + " as ma";
            insertQuery_18.Add(insertQuery_18_ma);

            #endregion

            #region F_DOCUMENT

            Document modelDocument = new Document();
            List<string> documentFieldList = modelDocument.getFieldList[DBVersion];
            string document_querySelect = string.Empty;

            foreach (string docFields in documentFieldList)
            {
                //Get all fields except alias

                if (docFields != documentFieldList.First())
                {
                    if (docFields == DatabaseLiterals.FieldDocumentSampleID)
                    {
                        document_querySelect = document_querySelect +
                            ", NULL as " + DatabaseLiterals.FieldDocumentSampleID;
                    }
                    else if (docFields == DatabaseLiterals.FieldDocumentDrillHoleID)
                    {
                        document_querySelect = document_querySelect +
                            ", NULL as " + DatabaseLiterals.FieldDocumentDrillHoleID;
                    }
                    else if (docFields == DatabaseLiterals.FieldDocumentStationID)
                    {
                        document_querySelect = document_querySelect +
                            ", " + DatabaseLiterals.FieldDocumentRelatedIDDeprecated + " as " + DatabaseLiterals.FieldDocumentStationID;
                    }
                    else if (docFields == DatabaseLiterals.FieldDocumentScaleDirection)
                    {
                        document_querySelect = document_querySelect +
                            ", NULL as " + DatabaseLiterals.FieldDocumentScaleDirection;
                    }
                    else if (docFields == DatabaseLiterals.FieldDocumentEarthMatID)
                    {
                        document_querySelect = document_querySelect +
                            ", NULL as " + DatabaseLiterals.FieldDocumentEarthMatID;
                    }
                    else
                    {
                        document_querySelect = document_querySelect + ", d." + docFields + " as " + docFields;
                    }

                }
                else
                {
                    document_querySelect = " d." + docFields + " as " + docFields;
                }

            }
            document_querySelect = document_querySelect.Replace(", ,", "");

            string insertQuery_18_doc = "INSERT INTO " + DatabaseLiterals.TableDocument + " SELECT " + document_querySelect;
            insertQuery_18_doc = insertQuery_18_doc + " FROM " + attachedDBName + "." + DatabaseLiterals.TableDocument + " as d";
            insertQuery_18.Add(insertQuery_18_doc);

            #endregion

            #region F_LOCATION

            FieldLocation modelLocation = new FieldLocation();
            List<string> locationFieldList = modelLocation.getFieldList[DBVersion];
            string location_querySelect = string.Empty;

            foreach (string locFields in locationFieldList)
            {
                //Get all fields except alias

                if (locFields != locationFieldList.First())
                {
                    if (locFields == DatabaseLiterals.FieldLocationEPSGProj)
                    {
                        //If something other then 4326 is found, copy it to this new field, else keep null, it's already in the good format
                        location_querySelect = location_querySelect +
                        ", (CASE WHEN(l." + DatabaseLiterals.FieldLocationDatum + " NOT LIKE '%4326%') THEN(l." + DatabaseLiterals.FieldLocationDatum + ") ELSE(NULL" +
                            ") END) as " + DatabaseLiterals.FieldLocationEPSGProj;
                    }
                    else if (locFields == DatabaseLiterals.FieldLocationDatum)
                    {
                        //Enforce default EPSG
                        location_querySelect = location_querySelect +
                        ", '4326' as " + DatabaseLiterals.FieldLocationDatum;
                    }
                    else if (locFields == DatabaseLiterals.FieldLocationTimestamp)
                    {
                        //Initiate timestamp with station visit date
                        location_querySelect = location_querySelect +
                        ", (CASE WHEN(s." + DatabaseLiterals.FieldStationVisitDate +
                        " IS NOT NULL) THEN(s." + DatabaseLiterals.FieldStationVisitDate + "|| ' ' ||" +
                        DatabaseLiterals.FieldStationVisitTime + ") ELSE(NULL) END) as " + DatabaseLiterals.FieldLocationTimestamp;
                    }
                    else
                    {
                        location_querySelect = location_querySelect + ", l." + locFields + " as " + locFields;
                    }

                }
                else
                {
                    location_querySelect = " l." + locFields + " as " + locFields;
                }

            }
            location_querySelect = location_querySelect.Replace(", ,", "");

            string insertQuery_18_location = "INSERT INTO " + DatabaseLiterals.TableLocation + " SELECT " + location_querySelect;
            string insertQuery_18_locationJOin = " JOIN dbUpgrade." + DatabaseLiterals.TableStation + " as s on s." + FieldLocationID + " = l." + FieldStationObsID;
            insertQuery_18_location = insertQuery_18_location + " FROM " + attachedDBName + "." + DatabaseLiterals.TableLocation + " as l" + insertQuery_18_locationJOin;

            insertQuery_18.Add(insertQuery_18_location);

            #endregion

            #region F_EARTH_MATERIAL
            Earthmaterial modelEarth = new Earthmaterial();
            List<string> earthFieldList = modelEarth.getFieldList[DBVersion180];
            string earth_querySelect = string.Empty;

            foreach (string earthFields in earthFieldList)
            {
                //Get all fields except alias

                if (earthFields != earthFieldList.First())
                {
                    if (earthFields == DatabaseLiterals.FieldEarthMatDrillHoleID)
                    {

                        earth_querySelect = earth_querySelect +
                            ", NULL as " + DatabaseLiterals.FieldEarthMatDrillHoleID;
                    }
                    else if (earthFields == DatabaseLiterals.FieldEarthMatContactNote)
                    {

                        earth_querySelect = earth_querySelect +
                            ", NULL as " + DatabaseLiterals.FieldEarthMatContactNote;
                    }
                    else
                    {
                        earth_querySelect = earth_querySelect + ", et." + earthFields + " as " + earthFields;
                    }

                }
                else
                {
                    earth_querySelect = " et." + earthFields + " as " + earthFields;
                }

            }
            earth_querySelect = earth_querySelect.Replace(", ,", "");

            string insertQuery_18_earth = "INSERT INTO " + DatabaseLiterals.TableEarthMat + " SELECT " + earth_querySelect;
            insertQuery_18_earth = insertQuery_18_earth + " FROM " + attachedDBName + "." + DatabaseLiterals.TableEarthMat + " as et";
            insertQuery_18.Add(insertQuery_18_earth);
            #endregion

            return insertQuery_18;
        }

        /// <summary>
        /// Will output a query to update database to version 1.9
        /// </summary>
        /// <param name="attachedDBName"></param>
        /// <returns></returns>
        public List<string> GetUpgradeQueryVersion1_9(string attachedDBName)
        {
            //Schema v 1.9: 
            //https://github.com/NRCan/GSC-Field-Application/milestone/13
            List<string> insertQuery_19 = new List<string>();

            #region F_SAMPLE

            Sample modelSample = new Sample();
            List<string> sampleFieldList = modelSample.getFieldList[DBVersion190];
            List<string> sampleNullFieldList = new List<string>() { FieldSampleFrostBoil };
            Tuple<string, string> samplePrimes = new Tuple<string, string>(FieldSampleID, FieldSampleID );
            insertQuery_19.Add(GenerateInsertQueriesFromModel(sampleFieldList, sampleNullFieldList, TableSample, samplePrimes, null, attachedDBName));

            #endregion

            #region F_EARTH_MATERIAL

            Earthmaterial modelEM = new Earthmaterial();
            List<string> EMFieldList = modelEM.getFieldList[DBVersion190];
            List<string> emNullFieldList = new List<string>() { FieldEarthMatDepthMin, FieldEarthMatDepthMax };
            Tuple<string, string> emPrimes = new Tuple<string, string>(FieldEarthMatID, FieldEarthMatID);
            insertQuery_19.Add(GenerateInsertQueriesFromModel(EMFieldList, emNullFieldList, TableEarthMat, emPrimes, null, attachedDBName));

            #endregion

            #region F_STRUCTURE

            Structure modelStructure = new Structure();
            List<string> strucFieldList = modelStructure.getFieldList[DBVersion190];
            List<string> strucNullFieldList = new List<string>() { FieldStructureDepth , FieldStructurePlotToMap};
            Tuple<string, string> strucPrimes = new Tuple<string, string>(FieldStructureID, FieldStructureID);
            insertQuery_19.Add(GenerateInsertQueriesFromModel(strucFieldList, strucNullFieldList, TableStructure, strucPrimes, null, attachedDBName));

            #endregion

            return insertQuery_19;
        }
        #endregion

    }
}
