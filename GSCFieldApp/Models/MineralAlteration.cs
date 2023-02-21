using GSCFieldApp.Dictionaries;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GSCFieldApp.Models
{
    [Table(DatabaseLiterals.TableMineralAlteration)]
    public class MineralAlteration
    {

        [PrimaryKey, Column(DatabaseLiterals.FieldMineralAlterationID)]
        public int MAID { get; set; }

        [Column(DatabaseLiterals.FieldMineralAlterationName)]
        public string MAName { get; set; }

        [Column(DatabaseLiterals.FieldMineralAlteration)]
        public string MAMA { get; set; }

        [Column(DatabaseLiterals.FieldMineralAlterationUnit)]
        public string MAUnit { get; set; }

        [Column(DatabaseLiterals.FieldMineralAlterationDistrubute)]
        public string MADistribute { get; set; }

        [Column(DatabaseLiterals.FieldMineralAlterationPhase)]
        public string MAPhase { get; set; }

        [Column(DatabaseLiterals.FieldMineralAlterationTexture)]
        public string MATexture { get; set; }

        [Column(DatabaseLiterals.FieldMineralAlterationFacies)]
        public string MAFacies { get; set; }

        [Column(DatabaseLiterals.FieldMineralAlterationNotes)]
        public string MANotes { get; set; }

        [Column(DatabaseLiterals.FieldMineralAlterationRelTable)]
        public string MAParentTable { get; set; }

        [Column(DatabaseLiterals.FieldMineralAlterationRelID)]
        public int MAParentID { get; set; }

        //Hierarchy
        public string ParentName = DatabaseLiterals.TableStation;

        /// <summary>
        /// Soft mandatory field check. User can still create record even if fields are not filled.
        /// Ignore attribute will tell sql not to try to write this field inside the database.
        /// </summary>
        [Ignore]
        public bool isValid
        {
            get
            {
                if ((MAMA != string.Empty && MAMA != null && MAMA != Dictionaries.DatabaseLiterals.picklistNACode))
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

                maFieldListDefault.Add(DatabaseLiterals.FieldMineralAlterationID);
                foreach (System.Reflection.PropertyInfo item in this.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ColumnAttribute))).ToList())
                {
                    if (item.CustomAttributes.First().ConstructorArguments.Count() > 0)
                    {
                        maFieldListDefault.Add(item.CustomAttributes.First().ConstructorArguments[0].ToString().Replace("\\", "").Replace("\"", ""));
                    }

                }

                maFieldList[DatabaseLiterals.DBVersion] = maFieldListDefault;

                //Revert shcema 1.7 changes
                //List<string> maFieldList160 = new List<string>();
                //maFieldList160.AddRange(maFieldListDefault);
                //maFieldList160.Remove(DatabaseLiterals.FieldGenericRowID);
                //maFieldList[DatabaseLiterals.DBVersion160] = maFieldList160;


                //Revert schema 1.6 changes. 
                List<string> maFieldList150 = new List<string>();
                maFieldList150.AddRange(maFieldListDefault);

                int unitIndex = maFieldList150.IndexOf(DatabaseLiterals.FieldMineralAlterationUnit);
                maFieldList150.Insert(unitIndex + 1, DatabaseLiterals.FieldMineralAlterationMineralDeprecated);
                maFieldList150.Insert(unitIndex + 2, DatabaseLiterals.FieldMineralAlterationModeDeprecated);
                maFieldList150.Remove(DatabaseLiterals.FieldMineralAlterationPhase);
                maFieldList150.Remove(DatabaseLiterals.FieldMineralAlterationTexture);
                maFieldList150.Remove(DatabaseLiterals.FieldMineralAlterationFacies);
                maFieldList[DatabaseLiterals.DBVersion150] = maFieldList150;

                return maFieldList;
            }
            set { }
        }
    }
}
