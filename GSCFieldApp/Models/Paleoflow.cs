using static GSCFieldApp.Dictionaries.DatabaseLiterals;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GSCFieldApp.Models
{
    [Table(TablePFlow)]
    public class Paleoflow
    {
        [Column(FieldPFlowID), PrimaryKey, AutoIncrement]
        public int PFlowID { get; set; }

        [Column(FieldPFlowName)]
        public string PFlowName { get; set; }

        [Column(FieldPFlowClass)]
        public string PFlowClass { get; set; }

        [Column(FieldPFlowSense)]
        public string PFlowSense{ get; set; }

        [Column(FieldPFlowFeature)]
        public string PFlowFeature { get; set; }

        [Column(FieldPFlowMethod)]
        public string PFlowMethod { get; set; }

        [Column(FieldPFlowAzimuth)]
        public int PFlowAzimuth { get; set; }

        [Column(FieldPFlowMainDir)]
        public string PFlowMainDir { get; set; }

        [Column(FieldPFlowRelage)]
        public int PFlowRelAge { get; set; }

        [Column(FieldPFlowDip)]
        public int PFlowDip { get; set; }

        [Column(FieldPFlowNumIndic)]
        public string PFlowNumIndic { get; set; }

        [Column(FieldPFlowDefinition)]
        public string PFlowDefinition { get; set; }

        [Column(FieldPFlowRelation)]
        public string PFlowRelation { get; set; }

        [Column(FieldPFlowBedsurf)]
        public string PFlowBedsurf { get; set; }

        [Column(FieldPFlowConfidence)]
        public string PFlowConfidence { get; set; }

        [Column(FieldPFlowNotes)]
        public string PFlowNotes { get; set; }

        [Column(FieldPFlowParentID)]
        public int PFlowParentID { get; set; }

        //Hierarchy
        public string ParentName = TableEarthMat;

        /// <summary>
        /// Soft mandatory field check. User can still create record even if fields are not filled.
        /// Ignore attribute will tell sql not to try to write this field inside the database.
        /// </summary>
        [Ignore]
        public bool isValid
        {
            get
            {
                if ((PFlowClass != string.Empty && PFlowClass != null && PFlowClass != picklistNACode) &&
                    (PFlowSense != string.Empty && PFlowSense != null && PFlowSense != picklistNACode))
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

                foreach (System.Reflection.PropertyInfo item in this.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ColumnAttribute))).ToList())
                {
                    if (item.CustomAttributes.First().ConstructorArguments.Count() > 0)
                    {
                        pflowFieldListDefault.Add(item.CustomAttributes.First().ConstructorArguments[0].ToString().Replace("\\", "").Replace("\"", ""));
                    }

                }

                pflowFieldList[DBVersion] = pflowFieldListDefault;

                //Revert shcema 1.7 changes
                //List<string> pflowFieldList160 = new List<string>();
                //pflowFieldList160.AddRange(pflowFieldListDefault);
                //pflowFieldList160.Remove(FieldGenericRowID);
                //pflowFieldList[DBVersion160] = pflowFieldList160;


                return pflowFieldList;
            }
            set { }
        }

        /// <summary>
        /// Property to get a smaller version of the alias, for mobile rendering mostly
        /// </summary>
        [Ignore]
        public string PflowAliasLight
        {
            get
            {
                if (PFlowName != string.Empty)
                {
                    int aliasNumber = 0;
                    int.TryParse(PFlowName.Substring(PFlowName.Length - 2), out aliasNumber);

                    if (aliasNumber > 0)
                    {
                        //Trim bunch of zeros
                        string shorterName = PFlowName.Substring(PFlowName.Length - 7);
                        return shorterName.TrimStart('0');
                    }
                    else
                    {
                        return PFlowName;
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
