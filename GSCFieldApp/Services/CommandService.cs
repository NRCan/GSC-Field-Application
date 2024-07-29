using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSCFieldApp.Dictionaries;
using GSCFieldApp.Models;
using GSCFieldApp.Services.DatabaseServices;

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
        /// <param name="tableToDeleteItemFrom"></param>
        /// <param name="itemAlias"></param>
        /// <param name="itemID"></param>
        /// <param name="skipPreventionDialog">Will act as a force delete without popup user needs to interact with. Used with quick button back command in map page</param>
        /// <returns></returns>
        public async Task<int> DeleteDatabaseItemCommand(DatabaseLiterals.TableNames tableToDeleteItemFrom, string itemAlias, int itemID, bool skipPreventionDialog = false)
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
                        numberOfRecordsDelete = await da.DeleteItemCascadeAsync(DatabaseLiterals.TableLocation, DatabaseLiterals.FieldLocationID, itemID);
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
                        break;
                    case DatabaseLiterals.TableNames.mineral:
                        Mineral minToDelete = new Mineral();
                        minToDelete.MineralID = itemID;
                        numberOfRecordsDelete = await da.DeleteItemAsync(minToDelete);
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
                        break;
                    default:
                        break;
                }
 
                await da.CloseConnectionAsync();

                //Show final messag to user
                if (!skipPreventionDialog)
                {
                    string doneTitle = LocalizationResourceManager["CommandDeleteCompleteTitle"].ToString();
                    string doneContent = String.Format(LocalizationResourceManager["CommandDeleteCompleteContent"].ToString(), itemAlias);
                    await Shell.Current.DisplayAlert(doneTitle, doneContent, LocalizationResourceManager["GenericButtonOk"].ToString());
                }

            }

            return numberOfRecordsDelete;
        }
    }
}
