using GeoAPI.CoordinateSystems;
using ProjNet.CoordinateSystems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSCFieldApp.Services
{
    //https://github.com/NetTopologySuite/ProjNet4GeoAPI/wiki/Loading-a-projection-by-Spatial-Reference-ID
    public class SridReader
    {
        private static string filename = @"SRID.csv"; //Change this to point to the SRID.CSV file.

        public struct WKTstring
        {
            /// <summary>Well-known ID</summary>
            public int WKID;
            /// <summary>Well-known Text</summary>
            public string WKT;
        }

        /// <summary>Enumerates all SRID's in the SRID.csv file.</summary>
        /// <returns>Enumerator</returns>
        public static async IAsyncEnumerable<WKTstring> GetSRIDs()
        {
            //IEnumerable<WKTstring> SRIDs = new List<WKTstring>();
            using (Stream stream = await FileSystem.OpenAppPackageFileAsync(filename))
            {
                using (StreamReader streamReader = new StreamReader(stream))
                {

                    while (!streamReader.EndOfStream)
                    {
                        string line = streamReader.ReadLine();
                        int split = line.IndexOf(';');
                        if (split > -1)
                        {
                            WKTstring wkt = new WKTstring();
                            wkt.WKID = int.Parse(line.Substring(0, split));
                            wkt.WKT = line.Substring(split + 1);
                            yield return wkt;
                        }
                    }
                    streamReader.Close();
                }
            }
        }
        /// <summary>Gets a coordinate system from the SRID.csv file</summary>
        /// <param name="id">EPSG ID</param>
        /// <returns>Coordinate system, or null if SRID was not found.</returns>
        public static async Task<CoordinateSystem> GetCSbyID(int id)
        {
            IAsyncEnumerable<WKTstring> wkts = SridReader.GetSRIDs();

            await foreach (SridReader.WKTstring wkt in wkts)
            {
                if (wkt.WKID == id) //We found it!
                {
                    CoordinateSystemFactory csFact = new CoordinateSystemFactory();
                    return csFact.CreateFromWkt(wkt.WKT);
                }
            }
            return null;
        }
    }
}
