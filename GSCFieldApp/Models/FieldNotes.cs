namespace GSCFieldApp.Models
{

    /// <summary>
    /// This class is mainly used to show information in the report page.
    /// In order to know what to edit, an item needs to know about it's parents and have access to some 
    /// field values. This class is intended to play this role.
    /// </summary>
    public class FieldNotes
    {
        #region PROPERTIES
        public Models.FieldLocation location { get; set; }

        public Models.Station station { get; set; }

        public Models.Metadata metadata { get; set; } 

        public Models.EarthMaterial earthmat { get; set; }

        public Models.Sample sample { get; set; }

        public Models.Document document { get; set; }

        public Models.Structure structure { get; set; }

        public Models.Fossil fossil { get; set; }
        public Models.Paleoflow paleoflow { get; set; }

        public Models.Mineral mineral { get; set; }

        public Models.MineralAlteration mineralAlteration { get; set; }

        public Models.Environment environment { get; set; }

        //Define here are the properties that can be used for edit and delete operation
        public string GenericID { get; set; }
        public string GenericTableName { get; set; }
        public string GenericFieldID { get; set; }

        public string GenericAliasName { get; set; }

        //Define here are the properties that can be used for add operation. TODO ParentTableName could be deduce from the relation inside the database.
        public string ParentID { get; set; }
        public string ParentTableName { get; set; }

        public string MainID { get; set; } //Basically the original location ID for all records

        public bool _isValid = true;

        public bool isValid
        {
            get
            {
                return _isValid;
            }
            set { _isValid = value;}
        }

        #endregion

        public FieldNotes()
        {
            location = new Models.FieldLocation(); //Init as a new class
            station = new Models.Station(); 
            metadata = new Models.Metadata(); 
            earthmat = new Models.EarthMaterial(); 
            sample = new Models.Sample(); 
            document = new Models.Document(); 
            structure = new Models.Structure();
            paleoflow = new Models.Paleoflow();
            fossil = new Models.Fossil();
            mineral = new Models.Mineral();
            mineralAlteration = new Models.MineralAlteration();
            environment = new Models.Environment();
        }

        public bool Validate()
        {
            if ((station.StationID != null && !station.isValid) || (earthmat.EarthMatID != null && !earthmat.isValid) || (sample.SampleID != null && !sample.isValid) ||
            (fossil.FossilID != null && !fossil.isValid) || (document.DocumentID != null && !document.isValid) || (structure.StructureID != null && !structure.isValid) ||
            (paleoflow.PFlowID != null && !paleoflow.isValid))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

    }
}
