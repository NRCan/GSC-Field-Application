using static GSCFieldApp.Dictionaries.DatabaseLiterals;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GSCFieldApp.Models
{
    [Table(TableMineralAlteration)]
    public class MineralAlteration
    {

        [PrimaryKey, AutoIncrement, Column(FieldMineralAlterationID)]
        public int MAID { get; set; }

        [Column(FieldMineralAlterationName)]
        public string MAName { get; set; }

        [Column(FieldMineralAlteration)]
        public string MAMA { get; set; }

        [Column(FieldMineralAlterationUnit)]
        public string MAUnit { get; set; }

        [Column(FieldMineralAlterationDistrubute)]
        public string MADistribute { get; set; }

        [Column(FieldMineralAlterationPhase)]
        public string MAPhase { get; set; }

        [Column(FieldMineralAlterationTexture)]
        public string MATexture { get; set; }

        [Column(FieldMineralAlterationFacies)]
        public string MAFacies { get; set; }

        [Column(FieldMineralAlterationNotes)]
        public string MANotes { get; set; }

        [Column(FieldMineralAlterationEarthmatID)]
        public int? MAEarthmatID { get; set; }

        [Column(FieldMineralAlterationStationID)]
        public int? MAStationID { get; set; }


        //Hierarchy
        public string ParentName = TableStation;

        /// <summary>
        /// Soft mandatory field check. User can still create record even if fields are not filled.
        /// Ignore attribute will tell sql not to try to write this field inside the database.
        /// </summary>
        [Ignore]
        public bool isValid
        {
            get
            {
                if ((MAMA != string.Empty && MAMA != null && MAMA != picklistNACode))
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
                Dictionary<double, List<string>> maFieldList = new Dictionary<double, List<string>>();
                List<string> maFieldListDefault = new List<string>();

                maFieldListDefault.Add(FieldMineralAlterationID);
                foreach (System.Reflection.PropertyInfo item in this.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ColumnAttribute))).ToList())
                {
                    if (item.CustomAttributes.First().ConstructorArguments.Count() > 0)
                    {
                        maFieldListDefault.Add(item.CustomAttributes.First().ConstructorArguments[0].ToString().Replace("\\", "").Replace("\"", ""));
                    }

                }

                maFieldList[DBVersion] = maFieldListDefault;

                //Revert schema 1.8 changes
                List<string> maFieldList170 = new List<string>();
                maFieldList170.AddRange(maFieldListDefault);
                maFieldList170.Remove(FieldMineralAlterationEarthmatID);
                maFieldList170.Remove(FieldMineralAlterationStationID);
                maFieldList170.Add(FieldMineralAlterationRelTableDeprecated);
                maFieldList170.Add(FieldMineralAlterationRelIDDeprecated);
                maFieldList[DBVersion170] = maFieldList170;

                //Revert shcema 1.7 changes
                //List<string> maFieldList160 = new List<string>();
                //maFieldList160.AddRange(maFieldListDefault);
                //maFieldList160.Remove(FieldGenericRowID);
                maFieldList[DBVersion160] = maFieldList170;


                //Revert schema 1.6 changes. 
                List<string> maFieldList150 = new List<string>();
                maFieldList150.AddRange(maFieldList170);

                int unitIndex = maFieldList150.IndexOf(FieldMineralAlterationUnit);
                maFieldList150.Insert(unitIndex + 1, FieldMineralAlterationMineralDeprecated);
                maFieldList150.Insert(unitIndex + 2, FieldMineralAlterationModeDeprecated);
                maFieldList150.Remove(FieldMineralAlterationPhase);
                maFieldList150.Remove(FieldMineralAlterationTexture);
                maFieldList150.Remove(FieldMineralAlterationFacies);
                maFieldList[DBVersion150] = maFieldList150;

                return maFieldList;
            }
            set { }
        }

        /// <summary>
        /// Property to get a smaller version of the alias, for mobile rendering mostly
        /// </summary>
        [Ignore]
        public string MineralALterationAliasLight
        {
            get
            {
                if (MAName != string.Empty)
                {
                    int aliasNumber = 0;
                    int.TryParse(MAName.Substring(MAName.Length - 2), out aliasNumber);

                    if (aliasNumber > 0)
                    {
                        //Trim bunch of zeros
                        string shorterStructureName = MAName.Substring(MAName.Length - 7);
                        return shorterStructureName.TrimStart('0');
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
