using System.Globalization;
using Microsoft.Maui.Controls;

namespace GSCFieldApp.Services
{
    public class UrlDetectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text && !string.IsNullOrEmpty(text))
            {
                // Check if the text is a valid URL
                string urlToValidate = text.Trim();
                if (!urlToValidate.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && 
                    !urlToValidate.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    urlToValidate = "https://" + urlToValidate;
                }

                try
                {
                    _ = new Uri(urlToValidate, UriKind.Absolute);
                    if (parameter?.ToString() == "Color")
                        return Color.FromArgb("#0066CC");
                    else if (parameter?.ToString() == "Underline")
                        return TextDecorations.Underline;
                }
                catch
                {
                    // Not a valid URL
                }
            }

            // Return default values for non-URLs
            if (parameter?.ToString() == "Color")
                return Color.FromArgb("#000000");
            else if (parameter?.ToString() == "Underline")
                return TextDecorations.None;

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}