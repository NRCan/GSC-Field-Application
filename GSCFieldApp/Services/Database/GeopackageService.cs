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
using NetTopologySuite;


namespace GSCFieldApp.Services.DatabaseServices
{
    public class GeopackageService
    {

        public DataAccess dAcccess = new DataAccess();
        public static GeometryFactory defaultGeometryFactory = new GeometryFactory();

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
        public byte[] CreateByteGeometryPoint(double x, double y)
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
        public NTS.Geometries.Point GetGeometryPointFromByte(byte[] geomBytes, int srid = DatabaseLiterals.KeywordEPSGDefault)
        {
            NTS.Geometries.Point outPoint = new NTS.Geometries.Point(new Coordinate());

            GeoPackageGeoReader geoReader = new GeoPackageGeoReader();
            
            NTS.Geometries.Geometry geom = geoReader.Read(geomBytes);
            
            if (geom.OgcGeometryType == NTS.Geometries.OgcGeometryType.Point)
            {
                outPoint = geom as NTS.Geometries.Point;
            }

            if (srid != DatabaseLiterals.KeywordEPSGDefault && outPoint != null)
            {
                //Create a coord factory for incoming traverses
                CoordinateSystemFactory csFact = new CoordinateSystemFactory();
                string wkt = outPoint.ToString();
                CoordinateSystem incomingProjection = csFact.CreateFromWkt(wkt);

                //Default map page coordinate
                GeographicCoordinateSystem wgs84 = GeographicCoordinateSystem.WGS84;

                //Transform
                outPoint = TransformPointCoordinates(outPoint, incomingProjection, wgs84);
            }

            return outPoint;
        }

        /// <summary>
        /// From a given byte array that defines a geometry, will return a line object
        /// TODO make this working, incoming geometry has a SRID = -1 so we can't convert incoming points
        /// </summary>
        /// <param name="geomBytes">The byte array to transform into a point object</param>
        /// <returns></returns>
        public async Task<NTS.Geometries.LineString> GetGeometryLineFromByte(byte[] geomBytes, int srid = DatabaseLiterals.KeywordEPSGDefault, int outSrid = DatabaseLiterals.KeywordEPSGMapsuiDefault)
        {
            //Init
            NTS.Geometries.LineString outLine = new NTS.Geometries.LineString(new Coordinate[] { });

            //build geopackag reader to get geometry as proper object
            PrecisionModel precisionModel = new PrecisionModel(PrecisionModels.FloatingSingle); //to get all decimals
            CoordinateSequenceFactory coordinateSequenceFactory = NtsGeometryServices.Instance.DefaultCoordinateSequenceFactory;
            GeoPackageGeoReader geoReader = new GeoPackageGeoReader(coordinateSequenceFactory, precisionModel);
            geoReader.HandleSRID = true;
            NTS.Geometries.Geometry geom = geoReader.Read(geomBytes);

            if (geom.OgcGeometryType == NTS.Geometries.OgcGeometryType.LineString)
            {
                outLine = geom as NTS.Geometries.LineString;
            }

            if (outLine != null && outLine.SRID != -1 && outLine.SRID != outSrid)
            {
                //Create a coord factory for incoming traverses
                CoordinateSystem incomingProjection = await SridReader.GetCSbyID(outLine.SRID);

                //Default map page coordinate
                CoordinateSystem outgoingProjection = await SridReader.GetCSbyID(outSrid);

                //Transform
                outLine = await TransformLineCoordinates(outLine, incomingProjection, outgoingProjection);
            }

            return outLine;
        }


        /// <summary>
        /// Will take a input point object and transform it from one coordinate system
        /// to another.
        /// </summary>
        /// <param name="inPointCoordinates"></param>
        /// <param name="inCoordSystem"></param>
        /// <param name="outCoordSystem"></param>
        /// <returns></returns>
        public static NTS.Geometries.Point TransformPointCoordinates(NTS.Geometries.Point inPointCoordinates, CoordinateSystem inCoordSystem, CoordinateSystem outCoordSystem)
        {
            //Init
            NTS.Geometries.Point outPoint = new NTS.Geometries.Point(new Coordinate());

            //Transform
            CoordinateTransformationFactory ctFact = new CoordinateTransformationFactory();
            ICoordinateTransformation trans = ctFact.CreateFromCoordinateSystems(inCoordSystem, outCoordSystem);
            double[] pointDouble = { inPointCoordinates.X, inPointCoordinates.Y };
            double[] transformedPoint = trans.MathTransform.Transform(pointDouble);

            //Create point
            outPoint = defaultGeometryFactory.CreatePoint(new NetTopologySuite.Geometries.Coordinate(transformedPoint[0], transformedPoint[1]));

            return outPoint;
        }

        /// <summary>
        /// Will take a input line string object and transform it from one coordinate system
        /// to another.
        /// TODO: Find why transforming 4326 to 3857 fails with a Y translation further south
        /// Even QGIS can transform correctly. Something is not properly set in the transformation.
        /// </summary>
        /// <param name="inLineCoordinates"></param>
        /// <param name="inCoordSystem"></param>
        /// <param name="outCoordSystem"></param>
        /// <returns></returns>
        public static async Task<NTS.Geometries.LineString> TransformLineCoordinates(NTS.Geometries.LineString inLineCoordinates, CoordinateSystem inCoordSystem, CoordinateSystem outCoordSystem)
        {
            //Init
            NTS.Geometries.LineString outLine = new NTS.Geometries.LineString(new Coordinate[] { });
            Coordinate[] transformedCoordinates = new Coordinate[inLineCoordinates.Count];
            int coordinateIndex = 0;

            //Keep error log
            try
            {
                //Transform
                CoordinateTransformationFactory ctFact = new CoordinateTransformationFactory();
                ICoordinateTransformation trans = ctFact.CreateFromCoordinateSystems(inCoordSystem, outCoordSystem);

                foreach (Coordinate c in inLineCoordinates.Coordinates)
                {
                    double[] pointDouble = { c.X, c.Y };
                    double[] transformedPoint = trans.MathTransform.Transform(pointDouble);
                    transformedCoordinates[coordinateIndex] = new Coordinate(transformedPoint[0], transformedPoint[1]);
                    coordinateIndex++;
                }

                //Create line
                outLine = defaultGeometryFactory.CreateLineString(transformedCoordinates);
            }
            catch (Exception e)
            {
                new ErrorToLogFile(e).WriteToFile();
                await Shell.Current.DisplayAlert("Error", e.Message, "Ok");
            }


            return outLine;
        }
    }
}