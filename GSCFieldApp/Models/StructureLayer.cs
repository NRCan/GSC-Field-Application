using GSCFieldApp.Dictionaries;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSCFieldApp.Models
{
    public class StructureLayer
    {
        [Column(DatabaseLiterals.FieldLocationID)]
        public int LocationID { get; set; }
        [Column(DatabaseLiterals.FieldGenericGeometry)]
        public byte[] LocationGeometry { get; set; }
        [Column(DatabaseLiterals.FieldStructureID)]
        public int StructureID { get; set; }
        [Column(DatabaseLiterals.FieldStructureName)]
        public string StructureName { get; set; }
        [Column(DatabaseLiterals.FieldStructureClass)]
        public string StructureClass { get; set; }
        [Column(DatabaseLiterals.FieldStructureType)]
        public string StructureType { get; set; }
        [Column(DatabaseLiterals.FieldStructureDetail)]
        public string StructureDetail { get; set; }
        [Column(DatabaseLiterals.FieldStructureAzimuth)]
        public int? StructureAzimuth { get; set; }
        [Column(DatabaseLiterals.FieldStructureRelated)]
        public int? StructureRelated { get; set; }
        [Column(DatabaseLiterals.FieldStructureDip)]
        public int? StructureDipPlunge { get; set; }
        [Column(DatabaseLiterals.FieldStructureLayerPriorrity)]
        public int PriorityOrder { get; set; }
    }
}
