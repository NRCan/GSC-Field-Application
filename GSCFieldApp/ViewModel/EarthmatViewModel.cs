using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSCFieldApp.ViewModel
{
    public partial class EarthmatViewModel : ObservableObject
    {
        public EarthmatViewModel() { }

        #region RELAY COMMANDS
        [RelayCommand]
        async Task Save()
        {

        }

        #endregion
    }
}
