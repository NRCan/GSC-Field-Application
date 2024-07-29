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

namespace GSCFieldApp.ViewModel
{
    public class AboutPageViewModel: ObservableObject
    {
        // Launcher.OpenAsync is provided by Essentials.
        public ICommand TapCommand => new Command<string>(async (url) => await Launcher.OpenAsync(url));

        public string AppVersion => AppInfo.Current.Version.ToString();

        public string AppDBVersion => DBVersion.ToString();
    }
}
