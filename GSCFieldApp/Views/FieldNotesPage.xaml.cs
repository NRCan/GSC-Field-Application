using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.UI.Xaml.Media;
using Windows.UI.Composition;
using SQLite.Net.Attributes;
using Template10.Controls;
using Template10.Mvvm;
using GSCFieldApp.Models;
using GSCFieldApp.Dictionaries;
using GSCFieldApp.ViewModels;
using GSCFieldApp.Services.DatabaseServices;

namespace GSCFieldApp.Views
{
    public sealed partial class ReportPage : Page
    {
        public bool navFromMapPage = false; //A value to keep track of user's movement between map page and any possible action with the station in it

        //Event to notify View of any changes
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
    
        public FieldNotesViewModel ViewModel { get; set; }

        //Local settings
        DataLocalSettings localSetting = new DataLocalSettings();

        public ReportPage()
        {

            InitializeComponent();
            NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Required; //Keep cache mode on, so values for view models are kept when navigating.
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
        }

        /// <summary>
        /// Will retrieve user parameter if any are given with the page
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            //base.OnNavigatedTo(e);

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
                IJsonValue dataValue;
                if (paramObject.TryGetValue("Data", out dataValue))
                {
                    string dataValueString = dataValue.GetString();
                    JsonObject dataValuesObject = JsonObject.Parse(dataValueString);
                    IJsonValue stationValues;
                    if (dataValuesObject.TryGetValue("$values", out stationValues))
                    {
                        JsonArray stationInfoArray = stationValues.GetArray();
                        ViewModel.userSelectedStationDate = stationInfoArray.GetStringAt(1);
                        ViewModel.userSelectedStationID = stationInfoArray.GetStringAt(0);
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
                if ((bool)localSetting.GetSettingValue("forceNoteRefresh"))
                {
                    this.ViewModel.FillStationFromList(); //Refill station based on new selected date
                    localSetting.SetSettingValue("forceNoteRefresh", false);
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
            else
            {
                //Reset selection
                ViewModel.userSelectedStationID = string.Empty;
                ViewModel.userSelectedStationDate = string.Empty;
                ViewModel.SetSelectedStationFromMapPage();
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
