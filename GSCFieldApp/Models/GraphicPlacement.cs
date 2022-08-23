using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSCFieldApp.Models
{
    /// <summary>
    /// Class to predetermine graphic placement over map view
    /// </summary>
    public class GraphicPlacement
    {
        public enum Placements { northEast, southWest, southEast, northWest, south, east, west, north }

        /// <summary>
        /// Placement offsets as tuples(offset x, offset y)
        /// </summary>
        public struct PlacementsStruct
        {
            public Tuple<double, double> north;
            public Tuple<double, double> northEast;
            public Tuple<double, double> northWest;
            public Tuple<double, double> south;
            public Tuple<double, double> southEast;
            public Tuple<double, double> southWest;
            public Tuple<double, double> east;
            public Tuple<double, double> west;

        }

        private PlacementsStruct _placementOffsets;

        /// <summary>
        /// Some default offsets to start with
        /// </summary>
        public PlacementsStruct PlacementOffsets
        { 
            get
            {
                _placementOffsets.north = new Tuple<double, double>(0, 14.0);
                _placementOffsets.south = new Tuple<double, double>(0, -14.0);
                _placementOffsets.east = new Tuple<double, double>(14.0, 0.0);
                _placementOffsets.west = new Tuple<double, double>(-14.0, 0.0);

                _placementOffsets.northEast = new Tuple<double, double>(7.0, 7.0);
                _placementOffsets.northWest = new Tuple<double, double>(-7.0, 7.0);
                _placementOffsets.southEast = new Tuple<double, double>(7.0, -7.0);
                _placementOffsets.southWest = new Tuple<double, double>(-7.0, -7.0);

                return _placementOffsets;
            }
            set
            {
                _placementOffsets = value;
            }
        }

        public GraphicPlacement()
        {
            _placementOffsets = new PlacementsStruct();
        }

        /// <summary>
        /// Will return a given Placement offset based on prioritis of placement
        /// </summary>
        /// <param name="priority">Number from 1 to 8</param>
        /// <returns></returns>
        public Tuple<double, double> GetOffsetFromPlacementPriority(int priority)
        {
            
            if (priority == 1)
            {
                return PlacementOffsets.northEast;
            }
            else if (priority == 2)
            {
                return PlacementOffsets.southWest;
            }
            else if (priority == 3)
            {
                return PlacementOffsets.southEast;
            }
            else if (priority == 4)
            {
                return PlacementOffsets.northWest;
            }
            else if (priority == 5)
            {
                return PlacementOffsets.south;
            }
            else if (priority == 6)
            {
                return PlacementOffsets.east;
            }
            else if (priority == 7)
            {
                return PlacementOffsets.west;
            }
            else if (priority == 8)
            {
                return PlacementOffsets.north;
            }
            else
            {
                return PlacementOffsets.northEast;
            }

        }

        /// <summary>
        /// From a given placement enumeration value, will return it's associated priority
        /// </summary>
        /// <param name="pl">Placements enums</param>
        /// <returns></returns>
        public int GetPlacementPriority(Placements pl)
        {
            switch (pl)
            {
                case Placements.northEast:
                    return 1;
                case Placements.southWest:
                    return 2;
                case Placements.southEast:
                    return 3;
                case Placements.northWest:
                    return 4;
                case Placements.south:
                    return 5;
                case Placements.east:
                    return 6;
                case Placements.west:
                    return 7;
                case Placements.north:
                    return 8;
                default:
                    return 1;
            }
        }
    }
}
