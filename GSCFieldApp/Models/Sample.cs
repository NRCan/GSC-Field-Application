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
    [Table(DatabaseLiterals.TableSample)]
    public class Sample
    {

        [PrimaryKey, Column(DatabaseLiterals.FieldSampleID)]
        public string SampleID { get; set; }

        [Column(DatabaseLiterals.FieldSampleName)]
        public string SampleName { get; set; }

        [Column(DatabaseLiterals.FieldSampleType)]
        public string SampleType { get; set; }

        [Column(DatabaseLiterals.FieldSamplePurpose)]
        public string SamplePurpose { get; set; }

        [Column(DatabaseLiterals.FieldSampleFormat)]
        public string SampleFormat { get; set; }

        [Column(DatabaseLiterals.FieldSampleAzim)]
        public string SampleAzim { get; set; }

        [Column(DatabaseLiterals.FieldSampleDipPlunge)]
        public string SampleDiplunge { get; set; }

        [Column(DatabaseLiterals.FieldSampleSurface)]
        public string SampleSurface { get; set; }

        [Column(DatabaseLiterals.FieldSampleNotes)]
        public string SampleNotes { get; set; }

        [Column(DatabaseLiterals.FieldCurationID)]
        public string SampleCuration { get; set; }

        [Column(DatabaseLiterals.FieldSampleManagementID)]
        public string SampleSMID { get; set; }

        [Column(DatabaseLiterals.FieldSampleEarthmatID)]
        public string SampleEarthmatID { get; set; }

        [Column(DatabaseLiterals.FieldSampleQuality)]
        public string SampleQuality { get; set; }

        [Column(DatabaseLiterals.FieldSampleHorizon)]
        public string SampleHorizon { get; set; }

        [Column(DatabaseLiterals.FieldSampleDepthMin)]
        public string SampleDepthMin { get; set; }

        [Column(DatabaseLiterals.FieldSampleDepthMax)]
        public string SampleDepthMax { get; set; }

        [Column(DatabaseLiterals.FieldSampleDuplicate)]
        public string SampleDuplicate { get; set; }

        [Column(DatabaseLiterals.FieldSampleDuplicateName)]
        public string SampleDuplicateName { get; set; }

        [Column(DatabaseLiterals.FieldSampleState)]
        public string SampleState { get; set; }


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
        public List<string> getFieldList
        {
            get
            {
                List<string> sampleFieldList = new List<string>();
                sampleFieldList.Add(DatabaseLiterals.FieldSampleID);
                foreach (System.Reflection.PropertyInfo item in this.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ColumnAttribute))).ToList())
                {
                    if (item.CustomAttributes.First().ConstructorArguments.Count() > 0)
                    {
                        sampleFieldList.Add(item.CustomAttributes.First().ConstructorArguments[0].ToString().Replace("\\", "").Replace("\"", ""));
                    }

                }

                return sampleFieldList;
            }
            set { }
        }
    }
}
