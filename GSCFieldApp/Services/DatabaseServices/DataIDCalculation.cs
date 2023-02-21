using System;
using System.Collections.Generic;
using System.Linq;
using GSCFieldApp.Models;
using GSCFieldApp.Dictionaries;
using Windows.ApplicationModel.Store.Preview.InstallControl;

namespace GSCFieldApp.Services.DatabaseServices
{
    public class DataIDCalculation
    {
        readonly DataLocalSettings localSetting = new DataLocalSettings();
        readonly FieldLocation locationModel = new FieldLocation();
        readonly Station stationModel = new Station();
        readonly EarthMaterial earthmatModel = new EarthMaterial();
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
        readonly DataAccess dAccess = new DataAccess();

        public DataIDCalculation()
        {
            
        }

        #region MODEL BASE

        #region LOCATION
        /// <summary>
        /// Will calculate an generic ID for location table.
        /// </summary>
        /// <returns></returns>
        public int CalculateLocationID()
        {
            return GetHashCodeFromGUID();
        }

        /// <summary>
        /// Will calculate a new alias for location, if wanted.
        /// Output can be either empty or like this 17GHV001XY
        /// </summary>
        /// <param name="inStationAlias"></param>
        /// <returns></returns>
        public string CalculateLocationAlias(string inStationAlias = "")
        {
            string locAlias = inStationAlias;

            if (inStationAlias!=string.Empty)
            {
                locAlias = locAlias + "XY";
            }
            else
            {
                locAlias = CalculateStationAlias(DateTime.Now) + "XY";
            }

            return locAlias;
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
        /// Will calculate a generic ID for station table from row count.
        /// </summary>
        /// <returns></returns>
        public int CalculateStationID()
        {
            return GetHashCodeFromGUID();
        }

        /// <summary>
        /// Will calculate a station alias name from a given station ID. Results: 16BEB001 (Year-Year-Geolcode-three digit number)
        /// </summary>
        /// <param name="currentID">The station ID to calculate the alias from</param>
        /// <param name="stationTime">The datetime object related to the station to get the year from</param>
        /// <returns></returns>
        public string CalculateStationAlias(DateTime stationDate)
        {

            //Get current geolcode
            string currentGeolcode = localSetting.GetSettingValue(DatabaseLiterals.FieldUserInfoUCode).ToString();
            if (localSetting.GetSettingValue(DatabaseLiterals.FieldUserInfoID) != null)
            {
                int currentMetaID = int.Parse(localSetting.GetSettingValue(DatabaseLiterals.FieldUserInfoID).ToString());

                //Querying with Linq
                string stationQuerySelect = "SELECT *";
                string stationQueryFrom = " FROM " + DatabaseLiterals.TableStation;
                string stationQueryWhere = " WHERE " + DatabaseLiterals.TableStation + "." + DatabaseLiterals.FieldStationObsType + " NOT LIKE '%" + DatabaseLiterals.KeywordStationWaypoint + "'";
                string stationQueryWhere2 = " OR " + DatabaseLiterals.TableStation + "." + DatabaseLiterals.FieldStationObsType + " IS NULL";
                string stationQueryFinal = stationQuerySelect + stationQueryFrom + stationQueryWhere + stationQueryWhere2;

                List<object> locationTableRaw = dAccess.ReadTable(locationModel.GetType(), string.Empty);
                List<object> stationTableRaw = dAccess.ReadTable(stationModel.GetType(), stationQueryFinal);

                IEnumerable<Station> stationTable = stationTableRaw.Cast<Station>(); //Cast to proper list type
                IEnumerable<FieldLocation> locationTable = locationTableRaw.Cast<FieldLocation>(); //Cast to proper list type
                IEnumerable<string> stations = from s in stationTable join l in locationTable on s.LocationID equals l.LocationID where l.MetaID == currentMetaID orderby s.StationAlias descending select s.StationAlias;

                //Get current year
                string currentDate = currentDate = stationDate.Year.ToString();

                //Get initial station start number if needed
                int stationCount = stations.Count();
                if (stationCount == 0)
                {
                    List<object> metadataTableRaw = dAccess.ReadTable(metadataModel.GetType(), null);
                    IEnumerable<Metadata> metadataTable = metadataTableRaw.Cast<Metadata>(); //Cast to proper list type
                    IEnumerable<Metadata> metadatas = from m in metadataTable where m.MetaID == currentMetaID select m;
                    List<Metadata> metadatasList = metadatas.ToList();
                    stationCount = Convert.ToInt16(metadatasList[0].StationStartNumber);
                }
                else
                {
                    //Get actual last alias and extract it's number
                    string lastAlias = stations.ToList()[0].ToString();
                    List<char> lastCharacters = lastAlias.ToList();
                    List<char> lastNumbers = lastCharacters.GetRange(lastCharacters.Count() - 3, 3);
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

                //Padd current ID with 0 if needed

                string outputStringID = string.Empty;
                bool breaker = true;
                string finaleStationString = currentDate.Substring(currentDate.Length - 2) + currentGeolcode + outputStringID; //Ex: 16BEB001
                while (breaker)
                {
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

                    finaleStationString = currentDate.Substring(currentDate.Length - 2) + currentGeolcode + outputStringID;

                    IEnumerable<Station> existinStations = from s in stationTable join l in locationTable on s.LocationID equals l.LocationID where l.MetaID == currentMetaID && s.StationAlias == finaleStationString select s;

                    if (existinStations.Count() == 0 || existinStations == null)
                    {
                        breaker = false;
                    }

                    stationCount++;
                }

                return finaleStationString;
            }
            else
            {
                return String.Empty;
            }
            



            

        }

        /// <summary>
        /// Will calculate a station waypoint alias name from a given station ID. Results: Waypoint1, Waypoint2, ... 
        /// </summary>
        /// <param name="currentID">The station ID to calculate the alias from</param>
        /// <param name="stationTime">The datetime object related to the station to get the year from</param>
        /// <returns></returns>
        public string CalculateStationWaypointAlias(string waypointVocabCode, string waypointVocabName)
        {

            //Get current geolcode
            string currentGeolcode = localSetting.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoUCode).ToString();
            string currentMetaID = localSetting.GetSettingValue(Dictionaries.DatabaseLiterals.FieldUserInfoID).ToString();

            //Build query to get a waypoint term count
            string querySelect= "SELECT s." + Dictionaries.DatabaseLiterals.FieldStationAlias + " ";
            string queryFrom = "FROM " + Dictionaries.DatabaseLiterals.TableStation + " as s ";
            string queryWhere = "WHERE " + "s." + Dictionaries.DatabaseLiterals.FieldStationObsType + " LIKE '%" + waypointVocabCode + "%' ";
            string queryOrderBy = "ORDER BY s." + Dictionaries.DatabaseLiterals.FieldStationAlias + " DESC LIMIT 1";
            string finalQuery = querySelect + queryFrom + queryWhere + queryOrderBy;

            //Get query result
            List<object> waypointRaw = dAccess.ReadTable(stationModel.GetType(), finalQuery);
            IEnumerable<Station> waypoints = waypointRaw.Cast<Station>(); //Cast to proper list type

            //Try parsing value
            int waypointLastNumber = 1;
            if (waypointRaw.Count() > 0)
            {
                if (waypoints.First().StationAlias.ToString().Contains(waypointVocabCode + currentGeolcode))
                {
                    waypointLastNumber = Convert.ToInt32(waypoints.First().StationAlias.ToString().Replace(waypointVocabCode + currentGeolcode, "")) + 1;   //prefix is wp
                }
                else
                {
                    waypointLastNumber = Convert.ToInt32(waypoints.First().StationAlias.Substring(waypoints.First().StationAlias.Length - 3, 3)) + 1;  //probably do not need code above
                }
            }

            //Padd current ID with 0 if needed
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

            string finaleWaypointString = waypointVocabName + currentGeolcode + outputStringID;  //prefix is waypoint
            //string finaleWaypointString = "wp" + currentGeolcode + outputStringID;   //prefix is wp

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
        public string CalculateEarthmatnAlias(int parentID, string parentAlias)
        {

            //Querying with Linq
            List<object> earthmatTableRaw = dAccess.ReadTable(earthmatModel.GetType(), null);
            IEnumerable<EarthMaterial> earthmatTable = earthmatTableRaw.Cast<EarthMaterial>(); //Cast to proper list type
            IEnumerable<string> eartmatParentStations = from e in earthmatTable where e.EarthMatStatID == parentID select e.EarthMatName;

            int startingNumber = 1;
            string finaleEarthmatString = parentAlias;

            //Detect last earthmat letter equivalent number and add 1 to it.
            if (eartmatParentStations.Count() > 0)
            {
                string lastAlias = eartmatParentStations.ToList()[eartmatParentStations.Count() - 1].ToString(); 
                string lastCharacter = lastAlias.ToList()[lastAlias.Length - 1].ToString();

                //Find if last two are characters
                string secondLastCharacter = lastAlias.ToList()[lastAlias.Length - 2].ToString();
                int secondLastInteger = -1;
                if (!int.TryParse(secondLastCharacter, out secondLastInteger))
                {
                    //Should be something like AB, AC, etc.
                    lastCharacter = secondLastCharacter + lastCharacter;
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
                    IEnumerable<EarthMaterial> existingEarth = from s in earthmatTable where s.EarthMatStatID == parentID && s.EarthMatName == finaleEarthmatString select s;

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

        /// <summary>
        /// Will calculate a generic ID from earth material table based on the highest current stored id.
        /// </summary>
        /// <returns></returns>
        public int CalculateEarthmatID()
        {
            return GetHashCodeFromGUID();
        }

        #endregion

        #region SAMPLE
        /// <summary>
        /// Will calculate a sample alias from a given parent id and parent alias.
        /// </summary>
        /// <param name="parentID"></param>
        /// <param name="parentAlias"></param>
        /// <returns></returns>
        public string CalculateSampleAlias(int parentID, string parentAlias)
        {
            //Querying with Linq
            List<object> sampleTableRaw = dAccess.ReadTable(sampleModel.GetType(), null);
            IEnumerable<Sample> sampleTable = sampleTableRaw.Cast<Sample>(); //Cast to proper list type
            IEnumerable<string> sampleParentEarth = from e in sampleTable where e.SampleEarthmatID == parentID orderby e.SampleName descending select e.SampleName;

            int newID = 1; //Incrementing step
            string newAlias = string.Empty;
            string finaleSampleString = parentAlias;

            //Detect last sample number and add 1 to it.
            if (sampleParentEarth.Count() > 0)
            {
                string lastAlias = sampleParentEarth.ToList()[0].ToString(); //Select first element since the list has been sorted in descending order
                string lastNumberString = lastAlias.Substring(lastAlias.Length - 2); //Sample only has two digits id in the alias

                newID = Convert.ToInt16(lastNumberString) + newID;

                //Find a non existing name
                bool breaker = true;
                while (breaker)
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

                    finaleSampleString = parentAlias + newAlias;

                    //Find existing
                    IEnumerable<Sample> existingSamples= from s in sampleTable where s.SampleEarthmatID == parentID && s.SampleName == finaleSampleString select s;
                    if (existingSamples.Count() == 0 || existingSamples == null)
                    {
                        breaker = false;
                    }

                    newID++;
                }

            }
            else
            {
                finaleSampleString = parentAlias + "0" + newID;
            }

            return finaleSampleString;
        }

        /// <summary>
        /// Will calculate a generic ID from sample table based on the highest current stored id.
        /// </summary>
        /// <returns></returns>
        public int CalculateSampleID()
        {
            return GetHashCodeFromGUID();
        }

        #endregion

        #region DOCUMENT
        /// <summary>
        /// Will calculate an generic ID for document table.
        /// </summary>
        /// <returns></returns>
        public int CalculateDocumentID()
        {
            return GetHashCodeFromGUID();
        }

        /// <summary>
        /// Will calculate a document alias(name) from a given parent ID and alias (name)
        /// </summary>
        /// <param name="parentID"></param>
        /// <param name="parentAlias"></param>
        /// <returns></returns>
        public string CalculateDocumentAlias(int parentID, string parentAlias, int startingDocNumber = 1)
        {
            //Querying with Linq
            List<object> doctableRaw = dAccess.ReadTable(docModel.GetType(), null);
            IEnumerable<Document> docTable = doctableRaw.Cast<Document>(); //Cast to proper list type
            IEnumerable<string> docParent = from d in docTable where d.RelatedID == parentID orderby d.DocumentName descending select d.DocumentName;

            string newAlias = string.Empty;
            string finaleDocumentString = parentAlias + DatabaseLiterals.TableDocumentAliasPrefix;

            //Detect last sample number and add 1 to it.
            if (docParent.Count() > 0)
            {
                int lastNumber = docParent.ToList().Count(); //Select first element since the list has been sorted in descending order
                //string lastNumberString = lastAlias.Substring(lastAlias.Length - 3); //Document only has three digits id in the alias
                int newNumber = lastNumber + startingDocNumber;
                bool breaker = true;
                while (breaker)
                {
                    //Padd current ID with 0 if needed
                    if (newNumber < 10)
                    {
                        newAlias = "00" + newNumber;
                    }
                    else if (newNumber < 100 && newNumber >= 10)
                    {
                        newAlias = "0" + newNumber;
                    }
                    else
                    {
                        newAlias = newNumber.ToString();
                    }

                    finaleDocumentString = parentAlias + DatabaseLiterals.TableDocumentAliasPrefix + newAlias;

                    //Find existing
                    IEnumerable<Document> existingDocument = from s in docTable where s.RelatedID == parentID && s.DocumentName == finaleDocumentString select s;
                    if (existingDocument.Count() == 0 || existingDocument == null)
                    {
                        breaker = false;
                    }

                    newNumber++;
                }


            }
            else
            {
                //Padd current ID with 0 if needed
                if (startingDocNumber < 10)
                {
                    newAlias = "00" + startingDocNumber;
                }
                else if (startingDocNumber < 100 && startingDocNumber >= 10)
                {
                    newAlias = "0" + startingDocNumber;
                }
                else
                {
                    newAlias = startingDocNumber.ToString();
                }

                finaleDocumentString = parentAlias + DatabaseLiterals.TableDocumentAliasPrefix + newAlias;
            }

            return finaleDocumentString;
        }

        #endregion

        #region STRUCTURE
        /// <summary>
        /// Will calculate a generic ID for structure table
        /// </summary>
        /// <returns></returns>
        public int CalculateStructureID()
        {
            return GetHashCodeFromGUID();
        }

        /// <summary>
        /// Will calculate a structure alias from a given parent id and parent alias.
        /// NOTE: It is related to parent alias which is earthmat
        /// </summary>
        /// <param name="parentID"></param>
        /// <param name="parentAlias"></param>
        /// <returns></returns>
        public string CalculateStructureAlias(int parentID, string parentAlias)
        {
            //Querying with Linq
            List<object> structureTableRaw = dAccess.ReadTable(structModel.GetType(), null);
            IEnumerable<Structure> strucTable = structureTableRaw.Cast<Structure>(); //Cast to proper list type
            IEnumerable<string> strucParentEarth = from e in strucTable where e.StructureParentID == parentID orderby e.StructureName descending select e.StructureName;

            int newID = 1; //Incrementing step
            string newAlias = string.Empty;
            string finaleStructureString = parentAlias;

            //Detect last sample number and add 1 to it.
            if (strucParentEarth.Count() > 0)
            {
                string lastAlias = strucParentEarth.ToList()[0].ToString(); //Select first element since the list has been sorted in descending order
                string lastNumberString = lastAlias.Substring(lastAlias.Length - 2); //Sample only has two digits id in the alias
                newID = Convert.ToInt16(lastNumberString) + newID;
                bool breaker = true;
                while (breaker)
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
                    IEnumerable<Structure> existingStructure = from s in strucTable where s.StructureParentID == parentID && s.StructureName == finaleStructureString select s;
                    if (existingStructure.Count() == 0 || existingStructure == null)
                    {
                        breaker = false;
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
        /// Will calculate an generic ID for paleoflow table.
        /// </summary>
        /// <returns></returns>
        public int CalculatePFlowID()
        {
            return GetHashCodeFromGUID();
        }

        /// <summary>
        /// Will calculate a paleaflow alias from a given parent id and parent alias.
        /// NOTE: identical to bedrock structure table.
        /// </summary>
        /// <param name="parentID"></param>
        /// <param name="parentAlias"></param>
        /// <returns></returns>
        public string CalculatePflowAlias(int parentID, string parentAlias)
        {
            //Querying with Linq
            List<object> pflowTableRaw = dAccess.ReadTable(pflowModel.GetType(), null);
            IEnumerable<Paleoflow> pflowTable = pflowTableRaw.Cast<Paleoflow>(); //Cast to proper list type
            IEnumerable<string> pflowParentEarth = from e in pflowTable where e.PFlowParentID == parentID orderby e.PFlowName descending select e.PFlowName;

            int newID = 1; //Incrementing step
            string newAlias = string.Empty;
            string finalePflowString = parentAlias;

            //Detect last sample number and add 1 to it.
            if (pflowParentEarth.Count() > 0)
            {
                string lastAlias = pflowParentEarth.ToList()[0].ToString(); //Select first element since the list has been sorted in descending order
                string lastNumberString = lastAlias.Substring(lastAlias.Length - 2); //Sample only has two digits id in the alias
                newID = Convert.ToInt16(lastNumberString) + newID;
                bool breaker = true;
                while (breaker)
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

                    finalePflowString = parentAlias + newAlias;

                    //Find existing
                    IEnumerable<Paleoflow> existingPflow = from s in pflowTable where s.PFlowParentID == parentID && s.PFlowName == finalePflowString select s;
                    if (existingPflow.Count() == 0 || existingPflow == null)
                    {
                        breaker = false;
                    }

                    newID++;
                }

            }
            else
            {
                finalePflowString = parentAlias + "0" + newID;
            }

            return finalePflowString;
        }

        #endregion

        #region FOSSIL
        /// <summary>
        /// Will calculate an generic ID for fossil table.
        /// </summary>
        /// <returns></returns>
        public int CalculateFossilID()
        {
            return GetHashCodeFromGUID();
        }

        /// <summary>
        /// Will calculate a paleaflow alias from a given parent id and parent alias.
        /// NOTE: identical to bedrock structure table.
        /// </summary>
        /// <param name="parentID"></param>
        /// <param name="parentAlias"></param>
        /// <returns></returns>
        public string CalculateFossilAlias(int parentID, string parentAlias)
        {
            //Querying with Linq
            List<object> fossilTableRaw = dAccess.ReadTable(fossilModel.GetType(), null);
            IEnumerable<Fossil>fossilTable = fossilTableRaw.Cast<Fossil>(); //Cast to proper list type
            IEnumerable<string> fossilParentEarth = from e in fossilTable where e.FossilParentID == parentID orderby e.FossilIDName descending select e.FossilIDName;

            int newID = 1; //Incrementing step
            string newAlias = string.Empty;
            string finaleFossilString = parentAlias;

            //Detect last sample number and add 1 to it.
            if (fossilParentEarth.Count() > 0)
            {
                string lastAlias = fossilParentEarth.ToList()[0].ToString(); //Select first element since the list has been sorted in descending order
                string lastNumberString = lastAlias.Substring(lastAlias.Length - 2); //Sample only has two digits id in the alias
                newID = Convert.ToInt16(lastNumberString) + newID;
                bool breaker = true;
                while (breaker)
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
                    finaleFossilString = parentAlias + newAlias;

                    //Find existing
                    IEnumerable<Fossil> existingFossil = from s in fossilTable where s.FossilParentID == parentID && s.FossilIDName == finaleFossilString select s;
                    if (existingFossil.Count() == 0 || existingFossil == null)
                    {
                        breaker = false;
                    }

                    newID++;
                }
            }
            else
            {
                finaleFossilString = parentAlias + "0" + newID;
            }

            return finaleFossilString;
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
        public string CalculateMineralAlias(int? parentID, string parentAlias, int idAddIncrement = 0)
        {
            //Querying with Linq
            List<object> MineralTableRaw = dAccess.ReadTable(mineralModel.GetType(), null);
            IEnumerable<Mineral> mineralTable = MineralTableRaw.Cast<Mineral>(); //Cast to proper list type
            IEnumerable<string> mineralParentEarth = from e in mineralTable where e.MineralEMID == parentID || e.MineralMAID == parentID orderby e.MineralIDName descending select e.MineralIDName;

            int newID = 1 + idAddIncrement; //Incrementing step
            string newAlias = string.Empty;
            string finaleMineralString = parentAlias + DatabaseLiterals.TableMineralAliasPrefix;

            //Detect last sample number and add 1 to it.
            if (mineralParentEarth.Count() > 0 && mineralParentEarth.ElementAt(0) !=null)
            {
                string lastAlias = mineralParentEarth.ToList()[0].ToString(); //Select first element since the list has been sorted in descending order
                string lastNumberString = lastAlias.Substring(lastAlias.Length - 2); //Sample only has two digits id in the alias

                newID = Convert.ToInt16(lastNumberString) + newID;

                //Find a non existing name
                bool breaker = true;
                while (breaker)
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

                    finaleMineralString = parentAlias + DatabaseLiterals.TableMineralAliasPrefix + newAlias;

                    //Find existing
                    IEnumerable<string> existingMinerals = from m in mineralTable where m.MineralIDName == finaleMineralString select m.MineralIDName;
                    if (existingMinerals.Count() == 0 || existingMinerals == null)
                    {
                        breaker = false;
                    }

                    newID++;
                }

            }
            else
            {
                finaleMineralString = parentAlias + DatabaseLiterals.TableMineralAliasPrefix + "0" + newID;
            }

            return finaleMineralString;
        }

        /// <summary>
        /// Will calculate a generic ID from sample table based on the highest current stored id.
        /// </summary>
        /// <returns></returns>
        public int CalculateMineralID()
        {
            return GetHashCodeFromGUID();
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
        public string CalculateMineralAlterationAlias(int parentID, string parentAlias)
        {
            //Variables
            string prefix = DatabaseLiterals.TableMineralAlterationPrefix;

            //Querying with Linq
            List<object> maTableRaw = dAccess.ReadTable(minAlterationModel.GetType(), null);
            IEnumerable<MineralAlteration> maTable = maTableRaw.Cast<MineralAlteration>(); //Cast to proper list type
            IEnumerable<string> maParentStations = from ma in maTable where ma.MAParentID == parentID orderby ma.MAName descending select ma.MAName;

            int startingNumber = 1;
            string startingNumberStr = string.Empty;
            string finaleMAString = parentAlias;

            //Calculate
            if (maParentStations.Count() > 0)
            {
                string lastAlias = maParentStations.ToList()[0].ToString();

                //Find a non existing name
                bool breaker = true;
                while (breaker)
                {
                    //Padd current ID with 0 if needed
                    if (startingNumber < 10)
                    {
                        startingNumberStr = "0" + startingNumber.ToString();
                    }
                    else
                    {
                        startingNumberStr = startingNumber.ToString();
                    }

                    finaleMAString = parentAlias + prefix + startingNumberStr;

                    //Find existing
                    IEnumerable<MineralAlteration> existingMA = from ma2 in maTable where ma2.MAParentID == parentID && ma2.MAName == finaleMAString select ma2;

                    if (existingMA.Count() == 0 || existingMA == null)
                    {
                        breaker = false;
                    }

                    startingNumber++;

                }
            }
            else
            {
                finaleMAString = parentAlias + prefix + "0" + startingNumber.ToString();
            }

            return finaleMAString;
        }

        /// <summary>
        /// Will calculate a generic ID from sample table based on the highest current stored id.
        /// </summary>
        /// <returns></returns>
        public int CalculateMineralAlterationID()
        {
            return GetHashCodeFromGUID();
        }
        #endregion

        #region ENVIRONMENT
        /// <summary>
        /// Will calculate an mineral alteration alias from a given parent id and parent alias.
        /// Should look like YYGeolcodeStationNumberXMANumber --> 17GHV001X01, first environment of first station of geo
        /// </summary>
        /// <param name="parentID"></param>
        /// <param name="parentAlias"></param>
        /// <returns></returns>
        public string CalculateEnvironmentAlias(int parentID, string parentAlias)
        {
            //Variables
            string prefix = DatabaseLiterals.TableEnvironmentPrefix;

            //Querying with Linq
            List<object> envTableRaw = dAccess.ReadTable(envModel.GetType(), null);
            IEnumerable<EnvironmentModel> envTable = envTableRaw.Cast<EnvironmentModel>(); //Cast to proper list type
            IEnumerable<string> envParentStations = from env in envTable where env.EnvStationID == parentID orderby env.EnvName descending select env.EnvName;

            int startingNumber = 1;
            string startingNumberStr = string.Empty;
            string finaleENVString = parentAlias;

            //Calculate
            if (envParentStations.Count() > 0)
            {
                string lastAlias = envParentStations.ToList()[0].ToString();

                //Find a non existing name
                bool breaker = true;
                while (breaker)
                {
                    //Padd current ID with 0 if needed
                    if (startingNumber < 10)
                    {
                        startingNumberStr = "0" + startingNumber.ToString();
                    }
                    else
                    {
                        startingNumberStr = startingNumber.ToString();
                    }

                    finaleENVString = parentAlias + prefix + startingNumberStr;

                    //Find existing
                    IEnumerable<EnvironmentModel> existingENV = from env2 in envTable where env2.EnvStationID == parentID && env2.EnvName == finaleENVString select env2;

                    if (existingENV.Count() == 0 || existingENV == null)
                    {
                        breaker = false;
                    }

                    startingNumber++;

                }
            }
            else
            {
                finaleENVString = parentAlias + prefix + "0" + startingNumber.ToString();
            }

            return finaleENVString;
        }

        /// <summary>
        /// Will calculate a generic ID from sample table based on the highest current stored id.
        /// </summary>
        /// <returns></returns>
        public int CalculateEnvironmentID()
        {
            return GetHashCodeFromGUID();
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

            List<string> alphaList = new List<string> { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", DatabaseLiterals.TableMineralAliasPrefix, "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

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
        #endregion
    }
}
