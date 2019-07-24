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
using Windows.UI.Core;
using Windows.ApplicationModel.Resources;
using Template10.Controls;
using GSCFieldApp.Services.DatabaseServices;
using Windows.UI.Xaml.Media.Animation;
using Template10.Services.NavigationService;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace GSCFieldApp.Views
{
    public sealed partial class LocationDialog : UserControl
    {
        public LocationViewModel locationVM { get; set; }
        public FieldNotes parentViewModel { get; set; }

        public ResourceLoader local = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();

        public LocationDialog(FieldNotes inDetailViewModel)
        {
            parentViewModel = inDetailViewModel;

            this.InitializeComponent();
            locationVM = new LocationViewModel(inDetailViewModel);
            locationVM.LocationAlias = parentViewModel.location.LocationAlias;
            locationVM.LocationID = parentViewModel.location.LocationID;

            //Keep in memory that this is a manual entry.
            if (parentViewModel.location.LocationEntryType.ToLower().Contains(Dictionaries.DatabaseLiterals.KeywordManual))
            {
                locationVM.entryType = parentViewModel.location.LocationEntryType;
            }
            
            this.Loading += LocationDialog_Loading;
            this.LocationSaveButton.GotFocus += LocationSaveButton_GotFocus;
            this.Unloaded += LocationDialog_Unloaded;
        }

        private void LocationDialog_Unloaded(object sender, RoutedEventArgs e)
        {
            //Detect manual entry, if it's the case pop station dialog
            if (locationVM.entryType != null)
            {
                //Create a field note report to act like a parent
                FieldNotes stationParent = new FieldNotes();
                stationParent.location = locationVM.locationModel;
                stationParent.GenericAliasName = locationVM.LocationAlias;
                stationParent.GenericID = locationVM.LocationID;
                stationParent.GenericID = Dictionaries.DatabaseLiterals.TableLocation;

                //Create a map point
                var modal = Window.Current.Content as ModalDialog;
                var view = modal.ModalContent as Views.StationDataPart;
                modal.ModalContent = view = new Views.StationDataPart(stationParent, false);
                view.mapPosition = locationVM.locationModel;
                view.ViewModel.newStationEdit += locationVM.NavigateToReportAsync; //Detect when the add/edit request has finished.
                modal.IsModal = true;

                DataLocalSettings dLocalSettings = new DataLocalSettings();
                dLocalSettings.SetSettingValue("forceNoteRefresh", false);
            }
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
            else
            {
                this.pageHeader.Text = this.pageHeader.Text + "  " + parentViewModel.location.LocationAlias;
                this.locationVM.SetReadOnlyFields();
            }

            DisplayUTMCoordinatesAsync();

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

        #region EVENTS
        private async void ButtonConvertToUTM_TappedAsync(object sender, TappedRoutedEventArgs e)
        {
            DisplayUTMCoordinatesAsync();
        }

        private void ButtonConvertToGeographic_Tapped(object sender, TappedRoutedEventArgs e)
        {
            DisplayGeoCoordinatesAsync();
        }

        #endregion

        #region METHODS
        /// <summary>
        /// Will convert a given set of geographic coordinates into projected.
        /// Given the user has selected a proper projection
        /// </summary>
        private async void DisplayUTMCoordinatesAsync()
        {
            //XY
            double x_value = 0.0;
            double y_value = 0.0;
            if (this.LocationLong.Text != string.Empty)
            {
                x_value = Double.Parse(this.LocationLong.Text);
            }
            if (this.LocationLat.Text != string.Empty)
            {
                y_value = Double.Parse(this.LocationLat.Text);
            }

            //Transform
            if (x_value != 0.0 && y_value != 0.0)
            {
                //Bad system
                bool isSystemValid = false;

                if (this.LocationDatum.SelectedValue != null)
                {
                    //Detect a projected system
                    int selectedEPGS = 0;
                    int.TryParse(this.LocationDatum.SelectedValue.ToString(), out selectedEPGS);
                    if (selectedEPGS > 10000)
                    {
                        //Detect Datum difference
                        SpatialReference inSR = SpatialReferences.Wgs84; //Default
                        if (selectedEPGS > 26900 && selectedEPGS < 27000)
                        {
                            inSR = new Esri.ArcGISRuntime.Geometry.SpatialReference(4617);
                        }

                        MapPoint geoPoint = new MapPoint(x_value, y_value, inSR);
                        var outSpatialRef = new Esri.ArcGISRuntime.Geometry.SpatialReference(selectedEPGS);
                        MapPoint projPoint = (MapPoint)Esri.ArcGISRuntime.Geometry.GeometryEngine.Project(geoPoint, outSpatialRef);

                        int y = (int)projPoint.Y;
                        int x = (int)projPoint.X;
                        this.LocationNorthing.Text = y.ToString();
                        this.LocationEasting.Text = x.ToString();



                        isSystemValid = true;
                    }

                }

                if (!isSystemValid)
                {
                    //Show warning to select something
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High, async () =>
                    {
                        ContentDialog defaultEventLocationDialog = new ContentDialog()
                        {
                            Title = local.GetString("LocationDialogDatumTitle"),
                            Content = local.GetString("LocationDialogDatumContent"),
                            CloseButtonText = local.GetString("GenericDialog_ButtonOK")
                        };
                        defaultEventLocationDialog.Style = (Style)Application.Current.Resources["WarningDialog"];
                        await Services.ContentDialogMaker.CreateContentDialogAsync(defaultEventLocationDialog, true);

                    }).AsTask();
                }

            }
            else
            {
                this.LocationNorthing.Text = x_value.ToString();
                this.LocationEasting.Text = y_value.ToString();
            }

        }

        /// <summary>
        /// Will convert a given set of projected coordinates into geographic ones.
        /// </summary>
        private async void DisplayGeoCoordinatesAsync()
        {
            //XY
            int x_value = 0;
            int y_value = 0;
            if (this.LocationEasting.Text != string.Empty)
            {
                x_value = int.Parse(this.LocationEasting.Text);
            }
            if (this.LocationNorthing.Text != string.Empty)
            {
                y_value = int.Parse(this.LocationNorthing.Text);
            }

            //Transform
            if (x_value != 0.0 && y_value != 0.0)
            {

                if (this.LocationDatum.SelectedValue != null)
                {
                    
                    //Detect a projected system
                    int selectedEPGS = 0;
                    int.TryParse(this.LocationDatum.SelectedValue.ToString(), out selectedEPGS);
                    
                    //Detect Datum difference
                    SpatialReference inSR = new Esri.ArcGISRuntime.Geometry.SpatialReference(selectedEPGS);
                    SpatialReference outSR = SpatialReferences.Wgs84; //Default
                    if ((selectedEPGS > 26900 && selectedEPGS < 27000) || selectedEPGS == 4617)
                    {
                        outSR = new Esri.ArcGISRuntime.Geometry.SpatialReference(4617);
                    }

                    //Get geographic point
                    MapPoint geoPoint = locationVM.CalculateGeographicCoordinate(x_value, y_value, inSR, outSR);

                    double y = geoPoint.Y;
                    double x = geoPoint.X;
                    this.LocationLat.Text = y.ToString();
                    this.LocationLong.Text = x.ToString();

                }
            }
            else
            {
                this.LocationLong.Text = x_value.ToString();
                this.LocationLat.Text = y_value.ToString();
            }

        }
        #endregion
    }
}
