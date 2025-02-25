using GSCFieldApp.Models;
using GSCFieldApp.ViewModels;
using Template10.Common;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace GSCFieldApp.Views
{
    public sealed partial class FossilDialog : UserControl
    {
        public FossilViewModel fossilModel { get; set; }
        public FieldNotes fossilParentViewModel { get; set; }

        private TranslateTransform dragTransform;
        private UIElement currentDraggedElement;

        public FossilDialog(FieldNotes inDetailViewModel)
        {
            fossilParentViewModel = inDetailViewModel;
            fossilModel = new FossilViewModel(fossilParentViewModel);
            this.InitializeComponent();

            this.Loading += FossilDialog_Loading;

            //#258 bringing back some old patch on save button
            this.fossilSaveButton.GotFocus -= FossilSaveButton_GotFocus;
            this.fossilSaveButton.GotFocus += FossilSaveButton_GotFocus;
            dragTransform = new TranslateTransform();
        }

        private void FossilSaveButton_GotFocus(object sender, RoutedEventArgs e)
        {
            this.fossilSaveButton.GotFocus -= FossilSaveButton_GotFocus;
            fossilModel.SaveDialogInfo();
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
            this.fossilSaveButton.Focus(FocusState.Keyboard);

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
