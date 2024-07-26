using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GSCFieldApp.Dictionaries.DatabaseLiterals;
using SQLite;

namespace GSCFieldApp.Models
{
    [Table(TableEnvironment)]
    public class EnvironmentModel
    {

        [PrimaryKey, AutoIncrement, Column(FieldEnvID)]
        public int EnvID { get; set; }

        [Column(FieldEnvName)]
        public string EnvName { get; set; }

        [Column(FieldEnvRelief)]
        public string EnvRelief { get; set; }

        [Column(FieldEnvSlope)]
        public int EnvSlope { get; set; }
        [Column(FieldEnvAzim)]
        public int EnvAzim { get; set; }
        [Column(FieldEnvDrainage)]
        public string EnvDrainage{ get; set; }
        [Column(FieldEnvPermIndicator)]
        public string EnvPermIndicator{ get; set; }
        [Column(FieldEnvGroundPattern)]
        public string EnvGroundPattern { get; set; }
        [Column(FieldEnvGroundIce)]
        public string EnvGroundIce { get; set; }
        [Column(FieldEnvGroundCover)]
        public string EnvGroundCover { get; set; }
        [Column(FieldEnvActiveLayerDepth)]
        public double EnvActiveLayerDepth { get; set; }
        [Column(FieldEnvBoulder)]
        public string EnvBoulder { get; set; }
        [Column(FieldEnvExposure)]
        public string EnvExposure { get; set; }
        [Column(FieldEnvNotes)]
        public string EnvNotes { get; set; }
        [Column(FieldEnvStationID)]
        public int EnvStationID { get; set; }

        /// <summary>
        /// Soft mandatory field check. User can still create record even if fields are not filled.
        /// Ignore attribute will tell sql not to try to write this field inside the database.
        /// </summary>
        [Ignore]
        public bool isValid
        {
            get
            {
                if (EnvRelief != string.Empty && EnvRelief != null)
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
                Dictionary<double, List<string>> envFieldList = new Dictionary<double, List<string>>();
                List<string> envFieldListDefault = new List<string>();

                envFieldListDefault.Add(FieldGenericRowID);
                foreach (System.Reflection.PropertyInfo item in this.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ColumnAttribute))).ToList())
                {
                    if (item.CustomAttributes.First().ConstructorArguments.Count() > 0)
                    {
                        envFieldListDefault.Add(item.CustomAttributes.First().ConstructorArguments[0].ToString().Replace("\\", "").Replace("\"", ""));
                    }

                }

                envFieldList[DBVersion] = envFieldListDefault;

                //Revert shcema 1.7 changes
                List<string> envFieldList160 = new List<string>();
                envFieldList160.AddRange(envFieldListDefault);
                envFieldList160.Remove(FieldGenericRowID);
                envFieldList[DBVersion160] = envFieldList160;


                return envFieldList;
            }
            set { }
        }

        /// <summary>
        /// Property to get a smaller version of the alias, for mobile rendering mostly
        /// </summary>
        [Ignore]
        public string EnvironmentAliasLight
        {
            get
            {
                if (EnvName != string.Empty)
                {
                    int aliasNumber = 0;
                    int.TryParse(EnvName.Substring(EnvName.Length - 2), out aliasNumber);

                    if (aliasNumber > 0)
                    {
                        //Trim bunch of zeros
                        string shorterStructureName = EnvName.Substring(EnvName.Length - 7);
                        return shorterStructureName.Trim('0');
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
