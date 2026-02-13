
// Platforms/Android/Services/PhotoEditorLauncher.cs
#if ANDROID
using Android.Content;
using Android.Content.PM;
using AndroidX.Core.Content;
using GSCFieldApp.Services.Abstraction;
using Microsoft.Maui.ApplicationModel; // Platform
using FileProvider = AndroidX.Core.Content.FileProvider;
using Uri = Android.Net.Uri;

namespace GSCFieldApp.Platforms.Android.Services;

public sealed class PhotoEditorLauncher : IPhotoEditorLauncher
{
    public Task<bool> EditAsync(string absoluteImagePath, CancellationToken ct = default)
    {
        var context = Platform.CurrentActivity ?? Platform.AppContext;
        string intentString = null;

        var file = new Java.IO.File(absoluteImagePath);
        if (!file.Exists()) return Task.FromResult(false);

        var authority = $"{context.PackageName}.fileprovider";
        Uri uri = FileProvider.GetUriForFile(context, authority, file);

        var intent = new Intent(Intent.ActionEdit);
        intent.SetDataAndType(uri, "image/*");
        intent.AddFlags(ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission);
        intent.AddFlags(ActivityFlags.NewTask);
        //intent.SetClipData(ClipData.NewRawUri(string.Empty, uri));

        // Pre-grant perms to all resolved handlers
        var handlers = context.PackageManager!.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
        foreach (var ri in handlers)
        {
            var pkg = ri.ActivityInfo?.PackageName;
            if (!string.IsNullOrEmpty(pkg))
                context.GrantUriPermission(pkg, uri,
                    ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission);
        }

        try
        {
            context.StartActivity(Intent.CreateChooser(intent, intentString));
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }
}
#endif
