using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSCFieldApp.Services;
using GSCFieldApp.Dictionaries;
using GSCFieldApp.Services.DatabaseServices;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Storage;
using System.Threading;

namespace GSCFieldApp.ViewModel
{
    public partial class SettingsViewModel: ObservableObject
    {
        #region PROPERTIES
        public LocalizationResourceManager LocalizationResourceManager
            => LocalizationResourceManager.Instance; // Will be used for in code dynamic local strings


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

        public bool LocationVisible
        {
            get { return Preferences.Get(nameof(LocationVisible), true); }
            set { Preferences.Set(nameof(LocationVisible), value); }
        }

        public bool LineworkVisible
        {
            get { return Preferences.Get(nameof(LineworkVisible), true); }
            set { Preferences.Set(nameof(LineworkVisible), value); }
        }

        public bool GPSLogEnabled
        {
            get { return Preferences.Get(nameof(GPSLogEnabled), false); }
            set { Preferences.Set(nameof(GPSLogEnabled), value); }
        }

        public bool GPSHighRateEnabled
        {
            get { return Preferences.Get(nameof(GPSHighRateEnabled), false); }
            set { Preferences.Set(nameof(GPSHighRateEnabled), value); }
        }

        public string GPSLogFilePath
        {
            get { return Preferences.Get(nameof(GPSLogFilePath), ""); }
            set { Preferences.Set(nameof(GPSLogFilePath), value); }
        }

        public bool LocationFollowEnabled
        {
            get { return Preferences.Get(nameof(LocationFollowEnabled), false); }
            set { Preferences.Set(nameof(LocationFollowEnabled), value); }
        }

        public bool CustomSampleNameEnabled
        {
            get { return Preferences.Get(nameof(CustomSampleNameEnabled), true); }
            set { Preferences.Set(nameof(CustomSampleNameEnabled), value); }
        }

        /// <summary>
        /// Property saved in the about page
        /// </summary>
        public bool DeveloperModeActivated
        {
            get { return Preferences.Get(nameof(DeveloperModeActivated), false); }
            set { }
        }

        private bool _isWaiting = false;
        public bool IsWaiting { get { return _isWaiting; } set { _isWaiting = value; } }

        #endregion

        #region RELAYS
        [RelayCommand]
        public async Task DoDebugLogBackup()
        {
            AppFileServices fileServices = new AppFileServices();
            await fileServices.SaveLogFile(ApplicationLiterals.errorLogFileNameExt, CancellationToken.None);
        }

        [RelayCommand]
        public async Task DoGPSLogBackup()
        {
            AppFileServices fileServices = new AppFileServices();
            GPSLogFilePath = await fileServices.SaveLogFile(ApplicationLiterals.gpsLogFileNameExt, CancellationToken.None);
        }

        [RelayCommand]
        public async Task RepairGeometry()
        {
            _isWaiting = true;
            OnPropertyChanged(nameof(IsWaiting));

            GeopackageService geopackageService = new GeopackageService();
            bool repairedCompletedWithoutErrors = await geopackageService.RepairLocationGeometry();

            //Use Toast to show card in window interface or system like notification rather then modal alert popup.
            if (repairedCompletedWithoutErrors)
            {
                string toastText = LocalizationResourceManager["ToastLocationRepaired"].ToString();

                await Toast.Make(toastText).Show(CancellationToken.None);
            }
            else
            {
                string toastText = LocalizationResourceManager["ToastLocationRepairedFailed"].ToString();
                await Toast.Make(toastText).Show(CancellationToken.None);
            }

            _isWaiting = false;
            OnPropertyChanged(nameof(IsWaiting));
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
