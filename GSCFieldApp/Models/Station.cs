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
        public List<string> getFieldList
        {
            get
            {
                List<string> stationFieldList = new List<string>();
                stationFieldList.Add(DatabaseLiterals.FieldStationID);
                foreach (System.Reflection.PropertyInfo item in this.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ColumnAttribute))).ToList())
                {
                    if (item.CustomAttributes.First().ConstructorArguments.Count() > 0)
                    {
                        stationFieldList.Add(item.CustomAttributes.First().ConstructorArguments[0].ToString().Replace("\\", "").Replace("\"", ""));
                    }

                }

                return stationFieldList;
            }
            set { }
        }

    }
}
