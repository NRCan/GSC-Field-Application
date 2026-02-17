using System;
using System.Threading;
using System.Threading.Tasks;

namespace GSCFieldApp.Services.Abstraction
{
    /// <summary>
    /// Reconciles photo edits after returning from an external editor.
    /// On Android it can scan MediaStore for newly saved images, copy/import them into the app area,
    /// and update your data model. On other platforms it's a no-op.
    /// </summary>
    public interface IDocumentRefreshService
    {
        /// <summary>
        /// Record the moment just before launching the editor (or app going background).
        /// </summary>
        void MarkBackgroundTimestamp(DateTime utcNow);

        /// <summary>
        /// Called when the app resumes; reconcile any edits that happened while away.
        /// </summary>
        Task RefreshIfNeededAsync(DateTime sinceUtc, CancellationToken ct = default);
    }
}