using GSCFieldApp.Models;
using GSCFieldApp.ViewModels;
using System.Collections.Generic;
using System.ComponentModel;
using Template10.Common;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace GSCFieldApp.Views
{
    public sealed partial class EnvironmentDialog : UserControl
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public FieldNotes parentEnvironmentReport;
        public EnvironmentViewModel EnvViewModel { get; set; }

        private TranslateTransform dragTransform;
        private UIElement currentDraggedElement;

        public EnvironmentDialog(FieldNotes inParentReport)
        {
            if (inParentReport != null)
            {
                parentEnvironmentReport = inParentReport;
            }

            this.InitializeComponent();
            this.EnvViewModel = new EnvironmentViewModel(inParentReport);

            this.Loading += environmentDialog_Loading;

            //#258 bringing back some old patch on save button
            this.envSaveButton.GotFocus -= EnvSaveButton_GotFocus;
            this.envSaveButton.GotFocus += EnvSaveButton_GotFocus;
            dragTransform = new TranslateTransform();
        }

        private void EnvSaveButton_GotFocus(object sender, RoutedEventArgs e)
        {
            this.envSaveButton.GotFocus -= EnvSaveButton_GotFocus;
            EnvViewModel.SaveDialogInfo();
            CloseControl();
        }

        private void environmentDialog_Loading(FrameworkElement sender, object args)
        {
            //Fill automatically the earthmat dialog if an edit is asked by the user.
            if (parentEnvironmentReport.GenericTableName == Dictionaries.DatabaseLiterals.TableEnvironment && EnvViewModel.doEnvironmentUpdate)
            {
                this.EnvViewModel.AutoFillDialog(parentEnvironmentReport);
                this.pageHeader.Text = this.pageHeader.Text + "  " + parentEnvironmentReport.GenericAliasName;
            }
            else if (!EnvViewModel.doEnvironmentUpdate)
            {
                this.pageHeader.Text = this.pageHeader.Text + "  " + this.EnvViewModel.Alias;
            }
            else
            {
                this.pageHeader.Text = this.pageHeader.Text + "  " + this.EnvViewModel.Alias;
            }
        }

        #region Dragging Implementation

        private void UIElement_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            if (sender is UIElement element)
            {
                currentDraggedElement = element;
                if (element.RenderTransform is TranslateTransform transform)
                {
                    dragTransform = transform;
                }
                else
                {
                    dragTransform = new TranslateTransform();
                    element.RenderTransform = dragTransform;
                }
            }
        }

        private void UIElement_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (currentDraggedElement != null && dragTransform != null)
            {
                dragTransform.X += e.Delta.Translation.X;
                dragTransform.Y += e.Delta.Translation.Y;
            }
        }

        private void UIElement_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                // Save the current position
                if (element.RenderTransform is TranslateTransform transform)
                {
                    var settings = ApplicationData.Current.LocalSettings;

                    // Save X and Y positions using the element's name as a key
                    if (!string.IsNullOrEmpty(element.Name))
                    {
                        settings.Values[$"{element.Name}_X"] = transform.X;
                        settings.Values[$"{element.Name}_Y"] = transform.Y;
                    }
                }
            }

            currentDraggedElement = null;
        }

        #endregion

        #region SAVE

        private void envSaveButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.envSaveButton.Focus(FocusState.Keyboard);
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
                var modal = Window.Current.Content as Template10.Controls.ModalDialog;
                var view = modal.ModalContent as EarthmatDialog;
                modal.ModalContent = view;
                modal.IsModal = false;
            });
        }

        private void envBackButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            CloseControl();
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
                    EnvViewModel.RemoveSelectedValue(values, parentListView.Name);
                }
            }
        }

        #endregion

        private void Element_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (sender is UIElement element)
            {
                // Get the current transform or create a new one
                if (element.RenderTransform is TranslateTransform transform)
                {
                    transform.X += e.Delta.Translation.X;
                    transform.Y += e.Delta.Translation.Y;
                }
                else
                {
                    var newTransform = new TranslateTransform
                    {
                        X = e.Delta.Translation.X,
                        Y = e.Delta.Translation.Y
                    };
                    element.RenderTransform = newTransform;
                }
            }
        }
    }
}
