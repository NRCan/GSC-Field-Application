using GSCFieldApp.ViewModels;
using Template10.Common;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
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
