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
        public string LineConfidence { get; set; }

        [Column(FieldLineworkSymbol)]
        public string LineSymbol { get; set; }

        [Column(FieldLineworkNotes)]
        public string LineNotes { get; set; }

        [Column(FieldLineworkMetaID)]
        public int LineMetaID { get; set; }

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

        /// <summary>
        /// Soft mandatory field check. User can still create record even if fields are not filled.
        /// Ignore attribute will tell sql not to try to write this field inside the database.
        /// </summary>
        [Ignore]
        public bool isValid
        {
            get
            {
                if (LineType != string.Empty)
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
        /// Will be used to trigger a cascade delete coming from location record
        /// </summary>
        [Ignore]
        public bool IsMapPageQuick { get; set; } = false;
    }
}