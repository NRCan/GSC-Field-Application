﻿using GSCFieldApp.Dictionaries;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GSCFieldApp.Models
{

    [Table(DatabaseLiterals.TableStructure)]
    public class Structure
    {
        [PrimaryKey, AutoIncrement, Column(DatabaseLiterals.FieldStructureID)]
        public int StructureID { get; set; }

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
        public int? StructureRelated { get; set; }

        [Column(DatabaseLiterals.FieldStructureFabric)]
        public string StructureFabric { get; set; }

        [Column(DatabaseLiterals.FieldStructureSense)]
        public string StructureSense { get; set; }

        [Column(DatabaseLiterals.FieldStructureAzimuth)]
        public int StructureAzimuth { get; set; }

        [Column(DatabaseLiterals.FieldStructureDip)]
        public int? StructureDipPlunge { get; set; }

        [Column(DatabaseLiterals.FieldStructureSymAng)]
        public int StructureSymAng
        {
            //Autocalculated field based on selected format, if any is chosen.
            get
            {
                if (StructureClass != null &&
                    StructureFormat != null &&
                    StructureFormat.ToLower().Contains(Dictionaries.DatabaseLiterals.KeywordDipDipDirectionRule) &&
                    StructureClass.ToLower().Contains(Dictionaries.DatabaseLiterals.KeywordPlanar))
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

        [Column(DatabaseLiterals.FieldStructureNotes)]
        public string StructureNotes { get; set; }

        [Column(DatabaseLiterals.FieldStructureParentID)]
        public int StructureParentID { get; set; }

        private int? _structureSymAng;

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
                if ((StructureType != string.Empty && StructureType != Dictionaries.DatabaseLiterals.picklistNACode) &&
                    (StructureDetail != string.Empty && StructureDetail != Dictionaries.DatabaseLiterals.picklistNACode) &&
                    (StructureFormat != string.Empty && StructureFormat != Dictionaries.DatabaseLiterals.picklistNACode) &&
                    (isRelatedStructuresAzimuthValid == true || isRelatedStructuresAzimuthValid == null) &&
                    (isRelatedStructuresDipValid == true || isRelatedStructuresDipValid == null))
                {
                    return true;
                }
                else
                {
                    if (StructureAttitude == DatabaseLiterals.structurePlanarAttitudeTrend)
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
                    && StructureRelated != null)
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
                        azimuthPlanar = StructureAzimuth;

                        if (azimuthPlanar != int.MinValue)
                        {
                            azimuthPlanarMax = azimuthPlanar + 180;
                        }
                    }
                    else if (relatedStructure != null && relatedStructure.StructureClass.Contains(DatabaseLiterals.KeywordPlanar))
                    {
                        azimuthPlanar = StructureAzimuth;

                        if (azimuthPlanar != int.MinValue)
                        {
                            azimuthPlanarMax = azimuthPlanar + 180;
                        }
                    }

                    if (StructureClass.Contains(DatabaseLiterals.KeywordLinear))
                    {
                        azimuthLinear = StructureAzimuth;
                    }
                    else if (relatedStructure != null &&
                        relatedStructure.StructureClass != null &&
                        relatedStructure.StructureClass.Contains(DatabaseLiterals.KeywordLinear))
                    {
                        azimuthLinear = StructureAzimuth;
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
                    && StructureClass != string.Empty)
                {
                    //Init variables
                    int? dipPlanar = null;
                    int? dipLinear = null;

                    //Get related structure azim
                    if (relatedStructure == null)
                    {
                        Services.DatabaseServices.DataAccess da = new Services.DatabaseServices.DataAccess();
                        relatedStructure = da.GetRelatedStructure(StructureRelated);
                    }


                    //Fill in variables
                    if (StructureClass.Contains(DatabaseLiterals.KeywordPlanar))
                    {
                        dipPlanar = StructureDipPlunge;
                    }
                    else if (relatedStructure.StructureClass.Contains(DatabaseLiterals.KeywordPlanar))
                    {
                        dipPlanar = StructureDipPlunge;
                    }

                    if (StructureClass.Contains(DatabaseLiterals.KeywordLinear))
                    {
                        dipLinear = StructureDipPlunge;
                    }
                    else if (relatedStructure.StructureClass.Contains(DatabaseLiterals.KeywordLinear))
                    {
                        dipLinear = StructureDipPlunge;
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
                if (StructureRelated != 0)
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

                structureFieldListDefault.Add(DatabaseLiterals.FieldStructureID);
                foreach (System.Reflection.PropertyInfo item in this.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ColumnAttribute))).ToList())
                {
                    if (item.CustomAttributes.First().ConstructorArguments.Count() > 0)
                    {
                        structureFieldListDefault.Add(item.CustomAttributes.First().ConstructorArguments[0].ToString().Replace("\\", "").Replace("\"", ""));
                    }

                }

                structureFieldList[DatabaseLiterals.DBVersion] = structureFieldListDefault;


                //Revert shcema 1.7 changes
                //List<string> strucFieldList160 = new List<string>();
                //strucFieldList160.AddRange(structureFieldListDefault);
                //strucFieldList160.Remove(DatabaseLiterals.FieldGenericRowID);
                //structureFieldList[DatabaseLiterals.DBVersion160] = strucFieldList160;

                structureFieldList[DatabaseLiterals.DBVersion150] = structureFieldListDefault;

                //Revert schema 1.5 changes. 
                List<string> structureFieldList144 = new List<string>();
                structureFieldList144.AddRange(structureFieldListDefault);
                int removeIndex = structureFieldList144.IndexOf(DatabaseLiterals.FieldStructureName);
                structureFieldList144.Remove(DatabaseLiterals.FieldStructureName);
                structureFieldList144.Insert(removeIndex, DatabaseLiterals.FieldStructureNameDeprecated);
                structureFieldList[DatabaseLiterals.DBVersion144] = structureFieldList144;

                return structureFieldList;
            }
            set { }
        }
    }
}