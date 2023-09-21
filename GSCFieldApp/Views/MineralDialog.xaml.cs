using GSCFieldApp.Models;
using GSCFieldApp.Services.DatabaseServices;
using GSCFieldApp.ViewModels;
using System.Collections.Generic;
using System.Linq;
using Template10.Common;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace GSCFieldApp.Views
{
    public sealed partial class MineralDialog : UserControl
    {

        public MineralViewModel MineralVM { get; set; }
        public FieldNotes parentViewModel { get; set; }

        public List<string> Minerals { get; private set; }
        private readonly DataAccess accessData = new DataAccess();

        public MineralDialog(FieldNotes inDetailViewModel)
        {
            parentViewModel = inDetailViewModel;

            this.InitializeComponent();

            MineralVM = new MineralViewModel(inDetailViewModel);
            this.Loading += MineralDialog_Loading;

            //#258 bringing back some old patch on save button
            this.mineralSaveButton.GotFocus -= MineralSaveButton_GotFocus;
            this.mineralSaveButton.GotFocus += MineralSaveButton_GotFocus;

        }

        private void MineralSaveButton_GotFocus(object sender, RoutedEventArgs e)
        {
            this.mineralSaveButton.GotFocus -= MineralSaveButton_GotFocus;
            MineralVM.SaveDialogInfo();
            CloseControl();
        }

        private void MineralDialog_Loading(FrameworkElement sender, object args)
        {

            this.Minerals = CreateSuggestionList();

            //Fill automatically the earthmat dialog if an edit is asked by the user.
            if (parentViewModel.GenericTableName == Dictionaries.DatabaseLiterals.TableMineral && MineralVM.doMineralUpdate)
            {
                this.MineralVM.AutoFillDialog(parentViewModel);
                this.pageHeader.Text = this.pageHeader.Text + "  " + parentViewModel.GenericAliasName;
            }
            else if (!MineralVM.doMineralUpdate)
            {
                this.pageHeader.Text = this.pageHeader.Text + "  " + this.MineralVM.MineralAlias;
            }
        }


        #region CLOSE
        /// <summary>
        /// Will close the modal dialog.
        /// </summary>
        public void CloseControl()
        {

            //Get the current window and cast it to a DeleteDialog ModalDialog and shut it down.
            WindowWrapper.Current().Dispatcher.Dispatch(() =>
            {
                var modalMineralClose = Window.Current.Content as Template10.Controls.ModalDialog;
                var viewMineralClose = modalMineralClose.ModalContent as MineralDialog;
                modalMineralClose.ModalContent = viewMineralClose;
                modalMineralClose.IsModal = false;
            });
        }

        private void mineralBackButton_Tapped(object sender, TappedRoutedEventArgs e)
        {

            CloseControl();
        }

        #endregion

        #region SAVE
        private void mineralSaveButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.mineralSaveButton.Focus(FocusState.Keyboard);
        }

        #endregion

        //Made by Jamel

        /// Filter based on the user's input into the combobox text area
        /// </summary>
        /// <param name="sender">The event sender</param>
        /// <param name="e">Any event arguments</param>
        private void MineralAutoSuggest_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            // Only get results when it was a user typing,
            // otherwise assume the value got filled in by TextMemberPath
            // or the handler for SuggestionChosen.
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                //Set the ItemsSource to be your filtered dataset
                //sender.ItemsSource = dataset;
                string search_term = MineralAutoSuggest.Text.ToLower();
                List<string> results = Minerals.Where(i => i.ToLower().Contains(search_term)).ToList();

                if (results != null && results.Count > 0)
                    MineralAutoSuggest.ItemsSource = results;
                else
                    MineralAutoSuggest.ItemsSource = new string[] { "No results found" };
            }

            //Reset mineral box
            if (sender.Text == string.Empty)
            {
                MineralNamesTextbox.Text = string.Empty;
                MineralVM.InitFill2ndRound(MineralNamesTextbox.Text); //Reset picklist
            }
        }

        private void MineralAutoSuggest_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {

            if (args.ChosenSuggestion != null && args.ChosenSuggestion.ToString() != "No results found" && sender.Text != string.Empty)
            {
                MineralNamesTextbox.Text = args.ChosenSuggestion.ToString();
                MineralNamesTextbox.Focus(FocusState.Programmatic); //Force focus, so viewmodel gets filled with value
            }
            else
            {
                //Reset litho box
                MineralNamesTextbox.Text = string.Empty;
            }

            //Update list that are bound to lithology selection
            MineralVM.InitFill2ndRound(MineralNamesTextbox.Text);

        }
        private List<string> CreateSuggestionList()
        {
            Vocabularies vocabularyModel = new Vocabularies();
            string vocQuerySelect = "SELECT * FROM " + Dictionaries.DatabaseLiterals.TableDictionary;
            string vocQueryWhere = " WHERE CODETHEME = 'MINERAL'";
            string vocQueryVisibility = " AND " + Dictionaries.DatabaseLiterals.TableDictionary + "." + Dictionaries.DatabaseLiterals.FieldDictionaryVisible + " = '" + Dictionaries.DatabaseLiterals.boolYes + "'";
            string vocFinalQuery = vocQuerySelect + vocQueryWhere + vocQueryVisibility;

            List<object> vocResults = accessData.ReadTable(vocabularyModel.GetType(), vocFinalQuery);

            var outResults = new List<string>();
            foreach (Vocabularies tmp in vocResults)
            {
                //outResults.Add(tmp.RelatedTo.ToString() + " ; " + tmp.Code.ToString());
                outResults.Add(tmp.Description.ToString());
            }

            return outResults;
        }

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
                    MineralVM.RemoveSelectedValue(values, parentListView.Name);
                }
            }
        }
    }
}
