﻿using System;
using System.Collections.Generic;
using System.Linq;
using SQLite;
using GSCFieldApp.Dictionaries;

namespace GSCFieldApp.Models
{
    [Table(DatabaseLiterals.TableLocation)]
    public class FieldLocation
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        [Column(DatabaseLiterals.FieldLocationID), PrimaryKey, AutoIncrement]
        public int LocationID { get; set; }

        [Column(DatabaseLiterals.FieldGenericGeometry)]
        public byte[] LocationGeometry { get; set; }

        [Column(DatabaseLiterals.FieldLocationAlias)]
        public string LocationAlias { get; set; }

        [Column(DatabaseLiterals.FieldLocationEasting)]
        public double? LocationEasting { get; set; }

        [Column(DatabaseLiterals.FieldLocationNorthing)]
        public double? LocationNorthing { get; set; }

        [Column(DatabaseLiterals.FieldLocationEPSGProj)]
        public string LocationEPSGProj{ get; set; }

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

        [Column(DatabaseLiterals.FieldLocationNTS)]
        public string locationNTS { get; set; }

        [Column(DatabaseLiterals.FieldLocationNotes)]
        public string LocationNotes { get; set; }

        [Column(DatabaseLiterals.FieldLocationReportLink)]
        public string LocationReportLink { get; set; }

        [Column(DatabaseLiterals.FieldLocationTimestamp)]
        public string LocationTimestamp { get; set; }

        [Column(DatabaseLiterals.FieldLocationMetaID)]
        public int MetaID { get; set; }

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
        /// Will check if current location has been entered by a tap on screen or a manual entry
        /// For validation purposes.
        /// </summary>
        [Ignore]
        public bool isManualEntry
        {
            get
            {
                if (LocationEntryType != null && LocationEntryType == Dictionaries.DatabaseLiterals.locationEntryTypeManual || LocationEntryType == Dictionaries.DatabaseLiterals.locationEntryTypeTap)
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
        /// A list of all possible fields from current class but also from previous schemas (for db upgrade)
        /// </summary>
        [Ignore]
        public Dictionary<double, List<string>> getFieldList
        {
            get
            {
                //Create a new list of all current columns in current class. This will act as the most recent
                //version of the class
                Dictionary<double, List<string>> locationFieldList = new Dictionary<double, List<string>>();
                List<string> locationFieldListDefault = new List<string>();

                foreach (System.Reflection.PropertyInfo item in this.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ColumnAttribute))).ToList())
                {
                    if (item.CustomAttributes.First().ConstructorArguments.Count() > 0)
                    {
                        locationFieldListDefault.Add(item.CustomAttributes.First().ConstructorArguments[0].ToString().Replace("\\", "").Replace("\"", ""));
                    }

                }

                locationFieldList[DatabaseLiterals.DBVersion] = locationFieldListDefault;

                //Revert shcema 1.8 changes
                List<string> locationFieldList170 = new List<string>();
                locationFieldList170.AddRange(locationFieldListDefault);
                locationFieldList170.Remove(DatabaseLiterals.FieldLocationEPSGProj);
                locationFieldList170.Remove(DatabaseLiterals.FieldLocationTimestamp);
                locationFieldList[DatabaseLiterals.DBVersion170] = locationFieldList170;

                //Revert shcema 1.7 changes
                List<string> locationFieldList160 = new List<string>();
                locationFieldList160.AddRange(locationFieldList170);
                locationFieldList160.Remove(DatabaseLiterals.FieldLocationReportLink);
                locationFieldList160.Remove(DatabaseLiterals.FieldGenericGeometry);
                locationFieldList[DatabaseLiterals.DBVersion160] = locationFieldList160;

                //Revert schema 1.6 changes. 
                List<string> locationFieldList15 = new List<string>();
                locationFieldList15.AddRange(locationFieldList160);
                locationFieldList15.Remove(DatabaseLiterals.FieldLocationNTS);
                locationFieldList[DatabaseLiterals.DBVersion150] = locationFieldList15;

                //Revert schema 1.5 changes. 
                List<string> locationFieldList144 = new List<string>();
                locationFieldList144.AddRange(locationFieldList15);
                int removeIndex = locationFieldList144.IndexOf(DatabaseLiterals.FieldLocationAlias);
                locationFieldList144.Remove(DatabaseLiterals.FieldLocationAlias);
                locationFieldList144.Insert(removeIndex, DatabaseLiterals.FieldLocationAliasDeprecated);
                locationFieldList144.Insert(locationFieldList144.Count() - 2, DatabaseLiterals.FieldLocationReportLink);

                locationFieldList[DatabaseLiterals.DBVersion144] = locationFieldList144;

                //Revert schema 1.4.4 
                List<string> locationFieldList143 = new List<string>();
                locationFieldList143.AddRange(locationFieldList144);
                int removeIndex2 = locationFieldList143.IndexOf(DatabaseLiterals.FieldLocationDatum);
                locationFieldList143.Remove(DatabaseLiterals.FieldLocationDatum);
                locationFieldList143.Insert(removeIndex2, DatabaseLiterals.FieldLocationDatumZone);
                locationFieldList[DatabaseLiterals.DBVersion143] = locationFieldList143;

                //Revert schema 1.4.3 changes
                List<string> locationFieldList142 = new List<string>();
                locationFieldList142.AddRange(locationFieldList143);
                locationFieldList[DatabaseLiterals.DBVersion142] = locationFieldList142;

                return locationFieldList;
            }
            set { }
        }

        /// <summary>
        /// Property to get a smaller version of the alias, for mobile rendering mostly
        /// </summary>
        [Ignore]
        public string LocationAliasLight
        {
            get
            {
                if (LocationAlias != string.Empty)
                {
                    int aliasNumber = 0;
                    int.TryParse(LocationAlias.Substring(LocationAlias.Length - 6, 4), out aliasNumber);

                    //Case waypoint
                    if (LocationAlias.Contains(DatabaseLiterals.KeywordStationWaypoint))
                    {
                        int.TryParse(LocationAlias.Substring(LocationAlias.Length - 5, 3), out aliasNumber);
                    }

                    //Case drill holes
                    if (LocationAlias.Contains(DatabaseLiterals.TableDrillHolePrefix))
                    {
                        int.TryParse(LocationAlias.Substring(LocationAlias.Length - 8, 4), out aliasNumber);
                    }

                    if (aliasNumber > 0)
                    {
                        //Case waypoint
                        if (LocationAlias.Contains(DatabaseLiterals.KeywordStationWaypoint))
                        {
                            return DatabaseLiterals.KeywordStationWaypointLight + aliasNumber.ToString();
                        }

                        //Case drill holes
                        if (LocationAlias.Contains(DatabaseLiterals.TableDrillHolePrefix))
                        {
                            return DatabaseLiterals.KeywordStationDrillHoleLight + aliasNumber.ToString();
                        }

                        return aliasNumber.ToString() + DatabaseLiterals.TableLocationAliasSuffix;
                    }
                    else
                    {
                        return LocationAlias;
                    }

                }
                else
                {
                    return DatabaseLiterals.picklistNACode;
                }
            }
            set { }
        }

        /// <summary>
        /// Will be used to trigger a cascade delete coming from location record
        /// </summary>
        [Ignore]
        public bool IsMapPageQuick { get; set; } = false;
    }
}
