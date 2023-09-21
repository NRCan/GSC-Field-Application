using System;
using Windows.UI.Xaml.Data;

namespace GSCFieldApp.Converters
{
    public class TextWithDegreeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null || value.ToString() == string.Empty)
            {
                return value;
            }

            else
            {
                double inDegree = double.Parse(value.ToString());
                inDegree = Math.Round(inDegree, 8);
                return string.Format("{0}°", inDegree.ToString());
            }


        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
