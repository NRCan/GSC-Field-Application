using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite.Net.Attributes;
using GSCFieldApp.Dictionaries;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using System.IO;
using Windows.UI.Xaml;

namespace GSCFieldApp.Models
{
    [Table(DatabaseLiterals.TableDocument)]
    public class Document
    {
        //Hierarchy
        public string ParentName = DatabaseLiterals.TableLocation;

        [PrimaryKey, Column(DatabaseLiterals.FieldDocumentID)]
        public string DocumentID { get; set; }

        [Column(DatabaseLiterals.FieldDocumentName)]
        public string DocumentName { get; set; }

        [Column(DatabaseLiterals.FieldDocumentType)]
        public string DocumentType { get; set; }

        [Column(DatabaseLiterals.FieldDocumentCategory)]
        public string Category { get; set; }

        [Column(DatabaseLiterals.FieldDocumentFileName)]
        public string FileName { get; set; }

        [Column(DatabaseLiterals.FieldDocumentFileNo)]
        public string FileNumber { get; set; }

        [Column(DatabaseLiterals.FieldDocumentDirection)]
        public string Direction { get; set; }

        [Column(DatabaseLiterals.FieldDocumentDescription)]
        public string Description { get; set; }

        [Column(DatabaseLiterals.FieldDocumentHyperlink)]
        public string Hyperlink { get; set; }

        [Column(DatabaseLiterals.FieldDocumentRelatedtable)]
        public string RelatedTable { get; set; }

        [Column(DatabaseLiterals.FieldDocumentRelatedID)]
        public string RelatedID { get; set; }

        [Column(DatabaseLiterals.FieldDocumentObjLocX)]
        public string ObjectX { get; set; }

        [Column(DatabaseLiterals.FIeldDocumentObjLocY)]
        public string ObjectY { get; set; }


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
        [Ignore]
        public string PhotoPath
        {
            get
            {
                //Access field book folder
                Services.DatabaseServices.DataLocalSettings localSetting = new Services.DatabaseServices.DataLocalSettings();
                string _fieldbookPath = string.Empty;
                if (localSetting.GetSettingValue(Dictionaries.ApplicationLiterals.KeywordFieldProject) != null)
                {
                    _fieldbookPath = Path.Combine(localSetting.GetSettingValue(Dictionaries.ApplicationLiterals.KeywordFieldProject).ToString(), DocumentName + ".jpg");
                }
                
                
                return _fieldbookPath;
            }
            set { }
        }

        /// <summary>
        /// Will check if theoric photo file really exists. Could prevent code from trying to show a picture.
        /// </summary>
        [Ignore]
        public bool PhotoFileExists
        {
            get
            {
                if (PhotoPath != string.Empty && PhotoPath != null)
                {
                    return File.Exists(PhotoPath);
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
                List<string> documentFieldList = new List<string>();
                documentFieldList.Add(DatabaseLiterals.FieldDocumentID);
                foreach (System.Reflection.PropertyInfo item in this.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ColumnAttribute))).ToList())
                {
                    if (item.CustomAttributes.First().ConstructorArguments.Count() > 0)
                    {
                        documentFieldList.Add(item.CustomAttributes.First().ConstructorArguments[0].ToString().Replace("\\", "").Replace("\"", ""));
                    }

                }

                return documentFieldList;
            }
            set { }
        }

    }
}
