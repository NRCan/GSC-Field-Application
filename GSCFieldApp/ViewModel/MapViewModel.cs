using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSCFieldApp.Views;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;

namespace GSCFieldApp.ViewModel
{
    public partial class MapViewModel: ObservableObject
    {

        public MapViewModel()
        {

        }

        [RelayCommand]
        async Task AddStation()
        {
            await Shell.Current.GoToAsync($"{nameof(StationPage)}");
        }

    }
}
