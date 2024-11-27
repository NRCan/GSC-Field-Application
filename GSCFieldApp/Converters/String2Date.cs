using GSCFieldApp.Dictionaries;
using NetTopologySuite.Mathematics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSCFieldApp.Converters
{
    public class String2Date : IValueConverter
    {
        public String2Date() { }

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime parsedDate = DateTime.Today;

            if (value != null && value.ToString() != string.Empty)
            {
                DateTime.TryParse(value.ToString(), out parsedDate);
            }

            return parsedDate;



        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && targetType == typeof(string))
            {
                DateTime incomingDateTime = (DateTime)value;
                return incomingDateTime.ToString(DatabaseLiterals.DateStringFormat);
            }
            else
            {
                return DateTime.Today.ToString(DatabaseLiterals.DateStringFormat);
            }
        }
    }
}
