using GSCFieldApp.Models;
using GSCFieldApp.ViewModels;
using Template10.Common;
using Windows.UI.Xaml;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace GSCFieldApp.Views
{
    public sealed partial class PaleoflowDialog : UserControl
    {
        public PaleoflowViewModel pflowModel { get; set; }
        public FieldNotes pflowParentViewModel { get; set; }

        public delegate void pflowCloseWithoutSaveEventHandler(object sender); //A delegate for execution events
        public event pflowCloseWithoutSaveEventHandler pflowClosed; //This event is triggered when a save has been done on station table.

        private TranslateTransform dragTransform;
        private UIElement currentDraggedElement;

        public PaleoflowDialog(FieldNotes inDetailViewModel)
        {
            pflowParentViewModel = inDetailViewModel;
            pflowModel = new PaleoflowViewModel(inDetailViewModel);
            this.InitializeComponent();

            this.Loading += Paleoflow_Loading;

            //#258 bringing back some old patch on save button
            this.pflowSaveButton.GotFocus -= PflowSaveButton_GotFocus;
            this.pflowSaveButton.GotFocus += PflowSaveButton_GotFocus;

            dragTransform = new TranslateTransform();
        }

        private void PflowSaveButton_GotFocus(object sender, RoutedEventArgs e)
        {
            this.pflowSaveButton.GotFocus -= PflowSaveButton_GotFocus;
            pflowModel.SaveDialogInfo();
            CloseControl();
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

            if (pflowClosed != null)
            {
                pflowClosed(this);
            }
        }
        #endregion

        #region EVENTS


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
            this.pflowSaveButton.Focus(FocusState.Keyboard);

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
