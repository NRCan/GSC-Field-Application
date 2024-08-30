using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mapsui.UI;
using Microsoft.Maui.Controls.PlatformConfiguration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static GSCFieldApp.Dictionaries.DatabaseLiterals;
using GSCFieldApp.Services;

namespace GSCFieldApp.ViewModel
{
    public partial class AboutPageViewModel: ObservableObject
    {
        //Localize
        public LocalizationResourceManager LocalizationResourceManager
        => LocalizationResourceManager.Instance; // Will be used for in code dynamic local strings

        // Launcher.OpenAsync is provided by Essentials.
        public ICommand TapCommand => new Command<string>(async (url) => await Launcher.OpenAsync(url));

        public string AppVersion => AppInfo.Current.Version.ToString();

        public string AppDBVersion => DBVersion.ToString();

        private int _logoRotation = 0;

        public int LogoRotation { get { return _logoRotation; } set { _logoRotation = value; }  }

        public bool DeveloperModeActivated
        {
            get { return Preferences.Get(nameof(DeveloperModeActivated), false); }
            set { Preferences.Set(nameof(DeveloperModeActivated), value); }
        }

        [RelayCommand]
        public async Task LogoTapped()
        {
            //Each time logo is tapped, rotate it
            _logoRotation = _logoRotation + 36;
            OnPropertyChanged(nameof(LogoRotation));

            //When it's been rotated for a full circle, enable dev mode
            if (_logoRotation % 360 == 0)
            {
                DeveloperModeActivated = !DeveloperModeActivated;
                OnPropertyChanged(nameof(DeveloperModeActivated));

                //Show toast to tell user the good news
                if (DeveloperModeActivated)
                {
                    await Toast.Make(LocalizationResourceManager["ToastDevModeActivated"].ToString()).Show(CancellationToken.None);
                }
                else
                {
                    await Toast.Make(LocalizationResourceManager["ToastDevModeDeactivated"].ToString()).Show(CancellationToken.None);
                }

            }
        }
    }
}
