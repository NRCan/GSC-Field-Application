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
    public class TileSchema4326: TileSchema
    {
        private const int TileSize = 256;
        private Point _origin = new Point(0, 0);
        private Point _lowerLeft = new Point(32.806470, -177.061845);
        private Point _upperRight = new Point(84.110195, -8.757565);

        public TileSchema4326()
        {
            //Recalculate the resolutions
            CalculateResolutions(20);

            Extent = new Extent(_lowerLeft.X, _lowerLeft.Y, _upperRight.X, _upperRight.Y);
            OriginX = _origin.X;
            OriginY = _origin.Y;
            Name = "EPSG:4326";
            Format = "image/png";
            YAxis = YAxis.TMS;
            Srs = "EPSG:4326";
        }

        public TileSchema4326(Tuple<Point, Point> extent)
        {
            //Force extent instead of default
            _lowerLeft = new Point(extent.Item1.Y, extent.Item2.X);
            _upperRight = new Point(extent.Item2.Y, extent.Item2.X);

            //Recalculate the resolutions
            CalculateResolutions(20);

            ///https://mapserver.org/ogc/wms_server.html#coordinate-systems-and-axis-orientation 
            Extent = new Extent(_lowerLeft.Y, _lowerLeft.X, _upperRight.Y, _upperRight.X);
            OriginX = _origin.X;
            OriginY = _origin.Y;
            Name = "EPSG:4326";
            Format = "image/png";
            YAxis = YAxis.OSM;
            Srs = "EPSG:4326";
        }

        /// <summary>
        /// Will transform current schema data into another SRID
        /// </summary>
        /// <param name="srid"></param>
        public async Task TransformTo(int srid)
        {
            //Transform extent to mapsui default
            _lowerLeft = await GeopackageService.TransformPointCoordinatesFromSrid(_lowerLeft, 4326, srid);
            _upperRight = await GeopackageService.TransformPointCoordinatesFromSrid(_upperRight, 4326, srid);
            Extent = new Extent(_lowerLeft.X, _lowerLeft.Y, _upperRight.X, _upperRight.Y);

            //Recalculate the resolutions
            CalculateResolutions(20);

            //Transform origin
            _origin = await GeopackageService.TransformPointCoordinatesFromSrid(_origin, 4326, srid);
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
