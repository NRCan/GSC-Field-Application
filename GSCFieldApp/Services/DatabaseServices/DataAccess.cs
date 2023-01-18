using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using GSCFieldApp.Models;
using static GSCFieldApp.Dictionaries.DatabaseLiterals;
using Windows.UI.Xaml.Controls;
using GSCFieldApp.Dictionaries;
using Windows.UI.Xaml;
using SQLite;
using Windows.UI.Core;
using Windows.ApplicationModel.Resources;
using System.Diagnostics;

// Based on code sample from: http://blogs.u2u.be/diederik/post/2015/09/08/Using-SQLite-on-the-Universal-Windows-Platform.aspx -Kaz

namespace GSCFieldApp.Services.DatabaseServices
{
    public class DataAccess
    {

        //ApplicationDataContainer currentLocalSettings = ApplicationData.Current.LocalSettings;
        public static DataLocalSettings localSetting;
        public ResourceLoader local = null;

        public static string _dbPath = string.Empty;
        public static string _dbName = string.Empty;
        public static SQLiteConnection _dbConnection;
        /// <summary>
        /// Constructor
        /// </summary>
        public DataAccess()
        {
            if (localSetting == null)
            {
                localSetting = new DataLocalSettings();
            }

            local = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
        }

        #region DB MANAGEMENT METHODS
        /// <summary>
        /// Will return the full path of the current fieldworkd sqlite database.
        /// </summary>
        public static string DbPath
        {
            get
            {
                _dbName = Dictionaries.DatabaseLiterals.DBName + Dictionaries.DatabaseLiterals.DBTypeSqlite;

                if (string.IsNullOrEmpty(_dbPath))
                {
                    _dbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, _dbName);

                    //For multiple project management
                    object _fieldbookPath = localSetting.GetSettingValue(Dictionaries.ApplicationLiterals.KeywordFieldProject);
                    if (_fieldbookPath != null)
                    {
                        _dbPath = Path.Combine(_fieldbookPath.ToString(), _dbName);
                    }
                    else
                    {
                        _dbPath = Path.Combine(Path.Combine(ApplicationData.Current.LocalFolder.Path, "1"), _dbName);
                    }
                }
                return _dbPath;
            }

            set
            {
                if (value != null && value != string.Empty)
                {
                    _dbPath = value;
                }

            }


        }

        public string ProjectPath
        {
            get
            {
                return Path.GetDirectoryName(DbPath);
            }
        }

        /// <summary>
        /// Get a sqlite connection object
        /// </summary>
        private static SQLiteConnection DbConnection
        {
            get
            {
                if (_dbConnection == null)
                {
                    return new SQLiteConnection(DbPath);
                }
                else
                {
                    return _dbConnection;
                }

            }
        }

        public SQLiteConnection GetConnectionFromPath(string inPath)
        {
            return new SQLiteConnection(inPath);
        }

        /// <summary>
        /// Will return true of false if default database exists in the system.
        /// </summary>
        /// <returns>True if exists</returns>
        public bool DoesDatabaseExists()
        {
            return System.IO.File.Exists(DbPath);
        }

        #endregion

        #region DATA MANAGEMENT METHODS (Create, Update, Read)
        /// <summary>
        /// NOTE: DEPRECATED
        /// Creates a new database with the basic user info table inside it. 
        /// TODO: Needs to be more generic - Gab
        /// </summary>
        public void CreateDatabase()
        {
            using (var db = DbConnection)
            {
                var c = db.CreateTable<Metadata>();
                db.Close();
            }
        }

        /// <summary>
        /// Will create the fieldworkd sqlite database from an embedded resource
        /// </summary>
        /// <returns></returns>
        public async Task CreateDatabaseFromResource()
        {
            await PrepNewFieldBookDatabase(_dbName, "ModelResources/" + _dbName);

        }

        /// <summary>
        /// Will create the fieldworkd sqlite database from an embedded resource
        /// </summary>
        /// <returns></returns>
        public async Task CreateDatabaseFromResourceTo(string toFolderPath, string dbName )
        {

            StorageFolder folderPath = await StorageFolder.GetFolderFromPathAsync(toFolderPath);

            await WriteResourceToFile(folderPath, "ModelResources/" + dbName, dbName);

        }

        /// <summary>
        /// Will write an embedded resource to a file with a binary writer. In case it exists, it will replace it.
        /// Will save the resource to the local folder.
        /// </summary>
        /// <param name="outFileNameWithExt">The outut file name, ex: "GSCFieldwork.sqlite"</param>
        /// <param name="inApplicationPathToFile">The folder inside the project containing the file to copy and the file itself, ex: ModelResources/GSCFieldWork.sqlite</param>
        public async Task WriteResourceToFile(StorageFolder inFolder, string inApplicationPathToFile, string newFileNameWithExt)
        {
            try
            {
                // Create or overwrite file target file in local app data folder
                StorageFile fileToWrite = await inFolder.CreateFileAsync(newFileNameWithExt, CreationCollisionOption.GenerateUniqueName);

                // Open file in application package
                StorageFile fileToRead = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///" + inApplicationPathToFile, UriKind.Absolute));

                byte[] buffer = new byte[1024];
                using (BinaryWriter fileWriter = new BinaryWriter(await fileToWrite.OpenStreamForWriteAsync()))
                {
                    using (BinaryReader fileReader = new BinaryReader(await fileToRead.OpenStreamForReadAsync()))
                    {
                        long readCount = 0;
                        while (readCount < fileReader.BaseStream.Length)
                        {
                            int read = fileReader.Read(buffer, 0, buffer.Length);
                            readCount += read;
                            fileWriter.Write(buffer, 0, read);
                        }
                    }
                }


                System.Runtime.InteropServices.Marshal.ReleaseComObject(fileToWrite);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(fileToRead);
            }
            catch (Exception e)
            {
                throw;
            }


        }

        /// <summary>
        /// Will write an embedded resource to a file with a binary writer. In case it exists, it will replace it.
        /// Will save the resource to the local folder.
        /// </summary>
        /// <param name="outFileNameWithExt">The outut file name, ex: "GSCFieldwork.sqlite"</param>
        /// <param name="inApplicationPathToFile">The folder inside the project containing the file to copy and the file itself, ex: ModelResources/GSCFieldWork.sqlite</param>
        public async Task PrepNewFieldBookDatabase(string outFileNameWithExt, string inApplicationPathToFile)
        {
            //Create field project hierarchy folder in local state
            int incrementer = 1; //Will be used to name project folders (pretty basic)
            string fieldProjectPath = Path.Combine(ApplicationData.Current.LocalFolder.Path); //Wanted path for project data
            StorageFolder fieldFolder = await StorageFolder.GetFolderFromPathAsync(fieldProjectPath);//Current folder object to local state
 
            bool breaker = false; //Will be used to break while clause whenever a folder has been created.
            while (!breaker)
            {
                //If wanted project folder doesn't exist create, else find another name
                if (!Directory.Exists(fieldProjectPath))
                {
                    fieldFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(incrementer.ToString());

                    //Keep in setting the latest path
                    localSetting.SetSettingValue(Dictionaries.ApplicationLiterals.KeywordFieldProject, fieldFolder.Path);

                    breaker = true;
                }
                else
                {
                    incrementer++;
                }

                fieldProjectPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, incrementer.ToString());
            }


            // Create or overwrite file target file in local app data folder
            await WriteResourceToFile(fieldFolder, inApplicationPathToFile, outFileNameWithExt);

            _dbPath = Path.Combine(fieldFolder.Path, outFileNameWithExt);

            //Keep in setting the latest path
            localSetting.SetSettingValue(Dictionaries.ApplicationLiterals.KeywordFieldProject, fieldProjectPath);
            ApplicationData.Current.SignalDataChanged();

            System.Runtime.InteropServices.Marshal.ReleaseComObject(fieldFolder);

        }

        /// <summary>
        /// Will save with an update or an insert operation a given ojbect inside the fieldwork database.
        /// 
        /// </summary>
        /// <param name="tableObject"></param>
        public void SaveFromSQLTableObject(object tableObject, bool doUpdate)
        {
            SaveSQLTableObjectFromDB(tableObject, doUpdate, DbConnection);
        }

        /// <summary>
        /// Will save with an update or an insert operation a given ojbect inside the fieldwork database.
        /// 
        /// </summary>
        /// <param name="tableObject"></param>
        public void SaveSQLTableObjectFromDB(object tableObject, bool doUpdate, SQLiteConnection inDB)
        {
            // Create a new connection
            using (inDB)
            {
                try
                {
                    inDB.RunInTransaction(() =>
                    {
                        //For debug
                        inDB.Tracer = new Action<string>(q => Debug.WriteLine(q));
                        inDB.Trace = true;

                        if (doUpdate)
                        {
                            // update - Not working version 3.13 SQLite-Net UWP
                            int sucess = inDB.Update(tableObject);

                            ////Update - To bypass update bug
                            //string upQuery = GetUpdateQueryFromClass(tableObject, inDB);
                            //SQLiteCommand command = inDB.CreateCommand(upQuery);
                            //command.ExecuteNonQuery();

                        }
                        else
                        {
                            int success = inDB.Insert(tableObject);
                        }
                    });
                }
                catch (SQLite.SQLiteException ex)
                {
                    Console.WriteLine(ex.ToString());
                }


                inDB.Close();
            }
        }

        /// <summary>
        /// Will save with an update or an insert operation a given ojbect inside the fieldwork database.
        /// 
        /// </summary>
        /// <param name="tableObject"></param>
        public void BatchSaveSQLTables(List<object> tableObject)
        {
            // Create a new connection
            using (DbConnection)
            {

                DbConnection.RunInTransaction(() =>
                {
                    foreach (object t in tableObject)
                    {
                        int sucess = DbConnection.Insert(t);
                    }
                });

                DbConnection.Close();
            }
        }

        /// <summary>
        /// Will execute a query on the database, usually for updates
        /// </summary>
        /// <param name="updateQuery"></param>
        public void SaveFromQuery(string updateQuery)
        {
            // Create a new connection
            using (var db = DbConnection)
            {

                db.RunInTransaction(() =>
                {
                    if (updateQuery != string.Empty)
                    {
                        // update - Not working version 3.13 SQLite-Net UWP
                        //int sucess = db.Update(tableObject);

                        //Update - To bypass update bug
                        SQLiteCommand command = db.CreateCommand(updateQuery);
                        command.ExecuteNonQuery();

                    }

                });

                db.Close();
            }
        }

        /// <summary>
        /// Will return a list of object (rows) from a given table name.
        /// In addition a query can be passed to filter results
        /// </summary>
        /// <param name="tableName">The table type object deriving from a class.</param>
        /// <param name="query">A query to filter table name, Can handle string.empty and null</param>
        /// <returns>A list of object that will act as rows.</returns>
        public List<object> ReadTable(Type tableType, string query)
        {
            return ReadTableFromDBConnection(tableType, query, DbConnection);

        }

        /// <summary>
        /// Will return a list of object (rows) from a given table name.
        /// In addition a query can be passed to filter results.
        /// Can be used with any database, not the default working one.
        /// </summary>
        /// <param name="tableName">The table type object deriving from a class.</param>
        /// <param name="query">A query to filter table name, Can handle string.empty and null</param>
        /// <returns>A list of object that will act as rows.</returns>
        public List<object> ReadTableFromDBConnection(Type tableType, string query, SQLiteConnection inConnection)
        {
            List<object> tableRows = new List<object>();

            using (var dbCon = inConnection)
            {

                //Get the proper table object to read from it
                TableMapping tableMap = dbCon.GetMapping(tableType);

                //Check for table existance
                try
                {

                    //Get table info
                    if (query == string.Empty || query == null)
                    {
                        tableRows = dbCon.Query(tableMap, "Select * from " + tableMap.TableName); //Added this because I'm not sure how the method will handle empty or null values in the query
                    }
                    else
                    {
                        tableRows = dbCon.Query(tableMap, query);
                    }
                }
                catch (Exception)
                {

                }

                dbCon.Close();

            }

            return tableRows;

        }

        /// <summary>
        /// Will return a list of object (rows) from a given table name.
        /// In addition a query can be passed to filter results.
        /// Can be used with any database, not the default working one.
        /// </summary>
        /// <param name="tableName">The table type object deriving from a class.</param>
        /// <param name="query">A query to filter table name, Can handle string.empty and null</param>
        /// <returns>A list of object that will act as rows.</returns>
        public List<object> ReadTableFromDBConnectionWithoutClosingConnection(Type tableType, string query, SQLiteConnection inConnection)
        {
            List<object> tableRows = new List<object>();



            //Get the proper table object to read from it
            TableMapping tableMap = inConnection.GetMapping(tableType);

            //Check for table existance
            try
            {

                //Get table info
                if (query == string.Empty || query == null)
                {
                    tableRows = inConnection.Query(tableMap, "Select * from " + tableMap.TableName); //Added this because I'm not sure how the method will handle empty or null values in the query
                }
                else
                {
                    tableRows = inConnection.Query(tableMap, query);
                }
            }
            catch (Exception)
            {

            }


            return tableRows;

        }

        /// <summary>
        /// Will delete any record from given parameters.
        /// TODO: The field name entry could be replace with the prime key if a TableMapping object is created. I think it returns the prime key field name. - Gab
        /// </summary>
        /// <param name="tableName">The table name to delete the record from</param>
        /// <param name="tableFieldName">The table field name to select the record with</param>
        /// <param name="recordIDToDelete">The table field value to delete.</param>
        public void DeleteRecord(string tableName, string tableFieldName, string recordIDToDelete)
        {

            using (SQLiteConnection dbConnect = DbConnection)
            {
                SQLiteCommand delCommand = dbConnect.CreateCommand("PRAGMA foreign_keys=ON");
                delCommand.ExecuteNonQuery();
                delCommand.CommandText = "DELETE FROM " + tableName + " WHERE " + tableFieldName + " = '" + recordIDToDelete + "'";
                delCommand.ExecuteNonQuery();

                dbConnect.Close();
            }
        }

        /// <summary>
        /// Will take an input database and will upgrade output database vocab tables (dictionaries) with latest coming from an input version
        /// </summary>
        public void DoSwapVocab(string vocabFromDBPath, SQLiteConnection vocabToDBConnection, bool closeConnection = true)
        {

            //Build delete vocab table query
            string deleteQuery = "DELETE FROM " + DatabaseLiterals.TableDictionaryManager + ";";
            string deleteQuery2 = "DELETE FROM " + DatabaseLiterals.TableDictionary + ";";


            //Build attach db query
            string attachQuery = "ATTACH '" + vocabFromDBPath + "' AS db2;";

            //Build insert queries
            string insertQuery = "INSERT INTO " + DatabaseLiterals.TableDictionaryManager + " SELECT * FROM db2." + DatabaseLiterals.TableDictionaryManager + ";";
            string insertQuery2 = "INSERT INTO " + DatabaseLiterals.TableDictionary + " SELECT * FROM db2." + DatabaseLiterals.TableDictionary + ";";

            //Build detach query
            string detachQuery = "DETACH DATABASE db2;";

            //Build vacuum query
            string vacuumQuery = "VACUUM";

            //Build final query
            string finalDeleteQuery = deleteQuery + deleteQuery2 + attachQuery + insertQuery + insertQuery2 + detachQuery + vacuumQuery;
            List<string> queryList = new List<string>() { deleteQuery, deleteQuery2, insertQuery, insertQuery2, detachQuery, vacuumQuery };

            vocabToDBConnection.Execute(attachQuery);

            if (closeConnection)
            {
                //Update working database
                using (var db = vocabToDBConnection)
                {

                    foreach (string q in queryList)
                    {
                        db.Execute(q);
                    }
                    db.Commit();
                    db.Close();
                }
            }

        }

        /// <summary>
        /// Will take an input database and will upgrade output database vocab tables (dictionaries) with latest coming from an input version
        /// </summary>
        public async void GetLatestVocab(string vocabFromDBPath, SQLiteConnection vocabToDBConnection, double dbVersion, bool closeConnection = true)
        {
            //Will hold all queries needed to be committed
            List<string> queryList = new List<string>() {};
            List<Exception> exceptionList = new List<Exception>();

            //Build attach db query
            string attachDBName = "db2";
            string attachQuery = "ATTACH '" + vocabFromDBPath + "' AS " + attachDBName + ";";
            queryList.Add(attachQuery);

            ////Set off foreign keys
            //string shutDownForeignConstraints = "PRAGMA foreign_keys = off;";
            //queryList.Add(shutDownForeignConstraints);

            ///Wipe new database of everything else but latest version of vocab
            //delete from M_DICTIONARY where M_DICTIONARY.VERSION != 1.5;
            //delete from M_DICTIONARY_MANAGER where M_DICTIONARY_MANAGER.VERSION != 1.5;
            string deleteQuery = "DELETE FROM " + DatabaseLiterals.TableDictionaryManager +
                " WHERE " + TableDictionaryManager + "." + FieldDictionaryManagerVersion + " is null or " + DatabaseLiterals.TableDictionaryManager + "." + DatabaseLiterals.FieldDictionaryManagerVersion + " < " + dbVersion.ToString() + ";";
            string deleteQuery2 = "DELETE FROM " + DatabaseLiterals.TableDictionary +
                " WHERE " + TableDictionary + "." + FieldDictionaryVersion + " is null or " + DatabaseLiterals.TableDictionary + "." + DatabaseLiterals.FieldDictionaryVersion + " < " + dbVersion.ToString() + ";";

            if (dbVersion <= 1.44)
            {
                //Version fields within dictionary didn't exist prior to version 1.5
                deleteQuery = "DELETE FROM " + DatabaseLiterals.TableDictionaryManager + ";";
                deleteQuery2 = "DELETE FROM " + DatabaseLiterals.TableDictionary + ";";
            }

            queryList.Add(deleteQuery2);
            queryList.Add(deleteQuery);

            //Build insert queries
            #region M_DICTIONARY

            Vocabularies modelVocab = new Vocabularies();
            List<string> vocabFieldList = modelVocab.getFieldList[DBVersion];
            string vocab_querySelect = string.Empty;

            foreach (string vocabFields in vocabFieldList)
            {
                //Get all fields except alias

                if (vocabFields != vocabFieldList.First())
                {
                    if (vocabFields == DatabaseLiterals.FieldDictionaryVersion && dbVersion >= 1.5)
                    {

                        vocab_querySelect = vocab_querySelect +
                            ", iif(NOT EXISTS (SELECT sql from " + attachDBName + ".sqlite_master where sql LIKE '%" + DatabaseLiterals.TableDictionary + "%" + DatabaseLiterals.FieldDictionaryVersion +
                            "%'),v." + DatabaseLiterals.FieldDictionaryVersion + ",NULL) as " + DatabaseLiterals.FieldDictionaryVersion;
                    }
                    else if (vocabFields == DatabaseLiterals.FieldDictionaryVersion && dbVersion == 1.44)
                    {
                        vocab_querySelect = vocab_querySelect +
                            ", NULL as " + DatabaseLiterals.FieldDictionaryVersion;
                    }
                    else if (vocabFields == DatabaseLiterals.FieldDictionaryVersion && dbVersion < 1.5)
                    {
                        //Do nothing, field didn't exist
                    }
                    else 
                    {
                        vocab_querySelect = vocab_querySelect + ", v." + vocabFields + " as " + vocabFields;
                    }

                }
                else
                {
                    vocab_querySelect = " v." + vocabFields + " as " + vocabFields;
                }

            }
            vocab_querySelect = vocab_querySelect.Replace(", ,", "");

            string insertQuery_vocab= "INSERT INTO " + DatabaseLiterals.TableDictionary + " SELECT " + vocab_querySelect;
            insertQuery_vocab = insertQuery_vocab + " FROM " + attachDBName + "." + DatabaseLiterals.TableDictionary + " as v";
            if (dbVersion >= 1.5)
            {
                ////Remove possible collision between new and old dictionaries
                //string deleteQuery_vocab_collision = "DELETE FROM " + DatabaseLiterals.TableDictionary + " WHERE " + DatabaseLiterals.FieldDictionaryTermID + " IN (SELECT vd." +
                //    DatabaseLiterals.FieldDictionaryTermID + " FROM " + attachDBName + "." + DatabaseLiterals.TableDictionary + " as vd WHERE vd." + DatabaseLiterals.FieldDictionaryVersion +
                //    " = " + DBVersion.ToString() + ");";

                //queryList.Add(deleteQuery_vocab_collision);

                insertQuery_vocab = insertQuery_vocab + " WHERE (v." + FieldDictionaryVersion + " is null or v." + FieldDictionaryVersion + " < " + DBVersion.ToString() +  ") AND (v." +
                    FieldDictionaryTermID + " NOT IN (SELECT v2." + FieldDictionaryTermID + " FROM " + TableDictionary + " as v2)); ";
            } 
                
            queryList.Add(insertQuery_vocab);

            #endregion

            #region M_DICTIONARY_MANAGER

            VocabularyManager modelVocabManager = new VocabularyManager();
            List<string> vocabMFieldList = modelVocabManager.getFieldList[DBVersion];
            string vocabm_querySelect = string.Empty;

            foreach (string vocabMFields in vocabMFieldList)
            {
                //Get all fields except alias

                if (vocabMFields != vocabMFieldList.First())
                {
                    if (vocabMFields == DatabaseLiterals.FieldDictionaryManagerVersion && dbVersion > 1.5)
                    {

                        vocabm_querySelect = vocabm_querySelect +
                            ", CASE WHEN EXISTS (SELECT sql from " + attachDBName + ".sqlite_master where sql LIKE '%" + DatabaseLiterals.TableDictionaryManager + "%" + DatabaseLiterals.FieldDictionaryManagerVersion +
                            "%') THEN (vm." + DatabaseLiterals.FieldDictionaryManagerVersion + ") ELSE NULL END as " + DatabaseLiterals.FieldDictionaryManagerVersion;
                    }
                    else if (vocabMFields == DatabaseLiterals.FieldDictionaryManagerVersion && dbVersion == 1.5 || vocabMFields == DatabaseLiterals.FieldDictionaryVersion && dbVersion == 1.44) 
                    {
                        vocabm_querySelect = vocabm_querySelect +
                            ", NULL as " + DatabaseLiterals.FieldDictionaryManagerVersion;
                    }
                    else if (vocabMFields == DatabaseLiterals.FieldDictionaryManagerVersion && dbVersion < 1.5)
                    {
                        //Do nothing, field didn't exist
                    }
                    else
                    {
                        vocabm_querySelect = vocabm_querySelect + ", vm." + vocabMFields + " as " + vocabMFields;
                    }

                }
                else
                {
                    vocabm_querySelect = " vm." + vocabMFields + " as " + vocabMFields;
                }

            }
            vocabm_querySelect = vocabm_querySelect.Replace(", ,", "");

            string insertQuery_vocabM = "INSERT INTO " + DatabaseLiterals.TableDictionaryManager + " SELECT " + vocabm_querySelect;
            insertQuery_vocabM = insertQuery_vocabM + " FROM " + attachDBName + "." + DatabaseLiterals.TableDictionaryManager + " as vm";
            if (dbVersion >= 1.5)
            {
                insertQuery_vocabM = insertQuery_vocabM + " WHERE vm." + FieldDictionaryManagerVersion + " is null or vm." + FieldDictionaryManagerVersion + " < " + DBVersion.ToString() + ";";
            }
            
            queryList.Add(insertQuery_vocabM);

            #endregion

            //Build detach query
            string detachQuery = "DETACH DATABASE " + attachDBName + ";";
            queryList.Add(detachQuery);

            //Build vacuum query
            string vacuumQuery = "VACUUM";
            queryList.Add(vacuumQuery);

            //Commit queries
            if (closeConnection)
            {
                //Update working database
                using (var db = vocabToDBConnection)
                {

                    foreach (string q in queryList)
                    {
                        try
                        {
                            db.Execute(q);
                        }
                        catch (Exception e)
                        {
                            exceptionList.Add(e);
                        }
                        
                    }
                    db.Commit();
                    db.Close();
                }
            }
            else 
            {
                foreach (string q in queryList)
                {
                    try
                    {
                        vocabToDBConnection.Execute(q);
                    }
                    catch (Exception e)
                    {
                        exceptionList.Add(e);
                    }
                    
                }
            }

            if (exceptionList.Count > 0)
            {
                string wholeStack = string.Empty;

                foreach (Exception es in exceptionList)
                {

                    wholeStack = wholeStack + "; " + es.Message + "; " + es.StackTrace;

                }

                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    ContentDialog deleteBookDialog = new ContentDialog()
                    {
                        Title = "DB Error",
                        Content = wholeStack,
                        PrimaryButtonText = "Bugger"
                    };
                    deleteBookDialog.Style = (Style)Application.Current.Resources["DeleteDialog"];
                    await Services.ContentDialogMaker.CreateContentDialogAsync(deleteBookDialog, false);

                }).AsTask();


            }


        }


        /// <summary>
        /// Will take an input database path and will upgrade it to current version
        /// </summary>
        public async Task DoUpgradeSchema(string inDBPath, SQLiteConnection outToDBConnection, double inDBVersion, bool closeConnection = true)
        {
            //Variables
            string attachDBName = "dbUpgrade";
            double newVersionNumber = DatabaseLiterals.DBVersion;
            List<Exception> exceptionList = new List<Exception>();

            //Untouched tables to upgrade
            List<string> upgradeUntouchedTables = new List<string>() { DatabaseLiterals.TableLocation, DatabaseLiterals.TableMetadata, 
                DatabaseLiterals.TableEarthMat, DatabaseLiterals.TableSample, DatabaseLiterals.TableStation,
                DatabaseLiterals.TableDocument, DatabaseLiterals.TableStructure, DatabaseLiterals.TableFossil,
                DatabaseLiterals.TableMineral, DatabaseLiterals.TableMineralAlteration , DatabaseLiterals.TablePFlow,  
                DatabaseLiterals.TableTraverseLine, DatabaseLiterals.TableTraversePoint , DatabaseLiterals.TableFieldCamp};


            //List of queries to send as a batch
            List<string> queryList = new List<string>();

            //Build attach db query
            string attachQuery = "ATTACH '" + inDBPath + "' AS " + attachDBName + "; ";

            //Shut down foreign keys constraints, else some loading might throws errors
            string shutDownForeignConstraints = "PRAGMA foreign_keys = off";

            //Get special queries
            //NOTE: tables field inserts must be in same order and same number as db table
            if (inDBVersion <= 1.42)
            {
                queryList.Add(GetUpgradeQueryVersion1_42(attachDBName));
                upgradeUntouchedTables.Remove(Dictionaries.DatabaseLiterals.TableEarthMat);

                newVersionNumber = DatabaseLiterals.DBVersion143;
            }
            if (inDBVersion == 1.43)
            {
                queryList.AddRange(GetUpgradeQueryVersion1_44(attachDBName));
                upgradeUntouchedTables.Remove(Dictionaries.DatabaseLiterals.TableLocation);
                upgradeUntouchedTables.Remove(Dictionaries.DatabaseLiterals.TableMetadata);

                newVersionNumber = DatabaseLiterals.DBVersion144;
            }
            if (inDBVersion < 1.5 && inDBVersion >= 1.44)
            { 
                queryList.AddRange(GetUpgradeQueryVersion1_5(attachDBName));
                upgradeUntouchedTables.Remove(Dictionaries.DatabaseLiterals.TableLocation);
                upgradeUntouchedTables.Remove(Dictionaries.DatabaseLiterals.TableSample);
                upgradeUntouchedTables.Remove(Dictionaries.DatabaseLiterals.TableStation);
                upgradeUntouchedTables.Remove(Dictionaries.DatabaseLiterals.TableStructure);
                upgradeUntouchedTables.Remove(Dictionaries.DatabaseLiterals.TableEarthMat);
                upgradeUntouchedTables.Remove(Dictionaries.DatabaseLiterals.TableMetadata);
                upgradeUntouchedTables.Remove(Dictionaries.DatabaseLiterals.TableDocument);

                newVersionNumber = DatabaseLiterals.DBVersion150;
            }

            if (inDBVersion == 1.5)
            {
                queryList.AddRange(GetUpgradeQueryVersion1_6(attachDBName));
                upgradeUntouchedTables.Remove(Dictionaries.DatabaseLiterals.TableStation);
                upgradeUntouchedTables.Remove(Dictionaries.DatabaseLiterals.TableEarthMat);
                upgradeUntouchedTables.Remove(Dictionaries.DatabaseLiterals.TableMineral);
                upgradeUntouchedTables.Remove(Dictionaries.DatabaseLiterals.TableMineralAlteration);
                upgradeUntouchedTables.Remove(Dictionaries.DatabaseLiterals.TableLocation);

                newVersionNumber = DatabaseLiterals.DBVersion160;
            }

            //Insert remaining tables
            foreach (string t in upgradeUntouchedTables)
            {
                //Build insert queries
                string insertQuery = "INSERT INTO " + t + " SELECT * FROM dbUpgrade." + t + ";";
                queryList.Add(insertQuery);
            }

            //Coin upgraded db version in metadata
            string coinNewVersion = "UPDATE " + DatabaseLiterals.TableMetadata + " SET " + DatabaseLiterals.FieldUserInfoVersionSchema + " = " + newVersionNumber.ToString() + ";";
            queryList.Add(coinNewVersion);

            //Build detach query
            string detachQuery = "DETACH DATABASE " + attachDBName + "; ";

            //Build vacuum query
            string vacuumQuery = "VACUUM;";

            using (SQLiteConnection dbConnect = DbConnection)
            {

                queryList.Add(vacuumQuery);

                //Attach
                try
                {
                    outToDBConnection.Execute(attachQuery);
                }
                catch (Exception e)
                {
                    exceptionList.Add(e);

                }


                //Shut down foreign keys constraints
                try
                {
                    outToDBConnection.Execute(shutDownForeignConstraints);
                }
                catch (Exception e)
                {
                    exceptionList.Add(e);

                }
                
            }

            //Update working database
            
            using (var db = outToDBConnection)
            {

                queryList.Add(detachQuery);

                foreach (string q in queryList)
                {
                    try
                    {
                        db.Execute(q);
                    }
                    catch (Exception e)
                    {
                        exceptionList.Add(e);

                    }

                }

                db.Commit();
                db.Close();

            }

            if (exceptionList.Count > 0)
            {
                string wholeStackUpgrade = string.Empty;

                foreach (Exception es in exceptionList)
                {
                    wholeStackUpgrade = wholeStackUpgrade + "; " + es.Message + "; " + es.StackTrace;
                }

                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    ContentDialog deleteBookDialog = new ContentDialog()
                    {
                        Title = "DB Error",
                        Content = wholeStackUpgrade,
                        PrimaryButtonText = "Bugger"
                    };
                    deleteBookDialog.Style = (Style)Application.Current.Resources["DeleteDialog"];
                    await Services.ContentDialogMaker.CreateContentDialogAsync(deleteBookDialog, true);

                }).AsTask();

            }

        }

        /// <summary>
        /// Will take select fieldbook db path and will perform some checks to validate if 
        /// database can be upgraded or not
        /// </summary>
        public bool CanUpgrade()
        {
            //Variables
            bool canUpgrade = false;

            //Check #1 Needs more then one location for it to be upgradable
            FieldLocation fieldLocationQuery = new FieldLocation();
            int locationCount = GetTableCount(fieldLocationQuery.GetType());

            //Check #2 DB version must be older then current
            double d_mVersions = GetDBVersion();

            if (locationCount > 0 && d_mVersions != DatabaseLiterals.DBVersion && d_mVersions != 0.0 && d_mVersions < DatabaseLiterals.DBVersion)
            {
                canUpgrade = true;
            }

            return canUpgrade;
        }

        /// <summary>
        /// Will get a read from F_METADATA.VERSIONSCHEMA and will return value in double
        /// </summary>
        /// <returns></returns>
        public double GetDBVersion()
        {
            //Build query to get version
            string dbSchemaVersionQuery = "SELECT fm." + DatabaseLiterals.FieldUserInfoVersionSchema + " from " + DatabaseLiterals.TableMetadata + " fm";

            //Parse result
            try
            {
                Metadata metadataQueryResult = new Metadata();
                List<object> mVersions = ReadTable(metadataQueryResult.GetType(), dbSchemaVersionQuery);
                metadataQueryResult = mVersions[0] as Metadata;
                double d_mVersions = DBVersion142;
                if (metadataQueryResult.VersionSchema != null)
                {
                    Double.TryParse(metadataQueryResult.VersionSchema.ToString(), out d_mVersions);
                }

                return d_mVersions;


            }
            catch (Exception e)
            {
                throw;
            }

        }

        #endregion

        #region GET METHODS (usually needs a connection object)
        /// <summary>
        /// Will return a table mapping object create from a type object that represent the table to map.
        /// </summary>
        /// <param name="tableName">The table type object from boxing a class into a type</param>
        /// <param name="dbConnect">An existing database connection</param>
        /// <returns>Will return an empty list if table name wasn't found in database.</returns>
        private static TableMapping GetATableObject(Type tableType, SQLiteConnection dbConnect)
        {
            //Will return a TableMapping object created from the given type. Type, deriving from the model class, should be true, else 
            //things might fail.
            return dbConnect.GetMapping(tableType);
        }

        /// <summary>
        /// Will return the field information from a given table name.
        /// Could be to instead of Models Classes to make the code free of hardcoded database schema.
        /// </summary>
        /// <param name="tableName">The string literals of the table name</param>
        /// <param name="dbConnect">The database connection</param>
        /// <returns></returns>
        private static List<SQLiteConnection.ColumnInfo> GetTableColumInfo(string tableName, SQLiteConnection dbConnect)
        {
            List<SQLiteConnection.ColumnInfo> tableNameColumnInfo = new List<SQLiteConnection.ColumnInfo>();

            tableNameColumnInfo = dbConnect.GetTableInfo(tableName);

            return tableNameColumnInfo;
        }

        private static string GetUpdateQueryFromClass(object inClassObject, SQLiteConnection dbConnect)
        {
            //Variables
            string updateQuery = @"UPDATE "; //Init

            //Get a table mapping object
            TableMapping inMapping = GetATableObject(inClassObject.GetType(), dbConnect);

            //Add table name in query
            updateQuery = updateQuery + inMapping.TableName + " SET ";

            int iterator = 0;

            //Proper casting to class
            switch (inMapping.TableName)
            {


                #region Station Case
                case Dictionaries.DatabaseLiterals.TableStation:

                    Station inStation = inClassObject as Station;

                    //Iterate through fields and them to the query
                    foreach (TableMapping.Column col in inMapping.Columns)
                    {
                        Type colType = col.ColumnType;

                        var value = col.GetValue(inStation);

                        if (value != null)
                        {
                            if (colType == typeof(System.String))
                            {
                                value = '"' + value.ToString() + '"';
                            }

                            if (iterator > 0)
                            {
                                updateQuery = updateQuery + ", " + col.Name + " = " + value;
                            }
                            else
                            {
                                updateQuery = updateQuery + col.Name + " = " + value;
                            }
                            iterator++;
                        }

                    }


                    //Finish the query with the where clause
                    updateQuery = updateQuery + " WHERE " + inMapping.PK.Name + " = '" + inMapping.FindColumn(inMapping.PK.Name).GetValue(inStation) + "'";

                    break;
                #endregion
                #region Earthmat Case

                case Dictionaries.DatabaseLiterals.TableEarthMat:

                    EarthMaterial inEarth = inClassObject as EarthMaterial;

                    //Iterate through fields and them to the query
                    foreach (TableMapping.Column col in inMapping.Columns)
                    {
                        Type colType = col.ColumnType;

                        var value = col.GetValue(inEarth);

                        if (value != null)
                        {
                            if (colType == typeof(System.String))
                            {
                                value = '"' + value.ToString() + '"';
                            }

                            if (iterator > 0)
                            {
                                updateQuery = updateQuery + ", " + col.Name + " = " + value;
                            }
                            else
                            {
                                updateQuery = updateQuery + col.Name + " = " + value;
                            }
                            iterator++;
                        }

                    }


                    //Finish the query with the where clause
                    updateQuery = updateQuery + " WHERE " + inMapping.PK.Name + " = '" + inMapping.FindColumn(inMapping.PK.Name).GetValue(inEarth) + "'";

                    break;
                #endregion
                #region Sample Case
                case Dictionaries.DatabaseLiterals.TableSample:

                    Sample inSample = inClassObject as Sample;

                    //Iterate through fields and them to the query
                    foreach (TableMapping.Column col in inMapping.Columns)
                    {
                        Type colType = col.ColumnType;

                        var value = col.GetValue(inSample);

                        if (value != null)
                        {
                            if (colType == typeof(System.String))
                            {
                                value = '"' + value.ToString() + '"';
                            }

                            if (iterator > 0)
                            {
                                updateQuery = updateQuery + ", " + col.Name + " = " + value;
                            }
                            else
                            {
                                updateQuery = updateQuery + col.Name + " = " + value;
                            }
                            iterator++;
                        }

                    }


                    //Finish the query with the where clause
                    updateQuery = updateQuery + " WHERE " + inMapping.PK.Name + " = '" + inMapping.FindColumn(inMapping.PK.Name).GetValue(inSample) + "'";

                    break;
                #endregion
                #region Dictionary Case
                case Dictionaries.DatabaseLiterals.TableDictionary:

                    Vocabularies inVocab = inClassObject as Vocabularies;

                    //Iterate through fields and them to the query
                    foreach (TableMapping.Column col in inMapping.Columns)
                    {
                        Type colType = col.ColumnType;

                        var value = col.GetValue(inVocab);

                        if (value != null)
                        {
                            if (colType == typeof(System.String))
                            {
                                value = '"' + value.ToString() + '"';
                            }

                            if (iterator > 0)
                            {
                                updateQuery = updateQuery + ", " + col.Name + " = " + value;
                            }
                            else
                            {
                                updateQuery = updateQuery + col.Name + " = " + value;
                            }
                            iterator++;
                        }

                    }


                    //Finish the query with the where clause
                    updateQuery = updateQuery + " WHERE " + inMapping.PK.Name + " = '" + inMapping.FindColumn(inMapping.PK.Name).GetValue(inVocab) + "'";

                    break;
                #endregion
                #region Document Case
                case Dictionaries.DatabaseLiterals.TableDocument:

                    Document inDoc = inClassObject as Document;

                    //Iterate through fields and them to the query
                    foreach (TableMapping.Column col in inMapping.Columns)
                    {
                        Type colType = col.ColumnType;

                        var value = col.GetValue(inDoc);

                        if (value != null)
                        {
                            if (colType == typeof(System.String))
                            {
                                value = '"' + value.ToString() + '"';
                            }

                            if (iterator > 0)
                            {
                                updateQuery = updateQuery + ", " + col.Name + " = " + value;
                            }
                            else
                            {
                                updateQuery = updateQuery + col.Name + " = " + value;
                            }
                            iterator++;
                        }

                    }


                    //Finish the query with the where clause
                    updateQuery = updateQuery + " WHERE " + inMapping.PK.Name + " = '" + inMapping.FindColumn(inMapping.PK.Name).GetValue(inDoc) + "'";

                    break;
                #endregion
                #region Structure Case
                case Dictionaries.DatabaseLiterals.TableStructure:

                    Structure inStruct = inClassObject as Structure;

                    //Iterate through fields and them to the query
                    foreach (TableMapping.Column col in inMapping.Columns)
                    {
                        Type colType = col.ColumnType;

                        var value = col.GetValue(inStruct);

                        if (value != null)
                        {
                            if (colType == typeof(System.String))
                            {
                                value = '"' + value.ToString() + '"';
                            }

                            if (iterator > 0)
                            {
                                updateQuery = updateQuery + ", " + col.Name + " = " + value;
                            }
                            else
                            {
                                updateQuery = updateQuery + col.Name + " = " + value;
                            }
                            iterator++;
                        }

                    }


                    //Finish the query with the where clause
                    updateQuery = updateQuery + " WHERE " + inMapping.PK.Name + " = '" + inMapping.FindColumn(inMapping.PK.Name).GetValue(inStruct) + "'";

                    break;
                #endregion
                #region Paleoflow Case
                case Dictionaries.DatabaseLiterals.TablePFlow:

                    Paleoflow inPflow = inClassObject as Paleoflow;

                    //Iterate through fields and them to the query
                    foreach (TableMapping.Column col in inMapping.Columns)
                    {
                        Type colType = col.ColumnType;

                        var value = col.GetValue(inPflow);

                        if (value != null)
                        {
                            if (colType == typeof(System.String))
                            {
                                value = '"' + value.ToString() + '"';
                            }

                            if (iterator > 0)
                            {
                                updateQuery = updateQuery + ", " + col.Name + " = " + value;
                            }
                            else
                            {
                                updateQuery = updateQuery + col.Name + " = " + value;
                            }
                            iterator++;
                        }

                    }


                    //Finish the query with the where clause
                    updateQuery = updateQuery + " WHERE " + inMapping.PK.Name + " = '" + inMapping.FindColumn(inMapping.PK.Name).GetValue(inPflow) + "'";

                    break;
                #endregion
                #region Fossil Case
                case Dictionaries.DatabaseLiterals.TableFossil:

                    Fossil inFossil = inClassObject as Fossil;

                    //Iterate through fields and them to the query
                    foreach (TableMapping.Column col in inMapping.Columns)
                    {
                        Type colType = col.ColumnType;

                        var value = col.GetValue(inFossil);

                        if (value != null)
                        {
                            if (colType == typeof(System.String))
                            {
                                value = '"' + value.ToString() + '"';
                            }

                            if (iterator > 0)
                            {
                                updateQuery = updateQuery + ", " + col.Name + " = " + value;
                            }
                            else
                            {
                                updateQuery = updateQuery + col.Name + " = " + value;
                            }
                            iterator++;
                        }

                    }


                    //Finish the query with the where clause
                    updateQuery = updateQuery + " WHERE " + inMapping.PK.Name + " = '" + inMapping.FindColumn(inMapping.PK.Name).GetValue(inFossil) + "'";

                    break;
                #endregion
                #region Location Case
                case Dictionaries.DatabaseLiterals.TableLocation:

                    FieldLocation inLocation = inClassObject as FieldLocation;

                    //Iterate through fields and them to the query
                    foreach (TableMapping.Column col in inMapping.Columns)
                    {
                        Type colType = col.ColumnType;

                        var value = col.GetValue(inLocation);

                        if (value != null)
                        {
                            if (colType == typeof(System.String))
                            {
                                value = '"' + value.ToString() + '"';
                            }

                            if (iterator > 0)
                            {
                                updateQuery = updateQuery + ", " + col.Name + " = " + value;
                            }
                            else
                            {
                                updateQuery = updateQuery + col.Name + " = " + value;
                            }
                            iterator++;
                        }

                    }


                    //Finish the query with the where clause
                    updateQuery = updateQuery + " WHERE " + inMapping.PK.Name + " = '" + inMapping.FindColumn(inMapping.PK.Name).GetValue(inLocation) + "'";

                    break;
                #endregion
                #region Mineral Case
                case Dictionaries.DatabaseLiterals.TableMineral:

                    Mineral inMineral = inClassObject as Mineral;

                    //Iterate through fields and them to the query
                    foreach (TableMapping.Column col in inMapping.Columns)
                    {
                        Type colType = col.ColumnType;

                        var value = col.GetValue(inMineral);

                        if (value != null)
                        {
                            if (colType == typeof(System.String))
                            {
                                value = '"' + value.ToString() + '"';
                            }

                            if (iterator > 0)
                            {
                                updateQuery = updateQuery + ", " + col.Name + " = " + value;
                            }
                            else
                            {
                                updateQuery = updateQuery + col.Name + " = " + value;
                            }
                            iterator++;
                        }

                    }


                    //Finish the query with the where clause
                    updateQuery = updateQuery + " WHERE " + inMapping.PK.Name + " = '" + inMapping.FindColumn(inMapping.PK.Name).GetValue(inMineral) + "'";

                    break;
                #endregion
                #region Mineral Alteration Case
                case Dictionaries.DatabaseLiterals.TableMineralAlteration:

                    MineralAlteration inMineralAlt = inClassObject as MineralAlteration;

                    //Iterate through fields and them to the query
                    foreach (TableMapping.Column col in inMapping.Columns)
                    {
                        Type colType = col.ColumnType;

                        var value = col.GetValue(inMineralAlt);

                        if (value != null)
                        {
                            if (colType == typeof(System.String))
                            {
                                value = '"' + value.ToString() + '"';
                            }

                            if (iterator > 0)
                            {
                                updateQuery = updateQuery + ", " + col.Name + " = " + value;
                            }
                            else
                            {
                                updateQuery = updateQuery + col.Name + " = " + value;
                            }
                            iterator++;
                        }

                    }


                    //Finish the query with the where clause
                    updateQuery = updateQuery + " WHERE " + inMapping.PK.Name + " = '" + inMapping.FindColumn(inMapping.PK.Name).GetValue(inMineralAlt) + "'";

                    break;
                #endregion
                #region Environment
                case Dictionaries.DatabaseLiterals.TableEnvironment:

                    EnvironmentModel inEnv = inClassObject as EnvironmentModel;

                    //Iterate through fields and them to the query
                    foreach (TableMapping.Column col in inMapping.Columns)
                    {
                        Type colType = col.ColumnType;

                        var value = col.GetValue(inEnv);

                        if (value != null)
                        {
                            if (colType == typeof(System.String))
                            {
                                value = '"' + value.ToString() + '"';
                            }

                            if (iterator > 0)
                            {
                                updateQuery = updateQuery + ", " + col.Name + " = " + value;
                            }
                            else
                            {
                                updateQuery = updateQuery + col.Name + " = " + value;
                            }
                            iterator++;
                        }

                    }


                    //Finish the query with the where clause
                    updateQuery = updateQuery + " WHERE " + inMapping.PK.Name + " = '" + inMapping.FindColumn(inMapping.PK.Name).GetValue(inEnv) + "'";

                    break;
                #endregion
                #region Metadata Case
                case Dictionaries.DatabaseLiterals.TableMetadata:

                    Metadata inMetadata = inClassObject as Metadata;

                    //Iterate through fields and them to the query
                    foreach (TableMapping.Column col in inMapping.Columns)
                    {
                        Type colType = col.ColumnType;

                        var value = col.GetValue(inMetadata);

                        if (value != null)
                        {
                            if (colType == typeof(System.String))
                            {
                                value = '"' + value.ToString() + '"';
                            }

                            if (iterator > 0)
                            {
                                updateQuery = updateQuery + ", " + col.Name + " = " + value;
                            }
                            else
                            {
                                updateQuery = updateQuery + col.Name + " = " + value;
                            }
                            iterator++;
                        }

                    }


                    //Finish the query with the where clause
                    updateQuery = updateQuery + " WHERE " + inMapping.PK.Name + " = '" + inMapping.FindColumn(inMapping.PK.Name).GetValue(inMetadata) + "'";

                    break;
                #endregion
                default:
                    break;
            }

            return updateQuery;

        }

        /// <summary>
        /// Will return a list containing value to fill comboboxes related to the database model
        /// </summary>
        /// <param name="tableName">The table name to use with with the picklist</param>
        /// <param name="inCBox">The combobox object with a proper naming convention so field name is deduce from it</param>
        /// <returns>A list contain resulting voca class entries</returns>
        public IEnumerable<Vocabularies> GetPicklistValues(string tableName, string fieldName)
        {
            return GetPicklistValuesFromParent(tableName, fieldName, string.Empty, false);
        }

        /// <summary>
        /// Will return a list containing value to fill comboboxes related to the database model
        /// </summary>
        /// <param name="tableName">The table name to use with with the picklist</param>
        /// <param name="fieldName">The database table field to get vocabs from</param>
        /// <param name="allValues">If all values, even non visible vocabs are needed</param>
        /// <param name="extraFieldValue"> The parent field that will be used to filter vocabs</param>
        /// <returns>A list contain resulting voca class entries</returns>
        public IEnumerable<Vocabularies> GetPicklistValuesFromParent(string tableName, string fieldName, string extraFieldValue, bool allValues)
        {
            //Build Not applicable vocab in case nothing is returned.
            Vocabularies vocNA = new Vocabularies
            {
                Code = Dictionaries.DatabaseLiterals.picklistNACode,
                Description = Dictionaries.DatabaseLiterals.picklistNACode
            };
            IEnumerable<Vocabularies> vocabNA = new Vocabularies[] { vocNA };

            Vocabularies vocEmpty = new Vocabularies
            {
                Code = string.Empty,
                Description = string.Empty
            };
            IEnumerable<Vocabularies> vocabEmpty = new Vocabularies[] { vocEmpty };
            //Get the current project type
            string fieldworkType = string.Empty;
            if (localSetting.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoFWorkType) != null)
            {
                fieldworkType = localSetting.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoFWorkType).ToString();
            }

            //Build query
            string querySelect = "SELECT * FROM " + TableDictionary;
            string queryJoin = " JOIN " + TableDictionaryManager + " ON " + TableDictionary + "." + FieldDictionaryCodedTheme + " = " + TableDictionaryManager + "." + FieldDictionaryManagerCodedTheme;
            string queryWhere = " WHERE " + TableDictionaryManager + "." + FieldDictionaryManagerAssignTable + " = '" + tableName + "'";
            string queryAndField = " AND " + TableDictionaryManager + "." + FieldDictionaryManagerAssignField + " = '" + fieldName + "'";
            string queryAndVisible = " AND " + TableDictionary + "." + FieldDictionaryVisible + " = '" + boolYes + "'";
            string queryAndWorkType = string.Empty;
            string queryAndParent = string.Empty;
            string queryOrdering = " ORDER BY " + TableDictionary + "." + FieldDictionaryOrder + " ASC";

            if (fieldworkType != string.Empty)
            {
                queryAndWorkType = " AND (lower(" + TableDictionaryManager + "." + FieldDictionaryManagerSpecificTo + ") = '" + fieldworkType + "' OR lower(" + TableDictionaryManager + "." + FieldDictionaryManagerSpecificTo + ") = '')";
            }

            if (extraFieldValue != string.Empty && extraFieldValue != null && extraFieldValue != "")
            {
                queryAndParent = " AND " + TableDictionary + "." + FieldDictionaryRelatedTo + " = '" + extraFieldValue + "'";
            }

            string finalQuery = querySelect + queryJoin + queryWhere + queryAndField + queryAndWorkType + queryAndParent;
            if (!allValues)
            {
                finalQuery = finalQuery + queryAndVisible + queryOrdering;
            }
            else
            {
                finalQuery = finalQuery + queryOrdering;
            }



            //Get query result
            Vocabularies voc = new Vocabularies();
            List<object> vocRaw = ReadTable(voc.GetType(), finalQuery);
            IEnumerable<Vocabularies> vocTable = vocabEmpty;

            if (vocRaw.Count == 0)
            {
                vocTable = vocabNA;
            }
            else
            {
                IEnumerable<Vocabularies> vocTable2 = vocRaw.Cast<Vocabularies>();
                vocTable = vocTable.Concat(vocTable2);
            }

            return vocTable;
        }

        /// <summary>
        /// Will return the number of records of a table
        /// </summary>
        /// <param name="inTabelType"></param>
        /// <returns></returns>
        public int GetTableCount(Type inTableType)
        {
            //Variables
            int tableCount = 0;

            //Get query result
            using (SQLiteConnection dbConnect = DbConnection)
            {
                List<object> tableRows = ReadTable(inTableType, null);

                tableCount = tableRows.Count();

                dbConnect.Close();
            }

            return tableCount;

        }

        /// <summary>
        /// From a given table and field name, will retrieve associated vocabulary and
        /// output a list of combobox items. An output parameter is also available 
        /// for default value if one is stated in the database or if N.A. is the only available choice.
        /// This method is meant for generic list with no queries
        /// </summary>
        /// <param name="tableName">The table name associated with the wanted vocab.</param>
        /// <param name="fieldName">The field name associated with the wanted vocab.</param>
        /// <param name="defaultValue">The output default value if there is any</param>
        /// <returns></returns>
        public List<Themes.ComboBoxItem> GetComboboxListWithVocab(string tableName, string fieldName, out string defaultValue)
        {
            //Outputs
            List<Themes.ComboBoxItem> outputVocabs = new List<Themes.ComboBoxItem>();
            defaultValue = string.Empty;

            //Get vocab
            DataAccess picklistAccess = new DataAccess();
            IEnumerable<Vocabularies> vocs = picklistAccess.GetPicklistValues(tableName, fieldName);

            //Fill in cbox
            outputVocabs = GetComboboxListFromVocab(vocs, out defaultValue);

            return outputVocabs;
        }

        /// <summary>
        /// From a given list of vocabularies items (usually coming from a more define query), will
        /// output a list of combobox items. An output parameter is also available 
        /// for default value if one is stated in the database or if N.A. is the only available choice.
        /// This method is meant for generic list with no queries
        /// </summary>
        /// <param name="tableName">The table name associated with the wanted vocab.</param>
        /// <param name="fieldName">The field name associated with the wanted vocab.</param>
        /// <param name="defaultValue">The output default value if there is any</param>
        /// <returns></returns>
        public List<Themes.ComboBoxItem> GetComboboxListFromVocab(IEnumerable<Vocabularies> inVocab, out string defaultValue)
        {
            //Outputs
            List<Themes.ComboBoxItem> outputVocabs = new List<Themes.ComboBoxItem>();
            defaultValue = string.Empty;

            //Fill in cbox
            foreach (Vocabularies vocabs in inVocab)
            {
                Themes.ComboBoxItem newItem = new Themes.ComboBoxItem();
                if (vocabs.Code == null)
                {
                    newItem.itemValue = string.Empty;
                }
                else
                {
                    newItem.itemValue = vocabs.Code;
                }
                if (vocabs.Description == null)
                {
                    newItem.itemName = string.Empty;
                }
                else
                {
                    newItem.itemName = vocabs.Description;
                }

                outputVocabs.Add(newItem);

                //Select default if stated in database
                if (vocabs.DefaultValue != null && vocabs.DefaultValue == Dictionaries.DatabaseLiterals.boolYes)
                {
                    defaultValue = vocabs.Code;
                }

                ////Select default value of N.A. if nothing is found for this particular vocab.
                //if (vocabs.Code == Dictionaries.DatabaseLiterals.picklistNACode)
                //{
                //    defaultValue = vocabs.Code;
                //}

            }
            if (defaultValue == null)
            {
                defaultValue = string.Empty;
            }
            return outputVocabs;
        }

        /// <summary>
        /// Will output a queyr to update database to version 1.42
        /// </summary>
        /// <returns></returns>
        public string GetUpgradeQueryVersion1_42(string attachedDBName)
        {
            ///Schema v 1.42 -- New note field in earthmat, will make earthmat dialog crash 
            ///INSERT INTO F_EARTH_MATERIAL SELECT *, CASE WHEN EXISTS (SELECT sql from db2.sqlite_master where sql LIKE '%F_EARTH_MATERIAL%NOTES%') THEN ("") ELSE NULL END as NOTES FROM db2.F_EARTH_MATERIAL
            EarthMaterial modelEM = new EarthMaterial();
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

            EarthMaterial modelEarthmat = new EarthMaterial();
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
            EarthMaterial modelEarth = new EarthMaterial();
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
                " = replace(" + DatabaseLiterals.FieldEarthMatModTextStruc + ", ' | ', '')" +" WHERE " + DatabaseLiterals.FieldEarthMatModTextStruc + 
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
                    else
                    {
                        mineral2_querySelect = mineral2_querySelect + ", NULL as " + minFields2;
                    }

                }
                else
                {
                    //Get a proper new GUID
                    mineral2_querySelect = "'" + idCal.CalculateMineralID() + "'" + " as " + minFields2;
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
        /// Will return a related structure record as an object
        /// </summary>
        /// <param name="StrucId"></param>
        /// <returns></returns>
        public Structure GetRelatedStructure(string StrucId)
        {
            Structure relatedStructure = new Structure();
            using (var db = DbConnection)
            {
                relatedStructure = db.Find<Structure>(struc => struc.StructureID == StrucId);
                // The following works if the field being searched is the primary key.
                //relatedStructure = db.Find<Structure>(StrucId);
                db.Close();
            }
            return relatedStructure;
        }

        #endregion

    }
}
