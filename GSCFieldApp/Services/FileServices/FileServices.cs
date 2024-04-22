using GSCFieldApp.Dictionaries;
using GSCFieldApp.Services.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace GSCFieldApp.Services.FileServices
{
    public class FileServices
    {
        //Settings
        readonly DataLocalSettings localSetting = new DataLocalSettings();
        readonly DataAccess accessData = new DataAccess();

        /// <summary>
        /// Will delete a given file from the local state folder of the current app
        /// </summary>
        /// <param name="filePath"></param>
        public async void DeleteLocalStateFile(string filePath)
        {
            if (filePath != string.Empty)
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
            string projectName = string.Empty;

            //Calculate current date
            string currentDate = String.Format("{0:yyyy_MM_dd_HH'h'mm}", DateTime.Now);

            //Get currennt geolcode
            if (userCode == string.Empty && localSetting.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoUCode) != null)
            {
                userCode = localSetting.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoUCode).ToString();
            }

            if (localSetting.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoPName) != null)
            {
                projectName = localSetting.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoPName).ToString().Replace(" ", "_");
            }

            //Calculate new output database name
            outputName = projectName + "_" + currentDate + "_" + userCode;

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
            var fileSavePicker = new Windows.Storage.Pickers.FileSavePicker
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop
            };
            fileSavePicker.FileTypeChoices.Add(DatabaseLiterals.DBTypeSqliteName, new List<string>() { DatabaseLiterals.DBTypeSqlite });
            fileSavePicker.DefaultFileExtension = DatabaseLiterals.DBTypeSqlite;
            fileSavePicker.SuggestedFileName = CalculateDBCopyName(currentUserCode); //Should be something like Geolcode_YYYYMMDD_GSCFieldwork.sqlite

            //Get users selected save files
            StorageFile savefile = await fileSavePicker.PickSaveFileAsync(); //This will save an empty file at the location user has selected

            //Get user local state database as binary buffer
            if (currentDBPath == string.Empty)
            {
                currentDBPath = DataAccess.DbPath;
            }
            StorageFile fileToRead = await StorageFile.GetFileFromPathAsync(currentDBPath);

            if (fileToRead != null)
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
        /// <param name="fieldbookpath">Path to field copy to archive</param>
        /// <param name="currentUserCode"></param>
        /// <param name="prefix">Some prefix needed in the archive name</param>
        /// <returns></returns>
        public async Task<string> SaveArchiveCopy(string fieldbookpath = "", string currentUserCode = "", string prefix = "")
        {
            if (fieldbookpath == string.Empty)
            {
                fieldbookpath = localSetting.GetSettingValue(Dictionaries.ApplicationLiterals.KeywordFieldProject).ToString();
            }

            string zipFile = CalculateDBCopyName(currentUserCode) + ".zip";

            //Variables
            string outputZipPhotoFilePath = string.Empty;

            //Create a file save picker for sqlite
            var fSavePicker = new Windows.Storage.Pickers.FileSavePicker
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop
            };
            fSavePicker.FileTypeChoices.Add("zip", new List<string>() { ".zip" });
            fSavePicker.DefaultFileExtension = ".zip";
            fSavePicker.SuggestedFileName = prefix + zipFile.Split('.')[0];

            //Get users selected save files
            StorageFile saveArchiveFile = await fSavePicker.PickSaveFileAsync(); //This will save an empty file at the location user has selected
            if (zipFile != null)
            {
                if (saveArchiveFile != null)
                {
                    if (saveArchiveFile != null && saveArchiveFile.Path != null)
                    {
                        outputZipPhotoFilePath = saveArchiveFile.Path;
                    }

                    //Delete empty shell file else zip will fail
                    await saveArchiveFile.DeleteAsync(StorageDeleteOption.PermanentDelete);

                    //Save
                    ZipFile.CreateFromDirectory(fieldbookpath, outputZipPhotoFilePath);

                    //Show end message
                    var loadLocalization = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
                    ContentDialog endProcessDialog = new ContentDialog()
                    {
                        Title = loadLocalization.GetString("SaveDBDialogTitle"),
                        Content = loadLocalization.GetString("SaveDBDialogContent") + "\n" + outputZipPhotoFilePath.ToString(),
                        PrimaryButtonText = loadLocalization.GetString("LoadDataButtonProcessEndMessageOk")
                    };

                    ContentDialogResult cdr = await endProcessDialog.ShowAsync();
                }
            }

            

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
        /// Will copy a set of files into a new directory/folder
        /// </summary>
        /// <param name="files"></param>
        /// <param name="fieldbookpath"></param>
        /// <param name="currentUserCode"></param>
        /// <returns></returns>
        public async Task<string> AddFilesToFolder(List<StorageFile> files, string fieldbookpath = "", string currentUserCode = "")
        {
            //Make a copy of the photo inside the field book folder only if it doesn't already exists
            if (fieldbookpath == string.Empty)
            {
                fieldbookpath = localSetting.GetSettingValue(Dictionaries.ApplicationLiterals.KeywordFieldProject).ToString();
            }

            //Create directory
            string newFolderPath = Path.Combine(fieldbookpath, CalculateDBCopyName(currentUserCode));
            if (!Directory.Exists(newFolderPath))
            {
                Directory.CreateDirectory(newFolderPath);

            }

            StorageFolder sF = await StorageFolder.GetFolderFromPathAsync(newFolderPath);

            //Copy files
            foreach (StorageFile f in files)
            {
                await f.CopyAsync(sF);
            }

            return newFolderPath;
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
