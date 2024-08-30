using CommunityToolkit.Maui.Core.Extensions;
using NetTopologySuite.Mathematics;
using System;
using System.Globalization;

namespace GSCFieldApp.Converters
{
    public class ElevationConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return value;

            double dd = (double)value;
            if (dd.IsZeroOrNaN())
            {
                return string.Format("N.A. m", value);
            }
            else
            {
                return string.Format("{0:0} m", value);//Keep only one digits, more is useless.
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        { 
            if (value is string)
            {
                return value.ToString().Replace('m', string.Empty.ToCharArray()[0]).Trim();
            }
            return false;

        }
    }
}
