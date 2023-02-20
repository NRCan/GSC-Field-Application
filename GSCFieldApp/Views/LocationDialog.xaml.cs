using GSCFieldApp.ViewModels;
using System;
using Template10.Common;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using GSCFieldApp.Models;
using Windows.ApplicationModel.Resources;
using Template10.Controls;
using GSCFieldApp.Services.DatabaseServices;
using System.Threading.Tasks;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace GSCFieldApp.Views
{
    public sealed partial class LocationDialog : UserControl
    {
        public LocationViewModel locationVM { get; set; }
        public FieldNotes parentViewModel { get; set; }

        public ResourceLoader local = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();

        bool isBackButtonPressed = false;

        private readonly SolidColorBrush failBrush = new SolidColorBrush(Windows.UI.Colors.Red);
        private readonly Brush defaultBrush;

        public LocationDialog(FieldNotes inDetailViewModel)
        {
            parentViewModel = inDetailViewModel;

            this.InitializeComponent();
            locationVM = new LocationViewModel(inDetailViewModel)
            {
                LocationAlias = parentViewModel.location.LocationAlias,
                LocationID = parentViewModel.location.LocationID,

                //Keep in memory that this is a manual entry.
                entryType = parentViewModel.location.LocationEntryType
            };

            this.Loading += LocationDialog_Loading;
            this.Unloaded += LocationDialog_Unloaded;

            defaultBrush = this.LocationAcuracy.BorderBrush;
        }

        private void LocationDialog_Unloaded(object sender, RoutedEventArgs e)
        {
            //Detect manual entry, if it's the case pop station dialog
            if (locationVM.entryType == Dictionaries.DatabaseLiterals.locationEntryTypeManual && locationVM.doLocationUpdate == false && !isBackButtonPressed )
            {
                //Create a field note report to act like a parent
                FieldNotes stationParent = new FieldNotes
                {
                    location = locationVM.locationModel,
                    GenericAliasName = locationVM.LocationAlias,
                    GenericID = locationVM.LocationID.ToString()
                };
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
        private async void LocationSaveButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;

            Task<bool> isUIValid = isLocationValidAsync();

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

            }
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

            double.TryParse(this.LocationLong.Text, out double _long);
            double.TryParse(this.LocationLat.Text, out double _lat);
            int.TryParse(this.LocationEasting.Text, out int _easting);
            int.TryParse(this.LocationNorthing.Text, out int _northing);

            //Detect a projected system
            int.TryParse(this.LocationDatum.SelectedValue.ToString(), out int selectedEPGS);

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
            bool result = double.TryParse(this.LocationEasting.Text, out double east);

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
            bool result = double.TryParse(this.LocationNorthing.Text, out double north);

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
