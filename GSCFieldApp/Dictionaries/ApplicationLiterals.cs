namespace GSCFieldApp.Dictionaries
{
    public static class ApplicationLiterals
    {
        #region UI keyword 

        //Will be use to keep track of user header expansion value in local settings.
        public const string KeyworkdExpandLocation = "expandLocation";
        public const string KeyworkdExpandStation = "expandStation";
        public const string KeyworkdExpandEarthmat= "expandEarthmat";
        public const string KeyworkdExpandSample = "expandSample";
        public const string KeywordExpandDocument = "expandDocument";
        public const string KeywordExpandStructure = "expandStructure";
        public const string KeywordExpandPflow = "expandPFlow";
        public const string KeywordExpandFossil = "expandFossil";
        public const string KeywordExpandMineral = "expandMineral";
        public const string KeywordExpandMineralAlt = "expandMineralAlteration";
        public const string KeywordExpandEnv = "expandEnvironment";

        //Will be used to keep track of user photo or document model for document dialog
        public const string KeywordDocumentMode = "documentMode";
        public const string KeywordDocumentHeaderTrue = "File";
        public const string KeywordDocumentHeaderFalse = "Photo";

        //Will be used to keep track of user station travers incrementation option
        public const string KeywordStationTraverseNo = "StationTraverseIncrement";

        //Will be used to keep track of user need to symbolize structures in map page
        public const string KeyworkStructureSymbols = "StructureSymbols";

        //Will be used to keep track of user map view parameters
        public const string KeywordMapViewScale = "mapViewScale";
        public const string KeywordMapViewRotation = "mapViewRotation";
        public const string KeywordMapViewLayersOrder = "mapViewLayersOrder";
        public const string KeywordMapViewGrid = "mapViewGrid";
        public const string KeywordMapViewLayersVisibility = "mapViewLayersVisibility";

        //Will be used for field project management
        public const string KeywordFieldProject = "FieldProjectPath";

        //Will be used for backing data
        public const string KeywordBackupFileData = "BackupData";
        public const string KeywordBackupFilePhotos = "BackupPhotos";
        public const string KeywordBackupFileMaps = "BackupMaps";
        public const string KeywordBackupPhotoType = "BackupPhotoType";
        public const string KeywordBackupPhotoYoungest = "BackupPhotoYoungest";

        //Will be use for note theme visibility
        public const string KeywordTableEarthmat = "earth";

        //Will be use for graphics
        public const string currentLocationOVerlay = "OverlayerCurrentLocation";

        #endregion

        #region UI Parent/child management
        public const string parentChildLevel1Seperator = " - "; //Will be used to make a clear distinction between a parent and child values in a text box for a first level (semantic zoom)
        public const string parentChildLevel2Seperator = " ; "; //Will be used to make a clear distinction between a parent and child values in a text box for a second level (semantic zoom)
        #endregion

        #region Local settings

        public const string LocalSettingMainContainer = "GSCMain"; //UWP

        public const string preferenceDatabasePath = "DatabaseFilePath"; //MAUI

        #endregion

        #region Local defaults constants
        public const double defaultMapScale = 10000;
        #endregion

        #region Assets
        public const double structureSymbolsImageHeight = 101; //Will be use for symbol placement 
        public const double structureSymbolsImageWidth = 101; //Will be use for symbol placement
        #endregion

        #region Text Symbols

        public const string gpsModeTap = "&#xF0A55;";
        public const string gpsModeGPS = "&#xF01A4;";

        #endregion

        #region GIS

        public const string keywordWMS = "NRCan_GSCFieldApp"; //Will be used for WMS caching prefix and token generation
        public const string keywordWMSLayers = "&layers="; //Will be used to get wms layer names in the URL
        public const string aliasOSM = "Open Street Map"; //Used for map page layer menu
        public const string aliasStations = "Stations"; //Used for map page layer menu
        public const string aliasTraversePoint = "Traverses"; //Used for map page layer menu
        #endregion

        #region Files / Folders
        public const string errorLogFileNameExt = "GSCFieldLog.txt"; //Will be used to save all error logs into a single file
        public const string photoFolderSuffix = "_Photos"; //Will be used to create subfolders containing all photos for a given field book. Easier when zipping for backup.
        #endregion

        #region Easter eggs

        public static readonly List<string> funnyNames = new List<string>(){ "kitty", "hullabaloo", "canoodle", "doink", "little_pony", "unicorn", "my_sweet_little_fairy" };

        #endregion
    }
}
