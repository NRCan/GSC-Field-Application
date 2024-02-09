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
using NetTopologySuite.Geometries;
//using GeoAPI.CoordinateSystems;
//using GeoAPI.CoordinateSystems.Transformations;
using ProjNet.CoordinateSystems.Projections;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;

namespace GSCFieldApp.Services.DatabaseServices
{
    public class GeopackageService
    {
        public DataAccess dAcccess = new DataAccess();
        public GeometryFactory defaultGeometryFactory = new GeometryFactory();

        /// <summary>
        /// This special class is used since mod_spatialite can't seem to be working
        /// along with sqlite-net-pcl. 
        /// </summary>
        public GeopackageService()
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
            defaultGeometryFactory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(DatabaseLiterals.KeywordEPSGDefault);

        }

        /// <summary>
        /// Will take input coordinates and transform them in
        /// geopackage valid byte array geometry.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public byte[] CreateByteGeometry(double x, double y)
        {
            // Create a point at desired location
            NTS.Geometries.Point inPoint = defaultGeometryFactory.CreatePoint(new NetTopologySuite.Geometries.Coordinate(x, y));

            // Convert geometry to geopackage geometry bytes
            GeoPackageGeoWriter geo = new GeoPackageGeoWriter();
            byte[] bytePoint = geo.Write(inPoint);
            
            return bytePoint;

        }

        /// <summary>
        /// From a given byte array that defines a geometry, will return a point object
        /// TODO make this working, incoming geometry has a SRID = -1 so we can't convert incoming points
        /// </summary>
        /// <param name="geomBytes">The byte array to transform into a point object</param>
        /// <returns></returns>
        public NTS.Geometries.Point GetGeometryFromByte(byte[] geomBytes, bool transformToWGS84 = true)
        {
            NTS.Geometries.Point outPoint = new NTS.Geometries.Point(new Coordinate());

            GeoPackageGeoReader geoReader = new GeoPackageGeoReader();
            
            NTS.Geometries.Geometry geom = geoReader.Read(geomBytes);
            
            if (geom.OgcGeometryType == NTS.Geometries.OgcGeometryType.Point)
            {
                outPoint = geom as NTS.Geometries.Point;
            }

            if (transformToWGS84 && outPoint != null)
            {
                //Create a coord factory for incoming traverses
                CoordinateSystemFactory csFact = new CoordinateSystemFactory();
                string wkt = outPoint.ToString();
                CoordinateSystem incomingProjection = csFact.CreateFromWkt(wkt);

                //Default map page coordinate
                GeographicCoordinateSystem wgs84 = GeographicCoordinateSystem.WGS84;

                //Transform
                CoordinateTransformationFactory ctFact = new CoordinateTransformationFactory(); 
                ICoordinateTransformation trans = ctFact.CreateFromCoordinateSystems(incomingProjection, wgs84);
                double[] pointDouble = { outPoint.X, outPoint.Y};
                double[] transformedPoint = trans.MathTransform.Transform(pointDouble);

                // Create a point at desired location
                outPoint = defaultGeometryFactory.CreatePoint(new NetTopologySuite.Geometries.Coordinate(transformedPoint[0], transformedPoint[1]));
            }

            return outPoint;
        }
    }
}