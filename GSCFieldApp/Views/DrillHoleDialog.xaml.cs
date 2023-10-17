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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace GSCFieldApp.Views
{
    public sealed partial class DrillHoleDialog : UserControl
    {
        public DrillHoleViewModel drillViewModel { get; set; }
        public FieldNotes parentDrillReport { get; set; }

        public delegate void drillCloseWithoutSaveEventHandler(object sender); //A delegate for execution events
        public event drillCloseWithoutSaveEventHandler drillClosed; //This event is triggered when a save has been done on station table.

        public FieldLocation mapPosition { get; set; }

        public DrillHoleDialog(FieldNotes inParentReport)
        {
            
            if (inParentReport != null)
            {
                parentDrillReport = inParentReport;
            }

            this.InitializeComponent();
            this.drillViewModel = new DrillHoleViewModel(inParentReport);
            this.Loading += DrillHoleDialog_Loading;

            //#258 bringing back some old patch on save button
            this.drillSaveButton.GotFocus -= DrillSaveButton_GotFocus;
            this.drillSaveButton.GotFocus += DrillSaveButton_GotFocus;

            
        }

        private void DrillHoleDialog_Loading(FrameworkElement sender, object args)
        {
            //Fill automatically the earthmat dialog if an edit is asked by the user.
            if (parentDrillReport.GenericTableName == Dictionaries.DatabaseLiterals.TableDrillHoles && drillViewModel.doDrillHoleUpdate)
            {
                this.drillViewModel.AutoFillDialog(parentDrillReport);
                this.pageHeader.Text = this.pageHeader.Text + "  " + parentDrillReport.GenericAliasName;
            }
            else if (!drillViewModel.doDrillHoleUpdate)
            {
                this.pageHeader.Text = this.pageHeader.Text + "  " + this.drillViewModel.DrillIDName;
            }
            else
            {
                this.pageHeader.Text = this.pageHeader.Text + "  " + this.drillViewModel.DrillIDName;
            }
        }

        #region EVENTS

        /// <summary>
        /// On button focus, will commit a save
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DrillSaveButton_GotFocus(object sender, RoutedEventArgs e)
        {
            this.drillSaveButton.GotFocus -= DrillSaveButton_GotFocus;
            drillViewModel.SaveDialogInfoAsync();
            CloseControl();
        }

        /// <summary>
        /// Will close the form and make sure to delete perviously added location
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void drillBackButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //Prevent delete on back button when in edit mode
            if (!this.drillViewModel.doDrillHoleUpdate)
            {
                this.drillViewModel.DeleteCascadeOnQuickDrillHole(parentDrillReport);
            }
            
            CloseControl();
        }

        /// <summary>
        /// Will save the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void drillSaveButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //Trigger focus on save button
            this.drillSaveButton.Focus(FocusState.Keyboard);
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
                    this.drillViewModel.RemoveSelectedValue(values, parentListView.Name);
                }
            }
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
                var modalDHClose = Window.Current.Content as Template10.Controls.ModalDialog;
                var viewDHClose = modalDHClose.ModalContent as DrillHoleDialog;
                modalDHClose.ModalContent = viewDHClose;
                modalDHClose.IsModal = false;
            });


        }

        #endregion


    }
}
