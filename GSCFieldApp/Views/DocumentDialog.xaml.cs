using GSCFieldApp.Models;
using GSCFieldApp.Services.DatabaseServices;
using GSCFieldApp.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using Template10.Common;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;


// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace GSCFieldApp.Views
{
    public sealed partial class DocumentDialog : UserControl
    {
        public DocumentViewModel DocViewModel { get; set; }
        public FieldNotes parentViewModel { get; set; }
        public bool isQuickPhoto = false;

        //Local settings
        readonly DataLocalSettings lSetting = new DataLocalSettings();

        public delegate void documentCloseWithoutSaveEventHandler(object sender); //A delegate for execution events
        public event documentCloseWithoutSaveEventHandler documentClosed; //This event is triggered when a save has been done on station table.

        private TranslateTransform dragTransform;
        private UIElement currentDraggedElement;

        public DocumentDialog(FieldNotes inDetailViewModel, FieldNotes stationSummaryID, bool quickPhoto)
        {
            parentViewModel = inDetailViewModel;
            isQuickPhoto = quickPhoto;

            this.InitializeComponent();

            DocViewModel = new DocumentViewModel(parentViewModel, stationSummaryID);

            this.Loading += DocumentDialog_Loading;
            this.Loaded += DocumentDialog_Loaded;

            //#258 bringing back some old patch on save button
            this.documentSaveButton.GotFocus -= DocumentSaveButton_GotFocus;
            this.documentSaveButton.GotFocus += DocumentSaveButton_GotFocus;
            dragTransform = new TranslateTransform();

        }

        private void DocumentSaveButton_GotFocus(object sender, RoutedEventArgs e)
        {
            this.documentSaveButton.GotFocus -= DocumentSaveButton_GotFocus;

            //Will save and close control only if a save can be performed.
            bool didSave = DocViewModel.SaveDialogInfoAsync();
            if (didSave)
            {
                CloseControl();
            }
        }

        private void DocumentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            DocViewModel.hasInitialized = true;

            if (parentViewModel.GenericTableName == Dictionaries.DatabaseLiterals.TableDocument && DocViewModel.doDocumentUpdate)
            {
                this.DocViewModel.AutoFillDialogAsync(parentViewModel);
            }
        }

        /// <summary>
        /// Loading event to fill in the dialog from the selected report
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void DocumentDialog_Loading(FrameworkElement sender, object args)
        {
            if (parentViewModel.GenericTableName == Dictionaries.DatabaseLiterals.TableDocument && DocViewModel.doDocumentUpdate)
            {
                //this.DocViewModel.AutoFillDialogAsync(parentViewModel);
                this.pageHeader.Text = parentViewModel.GenericAliasName;
            }
            else if (!DocViewModel.doDocumentUpdate)
            {
                this.pageHeader.Text = this.DocViewModel.DocumentName;
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
        private void documentSaveButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.documentSaveButton.Focus(FocusState.Keyboard);
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
                var view = modal.ModalContent as DocumentDialog;
                modal.ModalContent = view;
                modal.IsModal = false;
            });

            if (documentClosed != null)
            {
                documentClosed(this);
            }

        }

        private void documentBackButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //Make sure to delete earthmat, station and location records.
            if (isQuickPhoto)
            {
                DocViewModel.DeleteCascadeOnQuickPhoto(parentViewModel);
            }

            //Make sure to delete captured photo if there was one.
            DocViewModel.DeleteCapturePhoto();

            CloseControl();
        }





        #endregion

        #region PHOTO snapshot
        /// <summary>
        /// Will start camera app to take pictures.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void documentPhotoButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            DocViewModel.TakeSnapshotAsync();
        }

        /// <summary>
        /// Will open the photo if it's tapped
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void imagePreview_Tapped(object sender, TappedRoutedEventArgs e)
        {

            await Windows.System.Launcher.LaunchFileAsync(DocViewModel._documentPhotoFile);
        }

        #endregion

        private void Image_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            //Check if image exists
            if (File.Exists(DocViewModel.DocumentPhotoPath))
            {
                //Create a fallback image
                Image bitImage = (Image)sender;
                BitmapImage snapshotFailed = new BitmapImage(new Uri(DocViewModel.DocumentPhotoPath));
                bitImage.Width = 300;
                bitImage.Source = snapshotFailed;
            }
            else
            {
                //Create a fallback image
                Image bitImage = (Image)sender;
                BitmapImage failImageCatch = new BitmapImage(new Uri("ms-appx:///Assets/mosquito.png"));
                bitImage.Width = 300;
                bitImage.Source = failImageCatch;
            }


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
                    DocViewModel.RemoveSelectedValue(values, parentListView.Name);
                }
            }
        }
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
