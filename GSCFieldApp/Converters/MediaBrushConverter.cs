using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace GSCFieldApp.Converters
{
    /// <summary>
    /// A converter that takes a solidColorBrush and output it as a  Windows.UI.Xaml.Media brush, which
    /// is used for foregrounds of fonts.
    /// </summary>
    public class MediaBrushConverter: IValueConverter
    {
        public Brush defaultBrush = new SolidColorBrush();

        public MediaBrushConverter()
        {

            Color defaultColor = new Color();
            defaultColor.R = 0;
            defaultColor.G = 0;
            defaultColor.B = 0;
            defaultBrush = new SolidColorBrush(defaultColor);
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is SolidColorBrush)
            {
                Brush valueBrush = new SolidColorBrush((value as SolidColorBrush).Color);
                return valueBrush;
            };

            return (Brush)defaultBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is SolidColorBrush)
            {
                Brush valueBrush = new SolidColorBrush((value as SolidColorBrush).Color);
                return valueBrush;
            };

            return (Brush)defaultBrush;
        }
    }
}
