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
using System.Collections.ObjectModel;

namespace GSCFieldApp.ViewModel
{
    [QueryProperty(nameof(MineralAlteration), nameof(MineralAlteration))]
    [QueryProperty(nameof(Earthmaterial), nameof(Earthmaterial))]
    [QueryProperty(nameof(Station), nameof(Station))]
    public partial class MineralizationAlterationViewModel: FieldAppPageHelper
    {
        #region INIT
        #endregion

        #region PROPERTIES
        #endregion

        #region RELAYS
        [RelayCommand]
        public async Task Back()
        {
            //Exit 
            await NavigateToFieldNotes(TableNames.mineralization);

        }
        #endregion

        public MineralizationAlterationViewModel() { }

        #region METHODS
        #endregion
    }
}
