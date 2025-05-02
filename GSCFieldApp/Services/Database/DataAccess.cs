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
using BruTile.Wmts.Generated;
using GSCFieldApp.Controls;
using NetTopologySuite.Index.HPRtree;
using Microsoft.Maui.Controls.PlatformConfiguration;
using System.Globalization;
using Mapsui.Utilities;

namespace GSCFieldApp.Services.DatabaseServices
{
    public class DataAccess
    {
        private static SQLiteAsyncConnection _dbConnection;

        //Localization
        public LocalizationResourceManager LocalizationResourceManager
            => LocalizationResourceManager.Instance; // Will be used for in code dynamic local strings

        //TODO: find why on android .gpkg isn't a valid file type even though the database is sqlite.

#if WINDOWS
        public const string DatabaseFilename = DatabaseLiterals.DBName + DatabaseLiterals.DBTypeSqlite;
#elif ANDROID
        public const string DatabaseFilename = DatabaseLiterals.DBName + DatabaseLiterals.DBTypeSqliteDeprecated;
#else
        public const string DatabaseFilename = DatabaseLiterals.DBName + DatabaseLiterals.DBTypeSqlite;
#endif
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
            //Init database
            if (DbConnection == null)
            {
                //Create the database connection object
                DbConnection = new SQLiteAsyncConnection(PreferedDatabasePath);
            }

        }

        #region DB MANAGEMENT METHODS

        /// <summary>
        /// Get a sqlite connection object
        /// </summary>
        public static SQLiteAsyncConnection DbConnection
        {
            get
            {
                return _dbConnection;
            }

            set
            {
                _dbConnection = value;
            }
        }

        /// <summary>
        /// Will return a async connection from a string path
        /// </summary>
        /// <param name="inPath"></param>
        /// <returns></returns>
        public SQLiteAsyncConnection GetConnectionFromPath(string inPath)
        {
            return new SQLiteAsyncConnection(inPath);
        }

        /// <summary>
        /// Will close the database connection
        /// </summary>
        /// <returns></returns>
        public async Task CloseConnectionAsync()
        {
            try
            {
                await DbConnection.CloseAsync();
            }
            catch (Exception closeError)
            {
                new ErrorToLogFile(closeError).WriteToFile();
            }
            
        }

        /// <summary>
        /// Will close the database connection
        /// </summary>
        /// <returns></returns>
        public async Task SetConnectionAsync()
        {
            try
            {
                DbConnection = new SQLiteAsyncConnection(PreferedDatabasePath);
            }
            catch (Exception connectError)
            {
                new ErrorToLogFile(connectError).WriteToFile();
            }
            
        }

        #endregion

        #region VOCAB & DATA MANAGEMENT METHODS (Create, Update, Read)

        /// <summary>
        /// Will write an embedded resource to a file with a binary writer. In case it exists, it will replace it.
        /// Will save the resource to the local folder.
        /// </summary>
        public async Task<bool> CreateDatabaseFromResource(string outputDatabasePath, string resourceName = "")
        {
            try
            {
                if (!File.Exists(outputDatabasePath))
                {
                    //Default
                    if (resourceName == string.Empty)
                    {
                        resourceName = @"GSCFieldwork.gpkg";
                    }

                    //Validate app resource name
                    bool resourceNameExist = await FileSystem.AppPackageFileExistsAsync(resourceName);
                    if (!resourceNameExist)
                    {
                        //Under android, the file type needs to be sqlite else it doesn't work
                        //This is a workaround to keep the real resource name but change the extension
                        //after the copying process is over
                        resourceName = resourceName.Replace(DatabaseLiterals.DBTypeSqliteDeprecated, DatabaseLiterals.DBTypeSqlite);

                        //Last attempt
                        resourceNameExist = await FileSystem.AppPackageFileExistsAsync(resourceName);

                        if (!resourceNameExist)
                        {
                            return false;
                        }

                    }

                    //Open stream with embeded resource
                    using Stream package = await FileSystem.OpenAppPackageFileAsync(resourceName);

                    //Open empty stream for output file
                    using FileStream outputStream = System.IO.File.OpenWrite(outputDatabasePath);

                    //Need a binary write for geopackage database, else file will be corrupt with 
                    //default stream writer/reader
                    byte[] buffer = new byte[16*1024];
                    using (BinaryWriter fileWriter = new BinaryWriter(outputStream))
                    {
                        using (BinaryReader fileReader = new BinaryReader(package))
                        {
                            //NOTE: On android length isn't a property so we need to count the bytes instead
                            //https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/storage/file-system-helpers?view=net-maui-7.0&tabs=android#platform-differences 

                            //Read package by block of 1024 bytes.
                            int readCount = 0;

                            while ((readCount = fileReader.BaseStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                fileWriter.Write(buffer, 0, readCount);
                            }

                        }
                    }

                    //Keep database path as default
                    PreferedDatabasePath = outputDatabasePath;
                }

                return true;
                
            }
            catch (Exception e)
            {

                new ErrorToLogFile(e).WriteToFile();
                await Shell.Current.DisplayAlert("Error", e.Message, "Ok");
                return false;
            }


        }

        /// <summary>
        /// Will delete an item object that is a model table
        /// </summary>
        /// <param name="item"></param>
        /// <param name="doUpdate"></param>
        /// <returns></returns>
        public async Task<int> DeleteItemAsync(object item)
        {
            if (item != null)
            {
                // Create a new connection
                try
                {

                    //For debug
                    DbConnection.Tracer = new Action<string>(q => Debug.WriteLine(q));
                    DbConnection.Trace = true;


                    int numberOfRows = await DbConnection.DeleteAsync(item);
                    return numberOfRows;



                }
                catch (SQLite.SQLiteException ex)
                {
                    new ErrorToLogFile(ex).WriteToFile();
                    return 0;
                }
            }
            else
            {
                return 0;
            }

        }

        /// <summary>
        /// Will save (insert or update) an item object that is a model table
        /// </summary>
        /// <param name="item"></param>
        /// <param name="doUpdate"></param>
        /// <returns></returns>
        public async Task<object> SaveItemAsync(object item, bool doUpdate)
        {

            // Create a new connection
            try
            {

                //For debug
                DbConnection.Tracer = new Action<string>(q => Debug.WriteLine(q));
                DbConnection.Trace = true;

                if (doUpdate)
                {
                    await DbConnection.UpdateAsync(item);
                    return item;

                }
                else
                {
                    await DbConnection.InsertAsync(item);
                    return item;
                }

            }
            catch (SQLite.SQLiteException ex)
            {
                new ErrorToLogFile(ex).WriteToFile();
                return item;
            }

        }

        /// <summary>
        /// Will return a list containing value to fill comboboxes related to the database model
        /// </summary>
        /// <param name="tableName">The table name to use with with the picklist</param>
        /// <param name="fieldName">The database table field to get vocabs from</param>
        /// <param name="allValues">If all values, even non visible vocabs are needed</param>
        /// <param name="extraFieldValue"> The parent field that will be used to filter vocabs</param>
        /// <param name="fieldwork">Field book theme (bedrock, surficial)</param>
        /// <returns>A list contain resulting voca class entries</returns>
        public async Task<List<Vocabularies>> GetPicklistValuesAsync(string tableName, string fieldName, string extraFieldValue, 
            bool allValues, string databasePath = "")
        {

            if (databasePath == string.Empty)
            {
                databasePath = DatabaseFilePath;
            }

            //Get the current project type
            string fieldworkType = ApplicationThemeBedrock; //Default

            if (Preferences.ContainsKey(nameof(FieldUserInfoFWorkType)))
            {
                //This should be set whenever user selects a different field book
                fieldworkType = Preferences.Get(nameof(FieldUserInfoFWorkType), fieldworkType);

                //Something could have happened and nothing was selected, enforce bedrock
                if (fieldworkType == string.Empty || fieldworkType.ToLower() == ApplicationThemeDrillHole)
                {
                    fieldworkType = ApplicationThemeBedrock;
                }
            }

            //Build query
            string querySelect = "SELECT md.* FROM " + TableDictionary + " as md";
            string queryJoin = " JOIN " + TableDictionaryManager + " as mdm ON md." + FieldDictionaryCodedTheme + " = mdm." + FieldDictionaryManagerCodedTheme;
            string queryWhere = " WHERE mdm." + FieldDictionaryManagerAssignTable + " = '" + tableName + "'";
            string queryAndField = " AND mdm." + FieldDictionaryManagerAssignField + " = '" + fieldName + "'";
            string queryAndVisible = " AND md." + FieldDictionaryVisible + " = '" + boolYes + "'";
            string queryAndWorkType = string.Empty;
            string queryAndParent = string.Empty;
            string queryOrdering = " ORDER BY md." + FieldDictionaryOrder + " ASC";

            if (fieldworkType != string.Empty)
            {
                queryAndWorkType = " AND (lower(mdm." + FieldDictionaryManagerSpecificTo + ") like '" + fieldworkType + 
                    "%' OR lower(mdm." + FieldDictionaryManagerSpecificTo + ") = '' OR lower(mdm." + FieldDictionaryManagerSpecificTo + ") is null)";
            }

            if (extraFieldValue != string.Empty && extraFieldValue != null && extraFieldValue != "")
            {
                queryAndParent = " AND md." + FieldDictionaryRelatedTo + " = '" + extraFieldValue + "'";
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

            //Get vocab records from generic database not prefered one
            SQLiteAsyncConnection vocabConnection = GetConnectionFromPath(databasePath);
            List<Vocabularies> vocabs = await vocabConnection.QueryAsync<Vocabularies>(finalQuery);

            //Add empty record for user to remove any selected values
            Vocabularies emptyNull = new Vocabularies();
            emptyNull.Code = null;
            emptyNull.Description = " ";

            vocabs.Insert(0, emptyNull);

            await vocabConnection.CloseAsync();

            return vocabs;
        }

        /// <summary>
        /// From a given table and field name, will retrieve associated vocabulary and
        /// output a list of combobox items. An output parameter is also available 
        /// for default value if one is stated in the database or if N.A. is the only available choice.
        /// This method is meant for generic list with no queries
        /// </summary>
        /// <param name="tableName">The table name associated with the wanted vocab.</param>
        /// <param name="fieldName">The field name associated with the wanted vocab.</param>
        /// <returns></returns>
        public async Task<ComboBox> GetComboboxListWithVocabAsync(string tableName, string fieldName, string extraFieldValue = "")
        {
            //Outputs
            ComboBox outputVocabs = new ComboBox();

            //Get vocab
            List<Vocabularies> vocs = await GetPicklistValuesAsync(tableName, fieldName, extraFieldValue, false);

            //Fill in cbox
            outputVocabs = GetComboboxListFromVocab(vocs);


            return outputVocabs;
        }

        /// <summary>
        /// From a given list of vocabularies items (usually coming from a more define query), will
        /// output a list of combobox items. Will also output as in the default value else -1 for no
        /// selection
        /// </summary>
        /// <param name="inVocab">List of vocabularies that needs to be converted to picker</param>
        /// <returns></returns>
        public ComboBox GetComboboxListFromVocab(IEnumerable<Vocabularies> inVocab, bool SymbolAsValue = false)
        {
            //Outputs
            List<ComboBoxItem> outputVocabsList = new List<ComboBoxItem>();
            int defaultValueIndex = -1;

            //Fill in cbox
            foreach (Vocabularies vocabs in inVocab)
            {
                ComboBoxItem newItem = new ComboBoxItem();

                //Manage nulls
                if (vocabs.Code == null)
                {
                    newItem.itemValue = string.Empty;
                }
                else
                {
                    newItem.itemValue = vocabs.Code;
                }

                //Manage symbols over code
                if (vocabs.Symbol != null && SymbolAsValue)
                {
                    newItem.itemValue = vocabs.Symbol;
                }

                //Manage description null
                if (vocabs.Description == null)
                {
                    newItem.itemName = string.Empty;
                }
                else
                {
                    newItem.itemName = vocabs.Description;
                }

                //Manage language description
                if (CultureInfo.CurrentCulture.ToString().ToLower().Contains("fr"))
                {

                    if (vocabs.DescriptionFR != null && vocabs.DescriptionFR != string.Empty)
                    {
                        newItem.itemName = vocabs.DescriptionFR;
                    }
                    else
                    {
                        newItem.itemName = vocabs.Description;
                    }

                }
                else
                {
                    newItem.itemName = vocabs.Description;
                }

                //Select default if stated in database
                if (vocabs.DefaultValue != null && vocabs.DefaultValue == Dictionaries.DatabaseLiterals.boolYes)
                {
                    defaultValueIndex = outputVocabsList.Count;
                }

                //Keep relatedTo Value for filtering
                if (vocabs.RelatedTo != null && vocabs.RelatedTo != string.Empty)
                {
                    newItem.itemParent = vocabs.RelatedTo;
                }

                outputVocabsList.Add(newItem);
            }

            //If at the end there is onlye one record, make it a default
            if (outputVocabsList.Count == 1)
            {
                defaultValueIndex = 0;
            }

            //Set
            ComboBox outputVocabs = new ComboBox();
            outputVocabs.cboxItems = outputVocabsList;
            outputVocabs.cboxDefaultItemIndex = defaultValueIndex; 

            return outputVocabs;
        }

        /// <summary>
        /// From a given list of vocabularies items (usually coming from a more define query), will
        /// output a list of combobox items. Will also output as in the default value else -1 for no
        /// selection
        /// </summary>
        /// <param name="inVocab">List of vocabularies that needs to be converted to picker</param>
        /// <returns></returns>
        public ComboBox GetComboboxListFromStrings(IEnumerable<string> inStrings)
        {
            //Outputs
            List<ComboBoxItem> outputStringList = new List<ComboBoxItem>();
            int defaultValueIndex = -1;

            //Fill in cbox
            foreach (string s in inStrings)
            {
                ComboBoxItem newItem = new ComboBoxItem();

                newItem.itemValue = s;
                newItem.itemName = s;

                outputStringList.Add(newItem);
            }

            //If at the end there is onlye one record, make it a default
            if (outputStringList.Count == 1)
            {
                defaultValueIndex = 0;
            }

            //Set
            ComboBox outputCombo = new ComboBox();
            outputCombo.cboxItems = outputStringList;
            outputCombo.cboxDefaultItemIndex = defaultValueIndex;

            return outputCombo;
        }

        /// <summary>
        /// Will delete any record from given parameters.
        /// TODO: The field name entry could be replace with the prime key if a TableMapping object is created. I think it returns the prime key field name. - Gab
        /// </summary>
        /// <param name="tableName">The table name to delete the record from</param>
        /// <param name="tableFieldName">The table field name to select the record with</param>
        /// <param name="recordIDToDelete">The table field value to delete.</param>
        public async Task<int> DeleteItemCascadeAsync(string tableName, string tableFieldName, int recordIDToDelete)
        {

            // Create a new connection
            try
            {

                //For debug
                DbConnection.Tracer = new Action<string>(q => Debug.WriteLine(q));
                DbConnection.Trace = true;


                await DbConnection.ExecuteAsync("PRAGMA foreign_keys=ON");
                int delRecords = await DbConnection.ExecuteAsync(string.Format("DELETE FROM {0} WHERE {1} = {2};", tableName, tableFieldName, recordIDToDelete));
                return delRecords;



            }
            catch (SQLite.SQLiteException ex)
            {
                new ErrorToLogFile(ex).WriteToFile();
                return 0;
            }


        }

        /// <summary>
        /// Will take an input database and will upgrade output database vocab tables (dictionaries) with latest coming from an input version
        /// </summary>
        public async Task<bool> DoSwapVocab(string vocabFromDBPath, string vocabToDBPath, bool closeConnection = true)
        {
            //Output
            bool completedWithoutErrors = false;
            List<Exception> exceptionList = new List<Exception>();

            if (vocabFromDBPath != vocabToDBPath)
            {
                //Build delete vocab table query
                string deleteQuery = "DELETE FROM " + TableDictionaryManager + ";";
                string deleteQuery2 = "DELETE FROM " + TableDictionary + ";";

                //Build attach db query
                string attachQuery = "ATTACH '" + vocabFromDBPath + "' AS db2;";

                //Build insert queries
                string insertQuery = "INSERT INTO " + TableDictionaryManager + " SELECT * FROM db2." + TableDictionaryManager + ";";
                string insertQuery2 = "INSERT INTO " + TableDictionary + " SELECT * FROM db2." + TableDictionary + ";";

                //Build final query
                List<string> queryList = new List<string>() { deleteQuery, deleteQuery2, insertQuery, insertQuery2 };

                SQLiteAsyncConnection vocabToDBConnection = new SQLiteAsyncConnection(vocabToDBPath);
                await vocabToDBConnection.ExecuteAsync(attachQuery);

                //Update working database
                await vocabToDBConnection.RunInTransactionAsync((SQLiteConnection connection) =>
                {
                    foreach (string q in queryList)
                    {
                        try
                        {
                            connection.Execute(q);
                        }
                        catch (Exception e)
                        {
                            exceptionList.Add(e);
                        }
                    }
                });

                await vocabToDBConnection.CloseAsync();

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
                    new ErrorToLogFile(wholeStack + "\n DB:" + vocabFromDBPath).WriteToFile();

                }
                else
                {
                    completedWithoutErrors = true;
                }
            }

            return completedWithoutErrors;

        }

        /// <summary>
        /// Will take an input database and will upgrade output database vocab tables (dictionaries) with latest coming from an input version
        /// </summary>
        public async Task<bool> PushLatestVocab(string vocabFromDBPath, SQLiteAsyncConnection vocabToDBConnection, double fromDBVersion, bool closeConnection = true)
        {
            //Output
            bool completedWithoutErrors = false;

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

                    if (vocabFields == DatabaseLiterals.FieldDictionaryVersion && fromDBVersion >= 1.5)
                    {
                        vocab_querySelect = vocab_querySelect +
                            ", iif(NOT EXISTS (SELECT sql from " + attachDBName + ".sqlite_master where sql LIKE '%" + DatabaseLiterals.TableDictionary + "%" + DatabaseLiterals.FieldDictionaryVersion +
                            "%'),v." + DatabaseLiterals.FieldDictionaryVersion + ",NULL) as " + DatabaseLiterals.FieldDictionaryVersion;
                    }
                    else if (vocabFields == DatabaseLiterals.FieldDictionaryVersion && fromDBVersion == 1.44)
                    {
                        vocab_querySelect = vocab_querySelect +
                            ", NULL as " + DatabaseLiterals.FieldDictionaryVersion;
                    }
                    else if (vocabFields == DatabaseLiterals.FieldDictionaryVersion && fromDBVersion < 1.5)
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
                    if (vocabFields == FieldGenericRowID && fromDBVersion >= DBVersion160)
                    {
                        vocab_querySelect = " NULL as " + vocabFields; //Don't insert the ids back
                    }
                    else if (vocabFields == FieldGenericRowID && fromDBVersion < DBVersion160)
                    {
                        //Do nothing, skip that one, it was added in version 1.7
                    }


                }

            }
            vocab_querySelect = vocab_querySelect.Replace(", ,", "");

            string insertQuery_vocab = "INSERT INTO " + DatabaseLiterals.TableDictionary + " SELECT " + vocab_querySelect;
            insertQuery_vocab = insertQuery_vocab.Replace("SELECT ,", "SELECT ");
            insertQuery_vocab = insertQuery_vocab + " FROM " + attachDBName + "." + DatabaseLiterals.TableDictionary + " as v";

            insertQuery_vocab = insertQuery_vocab + " WHERE v." + FieldDictionaryCreator + " not in (select distinct(md." +
                FieldDictionaryCreator + ") from " + TableDictionary + " as md)";
            insertQuery_vocab = insertQuery_vocab + " AND v." + FieldDictionaryTermID + " not in (select md." + FieldDictionaryTermID + " from " +
                TableDictionary + " as md);";
            queryList.Add(insertQuery_vocab);

            #endregion

            //Build detach query
            string detachQuery = "DETACH DATABASE " + attachDBName + ";";
            queryList.Add(detachQuery);

            //Build vacuum query
            string vacuumQuery = "VACUUM;";
            queryList.Add(vacuumQuery);

            //Update working database
            foreach (string q in queryList)
            {
                try
                {
                    await vocabToDBConnection.ExecuteAsync(q);
                }
                catch (Exception e)
                {
                    exceptionList.Add(e);
                }

            }

            //Close if needed
            if (closeConnection)
            {

                await vocabToDBConnection.CloseAsync();
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
                completedWithoutErrors = true;
            }

            return completedWithoutErrors;
        }

        /// <summary>
        /// Will take an input database and will update matchin records and insert missing ones
        /// </summary>
        public async Task<bool> DoMergeVocab(string vocabFromDBPath, string vocabToDBPath, bool closeConnection = true)
        {
            //Output
            bool completedWithoutErrors = false;
            List<Exception> exceptionList = new List<Exception>();

            //Build attach db query
            string attachQuery = "ATTACH '" + vocabFromDBPath + "' AS db2;";

            Vocabularies modelVocab = new Vocabularies();
            List<string> vocabFieldList = modelVocab.getFieldList[DBVersion];
            string vocab_querySelect = string.Empty;

            foreach (string vocabFields in vocabFieldList)
            {
                //Get all fields except alias
                if (vocabFields != vocabFieldList.First())
                {
                    vocab_querySelect = vocab_querySelect + ", " + vocabFields + " as " + vocabFields;
                }
                else
                {
                    if (vocabFields == FieldGenericRowID)
                    {
                        vocab_querySelect = " NULL as " + vocabFields; //Don't insert the ids back
                    }

                }

            }
            vocab_querySelect = vocab_querySelect.Replace(", ,", "");

            //Build insert queries
            //insert into M_DICTIONARY select * from db2.M_DICTIONARY where db2.M_DICTIONARY.TERMID not in (select TERMID from M_DICTIONARY)
            string insertQuery = "INSERT INTO " + TableDictionaryManager + 
                " SELECT * FROM db2." + TableDictionaryManager + " WHERE db2." + TableDictionaryManager + "." + FieldDictionaryManagerLinkID +
                " NOT IN (SELECT " + FieldDictionaryManagerLinkID + " FROM " + TableDictionaryManager + "); ";
            string insertQuery2 = "INSERT INTO " + TableDictionary +
                " SELECT " + vocab_querySelect + " FROM db2." + TableDictionary + " WHERE db2." + TableDictionary + "." + FieldDictionaryTermID +
                " NOT IN (SELECT " + FieldDictionaryTermID + " FROM " + TableDictionary + "); ";

            //Build update queries
            //update M_DICTIONARY set DESCRIPTIONEN = db2.DESCRIPTIONEN,
            //    DESCRIPTIONFR = db2.DESCRIPTIONFR,
            //    ITEMORDER = db2.ITEMORDER,
            //    DEFAULTVALUE = db2.DEFAULTVALUE,
            //    EDITOR = db2.EDITOR,
            //    EDITDATE = db2.EDITDATE,
            //    VISIBLE = db2.VISIBLE
            //    FROM(select * from db2.M_DICTIONARY) as db2
            //    where db2.TERMID = M_DICTIONARY.TERMID
            string updateQuery = "UPDATE " + TableDictionary + " SET " +
                FieldDictionaryDescription + " = db2." + FieldDictionaryDescription + ", " +
                FieldDictionaryOrder + " = db2." + FieldDictionaryOrder + ", " +
                FieldDictionaryDefault + " = db2." + FieldDictionaryDefault + ", " +
                FieldDictionaryEditor + " = db2." + FieldDictionaryEditor + ", " +
                FieldDictionaryEditorDate + " = db2." + FieldDictionaryEditorDate + ", " +
                FieldDictionaryRemarks + " = db2." + FieldDictionaryRemarks + ", " +
                FieldDictionaryVisible + " = db2." + FieldDictionaryVisible + " FROM (SELECT * FROM db2. " +
                TableDictionary + ") as db2 WHERE db2." +
                FieldDictionaryTermID + " = " + TableDictionary + "." + FieldDictionaryTermID + ";";

            //Build detach query
            string detachQuery = "DETACH DATABASE db2;";

            //Build vacuum query
            string vacuumQuery = "VACUUM";

            //Build final query
            List<string> queryList = new List<string>() { updateQuery, insertQuery, insertQuery2, detachQuery, vacuumQuery };

            SQLiteAsyncConnection vocabToDBConnection = new SQLiteAsyncConnection(vocabToDBPath);
            await vocabToDBConnection.ExecuteAsync(attachQuery);

            //Update working database
            foreach (string q in queryList)
            {
                try
                {
                    await vocabToDBConnection.ExecuteAsync(q);
                }
                catch (Exception e)
                {
                    exceptionList.Add(e);
                }

            }

            await vocabToDBConnection.CloseAsync();

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
                new ErrorToLogFile(wholeStack + "\n DB:" + vocabFromDBPath).WriteToFile();

            }
            else
            {
                completedWithoutErrors = true;
            }

            return completedWithoutErrors;

        }

        #endregion

        #region GET METHODS (usually needs a connection object)
        /// <summary>
        /// Will return a table mapping object create from a type object that represent the table to map.
        /// </summary>
        /// <param name="tableName">The table type object from boxing a class into a type</param>
        /// <param name="dbConnect">An existing database connection</param>
        /// <returns>Will return an empty list if table name wasn't found in database.</returns>
        private static async Task<TableMapping> GetATableObjectAsync(Type tableType, SQLiteAsyncConnection dbConnect)
        {
            //Will return a TableMapping object created from the given type. Type, deriving from the model class, should be true, else 
            //things might fail.
            return await dbConnect.GetMappingAsync(tableType);
        }

        /// <summary>
        /// Will return the number of records of a table
        /// </summary>
        /// <param name="inTabelType"></param>
        /// <returns></returns>
        public async Task<int> GetTableCount(Type inTableType)
        {
            //Variables
            int tableCount = 0;

            //Get query result
            TableMapping tableMapping = await GetATableObjectAsync(inTableType, DbConnection);

            List<int> tableRows = await DbConnection.QueryScalarsAsync<int>("SELECT * FROM " + tableMapping.TableName);

            if (tableRows.Count() > 0)
            {
                tableCount = tableRows.Count();
            }
            
            return tableCount;

        }

        /// <summary>
        /// Will return a related structure record as an object
        /// </summary>
        /// <param name="StrucId"></param>
        /// <returns></returns>
        public async Task<Structure> GetRelatedStructure(int? StrucId)
        {
            Structure relatedStructure = new Structure();
            if (StrucId != null)
            {
                List<Structure> relatedStructures = await DbConnection.Table<Structure>().Where(struc => struc.StructureID == StrucId).ToListAsync();

                if (relatedStructures != null  && relatedStructures.Count > 0)
                {
                    relatedStructure = relatedStructures[0];
                }
            }

            return relatedStructure;
        }

        /// <summary>
        /// Will get a read from F_METADATA.VERSIONSCHEMA and will return value in double
        /// </summary>
        /// <returns></returns>
        public async Task<double> GetDBVersion()
        {
            double schemaVersion = 0.0;

            List<Metadata> mets = await DbConnection.Table<Metadata>().ToListAsync();

            if (mets != null && mets.Count() > 0)
            {
                //Default to first one 

                //Parse result
                if (mets[0].VersionSchema != null)
                {
                    double.TryParse(mets[0].VersionSchema.Replace(",", "."), System.Globalization.CultureInfo.InvariantCulture, out schemaVersion);
                }

            }

            return schemaVersion;
        }

        #endregion

    }
}
