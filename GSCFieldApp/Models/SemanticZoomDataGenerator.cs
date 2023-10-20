using GSCFieldApp.Dictionaries;
using GSCFieldApp.Services.DatabaseServices;
using System.Collections.Generic;
using System.Linq;
using static GSCFieldApp.Dictionaries.DatabaseLiterals;

namespace GSCFieldApp.Models
{
    public class SemanticDataGenerator
    {
        private static List<SemanticData> _data;
        private static List<SemanticData> _dataLithology;
        private static List<SemanticData> _dataStructures;
        /// <summary>
        /// Will be used to triggered a refresh on the semantic, else the code won't bother gathering back all the data, speeding up process.
        /// </summary>
        public static string lastAssignTable { get; set; }

        public static List<SemanticDataGroup> GetGroupedData(bool refresh, string inAssignTable, string inParentFieldName, string inChildFieldName)
        {
            if (inAssignTable == DatabaseLiterals.TableEarthMat)
            {
                _data = _dataLithology;
            }
            if (inAssignTable == DatabaseLiterals.TableStructure)
            {
                _data = _dataStructures;
            }

            if (_data == null || refresh)
            {
                GenerateData(inAssignTable, inParentFieldName, inChildFieldName);

                //Update past table
                lastAssignTable = inAssignTable;
            }
                

            return _data.GroupBy(d => d.Title, 
                (key, items) => new SemanticDataGroup() { Name = key, Items = items.ToList() }).ToList();
        }

        /// <summary>
        /// Will generate the data for the semantic zoom
        /// NOTE: Only lithology is treated for now.
        /// </summary>
        private static void GenerateData(string inAssignTable, string inParentFieldName, string inChildFieldName)
        {
            DataAccess dAccess = new DataAccess();

            _data = new List<SemanticData>();

            if (inAssignTable == DatabaseLiterals.TableEarthMat)
            {
                _dataLithology = new List<SemanticData>();
            }
            if (inAssignTable == DatabaseLiterals.TableStructure)
            {
                _dataStructures = new List<SemanticData>();
            }

            //Get a list of parent (title)
            Vocabularies voc = new Vocabularies();
            
            string finalQueryTitle = string.Empty;

            string querySelect = "SELECT * FROM " + TableDictionary;
            string queryJoin = " JOIN " + TableDictionaryManager + " ON " + TableDictionary + "." + 
                FieldDictionaryCodedTheme + " = " + TableDictionaryManager + "." + FieldDictionaryManagerCodedTheme;
            string queryAssignTable = " WHERE " + TableDictionaryManager + "." + FieldDictionaryManagerAssignTable + " = '" + inAssignTable + "'";
            string queryAssignFieldChild = " WHERE " + TableDictionaryManager + "." + FieldDictionaryManagerAssignField + " = '" + inChildFieldName + "'";

            string queryAssignFieldParent = " AND " + TableDictionaryManager + "." + FieldDictionaryManagerAssignField + " = '" + inParentFieldName + "'";
            string queryVisibility = " AND " + TableDictionary + "." + FieldDictionaryVisible + " = '" + boolYes + "'";
            string queryOrder = " ORDER BY " + TableDictionary + "." + FieldDictionaryOrder + " ASC";

            //In case there isn't parent and list still should display something.
            if (inParentFieldName != string.Empty)
            {

                finalQueryTitle = querySelect + queryJoin + queryAssignTable + queryAssignFieldParent + queryVisibility + queryOrder;
            }
            else
            {
                string queryProjectType = " AND " + TableDictionaryManager + "." + FieldDictionaryManagerSpecificTo +
                    " = '" + DatabaseLiterals.ApplicationThemeSurficial + "'";

                finalQueryTitle = querySelect + queryJoin + queryAssignTable + queryAssignFieldChild.Replace("WHERE", "AND") + 
                    queryProjectType + queryVisibility + queryOrder;
            }
            
            

            List<object> vocRaw = dAccess.ReadTable(voc.GetType(), finalQueryTitle);
            IEnumerable<Vocabularies> vocTable = vocRaw.Cast<Vocabularies>();

            //Iterate through parent and get a list of children
            foreach (Vocabularies sVocab in vocTable)
            {
                if (inParentFieldName != string.Empty)
                {
                    //Get detail from given title
                    string queryRelatedTo = " AND " + TableDictionary + "." + FieldDictionaryRelatedTo + " = '" + sVocab.Code + "'";
                    string finaleQueryDetail = querySelect + queryJoin + queryAssignFieldChild + queryRelatedTo + queryVisibility + queryOrder;
                    List<object> vocDetailRaw = dAccess.ReadTable(voc.GetType(), finaleQueryDetail);
                    IEnumerable<Vocabularies> vocDetailTable = vocDetailRaw.Cast<Vocabularies>();

                    foreach (Vocabularies dVocab in vocDetailTable)
                    {
                        //Build semantic data
                        _data.Add(new SemanticData(dVocab.RelatedTo, dVocab.Description));

                        if (inAssignTable == DatabaseLiterals.TableEarthMat)
                        {
                            _dataLithology.Add(new SemanticData(dVocab.RelatedTo, dVocab.Description));
                        }
                        if (inAssignTable == DatabaseLiterals.TableStructure)
                        {
                            _dataStructures.Add(new SemanticData(dVocab.RelatedTo, dVocab.Description));
                        }

                    }
                }
                else
                {
                    _dataLithology.Add(new SemanticData("Surficial test", sVocab.Description));
                }
            }

        }
    }
}
