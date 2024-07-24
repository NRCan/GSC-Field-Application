using GSCFieldApp.Controls;
using SkiaSharp.Views.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSCFieldApp.Services;


namespace GSCFieldApp.Converters
{
    public class ComboboxItemToString: IValueConverter
    {
        public ComboboxItemToString() { }

        /// <summary>
        /// This value will be set based on incoming list of cbox values
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ComboBoxItem incomingString = new ComboBoxItem();

            if (parameter is Binding binding)
            {
                parameter = BindingUtility.GetBindingContextPropertyValue(binding.Source, binding.Path);
            }

            if (value != null && value.ToString() != string.Empty && parameter != null && parameter is ComboBox)
            {
                ComboBox paraBox = (ComboBox)parameter;
                
                foreach (ComboBoxItem cbox in paraBox.cboxItems)
                {
                    if (cbox.itemValue == value.ToString())
                    {
                        incomingString = paraBox.cboxItems[paraBox.cboxItems.IndexOf(cbox)];
                        break;
                    }
                }  
            }

            return incomingString;
        }

        /// <summary>
        /// This combobox item string value will be the one saved in the database
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is ComboBoxItem && ((ComboBoxItem)value).itemValue != null)
            {
                ComboBoxItem comboboxItem = (ComboBoxItem)value;
                return comboboxItem.itemValue.ToString();
            }
            else
            {
                return string.Empty;
            }
        }

    }


}
