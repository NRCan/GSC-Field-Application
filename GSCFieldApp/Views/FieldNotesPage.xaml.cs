﻿using GSCFieldApp.Dictionaries;
using GSCFieldApp.Services;
using GSCFieldApp.Services.DatabaseServices;
using GSCFieldApp.ViewModels;
using System.ComponentModel;
using Windows.Data.Json;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace GSCFieldApp.Views
{
    public sealed partial class FieldNotesPage : Page
    {
        public bool navFromMapPage = false; //A value to keep track of user's movement between map page and any possible action with the station in it

        //Event to notify View of any changes
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public FieldNotesViewModel ViewModel { get; set; }

        //Local settings
        readonly DataLocalSettings localSetting = new DataLocalSettings();

        public FieldNotesPage()
        {

            InitializeComponent();
            NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Required; //Keep cache mode on, so values for view models are kept when navigating.
        }

        //protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        //{
        //    base.OnNavigatingFrom(e);
        //}

        /// <summary>
        /// Will retrieve user parameter if any are given with the page
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            base.OnNavigatedTo(e);

            //Init once
            if (this.ViewModel == null)
            {
                this.ViewModel = new FieldNotesViewModel();
            }

            //Get the parameters (they are inside a json object...)
            if (e.Parameter != null && e.Parameter.ToString() != string.Empty)
            {
                JsonObject paramObject = JsonObject.Parse(e.Parameter.ToString());

                //Get the data value out of the json
                if (paramObject.TryGetValue("Data", out IJsonValue dataValue))
                {
                    string dataValueString = dataValue.GetString();
                    JsonObject dataValuesObject = JsonObject.Parse(dataValueString);
                    if (dataValuesObject.TryGetValue("$values", out IJsonValue stationValues))
                    {
                        JsonArray stationInfoArray = stationValues.GetArray();
                        ViewModel.userSelectedStationDate = stationInfoArray.GetStringAt(1);
                        ViewModel.userSelectedStationID = stationInfoArray.GetStringAt(0);
                        ViewModel.userSelectedDrillID = stationInfoArray.GetStringAt(2);

                        navFromMapPage = true;

                        this.ViewModel.summaryDone -= ViewModel_summaryDone;
                        this.ViewModel.summaryDone += ViewModel_summaryDone;

                    }


                }
            }
            else
            {
                this.ViewModel.summaryDone -= ViewModel_summaryDone;
                this.ViewModel.summaryDone += ViewModel_summaryDone;
            }

            //Need refresh?
            //TODO find another way then saving in the settings, because now there is two way of updating
            //field notes, from nav parameter when closing station dialog or setting when user has taken a waypoint that doesn't navigate to field notes.
            if (localSetting.GetSettingValue("forceNoteRefresh") != null)
            {
                try
                {
                    this.ViewModel.FillSummaryReportDateItems(); //Refill station based on new selected date
                    localSetting.SetSettingValue("forceNoteRefresh", false);
                }
                catch (System.Exception ex)
                {
                    new ErrorLogToFile(ex).WriteToFile();
                }

            }
            else
            {
                //If not already set, force it to make a first save.
                localSetting.SetSettingValue("forceNoteRefresh", false);
            }

        }

        /// <summary>
        /// When summary is done, set date and station selection if wanted by the user (after a station selection on map page)
        /// </summary>
        /// <param name="sender"></param>
        private void ViewModel_summaryDone(object sender)
        {
            //If user has selected a station and wants to navigate to report
            if (navFromMapPage)
            {
                ViewModel.SetSelectedStationFromMapPage();
                navFromMapPage = false;
            }
        }

        /// <summary>
        /// Whenever the grid to display fielwork summary is loaded, call the method to fill it from the view model.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            if (navFromMapPage || this.ViewModel.ReportDateItems.Count == 0)
            {
                this.ViewModel.FillSummaryReportDateItems();
                this.ViewModel.FillDrill();
            }
            else
            {
                this.ViewModel.FillStationFromList();
                this.ViewModel.FillDrill();
            }

        }

        /// <summary>
        /// Will set a bool value in view model so events to fill report items are filled only a init. True being the page is loading.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Page_Loading(FrameworkElement sender, object args)
        {
            ViewModel.pageLoading = true;
            ViewModel.pageBeingLoaded();
        }

        /// <summary>
        /// Will set a bool value in view model so events to fill report items are filled only a init. False being the page has been loaded and 
        /// any subsequent request should be prevented, like navigating from.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.pageLoading = false;
        }
       
    }

}
