using System;
using Windows.UI.Xaml.Data;

namespace GSCFieldApp.Converters
{
    public class SecondOrderPicklistConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return null;

            if (parameter == null)
                return value;

            return (Themes.ComboBoxItem)value;

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return null;

            if (parameter == null)
                return value;

            return (Themes.ComboBoxItem)value;
        }

    }

}
