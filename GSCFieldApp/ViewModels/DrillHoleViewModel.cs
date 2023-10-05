using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;
using GSCFieldApp.Models;
using GSCFieldApp.Services.DatabaseServices;
using System.Text.RegularExpressions;
using Windows.UI.Xaml.Controls;
using GSCFieldApp.Dictionaries;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml;
using GSCFieldApp.Themes;

namespace GSCFieldApp.ViewModels
{
    public class DrillHoleViewModel: ViewModelBase
    {
        #region INITIALIZATION

        public FieldNotes existingDataDetail;

        //UI
        private string _notes = string.Empty;

        public DrillHoleViewModel(FieldNotes inReportModel) 
        {
            existingDataDetail = inReportModel;
        }

        #endregion

        #region PROPERTIES
        public string Notes { get { return _notes; } set { _notes = value; } }

        #endregion
    }
}
