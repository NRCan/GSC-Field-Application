using static GSCFieldApp.Dictionaries.DatabaseLiterals;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSCFieldApp.Models
{
    [Table(TableDrillHoles)]
    public class DrillHole
    {
        [PrimaryKey, AutoIncrement, Column(FieldDrillID)]
        public int DrillID { get; set; }

        [Column(FieldDrillIDName)]
        public string DrillIDName { get; set; }

        [Column(FieldDrillName)]
        public string DrillName { get; set; }

        [Column(FieldDrillCompany)]
        public string DrillCompany { get; set; }

        [Column(FieldDrillType)]
        public string DrillType { get; set; }

        [Column(FieldDrillAzimuth)]
        public double? DrillAzim { get; set; }

        [Column(FieldDrillDip)]
        public double? DrillDip { get; set; }

        [Column(FieldDrillDepth)]
        public double? DrillDepth { get; set; }

        [Column(FieldDrillUnit)]
        public string DrillUnit { get; set; }

        [Column(FieldDrillDate)]
        public string DrillDate { get; set; }

        [Column(FieldDrillHoleSize)]
        public string DrillHoleSize { get; set; }

        [Column(FieldDrillCoreSize)]
        public string DrillCoreSize { get; set; }

        [Column(FieldDrillRelogType)]
        public string DrillRelogType { get; set; }

        [Column(FieldDrillRelogBy)]
        public string DrillRelogBy { get; set; }

        [Column(FieldDrillRelogIntervals)]
        public string DrillRelogIntervals { get; set; }

        [Column(FieldDrillRelogDate)]
        public string DrillRelogDate { get; set; }

        [Column(FieldDrillLog)]
        public string DrillLog { get; set; }

        [Column(FieldDrillRelatedTo)]
        public string DrillRelatedTo { get; set; }

        [Column(FieldDrillNotes)]
        public string DrillNotes { get; set; }

        [Column(FieldDrillLocationID)]
        public int DrillLocationID { get; set; }

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
                if ((DrillType != null && DrillName != string.Empty))
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
        /// A list of all possible fields from current class but also from previous schemas (for db upgrade)
        /// </summary>
        [Ignore]
        public Dictionary<double, List<string>> getFieldList
        {
            get
            {
                //Create a new list of all current columns in current class. This will act as the most recent
                //version of the class
                Dictionary<double, List<string>> drillFieldList = new Dictionary<double, List<string>>();
                List<string> drillFieldListDefault = new List<string>();

                drillFieldListDefault.Add(FieldDrillID);
                foreach (System.Reflection.PropertyInfo item in this.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ColumnAttribute))).ToList())
                {
                    if (item.CustomAttributes.First().ConstructorArguments.Count() > 0)
                    {
                        drillFieldListDefault.Add(item.CustomAttributes.Last().ConstructorArguments[0].ToString().Replace("\\", "").Replace("\"", ""));
                    }

                }

                drillFieldList[DBVersion] = drillFieldListDefault;


                return drillFieldList;
            }
            set { }
        }

        /// <summary>
        /// Property to get a smaller version of the alias, for mobile rendering mostly
        /// </summary>
        [Ignore]
        public string DrillAliasLight
        {
            get
            {
                if (DrillIDName != string.Empty)
                {
                    int aliasNumber = 0;
                    int.TryParse(DrillIDName.Substring(DrillIDName.Length - 6,4), out aliasNumber);

                    if (aliasNumber > 0)
                    {
                        return aliasNumber.ToString() + TableDrillHolePrefix;
                    }
                    else
                    {
                        return picklistNACode;
                    }

                }
                else
                {
                    return picklistNACode;
                }
            }
            set { }
        }
    }
}
