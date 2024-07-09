using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GSCFieldApp.Services
{
    public class BindingUtility
    {
        public static object GetPropertyValue(object src, string propertyName)
        {
            if (propertyName.Contains("."))
            {
                var splitIndex = propertyName.IndexOf('.');
                var parent = propertyName.Substring(0, splitIndex);
                var child = propertyName.Substring(splitIndex + 1);
                var obj = src?.GetType().GetProperty(parent)?.GetValue(src, null);
                return GetPropertyValue(obj, child);
            }

            return src?.GetType().GetProperty(propertyName)?.GetValue(src, null);
        }

        /// <summary>
        /// Will return the binded object as an object for Converter Parameter values coming
        /// from xaml.
        /// </summary>
        /// <param name="src">The source name of where the object can be searched</param>
        /// <param name="propertyName">The property name needed to be returned as an object</param>
        /// <returns></returns>
        public static object GetBindingContextPropertyValue(object src, string propertyName)
        {
            //Get the binding context value
            var bindingContextObject = src?.GetType().GetProperty("BindingContext")?.GetValue(src, null);

            //From the binding context extract the right property from it's name.
            //Usually found in a view model class related to the xaml.
            var bindingObject = bindingContextObject?.GetType().GetProperty(propertyName)?.GetValue(bindingContextObject, null);
            
            return bindingObject;


        }
    }
}
