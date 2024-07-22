using System;
using System.Collections.Generic;
using System.Linq;
using GSCFieldApp.Dictionaries;
using SQLite;

namespace GSCFieldApp.Models
{
    [Table(DatabaseLiterals.TableStation)]
    public class Station
    {

        [PrimaryKey, AutoIncrement, Column(DatabaseLiterals.FieldStationID)]
        public int StationID { get; set; }

        [Column(DatabaseLiterals.FieldStationAlias)]
        public string StationAlias { get; set; }

        [Column(DatabaseLiterals.FieldStationTraverseNumber)]
        public int StationTravNo { get; set; }

        [Column(DatabaseLiterals.FieldStationOCQuality)]
        public string StationOCQuality { get; set; }

        [Column(DatabaseLiterals.FieldStationVisitDate)]
        public string StationVisitDate { get; set; }

        [Column(DatabaseLiterals.FieldStationVisitTime)]
        public string StationVisitTime { get; set; }

        [Column(DatabaseLiterals.FieldStationObsType)]
        public string StationObsType { get; set; }

        [Column(DatabaseLiterals.FieldStationObsSource)]
        public string StationObsSource { get; set; }

        [Column(DatabaseLiterals.FieldStationOCSize)]
        public string StationOCSize { get; set; }

        [Column(DatabaseLiterals.FieldStationPhysEnv)]
        public string StationPhysEnv { get; set; }

        [Column(DatabaseLiterals.FieldStationLegend)]
        public string StationLegend { get; set; }

        [Column(DatabaseLiterals.FieldStationInterpretation)]
        public string StationInterpretation { get; set; }

        [Column(DatabaseLiterals.FieldStationNote)]
        public string StationNote { get; set; }

        [Column(DatabaseLiterals.FieldStationSLSNote)]
        public string StationSLSNotes { get; set; }

        [Column(DatabaseLiterals.FieldStationRelatedTo)]
        public string StationRelatedTo { get; set; }

        [Column(DatabaseLiterals.FieldStationAirPhotoNumber)]
        public string StationAirNo { get; set; }

        [Column(DatabaseLiterals.FieldStationObsID)]
        public int LocationID { get; set; }

        //Hierarchy
        public string ParentName = DatabaseLiterals.TableLocation;

        /// <summary>
        /// Soft mandatory field check. User can still create record even if fields are not filled.
        /// Ignore attribute will tell sql not to try to write this field inside the database.
        /// </summary>
        [Ignore]
        public bool isValid
        {
            get
            {
                if (StationObsType != string.Empty && StationObsType != null && StationObsType != Dictionaries.DatabaseLiterals.picklistNACode)
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

                stationFieldListDefault.Add(DatabaseLiterals.FieldStationID);
                foreach (System.Reflection.PropertyInfo item in this.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ColumnAttribute))).ToList())
                {
                    if (item.CustomAttributes.First().ConstructorArguments.Count() > 0)
                    {
                        stationFieldListDefault.Add(item.CustomAttributes.First().ConstructorArguments[0].ToString().Replace("\\", "").Replace("\"", ""));
                    }

                }

                stationFieldList[DatabaseLiterals.DBVersion] = stationFieldListDefault;

                //Revert shcema 1.7 changes
                List<string> statFieldList160 = new List<string>();
                statFieldList160.AddRange(stationFieldListDefault);
                int insertIndex = stationFieldListDefault.IndexOf(DatabaseLiterals.FieldStationObsID) - 1;
                statFieldList160.Insert(insertIndex, DatabaseLiterals.FieldStationReportLinkDeprecated);
                stationFieldList[DatabaseLiterals.DBVersion160] = statFieldList160;


                //Revert schema 1.6 changes. 
                List<string> stationFieldList150 = new List<string>();
                stationFieldList150.AddRange(statFieldList160);
                stationFieldList150.Remove(DatabaseLiterals.FieldStationRelatedTo);
                stationFieldList150.Remove(DatabaseLiterals.FieldStationObsSource);

                stationFieldList[DatabaseLiterals.DBVersion150] = stationFieldList150;

                //Revert schema 1.5 changes. 
                List<string> stationFieldList144 = new List<string>();
                stationFieldList144.AddRange(stationFieldList150);
                int removeIndex = stationFieldList144.IndexOf(DatabaseLiterals.FieldStationAlias);
                stationFieldList144.Remove(DatabaseLiterals.FieldStationAlias);
                stationFieldList144.Insert(removeIndex, DatabaseLiterals.FieldStationAliasDeprecated);
                stationFieldList[DatabaseLiterals.DBVersion144] = stationFieldList144;

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
                    int aliasNumber = 0;
                    int.TryParse(StationAlias.Substring(StationAlias.Length - 4), out aliasNumber);

                    if (aliasNumber > 0) 
                    {
                        return aliasNumber.ToString();
                    }
                    else
                    {
                        return DatabaseLiterals.picklistNACode;
                    }
                     
                }
                else
                {
                    return DatabaseLiterals.picklistNACode;
                }
            }
            set { }
        }

        /// <summary>
        /// Will be used to trigger a cascade delete coming from location record
        /// </summary>
        [Ignore]
        public bool? IsQuickStation { get; set; }
    }
}
