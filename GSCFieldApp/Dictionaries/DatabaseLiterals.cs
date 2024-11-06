
using GSCFieldApp.Models;

namespace GSCFieldApp.Dictionaries
{
    public static class DatabaseLiterals
    {
        #region Database version
        public const double DBVersion = 1.8; //Will be used to verify loaded projects.
        public const double DBVersion180 = 1.8; //Will be used to verify loaded projects.
        public const double DBVersion170 = 1.7; //Will be used to verify loaded projects.
        public const double DBVersion160 = 1.6; //Will be used to verify loaded projects.
        public const double DBVersion150 = 1.5; //Will be used to verify and upgrade loaded projects
        public const double DBVersion144 = 1.44; //Will be used to verify and upgrade loaded projects
        public const double DBVersion143 = 1.43; //Will be used to verify and upgrade loaded projects
        public const double DBVersion142 = 1.42; //Will be used to verify and upgrade loaded projects
        public const double DBVersion139 = 1.39; //Will be used to verify and upgrade loaded projects
        #endregion

        #region Database names
        public const string DBName = "GSCFieldwork";
        public const string DBNameSuffixUpgrade = "version_"; //Version number will be added to the right
        #endregion

        #region Database field names

        public const string FieldGenericRowID = "OBJECTID"; //Version 1.7 mandatory with geopackages.
        public const string FieldGenericGeometry = "geometry"; //Version 1.7

        public const string FieldLocationID = "LOCATIONID";//Version 1.0
        public const string FieldLocationAlias = "LOCATIONIDNAME";//Version 1.5
        public const string FieldLocationAliasDeprecated = "LOCATIONNAME";//Version < 1.5
        public const string FieldLocationLongitude = "LONGITUDE";//Version 1.0
        public const string FieldLocationLatitude = "LATITUDE";//Version 1.0
        public const string FieldLocationElevation = "ELEVATION"; //Version 1.0
        public const string FieldLocationMetaID = "METAID";//
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
        public const string FieldLocationDatumZone = "DATUMZONE"; //Version 1.0 DEL Version 1.44
        public const string FieldLocationNotes = "NOTES"; //Version 1.0
        public const string FieldLocationReportLink = "REPORT_LINK"; //Version < 1.5; back in version 1.7 stolen from station
        public const string FieldLocationNTS = "NTS"; //Version 1.6
        public const string FieldLocationEPSGProj = "EPSG_PROJ"; //Version 1.8
        public const string FieldLocationTimestamp = "LOCATIONTIMESTAMP"; //Version 1.8

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
        public const string FieldUserInfoNotes = "NOTES"; //Version 1.5
        public const string FieldUserInfoActivityName = "ACTIVITY_NAME"; //version 1.5

        public const string FieldStationID = "STATIONID";//Version 1.0
        public const string FieldStationAlias = "STATIONIDNAME";//Version 1.5
        public const string FieldStationAliasDeprecated = "STATIONNAME";//Version < 1.5
        public const string FieldStationObsID = "LOCATIONID";//Version 1.0
        public const string FieldStationLongitude = "LONGITUDE";//Version 1.0 Deprecated
        public const string FieldStationLatitude = "LATITUDE";//Version 1.0 Deprecated
        public const string FieldStationElevation = "ELEVATION"; //Version 1.0 Deprecated
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
        public const string FieldStationReportLinkDeprecated = "REPORT_LINK"; //Version 1.7, moved to location
        public const string FieldStationLegend = "LEGENDVAL";
        public const string FieldStationInterpretation = "LNDINTERP";
        public const string FieldStationRelatedTo = "RELATEDTO"; //Version 1.60
        public const string FieldStationObsSource = "OBSSOURCE"; //Version 1.60

        public const string FieldEarthMatID = "EARTHMATID";//Version 1.0
        public const string FieldEarthMatName = "EARTHMATIDNAME";//Version 1.5
        public const string FieldEarthMatNameDeprecated = "EARTHMATNAME";//Version < 1.5
        public const string FieldEarthMatStatID = "STATIONID";//Version 1.0
        public const string FieldEarthMatLithgroup = "LITHGROUP";//Version 1.0
        public const string FieldEarthMatLithtype = "LITHTYPE";//Version 1.0
        public const string FieldEarthMatLithdetail = "LITHDETAIL";//Version 1.0
        public const string FieldEarthMatMapunit = "MAPUNIT";//Version 1.0
        public const string FieldEarthMatOccurs = "OCCURAS";//Version 1.0
        public const string FieldEarthMatModStrucDeprecated = "MODSTRUC";//Version 1.0 Concatenated field
        public const string FieldEarthMatModTextureDeprecated = "MODTEXTURE";//Version 1.0 Concatenated field
        public const string FieldEarthMatModTextStruc = "TEXTSTRUC";//Version 1.6 Merger between MODESTRUC and MODTEXTURE Concatenated field
        public const string FieldEarthMatModCompDeprecated = "MODCOMP";//Version 1.0 Concatenated field Deprecated
        public const string FieldEarthMatModComp = "LITHQUALIFIER";//Version 1.6 Concatenated field
        public const string FieldEarthMatGrSize = "GRCRYSIZE";//Version 1.0 Concatenated field
        public const string FieldEarthMatDefabric = "DEFFABRIC";//Version 1.0 Concatenated field
        public const string FieldEarthMatColourF = "COLOURF";//Version 1.0
        public const string FieldEarthMatColourW = "COLOURW";//Version 1.0
        public const string FieldEarthMatColourInd = "COLOURIND";//Version 1.0
        public const string FieldEarthMatMagSuscept = "MAGSUSCEPT";//Version 1.0
        public const string FieldEarthMatContactDeprecated = "CONTACT";//Version 1.0
        public const string FieldEarthMatContact = "RELATED_CONTACT_NOTE"; //Version 1.6
        public const string FieldEarthMatContactUp = "CONTACTUP";//Version 1.0
        public const string FieldEarthMatContactLow = "CONTACTLOW";//Version 1.0
        public const string FieldEarthMatInterp = "INTERP";//Version 1.0
        public const string FieldEarthMatInterpConf = "INTERPCONF";//Version 1.0
        public const string FieldEarthMatBedthick = "BEDTHICK"; //Version 1.0 Concatenated field
        public const string FieldEarthMatNotes = "NOTES"; //Version 1.43
        public const string FieldEarthMatPercent = "EM_PERCENT"; //Version 1.60
        public const string FieldEarthMatMagQualifier = "MAGQUALIFIER"; //Version 1.60
        public const string FieldEarthMatMetaIntensity = "METAINTENSITY"; // Version 1.60
        public const string FieldEarthMatMetaFacies = "METAFACIES"; // Version 1.60
        public const string FieldEarthMatSorting = "SORTING"; //Version 1.7
        public const string FieldEarthMatH2O = "H2OCONTENT"; //Version 1.7
        public const string FieldEarthMatOxidation = "OXIDATION"; //Version 1.7
        public const string FieldEarthMatClastForm = "CLASTFORM"; //Version 1.7
        public const string FieldEarthMatContactNote = "CONTACT_NOTE"; //Version 1.8
        public const string FieldEarthMatDrillHoleID = "DRILLHOLEID"; //Version 1.8

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
        public const string FieldSampleHorizon = "HORIZON"; //Version 1.5
        public const string FieldSampleDepthMin = "DEPTHMIN"; //Version 1.5
        public const string FieldSampleDepthMax = "DEPTHMAX"; //Version 1.5
        public const string FieldSampleDuplicate = "DUPLICATE";//Version 1.5
        public const string FieldSampleDuplicateName = "DUPLICATENAME";//Version 1.5
        public const string FieldSampleState = "STATE";//Version 1.5
        public const string FieldCurationID = "CURATIONID";
        public const string FieldSampleWarehouseLocation = "WAREHOUSE_LOCATION"; //Version 1.7
        public const string FieldSampleBucketTray = "BUCKET_OR_TRAY_NO"; //Version 1.7
        public const string FieldSampleIsBlank = "ISBLANK"; //Version 1.8
        public const string FieldSampleCoreFrom = "CORE_FROM";//Version 1.8
        public const string FieldSampleCoreTo = "CORE_TO"; //Version 1.8
        public const string FieldSampleCoreLength = "CORE_LENGTH"; //Version 1.8
        public const string FieldSampleCoreSize = "CORE_SAMPLE_SIZE"; //Version 1.8
        public const string FieldSampledBy = "SAMPLED_BY"; //Version 1.8

        public const string FieldSampleManagementID = "SMID";
        public const string FieldDictionaryTermID = "TERMID";//Version 1.0
        public const string FieldDictionaryCodedTheme = "CODETHEME";//Version 1.0
        public const string FieldDictionaryCode = "CODE";//Version 1.0
        public const string FieldDictionaryDescription = "DESCRIPTIONEN";//Version 1.0
        public const string FieldDictionaryDescriptionFR = "DESCRIPTIONFR";
        public const string FieldDictionaryOrder = "ITEMORDER";//Version 1.0
        public const string FieldDictionaryVisible = "VISIBLE";//Version 1.0
        public const string FieldDictionaryCreator = "CREATOR";//Version 1.0
        public const string FieldDictionaryCreatorDate = "CREATEDATE";//Version 1.0
        public const string FieldDictionaryEditor = "EDITOR";//Version 1.0
        public const string FieldDictionaryEditorDate = "EDITDATE";//Version 1.0
        public const string FieldDictionaryRelatedTo = "RELATEDTO";//Version 1.0
        public const string FieldDictionaryDefault = "DEFAULTVALUE";//Version 1.0
        public const string FieldDictionaryEditable = "EDITABLE"; //Version 1.0
        public const string FieldDictionaryVersion = "VERSION"; //Version 1.5
        public const string FieldDictionaryRemarks = "USERREMARKS";
        public const string FieldDictionarySymbol = "SYMBOL";


        public const string FieldDictionaryManagerLinkID = "LINKID";//Version 1.0
        public const string FieldDictionaryManagerCodedTheme = "CODETHEME";//Version 1.0
        public const string FieldDictionaryManagerAssignTable = "ASSIGNTABLE";//Version 1.0
        public const string FieldDictionaryManagerAssignField = "ASSIGNTOFIELD";//Version 1.0
        public const string FieldDictionaryManagerCodedThemeDescription = "THEMEDESC";//Version 1.0
        public const string FieldDictionaryManagerOutputFile = "OUTPUTFILE";//Version 1.0
        public const string FieldDictionaryManagerEditable = "CAN_EDIT";//Version 1.0
        public const string FieldDictionaryManagerSpecificTo = "SPECIFICTO";//Version 1.0
        public const string FieldDictionaryManagerVersion = "VERSION"; //Version 1.5

        public const string FieldFavoriteID = "ITEMID_ISFAVORITE";//Version 1.0 deprecated

        public const string FieldDocumentID = "DOCUMENTID";//Version 1.0
        public const string FieldDocumentName = "DOCUMENTIDNAME";//Version 1.5
        public const string FieldDocumentNameDeprecated = "DOCUMENTNAME";//Version < 1.5
        public const string FieldDocumentCategory = "CATEGORY";//Version 1.0
        public const string FieldDocumentFileNo = "FILENUMBER";//Version 1.0
        public const string FieldDocumentFileName = "FILENAME";//Version 1.0
        public const string FieldDocumentDirection = "DIRECTION";//Version 1.0
        public const string FieldDocumentDescription = "DESCRIPTION";//Version 1.0
        public const string FieldDocumentRelatedIDDeprecated = "RELATIONID";//Version 1.0, version 1.8
        public const string FieldDocumentRelatedtableDeprecated = "RELATIONTABLE"; //Version 1.0, version 1.8
        public const string FieldDocumentType = "DOCUMENTTYPE";//Version 1.0
        public const string FieldDocumentHyperlink = "HYPERLINK";
        public const string FieldDocumentObjLocX = "OBJECTLOCX";
        public const string FieldDocumentObjLocY = "OBJECTLOCY";
        public const string FieldDocumentScaleDirection = "SCALE_DIRECTION"; //Version 1.8
        public const string FieldDocumentStationID = "STATIONID"; //Version 1.8
        public const string FieldDocumentSampleID = "SAMPLEID"; //Version 1.8
        public const string FieldDocumentDrillHoleID = "DRILLHOLEID"; //Version 1.8
        public const string FieldDocumentEarthMatID = "EARTHMATID"; //Version 1.8

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
        public const string FieldMineralFormDeprecated = "FORM";//Version 1.0 deprecated
        public const string FieldMineralHabitDeprecated = "HABIT";//Version 1.0 deprecated
        public const string FieldMineralOccurence = "OCCURRENCE";//Version 1.0
        public const string FieldMineralColour = "COLOUR";//Version 1.0
        public const string FieldMineralSizeMin = "SIZEMINMM";//Version 1.0
        public const string FieldMineralSizeMax = "SIZEMAXMM";//Version 1.0
        public const string FieldMineralMode = "M_MODE";//Version 1.0
        public const string FieldMineralNote = "NOTES";//Version 1.0
        public const string FieldMineralEMID = "EARTHMATID";//Version 1.0
        public const string FieldMineralFormHabit = "FORM_HABIT"; //Version 1.6
        public const string FieldMineralMAID = "MAID"; //Version 1.6

        public const string FieldMineralAlterationID = "MAID";//Version 1.0
        public const string FieldMineralAlterationName = "MAIDNAME";//Version 1.0
        public const string FieldMineralAlteration = "MA";//Version 1.0
        public const string FieldMineralAlterationUnit = "UNIT";//Version 1.0
        public const string FieldMineralAlterationMineralDeprecated = "MINERAL";//Version 1.0 deprecated
        public const string FieldMineralAlterationModeDeprecated = "M_MODE";//Version 1.0 deprecated
        public const string FieldMineralAlterationDistrubute = "DISTRIBUTE";//Version 1.0
        public const string FieldMineralAlterationNotes = "NOTES";//Version 1.0
        public const string FieldMineralAlterationRelTableDeprecated = "RELATIONTABLE";//Version 1.0 deprecated
        public const string FieldMineralAlterationRelIDDeprecated = "RELATIONID";//Version 1.0 deprecated
        public const string FieldMineralAlterationPhase = "PHASE"; //Version 1.6
        public const string FieldMineralAlterationTexture = "TEXTSTRUC"; //Version 1.6
        public const string FieldMineralAlterationFacies = "ALTERATION_FACIES"; //Version 1.6
        public const string FieldMineralAlterationStationID = "STATIONID"; //Version 1.8
        public const string FieldMineralAlterationEarthmatID = "EARTHMATID"; //Version 1.8

        public const string FieldEnvID = "ENVIRONID"; //Version 1.6
        public const string FieldEnvName = "ENVIRONIDNAME"; //Version 1.6
        public const string FieldEnvRelief = "RELIEF"; //Version 1.6
        public const string FieldEnvSlope = "SLOPE"; //Version 1.6
        public const string FieldEnvAzim = "AZIMUTH"; //Version 1.6
        public const string FieldEnvDrainage = "DRAINAGE"; //Version 1.6
        public const string FieldEnvPermIndicator = "PERMAFROST_INDICATOR"; //Version 1.6
        public const string FieldEnvGroundPattern = "GROUND_PATTERN"; //Version 1.6
        public const string FieldEnvActiveLayerDepth = "ACTIVE_LAYER_DEPTH"; //Version 1.6
        public const string FieldEnvGroundIce = "GROUND_ICE"; //Version 1.6
        public const string FieldEnvGroundCover = "GROUND_COVER"; //Version 1.6
        public const string FieldEnvBoulder = "BOULDER_FIELD"; //Version 1.6
        public const string FieldEnvExposure = "EXPOSURE"; //Version 1.6
        public const string FieldEnvNotes = "NOTES";//Version 1.6
        public const string FieldEnvStationID = "STATIONID"; //Version 1.6
        public const string FieldEnvMineralization = "MINIMPORT"; //Origins in Ganfeld, not implemented here, gossan and mineralization have their own form.
        public const string FieldEnvMineralizationNote = "MINNOTE"; //Origins in Ganfeld, not implemented here, gossan and mineralization have their own form.
        public const string FieldEnvGossan = "GOSSPRES"; //Origins in Ganfeld, not implemented here, gossan and mineralization have their own form.

        public const string FieldDrillID = "DRILLHOLEID"; //Version 1.8
        public const string FieldDrillIDName = "DRILLHOLEIDNAME"; //Version 1.8
        public const string FieldDrillName = "DRILLHOLE_ORIGINAL_NAME"; //Version 1.8
        public const string FieldDrillCompany = "COMPANY"; //Version 1.8
        public const string FieldDrillType = "DRILLHOLE_TYPE"; //Version 1.8
        public const string FieldDrillAzimuth = "AZIMUTH"; //Version 1.8
        public const string FieldDrillDip = "DIP"; //Version 1.8
        public const string FieldDrillDepth = "DH_LENGTH"; //Version 1.8
        public const string FieldDrillUnit = "UNIT"; //Version 1.8
        public const string FieldDrillDate = "DRILHOLE_DATE"; //Version 1.8
        public const string FieldDrillHoleSize = "DRILLHOLE_SIZE"; //Version 1.8
        public const string FieldDrillCoreSize = "DRILLCORE_SIZE"; //Version 1.8
        public const string FieldDrillRelogType = "RELOG_TYPE"; //Version 1.8
        public const string FieldDrillRelogBy = "RELOG_BY"; //Version 1.8
        public const string FieldDrillRelogIntervals = "RELOG_INTERVALS"; //Version 1.8
        public const string FieldDrillLog = "LOG_SUMMARY"; //Version 1.8
        public const string FieldDrillNotes = "NOTES"; //Version 1.8
        public const string FieldDrillLocationID = "LOCATIONID"; //Version 1.8
        public const string FieldDrillRelatedTo = "RELATEDTO"; //Version 1.8
        public const string FieldDrillRelogDate = "RELOG_DATE"; //Version 1.8

        public const string FieldTravPointID = "TRAV_ID"; //Version 1.7
        public const string FieldTravPointGeometry = "geometry"; //Version 1.7
        public const string FieldTravPointDate= "TRAV_DATE"; //Version 1.7
        public const string FieldTravPointPilot = "PILOT"; //Version 1.7
        public const string FieldTravPointOrderFlight= "ORDER_FLIGHT"; //Version 1.7
        public const string FieldTravPointOrderVisit = "ORDER_VISIT"; //Version 1.7
        public const string FieldTravPointGeologist = "GEOLOGIST"; //Version 1.7
        public const string FieldTravPointPartner = "PARTNER"; //Version 1.7
        public const string FieldTravPointPlannedBy = "PLANNED_BY"; //Version 1.7
        public const string FieldTravPointLabel = "LABEL"; //Version 1.7
        public const string FieldTravPointNotes = "NOTES"; //Version 1.7
        public const string FieldTravPointXUTM = "X_UTM"; //Version 1.7
        public const string FieldTravPointYUTM = "Y_UTM"; //Version 1.7
        public const string FieldTravPointXDMS = "X_DMS"; //Version 1.7
        public const string FieldTravPointYDMS = "Y_DMS"; //Version 1.7
        public const string FieldTravPointXDD = "X_DD"; //Version 1.7
        public const string FieldTravPointYDD = "Y_DD"; //Version 1.7
        public const string FieldTravPointNM = "NM"; //Version 1.7
        public const string FieldTravPointNMCamp = "NM_TO_CAMP"; //Version 1.7
        public const string FieldTravPointNTS = "NTS"; //Version 1.7
        public const string FieldTravPointAirPhoto = "AIR_PHOTO"; //Version 1.7
        public const string FieldTravPointMagDeclination = "MAG_DECLIN"; //Version 1.7

        public const string FieldLineworkID = "LINEWORKID"; //Version 1.9
        public const string FieldLineworkGeometry = "geometry"; //Version 1.9
        public const string FieldLineworkIDName = "LINEWORKDIDNAME"; //Version 1.9
        public const string FieldLineworkType = "LINETYPE"; //Version 1.9
        public const string FieldLineworkConfidence = "CONFIDENCE"; //Version 1.9
        public const string FieldLineworkSymbol = "SYMBOL"; //Version 1.9
        public const string FieldLineworkNotes = "NOTES"; //Version 1.9
        public const string FieldLineworkMetaID = "METAID"; //Version 1.9

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
        public const string TableExternalMeasure = "F_EXT_MEASURE"; //Version 1.0 Deprecated
        public const string TableFossil = "F_FOSSIL"; //Version 1.0
        public const string TableEnvironment = "F_ENVIRONMENT"; //Version 1.0 
        public const string TableSoilProfile = "F_SOILRPO"; //Version 1.0 Deprecated
        public const string TablePFlow = "F_PALEO_FLOW"; //Version 1.0
        public const string TableDictionary = "M_DICTIONARY"; //Version 1.0
        public const string TableDictionaryManager = "M_DICTIONARY_MANAGER"; //Version 1.0
        public const string TableFavorites = "F_FAVORITE"; //Deprecated
        public const string TableTraversePointDeprecated = "FS_TRAVERSE_POINT";//Deprecated
        public const string TableTraverseLineDeprecated = "FS_TRAVERSE_LINE";//Deprecated
        public const string TableTraversePoint = "F_TRAVERSE_POINT";//Version 1.7
        public const string TableTraverseLine = "F_TRAVERSE_LINE";//Version 1.7
        public const string TableFieldCampDeprecated = "FS_FIELDCAMP"; //Deprecated version 1.7
        public const string TableDrillHoles = "F_DRILL_HOLE"; //Version 1.8
        public const string TableLinework = "F_LINE_WORK"; //Version 1.9

        public enum TableNames { meta, location, station, earthmat, sample, mineralization, mineral, document, structure, fossil, environment, pflow, drill, linework};

        #endregion

        #region Database table views name

        public const string ViewPrefix = "view_"; //Version 1.7 - used for legacy data format prime key mitigation
        public const string ViewGenericLegacyPrimeKey = "PRIME"; //Version 1.7 - used for legacy data format prime key mitigation
        public const string ViewGenericLegacyForeignKey = "FORE"; //Version 1.7 - used for legacy data format prime key mitigation

        #endregion

        #region Database extension types
        public const string DBTypeSqlite = ".gpkg"; //Version 1.7
        public const string DBTypeSqliteDeprecated = ".sqlite"; //Version < 1.7
        public const string DBTypeSqliteName = "geopackage"; //Version 1.7
        public const string DBTypeGeopackageWal = "-wal"; //Version 1.7
        public const string DBTypeGeopackageSHM = "-shm"; //Version 1.7
        public const string LayerTypeMBTiles = ".mbtiles";
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
        public const string sampleTypeOriented = "oriented";
        public const string samplePurposePaleomag = "paleomagnetism";
        public const string structurePlanarAttitudeTrend = "trend";
        public const string locationEntryTypeSatellite = "Satellite";
        public const string vocabularyLineType = "LINEWORK_TYPE";
        #endregion

        #region Database keyword
        public const string KeywordStation = "station";
        public const string KeywordDrill = "drill";
        public const string KeywordEarthmat = "earth";
        public const string KeywordSample = "sample";
        public const string KeywordStructure = "struc";
        public const string KeywordPhoto = "photo";
        public const string KeywordDocument = "document";
        public const string KeywordMA = "alteration";
        public const string KeywordMineral = "mineral";
        public const string KeywordFossil = "fossil";
        public const string KeywordEnvironment = "environ";
        public const string KeywordPflow = "flow";
        public const string KeywordConcatCharacter = " | ";
        public const string KeywordConcatCharacter2nd = " - "; //Used when first contact is already being used
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
        public const string KeywordDates = "dates"; //Used to enable/disable traverse date header in field notes
        public const int KeywordEPSGDefault = 4326; //WGS 84
        public const int KeywordEPSGMapsuiDefault = 3857; // WGS84 Spherical mercator
        public const string KeywordColourGeneric = "COLOUR_GENERIC"; //Name of the generic colour picklist within M_DICTIONNARY_MANAGER
        public const string KeywordColourIntensity = "COLOUR_INTENSITY"; //Name of the intensity colour picklist within M_DICTIONNARY_MANAGER
        public const string KeywordColourQualifier = "COLOUR_QUALIFIER"; //Name of the qualifier colour picklist within M_DICTIONNARY_MANAGER
        #endregion

        #region Database guids

        public const string termIDErrorTypeMeasure_Meter = "2A97235A-D929-4CB8-A69C-33ADFDB06402";
        public const string termIDElevmethod_GPS = "A763BE23-9359-4A7A-99D9-3409D92102DF";
        public const string termIDEntryType_Tap = "a59a2780-26a2-4f76-82ec-530df105d59d";
        public const string termIDEntryType_Manual = "7b60543f-a147-4625-968a-72ef81beb567";
        public const string termIDPaleoMagnetismSurficial = "A19B762E-C39D-4D20-80D7-81525F729A5E"; //sample purpose being paleomagnetism
        public const string termIDOriented = "6c4c5c6a-913f-4374-88de-90c4544be041"; //sample type being oriented

        #endregion

        #region Database Alias name prefixes

        public const string TableMineralAliasPrefix = "M";
        public const string TableDocumentAliasPrefix = "P";
        public const string TableMineralAlterationPrefix = "X";
        public const string TableEnvironmentPrefix = "E";
        public const string TableDrillHolePrefix = "DH";
        public const string TableLocationAliasSuffix = "XY";

        #endregion

        #region Application Themes
        public const string ApplicationThemeBedrock = "bedrock";
        public const string ApplicationThemeSurficial = "surficial";
        public const string ApplicationThemeForestry = "forestry";
        public const string ApplicationThemeCommon = "common";
        public const string ApplicationThemeDrillHole = "bedrock - drill hole";
        #endregion

    }
}
