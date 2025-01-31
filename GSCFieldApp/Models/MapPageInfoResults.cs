using GSCFieldApp.Dictionaries;
using GSCFieldApp.Services.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSCFieldApp.Models
{
    public class MapPageInfoResults
    {

        public Collection<MapPageInfoResult> infoResults { get; set; }

        public MapPageInfoResults()
        {
            infoResults = new Collection<MapPageInfoResult>();
        }

        /// <summary>
        /// Will output a collection of properly parsed results for UI displaying
        /// </summary>
        /// <param name="results"></param>
        public MapPageInfoResults(object?[][]? results)
        {
            infoResults = new Collection<MapPageInfoResult>();

            //Field names are stored in first array
            //Field values are stored in second array
            if (results != null && results.Count() >= 2)
            {
                int trackingIndex = 0;
                foreach (object obj in results[1])
                {
                    MapPageInfoResult mpi = new MapPageInfoResult();
                    mpi.FieldName = results[0][trackingIndex].ToString();
                    if (mpi.FieldName != DatabaseLiterals.FieldGenericGeometry &&
                        mpi.FieldName != GeopackageService.GpkgFieldGeometry)
                    {
                        try
                        {
                            if (obj != null)
                            {
                                mpi.FieldValue = obj.ToString();
                            }
                            else
                            {
                                mpi.FieldValue = string.Empty;
                            }

                        }
                        catch (Exception)
                        {
                            mpi.FieldValue = string.Empty;
                        }

                        infoResults.Add(mpi);
                    }

                    
                    trackingIndex++;
                }
            }
        }

    }

    public class MapPageInfoResult
    {
        public string FieldName { get; set; }
        public string FieldValue { get; set; }
    }
}
