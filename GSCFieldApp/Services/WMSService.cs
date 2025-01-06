using GSCFieldApp.Dictionaries;
using GSCFieldApp.Models;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Point = NetTopologySuite.Geometries.Point;

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
        public string OGCCrs = "CRS";
        public string BoundingBox = "BoundingBox";
        public string BoundMinx = "minx";
        public string BoundMiny = "miny";
        public string BoundMaxx = "maxx";
        public string BoundMaxy = "maxy";

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

        /// <summary>
        /// Will return a list of all coordinate system available in a get cap
        /// </summary>
        /// <param name="getCapabilityURL"></param>
        /// <returns></returns>
        public async Task<List<string>> GetListOfCRS(string getCapabilityURL)
        {
            //Init
            List<string> crs = new List<string>();

            if (getCapabilityURL != null && getCapabilityURL != string.Empty)
            {
                try
                {
                    //Get the xml doc
                    XDocument xdoc = XDocument.Load(getCapabilityURL);
                    if (xdoc != null)
                    {
                        //Get layer nodes
                        foreach (XElement rootLayerElement in xdoc.Descendants().Where(p => p.Name.LocalName == OGCCrs))
                        {
                            crs.Add(rootLayerElement.Value.ToString());
                        }
                    }
                }
                catch (Exception e)
                {
                    new ErrorToLogFile(e).WriteToFile();
                }

            }

            return crs;
        }

        public async Task<Tuple<Point, Point>> GetCRSExtent(string getCapabilityURL, string CRSName)
        {
            Tuple<Point, Point> extent = null;

            if (CRSName != null && CRSName != string.Empty)
            {
                try
                {
                    //Get the xml doc
                    XDocument xdoc = XDocument.Load(getCapabilityURL);
                    if (xdoc != null)
                    {
                        //Get layer nodes
                        foreach (XElement rootLayerElement in xdoc.Descendants().Where(p => p.Name.LocalName == BoundingBox))
                        {

                            //Get the queryable ones
                            if (rootLayerElement.Attribute(OGCCrs) != null && rootLayerElement.Attribute(OGCCrs).Value == CRSName)
                            {
                                if (rootLayerElement.Attribute(BoundMinx) != null)
                                {
                                    try
                                    {
                                        double minx = 0.0;
                                        double miny = 0.0;
                                        double maxx = 0.0;
                                        double maxy = 0.0;

                                        Double.TryParse(rootLayerElement.Attribute(BoundMinx).Value, out minx);
                                        Double.TryParse(rootLayerElement.Attribute(BoundMiny).Value, out miny);
                                        Double.TryParse(rootLayerElement.Attribute(BoundMaxx).Value, out maxx);
                                        Double.TryParse(rootLayerElement.Attribute(BoundMaxy).Value, out maxy);

                                        Point min = new Point(minx, miny);
                                        Point max = new Point(maxx, maxy);

                                        extent = new Tuple<Point, Point>( min, max);

                                    }
                                    catch (Exception e)
                                    {
                                        new ErrorToLogFile(e).WriteToFile();
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

            return extent;
        }
    }
}
