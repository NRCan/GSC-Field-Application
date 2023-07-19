using CommunityToolkit.Mvvm.Input;
using GSCFieldApp.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSCFieldApp.ViewModel
{
    public partial class FieldBookViewModel
    {
        public FieldBookViewModel()
        {

        }

        [RelayCommand]
        async Task Save()
        {
            //Navigate backward (../.. will navigate two pages back)"
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        async Task Back()
        {
            //Navigate backward (../.. will navigate two pages back)"
            await Shell.Current.GoToAsync("..");
        }

    }
}
