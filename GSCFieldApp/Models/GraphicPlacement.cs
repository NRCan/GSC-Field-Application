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
        /// Will return a given Placement offset based on prioritis of placement
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
        /// Will return a new location from a given priority for symbol placement
        /// </summary>
        /// <param name="priority">Number from 1 to 8</param>
        /// <param name="imageHeight">If symbol anchor is center and picture marker, imageHeight will help with proper placement</param>
        /// <param name="imageWidth">If symbol anchor is center and picture marker, imageWidht will help with proper placement.</param>
        /// <returns></returns>
        public Tuple<double, double> GetPositionOffsetFromPlacementPriority(int priority, double centerX, double centerY, double distanceAway)
        {
            //Create a projected point
            MapPoint centerPoint = new MapPoint(centerX, centerY, 0.0, SpatialReferences.Wgs84);
            MapPoint projectedPoint = (MapPoint)Esri.ArcGISRuntime.Geometry.GeometryEngine.Project(centerPoint, SpatialReferences.WebMercator);

            //Get proper rotation
            int orientationDegree = 45; //default to first priority orientation
            if (priority == 1)
            {
                orientationDegree = 45;
            }
            else if(priority == 2)
            {
                orientationDegree = 225;
            }
            else if (priority == 3)
            {
                orientationDegree = 315;
            }
            else if (priority == 4)
            {
                orientationDegree = 135;
            }
            else if (priority == 5)
            {
                orientationDegree = 270;
            }
            else if (priority == 6)
            {
                orientationDegree = 0;
            }
            else if (priority == 7)
            {
                orientationDegree = 180;
            }
            else if (priority == 8)
            {
                orientationDegree = 90;
            }
            else
            {
                orientationDegree = 45;
            }

            double returnX = projectedPoint.X + (distanceAway * Math.Cos(orientationDegree * Math.PI / 180));
            double returnY = projectedPoint.Y + (distanceAway * Math.Sin(orientationDegree * Math.PI / 180));

            //Reproject back to geographic
            centerPoint = (MapPoint)Esri.ArcGISRuntime.Geometry.GeometryEngine.Project(new MapPoint(returnX, returnY, 0.0, SpatialReferences.WebMercator), SpatialReferences.Wgs84);

            return new Tuple<double, double>(centerPoint.X, centerPoint.Y);
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
