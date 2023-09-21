using System;
using Windows.UI.Xaml.Data;

namespace GSCFieldApp.Converters
{
    public class ReverseBoolConverter : IValueConverter
    {

        public ReverseBoolConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool && !(bool)value)
            {
                return true;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is bool && !(bool)value)
            {
                return false;
            }
            return true;
        }

    }
}
