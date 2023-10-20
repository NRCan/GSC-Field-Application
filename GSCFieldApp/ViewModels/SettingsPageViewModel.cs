using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Template10.Mvvm;
using Windows.Storage;
using Windows.UI.Xaml;
using GSCFieldApp.Models;
using Windows.UI.Xaml.Controls;
using GSCFieldApp.Services.DatabaseServices;
using GSCFieldApp.Dictionaries;
using GSCFieldApp.Services.FileServices;
using SQLite;
using System.IO;
using System.Globalization;
using Windows.Foundation.Collections;

namespace GSCFieldApp.ViewModels
{

    public class SettingsPageViewModel : ViewModelBase
    {
        #region INITIALIZATION

        //Header toggles
        public bool _commonToggle = true;
        public bool _bedrockToggle = true;
        public bool _surficialToggle = false;

        //Sub toggles
        public bool _photoToggle = true;
        public bool _earthToggle = true;
        public bool _externalMeasureToggle = true;
        public bool _sampleToggle = true;

        public bool _mineralAlterationToggle = false;
        public bool _StructureToggle = true;
        public bool _fossilToggle = true;
        public bool _mineralToggle = true;
        public bool _drillToggle = true;

        public bool _environmentToggle = false;
        public bool _soilProfileToggle = false;
        public bool _pflowToggle = false;

        //Local setting
        readonly DataLocalSettings localSetting = new DataLocalSettings();

        //Events
        public static event EventHandler settingDeleteAllLayers; //This event is triggered when a factory reset is requested. Will need to wipe layers.

        //Other
        public bool isInit = false; //Will be used to toggle of some default toggles in the common groups.
        readonly DataAccess accessData = new DataAccess();
        public Visibility _loadPicklistVisibility = Visibility.Collapsed;
        public int _selectedPivotIndex = 0;

        #endregion

        #region PROPERTIES

        //Toggle buttons for table choice
        public bool CommonToggle {get { return _commonToggle; }
            set {
                _commonToggle = value;
                RaisePropertyChanged();
                localSetting.SetSettingValue(Dictionaries.DatabaseLiterals.ApplicationThemeCommon, value);
                ToggleCommons(value);
            }
        } 
        public bool BedrockToggle { get { return _bedrockToggle; }
            set
            {
                _bedrockToggle = value;
                RaisePropertyChanged();
                localSetting.SetSettingValue(Dictionaries.DatabaseLiterals.ApplicationThemeBedrock, value);
                ToggleBedrock(value);
            }
        }
        public bool SurficialToggle { get { return _surficialToggle; }
            set
            {
                _surficialToggle = value;
                RaisePropertyChanged();
                localSetting.SetSettingValue(Dictionaries.DatabaseLiterals.ApplicationThemeSurficial, value);
                ToggleSurficial(value);
            }
        }
        public bool PhotoToggle { get { return _photoToggle; } set { _photoToggle = value; RaisePropertyChanged(); localSetting.SetSettingValue(Dictionaries.DatabaseLiterals.TableDocument, value); } }
        public bool EarthToggle { get { return _earthToggle; } set { _earthToggle = value; RaisePropertyChanged(); localSetting.SetSettingValue(Dictionaries.DatabaseLiterals.TableEarthMat, value); } }
        public bool ExternalMeasureToggle { get { return _externalMeasureToggle; } set { _externalMeasureToggle = value; RaisePropertyChanged(); localSetting.SetSettingValue(Dictionaries.DatabaseLiterals.TableExternalMeasure, value); } }
        public bool SampleToggle { get { return _sampleToggle; } set { _sampleToggle = value; RaisePropertyChanged(); localSetting.SetSettingValue(Dictionaries.DatabaseLiterals.TableSample, value); } }
        public bool MAToggle { get { return _mineralAlterationToggle; } set { _mineralAlterationToggle = value; RaisePropertyChanged(); localSetting.SetSettingValue(Dictionaries.DatabaseLiterals.TableMineralAlteration, value); } }
        public bool StructureToggle { get { return _StructureToggle; } set { _StructureToggle = value; RaisePropertyChanged(); localSetting.SetSettingValue(Dictionaries.DatabaseLiterals.TableStructure, value); } }
        public bool FossilToggle { get { return _fossilToggle; } set { _fossilToggle = value; RaisePropertyChanged(); localSetting.SetSettingValue(Dictionaries.DatabaseLiterals.TableFossil, value); } }
        public bool MineralToggle { get { return _mineralToggle; } set { _mineralToggle = value; RaisePropertyChanged(); localSetting.SetSettingValue(Dictionaries.DatabaseLiterals.TableMineral, value); } }
        public bool EnvironmentToggle { get { return _environmentToggle; } set { _environmentToggle = value; RaisePropertyChanged(); localSetting.SetSettingValue(Dictionaries.DatabaseLiterals.TableEnvironment, value); } }
        public bool SoilProfileToggle { get { return _soilProfileToggle; } set { _soilProfileToggle = value; RaisePropertyChanged(); localSetting.SetSettingValue(Dictionaries.DatabaseLiterals.TableSoilProfile, value); } }
        public bool PflowToggle { get { return _pflowToggle; } set { _pflowToggle = value; RaisePropertyChanged(); localSetting.SetSettingValue(Dictionaries.DatabaseLiterals.TablePFlow, value); } }

        public bool DrillToggle { get { return _drillToggle; } set { _drillToggle = value; RaisePropertyChanged(); localSetting.SetSettingValue(Dictionaries.DatabaseLiterals.TableDrillHoles, value); } }

        //Other
        public Visibility LoadPicklistVisibility { get { return _loadPicklistVisibility; } set { _loadPicklistVisibility = value; } }
        public int SelectedPivotIndex { get { return _selectedPivotIndex; } set { _selectedPivotIndex = value; } }
        //Other view model referencing
        public SettingsPartViewModel SettingsPartViewModel { get; } = new SettingsPartViewModel();
        public AboutPartViewModel AboutPartViewModel { get; } = new AboutPartViewModel();
        public PicklistPartViewModel PicklistPartViewModel { get; } = new PicklistPartViewModel();

        public List<bool> commonSwitches = new List<bool>();
        public List<bool> bedrockSwitches = new List<bool>();
        public List<bool> surficialSwitches = new List<bool>();

        #endregion

        public SettingsPageViewModel()
        {
            InitializeToggleSwitches();
        }

        public void InitializeToggleSwitches()
        {
            #region Header toggle based on project type
            if (localSetting.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoFWorkType)!=null)
            {
                if (localSetting.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoFWorkType).ToString().Contains(Dictionaries.DatabaseLiterals.ApplicationThemeBedrock))
                {
                    _bedrockToggle = true;
                    _surficialToggle = false;
                }
                else if (localSetting.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoFWorkType).ToString() == Dictionaries.DatabaseLiterals.ApplicationThemeSurficial)
                {
                    _surficialToggle = true;
                    _bedrockToggle = false;
                }
            }

            #endregion

            #region Header toggles
            if (localSetting.GetSettingValue(Dictionaries.DatabaseLiterals.ApplicationThemeCommon)!=null)
            {
                _commonToggle = localSetting.GetBoolSettingValue(Dictionaries.DatabaseLiterals.ApplicationThemeCommon);

            }
            else
            {
                _commonToggle = true;

                //Apply default
                Switch_ChildSwitched(Dictionaries.DatabaseLiterals.ApplicationThemeCommon, _commonToggle);

                //Make sure fossil is by default not toggled on.
                _fossilToggle = false;
            }

            if (localSetting.GetSettingValue(Dictionaries.DatabaseLiterals.ApplicationThemeBedrock) != null)
            {
                _bedrockToggle = localSetting.GetBoolSettingValue(Dictionaries.DatabaseLiterals.ApplicationThemeBedrock);
            }
            else
            {
                _bedrockToggle = true;
                Switch_ChildSwitched(Dictionaries.DatabaseLiterals.ApplicationThemeBedrock, _bedrockToggle);
            }

            if (localSetting.GetSettingValue(Dictionaries.DatabaseLiterals.ApplicationThemeSurficial) != null)
            {
                _surficialToggle = localSetting.GetBoolSettingValue(Dictionaries.DatabaseLiterals.ApplicationThemeSurficial);
            }
            else
            {
                _surficialToggle = true;
                Switch_ChildSwitched(Dictionaries.DatabaseLiterals.ApplicationThemeSurficial, _surficialToggle);
            }

            if (localSetting.GetSettingValue(Dictionaries.DatabaseLiterals.ApplicationThemeDrillHole) != null)
            {
                _drillToggle = localSetting.GetBoolSettingValue(Dictionaries.DatabaseLiterals.ApplicationThemeDrillHole);
            }
            else
            {
                _drillToggle = true;
            }

            #endregion

            #region Child toggles

            if (localSetting.GetSettingValue(DatabaseLiterals.TableSample) != null)
            {
                SampleToggle = localSetting.GetBoolSettingValue(DatabaseLiterals.TableSample);
            }

            if (localSetting.GetSettingValue(DatabaseLiterals.TableMineral) != null)
            {
                MineralToggle = localSetting.GetBoolSettingValue(DatabaseLiterals.TableMineral);
            }

            if (localSetting.GetSettingValue(DatabaseLiterals.TableMineralAlteration) != null)
            {
                MAToggle = localSetting.GetBoolSettingValue(DatabaseLiterals.TableMineralAlteration);
            }

            if (localSetting.GetSettingValue(DatabaseLiterals.TableDocument) != null)
            {
                PhotoToggle = localSetting.GetBoolSettingValue(DatabaseLiterals.TableDocument);
            }

            if (localSetting.GetSettingValue(DatabaseLiterals.TableStructure) != null)
            {
                StructureToggle = localSetting.GetBoolSettingValue(DatabaseLiterals.TableStructure);
            }

            if (localSetting.GetSettingValue(DatabaseLiterals.TablePFlow) != null)
            {
                PflowToggle = localSetting.GetBoolSettingValue(DatabaseLiterals.TablePFlow);
            }

            if (localSetting.GetSettingValue(DatabaseLiterals.TableFossil) != null)
            {
                _fossilToggle = localSetting.GetBoolSettingValue(DatabaseLiterals.TableFossil);
            }
            if (localSetting.GetSettingValue(DatabaseLiterals.TableEarthMat) != null)
            {
                _earthToggle = localSetting.GetBoolSettingValue(DatabaseLiterals.TableEarthMat);
            }
            if (localSetting.GetSettingValue(DatabaseLiterals.TableEnvironment) != null)
            {
                _environmentToggle = localSetting.GetBoolSettingValue(DatabaseLiterals.TableEnvironment);
            }
            if (localSetting.GetSettingValue(DatabaseLiterals.TableDrillHoles) != null)
            {
                _drillToggle = localSetting.GetBoolSettingValue(DatabaseLiterals.TableDrillHoles);
            }
            //else
            //{
            //    _fossilToggle = false;
            //}
            RaisePropertyChanged("PhotoToggle");
            RaisePropertyChanged("EarthToggle");
            RaisePropertyChanged("ExternalMeasureToggle");
            RaisePropertyChanged("SampleToggle");
            RaisePropertyChanged("StructureToggle");
            RaisePropertyChanged("FossilToggle");
            RaisePropertyChanged("MineralToggle");
            RaisePropertyChanged("PflowToggle");
            RaisePropertyChanged("MAToggle");
            RaisePropertyChanged("EnvironmentToggle");
            RaisePropertyChanged("DrillToggle");
            #endregion

        }

        /// <summary>
        /// Will set sub toggles from parent toggle control value
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ToggleSwitch_Toggled(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ToggleSwitch senderSwitch = sender as ToggleSwitch;
            Switch_ChildSwitched(senderSwitch.Name, senderSwitch.IsOn);

        }

        /// <summary>
        /// From a given parent switch, will give the same switch value to its children
        /// </summary>
        /// <param name="parentSwitch"></param>
        private void Switch_ChildSwitched(string parentName, bool parentSwitchValue)
        {

            if (parentName.Contains(Dictionaries.DatabaseLiterals.ApplicationThemeCommon))
            {
                ToggleCommons(parentSwitchValue);
            }
            else if (parentName.Contains(Dictionaries.DatabaseLiterals.ApplicationThemeBedrock))
            {
                ToggleBedrock(parentSwitchValue);
            }
            else if (parentName.Contains(Dictionaries.DatabaseLiterals.ApplicationThemeSurficial))
            {
                ToggleSurficial(parentSwitchValue);
            }
            else if (parentName.ToLower().Contains(Dictionaries.ApplicationLiterals.KeywordTableEarthmat))
            {
                if (parentSwitchValue != EarthToggle)
                {
                    ToggleEarthChilds(parentSwitchValue);
                }
                
            }

        }

        /// <summary>
        /// Will switch the toggle value for common theme tables
        /// </summary>
        /// <param name="earthToggleValue"></param>
        public void ToggleEarthChilds(bool earthToggleValue)
        {

            SampleToggle = earthToggleValue;
            FossilToggle = earthToggleValue;
            EarthToggle = earthToggleValue;
            MineralToggle = earthToggleValue;
            PflowToggle = earthToggleValue;
            StructureToggle = earthToggleValue;

            RaisePropertyChanged("EarthToggle");
            RaisePropertyChanged("FossilToggle");
            RaisePropertyChanged("SampleToggle");
            RaisePropertyChanged("MineralToggle");
            RaisePropertyChanged("StructureToggle");
            RaisePropertyChanged("PflowToggle"); 

        }

        /// <summary>
        /// Will switch the toggle value for common theme tables
        /// </summary>
        /// <param name="commonToggleValue"></param>
        public void ToggleCommons(bool commonToggleValue)
        {

            PhotoToggle = commonToggleValue;
            ExternalMeasureToggle = commonToggleValue;
            SampleToggle = commonToggleValue;
            FossilToggle = commonToggleValue;
            EarthToggle = commonToggleValue;

            RaisePropertyChanged("EarthToggle"); 
            RaisePropertyChanged("FossilToggle");
            RaisePropertyChanged("CommonToggle");
            RaisePropertyChanged("PhotoToggle");
            RaisePropertyChanged("ExternalMeasureToggle");
            RaisePropertyChanged("SampleToggle");

        }


        /// <summary>
        /// Will switch the toggle value for bedrock theme tables
        /// </summary>
        /// <param name="commonToggleValue"></param>
        public void ToggleBedrock(bool bedrockToggleValue)
        {
            MAToggle = bedrockToggleValue;
            StructureToggle = bedrockToggleValue;
            MineralToggle = bedrockToggleValue;
            DrillToggle = bedrockToggleValue;

            RaisePropertyChanged("BedrockToggle");
            RaisePropertyChanged("MAToggle");
            RaisePropertyChanged("StructureToggle");
            RaisePropertyChanged("MineralToggle");
            RaisePropertyChanged("DrillToggle");

        }


        /// <summary>
        /// Will switch the toggle value for common theme tables
        /// </summary>
        /// <param name="commonToggleValue"></param>
        public void ToggleSurficial(bool surficialToggleValue)
        {

            EnvironmentToggle = surficialToggleValue;
            SoilProfileToggle = surficialToggleValue;
            PflowToggle = surficialToggleValue;

            RaisePropertyChanged("SurficialToggle");
            RaisePropertyChanged("EnvironmentToggle");
            RaisePropertyChanged("SoilProfileToggle");
            RaisePropertyChanged("PflowToggle");
        }

        /// <summary>
        /// An option to reset the application and start a clean slate version
        /// </summary>
        public async void ResetAppAsync()
        {
            var loadLocalization = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            ContentDialog deleteBookDialog = new ContentDialog()
            {
                Title = loadLocalization.GetString("SettingPageButtonResetTitle"),
                Content = loadLocalization.GetString("SettingPageButtonResetMessage/Text") ,
                PrimaryButtonText = loadLocalization.GetString("Generic_ButtonYes/Content"),
                SecondaryButtonText = loadLocalization.GetString("Generic_ButtonNo/Content")
            };
            deleteBookDialog.Style = (Style)Application.Current.Resources["DeleteDialog"];
            ContentDialogResult cdr = await deleteBookDialog.ShowAsync();

            if (cdr == ContentDialogResult.Primary)
            {
                //Clear map page of any layers before deleting them, else they won't be.
                EventHandler settingDeleteLayerRequest = settingDeleteAllLayers;
                if (settingDeleteLayerRequest != null)
                {
                    settingDeleteLayerRequest(this, null);
                }

                //Make a copy of database
                FileServices fileService = new FileServices();
                string newFilePath = await fileService.SaveDBCopy();

                if (newFilePath != null && newFilePath != string.Empty)
                {
                    Services.FileServices.FileServices deleteService = new Services.FileServices.FileServices();
                    Task deleteTask = deleteService.DeleteLocalStateFileAll();
                    await deleteTask;

                    //Delete local settings
                    Services.DatabaseServices.DataLocalSettings localSettings = new DataLocalSettings();
                    localSettings.WipeUserInfoSettings();

                    Application.Current.Exit();
                }

            }
        }
        
        /// <summary>
        /// An hidden double click to wipe (factory reset) the app
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Image_DoubleTapped(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            ResetAppAsync();
        }

        /// <summary>
        /// Whenever the user wants to upload any picklist from another database
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void settingLoadPicklistButton_TappedAsync(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            //Get local storage folder
            StorageFolder localFolder = await StorageFolder.GetFolderFromPathAsync(accessData.ProjectPath);

            //Create a file picker for sqlite 
            var filesPicker = new Windows.Storage.Pickers.FileOpenPicker
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop
            };
            filesPicker.FileTypeFilter.Add(DatabaseLiterals.DBTypeSqlite);
            filesPicker.FileTypeFilter.Add(DatabaseLiterals.DBTypeSqliteDeprecated);

            //Get users selected files
            StorageFile f = await filesPicker.PickSingleFileAsync();
            if (f != null)
            {
                // Create or overwrite file target file in local app data folder
                StorageFile fileToWrite = await localFolder.CreateFileAsync(f.Name, CreationCollisionOption.ReplaceExisting);

                //Copy else code won't be able to read it
                byte[] buffer = new byte[1024];
                using (BinaryWriter fileWriter = new BinaryWriter(await fileToWrite.OpenStreamForWriteAsync()))
                {
                    using (BinaryReader fileReader = new BinaryReader(await f.OpenStreamForReadAsync()))
                    {
                        long readCount = 0;
                        while (readCount < fileReader.BaseStream.Length)
                        {
                            int read = fileReader.Read(buffer, 0, buffer.Length);
                            readCount += read;
                            fileWriter.Write(buffer, 0, read);
 
                        }
                    }
                }

                //Connect to working database
                SQLiteConnection workingDBConnection = accessData.GetConnectionFromPath(DataAccess.DbPath);

                //Swap vocab
                accessData.DoSwapVocab(fileToWrite.Path, workingDBConnection);


                //Show end message
                var loadLocalization = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
                ContentDialog addedLayerDialog = new ContentDialog()
                {
                    Title = loadLocalization.GetString("SettingLoadPicklistProcessEndMessageTitle"),
                    Content = loadLocalization.GetString("SettingLoadPicklistProcessEndMessage/Text"),
                    PrimaryButtonText = loadLocalization.GetString("SettingLoadPicklistProcessEndMessageOk")
                };

                ContentDialogResult cdr = await addedLayerDialog.ShowAsync();

                FileServices fs = new FileServices();
                fs.DeleteLocalStateFile(fileToWrite.Path);

            }
        }

        /// <summary>
        /// Whenever the pivot tab changes, if it falls on picklist page, show a new optional button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MyPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Get info
            Pivot senderPivot = (Pivot)sender;
            PivotItem selectedPivotItem = (PivotItem)senderPivot.Items[_selectedPivotIndex];
            
            if (selectedPivotItem.Name.ToLower().Contains("picklist"))
            {
                _loadPicklistVisibility = Visibility.Visible;
                
            }
            else
            {
                _loadPicklistVisibility = Visibility.Collapsed;
            }
            RaisePropertyChanged("LoadPicklistVisibility");
        }

    }

    public class PicklistPartViewModel: ViewModelBase
    {

    }

    public class SettingsPartViewModel : ViewModelBase
    {
        //Events
        public static event EventHandler settingUseStructureSymbols; //This event is triggered when structure symbols can be used or not on map page.


        //Local setting
        readonly DataLocalSettings currentSettings = new DataLocalSettings();
        readonly DataAccess accessData = new DataAccess();
        readonly Services.SettingsServices.SettingsService _settings;

        public SettingsPartViewModel()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                // designtime
            }
            else
            {
                _settings = Services.SettingsServices.SettingsService.Instance;
                if (_settings.AppTheme != ApplicationTheme.Light)
                {
                    _settings.AppTheme = ApplicationTheme.Dark;
                    base.RaisePropertyChanged();
                }
                else
                {
                    _settings.AppTheme = ApplicationTheme.Light;
                    base.RaisePropertyChanged();
                }

            }
        }

        public bool UseShellBackButton
        {
            get { return false; }
            set { _settings.UseShellBackButton = value; base.RaisePropertyChanged(); }
        }

        public bool UseLightThemeButton
        {
            get { return _settings.AppTheme.Equals(ApplicationTheme.Light); }
            set { _settings.AppTheme = value ? ApplicationTheme.Light : ApplicationTheme.Dark; base.RaisePropertyChanged(); }
        }

        public bool UsePhotoModeDialogButton
        {
            get
            {
                if (currentSettings.GetSettingValue(ApplicationLiterals.KeywordDocumentMode) == null)
                {
                    return true;
                }
                else
                {
                    try
                    {
                        return (bool)currentSettings.GetSettingValue(Dictionaries.ApplicationLiterals.KeywordDocumentMode);
                    }
                    catch (Exception)
                    {

                        return true;
                    }
                    
                }
                
            }
            set
            {
                currentSettings.SetSettingValue(Dictionaries.ApplicationLiterals.KeywordDocumentMode, value);
                SetDocumentPicklist(value);
            }
        }

        public bool UseStationTraverseIncrementButton
        {
            get
            {
                if (currentSettings.GetSettingValue(ApplicationLiterals.KeywordStationTraverseNo) == null)
                {
                    return true;
                }
                else
                {
                    return (bool)currentSettings.GetSettingValue(Dictionaries.ApplicationLiterals.KeywordStationTraverseNo);
                }

            }
            set
            {
                currentSettings.SetSettingValue(Dictionaries.ApplicationLiterals.KeywordStationTraverseNo, value);
            }
        }

        public bool UseStructureSymbols
        {
            get
            {
                if (currentSettings.GetSettingValue(ApplicationLiterals.KeyworkStructureSymbols) == null)
                {
                    return false;
                }
                else
                {
                    return (bool)currentSettings.GetSettingValue(Dictionaries.ApplicationLiterals.KeyworkStructureSymbols);
                }

            }
            set
            {
                currentSettings.SetSettingValue(Dictionaries.ApplicationLiterals.KeyworkStructureSymbols, value);


                //Trigger event for map page
                EventHandler settingStructSymbolRequest = settingUseStructureSymbols;
                if (settingStructSymbolRequest != null)
                {
                    settingStructSymbolRequest(this, null);
                }
                
            }
        }

        /// <summary>
        /// Will enable some picklist terms in document type picklist based on user choice
        /// </summary>
        /// <param name="inValue"></param>
        public void SetDocumentPicklist(bool inValue)
        {
            //Get all document type picklist
            DataAccess accessDB = new DataAccess();
            IEnumerable<Vocabularies> docTypes = accessDB.GetPicklistValuesFromParent(Dictionaries.DatabaseLiterals.TableDocument, Dictionaries.DatabaseLiterals.FieldDocumentType, string.Empty, true);

            foreach (Vocabularies dTypes in docTypes)
            {
                //Photo mode
                if (inValue)
                {
                    //Only photo entries should be visible
                    if (dTypes.DefaultValue == Dictionaries.DatabaseLiterals.boolYes && dTypes.Editable == Dictionaries.DatabaseLiterals.boolNo)
                    {
                        dTypes.Visibility = Dictionaries.DatabaseLiterals.boolYes;
                    }
                    else if (dTypes.DefaultValue != Dictionaries.DatabaseLiterals.boolYes && dTypes.Editable == Dictionaries.DatabaseLiterals.boolNo)
                    {
                        dTypes.Visibility = Dictionaries.DatabaseLiterals.boolNo;
                    }
                    else if (dTypes.DefaultValue != Dictionaries.DatabaseLiterals.boolYes && dTypes.Editable == Dictionaries.DatabaseLiterals.boolYes)
                    {
                        //These would be user added values, keep them in photo mode.
                        dTypes.Visibility = Dictionaries.DatabaseLiterals.boolYes;
                    }
                }
                //Document mode
                else
                {
                    //All entries should be visible
                    dTypes.Visibility = Dictionaries.DatabaseLiterals.boolYes;
                }

                //Save model class
                //accessData.SaveFromSQLTableObject(dTypes, true);

                object dObject = (object)dTypes;
                accessData.SaveFromSQLTableObject(ref dObject, true);
                //dTypes = (Vocabularies)dObject;
            }

        }
    }

    public class AboutPartViewModel : ViewModelBase
    {

        public Uri Logo => Windows.ApplicationModel.Package.Current.Logo;

        public string DisplayName => Windows.ApplicationModel.Package.Current.DisplayName;

        public string Publisher => Windows.ApplicationModel.Package.Current.PublisherDisplayName;

        public string Version
        {
            get
            {
                var v = Windows.ApplicationModel.Package.Current.Id.Version;
                return $"{v.Major}.{v.Minor}.{v.Build}.{v.Revision}";
            }
        }

        public string VersionDB => DatabaseLiterals.DBVersion.ToString(CultureInfo.GetCultureInfo("en-US")); //Enforce english, else version will get a comma in french

        public Uri RateMe => new Uri("http://aka.ms/template10");
    }
}

