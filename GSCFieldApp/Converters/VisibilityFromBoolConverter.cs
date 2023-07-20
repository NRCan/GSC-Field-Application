using System;
using System.Globalization;

namespace GSCFieldApp.Converters
{
    public class VisibilityFromBoolConverter : IValueConverter
    {

        public VisibilityFromBoolConverter()
        {
        }


        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool && (bool)value)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility && (Visibility)value == Visibility.Visible)
            {
                return true;
            }
            return false;
        }
    }
}
