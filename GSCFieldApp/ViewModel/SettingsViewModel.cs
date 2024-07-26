using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSCFieldApp.ViewModel
{
    public partial class SettingsViewModel: ObservableObject
    {
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

        public bool DrillVisible
        {
            get { return Preferences.Get(nameof(DrillVisible), true); }
            set { Preferences.Set(nameof(DrillVisible), value); }
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


        public SettingsViewModel() { }  

    }
}
