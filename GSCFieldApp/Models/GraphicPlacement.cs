using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Geometry;

namespace GSCFieldApp.Models
{
    /// <summary>
    /// Class to predetermine graphic placement over map view
    /// </summary>
    public class GraphicPlacement
    {
        public enum Placements { northEast, southWest, southEast, northWest, south, east, west, north }

        /// <summary>
        /// List of azim placement based on main direction and with priorities (west, south, east then north)
        /// </summary>
        public List<int> defaultAzimuthPlacement = new List<int>() { 90, 180, 270, 0 };

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
                _placementOffsets.north = new Tuple<double, double>(0, 10.0);
                _placementOffsets.south = new Tuple<double, double>(0, -10.0);
                _placementOffsets.east = new Tuple<double, double>(10.0, 0.0);
                _placementOffsets.west = new Tuple<double, double>(-10.0, 0.0);

                _placementOffsets.northEast = new Tuple<double, double>(10.0, 10.0);
                _placementOffsets.northWest = new Tuple<double, double>(-10.0, 10.0);
                _placementOffsets.southEast = new Tuple<double, double>(10.0, -10.0);
                _placementOffsets.southWest = new Tuple<double, double>(-10.0, -10.0);

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
        /// Will return a given placement offset based on priority of placement.
        /// WARNING: this does weird thing with the symbols. Using a new location seems to fix this.
        /// </summary>
        /// <param name="priority">Number from 1 to 8</param>
        /// <param name="imageHeight">If symbol anchor is center and picture marker, imageHeight will help with proper placement</param>
        /// <param name="imageWidth">If symbol anchor is center and picture marker, imageWidht will help with proper placement.</param>
        /// <returns></returns>
        public Tuple<double, double> GetOffsetFromPlacementPriority(int priority, double imageHeight = 0.0, double imageWidth = 0.0)
        {
            double returnX = 0.0;
            double returnY = 0.0;

            if (priority == 1)
            {
                returnX = PlacementOffsets.northEast.Item1 + (imageWidth / 6.0);
                returnY = PlacementOffsets.northEast.Item2 + (imageHeight / 6.0);
            }
            else if (priority == 2)
            {
                returnX = PlacementOffsets.southWest.Item1 - (imageWidth / 6.0);
                returnY = PlacementOffsets.southWest.Item2 - (imageHeight / 6.0);
            }
            else if (priority == 3)
            {
                returnX = PlacementOffsets.southEast.Item1 + (imageWidth / 6.0);
                returnY = PlacementOffsets.southEast.Item2 - (imageHeight / 6.0);
            }
            else if (priority == 4)
            {
                returnX = PlacementOffsets.northWest.Item1 - (imageWidth / 6.0);
                returnY = PlacementOffsets.northWest.Item2 + (imageHeight / 6.0);
            }
            else if (priority == 5)
            {
                returnX = PlacementOffsets.south.Item1;
                returnY = PlacementOffsets.south.Item2 - (imageHeight / 6.0);
            }
            else if (priority == 6)
            {
                returnX = PlacementOffsets.east.Item1 + (imageWidth / 6.0);
                returnY = PlacementOffsets.east.Item2;
            }
            else if (priority == 7)
            {
                returnX = PlacementOffsets.west.Item1 - (imageWidth / 6.0);
                returnY = PlacementOffsets.west.Item2;
            }
            else if (priority == 8)
            {
                returnX = PlacementOffsets.north.Item1;
                returnY = PlacementOffsets.north.Item2 + (imageHeight / 6.0);
            }
            else
            {
                returnX = PlacementOffsets.northEast.Item1 + (imageWidth / 6.0);
                returnY = PlacementOffsets.northEast.Item2 + (imageHeight / 6.0);
            }

            return new Tuple<double, double>(returnX, returnY);
        }

        /// <summary>
        /// Will return a new location position from a given priority for symbol placement
        /// </summary>
        /// <param name="priority">Number from 1 to 8</param>
        /// <param name="imageHeight">If symbol anchor is center and picture marker, imageHeight will help with proper placement</param>
        /// <param name="imageWidth">If symbol anchor is center and picture marker, imageWidht will help with proper placement.</param>
        /// <returns></returns>
        public Tuple<double, double> GetPositionOffsetFromPlacementPriority(int order, double centerX, double centerY, double distanceAway)
        {
            //Create a projected point
            MapPoint centerPoint = new MapPoint(centerX, centerY, 0.0, SpatialReferences.Wgs84);
            MapPoint projectedPoint = (MapPoint)Esri.ArcGISRuntime.Geometry.GeometryEngine.Project(centerPoint, SpatialReferences.WebMercator);

            //Get main direction in azimuth
            int azimOrientationPlace = CalculateOrientationFromOrientation(order);

            //Calculate new location from distance and a new orientation
            double returnX = projectedPoint.X + (distanceAway * Math.Cos(azimOrientationPlace * Math.PI / 180));
            double returnY = projectedPoint.Y + (distanceAway * Math.Sin(azimOrientationPlace * Math.PI / 180));

            //Reproject back to geographic
            centerPoint = (MapPoint)Esri.ArcGISRuntime.Geometry.GeometryEngine.Project(new MapPoint(returnX, returnY, 0.0, SpatialReferences.WebMercator), SpatialReferences.Wgs84);

            return new Tuple<double, double>(centerPoint.X, centerPoint.Y);
        }

        /// <summary>
        /// Will calculate the proper azimuth placement, from a default set of 4 places. Will shift the result each time there is a modulo of 4 items
        /// Orientation = Sum[Divide[delta,n],{n,1,}]
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public int CalculateOrientationFromOrientation(int order)
        {
            //Basically, we have 4 default placement (west, south, east and north)
            //For each needed placement above 4, the direction will be shifted from 90 degrees / module of 4 (90/2, 90/3, etc.)
            //The main 4 direction will then always be rotating and giving new location

            //Get main orientation azimuth
            int azim = GetOrientationPlacement(order); //One of those { 180, 270, 0, 90 }
            int delta = 90; //Starting degree shift
            List<int> azimList = defaultAzimuthPlacement.ToList<int>();

            //Calculate the n value ( n = 1 for the first 4 symbols,n = 2 for 5 to 8, n = 3 for 9 to 12, etc.
            int n = 1;
            if (order > 4)
            {
                n = (int)Math.Ceiling(order / 4.0);
            }

            //Shift the symbol
            for (int i = 1; i <= n; i++)
            {
                //New azim shift
                azim = azim + (delta / i);

            }

            //Calculate new value
            return azim;
        }

        /// <summary>
        /// Will return the proper placement azimuth based on an order.
        /// This will be given from a modulo of 4, since there is 4 main direction for placement
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public int GetOrientationPlacement(int order)
        {
            int modulo4 = order % 4;

            if (modulo4 > 0)
            {
                return defaultAzimuthPlacement[modulo4 - 1];
            }
            else
            {
                return defaultAzimuthPlacement[3];
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
