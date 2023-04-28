using System;
using Windows.UI.Xaml.Data;
using System.Diagnostics;
using System.Globalization;

namespace GSCFieldApp.Converters
{
    public class String2Integer : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value == null ? null : value.ToString();

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            int? backDouble = null;

            if (value != null && value.ToString() != string.Empty)
            {
                backDouble = int.Parse(value.ToString());
            }

            return backDouble;
        }

    }
}
