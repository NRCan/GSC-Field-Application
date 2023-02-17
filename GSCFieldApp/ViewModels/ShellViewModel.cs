using System;
using System.Collections.Generic;
using Windows.Storage;
using Template10.Mvvm;
using GSCFieldApp.Services.DatabaseServices;
using Windows.UI.Xaml.Controls;
using GSCFieldApp.Services.FileServices;
using Windows.UI.Xaml;
using GSCFieldApp.Dictionaries;

namespace GSCFieldApp.ViewModels
{
    public class ShellViewModel: ViewModelBase
    {
        //UI
        private bool _shellEnableMapCommand = true;
        private bool _shellEnableNoteCommand = true;
        private bool _shellEnableBackupCommand = true;

        //Delegates and events

        //Settings
        readonly DataAccess accessData = new DataAccess();
        readonly DataLocalSettings localSetting = new DataLocalSettings();
        readonly ApplicationDataContainer currentLocalSettings = ApplicationData.Current.LocalSettings;

        //Properties
        public bool ShellEnableMapCommand { get { return _shellEnableMapCommand; } set { _shellEnableMapCommand = value; } }
        public bool ShellEnableNoteCommand { get { return _shellEnableNoteCommand; } set { _shellEnableNoteCommand = value; } }
        public bool ShellEnableBackupCommand { get { return _shellEnableBackupCommand; } set { _shellEnableBackupCommand = value; } }

        //Init.
        public ShellViewModel()
        {
            //Init
            GSCFieldApp.ViewModels.FieldBooksPageViewModel.fieldBooksUpdate += FieldBooksPageViewModel_fieldBooksUpdate;
        }

        /// <summary>
        /// If there is any field books, enable commands, else disable them
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FieldBooksPageViewModel_fieldBooksUpdate(object sender, bool e)
        {
            if (e)
            {
                _shellEnableMapCommand = _shellEnableNoteCommand = _shellEnableBackupCommand = true;
            }
            else
            {
                _shellEnableMapCommand = _shellEnableNoteCommand = _shellEnableBackupCommand = false;
            }

            RaisePropertyChanged("ShellEnableMapCommand");
            RaisePropertyChanged("ShellEnableNoteCommand");
            RaisePropertyChanged("ShellEnableBackupCommand");

        }

        /// <summary>
        /// Will save a field work copy, from local state folder to the my document folder of current user and will prompt a message when done.
        /// </summary>
        public async void QuickBackupAsync()
        {
            //Variables
            List<StorageFile> FilesToBackup = new List<StorageFile>();
            FileServices fs = new FileServices();
            int photoCount = 0;
            bool hasPhotos = false; // Will prevent warning message of no backuped photo showing when no photos were taken on the device
            DateTimeOffset youngestPhoto = DateTimeOffset.MinValue; //Default for current 
            DateTimeOffset inMemoryYoungestPhoto = DateTimeOffset.MinValue;
            string projectName = string.Empty;
            if (localSetting.GetSettingValue(Dictionaries.ApplicationLiterals.KeywordBackupPhotoYoungest) != null)
            {
                inMemoryYoungestPhoto = (DateTimeOffset)localSetting.GetSettingValue(Dictionaries.ApplicationLiterals.KeywordBackupPhotoYoungest);
            }
            if (localSetting.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoPName) != null)
            {
                projectName = localSetting.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoPName).ToString();
            }

            //calculate new name for output database in the archive
            string uCode = currentLocalSettings.Containers[ApplicationLiterals.LocalSettingMainContainer].Values[Dictionaries.DatabaseLiterals.FieldUserInfoUCode].ToString();
            FileServices fService = new FileServices();
            string newName = fService.CalculateDBCopyName(uCode) + DatabaseLiterals.DBTypeSqlite;

            //Iterate through field book folder files
            string _fieldBooLocalPath = localSetting.GetSettingValue(Dictionaries.ApplicationLiterals.KeywordFieldProject).ToString();
            StorageFolder fieldBook = await StorageFolder.GetFolderFromPathAsync(_fieldBooLocalPath);

            //Get a list of files from field book
            IReadOnlyList<StorageFile> fieldBookPhotosRO = await fieldBook.GetFilesAsync();

            //Get a list of photos only
            foreach (StorageFile files in fieldBookPhotosRO)
            {
                //Get databases
                if (files.Name.ToLower().Contains(DatabaseLiterals.DBTypeSqlite) && files.Name.Contains(Dictionaries.DatabaseLiterals.DBName))
                {
                    //Do not get logging files
                    if (!files.Name.ToLower().Contains(DatabaseLiterals.DBTypeGeopackageSHM) && !files.Name.ToLower().Contains(DatabaseLiterals.DBTypeGeopackageWal))
                    {
                        FilesToBackup.Add(files);
                    }
                    
                }

                //Get photos
                if (files.Name.ToLower().Contains(".jpg"))
                {
                    hasPhotos = true; 
                    if (files.DateCreated >= inMemoryYoungestPhoto)
                    {
                        //Copy all photos
                        FilesToBackup.Add(files);

                        //Keep youngest photo for futur backup
                        if (files.DateCreated > youngestPhoto)
                        {
                            youngestPhoto = files.DateCreated;
                        }

                        photoCount++;
                    }


                }
            }

            //If any photos needs to be copied, else show warning
            if (FilesToBackup.Count > 1)
            {
                //Copy database and rename it
                List<StorageFile> newList = new List<StorageFile>();
                foreach (StorageFile sf in FilesToBackup)
                {
                    if (sf.Name == Dictionaries.DatabaseLiterals.DBName + Dictionaries.DatabaseLiterals.DBTypeSqlite)
                    {
                        StorageFile newFile = await sf.CopyAsync(fieldBook, newName);
                        newList.Add(newFile);

                    }
                    else
                    {
                        newList.Add(sf);
                    }

                    

                }

                //Zip 
                string outputArchivePath = await fs.AddFilesToZip(newList);
                
                //Keep youngest photo in memory
                localSetting.SetSettingValue(Dictionaries.ApplicationLiterals.KeywordBackupPhotoYoungest, youngestPhoto);

                //Copy
                await fs.SaveArchiveCopy(newList, "", "", projectName + "_");

                //Delete renamed copy
                foreach (StorageFile fsToDelete in newList)
                {
                    if (fsToDelete.Name.Contains(newName))
                    {
                        await fsToDelete.DeleteAsync();
                        break;
                    }
                    
                }

                
            }
            else if (FilesToBackup.Count == 1 && FilesToBackup[0].Name.Contains(DatabaseLiterals.DBTypeSqlite))
            {
                //Do not zip, only output sqlite
                try
                {
                    await fs.SaveDBCopy();
                }
                catch (Exception)
                {
                    //Show error message
                    var loadLocalization = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
                    ContentDialog endProcessDialog = new ContentDialog()
                    {
                        Title = loadLocalization.GetString("SaveDBDialogTitle"),
                        Content = loadLocalization.GetString("SaveDBDialogContentError"),
                        PrimaryButtonText = loadLocalization.GetString("LoadDataButtonProcessEndMessageOk")
                    };

                    endProcessDialog.Style = (Style)Application.Current.Resources["DeleteDialog"];
                    ContentDialogResult cdr = await endProcessDialog.ShowAsync();
                }
                
            }
            else if (FilesToBackup.Count == 0)
            {
                //Show empty archive warning
                var loadLocalization = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
                ContentDialog warningNoPhotoBackup = new ContentDialog()
                {
                    Title = loadLocalization.GetString("ShellBackupEmptyTitle"),
                    Content = loadLocalization.GetString("ShellBackupEmptyMessage/Text"),
                    PrimaryButtonText = loadLocalization.GetString("GenericDialog_ButtonOK")
                };
                warningNoPhotoBackup.Style = (Style)Application.Current.Resources["WarningDialog"];
                ContentDialogResult cdr = await warningNoPhotoBackup.ShowAsync();
            }

            //Show empty backed up photo warning
            if (photoCount == 0 && hasPhotos)
            {
                
                var loadLocalization = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
                ContentDialog warningNoPhotoBackup = new ContentDialog()
                {
                    Title = loadLocalization.GetString("ShellBackupPhotoEmptyTitle"),
                    Content = loadLocalization.GetString("ShellBackupPhotoEmptyMessage/Text"),
                    PrimaryButtonText = loadLocalization.GetString("GenericDialog_ButtonOK")
                };
                warningNoPhotoBackup.Style = (Style)Application.Current.Resources["WarningDialog"];
                ContentDialogResult cdr = await warningNoPhotoBackup.ShowAsync();
            }

        }

        public void RestoreAsync()
        {
            return;
        }

    }
}
