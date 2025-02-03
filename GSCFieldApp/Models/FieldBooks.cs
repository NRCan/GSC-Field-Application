using CommunityToolkit.Mvvm.Input;
using static GSCFieldApp.Dictionaries.DatabaseLiterals;
using GSCFieldApp.Services.DatabaseServices;
using GSCFieldApp.Views;
using NetTopologySuite.IO;
using GSCFieldApp.Dictionaries;

namespace GSCFieldApp.Models
{
    public partial class FieldBooks
    {
        #region PROPERTIES

        //Define here are the properties that can be used for edit and delete operation
        public string GeologistGeolcode { get; set; } //Since geologist is not mandatory, a mix of geologist and geolcode will always give a value

        public string StationNumber { get; set; }
        public string StationLastEntered { get; set; }
        public string ProjectPath { get; set; }
        public string ProjectDBPath { get; set; }
        public string CreateDate { get; set; }
        public Metadata metadataForProject { get; set; }
        public bool isSelected { get; set; }
        public bool isValid 
        {
            get
            {
                if (metadataForProject != null && metadataForProject.VersionSchema != null)
                {
                    double version = 0.0;
                    double.TryParse(metadataForProject.VersionSchema, System.Globalization.CultureInfo.InvariantCulture, out version);

                    if (!double.IsNaN(version) && version == DatabaseLiterals.DBVersion)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }
                else
                {
                    return false;
                }
            }
            set { }
        }

        private DataAccess da = new DataAccess();

        #endregion

        public FieldBooks()
        {
            metadataForProject = new Metadata();
        }

    }
}
