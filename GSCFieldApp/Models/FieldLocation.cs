using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;
using SQLite.Net.Attributes;
using GSCFieldApp.Dictionaries;

namespace GSCFieldApp.Models
{
    [Table(DatabaseLiterals.TableLocation)]
    public class FieldLocation: BindableBase
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        [PrimaryKey, Column(DatabaseLiterals.FieldLocationID)]
        public string LocationID { get; set; }

        /// <summary>
        /// Gets or sets the first name.
        /// </summary> 
        [Column(DatabaseLiterals.FieldLocationAlias)]
        public string LocationAlias { get; set; }


        [Column(DatabaseLiterals.FieldLocationEasting)]
        public double? LocationEasting { get; set; }
        [Column(DatabaseLiterals.FieldLocationNorthing)]
        public double? LocationNorthing { get; set; }

        [Column(DatabaseLiterals.FieldLocationLatitude)]
        public double LocationLat { get; set; }

        [Column(DatabaseLiterals.FieldLocationLongitude)]
        public double LocationLong { get; set; }

        [Column(DatabaseLiterals.FieldLocationDatum)]
        public string LocationDatum { get; set; }

        [Column(DatabaseLiterals.FieldStationElevation)]
        public double LocationElev { get; set; }

        [Column(DatabaseLiterals.FieldLocationElevationMethod)]
        public string LocationElevMethod { get; set; }

        [Column(DatabaseLiterals.FieldLocationElevAccuracy)]
        public double? LocationElevationAccuracy { get; set; }

        [Column(DatabaseLiterals.FieldLocationEntryType)]
        public string LocationEntryType { get; set; }

        [Column(DatabaseLiterals.FieldLocationPDOP)]
        public double? LocationPDOP { get; set; }

        [Column(DatabaseLiterals.FieldLocationErrorMeasure)]
        public double LocationErrorMeasure { get; set; }

        [Column(DatabaseLiterals.FieldLocationErrorMeasureType)]
        public string LocationErrorMeasureType { get; set; }

        [Column(DatabaseLiterals.FieldLocationNotes)]
        public string LocationNotes { get; set; }

        [Column(DatabaseLiterals.FieldLocationReportLink)]
        public string LocationReportLink { get; set; }

        [Column(DatabaseLiterals.FieldLocationMetaID)]
        public string MetaID { get; set; }

        public string LocationTableName
        {
            get
            {
                return DatabaseLiterals.TableLocation;
            }
        }

        /// <summary>
        /// Soft mandatory field check. User can still create record even if fields are not filled.
        /// Ignore attribute will tell sql not to try to write this field inside the database.
        /// </summary>
        [Ignore]
        public bool isValid
        {
            get
            {
                if ((LocationLat != 0 && LocationLong != 0 && Math.Abs(LocationLat) <= 90 && Math.Abs(LocationLong) <= 360))
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

        /// <summary>
        /// A list of all possible fields
        /// </summary>
        [Ignore]
        public List<string> getFieldList
        {
            get
            {
                List<string> locationFieldList = new List<string>();
                locationFieldList.Add(DatabaseLiterals.FieldLocationID);
                foreach (System.Reflection.PropertyInfo item in this.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ColumnAttribute))).ToList())
                {
                    if (item.CustomAttributes.First().ConstructorArguments.Count() > 0)
                    {
                        locationFieldList.Add(item.CustomAttributes.First().ConstructorArguments[0].ToString().Replace("\\", "").Replace("\"", ""));
                    }

                }

                return locationFieldList;
            }
            set { }
        }

    }
}
