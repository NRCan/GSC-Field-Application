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
using GSCFieldApp.Themes;

// Based on code sample from: http://blogs.u2u.be/diederik/post/2015/09/08/Using-SQLite-on-the-Universal-Windows-Platform.aspx -Kaz
namespace GSCFieldApp.Services.DatabaseServices
{
    public class DataAccess
    {
        public static SQLiteAsyncConnection _dbConnection;
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
        private SQLiteAsyncConnection DbConnection
        {
            get
            {
                if (PreferedDatabasePath != string.Empty)
                {
                    return new SQLiteAsyncConnection(PreferedDatabasePath);
                }
                else
                {
                    return _dbConnection;
                }

            }
        }

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
            await DbConnection.CloseAsync();
        }

        #endregion

        #region DATA MANAGEMENT METHODS (Create, Update, Read)

        /// <summary>
        /// Will write an embedded resource to a file with a binary writer. In case it exists, it will replace it.
        /// Will save the resource to the local folder.
        /// </summary>
        public async Task<bool> CreateDatabaseFromResource(string outputDatabasePath)
        {
            try
            {
                if (!File.Exists(outputDatabasePath))
                {
                    //Open stream with embeded resource
                    using Stream package = await FileSystem.OpenAppPackageFileAsync(@"GSCFieldwork.gpkg");

                    //Open empty stream for output file
                    using FileStream outputStream = System.IO.File.OpenWrite(outputDatabasePath);

                    //Need a binary write for geopackage database, else file will be corrupt with 
                    //default stream writer/reader
                    byte[] buffer = new byte[1024];
                    using (BinaryWriter fileWriter = new BinaryWriter(outputStream))
                    {
                        using (BinaryReader fileReader = new BinaryReader(package))
                        {
                            //Read package by block of 1024 bytes.
                            long readCount = 0;
                            while (readCount < fileReader.BaseStream.Length)
                            {
                                int read = fileReader.Read(buffer, 0, buffer.Length);
                                readCount += read;

                                //Write
                                fileWriter.Write(buffer, 0, read);
                            }
                        }
                    }

                }

                return true;
                
            }
            catch (Exception e)
            {
                Debug.Write(e.Message);
                return false;
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
                Console.WriteLine(ex.ToString());
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
            bool allValues, string fieldwork = "")
        {

            //Get the current project type
            string fieldworkType = ScienceLiterals.ApplicationThemeBedrock; //Default

            if (fieldwork != string.Empty)
            {
                fieldworkType = fieldwork;
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

            //Get vocab records
            SQLiteAsyncConnection currentConnection = GetConnectionFromPath(DatabaseFilePath);
            List<Vocabularies> vocabs = await currentConnection.QueryAsync<Vocabularies>(finalQuery);

            Vocabularies voc = new Vocabularies();
            List<Vocabularies> vocTable = new List<Vocabularies> { voc };
            if (vocabs.Count != 0)
            {
                vocTable = vocabs;
            }

            return vocTable;
        }

        /// <summary>
        /// From a given table and field name, will retrieve associated vocabulary and
        /// output a list of combobox items. An output parameter is also available 
        /// for default value if one is stated in the database or if N.A. is the only available choice.
        /// This method is meant for generic list with no queries
        /// </summary>
        /// <param name="tableName">The table name associated with the wanted vocab.</param>
        /// <param name="fieldName">The field name associated with the wanted vocab.</param>
        /// <param name="fieldwork">The field book theme (bedrock, surficial)</param>
        /// <returns></returns>
        public async Task<Tuple<List<ComboBoxItem>,int>> GetComboboxListWithVocabAsync(string tableName, string fieldName, string fieldwork = "")
        {
            //Outputs
            Tuple<List<ComboBoxItem>, int> outputVocabs = Tuple.Create(new List<ComboBoxItem>(), -1);

            //Get vocab
            DataAccess picklistAccess = new DataAccess();
            List<Vocabularies> vocs = await picklistAccess.GetPicklistValuesAsync(tableName, fieldName, string.Empty, false, fieldwork);

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
        public Tuple<List<ComboBoxItem>, int> GetComboboxListFromVocab(IEnumerable<Vocabularies> inVocab)
        {
            //Outputs
            List<ComboBoxItem> outputVocabsList = new List<ComboBoxItem>();
            int defaultValueIndex = -1;

            //Fill in cbox
            foreach (Vocabularies vocabs in inVocab)
            {
                ComboBoxItem newItem = new ComboBoxItem();

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

                //Select default if stated in database
                if (vocabs.DefaultValue != null && vocabs.DefaultValue == Dictionaries.DatabaseLiterals.boolYes)
                {
                    defaultValueIndex = outputVocabsList.Count;
                }

                outputVocabsList.Add(newItem);
            }

            //Set
            Tuple<List<ComboBoxItem>, int> outputVocabs = Tuple.Create(outputVocabsList, defaultValueIndex);

            return outputVocabs;
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

        #endregion

        #region GEOPACKAGE

        /// <summary>
        /// Will generate an insert query for geopackages
        /// NOTE: Needs to stay here else it conflicts with the spatialitesharp which doesn't use
        /// the same libraries and throws a bunch of conflict.
        /// </summary>
        /// <param name="inClassObject"></param>
        /// <returns></returns>
        public async Task<string> GetGeopackageInsertQueryAsync(FieldLocation inLocation, string tableNameIfDifferent = "")
        {
            //Variables
            string insertQuery = @"INSERT INTO "; //Init

            //Get table mapping
            TableMapping inMapping = await GetATableObjectAsync(inLocation.GetType(), DbConnection);

            //Add table name in query
            string inTableName = inLocation.LocationTableName;
            if (tableNameIfDifferent != inTableName && tableNameIfDifferent != string.Empty)
            {
                inTableName = tableNameIfDifferent;
            }
            insertQuery = insertQuery + inTableName + " (" + DatabaseLiterals.FieldGenericGeometry + ",";

            //Fill in the field name list of the query
            foreach (TableMapping.Column col in inMapping.Columns)
            {
                Type colType = col.ColumnType;

                var value = col.GetValue(inLocation);

                if (value != null && col.Name != DatabaseLiterals.FieldGenericGeometry)
                {
                    insertQuery = insertQuery + col.Name + ",";
                }

            }

            //Remove last comma
            insertQuery = insertQuery.Remove(insertQuery.Length - 1, 1);

            //Add make point sql method
            insertQuery = insertQuery + ") VALUES (gpkgMakePoint( " + inLocation.LocationLong +
                ", " + inLocation.LocationLat + ", " + inLocation.LocationDatum + ")";

            //Finalize query with values
            foreach (TableMapping.Column col in inMapping.Columns)
            {
                Type colType = col.ColumnType;

                var value = col.GetValue(inLocation);

                if (value != null && col.Name != DatabaseLiterals.FieldGenericGeometry)
                {
                    if (colType == typeof(System.String))
                    {
                        value = '"' + value.ToString() + '"';
                    }

                    if (col.Name == DatabaseLiterals.FieldLocationID)
                    {
                        value = "NULL";
                    }

                    if (col == inMapping.Columns.Last())
                    {
                        insertQuery = insertQuery + ", " + value + ") ";
                    }
                    else
                    {
                        insertQuery = insertQuery + ", " + value;
                    }

                }

            }

            return insertQuery + " returning " + DatabaseLiterals.FieldLocationID + ";";


        }

        /// <summary>
        /// Will generate an update query for geopackages geometry field
        /// </summary>
        /// <param name="inClassObject"></param>
        /// <param name="tableNameIfDifferent"></param>
        /// <returns></returns>
        public string GetGeopackageUpdateQuery(string tableName)
        {
            //Variables cast(EPSG as integer)
            string upQuery = string.Empty; //Init

            if (tableName == DatabaseLiterals.TableLocation)
            {
                upQuery = @"UPDATE " + tableName + " SET " + FieldGenericGeometry + " = " +
                    " gpkgMakePoint( " + FieldLocationLongitude + ", " + FieldLocationLatitude + ", cast(" +
                    FieldLocationDatum + " as integer));";
            }

            return upQuery;
        }

        #endregion

    }
}
