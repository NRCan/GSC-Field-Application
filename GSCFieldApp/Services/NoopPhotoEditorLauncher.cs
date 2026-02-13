
using GSCFieldApp.Services.Abstraction;
using System.Threading;
using System.Threading.Tasks;

namespace GSCFieldApp.Services;

public sealed class NoopPhotoEditorLauncher : IPhotoEditorLauncher
{
    public Task<bool> EditAsync(string absoluteImagePath, CancellationToken ct = default)
        => Task.FromResult(false);
}

