using BruTile;
using ExCSS;
using GSCFieldApp.Services.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Point = NetTopologySuite.Geometries.Point;

namespace GSCFieldApp.Models
{
    internal class TileSchema3978: TileSchema
    {
        private const int TileSize = 256;
        private Point _origin = new Point(-938105.72, 787721.55);
        private Point _lowerLeft = new Point(-2708707.567300, -807190.452400);
        private Point _upperRight = new Point(3576874.186000, 3873396.596400);

        public TileSchema3978()
        {
            CalculateResolutions(20);

            Extent = new Extent(_lowerLeft.X, _lowerLeft.Y, _upperRight.X, _upperRight.Y);
            //OriginX = _origin.X;
            //OriginY = _origin.Y;
            Name = "NAD83 Canada Atlas Lambert";
            Format = "image/png";
            Srs = "EPSG:3978";
            YAxis = YAxis.OSM;
            
            
        }

        public TileSchema3978(Tuple<Point, Point> extent)
        {
            //Force extent instead of default
            _lowerLeft = extent.Item1;
            _upperRight = extent.Item2;

            CalculateResolutions(20);

            Extent = new Extent(_lowerLeft.X, _lowerLeft.Y, _upperRight.X, _upperRight.Y);
            //OriginX = _origin.X;
            //OriginY = _origin.Y;
            Name = "NAD83 Canada Atlas Lambert";
            Format = "image/png";
            Srs = "EPSG:3978";
            YAxis = YAxis.OSM;


        }

        /// <summary>
        /// Will transform current schema data into another SRID
        /// </summary>
        /// <param name="srid"></param>
        public async Task TransformTo(int srid)
        {
            //Transform extent to mapsui default
            _lowerLeft = await GeopackageService.TransformPointCoordinatesFromSrid(_lowerLeft, 3978, srid);
            _upperRight = await GeopackageService.TransformPointCoordinatesFromSrid(_upperRight, 3978, srid);

            Extent = new Extent(_lowerLeft.X, _lowerLeft.Y, _upperRight.X, _upperRight.Y);

            //Recalculate the resolutions
            CalculateResolutions(20);

            ////Transform origin
            //_origin = await GeopackageService.TransformPointCoordinatesFromSrid(_origin, 3978, srid);
            //OriginX = _origin.X;
            //OriginY = _origin.Y;
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

            for(int i = 0; i < arrayLength; i++)
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
