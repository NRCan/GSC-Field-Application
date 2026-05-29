using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace GSCFieldApp.Converters
{
    public class UrlDetectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Detect if value is a URL and return color or underline
            var str = value as string;
            bool isUrl = !string.IsNullOrEmpty(str) && (str.StartsWith("http://") || str.StartsWith("https://") || str.StartsWith("www."));

            if (parameter?.ToString() == "Color")
            {
                // Detect theme and return appropriate color
                AppTheme currentTheme = Application.Current.RequestedTheme;

                if (currentTheme == AppTheme.Dark)
                {
                    return isUrl ? Colors.Blue : Colors.White;
                }
                else if (currentTheme == AppTheme.Light)
                {
                    return isUrl ? Colors.Blue : Colors.Black;
                }
            }
            else if (parameter?.ToString() == "Underline")
            {
                return isUrl ? TextDecorations.Underline : TextDecorations.None;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}