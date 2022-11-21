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
using System.Threading.Tasks;
using Windows.UI;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace GSCFieldApp.Views
{
    public sealed partial class LocationDialog : UserControl
    {
        public LocationViewModel locationVM { get; set; }
        public FieldNotes parentViewModel { get; set; }

        public ResourceLoader local = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();

        bool isBackButtonPressed = false;

        private SolidColorBrush failBrush = new SolidColorBrush(Windows.UI.Colors.Red);
        private Brush defaultBrush;

        public LocationDialog(FieldNotes inDetailViewModel)
        {
            parentViewModel = inDetailViewModel;

            this.InitializeComponent();
            locationVM = new LocationViewModel(inDetailViewModel);
            locationVM.LocationAlias = parentViewModel.location.LocationAlias;
            locationVM.LocationID = parentViewModel.location.LocationID;

            //Keep in memory that this is a manual entry.
            locationVM.entryType = parentViewModel.location.LocationEntryType;

            this.LocationSaveButton.GotFocus += LocationSaveButton_GotFocusAsync;

            this.Loading += LocationDialog_Loading;
            this.Unloaded += LocationDialog_Unloaded;

            defaultBrush = this.LocationAcuracy.BorderBrush;
        }

        private async void LocationSaveButton_GotFocusAsync(object sender, RoutedEventArgs e)
        {
            
            Task<bool> isUIValid = isLocationValidAsync();
            await isUIValid;

            if (isUIValid.Result)
            {
                locationVM.SaveDialogInfo();
                CloseControl();
            }
            else
            {
                ContentDialog defaultEventLocationDialog = new ContentDialog()
                {
                    Title = local.GetString("LocationDialogBadSaveTitle"),
                    Content = local.GetString("LocationDialogBadSaveContent"),
                    CloseButtonText = local.GetString("GenericDialog_ButtonOK")
                };
                defaultEventLocationDialog.Style = (Style)Application.Current.Resources["WarningDialog"];
                await Services.ContentDialogMaker.CreateContentDialogAsync(defaultEventLocationDialog, true);

                this.LocationDatum.Focus(FocusState.Programmatic);
            }
        }

        private void LocationDialog_Unloaded(object sender, RoutedEventArgs e)
        {
            //Detect manual entry, if it's the case pop station dialog
            if (locationVM.entryType == Dictionaries.DatabaseLiterals.locationEntryTypeManual && locationVM.doLocationUpdate == false && !isBackButtonPressed )
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
                dLocalSettings.SetSettingValue("forceNoteRefresh", true);
            }
            else
            {
                locationVM.newLocationEdit += locationVM.NavigateToReportAsync;
            }
        }


        /// <summary>
        /// Will fill the dialog with known information
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void LocationDialog_Loading(FrameworkElement sender, object args)
        {
            isBackButtonPressed = false;

            //Fill automatically the earthmat dialog if an edit is asked by the user.
            if (parentViewModel.ParentTableName == Dictionaries.DatabaseLiterals.TableLocation && locationVM.doLocationUpdate)
            {
                this.locationVM.AutoFillDialog(parentViewModel);
                this.pageHeader.Text = this.pageHeader.Text + "  " + parentViewModel.location.LocationAlias;

                if (parentViewModel.location.LocationEntryType == Dictionaries.DatabaseLiterals.locationEntryTypeManual || parentViewModel.location.LocationEntryType == Dictionaries.DatabaseLiterals.locationEntryTypeTap)
                {
                    this.locationVM.SetReadOnlyFields(true);
                }
                
            }
            else
            {
                this.pageHeader.Text = this.pageHeader.Text + "  " + parentViewModel.location.LocationAlias;

                this.locationVM.SetReadOnlyFields();
            }

            //Get default accent color from textbox border, for validating easting northings
            SolidColorBrush defaultBorderBrush = this.LocationLat.BorderBrush as SolidColorBrush;

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
            isBackButtonPressed = true;
            CloseControl();
        }

        #endregion

        #region SAVE
        private void LocationSaveButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        #endregion

        #region EVENTS
        private void ButtonConvertToUTM_Tapped(object sender, TappedRoutedEventArgs e)
        {
            locationVM.DisplayUTMCoordinatesAsync();
        }

        private void ButtonConvertToGeographic_Tapped(object sender, TappedRoutedEventArgs e)
        {
            locationVM.DisplayGeoCoordinatesAsync();
        }

        /// <summary>
        /// Will perform a validation on digits number
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LocationEasting_TextChanged(object sender, TextChangedEventArgs e)
        {
            isEastingValid();
        }

        /// <summary>
        /// Will perform a validation on digits number
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LocationNorthing_TextChanged(object sender, TextChangedEventArgs e)
        {
            isNorthingValid();
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Will make sure that either one of the coordinate pairs are filled
        /// </summary>
        /// <returns></returns>
        public Task<bool> isLocationValidAsync()
        {
            bool isValid = true;

            //Parse coordinates
            double _long = 0.0;
            double _lat = 0.0;
            int _easting = 0;
            int _northing = 0;

            double.TryParse(this.LocationLong.Text, out _long);
            double.TryParse(this.LocationLat.Text, out _lat);
            int.TryParse(this.LocationEasting.Text, out _easting);
            int.TryParse(this.LocationNorthing.Text, out _northing);

            //Detect a projected system
            int selectedEPGS = 0;
            int.TryParse(this.LocationDatum.SelectedValue.ToString(), out selectedEPGS);

            //Make sure that everything has been filled
            if ((_long != 0 && _lat != 0) || (_easting != 0 && _northing != 0))
            {
                if ((_easting != 0 && _northing != 0) && (_long == 0 && _lat == 0) && (selectedEPGS != 4617 || selectedEPGS != 4326))
                {
                    isValid = false;
                }                
            }
            else
            {

                //Only show for manual entries.
                if (!this.locationVM.ReadOnlyFields)
                {
                    isValid = false;
                }
            }

            return Task.FromResult(isValid);
        }

        /// <summary>
        /// Will make sure the entered easting coordinate has the right amount of digits
        /// </summary>
        public void isEastingValid()
        {
            double east;
            bool result = double.TryParse(this.LocationEasting.Text, out east);

            if (result)
            {
                // Source: https://www.maptools.com/tutorials/utm/details
                if (east < 834000 && east > 160000)
                {
                    this.LocationEasting.Text = east.ToString();
                    this.LocationEasting.BorderBrush = defaultBrush;
                }
                else
                {
                    this.LocationEasting.BorderBrush = failBrush;

                }

            }
            else
            {
                this.LocationEasting.BorderBrush = failBrush;

            }
        }

        /// <summary>
        /// Will make sure the entered easting coordinate has the right amount of digits
        /// </summary>
        public void isNorthingValid()
        {
            double north;
            bool result = double.TryParse(this.LocationNorthing.Text, out north);

            if (result)
            {
                // Source: https://www.maptools.com/tutorials/utm/details
                if (north < 10000000 && north > 0)
                {
                    this.LocationNorthing.Text = north.ToString();
                    this.LocationNorthing.BorderBrush = defaultBrush;
                }
                else
                {
                    this.LocationNorthing.BorderBrush = failBrush;
                }

            }
            else
            {
                this.LocationNorthing.BorderBrush = failBrush;
            }
        }


        #endregion


    }
}
