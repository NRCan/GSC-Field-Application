using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Text;
using Windows.UI.Xaml.Data;

namespace GSCFieldApp.Converters
{
    class PicklistDefaultValueConverter : IValueConverter
    {
        
        public PicklistDefaultValueConverter()
        {

        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string && value.ToString() == Dictionaries.DatabaseLiterals.boolYes)
            {
                return FontWeights.Bold;
            }
            return FontWeights.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
