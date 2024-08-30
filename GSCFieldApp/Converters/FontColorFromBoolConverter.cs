using System;
using System.Globalization;

namespace GSCFieldApp.Converters
{
    public class FontColorFromBoolConverter : IValueConverter
    {

        public FontColorFromBoolConverter()
        {
        }


        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string && (string)value != string.Empty && (string)value == "Y")
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
                Color outColor = Application.Current.Resources["Gray100"] as Color;

                return outColor;
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Color && ((Color)value == Application.Current.Resources["Black"] || (Color)value == Application.Current.Resources["White"]))
            {
                return "Y";
            }
            return "N";
        }
    }
}
