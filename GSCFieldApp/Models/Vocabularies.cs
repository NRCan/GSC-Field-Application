using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite.Net.Attributes;
using GSCFieldApp.Dictionaries;
using System.Collections.ObjectModel;

namespace GSCFieldApp.Models
{
    static class Extensions
    {
        /// <summary>
        /// Extension method for sorting vocabularies, based on the order number.
        /// </summary>
        /// <typeparam name="Vocabularies"></typeparam>
        /// <param name="vocabCollection"></param>
        public static void Sort<Vocabularies>(this ObservableCollection<Vocabularies> vocabCollection) where Vocabularies: IComparable<Vocabularies>
        {
            List<Vocabularies> sorted = vocabCollection.OrderBy(x => x).ToList();
            for (int i = 0; i < sorted.Count; i++)
            {
                vocabCollection.Move(vocabCollection.IndexOf(sorted[i]), i);
            }
        }
    
    }

    /// <summary>
    /// This class should be Dictionaries like in the database schema, but since we are already using this keyword elsehwere in the code I 
    /// changed it to vocabularies
    /// </summary>
    [Table(DatabaseLiterals.TableDictionary)]
    public class Vocabularies: IComparable<Vocabularies>
    {
        [PrimaryKey, Column(DatabaseLiterals.FieldDictionaryTermID)]
        public string TermID { get; set; }

        [Column(DatabaseLiterals.FieldDictionaryCodedTheme)]
        public string CodedTheme { get; set; }

        [Column(DatabaseLiterals.FieldDictionaryRelatedTo)]
        public string RelatedTo { get; set; }

        [Column(DatabaseLiterals.FieldDictionaryCode)]
        public string Code { get; set; }

        [Column(DatabaseLiterals.FieldDictionaryDescription)]
        public string Description { get; set; }

        [Column(DatabaseLiterals.FieldDictionaryDescriptionFR)]
        public string DescriptionFR { get; set; }

        [Column(DatabaseLiterals.FieldDictionaryOrder)]
        public double Order { get; set; }

        [Column(DatabaseLiterals.FieldDictionaryDefault)]
        public string DefaultValue { get; set; }

        [Column(DatabaseLiterals.FieldDictionaryCreator)]
        public string Creator { get; set; }

        [Column(DatabaseLiterals.FieldDictionaryCreatorDate)]
        public string CreatorDate { get; set; }

        [Column(DatabaseLiterals.FieldDictionaryEditor)]
        public string Editor { get; set; }

        [Column(DatabaseLiterals.FieldDictionaryEditorDate)]
        public string EditorDate { get; set; }

        [Column(DatabaseLiterals.FieldDictionaryRemarks)]
        public string Remarks { get; set; }

        [Column(DatabaseLiterals.FieldDictionarySymbol)]
        public string Symbol { get; set; }

        [Column(DatabaseLiterals.FieldDictionaryEditable)]
        public string Editable { get; set; }

        [Column(DatabaseLiterals.FieldDictionaryVisible)]
        public string Visibility { get; set; }

        [Column(DatabaseLiterals.FieldDictionaryVersion)]
        public string Version { get; set; }

        /// <summary>
        /// CompareTo method to filter vocabularies by order number
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public int CompareTo(Vocabularies x)
        {
            return Order.CompareTo(x.Order);
        }

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
                Dictionary<double, List<string>> vocabFieldList = new Dictionary<double, List<string>>();
                List<string> vocabFieldListDefault = new List<string>();

                vocabFieldListDefault.Add(DatabaseLiterals.FieldDictionaryTermID);
                foreach (System.Reflection.PropertyInfo item in this.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ColumnAttribute))).ToList())
                {
                    if (item.CustomAttributes.First().ConstructorArguments.Count() > 0)
                    {
                        vocabFieldListDefault.Add(item.CustomAttributes.First().ConstructorArguments[0].ToString().Replace("\\", "").Replace("\"", ""));
                    }

                }

                vocabFieldList[DatabaseLiterals.DBVersion] = vocabFieldListDefault;

                //Revert schema 1.5 changes. 
                List<string> vocabFieldList144 = new List<string>();
                vocabFieldList144.AddRange(vocabFieldListDefault);
                vocabFieldList144.Remove(DatabaseLiterals.FieldDictionaryVersion);
                vocabFieldList[DatabaseLiterals.DBVersion144] = vocabFieldList144;

                return vocabFieldList;
            }
            set { }
        }
    }
}
