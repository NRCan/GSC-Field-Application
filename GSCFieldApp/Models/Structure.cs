using GSCFieldApp.Dictionaries;
using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSCFieldApp.Models
{

    [Table(DatabaseLiterals.TableStructure)]
    public class Structure
    {
        [PrimaryKey, Column(DatabaseLiterals.FieldStructureID)]
        public string StructureID { get; set; }

        [Column(DatabaseLiterals.FieldStructureName)]
        public string StructureName { get; set; }

        [Column(DatabaseLiterals.FieldStructureClass)]
        public string StructureClass { get; set; }

        [Column(DatabaseLiterals.FieldStructureType)]
        public string StructureType { get; set; }

        [Column(DatabaseLiterals.FieldStructureDetail)]
        public string StructureDetail { get; set; }

        [Column(DatabaseLiterals.FieldStructureMethod)]
        public string StructureMethod { get; set; }

        [Column(DatabaseLiterals.FieldStructureFormat)]
        public string StructureFormat { get; set; }

        [Column(DatabaseLiterals.FieldStructureAttitude)]
        public string StructureAttitude { get; set; }

        [Column(DatabaseLiterals.FieldStructureYoung)]
        public string StructureYounging { get; set; }

        [Column(DatabaseLiterals.FieldStructureGeneration)]
        public string StructureGeneration { get; set; }

        [Column(DatabaseLiterals.FieldStructureStrain)]
        public string StructureStrain { get; set; }

        [Column(DatabaseLiterals.FieldStructureFlattening)]
        public string StructureFlattening { get; set; }

        [Column(DatabaseLiterals.FieldStructureRelated)]
        public string StructureRelated { get; set; }

        [Column(DatabaseLiterals.FieldStructureFabric)]
        public string StructureFabric { get; set; }

        [Column(DatabaseLiterals.FieldStructureSense)]
        public string StructureSense { get; set; }

        [Column(DatabaseLiterals.FieldStructureAzimuth)]
        public string StructureAzimuth { get; set; }

        [Column(DatabaseLiterals.FieldStructureDip)]
        public string StructureDipPlunge { get; set; }

        [Column(DatabaseLiterals.FieldStructureNotes)]
        public string StructureNotes { get; set; }

        [Column(DatabaseLiterals.FieldStructureParentID)]
        public string StructureParentID { get; set; }


        private string _structureSymAng = string.Empty;

        [Column(DatabaseLiterals.FieldStructureSymAng)]
        public string StructureSymAng
        {
            //Autocalculated field based on selected format, if any is chosen.
            get
            {
                if (StructureClass != null &&
                    StructureFormat != null &&
                    StructureFormat.ToLower().Contains(Dictionaries.DatabaseLiterals.KeywordDipDipDirectionRule) &&
                    StructureClass.ToLower().Contains(Dictionaries.DatabaseLiterals.KeywordPlanar))
                {
                    int intAzim;
                    bool isInt = int.TryParse(StructureAzimuth, out intAzim);

                    if (isInt)
                    {
                        if (intAzim >= 0 && intAzim <= 90)
                        {
                            return (360 + (intAzim - 90)).ToString();
                        }
                        else
                        {
                            return (intAzim - 90).ToString();
                        }


                    }
                    else
                    {
                        return StructureAzimuth;
                    }
                }
                else
                {
                    return StructureAzimuth;
                }
            }
            set
            {
                _structureSymAng = value;
            }
        }

        //Hierarchy
        public string ParentName = DatabaseLiterals.TableEarthMat;

        /// <summary>
        /// Soft mandatory field check. User can still create record even if fields are not filled.
        /// Ignore attribute will tell sql not to try to write this field inside the database.
        /// </summary>
        [Ignore]
        public bool isValid
        {
            get
            {
                if ((StructureType != string.Empty && StructureType != null && StructureType != Dictionaries.DatabaseLiterals.picklistNACode) &&
                    (StructureDetail != string.Empty && StructureDetail != null && StructureDetail != Dictionaries.DatabaseLiterals.picklistNACode) &&
                    (StructureFormat != string.Empty && StructureFormat != null && StructureFormat != Dictionaries.DatabaseLiterals.picklistNACode) &&
                    (StructureAzimuth != string.Empty && StructureAzimuth != null && StructureAzimuth != Dictionaries.DatabaseLiterals.picklistNACode) &&
                    (StructureDipPlunge != string.Empty && StructureDipPlunge != null && StructureDipPlunge != Dictionaries.DatabaseLiterals.picklistNACode) &&
                    (isRelatedStructuresAzimuthValid == true || isRelatedStructuresAzimuthValid == null) &&
                    (isRelatedStructuresDipValid == true || isRelatedStructuresDipValid == null))
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
        public string getClassTypeDetail
        {
            get
            {
                if ((StructureClass != string.Empty && StructureClass != null && StructureClass != Dictionaries.DatabaseLiterals.picklistNACode))
                {
                    if ((StructureType != string.Empty && StructureType != null && StructureType != Dictionaries.DatabaseLiterals.picklistNACode))
                    {
                        if ((StructureDetail != string.Empty && StructureDetail != null && StructureDetail != Dictionaries.DatabaseLiterals.picklistNACode))
                        {
                            return StructureClass + " - " + StructureType + " ; " + StructureDetail;
                        }
                        else
                        {
                            return StructureClass + " - " + StructureType;
                        }

                    }
                    else
                    {
                        return StructureClass;
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
        /// Validate that linear feature falls within the plane of the planar feature.
        /// Azimuths need to be based on right hand rule and current structure must have a related structure.
        /// Could be linear related to planar, or planar related to linear
        /// </summary>

        [Ignore]
        public bool? isRelatedStructuresAzimuthValid
        {
            get
            {
                if (StructureClass != null && StructureClass != string.Empty
                    && StructureRelated != null
                    && StructureRelated != string.Empty
                    && StructureAzimuth != string.Empty
                    && StructureRelated != Dictionaries.DatabaseLiterals.picklistNACode)
                {
                    //Init variables
                    int azimuthPlanar = int.MinValue;
                    int azimuthPlanarMax = int.MinValue;
                    int azimuthLinear = int.MinValue;

                    //Get related structure azim
                    if (relatedStructure == null)
                    {
                        Services.DatabaseServices.DataAccess da = new Services.DatabaseServices.DataAccess();
                        relatedStructure = da.GetRelatedStructure(StructureRelated);
                    }

                    //Fill in variables
                    if (StructureClass.Contains(DatabaseLiterals.KeywordPlanar))
                    {
                        int.TryParse(StructureAzimuth, out azimuthPlanar);

                        if (azimuthPlanar != int.MinValue)
                        {
                            azimuthPlanarMax = azimuthPlanar + 180;
                        }
                    }
                    else if (relatedStructure != null && relatedStructure.StructureClass.Contains(DatabaseLiterals.KeywordPlanar) && relatedStructure.StructureAzimuth != string.Empty)
                    {
                        int.TryParse(relatedStructure.StructureAzimuth, out azimuthPlanar);

                        if (azimuthPlanar != int.MinValue)
                        {
                            azimuthPlanarMax = azimuthPlanar + 180;
                        }
                    }

                    if (StructureClass.Contains(DatabaseLiterals.KeywordLinear))
                    {
                        int.TryParse(StructureAzimuth, out azimuthLinear);
                    }
                    else if (relatedStructure != null && relatedStructure.StructureClass.Contains(DatabaseLiterals.KeywordLinear) && relatedStructure.StructureAzimuth != null && relatedStructure.StructureAzimuth != string.Empty)
                    {
                        int.TryParse(relatedStructure.StructureAzimuth, out azimuthLinear);
                    }

                    //Validate linear and planar azimuths
                    if (azimuthLinear != int.MinValue && azimuthPlanarMax != int.MinValue && azimuthPlanarMax > 360)
                    {
                        if ((azimuthPlanar <= azimuthLinear && azimuthLinear <= 360) | (0 <= azimuthLinear && azimuthLinear <= azimuthPlanarMax - 360))
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
                        if (azimuthPlanar <= azimuthLinear && azimuthLinear <= azimuthPlanarMax)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }

                }
                else
                {
                    return null;
                }

            }

            set { }

        }

        /// <summary>
        /// Validate dip/plunge of related structure features
        /// Plunge of linear feature must be equal to or less than dip of planar feature
        /// </summary>
        [Ignore]
        public bool? isRelatedStructuresDipValid
        {
            get
            {
                if (StructureClass != null
                    && StructureRelated != null
                    && StructureAzimuth != null
                    && StructureClass != string.Empty
                    && StructureRelated != string.Empty
                    && StructureAzimuth != string.Empty
                    && StructureRelated != Dictionaries.DatabaseLiterals.picklistNACode)
                {
                    //Init variables
                    int dipPlanar = int.MinValue;
                    int dipLinear = int.MinValue;

                    //Get related structure azim
                    if (relatedStructure == null)
                    {
                        Services.DatabaseServices.DataAccess da = new Services.DatabaseServices.DataAccess();
                        relatedStructure = da.GetRelatedStructure(StructureRelated);
                    }


                    //Fill in variables
                    if (StructureClass.Contains(DatabaseLiterals.KeywordPlanar))
                    {
                        int.TryParse(StructureDipPlunge, out dipPlanar);
                    }
                    else if (relatedStructure.StructureClass.Contains(DatabaseLiterals.KeywordPlanar) && relatedStructure.StructureDipPlunge != string.Empty)
                    {
                        int.TryParse(relatedStructure.StructureDipPlunge, out dipPlanar);
                    }

                    if (StructureClass.Contains(DatabaseLiterals.KeywordLinear))
                    {
                        int.TryParse(StructureDipPlunge, out dipLinear);
                    }
                    else if (relatedStructure.StructureClass.Contains(DatabaseLiterals.KeywordLinear) && relatedStructure.StructureDipPlunge != string.Empty)
                    {
                        int.TryParse(relatedStructure.StructureDipPlunge, out dipLinear);
                    }

                    //Validate linear and planar azimuths
                    if (dipPlanar != int.MinValue && dipLinear != int.MinValue && dipPlanar >= dipLinear)
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
                    return null;
                }

            }

            set { }

        }

        [Ignore]
        public Structure relatedStructure
        {
            get
            {
                if (StructureRelated != string.Empty)
                {
                    //Get related structure azim
                    Services.DatabaseServices.DataAccess da = new Services.DatabaseServices.DataAccess();
                    return da.GetRelatedStructure(StructureRelated);
                }
                else
                {
                    return null;
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
                List<string> structureFieldList = new List<string>();
                structureFieldList.Add(DatabaseLiterals.FieldStructureID);
                foreach (System.Reflection.PropertyInfo item in this.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ColumnAttribute))).ToList())
                {
                    if (item.CustomAttributes.First().ConstructorArguments.Count() > 0)
                    {
                        structureFieldList.Add(item.CustomAttributes.First().ConstructorArguments[0].ToString().Replace("\\", "").Replace("\"", ""));
                    }

                }

                return structureFieldList;
            }
            set { }
        }
    }
}