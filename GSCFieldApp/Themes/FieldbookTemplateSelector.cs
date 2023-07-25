using GSCFieldApp.Dictionaries;
using GSCFieldApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSCFieldApp.Themes
{
    public class FieldbookTemplateSelector: DataTemplateSelector
    {
        public DataTemplate DefaultTemplate { get; set; }
        public DataTemplate SelectedTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            //If the fieldbook database path matches the one set in the preferences, select it.
            return ((FieldBooks)item).ProjectDBPath == Preferences.Get(ApplicationLiterals.preferenceDatabasePath, string.Empty) ? SelectedTemplate : DefaultTemplate;
        }

    }
}
