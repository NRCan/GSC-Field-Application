﻿using GSCFieldApp.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Template10.Common;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using GSCFieldApp.Models;
using System.Diagnostics;
using GSCFieldApp.Services.DatabaseServices;
using Windows.Storage;
using Windows.UI;

//using SQLite.Net;
//using SQLite.Net.Platform.WinRT;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace GSCFieldApp.Views
{
    public sealed partial class StructureDialog : UserControl
    {
        public  StructureViewModel strucViewModel { get; set; }
        public FieldNotes parentViewModel { get; set; }

        public bool isAQuickStructure = false;

        private DataAccess accessData = new DataAccess();

        private SolidColorBrush passColour =  new SolidColorBrush(Windows.UI.Colors.LightGreen);
        private SolidColorBrush failColour = new SolidColorBrush(Windows.UI.Colors.Red);
        private Color defaultBorderColor;

        public int testing;

        public StructureDialog(FieldNotes inDetailViewModel, bool isQuickStructure)
        {
            parentViewModel = inDetailViewModel;
            isAQuickStructure = isQuickStructure;

            this.InitializeComponent();

            strucViewModel = new StructureViewModel(inDetailViewModel);

            this.Loading += StructureDialog_Loading;
            this.structSaveButton.GotFocus += StructSaveButton_GotFocus;

            SolidColorBrush defaultBorderBrush = this.strucType.BorderBrush as SolidColorBrush;
            defaultBorderColor = defaultBorderBrush.Color;
            //testing = 75;
        }

        private void StructSaveButton_GotFocus(object sender, RoutedEventArgs e)
        {
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
        }
        #endregion

        #region EVENTS    
        private void StructureDialog_Loading(FrameworkElement sender, object args)
        {
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
            this.structSaveButton.Focus(FocusState.Programmatic);
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

        /// <summary>
        /// Display appropriate generic planar or linear symbol
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void StrucType_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            if (sender.Text.StartsWith(Dictionaries.DatabaseLiterals.KeywordPlanar)) 
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
            strucViewModel.FillStructureRelated(sender.Text);
        }

        private void StructureRelatedCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            if (cb.SelectedValue != null)
            {
                string strucID = cb.SelectedValue.ToString();
                Structure result = accessData.GetRelatedStructure(strucID);

                if (result != null)
                {
                    if (!strucType.Text.Contains(result.StructureClass.ToString()))
                    {
                        //int primaryAzimuth = System.Convert.ToInt32(StructureAzimuthNumBox.Text.ToString());
                        int relatedAngle = System.Convert.ToInt32(result.StructureSymAng);
                        //int primaryDip = System.Convert.ToInt32(StructureDipNumBox.Text.ToString());
                        //int relatedDip = System.Convert.ToInt32(result.StructureDipPlunge);

                        //Rebuild model
                        strucViewModel.BuildStructureObject();

                        RelateInfo.Text = result.StructureSymAng + "°/" + result.StructureDipPlunge.ToString() + "°";

                        RotateTransform m_transform = new RotateTransform();
                        m_transform.Angle = relatedAngle;


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
                StructureDipNumBox.BorderBrush = passColour;
            }
            else
            {
                StructureDipNumBox.BorderBrush = failColour;
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
                StructureAzimuthNumBox.BorderBrush = passColour;
            }
            else
            {
                StructureAzimuthNumBox.BorderBrush = failColour;
            }

        }

        public void StructureAzimuthNumBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateSymAngAsync();
            
        }

        private void StructureDipNumBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox s = sender as TextBox;
            int sAngle;

            // Catch if no value in the control, such as when a user is making changes
            if (!string.IsNullOrEmpty(s.Text) && int.TryParse(s.Text, out sAngle))
            {
                sAngle = System.Convert.ToInt32(s.Text);
            }
            else
            {
                sAngle = 0;
            }

            if (StructureRelatedCombobox.SelectedValue != null)
            {
                //string strucID = StructureRelatedCombobox.SelectedValue.ToString();
                //Structure result = accessData.GetRelatedStructure(strucID);

                int primaryDip = System.Convert.ToInt32(sAngle);
                int relatedDip;
                int.TryParse(strucViewModel.structureModel.relatedStructure.StructureDipPlunge, out relatedDip);

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

        public async void UpdateSymAngAsync()
        {
            if (StructureFormatCombobox.SelectedIndex != -1)
            {
                string s = StructureAzimuthNumBox.Text;
                int sAngle;

                // Catch if no value in the control, such as when a user is making changes
                if (!string.IsNullOrEmpty(s) && int.TryParse(s, out sAngle))
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

                strucViewModel.structureModel.StructureAzimuth = sAngle.ToString();

                PassFailAzimuthTrend();
            }

           
        }

        private void StructureFormatCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSymAngAsync();
        }


    }
}
