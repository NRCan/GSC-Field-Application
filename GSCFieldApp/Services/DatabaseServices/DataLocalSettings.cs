using System;
using System.Collections.Generic;
using Windows.Storage;
using GSCFieldApp.Models;
using GSCFieldApp.Dictionaries;
using System.Diagnostics;

namespace GSCFieldApp.Services.DatabaseServices
{
    public class DataLocalSettings
    {
        readonly ApplicationDataContainer currentLocalSettings = ApplicationData.Current.LocalSettings;
        public const string containerName = Dictionaries.ApplicationLiterals.LocalSettingMainContainer;
        
        public DataLocalSettings()
        {
            if (!currentLocalSettings.Containers.ContainsKey(containerName))
            {
                currentLocalSettings.CreateContainer(containerName, ApplicationDataCreateDisposition.Always);
            }
            
        }

        /// <summary>
        /// Will save metadata type of class inside application setting so it can be used elsewhere in the code without reading the
        /// database.
        /// </summary>
        /// <param name="currentUser">The metadata class to keep in application setting.</param>
        public void SaveUserInfoInSettings(Metadata currentUser)
        {
            DataAccess dAccess = new DataAccess();

            currentLocalSettings.Containers[containerName].Values[Dictionaries.DatabaseLiterals.FieldUserInfoID] = currentUser.MetaID;
            currentLocalSettings.Containers[containerName].Values[Dictionaries.DatabaseLiterals.FieldUserInfoPName] = currentUser.ProjectName;
            currentLocalSettings.Containers[containerName].Values[Dictionaries.DatabaseLiterals.FieldUserInfoFWorkType] = currentUser.FieldworkType;
            currentLocalSettings.Containers[containerName].Values[Dictionaries.DatabaseLiterals.FieldUserInfoUCode] = currentUser.UserCode;
            currentLocalSettings.Containers[containerName].Values[Dictionaries.ApplicationLiterals.KeywordFieldProject] = dAccess.ProjectPath;
            currentLocalSettings.Containers[containerName].Values[Dictionaries.DatabaseLiterals.FieldUserInfoVersionSchema] = currentUser.VersionSchema;
            currentLocalSettings.Containers[containerName].Values[Dictionaries.DatabaseLiterals.FieldUserInfoActivityName] = currentUser.MetadataActivity;
        }


        /// <summary>
        /// Will delete any stored Metadata class information from application setting.
        /// </summary>
        public void WipeUserInfoSettings()
        {
            try
            {
                IEnumerator<KeyValuePair<string, ApplicationDataContainer >> containerList = currentLocalSettings.Containers.GetEnumerator();
                while (containerList.MoveNext())
                {
                    currentLocalSettings.DeleteContainer(containerList.Current.Key);
                }
            }
            catch (Exception)
            {

            }
            

        }

        /// <summary>
        /// Will delete user settings for some map information, not all
        /// </summary>
        public void WipeUserMapSettings_deprecated()
        {

            if (currentLocalSettings.Containers[containerName].Values.ContainsKey("mapLayers"))
            {
                currentLocalSettings.Containers[containerName].Values.Remove("mapLayers");
            }

        }

        /// <summary>
        /// Will delete user settings for some map information, not all
        /// </summary>
        public void WipeUserMapSettings()
        {
            //if (currentLocalSettings.Containers[containerName].Values.ContainsKey(ApplicationLiterals.KeywordMapViewLayersOrder))
            //{
            //    currentLocalSettings.Containers[containerName].Values.Remove(ApplicationLiterals.KeywordMapViewLayersOrder);
            //}
            if (currentLocalSettings.Containers[containerName].Values.ContainsKey(ApplicationLiterals.KeywordMapViewScale))
            {
                currentLocalSettings.Containers[containerName].Values.Remove(ApplicationLiterals.KeywordMapViewScale);
            }
            if (currentLocalSettings.Containers[containerName].Values.ContainsKey(ApplicationLiterals.KeywordMapViewRotation))
            {
                currentLocalSettings.Containers[containerName].Values.Remove(ApplicationLiterals.KeywordMapViewRotation);
            }
        }

        /// <summary>
        /// Will return a container key value 
        /// </summary>
        /// <param name="keyName"></param>
        /// <param name="inContainerName"></param>
        /// <returns></returns>
        public object GetSettingValue(string keyName, string inContainerName="")
        {
            //Variable 
            object output = "";
            string wantedContainer = containerName;

            if (inContainerName != string.Empty)
            {
                wantedContainer = inContainerName;
            }
            

            if (currentLocalSettings.Containers[containerName].Values.ContainsKey(keyName))
            {
                Debug.WriteLine(currentLocalSettings.Containers[containerName].Values[keyName].ToString());
                output = currentLocalSettings.Containers[containerName].Values[keyName];
            }

            return output;
        }

        /// <summary>
        /// Will return a container key value 
        /// </summary>
        /// <param name="keyName"></param>
        /// <param name="inContainerName"></param>
        /// <returns></returns>
        public bool GetBoolSettingValue(string keyName, string inContainerName = "")
        {
            //Variable 
            bool output = true;
            string wantedContainer = containerName;

            if (inContainerName != string.Empty)
            {
                wantedContainer = inContainerName;
            }


            if (currentLocalSettings.Containers[containerName].Values.ContainsKey(keyName))
            {
                Debug.WriteLine(currentLocalSettings.Containers[containerName].Values[keyName].ToString());
                try
                {
                    output = (bool)currentLocalSettings.Containers[containerName].Values[keyName];
                }
                catch (Exception)
                {
                    output = true;
                }
                
            }

            return output;
        }


        /// <summary>
        /// Will set a given container key with given object
        /// </summary>
        /// <param name="inFieldBookPath"></param>
        public void SetSettingValue(string inKey, object inKeyValue, string inContainerName="")
        {
            //Variables
            string updateContainer = containerName;
            if (inContainerName != string.Empty)
            {
                updateContainer = inContainerName;
            }
            if (currentLocalSettings.Containers.ContainsKey(updateContainer))
            {
                currentLocalSettings.Containers[updateContainer].Values[inKey] = inKeyValue;
            }
            
        }

        public bool DeleteSetting(string inKey, string inContainerName="")
        {
            //Variables
            string updateContainer = containerName;
            if (inContainerName != string.Empty)
            {
                updateContainer = inContainerName;
            }

            return currentLocalSettings.Containers[updateContainer].Values.Remove(inKey);

        }



        /// <summary>
        /// An init method for when user gets to the report page so see default headers. Only used for new projects
        /// At the end of User info part dialog.
        /// </summary>
        public void InitializeHeaderVisibility()
        {
            #region Header toggles
            
            //Default common
            if (currentLocalSettings.Containers[containerName].Values.ContainsKey(Dictionaries.DatabaseLiterals.FieldUserInfoFWorkType))
            {
                currentLocalSettings.Containers[containerName].Values[Dictionaries.DatabaseLiterals.ApplicationThemeCommon] = true;
                currentLocalSettings.Containers[containerName].Values[Dictionaries.DatabaseLiterals.TableDocument] = true;
                currentLocalSettings.Containers[containerName].Values[Dictionaries.DatabaseLiterals.TableExternalMeasure] = true;
                currentLocalSettings.Containers[containerName].Values[Dictionaries.DatabaseLiterals.TableSample] = true;
                currentLocalSettings.Containers[containerName].Values[Dictionaries.DatabaseLiterals.TableFossil] = true;
                currentLocalSettings.Containers[containerName].Values[Dictionaries.DatabaseLiterals.TableEarthMat] = true;
                currentLocalSettings.Containers[containerName].Values[Dictionaries.ApplicationLiterals.KeyworkStructureSymbols] = false;
                currentLocalSettings.Containers[containerName].Values[Dictionaries.ApplicationLiterals.KeywordStationTraverseNo] = true;

                //For bedrock projects only
                object fieldWorkType = currentLocalSettings.Containers[containerName].Values[Dictionaries.DatabaseLiterals.FieldUserInfoFWorkType];
                if (fieldWorkType != null && fieldWorkType.ToString().Contains(Dictionaries.DatabaseLiterals.ApplicationThemeBedrock))
                {

                    currentLocalSettings.Containers[containerName].Values[Dictionaries.DatabaseLiterals.ApplicationThemeBedrock] = true;
                    currentLocalSettings.Containers[containerName].Values[Dictionaries.DatabaseLiterals.TableMineralAlteration] = true;
                    currentLocalSettings.Containers[containerName].Values[Dictionaries.DatabaseLiterals.TableStructure] = true;
                    currentLocalSettings.Containers[containerName].Values[Dictionaries.DatabaseLiterals.TableMineral] = true;
                    currentLocalSettings.Containers[containerName].Values[Dictionaries.DatabaseLiterals.TableDrillHoles] = true;
                }
                else
                {
                    currentLocalSettings.Containers[containerName].Values[Dictionaries.DatabaseLiterals.ApplicationThemeBedrock] = false;
                    currentLocalSettings.Containers[containerName].Values[Dictionaries.DatabaseLiterals.TableMineralAlteration] = false;
                    currentLocalSettings.Containers[containerName].Values[Dictionaries.DatabaseLiterals.TableStructure] = false;
                    currentLocalSettings.Containers[containerName].Values[Dictionaries.DatabaseLiterals.TableMineral] = false;
                    currentLocalSettings.Containers[containerName].Values[Dictionaries.DatabaseLiterals.TableDrillHoles] = false;
                }



                //For surficial projects only
                if (fieldWorkType != null && fieldWorkType.ToString() == Dictionaries.DatabaseLiterals.ApplicationThemeSurficial)
                {
                    currentLocalSettings.Containers[containerName].Values[Dictionaries.DatabaseLiterals.ApplicationThemeSurficial] = true;
                    currentLocalSettings.Containers[containerName].Values[Dictionaries.DatabaseLiterals.TableEnvironment] = true;
                    currentLocalSettings.Containers[containerName].Values[Dictionaries.DatabaseLiterals.TableSoilProfile] = true;
                    currentLocalSettings.Containers[containerName].Values[Dictionaries.DatabaseLiterals.TablePFlow] = true;

                }
                else
                {
                    currentLocalSettings.Containers[containerName].Values[Dictionaries.DatabaseLiterals.ApplicationThemeSurficial] = false;
                    currentLocalSettings.Containers[containerName].Values[Dictionaries.DatabaseLiterals.TableEnvironment] = false;
                    currentLocalSettings.Containers[containerName].Values[Dictionaries.DatabaseLiterals.TableSoilProfile] = false;
                    currentLocalSettings.Containers[containerName].Values[Dictionaries.DatabaseLiterals.TablePFlow] = false;
                }

                if (fieldWorkType != null && fieldWorkType.ToString().Contains(Dictionaries.DatabaseLiterals.ApplicationThemeDrillHole))
                {
                    currentLocalSettings.Containers[containerName].Values[Dictionaries.DatabaseLiterals.TableDrillHoles] = true;
                }
                else
                {
                    currentLocalSettings.Containers[containerName].Values[Dictionaries.DatabaseLiterals.TableDrillHoles] = false;
                }
            }
            #endregion
        }
    }
}
