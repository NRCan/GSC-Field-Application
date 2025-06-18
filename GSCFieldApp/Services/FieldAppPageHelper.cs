using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GSCFieldApp.Controls;
using GSCFieldApp.Models;
using GSCFieldApp.Services;
using GSCFieldApp.Services.DatabaseServices;
using GSCFieldApp.Views;
using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.Controls.PlatformConfiguration;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static GSCFieldApp.Dictionaries.DatabaseLiterals;


namespace GSCFieldApp.Services
{
    public class FieldAppPageHelper: ObservableObject
    {
        #region INIT
        //Database
        public DataAccess da = new DataAccess();
        public DataIDCalculation idCalculator = new DataIDCalculation();

        //Localize
        public LocalizationResourceManager LocalizationResourceManager
        => LocalizationResourceManager.Instance; // Will be used for in code dynamic local strings

        //Services
        public CommandService commandServ = new CommandService();

        //Controls
        public ConcatenatedCombobox concat = new ConcatenatedCombobox(); //Use to concatenate values

        //Theme
        public FieldThemes FieldThemes { get; set; }

        //Navigation
        public static bool NavFromMapPage = false;  //Used to know where the user is coming from

        //Events
        public static EventHandler<Tuple<TableNames, object>> newRecord; //This event is triggered when a different fb is selected so field notes and map pages forces a refresh.  
        public static EventHandler<Tuple<TableNames, object>> updateRecord; //This event is triggered when a different fb is selected so field notes and map pages forces a refresh.  
        public static EventHandler<Tuple<TableNames, int>> deleteRecord; //This event is triggered when a different fb is selected so field notes and map pages forces a refresh.  

        //Enums
        public enum refreshType { insert, update, delete}

        #endregion

        /// <summary>
        /// Will navigate to field note page and force a refresh on notes
        /// related to input table name enum. Use tableNames.meta to force
        /// a refresh of all records.
        /// </summary>
        /// <param name="tableName">Input table name enum to refresh component in field notes</param>
        /// <returns></returns>
        public static async Task NavigateToFieldNotesOrMapPage(TableNames tableName, bool refreshTable = true)
        {
            //Make sure to remove all previous navigated pages to prevent them from showing up
            List<Page> lastPages = Shell.Current.Navigation.NavigationStack.ToList();

            foreach (Page p in lastPages)
            {
                if (p != null)
                {
                    Shell.Current.Navigation.RemovePage(p);
                }

            }

            if (refreshTable)
            {
                //Refresh a given table
                await Shell.Current.GoToAsync($"//{nameof(FieldNotesPage)}",
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
                await Shell.Current.GoToAsync($"//{nameof(FieldNotesPage)}");
            }

        }

        /// <summary>
        /// Will either navigate to the map page if user was in there, or it'll go the field notes.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="refreshTable"></param>
        /// <returns></returns>
        public static async Task NavigateAfterAction(TableNames tableName, bool refreshTable = true)
        {
            if (NavFromMapPage)
            {
                await Shell.Current.GoToAsync($"//{nameof(MapPage)}/");
            }
            else
            {
                await NavigateToFieldNotesOrMapPage(tableName, refreshTable);
            }
        }

        /// <summary>
        /// Will send a signal that a record has been created.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="recordObject"></param>
        public static async void RefreshFieldNotes(TableNames tableName, object recordObject, refreshType typeOfRefresh)
        {
            if (typeOfRefresh == refreshType.update)
            {
                //Send call to refresh other pages
                EventHandler<Tuple<TableNames, object>> updateRecordEvent = updateRecord;
                if (updateRecordEvent != null)
                {
                    Tuple<TableNames, object> tableRecordTuple = new(tableName, recordObject);
                    updateRecordEvent(updateRecord, tableRecordTuple);
                }
            }
            else if (typeOfRefresh == refreshType.insert)
            {
                //Send call to refresh other pages
                EventHandler<Tuple<TableNames, object>> newRecordEvent = newRecord;
                if (newRecordEvent != null)
                {
                    Tuple<TableNames, object> tableRecordTuple = new(tableName, recordObject);
                    newRecordEvent(newRecord, tableRecordTuple);
                }
            }
            else if (typeOfRefresh == refreshType.delete)
            {
                //Send call to refresh other pages
                EventHandler<Tuple<TableNames, int>> deleteRecordEvent = deleteRecord;
                if (deleteRecordEvent != null)
                {
                    Tuple<TableNames, int> tableRecordTuple = new(tableName, (int)recordObject);
                    deleteRecordEvent(deleteRecord, tableRecordTuple);
                }
            }

        }

    }
}
