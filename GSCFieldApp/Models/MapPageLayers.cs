using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace GSCFieldApp.Models
{
    public class MapPageLayers
    {
        public string LayerName { get; set; }
        public MapPageLayerSetting LayerSettings { get; set; }
        //public string FilePath { get; set; }
        //public bool FileVisible { get; set; } //Used for maps 
        //public Visibility FileCanDelete { get; set; } //If file can be deleted by user.
        //public double FileOpacity { get; set; }
    }
}
