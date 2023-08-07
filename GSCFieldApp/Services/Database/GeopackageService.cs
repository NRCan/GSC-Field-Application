using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSCFieldApp.Services.DatabaseServices;
//using SQLite;
using System.Data.SQLite;
using System.Data.Common;
using System.Data.SqlClient;
using System.Collections.ObjectModel;
using System.Transactions;
using SpatialiteSharp;

namespace GSCFieldApp.Services.DatabaseServices
{
    public class GeopackageService
    {
        public DataAccess dAcccess = new DataAccess();

        /// <summary>
        /// This special class is used since mod_spatialite can't seem to be working
        /// along with sqlite-net-pcl. 
        /// </summary>
        public GeopackageService()
        {

        }

        /// <summary>
        /// Will perform a spatialite sql query within a geopackage
        /// NOTE: Need to use system.data.sqlite else spatialiteloader rants about bad connection object
        /// </summary>
        /// <param name="in_query"></param>
        /// <returns></returns>
        public int DoSpatialiteQueryInGeopackage(string in_query, bool doScalar = true)
        {

            string dbPath = dAcccess.PreferedDatabasePath;

            int newId = int.MinValue;

            // Create a new connection
            ///TODO this causes android to stop building 'PE image does not have metadata'
            ///once removing system.data.sqlite.core from install libs this works.
            using (System.Data.SQLite.SQLiteConnection db = new System.Data.SQLite.SQLiteConnection())
            {
                db.ConnectionString = @"Data Source=" + dbPath + "; Version=3;";
                db.Open();

                //Make sure journal creation is off (anti wal and shm files)
                //https://www.sqlite.org/wal.html
                System.Data.SQLite.SQLiteCommand wallOffCommand = new System.Data.SQLite.SQLiteCommand(@"PRAGMA journal_mode=DELETE;", db);
                wallOffCommand.ExecuteNonQuery();


                using (var transaction = db.BeginTransaction())
                {


                    //Load spatialite extension
                    SpatialiteLoader.Load(db);

                    //Enable amphibious mode to use spatialite sql within geopackage
                    System.Data.SQLite.SQLiteCommand amphibiousCommand = new System.Data.SQLite.SQLiteCommand(@"select EnableGpkgAmphibiousMode()", db);
                    amphibiousCommand.ExecuteNonQuery();

                    //Pass query
                    //example: "INSERT INTO FS_LOCATION (Shape, locationid, latitude, longitude, metaid) values (MakePoint(-80.314,46.930, 4326), 'test_gab_visual_studio2', 46.930, -80.314, '7297f789-36e8-4c06-86e9-46b9ffcb1607')"
                    System.Data.SQLite.SQLiteCommand addLocation = new System.Data.SQLite.SQLiteCommand(in_query, db);
                    if (doScalar)
                    {
                        object scalar = addLocation.ExecuteScalar();
                        newId = int.Parse(scalar.ToString());
                    }
                    else
                    {
                        newId = addLocation.ExecuteNonQuery();
                    }


                    //Disable mode
                    System.Data.SQLite.SQLiteCommand amphibiousCommandOff = new System.Data.SQLite.SQLiteCommand(@"select DisableGpkgAmphibiousMode()", db);
                    amphibiousCommandOff.ExecuteNonQuery();

                    transaction.Commit();
                }

                db.Close();
            }

            return newId;
        }

    }
}