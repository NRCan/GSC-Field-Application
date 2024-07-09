using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSCFieldApp.Converters
{
    public class String2Integers: IValueConverter
    {
        public String2Integers() { }


        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? null : value.ToString();
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int parsedInteger = 0;

            if (value != null && value.ToString() != string.Empty)
            {
                int.TryParse(value.ToString(), out parsedInteger);
            }

            return parsedInteger;
        }
    }
}
