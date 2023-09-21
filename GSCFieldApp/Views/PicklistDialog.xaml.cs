using GSCFieldApp.Models;
using GSCFieldApp.ViewModels;
using System;
using System.Collections.Generic;
using Template10.Common;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace GSCFieldApp.Views
{
    public sealed partial class PicklistDialog : UserControl
    {
        PicklistViewModel picklistVM { get; set; }

        public PicklistDialog(string inputTheme)
        {
            this.InitializeComponent();

            picklistVM = new PicklistViewModel(inputTheme);

            if (inputTheme != string.Empty)
            {


                switch (inputTheme)
                {
                    case Dictionaries.DatabaseLiterals.KeywordStation:
                        ResourceLoader appResources = ResourceLoader.GetForCurrentView();
                        string themeText = appResources.GetString("PicklistDialogThemeStation/Text");
                        ModifyHeader(themeText);
                        break;
                    case Dictionaries.DatabaseLiterals.KeywordEarthmat:
                        ResourceLoader appResourcesE = ResourceLoader.GetForCurrentView();
                        string themeTextE = appResourcesE.GetString("PicklistDialogThemeEartmat/Text");
                        ModifyHeader(themeTextE);
                        break;
                    case Dictionaries.DatabaseLiterals.KeywordSample:
                        ResourceLoader appResourcesS = ResourceLoader.GetForCurrentView();
                        string themeTextS = appResourcesS.GetString("PicklistDialogThemeSample/Text");
                        ModifyHeader(themeTextS);
                        break;
                    case Dictionaries.DatabaseLiterals.KeywordDocument:
                        ResourceLoader appResourcesD = ResourceLoader.GetForCurrentView();
                        string themeTextD = appResourcesD.GetString("PicklistDialogThemeDocument/Text");
                        ModifyHeader(themeTextD);
                        break;
                    case Dictionaries.DatabaseLiterals.KeywordStructure:
                        ResourceLoader appResourcesST = ResourceLoader.GetForCurrentView();
                        string themeTextST = appResourcesST.GetString("PicklistDialogThemeStructure/Text");
                        ModifyHeader(themeTextST);
                        break;
                    case Dictionaries.DatabaseLiterals.KeywordFossil:
                        ResourceLoader appResourcesF = ResourceLoader.GetForCurrentView();
                        string themeTextF = appResourcesF.GetString("PicklistDialogThemeFossil/Text");
                        ModifyHeader(themeTextF);
                        break;
                    case Dictionaries.DatabaseLiterals.KeywordPflow:
                        ResourceLoader appResourcesPF = ResourceLoader.GetForCurrentView();
                        string themeTextPF = appResourcesPF.GetString("PicklistDialogThemePflow/Text");
                        ModifyHeader(themeTextPF);
                        break;
                    case Dictionaries.DatabaseLiterals.KeywordMineral:
                        ResourceLoader appResourcesMin = ResourceLoader.GetForCurrentView();
                        string themeTextMin = appResourcesMin.GetString("PicklistDialogThemeMineral/Text");
                        ModifyHeader(themeTextMin);
                        break;
                    case Dictionaries.DatabaseLiterals.KeywordMA:
                        ResourceLoader appResourcesMinAlt = ResourceLoader.GetForCurrentView();
                        string themeTextMinAlt = appResourcesMinAlt.GetString("PicklistDialogThemeMineralAlt/Text");
                        ModifyHeader(themeTextMinAlt);
                        break;
                    case Dictionaries.DatabaseLiterals.KeywordEnvironment:
                        ResourceLoader appResourcesEnv = ResourceLoader.GetForCurrentView();
                        string themeTextEnv = appResourcesEnv.GetString("PicklistDialogThemeEnvironment/Text");
                        ModifyHeader(themeTextEnv);
                        break;
                    default:
                        break;
                }



            }

            //#258 bringing back some old patch on save button
            this.picklistSaveButton.GotFocus -= PicklistSaveButton_GotFocus;
            this.picklistSaveButton.GotFocus += PicklistSaveButton_GotFocus;
        }

        private void PicklistSaveButton_GotFocus(object sender, RoutedEventArgs e)
        {
            this.picklistSaveButton.GotFocus -= PicklistSaveButton_GotFocus;
            picklistVM.SaveDialogInfo();
            CloseControl();
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
                var modalClose = Window.Current.Content as Template10.Controls.ModalDialog;
                var viewClose = modalClose.ModalContent as PicklistDialog;
                modalClose.ModalContent = viewClose;
                modalClose.IsModal = false;
            });
        }

        private async void picklistBackButton_TappedAsync(object sender, TappedRoutedEventArgs e)
        {
            ResourceLoader loadLocalization = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();

            ContentDialog closeEditorDialog = new ContentDialog()
            {
                Title = loadLocalization.GetString("GenericWarningTitle"),
                Content = loadLocalization.GetString("ClosingDialogWarning"),
                PrimaryButtonText = loadLocalization.GetString("GenericDialog_ButtonYes"),
                SecondaryButtonText = loadLocalization.GetString("GenericDialog_ButtonNo")
            };

            closeEditorDialog.Style = (Style)Application.Current.Resources["WarningDialog"];
            ContentDialogResult cdr = await closeEditorDialog.ShowAsync();
            if (cdr == ContentDialogResult.Primary)
            {
                CloseControl();
            }



        }

        #endregion

        #region EVENTS

        /// <summary>
        /// Will changed the selected item background value and visibility property
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PicklistValueDeleteIcon_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //Added a bunch of try catch because an obscure error was thrown from the generated code behind, not even this code.
            //TODO: try to find why the code crashes without the try/catch

            try
            {
                IList<object> selectedValues = this.picklistValues.SelectedItems;
                if (selectedValues.Count > 0)
                {
                    foreach (object values in selectedValues)
                    {

                        //Get children symbol icon
                        SymbolIcon childIcon = GetContainerChildren(this.picklistValues.ContainerFromItem(values));

                        //Revert icon
                        setPicklistValueIcon(childIcon, values, false);
                    }
                }
                else
                {
                    try
                    {
                        //Reverse all
                        this.picklistValues.SelectAll();
                        selectedValues = this.picklistValues.SelectedItems;
                        //ItemCollection allValues = this.picklistValues.Items;
                        foreach (object val in selectedValues)
                        {
                            //Get children symbol icon
                            SymbolIcon childIcon = GetContainerChildren(this.picklistValues.ContainerFromItem(val));

                            //Revert icon
                            setPicklistValueIcon(childIcon, val, false);
                        }
                    }
                    catch (Exception)
                    {

                    }

                }
            }
            catch (Exception)
            {

            }


        }

        /// <summary>
        /// Will changed the selected item background value and visibility property
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PicklistValueIcon_Tapped(object sender, TappedRoutedEventArgs e)
        {
            IList<object> selectedValues = this.picklistValues.SelectedItems;
            if (selectedValues.Count > 0)
            {
                foreach (object values in selectedValues)
                {
                    setPicklistValueIcon((SymbolIcon)sender, values, false);
                }
            }
        }

        /// <summary>
        /// Will fill the list box with items coming from user selected picklist to edit
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void picklistSelector_SelectionChangedAsync(object sender, SelectionChangedEventArgs e)
        {
            //Save current list if needed
            picklistVM.SaveDialogInfo();

            //Cast
            ComboBox senderBox = sender as ComboBox;
            Themes.ComboBoxItem selectedVocab = senderBox.SelectedItem as Themes.ComboBoxItem;

            //Fill the list box with selected value
            picklistVM.FillListBoxPicklistValues(selectedVocab.itemValue);
        }

        /// <summary>
        /// Will be triggered when new items are added. This particular event will manager the background color
        /// for items that are selected to not be visible in the forms by the user.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void picklistValues_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            //Get child icon

            SymbolIcon childIcon = GetContainerChildren(args.ItemContainer);

            setPicklistValueIcon(childIcon, args.Item, true);
        }



        #endregion

        #region SET METHODS

        /// <summary>
        /// Will inverse selection icon of selected vocabs. If methods is used for initialization of UI, 
        /// Only the "not visible" items will be colors, no inversion will take place.
        /// </summary>
        /// <param name="inItem">The item to retrieve vocabularies property from</param>
        /// <param name="inContainer">The item container in which the background will be changed</param>
        /// <param name="forInit">Bool value for intiliasation or interaction</param>
        public void setPicklistValueIcon(SymbolIcon inIcon, object inItem, bool forInit)
        {
            #region Set

            if (inItem != null && inIcon != null)
            {
                //Build a color brush for rejected picklist values
                SolidColorBrush rejectedColorBrush = new SolidColorBrush();
                Windows.UI.Color rejectedColor = Windows.UI.Colors.Red;
                rejectedColorBrush.Color = rejectedColor;

                SolidColorBrush selectedColorBrush = new SolidColorBrush();
                Windows.UI.Color selectedColor = Windows.UI.Colors.LawnGreen;
                selectedColorBrush.Color = selectedColor;

                //Find if current item is rejected and modify background accordingly
                Vocabularies valueVoc = inItem as Vocabularies;
                string currentVisibility = valueVoc.Visibility;

                if (forInit)
                {

                    if (currentVisibility == Dictionaries.DatabaseLiterals.boolNo)
                    {
                        inIcon.Symbol = Symbol.Cancel;
                        inIcon.Foreground = rejectedColorBrush;
                    }
                    else
                    {
                        inIcon.Symbol = Symbol.Accept;
                        inIcon.Foreground = selectedColorBrush;
                    }

                }
                else
                {
                    if (currentVisibility == Dictionaries.DatabaseLiterals.boolNo)
                    {
                        valueVoc.Visibility = Dictionaries.DatabaseLiterals.boolYes;


                        inIcon.Symbol = Symbol.Accept;
                        inIcon.Foreground = selectedColorBrush;


                    }
                    else
                    {
                        valueVoc.Visibility = Dictionaries.DatabaseLiterals.boolNo;

                        inIcon.Symbol = Symbol.Cancel;
                        inIcon.Foreground = rejectedColorBrush;


                    }
                }
            }

            #endregion
        }

        /// <summary>
        /// Will modify the dialog header with a given string
        /// </summary>
        /// <param name="wantedHeader"></param>
        private void ModifyHeader(string wantedHeader)
        {
            this.pageHeader.Text = wantedHeader;

        }

        #endregion

        #region SAVE
        private void PicklistSaveButton_TappedAsync(object sender, TappedRoutedEventArgs e)
        {
            this.picklistSaveButton.Focus(FocusState.Keyboard);
        }

        #endregion

        #region GET METHODS
        /// <summary>
        /// Get list of all childrens from a given XAML object
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        private SymbolIcon GetContainerChildren(DependencyObject parent)
        {
            SymbolIcon wantedChild = default(SymbolIcon);
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                wantedChild = child as SymbolIcon;
                if (wantedChild == null)
                {
                    wantedChild = GetContainerChildren(child);
                }
                if (wantedChild != null)
                {
                    break;
                }
            }
            return wantedChild;
        }

        #endregion

        /// <summary>
        /// Will be triggered when the check icon is tapped and will revert it's icon.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PicklistValueCheck_Tapped(object sender, TappedRoutedEventArgs e)
        {
            IList<object> selectedValues = this.picklistValues.SelectedItems;
            if (selectedValues.Count > 0)
            {
                foreach (object values in selectedValues)
                {
                    setPicklistValueIcon((SymbolIcon)sender, values, false);
                }
            }
        }

        /// <summary>
        /// Will reset mod. textbox so user can add terms. This will be usefull when a value can't be edited, but new values can be added.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void picklistValues_ItemClick(object sender, ItemClickEventArgs e)
        {
            ListView senderListView = sender as ListView;
            if (senderListView.SelectedIndex == senderListView.Items.IndexOf(e.ClickedItem))
            {
                this.picklistAddTextbox.IsEnabled = true;
                this.picklistAddTextbox.Text = string.Empty;
            }
        }
    }

}
