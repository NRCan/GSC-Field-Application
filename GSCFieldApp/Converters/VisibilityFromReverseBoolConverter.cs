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
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility && (Visibility)value == Visibility.Visible)
            {
                return false;
            }
            return true;
        }

    }
}
