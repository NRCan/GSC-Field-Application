using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace GSCFieldApp.Converters
{
    /// <summary>
    /// A converter that takes a solidcolorbrush and outputs a solidcolorbrush, 
    /// with a default if value is null. Used for control colors.
    /// </summary>
    public class SolidColorBrushConverter: IValueConverter
    {
        SolidColorBrush defaultBrush = new SolidColorBrush();

        public SolidColorBrushConverter()
        {
            
            Color defaultColor = new Color();
            defaultColor.R = 0;
            defaultColor.G = 0;
            defaultColor.B = 0;
            defaultBrush.Color = defaultColor;
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is SolidColorBrush)
            {
                SolidColorBrush valueBrush = value as SolidColorBrush;
                return valueBrush.Color;
            };

            return (SolidColorBrush)defaultBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is SolidColorBrush)
            {
                SolidColorBrush valueBrush = value as SolidColorBrush;
                return valueBrush.Color;
            };

            return (SolidColorBrush)defaultBrush;
        }
    }
}
