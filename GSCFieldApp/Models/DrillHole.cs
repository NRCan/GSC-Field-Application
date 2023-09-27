using GSCFieldApp.Dictionaries;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSCFieldApp.Models
{
    [Table(DatabaseLiterals.TableDrillHoles)]
    public class DrillHole
    {
        [PrimaryKey, AutoIncrement, Column(DatabaseLiterals.FieldDrillID)]
        public int DrillID { get; set; }

        [Column(DatabaseLiterals.FieldDrillName)]
        public string DrillName { get; set; }

        [Column(DatabaseLiterals.FieldDrillCompany)]
        public string DrillCompany { get; set; }

        [Column(DatabaseLiterals.FieldDrillType)]
        public string DrillType { get; set; }

        [Column(DatabaseLiterals.FieldDrillAzimuth)]
        public string DrillAzim { get; set; }

        [Column(DatabaseLiterals.FieldDrillDip)]
        public string DrillDip { get; set; }

        [Column(DatabaseLiterals.FieldDrillDepth)]
        public string DrillDepth { get; set; }

        [Column(DatabaseLiterals.FieldDrillUnit)]
        public string DrillUnit { get; set; }

        [Column(DatabaseLiterals.FieldDrillDate)]
        public string DrillDate { get; set; }

        [Column(DatabaseLiterals.FieldDrillHoleSize)]
        public string DrillHoleSize { get; set; }

        [Column(DatabaseLiterals.FieldDrillCoreSize)]
        public string DrillCoreSize { get; set; }

        [Column(DatabaseLiterals.FieldDrillRelogType)]
        public string DrillRelogType { get; set; }

        [Column(DatabaseLiterals.FieldDrillRelogBy)]
        public string DrillRelogBy { get; set; }

        [Column(DatabaseLiterals.FieldDrillRelogIntervals)]
        public string DrillRelogIntervals { get; set; }

        [Column(DatabaseLiterals.FieldDrillLog)]
        public string DrillLog { get; set; }

        [Column(DatabaseLiterals.FieldDrillNotes)]
        public string DrillNotes { get; set; }

        [Column(DatabaseLiterals.FieldDrillLocationID)]
        public string DrillLocationID { get; set; }

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

                drillFieldListDefault.Add(DatabaseLiterals.FieldDrillID);
                foreach (System.Reflection.PropertyInfo item in this.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ColumnAttribute))).ToList())
                {
                    if (item.CustomAttributes.First().ConstructorArguments.Count() > 0)
                    {
                        drillFieldListDefault.Add(item.CustomAttributes.First().ConstructorArguments[0].ToString().Replace("\\", "").Replace("\"", ""));
                    }

                }

                drillFieldList[DatabaseLiterals.DBVersion] = drillFieldListDefault;


                return drillFieldList;
            }
            set { }
        }


    }
}
