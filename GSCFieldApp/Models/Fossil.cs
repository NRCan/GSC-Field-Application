using static GSCFieldApp.Dictionaries.DatabaseLiterals;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GSCFieldApp.Models
{
    [Table(TableFossil)]
    public class Fossil
    {

        [PrimaryKey, AutoIncrement, Column(FieldFossilID)]
        public int FossilID { get; set; }

        [Column(FieldFossilName)]
        public string FossilIDName { get; set; }

        [Column(FieldFossilType)]
        public string FossilType { get; set; }

        [Column(FieldFossilNote)]
        public string FossilNote { get; set; }

        [Column(FieldFossilParentID)]
        public int FossilParentID { get; set; }

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
                if ((FossilType != string.Empty && FossilType != null && FossilType != picklistNACode))
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
                Dictionary<double, List<string>> fossilFieldList = new Dictionary<double, List<string>>();
                List<string> fossilFieldListDefault = new List<string>();

                fossilFieldListDefault.Add(FieldFossilID);
                foreach (System.Reflection.PropertyInfo item in this.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ColumnAttribute))).ToList())
                {
                    if (item.CustomAttributes.First().ConstructorArguments.Count() > 0)
                    {
                        fossilFieldListDefault.Add(item.CustomAttributes.Last().ConstructorArguments[0].ToString().Replace("\\", "").Replace("\"", ""));
                    }

                }

                fossilFieldList[DBVersion] = fossilFieldListDefault;

                //Revert shcema 1.7 changes
                //List<string> fossilFieldList160 = new List<string>();
                //fossilFieldList160.AddRange(fossilFieldListDefault);
                //fossilFieldList160.Remove(FieldGenericRowID);
                //fossilFieldList[DBVersion160] = fossilFieldList160;


                return fossilFieldList;
            }
            set { }
        }

        /// <summary>
        /// Property to get a smaller version of the alias, for mobile rendering mostly
        /// </summary>
        [Ignore]
        public string FossilAliasLight
        {
            get
            {
                if (FossilIDName != string.Empty)
                {
                    int aliasNumber = 0;
                    int.TryParse(FossilIDName.Substring(FossilIDName.Length - 2), out aliasNumber);

                    if (aliasNumber > 0)
                    {
                        //Trim bunch of zeros
                        string shorterName = FossilIDName.Substring(FossilIDName.Length - 7);
                        return shorterName.TrimStart('0');
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
