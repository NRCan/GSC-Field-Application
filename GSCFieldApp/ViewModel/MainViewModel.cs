using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace GSCFieldApp.ViewModel
{
    public partial class MainViewModel : ObservableObject
    {
        public MainViewModel()
        {
            Items = new ObservableCollection<string>();
        }

        [ObservableProperty]
        ObservableCollection<string> items;

        [ObservableProperty]
        string _text;

        ///Below is what is should have looked like before installing
        ///communityToolkit mvvm

        //public string Text  
        //{
        //    get { return _text; }
        //    set 
        //    {
        //        _text = value; 
        //        OnPropertyChanged(nameof(Text)); 
        //    }
        //}
        //public event PropertyChangedEventHandler PropertyChanged;
        //void OnPropertyChanged(string name)=>
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        [RelayCommand]
        void Add()
        {
            if (string.IsNullOrWhiteSpace(Text))
            {
                return;
            }
            Items.Add(Text);
            Text = string.Empty;
        }

        [RelayCommand]
        void Delete(string s)
        {
            if (Items.Contains(s))
            {
                Items.Remove(s);
            }
        }

        [RelayCommand]
        async Task Tap(string s)
        {
            await Shell.Current.GoToAsync($"{nameof(DetailPage)}?Text={s}");
        }

    }
}
