using CommunityToolkit.Mvvm.Input;
using GSCFieldApp.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSCFieldApp.Services.DatabaseServices;

namespace GSCFieldApp.ViewModel
{
    public partial class FieldBookViewModel
    {
        public FieldBookViewModel()
        {
            _ = ValidateDabaseExistence();
        }

        private async Task ValidateDabaseExistence()
        {
            DataAccess da = new DataAccess();
            await da.ValidateDatabase();
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
