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
using GSCFieldApp.ViewModels;
using GSCFieldApp.Models;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace GSCFieldApp.Views
{
    public sealed partial class SampleDialog : UserControl
    {
        public SampleViewModel ViewModel { get; set; }
        public FieldNotes parentViewModel { get; set; }
        public bool isAQuickSample = false;

        public SampleDialog(FieldNotes inDetailViewModel, bool isQuickSample)
        {
            parentViewModel = inDetailViewModel;
            isAQuickSample = isQuickSample;

            this.InitializeComponent();

            ViewModel = new SampleViewModel(inDetailViewModel);
            this.Loading += SampleDialog_Loading;
            this.sampleSaveButton.GotFocus += SampleSaveButton_GotFocus;
        }

        private void SampleSaveButton_GotFocus(object sender, RoutedEventArgs e)
        {
            ViewModel.SaveDialogInfo();
            CloseControl();
        }

        /// <summary>
        /// Will fill the dialog with known information
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void SampleDialog_Loading(FrameworkElement sender, object args)
        {
            //Fill automatically the earthmat dialog if an edit is asked by the user.
            if (parentViewModel.GenericTableName == Dictionaries.DatabaseLiterals.TableSample && ViewModel.doSampleUpdate)
            {
                this.ViewModel.AutoFillDialog(parentViewModel);
                this.pageHeader.Text = this.pageHeader.Text + "  " + parentViewModel.GenericAliasName;
            }
            else if (!ViewModel.doSampleUpdate)
            {
                this.pageHeader.Text = this.pageHeader.Text + "  " + this.ViewModel.SampleAlias;
            }

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
                var modalSampleClose = Window.Current.Content as Template10.Controls.ModalDialog;
                var viewSampleClose = modalSampleClose.ModalContent as SampleDialog;
                modalSampleClose.ModalContent = viewSampleClose;
                modalSampleClose.IsModal = false;
            });
        }

        private void sampleBackButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //Make sure to delete earthmat, station and location records.
            if (isAQuickSample)
            {
                ViewModel.DeleteCascadeOnQuickSample(parentViewModel);
            }
            
            CloseControl();
        }

        #endregion

        #region SAVE
        private void sampleSaveButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.sampleSaveButton.Focus(FocusState.Programmatic);

        }


        #endregion

        /// <summary>
        /// Will be triggered when the cancel icon is tapped and will revert it's icon.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PurposeValueCheck_Tapped(object sender, TappedRoutedEventArgs e)
        {
            IList<object> selectedValues = this.samplePurposesValues.SelectedItems;
            if (selectedValues.Count > 0)
            {
                foreach (object values in selectedValues)
                {
                    ViewModel.RemoveSelectedPurpose(values);
                }
            }
        }

    }
}
