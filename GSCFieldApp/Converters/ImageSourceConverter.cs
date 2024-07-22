using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSCFieldApp.Converters
{
    public class ImageSourceConverter: IValueConverter
    {
        public ImageSourceConverter()
        {
        }

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Uri inURI = null;
            if (value != null && value.ToString() != string.Empty)
            {
                inURI = new Uri(value.ToString());
            }

            return inURI;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && (UriImageSource)value != null)
            {
                UriImageSource outURI = (UriImageSource)value;
                string outPath = outURI.Uri.AbsolutePath;
                return outPath;
            }
            else
            {
                return string.Empty;
            }
            
        }

    }
}
