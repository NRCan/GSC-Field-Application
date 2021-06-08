using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;
using SQLite.Net.Attributes;
using GSCFieldApp.Dictionaries;

namespace GSCFieldApp.Models
{
    [Table(DatabaseLiterals.TableMetadata)]
    public class Metadata : BindableBase
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        [Column(DatabaseLiterals.FieldUserInfoID), PrimaryKey, NotNull]
        public string MetaID { get; set; }

        [Column(DatabaseLiterals.FieldUserInfoPCode)]
        public string ProjectCode { get; set; }

        [Column(DatabaseLiterals.FieldUserInfoPLeaderFN)]
        public string ProjectLeader_FN { get; set; }
        [Column(DatabaseLiterals.FieldUserInfoPLeaderMN)]
        public string ProjectLeader_MN { get; set; }
        [Column(DatabaseLiterals.FieldUserInfoPLeaderLN)]
        public string ProjectLeader_LN { get; set; }
        /// <summaryL>
        /// Gets or set the fieldwork type. e.g. Surficial or Bedrock?
        /// </summaryL>
        [Column(DatabaseLiterals.FieldUserInfoFWorkType), MaxLength(30), NotNull]
        public string FieldworkType { get; set; }
        [Column(DatabaseLiterals.FieldUserInfoPName)]
        public string ProjectName { get; set; }
        [Column(DatabaseLiterals.FieldUserInfoFN)]
        public string ProjectUser_FN { get; set; }
        [Column(DatabaseLiterals.FieldUserInfoMN)]
        public string ProjectUser_MN { get; set; }
        [Column(DatabaseLiterals.FieldUserInfoLN)]
        public string ProjectUser_LN { get; set; }
        /// <summaryL>
        /// Gets or set the user code or officer code.
        /// </summaryL>
        [Column(DatabaseLiterals.FieldUserInfoUCode), MaxLength(10), NotNull]
        public string UserCode { get; set; }
        [Column(DatabaseLiterals.FieldUserInfoStationStartNumber)]
        public string StationStartNumber { get; set; }
        [Column(DatabaseLiterals.FieldUserInfoVersion)]
        public string Version { get; set; }
        [Column(DatabaseLiterals.FieldUserInfoVersionSchema), Default(value: DatabaseLiterals.DBVersion)]
        public string VersionSchema { get; set; }


        [Column(DatabaseLiterals.FieldUserIsActive)]
        public int IsActive { get; set; }

        [Column(DatabaseLiterals.FieldUserStartDate)]
        public string StartDate { get; set; }



        /// <summary>
        /// Soft mandatory field check. User can still create record even if fields are not filled.
        /// Ignore attribute will tell sql not to try to write this field inside the database.
        /// </summary>
        [Ignore]
        public bool isValid
        {
            get
            {
                if (MetaID != string.Empty && UserCode != string.Empty && FieldworkType != string.Empty && ProjectName != string.Empty && 
                    ProjectUser_FN != string.Empty && ProjectUser_LN != string.Empty && Version != string.Empty && StationStartNumber != string.Empty && Convert.ToInt16(StationStartNumber) < 9999)
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
        /// Will calculate a geologist name from all of it's names to make it cute.
        /// </summary>
        [Ignore]
        public string Geologist
        {
            get
            {
                if (ProjectUser_MN != null && ProjectUser_MN != string.Empty)
                {
                    return ProjectUser_LN + ", " + ProjectUser_FN + " " + ProjectUser_MN.First() + ".";
                }
                else
                {
                    return ProjectUser_LN + ", " + ProjectUser_FN;
                }
                
            }
            set { }

        }

        /// <summary>
        /// A list of all possible fields
        /// </summary>
        [Ignore]
        public List<string> getFieldList
        {
            get
            {
                List<string> metadataFieldList = new List<string>();
                //metadataFieldList.Add(DatabaseLiterals.FieldUserInfoID);
                foreach (System.Reflection.PropertyInfo item in this.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ColumnAttribute))).ToList())
                {
                    if (item.CustomAttributes.First().ConstructorArguments.Count() > 0)
                    {
                        metadataFieldList.Add(item.CustomAttributes.First().ConstructorArguments[0].ToString().Replace("\\", "").Replace("\"", ""));
                    }

                }

                return metadataFieldList;
            }
            set { }
        }

    }
}
