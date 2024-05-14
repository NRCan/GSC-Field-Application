using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace GSCFieldApp.ViewModel
{
    [QueryProperty("Text", "Text")]
    public partial class DetailVieModel : ObservableObject
    {
        [ObservableProperty]
        string text;

        [RelayCommand]
        async Task GoBack()
        { 
            //Navigate backward (../.. will navigate two pages back)"
            await Shell.Current.GoToAsync("../");    
        }
    }
}
