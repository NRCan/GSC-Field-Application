using GSCFieldApp.Services.Abstraction;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GSCFieldApp.Services
{
    public sealed class NoopDocumentRefreshService : IDocumentRefreshService
    {
        public void MarkBackgroundTimestamp(DateTime utcNow) { /* no-op */ }
        public Task RefreshIfNeededAsync(DateTime sinceUtc, CancellationToken ct = default)
            => Task.CompletedTask;
    }
}