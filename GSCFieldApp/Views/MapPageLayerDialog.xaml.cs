using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Garibaldi.ViewModels;
using Garibaldi.Models;
using Esri.ArcGISRuntime.Mapping;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Garibaldi.Views
{
    public sealed partial class MapPageLayerDialog : ContentDialog
    {
        public MapPageLayerViewModel layerDialogViewModel { get; set; }
        public Map parentMap;
        public bool mapsLoaded = false;
        public MapPageLayerDialog(Map inMap)
        {
            
            this.InitializeComponent();
            parentMap = inMap;
            this.layerDialogViewModel = new MapPageLayerViewModel(inMap);
            this.Loaded += MapPageLayerDialog_Loaded;
        }

        private void MapPageLayerDialog_Loaded(object sender, RoutedEventArgs e)
        {
            mapsLoaded = true;
        }


        #region EVENTS

        /// <summary>
        /// Commit changes when user is ready. Mainly, the ordering of the layer is the only change that will be saved.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (sender !=null)
            {
                layerDialogViewModel.EndDialog();
            }
        }

        /// <summary>
        /// Will be triggered when user toggles the switch. This methods needs to be here, else in the view model it doesn't work since
        /// the event is inside a data template in the xaml.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mapFileName_Toggled(object sender, RoutedEventArgs e)
        {

            if (sender != null && mapsLoaded)
            {
                ToggleSwitch senderToggle = sender as ToggleSwitch;
                layerDialogViewModel.SetVisibility(senderToggle);
            }

        }
        
        /// <summary>
        /// Will triggered a delete action on the selected layer. It'll be removed from the map and deleted from the local state folder.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MapDeleteIcon_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender!= null && mapsLoaded)
            {
                layerDialogViewModel.DeleteFile();
            }
        }

        #endregion


    }
}
