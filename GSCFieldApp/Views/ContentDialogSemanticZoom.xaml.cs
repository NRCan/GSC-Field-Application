﻿using GSCFieldApp.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace GSCFieldApp.Views
{
    public sealed partial class ContentDialogSemanticZoom : ContentDialog
    {
        //Events and delegate
        public delegate void semanticZoomEventHandler(object sender); //A delegate for execution events
        public event semanticZoomEventHandler userHasSelectedAValue; //This event is triggered when a save has been done on station table.

        public ContentDialogSemanticZoomViewModel ViewModel { get; set; }
        public int _selectedIndex = -1;

        // For Multiple Levels like earth materials and structures
        public ContentDialogSemanticZoom(string tableName, string parentFieldName, string childFieldName)
        {

            this.InitializeComponent();
            ViewModel = new ContentDialogSemanticZoomViewModel
            {
                inAssignTable = tableName,
                inParentFieldName = parentFieldName,
                inChildFieldName = childFieldName
            };
            ViewModel.MakeGroup();
            this.Loaded += ContentDialogSemanticZoom_Loaded;
        }


        private void ContentDialogSemanticZoom_Loaded(object sender, RoutedEventArgs e)
        {
            var collectionGroups = Collection.View.CollectionGroups;
            ((ListViewBase)this.semanticZoom.ZoomedOutView).ItemsSource = collectionGroups;
            _selectedIndex = -1;
        }

        private void semanticZoomListView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            LaunchSelection();
            this.Hide();
        }


        private void SemanticZoomContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            LaunchSelection();
        }

        /// <summary>
        /// Will keep the selected value and send it to parent dialog.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LaunchSelection()
        {
            //Launch an event call for everyone that an earthmat has been edited.
            if (userHasSelectedAValue != null)
            {
                userHasSelectedAValue(this.semanticZoomListView);

            }
        }


    }
}
