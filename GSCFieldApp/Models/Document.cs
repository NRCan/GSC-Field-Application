using System;
using System.Collections.Generic;
using System.Linq;
using SQLite;
using GSCFieldApp.Dictionaries;
using System.IO;

namespace GSCFieldApp.Models
{
    [Table(DatabaseLiterals.TableDocument)]
    public class Document
    {
        //Hierarchy
        public string ParentName = DatabaseLiterals.TableLocation;

        [PrimaryKey, AutoIncrement, Column(DatabaseLiterals.FieldDocumentID)]
        public int DocumentID { get; set; }

        [Column(DatabaseLiterals.FieldDocumentName)]
        public string DocumentName { get; set; }

        [Column(DatabaseLiterals.FieldDocumentType)]
        public string DocumentType { get; set; }

        [Column(DatabaseLiterals.FieldDocumentCategory)]
        public string Category { get; set; }

        [Column(DatabaseLiterals.FieldDocumentFileName)]
        public string FileName { get; set; }

        [Column(DatabaseLiterals.FieldDocumentFileNo)]
        public int FileNumber { get; set; }

        [Column(DatabaseLiterals.FieldDocumentDirection)]
        public int Direction { get; set; }

        [Column(DatabaseLiterals.FieldDocumentDescription)]
        public string Description { get; set; }

        [Column(DatabaseLiterals.FieldDocumentHyperlink)]
        public string Hyperlink { get; set; }

        [Column(DatabaseLiterals.FieldDocumentObjLocX)]
        public double? ObjectX { get; set; }

        [Column(DatabaseLiterals.FieldDocumentObjLocY)]
        public double? ObjectY { get; set; }

        [Column(DatabaseLiterals.FieldDocumentScaleDirection)]
        public string ScaleDirection { get; set; }

        [Column(DatabaseLiterals.FieldDocumentStationID)]
        public int? StationID { get; set; }

        [Column(DatabaseLiterals.FieldDocumentSampleID)]
        public int? SampleID { get; set; }

        [Column(DatabaseLiterals.FieldDocumentDrillHoleID)]
        public int? DrillHoleID { get; set; }

        [Column(DatabaseLiterals.FieldDocumentEarthMatID)]
        public int? EarthmatID { get; set; }
        /// <summary>
        /// Soft mandatory field check. User can still create record even if fields are not filled.
        /// Ignore attribute will tell sql not to try to write this field inside the database.
        /// </summary>
        [Ignore]
        public bool isValid
        {
            get
            {
                if ((Category != string.Empty && Category != null && Category != Dictionaries.DatabaseLiterals.picklistNACode) &&
                    (FileName != string.Empty && FileName != null && FileName != Dictionaries.DatabaseLiterals.picklistNACode))
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
        /// A theoric photo path, might not exists
        /// </summary>
        //[Ignore]
        //public string PhotoPath
        //{
        //    get
        //    {
        //        //Access field book folder
        //        Services.DatabaseServices.DataLocalSettings localSetting = new Services.DatabaseServices.DataLocalSettings();
        //        string _fieldbookPath = string.Empty;
        //        if (localSetting.GetSettingValue(Dictionaries.ApplicationLiterals.KeywordFieldProject) != null)
        //        {
        //            _fieldbookPath = Path.Combine(localSetting.GetSettingValue(Dictionaries.ApplicationLiterals.KeywordFieldProject).ToString(), DocumentName + ".jpg");
        //        }


        //        return _fieldbookPath;
        //    }
        //    set { }
        //}

        ///// <summary>
        ///// Will check if theoric photo file really exists. Could prevent code from trying to show a picture.
        ///// </summary>
        //[Ignore]
        //public bool PhotoFileExists
        //{
        //    get
        //    {
        //        if (PhotoPath != string.Empty && PhotoPath != null)
        //        {
        //            return File.Exists(PhotoPath);
        //        }
        //        else
        //        {
        //            return false;
        //        }

        //    }
        //    set { }
        //}


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
                Dictionary<double, List<string>> documentFieldList = new Dictionary<double, List<string>>();
                List<string> documentFieldListDefault = new List<string>();

                documentFieldListDefault.Add(DatabaseLiterals.FieldDocumentID);
                foreach (System.Reflection.PropertyInfo item in this.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ColumnAttribute))).ToList())
                {
                    if (item.CustomAttributes.First().ConstructorArguments.Count() > 0)
                    {
                        documentFieldListDefault.Add(item.CustomAttributes.First().ConstructorArguments[0].ToString().Replace("\\", "").Replace("\"", ""));
                    }

                }

                documentFieldList[DatabaseLiterals.DBVersion] = documentFieldListDefault;

                //Revert shcema 1.8 changes
                List<string> documentFieldList170 = new List<string>();
                documentFieldList170.AddRange(documentFieldListDefault);
                int removeIndex170 = documentFieldListDefault.IndexOf(DatabaseLiterals.FieldDocumentHyperlink);
                documentFieldList170.Insert(removeIndex170 + 1, DatabaseLiterals.FieldDocumentRelatedtableDeprecated);
                documentFieldList170.Insert(removeIndex170 + 2, DatabaseLiterals.FieldDocumentRelatedIDDeprecated);
                documentFieldList170.Remove(DatabaseLiterals.FieldDocumentSampleID);
                documentFieldList170.Remove(DatabaseLiterals.FieldDocumentStationID);
                documentFieldList170.Remove(DatabaseLiterals.FieldDocumentDrillHoleID);
                documentFieldList170.Remove(DatabaseLiterals.FieldDocumentScaleDirection);
                documentFieldList170.Remove(DatabaseLiterals.FieldDocumentEarthMatID);
                documentFieldList[DatabaseLiterals.DBVersion170] = documentFieldList170;

                //Revert shcema 1.7 changes
                //List<string> documentFieldList160 = new List<string>();
                //documentFieldList160.AddRange(documentFieldListDefault);
                //documentFieldList160.Remove(DatabaseLiterals.FieldGenericRowID);
                //documentFieldList[DatabaseLiterals.DBVersion160] = documentFieldList160;

                //Noting has change in 1.6
                documentFieldList[DatabaseLiterals.DBVersion150] = documentFieldList170;

                //Revert schema 1.5 changes. 
                List<string> documentFieldList144 = new List<string>();
                documentFieldList144.AddRange(documentFieldList[DatabaseLiterals.DBVersion150]);
                int removeIndex = documentFieldList144.IndexOf(DatabaseLiterals.FieldDocumentName);
                documentFieldList144.Remove(DatabaseLiterals.FieldDocumentName);
                documentFieldList144.Insert(removeIndex, DatabaseLiterals.FieldDocumentNameDeprecated);
                documentFieldList[DatabaseLiterals.DBVersion144] = documentFieldList144;

                return documentFieldList;
            }
            set { }
        }

    }
}
