using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSCFieldApp.Services.DatabaseServices;
using System.Data.Common;
using System.Data.SqlClient;
using System.Collections.ObjectModel;
using System.Transactions;
using NTS = NetTopologySuite;
using GSCFieldApp.Models;
using NetTopologySuite.IO;
using GSCFieldApp.Dictionaries;

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
        /// Will take input coordinates and transform them in
        /// geopackage valid byte array geometry.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public byte[] GetGeometry(double x, double y)
        {
            //Initiate a new NTS instance
            NTS.NtsGeometryServices.Instance = new NTS.NtsGeometryServices(
                // default CoordinateSequenceFactory
                NTS.Geometries.Implementation.CoordinateArraySequenceFactory.Instance,
                // default precision model
                new NTS.Geometries.PrecisionModel(1000d),
                // default SRID
                DatabaseLiterals.KeywordEPSGDefault,
                // Geometry overlay operation function set to use (Legacy or NG)
                NTS.Geometries.GeometryOverlay.NG,
                // Coordinate equality comparer to use (CoordinateEqualityComparer or PerOrdinateEqualityComparer)
                new NTS.Geometries.CoordinateEqualityComparer()
            );

            // Create a geometry factory with the spatial reference id 4326
            var gf = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(DatabaseLiterals.KeywordEPSGDefault);

            // Create a point at desired location
            NTS.Geometries.Point inPoint = gf.CreatePoint(new NetTopologySuite.Geometries.Coordinate(x, y));

            // Convert geometry to geopackage geometry bytes
            GeoPackageGeoWriter geo = new GeoPackageGeoWriter();
            byte[] bytePoint = geo.Write(inPoint);

            return bytePoint;

        }

    }
}