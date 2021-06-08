using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSCFieldApp.Dictionaries;
using SQLite.Net.Attributes;

namespace GSCFieldApp.Models
{
    [Table(DatabaseLiterals.TableMineralAlteration)]
    public class MineralAlteration
    {
        [PrimaryKey, Column(DatabaseLiterals.FieldMineralAlterationID)]
        public string MAID { get; set; }

        [Column(DatabaseLiterals.FieldMineralAlterationName)]
        public string MAName { get; set; }

        [Column(DatabaseLiterals.FieldMineralAlteration)]
        public string MAMA { get; set; }

        [Column(DatabaseLiterals.FieldMineralAlterationUnit)]
        public string MAUnit { get; set; }
        [Column(DatabaseLiterals.FieldMineralAlterationMineral)]
        public string MAMineral { get; set; }

        [Column(DatabaseLiterals.FieldMineralAlterationMode)]
        public string MAMode { get; set; }

        [Column(DatabaseLiterals.FieldMineralAlterationDistrubute)]
        public string MADistribute { get; set; }

        [Column(DatabaseLiterals.FieldMineralAlterationNotes)]
        public string MANotes { get; set; }

        [Column(DatabaseLiterals.FieldMineralAlterationRelTable)]
        public string MAParentTable { get; set; }

        [Column(DatabaseLiterals.FieldMineralAlterationRelID)]
        public string MAParentID { get; set; }

        //Hierarchy
        public string ParentName = DatabaseLiterals.TableStation;

        /// <summary>
        /// Soft mandatory field check. User can still create record even if fields are not filled.
        /// Ignore attribute will tell sql not to try to write this field inside the database.
        /// </summary>
        [Ignore]
        public bool isValid
        {
            get
            {
                if ((MAMA != string.Empty && MAMA != null && MAMA != Dictionaries.DatabaseLiterals.picklistNACode) &&
                    (MAMineral != string.Empty && MAMineral != null && MAMineral != Dictionaries.DatabaseLiterals.picklistNACode))
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
    }
}
