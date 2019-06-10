using GSCFieldApp.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Template10.Common;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using GSCFieldApp.Models;
using System.Diagnostics;

using Esri.ArcGISRuntime.Geometry;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace GSCFieldApp.Views
{
    public sealed partial class LocationDialog : UserControl
    {
        public LocationViewModel locationVM { get; set; }
        public FieldNotes parentViewModel { get; set; }

        public LocationDialog(FieldNotes inDetailViewModel)
        {
            parentViewModel = inDetailViewModel;

            this.InitializeComponent();
            locationVM = new LocationViewModel(inDetailViewModel);
            this.Loading += LocationDialog_Loading;
            this.LocationSaveButton.GotFocus += LocationSaveButton_GotFocus;
        }



        /// <summary>
        /// Will fill the dialog with known information
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void LocationDialog_Loading(FrameworkElement sender, object args)
        {
            //Fill automatically the earthmat dialog if an edit is asked by the user.
            if (parentViewModel.ParentTableName == Dictionaries.DatabaseLiterals.TableLocation && locationVM.doLocationUpdate)
            {
                this.locationVM.AutoFillDialog(parentViewModel);
                this.pageHeader.Text = this.pageHeader.Text + "  " + parentViewModel.location.LocationAlias;
            }

            DisplayUTMCoordinates();

        }

        private void DisplayUTMCoordinates()
        {
            int zoneNumber = 0;
            if (Double.TryParse(this.LocationLong.Text, out double longitude))
            {
                zoneNumber = ((int)((longitude + 180) / 6) + 1);
            }

            string zoneQualifier = "";
            if (Double.TryParse(this.LocationLat.Text, out double latitude))
            {
                zoneQualifier = (latitude >= 0 ? "North" : "South");
            }

            this.LocationZone.Text = zoneNumber.ToString() + " " + zoneQualifier;

            // WGS84 based
            int outWKID = 0;
            if (zoneQualifier == "North")
            {
                outWKID = 32600 + zoneNumber;
            }
            else
            {
                outWKID = 32700 + zoneNumber;
            }

            MapPoint geoPoint = new MapPoint(Double.Parse(this.LocationLong.Text), Double.Parse(this.LocationLat.Text), SpatialReferences.Wgs84);
            var outSpatialRef = new Esri.ArcGISRuntime.Geometry.SpatialReference(outWKID);
            MapPoint projPoint = (MapPoint)Esri.ArcGISRuntime.Geometry.GeometryEngine.Project(geoPoint, outSpatialRef);

            this.LocationNorthing.Text = ((int)projPoint.Y).ToString();
            this.LocationEasting.Text = ((int)projPoint.X).ToString();
            
            // Applies to NAD83
            //int outWKID = 26900 + zoneNumber;

            //MapPoint geoPoint = new MapPoint(Double.Parse(this.LocationLong.Text), Double.Parse(this.LocationLat.Text), SpatialReferences.Wgs84);
            //var outSpatialRef = new Esri.ArcGISRuntime.Geometry.SpatialReference(outWKID);
            //MapPoint projPoint = (MapPoint)Esri.ArcGISRuntime.Geometry.GeometryEngine.Project(geoPoint, outSpatialRef);

            //// Definitions exist for UTM zones 1-23, 58,59,60 North
            //if (Enumerable.Range(1, 23).Contains(zoneNumber))
            //{
            //    this.LocationNorthing.Text = ((int)projPoint.Y).ToString();
            //    this.LocationEasting.Text = ((int)projPoint.X).ToString();
            //}
            //else
            //{
            //    this.LocationNorthing.Text = "na";
            //    this.LocationEasting.Text = "na";
            //}
        }


        #region CLOSE
        /// <summary>
        /// Will close the modal dialog.
        /// </summary>
        public void CloseControl()
        {

            //Get the current window and cast it to a DeleteDialog ModalDialog and shut it down.
            WindowWrapper.Current().Dispatcher.Dispatch(() =>
            {
                var modalLocationClose = Window.Current.Content as Template10.Controls.ModalDialog;
                var viewLocationClose = modalLocationClose.ModalContent as LocationDialog;
                modalLocationClose.ModalContent = viewLocationClose;
                modalLocationClose.IsModal = false;
            });
        }

        private void LocationBackButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            CloseControl();
        }

        #endregion

        #region SAVE
        private void LocationSaveButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.LocationSaveButton.Focus(FocusState.Programmatic);

        }
        private void LocationSaveButton_GotFocus(object sender, RoutedEventArgs e)
        {
            locationVM.SaveDialogInfo();
            CloseControl();
        }

        #endregion
    }
}
