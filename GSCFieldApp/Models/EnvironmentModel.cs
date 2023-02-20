using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSCFieldApp.Dictionaries;
using SQLite;

namespace GSCFieldApp.Models
{
    [Table(DatabaseLiterals.TableEnvironment)]
    public class EnvironmentModel
    {

        [PrimaryKey, Column(DatabaseLiterals.FieldEnvID)]
        public int EnvID { get; set; }

        [Column(DatabaseLiterals.FieldEnvName)]
        public string EnvName { get; set; }

        [Column(DatabaseLiterals.FieldEnvRelief)]
        public string EnvRelief { get; set; }

        [Column(DatabaseLiterals.FieldEnvSlope)]
        public int EnvSlope { get; set; }
        [Column(DatabaseLiterals.FieldEnvAzim)]
        public int EnvAzim { get; set; }
        [Column(DatabaseLiterals.FieldEnvDrainage)]
        public string EnvDrainage{ get; set; }
        [Column(DatabaseLiterals.FieldEnvPermIndicator)]
        public string EnvPermIndicator{ get; set; }
        [Column(DatabaseLiterals.FieldEnvGroundPattern)]
        public string EnvGroundPattern { get; set; }
        [Column(DatabaseLiterals.FieldEnvGroundIce)]
        public string EnvGroundIce { get; set; }
        [Column(DatabaseLiterals.FieldEnvGroundCover)]
        public string EnvGroundCover { get; set; }
        [Column(DatabaseLiterals.FieldEnvActiveLayerDepth)]
        public double EnvActiveLayerDepth { get; set; }
        [Column(DatabaseLiterals.FieldEnvBoulder)]
        public string EnvBoulder { get; set; }
        [Column(DatabaseLiterals.FieldEnvExposure)]
        public string EnvExposure { get; set; }
        [Column(DatabaseLiterals.FieldEnvNotes)]
        public string EnvNotes { get; set; }
        [Column(DatabaseLiterals.FieldEnvStationID)]
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

                envFieldListDefault.Add(DatabaseLiterals.FieldGenericRowID);
                foreach (System.Reflection.PropertyInfo item in this.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ColumnAttribute))).ToList())
                {
                    if (item.CustomAttributes.First().ConstructorArguments.Count() > 0)
                    {
                        envFieldListDefault.Add(item.CustomAttributes.First().ConstructorArguments[0].ToString().Replace("\\", "").Replace("\"", ""));
                    }

                }

                envFieldList[DatabaseLiterals.DBVersion] = envFieldListDefault;

                //Revert shcema 1.7 changes
                List<string> envFieldList160 = new List<string>();
                envFieldList160.AddRange(envFieldListDefault);
                envFieldList160.Remove(DatabaseLiterals.FieldGenericRowID);
                envFieldList[DatabaseLiterals.DBVersion160] = envFieldList160;


                return envFieldList;
            }
            set { }
        }


    }
}
