﻿using GSCFieldApp.Models;
using GSCFieldApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Template10.Common;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace GSCFieldApp.Views
{
    public sealed partial class SampleDialog : UserControl
    {
        public SampleViewModel ViewModel { get; set; }
        public FieldNotes parentViewModel { get; set; }
        public bool isAQuickSample = false;

        public delegate void sampCloseWithoutSaveEventHandler(object sender); //A delegate for execution events
        public event sampCloseWithoutSaveEventHandler sampClosed; //This event is triggered when a save has been done on station table.


        public SampleDialog(FieldNotes inDetailViewModel, bool isQuickSample)
        {
            parentViewModel = inDetailViewModel;
            isAQuickSample = isQuickSample;

            this.InitializeComponent();

            ViewModel = new SampleViewModel(inDetailViewModel);
            this.Loading += SampleDialog_LoadingAsync;

            //#258 bringing back some old patch on save button
            this.sampleSaveButton.GotFocus -= SampleSaveButton_GotFocus;
            this.sampleSaveButton.GotFocus += SampleSaveButton_GotFocus;

        }

        private void SampleSaveButton_GotFocus(object sender, RoutedEventArgs e)
        {
            this.sampleSaveButton.GotFocus -= SampleSaveButton_GotFocus;
            ViewModel.SaveDialogInfo();
            CloseControl();
        }

        /// <summary>
        /// Will fill the dialog with known information
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void SampleDialog_LoadingAsync(FrameworkElement sender, object args)
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

                //Display alert to take a blank or a duplicate for surficial geologist
                if (this.ViewModel.DuplicateReminder())
                {
                    var loadLocalization = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();

                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        ContentDialog reminderDialog = new ContentDialog()
                        {
                            Title = loadLocalization.GetString("SampleDialogDuplicateReminderTitle"),
                            Content = loadLocalization.GetString("SampleDialogDuplicateReminderContent"),
                            PrimaryButtonText = loadLocalization.GetString("GenericDialog_ButtonOK")
                        };
                        reminderDialog.Style = (Style)Application.Current.Resources["WarningDialog"];
                        await Services.ContentDialogMaker.CreateContentDialogAsync(reminderDialog, false);

                    }).AsTask();

                }

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

            if (sampClosed != null)
            {
                sampClosed(this);
            }

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
            this.sampleSaveButton.Focus(FocusState.Keyboard);
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

        private void SampleLength_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox senderBox = sender as TextBox;
            if (senderBox.Text != null && senderBox.Text != string.Empty)
            {
                CalculateTo();
            }
        }

        /// <summary>
        /// Will recalculate the To value based on From or Length values
        /// </summary>
        private void CalculateTo()
        {
            //Recalculate new To value
            bool resultFrom = double.TryParse(this.SampleFrom.Text, out double doubleFrom);
            bool resultLength = double.TryParse(this.SampleLength.Text, out double doubleLength);

            if (resultFrom && resultLength)
            {
                this.SampleTo.Text = (doubleFrom + doubleLength).ToString();
            }
            else
            {
                this.SampleTo.Text = "0";
            }

            this.ViewModel.SampleCoreTo = this.SampleTo.Text;
        }

        private void SampleFrom_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox senderBox = sender as TextBox;
            if (senderBox.Text != null && senderBox.Text != string.Empty)
            {
                CalculateTo();
            }
        }
    }
}
