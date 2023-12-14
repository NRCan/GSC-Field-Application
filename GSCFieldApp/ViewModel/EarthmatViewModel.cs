using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GSCFieldApp.Views;
using GSCFieldApp.Services.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSCFieldApp.Models;
using GSCFieldApp.Dictionaries;
using GSCFieldApp.Themes;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.Maui.ApplicationModel.Communication;
using System.Xml.Linq;
using System.Reflection;
using SQLite;
using System.Text.RegularExpressions;

namespace GSCFieldApp.ViewModel
{
    [QueryProperty(nameof(Earthmaterial), nameof(Earthmaterial))]
    [QueryProperty(nameof(Station), nameof(Station))]
    public partial class EarthmatViewModel : ObservableObject
    {
        #region INIT

        DataAccess da = new DataAccess();
        SQLiteAsyncConnection currentConnection;

        private Earthmaterial _model = new Earthmaterial();
        public DataIDCalculation idCalculator = new DataIDCalculation();
        ConcatenatedCombobox concat = new ConcatenatedCombobox(); //Use to concatenate values

        private bool _isLithoGroupListVisible = false;
        private bool _isLithoDetailListVisible = false;
        private List<string> _lihthoDetailSearchResults = new List<string>();
        private List<string> _lihthoGroupSearchResults = new List<string>();
        private IEnumerable<Vocabularies> _litho_detail_vocab; //Default list to keep in order to not redo the query each time
        private IEnumerable<Vocabularies> _litho_group_vocab; //Default list to keep in order to not redo the query each time
        //private string _selectedLithoGroup = string.Empty;
        private ComboBox _earthLithQualifier = new ComboBox();
        private ComboBoxItem _selectedEarthLithQualifier = new ComboBoxItem();
        private ObservableCollection<ComboBoxItem> _qualifierCollection = new ObservableCollection<ComboBoxItem>();

        private List<Lithology> lithologies = new List<Lithology>();

        #endregion

        #region PROPERTIES

        [ObservableProperty]
        private Earthmaterial _earthmaterial;

        [ObservableProperty]
        private Station _station;

        public Earthmaterial Model { get { return _model; } set { _model = value; } }
        public bool EMLithoVisibility
        {
            get { return Preferences.Get(nameof(EMLithoVisibility), true); }
            set { Preferences.Set(nameof(EMLithoVisibility), value); }
        }
        public bool EarthLithQualifierVisibility
        {
            get { return Preferences.Get(nameof(EarthLithQualifierVisibility), true); }
            set { Preferences.Set(nameof(EarthLithQualifierVisibility), value); }
        }

        //public string SelectedLithoGroup 
        //{
        //    get
        //    {
        //        return _selectedLithoGroup;
        //    }
        //    set
        //    {
        //        _selectedLithoGroup = value;
        //        OnPropertyChanged(nameof(SelectedLithoGroup));
        //    }
        //}

        public List<string> LihthoDetailSearchResults
        {
            get
            {
                return _lihthoDetailSearchResults;
            }
            set
            {
                _lihthoDetailSearchResults = value;
                OnPropertyChanged(nameof(LihthoDetailSearchResults));
            }
        }

        public List<string> LihthoGroupSearchResults
        {
            get
            {
                return _lihthoGroupSearchResults;
            }
            set
            {
                _lihthoGroupSearchResults = value;
                OnPropertyChanged(nameof(LihthoGroupSearchResults));
            }
        }

        public bool isLithoGroupListVisible { get { return _isLithoGroupListVisible; }  set { _isLithoGroupListVisible = value; OnPropertyChanged(nameof(isLithoGroupListVisible)); } }
        public bool isLithoDetailListVisible { get { return _isLithoDetailListVisible; } set { _isLithoDetailListVisible = value; OnPropertyChanged(nameof(isLithoDetailListVisible)); } }

        public ComboBox EarthLithQualifier { get { return _earthLithQualifier; } set { _earthLithQualifier = value; } }
        public ComboBoxItem SelectedEarthLithQualifier
        {
            get
            {
                return _selectedEarthLithQualifier;
            }
            set
            {
                if (_selectedEarthLithQualifier != value)
                {
                    if (_qualifierCollection != null)
                    {
                        if (_qualifierCollection.Count > 0 && _qualifierCollection[0] == null)
                        {
                            _qualifierCollection.RemoveAt(0);
                        }
                        _qualifierCollection.Add(value);
                        _selectedEarthLithQualifier = value;
                        OnPropertyChanged(nameof(EarthLithQualifierCollection));
                    }


                }

            }
        }
        public ObservableCollection<ComboBoxItem> EarthLithQualifierCollection { get { return _qualifierCollection; } set { _qualifierCollection = value; } }



        #endregion
        public EarthmatViewModel() 
        {
            currentConnection = da.GetConnectionFromPath(da.PreferedDatabasePath);

            FillSearchListAsync();
        }

        #region RELAY COMMANDS
        [RelayCommand]
        async Task Save()
        {
            //Fill out missing values in model
            await SetModelAsync();

            //Validate if new entry or update
            if (_earthmaterial != null && _earthmaterial.EarthMatName != string.Empty)
            {
                await da.SaveItemAsync(Model, true);
            }
            else
            {
                //Insert new record
                await da.SaveItemAsync(Model, false);
            }

            //Close to be sure
            await da.CloseConnectionAsync();

            //Exit
            await Shell.Current.GoToAsync($"{nameof(FieldNotesPage)}");
        }

        /// <summary>
        /// Will delete a selected item in quality collection box.
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        async Task Delete(ComboBoxItem item)
        {
            if (_qualifierCollection.Contains(item))
            {
                _qualifierCollection.Remove(item);
                OnPropertyChanged(nameof(EarthLithQualifierCollection));
            }

        }

        [RelayCommand]
        public async Task Hide(string visibilityObjectName)
        {
            //Use reflection to parse incoming block to hide
            PropertyInfo? prop = typeof(EarthmatViewModel).GetProperty(visibilityObjectName);

            if (prop != null)
            {
                bool propBool = (bool)prop.GetValue(this);

                // Reverse
                propBool = propBool ? false : true;

                prop.SetValue(this, propBool);
                OnPropertyChanged(visibilityObjectName);
            }

        }

        [RelayCommand]
        public async Task PerformDetailSearch(string searchText)
        {

            var search_term = searchText.ToLower();
            var results = _litho_detail_vocab.Where(i => i.Code.ToLower().Contains(search_term)).ToList();

            if (results.Count > 0)
            {
                _lihthoDetailSearchResults = new List<string>();
                foreach (Vocabularies tmp in results)
                {
                    if (!_lihthoDetailSearchResults.Contains(tmp.Code.ToString()))
                    {
                        _lihthoDetailSearchResults.Add(tmp.Code.ToString());
                    }
                }

                LihthoDetailSearchResults = _lihthoDetailSearchResults;

                isLithoDetailListVisible = true;
            }
            else
            {
                isLithoDetailListVisible = false;
            }

        }

        [RelayCommand]
        public async Task PerformGroupSearch(string searchText)
        {

            var search_term = searchText.ToLower();
            var results = _litho_group_vocab.Where(i => i.Code.ToLower().Contains(search_term)).ToList();

            if (results.Count > 0)
            {
                _lihthoGroupSearchResults = new List<string>();
                foreach (Vocabularies tmp in results)
                {
                    if (!_lihthoGroupSearchResults.Contains(tmp.Code.ToString()))
                    {
                        _lihthoGroupSearchResults.Add(tmp.Code.ToString());
                    }
                }

                LihthoGroupSearchResults = _lihthoGroupSearchResults;

                isLithoGroupListVisible = true;
            }
            else
            {
                isLithoGroupListVisible = false;
            }


        }

        #endregion

        #region METHODS

        /// <summary>
        /// Will fill the detail search list values
        /// </summary>
        /// <param name="in_vocab"></param>
        /// <param name="in_projectType"></param>
        /// <returns></returns>
        private async Task FillLithoSearchListAsync(List<Vocabularies> in_vocab, string in_projectType)
        {

            List<VocabularyManager> vocab_manager = await currentConnection.Table<VocabularyManager>().Where(theme => (theme.ThemeField == DatabaseLiterals.FieldEarthMatLithdetail) && (theme.ThemeProjectType.Contains(in_projectType))) .ToListAsync();
            _litho_detail_vocab = from v in in_vocab join vm in vocab_manager on v.CodedTheme equals vm.ThemeName orderby v.Code select v;

            var _lihthoSearchResults = new List<string>();
            foreach (Vocabularies tmp in _litho_detail_vocab)
            {
                if (!_lihthoSearchResults.Contains(tmp.Code.ToString()))
                {
                    _lihthoSearchResults.Add(tmp.Code.ToString());
                }

                IEnumerable<Lithology> existingDetails = lithologies.Where(l => l.GroupTypeCode == tmp.RelatedTo.ToString());
                if (existingDetails != null && existingDetails.Count() > 0)
                {
                    LithologyDetail detail = new LithologyDetail();
                    detail.DetailCode = tmp.Code.ToString();
                    existingDetails.First().lithologyDetails.Add(detail);

                }
            }

            LihthoDetailSearchResults = _lihthoSearchResults;

        }

        /// <summary>
        /// Will fill the group search list values
        /// </summary>
        /// <param name="in_vocab"></param>
        /// <param name="in_projectType"></param>
        /// <returns></returns>
        private async Task FillLithoGroupSearchListAsync(List<Vocabularies> in_vocab, string in_projectType)
        {

            List<VocabularyManager> vocab_manager = await currentConnection.Table<VocabularyManager>().Where(theme => (theme.ThemeField == DatabaseLiterals.FieldEarthMatLithgroup) && (theme.ThemeProjectType.Contains(in_projectType))).ToListAsync();
            _litho_group_vocab = from v in in_vocab join vm in vocab_manager on v.CodedTheme equals vm.ThemeName orderby v.Code select v;

            var _lihthoSearchResults = new List<string>();
            foreach (Vocabularies tmp in _litho_group_vocab)
            {
                if (!_lihthoSearchResults.Contains(tmp.Code.ToString()))
                {
                    _lihthoSearchResults.Add(tmp.Code.ToString());

                    IEnumerable<Lithology> existingGroupType = lithologies.Where(l => l.GroupTypeCode == tmp.Code.ToString());
                    if (existingGroupType == null || existingGroupType.Count() == 0)
                    {
                        lithologies.Add(new Lithology(tmp.Code.ToString()));
                    }

                }
            }

            LihthoDetailSearchResults = _lihthoSearchResults;

        }
        private async Task FillSearchListAsync()
        {
            //Prepare vocabulary
            List<Vocabularies> vocab = await currentConnection.Table<Vocabularies>().Where(vis => vis.Visibility == DatabaseLiterals.boolYes).ToListAsync();
            List<Metadata> meta = await currentConnection.Table<Metadata>().Where(metadata => metadata.MetaID == 1).ToListAsync();
            string currentProjectType = meta.First().FieldworkType.ToString();

            await FillLithoGroupSearchListAsync(vocab, currentProjectType);
            await FillLithoSearchListAsync(vocab, currentProjectType);
        }

        /// <summary>
        /// Will force a filtered down list of lithology details based
        /// on user selected group
        /// </summary>
        /// <param name="groupName"></param>
        public void RefineDetailListFromGroup(string groupName)
        {
            //Set group search bar
            _model.GroupType = groupName;
            OnPropertyChanged(nameof(Model));

            //Reset list
            _lihthoDetailSearchResults = new List<string>();

            //Get proper lith group
            IEnumerable<Lithology> existingGroupType = lithologies.Where(l => l.GroupTypeCode == groupName);
            if (existingGroupType != null && existingGroupType.Count() == 1)
            {
                foreach (LithologyDetail lDetail in existingGroupType.FirstOrDefault().lithologyDetails)
                {
                    if (!_lihthoDetailSearchResults.Contains(lDetail.DetailCode))
                    {
                        _lihthoDetailSearchResults.Add(lDetail.DetailCode);
                    }
                }
            }


            LihthoDetailSearchResults = _lihthoDetailSearchResults;

            isLithoDetailListVisible = true;

            
        }

        /// <summary>
        /// Will force a filtered down list of lithology group/types based
        /// on user selected detail (usually 1 value)
        /// </summary>
        /// <param name="groupName"></param>
        public void RefineGroupListFromDetail(string detailName)
        {
            //Reset list
            _lihthoGroupSearchResults = new List<string>();

            //Get proper lith group
            foreach (Lithology lith in lithologies)
            {

                foreach (LithologyDetail lDetail in lith.lithologyDetails)
                {
                    if (lDetail.DetailCode == detailName)
                    {
                        if (!_lihthoGroupSearchResults.Contains(lith.GroupTypeCode))
                        {
                            _lihthoGroupSearchResults.Add(lith.GroupTypeCode);
                        }
                    }

                }

            }

            LihthoGroupSearchResults = _lihthoGroupSearchResults;

            isLithoGroupListVisible = true;
        }

        /// <summary>
        /// Will fill all picker controls
        /// </summary>
        /// <returns></returns>
        public async Task FillPickers()
        {
            
        }

        /// <summary>
        /// Will fill all picker controls
        /// </summary>
        /// <returns></returns>
        public async Task Fill2ndRoundPickers()
        {
            //second round pickers
            if (_model.GroupType != string.Empty)
            {
                _earthLithQualifier = await FillAPicker(DatabaseLiterals.FieldEarthMatModComp, _model.EarthMatLithgroup);

                OnPropertyChanged(nameof(EarthLithQualifier));
            }

        }

        /// <summary>
        /// Will fill a needed picker control with vocabulary
        /// </summary>
        private async Task<ComboBox> FillAPicker(string fieldName, string extraField = "")
        {
            //Make sure to user default database rather then the prefered one. This one will always be there.
            return await da.GetComboboxListWithVocabAsync(DatabaseLiterals.TableEarthMat, fieldName, extraField);

        }

        /// <summary>
        /// Will fill out missing fields for model. Default auto-calculated values
        /// </summary>
        private async Task SetModelAsync()
        {
            //Make sure it's for a new field book
            if (Model.EarthMatID == 0)
            {
                //Get current application version
                Model.EarthMatStatID = _station.StationID;
                Model.EarthMatName = await idCalculator.CalculateEarthmatAliasAsync(_station.StationID, _station.StationAlias);
            }

            //Process pickers
            if (EarthLithQualifierCollection.Count > 0)
            {
                Model.EarthMatModComp = concat.PipeValues(EarthLithQualifierCollection); //process list of values so they are concatenated.
            }

        }

        /// <summary>
        /// Will refill the form with existing values for update/editing purposes
        /// </summary>
        /// <returns></returns>
        public async Task Load()
        {
            if (_earthmaterial != null && _earthmaterial.EarthMatName != string.Empty)
            {
                //Set model like actual record
                _model = _earthmaterial;

                //Refresh
                OnPropertyChanged(nameof(Model));


                //Piped value field
                await Fill2ndRoundPickers();
                await Load2nRound();

            }
        }

        /// <summary>
        /// Will make sure to load the second round of piclist
        /// that depends on the user selected litho group
        /// </summary>
        /// <returns></returns>
        public async Task Load2nRound()
        {
            if (_model.GroupType != string.Empty)
            {
                List<string> qualifiers = concat.UnpipeString(_earthmaterial.EarthMatModComp);
                _qualifierCollection.Clear(); //Clear any possible values first
                foreach (ComboBoxItem cbox in EarthLithQualifier.cboxItems)
                {
                    if (qualifiers.Contains(cbox.itemValue) && !_qualifierCollection.Contains(cbox))
                    {
                        _qualifierCollection.Add(cbox);
                    }
                }
                OnPropertyChanged(nameof(EarthLithQualifierCollection));
            }

        }

        #endregion

    }
}
