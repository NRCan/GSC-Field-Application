using System;
using Windows.UI.Xaml.Data;

namespace GSCFieldApp.Converters
{
    public class MandatoryFieldConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
            {
                return null;
            }
            else if (value.ToString() != "")
            {
                return value.ToString() + "*";
            }
            else
            {
                return value.ToString();
            }


        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
