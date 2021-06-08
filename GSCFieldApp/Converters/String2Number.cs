using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using System.Diagnostics;

namespace GSCFieldApp.Converters
{
    public class String2Number : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {

            //object dd = value;
            double i;
            if (string.IsNullOrEmpty(value.ToString()))
            {
                Debug.WriteLine("problem");
                i = 0D;
            }
            else
            {
                Debug.WriteLine("not a problem " + value.ToString());
                Debug.WriteLine(value.ToString());
                i = 0;
                Double.TryParse(value.ToString(), out i);

            }

            return i;

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}
