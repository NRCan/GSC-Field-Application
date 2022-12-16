using GSCFieldApp.Dictionaries;
using SQLite;

namespace GSCFieldApp.Models
{
    [Table(DatabaseLiterals.TablePFlow)]
    public class Paleoflow
    {
        [PrimaryKey, Column(DatabaseLiterals.FieldPFlowID)]
        public string PFlowID { get; set; }

        [Column(DatabaseLiterals.FieldPFlowName)]
        public string PFlowName { get; set; }

        [Column(DatabaseLiterals.FieldPFlowClass)]
        public string PFlowClass { get; set; }

        [Column(DatabaseLiterals.FieldPFlowSense)]
        public string PFlowSense{ get; set; }

        [Column(DatabaseLiterals.FieldPFlowFeature)]
        public string PFlowFeature { get; set; }

        [Column(DatabaseLiterals.FieldPFlowMethod)]
        public string PFlowMethod { get; set; }

        [Column(DatabaseLiterals.FieldPFlowAzimuth)]
        public string PFlowAzimuth { get; set; }

        [Column(DatabaseLiterals.FieldPFlowMainDir)]
        public string PFlowMainDir { get; set; }

        [Column(DatabaseLiterals.FieldPFlowRelage)]
        public string PFlowRelAge { get; set; }

        [Column(DatabaseLiterals.FieldPFlowDip)]
        public string PFlowDip { get; set; }

        [Column(DatabaseLiterals.FieldPFlowNumIndic)]
        public string PFlowNumIndic { get; set; }

        [Column(DatabaseLiterals.FieldPFlowDefinition)]
        public string PFlowDefinition { get; set; }

        [Column(DatabaseLiterals.FieldPFlowRelation)]
        public string PFlowRelation { get; set; }

        [Column(DatabaseLiterals.FieldPFlowBedsurf)]
        public string PFlowBedsurf { get; set; }

        [Column(DatabaseLiterals.FieldPFlowConfidence)]
        public string PFlowConfidence { get; set; }

        [Column(DatabaseLiterals.FieldPFlowNotes)]
        public string PFlowNotes { get; set; }

        [Column(DatabaseLiterals.FieldPFlowParentID)]
        public string PFlowParentID { get; set; }

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
                if ((PFlowClass != string.Empty && PFlowClass != null && PFlowClass != Dictionaries.DatabaseLiterals.picklistNACode) &&
                    (PFlowSense != string.Empty && PFlowSense != null && PFlowSense != Dictionaries.DatabaseLiterals.picklistNACode))
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
