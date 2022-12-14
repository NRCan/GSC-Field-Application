using System;
using Windows.UI.Xaml.Data;

namespace GSCFieldApp.Converters
{
    public class TextWithPercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null && value.ToString() != string.Empty && Int16.TryParse(value.ToString(), out short numValue))
                return string.Format("{0}%", value);
            else
            {
                return value;
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
