using SpatialiteSharp;
using System.Data.SQLite;

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
        /// </summary>
        /// <param name="in_query"></param>
        /// <returns></returns>
        public int DoSpatialiteQueryInGeopackage(string in_query, bool doScalar = true)
        {

            string dbPath = dAcccess.GetDefaultDatabasePath;

            int newId = int.MinValue;

            // Create a new connection
            using (SQLiteConnection db = new SQLiteConnection())
            {
                db.ConnectionString = @"Data Source=" + dbPath + "; Version=3";
                db.Open();

                //Make sure journal creation is off (anti wal and shm files)
                //https://www.sqlite.org/wal.html
                SQLiteCommand wallOffCommand = new SQLiteCommand(@"PRAGMA journal_mode=DELETE;", db);
                wallOffCommand.ExecuteNonQuery();

                using (var transaction = db.BeginTransaction())
                {
                    //Load spatialite extension
                    SpatialiteLoader.Load(db);

                    //Enable amphibious mode to use spatialite sql within geopackage
                    SQLiteCommand amphibiousCommand = new SQLiteCommand(@"select EnableGpkgAmphibiousMode()", db);
                    amphibiousCommand.ExecuteNonQuery();

                    //Pass query
                    //example: "INSERT INTO FS_LOCATION (Shape, locationid, latitude, longitude, metaid) values (MakePoint(-80.314,46.930, 4326), 'test_gab_visual_studio2', 46.930, -80.314, '7297f789-36e8-4c06-86e9-46b9ffcb1607')"
                    SQLiteCommand addLocation = new SQLiteCommand(in_query, db);
                    if (doScalar)
                    {
                        object scalar = addLocation.ExecuteScalar(System.Data.CommandBehavior.SingleResult);
                        newId = int.Parse(scalar.ToString());
                    }
                    else
                    {
                        newId = addLocation.ExecuteNonQuery();
                    }


                    //Disable mode
                    SQLiteCommand amphibiousCommandOff = new SQLiteCommand(@"select DisableGpkgAmphibiousMode()", db);
                    amphibiousCommandOff.ExecuteNonQuery();


                    transaction.Commit();
                }

                db.Close();
            }

            return newId;
        }

    }
}
