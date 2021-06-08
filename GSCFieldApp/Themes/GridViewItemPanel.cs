using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace GSCFieldApp.Themes
{
    /// <summary>
    /// This custom panel is intended to have grid views items with different width and height. By default gridview items are all set
    /// just like the first item in the grid. With this custom panel all items have their own size.
    /// To be used inside itemsPanelTemplate.
    /// <GridView.ItemsPanel>
    ///    <ItemsPanelTemplate>
    ///        <customTheme:CustomPanel/>
    ///    </ItemsPanelTemplate>
    //</GridView.ItemsPanel>
    /// </summary>
    public class GridViewItemPanel : Panel
    {
        /// <summary>
        /// Will calculate size of item
        /// </summary>
        /// <param name="finalSize"></param>
        /// <returns></returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            var x = 0.0;
            var y = 0.0;
            double lastItemHeight = 0.0; //Keep taller item, else there could be unsync with only setting y to desired height of child since they are all different size.
            foreach (var child in Children)
            {

                if (child is GridViewItem)
                {
                    GridViewItem childGV = child as GridViewItem;
                    if (!(childGV.Content is GridViewHeaderItem))
                    {

                        if ((child.DesiredSize.Width + x) > finalSize.Width)
                        {
                            x = 0;
                            y += lastItemHeight;
                        }

                        var newpos = new Rect(x, y, child.DesiredSize.Width, child.DesiredSize.Height);
                        if (lastItemHeight < child.DesiredSize.Height)
                        {
                            lastItemHeight = child.DesiredSize.Height;
                        }
                        
                        child.Arrange(newpos);
                        x += child.DesiredSize.Width;
                    }
                    else
                    {
                        var newpos = new Rect(0, 0, child.DesiredSize.Width, child.DesiredSize.Height);
                        child.Arrange(newpos);

                        if (lastItemHeight < child.DesiredSize.Height)
                        {
                            y = lastItemHeight = child.DesiredSize.Height;
                        }
                        y = lastItemHeight;


                    }
                }
                else
                {
                    if ((child.DesiredSize.Width + x) > finalSize.Width)
                    {
                        x = 0;
                        y += lastItemHeight;
                    }

                    var newpos = new Rect(x, y, child.DesiredSize.Width, child.DesiredSize.Height);

                    if (lastItemHeight < child.DesiredSize.Height)
                    {
                        lastItemHeight = child.DesiredSize.Height;
                    }

                    
                    child.Arrange(newpos);
                    x += child.DesiredSize.Width;
                }




            }
            return finalSize;
        }

        /// <summary>
        /// Will calculate sixe of panel
        /// NOTE: Needs to take into account header items, and listviews that can grow in height
        /// </summary>
        /// <param name="availableSize"></param>
        /// <returns></returns>
        protected override Size MeasureOverride(Size availableSize)
        {

            //Reset 
            double _maxHeight = 0.0;

            //Width management
            double fullWidth = availableSize.Width; //Will be used to track when total item width is oversize
            List<double> processedWidthList = new List<double>(); //Will contain total row width (sum of items.width if < available size width)
            List<double> currentWidthList = new List<double>(); //to keep track of available item sizes
            double lastWidth = 0; //To keep track of last processed width
            double finalWidth = 0; //Will be set as the wider row from processedWidthList

            //Height management
            double fullHeigth = availableSize.Width; //Needs to be set as width else height is usually equal to infinity
            List<double> processedHeightList = new List<double>(); //Will contain total row width (sum of items.width if < available size width)
            List<double> currentHeightList = new List<double>(); //to keep track of available item sizes

            int iteration = 0; //To keep track of for loop iterations
            bool hasHeader = false; //To detect if an header needs to be taken care inside the process
            double headerHeight = 0.0;

            foreach (var child in Children)
            {
                if (child is GridViewItem)
                {
                    GridViewItem childGV = child as GridViewItem;

                    if (childGV.Content is GridViewHeaderItem)
                    {
                        hasHeader = true;
                    }
                }

                //Measure child
                try
                {
                    child.Measure(availableSize);
                }
                catch (Exception)
                {
                    child.Measure(new Size(availableSize.Width, availableSize.Width));
                }
                

                //Keep child width and height and add to list
                double currentWidth = child.DesiredSize.Width;
                double currentHeight = child.DesiredSize.Height;
                currentWidthList.Add(currentWidth);
                currentHeightList.Add(currentHeight);

                if (iteration == 0 && !hasHeader)
                {
                    processedWidthList.Add(currentWidth);
                    processedHeightList.Add(currentHeight);
                    fullHeigth = child.DesiredSize.Height; //Set a default height
                }
                else if (iteration == 0 && hasHeader)
                {
                    headerHeight = currentHeight;
                    processedWidthList.Add(0.0);
                    processedHeightList.Add(0.0);

                    //Set a default height
                    if (Children.Count > 1)
                    {
                        Children[1].Measure(availableSize);
                        fullHeigth = Children[1].DesiredSize.Height;
                    }
                    else
                    {
                        fullHeigth = headerHeight;
                    }

                }
                else
                {

                    //Check if total item width is wider then available size width
                    if (currentWidth + processedWidthList.Last() >= fullWidth)
                    {
                        processedWidthList.Add(currentWidth);
                    }
                    else
                    {
                        //If not wider, replace last item with a sum of last item and current width (so it makes the theorical row width)
                        lastWidth = processedWidthList.Last();
                        processedWidthList.RemoveAt(processedWidthList.Count -1);
                        processedWidthList.Add(lastWidth + currentWidth);
                    }

                    //Check if total item height is higher then available size height
                    if (currentHeight * processedWidthList.Count > fullHeigth)
                    {
                        //processedHeightList.Add(currentHeight);
                        fullHeigth = fullHeigth + currentHeight;
                    }

                }

                iteration++;

                //Keep track of heighest item value
                var desiredheight = child.DesiredSize.Height;
                if (desiredheight > _maxHeight)
                    _maxHeight = desiredheight;
                
            }

            //Find larger row
            foreach (double ws in processedWidthList)
            {
                if (ws > finalWidth)
                {
                    finalWidth = ws;
                }
            }

            //Count number of rows
            double rows = processedWidthList.Count;

            //Set size and height to maximum item height if there is only one row
            Size newSize = new Size(finalWidth, (_maxHeight * rows) + headerHeight );

            //If there is more then one row set height as the sum of height for all rows.
            if (rows > 1 && !hasHeader)
            {
                newSize = new Size(finalWidth, fullHeigth);
            }
            else if (rows > 1 && hasHeader)
            {
                newSize = new Size(finalWidth, fullHeigth + headerHeight);
            }
            return newSize;

        }

    }
}
