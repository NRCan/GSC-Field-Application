﻿using GSCFieldApp.Dictionaries;
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
        public enum LayerTypes { mbtiles, wms, gpkg }

        public string LayerName { get; set; }

        public int LayerOrder {get; set;}

        public LayerTypes LayerType { get; set;}

        public string? LayerPathOrURL { get; set; }

        public bool LayerVisibility { get; set; }

        public double LayerOpacity { get; set; }

        public string LayerID { get; set; }
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
                mpl.LayerOpacity = inLayer.Opacity;
                mpl.LayerVisibility = inLayer.Enabled;
                mpl.LayerID = string.Empty;

                //Retrieve hidden info from tag
                if (inLayer.Tag != null)
                {
                    mpl.LayerPathOrURL = inLayer.Tag.ToString();

                    TileLayer isTileLayer = inLayer as TileLayer;
                    if (isTileLayer != null && mpl.LayerPathOrURL.Contains(DatabaseLiterals.LayerTypeMBTiles))
                    {
                        mpl.LayerType = MapPageLayer.LayerTypes.mbtiles;
                    }
                    else if (isTileLayer != null && (mpl.LayerPathOrURL.ToLower().Contains("wms") || mpl.LayerPathOrURL.ToLower().Contains("ows")))
                    {
                        mpl.LayerType = MapPageLayer.LayerTypes.wms;

                        //Get layer id
                        string[] lid = mpl.LayerPathOrURL.Split("=");
                        if (lid.Count() > 0)
                        {
                            mpl.LayerID = lid[1];
                        }
                    }
                    else if (mpl.LayerPathOrURL.Contains("gpkg"))
                    {
                        mpl.LayerType = MapPageLayer.LayerTypes.gpkg;
                    }

                }

            }

            return mpl;
        }
    }
}
