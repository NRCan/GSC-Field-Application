using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GSCFieldApp.Views;
using GSCFieldApp.Services.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSCFieldApp.ViewModel
{
    public partial class StationViewModel: ObservableObject
    {
        public StationViewModel()
        {

        }

        [RelayCommand]
        async Task Save()
        {
            //Navigate backward (../.. will navigate two pages back)"
            await Shell.Current.GoToAsync($"{nameof(MapPage)}");
        }
    }
}
