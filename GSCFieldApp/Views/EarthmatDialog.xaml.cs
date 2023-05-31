using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using GSCFieldApp.ViewModels;
using GSCFieldApp.Models;
using Template10.Common;
using GSCFieldApp.Services.DatabaseServices;
using Windows.UI.Core;
using GSCFieldApp.Dictionaries;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace GSCFieldApp.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EarthmatDialog : UserControl
    {
        public EarthmatViewModel ViewModel { get; set; }
        public FieldNotes parentViewMode { get; set; }
        
        public List<string> Rocks { get; private set; }

        private readonly DataAccess accessData = new DataAccess();
        public string level1Sep = ApplicationLiterals.parentChildLevel1Seperator;
        public string level2Sep = ApplicationLiterals.parentChildLevel2Seperator;

        public EarthmatDialog(FieldNotes inDetailViewModel)
        {

            parentViewMode = inDetailViewModel;

            this.InitializeComponent();
            ViewModel = new EarthmatViewModel(inDetailViewModel);
            this.Loading += EarthmatDialog_Loading;

            //#258 bringing back some old patch on save button
            this.earthmatSaveButton.GotFocus -= EarthmatSaveButton_GotFocus;
            this.earthmatSaveButton.GotFocus += EarthmatSaveButton_GotFocus;

        }

        private void EarthmatSaveButton_GotFocus(object sender, RoutedEventArgs e)
        {
            this.earthmatSaveButton.GotFocus -= EarthmatSaveButton_GotFocus;
            ViewModel.SaveDialogInfo();
            CloseControl();
        }

        private void EarthmatDialog_Loading(FrameworkElement sender, object args)
        {
            this.Rocks = CreateSuggestionList();

            //Fill automatically the earthmat dialog if an edit is asked by the user.
            if (parentViewMode.GenericTableName == DatabaseLiterals.TableEarthMat && ViewModel.doEarthUpdate)
            {
                this.ViewModel.AutoFillDialog(parentViewMode);
                this.pageHeader.Text = this.pageHeader.Text + "  " + parentViewMode.GenericAliasName;
            }
            else if (!ViewModel.doEarthUpdate)
            {
                this.pageHeader.Text = this.pageHeader.Text + "  " + this.ViewModel.Alias;
            }
            else
            {
                this.pageHeader.Text = this.pageHeader.Text + "  " + this.ViewModel.Alias;
            }

            
        }
      
        #region SAVE
        private void earthmatSaveButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.earthmatSaveButton.Focus(FocusState.Keyboard);
        }

        #endregion

        #region CLOSE
        /// <summary>
        /// Will close the modal dialog.
        /// </summary>
        public void CloseControl()
        {

            //Get the current window and cast it to a DeleteDialog ModalDialog and shut it down.
            WindowWrapper.Current().Dispatcher.Dispatch(() =>
            {
                var modal = Window.Current.Content as Template10.Controls.ModalDialog;
                var view = modal.ModalContent as EarthmatDialog;
                modal.ModalContent = view;
                modal.IsModal = false;
            });
        }

        private void earthmatBackButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            CloseControl();
        }

        #endregion

        #region SHOW
        /// <summary>
        /// Will be triggered whenever the user wants to see the complete lithologic type list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void EarthLithoSearch_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.projectType == ScienceLiterals.ApplicationThemeBedrock)
            {
                ContentDialogSemanticZoom newDialog = new ContentDialogSemanticZoom(DatabaseLiterals.TableEarthMat, DatabaseLiterals.FieldEarthMatLithgroup, DatabaseLiterals.FieldEarthMatLithdetail);
                newDialog.userHasSelectedAValue += NewDialog_userHasSelectedAValue;
                ContentDialogResult results = await newDialog.ShowAsync();
            }
            else if (ViewModel.projectType == ScienceLiterals.ApplicationThemeSurficial)
            {
                ContentDialogSemanticZoom newDialog = new ContentDialogSemanticZoom(DatabaseLiterals.TableEarthMat, string.Empty, DatabaseLiterals.FieldEarthMatLithdetail);
                newDialog.userHasSelectedAValue += NewDialog_userHasSelectedAValue;
                ContentDialogResult results = await newDialog.ShowAsync();
            }

            
            
        }



        #endregion

        /// <summary>
        /// Will be triggered whenever the user has selected a value from the list
        /// Cast the sender and add it to a proper textbox to show selected value
        /// </summary>
        /// <param name="sender"></param>
        public void NewDialog_userHasSelectedAValue(object sender)
        {
            ListView inListView = sender as ListView;
            Models.SemanticData inSD = inListView.SelectedValue as Models.SemanticData;
            if (inSD != null)
            {
                string raw_litho_text = inSD.Title + level2Sep + inSD.Subtitle;
                ViewModel.InitFill2ndRound(raw_litho_text);
                this.EarthLithAutoSuggest.Text = raw_litho_text;
            }

        }

        /// <summary>
        /// Will be triggered when the cancel icon is tapped and will revert it's icon.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConcatValueCheck_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //Find the clicked symbol icon list view parent
            SymbolIcon senderIcon = sender as SymbolIcon;
            DependencyObject iconParent = VisualTreeHelper.GetParent(senderIcon);
            while (!(iconParent is ListView))
            {
                iconParent = VisualTreeHelper.GetParent(iconParent);
                
            }

            //Find value associated with clicked symbol icon and remove from list view.
            ListView parentListView = iconParent as ListView;
            IList<object> selectedValues = parentListView.SelectedItems;
            if (selectedValues.Count > 0)
            {
                foreach (object values in selectedValues)
                {
                    ViewModel.RemoveSelectedValue(values, parentListView.Name);
                }
            }
        }

        private void EarthLithAutoSuggest_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                //var search_term = EarthLithAutoSuggest.Text.ToLower();
                //var results = Rocks.Where(i => i.StartsWith(search_term)).ToList();

                var search_term = EarthLithAutoSuggest.Text.ToLower();
                var results = Rocks.Where(i => i.ToLower().Contains(search_term)).ToList();

                if (results.Count > 0)
                    EarthLithAutoSuggest.ItemsSource = results;
                else
                    EarthLithAutoSuggest.ItemsSource = new string[] { "No results found" };
            }
            
            //Reset litho box
            if (sender.Text == string.Empty)
            {
                EarthLitho.Text = string.Empty;
                ViewModel.InitFill2ndRound(EarthLitho.Text); //Reset picklist
            }

        }

        private void EarthLithAutoSuggest_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null && args.ChosenSuggestion.ToString() != "No results found" && sender.Text != string.Empty)
            {
                EarthLitho.Text = args.ChosenSuggestion.ToString();
            }
            else
            {
                //Reset litho box
                EarthLitho.Text = string.Empty;
            }

            //Update list that are bound to lithology selection
            ViewModel.InitFill2ndRound(EarthLitho.Text);

        }

        /// <summary>
        /// Build a suggestion list for current project type lith detail
        /// </summary>
        /// <returns></returns>
        private List<string> CreateSuggestionList()
        {
            Vocabularies vocabularyModel = new Vocabularies();
            string vocQuerySelect = "SELECT * FROM " + DatabaseLiterals.TableDictionary + " as md ";
            string vocQueryJoin = "JOIN " + DatabaseLiterals.TableDictionaryManager + " as mdm ON md." +
                DatabaseLiterals.FieldDictionaryCodedTheme + " = mdm." + DatabaseLiterals.FieldDictionaryManagerCodedTheme + " ";
            string vocQueryWhere = "WHERE " + DatabaseLiterals.FieldDictionaryManagerAssignField + " = 'LITHDETAIL' ";
            string vocQueryWhereAnd = "AND mdm." + DatabaseLiterals.FieldDictionaryManagerSpecificTo + " = '" + ViewModel.projectType + "' ";
            string vocQueryVisibility = " AND md." + DatabaseLiterals.FieldDictionaryVisible + " = '" + DatabaseLiterals.boolYes + "'";
            string vocFinalQuery = vocQuerySelect + vocQueryJoin + vocQueryWhere + vocQueryWhereAnd + vocQueryVisibility;

            List<object> vocResults = accessData.ReadTable(vocabularyModel.GetType(), vocFinalQuery);

            var outResults = new List<string>();
            foreach (Vocabularies tmp in vocResults)
            {
                if (tmp.RelatedTo != null && tmp.RelatedTo.ToString() != string.Empty)
                {
                    outResults.Add(tmp.RelatedTo.ToString() + " ; " + tmp.Code.ToString());

                }
                else
                {
                    outResults.Add(tmp.Code.ToString());
                }
            }

            return outResults;
        }

        /// <summary>
        /// Filter based on the user's input for minerals
        /// </summary>
        /// <param name="sender">The event sender</param>
        /// <param name="e">Any event arguments</param>
        private void EarthMineralAutoSuggest_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            // Only get results when it was a user typing,
            // otherwise assume the value got filled in by TextMemberPath
            // or the handler for SuggestionChosen.
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                //Set the ItemsSource to be your filtered dataset
                var search_term = EarthMineralAutoSuggest.Text.ToLower();
                var results = ViewModel.EarthmatMineral.Where(i => i.itemName.ToLower().Contains(search_term)).ToList(); //Take existing mineral list from VM

                if (results.Count > 0)
                    EarthMineralAutoSuggest.ItemsSource = results;
                else
                    EarthMineralAutoSuggest.ItemsSource = new string[] { "No results found" };
            }

        }


    }
}
