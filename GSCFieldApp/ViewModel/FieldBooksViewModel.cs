using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using GSCFieldApp.Services.DatabaseServices;
using GSCFieldApp.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GSCFieldApp.ViewModel
{
    public partial class FieldBooksViewModel
    {
        public FieldBooksViewModel() 
        {

        }

        [RelayCommand]
        async Task AddFieldBook()
        {
            //TEST db resource to file
            //DataAccess dataAccess = new DataAccess();
            //dataAccess.CreateDatabaseFromResource();

            await Shell.Current.GoToAsync($"{nameof(FieldBookPage)}");
        }

        [RelayCommand]
        async Task UploadFieldBook()
        {
            //await DisplayAlert("Alert", "Not yet implemented", "OK");
        }

        [RelayCommand]
        async Task DownloadFieldBook()
        {
            //await DisplayAlert("Alert", "Not yet implemented", "OK");
        }

        [RelayCommand]
        async Task UpdateFieldBook()
        {
            //await DisplayAlert("Alert", "Not yet implemented", "OK");
        }
    }
}
