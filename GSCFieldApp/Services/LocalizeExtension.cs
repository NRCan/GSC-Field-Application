using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSCFieldApp.Services;
using GSCFieldApp.Resources.Strings;

namespace GSCFieldApp.Services
{
    /// <summary>
    /// This class will add a localization extension for dynamic
    /// values directly in the XAML.
    /// </summary>
    [ContentProperty(nameof(Key))]
    //give any name you want to this class; however,
    //you will use this name in XML like so: Text="{local:Localize hello_world}"
    public class LocalizeExtension : IMarkupExtension
    {
        //Generic LocalizableStrings name has to match your .resx filename
        private IStringLocalizer<LocalizableStrings> _localizer { get; }

        public string Key { get; set; } = string.Empty;

        public LocalizeExtension()
        {
            //you have to inject this like so because LocalizeExtension constructor 
            //has to be parameterless in order to be used in XML
            _localizer = ServiceHelper.GetService<IStringLocalizer<LocalizableStrings>>();
        }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            string localizedText = _localizer[Key];
            return localizedText;
        }

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => ProvideValue(serviceProvider);
    }
}
