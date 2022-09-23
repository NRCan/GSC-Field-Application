using GSCFieldApp.Dictionaries;
using GSCFieldApp.Services.DatabaseServices;
using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSCFieldApp.Models
{
    [Table(DatabaseLiterals.TableMineral)]
    public class Mineral
    {
        [PrimaryKey, Column(DatabaseLiterals.FieldMineralID)]
        public string MineralID { get; set; }

        [Column(DatabaseLiterals.FieldMineralIDName)]
        public string MineralIDName { get; set; }

        [Column(DatabaseLiterals.FieldMineral)]
        public string MineralName { get; set; }

        [Column(DatabaseLiterals.FieldMineralFormHabit)]
        public string MineralFormHabit { get; set; }

        [Column(DatabaseLiterals.FieldMineralOccurence)]
        public string MineralOccur { get; set; }

        [Column(DatabaseLiterals.FieldMineralColour)]
        public string MineralColour { get; set; }

        [Column(DatabaseLiterals.FieldMineralSizeMin)]
        public string MineralSizeMin { get; set; }

        [Column(DatabaseLiterals.FieldMineralSizeMax)]
        public string MineralSizeMax { get; set; }

        [Column(DatabaseLiterals.FieldMineralMode)]
        public string MineralMode { get; set; }

        [Column(DatabaseLiterals.FieldMineralNote)]
        public string MineralNote { get; set; }

        [Column(DatabaseLiterals.FieldMineralParentID)]
        public string MineralParentID { get; set; }

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
                if ((MineralName != string.Empty && MineralName != null && MineralName != Dictionaries.DatabaseLiterals.picklistNACode) &&
                    (MineralMode != string.Empty && MineralMode != null && MineralMode != Dictionaries.DatabaseLiterals.picklistNACode))
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

        [Ignore] //DEPRECATED: To keep full description of the mineral, not only the code.
        public string MineralNameHuman
        {
            get
            {
                if (MineralName != string.Empty && MineralName != null)
                {
                    Vocabularies voc = new Vocabularies();
                    //Get a list of parent (title)
                    string querySelect = "SELECT * FROM " + DatabaseLiterals.TableDictionary + " d";
                    string queryJoin = " JOIN " + DatabaseLiterals.TableDictionaryManager + " m on d." + DatabaseLiterals.FieldDictionaryCodedTheme + " = m." + DatabaseLiterals.FieldDictionaryCodedTheme;
                    string queryWhere = " WHERE d." + DatabaseLiterals.FieldDictionaryCode + " = '" + MineralName + "'";
                    string queryWhere2 = " AND m." + DatabaseLiterals.FieldDictionaryManagerAssignTable + " = '" + DatabaseLiterals.TableMineral + "'";
                    string queryWhere3 = " AND m." + DatabaseLiterals.FieldDictionaryManagerAssignField + " = '" + DatabaseLiterals.FieldMineral + "'";
                    string finalQuery = querySelect + queryJoin + queryWhere + queryWhere2 + queryWhere3;

                    DataAccess dAccess = new DataAccess();
                    List<object> vocRaw = dAccess.ReadTable(voc.GetType(), finalQuery);
                    IEnumerable<Vocabularies> vocTable = vocRaw.Cast<Vocabularies>();

                    if (vocTable.Count() != 0)
                    {
                        return vocTable.First().Description;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                else
                {
                    return string.Empty;
                }
            }

            set { }
        } 
    }
}
