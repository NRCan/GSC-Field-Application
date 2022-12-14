using System;
using System.IO;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Template10.Common;
using Template10.Controls;
using GSCFieldApp.ViewModels;
using System.Threading.Tasks;
using GSCFieldApp.Services.DatabaseServices;
using GSCFieldApp.Models;
using Windows.Storage;

namespace GSCFieldApp.Views
{
    public sealed partial class FieldBookDialog : UserControl
    {
        public FieldBookDialogViewModel ViewModel { get; set; }

        public FieldBooks incomingProject;
        public bool isForInit = false;

        //Events
        public static event EventHandler deleteAllLayers; //This event is triggered when a factory reset is requested. Will need to wipe layers.
        public static event EventHandler newFieldBookSaved; //This event is triggered when a new field book has been saved on system

        //Local settings
        readonly DataLocalSettings localSetting = new DataLocalSettings();

        public FieldBookDialog(FieldBooks inProject, bool init = false)
        {

            if (inProject != null)
            {
                incomingProject = inProject;
            }

            if (init)
            {
                isForInit = true;
            }
            this.InitializeComponent();

            this.ViewModel = new FieldBookDialogViewModel();

            this.Loading += UserInfoPart_Loading;
            this.userInfoSaveButton.GotFocus -= userInfoSaveButton_GotFocusAsync;
            this.userInfoSaveButton.GotFocus += userInfoSaveButton_GotFocusAsync; //bug 306
            
            
        }

        private void UserInfoPart_Loading(FrameworkElement sender, object args)
        {
            //Get information to automatically fill the dialog if data already exists (report page vs new station)
            if (incomingProject != null)
            {
                this.ViewModel.AutoFillDialog(incomingProject);
            }

        }

        private void CloseClicked(object sender, RoutedEventArgs e)
        {
            CloseControl();
        }

        /// <summary>
        /// Keep upper case the geolcode.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void UserCodeTextBox_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            int selectionStart = sender.SelectionStart;
            sender.Text = sender.Text.ToUpper();
            sender.SelectionStart = selectionStart;
            sender.SelectionLength = 0;
        }

        

        #region SAVE

        private void saveUserInfo_TappedAsync(object sender, TappedRoutedEventArgs e)
        {
            this.userInfoSaveButton.Focus(FocusState.Programmatic);
        }

        private async void userInfoSaveButton_GotFocusAsync(object sender, RoutedEventArgs e)
        {
            this.userInfoSaveButton.GotFocus -= userInfoSaveButton_GotFocusAsync;

            await ViewModel.SetModel();
            bool isMetadataValid = ViewModel.Model.isValid;

            if (ViewModel.Model.isValid && !isForInit && ViewModel.existingUserDetail == null)
            {
                //Update settings with new selected project
                ApplicationData.Current.SignalDataChanged();

                //Create a new database
                DataAccess accessData = new DataAccess();
                await accessData.CreateDatabaseFromResource();

                await ViewModel.SaveDialogInfoAsync();
                CloseControl();

                EventHandler newFieldBookSavedEvent = newFieldBookSaved;
                if (newFieldBookSavedEvent != null)
                {
                    newFieldBookSavedEvent(this, null);
                }

            }
            else if (isMetadataValid && isForInit)
            {
                await ViewModel.SaveDialogInfoAsync();
                CloseControl();
            }
            else if (ViewModel.Model.isValid && !isForInit && ViewModel.existingUserDetail != null)
            {
                await ViewModel.SaveDialogInfoAsync();
                CloseControl();

                EventHandler newFieldBookSavedEvent = newFieldBookSaved;
                if (newFieldBookSavedEvent != null)
                {
                    newFieldBookSavedEvent(this, null);
                }
            }

            if (!ViewModel.Model.isValid)
            {

                // Language localization using Resource.resw
                var loadLocalization = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();

                ContentDialog mandatoryFields = new ContentDialog()
                {
                    Title = loadLocalization.GetString("MandatoryFieldWarningTitle"),
                    Content = loadLocalization.GetString("MandatoryFieldWarningContent"),
                    PrimaryButtonText = loadLocalization.GetString("MandatoryFieldWarningOk"),
                };

                await mandatoryFields.ShowAsync();
                
            }
            
            this.userInfoSaveButton.GotFocus += userInfoSaveButton_GotFocusAsync;
        }

        #endregion

        #region CLOSE
        /// <summary>
        /// Will close the modal dialog.
        /// </summary>
        public void CloseControl()
        {

            //Get the current window and cast it to a UserInfoPart ModalDialog and shut it down.
            WindowWrapper.Current().Dispatcher.Dispatch(() =>
            {
                var modal = Window.Current.Content as ModalDialog;
                var view = modal.ModalContent as FieldBookDialog;
                modal.ModalContent = view;
                modal.IsModal = false;
            });

            //Set header visibility init
            Services.DatabaseServices.DataLocalSettings localSetting = new DataLocalSettings();
            localSetting.InitializeHeaderVisibility();

        }


        /// <summary>
        /// Launched when user wants to close dialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void userInfoCancelButton_TappedAsync(object sender, TappedRoutedEventArgs e)
        {
            CloseControl();

            //If user tries to exit at init of the app, close it
            if (isForInit)
            {
                await DeleteInitBook();
            }

           

        }

        /// <summary>
        /// Will erase the initial fieldbook from local state.
        /// </summary>
        /// <returns></returns>
        public async Task DeleteInitBook()
        {
            //Get folder
            DataAccess da = new DataAccess();
            StorageFolder currentCreatedFieldBookFolder = await StorageFolder.GetFolderFromPathAsync(da.ProjectPath);

            //Initiate a task with a timeout of a minute, in case field book is still in use in some other windows
            //this is true when a user creates a field book, add tpks, goes back to field book page, deletes it, then recreate one
            //then closes the field book dialog without saving, for some reason there is a lock that happen which prevent the folder being
            //deleted. We only need all the code to have finished and closed the database connection.
            int timeout = 60000;
            Task t = DeleteABook(currentCreatedFieldBookFolder); //delete a field book
            if (await Task.WhenAny(t, Task.Delay(timeout)) == t)
            {
                //Delete local settings
                Services.DatabaseServices.DataLocalSettings localSettings = new DataLocalSettings();
                localSettings.WipeUserInfoSettings();
            }

        }

        /// <summary>
        /// Will initiate a field book folder delete in a while loop as long as the file can't be deleted.
        /// This method should be called with a delay timeout because it uses an almost infinit loop.
        /// </summary>
        /// <param name="inSF"></param>
        /// <returns></returns>
        public async Task<bool> DeleteABook(StorageFolder inSF)
        {
            bool fileStillExists = Directory.Exists(inSF.Path);
            while (fileStillExists)
            {
                try
                {
                    await inSF.DeleteAsync(StorageDeleteOption.PermanentDelete);
                }
                catch (Exception)
                {

                }
                
                fileStillExists = Directory.Exists(inSF.Path); ;
            }

            return fileStillExists;
            
        }


        #endregion


    }
}
