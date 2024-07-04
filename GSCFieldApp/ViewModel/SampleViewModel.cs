using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GSCFieldApp.Models;
using GSCFieldApp.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSCFieldApp.ViewModel
{
    [QueryProperty(nameof(Earthmaterial), nameof(Earthmaterial))]
    [QueryProperty(nameof(Sample), nameof(Sample))]
    public partial class SampleViewModel : ObservableObject
    {
        #region PROPERTIES

        [ObservableProperty]
        private Earthmaterial _earthmaterial;

        [ObservableProperty]
        private Station _station;
        #endregion

        public SampleViewModel() { }

        #region RELAYS
        /// <summary>
        /// Back button command
        /// </summary>
        /// <returns></returns>
        [RelayCommand]

        public async Task Back()
        {
            //Android when navigating back, ham menu disapears if / isn't added to path
            await Shell.Current.GoToAsync($"{nameof(FieldNotesPage)}/");
        }

        #endregion
    }
}
