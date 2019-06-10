using GSCFieldApp.Dictionaries;
using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSCFieldApp.Models
{
    [Table(DatabaseLiterals.TableFossil)]
    public class Fossil
    {
        [PrimaryKey, Column(DatabaseLiterals.FieldFossilID)]
        public string FossilID { get; set; }

        [Column(DatabaseLiterals.FieldFossilName)]
        public string FossilIDName { get; set; }

        [Column(DatabaseLiterals.FieldFossilType)]
        public string FossilType { get; set; }

        [Column(DatabaseLiterals.FieldFossilNote)]
        public string FossilNote { get; set; }

        [Column(DatabaseLiterals.FieldFossilParentID)]
        public string FossilParentID { get; set; }

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
                if ((FossilType != string.Empty && FossilType != null && FossilType != Dictionaries.DatabaseLiterals.picklistNACode))
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
