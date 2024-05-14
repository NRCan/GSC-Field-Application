using GeoAPI.Geometries;
using System;
using System.Globalization;

namespace GSCFieldApp.Converters
{
    public class ColorFromBoolConverter : IValueConverter
    {

        public ColorFromBoolConverter()
        {
        }


        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool && (bool)value)
            {
                Color outColor = Application.Current.Resources["Black"] as Color;

                return outColor;
            }
            else
            {
                Color outColor = Application.Current.Resources["ErrorColor"] as Color;

                return outColor;
            }
            
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Color && (Color)value == Color.FromRgb(0,0,0))
            {
                return true;
            }
            return false;
        }
    }
}
