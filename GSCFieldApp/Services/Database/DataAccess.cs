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

namespace GSCFieldApp.Services.DatabaseServices
{
    public class DataAccess
    {
        public static SQLiteAsyncConnection _dbConnection;

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
            bool allValues)
        {

            //Get the current project type
            string fieldworkType = DatabaseLiterals.ApplicationThemeBedrock; //Default

            if (Preferences.ContainsKey(DatabaseLiterals.FieldUserInfoFWorkType))
            {
                //This should be set whenever user selects a different field book
                fieldworkType = Preferences.Get(DatabaseLiterals.FieldUserInfoFWorkType, fieldworkType);
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
                queryAndWorkType = " AND (lower(" + TableDictionaryManager + "." + FieldDictionaryManagerSpecificTo + ") like '" + fieldworkType + "%' OR lower(" + TableDictionaryManager + "." + FieldDictionaryManagerSpecificTo + ") = '')";
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
            SQLiteAsyncConnection currentConnection = GetConnectionFromPath(PreferedDatabasePath);

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
        public ComboBox GetComboboxListFromVocab(IEnumerable<Vocabularies> inVocab)
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
            ComboBox outputVocabs = new ComboBox();
            outputVocabs.cboxItems = outputVocabsList;
            outputVocabs.cboxDefaultItemIndex = defaultValueIndex; 

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

    }
}
