﻿using System;
using System.Collections.Generic;
using System.Linq;
using GSCFieldApp.Models;
using static GSCFieldApp.Dictionaries.DatabaseLiterals;
using SQLite;
using GSCFieldApp.Dictionaries;


namespace GSCFieldApp.Services.DatabaseServices
{
    public class DataIDCalculation
    {

        readonly FieldLocation locationModel = new FieldLocation();
        readonly Station stationModel = new Station();
        readonly Earthmaterial earthmatModel = new Earthmaterial();
        readonly Sample sampleModel = new Sample();
        readonly Vocabularies vocabModel = new Vocabularies();
        readonly Document docModel = new Document();
        readonly Structure structModel = new Structure();
        readonly Paleoflow pflowModel = new Paleoflow();
        readonly Fossil fossilModel = new Fossil();
        readonly Mineral mineralModel = new Mineral();
        readonly Metadata metadataModel = new Metadata();
        readonly MineralAlteration minAlterationModel = new MineralAlteration();
        readonly EnvironmentModel envModel = new EnvironmentModel();
        readonly DrillHole drillHoleModel = new DrillHole();
        readonly DataAccess da = new DataAccess();

        private bool CustomSampleNameEnabled
        {
            get { return Preferences.Get(nameof(CustomSampleNameEnabled), false); }
            set { }
        }

        public DataIDCalculation()
        {
            
        }

        #region MODEL BASE

        #region LOCATION
        /// <summary>
        /// Will calculate a new alias for location, if wanted.
        /// Output can be either empty or like this 17GHV001XY
        /// </summary>
        /// <param name="inStationAlias"></param>
        /// <returns></returns>
        public async Task<string> CalculateLocationAliasAsync(string inStationAlias = "", string officerCode = "")
        {
            string finaleLocationString = inStationAlias;

            if (inStationAlias!=string.Empty)
            {
                finaleLocationString = inStationAlias + TableLocationAliasSuffix;
            }
            else
            {
                try
                {
                    //Querying with Linq
                    string locationQuerySelect = "SELECT " + FieldLocationAlias;
                    string locationQueryFrom = " FROM " + TableLocation;
                    string locationQueryWhere = " WHERE " + TableLocation + "." + FieldLocationAlias + " NOT LIKE '" + KeywordStationWaypoint + "%'";
                    string locationQueryOrderbyLimit = " ORDER BY " + FieldLocationAlias + " DESC LIMIT 1;";
                    string locationQueryFinal = locationQuerySelect + locationQueryFrom + locationQueryWhere + locationQueryOrderbyLimit;

                    List<FieldLocation> locs = await DataAccess.DbConnection.QueryAsync<FieldLocation>(locationQueryFinal);

                    //Get current year
                    string currentDate = DateTime.Now.Year.ToString();

                    //Get officer code
                    if (officerCode == string.Empty)
                    {
                        List<Metadata> mets = await DataAccess.DbConnection.QueryAsync<Metadata>(string.Format("select * from {0} limit 1", TableMetadata));
                        officerCode = mets[0].UserCode;
                    }

                    //Get actual last alias and extract it's number
                    int locationCount = locs.Count();
                    int lastCharacterNumber = 0;

                    if (locationCount > 0)
                    {
                        //Remove every possible suffix/prefix from the alias
                        string lastCharacters = locs[0].LocationAliasLight.Replace(TableLocationAliasSuffix, "").Replace(KeywordStationDrillHoleLight, "").Replace(KeywordStationWaypointLight, "");
                        lastCharacterNumber = Convert.ToInt32(lastCharacters);
                    }

                    //Increment
                    locationCount = lastCharacterNumber + 1;

                    //Pad current ID with 0 if needed
                    string outputStringID = string.Empty;
                    if (locationCount < 10)
                    {
                        outputStringID = "000" + locationCount.ToString();
                    }
                    else if (locationCount >= 10 && locationCount < 100)
                    {
                        outputStringID = "00" + locationCount.ToString();
                    }
                    else if (locationCount >= 100 && locationCount < 1000)
                    {
                        outputStringID = "0" + locationCount.ToString();
                    }
                    else
                    {
                        outputStringID = locationCount.ToString();
                    }

                    finaleLocationString = currentDate.Substring(currentDate.Length - 2) + officerCode + outputStringID + TableLocationAliasSuffix; //Ex: 16BEB001
                }
                catch (Exception e)
                {
                    new ErrorToLogFile(e).WriteToFile();
                }


            }

            return finaleLocationString;
        }
        #endregion

        #region METADATA/DICTIONARIES
        /// <summary>
        /// Will calculate a new metadata id for current user
        /// </summary>
        /// <returns></returns>
        public int CalculateMetadataID()
        {
            return GetHashCodeFromGUID();
        }

        /// <summary>
        /// Will calculate a new termID for new added values in picklist (Dictionary)
        /// </summary>
        /// <returns></returns>
        public string CalculateTermID()
        {
            return CalculateGUID();
        }
        #endregion

        #region STATION

        /// <summary>
        /// Will calculate a station alias name from a given station ID. Results: 16BEB001 (Year-Year-Geolcode-three digit number)
        /// </summary>
        /// <param name="currentID">The station ID to calculate the alias from</param>
        /// <param name="stationTime">The datetime object related to the station to get the year from</param>
        /// <returns></returns>
        public async Task<string> CalculateStationAliasAsync(DateTime stationDate, string currentGeolcode = "", bool followDrillAlias = true)
        {

            //Querying with Linq
            string stationQuerySelect = "SELECT " + FieldStationAlias;
            string stationQueryFrom = " FROM " + TableStation;
            string stationQueryWhere = " WHERE " + TableStation + "." + FieldStationObsType + " NOT LIKE '%" + KeywordStationWaypoint + "'";
            string stationQueryWhere2 = " OR " + TableStation + "." + FieldStationObsType + " IS NULL";
            string stationQueryOrderbyLimit = " ORDER BY " + FieldStationAlias + " DESC LIMIT 1;";
            string stationQueryFinal = stationQuerySelect + stationQueryFrom + stationQueryWhere + stationQueryWhere2 + stationQueryOrderbyLimit;

            List<Station> stats = await DataAccess.DbConnection.QueryAsync<Station>(stationQueryFinal);

            //Get current year
            string currentDate = stationDate.Year.ToString();

            //Get officer code
            if (currentGeolcode == string.Empty)
            {
                List<Metadata> mets = await DataAccess.DbConnection.QueryAsync<Metadata>(string.Format("select * from {0} limit 1", TableMetadata));
                currentGeolcode = mets[0].UserCode;
            }

            //Get initial station start number and officer code
            int stationCount = stats.Count();
            if (stationCount == 0)
            {
                List<Metadata> metStationCount = await DataAccess.DbConnection.QueryAsync<Metadata>(string.Format("select * from {0} limit 1", TableMetadata));
                stationCount = metStationCount[0].StationStartNumber;
            }
            else
            {
                //Get actual last alias and extract it's number
                List<char> lastCharacters = stats[0].StationAlias.ToList();
                List<char> lastNumbers = lastCharacters.GetRange(lastCharacters.Count() - 4, 4);
                string lastNumber = string.Empty;
                foreach (char c in lastNumbers)
                {
                    //Rebuild number
                    lastNumber = lastNumber + c;
                }
                int lastCharacterNumber = Convert.ToInt32(lastNumber);

                //Increment
                stationCount = lastCharacterNumber + 1;

            }

            if (followDrillAlias)
            {
                List<DrillHole> drills = await DataAccess.DbConnection.Table<DrillHole>().OrderByDescending(s => s.DrillIDName).ToListAsync();
                if (drills != null && drills.Count > 0)
                {
                    int drillIDNo = 0;
                    int.TryParse(drills[0].DrillAliasLight.Replace(TableDrillHolePrefix, ""), out drillIDNo);

                    //Increment only if it's higher then last drill hole
                    if (stationCount <= drillIDNo)
                    {
                        stationCount = drillIDNo + 1;
                    }
                    
                }
            }

            //Padd current ID with 0 if needed
            string outputStringID = string.Empty;
            if (stationCount < 10)
            {
                outputStringID = "000" + stationCount.ToString();
            }
            else if (stationCount >= 10 && stationCount < 100)
            {
                outputStringID = "00" + stationCount.ToString();
            }
            else if (stationCount >= 100 && stationCount < 1000)
            {
                outputStringID = "0" + stationCount.ToString();
            }
            else
            {
                outputStringID = stationCount.ToString();
            }

            string finaleStationString = currentDate.Substring(currentDate.Length - 2) + currentGeolcode + outputStringID; //Ex: 16BEB001

            return finaleStationString;


        }

        /// <summary>
        /// Will calculate a station waypoint alias name from a given station ID. Results: Waypoint1, Waypoint2, ... 
        /// </summary>
        /// <param name="currentID">The station ID to calculate the alias from</param>
        /// <param name="stationTime">The datetime object related to the station to get the year from</param>
        /// <returns></returns>
        public async Task<string> CalculateStationWaypointAlias(string currentGeolcode = "")
        {

            //Build query to get a waypoint term count
            string querySelect = "SELECT s." + FieldStationAlias + " ";
            string queryFrom = "FROM " + TableStation + " as s ";
            string queryWhere = "WHERE " + "s." + FieldStationObsType + " LIKE '%" + KeywordStationWaypoint + "%' ";
            string queryOrderBy = "ORDER BY s." + FieldStationAlias + " DESC LIMIT 1";
            string finalQuery = querySelect + queryFrom + queryWhere + queryOrderBy;

            //Get query result
            SQLiteAsyncConnection currentConnection = da.GetConnectionFromPath(da.PreferedDatabasePath);
            List<Station> waypoints = await currentConnection.QueryAsync<Station>(finalQuery);

            //Get officer code
            if (currentGeolcode == string.Empty)
            {
                List<Metadata> mets = await currentConnection.QueryAsync<Metadata>(string.Format("select * from {0} limit 1", TableMetadata));
                currentGeolcode = mets[0].UserCode;
            }

            //Try parsing value
            int waypointLastNumber = 1;
            if (waypoints.Count() > 0)
            {
                if (waypoints.First().StationAlias.ToString().Contains(KeywordStationWaypoint + currentGeolcode))
                {
                    waypointLastNumber = Convert.ToInt32(waypoints.First().StationAlias.ToString().Replace(KeywordStationWaypoint + currentGeolcode, "")) + 1;   //prefix is wp
                }
                else
                {
                    waypointLastNumber = Convert.ToInt32(waypoints.First().StationAlias.Substring(waypoints.First().StationAlias.Length - 3, 3)) + 1;  //probably do not need code above
                }
            }

            //Pad current ID with 0 if needed
            string outputStringID = string.Empty;

            if (waypointLastNumber < 10)
            {
                outputStringID = "00" + waypointLastNumber.ToString();
            }
            else if (waypointLastNumber >= 10 && waypointLastNumber < 100)
            {
                outputStringID = "0" + waypointLastNumber.ToString();
            }
            else
            {
                outputStringID = waypointLastNumber.ToString();
            }

            string finaleWaypointString = KeywordStationWaypoint + currentGeolcode + outputStringID;  //prefix is waypoint

            return finaleWaypointString;

        }

        #endregion

        #region EARTHMAT
        /// <summary>
        /// Will calculate an earthmat alias from a given parent id and parent alias.
        /// </summary>
        /// <param name="parentID"></param>
        /// <param name="parentAlias"></param>
        /// <returns></returns>
        public async Task<string> CalculateEarthmatAliasAsync(int parentID, string parentAlias)
        {
            //Variables
            bool isDrillHole = false;

            //Querying with Linq
            SQLiteAsyncConnection currentConnection = da.GetConnectionFromPath(da.PreferedDatabasePath);
            List<Earthmaterial> eartmatParentStations = await currentConnection.Table<Earthmaterial>().Where(e => e.EarthMatStatID == parentID).ToListAsync();

            if (parentAlias.Contains(TableDrillHolePrefix))
            {
                eartmatParentStations = await currentConnection.Table<Earthmaterial>().Where(e => e.EarthMatDrillHoleID == parentID).ToListAsync();
                isDrillHole = true;
            }

            int startingNumber = 1;
            string finaleEarthmatString = parentAlias;

            //Detect last earthmat letter equivalent number and add 1 to it.
            if (eartmatParentStations.Count() > 0)
            {
                string lastAlias = eartmatParentStations.ToList()[eartmatParentStations.Count() - 1].EarthMatName.ToString();
                string lastCharacter = lastAlias.ToList()[lastAlias.Length - 1].ToString();

                //Find if last two are characters
                string secondLastCharacter = lastAlias.ToList()[lastAlias.Length - 2].ToString();
                if (!isDrillHole && secondLastCharacter != "H")
                {
                    int secondLastInteger = -1;
                    if (!int.TryParse(secondLastCharacter, out secondLastInteger))
                    {
                        //Should be something like AB, AC, etc.
                        lastCharacter = secondLastCharacter + lastCharacter;
                    }
                }
                int lastCharacterNumber = CalculateNumberFromAlpha(lastCharacter);

                //Find a non existing name
                bool breaker = true;
                while (breaker)
                {
                    //Build name
                    if (lastCharacterNumber == 0)
                    {
                        startingNumber = lastCharacterNumber + 2;
                    }
                    else
                    {
                        startingNumber = lastCharacterNumber + 1;
                    }
                    string rawAlphaID = CalculateAlphabeticID(true, startingNumber);
                    finaleEarthmatString = parentAlias + rawAlphaID;

                    //Find existing
                    List<Earthmaterial> existingEarth = await currentConnection.Table<Earthmaterial>().Where(e => e.EarthMatStatID == parentID && e.EarthMatName == finaleEarthmatString).ToListAsync();
                    if (parentAlias.Contains(TableDrillHolePrefix))
                    {
                        existingEarth = await currentConnection.Table<Earthmaterial>().Where(e => e.EarthMatDrillHoleID == parentID && e.EarthMatName == finaleEarthmatString).ToListAsync();
                    }

                    if (existingEarth.Count() == 0 || existingEarth == null)
                    {
                        breaker = false;
                    }

                    startingNumber++;

                }
            }
            else
            {
                finaleEarthmatString = parentAlias + CalculateAlphabeticID(true, 1); ;
            }

            return finaleEarthmatString;
        }

        #endregion

        #region SAMPLE
        /// <summary>
        /// Will calculate a sample alias from a given parent id and parent alias.
        /// </summary>
        /// <param name="parentID"></param>
        /// <param name="parentAlias"></param>
        /// <returns></returns>
        public async Task<string> CalculateSampleAliasAsync(int parentID, string parentAlias, string drillFrom = "")
        {
            //Querying with Linq
            SQLiteAsyncConnection currentConnection = da.GetConnectionFromPath(da.PreferedDatabasePath);
            List<Sample> sampleParent = await currentConnection.Table<Sample>().Where(s => s.SampleEarthmatID == parentID).ToListAsync();

            int newID = 1; //Incrementing step
            string finaleSampleString = parentAlias + "0" + newID; //Default

            //Find siblings and increment
            if (sampleParent.Count() > 0)
            {
                string lastAlias = sampleParent.ToList()[sampleParent.Count() - 1].SampleName.ToString();
                string lastNumberString = lastAlias.ToList()[lastAlias.Length - 2].ToString(); //Sample only has two digits id in the alias

                finaleSampleString = await CalculateSampleDefaultAlias(currentConnection, lastAlias, lastNumberString, parentAlias, newID, parentID);
            }

            //Special drill core case ("mineName/sampleDepth")
            if (CustomSampleNameEnabled && drillFrom != string.Empty)
            {
                //Find parent core name
                List<Earthmaterial> emParent = await currentConnection.Table<Earthmaterial>().Where(e => e.EarthMatID == parentID).ToListAsync();
                if (emParent.Count() > 0)
                {
                    if (emParent.ToList()[0].EarthMatDrillHoleID.HasValue)
                    {
                        int drillID = emParent.ToList()[0].EarthMatDrillHoleID.Value;
                        List<DrillHole> drillParent = await currentConnection.Table<DrillHole>().Where(e => e.DrillID == drillID).ToListAsync();

                        if (drillParent.Count() > 0)
                        {
                            //Find core name
                            string drillName = drillParent.ToList()[0].DrillName;
                            if (drillName != null && drillName != string.Empty)
                            {
                                //Find if
                                finaleSampleString = drillName + "/" + drillFrom;
                            }
                        }
                    }
                }
            }

            return finaleSampleString;
        }

        public async Task<string> CalculateSampleDefaultAlias(SQLiteAsyncConnection currentConnection, string lastAlias, string lastNumberString, string parentAlias, int startID, int parentID)
        {

            short parsedID = 0;
            bool processingID = Int16.TryParse(lastNumberString, out parsedID);
            string newAlias = string.Empty;
            string finaleSampleString = string.Empty;

            //Find a non existing name
            while (processingID)
            {
                //Padd current ID with 0 if needed
                if (startID < 10)
                {
                    newAlias = "0" + startID;
                }
                else
                {
                    newAlias = startID.ToString();
                }

                finaleSampleString = parentAlias + newAlias;

                //Find existing
                List<Sample> existingSamples = await currentConnection.Table<Sample>().Where(e => e.SampleEarthmatID == parentID && e.SampleName == finaleSampleString).ToListAsync();
                if (existingSamples.Count() == 0 || existingSamples == null)
                {
                    processingID = false;
                }

                startID++;
            }

            return finaleSampleString;

        }

        #endregion

        #region DOCUMENT

        /// <summary>
        /// Will calculate a document alias(name) from a given parent ID and alias (name)
        /// </summary>
        /// <param name="parentID"></param>
        /// <param name="parentAlias"></param>
        /// <returns></returns>
        public async Task<string> CalculateDocumentAliasAsync(int parentID, string parentAlias, SQLiteAsyncConnection connection = null, int startingDocNumber = 1)
        {
            //Querying with Linq
            if (connection == null)
            {
                connection = da.GetConnectionFromPath(da.PreferedDatabasePath);
            }
            List<Document> docParent = await connection.Table<Document>().Where(e => e.StationID == parentID || e.DrillHoleID == parentID).ToListAsync();

            int newID = 1; //Incrementing step
            string newAlias = string.Empty;
            string finaleDocumentString = parentAlias + TableDocumentAliasPrefix;

            if (docParent.Count() > 0)
            {
                string lastAlias = docParent.ToList()[docParent.Count() - 1].DocumentName.ToString();
                string lastNumberString = lastAlias.Substring(lastAlias.Length - 3); //doc only has three digits id in the alias
                short parsedID = 0;
                bool processingID = Int16.TryParse(lastNumberString, out parsedID);
                newID = parsedID + newID;

                while (processingID)
                {
                    //Padd current ID with 0 if needed
                    if (newID < 10)
                    {
                        newAlias = "00" + newID;
                    }
                    else if (newID < 100 && newID >= 10)
                    {
                        newAlias = "0" + newID;
                    }
                    else
                    {
                        newAlias = newID.ToString();
                    }

                    finaleDocumentString = parentAlias + TableDocumentAliasPrefix + newAlias;

                    //Find existing
                    List<Document> existingDocument = await connection.Table<Document>().Where(e => e.StationID == parentID && e.DocumentName == finaleDocumentString).ToListAsync();
                    if (existingDocument.Count() == 0 || existingDocument == null)
                    {
                        processingID = false;
                    }

                    newID++;
                 
                }


            }
            else
            {
                //Padd current ID with 0 
                newAlias = "00" + newID;

                finaleDocumentString = parentAlias + TableDocumentAliasPrefix + newAlias;
            }

            return finaleDocumentString;
        }

        #endregion

        #region STRUCTURE

        /// <summary>
        /// Will calculate a structure alias from a given parent id and parent alias.
        /// NOTE: It is related to parent alias which is earthmat
        /// It's exactly the same procedure as sample alias
        /// </summary>
        /// <param name="parentID"></param>
        /// <param name="parentAlias"></param>
        /// <returns></returns>
        public async Task<string> CalculateStructureAliasAsync(int parentID, string parentAlias)
        {
            //Querying with Linq
            SQLiteAsyncConnection currentConnection = da.GetConnectionFromPath(da.PreferedDatabasePath);
            List<Structure> strucParentEarth = await currentConnection.Table<Structure>().Where(e => e.StructureEarthmatID == parentID).ToListAsync();

            int newID = 1; //Incrementing step
            string newAlias = string.Empty;
            string finaleStructureString = parentAlias;

            //Detect last sample number and add 1 to it.
            if (strucParentEarth.Count() > 0)
            {
                string lastAlias = strucParentEarth.ToList()[strucParentEarth.Count() - 1].StructureName.ToString();
                string lastNumberString = lastAlias.ToList()[lastAlias.Length - 2].ToString(); //Sample only has two digits id in the alias
                short parsedID = 0;
                bool processingID = Int16.TryParse(lastNumberString, out parsedID);
                newID = parsedID + newID;

                //Find a non existing name
                while (processingID)
                {
                    //Padd current ID with 0 if needed
                    if (newID < 10)
                    {
                        newAlias = "0" + newID;
                    }
                    else
                    {
                        newAlias = newID.ToString();
                    }

                    finaleStructureString = parentAlias + newAlias;

                    //Find existing
                    List<Structure> existingStructures = await currentConnection.Table<Structure>().Where(e => e.StructureEarthmatID == parentID && e.StructureName == finaleStructureString).ToListAsync();
                    if (existingStructures.Count() == 0 || existingStructures == null)
                    {
                        processingID = false;
                    }

                    newID++;
                }

            }
            else
            {
                finaleStructureString = parentAlias + "0" + newID;
            }

            return finaleStructureString;
        }

        #endregion

        #region PALEO FLOW

        /// <summary>
        /// Will calculate a paleaflow alias from a given parent id and parent alias.
        /// NOTE: identical to bedrock structure table.
        /// </summary>
        /// <param name="parentID"></param>
        /// <param name="parentAlias"></param>
        /// <returns></returns>
        public async Task<string> CalculatePflowAliasAsync(int parentID, string parentAlias)
        {
            //Querying with Linq
            SQLiteAsyncConnection currentConnection = da.GetConnectionFromPath(da.PreferedDatabasePath);
            List<Paleoflow> pflowParentEarth = await currentConnection.Table<Paleoflow>().Where(e => e.PFlowParentID == parentID).ToListAsync();

            int newID = 1; //Incrementing step
            string newAlias = string.Empty;
            string finalPflowString = parentAlias;

            //Detect last sample number and add 1 to it.
            if (pflowParentEarth.Count() > 0)
            {
                string lastAlias = pflowParentEarth.ToList()[pflowParentEarth.Count() - 1].PFlowName.ToString();
                string lastNumberString = lastAlias.ToList()[lastAlias.Length - 2].ToString(); //Sample only has two digits id in the alias
                short parsedID = 0;
                bool processingID = Int16.TryParse(lastNumberString, out parsedID);
                newID = parsedID + newID;

                //Find a non existing name
                while (processingID)
                {
                    //Padd current ID with 0 if needed
                    if (newID < 10)
                    {
                        newAlias = "0" + newID;
                    }
                    else
                    {
                        newAlias = newID.ToString();
                    }

                    finalPflowString = parentAlias + newAlias;

                    //Find existing
                    List<Paleoflow> existingPflows = await currentConnection.Table<Paleoflow>().Where(e => e.PFlowParentID == parentID && e.PFlowName == finalPflowString).ToListAsync();
                    if (existingPflows.Count() == 0 || existingPflows == null)
                    {
                        processingID = false;
                    }

                    newID++;
                }

            }
            else
            {
                finalPflowString = parentAlias + "0" + newID;
            }

            return finalPflowString;
        }

        #endregion

        #region FOSSIL

        /// <summary>
        /// Will calculate a paleaflow alias from a given parent id and parent alias.
        /// NOTE: identical to bedrock sample table.
        /// </summary>
        /// <param name="parentID"></param>
        /// <param name="parentAlias"></param>
        /// <returns></returns>
        public async Task<string> CalculateFossilAliasAsync(int parentID, string parentAlias)
        {
            //Querying with Linq
            SQLiteAsyncConnection currentConnection = da.GetConnectionFromPath(da.PreferedDatabasePath);
            List<Fossil> fossilParentEarth = await currentConnection.Table<Fossil>().Where(e => e.FossilParentID == parentID).ToListAsync();

            int newID = 1; //Incrementing step
            string newAlias = string.Empty;
            string finalFossilString = parentAlias;

            //Detect last sample number and add 1 to it.
            if (fossilParentEarth.Count() > 0)
            {
                string lastAlias = fossilParentEarth.ToList()[fossilParentEarth.Count() - 1].FossilIDName.ToString();
                string lastNumberString = lastAlias.ToList()[lastAlias.Length - 2].ToString(); //Sample only has two digits id in the alias
                short parsedID = 0;
                bool processingID = Int16.TryParse(lastNumberString, out parsedID);
                newID = parsedID + newID;

                //Find a non existing name
                while (processingID)
                {
                    //Padd current ID with 0 if needed
                    if (newID < 10)
                    {
                        newAlias = "0" + newID;
                    }
                    else
                    {
                        newAlias = newID.ToString();
                    }

                    finalFossilString = parentAlias + newAlias;

                    //Find existing
                    List<Fossil> existingFossils= await currentConnection.Table<Fossil>().Where(e => e.FossilParentID == parentID && e.FossilIDName == finalFossilString).ToListAsync();
                    if (existingFossils.Count() == 0 || existingFossils == null)
                    {
                        processingID = false;
                    }

                    newID++;
                }

            }
            else
            {
                finalFossilString = parentAlias + "0" + newID;
            }

            return finalFossilString;
        }

        #endregion

        #region MINERAL
        /// <summary>
        /// Will calculate a sample alias from a given parent id and parent alias.
        /// </summary>
        /// <param name="parentID"></param>
        /// <param name="parentAlias"></param>
        /// <param name="idAddIncrement">A value to add to ID for increment start, used for bacth calculate</param>
        /// <returns></returns>
        public async Task<string> CalculateMineralAlias(int? parentID, string parentAlias, TableNames parentType)
        {
            //Querying with Linq
            SQLiteAsyncConnection currentConnection = da.GetConnectionFromPath(da.PreferedDatabasePath);
            List<Mineral> minerals = await currentConnection.Table<Mineral>().ToListAsync(); //Get all records

            if (parentType == TableNames.mineralization)
            {
                minerals = minerals.Where(m => m.MineralMAID == parentID.Value).ToList();
            }
            else if (parentType == TableNames.earthmat)
            {
                minerals = minerals.Where(m => m.MineralEMID == parentID.Value).ToList();
            }

            int newID = 1; //Incrementing step
            string newAlias = string.Empty;
            string finaleMineralString = parentAlias + TableMineralAliasPrefix;

            //Detect last sample number and add 1 to it.
            if (minerals.Count() > 0 && minerals.ElementAt(0) != null)
            {
                string lastAlias = minerals.ToList()[0].MineralIDName; //Select first element since the list has been sorted in descending order
                string lastNumberString = lastAlias.Substring(lastAlias.Length - 2); //mineral only has two digits id in the alias
                short parsedID = 0;
                bool processingID = Int16.TryParse(lastNumberString, out parsedID);
                newID = parsedID + newID;

                //Find a non existing name
                while (processingID)
                {
                    //Padd current ID with 0 if needed
                    if (newID < 10)
                    {
                        newAlias = "0" + newID;
                    }
                    else
                    {
                        newAlias = newID.ToString();
                    }

                    finaleMineralString = parentAlias + TableMineralAliasPrefix + newAlias;

                    //Find existing
                    List<Mineral> existingMinerals = minerals.Where(e => e.MineralIDName == finaleMineralString).ToList();
                    if (existingMinerals.Count() == 0 || existingMinerals == null)
                    {
                        processingID = false;
                    }


                    newID++;
                }

            }
            else
            {
                finaleMineralString = finaleMineralString + "0" + newID;
            }

            return finaleMineralString;
        }

        #endregion

        #region MINERAL ALTERATION

        /// <summary>
        /// Will calculate an mineral alteration alias from a given parent id and parent alias.
        /// Should look like YYGeolcodeStationNumberXMANumber --> 17GHV001X01, first min. alteration of first station of geo
        /// </summary>
        /// <param name="parentID"></param>
        /// <param name="parentAlias"></param>
        /// <returns></returns>
        public async Task<string> CalculateMineralAlterationAliasAsync(int parentID, string parentAlias)
        {
            //Querying with Linq
            SQLiteAsyncConnection currentConnection = da.GetConnectionFromPath(da.PreferedDatabasePath);
            List<MineralAlteration> mas = await currentConnection.Table<MineralAlteration>().ToListAsync(); //Get all records
            List<MineralAlteration> maParentStations = await currentConnection.Table<MineralAlteration>().Where(e => e.MAEarthmatID == parentID || e.MAStationID == parentID).OrderBy(e => e.MAName).ToListAsync();
            int newID = 1; //Incrementing step
            string newAlias = string.Empty;
            string finaleMAString = parentAlias + TableMineralAlterationPrefix;

            //Detect last record number and add 1 to it.
            if (maParentStations.Count() > 0)
            {
                string lastAlias = maParentStations.ToList()[maParentStations.Count() - 1].MAName.ToString();
                string lastNumberString = lastAlias.ToList()[lastAlias.Length - 2].ToString();
                short parsedID = 0;
                bool processingID = Int16.TryParse(lastNumberString, out parsedID);
                newID = parsedID + newID;

                while (processingID)
                {
                    //Padd current ID with 0 if needed
                    if (newID < 10)
                    {
                        newAlias = "0" + newID;
                    }
                    else
                    {
                        newAlias = newID.ToString();
                    }

                    finaleMAString = parentAlias + TableMineralAlterationPrefix + newAlias;

                    //Find existing
                    List<MineralAlteration> existingMA = mas.Where(e => e.MAName == finaleMAString).ToList();
                    if (existingMA.Count() == 0 || existingMA == null)
                    {
                        processingID = false;
                    }

                    newID++;

                }


            }
            else
            {
                //Padd current ID with 0 
                newAlias = "0" + newID;

                finaleMAString = parentAlias + TableMineralAlterationPrefix + newAlias;
            }

            return finaleMAString;
        }

        #endregion

        #region ENVIRONMENT
        /// <summary>
        /// Will calculate an environment alias from a given parent id and parent alias.
        /// NOTE: identical to photo/document alias or earth material without incrementing letter
        /// </summary>
        /// <param name="parentID"></param>
        /// <param name="parentAlias"></param>
        /// <returns></returns>
        public async Task<string> CalculateEnvironmentAliasAsync(int parentID, string parentAlias)
        {
            //Querying with Linq
            SQLiteAsyncConnection currentConnection = da.GetConnectionFromPath(da.PreferedDatabasePath);
            List<EnvironmentModel> envParentStations = await currentConnection.Table<EnvironmentModel>().Where(e => e.EnvStationID == parentID).ToListAsync();

            int newID = 1; //Incrementing step
            string newAlias = string.Empty;
            string finaleEnvironmentString = parentAlias + TableEnvironmentPrefix;

            //Detect last record number and add 1 to it.
            if (envParentStations.Count() > 0)
            {
                string lastAlias = envParentStations.ToList()[envParentStations.Count() - 1].EnvName.ToString();
                string lastNumberString = lastAlias.ToList()[lastAlias.Length - 2].ToString(); 
                short parsedID = 0;
                bool processingID = Int16.TryParse(lastNumberString, out parsedID);
                newID = parsedID + newID;

                while (processingID)
                {
                    //Padd current ID with 0 if needed
                    if (newID < 10)
                    {
                        newAlias = "0" + newID;
                    }
                    else
                    {
                        newAlias = newID.ToString();
                    }

                    finaleEnvironmentString = parentAlias + TableEnvironmentPrefix + newAlias;

                    //Find existing
                    List<EnvironmentModel> existingDocument = await currentConnection.Table<EnvironmentModel>().Where(e => e.EnvStationID == parentID && e.EnvName == finaleEnvironmentString).ToListAsync();
                    if (existingDocument.Count() == 0 || existingDocument == null)
                    {
                        processingID = false;
                    }

                    newID++;

                }


            }
            else
            {
                //Padd current ID with 0 
                newAlias = "0" + newID;

                finaleEnvironmentString = parentAlias + TableEnvironmentPrefix + newAlias;
            }

            return finaleEnvironmentString;
        }

        #endregion

        #region DRILL HOLE

        /// <summary>
        /// Will calculate a drill alias name from a given parent ID. Results: 16BEB0001DH (Year-Year-Geolcode-three digit number)
        /// </summary>
        /// <param name="recordDate">The field note date</param>
        /// <param name="parentID">The location id</param>
        /// <param name="followStationAlias">If drillhole alias must follow the numbering system from station form</param>
        /// <returns></returns>
        public async Task<string> CalculateDrillAliasAsync(DateTime recordDate, int parentID, bool followStationAlias = true)
        {
            //Querying with Linq
            List<DrillHole> drills = await DataAccess.DbConnection.Table<DrillHole>().ToListAsync();

            //Get current year
            string currentDate = recordDate.Year.ToString();

            //Get officer code
            List<Metadata> mets = await DataAccess.DbConnection.QueryAsync<Metadata>(string.Format("select * from {0} limit 1", TableMetadata));
            string currentGeolcode = mets[0].UserCode;

            //Get initial station start number and officer code
            int drillCount = drills.Count() + 1;

            //Follow alias system of station to start
            string finalDrillAlias = currentDate.Substring(currentDate.Length - 2) + currentGeolcode + drillCount.ToString() + TableDrillHolePrefix;
            if (followStationAlias)
            {
                List<Station> stations = await DataAccess.DbConnection.Table<Station>().OrderByDescending(s=>s.StationAlias).ToListAsync();
                if (stations != null && stations.Count > 0)
                {
                    foreach (Station stat in stations)
                    {
                        if (stat != null && stat.StationObsType != null && stat.StationObsType != KeywordStationWaypoint)
                        {
                            int stationIDNo = 0;
                            int.TryParse(stations[0].StationAliasLight, out stationIDNo);

                            //Increment only if it's higher then last station
                            if (drillCount <= stationIDNo)
                            {
                                drillCount = stationIDNo + 1;
                                break;
                            }
                        }
                    }

                }
            }

            //Increment until record doesn't exist
            bool processingID = true;
            while (processingID)
            {
                //Padd current ID with 0 if needed
                string outputStringID = string.Empty;
                if (drillCount < 10)
                {
                    outputStringID = "000" + drillCount.ToString();
                }
                else if (drillCount >= 10 && drillCount < 100)
                {
                    outputStringID = "00" + drillCount.ToString();
                }
                else if (drillCount >= 100 && drillCount < 1000)
                {
                    outputStringID = "0" + drillCount.ToString();
                }
                else
                {
                    outputStringID = drillCount.ToString();
                }

                finalDrillAlias = currentDate.Substring(currentDate.Length - 2) + currentGeolcode + outputStringID + TableDrillHolePrefix;

                //Find existing
                List<DrillHole> existingDrill = await DataAccess.DbConnection.Table<DrillHole>().Where(e => e.DrillIDName == finalDrillAlias).ToListAsync();
                if (existingDrill.Count() == 0 || existingDrill == null)
                {
                    processingID = false;
                }

                drillCount++;

            }

            return finalDrillAlias;
        }


        #endregion

        #region LINE WORK

        /// <summary>
        /// Will calculate a station alias name from a given station ID. Results: 16BEB001 (Year-Year-Geolcode-three digit number)
        /// </summary>
        /// <param name="stationTime">The datetime object related to the line</param>
        /// <param name="currentGeolcode">The officer code to append to the alias</param>
        /// <returns></returns>
        public async Task<string> CalculateLineworkAliasAsync(DateTime stationDate, string currentGeolcode = "")
        {

            //Querying with Linq
            string lineworkQuerySelect = "SELECT " + FieldLineworkIDName;
            string lineworkQueryFrom = " FROM " + TableLinework;
            string lineworkQueryOrderbyLimit = " ORDER BY " + FieldLineworkIDName + " DESC LIMIT 1;";
            string lineworkQueryFinal = lineworkQuerySelect + lineworkQueryFrom + lineworkQueryOrderbyLimit;

            List<Linework> lines = await DataAccess.DbConnection.QueryAsync<Linework>(lineworkQueryFinal);

            //Get current year
            string currentDate = stationDate.Year.ToString();

            //Get officer code
            if (currentGeolcode == string.Empty)
            {
                List<Metadata> mets = await DataAccess.DbConnection.QueryAsync<Metadata>(string.Format("select * from {0} limit 1", TableMetadata));
                currentGeolcode = mets[0].UserCode;
            }

            //Get actual last alias and extract it's number
            int lineworkCount = lines.Count();
            int lastCharacterNumber = 0;

            if (lineworkCount > 0)
            {
                List<char> lastCharacters = lines[0].LineIDName.ToList();
                List<char> lastNumbers = lastCharacters.GetRange(lastCharacters.Count() - 4, 4);
                string lastNumber = string.Empty;
                foreach (char c in lastNumbers)
                {
                    //Rebuild number
                    lastNumber = lastNumber + c;
                }

                lastCharacterNumber = Convert.ToInt32(lastNumber);
            }

            //Increment
            lineworkCount = lastCharacterNumber + 1;

            //Padd current ID with 0 if needed
            string outputStringID = string.Empty;
            if (lineworkCount < 10)
            {
                outputStringID = "000" + lineworkCount.ToString();
            }
            else if (lineworkCount >= 10 && lineworkCount < 100)
            {
                outputStringID = "00" + lineworkCount.ToString();
            }
            else if (lineworkCount >= 100 && lineworkCount < 1000)
            {
                outputStringID = "0" + lineworkCount.ToString();
            }
            else
            {
                outputStringID = lineworkCount.ToString();
            }

            string finaleLineworkString = currentDate.Substring(currentDate.Length - 2) + currentGeolcode + outputStringID; //Ex: 16BEB001

            return finaleLineworkString;

        }

        #endregion

        #endregion

        #region GENERIC
        /// <summary>
        /// Will return an id based on alphabet
        /// </summary>
        /// <param name="overload"> if True, value will be AA, AB, etc. if it goes over 26, until for infinity, if false returned value will always be between A and Z.</param>
        /// <returns>Shall return A;Z than AA, AB, AC, etc.</returns>
        public string CalculateAlphabeticID(bool overload, int startingNumber)
        {
            //Variable
            string id = ""; //New calculated value to return
            int getRemainder; //In case it's needed, will be calculate from modulo
            int getQuotient; //In case needed, will be used for overload

            List<string> alphaList = new List<string> { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", TableMineralAliasPrefix, "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

            //Retrieve a new numerical id
            int numID = startingNumber;

            //Parse new id with associated caracter
            if (numID <= alphaList.Count)
            {
                if (numID == 0)
                {
                    id = alphaList[0];
                }
                else
                {
                    id = alphaList[numID - 1];
                }
                
            }
            else
            {
                //Get modulo from id
                getRemainder = numID % alphaList.Count;

                //Get current alpha from modulo
                id = alphaList[getRemainder - 1];

                //Get if user wants an overload
                if (overload == true)
                {
                    //Get how many time alphalist.count is found within current numID
                    getQuotient = numID / alphaList.Count;

                    //Check if quotient is within count range, if over, iterate until it's under range of the count number
                    if (getQuotient >= alphaList.Count)
                    {
                        //Iteration
                        while (getQuotient > alphaList.Count)
                        {
                            //Get modulo again
                            getRemainder = getQuotient % alphaList.Count;

                            //Add new alpha id to current id
                            id = alphaList[getRemainder - 1] + id;

                            //Recalculate quotient to make while run
                            getQuotient = getQuotient / alphaList.Count;
                        }

                        //Get id result of final while iteration
                        id = alphaList[getQuotient - 1] + id;
                    }
                    else
                    {
                        id = alphaList[getQuotient - 1] + id;
                    }

                }
            }

            return id;
        }

        /// <summary>
        /// This is the reverse method of CalculateAlphabeticID
        /// </summary>
        /// <param name="alphaCharacter">The letter to retrieve the number from</param>
        /// <returns></returns>
        public int CalculateNumberFromAlpha(string alphaCharacter)
        {
            int equivalentNumber = -1;
            string iterativeString = string.Empty;

            while (alphaCharacter!= iterativeString)
            {
                equivalentNumber++;
                iterativeString = CalculateAlphabeticID(true, equivalentNumber); 
            }

            return equivalentNumber;

        }

        /// <summary>
        /// Will calculate a new GUID
        /// </summary>
        /// <returns></returns>
        public string CalculateGUID()
        {
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Will create an integer hash code from insert GUID or from a new one if inGUID stays empty
        /// Warning possible collision ahead.
        /// </summary>
        /// <returns></returns>
        public int GetHashCodeFromGUID(string inGUID = "")
        {
            if (inGUID != string.Empty)
            {
                return inGUID.GetHashCode();
            }
            else
            {
                return Guid.NewGuid().GetHashCode();
            }
            
        }

        /// <summary>
        /// Will format time with all values for a full time stamp
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public string FormatFullDate(DateTime dateTime)
        {
            return String.Format("{0:yyyy-MM-dd HH:mm}", dateTime);
        }

        public string GetDate()
        {
            return String.Format("{0:yyyy-MM-dd}", DateTime.Now); ;
        }

        public string GetTime()
        {
            return String.Format("{0:HH:mm}", DateTime.Now); ;
        }

        #endregion

    }
}
