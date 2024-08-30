using CommunityToolkit.Maui.Core.Extensions;
using System;
using System.Globalization;

namespace GSCFieldApp.Converters
{
    class DD2DMSConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return value;

            if (parameter == null)
                return value;

            double dd = (double)value;
            bool isNegative = dd < 0;
            double d = Math.Abs(dd);
            double m = (d - Math.Floor(d)) * 60;
            double s = (m - Math.Floor(m)) * 60;

            d = Math.Floor(d);
            m = Math.Floor(m);
            //s = Math.Floor(s);

            string output = null;
            switch (parameter.ToString())
            {
                case "Longitude":
                    if (d.IsZeroOrNaN())
                    {
                        output = "N.A.";
                    }
                    else
                    {
                        output = string.Format("{0}°{1:00}\'{2:00.00} {3}", d, m, s, isNegative ? 'W' : 'E');
                    }

                    
                    break;
                case "Latitude":
                    if (d.IsZeroOrNaN())
                    {
                        output = "N.A.";
                    }
                    else
                    {
                        output = string.Format("{0}°{1:00}\'{2:00.00} {3}", d, m, s, isNegative ? 'S' : 'N');
                    }
                        
                    break;
            }
            return output;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
