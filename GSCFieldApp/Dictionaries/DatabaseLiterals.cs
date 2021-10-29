using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSCFieldApp.Dictionaries
{
    public static class DatabaseLiterals
    {
        #region Database version
        public const double DBVersion = 1.5; //Will be used to verify loaded projects.
        #endregion

        #region Database names
        public const string DBName = "GSCFieldwork";
        #endregion

        #region Database field names
        public const string FieldLocationID = "LOCATIONID";//Version 1.0
        public const string FieldLocationAlias = "LOCATIONIDNAME";//Version 1.5
        public const string FieldLocationAliasDeprecated = "LOCATIONNAME";//Version < 1.5
        public const string FieldLocationLongitude = "LONGITUDE";//Version 1.0
        public const string FieldLocationLatitude = "LATITUDE";//Version 1.0
        public const string FieldLocationElevation = "ELEVATION"; //Version 1.0
        public const string FieldLocationMetaID = "METAID";//Version 1.0
        public const string FieldLocationElevationMethod = "ELEVMETHOD";//Version 1.0
        public const string FieldLocationEntryType = "ENTRYTYPE";//Version 1.0
        public const string FieldLocationErrorMeasure = "ERRORMEASURE";//Version 1.0
        public const string FieldLocationErrorMeasureType = "ERRORTYPEMEASURE";//Version 1.0
        public const string FieldLocationElevAccuracy = "ELEVACCURACY";//Version 1.39
        //public const string FieldLocationSatUsed = "SATSUED"; //Version 1.00
        public const string FieldLocationPDOP = "PDOP";//Version 1.39
        //public const string FieldLocationCoordsource = "COORDSOURCE"; //Version 1.39
        public const string FieldLocationDatum = "EPSG"; //Version 1.44
        public const string FieldLocationEasting = "EASTING";//Version 1.0
        public const string FieldLocationNorthing = "NORTHING";//Version 1.0
        //public const string FieldLocationDatumZone = "DATUMZONE"; //Version 1.0 DEL Version 1.44
        public const string FieldLocationNotes = "NOTES"; //Version 1.0
        public const string FieldLocationReportLink = "REPORT_LINK"; //Version 1.0

        public const string FieldUserInfoID = "METAID";//Version 1.0
        public const string FieldUserInfoUCode = "GEOLCODE";//Version 1.0
        public const string FieldUserInfoFWorkType = "PRJCT_TYPE";//Version 1.0
        public const string FieldUserIsActive = "ISACTIVE";//Version 1.0
        public const string FieldUserInfoPName = "PRJCT_NAME"; //Version 1.0
        public const string FieldUserInfoPCode = "PRJCT_CODE"; //Version 1.0
        //public const string FieldUserInfoPLeader = "PRJCT_LEAD";//Version 1.0
        public const string FieldUserInfoPLeaderFN = "PRJCT_LEAD_FIRSTNAME";
        public const string FieldUserInfoPLeaderMN = "PRJCT_LEAD_MIDNAME";
        public const string FieldUserInfoPLeaderLN = "PRJCT_LEAD_LASTNAME";
        //public const string FieldUserInfoGeologist = "GEOLOGIST"; //Version 1.0
        public const string FieldUserInfoFN = "FIRSTNAME";
        public const string FieldUserInfoMN = "MIDDLENAME";
        public const string FieldUserInfoLN = "LASTNAME";
        public const string FieldUserInfoProjectionType = "PRJCT_TYPE"; //Version 1.0
        //public const string FieldUserInfoProjectionDatum = "EPSG"; //Version 1.0
        public const string FieldUserInfoStationStartNumber = "STNSTARTNO"; //Version 1.0
        public const string FieldUserInfoVersion = "VERSION_APP"; //Version 1.0
        public const string FieldUserStartDate = "START_DATE"; //Version 1.0
        public const string FieldUserInfoVersionSchema = "VERSIONSCHEMA"; //Version 1.39
        public const string FieldUserInfoEPSG = "EPSG"; //Deprecated since 1.44, is now accessible in F_LOCATION

        public const string FieldStationID = "STATIONID";//Version 1.0
        public const string FieldStationAlias = "STATIONIDNAME";//Version 1.5
        public const string FieldStationAliasDeprecated = "STATIONNAME";//Version < 1.5
        public const string FieldStationObsID = "LOCATIONID";//Version 1.0
        public const string FieldStationLongitude = "LONGITUDE";//Version 1.0
        public const string FieldStationLatitude = "LATITUDE";//Version 1.0
        public const string FieldStationElevation = "ELEVATION"; //Version 1.0
        public const string FieldStationVisitDate = "VISITDATE"; //Version 1.0
        public const string FieldStationVisitTime = "VISITTIME"; //Version 1.0
        public const string FieldStationNote = "NOTES"; //Version 1.0
        public const string FieldStationSLSNote = "SLSNOTES"; //Version 1.0
        public const string FieldStationPhysEnv = "PHYSENV"; //Version 1.0
        public const string FieldStationAirPhotoNumber = "AIRPHOTO"; //Version 1.0
        public const string FieldStationTraverseNumber = "TRAVNO"; //Version 1.0
        public const string FieldStationObsType = "OBSTYPE"; //Version 1.0
        public const string FieldStationOCQuality = "OCQUALITY";
        public const string FieldStationOCSize = "OCSIZE";

        public const string FieldEarthMatID = "EARTHMATID";//Version 1.0
        public const string FieldEarthMatName = "EARTHMATIDNAME";//Version 1.5
        public const string FieldEarthMatNameDeprecated = "EARTHMATNAME";//Version < 1.5
        public const string FieldEarthMatStatID = "STATIONID";//Version 1.0
        public const string FieldEarthMatLithgroup = "LITHGROUP";//Version 1.0
        public const string FieldEarthMatLithtype = "LITHTYPE";//Version 1.0
        public const string FieldEarthMatLithdetail = "LITHDETAIL";//Version 1.0
        public const string FieldEarthMatMapunit = "MAPUNIT";//Version 1.0
        public const string FieldEarthMatOccurs = "OCCURAS";//Version 1.0
        public const string FieldEarthMatModStruc = "MODSTRUC";//Version 1.0 Concatenated field
        public const string FieldEarthMatModTexture = "MODTEXTURE";//Version 1.0 Concatenated field
        public const string FieldEarthMatModComp = "MODCOMP";//Version 1.0 Concatenated field
        public const string FieldEarthMatGrSize = "GRCRYSIZE";//Version 1.0 Concatenated field
        public const string FieldEarthMatDefabric = "DEFFABRIC";//Version 1.0 Concatenated field
        public const string FieldEarthMatColourF = "COLOURF";//Version 1.0
        public const string FieldEarthMatColourW = "COLOURW";//Version 1.0
        public const string FieldEarthMatColourInd = "COLOURIND";//Version 1.0
        public const string FieldEarthMatMagSuscept = "MAGSUSCEPT";//Version 1.0
        public const string FieldEarthMatContact = "CONTACT";//Version 1.0
        public const string FieldEarthMatContactUp = "CONTACTUP";//Version 1.0
        public const string FieldEarthMatContactLow = "CONTACTLOW";//Version 1.0
        public const string FieldEarthMatInterp = "INTERP";//Version 1.0
        public const string FieldEarthMatInterpConf = "INTERPCONF";//Version 1.0
        public const string FieldEarthMatBedthick = "BEDTHICK"; //Version 1.0 Concatenated field
        public const string FieldEarthMatNotes = "NOTES"; //Version 1.43

        public const string FieldSampleID = "SAMPLEID"; //Version 1.0
        public const string FieldSampleName = "SAMPLEIDNAME"; //Version 1.5
        public const string FieldSampleNameDeprecated = "SAMPLENAME";//Version < 1.5
        public const string FieldSampleNotes = "NOTES"; //Version 1.0
        public const string FieldSampleType = "SAMPLETYPE"; //Version 1.0
        public const string FieldSamplePurpose = "PURPOSE"; //Version 1.0
        public const string FieldSampleEarthmatID = "EARTHMATID"; //Version 1.0
        public const string FieldSampleAzim = "AZIMUTH";
        public const string FieldSampleDipPlunge = "DIPPLUNGE";
        public const string FieldSampleSurface = "SURFACE";
        public const string FieldSampleFormat = "FORMAT";
        public const string FieldSampleQuality = "QUALITY";

        public const string FieldDictionaryCodedTheme = "CODETHEME";//Version 1.0
        public const string FieldDictionaryCode = "CODE";//Version 1.0
        public const string FieldDictionaryDescription = "DESCRIPTIONEN";//Version 1.0
        public const string FieldDictionaryTermID = "TERMID";//Version 1.0
        public const string FieldDictionaryOrder = "ITEMORDER";//Version 1.0
        public const string FieldDictionaryVisible = "VISIBLE";//Version 1.0
        public const string FieldDictionaryCreator = "CREATOR";//Version 1.0
        public const string FieldDictionaryCreatorDate = "CREATEDATE";//Version 1.0
        public const string FieldDictionaryEditor = "EDITOR";//Version 1.0
        public const string FieldDictionaryEditorDate = "EDITDATE";//Version 1.0
        public const string FieldDictionaryRelatedTo = "RELATEDTO";//Version 1.0
        public const string FieldDictionaryDefault = "DEFAULTVALUE";//Version 1.0
        public const string FieldDictionaryEditable = "EDITABLE"; //Version 1.0

        public const string FieldDictionaryManagerCodedTheme = "CODETHEME";//Version 1.0
        public const string FieldDictionaryManagerAssignTable = "ASSIGNTABLE";//Version 1.0
        public const string FieldDictionaryManagerAssignField = "ASSIGNTOFIELD";//Version 1.0
        public const string FieldDictionaryManagerCodedThemeDescription = "THEMEDESC";//Version 1.0
        public const string FieldDictionaryManagerEditable = "CAN_EDIT";//Version 1.0
        public const string FieldDictionaryManagerSpecificTo = "SPECIFICTO";//Version 1.0

        public const string FieldFavoriteID = "ITEMID_ISFAVORITE";//Version 1.0

        public const string FieldDocumentID = "DOCUMENTID";//Version 1.0
        public const string FieldDocumentName = "DOCUMENTIDNAME";//Version 1.5
        public const string FieldDocumentNameDeprecated = "DOCUMENTNAME";//Version < 1.5
        public const string FieldDocumentCategory = "CATEGORY";//Version 1.0
        public const string FieldDocumentFileNo = "FILENUMBER";//Version 1.0
        public const string FieldDocumentFileName = "FILENAME";//Version 1.0
        public const string FieldDocumentDirection = "DIRECTION";//Version 1.0
        public const string FieldDocumentDescription = "DESCRIPTION";//Version 1.0
        public const string FieldDocumentRelatedID = "RELATIONID";//Version 1.0
        public const string FieldDocumentRelatedtable = "RELATIONTABLE"; //Version 1.0
        public const string FieldDocumentType = "DOCUMENTTYPE";//Version 1.0


        public const string FieldStructureID = "STRUCID";//Version 1.0
        public const string FieldStructureName = "STRUCIDNAME";//Version 1.5
        public const string FieldStructureNameDeprecated = "STRUCNAME";//Version < 1.5
        public const string FieldStructureClass = "STRUCCLASS"; //Version 1.0
        public const string FieldStructureType = "STRUCTYPE"; //Version 1.0
        public const string FieldStructureDetail = "DETAIL"; //Verison 1.0
        public const string FieldStructureMethod = "METHOD";//Version 1.0
        public const string FieldStructureFormat = "FORMAT";//Version 1.0
        public const string FieldStructureAttitude = "ATTITUDE";//Version 1.0
        public const string FieldStructureYoung = "YOUNGING";//Version 1.0
        public const string FieldStructureGeneration = "GENERATION";//Version 1.0
        public const string FieldStructureStrain = "STRAIN";//Version 1.0
        public const string FieldStructureFlattening = "FLATTENING";//Version 1.0
        public const string FieldStructureRelated = "RELATED";//Version 1.0
        public const string FieldStructureSense = "SENSE";//Version 1.0
        public const string FieldStructureFabric = "FABRIC";//Version 1.0
        public const string FieldStructureAzimuth = "AZIMUTH";//Version 1.0
        public const string FieldStructureDip = "DIPPLUNGE";//Version 1.0
        public const string FieldStructureNotes = "NOTES";//Version 1.0
        public const string FieldStructureParentID = "EARTHMATID";//Version 1.0
        public const string FieldStructureSymAng = "SYMANG";//Version 1.0

        public const string FieldPFlowID = "PFLOWID";//Version 1.0
        public const string FieldPFlowName = "PFLOWIDNAME";//Version 1.0
        public const string FieldPFlowClass = "PFCLASS";//Version 1.0
        public const string FieldPFlowSense = "PFSENSE";//Version 1.0
        public const string FieldPFlowFeature = "PFTYPE";//Version 1.0
        public const string FieldPFlowMethod = "METHOD";//Version 1.0
        public const string FieldPFlowAzimuth = "PFAZIMUTH";//Version 1.0
        public const string FieldPFlowMainDir = "MAINDIR";//Version 1.0
        public const string FieldPFlowRelage = "RELAGE";//Version 1.0
        public const string FieldPFlowDip = "PFDIP";//Version 1.0
        public const string FieldPFlowNumIndic = "NUMINDIC";//Version 1.0
        public const string FieldPFlowDefinition = "PFQUALITY";//Version 1.0
        public const string FieldPFlowRelation = "RELATION";//Version 1.0
        public const string FieldPFlowBedsurf = "BEDSURF";//Version 1.0
        public const string FieldPFlowConfidence = "CONFIDENCE";//Version 1.0
        public const string FieldPFlowNotes = "NOTES";//Version 1.0
        public const string FieldPFlowParentID = "EARTHMATID";//Version 1.0

        public const string FieldFossilID = "FOSSILID";//Version 1.0
        public const string FieldFossilName = "FOSSILIDNAME";//Version 1.0
        public const string FieldFossilType = "FOSSILTYPE";//Version 1.0
        public const string FieldFossilNote = "NOTES";//Version 1.0
        public const string FieldFossilParentID = "EARTHMATID";//Version 1.0

        public const string FieldMineralID = "MINERALID";//Version 1.0
        public const string FieldMineralIDName = "MINERALIDNAME";//Version 1.0
        public const string FieldMineral = "MINERAL";//Version 1.0
        public const string FieldMineralForm = "FORM";//Version 1.0
        public const string FieldMineralHabit = "HABIT";//Version 1.0
        public const string FieldMineralOccurence = "OCCURRENCE";//Version 1.0
        public const string FieldMineralColour = "COLOUR";//Version 1.0
        public const string FieldMineralSizeMin = "SIZEMINMM";//Version 1.0
        public const string FieldMineralSizeMax = "SIZEMAXMM";//Version 1.0
        public const string FieldMineralMode = "M_MODE";//Version 1.0
        public const string FieldMineralNote = "NOTES";//Version 1.0
        public const string FieldMineralParentID = "EARTHMATID";//Version 1.0

        public const string FieldMineralAlterationID = "MAID";//Version 1.0
        public const string FieldMineralAlterationName = "MAIDNAME";//Version 1.0
        public const string FieldMineralAlteration = "MA";//Version 1.0
        public const string FieldMineralAlterationUnit = "UNIT";//Version 1.0
        public const string FieldMineralAlterationMineral = "MINERAL";//Version 1.0
        public const string FieldMineralAlterationMode = "M_MODE";//Version 1.0
        public const string FieldMineralAlterationDistrubute = "DISTRIBUTE";//Version 1.0
        public const string FieldMineralAlterationNotes = "NOTES";//Version 1.0
        public const string FieldMineralAlterationRelTable = "RELATIONTABLE";//Version 1.0
        public const string FieldMineralAlterationRelID = "RELATIONID";//Version 1.0


        #endregion

        #region Database table names
        public const string TableLocation = "F_LOCATION"; //Version 1.0
        public const string TableMetadata = "F_METADATA"; //Version 1.0
        public const string TableEarthMat = "F_EARTH_MATERIAL"; //Version 1.0
        public const string TableSample = "F_SAMPLE"; //Version 1.0
        public const string TableMineralAlteration = "F_MINERALIZATION_ALTERATION";//Version 1.0
        public const string TableStation = "F_STATION"; //Version 1.0
        public const string TableMineral = "F_MINERAL"; //Version 1.0
        public const string TableDocument = "F_DOCUMENT"; //Version 1.0
        public const string TableStructure = "F_STRUCTURE";
        public const string TableExternalMeasure = "F_EXT_MEASURE"; //Version 1.0
        public const string TableFossil = "F_FOSSIL"; //Version 1.0
        public const string TableEnvironment = "F_ENVIRONMENT"; //Version 1.0
        public const string TableSoilProfile = "F_SOILRPO"; //Version 1.0
        public const string TablePFlow = "F_PALEO_FLOW"; //Version 1.0
        public const string TableDictionary = "M_DICTIONARY"; //Version 1.0
        public const string TableDictionaryManager = "M_DICTIONARY_MANAGER"; //Version 1.0
        public const string TableFavorites = "F_FAVORITE";
        public const string TableTraversePoint = "FS_TRAVERSE_POINT";
        public const string TableTraverseLine = "FS_TRAVERSE_LINE";
        public const string TableFieldCamp = "FS_FIELDCAMP";

        #endregion

        #region Database extension types
        public const string DBTypeSqlite = ".sqlite";
        #endregion

        #region Database default values for field
        public const string DefaultFieldObservationType = "Outcrop";
        public const string DefaultNoData = "No Data";
        #endregion

        #region Database values for field
        public const string boolYes = "Y";
        public const string boolNo = "N";
        public const string picklistNACode = "N.A.";
        public const string picklistNADescription = "N.A.";
        public const string documentTableFileSuffix = ".jpg";
        public const string locationEntryTypeManual = "Manual";
        public const string locationEntryTypeTap = "Tap";

        #endregion

        #region Database keyword
        public const string KeywordStation = "station";
        public const string KeywordEarthmat = "earth";
        public const string KeywordSample = "sample";
        public const string KeywordStructure = "struc";
        public const string KeywordPhoto = "photo";
        public const string KeywordDocument = "document";
        public const string KeywordMA = "alteration"; 
        public const string KeywordMineral = "mineral";
        public const string KeywordFossil = "fossil";
        public const string KeywordPflow = "flow";
        public const string KeywordConcatCharacter = " | ";
        public const string KeywordLithgrouptype = "LITHGROUPTYPE"; //Used to detect lithgrouptype picklist selection in the picklist editor, to launch a semantic zoom data update.
        public const string KeywordLithDetail = "LITHDETAIL"; //Used to detect lithdetail picklist selection in the picklist editor, to launch a semantic zoom data update.
        public const string KeywordStrucClassType = "STRUCCLASSTYPE"; //Used to detect strucclasstype picklist selection in the picklist editor, to launch a semantic zoom data update.
        public const string KeywordStrucDetail = "STRUCDETAIL"; //Used to detect strucdetail picklist selection in the picklist editor, to launch a semantic zoom data update.
        public const string KeywordStationWaypoint = "waypoint";
        public const string KeywordDipDipDirectionRule = "dip"; //Used to calculate SYMANG field in structure table.
        public const string KeywordPlanar = "planar"; //Used to calculate SYMANG field in structure table.
        public const string KeywordLinear = "linear";
        public const string KeywordLocation = "location";
        public const string KeywordManual = "manual";
        public const string KeywordEPSGDefault = "4326"; //WGS 84

        #endregion

        #region Database guids

        public const string termIDErrorTypeMeasure_Meter = "2A97235A-D929-4CB8-A69C-33ADFDB06402";
        public const string termIDElevmethod_GPS = "A763BE23-9359-4A7A-99D9-3409D92102DF";
        public const string termIDEntryType_Tap = "a59a2780-26a2-4f76-82ec-530df105d59d";
        public const string termIDEntryType_Manual = "7b60543f-a147-4625-968a-72ef81beb567";

        #endregion

    }
}
