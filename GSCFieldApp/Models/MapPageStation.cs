using GSCFieldApp.Dictionaries;
using SQLite;

namespace GSCFieldApp.Models
{
    /// <summary>
    /// Special class that maps some station fields and some location fields.
    /// Was created to speed up graph build for map page.
    /// </summary>
    [Table(DatabaseLiterals.TableStation)]
    public class MapPageStation
    {
        [PrimaryKey, Column(DatabaseLiterals.FieldStationID)]
        public string StationID { get; set; }

        [Column(DatabaseLiterals.FieldStationAlias)]
        public string StationAlias { get; set; }

        [Column(DatabaseLiterals.FieldStationVisitDate)]
        public string StationVisitDate { get; set; }

        [Column(DatabaseLiterals.FieldStationVisitTime)]
        public string StationVisitTime { get; set; }

        [Column(DatabaseLiterals.FieldStationObsType)]
        public string StationObsType { get; set; }

        [Column(DatabaseLiterals.FieldLocationID)]
        public string LocationID { get; set; }

        [Column(DatabaseLiterals.FieldLocationLongitude)]
        public double LocationLong { get; set; }

        [Column(DatabaseLiterals.FieldLocationLatitude)]
        public double LocationLat { get; set; }

        [Column(DatabaseLiterals.FieldLocationDatum)]
        public string LocationDatum { get; set; }
    }
}
