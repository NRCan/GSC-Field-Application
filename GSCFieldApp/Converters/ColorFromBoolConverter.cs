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
                //Detect current theme
                AppTheme currentTheme = Application.Current.RequestedTheme;
                Color outColor = Application.Current.Resources["Black"] as Color; //Default

                if (currentTheme == AppTheme.Dark)
                {
                    outColor = Application.Current.Resources["White"] as Color;
                }
                
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
