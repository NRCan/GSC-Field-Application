using GSCFieldApp.Dictionaries;
using GSCFieldApp.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GSCFieldApp.Services
{
    public class WMSService
    {
        public string OGCWmsLayer = "Layer";
        public string OGCWmsLayerQueryable = "queryable";
        public string OGCWmsLayerQueryableTrue = "1";
        public string OGCWmsLayerTitle = "Title";
        public string OGCWmsLayerName = "Name";
        public string GetCap = "GetCapabilities";

        /// <summary>
        /// Will return a list of all the layers from a given get capability xml url
        /// </summary>
        /// <param name="getCapabilityURL"></param>
        /// <returns></returns>
        public async Task<List<MapPageLayerSelection>> GetListOfWMSLayers(string getCapabilityURL)
        { 
            //Init
            List<MapPageLayerSelection> layers = new List<MapPageLayerSelection>();

            if (getCapabilityURL != null && getCapabilityURL != string.Empty)
            {
                try
                {
                    //Get the xml doc
                    XDocument xdoc = XDocument.Load(getCapabilityURL);
                    if (xdoc != null)
                    {
                        //Get layer nodes
                        foreach (XElement rootLayerElement in xdoc.Descendants().Where(p => p.Name.LocalName == OGCWmsLayer))
                        {
                            //Go through child nodes of layer
                            foreach (XElement subLayerElement in rootLayerElement.Descendants())
                            {
                                //Get the queryable ones
                                if (subLayerElement.Attribute(OGCWmsLayerQueryable) != null && subLayerElement.Attribute(OGCWmsLayerQueryable).Value == OGCWmsLayerQueryableTrue)
                                {
                                    MapPageLayerSelection mpls = new MapPageLayerSelection();

                                    //Go through child nodes to retrive layer name and title
                                    XElement layerNameElement = subLayerElement.Descendants().Where(p => p.Name.LocalName == OGCWmsLayerName).First();
                                    
                                    if (layerNameElement.Value.ToString() != string.Empty )
                                    {
                                            
                                        mpls.Selected = false;
                                        mpls.ID = layerNameElement.Value.ToString();
                                        mpls.URL = getCapabilityURL.Split("?")[0] + "?" + ApplicationLiterals.keywordWMSLayers + mpls.ID;
                                    }

                                    //Get layer title
                                    XElement titleElement = subLayerElement.Descendants().Where(p => p.Name.LocalName == OGCWmsLayerTitle).First();
                                    if (titleElement.Value.ToString() != string.Empty)
                                    {
                                        mpls.Name = titleElement.Value.ToString();
                                    }

                                    if (mpls.ID != null && mpls.ID != string.Empty)
                                    {
                                        layers.Add(mpls);
                                    }

                                }
                            }
                                
                        }
                    }
                }
                catch (Exception e)
                {
                    new ErrorToLogFile(e).WriteToFile();
                }

            }

            return layers;
        }
    }
}
