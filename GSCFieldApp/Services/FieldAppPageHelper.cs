using static GSCFieldApp.Dictionaries.DatabaseLiterals;
using System.Security.Cryptography;
using GSCFieldApp.Views;
using CommunityToolkit.Mvvm.ComponentModel;


namespace GSCFieldApp.Services
{
    public class FieldAppPageHelper: ObservableObject
    {

        /// <summary>
        /// Will navigate to field note page and force a refresh on notes
        /// related to input table name enum. Use tableNames.meta to force
        /// a refresh of all records.
        /// </summary>
        /// <param name="tableName">Input table name enum to refresh component in field notes</param>
        /// <returns></returns>
        public static async Task NavigateToFieldNotes(TableNames tableName, bool refreshTable = true)
        {
            if (refreshTable)
            {
                //Refresh a given table
                await Shell.Current.GoToAsync($"{nameof(FieldNotesPage)}/",
                    new Dictionary<string, object>
                    {
                        ["UpdateTableID"] = RandomNumberGenerator.GetHexString(10, false),
                        ["UpdateTable"] = tableName,
                    }
                );
            }
            else
            {
                //Default navigation
                await Shell.Current.GoToAsync($"{nameof(FieldNotesPage)}/");
            }

        }

    }
}
