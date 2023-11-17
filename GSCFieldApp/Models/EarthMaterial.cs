using System;
using System.Collections.Generic;
using System.Linq;
using SQLite;
using GSCFieldApp.Dictionaries;

namespace GSCFieldApp.Models
{
    [Table(DatabaseLiterals.TableEarthMat)]
    public class EarthMaterial
    {
        [PrimaryKey, AutoIncrement, Column(DatabaseLiterals.FieldEarthMatID)]
        public int EarthMatID { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatName)]
        public string EarthMatName { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatStatID)]
        public int? EarthMatStatID { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatLithgroup)]
        public string EarthMatLithgroup { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatLithtype)]
        public string EarthMatLithtype { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatLithdetail)]
        public string EarthMatLithdetail { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatModComp)]
        public string EarthMatModComp { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatMetaFacies)]
        public string EarthMatMetaIFacies { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatMetaIntensity)]
        public string EarthMatMetaIntensity { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatMapunit)]
        public string EarthMatMapunit { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatOccurs)]
        public string EarthMatOccurs { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatPercent)]
        public int? EarthMatPercent { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatModTextStruc)]
        public string EarthMatModTextStruc { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatGrSize)]
        public string EarthMatGrSize { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatDefabric)]
        public string EarthMatDefabric { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatBedthick)]
        public string EarthMatBedthick { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatColourF)]
        public string EarthMatColourF { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatColourW)]
        public string EarthMatColourW { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatColourInd)]
        public int EarthMatColourInd { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatMagSuscept)]
        public double? EarthMatMagSuscept { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatMagQualifier)]
        public string EarthMatMagQualifier { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatContact)]
        public string EarthMatContact { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatContactUp)]
        public string EarthMatContactUp { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatContactLow)]
        public string EarthMatContactLow { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatContactNote)]
        public string EarthMatContactNote { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatInterp)]
        public string EarthMatInterp { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatInterpConf)]
        public string EarthMatInterpConf { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatSorting)]
        public string EarthMatSorting { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatH2O)]
        public string EarthMatH2O { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatOxidation)]
        public string EarthMatOxidation { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatClastForm)]
        public string EarthMatClastForm { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatNotes)]
        public string EarthMatNotes { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatDrillHoleID)]
        public int? EarthMatDrillHoleID { get; set; }

        //Hierarchy
        public string ParentName = DatabaseLiterals.TableStation;

        /// <summary>
        /// Soft mandatory field check. User can still create record even if fields are not filled.
        /// Ignore attribute will tell sql not to try to write this field inside the database.
        /// </summary>
        [Ignore]
        public bool isValid
        {
            get
            {
                if ((EarthMatLithdetail != null && EarthMatLithdetail != string.Empty && EarthMatLithdetail != Dictionaries.DatabaseLiterals.picklistNACode))
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
        /// A concatenation of all three field. For UI purposes
        /// </summary>
        [Ignore]
        public string getGroupTypeDetail
        {
            get
            {
                if ((EarthMatLithgroup != string.Empty && EarthMatLithgroup != null && EarthMatLithgroup != Dictionaries.DatabaseLiterals.picklistNACode))
                {
                    if ((EarthMatLithtype != string.Empty && EarthMatLithtype != null && EarthMatLithtype != Dictionaries.DatabaseLiterals.picklistNACode))
                    {
                        if ((EarthMatLithdetail != string.Empty && EarthMatLithdetail != null && EarthMatLithdetail != Dictionaries.DatabaseLiterals.picklistNACode))
                        {
                            return EarthMatLithgroup + " - " + EarthMatLithtype + " ; " + EarthMatLithdetail;
                        }
                        else
                        {
                            return EarthMatLithgroup + " - " + EarthMatLithtype;
                        }

                    }
                    else
                    {
                        if ((EarthMatLithdetail != string.Empty && EarthMatLithdetail != null && EarthMatLithdetail != Dictionaries.DatabaseLiterals.picklistNACode))
                        {
                            return EarthMatLithgroup + " ; " + EarthMatLithdetail;
                        }
                        else
                        {
                            return EarthMatLithgroup;
                        }
                    }

                }
                else
                {
                    return string.Empty;
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
                Dictionary<double, List<string>> earthmatFieldList = new Dictionary<double, List<string>>();
                List<string> earthmatFieldListDefault = new List<string>();

                earthmatFieldListDefault.Add(DatabaseLiterals.FieldEarthMatID);
                foreach (System.Reflection.PropertyInfo item in this.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ColumnAttribute))).ToList())
                {
                    if (item.CustomAttributes.First().ConstructorArguments.Count() > 0)
                    {
                        earthmatFieldListDefault.Add(item.CustomAttributes.First().ConstructorArguments[0].ToString().Replace("\\", "").Replace("\"", ""));
                    }

                }

                earthmatFieldList[DatabaseLiterals.DBVersion] = earthmatFieldListDefault;

                //Revert shcema 1.8 changes
                List<string> earthmatFieldList170 = new List<string>();
                earthmatFieldList170.AddRange(earthmatFieldListDefault);
                earthmatFieldList170.Remove(DatabaseLiterals.FieldEarthMatContactNote);
                earthmatFieldList170.Remove(DatabaseLiterals.FieldEarthMatDrillHoleID);
                earthmatFieldList[DatabaseLiterals.DBVersion170] = earthmatFieldList170;

                //Revert shcema 1.7 changes
                List<string> earthmatFieldList160 = new List<string>();
                earthmatFieldList160.AddRange(earthmatFieldList170);
                earthmatFieldList160.Remove(DatabaseLiterals.FieldEarthMatSorting);
                earthmatFieldList160.Remove(DatabaseLiterals.FieldEarthMatH2O);
                earthmatFieldList160.Remove(DatabaseLiterals.FieldEarthMatOxidation);
                earthmatFieldList160.Remove(DatabaseLiterals.FieldEarthMatClastForm);
                earthmatFieldList[DatabaseLiterals.DBVersion160] = earthmatFieldList160;

                //Revert schema 1.6 changes. 
                List<string> earthmatFieldList15 = new List<string>();
                earthmatFieldList15.AddRange(earthmatFieldList160);
                earthmatFieldList15.Remove(DatabaseLiterals.FieldEarthMatPercent);
                earthmatFieldList15.Remove(DatabaseLiterals.FieldEarthMatMagQualifier);
                earthmatFieldList15.Remove(DatabaseLiterals.FieldEarthMatMetaIntensity);
                earthmatFieldList15.Remove(DatabaseLiterals.FieldEarthMatMetaFacies);
                earthmatFieldList15.Remove(DatabaseLiterals.FieldEarthMatModComp);
                earthmatFieldList15.Remove(DatabaseLiterals.FieldEarthMatModTextStruc);
                earthmatFieldList15.Remove(DatabaseLiterals.FieldEarthMatContact);

                earthmatFieldList15.Insert(8, DatabaseLiterals.FieldEarthMatModStrucDeprecated);
                earthmatFieldList15.Insert(9, DatabaseLiterals.FieldEarthMatModTextureDeprecated);
                earthmatFieldList15.Insert(10, DatabaseLiterals.FieldEarthMatModCompDeprecated);
                earthmatFieldList15.Insert(18, DatabaseLiterals.FieldEarthMatContactDeprecated);

                earthmatFieldList[DatabaseLiterals.DBVersion150] = earthmatFieldList15;

                //Revert schema 1.5 changes. 
                List<string> earthmatFieldList144 = new List<string>();
                earthmatFieldList144.AddRange(earthmatFieldList15);
                int removeIndex = earthmatFieldList144.IndexOf(DatabaseLiterals.FieldEarthMatName);
                earthmatFieldList144.Remove(DatabaseLiterals.FieldEarthMatName);
                earthmatFieldList144.Insert(removeIndex, DatabaseLiterals.FieldEarthMatNameDeprecated);
                earthmatFieldList[DatabaseLiterals.DBVersion144] = earthmatFieldList144;

                //Revert schema 1.4.3 changes
                List<string> eartmatFieldList143 = new List<string>();
                eartmatFieldList143.AddRange(earthmatFieldList144);
                earthmatFieldList[DatabaseLiterals.DBVersion142] = eartmatFieldList143;

                //Revert schema 1.4.2 changes
                List<string> eartmatFieldList142 = new List<string>();
                eartmatFieldList142.AddRange(eartmatFieldList143);
                eartmatFieldList142.Remove(DatabaseLiterals.FieldEarthMatNotes);
                earthmatFieldList[DatabaseLiterals.DBVersion139] = eartmatFieldList142;

                return earthmatFieldList;
            }
            set { }
        }

        /// <summary>
        /// Will extract the letter defining the ID of current earthmat, A-B-C, etc.
        /// </summary>
        /// <returns></returns>
        [Ignore]
        public string GetIDLetter
        {
            get
            {
                //Get index of last digits
                string getLast3 = this.EarthMatName.Substring(this.EarthMatName.Length - 3);
                string strOnlyID = string.Empty;
                foreach (char c in getLast3)
                {
                    if (char.IsLetter(c))
                    {
                        strOnlyID = strOnlyID + c;
                    }
                }
                return strOnlyID;
            }

            set { }

        }
    }
}
