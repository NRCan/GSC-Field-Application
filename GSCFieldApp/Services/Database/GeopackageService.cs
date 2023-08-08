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

        public byte[] SaveGeometry()
        {
            NTS.NtsGeometryServices.Instance = new NTS.NtsGeometryServices(
                // default CoordinateSequenceFactory
                NTS.Geometries.Implementation.CoordinateArraySequenceFactory.Instance,
                // default precision model
                new NTS.Geometries.PrecisionModel(1000d),
                // default SRID
                4326,
                /********************************************************************
                 * Note: the following arguments are only valid for NTS >= v2.2
                 ********************************************************************/
                // Geometry overlay operation function set to use (Legacy or NG)
                NTS.Geometries.GeometryOverlay.NG,
                // Coordinate equality comparer to use (CoordinateEqualityComparer or PerOrdinateEqualityComparer)
                new NTS.Geometries.CoordinateEqualityComparer()
            );

            // Create a geometry factory with the spatial reference id 4326
            var gf = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(4326);

            // Create a point at Aurich (lat=53.4837, long=7.5404)
            NTS.Geometries.Point pntAUR = gf.CreatePoint(new NetTopologySuite.Geometries.Coordinate(7.5404, 53.4837));

            GeoPackageGeoWriter geo = new GeoPackageGeoWriter();
            byte[] bytePoint = geo.Write(pntAUR);

            return bytePoint;



            //GdalConfiguration.ConfigureOgr();
            //bool isUsable = GdalConfiguration.Usable;

            //Ogr.RegisterAll();
            //var num = Ogr.GetDriverCount();
            //for (var i = 0; i < num; i++)
            //{
            //    var d = Ogr.GetDriver(i);
            //    Console.WriteLine(string.Format("OGR {0}: {1}", i, d.name));
            //}


            //OSGeo.OGR.Driver driver = Ogr.GetDriverByName("GPKG");
            //driver.Register();

            //if (driver == null)
            //{
            //    Console.WriteLine("Cannot get drivers. Exiting");
            //    System.Environment.Exit(-1);
            //}

            //Console.WriteLine("Drivers fetched");

            //// Creating a shapefile
            //OSGeo.OGR.DataSource dataSourceSH = driver.Open(dAcccess.PreferedDatabasePath, 0);
            //if (dataSourceSH == null)
            //{
            //    Console.WriteLine("Cannot create datasource");
            //    System.Environment.Exit(-1);
            //}

            //// Creating a point layer
            //OSGeo.OGR.Layer layerSH;
            //layerSH = dataSourceSH.GetLayerByName("F_LOCATION");
            //if (layerSH == null)
            //{
            //    Console.WriteLine("Layer creation failed, exiting...");
            //    System.Environment.Exit(-1);
            //}

            //OSGeo.OGR.Feature featureSH = new OSGeo.OGR.Feature(layerSH.GetLayerDefn());
            //featureSH.SetField("LOCATIONIDNAME", "Test");

            //// Outer Ring 
            //// Methodology: Create a linear ring geometry, add it to a polygon geometry. Add polygon geometry to feature. Add feature to layer

            //OSGeo.OGR.Feature feature = new OSGeo.OGR.Feature(layerSH.GetLayerDefn());
            //OSGeo.OGR.Geometry geom = OSGeo.OGR.Geometry.CreateFromWkt(pntAUR.AsText());

            //feature.SetGeometry(geom);
            //int newFeat = layerSH.CreateFeature(feature);

            //dataSourceSH.Dispose();

        }

    }
}