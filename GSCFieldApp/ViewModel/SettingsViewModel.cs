using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSCFieldApp.Services;

namespace GSCFieldApp.ViewModel
{
    public partial class SettingsViewModel: ObservableObject
    {
        #region PROPERTIES
        public bool EarthMaterialVisible
        {
            get { return Preferences.Get(nameof(EarthMaterialVisible), true); }
            set { Preferences.Set(nameof(EarthMaterialVisible), value); }
        }

        public bool DocumentVisible
        {
            get { return Preferences.Get(nameof(DocumentVisible), true); }
            set { Preferences.Set(nameof(DocumentVisible), value); }
        }

        public bool SampleVisible
        {
            get { return Preferences.Get(nameof(SampleVisible), true); }
            set { Preferences.Set(nameof(SampleVisible), value); }
        }

        public bool FossilVisible
        {
            get { return Preferences.Get(nameof(FossilVisible), true); }
            set { Preferences.Set(nameof(FossilVisible), value); }
        }

        public bool MineralVisible
        {
            get { return Preferences.Get(nameof(MineralVisible), true); }
            set { Preferences.Set(nameof(MineralVisible), value); }
        }

        public bool MineralizationVisible
        {
            get { return Preferences.Get(nameof(MineralizationVisible), true); }
            set { Preferences.Set(nameof(MineralizationVisible), value); }
        }

        public bool StructureVisible
        {
            get { return Preferences.Get(nameof(StructureVisible), true); }
            set { Preferences.Set(nameof(StructureVisible), value); }
        }

        public bool DrillHoleVisible
        {
            get { return Preferences.Get(nameof(DrillHoleVisible), true); }
            set { Preferences.Set(nameof(DrillHoleVisible), value); }
        }

        public bool PaleoflowVisible
        {
            get { return Preferences.Get(nameof(PaleoflowVisible), true); }
            set { Preferences.Set(nameof(PaleoflowVisible), value); }
        }

        public bool EnvironmentVisible
        {
            get { return Preferences.Get(nameof(EnvironmentVisible), true); }
            set { Preferences.Set(nameof(EnvironmentVisible), value); }
        }

        /// <summary>
        /// Property saved in the about page
        /// </summary>
        public bool DeveloperModeActivated
        {
            get { return Preferences.Get(nameof(DeveloperModeActivated), false); }
            set { }
        }

        #endregion

        #region RELAYS
        [RelayCommand]
        public async Task DoDebugLogBackup()
        {
            AppFileServices fileServices = new AppFileServices();
            await fileServices.SaveBugLogFile(CancellationToken.None);
        }
        #endregion

        public SettingsViewModel() { }

        #region METHODS

        /// <summary>
        /// Simple method to hide/show some section
        /// </summary>
        public void SettingRefresh()
        {
            OnPropertyChanged(nameof(DeveloperModeActivated));
        }

        #endregion
    }
}
