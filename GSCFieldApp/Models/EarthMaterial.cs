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
    [Table(DatabaseLiterals.TableEarthMat)]
    public class EarthMaterial
    {
        [PrimaryKey, Column(DatabaseLiterals.FieldEarthMatID)]
        public string EarthMatID { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatName)]
        public string EarthMatName { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatStatID)]
        public string EarthMatStatID { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatLithgroup)]
        public string EarthMatLithgroup { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatLithtype)]
        public string EarthMatLithtype { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatLithdetail)]
        public string EarthMatLithdetail { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatMapunit)]
        public string EarthMatMapunit { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatOccurs)]
        public string EarthMatOccurs { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatModStruc)]
        public string EarthMatModStruc { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatModTexture)]
        public string EarthMatModTextur { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatModComp)]
        public string EarthMatModComp { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatGrSize)]
        public string EarthMatGrSize { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatDefabric)]
        public string EarthMatDefabric { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatColourF)]
        public string EarthMatColourF { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatColourW)]
        public string EarthMatColourW { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatColourInd)]
        public int EarthMatColourInd { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatMagSuscept)]
        public double EarthMatMagSuscept { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatContact)]
        public string EarthMatContact { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatContactUp)]
        public string EarthMatContactUp { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatContactLow)]
        public string EarthMatContactLow { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatInterp)]
        public string EarthMatInterp { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatInterpConf)]
        public string EarthMatInterpConf { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatBedthick)]
        public string EarthMatBedthick { get; set; }

        [Column(DatabaseLiterals.FieldEarthMatNotes)]
        public string EarthMatNotes{ get; set; }

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
                if ((EarthMatLithgroup != string.Empty && EarthMatLithgroup != null && EarthMatLithgroup != Dictionaries.DatabaseLiterals.picklistNACode) && 
                    (EarthMatLithdetail != string.Empty && EarthMatLithdetail != null && EarthMatLithdetail != Dictionaries.DatabaseLiterals.picklistNACode))
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
        /// A list of all possible fields
        /// </summary>
        [Ignore]
        public List<string> getFieldList
        {
            get {
                List<string> earthmatFieldList = new List<string>();
                earthmatFieldList.Add(DatabaseLiterals.FieldEarthMatID);
                foreach (System.Reflection.PropertyInfo item in this.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ColumnAttribute))).ToList())
                {
                    if (item.CustomAttributes.First().ConstructorArguments.Count() > 0)
                    {
                        earthmatFieldList.Add(item.CustomAttributes.First().ConstructorArguments[0].ToString().Replace("\\", "").Replace("\"", ""));
                    }
                    
                }

                return earthmatFieldList;
            }
            set { }
        }
    }
}
