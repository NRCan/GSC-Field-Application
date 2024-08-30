using System;
using System.Collections.Generic;
using System.Linq;
using SQLite;
using GSCFieldApp.Dictionaries;

namespace GSCFieldApp.Models
{
    [Table(DatabaseLiterals.TableDictionaryManager)]
    public class VocabularyManager
    {
        [PrimaryKey, AutoIncrement, Column(DatabaseLiterals.FieldGenericRowID)]
        public int rowid { get; set; }

        [Column(DatabaseLiterals.FieldDictionaryManagerLinkID)]
        public string ThemeID { get; set; }

        [Column(DatabaseLiterals.FieldDictionaryManagerCodedTheme)]
        public string ThemeCodedTheme { get; set; }

        [Column(DatabaseLiterals.FieldDictionaryManagerSpecificTo)]
        public string ThemeSpecificTo { get; set; }

        [Column(DatabaseLiterals.FieldDictionaryManagerAssignTable)]
        public string ThemeAssignTable { get; set; }

        [Column(DatabaseLiterals.FieldDictionaryManagerCodedThemeDescription)]
        public string ThemeCodeThemeDesc { get; set; }

        [Column(DatabaseLiterals.FieldDictionaryManagerOutputFile)]
        public string ThemeOutputFile { get; set; }

        [Column(DatabaseLiterals.FieldDictionaryManagerAssignField)]
        public string ThemeAssignField { get; set; }

        [Column(DatabaseLiterals.FieldDictionaryManagerEditable)]
        public string ThemeEditable { get; set; }

        [Column(DatabaseLiterals.FieldDictionaryManagerVersion)]
        public double Version { get; set; }

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
                Dictionary<double, List<string>> vocabManagerFieldList = new Dictionary<double, List<string>>();
                List<string> vocabManagerFieldListDefault = new List<string>();

                vocabManagerFieldListDefault.Add(DatabaseLiterals.FieldGenericRowID);
                foreach (System.Reflection.PropertyInfo item in this.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ColumnAttribute))).ToList())
                {
                    if (item.CustomAttributes.First().ConstructorArguments.Count() > 0)
                    {
                        vocabManagerFieldListDefault.Add(item.CustomAttributes.First().ConstructorArguments[0].ToString().Replace("\\", "").Replace("\"", ""));
                    }

                }

                vocabManagerFieldList[DatabaseLiterals.DBVersion] = vocabManagerFieldListDefault;

                //Revert shcema 1.7 changes
                List<string> vocMaFieldList160 = new List<string>();
                vocMaFieldList160.AddRange(vocabManagerFieldListDefault);
                vocMaFieldList160.Remove(DatabaseLiterals.FieldGenericRowID);
                vocabManagerFieldList[DatabaseLiterals.DBVersion160] = vocMaFieldList160;


                //Revert schema 1.5 changes. 
                List<string> vocabFieldList144 = new List<string>();
                vocabFieldList144.AddRange(vocMaFieldList160);
                vocabFieldList144.Remove(DatabaseLiterals.FieldDictionaryManagerVersion);
                vocabManagerFieldList[DatabaseLiterals.DBVersion144] = vocabFieldList144;

                return vocabManagerFieldList;
            }
            set { }
        }

    }
}
