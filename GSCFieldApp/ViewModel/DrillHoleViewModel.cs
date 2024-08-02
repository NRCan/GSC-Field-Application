using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GSCFieldApp.Models;
using GSCFieldApp.Services.DatabaseServices;
using GSCFieldApp.Controls;
using GSCFieldApp.Views;
using GSCFieldApp.Services;
using static GSCFieldApp.Dictionaries.DatabaseLiterals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using CommunityToolkit.Maui.Alerts;
using System.Security.Cryptography;
namespace GSCFieldApp.ViewModel
{
    [QueryProperty(nameof(DrillHole), nameof(DrillHole))]
    [QueryProperty(nameof(FieldLocation), nameof(FieldLocation))]
    public partial class DrillHoleViewModel: FieldAppPageHelper
    {
        #region INIT

        private DrillHole _model = new DrillHole();

        #endregion

        #region PROPERTIES

        [ObservableProperty]
        private DrillHole _drillHole;

        [ObservableProperty]
        private FieldLocation _fieldLocation;

        public DrillHole Model { get { return _model; } set { _model = value; } }

        public bool DrillHoleContextVisibility
        {
            get { return Preferences.Get(nameof(DrillHoleContextVisibility), true); }
            set { Preferences.Set(nameof(DrillHoleContextVisibility), value); }
        }

        public bool DrillHoleMetricsVisibility
        {
            get { return Preferences.Get(nameof(DrillHoleMetricsVisibility), true); }
            set { Preferences.Set(nameof(DrillHoleMetricsVisibility), value); }
        }

        public bool DrillHoleLogVisibility
        {
            get { return Preferences.Get(nameof(DrillHoleLogVisibility), true); }
            set { Preferences.Set(nameof(DrillHoleLogVisibility), value); }
        }

        public bool DrillHoleGeneralVisibility
        {
            get { return Preferences.Get(nameof(DrillHoleGeneralVisibility), true); }
            set { Preferences.Set(nameof(DrillHoleGeneralVisibility), value); }
        }

        #endregion

        #region RELAYS
        [RelayCommand]
        public async Task Back()
        {
            //Exit
            await NavigateToFieldNotes(TableNames.drill, false);

        }

        #endregion

        #region METHODS
        #endregion
    }
}
