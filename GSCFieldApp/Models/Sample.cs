using System;
using System.Collections.Generic;
using System.Linq;
using SQLite;
using GSCFieldApp.Dictionaries;

namespace GSCFieldApp.Models
{
    [Table(DatabaseLiterals.TableSample)]
    public class Sample
    {
        [Column(DatabaseLiterals.FieldSampleID), PrimaryKey, AutoIncrement]
        public int SampleID { get; set; }

        [Column(DatabaseLiterals.FieldSampleName)]
        public string SampleName { get; set; }

        [Column(DatabaseLiterals.FieldSampleType)]
        public string SampleType { get; set; }

        /// <summary>
        /// Is a concatenated field
        /// </summary>
        [Column(DatabaseLiterals.FieldSamplePurpose)]
        public string SamplePurpose { get; set; }

        [Column(DatabaseLiterals.FieldSampleFormat)]
        public string SampleFormat { get; set; }

        [Column(DatabaseLiterals.FieldSampleAzim)]
        public int SampleAzim { get; set; }

        [Column(DatabaseLiterals.FieldSampleDipPlunge)]
        public int SampleDiplunge { get; set; }

        [Column(DatabaseLiterals.FieldSampleSurface)]
        public string SampleSurface { get; set; }

        [Column(DatabaseLiterals.FieldSampleNotes)]
        public string SampleNotes { get; set; }

        [Column(DatabaseLiterals.FieldCurationID)]
        public string SampleCuration { get; set; }

        [Column(DatabaseLiterals.FieldSampleManagementID)]
        public int SampleSMID { get; set; }

        [Column(DatabaseLiterals.FieldSampleEarthmatID)]
        public int SampleEarthmatID { get; set; }

        [Column(DatabaseLiterals.FieldSampleQuality)]
        public string SampleQuality { get; set; }

        [Column(DatabaseLiterals.FieldSampleHorizon)]
        public string SampleHorizon { get; set; }

        [Column(DatabaseLiterals.FieldSampleDepthMin)]
        public int SampleDepthMin { get; set; }

        [Column(DatabaseLiterals.FieldSampleDepthMax)]
        public int SampleDepthMax { get; set; }

        [Column(DatabaseLiterals.FieldSampleDuplicate)]
        public int SampleDuplicate { get; set; }

        [Column(DatabaseLiterals.FieldSampleDuplicateName)]
        public string SampleDuplicateName { get; set; }

        [Column(DatabaseLiterals.FieldSampleState)]
        public string SampleState { get; set; }

        [Column(DatabaseLiterals.FieldSampleWarehouseLocation)]
        public string SampleWarehouse { get; set; }

        [Column(DatabaseLiterals.FieldSampleBucketTray)]
        public string SampleBucket { get; set; }

        [Column(DatabaseLiterals.FieldSampleIsBlank)]
        public string SampleBlank { get; set; }

        [Column(DatabaseLiterals.FieldSampleCoreFrom)]
        public double? SampleCoreFrom { get; set; }

        [Column(DatabaseLiterals.FieldSampleCoreTo)]
        public double? SampleCoreTo { get; set; }

        [Column(DatabaseLiterals.FieldSampleCoreLength)]
        public double? SampleCoreLength { get; set; }

        [Column(DatabaseLiterals.FieldSampleCoreSize)]
        public string SampleCoreSize { get; set; }

        [Column(DatabaseLiterals.FieldSampledBy)]
        public string SampleBy { get; set; }

        //Hierarchy
        public string ParentName = DatabaseLiterals.TableEarthMat;

        /// <summary>
        /// Soft mandatory field check. User can still create record even if fields are not filled.
        /// Ignore attribute will tell sql not to try to write this field inside the database.
        /// </summary>
        [Ignore]
        public bool isValid
        {
            get
            {
                if ((SamplePurpose != string.Empty && SamplePurpose != null && SamplePurpose != Dictionaries.DatabaseLiterals.picklistNACode) &&
                    (SampleType != string.Empty && SampleType != null && SampleType != Dictionaries.DatabaseLiterals.picklistNACode))
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
                Dictionary<double, List<string>> sampleFieldList = new Dictionary<double, List<string>>();
                List<string> sampleFieldListDefault = new List<string>();

                foreach (System.Reflection.PropertyInfo item in this.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ColumnAttribute))).ToList())
                {
                    if (item.CustomAttributes.First().ConstructorArguments.Count() > 0)
                    {
                        sampleFieldListDefault.Add(item.CustomAttributes.First().ConstructorArguments[0].ToString().Replace("\\", "").Replace("\"", ""));
                    }

                }

                sampleFieldList[DatabaseLiterals.DBVersion] = sampleFieldListDefault;


                //Revert shcema 1.8 changes
                List<string> sampleFieldList170 = new List<string>();
                sampleFieldList170.AddRange(sampleFieldListDefault);
                sampleFieldList170.Remove(DatabaseLiterals.FieldSampleIsBlank);
                sampleFieldList170.Remove(DatabaseLiterals.FieldSampleCoreFrom);
                sampleFieldList170.Remove(DatabaseLiterals.FieldSampleCoreTo);
                sampleFieldList170.Remove(DatabaseLiterals.FieldSampleCoreSize);
                sampleFieldList170.Remove(DatabaseLiterals.FieldSampledBy);
                sampleFieldList170.Remove(DatabaseLiterals.FieldSampleCoreLength);
                sampleFieldList[DatabaseLiterals.DBVersion170] = sampleFieldList170;

                //Revert shcema 1.7 changes
                List<string> sampleFieldList160 = new List<string>();
                sampleFieldList160.AddRange(sampleFieldList170);
                sampleFieldList160.Remove(DatabaseLiterals.FieldSampleBucketTray);
                sampleFieldList160.Remove(DatabaseLiterals.FieldSampleWarehouseLocation);
                sampleFieldList[DatabaseLiterals.DBVersion160] = sampleFieldList160;

                sampleFieldList[DatabaseLiterals.DBVersion150] = sampleFieldList160;

                //Revert schema 1.5 changes. 
                List<string> sampleFieldList144 = new List<string>();
                sampleFieldList144.AddRange(sampleFieldList[DatabaseLiterals.DBVersion150]);
                int removeIndex = sampleFieldList144.IndexOf(DatabaseLiterals.FieldSampleName);
                sampleFieldList144.Remove(DatabaseLiterals.FieldSampleName);
                sampleFieldList144.Insert(removeIndex, DatabaseLiterals.FieldSampleNameDeprecated);
                sampleFieldList144.Remove(DatabaseLiterals.FieldSampleHorizon);
                sampleFieldList144.Remove(DatabaseLiterals.FieldSampleDepthMin);
                sampleFieldList144.Remove(DatabaseLiterals.FieldSampleDepthMax);
                sampleFieldList144.Remove(DatabaseLiterals.FieldSampleDuplicate);
                sampleFieldList144.Remove(DatabaseLiterals.FieldSampleDuplicateName);
                sampleFieldList144.Remove(DatabaseLiterals.FieldSampleState);

                sampleFieldList144.Remove(DatabaseLiterals.FieldSampleName);
                sampleFieldList[DatabaseLiterals.DBVersion144] = sampleFieldList144;

                return sampleFieldList;
            }
            set { }
        }

        /// <summary>
        /// Property to get a smaller version of the alias, for mobile rendering mostly
        /// </summary>
        [Ignore]
        public string SampleAliasLight
        {
            get
            {
                if (SampleName != string.Empty)
                {
                    int aliasNumber = 0;
                    int.TryParse(SampleName.Substring(SampleName.Length - 2), out aliasNumber);

                    if (aliasNumber > 0)
                    {
                        //Trim bunch of zeros
                        string shorterSampleName = SampleName.Substring(SampleName.Length - 7);
                        return shorterSampleName.TrimStart('0');
                    }
                    else
                    {
                        return DatabaseLiterals.picklistNACode;
                    }

                }
                else
                {
                    return DatabaseLiterals.picklistNACode;
                }
            }
            set { }
        }

    }
}
