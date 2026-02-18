using GSCFieldApp.Services.Abstraction;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GSCFieldApp.Services
{
    public sealed class NoopDocumentRefreshService : IDocumentRefreshService
    {
        public static event EventHandler<string> newAnnotatedDocument; //This event is triggered when a document is edited in an external editor and a new copy is created and available for thumbnail refresh.

        public void MarkBackgroundTimestamp(DateTime utcNow) { /* no-op */ }
        public Task RefreshIfNeededAsync(DateTime sinceUtc, CancellationToken ct = default)
            => Task.CompletedTask;
    }
}