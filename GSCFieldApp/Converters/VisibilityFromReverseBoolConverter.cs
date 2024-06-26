﻿using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace GSCFieldApp.Converters
{
    public class VisibilityFromReverseBoolConverter : IValueConverter
    {

        public VisibilityFromReverseBoolConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool && !(bool)value)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is Visibility && (Visibility)value == Visibility.Visible)
            {
                return false;
            }
            return true;
        }

    }
}
