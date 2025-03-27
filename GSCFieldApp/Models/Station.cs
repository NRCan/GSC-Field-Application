using System;
using System.Collections.Generic;
using System.Linq;
using static GSCFieldApp.Dictionaries.DatabaseLiterals;
using SQLite;

namespace GSCFieldApp.Models
{
    [Table(TableStation)]
    public class Station
    {

        [Column(FieldStationID), PrimaryKey, AutoIncrement]
        public int StationID { get; set; }

        [Column(FieldStationAlias)]
        public string StationAlias { get; set; }

        [Column(FieldStationTraverseNumber)]
        public int StationTravNo { get; set; }

        [Column(FieldStationOCQuality)]
        public string StationOCQuality { get; set; }

        [Column(FieldStationVisitDate)]
        public string StationVisitDate { get; set; }

        [Column(FieldStationVisitTime)]
        public string StationVisitTime { get; set; }

        [Column(FieldStationObsType)]
        public string StationObsType { get; set; }

        [Column(FieldStationObsSource)]
        public string StationObsSource { get; set; }

        [Column(FieldStationOCSize)]
        public string StationOCSize { get; set; }

        [Column(FieldStationPhysEnv)]
        public string StationPhysEnv { get; set; }

        [Column(FieldStationLegend)]
        public string StationLegend { get; set; }

        [Column(FieldStationInterpretation)]
        public string StationInterpretation { get; set; }

        [Column(FieldStationNote)]
        public string StationNote { get; set; }

        [Column(FieldStationSLSNote)]
        public string StationSLSNotes { get; set; }

        [Column(FieldStationRelatedTo)]
        public string StationRelatedTo { get; set; }

        [Column(FieldStationAirPhotoNumber)]
        public string StationAirNo { get; set; }

        [Column(FieldStationObsID)]
        public int LocationID { get; set; }

        //Hierarchy
        public string ParentName = TableLocation;

        /// <summary>
        /// Soft mandatory field check. User can still create record even if fields are not filled.
        /// Ignore attribute will tell sql not to try to write this field inside the database.
        /// </summary>
        [Ignore]
        public bool isValid
        {
            get
            {
                if (StationObsType != string.Empty && StationObsType != null && StationObsType != picklistNACode)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set { }
        }

        /// <summary>
        /// A list of all possible fields
        /// </summary>
        [Ignore]
        public Dictionary<double, List<string>> getFieldList
        {
            get
            {

                //Create a new list of all current columns in current class. This will act as the most recent
                //version of the class
                Dictionary<double, List<string>> stationFieldList = new Dictionary<double, List<string>>();
                List<string> stationFieldListDefault = new List<string>();

                foreach (System.Reflection.PropertyInfo item in this.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ColumnAttribute))).ToList())
                {
                    if (item.CustomAttributes.First().ConstructorArguments.Count() > 0)
                    {
                        stationFieldListDefault.Add(item.CustomAttributes.First().ConstructorArguments[0].ToString().Replace("\\", "").Replace("\"", ""));
                    }

                }

                stationFieldList[DBVersion] = stationFieldListDefault;

                //Revert shcema 1.7 changes
                List<string> statFieldList160 = new List<string>();
                statFieldList160.AddRange(stationFieldListDefault);
                int insertIndex = stationFieldListDefault.IndexOf(FieldStationObsID) - 1;
                statFieldList160.Insert(insertIndex, FieldStationReportLinkDeprecated);
                stationFieldList[DBVersion160] = statFieldList160;


                //Revert schema 1.6 changes. 
                List<string> stationFieldList150 = new List<string>();
                stationFieldList150.AddRange(statFieldList160);
                stationFieldList150.Remove(FieldStationRelatedTo);
                stationFieldList150.Remove(FieldStationObsSource);

                stationFieldList[DBVersion150] = stationFieldList150;

                //Revert schema 1.5 changes. 
                List<string> stationFieldList144 = new List<string>();
                stationFieldList144.AddRange(stationFieldList150);
                int removeIndex = stationFieldList144.IndexOf(FieldStationAlias);
                stationFieldList144.Remove(FieldStationAlias);
                stationFieldList144.Insert(removeIndex, FieldStationAliasDeprecated);
                stationFieldList[DBVersion144] = stationFieldList144;

                return stationFieldList;
            }
            set { }
        }

        /// <summary>
        /// Property to get a smaller version of the alias, for mobile rendering mostly
        /// </summary>
        [Ignore]
        public string StationAliasLight
        {
            get 
            {
                if (StationAlias != string.Empty)
                {
                    if (IsWaypoint)
                    {
                        int aliasNumber = 0;
                        int.TryParse(StationAlias.Substring(StationAlias.Length - 3), out aliasNumber);

                        if (aliasNumber > 0)
                        {
                            return "W" + aliasNumber.ToString();
                        }
                        else
                        {
                            return StationAlias;
                        }
                    }
                    else
                    {
                        int aliasNumber = 0;
                        int.TryParse(StationAlias.Substring(StationAlias.Length - 4), out aliasNumber);

                        if (aliasNumber > 0)
                        {
                            return aliasNumber.ToString();
                        }
                        else
                        {
                            return StationAlias;
                        }
                    }

                     
                }
                else
                {
                    return picklistNACode;
                }
            }
            set { }
        }

        /// <summary>
        /// Will be used to trigger a cascade delete coming from location record
        /// </summary>
        [Ignore]
        public bool IsMapPageQuick { get; set; } = false;

        /// <summary>
        /// Will be used to render differently the station form, leaving only needed controls for a waypoint type
        /// </summary>
        [Ignore]
        public bool IsWaypoint 
        {
            get
            {
                if ((StationAlias != null && StationAlias != string.Empty && StationAlias.ToLower().Contains(KeywordStationWaypoint)) ||
                    (StationObsType !=null && StationObsType != string.Empty && StationObsType.ToLower().Contains(KeywordStationWaypoint)))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set { }

        } 
    }
}
