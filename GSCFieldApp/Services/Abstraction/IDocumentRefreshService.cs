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
        public static event EventHandler<string> newAnnotatedDocument; //This event is triggered when a document is edited in an external editor and a new copy is created and available for thumbnail refresh.

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