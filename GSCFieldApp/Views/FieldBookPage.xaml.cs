using GSCFieldApp.ViewModel;
using System.Windows.Input;

namespace GSCFieldApp.Views;

public partial class FieldBookPage : ContentPage
{
    //Events
    public static event EventHandler newFieldBookSaved; //This event is triggered when a new field book has been saved on system

    public FieldBookPage(FieldBookViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
    }


    /// <summary>
    /// Force capital case officer code.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void FieldBookPageOfficerCode_TextChanged(object sender, TextChangedEventArgs e)
    {
        Entry senderEntry = sender as Entry;
        senderEntry.Text = senderEntry.Text.ToUpper();

    }

    ///// <summary>
    ///// Detect when the save button is clicked and send an event
    ///// </summary>
    ///// <param name="sender"></param>
    ///// <param name="e"></param>
    //private void FieldBookSaveButton_Clicked(object sender, EventArgs e)
    //{
    //    newFieldBookSaved?.Invoke(this, null);
    //}

    protected override void OnNavigatingFrom(NavigatingFromEventArgs args)
    {
        newFieldBookSaved?.Invoke(this, null);
    }
}