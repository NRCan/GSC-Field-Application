using GSCFieldApp.Models;
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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace GSCFieldApp.Views
{
    public sealed partial class PaleoflowDialog: UserControl
    {
        public PaleoflowViewModel pflowModel { get; set; }
        public FieldNotes pflowParentViewModel { get; set; }

        public PaleoflowDialog(FieldNotes inDetailViewModel)
        {
            pflowParentViewModel = inDetailViewModel;
            pflowModel = new PaleoflowViewModel(inDetailViewModel);
            this.InitializeComponent();

            this.Loading += Paleoflow_Loading;
            this.pflowSaveButton.GotFocus += PflowSaveButton_GotFocus;
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
                var modalpflowClose = Window.Current.Content as Template10.Controls.ModalDialog;
                var viewPflowClose = modalpflowClose.ModalContent as PaleoflowDialog;
                modalpflowClose.ModalContent = viewPflowClose;
                modalpflowClose.IsModal = false;
            });
        }
        #endregion

        #region EVENTS

        private void PflowSaveButton_GotFocus(object sender, RoutedEventArgs e)
        {
            pflowModel.SaveDialogInfo();
            CloseControl();
        }

        private void Paleoflow_Loading(FrameworkElement sender, object args)
        {
            //Fill automatically the earthmat dialog if an edit is asked by the user.
            if (pflowParentViewModel.GenericTableName == Dictionaries.DatabaseLiterals.TablePFlow && pflowModel.doPflowUpdate)
            {
                this.pflowModel.AutoFillDialog(pflowParentViewModel);
                this.pageHeader.Text = this.pageHeader.Text + "  " + pflowParentViewModel.GenericAliasName;
            }
            else if (!pflowModel.doPflowUpdate)
            {
                this.pageHeader.Text = this.pageHeader.Text + "  " + this.pflowModel.PflowName;
            }
        }

        /// <summary>
        /// Will close the dialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pflowBackButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            CloseControl();
        }

        /// <summary>
        /// Will close and save the dialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pflowSaveButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.pflowSaveButton.Focus(FocusState.Programmatic);

        }


        #endregion


    }
}
