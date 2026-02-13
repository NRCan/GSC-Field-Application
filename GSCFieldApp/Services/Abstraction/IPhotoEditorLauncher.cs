using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSCFieldApp.Services.Abstraction
{
    /// <summary>
    /// Interface to launch photo editor from the image tapped relay command
    /// </summary>
    public interface IPhotoEditorLauncher
    {
        Task<bool> EditAsync(string absoluteImagePath, CancellationToken ct = default);
    }

}
