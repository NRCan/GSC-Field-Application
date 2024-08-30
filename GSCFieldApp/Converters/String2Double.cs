using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSCFieldApp.Converters
{
    public class String2Double : IValueConverter
    {
        public String2Double() { }


        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? null : value.ToString();
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double parsedDouble = 0;

            if (value != null && value.ToString() != string.Empty)
            {
                double.TryParse(value.ToString(), out parsedDouble);
            }

            return parsedDouble;
        }
    }
}
