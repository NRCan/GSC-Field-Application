using GSCFieldApp.Models;
using GSCFieldApp.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace GSCFieldApp.Views
{
    public sealed partial class DrillHoleDialog : UserControl
    {
        public DrillHoleViewModel drillViewModel { get; set; }
        public FieldNotes parentDrillReport { get; set; }

        public delegate void drillCloseWithoutSaveEventHandler(object sender); //A delegate for execution events
        public event drillCloseWithoutSaveEventHandler drillClosed; //This event is triggered when a save has been done on station table.

        public FieldLocation mapPosition { get; set; }

        public DrillHoleDialog(FieldNotes inParentReport)
        {
            {
                if (inParentReport != null)
                {
                    parentDrillReport = inParentReport;
                }

                this.InitializeComponent();
                this.drillViewModel = new DrillHoleViewModel(inParentReport);
            }
        }

        public void drillBackButton_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }

        public void drillSaveButton_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }

        public void ConcatValueCheck_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }

        private void DrillDipNumBox_TextChanged(TextBox sender, TextBoxTextChangingEventArgs args)
        {

        }

        private void DrillAzimuthNumBox_TextChanged(TextBox sender, TextBoxTextChangingEventArgs args)
        {

        }
    }
}
