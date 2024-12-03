using static GSCFieldApp.Dictionaries.DatabaseLiterals;
using GSCFieldApp.Services.DatabaseServices;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GSCFieldApp.Models
{
    [Table(TableMineral)]
    public class Mineral
    {

        [Column(FieldMineralID), PrimaryKey, AutoIncrement]
        public int MineralID { get; set; }

        [Column(FieldMineralIDName)]
        public string MineralIDName { get; set; }

        [Column(FieldMineral)]
        public string MineralName { get; set; }

        [Column(FieldMineralFormHabit)]
        public string MineralFormHabit { get; set; }

        [Column(FieldMineralOccurence)]
        public string MineralOccur { get; set; }

        [Column(FieldMineralColour)]
        public string MineralColour { get; set; }

        [Column(FieldMineralSizeMin)]
        public int MineralSizeMin { get; set; }

        [Column(FieldMineralSizeMax)]
        public int MineralSizeMax { get; set; }

        [Column(FieldMineralMode)]
        public string MineralMode { get; set; }

        [Column(FieldMineralNote)]
        public string MineralNote { get; set; }

        [Column(FieldMineralEMID)]
        public int? MineralEMID { get; set; }

        [Column(FieldMineralMAID)]
        public int? MineralMAID { get; set; }


        /// <summary>
        /// Soft mandatory field check. User can still create record even if fields are not filled.
        /// Ignore attribute will tell sql not to try to write this field inside the database.
        /// </summary>
        [Ignore]
        public bool isValid
        {
            get
            {
                if ((MineralName != string.Empty && MineralName != null && MineralName != picklistNACode) &&
                    (MineralMode != string.Empty && MineralMode != null && MineralMode != picklistNACode))
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
                Dictionary<double, List<string>> mineralFieldList = new Dictionary<double, List<string>>();
                List<string> mineralFieldListDefault = new List<string>();

                foreach (System.Reflection.PropertyInfo item in this.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ColumnAttribute))).ToList())
                {
                    if (item.CustomAttributes.First().ConstructorArguments.Count() > 0)
                    {
                        mineralFieldListDefault.Add(item.CustomAttributes.First().ConstructorArguments[0].ToString().Replace("\\", "").Replace("\"", ""));
                    }

                }

                mineralFieldList[DBVersion] = mineralFieldListDefault;

                //Revert shcema 1.7 changes
                //List<string> mineralFieldList160 = new List<string>();
                //mineralFieldList160.AddRange(mineralFieldListDefault);
                //mineralFieldList160.Remove(FieldGenericRowID);
                mineralFieldList[DBVersion160] = mineralFieldListDefault;


                //Revert schema 1.6 changes. 
                List<string> mineralFieldList150 = new List<string>();
                mineralFieldList150.AddRange(mineralFieldListDefault);
                int removeIndex = mineralFieldList150.IndexOf(FieldMineralFormHabit);
                mineralFieldList150.Remove(FieldMineralFormHabit);
                mineralFieldList150.Insert(removeIndex, FieldMineralHabitDeprecated);
                mineralFieldList150.Insert(removeIndex, FieldMineralFormDeprecated);

                mineralFieldList150.Remove(FieldMineralMAID);

                mineralFieldList[DBVersion150] = mineralFieldList150;

                return mineralFieldList;
            }
            set { }
        }

        /// <summary>
        /// Property to get a smaller version of the alias, for mobile rendering mostly
        /// </summary>
        [Ignore]
        public string MineralAliasLight
        {
            get
            {
                if (MineralIDName != string.Empty)
                {
                    int aliasNumber = 0;
                    int.TryParse(MineralIDName.Substring(MineralIDName.Length - 2), out aliasNumber);

                    if (aliasNumber > 0)
                    {
                        //Trim bunch of zeros
                        string shorterStructureName = MineralIDName.Substring(MineralIDName.Length - 8);
                        return shorterStructureName.TrimStart('0');
                    }
                    else
                    {
                        return MineralIDName;
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
