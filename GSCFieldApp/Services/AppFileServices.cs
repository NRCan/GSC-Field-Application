using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Storage;
using GSCFieldApp.Dictionaries;
using GSCFieldApp.Services.DatabaseServices;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SQLite;
using GSCFieldApp.Models;

namespace GSCFieldApp.Services
{
    public class AppFileServices
    {
        public LocalizationResourceManager LocalizationResourceManager
            => LocalizationResourceManager.Instance; // Will be used for in code dynamic local strings

        public AppFileServices() { }

        /// <summary>
        /// Will save prefered database
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task SaveBackupDBFile(CancellationToken cancellationToken)
        {

            //Open desired file
            DataAccess da = new DataAccess();
            using Stream stream = System.IO.File.OpenRead(da.PreferedDatabasePath);

            //Get output name
            string outputFileName = Path.GetFileName(da.PreferedDatabasePath).Replace(DatabaseLiterals.DBTypeSqliteDeprecated, DatabaseLiterals.DBTypeSqlite);

            //Open save dialog
            try
            {
                var fileSaverResult = await FileSaver.Default.SaveAsync(outputFileName, stream, cancellationToken);

                //Use Toast to show card in window interface or system like notification rather then modal alert popup.
                if (fileSaverResult.IsSuccessful)
                {
                    string toastText = String.Format(LocalizationResourceManager["ToastSaveBackup"].ToString(), fileSaverResult.FilePath);

                    await Toast.Make(toastText).Show(cancellationToken);
                }
                else
                {
                    string toastText = String.Format(LocalizationResourceManager["ToastSaveBackupFailed"].ToString(), fileSaverResult.Exception.Message);
                    await Toast.Make(toastText).Show(cancellationToken);
                }
            }
            catch (Exception e)
            {
                new ErrorToLogFile(e.Message).WriteToFile();
            }

            stream.Close();
        }

        /// <summary>
        /// Will save prefered database
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task SaveBugLogFile(CancellationToken cancellationToken)
        {

            //Open desired file
            DataAccess da = new DataAccess();
            string errorLogFilePath = Path.Combine(FileSystem.Current.AppDataDirectory, ApplicationLiterals.errorLogFileNameExt);
            if (Path.Exists(errorLogFilePath))
            {
                using Stream stream = System.IO.File.OpenRead(errorLogFilePath);

                if (stream != null)
                {
                    //Open save dialog
                    var fileSaverResult = await FileSaver.Default.SaveAsync(ApplicationLiterals.errorLogFileNameExt, stream, cancellationToken);

                    //Use Toast to show card in window interface or system like notification rather then modal alert popup.
                    if (fileSaverResult.IsSuccessful)
                    {
                        string toastText = String.Format(LocalizationResourceManager["ToastSaveBackup"].ToString(), fileSaverResult.FilePath);

                        await Toast.Make(toastText).Show(cancellationToken);
                    }
                    else
                    {
                        string toastText = String.Format(LocalizationResourceManager["ToastSaveBackupFailed"].ToString(), fileSaverResult.Exception.Message);
                        await Toast.Make(toastText).Show(cancellationToken);
                    }
                }

                stream.Close();
            }
            else
            {
                string toastText = String.Format(LocalizationResourceManager["ToastSaveBackupFailed"].ToString(), "Log file not found.");
                await Toast.Make(toastText).Show(cancellationToken);
            }

        }


        /// <summary>
        /// Will start a backup process for field books.
        /// </summary>
        /// <returns></returns>
        public async Task BackupFieldBook()
        {

            //Variables
            DataAccess da = new DataAccess();
            string userLocalFolder = Path.GetDirectoryName(da.PreferedDatabasePath);
            string userDBName = Path.GetFileNameWithoutExtension(da.PreferedDatabasePath);
            string newDirectoryPath = Path.Combine(userLocalFolder, LocalizationResourceManager["FieldBookBackupGeneric"].ToString() + "_" + userDBName);
            string fieldBookZipPath = newDirectoryPath + ".zip";

            //Clean first
            if (File.Exists(fieldBookZipPath))
            {
                File.Delete(fieldBookZipPath);
            }

            //Zip needed files
            using (FileStream zipToOpen = new FileStream(fieldBookZipPath, FileMode.Create))
            using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
            {
                //Get a list of files from field book that has the same name (layer json and other file types)
                foreach (var file in Directory.GetFiles(userLocalFolder, userDBName + "*"))
                {
                    //Make sure to not self zip current zip
                    if (!file.Contains(LocalizationResourceManager["FieldBookBackupGeneric"].ToString()))
                    {
                        var entryName = Path.GetFileName(file);
                        var entry = archive.CreateEntry(entryName);
                        entry.LastWriteTime = File.GetLastWriteTime(file);
                        using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        using (var stream = entry.Open())
                        {
                            fs.CopyTo(stream);
                            fs.Close();
                        }
                    }

                }

                //Get a list of embeded device photos 
                AppFileServices afs = new AppFileServices();
                string photoSubFolder = afs.GetPhotoSubFolder();
                if (Directory.Exists(photoSubFolder))
                {
                    foreach (var photos in Directory.GetFiles(photoSubFolder))
                    {
                        var entryName = Path.GetFileName(photos);
                        var entry = archive.CreateEntry(entryName);
                        entry.LastWriteTime = File.GetLastWriteTime(photos);
                        using (var fs = new FileStream(photos, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        using (var stream = entry.Open())
                        {
                            fs.CopyTo(stream);
                            fs.Close();
                        }
                    }
                }
            }

            //Save a copy of zipped folder with a prompt
            using Stream localStream = System.IO.File.OpenRead(fieldBookZipPath);
            CancellationToken cancellationToken = new CancellationToken();
            string zipFileName = Path.GetFileName(fieldBookZipPath);
            var fileSaverResult = await FileSaver.Default.SaveAsync(zipFileName, localStream, cancellationToken);

            //Use Toast to show card in window interface or system like notification rather then modal alert popup.
            if (fileSaverResult.IsSuccessful)
            {
                string toastText = String.Format(LocalizationResourceManager["ToastFieldBookBackup"].ToString(), fileSaverResult.FilePath);

                await Toast.Make(toastText).Show(cancellationToken);
            }
            else
            {
                string toastText = String.Format(LocalizationResourceManager["ToastFieldBookBackupFailed"].ToString(), fileSaverResult.Exception.Message);
                await Toast.Make(toastText).Show(cancellationToken);
            }

            //Clean up uncessary files and dir
            File.Delete(fieldBookZipPath);
            localStream.Close();
            return;

        }

        /// <summary>
        /// Will upload a given field book
        /// TODO: Add zip loading option
        /// </summary>
        /// <returns></returns>
        public async Task<string> UploadFieldBook()
        {

            //Variables
            DataAccess da = new DataAccess();
            string userLocalFolder = Path.GetDirectoryName(da.PreferedDatabasePath);
            string copiedFieldBookPath = userLocalFolder;

            //Custom file picker for fieldbooks
            FilePickerFileType customFileType = new FilePickerFileType(
                new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                                    {DevicePlatform.WinUI, new [] { DatabaseLiterals.DBTypeSqlite, DatabaseLiterals.DBTypeSqliteDeprecated} },
                                    {DevicePlatform.Android, new [] { "application/*"} },
                                    {DevicePlatform.iOS, new [] { DatabaseLiterals.DBTypeSqlite, DatabaseLiterals.DBTypeSqliteDeprecated } },
                });

            PickOptions options = new PickOptions();
            options.PickerTitle = LocalizationResourceManager["FieldBooksUploadTitle"].ToString();
            options.FileTypes = customFileType;

            var result = await FilePicker.Default.PickAsync(options);
            if (result != null)
            {
                //Get proper database name to fit standard naming template
                SQLiteAsyncConnection currentConnection = da.GetConnectionFromPath(result.FullPath);
                List<Metadata> metadataTableRows = await currentConnection.Table<Metadata>()?.ToListAsync();
                await currentConnection.CloseAsync();

                if (metadataTableRows != null && metadataTableRows.Count() == 1)
                {
                    copiedFieldBookPath = System.IO.Path.Join(userLocalFolder, metadataTableRows[0].FieldBookFileName + DatabaseLiterals.DBTypeSqlite);

                    //Legacy extension
                    if (result.FileName.Contains(DatabaseLiterals.DBTypeSqliteDeprecated))
                    {
                        copiedFieldBookPath = System.IO.Path.Join(userLocalFolder, metadataTableRows[0].FieldBookFileName + DatabaseLiterals.DBTypeSqliteDeprecated);
                    }

                    //Copy to local state
                    using (FileStream copiedFieldBookStream = new FileStream(copiedFieldBookPath, FileMode.Create))
                    using (Stream incomingFieldBookStream = System.IO.File.OpenRead(result.FullPath))
                        await incomingFieldBookStream.CopyToAsync(copiedFieldBookStream);
                }
                else
                {
                    //Show error
                    new ErrorToLogFile(LocalizationResourceManager["FieldBooksUploadContentInvalid"].ToString()).WriteToFile();
                    await Shell.Current.DisplayAlert(LocalizationResourceManager["FieldBooksUploadTitle"].ToString(),
                        LocalizationResourceManager["FieldBooksUploadContentInvalid"].ToString(),
                        LocalizationResourceManager["GenericButtonOk"].ToString());
                }

            }

            return copiedFieldBookPath;

        }

        /// <summary>
        /// Will create the photo subfolder to store embeded photos from the device
        /// </summary>
        /// <returns></returns>
        public string CreatePhotoSubFolder()
        {
            string photoSubFolder = GetPhotoSubFolder();

            if (!Directory.Exists(photoSubFolder))
            {
                Directory.CreateDirectory(photoSubFolder);
            }

            return photoSubFolder;
        }

        /// <summary>
        /// Will return the needed sub folder to store user embeded device photos for the current field book
        /// User could have the same officer code (used in photo naming), but for different field books. 
        /// Sub folder solution solves this and will help backup to get only the right photos.
        /// </summary>
        /// <returns></returns>
        public string GetPhotoSubFolder()
        {
            //Variables
            DataAccess da = new DataAccess();
            string subFolderPath = Path.Combine(Path.GetDirectoryName(da.PreferedDatabasePath), Path.GetFileNameWithoutExtension(da.PreferedDatabasePath) + ApplicationLiterals.photoFolderSuffix);

            return subFolderPath;
        }

    }
}
