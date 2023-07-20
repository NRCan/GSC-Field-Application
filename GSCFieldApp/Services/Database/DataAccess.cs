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

        /// <summary>
        /// Will return true of false if default database exists in the system.
        /// </summary>
        /// <returns>True if exists</returns>
        public bool DoesDefaultDatabaseExists()
        {
            if (DatabaseFilePath == string.Empty)
            {
                return false;
            }
            else
            {
                return File.Exists(DatabaseFilePath);
            }
            
        }


        #endregion

        #region DATA MANAGEMENT METHODS (Create, Update, Read)

        /// <summary>
        /// Will write an embedded resource to a file with a binary writer. In case it exists, it will replace it.
        /// Will save the resource to the local folder.
        /// </summary>
        public async Task CreateDatabaseFromResource(string outputDatabasePath)
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

                return;
                
            }
            catch (Exception e)
            {
                Debug.Write(e.Message);
            }


        }

        public async Task<int> SaveItemAsync(object item, bool doUpdate)
        {

            // Create a new connection
            try
            {

                //For debug
                DbConnection.Tracer = new Action<string>(q => Debug.WriteLine(q));
                DbConnection.Trace = true;

                if (doUpdate)
                {

                    return await DbConnection.UpdateAsync(item);

                }
                else
                {
                    return await DbConnection.InsertAsync(item);
                }

            }
            catch (SQLite.SQLiteException ex)
            {
                Console.WriteLine(ex.ToString());
                return 0;
            }

        }

        /// <summary>
        /// Will return a list of object (rows) from a given table name.
        /// In addition a query can be passed to filter results
        /// </summary>
        /// <param name="tableName">The table type object deriving from a class.</param>
        /// <param name="query">A query to filter table name, Can handle string.empty and null</param>
        /// <returns>A list of object that will act as rows.</returns>
        public async Task<List<object>> ReadTableAsync(Type tableType, string query)
        {
            return await ReadTableFromDBConnection(tableType, query, DbConnection);

        }

        /// <summary>
        /// Will return a list of object (rows) from a given table name.
        /// In addition a query can be passed to filter results.
        /// Can be used with any database, not the default working one.
        /// </summary>
        /// <param name="tableName">The table type object deriving from a class.</param>
        /// <param name="query">A query to filter table name, Can handle string.empty and null</param>
        /// <returns>A list of object that will act as rows.</returns>
        public async Task<List<object>> ReadTableFromDBConnection(Type tableType, string query, SQLiteAsyncConnection inConnection)
        {
            //Get the proper table object to read from it
            TableMapping tableMapTask = await inConnection.GetMappingAsync(tableType);

            //Check for table existance
            try
            {
                //Get table info
                return await inConnection.QueryAsync(tableMapTask, query);

            }
            catch (Exception e)
            {
                Debug.Write(e.Message);
                return null;
            }
        
        }

        #endregion

    }
}
