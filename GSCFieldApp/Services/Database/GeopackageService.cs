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
using System.Runtime.CompilerServices;

namespace GSCFieldApp.Services.DatabaseServices
{
    public class GeopackageService
    {

        public DataAccess dAcccess = new DataAccess();
        public static GeometryFactory defaultGeometryFactory = new GeometryFactory();
        public static GeometryFactory defaultMapsuiGeometryFactory = new GeometryFactory();

        //Geopackage strings
        public const string GpkgTableGeometry = "gpkg_geometry_columns";
        public const string GpkgTableStyle = "layer_styles";

        public const string GpkgFieldTableName = "table_name";
        public const string GpkgFieldSRID = "srs_id";
        public const string GpkgFieldStyleTableName = "f_table_name";
        public const string GpkgFieldStyleSLD = "styleSLD";
        public const string GpkgFieldGeometry = "geom";
        public const string GpkgFieldGeometryType = "geometry_type_name";

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

            //Create another one with the default spatial reference from Mapsui
            defaultMapsuiGeometryFactory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(DatabaseLiterals.KeywordEPSGMapsuiDefault);

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
        public async Task<NTS.Geometries.Point> GetGeometryPointFromByteAsync(byte[] geomBytes, int srid = DatabaseLiterals.KeywordEPSGDefault, int outSRID = DatabaseLiterals.KeywordEPSGMapsuiDefault)
        {
            NTS.Geometries.Point outPoint = new NTS.Geometries.Point(new Coordinate());

            //build geopackage reader to get geometry as proper object
            PrecisionModel precisionModel = new PrecisionModel(PrecisionModels.FloatingSingle); //to get all decimals
            CoordinateSequenceFactory coordinateSequenceFactory = NtsGeometryServices.Instance.DefaultCoordinateSequenceFactory;
            GeoPackageGeoReader geoReader = new GeoPackageGeoReader(coordinateSequenceFactory, precisionModel);
            geoReader.HandleSRID = true;
            NTS.Geometries.Geometry geom = geoReader.Read(geomBytes);

            if (geom.OgcGeometryType == NTS.Geometries.OgcGeometryType.Point)
            {
                outPoint = geom as NTS.Geometries.Point;
            }

            if (outPoint != null && outPoint.SRID != -1 && outPoint.SRID != outSRID)
            {
                //Create a coord factory for incoming traverses
                CoordinateSystem incomingProjection = await SridReader.GetCSbyID(outPoint.SRID);

                //Default map page coordinate
                CoordinateSystem outgoingProjection = await SridReader.GetCSbyID(outSRID);


                //Transform
                outPoint = await TransformPointCoordinates(outPoint, incomingProjection, outgoingProjection);
            }

            return outPoint;
        }

        /// <summary>
        /// From a given byte array that defines a geometry, will return a line object
        /// WARNING: About EPSG3857 and it's Y-axis problem
        /// https://alastaira.wordpress.com/2011/01/23/the-google-maps-bing-maps-spherical-mercator-projection/
        /// </summary>
        /// <param name="geomBytes">The byte array to transform into a point object</param>
        /// <returns></returns>
        public async Task<NTS.Geometries.LineString> GetGeometryLineFromByte(byte[] geomBytes, int srid = DatabaseLiterals.KeywordEPSGDefault, int outSrid = DatabaseLiterals.KeywordEPSGMapsuiDefault)
        {
            //Init
            NTS.Geometries.LineString outLine = new NTS.Geometries.LineString(new Coordinate[] { });

            //Build geopackage reader to get geometry as proper object
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
        public static async Task<NTS.Geometries.Point> TransformPointCoordinates(NTS.Geometries.Point inPointCoordinates, CoordinateSystem inCoordSystem, CoordinateSystem outCoordSystem)
        {
            //Init
            NTS.Geometries.Point outPoint = new NTS.Geometries.Point(new Coordinate());

            //Keep error log
            try
            {
                //Transform
                CoordinateTransformationFactory ctFact = new CoordinateTransformationFactory();
                ICoordinateTransformation trans = ctFact.CreateFromCoordinateSystems(inCoordSystem, outCoordSystem);
                double[] pointDouble = { inPointCoordinates.X, inPointCoordinates.Y };
                double[] transformedPoint = trans.MathTransform.Transform(pointDouble);

                //Create point
                outPoint = defaultGeometryFactory.CreatePoint(new NetTopologySuite.Geometries.Coordinate(transformedPoint[0], transformedPoint[1]));

            }
            catch (Exception e)
            {
                new ErrorToLogFile(e).WriteToFile();
                await Shell.Current.DisplayAlert("Error", e.Message, "Ok");
            }


            return outPoint;
        }

        /// <summary>
        /// Will take a input line string object and transform it from one coordinate system
        /// to another.
        /// WARNING: About EPSG3857 and it's Y-axis problem
        /// https://alastaira.wordpress.com/2011/01/23/the-google-maps-bing-maps-spherical-mercator-projection/
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
                outLine = defaultMapsuiGeometryFactory.CreateLineString(transformedCoordinates);
            }
            catch (Exception e)
            {
                new ErrorToLogFile(e).WriteToFile();
                await Shell.Current.DisplayAlert("Error", e.Message, "Ok");
            }


            return outLine;
        }

        /// <summary>
        /// Will take a input line string object and transform it from one coordinate system
        /// to another.
        /// WARNING: About EPSG3857 and it's Y-axis problem
        /// https://alastaira.wordpress.com/2011/01/23/the-google-maps-bing-maps-spherical-mercator-projection/
        /// </summary>
        /// <param name="inLineCoordinates"></param>
        /// <param name="inCoordSystem"></param>
        /// <param name="outCoordSystem"></param>
        /// <returns></returns>
        public static async Task<NTS.Geometries.MultiPolygon> TransformPolygonCoordinates(NTS.Geometries.MultiPolygon inPolyCoordinates, CoordinateSystem inCoordSystem, CoordinateSystem outCoordSystem)
        {
            //Init
            NTS.Geometries.MultiPolygon outPoly = new NTS.Geometries.MultiPolygon(new Polygon[] { });
            Polygon[] transformedPolygons = new Polygon[inPolyCoordinates.Count];
            int coordinateIndex = 0;

            //Keep error log
            try
            {
                
                //Transform
                CoordinateTransformationFactory ctFact = new CoordinateTransformationFactory();
                ICoordinateTransformation trans = ctFact.CreateFromCoordinateSystems(inCoordSystem, outCoordSystem);

                //Shell
                LinearRing shellRing = null;
                foreach (Geometry g in inPolyCoordinates.Geometries)
                {
                    if (g.OgcGeometryType == NTS.Geometries.OgcGeometryType.Polygon)
                    {

                        //foreach (Coordinate c in inPolyCoordinates.Coordinates)
                        //{
                        //    double[] pointDouble = { c.X, c.Y };
                        //    double[] transformedPoint = trans.MathTransform.Transform(pointDouble);
                        //}

                        //if (shellRing == null)
                        //{
                        //    shellRing = new LinearRing()
                        //}

                        //transformedPolygons[coordinateIndex] = new Polygon(transformedPoint[0], transformedPoint[1]);
                        //coordinateIndex++;
                    }

                }

                //Create polygon
                outPoly = defaultMapsuiGeometryFactory.CreateMultiPolygon(transformedPolygons);
            }
            catch (Exception e)
            {
                new ErrorToLogFile(e).WriteToFile();
                await Shell.Current.DisplayAlert("Error", e.Message, "Ok");
            }


            return outPoly;
        }

        /// <summary>
        /// Will add a new coordinate to a linestring object
        /// </summary>
        /// <param name="inLineCoordinates"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static async Task<NTS.Geometries.LineString> AddPointToLineString(NTS.Geometries.LineString inLineCoordinates, double x, double y)
        {
            //Init
            Coordinate[] coordinates = new Coordinate[inLineCoordinates.Count + 1];

            //Add
            for (int i = 0; i < inLineCoordinates.Count; i++)
            {
                coordinates[i] = inLineCoordinates[i];
            }
            coordinates[inLineCoordinates.Count] = new Coordinate(x, y);

            //Convert
            return defaultMapsuiGeometryFactory.CreateLineString(coordinates);
        }

        /// <summary>
        /// Will take input linestring and transform them in
        /// geopackage valid byte array geometry.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public byte[] CreateByteGeometryLine(LineString inLine)
        {

            // Convert geometry to geopackage geometry bytes
            GeoPackageGeoWriter geo = new GeoPackageGeoWriter();
            byte[] bytePoint = geo.Write(inLine);

            return bytePoint;

        }

        /// <summary>
        /// From a given byte array that defines a geometry, will return a polygon object
        /// WARNING: About EPSG3857 and it's Y-axis problem
        /// https://alastaira.wordpress.com/2011/01/23/the-google-maps-bing-maps-spherical-mercator-projection/
        /// </summary>
        /// <param name="geomBytes">The byte array to transform into a point object</param>
        /// <returns></returns>
        public async Task<NTS.Geometries.MultiPolygon> GetGeometryPolygonFromByte(byte[] geomBytes, int srid = DatabaseLiterals.KeywordEPSGDefault, int outSrid = DatabaseLiterals.KeywordEPSGMapsuiDefault)
        {
            //Init
            NTS.Geometries.MultiPolygon outPolygon = new NTS.Geometries.MultiPolygon(new Polygon[] { });

            //Build geopackage reader to get geometry as proper object
            PrecisionModel precisionModel = new PrecisionModel(PrecisionModels.FloatingSingle); //to get all decimals
            CoordinateSequenceFactory coordinateSequenceFactory = NtsGeometryServices.Instance.DefaultCoordinateSequenceFactory;
            GeoPackageGeoReader geoReader = new GeoPackageGeoReader(coordinateSequenceFactory, precisionModel);
            geoReader.HandleSRID = true;
            NTS.Geometries.Geometry geom = geoReader.Read(geomBytes);

            if (geom.OgcGeometryType == NTS.Geometries.OgcGeometryType.MultiPolygon)
            {
                outPolygon = geom as NTS.Geometries.MultiPolygon;
            }

            if (outPolygon != null && outPolygon.SRID != -1 && outPolygon.SRID != outSrid)
            {
                //Create a coord factory for incoming geometries
                CoordinateSystem incomingProjection = await SridReader.GetCSbyID(outPolygon.SRID);

                //Default map page coordinate
                CoordinateSystem outgoingProjection = await SridReader.GetCSbyID(outSrid);

                //Transform
                outPolygon = await TransformPolygonCoordinates(outPolygon, incomingProjection, outgoingProjection);
            }

            return outPolygon;
        }
    }
}