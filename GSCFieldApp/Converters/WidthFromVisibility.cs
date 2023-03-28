using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI;

namespace GSCFieldApp.Converters
{
    public class WidthFromVisibility : IValueConverter
    {
        public WidthFromVisibility()
        {
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((Visibility)value == Visibility.Visible)
            {
                return Double.NaN;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is Double && (Double)value == Double.NaN)
            {
                return 0;
            }
            return 1;
        }
    }
}
