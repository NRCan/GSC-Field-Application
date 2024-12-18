using Mapsui.Styles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Color = Mapsui.Styles.Color;

namespace GSCFieldApp.Models
{
    /// <summary>
    /// A class to mimic what can be found within the SLD field that defines
    /// a feature styling inside a geopackage. Will be used to build those
    /// feature in the map.
    /// </summary>
    public class GeopackageLayerStyling
    {
        //Generic classified style
        public string className { get; set; }
        public string classValue { get; set; }

        //Related to lines
        public Color lineStrokeColor { get; set; }
        public double lineStrokeWidth { get; set; } 
        public VectorStyle lineVectorStyle { get; set; }

        //Related to polygons
        public Color polyFillColor { get; set; }
        public Color polyOutlineColor { get; set; }
        public double polyOutlineWidth { get; set; }
        public VectorStyle polyVectorStyle { get; set; }

        //Related to points/marker
        public Color pointFillColor { get; set; }
        public Color pointOutlineColor { get; set; }
        public double pointOutlineWidth { get; set; }
        public SymbolStyle pointVectorStyle { get; set; }

        public GeopackageLayerStyling() 
        { 
            className = string.Empty;
            classValue = string.Empty;
        }

        /// <summary>
        /// Will default to a grey line
        /// </summary>
        /// <returns></returns>
        public GeopackageLayerStyling SetDefaultLineStyle()
        {
            GeopackageLayerStyling lineStyle = new GeopackageLayerStyling();
            lineStrokeColor = Color.FromString("Grey");
            lineStrokeWidth = 3;
            lineVectorStyle = new VectorStyle { Line = new Pen(lineStrokeColor, lineStrokeWidth) };

            return lineStyle;
        }

        /// <summary>
        /// Will default to grey polygons
        /// </summary>
        /// <returns></returns>
        public GeopackageLayerStyling SetDefaultPolyStyle()
        {
            GeopackageLayerStyling polyStyle = new GeopackageLayerStyling();
            polyFillColor = Color.FromString("LightGrey");
            polyOutlineColor = Color.FromString("Grey");
            polyOutlineWidth = 1;

            polyVectorStyle = new VectorStyle { Fill = new Mapsui.Styles.Brush(polyFillColor), Outline = new Pen(polyOutlineColor, polyOutlineWidth) };

            return polyStyle;
        }

        /// <summary>
        /// Will default to grey points
        /// </summary>
        /// <returns></returns>
        public GeopackageLayerStyling SetDefaultPointStyle()
        {
            GeopackageLayerStyling pointStyle = new GeopackageLayerStyling();
            pointFillColor = Color.FromString("LightGrey");
            pointOutlineColor = Color.FromString("Grey");
            pointOutlineWidth = 0.1;

            pointVectorStyle = new SymbolStyle { BlendModeColor = pointFillColor, SymbolScale = 0.3, Fill = new Mapsui.Styles.Brush(pointFillColor), Outline = new Pen(pointOutlineColor, pointOutlineWidth) };

            return pointStyle;
        }

    }
}
