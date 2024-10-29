using GSCFieldApp.Dictionaries;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSCFieldApp.Converters
{
    public class VisibilityFromILayer: IValueConverter
    {
        public VisibilityFromILayer()
        {
        }

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value.ToString() != ApplicationLiterals.aliasStations && 
                value.ToString() != ApplicationLiterals.aliasOSM && value.ToString() != ApplicationLiterals.aliasLinework)
            {
                return true;
            }
            else
            {
                return false;
            }
            
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return "";
        }
    }
}
