using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GSCFieldApp.Views;
using GSCFieldApp.Services.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSCFieldApp.Models;
using static GSCFieldApp.Dictionaries.DatabaseLiterals;
using GSCFieldApp.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.Maui.ApplicationModel.Communication;
using System.Xml.Linq;
using System.Reflection;
using Microsoft.Maui.Controls.PlatformConfiguration;
using CommunityToolkit.Maui.Alerts;
using SQLite;
using GSCFieldApp.Services;
using System.Security.Cryptography;


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
