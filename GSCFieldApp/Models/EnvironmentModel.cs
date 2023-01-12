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
        public string EnvID { get; set; }

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
        public string EnvStationID { get; set; }

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

    }
}
