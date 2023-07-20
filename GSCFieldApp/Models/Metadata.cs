using System;
using System.Collections.Generic;
using System.Linq;
using SQLite;
using GSCFieldApp.Dictionaries;

namespace GSCFieldApp.Models
{
    [Table(DatabaseLiterals.TableMetadata)]
    public class Metadata
    {

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        [PrimaryKey, AutoIncrement, Column(DatabaseLiterals.FieldUserInfoID), NotNull]
        public int MetaID { get; set; }

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
        [Column(DatabaseLiterals.FieldUserInfoVersionSchema)]
        public string VersionSchema { get; set; }


        [Column(DatabaseLiterals.FieldUserIsActive)]
        public int IsActive { get; set; }

        [Column(DatabaseLiterals.FieldUserStartDate)]
        public string StartDate { get; set; }

        [Column(DatabaseLiterals.FieldUserInfoActivityName)]
        public string MetadataActivity { get; set; }

        [Column(DatabaseLiterals.FieldUserInfoNotes)]
        public string MetadataNotes { get; set; }


        /// <summary>
        /// Soft mandatory field check. User can still create record even if fields are not filled.
        /// Ignore attribute will tell sql not to try to write this field inside the database.
        /// </summary>
        [Ignore]
        public bool isValid
        {
            get
            {
                if (UserCode != string.Empty && FieldworkType != string.Empty && MetadataActivity != string.Empty &&
                    ProjectName != string.Empty &&
                    ProjectUser_FN != string.Empty && ProjectUser_LN != string.Empty && Version != string.Empty &&
                    VersionSchema != string.Empty)
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
        /// A list of all possible fields from current class but also from previous schemas (for db upgrade)
        /// </summary>
        [Ignore]
        public Dictionary<double, List<string>> getFieldList
        {
            get
            {
                //Create a new list of all current columns in current class. This will act as the most recent
                //version of the class
                Dictionary<double, List<string>> metadataFieldList = new Dictionary<double, List<string>>();
                List<string> metadataFieldListDefault = new List<string>();
                metadataFieldListDefault.Add(DatabaseLiterals.FieldUserInfoID);
                foreach (System.Reflection.PropertyInfo item in this.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ColumnAttribute))).ToList())
                {
                    if (item.CustomAttributes.First().ConstructorArguments.Count() > 0)
                    {
                        metadataFieldListDefault.Add(item.CustomAttributes.First().ConstructorArguments[0].ToString().Replace("\\", "").Replace("\"", ""));
                    }

                }

                metadataFieldList[DatabaseLiterals.DBVersion] = metadataFieldListDefault;


                //Revert shcema 1.7 changes
                List<string> metadataFieldList160 = new List<string>();
                metadataFieldList160.AddRange(metadataFieldListDefault);
                metadataFieldList160.Remove(DatabaseLiterals.FieldGenericRowID);
                metadataFieldList[DatabaseLiterals.DBVersion160] = metadataFieldList160;

                //Noting changed in 1.6
                metadataFieldList[DatabaseLiterals.DBVersion150] = metadataFieldList160;

                //Revert schema 1.5 changes. 
                List<string> metadataFieldList144 = new List<string>();
                metadataFieldList144.AddRange(metadataFieldList[DatabaseLiterals.DBVersion150]);
                metadataFieldList144.Remove(DatabaseLiterals.FieldUserInfoActivityName);
                metadataFieldList144.Remove(DatabaseLiterals.FieldUserInfoNotes);
                metadataFieldList[DatabaseLiterals.DBVersion144] = metadataFieldList144;

                //Revert schema 1.4.4 
                List<string> metadataFieldList143 = new List<string>();
                metadataFieldList143.AddRange(metadataFieldList144);
                metadataFieldList143.Remove(DatabaseLiterals.FieldUserInfoEPSG);
                metadataFieldList[DatabaseLiterals.DBVersion143] = metadataFieldList143;

                //Revert schema 1.4.3 changes
                List<string> metadataFieldList142 = new List<string>();
                metadataFieldList142.AddRange(metadataFieldList143);
                metadataFieldList[DatabaseLiterals.DBVersion142] = metadataFieldList142;


                return metadataFieldList;
            }
            set { }
        }

        /// <summary>
        /// Will calculate a field book file name. Used for database copies
        /// </summary>
        /// <returns></returns>
        [Ignore]
        public string FieldBookFileName
        {
            get
            {

                //If project name isn't empty use that first
                if (ProjectName != null && ProjectName != string.Empty)
                {
                    return ProjectName.ToString().Replace(" ", "_") + "_" + UserCode;
                }
                //If proejctName is empty take geologist name
                else
                {
                    return Geologist.ToString().Replace(",", "_") + "_" + UserCode;
                }

            }
            set { }
        }

        /// <summary>
        /// Will calculate a field book file name. Used for database copies
        /// </summary>
        /// <returns></returns>
        [Ignore]
        public string FieldBookFileNameWithDate
        {
            get
            {
                //Calculate current date
                string currentDate = String.Format("{0:yyyy_MM_dd_HH'h'mm}", DateTime.Now);

                //If project name isn't empty use that first
                if (ProjectName != null && ProjectName != string.Empty)
                {
                    return ProjectName.ToString().Replace(" ", "_") + "_" + currentDate + "_" + UserCode;
                }
                //If proejctName is empty take geologist name
                else
                {
                    return Geologist.ToString().Replace(",", "_") + "_" + currentDate + "_" + UserCode;
                }

            }
            set { }
        }

    }
}
