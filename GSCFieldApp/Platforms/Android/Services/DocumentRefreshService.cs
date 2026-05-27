#if ANDROID
using Android.Content;
using Android.Database;
using Android.Media;
using Android.Media.TV;
using Android.Net;
using Android.Provider;
using GSCFieldApp.Dictionaries;
using GSCFieldApp.Services.Abstraction;
using Java.Net;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.PlatformConfiguration;
using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using static AndroidX.Concurrent.Futures.CallbackToFutureAdapter;
using static GSCFieldApp.Dictionaries.DatabaseLiterals;

namespace GSCFieldApp.Services
{
    public sealed class DocumentRefreshService : IDocumentRefreshService
    {
        private DateTime _lastBackgroundUtc = DateTime.UtcNow;
        private Android.Net.Uri? _mediaUri = null;
        private string _displayName = string.Empty;
        private string? _mime = null;
        private Context _ctx = null;

        public void MarkBackgroundTimestamp(DateTime utcNow)
            => _lastBackgroundUtc = utcNow;

        public static event EventHandler<string>? newAnnotatedDocument;

        public async Task RefreshIfNeededAsync(DateTime sinceUtc, CancellationToken ct = default)
        {
            try
            {            
                //Init some platform variables
                _ctx = Platform.CurrentActivity ?? Platform.AppContext;
                ContentResolver resolver = _ctx.ContentResolver!;
                Android.Net.Uri picturesFolderUri = MediaStore.Images.Media.GetContentUri("external");

                //Calculate seconds since last time app was in background
                long sinceSeconds = Math.Max(
                    0L,
                    (long)(sinceUtc - DateTime.UnixEpoch).TotalSeconds - 100
                );

                //Search in Pictures/GSCFieldApp for images modified after `sinceUtc`.
                string selectionPreferred =
                    $"{MediaStore.IMediaColumns.RelativePath} LIKE ? AND " +
                    $"{MediaStore.IMediaColumns.DateModified} >= ?";

                //Set some arugments to filter the results
                string[] argsPreferred = new[]
                {
                    string.Format("%Pictures/{0}%",ApplicationLiterals.androidPictureFolder),
                    sinceSeconds.ToString(CultureInfo.InvariantCulture)
                };

                //Set some attributes that will be extracted from the results
                string[] projection = new[]
                {
                    MediaStore.IMediaColumns.DisplayName,
                    MediaStore.IMediaColumns.DateModified
                };

                //Ordering query result to get first row being the latest file
                string sortOrder = $"{MediaStore.IMediaColumns.DateModified} DESC";

                //Try preferred location first
                _mediaUri = QueryFiles(selectionPreferred, argsPreferred, resolver, picturesFolderUri, projection, sortOrder, _displayName, _mime);

                if (_mediaUri != null)
                {

                    //Manage file name (Android photo only saving copies), will need a rename
                    string newDisplayName = System.IO.Path.GetFileName(_mediaUri.EncodedPath);
                    newDisplayName = newDisplayName.Replace("(1)", ""); //Android creates a copy of the original photo with (1) at the end of the file name, we need to remove it to get the original name for the thumbnail refresh to work.
                    newDisplayName = newDisplayName.Replace(" ", "");
                    newDisplayName = newDisplayName.Replace("%20", "");
                    if (!newDisplayName.Contains(ApplicationLiterals.annotatedDocumentSuffix))
                    {
                        newDisplayName = newDisplayName.Replace(".", ApplicationLiterals.annotatedDocumentSuffix + ".");
                    }

                    //Build destination path for copied photo
                    AppFileServices appFileServices = new AppFileServices();
                    string projectPhotoFolder = appFileServices.GetPhotoSubFolder();
                    var destPath = Path.Combine(projectPhotoFolder, newDisplayName);

                    //Stream the file from the content resolver to the destination path
                    using (System.IO.Stream inStream = resolver.OpenInputStream(_mediaUri))
                    {
                        if (inStream != null)
                        {
                            using (FileStream outStream = File.Create(destPath))
                            {
                                await inStream.CopyToAsync(outStream, ct);
                            }

                            //Send call to refresh thumbnail in document page and image hyperlink in database
                            newAnnotatedDocument?.Invoke(this, destPath);

                        }
                    }
                }               
            }
            catch (Exception ex) 
            {
                //Skip
                new ErrorToLogFile(ex).WriteToFile();
            }

            return;
        }

        //Query files and get the one that matches the selection
        public Android.Net.Uri? QueryFiles(string? selection, string[]? selectionArgs, ContentResolver resolver, Android.Net.Uri pictures, 
            string[] projection, string sortOrder, string? displayName, string? mime )
        {
            try
            {
                /// Tried this method, won't work because Google Photos doesn't update. 
                /// Keeping this code here in case we find another way to trigger a media scan or get the new photo URI directly from Google Photos.
                //Force a new scan to detect Google photo save copy
                //MediaScannerConnection.ScanFile(
                //    Android.App.Application.Context,
                //    new string[] { "/sdcard/Pictures/GSCFieldApp" },
                //    null,
                //    null
                //);

                using ICursor? cursor = resolver.Query(pictures, projection, selection, selectionArgs, sortOrder);
                if (cursor == null || cursor.Count == 0 || !cursor.MoveToFirst())
                    return null;

                while (cursor != null) 
                {
                    //Get indexes
                    int nameIdx = cursor.GetColumnIndexOrThrow(MediaStore.IMediaColumns.DisplayName);

                    //Get values
                    string originalDisplayName = cursor.GetString(nameIdx);

                    if (originalDisplayName != null && 
                        (originalDisplayName.Contains("(") || 
                        originalDisplayName.Contains("~­") || 
                        originalDisplayName.Contains(ApplicationLiterals.annotatedDocumentSuffix)))
                    {

                        //Build the URI
                        Java.IO.File androidDirectoryPictures = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures);
                        Java.IO.File fieldAppDirectoryPictures = new Java.IO.File(androidDirectoryPictures, ApplicationLiterals.androidPictureFolder);
                        Java.IO.File newPhotoFile = new Java.IO.File(fieldAppDirectoryPictures, originalDisplayName);

                        if (newPhotoFile.Exists())
                        {
                            URI uri = newPhotoFile.ToURI();
                            if (uri != null)
                            {
                                _mediaUri = Android.Net.Uri.Parse(uri.ToString());
                            }
                            else 
                            { 
                                _mediaUri= null;
                            }
                        }
                        else
                        {
                            _mediaUri = null;
                        }
                    }
                    else 
                    { 
                        _mediaUri = null;
                    }

                    cursor.MoveToNext();
                }

            }
            catch (Exception queryFilesException)
            {
                new ErrorToLogFile(queryFilesException).WriteToFile();
            }
            
            return _mediaUri;

        }
    }
}
#endif