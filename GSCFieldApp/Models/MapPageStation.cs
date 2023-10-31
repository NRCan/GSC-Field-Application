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
        [Column(DatabaseLiterals.FieldStationID)]
        public int? StationID { get; set; }

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

        [Column(DatabaseLiterals.FieldDrillID)]
        public int? DrillID { get; set; }

        [Column(DatabaseLiterals.FieldDrillIDName)]
        public string DrillIDName { get; set; }

        [Column(DatabaseLiterals.FieldDrillName)]
        public string DrillName { get; set; }
    }
}
