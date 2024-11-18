using static GSCFieldApp.Dictionaries.DatabaseLiterals;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GSCFieldApp.Models
{

    [Table(TableStructure)]
    public class Structure
    {
        [Column(FieldStructureID), PrimaryKey, AutoIncrement]
        public int StructureID { get; set; }

        [Column(FieldStructureName)]
        public string StructureName { get; set; }

        [Column(FieldStructureClass)]
        public string StructureClass { get; set; }

        [Column(FieldStructureType)]
        public string StructureType { get; set; }

        [Column(FieldStructureDetail)]
        public string StructureDetail { get; set; }

        [Column(FieldStructureMethod)]
        public string StructureMethod { get; set; }

        [Column(FieldStructureFormat)]
        public string StructureFormat { get; set; }

        [Column(FieldStructureAttitude)]
        public string StructureAttitude { get; set; }

        [Column(FieldStructureYoung)]
        public string StructureYounging { get; set; }

        [Column(FieldStructureGeneration)]
        public string StructureGeneration { get; set; }

        [Column(FieldStructureStrain)]
        public string StructureStrain { get; set; }

        [Column(FieldStructureFlattening)]
        public string StructureFlattening { get; set; }

        [Column(FieldStructureRelated)]
        public int? StructureRelated { get; set; }

        [Column(FieldStructureFabric)]
        public string StructureFabric { get; set; }

        [Column(FieldStructureSense)]
        public string StructureSense { get; set; }

        [Column(FieldStructureAzimuth)]
        public int StructureAzimuth { get; set; }

        [Column(FieldStructureDip)]
        public int StructureDipPlunge { get; set; }

        [Column(FieldStructureSymAng)]
        public int StructureSymAng
        {
            //Autocalculated field based on selected format, if any is chosen.
            get
            {
                if (StructureClass != null &&
                    StructureFormat != null &&
                    StructureFormat.ToLower().Contains(KeywordDipDipDirectionRule) &&
                    StructureClass.ToLower().Contains(KeywordPlanar))
                {

                    if (StructureAzimuth >= 0 && StructureAzimuth <= 90)
                    {
                        return (360 + (StructureAzimuth - 90));
                    }
                    else
                    {
                        return (StructureAzimuth - 90);
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

        [Column(FieldStructureNotes)]
        public string StructureNotes { get; set; }

        [Column(FieldStructureParentID)]
        public int StructureEarthmatID { get; set; }

        private int? _structureSymAng;

        //Hierarchy
        public string ParentName = TableEarthMat;

        /// <summary>
        /// Soft mandatory field check. User can still create record even if fields are not filled.
        /// Ignore attribute will tell sql not to try to write this field inside the database.
        /// </summary>
        [Ignore]
        public bool isValid
        {
            get
            {
                //TODO: Should we add back the validation on related azim and dips? Commented off the properties
                if ((StructureType != string.Empty && StructureType != picklistNACode) &&
                    (StructureDetail != string.Empty && StructureDetail != picklistNACode) &&
                    (StructureFormat != string.Empty && StructureFormat != picklistNACode))
                {
                    return true;
                }
                else
                {
                    if (StructureAttitude == structurePlanarAttitudeTrend)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

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
                if ((StructureClass != string.Empty && StructureClass != null && StructureClass != picklistNACode))
                {
                    if ((StructureType != string.Empty && StructureType != null && StructureType != picklistNACode))
                    {
                        if ((StructureDetail != string.Empty && StructureDetail != null && StructureDetail != picklistNACode))
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
        //[Ignore]
        //public bool? isRelatedStructuresAzimuthValid
        //{
        //    get
        //    {
        //        if (StructureClass != null && StructureClass != string.Empty
        //            && StructureRelated != null)
        //        {
        //            //Init variables
        //            int azimuthPlanar = int.MinValue;
        //            int azimuthPlanarMax = int.MinValue;
        //            int azimuthLinear = int.MinValue;

        //            //Get related structure azim
        //            if (relatedStructure == null)
        //            {
        //                Services.DatabaseServices.DataAccess da = new Services.DatabaseServices.DataAccess();
        //                relatedStructure = da.GetRelatedStructure(StructureRelated);
        //            }

        //            //Fill in variables
        //            if (StructureClass.Contains(KeywordPlanar))
        //            {
        //                azimuthPlanar = StructureAzimuth;

        //                if (azimuthPlanar != int.MinValue)
        //                {
        //                    azimuthPlanarMax = azimuthPlanar + 180;
        //                }
        //            }
        //            else if (relatedStructure != null && relatedStructure.StructureClass.Contains(KeywordPlanar))
        //            {
        //                azimuthPlanar = StructureAzimuth;

        //                if (azimuthPlanar != int.MinValue)
        //                {
        //                    azimuthPlanarMax = azimuthPlanar + 180;
        //                }
        //            }

        //            if (StructureClass.Contains(KeywordLinear))
        //            {
        //                azimuthLinear = StructureAzimuth;
        //            }
        //            else if (relatedStructure != null &&
        //                relatedStructure.StructureClass != null &&
        //                relatedStructure.StructureClass.Contains(KeywordLinear))
        //            {
        //                azimuthLinear = StructureAzimuth;
        //            }

        //            //Validate linear and planar azimuths
        //            if (azimuthLinear != int.MinValue && azimuthPlanarMax != int.MinValue && azimuthPlanarMax > 360)
        //            {
        //                if ((azimuthPlanar <= azimuthLinear && azimuthLinear <= 360) | (0 <= azimuthLinear && azimuthLinear <= azimuthPlanarMax - 360))
        //                {
        //                    return true;
        //                }
        //                else
        //                {
        //                    return false;
        //                }
        //            }
        //            else
        //            {
        //                if (azimuthPlanar <= azimuthLinear && azimuthLinear <= azimuthPlanarMax)
        //                {
        //                    return true;
        //                }
        //                else
        //                {
        //                    return false;
        //                }
        //            }

        //        }
        //        else
        //        {
        //            return null;
        //        }

        //    }

        //    set { }

        //}

        ///// <summary>
        ///// Validate dip/plunge of related structure features
        ///// Plunge of linear feature must be equal to or less than dip of planar feature
        ///// </summary>
        //[Ignore]
        //public bool? isRelatedStructuresDipValid
        //{
        //    get
        //    {
        //        if (StructureClass != null
        //            && StructureRelated != null
        //            && StructureClass != string.Empty)
        //        {
        //            //Init variables
        //            int dipPlanar = int.MinValue;
        //            int dipLinear = int.MinValue;

        //            //Get related structure azim
        //            if (relatedStructure == null)
        //            {
        //                Services.DatabaseServices.DataAccess da = new Services.DatabaseServices.DataAccess();
        //                relatedStructure = da.GetRelatedStructure(StructureRelated);
        //            }


        //            //Fill in variables
        //            if (StructureClass.Contains(KeywordPlanar))
        //            {
        //                dipPlanar = StructureDipPlunge;
        //            }
        //            else if (relatedStructure.StructureClass.Contains(KeywordPlanar))
        //            {
        //                dipPlanar = StructureDipPlunge;
        //            }

        //            if (StructureClass.Contains(KeywordLinear))
        //            {
        //                dipLinear = StructureDipPlunge;
        //            }
        //            else if (relatedStructure.StructureClass.Contains(KeywordLinear))
        //            {
        //                dipLinear = StructureDipPlunge;
        //            }

        //            //Validate linear and planar azimuths
        //            if (dipPlanar != int.MinValue && dipLinear != int.MinValue && dipPlanar >= dipLinear)
        //            {
        //                return true;
        //            }
        //            else
        //            {
        //                return false;
        //            }


        //        }
        //        else
        //        {
        //            return null;
        //        }

        //    }

        //    set { }

        //}

        //[Ignore]
        //public Structure relatedStructure
        //{
        //    get
        //    {
        //        if (StructureRelated != 0)
        //        {
        //            //Get related structure azim
        //            Services.DatabaseServices.DataAccess da = new Services.DatabaseServices.DataAccess();
        //            return da.GetRelatedStructure(StructureRelated);
        //        }
        //        else
        //        {
        //            return null;
        //        }
        //    }
        //    set { }
        //}

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
                Dictionary<double, List<string>> structureFieldList = new Dictionary<double, List<string>>();
                List<string> structureFieldListDefault = new List<string>();

                foreach (System.Reflection.PropertyInfo item in this.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ColumnAttribute))).ToList())
                {
                    if (item.CustomAttributes.First().ConstructorArguments.Count() > 0)
                    {
                        structureFieldListDefault.Add(item.CustomAttributes.First().ConstructorArguments[0].ToString().Replace("\\", "").Replace("\"", ""));
                    }

                }

                structureFieldList[DBVersion] = structureFieldListDefault;
                

                //Revert shcema 1.7 changes
                //List<string> strucFieldList160 = new List<string>();
                //strucFieldList160.AddRange(structureFieldListDefault);
                //strucFieldList160.Remove(FieldGenericRowID);
                //structureFieldList[DBVersion160] = strucFieldList160;

                structureFieldList[DBVersion150] = structureFieldListDefault;

                //Revert schema 1.5 changes. 
                List<string> structureFieldList144 = new List<string>();
                structureFieldList144.AddRange(structureFieldListDefault);
                int removeIndex = structureFieldList144.IndexOf(FieldStructureName);
                structureFieldList144.Remove(FieldStructureName);
                structureFieldList144.Insert(removeIndex,FieldStructureNameDeprecated);
                structureFieldList[DBVersion144] = structureFieldList144;

                return structureFieldList;
            }
            set { }
        }

        /// <summary>
        /// Property to get a smaller version of the alias, for mobile rendering mostly
        /// </summary>
        [Ignore]
        public string StructureAliasLight
        {
            get
            {
                if (StructureName != string.Empty)
                {
                    int aliasNumber = 0;
                    int.TryParse(StructureName.Substring(StructureName.Length - 2), out aliasNumber);

                    if (aliasNumber > 0)
                    {
                        //Trim bunch of zeros
                        string shorterStructureName = StructureName.Substring(StructureName.Length - 7);
                        return shorterStructureName.TrimStart('0');
                    }
                    else
                    {
                        return picklistNACode;
                    }

                }
                else
                {
                    return picklistNACode;
                }
            }
            set { }
        }

        [Ignore]
        public string GetClassType
        {
            get 
            {
                return string.Format("{0}{1}{2}", StructureClass, KeywordConcatCharacter2nd, StructureType);
            }
            set { }
        }
    }
}