using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GSCFieldApp.Models;
using static GSCFieldApp.Dictionaries.DatabaseLiterals;
using GSCFieldApp.Dictionaries;
using SQLite;
using System.Diagnostics;


// Based on code sample from: http://blogs.u2u.be/diederik/post/2015/09/08/Using-SQLite-on-the-Universal-Windows-Platform.aspx -Kaz
namespace GSCFieldApp.Services.DatabaseServices
{
    public class DataAccess
    {
        public static SQLiteConnection _dbConnection;
        public const string DatabaseFilename = DatabaseLiterals.DBName + DatabaseLiterals.DBTypeSqlite;

        /// <summary>
        /// Default database patch in the app directory.
        /// Will be saved as another name once the field book is properly filled and then created.
        /// </summary>
        public string DatabaseFilePath =>
            Path.Combine(FileSystem.Current.AppDataDirectory, DatabaseFilename);

        /// <summary>
        /// Property directly set within user preferences
        /// </summary>
        public string PreferedDatabasePath
        {
            get { return Preferences.Get(nameof(DatabaseFilePath), DatabaseFilePath); }
            set { Preferences.Set(nameof(DatabaseFilePath), value); }
        }

        public DataAccess()
        {

        }

        #region DB MANAGEMENT METHODS

        /// <summary>
        /// Get a sqlite connection object
        /// </summary>
        private SQLiteConnection DbConnection
        {
            get
            {
                if (PreferedDatabasePath != string.Empty)
                {
                    return new SQLiteConnection(PreferedDatabasePath);
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
            if (PreferedDatabasePath == string.Empty)
            {
                return false;
            }
            else
            {
                return File.Exists(PreferedDatabasePath);
            }
            
        }


        #endregion

        #region DATA MANAGEMENT METHODS (Create, Update, Read)

        /// <summary>
        /// Will write an embedded resource to a file with a binary writer. In case it exists, it will replace it.
        /// Will save the resource to the local folder.
        /// </summary>
    
        public async Task CreateDatabaseFromResource()
        {
            try
            {
                if (!DoesDatabaseExists())
                {
                    //This line works for an existing file
                    using var package = await FileSystem.OpenAppPackageFileAsync(@"GSCFieldwork.gpkg");
                    using var inputStream = new StreamReader(package);
                    var fileContent = inputStream.ReadToEnd();

                    using FileStream outputStream = System.IO.File.OpenWrite(PreferedDatabasePath);
                    using StreamWriter streamWriter = new StreamWriter(outputStream);

                    await streamWriter.WriteAsync(fileContent);
                }

            }
            catch (Exception e)
            {
                Debug.Write(e.Message);
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
                        db.Commit();
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
                catch (Exception e)
                {
                    Debug.Write(e.Message);
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
        public void DeleteRecord(string tableName, string tableFieldName, int recordIDToDelete)
        {

            using (SQLiteConnection dbConnect = DbConnection)
            {
                SQLiteCommand delCommand = dbConnect.CreateCommand("PRAGMA foreign_keys=ON");
                delCommand.ExecuteNonQuery();
                delCommand.CommandText = "DELETE FROM " + tableName + " WHERE " + tableFieldName + " = " + recordIDToDelete + ";";
    
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
            List<string> queryList = new List<string>() { };
            List<Exception> exceptionList = new List<Exception>();

            //Build attach db query
            string attachDBName = "db2";
            string attachQuery = "ATTACH '" + vocabFromDBPath + "' AS " + attachDBName + ";";
            queryList.Add(attachQuery);

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
                    if (vocabFields == FieldGenericRowID && dbVersion == DBVersion160)
                    {
                        vocab_querySelect = " NULL as " + vocabFields;
                    }
                    else if (vocabFields == FieldGenericRowID && dbVersion < DBVersion160)
                    {
                        //Do nothing, skip that one, it was added in version 1.7
                    }
                    else
                    {
                        vocab_querySelect = " v." + vocabFields + " as " + vocabFields;
                    }
                    
                }

            }
            vocab_querySelect = vocab_querySelect.Replace(", ,", "");

            string insertQuery_vocab = "INSERT INTO " + DatabaseLiterals.TableDictionary + " SELECT " + vocab_querySelect;
            insertQuery_vocab = insertQuery_vocab.Replace("SELECT ,", "SELECT ");
            insertQuery_vocab = insertQuery_vocab + " FROM " + attachDBName + "." + DatabaseLiterals.TableDictionary + " as v";

            string defaultCreatorsEditors = "'Bedrock Committee', 'GSC Field App', 'Gabriel Huot-Vézina', 'Microsoft default', 'GanFeld', 'Ganfeld', " +
                "'Janet Campbell', 'Jessey Rice', 'New term', 'Jessey Rice/Janet Campbell', 'Microsoft', " + 
                "'Pierre Brouillette', 'Surficial Committee', 'Surficial Commitee'";
            
            insertQuery_vocab = insertQuery_vocab + " WHERE (v." + FieldDictionaryCreator + " not in (" + defaultCreatorsEditors + 
                ") or v." + FieldDictionaryEditor + " not in (" + defaultCreatorsEditors + ") AND (v." +
                FieldDictionaryTermID + " NOT IN (SELECT v2." + FieldDictionaryTermID + " FROM " + TableDictionary + " as v2))); ";
            queryList.Add(insertQuery_vocab);

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

                //await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                //{
                //    ContentDialog deleteBookDialog = new ContentDialog()
                //    {
                //        Title = "DB Error",
                //        Content = wholeStack,
                //        PrimaryButtonText = "Bugger"
                //    };
                //    deleteBookDialog.Style = (Style)Application.Current.Resources["DeleteDialog"];
                //    await Services.ContentDialogMaker.CreateContentDialogAsync(deleteBookDialog, false);

                //}).AsTask();


            }


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
                Debug.Write(e.Message);
                return 0.0;
            }

        }

        #endregion

    }
}
