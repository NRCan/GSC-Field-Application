﻿using GSCFieldApp.Dictionaries;
using GSCFieldApp.Models;
using GSCFieldApp.Services.DatabaseServices;
using GSCFieldApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Template10.Common;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

//using SQLite.Net;
//using SQLite.Net.Platform.WinRT;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace GSCFieldApp.Views
{
    public sealed partial class StructureDialog : UserControl
    {
        public StructureViewModel strucViewModel { get; set; }
        public FieldNotes parentViewModel { get; set; }

        public bool isAQuickStructure = false;
        //public bool Focus();
        public List<string> Structures { get; private set; }
        private readonly DataAccess accessData = new DataAccess();

        private readonly SolidColorBrush failColour = new SolidColorBrush(Windows.UI.Colors.Red);
        private Brush defaultColourBrush;
        private Color defaultBorderColor;

        public delegate void strucCloseWithoutSaveEventHandler(object sender); //A delegate for execution events
        public event strucCloseWithoutSaveEventHandler strucClosed; //This event is triggered when a save has been done on station table.


        public StructureDialog(FieldNotes inDetailViewModel, bool isQuickStructure)
        {
            parentViewModel = inDetailViewModel;
            isAQuickStructure = isQuickStructure;

            this.InitializeComponent();

            strucViewModel = new StructureViewModel(inDetailViewModel);

            this.Loading += StructureDialog_Loading;

            defaultColourBrush = this.strucType.BorderBrush;

            //#258 bringing back some old patch on save button
            this.structSaveButton.GotFocus -= StructSaveButton_GotFocus;
            this.structSaveButton.GotFocus += StructSaveButton_GotFocus;

        }

        private void StructSaveButton_GotFocus(object sender, RoutedEventArgs e)
        {
            this.structSaveButton.GotFocus -= StructSaveButton_GotFocus;
            strucViewModel.SaveDialogInfoAsync();
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
                var modalStructClose = Window.Current.Content as Template10.Controls.ModalDialog;
                var viewStructClose = modalStructClose.ModalContent as StructureDialog;
                modalStructClose.ModalContent = viewStructClose;
                modalStructClose.IsModal = false;
            });

            if (strucClosed != null)
            {
                strucClosed(this);
            }

        }
        #endregion

        #region EVENTS    
        private void StructureDialog_Loading(FrameworkElement sender, object args)
        {

            this.Structures = CreateSuggestionList();

            //Fill automatically the earthmat dialog if an edit is asked by the user.
            if (parentViewModel.GenericTableName == Dictionaries.DatabaseLiterals.TableStructure && strucViewModel.doStructureUpdate)
            {
                this.strucViewModel.AutoFillDialog(parentViewModel);
                this.pageHeader.Text = this.pageHeader.Text + "  " + parentViewModel.GenericAliasName;

                // spw 2019
                //this.pa.Angle = System.Convert.ToDouble(strucViewModel.StructSymAng);

            }
            else if (!strucViewModel.doStructureUpdate)
            {
                this.pageHeader.Text = this.pageHeader.Text + "  " + this.strucViewModel.StructName;
            }



        }

        /// <summary>
        /// Will close the current dialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void structBackButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //Make sure to delete earthmat, station and location records.
            if (isAQuickStructure)
            {
                strucViewModel.DeleteCascadeOnQuickStructure(parentViewModel);
            }
            CloseControl();
        }

        /// <summary>
        /// Will close dialog and save into the DB entered data by user.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void structSaveButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.structSaveButton.Focus(FocusState.Keyboard);
        }

        /// <summary>
        /// Will show a semantic zoom containing the different structure class-type and details.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void structTypeSearch_Click(object sender, RoutedEventArgs e)
        {
            ContentDialogSemanticZoom newDialog = new ContentDialogSemanticZoom(Dictionaries.DatabaseLiterals.TableStructure, Dictionaries.DatabaseLiterals.FieldStructureClass, Dictionaries.DatabaseLiterals.FieldStructureDetail);
            newDialog.userHasSelectedAValue += strucViewModel.NewDialog_userHasSelectedAValue;
            ContentDialogResult results = await newDialog.ShowAsync();
        }

        #endregion

        private void StructureRelatedCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            if (cb.SelectedValue != null && cb.SelectedValue.ToString() != string.Empty
                && cb.SelectedValue.ToString() != DatabaseLiterals.picklistNACode)
            {
                int strucID = int.Parse(cb.SelectedValue.ToString());
                Structure result = accessData.GetRelatedStructure(strucID);

                if (result != null)
                {
                    if (result.StructureClass != null && !strucType.Text.Contains(result.StructureClass.ToString()))
                    {
                        //int primaryAzimuth = System.Convert.ToInt32(StructureAzimuthNumBox.Text.ToString());
                        int relatedAngle = System.Convert.ToInt32(result.StructureSymAng);
                        //int primaryDip = System.Convert.ToInt32(StructureDipNumBox.Text.ToString());
                        //int relatedDip = System.Convert.ToInt32(result.StructureDipPlunge);

                        //Rebuild model
                        strucViewModel.BuildStructureObject();

                        RelateInfo.Text = result.StructureSymAng + "°/" + result.StructureDipPlunge.ToString() + "°";

                        RotateTransform m_transform = new RotateTransform
                        {
                            Angle = relatedAngle
                        };


                        if (result.StructureClass == Dictionaries.DatabaseLiterals.KeywordLinear)
                        {
                            LinearIcon.RenderTransform = m_transform;
                            LinearIcon.Visibility = Visibility.Visible;

                            PassFailAzimuthTrend();
                            PassFailDip();

                        }
                        else
                        {
                            PlanarIcon.RenderTransform = m_transform;
                            PlanarIcon.Visibility = Visibility.Visible;

                            PassFailAzimuthTrend();
                            PassFailDip();
                        }
                    }
                    else
                    {
                        if (strucType.Text.Contains(Dictionaries.DatabaseLiterals.KeywordPlanar))
                        {
                            LinearIcon.Visibility = Visibility.Collapsed;
                        }
                        else if (strucType.Text.Contains(Dictionaries.DatabaseLiterals.KeywordLinear))
                        {
                            PlanarIcon.Visibility = Visibility.Collapsed;
                        }

                        StructureDipNumBox.BorderBrush = new SolidColorBrush(defaultBorderColor);
                        StructureAzimuthNumBox.BorderBrush = new SolidColorBrush(defaultBorderColor);

                        Debug.WriteLine(StructureAzimuthNumBox.GetType().GetProperty("BorderBrush").GetValue(StructureAzimuthNumBox));
                    }
                }
            }



        }

        /// <summary>
        /// Validate dip/plunge of related structure features
        /// Plunge of linear feature must be equal to or less than dip of planar feature
        /// </summary>
        /// <param name="primaryDip"></param>
        /// <param name="relatedDip"></param>
        private void PassFailDip()
        {
            bool? isDipValid = strucViewModel.structureModel.isRelatedStructuresDipValid;

            if (isDipValid == null || isDipValid == true)
            {
                StructureDipNumBox.BorderBrush = defaultColourBrush;
                this.StructureDipError.Visibility = Visibility.Collapsed;
            }
            else
            {
                //Preserver previous brush
                defaultColourBrush = StructureDipNumBox.BorderBrush;

                //Set to fail border color
                StructureDipNumBox.BorderBrush = failColour;
                this.StructureDipError.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Validate that linear feature falls within the plane of the planar feature.
        /// Azimuths need to be based on right hand rule
        /// </summary>
        /// <param name="numPlanar"></param>
        /// <param name="numLinear"></param>
        private void PassFailAzimuthTrend()
        {
            bool? isAzimValid = strucViewModel.structureModel.isRelatedStructuresAzimuthValid;

            if (isAzimValid == null || isAzimValid == true)
            {
                StructureAzimuthNumBox.BorderBrush = defaultColourBrush;
                this.StructureAzimError.Visibility = Visibility.Collapsed;
            }
            else
            {
                //Preserver previous brush
                defaultColourBrush = StructureDipNumBox.BorderBrush;

                //Set to fail border color
                StructureAzimuthNumBox.BorderBrush = failColour;
                this.StructureAzimError.Visibility = Visibility.Visible;
            }

        }

        public void StructureAzimuthNumBox_TextChanged(object sender, TextBoxTextChangingEventArgs e)
        {
            UpdateSymAng();

        }

        private void StructureDipNumBox_TextChanged(object sender, TextBoxTextChangingEventArgs e)
        {
            TextBox s = sender as TextBox;

            // Catch if no value in the control, such as when a user is making changes
            if (!string.IsNullOrEmpty(s.Text) && int.TryParse(s.Text, out int sAngle))
            {
                sAngle = System.Convert.ToInt32(s.Text);
            }
            else
            {
                sAngle = 0;
            }

            if (StructureRelatedCombobox.SelectedValue != null && StructureRelatedCombobox.SelectedValue.ToString() != String.Empty && strucViewModel.structureModel.relatedStructure != null)
            {
                //string strucID = StructureRelatedCombobox.SelectedValue.ToString();
                //Structure result = accessData.GetRelatedStructure(strucID);

                int primaryDip = System.Convert.ToInt32(sAngle);
                int? relatedDip = strucViewModel.structureModel.relatedStructure.StructureDipPlunge;

                if (strucViewModel.structureModel.StructureClass == Dictionaries.DatabaseLiterals.KeywordLinear)
                {
                    LinearIcon.Visibility = Visibility.Visible;
                    PassFailDip();
                }
                else
                {
                    PlanarIcon.Visibility = Visibility.Visible;
                    PassFailDip();
                }
            }
        }

        public void UpdateSymAng()
        {
            if (StructureFormatCombobox.SelectedIndex != -1)
            {
                string s = StructureAzimuthNumBox.Text;

                // Catch if no value in the control, such as when a user is making changes
                if (!string.IsNullOrEmpty(s) && int.TryParse(s, out int sAngle))
                {
                    sAngle = System.Convert.ToInt32(s);
                }
                else
                {
                    sAngle = 0;
                }


                if (StructureFormatCombobox.SelectedValue.ToString() == "dip dip direction")
                {
                    if (sAngle < 90)
                    {
                        sAngle = 360 + (sAngle - 90);
                    }
                    else
                    {
                        sAngle = sAngle - 90;
                    }

                    this.PlanarIconRotate.Angle = sAngle;
                }
                else
                {
                    this.PlanarIconRotate.Angle = sAngle;
                }

                strucViewModel.structureModel.StructureAzimuth = sAngle;

                PassFailAzimuthTrend();
            }


        }

        private void StructureFormatCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSymAng();
        }

        private void StructureAutoSuggest_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                //var search_term = EarthLithAutoSuggest.Text.ToLower();
                //var results = Rocks.Where(i => i.StartsWith(search_term)).ToList();

                var search_term = StructureAutoSuggest.Text.ToLower();
                var results = Structures.Where(i => i.ToLower().Contains(search_term)).ToList();

                if (results.Count > 0)
                    StructureAutoSuggest.ItemsSource = results;
                else
                    StructureAutoSuggest.ItemsSource = new string[] { "No results found" };
            }

            //Reset structure box
            if (sender.Text == string.Empty)
            {
                strucType.Text = string.Empty;
                strucViewModel.InitFill2ndRound(strucType.Text); //Reset picklist
            }

        }


        private void StructureAutoSuggest_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null && args.ChosenSuggestion.ToString() != "No results found" && sender.Text != string.Empty)
            {
                //strucType.Text = args.ChosenSuggestion.ToString();
                //strucType.Focus(FocusState.Programmatic);

                //Update list that are bound to lithology selection
                strucViewModel.InitFill2ndRound(strucType.Text);

                if (args.ChosenSuggestion.ToString().StartsWith(Dictionaries.DatabaseLiterals.KeywordPlanar))
                {
                    PlanarIcon.Visibility = Visibility.Visible;
                    LinearIcon.Visibility = Visibility.Collapsed;
                }
                else
                {
                    PlanarIcon.Visibility = Visibility.Collapsed;
                    LinearIcon.Visibility = Visibility.Visible;
                }

                //Refresh related list.
                strucViewModel.NewSearch_userHasSelectedAValue(args.ChosenSuggestion.ToString());
            }
            else
            {
                //Reset litho box
                strucType.Text = string.Empty;
            }



        }
        private List<string> CreateSuggestionList()
        {
            Vocabularies vocabularyModel = new Vocabularies();
            string vocQuerySelect = "SELECT * FROM " + Dictionaries.DatabaseLiterals.TableDictionary;
            string vocQueryWhere = " WHERE CODETHEME = 'STRUCDETAIL'";
            string vocQueryVisibility = " AND " + Dictionaries.DatabaseLiterals.TableDictionary + "." + Dictionaries.DatabaseLiterals.FieldDictionaryVisible + " = '" + Dictionaries.DatabaseLiterals.boolYes + "'";
            string vocFinalQuery = vocQuerySelect + vocQueryWhere + vocQueryVisibility;

            List<object> vocResults = accessData.ReadTable(vocabularyModel.GetType(), vocFinalQuery);

            var outResults = new List<string>();
            foreach (Vocabularies tmp in vocResults)
            {
                outResults.Add(tmp.RelatedTo.ToString() + " ; " + tmp.Code.ToString());
            }

            return outResults;
        }

        private void strucType_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox senderBox = sender as TextBox;
            if (senderBox.Text.StartsWith(Dictionaries.DatabaseLiterals.KeywordPlanar))
            {
                PlanarIcon.Visibility = Visibility.Visible;
                LinearIcon.Visibility = Visibility.Collapsed;
            }
            else
            {
                PlanarIcon.Visibility = Visibility.Collapsed;
                LinearIcon.Visibility = Visibility.Visible;
            }

            //Refresh related list.
            strucViewModel.NewSearch_userHasSelectedAValue(senderBox.Text);

        }

        /// <summary>
        /// Event trigger when user selects a new attitude
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StructureAttitudeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Will nullify dip value if user selects a trend as attitude.
            ComboBox senderBox = sender as ComboBox;
            Themes.ComboBoxItem senderItem = senderBox.SelectedItem as Themes.ComboBoxItem;

            if (senderItem != null && senderItem.itemValue == DatabaseLiterals.structurePlanarAttitudeTrend)
            {
                //Nullify dip
                strucViewModel.StructDip = string.Empty;

            }


        }
    }
}
