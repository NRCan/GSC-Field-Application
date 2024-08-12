using GeoAPI.Geometries;
using System;
using System.Globalization;

namespace GSCFieldApp.Converters
{
    public class ColorFromBoolConverterFieldBooks : IValueConverter
    {

        public ColorFromBoolConverterFieldBooks()
        {
        }


        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool && (bool)value)
            {
                //Detect current theme
                AppTheme currentTheme = Application.Current.RequestedTheme;
                Color outColor = Application.Current.Resources["Primary"] as Color; //Default
                
                return outColor;
            }
            else
            {
                Color outColor = Application.Current.Resources["Tertiary"] as Color;

                return outColor;
            }
            
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Color && (Color)value == Color.FromHex("#710B2C"))
            {
                return true;
            }
            return false;
        }
    }
}
