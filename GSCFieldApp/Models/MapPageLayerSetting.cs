using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSCFieldApp.Models
{
    public class MapPageLayerSetting
    {
        public double LayerOpacity { get; set; } //Layer menu slider opacity
        public bool LayerVisibility { get; set; } //Layer menu toggle value
        public int LayerOrder { get; set; } //Layer menu order
    }
}
