using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Template10.Mvvm;
using Template10.Services.SettingsService;
using Windows.Storage;
using Windows.UI.Xaml;
using GSCFieldApp.Models;
using Template10.Common;
using Windows.UI.Xaml.Controls;
using GSCFieldApp.Services.DatabaseServices;
using GSCFieldApp.Dictionaries;
using Windows.ApplicationModel.Resources;
using Windows.Storage.Search;
using SQLite.Net;
using Template10.Services.NavigationService;
using Template10.Controls;
using System.IO;
using SQLite.Net.Platform.WinRT;
using GSCFieldApp.Services.FileServices;
using Windows.Storage.Pickers;
using Windows.Foundation;

namespace GSCFieldApp.ViewModels
{

    public class FieldBooksPageViewModel : ViewModelBase
    {
        #region INITIALIZATION

        //UI
        public ObservableCollection<FieldBooks> _projectCollection = new ObservableCollection<FieldBooks>();
        public int _selectedProjectIndex = -1;
        private bool _noFieldBookWatermark = false;

        //Data
        DataAccess accessData = new DataAccess();

        //Models
        Station stationModel = new Station();
        Metadata metadataModel = new Metadata();

        //Progress ring
        private bool _progressRingActive = false;
        private bool _progressRingVisibility = false;

        //Local settings
        DataLocalSettings localSetting = new DataLocalSettings();
        ApplicationDataContainer currentLocalSettings = ApplicationData.Current.LocalSettings;

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
            _projectCollection = new ObservableCollection<FieldBooks>();
            RaisePropertyChanged("ProjectCollection");

            //Fill list view of projects
            FillProjectCollectionAsync();

            //Detect new field book save
            GSCFieldApp.Views.FieldBookDialog.newFieldBookSaved -= FieldBookDialog_newFieldBookSaved;
            GSCFieldApp.Views.FieldBookDialog.newFieldBookSaved += FieldBookDialog_newFieldBookSaved;
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
        private async void FillProjectCollectionAsync()
        {
            _projectCollection.Clear();

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
                    if (sfi.FileType.Contains("sql") && sfi.DisplayName == DatabaseLiterals.DBName)
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

                            //For stations
                            string stationQuerySelect = "SELECT *";
                            string stationQueryFrom = " FROM " + DatabaseLiterals.TableStation;
                            string stationQueryWhere = " WHERE " + DatabaseLiterals.TableStation + "." + DatabaseLiterals.FieldStationAlias + " NOT LIKE '%" + DatabaseLiterals.KeywordStationWaypoint + "%'";
                            string stationQueryFinal = stationQuerySelect + stationQueryFrom + stationQueryWhere;
                            List<object> stationCountResult = accessData.ReadTableFromDBConnectionWithoutClosingConnection(stationModel.GetType(), stationQueryFinal, currentConnection);
                            if (stationCountResult != null && stationCountResult.Count > 0)
                            {
                                currentDB.StationNumber = stationCountResult.Count.ToString();
                            }
                            else if (stationCountResult != null && stationCountResult.Count == 0)
                            {
                                currentDB.StationNumber = "0";
                            }
                            else
                            {
                                currentDB.StationNumber = "?";
                            }
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

                            _projectCollection.Add(currentDB);

                            currentConnection.Close();
                            GC.Collect();
                            GC.WaitForPendingFinalizers();

                            System.Runtime.InteropServices.Marshal.ReleaseComObject(sfi);

                            break; //Forget about other files
                        }
                    }
                }
            }

            //Refresh UI
            RaisePropertyChanged("ProjectCollection");

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

        }

        /// <summary>
        /// Will select the currently active project from the project list.
        /// </summary>
        public void SelectActiveProject()
        {
            //Make sure the setting exists, with user info id
            if (localSetting.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoID) != null)
            {
                //Iterate through project to find match
                foreach (FieldBooks prjs in _projectCollection)
                {
                    //Get match
                    if (prjs.metadataForProject.MetaID == localSetting.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoID).ToString())
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
            var loadLocalization = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
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

                ApplicationData.Current.SignalDataChanged();
                DataAccess.DbPath = _projectCollection[newIndex].ProjectDBPath;
            }



            //Delete
            if (_projectCollection[_selectedProjectIndex].ProjectPath != null && _projectCollection[_selectedProjectIndex].ProjectPath != string.Empty)
            {
                StorageFolder folderToDelete = await StorageFolder.GetFolderFromPathAsync(_projectCollection[_selectedProjectIndex].ProjectPath);
                try
                {
                    await folderToDelete.DeleteAsync();
                }
                catch (Exception)
                {

                }
            }




            //If user is trying to delete the only loaded field book
            if (_projectCollection.Count == 1)
            {
                AddNewProject();
            }

            //Refresh page
            FillProjectCollectionAsync();


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
        public async void OpenFieldBook(string projectPath, string fieldworkType, string userCode, string metaID, string dbPath, string dbVersion, bool withNavigateToMap = true)
        {
            //Clear previous field book settings
            localSetting.WipeUserMapSettings();

            //Update settings with new selected project
            localSetting.SetSettingValue(ApplicationLiterals.KeywordFieldProject, projectPath);
            localSetting.SetSettingValue(DatabaseLiterals.FieldUserInfoFWorkType, fieldworkType);
            localSetting.SetSettingValue(DatabaseLiterals.FieldUserInfoUCode, userCode);
            localSetting.SetSettingValue(DatabaseLiterals.FieldUserInfoID, metaID);
            localSetting.SetSettingValue(DatabaseLiterals.FieldUserInfoVersionSchema, dbVersion);

            ApplicationData.Current.SignalDataChanged();
            DataAccess.DbPath = dbPath;

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
                string newName = fService.CalculateDBCopyName(uCode) + ".sqlite";
                StorageFile databaseToRename = null;

                //Build list of files to add to archive
                foreach (StorageFile files in fieldBookPhotosRO)
                {
                    //Get databases
                    if (files.Name.ToLower().Contains(".sqlite") && files.Name.Contains(Dictionaries.DatabaseLiterals.DBName))
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
                    StorageFile newFile = await databaseToRename.CopyAsync(fieldBook, newName);
                    FilesToBackup.Add(newFile);

                    //Zip and Copy
                    var loadLocalization = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
                    await fs.SaveArchiveCopy(FilesToBackup, selectedBook.ProjectPath, selectedBook.metadataForProject.UserCode, selectedBook.metadataForProject.ProjectName + "_");

                    await newFile.DeleteAsync();
                }



            }

            //Unset progress ring
            _progressRingActive = false;
            _progressRingVisibility = false;
            RaisePropertyChanged("ProgressRingActive");
            RaisePropertyChanged("ProgressRingVisibility");

            return;

        }

        #endregion

        #region EVENTS

        /// <summary>
        /// When a user wants to open an existing field book, change settings and navigate to map.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void projectOpenButton_TappedAsync(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            string pPath = _projectCollection[_selectedProjectIndex].ProjectPath;
            string wType = _projectCollection[_selectedProjectIndex].metadataForProject.FieldworkType;
            string uCode = _projectCollection[_selectedProjectIndex].metadataForProject.UserCode;
            string mID = _projectCollection[_selectedProjectIndex].metadataForProject.MetaID;
            string dbP = _projectCollection[_selectedProjectIndex].ProjectDBPath;
            string dbVersion = _projectCollection[_selectedProjectIndex].metadataForProject.VersionSchema;

            OpenFieldBook(pPath, wType, uCode, mID, dbP, dbVersion);

            //Send call to refresh other pages
            EventHandler<string> newFieldBookRequest = newFieldBookSelected;
            if (newFieldBookRequest != null)
            {
                newFieldBookRequest(this, null);
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
                SQLiteConnection selectedProjectConnection = new SQLiteConnection(new SQLitePlatformWinRT(), selectedProject.ProjectDBPath);

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

                var loadLocalization = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
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
            FillProjectCollectionAsync();

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
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.List;
            openPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            openPicker.FileTypeFilter.Add(".sqlite");
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
                    if (sf.Name.Contains(".sqlite"))
                    {
                        if (isRestoreFromZip)
                        {
                            if (inFile.Name.Contains(sf.Name.Split('.')[0]) || sf.Name == Dictionaries.DatabaseLiterals.DBName + Dictionaries.DatabaseLiterals.DBTypeSqlite)
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
                    if (wantedDB.Name != Dictionaries.DatabaseLiterals.DBName + Dictionaries.DatabaseLiterals.DBTypeSqlite)
                    {
                        await wantedDB.RenameAsync(Dictionaries.DatabaseLiterals.DBName + Dictionaries.DatabaseLiterals.DBTypeSqlite);
                    }

                    SQLiteConnection loadedDBConnection = accessData.GetConnectionFromPath(wantedDB.Path);

                    //Fill in current setting and change field book.
                    IEnumerable<object> metadata_raw = accessData.ReadTableFromDBConnection(metadataModel.GetType(), string.Empty, loadedDBConnection);
                    IEnumerable<Metadata> metadataTable = metadata_raw.Cast<Metadata>();
                    Metadata metItem = metadataTable.First() as Metadata;

                    //Display a warning for version validation
                    if (metItem.VersionSchema != DatabaseLiterals.DBVersion.ToString())
                    {
                        // Language localization using Resource.resw
                        var local = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();

                        ContentDialog outDatedVersionDialog = new ContentDialog()
                        {
                            Title = local.GetString("WarningBadVersionTitle"),
                            Content = local.GetString("WarningBadVersionContent") + " " + DatabaseLiterals.DBVersion.ToString(),
                            PrimaryButtonText = local.GetString("GenericDialog_ButtonOK")
                        };

                        outDatedVersionDialog.Style = (Style)Application.Current.Resources["WarningDialog"];
                        ContentDialogResult cdr = await outDatedVersionDialog.ShowAsync();
                    }

                    OpenFieldBook(fieldProjectPath, metItem.FieldworkType, metItem.UserCode, metItem.MetaID, wantedDB.Path, metItem.VersionSchema, false);
                    FillProjectCollectionAsync();
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
        public async void ProjectUpgrade_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (System.IO.Directory.Exists(accessData.ProjectPath))
            {
                //Validate if selected fieldbook needs upgrade or can be upgraded
                //New field books or empty ones shouldn't be upgraded
                if (accessData.CanUpgrade())
                {
                    DataAccess dAccess = new DataAccess();
                    FileServices fService = new FileServices();
                    //Get local storage folder
                    StorageFolder localFolder = await StorageFolder.GetFolderFromPathAsync(accessData.ProjectPath);

                    //Keep current database path before creating the new one
                    string dbFolderToUpgrade = Path.GetDirectoryName(localFolder.Path);
                    string dbpathToUpgrade = Path.Combine(dbFolderToUpgrade, DataAccess._dbName); //in root of local state folder for now

                    //Create new fieldbook
                    Task createNewDatabase = accessData.CreateDatabaseFromResourceTo(dbFolderToUpgrade);
                    await createNewDatabase;
                    if (createNewDatabase.IsCompleted)
                    {
                        //Connect to the new working database
                        SQLiteConnection upgradeDBConnection = accessData.GetConnectionFromPath(dbpathToUpgrade);

                        //Keep user vocab
                        accessData.GetLatestVocab(DataAccess.DbPath, upgradeDBConnection, false);

                        //Upgrade other tables
                        await accessData.DoUpgradeSchema(DataAccess.DbPath, upgradeDBConnection);

                    }

                    //Rename current fieldbook
                    string upgradedDBName = fService.CalculateDBCopyName(Dictionaries.DatabaseLiterals.DBNameSuffixUpgrade) + Dictionaries.DatabaseLiterals.DBTypeSqlite;
                    string upgradedDBPath = Path.Combine(Path.GetDirectoryName(DataAccess.DbPath), upgradedDBName);
                    string copyPathBeforeMove = DataAccess.DbPath;
                    File.Move(DataAccess.DbPath, upgradedDBPath); //Rename temp one to it's previous name

                    //Copy upgraded version
                    File.Move(dbpathToUpgrade, copyPathBeforeMove);

                    //Show end message
                    var loadLocalization = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
                    ContentDialog upgradedDBDialog = new ContentDialog()
                    {
                        Title = loadLocalization.GetString("FieldBookUpgradeTitle"),
                        Content = loadLocalization.GetString("FieldBookUpgradeContent"),
                        PrimaryButtonText = loadLocalization.GetString("GenericDialog_ButtonOK")
                    };

                    ContentDialogResult cdr = await upgradedDBDialog.ShowAsync();
                    if (cdr == ContentDialogResult.Primary)
                    {
                        FillProjectCollectionAsync();

                        //Send call to refresh other pages
                        EventHandler<string> newFieldBookRequest = newFieldBookSelected;
                        if (newFieldBookRequest != null)
                        {
                            newFieldBookRequest(this, System.IO.Directory.GetParent(DataAccess.DbPath).FullName);
                        }
                    }
                }
                else
                {
                    //Show warning stating current db doesn't need upgraded version
                    var loadLocalization = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
                    ContentDialog invalidUpgradeDBDialog = new ContentDialog()
                    {
                        Title = loadLocalization.GetString("FieldBookUpgradeTitle"),
                        Content = loadLocalization.GetString("FieldBookUpgradeContentInvalid"),
                        PrimaryButtonText = loadLocalization.GetString("GenericDialog_ButtonOK")
                    };
                    ContentDialogResult cdr = await invalidUpgradeDBDialog.ShowAsync();
                }

            }


        }
        #endregion
    }
}

