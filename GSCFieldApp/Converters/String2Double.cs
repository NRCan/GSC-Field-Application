using System;
using Windows.UI.Xaml.Data;

namespace GSCFieldApp.Converters
{
    public class String2Double : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value == null ? null : value.ToString();

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            double? backDouble = null;

            if (value != null && value.ToString() != string.Empty)
            {
                backDouble = double.Parse(value.ToString());
            }

            return backDouble;
        }

    }
}
