using BruTile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GSCFieldApp.Models
{
    internal class TileSchema3978: TileSchema
    {
        private const int TileSize = 256;
        private readonly double _originX = -938105.72;
        private readonly double _originY = 787721.55;

        public TileSchema3978()
        {
            double[] resolution = new[] {
                48343.8017578125,
                24171.90087890625,
                12085.950439453125,
                6042.9752197265625,
                3021.4876098632812,
                1510.7438049316406,
                755.3719024658203,
                377.68595123291016,
                188.84297561645508,
                94.42148780822754,
                47.21074390411377,
                23.605371952056885,
                11.802685976028442,
                5.901342988014221,
                2.9506714940071106,
                1.4753357470035553,
                0.7376678735017776,
                0.3688339367508888,
                0.1844169683754444,
                0.0922084841877222,
            };

            for (int i = 0; i < resolution.Length; i++)
            {
                Resolutions[i] = new Resolution
                (
                    i,
                    resolution[i],
                    TileSize,
                    TileSize,
                    _originX,
                    _originY
                );
            }

            Extent = new Extent(-7192737.96, -3004297.73, 5183275.29, 4484204.83);
            OriginX = _originX;
            OriginY = _originY;
            Name = "NAD83 Canada Atlas Lambert";
            Format = "image/png";
            Srs = "EPSG:3978";
            
        }
    }
}
