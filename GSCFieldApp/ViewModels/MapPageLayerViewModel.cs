using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;
using Windows.UI.Xaml.Controls;
using Garibaldi.Models;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Windows.Storage;
using System.IO;
using Windows.UI.Xaml;
using Template10.Common;
using Windows.ApplicationModel.Resources;

namespace Garibaldi.ViewModels
{
    public class MapPageLayerViewModel : ViewModelBase
    {
        #region INIT
        Files filesModels = new Files();
        public ArcGISTiledLayer _basemapLayer;
        public Map esriMap;

        private ObservableCollection<Files> _filenameValues = new ObservableCollection<Files>();
        private object _selectedLayer;

        public MapPageLayerViewModel(Map inMap)
        {
            //Get maps
            esriMap = inMap;

            //Fill the toggle list with files
            GetFiles();
        }

        #endregion

        #region PROPERTIES
        public ObservableCollection<Files> FilenameValues { get { return _filenameValues; } set { _filenameValues = value; } }
        public object SelectedLayer { get { return _selectedLayer; } set { _selectedLayer = value; } }
        #endregion

        #region METHODS
        /// <summary>
        /// Will build the list of tpks inside local state folder and upate view of dialog with it.
        /// </summary>
        public async void GetFiles()
        {
            //Clear current
            _filenameValues.Clear();

            // Get list of TPK files in, for the time being, the localFolder - this is not ideal
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            IReadOnlyList<StorageFile> fileList = await localFolder.GetFilesAsync();

            List<string> mapLayers = new List<string>();

            foreach (var layer in esriMap.AllLayers)
            {
                mapLayers.Add(layer.Name);
            }

            foreach (StorageFile file in fileList)
            {
                if (file.FileType.ToLower() == ".tpk")
                {
                    if (!mapLayers.Contains(Path.GetFileNameWithoutExtension(file.Name)))
                    {
                        var localUri = new Uri("file:\\" + file.Path);

                        _basemapLayer = new ArcGISTiledLayer(localUri);

                        await _basemapLayer.LoadAsync();
                        if (_basemapLayer.LoadStatus == Esri.ArcGISRuntime.LoadStatus.Loaded)
                        {
                            _basemapLayer.IsVisible = true;
                            esriMap.Basemap.BaseLayers.Add(_basemapLayer);
                        }
                    }
                }
            }

            // Create cells for each of the sublayers
            foreach (ArcGISTiledLayer sublayer in esriMap.AllLayers)
            {
                Files layerFile = new Files();
                layerFile.FileName = sublayer.Name;
                layerFile.FileVisible = sublayer.IsVisible;
                layerFile.FilePath = sublayer.Source.OriginalString;

                //Reverse order on screen to match what is available in SIG usually
                _filenameValues.Insert(0, layerFile);

            }

            RaisePropertyChanged("FilenameValues");
        }

        /// <summary>
        /// Will set the toggle switch visibility inside the layer and keep it in the current list of files
        /// </summary>
        public void SetVisibility(ToggleSwitch inSwitch)
        {
            if (esriMap!=null)
            {
                // Find the layer from the image layer
                var sublayer = esriMap.AllLayers.First(x => x.Name == inSwitch.Header.ToString());

                // Change sublayers visibility in the map
                if (sublayer != null)
                {
                    sublayer.IsVisible = inSwitch.IsOn;
                }

                // Find the layer from the image layer
                Files subFile = _filenameValues.First(x => x.FileName == inSwitch.Header.ToString());

                // Change sublayers visibility in the map
                if (subFile != null)
                {
                    subFile.FileVisible = inSwitch.IsOn;
                }

            }

        }

        /// <summary>
        /// Will finalize the changes wanted by the user
        /// </summary>
        public void EndDialog()
        {
            SetOrder();
        }

        /// <summary>
        /// Will set the maps (layers) order in the map control from user choices.
        /// </summary>
        public void SetOrder()
        {
            esriMap.Basemap.BaseLayers.Clear();
            foreach (Files orderedFiles in _filenameValues.Reverse()) //Reverse order while iteration because UI is reversed intentionnaly
            {
                //Build path
                Uri localUri = new Uri(orderedFiles.FilePath);

                //Build tile layer
                ArcGISTiledLayer orderedLayer = new ArcGISTiledLayer(localUri);
                orderedLayer.IsVisible = orderedFiles.FileVisible;
                orderedLayer.Name = orderedFiles.FileName;
                esriMap.Basemap.BaseLayers.Add(orderedLayer);
            }
        }

        /// <summary>
        /// Will remove a selected layer from the map but also delete the original file from the local state folder.
        /// </summary>
        public void DeleteFile()
        {
            if (esriMap != null && _selectedLayer != null)
            {
                // Get selected layer
                Files subFile = (Files)_selectedLayer;

                if (subFile.FilePath.Contains("file:\\"))
                {
                    //Show UserInfoPart window as a modal dialog
                    WindowWrapper.Current().Dispatcher.Dispatch(() =>
                    {
                        string deleteMessage = string.Empty;
                        ResourceLoader resources = ResourceLoader.GetForCurrentView();
                        deleteMessage = resources.GetString("DeleteDialog_textBox/Text") + " (" + subFile.FileName + ").";

                        var modal = Window.Current.Content as Template10.Controls.ModalDialog;
                        var view = modal.ModalContent as Views.DeleteDialog;
                        modal.ModalContent = view = new Views.DeleteDialog(deleteMessage);
                        modal.IsModal = true;
                        view.deleteRecordEvent += View_deleteRecordEvent; //Wait for any delete action from user (yes button click)
                    });


                }

            }
        }

        /// <summary>
        /// Event triggered when user really wants to delete the tpk (file)
        /// </summary>
        /// <param name="sender"></param>
        private async void View_deleteRecordEvent(object sender)
        {
            // Get selected layer
            Files subFile = (Files)_selectedLayer;

            // Find the layer from the image layer
            Layer sublayer = esriMap.AllLayers.First(x => x.Name == subFile.FileName);

            // Change sublayers visibility in the map
            if (sublayer != null)
            {
                esriMap.Basemap.BaseLayers.Remove(sublayer);
            }

            Services.FileServices.FileServices deleteLayerFile = new Services.FileServices.FileServices();
            await deleteLayerFile.DeleteLocalStateFile(subFile.FilePath);

            //Refresh dialog
            GetFiles();
        }
        #endregion


    }
}