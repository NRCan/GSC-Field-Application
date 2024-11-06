using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GSCFieldApp.Models;
using GSCFieldApp.Services.DatabaseServices;
using GSCFieldApp.Controls;
using GSCFieldApp.Views;
using GSCFieldApp.Services;
using static GSCFieldApp.Dictionaries.DatabaseLiterals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using CommunityToolkit.Maui.Alerts;

namespace GSCFieldApp.ViewModel
{
    [QueryProperty(nameof(Linework), nameof(Linework))]
    public partial class LineworkViewModel : FieldAppPageHelper
    {
        #region INIT
        //Database
        private Linework _model = new Linework();

        //UI
        private ComboBox _lineworkType = new ComboBox();
        private ComboBox _lineworkConfidence = new ComboBox();

        #endregion

        #region PROPERTIES

        [ObservableProperty]
        private Linework _linework;

        public Linework Model { get { return _model; } set { _model = value; } }

        public ComboBox LineworkType { get { return _lineworkType; } set { _lineworkType = value; } }
        public ComboBox LineworkConfidence { get { return _lineworkConfidence; } set { _lineworkConfidence = value; } }
 
        #endregion

        #region RELAYS

        [RelayCommand]
        public async Task Back()
        {
            //Make sure to delete station and location records if user is coming from map page
            if (_model != null && _model.LineID == 0)
            {
                //Delete without forced pop-up warning and question
                await commandServ.DeleteDatabaseItemCommand(TableNames.linework, _linework.LineIDName, _linework.LineID, true);
            }

            if (_linework == null || _linework.IsMapPageQuick)
            {
                //Exit on map
                await Shell.Current.GoToAsync($"//{nameof(MapPage)}/");

            }
            else
            {
                //Exit in field notes
                await NavigateToFieldNotes(TableNames.linework);
            }
        }

        /// <summary>
        /// Save button command
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        async Task Save()
        {
            await SetModel();

            //Validate if new entry or update
            if (_linework != null && _model.LineID != 0)
            {

                await da.SaveItemAsync(Model, true);
            }
            else
            {
                //New entry coming from parent form
                //Insert new record
                await da.SaveItemAsync(Model, false);
            }

            //Close to be sure
            await da.CloseConnectionAsync();

            //Exit or stay in map page if quick photo
            if (_linework != null && _linework.IsMapPageQuick)
            {
                await Shell.Current.GoToAsync($"//{nameof(MapPage)}/");
            }
            else
            {
                await NavigateToFieldNotes(TableNames.linework);
            }
        }

        [RelayCommand]
        async Task SaveDelete()
        {
            if (_model.LineID != 0)
            {
                await commandServ.DeleteDatabaseItemCommand(TableNames.linework, _model.LineIDName, _model.LineID);
            }

            //Exit
            if (_linework.IsMapPageQuick)
            {
                //Exit on map
                await Shell.Current.GoToAsync($"//{nameof(MapPage)}/");

            }
            else
            {
                //Exit in field notes
                await NavigateToFieldNotes(TableNames.linework);
            }


        }

        /// <summary>
        /// Save button command
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        async Task SaveStay()
        {

            //Display a warning to user
            await Shell.Current.DisplayAlert(LocalizationResourceManager["DisplayAlertNotAllowed"].ToString(),
                LocalizationResourceManager["DisplayAlertNotAllowedContent"].ToString(),
                LocalizationResourceManager["GenericButtonOk"].ToString());

        }

        #endregion

        public LineworkViewModel()
        {

        }

        #region METHODS

        /// <summary>
        /// Initialize all pickers. 
        /// </summary>
        /// <returns></returns>
        public async Task FillPickers()
        {
            //Connect to db
            currentConnection = da.GetConnectionFromPath(da.PreferedDatabasePath);

            //First order pickers
            _lineworkConfidence = await FillAPicker(FieldLineworkConfidence);
            OnPropertyChanged(nameof(LineworkConfidence));

            _lineworkType = await FillAPicker(FieldLineworkType);
            OnPropertyChanged(nameof(LineworkType));

            await currentConnection.CloseAsync();
        }

        /// <summary>
        /// Will fill the project type combobox
        /// </summary>
        private async Task<ComboBox> FillAPicker(string fieldName, string extraField = "")
        {
            //Make sure to user default database rather then the prefered one. This one will always be there.
            return await da.GetComboboxListWithVocabAsync(TableLinework, fieldName, extraField);

        }

        /// <summary>
        /// Will initialize the model with needed calculated fields
        /// </summary>
        /// <returns></returns>
        public async Task InitModel()
        {
            if (Model != null && Model.LineID == 0 && _linework != null)
            {
                //Get current application version
                Model.LineIDName = await idCalculator.CalculateLineworkAliasAsync(DateTime.Now);
                OnPropertyChanged(nameof(Model));

            }
        }

        /// <summary>
        /// Will refill the form with existing values for update/editing purposes
        /// </summary>
        /// <returns></returns>
        public async Task Load()
        {

            if (_linework != null && _linework.LineIDName != string.Empty)
            {
                //Set model like actual record
                _model = _linework;

                //Refresh
                OnPropertyChanged(nameof(Model));


            }

        }

        /// <summary>
        /// Will fill out missing fields for model. Default auto-calculated values
        /// Done before actually saving
        /// </summary>
        private async Task SetModel()
        {
            //Set proper color schema if nothing available
            if (_model.LineType != null && (_model.LineSymbol == null || _model.LineSymbol == string.Empty))
            {
                SQLiteAsyncConnection currentConnection = new SQLiteAsyncConnection(da.PreferedDatabasePath);
                List<Vocabularies> vocab = await currentConnection.Table<Vocabularies>().Where(col => col.CodedTheme == vocabularyLineType && col.Code == _model.LineType).ToListAsync();

                if (vocab != null && vocab.Count > 0)
                {
                    _model.LineSymbol = vocab[0].Symbol;
                }
                await currentConnection.CloseAsync();
            }
            else if (_model.LineType == null)
            {
                //Save default to grey
                _model.LineSymbol = Mapsui.Styles.Color.Grey.ToString();
            }
        }

        #endregion
    }
}
