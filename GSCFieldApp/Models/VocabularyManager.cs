using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite.Net.Attributes;
using GSCFieldApp.Dictionaries;

namespace GSCFieldApp.Models
{
    [Table(DatabaseLiterals.TableDictionaryManager)]
    public class VocabularyManager
    {

        [PrimaryKey, Column(DatabaseLiterals.FieldDictionaryManagerLinkID)]
        public string ThemeID { get; set; }

        [Column(DatabaseLiterals.FieldDictionaryManagerCodedTheme)]
        public string ThemeName { get; set; }

        [Column(DatabaseLiterals.FieldDictionaryManagerSpecificTo)]
        public string ThemeProjectType { get; set; }

        [Column(DatabaseLiterals.FieldDictionaryManagerAssignTable)]
        public string ThemeTable { get; set; }

        [Column(DatabaseLiterals.FieldDictionaryManagerCodedThemeDescription)]
        public string ThemeNameDesc { get; set; }

        [Column(DatabaseLiterals.FieldDictionaryManagerOutputFile)]
        public string ThemeOutputFile { get; set; }

        [Column(DatabaseLiterals.FieldDictionaryManagerAssignField)]
        public string ThemeField { get; set; }

        [Column(DatabaseLiterals.FieldDictionaryManagerEditable)]
        public string ThemeEditable { get; set; }

        [Column(DatabaseLiterals.FieldDictionaryManagerVersion)]
        public string Version { get; set; }

        /// <summary>
        /// A list of all possible fields
        /// </summary>
        [Ignore]
        public List<string> getFieldList
        {
            get
            {
                List<string> vocabManagerFieldList = new List<string>();
                vocabManagerFieldList.Add(DatabaseLiterals.FieldDictionaryManagerLinkID);
                foreach (System.Reflection.PropertyInfo item in this.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ColumnAttribute))).ToList())
                {
                    if (item.CustomAttributes.First().ConstructorArguments.Count() > 0)
                    {
                        vocabManagerFieldList.Add(item.CustomAttributes.First().ConstructorArguments[0].ToString().Replace("\\", "").Replace("\"", ""));
                    }

                }

                return vocabManagerFieldList;
            }
            set { }
        }

    }
}
