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
using NetTopologySuite.Geometries;

namespace GSCFieldApp.ViewModel
{
    [QueryProperty(nameof(Linework), nameof(Linework))]
    [QueryProperty(nameof(LineString), nameof(LineString))]
    [QueryProperty(nameof(Metadata), nameof(Metadata))]
    public partial class LineworkViewModel : FieldAppPageHelper
    {
        #region INIT
        //Database
        private Linework _model = new Linework();

        //UI
        private ComboBox _lineworkType = new ComboBox();
        private ComboBox _lineworkConfidence = new ComboBox();
        private ComboBox _lineworkColor = new ComboBox();

        private ComboBox _lineworkTypeColor = new ComboBox(); // Only used to gather a list of line types and their symbols

        #endregion

        #region PROPERTIES

        [ObservableProperty]
        private Linework _linework;

        [ObservableProperty]
        private LineString _lineString;

        [ObservableProperty]
        private Metadata _metadata;

        public Linework Model { get { return _model; } set { _model = value; } }

        public ComboBox LineworkType { get { return _lineworkType; } set { _lineworkType = value; } }
        public ComboBox LineworkConfidence { get { return _lineworkConfidence; } set { _lineworkConfidence = value; } }
        public ComboBox LineworkColor { get { return _lineworkColor; } set { _lineworkColor = value; } }
        #endregion

        #region RELAYS

        [RelayCommand]
        public async Task Back()
        {

            if (_linework == null)
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
            if (_model.LineID != 0)
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
            if (_model.IsMapPageQuick)
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
            if (_linework == null || _linework.IsMapPageQuick == null ||_linework.IsMapPageQuick)
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

            _lineworkType = await FillAPicker(FieldLineworkType);
            _lineworkConfidence = await FillAPicker(FieldLineworkConfidence);
            _lineworkColor = await FillColorPicker(FieldLineworkSymbol);
            _lineworkTypeColor = await FillColorPicker(FieldLineworkType);

            OnPropertyChanged(nameof(LineworkType));
            OnPropertyChanged(nameof(LineworkConfidence));
            OnPropertyChanged(nameof(LineworkColor));
        }

        /// <summary>
        /// Will fill the project type combobox
        /// </summary>
        private async Task<ComboBox> FillAPicker(string fieldName)
        {
            //Make sure to user default database rather then the prefered one. This one will always be there.
            return await da.GetComboboxListWithVocabAsync(TableLinework, fieldName);

        }

        /// <summary>
        /// Will fill the project type combobox
        /// </summary>
        private async Task<ComboBox> FillColorPicker(string fieldName)
        {
            //Get default colors
            List<Vocabularies> vocs = await da.GetPicklistValuesAsync(TableLinework, fieldName, null, true);

            //Return converted list to combobox
            return da.GetComboboxListFromVocab(vocs, true);

        }

        /// <summary>
        /// Will initialize the model with needed calculated fields
        /// </summary>
        /// <returns></returns>
        public async Task InitModel()
        {
            if (Model != null && Model.LineID == 0 && _linework == null)
            {
                //Get current application version
                _model.LineIDName = await idCalculator.CalculateLineworkAliasAsync(DateTime.Now);
                OnPropertyChanged(nameof(Model));

                //Coming from map page, keep for navitation after saving
                _model.IsMapPageQuick = true;

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

            //Enforce color choice based on default linetype
            SelectColorBasedOnLineType();

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
                List<ComboBoxItem> lineColorItem = _lineworkTypeColor.cboxItems.Where(x=>x.itemName == _lineworkType.cboxItems.ElementAt(_lineworkType.cboxDefaultItemIndex).itemName).ToList();
                if (lineColorItem != null && lineColorItem.Count > 0)
                {
                    _model.LineSymbol = lineColorItem[0].itemValue;
                }
               
            }
            else if (_model.LineType == null)
            {
                //Save default to grey
                _model.LineSymbol = Mapsui.Styles.Color.Grey.ToString();
            }

            //Set geometry
            if (_lineString != null)
            {
                GeopackageService geoService = new GeopackageService();
                _model.LineGeom = geoService.CreateByteGeometryLine(_lineString);
            }

            //Foreign key
            if (_metadata != null && _metadata.MetaID > 0)
            {
                _model.LineMetaID = _metadata.MetaID;
            }
            else
            {
                _model.LineMetaID = 1;
            }
        }

        /// <summary>
        /// Change chosen color based on selected linetype
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SelectColorBasedOnLineType()
        {
            if (_lineworkType != null && _lineworkType.cboxItems.Count() > 0)
            {
                //Find same symbol color in picklist
                List<ComboBoxItem> lineColorItem = _lineworkTypeColor.cboxItems.Where(x => x.itemName == _lineworkType.cboxItems.ElementAt(_lineworkType.cboxDefaultItemIndex).itemName).ToList();
                if (lineColorItem != null && lineColorItem.Count > 0)
                {
                    foreach (ComboBoxItem colors in _lineworkColor.cboxItems)
                    {
                        if (colors.itemValue == lineColorItem[0].itemValue)
                        {
                            _lineworkColor.cboxDefaultItemIndex = _lineworkColor.cboxItems.IndexOf(colors);
                            OnPropertyChanged(nameof(LineworkColor));
                            break;
                        }
                    }

                }
            }


        }
        #endregion



    }
}
