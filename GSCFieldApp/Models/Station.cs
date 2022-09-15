using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite.Net.Attributes;
using GSCFieldApp.Dictionaries;

namespace GSCFieldApp.Models
{
    [Table(DatabaseLiterals.TableStation)]
    public class Station
    {

        [PrimaryKey, Column(DatabaseLiterals.FieldStationID)]
        public string StationID { get; set; }

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

        [Column(DatabaseLiterals.FieldStationReportLink)]
        public string StateionReportLink { get; set; }

        [Column(DatabaseLiterals.FieldStationObsID)]
        public string LocationID { get; set; }

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

                //Revert schema 1.5 changes. 
                List<string> stationFieldList144 = new List<string>();
                stationFieldList144.AddRange(stationFieldListDefault);
                int removeIndex = stationFieldList144.IndexOf(DatabaseLiterals.FieldStationAlias);
                stationFieldList144.Remove(DatabaseLiterals.FieldStationAlias);
                stationFieldList144.Insert(removeIndex,DatabaseLiterals.FieldStationAliasDeprecated);
                stationFieldList[DatabaseLiterals.DBVersion144] = stationFieldList144;

                return stationFieldList;
            }
            set { }
        }

    }
}
