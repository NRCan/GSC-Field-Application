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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace GSCFieldApp.Views
{
    public sealed partial class MineralDialog : UserControl
    {
        public MineralViewModel MineralVM { get; set; }
        public FieldNotes parentViewModel { get; set; }

        public MineralDialog(FieldNotes inDetailViewModel)
        {
            parentViewModel = inDetailViewModel;

            this.InitializeComponent();

            MineralVM = new MineralViewModel(inDetailViewModel);
            this.Loading += MineralDialog_Loading;
            this.mineralSaveButton.GotFocus += MineralSaveButton_GotFocus;
        }


        private void MineralDialog_Loading(FrameworkElement sender, object args)
        {
            //Fill automatically the earthmat dialog if an edit is asked by the user.
            if (parentViewModel.GenericTableName == Dictionaries.DatabaseLiterals.TableMineral && MineralVM.doMineralUpdate)
            {
                this.MineralVM.AutoFillDialog(parentViewModel);
                this.pageHeader.Text = this.pageHeader.Text + "  " + parentViewModel.GenericAliasName;
            }
            else if (!MineralVM.doMineralUpdate)
            {
                this.pageHeader.Text = this.pageHeader.Text + "  " + this.MineralVM.MineralAlias;
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
                var modalMineralClose = Window.Current.Content as Template10.Controls.ModalDialog;
                var viewMineralClose = modalMineralClose.ModalContent as MineralDialog;
                modalMineralClose.ModalContent = viewMineralClose;
                modalMineralClose.IsModal = false;
            });
        }

        private void mineralBackButton_Tapped(object sender, TappedRoutedEventArgs e)
        {

            CloseControl();
        }

        #endregion

        #region SAVE
        private void mineralSaveButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.mineralSaveButton.Focus(FocusState.Programmatic);

        }
        private void MineralSaveButton_GotFocus(object sender, RoutedEventArgs e)
        {
            MineralVM.SaveDialogInfo();
            CloseControl();
        }


        #endregion


    }
}
