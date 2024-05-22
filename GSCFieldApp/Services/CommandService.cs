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
        /// <returns></returns>
        public async Task<int> DeleteDatabaseItemCommand(DatabaseLiterals.TableNames tableToDeleteItemFrom, string itemAlias, int itemID)
        {
            //Var
            int numberOfRecordsDelete = 0;

            //Display a prompt with an answer to prevent butt or fat finger deleting stations.
            string title = String.Format(LocalizationResourceManager["CommandDeleteTitle"].ToString(), itemAlias);
            string content = LocalizationResourceManager["CommandDeleteContent"].ToString();
            string answer = await Shell.Current.DisplayPromptAsync(title, content, LocalizationResourceManager["GenericButtonDelete"].ToString(),
                LocalizationResourceManager["GenericButtonCancel"].ToString());

            if (answer == DateTime.Now.Year.ToString().Substring(2) && itemAlias != string.Empty && itemID > 0)
            {
                switch (tableToDeleteItemFrom)
                {
                    case DatabaseLiterals.TableNames.meta:
                        break;
                    case DatabaseLiterals.TableNames.location:
                        break;
                    case DatabaseLiterals.TableNames.station:
                        //Special case with station, it shall delete from location
                        FieldLocation flToDelete = new FieldLocation();
                        flToDelete.LocationID = itemID;
                        numberOfRecordsDelete = await da.DeleteItemAsync(flToDelete);
                        break;
                    case DatabaseLiterals.TableNames.em:
                        Earthmaterial emToDelete = new Earthmaterial();
                        emToDelete.EarthMatID = itemID;
                        numberOfRecordsDelete = await da.DeleteItemAsync(emToDelete);
                        break;
                    case DatabaseLiterals.TableNames.sample:
                        break;
                    case DatabaseLiterals.TableNames.ma:
                        break;
                    case DatabaseLiterals.TableNames.mineral:
                        break;
                    case DatabaseLiterals.TableNames.document:
                        break;
                    case DatabaseLiterals.TableNames.structure:
                        break;
                    case DatabaseLiterals.TableNames.fossil:
                        break;
                    case DatabaseLiterals.TableNames.environment:
                        break;
                    case DatabaseLiterals.TableNames.pflow:
                        break;
                    case DatabaseLiterals.TableNames.drill:
                        break;
                    default:
                        break;
                }


                
                await da.CloseConnectionAsync();

                //Show final messag to user
                string doneTitle = LocalizationResourceManager["CommandDeleteCompleteTitle"].ToString();
                string doneContent = String.Format(LocalizationResourceManager["CommandDeleteCompleteContent"].ToString(), itemAlias); 
                await Shell.Current.DisplayAlert(doneTitle, doneContent, LocalizationResourceManager["GenericButtonOk"].ToString());
            }

            return numberOfRecordsDelete;
        }
    }
}
