using GSCFieldApp.Dictionaries;
using GSCFieldApp.Services.DatabaseServices;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GSCFieldApp.Models
{
    [Table(DatabaseLiterals.TableMineral)]
    public class Mineral
    {

        [PrimaryKey, AutoIncrement, Column(DatabaseLiterals.FieldMineralID)]
        public int MineralID { get; set; }

        [Column(DatabaseLiterals.FieldMineralIDName)]
        public string MineralIDName { get; set; }

        [Column(DatabaseLiterals.FieldMineral)]
        public string MineralName { get; set; }

        [Column(DatabaseLiterals.FieldMineralFormHabit)]
        public string MineralFormHabit { get; set; }

        [Column(DatabaseLiterals.FieldMineralOccurence)]
        public string MineralOccur { get; set; }

        [Column(DatabaseLiterals.FieldMineralColour)]
        public string MineralColour { get; set; }

        [Column(DatabaseLiterals.FieldMineralSizeMin)]
        public string MineralSizeMin { get; set; }

        [Column(DatabaseLiterals.FieldMineralSizeMax)]
        public string MineralSizeMax { get; set; }

        [Column(DatabaseLiterals.FieldMineralMode)]
        public string MineralMode { get; set; }

        [Column(DatabaseLiterals.FieldMineralNote)]
        public string MineralNote { get; set; }

        [Column(DatabaseLiterals.FieldMineralEMID)]
        public int? MineralEMID { get; set; }

        [Column(DatabaseLiterals.FieldMineralMAID)]
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
                if ((MineralName != string.Empty && MineralName != null && MineralName != Dictionaries.DatabaseLiterals.picklistNACode) &&
                    (MineralMode != string.Empty && MineralMode != null && MineralMode != Dictionaries.DatabaseLiterals.picklistNACode))
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

                mineralFieldListDefault.Add(DatabaseLiterals.FieldMineralID);
                foreach (System.Reflection.PropertyInfo item in this.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ColumnAttribute))).ToList())
                {
                    if (item.CustomAttributes.First().ConstructorArguments.Count() > 0)
                    {
                        mineralFieldListDefault.Add(item.CustomAttributes.First().ConstructorArguments[0].ToString().Replace("\\", "").Replace("\"", ""));
                    }

                }

                mineralFieldList[DatabaseLiterals.DBVersion] = mineralFieldListDefault;

                //Revert shcema 1.7 changes
                //List<string> mineralFieldList160 = new List<string>();
                //mineralFieldList160.AddRange(mineralFieldListDefault);
                //mineralFieldList160.Remove(DatabaseLiterals.FieldGenericRowID);
                mineralFieldList[DatabaseLiterals.DBVersion160] = mineralFieldListDefault;


                //Revert schema 1.6 changes. 
                List<string> mineralFieldList150 = new List<string>();
                mineralFieldList150.AddRange(mineralFieldListDefault);
                int removeIndex = mineralFieldList150.IndexOf(DatabaseLiterals.FieldMineralFormHabit);
                mineralFieldList150.Remove(DatabaseLiterals.FieldMineralFormHabit);
                mineralFieldList150.Insert(removeIndex, DatabaseLiterals.FieldMineralHabitDeprecated);
                mineralFieldList150.Insert(removeIndex, DatabaseLiterals.FieldMineralFormDeprecated);

                mineralFieldList150.Remove(DatabaseLiterals.FieldMineralMAID);

                mineralFieldList[DatabaseLiterals.DBVersion150] = mineralFieldList150;

                return mineralFieldList;
            }
            set { }
        }
    }
}
