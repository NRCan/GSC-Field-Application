using System;
using System.Globalization;

namespace GSCFieldApp.Converters
{
    public class BoldFontFromStringConverter : IValueConverter
    {

        public BoldFontFromStringConverter()
        {
        }


        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string && (string)value != string.Empty && (string)value == "Y")
            {
                return FontAttributes.Bold;
            }
            return FontAttributes.None;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FontAttributes && (FontAttributes)value == FontAttributes.Bold)
            {
                return "Y";
            }
            return "N";
        }
    }
}
