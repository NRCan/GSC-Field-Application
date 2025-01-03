using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GSCFieldApp.Models
{
    /// <summary>
    /// A class used to show a list of layers/features to add, 
    /// coming from a selected geopackage 
    /// </summary>
    public class MapPageLayerSelection
    {
        public bool Selected { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ID { get; set; }
        public string Path { get; set; } //For geopackages
        public string URL { get; set; } //For WMS

    }
}
