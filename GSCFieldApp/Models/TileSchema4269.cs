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
    /// <summary>
    /// NAD 83 geographic epsg
    /// </summary>
    public class TileSchema4269: TileSchema
    {
        private const int TileSize = 256;
        private Point _origin = new Point(0, 0);
        private Point _lowerLeft = new Point(-172.54, 23.81);
        private Point _upperRight = new Point(-47.74, 86.46);

        public TileSchema4269()
        {
            //Recalculate the resolutions
            CalculateResolutions(20);

            Extent = new Extent(_lowerLeft.X, _lowerLeft.Y, _upperRight.X, _upperRight.Y);
            OriginX = _origin.X;
            OriginY = _origin.Y;
            Name = "EPSG:4269";
            Format = "image/png";
            YAxis = YAxis.TMS;
            Srs = "EPSG:4269";
        }

        public TileSchema4269(Tuple<Point, Point> extent)
        {
            //Force extent instead of default
            _lowerLeft = new Point(extent.Item1.X, extent.Item1.Y);
            _upperRight = new Point(extent.Item2.X, extent.Item2.Y);

            //Recalculate the resolutions
            CalculateResolutions(20);

            ///https://mapserver.org/ogc/wms_server.html#coordinate-systems-and-axis-orientation 
            Extent = new Extent(_lowerLeft.X, _lowerLeft.Y, _upperRight.X, _upperRight.Y);
            OriginX = _origin.X;
            OriginY = _origin.Y;
            Name = "EPSG:4269";
            Format = "image/png";
            YAxis = YAxis.OSM;
            Srs = "EPSG:4269";
        }

        /// <summary>
        /// Will transform current schema data into another SRID
        /// </summary>
        /// <param name="srid"></param>
        public async Task TransformTo(int srid)
        {
            //Transform extent to mapsui default
            _lowerLeft = await GeopackageService.TransformPointCoordinatesFromSrid(_lowerLeft, 4269, srid);
            _upperRight = await GeopackageService.TransformPointCoordinatesFromSrid(_upperRight, 4269, srid);
            Extent = new Extent(_lowerLeft.X, _lowerLeft.Y, _upperRight.X, _upperRight.Y);

            //Recalculate the resolutions
            CalculateResolutions(20);

            //Transform origin
            _origin = await GeopackageService.TransformPointCoordinatesFromSrid(_origin, 4269, srid);
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

            double width = Math.Abs(_upperRight.X - _lowerLeft.X);
            double height = Math.Abs(_upperRight.Y - _lowerLeft.Y);

            _origin = _lowerLeft;

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
