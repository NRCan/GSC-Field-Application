using CommunityToolkit.Mvvm.Input;
using GSCFieldApp.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSCFieldApp.Services.DatabaseServices;
using GSCFieldApp.Models;
using GSCFieldApp.Dictionaries;
using ShimSkiaSharp;

namespace GSCFieldApp.ViewModel
{
    public partial class FieldBookViewModel
    {
        #region INIT
        DataAccess da = new DataAccess();
        private Metadata model = new Metadata();

        #endregion

        #region PROPERTIES
        public Metadata Model { get { return model; } set { model = value; } }

        #endregion

        public FieldBookViewModel()
        {
            _ = ValidateDabaseExistence();
        }

        private async Task ValidateDabaseExistence()
        {
            await da.CreateDatabaseFromResource(da.DatabaseFilePath);
        }

        #region RELAY COMMANDS

        [RelayCommand]
        async Task ValidateAndSave()
        {
            //Validate if all mandatory entries have been filled.
            if (Model.isValid || !Model.isValid)
            {
                //Make sure current field book database exists
                da.PreferedDatabasePath = Path.Combine(FileSystem.Current.AppDataDirectory, Model.FieldBookFileName + DatabaseLiterals.DBTypeSqlite);
                await da.CreateDatabaseFromResource(da.PreferedDatabasePath);
                await da.SaveItemAsync(Model, false);
                await da.CloseConnectionAsync();
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                //Show error
                await Shell.Current.DisplayAlert("Warning", "Some mandatory fields have not been filled.", "Ok");
            }


        }

        [RelayCommand]
        async Task Back()
        {
            //Navigate backward (../.. will navigate two pages back)"
            await Shell.Current.GoToAsync("..");
        }

        #endregion

        #region METHODS


        #endregion
    }
}
