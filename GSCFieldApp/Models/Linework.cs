using static GSCFieldApp.Dictionaries.DatabaseLiterals;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GSCFieldApp.Models
{

    [Table(TableLinework)]
    public class Linework
    {
        [PrimaryKey, AutoIncrement, Column(FieldLineworkID)]
        public int LineID { get; set; }

        [Column(FieldLineworkGeometry)]
        public byte[] LineGeom { get; set; }

        [Column(FieldLineworkIDName)]
        public string LineIDName { get; set; }

        [Column(FieldLineworkType)]
        public string LineType { get; set; }

        [Column(FieldLineworkConfidence)]
        public int LineCpnfidence { get; set; }

        [Column(FieldLineworkSymbol)]
        public int LineSymbol { get; set; }

        [Column(FieldLineworkNotes)]
        public string LineNotes { get; set; }

        [Column(FieldLineworkMetaID)]
        public string LineMetaID { get; set; }

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
                Dictionary<double, List<string>> lineworkFieldList = new Dictionary<double, List<string>>();
                List<string> lineworkFieldListDefault = new List<string>();

                lineworkFieldListDefault.Add(FieldLineworkID);
                foreach (System.Reflection.PropertyInfo item in this.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ColumnAttribute))).ToList())
                {
                    if (item.CustomAttributes.First().ConstructorArguments.Count() > 0)
                    {
                        lineworkFieldListDefault.Add(item.CustomAttributes.First().ConstructorArguments[0].ToString().Replace("\\", "").Replace("\"", ""));
                    }

                }

                lineworkFieldList[DBVersion] = lineworkFieldListDefault;

                return lineworkFieldList;
            }
            set { }
        }

        /// <summary>
        /// Property to get a smaller version of the alias, for mobile rendering mostly
        /// </summary>
        [Ignore]
        public string LineAliasLight
        {
            get
            {
                if (LineIDName != null && LineIDName != string.Empty)
                {
                    int aliasNumber = 0;
                    int.TryParse(LineIDName.Substring(LineIDName.Length - 4), out aliasNumber);

                    if (aliasNumber > 0)
                    {
                        return "L" + aliasNumber.ToString();
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