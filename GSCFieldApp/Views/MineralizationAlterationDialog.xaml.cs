using GSCFieldApp.Models;
using GSCFieldApp.ViewModels;
using System.Collections.Generic;
using Template10.Common;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace GSCFieldApp.Views
{
    public sealed partial class MineralizationAlterationDialog : UserControl
    {
        public MineralizationAlterationViewModel MAViewModel { get; set; }
        public FieldNotes parentViewModel { get; set; }

        public MineralizationAlterationDialog(FieldNotes inDetailVM)
        {

            parentViewModel = inDetailVM;

            this.InitializeComponent();

            MAViewModel = new MineralizationAlterationViewModel(inDetailVM);
            this.Loading += MineralAlterationDialog_Loading;
            this.mineralAltSaveButton.GotFocus += MineralAltSaveButton_GotFocus;

        }



        /// <summary>
        /// When loading is being conducted, refresh dialog header with mineral alt alias
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void MineralAlterationDialog_Loading(FrameworkElement sender, object args)
        {
            //Fill automatically the earthmat dialog if an edit is asked by the user.
            if (parentViewModel.GenericTableName == Dictionaries.DatabaseLiterals.TableMineralAlteration && MAViewModel.doMineralAltUpdate)
            {
                this.MAViewModel.AutoFillDialog(parentViewModel);
                this.pageHeader.Text = this.pageHeader.Text + "  " + parentViewModel.GenericAliasName;
            }
            else if (!MAViewModel.doMineralAltUpdate)
            {
                this.pageHeader.Text = this.pageHeader.Text + "  " + this.MAViewModel.MineralAltAlias;
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
                var modalMAClose = Window.Current.Content as Template10.Controls.ModalDialog;
                var viewMAClose = modalMAClose.ModalContent as MineralizationAlterationDialog;
                modalMAClose.ModalContent = viewMAClose;
                modalMAClose.IsModal = false;
            });
        }

        private void mineralAltBackButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            CloseControl();
        }

        #endregion

        #region SAVE
        private void mineralAltSaveButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.mineralAltSaveButton.Focus(FocusState.Programmatic);

        }
        private void MineralAltSaveButton_GotFocus(object sender, RoutedEventArgs e)
        {
            MAViewModel.SaveDialogInfo();
            CloseControl();
        }

        #endregion

        /// <summary>
        /// Will be triggered when the cancel icon is tapped and will revert it's icon.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConcatValueCheck_Tapped(object sender, TappedRoutedEventArgs e)
        {
            IList<object> selectedValues = this.MineralAltDistConcat.SelectedItems;
            if (selectedValues.Count > 0)
            {
                foreach (object values in selectedValues)
                {
                    MAViewModel.RemoveSelectedDistribution(values);
                }
            }
        }


    }
}
