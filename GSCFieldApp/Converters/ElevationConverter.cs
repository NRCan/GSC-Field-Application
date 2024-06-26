﻿using System;
using Windows.UI.Xaml.Data;

namespace GSCFieldApp.Converters
{
    public class ElevationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return value;

            return string.Format("{0} m", value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
