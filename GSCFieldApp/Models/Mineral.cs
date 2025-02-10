using static GSCFieldApp.Dictionaries.DatabaseLiterals;
using GSCFieldApp.Services.DatabaseServices;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

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
        /// Possible values are:    
        /// Associated to the first earth material record of the tenth station--> 25GHV0010AM01
        /// Associated to the first mineralization record of the tenth station --> 25GHV0010X01M01
        /// Associated to the first mineralization associated to the first earth mateerial of the tenth station --> 25GHV0010AX01M01
        /// ^\d{4}$
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
                        string shorterStructureName = MineralIDName;

                        //Get the desire pattern with regex
                        string stationDigitPattern = @"\d{4}\w*"; //Any 4 consecutive digits and any word that comes afterward
                        Regex reg = new Regex(stationDigitPattern);
                        Match regMatch = reg.Match(MineralIDName);

                        if (regMatch != null && regMatch.Value != string.Empty)
                        {
                            shorterStructureName = regMatch.Value;
                        }

                        //Trim bunch of zeros
                        shorterStructureName = shorterStructureName.TrimStart('0');

                        return shorterStructureName;
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
