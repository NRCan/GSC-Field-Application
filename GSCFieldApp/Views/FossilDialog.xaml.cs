﻿using GSCFieldApp.ViewModels;
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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace GSCFieldApp.Views
{
    public sealed partial class FossilDialog : UserControl
    {
        public FossilViewModel fossilModel { get; set; }
        public FieldNotes fossilParentViewModel { get; set; }

        public FossilDialog(FieldNotes inDetailViewModel)
        {
            fossilParentViewModel = inDetailViewModel;
            fossilModel = new FossilViewModel(fossilParentViewModel);
            this.InitializeComponent();

            this.Loading += FossilDialog_Loading;
            this.fossilSaveButton.GotFocus += FossilSaveButton_GotFocus;
        }

        #region EVENTS
        private void FossilDialog_Loading(FrameworkElement sender, object args)
        {
            //Fill automatically the earthmat dialog if an edit is asked by the user.
            if (fossilParentViewModel.GenericTableName == Dictionaries.DatabaseLiterals.TableFossil && fossilModel.doFossilUpdate)
            {
                this.fossilModel.AutoFillDialog(fossilParentViewModel);
                this.pageHeader.Text = this.pageHeader.Text + "  " + fossilParentViewModel.GenericAliasName;
            }
            else if (!fossilModel.doFossilUpdate)
            {
                this.pageHeader.Text = this.pageHeader.Text + "  " + this.fossilModel.FossilName;
            }
        }

        private void fossilBackButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            CloseControl();
        }

        private void fossilSaveButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.fossilSaveButton.Focus(FocusState.Programmatic);

        }
        private void FossilSaveButton_GotFocus(object sender, RoutedEventArgs e)
        {
            fossilModel.SaveDialogInfo();
            CloseControl();
        }
        #endregion

        #region CLOSE
        /// <summary>
        /// Will close the modal dialog.
        /// </summary>
        public void CloseControl()
        {

            //Get the current window and cast it to a DeleteDialog ModalDialog and shut it down.
            WindowWrapper.Current().Dispatcher.Dispatch(() =>
            {
                var modalFossilClose = Window.Current.Content as Template10.Controls.ModalDialog;
                var viewFossilClose = modalFossilClose.ModalContent as FossilDialog;
                modalFossilClose.ModalContent = viewFossilClose;
                modalFossilClose.IsModal = false;
            });
        }
        #endregion
    }
}
