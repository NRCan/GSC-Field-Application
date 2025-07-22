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
using Mapsui.Nts.Extensions;
using CommunityToolkit.Maui.Core.Extensions;
using Mapsui.Styles;
using SQLite;
using Color = Mapsui.Styles.Color;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using CommunityToolkit.Mvvm.DependencyInjection;
using static System.Runtime.InteropServices.JavaScript.JSType;
using SQLitePCL;
using NetTopologySuite.Index;
using System.Diagnostics;

namespace GSCFieldApp.Services.DatabaseServices
{
    public class GeopackageService
    {

        public DataAccess da = new DataAccess();
        public static GeometryFactory defaultGeometryFactory = new GeometryFactory();
        public static GeometryFactory defaultMapsuiGeometryFactory = new GeometryFactory();
        public LocalizationResourceManager LocalizationResourceManager 
            => LocalizationResourceManager.Instance; // Will be used for in code dynamic local strings

        //Geopackage strings
        public const string GpkgTableGeometry = "gpkg_geometry_columns";
        public const string GpkgTableStyle = "layer_styles";

        public const string GpkgFieldTableName = "table_name";
        public const string GpkgFieldSRID = "srs_id";
        public const string GpkgFieldStyleTableName = "f_table_name";
        public const string GpkgFieldStyleSLD = "styleSLD";
        public const string GpkgFieldGeometry = "geom";
        public const string GpkgFieldGeometryType = "geometry_type_name";
        public const string GpkgFieldGeometryColumnName = "column_name";

        //Geopackage table names to delete for ArcGIS Pro compatibility
        public const string GpkgDeleteTableGeomColumn = "geometry_columns";
        public const string GpkgDeleteTableGeomColumnAuth = "geometry_columns_auth";
        public const string GpkgDeleteTableGeomColumnFieldInfo = "geometry_columns_field_infos";
        public const string GpkgDeleteTableGeomColumnStat = "geometry_columns_statistics";
        public const string GpkgDeleteTableGeomColumnTime = "geometry_columns_time";

        public const string GpkgDeleteTableViewGeomColumn = "views_geometry_columns";
        public const string GpkgDeleteTableViewGeomColumnAuth = "views_geometry_columns_auth";
        public const string GpkgDeleteTableViewGeomColumnFieldInfo = "views_geometry_columns_field_infos";
        public const string GpkgDeleteTableViewGeomColumnStat = "views_geometry_columns_statistics";

        public const string GpkgDeleteTableVirtGeomColumn = "virts_geometry_columns";
        public const string GpkgDeleteTableVirtGeomColumnAuth = "virts_geometry_columns_auth";
        public const string GpkgDeleteTableVirtGeomColumnFieldInfo = "virts_geometry_columns_field_infos";
        public const string GpkgDeleteTableVirtGeomColumnStat = "virts_geometry_columns_statistics";

        //public const string GpkgDeleteSpatialIndex = "SpatialIndex"; 
        public const string GpkgDeleteSpatialite= "spatialite_history";
        public const string GpkgDeleteStatements = "sql_statements_log";
        public const string GpkgDeleteDataLicenses = "data_licenses";
        //public const string GpkgDeleteElemGeom = "ElementaryGeometries";
        public const string GpkgDeleteSpatialRef = "spatial_ref_sys";
        public const string GpkgDeleteSpatialRefAux = "spatial_ref_sys_aux";

        //Geopackage style strings
        public const string GpkgStyleRoot = "NamedLayer"; //Basic root node
        public const string GpkgStyleStrokeRoot = "Stroke";
        public const string GpkgStyleStroke = "stroke";
        public const string GpkgStyleStrokeWidth = "stroke-width";
        public const string GpkgStyleAttrName = "name";
        public const string GpkgStyleRule = "Rule";
        public const string GpkgStyleClass = "PropertyName";
        public const string GpkgStyleValue = "Literal";
        public const string GpkgStyleFillRoot = "Fill";
        public const string GpkgStyleFill = "fill";

        private static ICoordinateTransformation _polyTransform = null;
        private static PrecisionModel _precisionModel = new PrecisionModel(PrecisionModels.Floating); //to get all decimals
        private static CoordinateSequenceFactory _coordinateSequenceFactory = NtsGeometryServices.Instance.DefaultCoordinateSequenceFactory;
        private static GeoPackageGeoReader _geoReader = new GeoPackageGeoReader(_coordinateSequenceFactory, _precisionModel);

        private static ParallelOptions _parallelOptions = new()
        {
            MaxDegreeOfParallelism = 10
        };

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
            
            NTS.Geometries.Point outPoint = new NTS.Geometries.Point(new Coordinate(0,0));

            //build geopackage reader to get geometry as proper object
            PrecisionModel precisionModel = new PrecisionModel(PrecisionModels.Floating); //to get all decimals
            CoordinateSequenceFactory coordinateSequenceFactory = NtsGeometryServices.Instance.DefaultCoordinateSequenceFactory;
            GeoPackageGeoReader geoReader = new GeoPackageGeoReader(coordinateSequenceFactory, precisionModel);
            geoReader.HandleSRID = true;

            try
            {
                NTS.Geometries.Geometry geom = geoReader.Read(geomBytes);

                if (geom.OgcGeometryType == NTS.Geometries.OgcGeometryType.Point)
                {
                    outPoint = geom as NTS.Geometries.Point;
                }
            }
            catch (Exception pointFromByteException)
            {
                new ErrorToLogFile(pointFromByteException).WriteToFile();
                outPoint = null;
                //await Shell.Current.DisplayAlert("Geometry Error", pointFromByteException.Message, "Ok");
            }

            if (outPoint != NTS.Geometries.Point.Empty)
            {
                if (outPoint != null && outPoint.SRID != -1 && outPoint.SRID != outSRID)
                {
                    //Create a coord factory for incoming traverses
                    CoordinateSystem incomingProjection = await SridReader.GetCSbyID(outPoint.SRID);

                    //Default map page coordinate
                    CoordinateSystem outgoingProjection = await SridReader.GetCSbyID(outSRID);

                    //Transform
                    outPoint = await TransformPointCoordinates(outPoint, incomingProjection, outgoingProjection);
                }
            }
            else
            {
                new ErrorToLogFile(LocalizationResourceManager["GeopackageServiceEmptyGeometry"].ToString() + " (" + (nameof(GetGeometryPointFromByteAsync)) + ").").WriteToFile();
                outPoint = null;
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
            PrecisionModel precisionModel = new PrecisionModel(PrecisionModels.Floating); //to get all decimals
            CoordinateSequenceFactory coordinateSequenceFactory = NtsGeometryServices.Instance.DefaultCoordinateSequenceFactory;
            GeoPackageGeoReader geoReader = new GeoPackageGeoReader(coordinateSequenceFactory, precisionModel);
            geoReader.HandleSRID = true;
            NTS.Geometries.Geometry geom = geoReader.Read(geomBytes);

            if (geom.OgcGeometryType == NTS.Geometries.OgcGeometryType.LineString)
            {
                outLine = geom as NTS.Geometries.LineString;
            }

            if (outLine != LineString.Empty)
            {
                if (outLine != null && outLine.SRID != -1 && outLine.SRID != outSrid)
                {
                    //Create a coord factory for incoming traverses
                    CoordinateSystem incomingProjection = await SridReader.GetCSbyID(outLine.SRID);

                    //Default map page coordinate
                    CoordinateSystem outgoingProjection = await SridReader.GetCSbyID(outSrid);

                    //Transform
                    outLine = await TransformLineCoordinates(outLine, incomingProjection, outgoingProjection);
                }
            }
            else
            {
                new ErrorToLogFile(LocalizationResourceManager["GeopackageServiceEmptyGeometry"].ToString() + " (" + (nameof(GetGeometryLineFromByte)) + ").").WriteToFile();
                outLine = null;
            }


            return outLine;
        }

        /// <summary>
        /// From a given byte array that defines a geometry, will return a line object
        /// WARNING: About EPSG3857 and it's Y-axis problem
        /// https://alastaira.wordpress.com/2011/01/23/the-google-maps-bing-maps-spherical-mercator-projection/
        /// </summary>
        /// <param name="geomBytes">The byte array to transform into a point object</param>
        /// <returns></returns>
        public async Task<NTS.Geometries.MultiLineString> GetGeometryMultiLineFromByte(byte[] geomBytes, int srid = DatabaseLiterals.KeywordEPSGDefault, int outSrid = DatabaseLiterals.KeywordEPSGMapsuiDefault)
        {
            //Init
            NTS.Geometries.MultiLineString outLine = new NTS.Geometries.MultiLineString(new LineString[] { });
            NTS.Geometries.LineString[] outLineArray = new LineString[] { };

            //Build geopackage reader to get geometry as proper object
            PrecisionModel precisionModel = new PrecisionModel(PrecisionModels.Floating); //to get all decimals
            CoordinateSequenceFactory coordinateSequenceFactory = NtsGeometryServices.Instance.DefaultCoordinateSequenceFactory;
            GeoPackageGeoReader geoReader = new GeoPackageGeoReader(coordinateSequenceFactory, precisionModel);
            geoReader.HandleSRID = true;
            NTS.Geometries.Geometry geom = geoReader.Read(geomBytes);

            if (geom.OgcGeometryType == NTS.Geometries.OgcGeometryType.MultiLineString)
            {
                outLine = geom as NTS.Geometries.MultiLineString;
            }

            if (outLine != MultiLineString.Empty)
            {
                if (outLine != null && outLine.SRID != -1 && outLine.SRID != outSrid)
                {
                    //Create a coord factory for incoming traverses
                    CoordinateSystem incomingProjection = await SridReader.GetCSbyID(outLine.SRID);

                    //Default map page coordinate
                    CoordinateSystem outgoingProjection = await SridReader.GetCSbyID(outSrid);

                    //Transform
                    int index = 0;
                    outLineArray = new LineString[outLine.Count];
                    foreach (LineString ls in outLine)
                    {
                        LineString newLine = await TransformLineCoordinates(ls, incomingProjection, outgoingProjection);
                        outLineArray[index] = newLine;
                        index++;
                    }

                    outLine = new NTS.Geometries.MultiLineString(outLineArray);
                }
            }
            else
            {
                new ErrorToLogFile(LocalizationResourceManager["GeopackageServiceEmptyGeometry"].ToString() + " (" + (nameof(GetGeometryMultiLineFromByte)) + ").").WriteToFile();
                outLine = null;
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
                outPoint = null; //nullify
            }


            return outPoint;
        }

        /// <summary>
        /// Will take a input point object and transform it from one coordinate system
        /// to another.
        /// </summary>
        /// <param name="inPointCoordinates"></param>
        /// <param name="inCoordSystem"></param>
        /// <param name="outCoordSystem"></param>
        /// <returns></returns>
        public static async Task<NTS.Geometries.Point> TransformPointCoordinatesFromSrid(NTS.Geometries.Point inPointCoordinates, int inSrid, int outSrid)
        {
            
            //Create a coord factory for incoming traverses
            CoordinateSystem incomingProjection = await SridReader.GetCSbyID(inSrid);

            //Default map page coordinate
            CoordinateSystem outgoingProjection = await SridReader.GetCSbyID(outSrid);

            //Transform
            return await TransformPointCoordinates(inPointCoordinates, incomingProjection, outgoingProjection);

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
                outLine = null; //nullify
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

                //Build transformation or use last one if it's the same
                if (_polyTransform == null || _polyTransform.TargetCS != outCoordSystem || _polyTransform.SourceCS != inCoordSystem)
                {
                    CoordinateTransformationFactory ctFact = new CoordinateTransformationFactory();
                    _polyTransform = ctFact.CreateFromCoordinateSystems(inCoordSystem, outCoordSystem);
                }

                //Shell
                foreach (Geometry g in inPolyCoordinates.Geometries)
                {

                    if (g.OgcGeometryType == NTS.Geometries.OgcGeometryType.Polygon)
                    {
                        Coordinate[] coordinates = new Coordinate[inPolyCoordinates.Coordinates.Count()];
                        int ringCoordinateIndex = 0;

                        //Transform vertices
                        foreach (Coordinate c in inPolyCoordinates.Coordinates)
                        {
                            double[] pointDouble = { c.X, c.Y};
                            double[] transformedPoint = _polyTransform.MathTransform.Transform(pointDouble);
                            
                            transformedPoint = _polyTransform.MathTransform.Transform(pointDouble);
                            

                            coordinates[ringCoordinateIndex] = new Coordinate(transformedPoint[0], transformedPoint[1]);
                            ringCoordinateIndex++;
                        }

                        //Add to linearRing
                        LinearRing linearRing = coordinates.ToLinearRing();

                        //Add ring to poly
                        transformedPolygons[coordinateIndex] = new Polygon(linearRing,defaultMapsuiGeometryFactory);
                        coordinateIndex++;
                    }

                }

                //Create polygon
                outPoly = defaultMapsuiGeometryFactory.CreateMultiPolygon(transformedPolygons);
            }
            catch (Exception e)
            {
                new ErrorToLogFile(e).WriteToFile();
                outPoly = null; //nullify
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
        /// Will add a new coordinate to a multipoint geometry
        /// </summary>
        /// <param name="pCoordinates"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static async Task<NTS.Geometries.MultiPoint> AddPointToMultiPoint(NTS.Geometries.MultiPoint pCoordinates, double x, double y)
        {
            //Init
            NTS.Geometries.Point[] points = new NTS.Geometries.Point[pCoordinates.Coordinates.Count() + 1];

            //Add
            for (int i = 0; i < pCoordinates.Coordinates.Count(); i++)
            {
                points[i] = new NTS.Geometries.Point(pCoordinates.Coordinates[i]);
            }
            points[pCoordinates.Coordinates.Count()] = new NTS.Geometries.Point(new Coordinate(x,y));

            NTS.Geometries.MultiPoint newPoints = new NTS.Geometries.MultiPoint(points);
            
            return newPoints;
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

            //Build geopackage reader to get geometry as it's own object
            _geoReader.HandleSRID = true;
            NTS.Geometries.Geometry geom = _geoReader.Read(geomBytes);

            //Cast just to make sure
            if (geom.OgcGeometryType == NTS.Geometries.OgcGeometryType.MultiPolygon)
            {
                outPolygon = geom as NTS.Geometries.MultiPolygon;
            }

            if (outPolygon != MultiPolygon.Empty)
            {
                //Transform geometry if needed
                if (outPolygon != null && outPolygon.SRID != -1 && outPolygon.SRID != outSrid)
                {
                    //Create a coord factory for incoming geometries
                    CoordinateSystem incomingProjection = ProjNet.CoordinateSystems.GeographicCoordinateSystem.WGS84;
                    if (outSrid != DatabaseLiterals.KeywordEPSGDefault)
                    {
                        //Manage SRID that are different from parameter and geometry
                        if (outPolygon.SRID != srid)
                        {
                            incomingProjection = await SridReader.GetCSbyID(srid);
                        }
                        else
                        {
                            incomingProjection = await SridReader.GetCSbyID(outPolygon.SRID);
                        }
                    }


                    //Default map page coordinate
                    CoordinateSystem outgoingProjection = ProjNet.CoordinateSystems.ProjectedCoordinateSystem.WebMercator;
                    if (outSrid != DatabaseLiterals.KeywordEPSGMapsuiDefault)
                    {
                        outgoingProjection = await SridReader.GetCSbyID(outSrid);
                    }


                    //Transform
                    outPolygon = await TransformPolygonCoordinates(outPolygon, incomingProjection, outgoingProjection);
                }

            }
            else
            {
                new ErrorToLogFile(LocalizationResourceManager["GeopackageServiceEmptyGeometry"].ToString() + " (" + (nameof(GetGeometryPolygonFromByte)) + ").").WriteToFile();
                outPolygon = null;
            }


            return outPolygon;
        }

        /// <summary>
        /// Will return a default or user style from the geopackage style table. It'll be set as the incoming geometry type.
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public async Task<List<GeopackageLayerStyling>> GetGeopackageStyle(string xmlString, string tableName, string geometry)
        {
            //Prep default
            List<GeopackageLayerStyling> stylingList = new List<GeopackageLayerStyling>();

            //Parse XML
            if (xmlString != null && xmlString != string.Empty)
            {
                //Get full document
                XDocument xdoc = XDocument.Parse(xmlString);

                //Get nodes within NamedLayer
                foreach (XElement namedLayerElement in xdoc.Descendants().Where(p => p.Name.LocalName == GpkgStyleRoot))
                {
                    //Make sure the styling is meant for the right layer
                    if (namedLayerElement.Value != null && namedLayerElement.Value.Contains(tableName))
                    {
                        //For classified styles, there'll be more than 1 rule
                        foreach (XElement ruleElement in namedLayerElement.Descendants().Where(p => p.Name.LocalName == GpkgStyleRule))
                        {
                            //Create a new styling layer to keep all properties nice and cozy somewhere
                            GeopackageLayerStyling ruleStyling = new GeopackageLayerStyling();
                            
                            //Keep class name and value if there is any
                            foreach (XElement classElement in ruleElement.Descendants().Where(p => p.Name.LocalName == GpkgStyleClass))
                            {
                                ruleStyling.className = classElement.Value;
                            }

                            foreach (XElement classElement in ruleElement.Descendants().Where(p => p.Name.LocalName == GpkgStyleValue))
                            {
                                ruleStyling.classValue = classElement.Value;
                            }

                            //Get style based on geometry type (point,lines or polygons)
                            if (geometry.ToLower() == Geometry.TypeNameMultiPolygon.ToLower())
                            {
                                ruleStyling = GetGeopackagePolyStyle(ruleElement, ruleStyling);
                            }
                            else if (geometry.ToLower() == Geometry.TypeNameLineString.ToLower())
                            {
                                ruleStyling = GetGeopackageLineStyle(ruleElement, ruleStyling);
                            }
                            else if (geometry.ToLower() == Geometry.TypeNamePoint.ToLower())
                            {
                                ruleStyling = GetGeopackagePointStyle(ruleElement, ruleStyling);
                            }

                            //Add new rule to list
                            stylingList.Add(ruleStyling);
                        }

                    }

                }

            }
            else
            {
                //Create a new styling layer to keep all properties nice and cozy somewhere
                GeopackageLayerStyling ruleStyling = new GeopackageLayerStyling();

                //Get style based on geometry type (point,lines or polygons)
                if (geometry.ToLower() == Geometry.TypeNameMultiPolygon.ToLower())
                {
                    ruleStyling.SetDefaultPolyStyle();
                }
                else if (geometry.ToLower() == Geometry.TypeNameLineString.ToLower() || geometry.ToLower() == Geometry.TypeNameMultiLineString.ToLower())
                {
                    ruleStyling.SetDefaultLineStyle();
                }
                else if (geometry.ToLower() == Geometry.TypeNamePoint.ToLower())
                {
                    ruleStyling.SetDefaultPointStyle();
                }

                //Add new rule to list
                stylingList.Add(ruleStyling);
            }

            return stylingList;
        }

        /// <summary>
        /// Will return a default or user style from the geopacakge style table
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public GeopackageLayerStyling GetGeopackageLineStyle(XElement ruleElement, GeopackageLayerStyling currentStyling)
        {
            //Default in case nothing is found
            currentStyling.SetDefaultLineStyle();

            //Get nodes inside the line stroke element
            foreach (XElement strokeElement in ruleElement.Descendants().Where(p => p.Name.LocalName == GpkgStyleStrokeRoot))
            {
                //Go through child nodes of stroke
                foreach (XElement linesStrokes in strokeElement.Descendants())
                {
                    if (linesStrokes.Attribute(GpkgStyleAttrName) != null && linesStrokes.Attribute(GpkgStyleAttrName).Value == GpkgStyleStroke)
                    {
                        try
                        {
                            currentStyling.lineVectorStyle.Line.Color = Color.FromString(linesStrokes.Value);
                        }
                        catch (Exception e)
                        {
                            new ErrorToLogFile(e).WriteToFile();
                        }

                    }
                    if (linesStrokes.Attribute(GpkgStyleAttrName) != null && linesStrokes.Attribute(GpkgStyleAttrName).Value == GpkgStyleStrokeWidth)
                    {

                        try
                        {
                            currentStyling.lineVectorStyle.Line.Width = double.Parse(linesStrokes.Value);
                        }
                        catch (Exception e)
                        {
                            new ErrorToLogFile(e).WriteToFile();
                        }

                    }

                }

            }

            return currentStyling;
        }

        /// <summary>
        /// Will return a default or user style from the geopacakge style table
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public GeopackageLayerStyling GetGeopackagePolyStyle(XElement ruleElement, GeopackageLayerStyling currentStyling)
        {
            //Default in case nothing is found
            currentStyling.SetDefaultPolyStyle();

            //Get nodes of outlines if there is any
            IEnumerable<XElement> strokeElements = ruleElement.Descendants().Where(p => p.Name.LocalName == GpkgStyleStrokeRoot);
            if (strokeElements.Count() > 0)
            {
                foreach (XElement strokeElement in strokeElements)
                {
                    //Go through child nodes of stroke
                    foreach (XElement linesStrokes in strokeElement.Descendants())
                    {
                        if (linesStrokes.Attribute(GpkgStyleAttrName) != null && linesStrokes.Attribute(GpkgStyleAttrName).Value == GpkgStyleStroke)
                        {
                            try
                            {
                                currentStyling.polyVectorStyle.Outline.Color = Color.FromString(linesStrokes.Value);
                            }
                            catch (Exception e)
                            {
                                new ErrorToLogFile(e).WriteToFile();
                            }

                        }
                        if (linesStrokes.Attribute(GpkgStyleAttrName) != null && linesStrokes.Attribute(GpkgStyleAttrName).Value == GpkgStyleStrokeWidth)
                        {

                            try
                            {
                                currentStyling.polyVectorStyle.Outline.Width = double.Parse(linesStrokes.Value);
                            }
                            catch (Exception e)
                            {
                                new ErrorToLogFile(e).WriteToFile();
                            }

                        }

                    }

                }
            }
            else 
            {
                currentStyling.polyOutlineColor = Color.Transparent;
                currentStyling.polyOutlineWidth = 0;
            }

            //Get the polygon fill color
            IEnumerable<XElement> fillElements = ruleElement.Descendants().Where(p => p.Name.LocalName == GpkgStyleFillRoot);
            if (fillElements.Count() > 0)
            {
                foreach (XElement fillElement in fillElements)
                {
                    //Go through child nodes of stroke
                    foreach (XElement fill in fillElement.Descendants())
                    {
                        if (fill.Attribute(GpkgStyleAttrName) != null && fill.Attribute(GpkgStyleAttrName).Value == GpkgStyleFill)
                        {
                            try
                            {
                                currentStyling.polyVectorStyle.Fill.Color = Color.FromString(fill.Value);
                            }
                            catch (Exception e)
                            {
                                new ErrorToLogFile(e).WriteToFile();
                            }

                        }

                    }

                }
            }
            else
            {
                currentStyling.polyFillColor = Color.Transparent;
            }

            return currentStyling;
        }

        /// <summary>
        /// Will return a default or user style from the geopacakge style table
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public GeopackageLayerStyling GetGeopackagePointStyle(XElement ruleElement, GeopackageLayerStyling currentStyling)
        {
            //Default in case nothing is found
            currentStyling.SetDefaultPointStyle();

            //Get nodes of outlines if there is any
            IEnumerable<XElement> strokeElements = ruleElement.Descendants().Where(p => p.Name.LocalName == GpkgStyleStrokeRoot);
            if (strokeElements.Count() > 0)
            {
                foreach (XElement strokeElement in strokeElements)
                {
                    //Go through child nodes of stroke
                    foreach (XElement linesStrokes in strokeElement.Descendants())
                    {
                        if (linesStrokes.Attribute(GpkgStyleAttrName) != null && linesStrokes.Attribute(GpkgStyleAttrName).Value == GpkgStyleStroke)
                        {
                            try
                            {
                                currentStyling.pointVectorStyle.Outline.Color = Color.FromString(linesStrokes.Value);
                            }
                            catch (Exception e)
                            {
                                new ErrorToLogFile(e).WriteToFile();
                            }

                        }
                        if (linesStrokes.Attribute(GpkgStyleAttrName) != null && linesStrokes.Attribute(GpkgStyleAttrName).Value == GpkgStyleStrokeWidth)
                        {

                            try
                            {
                                currentStyling.pointVectorStyle.Outline.Width = double.Parse(linesStrokes.Value);
                            }
                            catch (Exception e)
                            {
                                new ErrorToLogFile(e).WriteToFile();
                            }

                        }

                    }

                }
            }

            //Get the polygon fill color
            IEnumerable<XElement> fillElements = ruleElement.Descendants().Where(p => p.Name.LocalName == GpkgStyleFillRoot);
            if (fillElements.Count() > 0)
            {
                foreach (XElement fillElement in fillElements)
                {
                    //Go through child nodes of stroke
                    foreach (XElement fill in fillElement.Descendants())
                    {
                        if (fill.Attribute(GpkgStyleAttrName) != null && fill.Attribute(GpkgStyleAttrName).Value == GpkgStyleFill)
                        {
                            try
                            {
                                currentStyling.pointVectorStyle.Fill.Color = Color.FromString(fill.Value);
                            }
                            catch (Exception e)
                            {
                                new ErrorToLogFile(e).WriteToFile();
                            }

                        }

                    }

                }
            }


            return currentStyling;
        }

        /// <summary>
        /// Will output the geopackage style xml as a string
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetGeopackageStyleXMLString(SQLiteAsyncConnection gpkgConnection, string tableName)
        {
            string xmlString = string.Empty;

            //Check for layer style table existance
            string getLayerStyleTable = string.Format("SELECT 1 FROM sqlite_master WHERE type='table' and name = '{0}'", GpkgTableStyle);

            List<string> layerStyleTable = await gpkgConnection.QueryScalarsAsync<string>(getLayerStyleTable);

            if (layerStyleTable != null && layerStyleTable.Count() > 0)
            {
                //Read from geopackage style table
                string getStyleXML = string.Format("SELECT {0} FROM {1} WHERE {2} = '{3}';", GpkgFieldStyleSLD, GpkgTableStyle, GpkgFieldStyleTableName, tableName);
                try
                {
                    List<string> xmlStyle = await gpkgConnection.QueryScalarsAsync<string>(getStyleXML);

                    if (xmlStyle != null && xmlStyle.Count() > 0)
                    {
                        xmlString = xmlStyle[0];
                    }
                }
                catch (Exception e)
                {
                    new ErrorToLogFile(e).WriteToFile();
                }
            }

            return xmlString;
        }

        /// <summary>
        /// Will run a generic query on a database and will output an object array of arrays
        /// Basically it'll output all field and records value, as opposed to sqlite.netpcl that
        /// works with a mapping type.
        /// </summary>
        /// <param name="databasePath"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static object?[][]? RunGenericQuery(string databasePath, string query)
        {
            //Init
            object?[][]? data = null;
            SQLiteConnection dbConnection = new SQLiteConnection(databasePath);
            sqlite3_stmt sqlStatement = SQLite3.Prepare2(dbConnection.Handle, query);

            //Get number of columns
            int columnNumber = SQLite3.ColumnCount(sqlStatement);

            //Select all rows and when done, execute the query
            try
            {
                return SelectRows().ToArray();
            }
            catch (Exception e)
            {
                return null;
            }
            finally
            {
                if (sqlStatement != null)
                {
                    SQLite3.Finalize(sqlStatement);
                }
            }

            //Function to retrieve all rows and keep them in an enumerable object
            IEnumerable<object?[]> SelectRows()
            {
                //Set column names as first row
                yield return SelectColumnNames(sqlStatement, columnNumber).ToArray();
                

                //Get columns into an array
                while (SQLite3.Step(sqlStatement) == SQLite3.Result.Row)
                {
                    yield return SelectColumns(sqlStatement, columnNumber).ToArray();
                }

                //Get column names
                static IEnumerable<object> SelectColumnNames(SQLitePCL.sqlite3_stmt stQuery, int colLength)
                {
                    for (int i = 0; i < colLength; i++)
                    {
                        yield return SQLite3.ColumnName(stQuery, i);
                    }
                }

                //Get column definition type, cast their value and add to array
                static IEnumerable<object?> SelectColumns(SQLitePCL.sqlite3_stmt stQuery, int colLength)
                {
                    for (int i = 0; i < colLength; i++)
                    {
                        utf8z columnDeclarationType = SQLitePCL.raw.sqlite3_column_decltype(stQuery, i);     
                        string columnType = columnDeclarationType.utf8_to_string().ToLower();
                        yield return columnType switch
                        {
                            
                            "text" => SQLite3.ColumnString(stQuery, i),
                            "integer" => SQLite3.ColumnInt(stQuery, i),
                            "bigint" => SQLite3.ColumnInt64(stQuery, i),
                            "mediumint" => SQLite3.ColumnInt64(stQuery, i),
                            "real" => SQLite3.ColumnDouble(stQuery, i),
                            "blob" => SQLite3.ColumnBlob(stQuery, i),
                            "null" => null,
                            "point" => SQLite3.ColumnBlob(stQuery, i),
                            "linestring" => SQLite3.ColumnBlob(stQuery, i),
                            "multipolygon" => SQLite3.ColumnBlob(stQuery, i),
                            "polygon" => SQLite3.ColumnBlob(stQuery, i),
                            "multipolygon z" => SQLite3.ColumnBlob(stQuery, i),
                            _ => SQLite3.ColumnString(stQuery, i),

                        };
                    }
                }
            }

        }

        /// <summary>
        /// Need to run some clean-up process (table deletion) so the output
        /// geopackage works correctly in ArcGIS Pro.
        /// </summary>
        /// <returns></returns>
        public static async Task<int> MakeGeopackageArcGISCompatible(string connectionPath)
        {
            SQLiteAsyncConnection inConnection = new SQLiteAsyncConnection(connectionPath);

            List<string> tablesToDelete = new List<string>() { GpkgDeleteDataLicenses, GpkgDeleteSpatialite,
                GpkgDeleteSpatialRef, GpkgDeleteSpatialRefAux,
                GpkgDeleteStatements, GpkgDeleteTableGeomColumn, GpkgDeleteTableGeomColumnAuth, 
                GpkgDeleteTableGeomColumnFieldInfo, GpkgDeleteTableGeomColumnStat, GpkgDeleteTableGeomColumnTime,
                GpkgDeleteTableViewGeomColumn, GpkgDeleteTableViewGeomColumnAuth, GpkgDeleteTableViewGeomColumnFieldInfo,
                GpkgDeleteTableViewGeomColumnStat, GpkgDeleteTableVirtGeomColumn, GpkgDeleteTableVirtGeomColumnAuth, 
                GpkgDeleteTableVirtGeomColumnFieldInfo, GpkgDeleteTableVirtGeomColumnStat};

            List<int> deletedTables = new List<int>();

            await Parallel.ForEachAsync(tablesToDelete, _parallelOptions, async (table, token) =>
            {
                deletedTables.Add(await inConnection.ExecuteAsync(string.Format("DROP TABLE IF EXISTS {0};", table)));
            });

            await inConnection.CloseAsync();

            return deletedTables.Where(x=>x==1).Count();

        }

        /// <summary>
        /// Will repair the geometry of feature and enforce the right bytes in the geometry field coming from textual coordinates
        /// </summary>
        /// <returns></returns>
        public async Task<bool> RepairLocationGeometry()
        {
            bool repaired = true;

            //Get all features
            List<FieldLocation> fieldLocations = await DataAccess.DbConnection.Table<FieldLocation>().ToListAsync();

            //Go through all features and get byte for long/lat
            if (fieldLocations != null)
            {
                foreach (FieldLocation locs in fieldLocations)
                {
                    if (!locs.LocationLong.IsZeroOrNaN() && !locs.LocationLat.IsZeroOrNaN())
                    {
                        try
                        {
                            byte[] byteGeom = CreateByteGeometryPoint(locs.LocationLong, locs.LocationLat);
                            locs.LocationGeometry = byteGeom;

                            await da.SaveItemAsync(locs, true);
                        }
                        catch (Exception e)
                        {
                            new ErrorToLogFile(e).WriteToFile();
                            repaired = false;
                        }

                    }
                    
                }
            }

            return repaired;
        }
    }
}