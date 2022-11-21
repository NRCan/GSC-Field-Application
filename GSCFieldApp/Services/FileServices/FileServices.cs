using GSCFieldApp.Services.DatabaseServices;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using System.Runtime.InteropServices.WindowsRuntime;
using System.IO.Compression;
using Windows.UI.Xaml;

namespace GSCFieldApp.Services.FileServices
{
    public class FileServices
    {
        //Settings
        DataLocalSettings localSetting = new DataLocalSettings();
        DataAccess accessData = new DataAccess();

        /// <summary>
        /// Will delete a given file from the local state folder of the current app
        /// </summary>
        /// <param name="filePath"></param>
        public async void DeleteLocalStateFile(string filePath)
        {
            if (filePath!=string.Empty)
            {
                try
                {
                    filePath = filePath.Replace("file:\\", "");
                    StorageFile inFile = await StorageFile.GetFileFromPathAsync(filePath);
                    await inFile.DeleteAsync();
                }
                catch (Exception)
                {
                }


            }


        }

        /// <summary>
        /// Will delete all the folders from the local state folder.
        /// </summary>
        public async Task DeleteLocalStateFileAll()
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            IReadOnlyList<StorageFolder> fileList = await localFolder.GetFoldersAsync();
            foreach (StorageFolder folder in fileList)
            {
                try
                {
                    await folder.DeleteAsync();
                }
                catch (Exception)
                {

                }
                    
                

            }
        }

        /// <summary>
        /// Will calculate a filename for output database copye, based on user geolcode, current date and the original database name
        /// </summary>
        /// <param name="userCode"></param>
        /// <returns></returns>
        public string CalculateDBCopyName(string userCode = "")
        {
            //Variables
            string outputName = string.Empty;

            string currentDBName = DataAccess._dbName;

            //Calculate current date
            string currentDate = String.Format("{0:yyyyMMdd_HH'h'mm}", DateTime.Now);

            //Get currennt geolcode
            if (userCode == string.Empty && localSetting.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoUCode)!=null)
            {
                userCode = localSetting.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoUCode).ToString() + "_";
            }
            else
            {
                userCode = userCode + "_";
            }

            //Calculate new output database name
            outputName = userCode + currentDate;

            return outputName;

        }

        /// <summary>
        /// Will save a field work copy, from local state folder to user selected output folder from a save picker dialog
        /// </summary>
        public async Task<string> SaveDBCopy(string currentDBPath = "", string currentUserCode = "")
        {
            //Variables
            string outputSaveFilePath = string.Empty;

            //Create a file save picker for sqlite
            var fileSavePicker = new Windows.Storage.Pickers.FileSavePicker();
            fileSavePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            fileSavePicker.FileTypeChoices.Add("sqlite", new List<string>() { ".sqlite" });
            fileSavePicker.DefaultFileExtension = ".sqlite";
            fileSavePicker.SuggestedFileName = CalculateDBCopyName(currentUserCode); //Should be something like Geolcode_YYYYMMDD_GSCFieldwork.sqlite

            //Get users selected save files
            StorageFile savefile = await fileSavePicker.PickSaveFileAsync(); //This will save an empty file at the location user has selected

            //Get user local state database as binary buffer
            if (currentDBPath == string.Empty)
            {
                currentDBPath = DataAccess.DbPath;
            }
            StorageFile fileToRead = await StorageFile.GetFileFromPathAsync(currentDBPath);

            if (fileToRead!=null)
            {
                IBuffer currentDBBuffer = await Windows.Storage.FileIO.ReadBufferAsync(fileToRead as IStorageFile);
                byte[] currentDBByteArray = currentDBBuffer.ToArray();

                if (savefile != null)
                {
                    //Lock the file
                    Windows.Storage.CachedFileManager.DeferUpdates(savefile);

                    //Save
                    await Windows.Storage.FileIO.WriteBytesAsync(savefile, currentDBByteArray);
                    Windows.Storage.Provider.FileUpdateStatus status = await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(savefile);
                    if (status == Windows.Storage.Provider.FileUpdateStatus.Complete)
                    {
                        //Show end message
                        var loadLocalization = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
                        ContentDialog endProcessDialog = new ContentDialog()
                        {
                            Title = loadLocalization.GetString("SaveDBDialogTitle"),
                            Content = loadLocalization.GetString("SaveDBDialogContent") + "\n" + savefile.Path.ToString(),
                            PrimaryButtonText = loadLocalization.GetString("LoadDataButtonProcessEndMessageOk")
                        };

                        ContentDialogResult cdr = await endProcessDialog.ShowAsync();
                    }
                    else
                    {
                        //Show error message
                        var loadLocalization = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
                        ContentDialog endProcessDialog = new ContentDialog()
                        {
                            Title = loadLocalization.GetString("SaveDBDialogTitle"),
                            Content = loadLocalization.GetString("SaveDBDialogContentError"),
                            PrimaryButtonText = loadLocalization.GetString("LoadDataButtonProcessEndMessageOk")
                        };

                        ContentDialogResult cdr = await endProcessDialog.ShowAsync();
                    }

                    //Last validation just in case
                    Windows.Storage.FileProperties.BasicProperties baseProp = await savefile.GetBasicPropertiesAsync();
                    if (baseProp.Size == 0)
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
            }

            if (savefile != null && savefile.Path != null)
            {
                outputSaveFilePath = savefile.Path;
            } 

            return outputSaveFilePath;

        }

        /// <summary>
        /// Will save a list of photos from local state folder to user selected output folder from a save picker dialog
        /// </summary>
        /// <param name="photos"></param>
        /// <param name="currentUserCode"></param>
        /// <returns></returns>
        public async Task<string> SaveArchiveCopy(List<StorageFile> photos, string fieldbookpath = "", string currentUserCode = "", string prefix = "")
        {
            if (fieldbookpath == string.Empty)
            {
                fieldbookpath = localSetting.GetSettingValue(Dictionaries.ApplicationLiterals.KeywordFieldProject).ToString();
            }

            //Add photos to a zip file
            string photoZipFilePath = await AddFilesToZip(photos, fieldbookpath, currentUserCode);
            StorageFile arhiveToRead = await StorageFile.GetFileFromPathAsync(photoZipFilePath);

            //Variables
            string outputZipPhotoFilePath = string.Empty;

            //Create a file save picker for sqlite
            var fSavePicker = new Windows.Storage.Pickers.FileSavePicker();
            fSavePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            fSavePicker.FileTypeChoices.Add("zip", new List<string>() { ".zip" });
            fSavePicker.DefaultFileExtension = ".zip";
            fSavePicker.SuggestedFileName = prefix + arhiveToRead.Name.Split('.')[0];

            //Get users selected save files
            StorageFile saveArchiveFile = await fSavePicker.PickSaveFileAsync(); //This will save an empty file at the location user has selected

            if (arhiveToRead != null)
            {
                IBuffer currentArchiveBuffer = await Windows.Storage.FileIO.ReadBufferAsync(arhiveToRead as IStorageFile);
                byte[] currentArchiveByteArray = currentArchiveBuffer.ToArray();

                if (saveArchiveFile != null)
                {
                    //Lock the file
                    Windows.Storage.CachedFileManager.DeferUpdates(saveArchiveFile);

                    //Save
                    await Windows.Storage.FileIO.WriteBytesAsync(saveArchiveFile, currentArchiveByteArray);
                    Windows.Storage.Provider.FileUpdateStatus status = await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(saveArchiveFile);
                    if (status == Windows.Storage.Provider.FileUpdateStatus.Complete)
                    {
                        //Show end message
                        var loadLocalization = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
                        ContentDialog endProcessDialog = new ContentDialog()
                        {
                            Title = loadLocalization.GetString("SaveDBDialogTitle"),
                            Content = loadLocalization.GetString("SaveDBDialogContent") + "\n" + saveArchiveFile.Path.ToString(),
                            PrimaryButtonText = loadLocalization.GetString("LoadDataButtonProcessEndMessageOk")
                        };

                        ContentDialogResult cdr = await endProcessDialog.ShowAsync();
                    }
                    else
                    {
                        //Show error message
                        var loadLocalization = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
                        ContentDialog endProcessDialog = new ContentDialog()
                        {
                            Title = loadLocalization.GetString("SaveDBDialogTitle"),
                            Content = loadLocalization.GetString("SaveDBDialogContentError"),
                            PrimaryButtonText = loadLocalization.GetString("LoadDataButtonProcessEndMessageOk")
                        };

                        ContentDialogResult cdr = await endProcessDialog.ShowAsync();
                    }


                }
            }

            if (saveArchiveFile != null && saveArchiveFile.Path != null)
            {
                outputZipPhotoFilePath = saveArchiveFile.Path;
            }

            //Delete original archive
            await arhiveToRead.DeleteAsync();

            return outputZipPhotoFilePath;
        }

        /// <summary>
        /// Will take a list of storage files and add them inside a new created zip archive.
        /// </summary>
        /// <param name="files">A list of storage files to add inside zip archive</param>
        /// <param name="currentUserCode"></param>
        /// <returns>archive path</returns>
        public Task<string> AddFilesToZip(List<StorageFile> files, string fieldbookpath = "", string currentUserCode = "")
        {
            //Make a copy of the photo inside the field book folder only if it doesn't already exists
            if (fieldbookpath == string.Empty)
            {
                fieldbookpath = localSetting.GetSettingValue(Dictionaries.ApplicationLiterals.KeywordFieldProject).ToString();
            }

            //Variables
            string zipFile = Path.Combine(fieldbookpath, CalculateDBCopyName(currentUserCode) + ".zip");

            using (FileStream zipToOpen = new FileStream(zipFile, FileMode.Create))
            {
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
                {
                    foreach (StorageFile f in files)
                    {
                        archive.CreateEntryFromFile(f.Path, f.Name);
                    }
                    
                }
            }

            return Task.FromResult(zipFile);
        }

        /// <summary>
        /// Will take a zip archive and retrieve files out of it.
        /// </summary>
        /// <param name="outputFolderPath">Path to a folder in which files will be extracted to.</param>
        /// <returns>archive path</returns>
        public async Task GetFilesFromZip(string outputFolderPath, StorageFile pathToZipArchive)
        {
            using (Stream zipToOpen = await pathToZipArchive.OpenStreamForReadAsync())
            {
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read))
                {

                    for (int i = 0; i < archive.Entries.Count; i++)
                    {
                        archive.Entries[i].ExtractToFile(Path.Combine(outputFolderPath, archive.Entries[i].Name));
                    }
                }
            }

            return;
        }


    }
}
