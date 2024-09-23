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
using static GSCFieldApp.Dictionaries.DatabaseLiterals;
using GSCFieldApp.Controls;
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
using System.Security.Cryptography;

namespace GSCFieldApp.ViewModel
{
    [QueryProperty(nameof(Earthmaterial), nameof(Earthmaterial))]
    [QueryProperty(nameof(Station), nameof(Station))]
    [QueryProperty(nameof(DrillHole), nameof(DrillHole))]
    public partial class EarthmatViewModel : FieldAppPageHelper
    {
        #region INIT

        //Model
        private Earthmaterial _model = new Earthmaterial();
        private string _occurenceLoadedValue = string.Empty; //Will be filled with occurence value on load
        private string currentProjectType = ApplicationThemeBedrock; //default in case failing

        //UI
        private bool _isLithoGroupListVisible = false;
        private bool _isLithoDetailListVisible = true;

        private List<string> _lihthoDetailSearchResults = new List<string>();

        private List<Lithology> lithologies = new List<Lithology>();

        private string _earthResidualText = string.Empty;

        private IEnumerable<Vocabularies> _litho_detail_vocab; //Default list to keep in order to not redo the query each time
        private IEnumerable<Vocabularies> _litho_group_vocab; //Default list to keep in order to not redo the query each time

        private ComboBox _earthLithoGroup = new ComboBox();
        private ComboBox _earthLithDetail = new ComboBox(); //Surficial only
        private ComboBox _earthLithOccurAs = new ComboBox();
        private ComboBox _earthLithOccursAsAll = new ComboBox();
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

        [ObservableProperty]
        private DrillHole _drillHole;

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
        public ComboBox EarthLithDetail { get { return _earthLithDetail; } set { _earthLithDetail = value; } }
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
            //Exit
            await NavigateToFieldNotes(TableNames.earthmat, false);
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
            await NavigateToFieldNotes(TableNames.earthmat);
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
                await commandServ.DeleteDatabaseItemCommand(TableNames.earthmat, _model.EarthMatName, _model.EarthMatID);
            }

            //Exit
            await NavigateToFieldNotes(TableNames.earthmat);

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

        [RelayCommand]
        public async Task AddSample()
        {
            //Save
            await SetAndSaveModelAsync();

            //Navigate to child
            await Shell.Current.GoToAsync($"////FieldNotesPage/SamplePage/",
                new Dictionary<string, object>
                {
                    [nameof(Sample)] = null,
                    [nameof(Earthmaterial)] = Model
                }
            );
        }

        [RelayCommand]
        public async Task AddStructure()
        {
            //Save
            await SetAndSaveModelAsync();

            //Navigate to child
            await Shell.Current.GoToAsync($"{nameof(StructurePage)}/",
                new Dictionary<string, object>
                {
                    [nameof(Structure)] = null,
                    [nameof(Earthmaterial)] = Model,
                }
            );
        }

        [RelayCommand]
        public async Task AddPaleoflow()
        {
            //Save
            await SetAndSaveModelAsync();

            //Navigate to pflow page 
            await Shell.Current.GoToAsync($"{nameof(PaleoflowPage)}/",
                new Dictionary<string, object>
                {
                    [nameof(PaleoflowPage)] = null,
                    [nameof(Earthmaterial)] = Model,
                }
            );
        }

        [RelayCommand]
        public async Task AddFossil()
        {
            //Save
            await SetAndSaveModelAsync();

            //Navigate to pflow page 
            await Shell.Current.GoToAsync($"{nameof(FossilPage)}/",
                new Dictionary<string, object>
                {
                    [nameof(FossilPage)] = null,
                    [nameof(Earthmaterial)] = Model,
                }
            );
        }

        [RelayCommand]
        public async Task AddMineral()
        {
            //Save
            await SetAndSaveModelAsync();

            //Navigate to pflow page 
            await Shell.Current.GoToAsync($"{nameof(MineralPage)}/",
                new Dictionary<string, object>
                {
                    [nameof(MineralPage)] = null,
                    [nameof(Earthmaterial)] = Model,
                    [nameof(MineralAlteration)] = null,
                }
            );
        }

        [RelayCommand]
        public async Task AddMineralization()
        {
            //Save
            await SetAndSaveModelAsync();

            //Navigate to pflow page 
            await Shell.Current.GoToAsync($"{nameof(MineralizationAlterationPage)}/",
                new Dictionary<string, object>
                {
                    [nameof(MineralAlteration)] = null,
                    [nameof(Earthmaterial)] = Model,
                    [nameof(Station)] = null,
                }
            );
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

            List<VocabularyManager> vocab_manager = await currentConnection.Table<VocabularyManager>().Where(theme => (theme.ThemeAssignField == FieldEarthMatLithdetail) && (theme.ThemeSpecificTo.Contains(in_projectType))) .ToListAsync();
            _litho_detail_vocab = from v in in_vocab join vm in vocab_manager on v.CodedTheme equals vm.ThemeCodedTheme orderby v.Code select v;

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

            List<VocabularyManager> vocab_manager = await currentConnection.Table<VocabularyManager>().Where(theme => (theme.ThemeAssignField == FieldEarthMatLithgroup) && (theme.ThemeSpecificTo.Contains(in_projectType))).ToListAsync();
            _litho_group_vocab = from v in in_vocab join vm in vocab_manager on v.CodedTheme equals vm.ThemeCodedTheme orderby v.Code select v;

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
            //Connect to db
            currentConnection = da.GetConnectionFromPath(da.PreferedDatabasePath);

            //Prepare vocabulary
            List<Vocabularies> vocab = await currentConnection.Table<Vocabularies>().Where(vis => vis.Visibility == boolYes).ToListAsync();
            currentProjectType = Preferences.Get(nameof(FieldUserInfoFWorkType), currentProjectType);

            await FillLithoGroupSearchListAsync(vocab, currentProjectType);

            //TODO: Make sure this one doesn't slow up the rendering process of the form
            await FillLithoSearchListAsync(vocab, currentProjectType);

            await currentConnection.CloseAsync();
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
            //Bedrock pickers
            if (currentProjectType.Contains(ApplicationThemeBedrock))
            {
                _earthLithBedThick = await FillAPicker(FieldEarthMatBedthick, "");
                _earthLithDefFab = await FillAPicker(FieldEarthMatDefabric, "");
                _earthLithMetaFacies = await FillAPicker(FieldEarthMatMetaFacies);
                _earthLithMetaInt = await FillAPicker(FieldEarthMatMetaIntensity);
                _earthLithMagQualifier = await FillAPicker(FieldEarthMatMagQualifier);
                _earthLithContactUpper = await FillAPicker(FieldEarthMatContactUp);
                _earthLithContactLower = await FillAPicker(FieldEarthMatContactLow);
                _earthLithContactType = await FillAPicker(FieldEarthMatContactUp);

                OnPropertyChanged(nameof(EarthLithBedThick));
                OnPropertyChanged(nameof(EarthLithDefFab));
                OnPropertyChanged(nameof(EarthLithMetaFacies));
                OnPropertyChanged(nameof(EarthLithMetaInt));
                OnPropertyChanged(nameof(EarthLithMagQualifier));
                OnPropertyChanged(nameof(EarthLithContactUpper));
                OnPropertyChanged(nameof(EarthLithContactLower));
                OnPropertyChanged(nameof(EarthLithContactType));

                //There is one picker that needs all brotha's and sista's listing
                _earthLithContactRelatedAlias = await FillRelatedEarthmatAsync();
                OnPropertyChanged(nameof(EarthLithContactRelatedAlias));

            }
            else if (currentProjectType == ApplicationThemeSurficial)
            {
                _earthLithSorting = await FillAPicker(FieldEarthMatSorting, "");
                _earthLithWater = await FillAPicker(FieldEarthMatH2O, "");
                _earthLithOxidation = await FillAPicker(FieldEarthMatOxidation, "");
                _earthLithClast = await FillAPicker(FieldEarthMatClastForm, "");
                _earthLithDetail = await FillAPicker(FieldEarthMatLithdetail);
                OnPropertyChanged(nameof(EarthLithSorting));
                OnPropertyChanged(nameof(EarthLithWater));
                OnPropertyChanged(nameof(EarthLithOxidation));
                OnPropertyChanged(nameof(EarthLithClast));
                OnPropertyChanged(nameof(EarthLithDetail));

                //There is one picker that needs a parent in bedrock, but doesn't in surficial
                _earthLithTextureStruct = await FillAPicker(FieldEarthMatModTextStruc, _model.EarthMatLithgroup);
                OnPropertyChanged(nameof(EarthLithTextureStruct));

            }

            _earthLithoGroup = await FillAPicker(FieldEarthMatLithgroup);
            _earthLithMapUnit = await FillAPicker(FieldEarthMatMapunit, "");
            _earthLithConfidence = await FillAPicker(FieldEarthMatInterpConf);
            _earthLithColourGeneric = await FillAPicker(KeywordColourGeneric);
            _earthLithColourIntensity = await FillAPicker(KeywordColourIntensity);
            _earthLithColourQualifier = await FillAPicker(KeywordColourQualifier);
            _earthLithOccurAs = _earthLithOccursAsAll = await FillAPicker(FieldEarthMatOccurs); 

            OnPropertyChanged(nameof(EarthLithoGroup));
            OnPropertyChanged(nameof(EarthLithMapUnit));
            OnPropertyChanged(nameof(EarthLithConfidence));
            OnPropertyChanged(nameof(EarthLithColourGeneric));
            OnPropertyChanged(nameof(EarthLithColourIntensity));
            OnPropertyChanged(nameof(EarthLithColourQualifier));
            OnPropertyChanged(nameof(EarthLithOccurAs)); //Init with all values and filter down at 2nd load
        }

        /// <summary>
        /// Will fill all picker controls that are dependant on the user selected
        /// lithology group/type
        /// </summary>
        /// <returns></returns>
        public async Task Fill2ndRoundPickers()
        {
            //second round pickers
            if (_earthmaterial.GroupType != string.Empty && currentProjectType.Contains(ApplicationThemeBedrock))
            {
                _earthLithQualifier = await FillAPicker(FieldEarthMatModComp, _earthmaterial.EarthMatLithgroup);
                //_earthLithOccurAs = await FillAPicker(FieldEarthMatOccurs, _model.EarthMatLithgroup);
                _earthLithTextureStruct = await FillAPicker(FieldEarthMatModTextStruc, _model.EarthMatLithgroup);
                _earthLithGrainSize = await FillAPicker(FieldEarthMatGrSize, _earthmaterial.EarthMatLithgroup);
                OnPropertyChanged(nameof(EarthLithQualifier));
                OnPropertyChanged(nameof(EarthLithTextureStruct));
                OnPropertyChanged(nameof(EarthLithGrainSize));

                //_earthLithOccurAs.cboxItems.Clear();
                _earthLithOccurAs.cboxItems = _earthLithOccursAsAll.cboxItems.Where(f => f.itemParent != null && _earthmaterial.EarthMatLithgroup.Contains(f.itemParent)).ToList();
                if (_earthLithOccurAs.cboxItems.Count == 1)
                {
                    _earthLithOccurAs.cboxDefaultItemIndex = 0;
                }
                OnPropertyChanged(nameof(EarthLithOccurAs));

            }

        }

        /// <summary>
        /// Generic method to fill a needed picker control with vocabulary
        /// </summary>
        private async Task<ComboBox> FillAPicker(string fieldName, string extraField = "")
        {
            //Make sure to user default database rather then the prefered one. This one will always be there.
            return await da.GetComboboxListWithVocabAsync(TableEarthMat, fieldName, extraField);

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
                    if (_earthmaterial.ParentName == TableStation)
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
                        Controls.ComboBoxItem newItem = new Controls.ComboBoxItem();
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

            #region Process pickers
            if (EarthLithQualifierCollection.Count > 0)
            {
                Model.EarthMatModComp = ConcatenatedCombobox.PipeValues(EarthLithQualifierCollection); //process list of values so they are concatenated.
            }
            if (EarthLithTextStrucCollection.Count > 0)
            {
                Model.EarthMatModTextStruc = ConcatenatedCombobox.PipeValues(EarthLithTextStrucCollection); //process list of values so they are concatenated.
            }
            if (EarthLithGrainSizeCollection.Count > 0)
            {
                Model.EarthMatGrSize = ConcatenatedCombobox.PipeValues(EarthLithGrainSizeCollection); //process list of values so they are concatenated.
            }
            if (EarthLithBedThickCollection.Count > 0)
            {
                Model.EarthMatBedthick = ConcatenatedCombobox.PipeValues(EarthLithBedThickCollection); //process list of values so they are concatenated.
            }
            if (EarthLithDefFabCollection.Count > 0)
            {
                Model.EarthMatDefabric = ConcatenatedCombobox.PipeValues(EarthLithDefFabCollection); //process list of values so they are concatenated.
            }
            if (EarthLithContactRelationCollection.Count > 0)
            {
                Model.EarthMatContact = ConcatenatedCombobox.PipeValues(EarthLithContactRelationCollection); //process list of values so they are concatenated.
            }
            if (EarthLithoGroup.cboxItems.Count() > 0 && EarthLithoGroup.cboxDefaultItemIndex != -1)
            {
                Model.EarthMatLithgroup = EarthLithoGroup.cboxItems[EarthLithoGroup.cboxDefaultItemIndex].itemValue;
            }

            //Special cases pickers
            //XAML converters usually saves directly in the model except for these
            if (EarthLithDetail.cboxItems.Count() > 0 && EarthLithDetail.cboxDefaultItemIndex != -1)
            {
                //Special picker only available in surficial, conflicting with bedrock on initialization
                Model.EarthMatLithdetail = EarthLithDetail.cboxItems[EarthLithDetail.cboxDefaultItemIndex].itemValue; 
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

            if (_drillHole != null)
            {
                // if coming from station notes, calculate new alias
                Model.EarthMatDrillHoleID = _drillHole.DrillID;
                Model.EarthMatName = await idCalculator.CalculateEarthmatAliasAsync(_drillHole.DrillID, _drillHole.DrillIDName);
            }
            else if (Model.EarthMatDrillHoleID != null)
            {
                // if coming from field notes on a record edit that needs to be saved as a new record with stay/save
                SQLiteAsyncConnection currentConnection = da.GetConnectionFromPath(da.PreferedDatabasePath);
                List<DrillHole> parentAlias = await currentConnection.Table<DrillHole>().Where(e => e.DrillID == Model.EarthMatDrillHoleID.Value).ToListAsync();
                await currentConnection.CloseAsync();
                Model.EarthMatName = await idCalculator.CalculateEarthmatAliasAsync(Model.EarthMatDrillHoleID.Value, parentAlias.First().DrillIDName);
            }

            Model.EarthMatID = 0;
            
        }

        /// <summary>
        /// Will refill the form with existing values for update/editing purposes
        /// Will refill the form with existing values for update/editing purposes
        /// </summary>
        /// <returns></returns>
        public async Task Load()
        {
            if (_earthmaterial != null && _earthmaterial.EarthMatName != string.Empty)
            {
                //Special case for occurences
                //Setting model object will remove its value because of
                //how is the picker value converter built
                _occurenceLoadedValue = _earthmaterial.EarthMatOccurs;

                //Set model like actual record
                _model = _earthmaterial;

                //Refresh
                OnPropertyChanged(nameof(Model));


                #region Pickers
                //Select values in pickers
                foreach (ComboBoxItem cbox in EarthLithoGroup.cboxItems)
                {
                    if (cbox.itemValue == _earthmaterial.EarthMatLithgroup)
                    {
                        EarthLithoGroup.cboxDefaultItemIndex = EarthLithoGroup.cboxItems.IndexOf(cbox);
                        break;
                    }
                }
                OnPropertyChanged(nameof(EarthLithoGroup));


                List<string> bts = ConcatenatedCombobox.UnpipeString(_earthmaterial.EarthMatBedthick);
                _bedThickCollection.Clear(); //Clear any possible values first
                foreach (ComboBoxItem cbox in EarthLithBedThick.cboxItems)
                {
                    if (bts.Contains(cbox.itemValue) && !_bedThickCollection.Contains(cbox))
                    {
                        _bedThickCollection.Add(cbox);
                    }
                }
                OnPropertyChanged(nameof(EarthLithBedThickCollection));

                List<string> bfs = ConcatenatedCombobox.UnpipeString(_earthmaterial.EarthMatDefabric);
                _defFabCollection.Clear(); //Clear any possible values first
                foreach (ComboBoxItem cbox in EarthLithDefFab.cboxItems)
                {
                    if (bfs.Contains(cbox.itemValue) && !_defFabCollection.Contains(cbox))
                    {
                        _defFabCollection.Add(cbox);
                    }
                }
                OnPropertyChanged(nameof(EarthLithDefFabCollection));

                List<string> ecs = ConcatenatedCombobox.UnpipeString(_earthmaterial.EarthMatContact);
                _contactRelationCollection.Clear(); //Clear any possible values first
                foreach (string ec in ecs)
                {
                    ComboBoxItem new_ec = new ComboBoxItem();
                    new_ec.itemValue = ec;
                    new_ec.itemName = ec;
                    new_ec.canRemoveItem = true;
                    _contactRelationCollection.Add(new_ec);
                }
                OnPropertyChanged(nameof(EarthLithContactRelationCollection));

                #endregion

                if (currentProjectType == ApplicationThemeSurficial)
                {
                    List<string> textStrucs = ConcatenatedCombobox.UnpipeString(_earthmaterial.EarthMatModTextStruc);
                    _textStructCollection.Clear(); //Clear any possible values first
                    foreach (ComboBoxItem cbox in EarthLithTextureStruct.cboxItems)
                    {
                        if (textStrucs.Contains(cbox.itemValue) && !_textStructCollection.Contains(cbox))
                        {
                            _textStructCollection.Add(cbox);
                        }
                    }
                    OnPropertyChanged(nameof(EarthLithTextStrucCollection));

                    //Special picker for earth material details
                    //Else it interacts badly on initialization with bedrock form
                    foreach (ComboBoxItem cbox in EarthLithDetail.cboxItems)
                    {
                        if (cbox.itemValue == _earthmaterial.EarthMatLithdetail)
                        {
                            EarthLithDetail.cboxDefaultItemIndex = EarthLithDetail.cboxItems.IndexOf(cbox); break;
                        }
                    }
                    OnPropertyChanged(nameof(EarthLithDetail));

                }

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
            if (_earthmaterial.GroupType != string.Empty)
            {

                List<string> qualifiers = ConcatenatedCombobox.UnpipeString(_earthmaterial.EarthMatModComp);
                _qualifierCollection.Clear(); //Clear any possible values first
                foreach (ComboBoxItem cbox in EarthLithQualifier.cboxItems)
                {
                    if (qualifiers.Contains(cbox.itemValue) && !_qualifierCollection.Contains(cbox))
                    {
                        _qualifierCollection.Add(cbox);
                    }
                }
                OnPropertyChanged(nameof(EarthLithQualifierCollection));

                List<string> textStrucs = ConcatenatedCombobox.UnpipeString(_earthmaterial.EarthMatModTextStruc);
                _textStructCollection.Clear(); //Clear any possible values first
                foreach (ComboBoxItem cbox in EarthLithTextureStruct.cboxItems)
                {
                    if (textStrucs.Contains(cbox.itemValue) && !_textStructCollection.Contains(cbox))
                    {
                        _textStructCollection.Add(cbox);
                    }
                }
                OnPropertyChanged(nameof(EarthLithTextStrucCollection));

                List<string> grainSizes = ConcatenatedCombobox.UnpipeString(_earthmaterial.EarthMatGrSize);
                _grainSizeCollection.Clear(); //Clear any possible values first
                foreach (ComboBoxItem cbox in EarthLithGrainSize.cboxItems)
                {
                    if (grainSizes.Contains(cbox.itemValue) && !_grainSizeCollection.Contains(cbox))
                    {
                        _grainSizeCollection.Add(cbox);
                    }
                }
                OnPropertyChanged(nameof(EarthLithGrainSizeCollection));

                foreach (ComboBoxItem occ in EarthLithOccurAs.cboxItems)
                {
                    if (occ.itemValue == _occurenceLoadedValue || (occ.itemValue != string.Empty && occ.itemValue == _earthmaterial.EarthMatOccurs ) )
                    {
                        EarthLithOccurAs.cboxDefaultItemIndex = EarthLithOccurAs.cboxItems.IndexOf(occ); break;
                    }
                }
                OnPropertyChanged(nameof(EarthLithOccurAs));

            }

        }

        /// <summary>
        /// Will create a quick earthmat record inside earthmat table, from a given map position.
        /// A quick station will be created also.
        /// </summary>
        /// <param name="locationID"></param>
        /// <returns></returns>
        public async Task<Earthmaterial> QuickEarthmat(int locationID)
        {
            //Create a quick station and assign it 
            StationViewModel stationViewModel = new StationViewModel();
            _station = await stationViewModel.QuickStation(locationID);

            //Fill out model and save new record
            await InitModel();
            Earthmaterial quickEM = await da.SaveItemAsync(Model, false) as Earthmaterial;
            quickEM.IsMapPageQuick = true;

            return quickEM;

        }

        /// <summary>
        /// Will initialize the model with needed calculated fields
        /// </summary>
        /// <returns></returns>
        public async Task InitModel()
        {
            //Make sure it's for a new field book
            if (Model != null && Model.EarthMatID == 0 && _station != null)
            {
                //Get current application version
                Model.EarthMatStatID = _station.StationID;
                Model.EarthMatName = await idCalculator.CalculateEarthmatAliasAsync(_station.StationID, _station.StationAlias);
                OnPropertyChanged(nameof(Model));
            }

            //Might be coming from a less known parent
            if (Model != null && Model.EarthMatID == 0 && _drillHole != null)
            {
                //Get current application version
                Model.EarthMatDrillHoleID = _drillHole.DrillID;
                Model.EarthMatName = await idCalculator.CalculateEarthmatAliasAsync(_drillHole.DrillID, _drillHole.DrillIDName);
                OnPropertyChanged(nameof(Model));
            }
        }

        public async Task SetAndSaveModelAsync()
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
                    if (_earthmaterial.ParentName == TableStation)
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
