using GSCFieldApp.Dictionaries;
using GSCFieldApp.Services;
using NetTopologySuite.Mathematics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSCFieldApp.Converters
{
    public class String2Time : IValueConverter
    {
        public String2Time() { }

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime parsedDate = DateTime.Now;

            string hours = parsedDate.ToString("HH");
            string minutes = parsedDate.ToString("mm");
            string seconds = parsedDate.ToString("ss");

            TimeSpan timeSpan = new TimeSpan(Convert.ToInt32(hours), Convert.ToInt32(minutes), Convert.ToInt32(seconds));

            if (value != null && value.ToString() != string.Empty)
            {
                try
                {
                    //Need to add a fake date in order to parse the time
                    DateTime.TryParse(String.Format("2000-01-01 {0}", value), out parsedDate);

                    timeSpan = new TimeSpan(parsedDate.Hour, parsedDate.Minute, parsedDate.Second);
                }
                catch (Exception e)
                {
                    new ErrorToLogFile(e).WriteToFile();
                }


            }

            return timeSpan;



        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && targetType == typeof(string))
            {
                try
                {
                    TimeSpan incomingTime = (TimeSpan)value;

                    return incomingTime.ToString();
                }
                catch (Exception e)
                {
                    
                    new ErrorToLogFile(e).WriteToFile();

                    return DateTime.Now.TimeOfDay.ToString(DatabaseLiterals.DateTimeStringFormat);
                }
      
            }
            else
            {
                return DateTime.Now.TimeOfDay.ToString(DatabaseLiterals.DateTimeStringFormat, CultureInfo.CurrentCulture);
            }
        }
    }
}
