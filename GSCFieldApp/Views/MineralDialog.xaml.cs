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
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.Foundation;

namespace GSCFieldApp.Views
{
    public sealed partial class MineralDialog : UserControl
    {
        public MineralViewModel MineralVM { get; set; }
        public FieldNotes parentViewModel { get; set; }
        public List<string> Minerals { get; private set; }
        private readonly DataAccess accessData = new DataAccess();
        private TranslateTransform dragTransform;
        private UIElement currentDraggedElement;

        public MineralDialog(FieldNotes inDetailViewModel)
        {
            parentViewModel = inDetailViewModel;
            InitializeComponent();
            MineralVM = new MineralViewModel(inDetailViewModel);
            this.Loading += MineralDialog_Loading;
            dragTransform = new TranslateTransform();

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

            // Auto-fill dialog for edit mode
            if (parentViewModel.GenericTableName == Dictionaries.DatabaseLiterals.TableMineral && MineralVM.doMineralUpdate)
            {
                this.MineralVM.AutoFillDialog(parentViewModel);
                this.pageHeader.Text += "  " + parentViewModel.GenericAliasName;
            }
            else if (!MineralVM.doMineralUpdate)
            {
                this.pageHeader.Text += "  " + this.MineralVM.MineralAlias;
            }
        }

        #region Dragging Implementation

        private void UIElement_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            if (sender is UIElement element)
            {
                currentDraggedElement = element;
                if (element.RenderTransform is TranslateTransform transform)
                {
                    dragTransform = transform;
                }
                else
                {
                    dragTransform = new TranslateTransform();
                    element.RenderTransform = dragTransform;
                }
            }
        }

        private void UIElement_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (currentDraggedElement != null && dragTransform != null)
            {
                dragTransform.X += e.Delta.Translation.X;
                dragTransform.Y += e.Delta.Translation.Y;
            }
        }

        private void UIElement_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            currentDraggedElement = null;
        }

        #endregion

        #region Close
        public void CloseControl()
        {
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

        #region Save
        private void mineralSaveButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.mineralSaveButton.Focus(FocusState.Keyboard);
        }

        #endregion

        #region AutoSuggestBox
        private void MineralAutoSuggest_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                string search_term = MineralAutoSuggest.Text.ToLower();
                List<string> results = Minerals.Where(i => i.ToLower().Contains(search_term)).ToList();

                MineralAutoSuggest.ItemsSource = results.Count > 0
                    ? results
                    : new string[] { "No results found" } as IEnumerable<string>;
            }

            if (sender.Text == string.Empty)
            {
                MineralNamesTextbox.Text = string.Empty;
                MineralVM.InitFill2ndRound(MineralNamesTextbox.Text);
            }
        }

        private void MineralAutoSuggest_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null && args.ChosenSuggestion.ToString() != "No results found" && sender.Text != string.Empty)
            {
                MineralNamesTextbox.Text = args.ChosenSuggestion.ToString();
                MineralNamesTextbox.Focus(FocusState.Programmatic);
            }
            else
            {
                MineralNamesTextbox.Text = string.Empty;
            }

            MineralVM.InitFill2ndRound(MineralNamesTextbox.Text);
        }

        #endregion

        private List<string> CreateSuggestionList()
        {
            Vocabularies vocabularyModel = new Vocabularies();
            string vocQuerySelect = "SELECT * FROM " + Dictionaries.DatabaseLiterals.TableDictionary;
            string vocQueryWhere = " WHERE CODETHEME = 'MINERAL'";
            string vocQueryVisibility = " AND " + Dictionaries.DatabaseLiterals.TableDictionary + "." + Dictionaries.DatabaseLiterals.FieldDictionaryVisible + " = '" + Dictionaries.DatabaseLiterals.boolYes + "'";
            string vocFinalQuery = vocQuerySelect + vocQueryWhere + vocQueryVisibility;

            List<object> vocResults = accessData.ReadTable(vocabularyModel.GetType(), vocFinalQuery);

            return vocResults.Select(v => ((Vocabularies)v).Description.ToString()).ToList();
        }

        private void ConcatValueCheck_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SymbolIcon senderIcon = sender as SymbolIcon;
            DependencyObject iconParent = VisualTreeHelper.GetParent(senderIcon);

            while (!(iconParent is ListView))
            {
                iconParent = VisualTreeHelper.GetParent(iconParent);
            }

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

        private void Element_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (sender is UIElement element)
            {
                // Get the current transform or create a new one
                if (element.RenderTransform is TranslateTransform transform)
                {
                    transform.X += e.Delta.Translation.X;
                    transform.Y += e.Delta.Translation.Y;
                }
                else
                {
                    var newTransform = new TranslateTransform
                    {
                        X = e.Delta.Translation.X,
                        Y = e.Delta.Translation.Y
                    };
                    element.RenderTransform = newTransform;
                }
            }
        }

    }
}
