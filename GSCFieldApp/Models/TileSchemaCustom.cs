using BruTile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSCFieldApp.Services.DatabaseServices;
using NetTopologySuite.Geometries;
using Point = NetTopologySuite.Geometries.Point;

namespace GSCFieldApp.Models
{
    public class TileSchemaCustom: TileSchema
    {
        public const int TileSize = 256;
        private Point _origin = new Point(0, 0);
        public Point LowerLeft { get; set; }
        public Point UpperRight { get; set; }

        public int inSRID { get; set; } = 3857;

        public TileSchemaCustom(int srid)
        {
            inSRID = srid;

            //Recalculate the resolutions
            CalculateResolutions(20);

            Extent = new Extent(LowerLeft.X, LowerLeft.Y, UpperRight.X, UpperRight.Y);
            OriginX = _origin.X;
            OriginY = _origin.Y;
            Name = string.Format("EPSG:{0}", inSRID);
            Format = "image/png";
            YAxis = YAxis.TMS;
            Srs = string.Format("EPSG:{0}", inSRID);
        }

        public TileSchemaCustom(Tuple<Point, Point> extent, int srid)
        {
            if (extent != null && srid != null)
            {
                inSRID = srid;

                //Force extent instead of default
                LowerLeft = new Point(extent.Item1.Y, extent.Item1.X);
                UpperRight = new Point(extent.Item2.Y, extent.Item2.X);

                //Recalculate the resolutions
                CalculateResolutions(20);

                ///https://mapserver.org/ogc/wms_server.html#coordinate-systems-and-axis-orientation 
                Extent = new Extent(LowerLeft.Y, LowerLeft.X, UpperRight.Y, UpperRight.X);
                OriginX = _origin.X;
                OriginY = _origin.Y;
                Name = string.Format("EPSG:{0}", inSRID);
                Format = "image/png";
                YAxis = YAxis.OSM;
                Srs = string.Format("EPSG:{0}", inSRID);
            }

        }

        /// <summary>
        /// Will transform current schema data into another SRID
        /// </summary>
        /// <param name="srid"></param>
        public async Task TransformTo(int srid)
        {
            //Transform extent to mapsui default
            LowerLeft = await GeopackageService.TransformPointCoordinatesFromSrid(LowerLeft, inSRID, srid);
            UpperRight = await GeopackageService.TransformPointCoordinatesFromSrid(UpperRight, inSRID, srid);
            Extent = new Extent(LowerLeft.X, LowerLeft.Y, UpperRight.X, UpperRight.Y);

            //Recalculate the resolutions
            CalculateResolutions(20);

            //Transform origin
            _origin = await GeopackageService.TransformPointCoordinatesFromSrid(_origin, inSRID, srid);
            OriginX = _origin.X;
            OriginY = _origin.Y;
        }

        /// <summary>
        /// Will recalculate the resolutions
        /// </summary>
        /// <param name="arrayLength"></param>
        private void CalculateResolutions(int arrayLength)
        {
            double[] res = new double[arrayLength];

            double width = Math.Abs(UpperRight.X - LowerLeft.X);
            double height = Math.Abs(UpperRight.Y - LowerLeft.Y);

            _origin = LowerLeft;

            for (int i = 0; i < arrayLength; i++)
            {
                //Calc
                res[i] = Math.Max(width, height) / (TileSize * (Math.Pow(2, i)));

                //Set
                Resolutions[i] = new BruTile.Resolution
                (
                    i,
                    res[i],
                    TileSize,
                    TileSize
                );
            }
        }
    }
}
