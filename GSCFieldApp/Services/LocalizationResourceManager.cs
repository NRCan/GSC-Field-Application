using GSCFieldApp.Resources.Strings;
using System.ComponentModel;
using System.Globalization;

namespace GSCFieldApp.Services
{

    /// <summary>
    /// This class will be used to get in code dynamic localization
    /// </summary>
    public class LocalizationResourceManager: INotifyPropertyChanged
    {
        private LocalizationResourceManager()
        {
            LocalizableStrings.Culture = CultureInfo.CurrentCulture;
        }

        public static LocalizationResourceManager Instance { get; } = new();

        public object this[string resourceKey]
            => LocalizableStrings.ResourceManager.GetObject(resourceKey, LocalizableStrings.Culture) ?? Array.Empty<byte>();

        public event PropertyChangedEventHandler PropertyChanged;

        public void SetCulture(CultureInfo culture)
        {
            LocalizableStrings.Culture = culture;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }
    }
}
