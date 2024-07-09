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

        public static object GetBindingContextPropertyValue(object src, string propertyName)
        {
            var obj = src?.GetType().GetProperty("BindingContext")?.GetValue(src, null);
            var obj2 = obj?.GetType().GetProperty(propertyName)?.GetValue(obj, null);
            
            return obj2;


        }
    }
}
