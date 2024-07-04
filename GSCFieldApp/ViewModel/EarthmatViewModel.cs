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
using GSCFieldApp.Services;
using static Microsoft.Maui.ApplicationModel.Permissions;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Storage;
using System.Threading;

namespace GSCFieldApp.ViewModel
{
    [QueryProperty(nameof(Earthmaterial), nameof(Earthmaterial))]
    [QueryProperty(nameof(Station), nameof(Station))]
    public partial class EarthmatViewModel : ObservableObject
    {
        #region INIT

        //Localize
        public LocalizationResourceManager LocalizationResourceManager
        => LocalizationResourceManager.Instance; // Will be used for in code dynamic local strings

        //Services
        public CommandService commandServ = new CommandService();

        //Database
        DataAccess da = new DataAccess();
        SQLiteAsyncConnection currentConnection;

        //Model
        private Earthmaterial _model = new Earthmaterial();
        public DataIDCalculation idCalculator = new DataIDCalculation();
        ConcatenatedCombobox concat = new ConcatenatedCombobox(); //Use to concatenate values

        private string currentProjectType = DatabaseLiterals.ApplicationThemeBedrock; //default in case failing

        //UI
        private bool _isLithoGroupListVisible = false;
        private bool _isLithoDetailListVisible = true;

        private List<string> _lihthoDetailSearchResults = new List<string>();

        private List<Lithology> lithologies = new List<Lithology>();

        private string _earthResidualText = string.Empty;

        private IEnumerable<Vocabularies> _litho_detail_vocab; //Default list to keep in order to not redo the query each time
        private IEnumerable<Vocabularies> _litho_group_vocab; //Default list to keep in order to not redo the query each time

        private ComboBox _earthLithoGroup = new ComboBox();
        private ComboBox _earthLithOccurAs = new ComboBox();
        private ComboBox _earthLithMapUnit = new ComboBox();
        private ComboBox _earthLithSorting = new ComboBox();
        private ComboBox _earthLithWater = new ComboBox();
        private ComboBox _earthLithOxidation = new ComboBox();
        private ComboBox _earthLithClast = new ComboBox();
        private ComboBox _earthLithMetaFacies = new ComboBox();
        private ComboBox _earthLithMetaInt = new ComboBox();
        private ComboBox _earthLithMagQualifier = new ComboBox();
        private ComboBox _earthLithConfidence = new ComboBox();
        private ComboBox _earthLithColourGeneric = new ComboBox();
        private ComboBox _earthLithColourIntensity = new ComboBox();
        private ComboBox _earthLithColourQualifier = new ComboBox();
        private ComboBox _earthLithContactUpper = new ComboBox();
        private ComboBox _earthLithContactLower = new ComboBox();
        private ComboBox _earthLithContactType = new ComboBox();
        private ComboBox _earthLithContactRelatedAlias = new ComboBox();

        private ComboBoxItem _selectedEarthLithGroup = new ComboBoxItem();

        private Models.Colour _earthColourW = new Models.Colour();
        private Models.Colour _earthColourF = new Models.Colour();
        private Models.Contacts _earthContact = new Models.Contacts();

        //Concatenated fields
        private ComboBox _earthLithTextureStruct = new ComboBox();
        private ComboBox _earthLithQualifier = new ComboBox();
        private ComboBox _earthLithGrainSize = new ComboBox();
        private ComboBox _earthLithBedThick = new ComboBox();
        private ComboBox _earthLithDefFab = new ComboBox();

        private ComboBoxItem _selectedEarthLithQualifier = new ComboBoxItem();
        private ComboBoxItem _selectedEarthLithTextStruc = new ComboBoxItem();
        private ComboBoxItem _selectedEarthLithGrainSize = new ComboBoxItem();
        private ComboBoxItem _selectedEarthLithBedThick = new ComboBoxItem();
        private ComboBoxItem _selectedEarthLithDefFab = new ComboBoxItem();

        private ObservableCollection<ComboBoxItem> _qualifierCollection = new ObservableCollection<ComboBoxItem>();
        private ObservableCollection<ComboBoxItem> _textStructCollection = new ObservableCollection<ComboBoxItem>();
        private ObservableCollection<ComboBoxItem> _grainSizeCollection = new ObservableCollection<ComboBoxItem>();
        private ObservableCollection<ComboBoxItem> _bedThickCollection = new ObservableCollection<ComboBoxItem>();
        private ObservableCollection<ComboBoxItem> _defFabCollection = new ObservableCollection<ComboBoxItem>();
        private ObservableCollection<ComboBoxItem> _contactRelationCollection = new ObservableCollection<ComboBoxItem>();

        #endregion

        #region PROPERTIES

        [ObservableProperty]
        private Earthmaterial _earthmaterial;

        [ObservableProperty]
        private Station _station;

        public FieldThemes FieldThemes { get; set; } //Enable/Disable certain controls based on work type

        public Earthmaterial Model { get { return _model; } set { _model = value; } }
        public bool EMLithoVisibility
        {
            get { return Preferences.Get(nameof(EMLithoVisibility), true); }
            set { Preferences.Set(nameof(EMLithoVisibility), value); }
        }
        public bool EarthLithModifierVisibility
        {
            get { return Preferences.Get(nameof(EarthLithModifierVisibility), true); }
            set { Preferences.Set(nameof(EarthLithModifierVisibility), value); }
        }

        public bool EMMineralVisibility
        {
            get { return Preferences.Get(nameof(EMMineralVisibility), true); }
            set { Preferences.Set(nameof(EMMineralVisibility), value); }
        }

        public bool EMColourVisibility
        {
            get { return Preferences.Get(nameof(EMColourVisibility), true); }
            set { Preferences.Set(nameof(EMColourVisibility), value); }
        }

        public bool EMMetaFaciesVisibility
        {
            get { return Preferences.Get(nameof(EMMetaFaciesVisibility), true); }
            set { Preferences.Set(nameof(EMMetaFaciesVisibility), value); }
        }

        public bool EMContactVisibility
        {
            get { return Preferences.Get(nameof(EMContactVisibility), true); }
            set { Preferences.Set(nameof(EMContactVisibility), value); }
        }

        public bool EMContactRelVisibility
        {
            get { return Preferences.Get(nameof(EMContactRelVisibility), true); }
            set { Preferences.Set(nameof(EMContactRelVisibility), value); }
        }

        public bool EMMagVisibility
        {
            get { return Preferences.Get(nameof(EMMagVisibility), true); }
            set { Preferences.Set(nameof(EMMagVisibility), value); }
        }

        public bool EMInterpVisibility
        {
            get { return Preferences.Get(nameof(EMInterpVisibility), true); }
            set { Preferences.Set(nameof(EMInterpVisibility), value); }
        }

        public bool EMGeneralVisibility
        {
            get { return Preferences.Get(nameof(EMGeneralVisibility), true); }
            set { Preferences.Set(nameof(EMGeneralVisibility), value); }
        }

        public ComboBoxItem SelectedEarthLithoGroup
        {
            get
            {
                return _selectedEarthLithGroup;
            }
            set
            {
                _selectedEarthLithGroup = value;
                OnPropertyChanged(nameof(SelectedEarthLithoGroup));
            }
        }

        public List<string> LihthoDetailSearchResults { get { return _lihthoDetailSearchResults; } set { _lihthoDetailSearchResults = value; }  }

        public bool isLithoDetailListVisible { get { return _isLithoDetailListVisible; } set { _isLithoDetailListVisible = value; } }

        public ComboBox EarthLithoGroup { get { return _earthLithoGroup; } set { _earthLithoGroup = value; } }
        public ComboBox EarthLithQualifier { get { return _earthLithQualifier; } set { _earthLithQualifier = value; } }
        public ComboBox EarthLithOccurAs { get { return _earthLithOccurAs; } set { _earthLithOccurAs = value; } }
        public ComboBox EarthLithMapUnit { get { return _earthLithMapUnit; } set { _earthLithMapUnit = value; } }
        public ComboBox EarthLithSorting { get { return _earthLithSorting; } set { _earthLithSorting = value; } }
        public ComboBox EarthLithWater { get { return _earthLithWater; } set { _earthLithWater = value; } }
        public ComboBox EarthLithOxidation { get { return _earthLithOxidation; } set { _earthLithOxidation = value; } }
        public ComboBox EarthLithClast { get { return _earthLithClast; } set { _earthLithClast = value; } }
        public ComboBox EarthLithMetaFacies { get { return _earthLithMetaFacies; } set { _earthLithMetaFacies = value; } }
        public ComboBox EarthLithMetaInt { get { return _earthLithMetaInt; } set { _earthLithMetaInt = value; } }
        public ComboBox EarthLithMagQualifier { get { return _earthLithMagQualifier; } set { _earthLithMagQualifier = value; } }
        public ComboBox EarthLithConfidence { get { return _earthLithConfidence; } set { _earthLithConfidence = value; } }
        public ComboBox EarthLithTextureStruct { get { return _earthLithTextureStruct; } set { _earthLithTextureStruct = value; } }
        public ComboBox EarthLithGrainSize { get { return _earthLithGrainSize; } set { _earthLithGrainSize = value; } }
        public ComboBox EarthLithBedThick { get { return _earthLithBedThick; } set { _earthLithBedThick = value; } }
        public ComboBox EarthLithDefFab { get { return _earthLithDefFab; } set { _earthLithDefFab = value; } }
        public ComboBox EarthLithColourGeneric { get { return _earthLithColourGeneric; } set { _earthLithColourGeneric = value; } }
        public ComboBox EarthLithColourIntensity { get { return _earthLithColourIntensity; } set { _earthLithColourIntensity = value; } }
        public ComboBox EarthLithColourQualifier { get { return _earthLithColourQualifier; } set { _earthLithColourQualifier = value; } }
        public ComboBox EarthLithContactUpper { get { return _earthLithContactUpper; } set { _earthLithContactUpper = value; } }
        public ComboBox EarthLithContactLower { get { return _earthLithContactLower; } set { _earthLithContactLower = value; } }
        public ComboBox EarthLithContactType { get { return _earthLithContactType; } set { _earthLithContactType = value; } }
        public ComboBox EarthLithContactRelatedAlias { get { return _earthLithContactRelatedAlias; } set { _earthLithContactRelatedAlias = value; } }

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
                        if (value != null && value.itemName != string.Empty)
                        {
                            _qualifierCollection.Add(value);
                            _selectedEarthLithQualifier = value;
                            OnPropertyChanged(nameof(EarthLithQualifierCollection));
                        }

                    }


                }

            }
        }

        public ComboBoxItem SelectedEarthLithTextureStructure
        {
            get
            {
                return _selectedEarthLithTextStruc;
            }
            set
            {
                if (_selectedEarthLithTextStruc != value)
                {
                    if (_textStructCollection != null)
                    {
                        if (_textStructCollection.Count > 0 && _textStructCollection[0] == null)
                        {
                            _textStructCollection.RemoveAt(0);
                        }

                        if (value != null && value.itemName != string.Empty)
                        {
                            _textStructCollection.Add(value);
                            _selectedEarthLithTextStruc = value;
                            OnPropertyChanged(nameof(EarthLithTextStrucCollection));
                        }
                    }


                }

            }
        }
        public ComboBoxItem SelectedEarthLithGrainSize
        {
            get
            {
                return _selectedEarthLithGrainSize;
            }
            set
            {
                if (_selectedEarthLithGrainSize != value)
                {
                    if (_grainSizeCollection != null)
                    {
                        if (_grainSizeCollection.Count > 0 && _grainSizeCollection[0] == null)
                        {
                            _grainSizeCollection.RemoveAt(0);
                        }

                        if (value != null && value.itemName != string.Empty)
                        {
                            _grainSizeCollection.Add(value);
                            _selectedEarthLithGrainSize = value;
                            OnPropertyChanged(nameof(EarthLithGrainSizeCollection));
                        }

                    }


                }

            }
        }
        public ComboBoxItem SelectedEarthLithBedThick
        {
            get
            {
                return _selectedEarthLithBedThick;
            }
            set
            {
                if (_selectedEarthLithBedThick != value)
                {
                    if (_bedThickCollection != null)
                    {
                        if (_bedThickCollection.Count > 0 && _bedThickCollection[0] == null)
                        {
                            _bedThickCollection.RemoveAt(0);
                        }

                        if (value != null && value.itemName != string.Empty)
                        {
                            _bedThickCollection.Add(value);
                            _selectedEarthLithBedThick = value;
                            OnPropertyChanged(nameof(EarthLithBedThickCollection));
                        }

                    }


                }

            }
        }
        public ComboBoxItem SelectedEarthLithDefFab
        {
            get
            {
                return _selectedEarthLithDefFab;
            }
            set
            {
                if (_selectedEarthLithDefFab != value)
                {
                    if (_defFabCollection != null)
                    {
                        if (_defFabCollection.Count > 0 && _defFabCollection[0] == null)
                        {
                            _defFabCollection.RemoveAt(0);
                        }

                        if (value != null && value.itemName != string.Empty)
                        {
                            _defFabCollection.Add(value);
                            _selectedEarthLithDefFab = value;
                            OnPropertyChanged(nameof(EarthLithDefFabCollection));
                        }

                    }


                }

            }
        }
        public ObservableCollection<ComboBoxItem> EarthLithQualifierCollection { get { return _qualifierCollection; } set { _qualifierCollection = value; OnPropertyChanged(nameof(EarthLithQualifierCollection)); } }
        public ObservableCollection<ComboBoxItem> EarthLithTextStrucCollection { get { return _textStructCollection; } set { _textStructCollection = value; OnPropertyChanged(nameof(EarthLithTextStrucCollection)); } }
        public ObservableCollection<ComboBoxItem> EarthLithGrainSizeCollection { get { return _grainSizeCollection; } set { _grainSizeCollection = value; OnPropertyChanged(nameof(EarthLithGrainSizeCollection)); } }
        public ObservableCollection<ComboBoxItem> EarthLithBedThickCollection { get { return _bedThickCollection; } set { _bedThickCollection = value; OnPropertyChanged(nameof(EarthLithBedThickCollection)); } }
        public ObservableCollection<ComboBoxItem> EarthLithDefFabCollection { get { return _defFabCollection; } set { _defFabCollection = value; OnPropertyChanged(nameof(EarthLithDefFabCollection)); } }
        public ObservableCollection<ComboBoxItem> EarthLithContactRelationCollection { get { return _contactRelationCollection; } set { _contactRelationCollection = value; OnPropertyChanged(nameof(EarthLithContactRelationCollection)); } }

        public string EarthResidualText { get { return _earthResidualText; } set { _earthResidualText = value; } }

        #endregion

        public EarthmatViewModel() 
        {
            //Connect to db
            currentConnection = da.GetConnectionFromPath(da.PreferedDatabasePath);

            //Init new field theme
            FieldThemes = new FieldThemes();

            //Init picklists
            _ = FillSearchListAsync();

        }

        #region RELAY COMMANDS

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

        /// <summary>
        /// Save button command
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        async Task Save()
        {
            //Fill out missing values in model
            await SetModelAsync();

            //Validate if new entry or update
            if (_earthmaterial != null && _earthmaterial.EarthMatName != string.Empty && _model.EarthMatID != 0)
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

            //Exit
            await Shell.Current.GoToAsync($"../{nameof(FieldNotesPage)}");
            //await Shell.Current.GoToAsync("../");
        }

        /// <summary>
        /// Save button command
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        async Task SaveStay()
        {
            //Fill out missing values in model
            await SetModelAsync();

            //Validate if new entry or update
            if (_earthmaterial != null && _earthmaterial.EarthMatName != string.Empty && _model.EarthMatID != 0)
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

            //Show saved message
            await Toast.Make(LocalizationResourceManager["ToastSaveRecord"].ToString()).Show(CancellationToken.None);

            //Reset
            await ResetModelAsync();
            OnPropertyChanged(nameof(Model));


        }

        [RelayCommand]
        async Task SaveDelete()
        {
            if (_model.EarthMatID != 0)
            {
                await commandServ.DeleteDatabaseItemCommand(DatabaseLiterals.TableNames.em, _model.EarthMatName, _model.EarthMatID);
            }

            //Exit
            await Shell.Current.GoToAsync($"/{nameof(FieldNotesPage)}/");
            //await Shell.Current.GoToAsync("../");

        }

        /// <summary>
        /// Hide command to hide group of controls
        /// </summary>
        /// <param name="visibilityObjectName"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Special command to filter down all lithological details
        /// </summary>
        /// <param name="searchText"></param>
        /// <returns></returns>
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

                _isLithoDetailListVisible = true;
            }
            else
            {
                _isLithoDetailListVisible = false;
            }
            OnPropertyChanged(nameof(isLithoDetailListVisible));
            OnPropertyChanged(nameof(LihthoDetailSearchResults));
        }

        /// <summary>
        /// Special command to set colour system for fresh material
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        public async Task SetFreshColour()
        {
            if (_earthLithColourGeneric.cboxItems.Count() > 0 && _earthLithColourGeneric.cboxDefaultItemIndex != -1)
            {
                _earthColourF.generic = _earthLithColourGeneric.cboxItems[_earthLithColourGeneric.cboxDefaultItemIndex].itemValue; 
            }

            if (_earthLithColourIntensity.cboxItems.Count() > 0 && _earthLithColourIntensity.cboxDefaultItemIndex != -1)
            {
                _earthColourF.intensity = _earthLithColourIntensity.cboxItems[_earthLithColourIntensity.cboxDefaultItemIndex].itemValue; 
            }

            if (_earthLithColourQualifier.cboxItems.Count() > 0 && _earthLithColourQualifier.cboxDefaultItemIndex != -1)
            {
                _earthColourF.qualifier = _earthLithColourQualifier.cboxItems[_earthLithColourQualifier.cboxDefaultItemIndex].itemValue; 
            }
            Model.EarthMatColourF = _earthColourF.ToString();
            OnPropertyChanged(nameof(Model));
        }

        /// <summary>
        /// Special command to set colour system for weathered material
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        public async Task SetWeatheredColour()
        {
            if (_earthLithColourGeneric.cboxItems.Count() > 0 && _earthLithColourGeneric.cboxDefaultItemIndex != -1)
            {
                _earthColourW.generic = _earthLithColourGeneric.cboxItems[_earthLithColourGeneric.cboxDefaultItemIndex].itemValue;
            }

            if (_earthLithColourIntensity.cboxItems.Count() > 0 && _earthLithColourIntensity.cboxDefaultItemIndex != -1)
            {
                _earthColourW.intensity = _earthLithColourIntensity.cboxItems[_earthLithColourIntensity.cboxDefaultItemIndex].itemValue;
            }

            if (_earthLithColourQualifier.cboxItems.Count() > 0 && _earthLithColourQualifier.cboxDefaultItemIndex != -1)
            {
                _earthColourW.qualifier = _earthLithColourQualifier.cboxItems[_earthLithColourQualifier.cboxDefaultItemIndex].itemValue;
            }
            Model.EarthMatColourW = _earthColourW.ToString();
            OnPropertyChanged(nameof(Model));
        }

        /// <summary>
        /// Special command to set contact relation with other records
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        public async Task SetContact()
        {
            if (_earthLithContactType.cboxItems.Count() > 0 && _earthLithContactType.cboxDefaultItemIndex != -1 &&
                _earthLithContactRelatedAlias.cboxItems.Count() > 0 && _earthLithContactType.cboxDefaultItemIndex != -1)
            {
                //Build proper string from selected values
                _earthContact.type = _earthLithContactType.cboxItems[_earthLithContactType.cboxDefaultItemIndex].itemValue;
                Earthmaterial em = new Earthmaterial();
                em.EarthMatName = _earthLithContactRelatedAlias.cboxItems[_earthLithContactRelatedAlias.cboxDefaultItemIndex].itemValue;
                _earthContact.relatedEarthMaterialID = em.GetIDLetter;

                //Add 
                ComboBoxItem newContact = new ComboBoxItem();
                newContact.itemName = _earthContact.ToString();
                newContact.itemValue = _earthContact.ToString();
                newContact.canRemoveItem = true;
                _contactRelationCollection.Add(newContact);
            }

            OnPropertyChanged(nameof(EarthLithContactRelationCollection));
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Will fill the lithological detail search list values (approx. 450 values)
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
        /// Will fill the group search list values (approx. 65 items)
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

        /// <summary>
        /// Will initialize some preset list of lithologies for type/group and details
        /// </summary>
        /// <returns></returns>
        private async Task FillSearchListAsync()
        {
            //Prepare vocabulary
            List<Vocabularies> vocab = await currentConnection.Table<Vocabularies>().Where(vis => vis.Visibility == DatabaseLiterals.boolYes).ToListAsync();
            List<Metadata> meta = await currentConnection.Table<Metadata>().Where(metadata => metadata.MetaID == 1).ToListAsync();
            currentProjectType = meta.First().FieldworkType.ToString();

            await FillLithoGroupSearchListAsync(vocab, currentProjectType);

            //TODO: Make sure this one doesn't slow up the rendering process of the form
            await FillLithoSearchListAsync(vocab, currentProjectType);
        }

        /// <summary>
        /// Will force a filtered down list of lithology details based
        /// on user selected group
        /// </summary>
        /// <param name="groupName"></param>
        public void RefineDetailListFromGroup(ComboBoxItem groupName)
        {
            //Set group search bar
            _model.GroupType = groupName.itemValue;
            OnPropertyChanged(nameof(Model));

            //Reset list
            _lihthoDetailSearchResults = new List<string>();

            //Get proper lith group
            IEnumerable<Lithology> existingGroupType = lithologies.Where(l => l.GroupTypeCode == groupName.itemValue);
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

            _isLithoDetailListVisible = true;
            LihthoDetailSearchResults = _lihthoDetailSearchResults;
            OnPropertyChanged(nameof(isLithoDetailListVisible));
            OnPropertyChanged(nameof(LihthoDetailSearchResults));
            
        }

        /// <summary>
        /// Will select a proper group/type lithology based on selected
        /// detail
        /// </summary>
        /// <param name="groupName"></param>
        public void RefineGroupListFromDetail(string detailName)
        {

            //Find group match from detail
            bool foundMatch = false;
            while (!foundMatch)
            {
                foreach (Lithology lith in lithologies)
                {
                    if (detailName != string.Empty)
                    {
                        foreach (LithologyDetail lDetail in lith.lithologyDetails)
                        {
                            if (lDetail.DetailCode == detailName)
                            {
                                ComboBoxItem matchGroupItem = _earthLithoGroup.cboxItems.Where(i => i.itemName == lith.GroupTypeCode).First();
                                if (matchGroupItem != null)
                                {
                                    _earthLithoGroup.cboxDefaultItemIndex = _earthLithoGroup.cboxItems.IndexOf(matchGroupItem);
                                    foundMatch = true; //Get out of all for loops
                                }


                            }

                        }
                    }
                }

                foundMatch = true; //Break while if nothing was found
            }

            OnPropertyChanged(nameof(EarthLithoGroup));
            OnPropertyChanged(nameof(EarthLithoGroup.cboxDefaultItemIndex));

        }

        /// <summary>
        /// Will fill all picker controls
        /// TODO: make sure this whole thing doesn't slow too much form rendering
        /// </summary>
        /// <returns></returns>
        public async Task FillPickers()
        {
            _earthLithoGroup = await FillAPicker(DatabaseLiterals.FieldEarthMatLithgroup);
            _earthLithMapUnit = await FillAPicker(DatabaseLiterals.FieldEarthMatMapunit, "", currentProjectType);
            _earthLithSorting = await FillAPicker(DatabaseLiterals.FieldEarthMatSorting, "", currentProjectType);
            _earthLithWater = await FillAPicker(DatabaseLiterals.FieldEarthMatH2O, "", currentProjectType);
            _earthLithOxidation = await FillAPicker(DatabaseLiterals.FieldEarthMatOxidation, "", currentProjectType);
            _earthLithClast = await FillAPicker(DatabaseLiterals.FieldEarthMatClastForm, "", currentProjectType);
            _earthLithBedThick = await FillAPicker(DatabaseLiterals.FieldEarthMatBedthick, "", currentProjectType);
            _earthLithDefFab = await FillAPicker(DatabaseLiterals.FieldEarthMatDefabric, "", currentProjectType);
            _earthLithMetaFacies = await FillAPicker(DatabaseLiterals.FieldEarthMatMetaFacies);
            _earthLithMetaInt = await FillAPicker(DatabaseLiterals.FieldEarthMatMetaIntensity);
            _earthLithMagQualifier = await FillAPicker(DatabaseLiterals.FieldEarthMatMagQualifier);
            _earthLithConfidence = await FillAPicker(DatabaseLiterals.FieldEarthMatInterpConf);
            _earthLithColourGeneric = await FillAPicker(DatabaseLiterals.KeywordColourGeneric);
            _earthLithColourIntensity = await FillAPicker(DatabaseLiterals.KeywordColourIntensity);
            _earthLithColourQualifier = await FillAPicker(DatabaseLiterals.KeywordColourQualifier);
            _earthLithContactUpper = await FillAPicker(DatabaseLiterals.FieldEarthMatContactUp);
            _earthLithContactLower = await FillAPicker(DatabaseLiterals.FieldEarthMatContactLow);
            _earthLithContactType = await FillAPicker(DatabaseLiterals.FieldEarthMatContactUp);

            OnPropertyChanged(nameof(EarthLithoGroup));
            OnPropertyChanged(nameof(EarthLithMapUnit));
            OnPropertyChanged(nameof(EarthLithSorting));
            OnPropertyChanged(nameof(EarthLithWater));
            OnPropertyChanged(nameof(EarthLithOxidation));
            OnPropertyChanged(nameof(EarthLithClast));
            OnPropertyChanged(nameof(EarthLithBedThick));
            OnPropertyChanged(nameof(EarthLithDefFab));
            OnPropertyChanged(nameof(EarthLithMetaFacies));
            OnPropertyChanged(nameof(EarthLithMetaInt));
            OnPropertyChanged(nameof(EarthLithMagQualifier));
            OnPropertyChanged(nameof(EarthLithConfidence));
            OnPropertyChanged(nameof(EarthLithColourGeneric));
            OnPropertyChanged(nameof(EarthLithColourIntensity));
            OnPropertyChanged(nameof(EarthLithColourQualifier));
            OnPropertyChanged(nameof(EarthLithContactUpper));
            OnPropertyChanged(nameof(EarthLithContactLower));
            OnPropertyChanged(nameof(EarthLithContactType));

            //There is one picker that needs a parent in bedrock, but doesn't in surficial
            if (currentProjectType == DatabaseLiterals.ApplicationThemeSurficial)
            {
                _earthLithTextureStruct = await FillAPicker(DatabaseLiterals.FieldEarthMatModTextStruc, _model.EarthMatLithgroup, currentProjectType);
                OnPropertyChanged(nameof(EarthLithTextureStruct));
            }

            //There is one picker that needs all brotha's and sista's listing
            if (currentProjectType == DatabaseLiterals.ApplicationThemeBedrock)
            {
                _earthLithContactRelatedAlias = await FillRelatedEarthmatAsync();
                OnPropertyChanged(nameof(EarthLithContactRelatedAlias));
            }
        }

        /// <summary>
        /// Will fill all picker controls that are dependant on the user selected
        /// lithology group/type
        /// </summary>
        /// <returns></returns>
        public async Task Fill2ndRoundPickers()
        {
            //second round pickers
            if (_model.GroupType != string.Empty)
            {
                _earthLithQualifier = await FillAPicker(DatabaseLiterals.FieldEarthMatModComp, _model.EarthMatLithgroup, currentProjectType);
                _earthLithOccurAs = await FillAPicker(DatabaseLiterals.FieldEarthMatOccurs, _model.EarthMatLithgroup);
                _earthLithTextureStruct = await FillAPicker(DatabaseLiterals.FieldEarthMatModTextStruc, _model.EarthMatLithgroup, currentProjectType);
                _earthLithGrainSize = await FillAPicker(DatabaseLiterals.FieldEarthMatGrSize, _model.EarthMatLithgroup, currentProjectType);
                OnPropertyChanged(nameof(EarthLithQualifier));
                OnPropertyChanged(nameof(EarthLithOccurAs));
                OnPropertyChanged(nameof(EarthLithTextureStruct));
                OnPropertyChanged(nameof(EarthLithGrainSize));
            }

        }

        /// <summary>
        /// Generic method to fill a needed picker control with vocabulary
        /// </summary>
        private async Task<ComboBox> FillAPicker(string fieldName, string extraField = "", string fieldWork = "")
        {
            //Make sure to user default database rather then the prefered one. This one will always be there.
            return await da.GetComboboxListWithVocabAsync(DatabaseLiterals.TableEarthMat, fieldName, extraField, fieldWork);

        }

        /// <summary>
        /// Will fill with all other earthmaterial associated with parent station/drill hole
        /// </summary>
        public async Task<ComboBox> FillRelatedEarthmatAsync()
        {
            ComboBox relatedCbx = new ComboBox();
            relatedCbx.cboxDefaultItemIndex = -1;

            if (_earthmaterial != null || _station != null)
            {
                //Find proper parent id (request could come from a mineral or an earthmat selection)
                List<Earthmaterial> ems = new List<Earthmaterial>();
                if (_earthmaterial != null)
                {
                    if (_earthmaterial.ParentName == Dictionaries.DatabaseLiterals.TableStation)
                    {
                        ems = await currentConnection.Table<Earthmaterial>().Where(i => (i.EarthMatStatID == Earthmaterial.EarthMatStatID || i.EarthMatID <= 1) && (i.EarthMatID != _earthmaterial.EarthMatID)).ToListAsync();
                    }
                    else
                    {
                        ems = await currentConnection.Table<Earthmaterial>().Where(i => i.EarthMatDrillHoleID == Earthmaterial.EarthMatDrillHoleID || i.EarthMatID != _earthmaterial.EarthMatID).ToListAsync();
                    }
                }
                else
                {
                    if (_station != null)
                    {
                        ems = await currentConnection.Table<Earthmaterial>().Where(i => (i.EarthMatStatID == _station.StationID)).ToListAsync();
                    }
                }

                if (ems != null && ems.Count > 0)
                {

                    foreach (Earthmaterial em in ems)
                    {
                        Themes.ComboBoxItem newItem = new Themes.ComboBoxItem();
                        newItem.itemValue = em.EarthMatName;
                        newItem.itemName = em.EarthMatName;
                        relatedCbx.cboxItems.Add(newItem);
                    }

                    relatedCbx.cboxDefaultItemIndex = -1;
                }
            }

            return relatedCbx;

        }

        /// <summary>
        /// Will fill out missing fields for model. Default auto-calculated values
        /// Done before actually saving
        /// </summary>
        private async Task SetModelAsync()
        {
            //Make sure it's for a new field book
            if (Model.EarthMatID == 0 && _station != null)
            {
                //Get current application version
                Model.EarthMatStatID = _station.StationID;
                Model.EarthMatName = await idCalculator.CalculateEarthmatAliasAsync(_station.StationID, _station.StationAlias);
            }

            #region Process pickers
            if (EarthLithQualifierCollection.Count > 0)
            {
                Model.EarthMatModComp = concat.PipeValues(EarthLithQualifierCollection); //process list of values so they are concatenated.
            }
            if (EarthLithTextStrucCollection.Count > 0)
            {
                Model.EarthMatModTextStruc = concat.PipeValues(EarthLithTextStrucCollection); //process list of values so they are concatenated.
            }
            if (EarthLithGrainSizeCollection.Count > 0)
            {
                Model.EarthMatGrSize = concat.PipeValues(EarthLithGrainSizeCollection); //process list of values so they are concatenated.
            }
            if (EarthLithBedThickCollection.Count > 0)
            {
                Model.EarthMatBedthick = concat.PipeValues(EarthLithBedThickCollection); //process list of values so they are concatenated.
            }
            if (EarthLithDefFabCollection.Count > 0)
            {
                Model.EarthMatDefabric = concat.PipeValues(EarthLithDefFabCollection); //process list of values so they are concatenated.
            }
            if (EarthLithOccurAs.cboxItems.Count() > 0 && EarthLithOccurAs.cboxDefaultItemIndex != -1)
            {
                Model.EarthMatOccurs = EarthLithOccurAs.cboxItems[EarthLithOccurAs.cboxDefaultItemIndex].itemValue; //process list of values so they are concatenated.
            }
            if (EarthLithMapUnit.cboxItems.Count() > 0 &&  EarthLithMapUnit.cboxDefaultItemIndex != -1)
            {
                Model.EarthMatMapunit = EarthLithMapUnit.cboxItems[EarthLithMapUnit.cboxDefaultItemIndex].itemValue; //process list of values so they are concatenated.
            }
            if (EarthLithSorting.cboxItems.Count() > 0 &&  EarthLithSorting.cboxDefaultItemIndex != -1)
            {
                Model.EarthMatSorting = EarthLithSorting.cboxItems[EarthLithSorting.cboxDefaultItemIndex].itemValue; //process list of values so they are concatenated.
            }
            if (EarthLithWater.cboxItems.Count() > 0 &&  EarthLithWater.cboxDefaultItemIndex != -1)
            {
                Model.EarthMatH2O = EarthLithWater.cboxItems[EarthLithWater.cboxDefaultItemIndex].itemValue; //process list of values so they are concatenated.
            }
            if (EarthLithOxidation.cboxItems.Count() > 0 &&  EarthLithOxidation.cboxDefaultItemIndex != -1)
            {
                Model.EarthMatOxidation = EarthLithOxidation.cboxItems[EarthLithOxidation.cboxDefaultItemIndex].itemValue; //process list of values so they are concatenated.
            }
            if (EarthLithClast.cboxItems.Count() > 0 &&  EarthLithClast.cboxDefaultItemIndex != -1)
            {
                Model.EarthMatClastForm = EarthLithClast.cboxItems[EarthLithClast.cboxDefaultItemIndex].itemValue; //process list of values so they are concatenated.
            }
            if (EarthLithMetaFacies.cboxItems.Count() > 0 && EarthLithMetaFacies.cboxDefaultItemIndex != -1)
            {
                Model.EarthMatMetaIFacies = EarthLithMetaFacies.cboxItems[EarthLithMetaFacies.cboxDefaultItemIndex].itemValue; //process list of values so they are concatenated.
            }
            if (EarthLithMetaInt.cboxItems.Count() > 0 && EarthLithMetaInt.cboxDefaultItemIndex != -1)
            {
                Model.EarthMatMetaIntensity = EarthLithMetaInt.cboxItems[EarthLithMetaInt.cboxDefaultItemIndex].itemValue; //process list of values so they are concatenated.
            }
            if (EarthLithMagQualifier.cboxItems.Count() > 0 && EarthLithMagQualifier.cboxDefaultItemIndex != -1)
            {
                Model.EarthMatMagQualifier = EarthLithMagQualifier.cboxItems[EarthLithMagQualifier.cboxDefaultItemIndex].itemValue; //process list of values so they are concatenated.
            }
            if (EarthLithConfidence.cboxItems.Count() > 0 && EarthLithConfidence.cboxDefaultItemIndex != -1)
            {
                Model.EarthMatInterpConf = EarthLithConfidence.cboxItems[EarthLithConfidence.cboxDefaultItemIndex].itemValue; //process list of values so they are concatenated.
            }
            if (EarthLithContactUpper.cboxItems.Count() > 0 && EarthLithContactUpper.cboxDefaultItemIndex != -1)
            {
                Model.EarthMatContactUp = EarthLithContactUpper.cboxItems[EarthLithContactUpper.cboxDefaultItemIndex].itemValue; //process list of values so they are concatenated.
            }
            if (EarthLithContactLower.cboxItems.Count() > 0 && EarthLithContactLower.cboxDefaultItemIndex != -1)
            {
                Model.EarthMatContactLow = EarthLithContactLower.cboxItems[EarthLithContactLower.cboxDefaultItemIndex].itemValue; //process list of values so they are concatenated.
            }
            if (EarthLithContactRelationCollection.Count > 0)
            {
                Model.EarthMatContact = concat.PipeValues(EarthLithContactRelationCollection); //process list of values so they are concatenated.
            }
            if (EarthLithoGroup.cboxItems.Count() > 0 && EarthLithoGroup.cboxDefaultItemIndex != -1)
            {
                Model.EarthMatLithgroup = EarthLithoGroup.cboxItems[EarthLithoGroup.cboxDefaultItemIndex].itemValue;
            }
            #endregion
        }


        /// <summary>
        /// Will reset model fields to default just like it's a new record
        /// </summary>
        /// <returns></returns>
        private async Task ResetModelAsync()
        {

            //Reset model
            if (_station != null)
            {
                // if coming from station notes, calculate new alias
                Model.EarthMatStatID = _station.StationID;
                Model.EarthMatName = await idCalculator.CalculateEarthmatAliasAsync(_station.StationID, _station.StationAlias);
            }
            else if (Model.EarthMatStatID != null)
            {
                // if coming from field notes on a record edit that needs to be saved as a new record with stay/save
                SQLiteAsyncConnection currentConnection = da.GetConnectionFromPath(da.PreferedDatabasePath);
                List<Station> parentAlias = await currentConnection.Table<Station>().Where(e => e.StationID == Model.EarthMatStatID.Value).ToListAsync();
                await currentConnection.CloseAsync();
                Model.EarthMatName = await idCalculator.CalculateEarthmatAliasAsync(Model.EarthMatStatID.Value, parentAlias.First().StationAlias);
            }

            Model.EarthMatID = 0;
            
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

                #region Pickers
                //Select values in pickers
                foreach (ComboBoxItem cbox in EarthLithoGroup.cboxItems)
                {
                    if (cbox.itemValue == _model.EarthMatLithgroup)
                    {
                        EarthLithoGroup.cboxDefaultItemIndex = EarthLithoGroup.cboxItems.IndexOf(cbox);
                        break;
                    }
                }
                OnPropertyChanged(nameof(EarthLithoGroup));

                foreach (ComboBoxItem cbox in EarthLithMapUnit.cboxItems)
                {
                    if (cbox.itemValue == _earthmaterial.EarthMatMapunit)
                    {
                        EarthLithMapUnit.cboxDefaultItemIndex = EarthLithMapUnit.cboxItems.IndexOf(cbox); break;
                    }
                }
                OnPropertyChanged(nameof(EarthLithMapUnit));

                foreach (ComboBoxItem cbox in EarthLithSorting.cboxItems)
                {
                    if (cbox.itemValue == _earthmaterial.EarthMatSorting)
                    {
                        EarthLithSorting.cboxDefaultItemIndex = EarthLithSorting.cboxItems.IndexOf(cbox); break;
                    }
                }
                OnPropertyChanged(nameof(EarthLithSorting));

                foreach (ComboBoxItem cbox in EarthLithWater.cboxItems)
                {
                    if (cbox.itemValue == _earthmaterial.EarthMatH2O)
                    {
                        EarthLithWater.cboxDefaultItemIndex = EarthLithWater.cboxItems.IndexOf(cbox); break;
                    }
                }
                OnPropertyChanged(nameof(EarthLithWater));

                foreach (ComboBoxItem cbox in EarthLithOxidation.cboxItems)
                {
                    if (cbox.itemValue == _earthmaterial.EarthMatOxidation)
                    {
                        EarthLithOxidation.cboxDefaultItemIndex = EarthLithOxidation.cboxItems.IndexOf(cbox); break;
                    }
                }
                OnPropertyChanged(nameof(EarthLithOxidation));

                foreach (ComboBoxItem cbox in EarthLithClast.cboxItems)
                {
                    if (cbox.itemValue == _earthmaterial.EarthMatClastForm)
                    {
                        EarthLithClast.cboxDefaultItemIndex = EarthLithClast.cboxItems.IndexOf(cbox); break;
                    }
                }
                OnPropertyChanged(nameof(EarthLithClast));

                List<string> bts = concat.UnpipeString(_earthmaterial.EarthMatBedthick);
                _bedThickCollection.Clear(); //Clear any possible values first
                foreach (ComboBoxItem cbox in EarthLithBedThick.cboxItems)
                {
                    if (bts.Contains(cbox.itemValue) && !_bedThickCollection.Contains(cbox))
                    {
                        _bedThickCollection.Add(cbox);
                    }
                }
                OnPropertyChanged(nameof(EarthLithBedThickCollection));

                List<string> bfs = concat.UnpipeString(_earthmaterial.EarthMatDefabric);
                _defFabCollection.Clear(); //Clear any possible values first
                foreach (ComboBoxItem cbox in EarthLithDefFab.cboxItems)
                {
                    if (bfs.Contains(cbox.itemValue) && !_defFabCollection.Contains(cbox))
                    {
                        _defFabCollection.Add(cbox);
                    }
                }
                OnPropertyChanged(nameof(EarthLithDefFabCollection));

                foreach (ComboBoxItem cbox in EarthLithMetaFacies.cboxItems)
                {
                    if (cbox.itemValue == _earthmaterial.EarthMatMetaIFacies)
                    {
                        EarthLithMetaFacies.cboxDefaultItemIndex = EarthLithMetaFacies.cboxItems.IndexOf(cbox); break;
                    }
                }
                OnPropertyChanged(nameof(EarthLithMetaFacies));

                foreach (ComboBoxItem cbox in EarthLithMetaInt.cboxItems)
                {
                    if (cbox.itemValue == _earthmaterial.EarthMatMetaIntensity)
                    {
                        EarthLithMetaInt.cboxDefaultItemIndex = EarthLithMetaInt.cboxItems.IndexOf(cbox); break;
                    }
                }
                OnPropertyChanged(nameof(EarthLithMetaInt));

                foreach (ComboBoxItem cbox in EarthLithMagQualifier.cboxItems)
                {
                    if (cbox.itemValue == _earthmaterial.EarthMatMagQualifier)
                    {
                        EarthLithMagQualifier.cboxDefaultItemIndex = EarthLithMagQualifier.cboxItems.IndexOf(cbox); break;
                    }
                }
                OnPropertyChanged(nameof(EarthLithMagQualifier));

                foreach (ComboBoxItem cbox in EarthLithConfidence.cboxItems)
                {
                    if (cbox.itemValue == _earthmaterial.EarthMatInterpConf)
                    {
                        EarthLithConfidence.cboxDefaultItemIndex = EarthLithConfidence.cboxItems.IndexOf(cbox); break;
                    }
                }
                OnPropertyChanged(nameof(EarthLithConfidence));

                foreach (ComboBoxItem cbox in EarthLithContactUpper.cboxItems)
                {
                    if (cbox.itemValue == _earthmaterial.EarthMatContactUp)
                    {
                        EarthLithContactUpper.cboxDefaultItemIndex = EarthLithContactUpper.cboxItems.IndexOf(cbox); break;
                    }
                }
                OnPropertyChanged(nameof(EarthLithContactUpper));

                foreach (ComboBoxItem cbox in EarthLithContactLower.cboxItems)
                {
                    if (cbox.itemValue == _earthmaterial.EarthMatContactLow)
                    {
                        EarthLithContactLower.cboxDefaultItemIndex = EarthLithContactLower.cboxItems.IndexOf(cbox); break;
                    }
                }
                OnPropertyChanged(nameof(EarthLithContactLower));

                List<string> ecs = concat.UnpipeString(_earthmaterial.EarthMatContact);
                _contactRelationCollection.Clear(); //Clear any possible values first
                foreach (string ec in ecs)
                {
                    ComboBoxItem new_ec = new ComboBoxItem();
                    new_ec.itemValue = ec;
                    new_ec.itemName = ec;
                    new_ec.canRemoveItem = true;
                    _contactRelationCollection.Add(new_ec);
                }
                OnPropertyChanged(nameof(EarthLithBedThickCollection));

                #endregion

                if (currentProjectType == DatabaseLiterals.ApplicationThemeSurficial)
                {
                    List<string> textStrucs = concat.UnpipeString(_earthmaterial.EarthMatModTextStruc);
                    _textStructCollection.Clear(); //Clear any possible values first
                    foreach (ComboBoxItem cbox in EarthLithTextureStruct.cboxItems)
                    {
                        if (textStrucs.Contains(cbox.itemValue) && !_textStructCollection.Contains(cbox))
                        {
                            _textStructCollection.Add(cbox);
                        }
                    }
                    OnPropertyChanged(nameof(EarthLithTextStrucCollection));

                }

                //Piped value field
                await Fill2ndRoundPickers();
                await Load2nRound();

            }
        }

        /// <summary>
        /// Will make sure to load the second round of picklist
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

                foreach (ComboBoxItem cbox in EarthLithOccurAs.cboxItems)
                {
                    if (cbox.itemValue == _earthmaterial.EarthMatOccurs)
                    {
                        EarthLithOccurAs.cboxDefaultItemIndex = EarthLithOccurAs.cboxItems.IndexOf(cbox); break;
                    }
                }
                OnPropertyChanged(nameof(EarthLithOccurAs));

                List<string> textStrucs = concat.UnpipeString(_earthmaterial.EarthMatModTextStruc);
                _textStructCollection.Clear(); //Clear any possible values first
                foreach (ComboBoxItem cbox in EarthLithTextureStruct.cboxItems)
                {
                    if (textStrucs.Contains(cbox.itemValue) && !_textStructCollection.Contains(cbox))
                    {
                        _textStructCollection.Add(cbox);
                    }
                }
                OnPropertyChanged(nameof(EarthLithTextStrucCollection));

                List<string> grainSizes = concat.UnpipeString(_earthmaterial.EarthMatGrSize);
                _grainSizeCollection.Clear(); //Clear any possible values first
                foreach (ComboBoxItem cbox in EarthLithGrainSize.cboxItems)
                {
                    if (grainSizes.Contains(cbox.itemValue) && !_grainSizeCollection.Contains(cbox))
                    {
                        _grainSizeCollection.Add(cbox);
                    }
                }
                OnPropertyChanged(nameof(EarthLithGrainSizeCollection));
            }

        }

        #endregion

        #region CALCULATE

        /// <summary>
        /// Will calculate the total amount of percentage
        /// from all related station or drill holes child of
        /// same parent
        /// </summary>
        /// <param name="newMode"></param>
        public async void CalculateResidual(string newMode = "")
        {
            if (_earthmaterial != null || _station != null)
            {
                //Find proper parent id (request could come from a mineral or an earthmat selection)
                List<Earthmaterial> ems = new List<Earthmaterial>();
                if (_earthmaterial != null)
                {
                    if (_earthmaterial.ParentName == Dictionaries.DatabaseLiterals.TableStation)
                    {
                        ems = await currentConnection.Table<Earthmaterial>().Where(i => (i.EarthMatStatID == Earthmaterial.EarthMatStatID || i.EarthMatID <= 1) && (i.EarthMatID != _earthmaterial.EarthMatID)).ToListAsync();
                    }
                    else
                    {
                        ems = await currentConnection.Table<Earthmaterial>().Where(i => i.EarthMatDrillHoleID == Earthmaterial.EarthMatDrillHoleID || i.EarthMatID != _earthmaterial.EarthMatID).ToListAsync();
                    }
                }
                else
                {
                    if (_station != null)
                    {
                        ems = await currentConnection.Table<Earthmaterial>().Where(i => (i.EarthMatStatID == _station.StationID)).ToListAsync();
                    }
                }

                if (ems != null && ems.Count > 0)
                {
                    int currentPercentage = 0;

                    if (newMode != string.Empty)
                    {
                        int.TryParse(newMode, out currentPercentage);
                    }
                    foreach (Earthmaterial em in ems)
                    {
                        if (em.EarthMatPercent != null)
                        {
                            currentPercentage = currentPercentage + (int)em.EarthMatPercent;
                        }

                    }

                    _earthResidualText = String.Format(LocalizationResourceManager["EarthmatPageResidualLabel"].ToString(), currentPercentage, (ems.Count() + 1).ToString());
                    OnPropertyChanged(nameof(EarthResidualText));
                }
            }




        }
        #endregion

    }
}
