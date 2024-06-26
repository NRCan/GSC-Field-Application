﻿namespace GSCFieldApp.Models
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

        public Models.EnvironmentModel environment { get; set; }

        public Models.DrillHole drillHoles { get; set; }

        //Define here are the properties that can be used for edit and delete operation
        public int GenericID { get; set; }
        public string GenericTableName { get; set; }
        //Name of the field that holds the ids
        public string GenericFieldID { get; set; }

        public string GenericAliasName { get; set; }

        //Define here are the properties that can be used for add operation. TODO ParentTableName could be deduce from the relation inside the database.
        public int ParentID { get; set; }

        public string ParentTableName { get; set; }

        public int MainID { get; set; } //Basically the original location ID for all records

        public bool _isValid = true;

        public bool isValid
        {
            get
            {
                return _isValid;
            }
            set { _isValid = value; }
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
            environment = new Models.EnvironmentModel();
            drillHoles = new DrillHole();
        }

        public bool Validate()
        {
            if ((station.StationID != 0 && !station.isValid) || (earthmat.EarthMatID != 0 && !earthmat.isValid) || (sample.SampleID != 0 && !sample.isValid) ||
            (fossil.FossilID != 0 && !fossil.isValid) || (document.DocumentID != 0 && !document.isValid) || (structure.StructureID != 0 && !structure.isValid) ||
            (paleoflow.PFlowID != 0 && !paleoflow.isValid))
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
