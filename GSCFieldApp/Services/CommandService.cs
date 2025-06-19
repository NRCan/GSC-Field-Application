using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSCFieldApp.Dictionaries;
using GSCFieldApp.Models;
using GSCFieldApp.Services.DatabaseServices;
using SQLite;

namespace GSCFieldApp.Services
{
    public class CommandService
    {
        //Database
        DataAccess da = new DataAccess();

        //Localize
        public LocalizationResourceManager LocalizationResourceManager
        => LocalizationResourceManager.Instance; // Will be used for in code dynamic local strings


        public CommandService() { }

        /// <summary>
        /// Will display an alert in order to delete a datable item.
        /// Will force entry to valide delete action to prevent evil butts to activate command.
        /// </summary>
        /// <param name="tableToDeleteItemFrom">table name to delete from</param>
        /// <param name="itemAlias">record alias for popup window title</param>
        /// <param name="itemID">record parent or actual id to delete</param>
        /// <param name="anotherID">a second id in case parent and actual id are needed (drill hole cases)</param>
        /// <param name="skipPreventionDialog">Will act as a force delete without popup user needs to interact with. Used with quick button back command in map page</param>
        /// <returns></returns>
        public async Task<int> DeleteDatabaseItemCommand(DatabaseLiterals.TableNames tableToDeleteItemFrom, string itemAlias, int itemID, bool skipPreventionDialog = false, int anotherID = -1)
        {
            //Var
            int numberOfRecordsDelete = 0;
            bool proceedWithDelete = skipPreventionDialog;
            //Prevention dialog
            if (!skipPreventionDialog)
            {
                //Display a prompt with an answer to prevent butt or fat finger deleting stations.
                string title = String.Format(LocalizationResourceManager["CommandDeleteTitle"].ToString(), itemAlias);
                string content = LocalizationResourceManager["CommandDeleteContent"].ToString();
                string answer = await Shell.Current.DisplayPromptAsync(title, content, LocalizationResourceManager["GenericButtonDelete"].ToString(),
                    LocalizationResourceManager["GenericButtonCancel"].ToString());

                if (answer == DateTime.Now.Year.ToString().Substring(2))
                {
                    proceedWithDelete = true;
                }
            }

            if (proceedWithDelete && itemAlias != string.Empty && itemID > 0)
            {
                DatabaseLiterals.TableNames deleteEventTableName = tableToDeleteItemFrom;

                switch (tableToDeleteItemFrom)
                {
                    case DatabaseLiterals.TableNames.meta:
                        break;
                    case DatabaseLiterals.TableNames.location:
                        //Location always does a cascades delete
                        numberOfRecordsDelete = await da.DeleteItemCascadeAsync(DatabaseLiterals.TableLocation, DatabaseLiterals.FieldLocationID, itemID);
                        break;
                    case DatabaseLiterals.TableNames.station:
                        //Special case with station, it shall delete from location
                        //And location always cascades
                        List<DrillHole> drillSiblings = await DataAccess.DbConnection.Table<DrillHole>().Where(d=>d.DrillLocationID == itemID).ToListAsync();

                        if (drillSiblings != null && drillSiblings.Count() > 0)
                        {
                            numberOfRecordsDelete = await da.DeleteItemCascadeAsync(DatabaseLiterals.TableStation, DatabaseLiterals.FieldStationObsID, itemID);
                            deleteEventTableName = DatabaseLiterals.TableNames.station;
                        }
                        else
                        {
                            numberOfRecordsDelete = await da.DeleteItemCascadeAsync(DatabaseLiterals.TableLocation, DatabaseLiterals.FieldLocationID, itemID);
                            deleteEventTableName = DatabaseLiterals.TableNames.location;
                        }
                        
                        break;
                    case DatabaseLiterals.TableNames.earthmat:
                        numberOfRecordsDelete = await da.DeleteItemCascadeAsync(DatabaseLiterals.TableEarthMat, DatabaseLiterals.FieldEarthMatID, itemID);
                        break;
                    case DatabaseLiterals.TableNames.sample:
                        Sample samToDelete = new Sample();
                        samToDelete.SampleID = itemID;
                        numberOfRecordsDelete = await da.DeleteItemAsync(samToDelete);
                        break;
                    case DatabaseLiterals.TableNames.mineralization:
                        numberOfRecordsDelete = await da.DeleteItemCascadeAsync(DatabaseLiterals.TableMineralAlteration, DatabaseLiterals.FieldMineralAlterationID, itemID);
                        break;
                    case DatabaseLiterals.TableNames.mineral:
                        numberOfRecordsDelete = await da.DeleteItemCascadeAsync(DatabaseLiterals.TableMineral, DatabaseLiterals.FieldMineralID, itemID);
                        break;
                    case DatabaseLiterals.TableNames.document:
                        Document docToDelete = new Document();
                        docToDelete.DocumentID = itemID;
                        numberOfRecordsDelete = await da.DeleteItemAsync(docToDelete);
                        break;
                    case DatabaseLiterals.TableNames.structure:
                        Structure strucToDelete = new Structure();
                        strucToDelete.StructureID = itemID;
                        numberOfRecordsDelete = await da.DeleteItemAsync(strucToDelete);
                        break;
                    case DatabaseLiterals.TableNames.fossil:
                        Fossil fossilToDelete = new Fossil();
                        fossilToDelete.FossilID = itemID;
                        numberOfRecordsDelete = await da.DeleteItemAsync(fossilToDelete);
                        break;
                    case DatabaseLiterals.TableNames.environment:
                        EnvironmentModel envToDelete = new EnvironmentModel();
                        envToDelete.EnvID = itemID;
                        numberOfRecordsDelete = await da.DeleteItemAsync(envToDelete);
                        break;
                    case DatabaseLiterals.TableNames.pflow:
                        Paleoflow pflowToDelete = new Paleoflow();
                        pflowToDelete.PFlowID = itemID;
                        numberOfRecordsDelete = await da.DeleteItemAsync(pflowToDelete);
                        break;
                    case DatabaseLiterals.TableNames.drill:
                        //Case # 1
                        // Parent location without other drill holes or stations should delete and cascade from there
                        //Case # 2
                        // Parent location with related stations should only delete the drill hole
                        //Case # 3
                        // Parent location with related drill holes should only delete the drill hole

                        List<Station> statSiblings = await DataAccess.DbConnection.Table<Station>().Where(s => s.LocationID == itemID).ToListAsync();
                        List<DrillHole> dhSiblings = await DataAccess.DbConnection.Table<DrillHole>().Where(d => d.DrillLocationID == itemID).ToListAsync();

                        if ((statSiblings != null && statSiblings.Count() > 0) || (dhSiblings != null && dhSiblings.Count() > 1))
                        {
                            numberOfRecordsDelete = await da.DeleteItemCascadeAsync(DatabaseLiterals.TableDrillHoles, DatabaseLiterals.FieldDrillID, anotherID);
                            deleteEventTableName = DatabaseLiterals.TableNames.drill;
                        }
                        else
                        {
                            numberOfRecordsDelete = await da.DeleteItemCascadeAsync(DatabaseLiterals.TableLocation, DatabaseLiterals.FieldLocationID, itemID);
                            deleteEventTableName = DatabaseLiterals.TableNames.location;
                        }
                            
                        break;
                    case DatabaseLiterals.TableNames.linework:
                        //Location always does a cascades delete
                        numberOfRecordsDelete = await da.DeleteItemCascadeAsync(DatabaseLiterals.TableLinework, DatabaseLiterals.FieldLineworkID, itemID);
                        break;
                    default:
                        break;
                }
 
                //await da.CloseConnectionAsync();

                //Show final messag to user
                if (!skipPreventionDialog)
                {
                    string doneTitle = LocalizationResourceManager["CommandDeleteCompleteTitle"].ToString();
                    string doneContent = String.Format(LocalizationResourceManager["CommandDeleteCompleteContent"].ToString(), itemAlias);
                    await Shell.Current.DisplayAlert(doneTitle, doneContent, LocalizationResourceManager["GenericButtonOk"].ToString());
                }

                //Force refresh of field notes
                FieldAppPageHelper.deleteRecord?.Invoke(this, new Tuple<DatabaseLiterals.TableNames, int>(deleteEventTableName, itemID));
            }

            return numberOfRecordsDelete;
        }
    }
}
