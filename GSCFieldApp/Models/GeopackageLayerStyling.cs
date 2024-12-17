using Mapsui.Styles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Color = Mapsui.Styles.Color;

namespace GSCFieldApp.Models
{
    public class GeopackageLayerStyling
    {
        //Generic classified style
        public string className { get; set; }
        public string classValue { get; set; }

        //Related to lines
        public Color lineStrokeColor { get; set; }
        public double lineStrokeWidth { get; set; } 
        public VectorStyle lineVectorStyle { get; set; }


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
    }
}
