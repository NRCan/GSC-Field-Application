using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using GSCFieldApp.Services.DatabaseServices;
using GSCFieldApp.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSCFieldApp.ViewModel
{
    public partial class FieldBooksViewModel: ObservableObject
    {
        public FieldBooksViewModel() 
        {

        }

        [RelayCommand]
        async Task AddFieldBook()
        {
            //TEST db resource to file
            DataAccess dataAccess = new DataAccess();
            dataAccess.CreateDatabaseFromResource();

            await Shell.Current.GoToAsync($"{nameof(FieldBook234Page)}");
        }
    }
}
