using GSCFieldApp.Dictionaries;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GSCFieldApp.Models
{

    [Table(DatabaseLiterals.TableLinework)]
    public class Linework
    {
        [PrimaryKey, AutoIncrement, Column(DatabaseLiterals.FieldLineworkID)]
        public int LineID { get; set; }

        [Column(DatabaseLiterals.FieldLineworkGeometry)]
        public byte[] LineGeom { get; set; }

        [Column(DatabaseLiterals.FieldLineworkIDName)]
        public string LineIDName { get; set; }

        [Column(DatabaseLiterals.FieldLineworkType)]
        public string LineType { get; set; }

        [Column(DatabaseLiterals.FieldLineworkConfidence)]
        public int LineCpnfidence { get; set; }

        [Column(DatabaseLiterals.FieldLineworkSymbol)]
        public int LineSymbol { get; set; }

        [Column(DatabaseLiterals.FieldLineworkNotes)]
        public string LineNotes { get; set; }

        [Column(DatabaseLiterals.FieldLineworkMetaID)]
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

                lineworkFieldListDefault.Add(DatabaseLiterals.FieldLineworkID);
                foreach (System.Reflection.PropertyInfo item in this.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ColumnAttribute))).ToList())
                {
                    if (item.CustomAttributes.First().ConstructorArguments.Count() > 0)
                    {
                        lineworkFieldListDefault.Add(item.CustomAttributes.First().ConstructorArguments[0].ToString().Replace("\\", "").Replace("\"", ""));
                    }

                }

                lineworkFieldList[DatabaseLiterals.DBVersion] = lineworkFieldListDefault;

                return lineworkFieldList;
            }
            set { }
        }
    }
}