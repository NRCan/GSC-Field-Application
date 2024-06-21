using GSCFieldApp.Dictionaries;
using GSCFieldApp.Models;
using GSCFieldApp.Services.DatabaseServices;
using GSCFieldApp.Services.FileServices;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Template10.Common;
using Template10.Controls;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using Template10.Utils;
using System.Diagnostics;
using Esri.ArcGISRuntime.Geometry;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using GSCFieldApp.Views;
using Windows.UI.Xaml.Shapes;
using System.Data;
using System.Security.Cryptography;
using System.Xml.Linq;
using Path = System.IO.Path;

namespace GSCFieldApp.ViewModels
{

    public class FieldBooksPageViewModel : ViewModelBase
    {
        #region INITIALIZATION

        //UI
        private ObservableCollection<FieldBooks> _projectCollection = new ObservableCollection<FieldBooks>();
        public int _selectedProjectIndex = -1;
        private bool _noFieldBookWatermark = false;

        //Data
        readonly DataAccess accessData = new DataAccess();

        //Models
        readonly Station stationModel = new Station();
        readonly Metadata metadataModel = new Metadata();

        //Progress ring
        private bool _progressRingActive = false;
        private bool _progressRingVisibility = false;

        //Local settings
        readonly DataLocalSettings localSetting = new DataLocalSettings();
        readonly ApplicationDataContainer currentLocalSettings = ApplicationData.Current.LocalSettings;
        ResourceLoader loadLocalization = null;
        //Events
        public static event EventHandler deleteAllLayers; //This event is triggered when a factory reset is requested. Will need to wipe layers.
        public static event EventHandler<bool> fieldBooksUpdate; //This event is triggered whenever there is no more field books or one has been added
        public static event EventHandler<string> newFieldBookSelected; //This event is triggered when a field book has been sedt as active

        #endregion

        #region PROPERTIES
        /// <summary>
        /// A property that will hold all the available project item details
        /// </summary>
        public ObservableCollection<FieldBooks> ProjectCollection
        {
            get { return _projectCollection; }
            set { _projectCollection = value; }
        }

        public bool ProgressRingActive
        {
            get { return _progressRingActive; }
            set { _progressRingActive = value; }
        }
        public bool ProgressRingVisibility
        {
            get { return _progressRingVisibility; }
            set { _progressRingVisibility = value; }
        }
        public int SelectedProjectIndex { get { return _selectedProjectIndex; } set { _selectedProjectIndex = value; } }
        public bool NoFieldBookWatermark { get { return _noFieldBookWatermark; } set { _noFieldBookWatermark = value; } }

        #endregion

        public FieldBooksPageViewModel()
        {
            //Fill list view of projects
            _ = FillProjectCollectionAsync();

            //Detect new field book save
            GSCFieldApp.Views.FieldBookDialog.newFieldBookSaved -= FieldBookDialog_newFieldBookSaved;
            GSCFieldApp.Views.FieldBookDialog.newFieldBookSaved += FieldBookDialog_newFieldBookSaved;

            //Get localization for UI purposes
            loadLocalization = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
        }

        private void FieldBookDialog_newFieldBookSaved(object sender, EventArgs e)
        {
            //Send call to refresh other pages
            EventHandler<string> newFieldBookRequest = newFieldBookSelected;
            if (newFieldBookRequest != null && _selectedProjectIndex != -1)
            {
                newFieldBookRequest(this, _projectCollection[_selectedProjectIndex].ProjectPath);
            }

        }

        #region METHODS
        /// <summary>
        /// Will fill the project collection with information related to it
        /// </summary>
        private async Task<ObservableCollection<FieldBooks>> FillProjectCollectionAsync()
        {
            _projectCollection = new ObservableCollection<FieldBooks>();
            RaisePropertyChanged("ProjectCollection");
            List<string> invalidFieldBookToDelete = new List<string>();

            //Iterate through local state folder
            IReadOnlyList<StorageFolder> localStateFolders = await ApplicationData.Current.LocalFolder.GetFoldersAsync();
            IEnumerable<StorageFolder> reverseStateList = localStateFolders.Reverse();

            foreach (StorageFolder sf in reverseStateList)
            {
                //Get files 
                IReadOnlyList<StorageFile> localFiles = await sf.GetFilesAsync();
                foreach (StorageFile sfi in localFiles)
                {
                    //Get the database
                    if ((sfi.FileType.Contains(DatabaseLiterals.DBTypeSqlite) || sfi.FileType.Contains(DatabaseLiterals.DBTypeSqliteDeprecated))
                        && sfi.DisplayName == DatabaseLiterals.DBName)
                    {
                        FieldBooks currentDB = new FieldBooks();

                        using (SQLiteConnection currentConnection = accessData.GetConnectionFromPath(sfi.Path))
                        {
                            List<object> metadataTableRows = accessData.ReadTableFromDBConnectionWithoutClosingConnection(metadataModel.GetType(), string.Empty, currentConnection);
                            foreach (object m in metadataTableRows)
                            {
                                //For metadata
                                Metadata met = m as Metadata;
                                currentDB.CreateDate = met.StartDate;
                                currentDB.GeologistGeolcode = met.Geologist + "[" + met.UserCode + "]";
                                //currentDB.metadataForProject.FieldworkType = met.FieldworkType;
                                //currentDB.metadataForProject.MetaID = met.MetaID;
                                currentDB.ProjectPath = Path.GetDirectoryName(sfi.Path);
                                currentDB.ProjectDBPath = sfi.Path;
                                currentDB.metadataForProject = m as Metadata;
                            }

                            #region For stations
                            string stationQuerySelect = "SELECT *";
                            string stationQueryFrom = " FROM " + DatabaseLiterals.TableStation;
                            string stationQueryWhere = " WHERE " + DatabaseLiterals.TableStation + "." + DatabaseLiterals.FieldStationAlias + " NOT LIKE '%" + DatabaseLiterals.KeywordStationWaypoint + "%'";
                            string stationQueryFinal = stationQuerySelect + stationQueryFrom + stationQueryWhere;
                            List<object> stationCountResult = accessData.ReadTableFromDBConnectionWithoutClosingConnection(stationModel.GetType(), stationQueryFinal, currentConnection);

                            if (stationCountResult.Count != 0)
                            {
                                Station lastStation = (Station)stationCountResult[stationCountResult.Count - 1];
                                currentDB.StationLastEntered = lastStation.StationAlias;
                            }

                            //If field book is invalid keep parent folder path at least user will be able to delete it.
                            if (metadataTableRows.Count == 0 && stationCountResult.Count == 0)
                            {
                                StorageFolder parentFolder = await sfi.GetParentAsync();
                                currentDB.ProjectPath = parentFolder.Path;
                            }
                            #endregion

                            #region For locations
                            string queryLocation = "select count(*) from " + DatabaseLiterals.TableLocation;
                            List<int> locationCountResult = accessData.ReadScalarFromDBConnectionWithoutClosingConnection(queryLocation, currentConnection);
                            if (locationCountResult != null && locationCountResult.Count() > 0)
                            {
                                currentDB.StationNumber = locationCountResult[0].ToString();
                            }
                            else
                            {
                                currentDB.StationNumber = 0.ToString();
                            }


                            #endregion
                            if (!_projectCollection.Contains(currentDB))
                            {
                                _projectCollection.Add(currentDB);
                            }

                            currentConnection.Close();

                            break; //Forget about other files
                        }
                    }
                }
            }

            //Refresh UI
            //RaisePropertyChanged("ProjectCollection");

            //Select the current active project, so it's highlighted in the list view
            ResourceLoader loadLocal = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
            if (_projectCollection.Count == 0)
            {

                _noFieldBookWatermark = true;
                RaisePropertyChanged("NoFieldBookWatermark");

                //Send event about missing field books.
                fieldBooksUpdate?.Invoke(this, false);

            }
            else
            {
                SelectActiveProject();
                _noFieldBookWatermark = false;
                RaisePropertyChanged("NoFieldBookWatermark");

                //Send event about missing field books.
                fieldBooksUpdate?.Invoke(this, true);
            }

            return _projectCollection;
        }

        /// <summary>
        /// Will select the currently active project from the project list.
        /// </summary>
        public void SelectActiveProject()
        {
            //Make sure the setting exists, with user info id
            if (localSetting.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoID) != null)
            {
                //Get some unique values
                string currentUser = string.Empty;
                string currentProject = string.Empty;
                string currentActivity = string.Empty;

                try
                {
                    currentUser = localSetting.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoUCode).ToString();
                    currentProject = localSetting.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoPName).ToString();
                    currentActivity = localSetting.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoActivityName).ToString();

                }
                catch (Exception e)
                {
                    //Probably missing a setting, no big deal it'll fallback on first book.
                    Debug.WriteLine(e.Message);
                }

                //Iterate through project to find match
                foreach (FieldBooks prjs in _projectCollection)
                {


                    //Get match
                    if (prjs.metadataForProject.UserCode == currentUser &&
                        prjs.metadataForProject.MetadataActivity == currentActivity &&
                        prjs.metadataForProject.ProjectName == currentProject)
                    {
                        //Refresh UI
                        _selectedProjectIndex = _projectCollection.IndexOf(prjs);
                        RaisePropertyChanged("SelectedProjectIndex");

                        break;
                    }
                }
            }

        }

        /// <summary>
        /// From a given field book, will open a user info dialog
        /// </summary>
        /// <param name="stationReport"></param>
        public void PopUserInfo(FieldBooks projectDetails)
        {
            var modal = Window.Current.Content as ModalDialog;
            var view = modal.ModalContent as Views.FieldBookDialog;
            view = new Views.FieldBookDialog(projectDetails);
            //view.Tag = ReportDetailedStation;
            modal.ModalContent = view;
            modal.IsModal = true;
        }

        /// <summary>
        /// Will delete the selected field book from local state
        /// </summary>
        /// <param name="sender"></param>
        public async void ValidateDeleteProject(object sender)
        {
            //Get selected book
            FieldBooks selectedProject = _projectCollection[_selectedProjectIndex];

            //Ask user for backup
            ContentDialog backupDialog = new ContentDialog()
            {
                Title = loadLocalization.GetString("ProjectBackupTitle"),
                Content = loadLocalization.GetString("ProjectBackupContent"),
                PrimaryButtonText = loadLocalization.GetString("Generic_ButtonYes/Content"),
                SecondaryButtonText = loadLocalization.GetString("Generic_ButtonNo/Content")
            };
            backupDialog.Closed += BackupDialog_Closed;
            await backupDialog.ShowAsync();


        }

        /// <summary>
        /// Whenever the dialog as close continu with operation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void BackupDialog_Closed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            if (args.Result == ContentDialogResult.Primary)
            {
                //Make a field book bakcup
                await BackupFieldBook();

                DeleteFieldBook();

            }
            else
            {
                DeleteFieldBook();
            }
        }

        /// <summary>
        /// Will delete an entire field book and start fresh
        /// </summary>
        public async void DeleteFieldBook()
        {

            //Clear map page of any layers before deleting them, else they won't be.
            EventHandler deleteLayerRequest = deleteAllLayers;
            if (deleteLayerRequest != null)
            {
                deleteLayerRequest(this, null);
            }

            //In case user is deleting the only project
            if (_projectCollection.Count > 1)
            {
                //Update application to be set as the first project in the list
                int newIndex = 0;
                while (newIndex == _selectedProjectIndex)
                {
                    newIndex++;
                }

                FieldBooks resetProject = _projectCollection[newIndex];
                localSetting.SetSettingValue(ApplicationLiterals.KeywordFieldProject, _projectCollection[newIndex].ProjectPath);
                localSetting.SetSettingValue(DatabaseLiterals.FieldUserInfoFWorkType, _projectCollection[newIndex].metadataForProject.FieldworkType);
                localSetting.SetSettingValue(DatabaseLiterals.FieldUserInfoUCode, _projectCollection[newIndex].metadataForProject.UserCode);
                localSetting.SetSettingValue(DatabaseLiterals.FieldUserInfoID, _projectCollection[newIndex].metadataForProject.MetaID);
                DataAccess.DbPath = _projectCollection[newIndex].ProjectDBPath;

                ApplicationData.Current.SignalDataChanged();

            }



            //Delete
            if (_projectCollection[_selectedProjectIndex].ProjectPath != null && _projectCollection[_selectedProjectIndex].ProjectPath != string.Empty)
            {
                StorageFolder folderToDelete = await StorageFolder.GetFolderFromPathAsync(_projectCollection[_selectedProjectIndex].ProjectPath);
                try
                {
                    await folderToDelete.DeleteAsync();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }

            //Send call to refresh other pages
            EventHandler<string> newFieldBookRequest = newFieldBookSelected;
            if (newFieldBookRequest != null)
            {
                newFieldBookRequest(this, null);
            }

            //Refresh page
            _ =FillProjectCollectionAsync();

        }

        /// <summary>
        /// will pop a new user info dialog.
        /// </summary>
        public async Task AddNewProject()
        {
            //Init a new project if nothing has been done.
            if (_projectCollection.Count == 0)
            {
                bool dbExists = accessData.DoesDatabaseExists();

                if (!dbExists || localSetting.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoID) == null)
                {
                    //Wipe application setting of user info before iniating a new user dialog
                    localSetting.WipeUserInfoSettings();

                    await accessData.CreateDatabaseFromResource();

                    //Show UserInfoPart window as a modal dialog
                    WindowWrapper.Current().Dispatcher.Dispatch(() =>
                    {
                        var modal = Window.Current.Content as ModalDialog;
                        var view = modal.ModalContent as Views.FieldBookDialog;
                        modal.ModalContent = view = new Views.FieldBookDialog(null, true);
                        modal.IsModal = true;
                    });

                }

                //Send event about missing field books.
                fieldBooksUpdate?.Invoke(this, true);
            }
            else
            {
                //Show UserInfoPart window as a modal dialog
                WindowWrapper.Current().Dispatcher.Dispatch(() =>
                {
                    var modal = Window.Current.Content as ModalDialog;
                    var view = modal.ModalContent as Views.FieldBookDialog;
                    modal.ModalContent = view = new Views.FieldBookDialog(null);
                    modal.IsModal = true;
                });
            }


        }

        /// <summary>
        /// Will open a given field book with some default metadata values
        /// </summary>
        /// <param name="projectPath"></param>
        /// <param name="fieldworkType"></param>
        /// <param name="userCode"></param>
        /// <param name="metaID"></param>
        public async void OpenFieldBook(FieldBooks fieldBook, bool withNavigateToMap = true)
        {
            //Save in local setting
            SetFieldBook(fieldBook);

            //Navigate to map page
            if (withNavigateToMap)
            {
                INavigationService navService = BootStrapper.Current.NavigationService;
                navService.Navigate(typeof(Views.MapPage), false);
                await Task.CompletedTask;
            }


        }

        /// <summary>
        /// Will start a backup process for field books.
        /// </summary>
        /// <returns></returns>
        public async Task BackupFieldBook()
        {

            ContentDialog longProcessDialog = new ContentDialog()
            {
                Title = loadLocalization.GetString("SaveDBDialogTitle"),
                Content = loadLocalization.GetString("SaveDBDialogLongProcessContent"),
                PrimaryButtonText = loadLocalization.GetString("MapPageDialogTextYes"),
                SecondaryButtonText = loadLocalization.GetString("MapPageDialogTextNo"),
            };
            longProcessDialog.Style = (Style)Application.Current.Resources["WarningDialog"];

            ContentDialogResult cdr = await Services.ContentDialogMaker.CreateContentDialogAsync(longProcessDialog, true).Result;

            if (cdr == ContentDialogResult.Primary)
            {
                //Set progress ring
                _progressRingActive = true;
                _progressRingVisibility = true;
                RaisePropertyChanged("ProgressRingActive");
                RaisePropertyChanged("ProgressRingVisibility");

                //Variables
                List<StorageFile> FilesToBackup = new List<StorageFile>();
                FileServices fs = new FileServices();

                //Iterate through field book folder files
                //Make sure something is selected and that user is not trying to delete the only project left
                if (_projectCollection != null && _selectedProjectIndex != -1)
                {
                    FieldBooks selectedBook = _projectCollection[_selectedProjectIndex];
                    StorageFolder fieldBook = await StorageFolder.GetFolderFromPathAsync(selectedBook.ProjectPath);

                    //Get a list of files from field book
                    IReadOnlyList<StorageFile> fieldBookPhotosRO = await fieldBook.GetFilesAsync();

                    //calculate new name for output database in the archive
                    string uCode = currentLocalSettings.Containers[ApplicationLiterals.LocalSettingMainContainer].Values[Dictionaries.DatabaseLiterals.FieldUserInfoUCode].ToString();
                    FileServices fService = new FileServices();
                    string newName = fService.CalculateDBCopyName(uCode) + DatabaseLiterals.DBTypeSqlite;
                    StorageFile databaseToRename = null;

                    //Build list of files to add to archive
                    foreach (StorageFile files in fieldBookPhotosRO)
                    {
                        //Get databases
                        if (files.Name.ToLower().Contains(DatabaseLiterals.DBTypeSqlite) && files.Name.Contains(Dictionaries.DatabaseLiterals.DBName))
                        {

                            databaseToRename = files;
                        }
                        else if (!files.Name.Contains("zip"))
                        {
                            FilesToBackup.Add(files);
                        }

                    }

                    //Copy and rename database
                    if (databaseToRename != null)
                    {
                        var loadLocalization = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();

                        //Zip and Copy
                        string outputZipFilePath = await fs.SaveArchiveCopy(selectedBook.ProjectPath,
                            selectedBook.metadataForProject.UserCode);
                    }
                }

                //Unset progress ring
                _progressRingActive = false;
                _progressRingVisibility = false;
                RaisePropertyChanged("ProgressRingActive");
                RaisePropertyChanged("ProgressRingVisibility");
            }

            return;

        }

        /// <summary>
        /// Will set in the local setting the input fieldbook
        /// </summary>
        /// <param name="fieldBook"></param>
        public void SetFieldBook(FieldBooks fieldBook)
        {
            //Clear previous field book settings
            localSetting.WipeUserMapSettings();

            //Update settings with new selected project
            localSetting.SetSettingValue(ApplicationLiterals.KeywordFieldProject, fieldBook.ProjectPath);
            localSetting.SetSettingValue(DatabaseLiterals.FieldUserInfoFWorkType, fieldBook.metadataForProject.FieldworkType);
            localSetting.SetSettingValue(DatabaseLiterals.FieldUserInfoUCode, fieldBook.metadataForProject.UserCode);
            localSetting.SetSettingValue(DatabaseLiterals.FieldUserInfoID, fieldBook.metadataForProject.MetaID);
            localSetting.SetSettingValue(DatabaseLiterals.FieldUserInfoVersionSchema, fieldBook.metadataForProject.VersionSchema);
            localSetting.SetSettingValue(DatabaseLiterals.FieldUserInfoPName, fieldBook.metadataForProject.ProjectName);
            localSetting.SetSettingValue(DatabaseLiterals.FieldUserInfoActivityName, fieldBook.metadataForProject.MetadataActivity);

            //Special setting for drill hole
            if (fieldBook.metadataForProject.FieldworkType.Contains(DatabaseLiterals.KeywordDrill))
            {
                localSetting.SetSettingValue(DatabaseLiterals.TableDrillHoles, true);
            }
            else
            {
                localSetting.SetSettingValue(DatabaseLiterals.TableDrillHoles, false);
            }
            

            ApplicationData.Current.SignalDataChanged();
            DataAccess.DbPath = fieldBook.ProjectDBPath;
        }

        #endregion

        #region EVENTS

        /// <summary>
        /// Event detected on project/field book selection change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ProjectList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_selectedProjectIndex != -1)
            {
                SetFieldBook(_projectCollection[_selectedProjectIndex]);
            }
        }

        /// <summary>
        /// User clicked open field book event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void projectOpenButton_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (_selectedProjectIndex != -1)
            {
                OpenFieldBook(_projectCollection[_selectedProjectIndex]);

                //Send call to refresh other pages
                EventHandler<string> newFieldBookRequest = newFieldBookSelected;
                if (newFieldBookRequest != null)
                {
                    newFieldBookRequest(this, null);
                }
            }

        }

        /// <summary>
        /// This button will pop a new user info dialog.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void projectAddButton_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Task addNewProject = AddNewProject();
            await addNewProject;

        }

        /// <summary>
        /// This button will pop a new user info dialog.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ProjectEditButton_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {

            //Update settings with new selected project
            if (_projectCollection != null && _selectedProjectIndex != -1 && _projectCollection.Count > 0)
            {
                //Get selected project
                FieldBooks selectedProject = _projectCollection[_selectedProjectIndex];

                //Build connection file
                SQLiteConnection selectedProjectConnection = new SQLiteConnection(selectedProject.ProjectDBPath);

                //Get metadata 
                List<object> inMeta = accessData.ReadTableFromDBConnection(metadataModel.GetType(), null, selectedProjectConnection);

                if (inMeta != null && inMeta.Count > 0)
                {
                    //Show UserInfoPart window as a modal dialog
                    WindowWrapper.Current().Dispatcher.Dispatch(() =>
                    {
                        selectedProject.metadataForProject = inMeta[0] as Metadata;

                        var modal = Window.Current.Content as ModalDialog;
                        var view = modal.ModalContent as Views.FieldBookDialog;
                        modal.ModalContent = view = new Views.FieldBookDialog(selectedProject);
                        view.ViewModel.projectEdit -= ViewModel_projectEdit;
                        view.ViewModel.projectEdit += ViewModel_projectEdit;
                        modal.IsModal = true;
                    });
                }

            }

        }

        /// <summary>
        /// This button will delete the selected project/field book from the local state folder.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void ProjectDeleteButton_TappedAsync(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {

            //Make sure something is selected and that user is not trying to delete the only project left
            if (_projectCollection != null && _selectedProjectIndex != -1)
            {
                FieldBooks selectedBook = _projectCollection[_selectedProjectIndex];

                ContentDialog deleteBookDialog = new ContentDialog()
                {
                    Title = loadLocalization.GetString("ProjectDeleteDialogTitle"),
                    Content = loadLocalization.GetString("ProjectDeleteDialogContent") + " (" + selectedBook.metadataForProject.Geologist + ", " + selectedBook.metadataForProject.ProjectName + ").",
                    PrimaryButtonText = loadLocalization.GetString("Generic_ButtonYes/Content"),
                    SecondaryButtonText = loadLocalization.GetString("Generic_ButtonNo/Content")
                };
                deleteBookDialog.Style = (Style)Application.Current.Resources["DeleteDialog"];
                ContentDialogResult cdr = await deleteBookDialog.ShowAsync();

                if (cdr == ContentDialogResult.Primary)
                {
                    ValidateDeleteProject(this);
                }

            }
        }

        /// <summary>
        /// Event that is triggered when user has finished editing the project
        /// </summary>
        /// <param name="sender"></param>
        private void ViewModel_projectEdit(object sender)
        {
            //Refresh page
            _ =FillProjectCollectionAsync();

            RaisePropertyChanged("ProjectCollection");
        }

        /// <summary>
        /// This button will backup everything that is inside field book folder.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void ProjectBackup_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            await BackupFieldBook();
        }

        /// <summary>
        /// This button will take a zip archive as input and will create a field book out of it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void ProjectRestore_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            //Some variables
            bool isRestoreFromZip = false; //Will be used to renamed sqlite properly, because there could be multiple database in the zip.
            StorageFile copiedFile = null;

            //Set progress ring
            _progressRingActive = true;
            _progressRingVisibility = true;
            RaisePropertyChanged("ProgressRingActive");
            RaisePropertyChanged("ProgressRingVisibility");

            //Get zip archive from user
            FileOpenPicker openPicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.Desktop
            };
            openPicker.FileTypeFilter.Add(DatabaseLiterals.DBTypeSqlite);
            openPicker.FileTypeFilter.Add(DatabaseLiterals.DBTypeSqliteDeprecated);
            openPicker.FileTypeFilter.Add(".zip");

            StorageFile inFile = await openPicker.PickSingleFileAsync();
            if (inFile != null)
            {
                //Create a new field book folder
                int incrementer = 1; //Will be used to name project folders (pretty basic)
                string fieldProjectPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, incrementer.ToString()); //Wanted path for project data
                StorageFolder fieldFolder = ApplicationData.Current.LocalFolder; //Current folder object to local state
                bool breaker = false; //Will be used to break while clause whenever a folder has been created.
                while (!breaker)
                {
                    //If wanted project folder doesn't exist create, else find another name
                    if (!Directory.Exists(fieldProjectPath))
                    {
                        fieldFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(incrementer.ToString());
                        breaker = true;
                    }
                    else
                    {
                        incrementer++;
                    }

                    fieldProjectPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, incrementer.ToString());
                }


                //Copy to local state
                FileServices fileService = new FileServices();
                StorageFolder newFieldBookFolder = await StorageFolder.GetFolderFromPathAsync(fieldProjectPath);
                copiedFile = await inFile.CopyAsync(newFieldBookFolder);

                //Unzip if needed
                if (inFile.FileType.Contains("zip"))
                {
                    await fileService.GetFilesFromZip(fieldProjectPath, inFile);
                    isRestoreFromZip = true;
                }

                //Connect to the new database
                IReadOnlyList<StorageFile> storageFiles = await newFieldBookFolder.GetFilesAsync();
                StorageFile wantedDB = null;

                foreach (StorageFile sf in storageFiles)
                {
                    if (sf.Name.Contains(Dictionaries.DatabaseLiterals.DBTypeSqlite) || sf.Name.Contains(DatabaseLiterals.DBTypeSqliteDeprecated))
                    {
                        if (isRestoreFromZip)
                        {
                            if (inFile.Name.Contains(sf.Name.Split('.')[0]) || sf.Name == Dictionaries.DatabaseLiterals.DBName + Dictionaries.DatabaseLiterals.DBTypeSqlite
                                || sf.Name == Dictionaries.DatabaseLiterals.DBName + Dictionaries.DatabaseLiterals.DBTypeSqliteDeprecated)
                            {
                                wantedDB = sf;
                            }
                        }
                        else
                        {
                            wantedDB = sf;
                        }
                    }
                }

                //Set as new field book
                if (wantedDB != null)
                {
                    //Rename if needed
                    string extension = DatabaseLiterals.DBTypeSqlite;
                    if (!wantedDB.Name.Contains(extension))
                    {
                        if (wantedDB.Name.Contains(DatabaseLiterals.DBTypeSqliteDeprecated))
                        {
                            extension = DatabaseLiterals.DBTypeSqliteDeprecated;
                        }
                    }
                    if (wantedDB.Name != Dictionaries.DatabaseLiterals.DBName + extension)
                    {
                        await wantedDB.RenameAsync(Dictionaries.DatabaseLiterals.DBName + extension);
                    }


                    SQLiteConnection loadedDBConnection = accessData.GetConnectionFromPath(wantedDB.Path);

                    //Fill in current setting and change field book.
                    IEnumerable<object> metadata_raw = accessData.ReadTableFromDBConnection(metadataModel.GetType(), string.Empty, loadedDBConnection);
                    IEnumerable<Metadata> metadataTable = metadata_raw.Cast<Metadata>();

                    if (metadataTable != null && metadataTable.Count() > 0)
                    {
                        Metadata metItem = metadataTable.First() as Metadata;

                        //Display a warning for version validation
                        if (metItem.VersionSchema != DatabaseLiterals.DBVersion.ToString())
                        {
                            // Language localization using Resource.resw
                            var local = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();

                            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                            {
                                ContentDialog outDatedVersionDialog = new ContentDialog()
                                {
                                    Title = local.GetString("WarningBadVersionTitle"),
                                    Content = local.GetString("WarningBadVersionContent"),
                                    PrimaryButtonText = local.GetString("GenericDialog_ButtonOK")
                                };
                                outDatedVersionDialog.Style = (Style)Application.Current.Resources["WarningDialog"];
                                await Services.ContentDialogMaker.CreateContentDialogAsync(outDatedVersionDialog, false);

                            }).AsTask();

                        }
                        FieldBooks restFieldBook = new FieldBooks();
                        restFieldBook.ProjectPath = fieldProjectPath;
                        restFieldBook.metadataForProject = metItem;

                        SetFieldBook(restFieldBook);

                        _ = FillProjectCollectionAsync();
                    }
                    else
                    {
                        ContentDialog missingMetadataDialog = new ContentDialog()
                        {
                            Title = loadLocalization.GetString("Generic_MessageErrorTitle"),
                            Content = loadLocalization.GetString("UpgradeErrorMetadataContent"),
                            CloseButtonText = loadLocalization.GetString("GenericCloseLabel/Label"),
                        };
                        missingMetadataDialog.Style = (Style)Application.Current.Resources["DeleteDialog"];
                        ContentDialogResult cdr = await missingMetadataDialog.ShowAsync();

                    }

                }


            }

            //Delete zip folder if needed
            if (copiedFile != null && isRestoreFromZip)
            {
                await copiedFile.DeleteAsync();
            }

            //Unset progress ring
            _progressRingActive = false;
            _progressRingVisibility = false;
            RaisePropertyChanged("ProgressRingActive");
            RaisePropertyChanged("ProgressRingVisibility");

            _noFieldBookWatermark = false;
            RaisePropertyChanged("NoFieldBookWatermark");

            //Send event about missing field books.
            fieldBooksUpdate?.Invoke(this, true);

            //Send call to refresh other pages
            EventHandler<string> newFieldBookRequest = newFieldBookSelected;
            if (newFieldBookRequest != null)
            {
                newFieldBookRequest(this, null);
            }

        }

        /// <summary>
        /// This button will take upgrade a selected field book into current schema version
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 

        public async void ProjectUpgrade_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (System.IO.Directory.Exists(accessData.ProjectPath))
            {
                //Validate if selected fieldbook needs upgrade or can be upgraded
                //New field books or empty ones shouldn't be upgraded
                if (accessData.CanUpgrade())
                {

                    //Set progress ring
                    _progressRingActive = true;
                    _progressRingVisibility = true;
                    RaisePropertyChanged("ProgressRingActive");
                    RaisePropertyChanged("ProgressRingVisibility");

                    DataAccess dAccess = new DataAccess();

                    FileServices fService = new FileServices();

                    //Get local storage folder
                    StorageFolder localFolder = await StorageFolder.GetFolderFromPathAsync(accessData.ProjectPath);

                    //Check current database version and upgrade until up-to-date
                    double processedDBVersion = dAccess.GetDBVersion();

                    while (processedDBVersion < DatabaseLiterals.DBVersion)
                    {
                        string currentDBPath = DataAccess.DbPath;

                        //Keep current database path before creating the new one
                        string dbFolderToUpgrade = Path.GetDirectoryName(localFolder.Path);

                        //Create new fieldbook with embeded resource of legacy schema
                        string versionFileName = DatabaseLiterals.DBName;

                        if (processedDBVersion == 1.42)
                        {
                            //Skip straight to 1.44, since 1.43 only targeted picklist values
                            versionFileName = versionFileName + "_v" + DatabaseLiterals.DBVersion143.ToString().Replace(".", "") + DatabaseLiterals.DBTypeSqliteDeprecated;
                        }
                        else if (processedDBVersion == 1.43)
                        {
                            versionFileName = versionFileName + "_v" + DatabaseLiterals.DBVersion144.ToString().Replace(".", "") + DatabaseLiterals.DBTypeSqliteDeprecated;
                        }
                        else if (processedDBVersion == 1.44)
                        {
                            versionFileName = versionFileName + "_v" + DatabaseLiterals.DBVersion150.ToString().Replace(".", "") + "0" + DatabaseLiterals.DBTypeSqliteDeprecated;
                        }
                        else if (processedDBVersion == 1.5)
                        {
                            versionFileName = versionFileName + "_v" + DatabaseLiterals.DBVersion160.ToString().Replace(".", "") + "0" + DatabaseLiterals.DBTypeSqliteDeprecated;
                        }
                        else if (processedDBVersion == 1.6)
                        {
                            versionFileName = versionFileName + "_v" + DatabaseLiterals.DBVersion170.ToString().Replace(".", "") + "0" + DatabaseLiterals.DBTypeSqlite;
                        }
                        else if (processedDBVersion == 1.7)
                        {
                            //Current defaulting to 1.8
                            versionFileName = versionFileName + DatabaseLiterals.DBTypeSqlite;

                        }

                        string dbpathToUpgrade = Path.Combine(dbFolderToUpgrade, versionFileName); //in root of local state folder for now
                        //string oldVersionDbpathToUpgrade = Path.Combine(dbFolderToUpgrade, OldVersionFileName); //in root of local state folder for now

                        Task createNewDatabase = accessData.CreateDatabaseFromResourceTo(dbFolderToUpgrade, versionFileName);
                        await createNewDatabase;
                        if (createNewDatabase.IsCompleted)
                        {
                            //Connect to the new working database
                            SQLiteConnection upgradeDBConnection = accessData.GetConnectionFromPath(dbpathToUpgrade);

                            //Keep user vocab
                            accessData.GetLatestVocab(DataAccess.DbPath, upgradeDBConnection, processedDBVersion, false);

                            //Upgrade other tables
                            Task upgradeSchema = accessData.DoUpgradeSchema(DataAccess.DbPath, upgradeDBConnection, processedDBVersion);
                            upgradeDBConnection.Close();

                            if (upgradeSchema.IsCompleted && upgradeSchema.Status != TaskStatus.Faulted)
                            {
                                //Rename current fieldbook
                                string upgradedDBName = fService.CalculateDBCopyName(Dictionaries.DatabaseLiterals.DBNameSuffixUpgrade + processedDBVersion.ToString().Replace(".", ""));

                                string upgradedDBPath = Path.Combine(Path.GetDirectoryName(DataAccess.DbPath), upgradedDBName) + DatabaseLiterals.DBTypeSqlite;
                                string copyPathBeforeMove = currentDBPath;

                                //Legacy file type management for file naming
                                if (processedDBVersion <= DatabaseLiterals.DBVersion160)
                                {
                                    upgradedDBPath = upgradedDBPath.Replace(DatabaseLiterals.DBTypeSqlite, DatabaseLiterals.DBTypeSqliteDeprecated);
                                    copyPathBeforeMove = copyPathBeforeMove.Replace(DatabaseLiterals.DBTypeSqliteDeprecated, DatabaseLiterals.DBTypeSqlite);
                                }

                                //Move
                                GC.Collect();
                                GC.WaitForPendingFinalizers();
                                File.Move(currentDBPath, upgradedDBPath); //Rename temp one to it's previous name

                                //Copy upgraded version
                                File.Move(dbpathToUpgrade, copyPathBeforeMove);

                                //Update local settings
                                DataAccess.DbPath = copyPathBeforeMove;

                                //Last check on db version to keep iterating
                                processedDBVersion = dAccess.GetDBVersion();

                                //One last special move for legacy format
                                if (processedDBVersion == 1.7)
                                {
                                    //Force an update on geometry field
                                    string updateGeometryQuery = dAccess.GetGeopackageUpdateQuery(DatabaseLiterals.TableLocation);
                                    GeopackageService packService = new GeopackageService();
                                    packService.DoSpatialiteQueryInGeopackage(updateGeometryQuery, false);
                                }

                                //Another last op on geometry, to make sure they're all in WGS84
                                if (processedDBVersion == 1.8)
                                {
                                    GeopackageService packService = new GeopackageService();

                                    FieldLocation fl = new FieldLocation();
                                    List<object> flsObject = dAccess.ReadTable(fl.GetType(), null);
                                    IEnumerable<FieldLocation> flTable = flsObject.Cast<FieldLocation>();
                                    foreach (FieldLocation fls in flTable)
                                    {
                                        if (fls.LocationEPSGProj != "4326")
                                        {
                                            try
                                            {
                                                double easting = (double)fls.LocationEasting;
                                                double northing = (double)fls.LocationNorthing;
                                                int inEPSG = int.Parse(fls.LocationEPSGProj);

                                                SpatialReference inSR = SpatialReference.Create(inEPSG);

                                                MapPoint newPoint = packService.CalculateGeographicCoordinate(easting, northing, inSR);
                                                fls.LocationLat = newPoint.Y;
                                                fls.LocationLong = newPoint.X;
                                            }
                                            catch (Exception)
                                            {

                                            }
                                        }
                                    }
                                    List<object> newObj = flTable.Cast<object>().ToList(); 
                                    dAccess.BatchSaveSQLTables(newObj, true);

                                    //Force an update on geometry field
                                    string updateGeometryQuery = dAccess.GetGeopackageUpdateQuery(DatabaseLiterals.TableLocation);
                                    
                                    packService.DoSpatialiteQueryInGeopackage(updateGeometryQuery, false);
                                }

                                //Show end message if last upgrade in list
                                if (processedDBVersion == DatabaseLiterals.DBVersion180)
                                {
                                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                                    {
                                        ContentDialog upgradedDBDialog = new ContentDialog()
                                        {
                                            Title = loadLocalization.GetString("FieldBookUpgradeTitle"),
                                            Content = loadLocalization.GetString("FieldBookUpgradeContent"),
                                            PrimaryButtonText = loadLocalization.GetString("GenericDialog_ButtonOK")
                                        };
                                        upgradedDBDialog.Closed += upgradedDBDialog_Closed;
                                        await Services.ContentDialogMaker.CreateContentDialogAsync(upgradedDBDialog, false);

                                    }).AsTask();
                                }

                            }
                            else
                            {
                                if (upgradeSchema.Exception != null)
                                {
                                    //Show error message
                                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                                    {
                                        ContentDialog deleteBookDialog = new ContentDialog()
                                        {
                                            Title = "DB Error",
                                            Content = upgradeSchema.Exception.Message,
                                            PrimaryButtonText = "Bugger"
                                        };
                                        deleteBookDialog.Style = (Style)Application.Current.Resources["DeleteDialog"];
                                        await Services.ContentDialogMaker.CreateContentDialogAsync(deleteBookDialog, false);

                                    }).AsTask();
                                }

                            }

                        }


                    }

                    //Unset progress ring
                    _progressRingActive = false;
                    _progressRingVisibility = false;
                    RaisePropertyChanged("ProgressRingActive");
                    RaisePropertyChanged("ProgressRingVisibility");
                }
                else
                {
                    //Show warning stating current db doesn't need upgraded version
                    ContentDialog invalidUpgradeDBDialog = new ContentDialog()
                    {
                        Title = loadLocalization.GetString("FieldBookUpgradeTitle"),
                        Content = loadLocalization.GetString("FieldBookUpgradeContentInvalid"),
                        PrimaryButtonText = loadLocalization.GetString("GenericDialog_ButtonOK")
                    };
                    await invalidUpgradeDBDialog.ShowAsync();
                }

            }


        }

        private void upgradedDBDialog_Closed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            //Send call to refresh other pages
            _ = FillProjectCollectionAsync();
            EventHandler<string> newFieldBookRequest = newFieldBookSelected;
            if (newFieldBookRequest != null)
            {
                newFieldBookRequest(this, System.IO.Directory.GetParent(DataAccess.DbPath).FullName);
            }
        }
        #endregion
    }
}

