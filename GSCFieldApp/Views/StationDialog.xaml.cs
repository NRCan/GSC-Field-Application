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
using Windows.Devices.Geolocation;
using Template10.Common;
using Template10.Controls;
using GSCFieldApp.Services.SettingsServices;
using GSCFieldApp.ViewModels;
using GSCFieldApp.Models;
using System.ComponentModel;
using Esri.ArcGISRuntime.Geometry;
using System.Reflection;
using System.Threading.Tasks;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace GSCFieldApp.Views
{
    public sealed partial class StationDataPart : UserControl 
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public FieldNotes parentStationReport;
        public StationViewModel ViewModel { get; set; }
        public FieldLocation mapPosition { get; set; }
        public bool _isWaypoint = false;
        public StationDataPart(FieldNotes inParentReport, bool isWaypoint)
        {
            if (inParentReport != null)
            {
                parentStationReport = inParentReport;
            }

            _isWaypoint = isWaypoint;

            this.InitializeComponent();
            this.ViewModel = new StationViewModel(isWaypoint);

            if (mapPosition !=null)
            {
                this.ViewModel.Location = mapPosition;
            }
            this.Loading += StationDataPart_Loading;
            this.Loaded += StationDataPart_Loaded;

            this.stationSaveButton.GotFocus += StationSaveButton_GotFocus; //bug 306
        }

        private void StationSaveButton_GotFocus(object sender, RoutedEventArgs e)
        {

            ViewModel.SaveDialogInfo();
            CloseControlAsync();
        }

        private void StationDataPart_Loaded(object sender, RoutedEventArgs e)
        {
            //EasterEgg();
        }

        private void StationDataPart_Loading(FrameworkElement sender, object args)
        {
            //Get information to automatically fill the dialog if data already exists (report page vs new station)
            if (parentStationReport != null && parentStationReport.station.StationID != null)
            {
                if (parentStationReport.station.StationAlias.Contains(Dictionaries.DatabaseLiterals.KeywordStationWaypoint))
                {
                    _isWaypoint = true;
                    this.pageHeader.Text = parentStationReport.station.StationAlias; //Set to selected item not calculated one.
                }
                else
                {
                    this.pageHeader.Text = this.pageHeader.Text + "  " + parentStationReport.station.StationAlias; //Set to selected item not calculated one.
                }
                this.ViewModel.AutoFillDialog(parentStationReport, _isWaypoint);

            }
            else
            {
                if (!_isWaypoint)
                {
                    this.pageHeader.Text = this.pageHeader.Text + "  " + this.ViewModel.Alias;
                }
                else
                {
                    this.pageHeader.Text = this.ViewModel.Alias;
                }
                
                ViewModel.SetCurrentLocationInUI(mapPosition);
            }

            
        }


        #region SAVE

        private void stationSaveButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.stationSaveButton.Focus(FocusState.Programmatic);

        }

        #endregion

        #region CLOSE
        /// <summary>
        /// Will close the modal dialog.
        /// </summary>
        public async void CloseControlAsync()
        {

            //Get the current window and cast it to a DeleteDialog ModalDialog and shut it down.
            WindowWrapper.Current().Dispatcher.Dispatch(() =>
            {
                var modal = Window.Current.Content as Template10.Controls.ModalDialog;
                var view = modal.ModalContent as StationDataPart;
                modal.ModalContent = view;
                modal.IsModal = false;
            });

        }

        private async void stationBackButton_Click(object sender, RoutedEventArgs e)
        {
            //variables
            bool canProceedWithClose = true;

            //Warning user that if this station isn't closed normally the associated location will also be deleted.
            if (ViewModel.Location.isManualEntry)
            {
                Task<bool> canClose = ViewModel.DeleteAssociatedLocationIfManualEntryAsync();
                await canClose;
                canProceedWithClose = canClose.Result;
            }

            if (canProceedWithClose)
            {
                CloseControlAsync();
            }

            
        }

        private async void stationBackButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //variables
            bool canProceedWithClose = true;

            //Warning user that if this station isn't closed normally the associated location will also be deleted.
            if (ViewModel.Location.isManualEntry)
            {
                Task<bool> canClose = ViewModel.DeleteAssociatedLocationIfManualEntryAsync();
                await canClose;
                canProceedWithClose = canClose.Result;
            }

            if (canProceedWithClose)
            {
                CloseControlAsync();
            }
        }
        #endregion

        /// <summary>
        /// Surprise!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NoteTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox senderBox = sender as TextBox;
            if (senderBox.Text.ToLower().Contains("mosquito"))
            {
                GSCFieldApp.Themes.EasterEgg mosquitoEgg = new Themes.EasterEgg();
                mosquitoEgg.ShowMosquito(this.obsRelativePanel, 42);
            }
            if (senderBox.Text.ToLower().Contains("do a barrel roll"))
            {
                GSCFieldApp.Themes.EasterEgg barrel = new Themes.EasterEgg();
                barrel.DoABarrelRollAsync(this.stationUserControl);
            }
            if (senderBox.Text.ToLower().Contains("flip me"))
            {
                GSCFieldApp.Themes.EasterEgg ee = new Themes.EasterEgg();
                ee.pilf(this.stationUserControl);
            }
            if (senderBox.Text.ToLower().Contains("unicorn theme"))
            {
                GSCFieldApp.Themes.EasterEgg ut = new Themes.EasterEgg();
                ut.UnicornThemeAsync();

            }
        }

        private void ConcatValueCheck_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //Find the clicked symbol icon list view parent
            SymbolIcon senderIcon = sender as SymbolIcon;
            DependencyObject iconParent = VisualTreeHelper.GetParent(senderIcon);
            while (!(iconParent is ListView))
            {
                iconParent = VisualTreeHelper.GetParent(iconParent);

            }

            //Find value associated with clicked symbol icon and remove from list view.
            ListView parentListView = iconParent as ListView;
            IList<object> selectedValues = parentListView.SelectedItems;
            if (selectedValues.Count > 0)
            {
                foreach (object values in selectedValues)
                {
                    ViewModel.RemoveSelectedValue(values, parentListView.Name);
                }
            }
        }
    }
}
