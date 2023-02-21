using GSCFieldApp.Dictionaries;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GSCFieldApp.Models
{
    [Table(DatabaseLiterals.TablePFlow)]
    public class Paleoflow
    {
        [PrimaryKey, Column(DatabaseLiterals.FieldPFlowID)]
        public int PFlowID { get; set; }

        [Column(DatabaseLiterals.FieldPFlowName)]
        public string PFlowName { get; set; }

        [Column(DatabaseLiterals.FieldPFlowClass)]
        public string PFlowClass { get; set; }

        [Column(DatabaseLiterals.FieldPFlowSense)]
        public string PFlowSense{ get; set; }

        [Column(DatabaseLiterals.FieldPFlowFeature)]
        public string PFlowFeature { get; set; }

        [Column(DatabaseLiterals.FieldPFlowMethod)]
        public string PFlowMethod { get; set; }

        [Column(DatabaseLiterals.FieldPFlowAzimuth)]
        public string PFlowAzimuth { get; set; }

        [Column(DatabaseLiterals.FieldPFlowMainDir)]
        public string PFlowMainDir { get; set; }

        [Column(DatabaseLiterals.FieldPFlowRelage)]
        public string PFlowRelAge { get; set; }

        [Column(DatabaseLiterals.FieldPFlowDip)]
        public string PFlowDip { get; set; }

        [Column(DatabaseLiterals.FieldPFlowNumIndic)]
        public string PFlowNumIndic { get; set; }

        [Column(DatabaseLiterals.FieldPFlowDefinition)]
        public string PFlowDefinition { get; set; }

        [Column(DatabaseLiterals.FieldPFlowRelation)]
        public string PFlowRelation { get; set; }

        [Column(DatabaseLiterals.FieldPFlowBedsurf)]
        public string PFlowBedsurf { get; set; }

        [Column(DatabaseLiterals.FieldPFlowConfidence)]
        public string PFlowConfidence { get; set; }

        [Column(DatabaseLiterals.FieldPFlowNotes)]
        public string PFlowNotes { get; set; }

        [Column(DatabaseLiterals.FieldPFlowParentID)]
        public int PFlowParentID { get; set; }

        //Hierarchy
        public string ParentName = DatabaseLiterals.TableEarthMat;

        /// <summary>
        /// Soft mandatory field check. User can still create record even if fields are not filled.
        /// Ignore attribute will tell sql not to try to write this field inside the database.
        /// </summary>
        [Ignore]
        public bool isValid
        {
            get
            {
                if ((PFlowClass != string.Empty && PFlowClass != null && PFlowClass != Dictionaries.DatabaseLiterals.picklistNACode) &&
                    (PFlowSense != string.Empty && PFlowSense != null && PFlowSense != Dictionaries.DatabaseLiterals.picklistNACode))
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
                Dictionary<double, List<string>> pflowFieldList = new Dictionary<double, List<string>>();
                List<string> pflowFieldListDefault = new List<string>();

                pflowFieldListDefault.Add(DatabaseLiterals.FieldPFlowID);
                foreach (System.Reflection.PropertyInfo item in this.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ColumnAttribute))).ToList())
                {
                    if (item.CustomAttributes.First().ConstructorArguments.Count() > 0)
                    {
                        pflowFieldListDefault.Add(item.CustomAttributes.First().ConstructorArguments[0].ToString().Replace("\\", "").Replace("\"", ""));
                    }

                }

                pflowFieldList[DatabaseLiterals.DBVersion] = pflowFieldListDefault;

                //Revert shcema 1.7 changes
                //List<string> pflowFieldList160 = new List<string>();
                //pflowFieldList160.AddRange(pflowFieldListDefault);
                //pflowFieldList160.Remove(DatabaseLiterals.FieldGenericRowID);
                //pflowFieldList[DatabaseLiterals.DBVersion160] = pflowFieldList160;


                return pflowFieldList;
            }
            set { }
        }

    }
}
