
#if ANDROID
using Android.Content;
using Android.Content.PM;
using Android.Provider;
using Android.Webkit;
using AndroidX.Core.Content;
using GSCFieldApp.Services.Abstraction;
using Microsoft.Maui.ApplicationModel; // Platform
using FileProvider = AndroidX.Core.Content.FileProvider;
using Uri = Android.Net.Uri;

namespace GSCFieldApp.Platforms.Android.Services;

public sealed class PhotoEditorLauncher : IPhotoEditorLauncher
{
    /// <summary>
    /// Will launch an edit work on the incoming photo path.
    /// Since Android photo apps can't read/write images stored in /data/user/0/gscfieldapp.gscfieldapp/files/filename.jpg
    /// we have to copy the file into a shared location first (e.g. MediaStore) and grant permissions. Copy will be deleted and 
    /// newly annotated photos will be brought back in the app.
    /// </summary>
    /// <param name="absoluteImagePath"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<bool> EditAsync(string absoluteImagePath, CancellationToken ct = default)
    {

        try
        {
            //Build the context for editing a photo (platform specific)
            Context context = Platform.CurrentActivity ?? Platform.AppContext;
            if (!string.IsNullOrWhiteSpace(absoluteImagePath) || File.Exists(absoluteImagePath))
            {
                //Get FileProvider path, so the image path is as seen from an external app 
                string workingPath = await EnsureUnderFilesAsync(context, absoluteImagePath, ct);

                //Build proper URI for the file with FileProvider 
                var authority = $"{context.PackageName}.fileprovider"; // must match AndroidManifest.xml
                var file = new Java.IO.File(workingPath);
                Uri fpUri = FileProvider.GetUriForFile(context, authority, file);

                //Build MIME type or use generic one for images
                var mime = GetMimeFromExtension(Path.GetExtension(workingPath));
                if (mime is null)
                {
                    mime = "image/*";
                } 

                //Copy photo to shared media store (Pictures/GSCFieldApp)
                var mediaUri = await ImportToMediaStoreAsync(context, file.Path, mime, ct);
                if (mediaUri is null) return false;

                //Launches the photo editor selected by user
                return TryLaunchEditor(context, mediaUri, mime);

            }
            else
            {
                return false;
            }
        }
        catch
        {
            return false;
        }

    }

    /// <summary>
    /// Will set a proper media mime type based on file extension
    /// </summary>
    /// <param name="ext"></param>
    /// <returns></returns>
    private static string? GetMimeFromExtension(string? ext)
    {
        if (string.IsNullOrWhiteSpace(ext)) return null;
        string clean = ext.TrimStart('.').ToLowerInvariant();
        if (string.IsNullOrEmpty(clean)) return null;
        return MimeTypeMap.Singleton.GetMimeTypeFromExtension(clean);
    }

    /// <summary>
    /// Will copy the local stored file into the files paths accessible by external apps
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="path"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    private static async Task<string> EnsureUnderFilesAsync(Context ctx, string path, CancellationToken ct)
    {
        // Prefer app's external files directory so it’s covered by <external-files-path>
        Java.IO.File extDir = ctx.FilesDir;
        string targetDir = extDir!.AbsolutePath; // /storage/emulated/0/Android/data/<pkg>/files
        string target = Path.Combine(targetDir, Path.GetFileName(path));

        // If already under target directory, keep it; else copy
        if (!Path.GetFullPath(path).StartsWith(Path.GetFullPath(targetDir), StringComparison.OrdinalIgnoreCase))
        {
            // Copy (overwrite OK)
            using FileStream input = File.OpenRead(path);
            using FileStream output = File.Create(target);
            await input.CopyToAsync(output, ct).ConfigureAwait(false);
        }
        else
        {
            target = path;
        }
        return target;
    }

    /// <summary>
    /// Launches a photo editor
    /// NOTE: this one doesn't work at all with apps like Google Photos if the URI doesn't come from a shared media location
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="uri"></param>
    /// <param name="mime"></param>
    /// <returns></returns>
    private static bool TryLaunchEditor(Context ctx, Uri uri, string mime)
    {
        try
        {
            Intent editIntent = new Intent(Intent.ActionEdit);
            editIntent.SetDataAndType(uri, mime);
            editIntent.AddFlags(ActivityFlags.GrantReadUriPermission |
                          ActivityFlags.GrantWriteUriPermission |
                          ActivityFlags.NewTask);

            //Add URI to clipboard for more reliability
            editIntent.ClipData = ClipData.NewRawUri(string.Empty, uri);

            //Force write/read permissions
            IList<ResolveInfo> candidates = ctx.PackageManager!.QueryIntentActivities(editIntent, PackageInfoFlags.MatchAll);
            foreach (ResolveInfo ri in candidates)
            {
                string pkg = ri.ActivityInfo?.PackageName;
                if (!string.IsNullOrEmpty(pkg))
                {
                    ctx.GrantUriPermission(pkg, uri,
                        ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission);
                }
            }

            //Present editor options to user and again force read/write permissions
            Intent chooser = Intent.CreateChooser(editIntent, "", null);
            chooser.AddFlags(ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission);
            ctx.StartActivity(chooser);
            
            return true;
        }
        catch (ActivityNotFoundException)
        {
            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Copies the given file into MediaStore (Pictures/GSCFieldApp) and returns the new media Uri.
    /// </summary>
    private static async Task<Uri?> ImportToMediaStoreAsync(Context ctx, string path, string mime, CancellationToken ct)
    {
        try
        {

            //Set URI
            string displayName = Path.GetFileName(path);
            ContentResolver resolver = ctx.ContentResolver!;
            Uri pictures = MediaStore.Images.Media.GetContentUri("external");

            //Set content values for the new media
            ContentValues values = new ContentValues();
            values.Put(MediaStore.IMediaColumns.DisplayName, displayName);
            values.Put(MediaStore.IMediaColumns.MimeType, mime);
            values.Put(MediaStore.Images.ImageColumns.RelativePath, "Pictures/GSCFieldApp");
            values.Put(MediaStore.IMediaColumns.IsPending, true);

            //Set new URI
            Uri mediaUri = resolver.Insert(pictures, values);
            if (mediaUri != null)
            {
                //Copy file content into the new media URI
                using (FileStream src = File.OpenRead(path))
                using (Stream dst = resolver.OpenOutputStream(mediaUri, "w"))
                {
                    if (dst != null)
                    { 
                        await src.CopyToAsync(dst, ct).ConfigureAwait(false);
                    }
                    else
                    {
                        return null;
                    }
                    
                }

                //Update
                values.Clear();
                values.Put(MediaStore.IMediaColumns.IsPending, false);
                resolver.Update(mediaUri, values, null, null);

                return mediaUri;
            }
            else
            {
                return null;
            }


        }
        catch
        {
            return null;
        }
    }

}
#endif
