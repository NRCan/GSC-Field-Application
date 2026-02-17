#if ANDROID
using Android.Content;
using Android.Database;
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

namespace GSCFieldApp.Services
{
    public sealed class DocumentRefreshService : IDocumentRefreshService
    {
        private DateTime _lastBackgroundUtc = DateTime.UtcNow;
        private Android.Net.Uri? _mediaUri = null;
        private string? _displayName = null;
        private string? _mime = null;
        private Context _ctx = null;

        public void MarkBackgroundTimestamp(DateTime utcNow)
            => _lastBackgroundUtc = utcNow;

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
                    (long)(sinceUtc - DateTime.UnixEpoch).TotalSeconds - 15
                );

                //Search in Pictures/GSCFieldApp for images modified after `sinceUtc`.
                string selectionPreferred =
                    $"{MediaStore.IMediaColumns.RelativePath} LIKE ? AND " +
                    $"{MediaStore.IMediaColumns.DateModified} >= ?";

                //Set some arugments to filter the results
                string[] argsPreferred = new[]
                {
                    "%Pictures/GSCFieldApp%",
                    sinceSeconds.ToString(CultureInfo.InvariantCulture)
                };

                //Set some attributes that will be extracted from the results
                string[] projection = new[]
                {
                    MediaStore.IMediaColumns.OriginalDocumentId,
                    MediaStore.IMediaColumns.DisplayName,
                    MediaStore.IMediaColumns.DateModified,
                    MediaStore.IMediaColumns.MimeType
                };

                //Ordering query result to get first row being the latest file
                string sortOrder = $"{MediaStore.IMediaColumns.DateModified} DESC";

                //Try preferred location first
                _mediaUri = QueryFiles(selectionPreferred, argsPreferred, resolver, picturesFolderUri, projection, sortOrder, _displayName, _mime);

                if (_mediaUri != null)
                {
                    //Build destination path for copied photo
                    AppFileServices appFileServices = new AppFileServices();
                    string projectPhotoFolder = appFileServices.GetPhotoSubFolder();
                    var destPath = Path.Combine(projectPhotoFolder, _displayName);

                    //Stream the file from the content resolver to the destination path
                    using Stream inStream = resolver.OpenInputStream(_mediaUri);
                    if (inStream != null)
                    {
                        using FileStream outStream = File.Create(destPath);
                        await inStream.CopyToAsync(outStream, ct).ConfigureAwait(false);

                        //TODO event to track new picture name and force a thumbnail refresh
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    return; 
                }
                  
            }
            catch
            {
                //Skip
            }
        }

        //Query files and get the one that matches the selection
        public Android.Net.Uri? QueryFiles(string? selection, string[]? selectionArgs, ContentResolver resolver, Android.Net.Uri pictures, 
            string[] projection, string sortOrder, string? displayName, string? mime )
        {
            using ICursor? cursor = resolver.Query(pictures, projection, selection, selectionArgs, sortOrder);
            if (cursor == null || cursor.Count == 0 || !cursor.MoveToFirst())
                return null;

            //Get indexes
            int idIdx = cursor.GetColumnIndexOrThrow(MediaStore.IMediaColumns.OriginalDocumentId);
            int nameIdx = cursor.GetColumnIndexOrThrow(MediaStore.IMediaColumns.DisplayName);
            int mimeIdx = cursor.GetColumnIndexOrThrow(MediaStore.IMediaColumns.MimeType);

            //Get values
            long id = cursor.GetLong(idIdx);
            _displayName = cursor.GetString(nameIdx);
            _mime = cursor.IsNull(mimeIdx) ? "image/*" : cursor.GetString(mimeIdx);

            //Build the URI
            Java.IO.File androidDirectoryPictures = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures);
            Java.IO.File fieldAppDirectoryPictures = new Java.IO.File(androidDirectoryPictures, ApplicationLiterals.androidPictureFolder);
            Java.IO.File newPhotoFile = new Java.IO.File(fieldAppDirectoryPictures, _displayName);

            if (newPhotoFile.Exists())
            {
                URI uri = newPhotoFile.ToURI();
                if (uri != null)
                {
                    _mediaUri = Android.Net.Uri.Parse(uri.ToString());
                }
            }

            return _mediaUri;

        }
    }
}
#endif