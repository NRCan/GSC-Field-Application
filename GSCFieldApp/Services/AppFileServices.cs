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

            stream.Close();
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
                //Get a list of files from field book that has the same name (layer json and other file types and photos)
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
            }

            //Save a copy of zipped folder with a prompt
            using Stream localStream = System.IO.File.OpenRead(da.PreferedDatabasePath);
            CancellationToken cancellationToken = new CancellationToken();
            var fileSaverResult = await FileSaver.Default.SaveAsync(fieldBookZipPath, localStream, cancellationToken);

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
                                    {DevicePlatform.WinUI, new [] { DatabaseLiterals.DBTypeSqlite} },
                                    {DevicePlatform.Android, new [] { "application/*"} },
                                    {DevicePlatform.iOS, new [] { DatabaseLiterals.DBTypeSqlite } },
                });

            PickOptions options = new PickOptions();
            options.PickerTitle = "Upload Field Book";
            options.FileTypes = customFileType;

            var result = await FilePicker.Default.PickAsync(options);
            if (result != null)
            {
                if (result.FileName.Contains(DatabaseLiterals.DBTypeSqlite))
                {
                    //Copy to local state
                    copiedFieldBookPath = System.IO.Path.Join(userLocalFolder, result.FileName);
                    using (FileStream copiedFieldBookStream = new FileStream(copiedFieldBookPath, FileMode.Create))
                    using (Stream incomingFieldBookStream = System.IO.File.OpenRead(result.FullPath))
                    await incomingFieldBookStream.CopyToAsync(copiedFieldBookStream);

                }

            }

            return copiedFieldBookPath;

        }

    }
}
