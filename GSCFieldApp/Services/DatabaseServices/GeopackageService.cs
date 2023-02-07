using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSCFieldApp.Services.DatabaseServices;
using System.Data.SQLite;
using System.Data.Common;
using System.Data.SqlClient;
using System.Collections.ObjectModel;
using SpatialiteSharp;

namespace GSCFieldApp.Services.DatabaseServices
{
    public class GeopackageService
    {
        public DataAccess dAcccess = new DataAccess();

        public GeopackageService()
        { 

        }

        /// <summary>
        /// Will perform a spatialite sql query within a geopackage
        /// </summary>
        /// <param name="in_query"></param>
        /// <returns></returns>
        public void DoSpatialiteQueryInGeopackage(string in_query)
        {

            string dbPath = dAcccess.GetDefaultDatabasePath;

            // Create a new connection
            using (SQLiteConnection db = new SQLiteConnection())
            {
                db.ConnectionString = @"Data Source=" + dbPath + "; Version=3";
                db.Open();

                using (var transaction = db.BeginTransaction())
                {
                    //Load spatialite extension
                    SpatialiteLoader.Load(db);

                    //Enable amphibious mode to use spatialite sql within geopackage
                    SQLiteCommand amphibiousCommand = new SQLiteCommand(@"select EnableGpkgAmphibiousMode()", db);
                    amphibiousCommand.ExecuteNonQuery();

                    //Pass query
                    //example: "INSERT INTO FS_LOCATION4 (Shape, locationid) values (MakePoint(-80.314,46.930, 4326), 'test_gab_visual_studio2')"
                    SQLiteCommand addLocation = new SQLiteCommand(in_query, db);
                    addLocation.ExecuteNonQuery();
                    
                    transaction.Commit();
                }

                db.Close();
            }

        }

    }
}
