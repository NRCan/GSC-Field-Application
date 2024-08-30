using System;
using System.Globalization;

namespace GSCFieldApp.Converters
{
    public class VisibilityFromReverseBoolConverter: IValueConverter
    {

        public VisibilityFromReverseBoolConverter()
        {
        }

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool && !(bool)value)
            {
                return true;
            }
            return false;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool && (bool)value)
            {
                return false;
            }
            return true;
        }

    }
}
