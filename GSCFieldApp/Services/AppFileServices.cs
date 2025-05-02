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
using CommunityToolkit.Maui.Core.Primitives;
using System.ComponentModel.DataAnnotations;

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
            DataAccess da = new DataAccess();

            //Swap vocab, take global ones and replaced with whatever is in the prefered database
            bool swapedWithoutError = await da.DoSwapVocab(da.DatabaseFilePath, da.PreferedDatabasePath, true);

            if (!swapedWithoutError)
            {
                //Show error
                await Shell.Current.DisplayAlert(LocalizationResourceManager["FieldBookPageFailedToSaveTitle"].ToString(),
                    LocalizationResourceManager["ShellFileSaveVocabFail"].ToString(),
                    LocalizationResourceManager["GenericButtonOk"].ToString());
            }

            //Clean geopackage so it's compatible with ArcGIS
            await GeopackageService.MakeGeopackageArcGISCompatible(da.PreferedDatabasePath);

            //Open desired file
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
        /// Will save a log text file from a given filename and extension. Will show a toast type banner if it worked.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<string> SaveLogFile(string fileName, CancellationToken cancellationToken)
        {
            //Open desired file
            DataAccess da = new DataAccess();
            string logFilePath = Path.Combine(FileSystem.Current.AppDataDirectory, fileName);
            if (Path.Exists(logFilePath))
            {
                using Stream stream = System.IO.File.OpenRead(logFilePath);

                if (stream != null)
                {
                    //Open save dialog
                    var fileSaverResult = await FileSaver.Default.SaveAsync(fileName, stream, cancellationToken);

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

            return logFilePath;
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

            //Swap vocab, take global ones and replaced with whatever is in the prefered database
            bool swapedWithoutError = await da.DoSwapVocab(da.DatabaseFilePath, da.PreferedDatabasePath, true);

            //Clean geopackage so it's compatible with ArcGIS
            await GeopackageService.MakeGeopackageArcGISCompatible(da.PreferedDatabasePath);

            if (swapedWithoutError)
            {
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
                            var entryName = Path.GetFileName(file).Replace(DatabaseLiterals.DBTypeSqliteDeprecated, DatabaseLiterals.DBTypeSqlite); ;
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
                try
                {
                    File.Delete(fieldBookZipPath);
                    localStream.Close();
                }
                catch (Exception)
                {
                    localStream.Close();
                }
            }

            return;

        }

        /// <summary>
        /// Will upload a given field book
        /// </summary>
        /// <returns></returns>
        public async Task<string> UploadFieldBook()
        {

            //Variables
            DataAccess da = new DataAccess();
            string userLocalFolder = Path.GetDirectoryName(da.PreferedDatabasePath);
            string copiedFieldBookPath = userLocalFolder;

            //Validate main general database existance 
            ///Use case: fresh install no field book were created and user uploads one on start.
            if (!File.Exists(da.DatabaseFilePath))
            {
                await da.CreateDatabaseFromResource(da.DatabaseFilePath);
            }

            //Custom file picker for fieldbooks
            FilePickerFileType customFileType = new FilePickerFileType(
                new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                                    {DevicePlatform.WinUI, new [] { DatabaseLiterals.DBTypeSqlite, DatabaseLiterals.DBTypeSqliteDeprecated, ".zip"} },
                                    {DevicePlatform.Android, new [] { "application/*"} },
                                    {DevicePlatform.iOS, new [] { DatabaseLiterals.DBTypeSqlite, DatabaseLiterals.DBTypeSqliteDeprecated, ".zip" } },
                });

            PickOptions options = new PickOptions();
            options.PickerTitle = LocalizationResourceManager["FieldBooksUploadTitle"].ToString();
            options.FileTypes = customFileType;

            var result = await FilePicker.Default.PickAsync(options);
            if (result != null)
            {
                string resultFullPath = result.FullPath;
                string resultFileName = result.FileName;

                //Manage zip file
                if (result.FullPath.Contains(".zip"))
                {
                    using (FileStream zipToOpen = new FileStream(result.FullPath, FileMode.Open))
                    using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read))
                    {

                        //Get the database file in the zip
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            if (entry.FullName.Contains(DatabaseLiterals.DBTypeSqlite) || entry.FullName.Contains(DatabaseLiterals.DBTypeSqliteDeprecated))
                            {
                                resultFileName = entry.FullName;
                                resultFullPath = System.IO.Path.Join(userLocalFolder, resultFileName);

                                //Manage existing database
                                bool userWantsToReplace = await AskToOverwriteExistingDatabase(resultFullPath);

                                archive.ExtractToDirectory(userLocalFolder, userWantsToReplace);

                                break;
                            }
                        }
                    }
                }

                if (!resultFullPath.Contains(".zip"))
                {
                    //Get proper database name to fit standard naming template
                    SQLiteAsyncConnection currentConnection = da.GetConnectionFromPath(resultFullPath);
                    try
                    {
                        List<Metadata> metadataTableRows = await currentConnection.Table<Metadata>()?.ToListAsync();
                        await currentConnection.CloseAsync();

                        if (metadataTableRows != null && metadataTableRows.Count() == 1)
                        {
                            copiedFieldBookPath = System.IO.Path.Join(userLocalFolder, metadataTableRows[0].FieldBookFileName + DatabaseLiterals.DBTypeSqlite);

#if WINDOWS
                            copiedFieldBookPath = System.IO.Path.Join(userLocalFolder, metadataTableRows[0].FieldBookFileName + DatabaseLiterals.DBTypeSqlite);
#elif ANDROID
                            copiedFieldBookPath = System.IO.Path.Join(userLocalFolder, metadataTableRows[0].FieldBookFileName + DatabaseLiterals.DBTypeSqliteDeprecated);
#else
                            copiedFieldBookPath = System.IO.Path.Join(userLocalFolder, metadataTableRows[0].FieldBookFileName + DatabaseLiterals.DBTypeSqlite);
#endif

                            //Legacy extension
                            if (resultFileName.Contains(DatabaseLiterals.DBTypeSqliteDeprecated))
                            {
                                copiedFieldBookPath = System.IO.Path.Join(userLocalFolder, metadataTableRows[0].FieldBookFileName + DatabaseLiterals.DBTypeSqliteDeprecated);
                            }

                            //Copy to local state if not already there (coming from a zip)
                            if (!result.FullPath.Contains(".zip"))
                            {
                                //Manage existing database
                                bool userWantsToReplace = await AskToOverwriteExistingDatabase(copiedFieldBookPath);

                                if (userWantsToReplace)
                                {
                                    using (FileStream copiedFieldBookStream = new FileStream(copiedFieldBookPath, FileMode.Create))
                                    {
                                        using (Stream incomingFieldBookStream = System.IO.File.OpenRead(resultFullPath))
                                        {
                                            await incomingFieldBookStream.CopyToAsync(copiedFieldBookStream);
                                        }
                                    }
                                }
                            }

                        }
                        else
                        {
                            //Show error
                            await Shell.Current.DisplayAlert(LocalizationResourceManager["FieldBooksUploadTitle"].ToString(),
                                LocalizationResourceManager["FieldBooksUploadContentInvalid"].ToString(),
                                LocalizationResourceManager["GenericButtonOk"].ToString());
                        }
                    }
                    catch (Exception e)
                    {
                        //Show error
                        new ErrorToLogFile(e).WriteToFile();
                        await Shell.Current.DisplayAlert(LocalizationResourceManager["FieldBooksUploadTitle"].ToString(),
                            LocalizationResourceManager["FieldBooksUploadContentInvalid"].ToString(),
                            LocalizationResourceManager["GenericButtonOk"].ToString());
                    }
                }

            }

            return copiedFieldBookPath;

        }

        /// <summary>
        /// Manages existing database
        /// </summary>
        /// <returns></returns>
        public async Task<bool> AskToOverwriteExistingDatabase(string fieldBookPath)
        {
            bool userWantsToReplace = true;
            if (File.Exists(fieldBookPath))
            {
                userWantsToReplace = await Shell.Current.DisplayAlert(LocalizationResourceManager["FieldBooksUploadTitle"].ToString(),
                  LocalizationResourceManager["FieldBooksUploadContentExisting"].ToString(),
                  LocalizationResourceManager["GenericButtonYes"].ToString(), LocalizationResourceManager["GenericButtonNo"].ToString());
            }

            return userWantsToReplace;
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


        /// <summary>
        /// Will backup the photo folder as a zip archive
        /// </summary>
        /// <returns></returns>
        public async Task SaveBackupPhotos(CancellationToken cancellationToken)
        {
            try
            {
                //Variables
                DataAccess da = new DataAccess();
                string userFolder = Path.GetDirectoryName(da.PreferedDatabasePath);
                string userFolderPath = GetPhotoSubFolder();
                string userFolderName = Path.GetFileName(userFolderPath);
                string userZipPath = Path.Combine(userFolder, LocalizationResourceManager["ShellQuickPhotoBackupFileName"].ToString() + "_" + userFolderName);
                string userPhotoZipPath = userZipPath + ".zip";
                bool validates = false;

                //Validate if there is any photo to backup
                if (Directory.Exists(userFolderPath))
                {
                    if (Directory.GetFiles(userFolderPath).Count() > 0)
                    {
                        validates = true;
                    }
                }

                if (validates)
                {
                    //Clean first
                    if (File.Exists(userPhotoZipPath))
                    {
                        File.Delete(userPhotoZipPath);
                    }

                    //Zip needed files
                    using (FileStream zipToOpen = new FileStream(userPhotoZipPath, FileMode.Create))
                    using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
                    {
                        //Get a list of photos to zip
                        foreach (var file in Directory.GetFiles(userFolderPath))
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

                    //Save a copy of zipped folder with a prompt
                    using Stream localStream = System.IO.File.OpenRead(userPhotoZipPath);
                    string zipFileName = Path.GetFileName(userPhotoZipPath);
                    var fileSaverResult = await FileSaver.Default.SaveAsync(zipFileName, localStream, cancellationToken);

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

                    //Clean up uncessary files and dir
                    localStream.Close();
                    File.Delete(userPhotoZipPath);
                    
                }
                else
                {
                    await Shell.Current.DisplayAlert(LocalizationResourceManager["FieldBookBackupGeneric"].ToString(),
                        LocalizationResourceManager["ShellQuickPhotoBackupEmpty"].ToString(),
                        LocalizationResourceManager["GenericButtonOk"].ToString());
                }


            }
            catch (Exception e)
            {
                new ErrorToLogFile(e).WriteToFile();

                await Shell.Current.DisplayAlert(LocalizationResourceManager["GenericErrorTitle"].ToString(),
                    String.Format(LocalizationResourceManager["ToastSaveBackupFailed"].ToString(), e.Message),
                    LocalizationResourceManager["GenericButtonOk"].ToString());

            }

            return;
        }

    }
}
