using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSCFieldApp.Controls
{
    public class ComboBox
    {
        public List<ComboBoxItem> cboxItems 
        { 
            get; 
            set; 
        }
        public int cboxDefaultItemIndex { get; set; }

        public ComboBox() 
        { 
            cboxItems = new List<ComboBoxItem>();
        }
    }
}
