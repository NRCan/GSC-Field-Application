using System;
using System.Globalization;

namespace GSCFieldApp.Converters
{
    public class BoolFromStringConverter : IValueConverter
    {

        public BoolFromStringConverter()
        {
        }


        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //Based on the gsc field app data model of string bool value of Y and N
            if (value is string && (string)value != string.Empty && (string)value == "Y")
            {
                return true;
            }
            return false;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool && (bool)value == true)
            {
                return "Y";
            }
            return "N";
        }
    }
}
