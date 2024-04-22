using System;
using Windows.UI.Xaml.Data;

namespace GSCFieldApp.Converters
{
    public class TextWithAzimDegreeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null || value.ToString() == string.Empty)
                return value;

            if (System.Convert.ToInt32(value) < 100)
            {
                return string.Format("0{0}°", value);
            }
            else
            {
                return string.Format("{0}°", value);
            }


        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
