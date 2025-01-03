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
        private readonly Point _origin = new Point(0, 0);
        private readonly Point _lowerLeft = new Point(32.806470, -177.061845);
        private readonly Point _upperRight = new Point(84.110195, -8.757565);

        public TileSchema4326()
        {
            double[] resolution = new[] {
                0.7258570703125,
                0.36292853515625,
                0.181464267578125,
                0.0907321337890625,
                0.04536606689453125,
                0.022683033447265626,
                0.011341516723632813,
                0.005670758361816406,
                0.002835379180908203,
                0.0014176895904541016,
                0.0007088447952270508,
                0.0003544223976135254,
                0.0001772111988067627,
                8.860559940338135e-05,
                4.4302799701690675e-05,
                2.2151399850845337e-05,
                1.1075699925422669e-05,
                5.537849962711334e-06,
                2.768924981355667e-06,
                1.3844624906778336e-06,
            };

            for (int i = 0; i < resolution.Length; i++)
            {
                Resolutions[i] = new Resolution
                (
                    i,
                    resolution[i],
                    TileSize,
                    TileSize,
                    _origin.X,
                    _origin.Y
                );
            }

            Extent = new Extent(_lowerLeft.X, _lowerLeft.Y, _upperRight.X, _upperRight.Y);
            OriginX = _origin.X;
            OriginY = _origin.Y;
            Name = "EPSG:4326";
            Format = "image/png";
            YAxis = YAxis.TMS;
            Srs = "EPSG:4326";
        }

        /// <summary>
        /// Will transform current schema data into another SRID
        /// </summary>
        /// <param name="srid"></param>
        public async Task TransformTo(int srid)
        {
            //Transform extent to mapsui default
            Point lowerLeft = await GeopackageService.TransformPointCoordinatesFromSrid(_lowerLeft, 4326, srid);
            Point upperRight = await GeopackageService.TransformPointCoordinatesFromSrid(_upperRight, 4326, srid);
            Extent = new Extent(_lowerLeft.X, _lowerLeft.Y, _upperRight.X, _upperRight.Y);

            //Transform origin
            Point origin = await GeopackageService.TransformPointCoordinatesFromSrid(_origin, 4326, srid);
            OriginX = origin.X;
            OriginY = origin.Y;
        }
    }
}
