using GSCFieldApp.Dictionaries;
using Mapsui.Layers;
using Mapsui.Tiling.Layers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GSCFieldApp.Models
{
    /// <summary>
    /// A class that will be used to track user added
    /// layers in the map page.
    /// This class will need to be serialized to json and saved locally
    /// beside the database of the field book.
    /// </summary>
    public class MapPageLayer
    {
        public enum LayerTypes { mbtiles, wms }

        public string LayerName { get; set; }

        public int LayerOrder {get; set;}

        public LayerTypes LayerType { get; set;}

        public string? LayerPathOrURL { get; set; }
    }

    public class MapPageLayerBuilder
    {
        public MapPageLayerBuilder()
        {

        }

        public MapPageLayer GetMapPageLayer(ILayer inLayer = null, int index = 0)
        {
            MapPageLayer mpl = new MapPageLayer();

            mpl.LayerOrder = index;

            //In case something is passed, keep key information
            if (inLayer != null)
            {
                mpl.LayerName = inLayer.Name;
                mpl.LayerOrder = 0;

                //Retrieve hidden info from tag
                if (inLayer.Tag != null)
                {
                    mpl.LayerPathOrURL = inLayer.Tag.ToString();

                    TileLayer isTileLayer = inLayer as TileLayer;
                    if (isTileLayer != null && mpl.LayerPathOrURL.Contains(DatabaseLiterals.LayerTypeMBTiles))
                    {
                        mpl.LayerType = MapPageLayer.LayerTypes.mbtiles;
                    }
                    else if (isTileLayer != null && (mpl.LayerPathOrURL.Contains("wms") || mpl.LayerPathOrURL.Contains("ows")))
                    {
                        mpl.LayerType = MapPageLayer.LayerTypes.wms;
                    }

                }

            }

            return mpl;
        }
    }
}
