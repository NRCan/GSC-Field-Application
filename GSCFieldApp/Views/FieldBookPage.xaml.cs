using GSCFieldApp.ViewModel;
using System.Windows.Input;

namespace GSCFieldApp.Views;

public partial class FieldBookPage : ContentPage
{

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
}