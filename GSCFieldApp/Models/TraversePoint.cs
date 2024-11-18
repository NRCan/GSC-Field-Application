using GSCFieldApp.Dictionaries;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GSCFieldApp.Models
{

    [Table(DatabaseLiterals.TableTraversePoint)]
    public class TraversePoint
    {
        [Column(DatabaseLiterals.FieldTravPointID), PrimaryKey, AutoIncrement]
        public int TravID { get; set; }

        [Column(DatabaseLiterals.FieldTravPointGeometry)]
        public byte[] TravGeom { get; set; }

        [Column(DatabaseLiterals.FieldTravPointDate)]
        public string TravDate { get; set; }

        [Column(DatabaseLiterals.FieldTravPointPilot)]
        public string TravPilot { get; set; }

        [Column(DatabaseLiterals.FieldTravPointOrderFlight)]
        public int TravOrderFlight { get; set; }

        [Column(DatabaseLiterals.FieldTravPointOrderVisit)]
        public int TravOrderVisit { get; set; }

        [Column(DatabaseLiterals.FieldTravPointGeologist)]
        public string TravGeologist { get; set; }

        [Column(DatabaseLiterals.FieldTravPointPartner)]
        public string TravPartner { get; set; }

        [Column(DatabaseLiterals.FieldTravPointPlannedBy)]
        public string TravPlannedBy { get; set; }

        [Column(DatabaseLiterals.FieldTravPointLabel)]
        public string TravLabel { get; set; }

        [Column(DatabaseLiterals.FieldTravPointNotes)]
        public string TravNotes { get; set; }

        [Column(DatabaseLiterals.FieldTravPointXUTM)]
        public int? TravXUTM { get; set; }

        [Column(DatabaseLiterals.FieldTravPointYUTM)]
        public int? TravYUTM { get; set; }

        [Column(DatabaseLiterals.FieldTravPointXDMS)]
        public string TravXDMS { get; set; }

        [Column(DatabaseLiterals.FieldTravPointYDMS)]
        public string TravYDMS { get; set; }

        [Column(DatabaseLiterals.FieldTravPointXDD)]
        public double TravXDD { get; set; }

        [Column(DatabaseLiterals.FieldTravPointYDD)]
        public double TravYDD { get; set; }

        [Column(DatabaseLiterals.FieldTravPointNM)]
        public double? TravNM { get; set; }

        [Column(DatabaseLiterals.FieldTravPointNMCamp)]
        public double? TravNMCamp { get; set; }

        [Column(DatabaseLiterals.FieldTravPointNTS)]
        public string TravNTS { get; set; }

        [Column(DatabaseLiterals.FieldTravPointAirPhoto)]
        public string TravAirPhoto { get; set; }

        [Column(DatabaseLiterals.FieldTravPointMagDeclination)]
        public string TravMag { get; set; }


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
                Dictionary<double, List<string>> travPointFieldList = new Dictionary<double, List<string>>();
                List<string> travPointFieldListDefault = new List<string>();

                foreach (System.Reflection.PropertyInfo item in this.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ColumnAttribute))).ToList())
                {
                    if (item.CustomAttributes.First().ConstructorArguments.Count() > 0)
                    {
                        travPointFieldListDefault.Add(item.CustomAttributes.First().ConstructorArguments[0].ToString().Replace("\\", "").Replace("\"", ""));
                    }

                }

                travPointFieldList[DatabaseLiterals.DBVersion] = travPointFieldListDefault;

                return travPointFieldList;
            }
            set { }
        }
    }
}