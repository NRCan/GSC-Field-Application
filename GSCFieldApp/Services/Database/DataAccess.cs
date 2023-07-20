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

        /// <summary>
        /// Will save (insert or update) an item object that is a model table
        /// </summary>
        /// <param name="item"></param>
        /// <param name="doUpdate"></param>
        /// <returns></returns>
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

        #endregion

    }
}
