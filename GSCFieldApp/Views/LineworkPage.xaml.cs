using GSCFieldApp.Controls;
using GSCFieldApp.Services;
using GSCFieldApp.ViewModel;

namespace GSCFieldApp.Views;

public partial class LineworkPage : ContentPage
{
    public LineworkPage(LineworkViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        //After binding context is setup fill pickers
        LineworkViewModel vm2 = this.BindingContext as LineworkViewModel;
        await vm2.FillPickers();
        await vm2.InitModel();
        await vm2.Load(); //In case it is coming from an existing record in field notes

    }

    #region EVENTS

    /// <summary>
    /// Change chosen color based on selected linetype
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void LineworkTypePicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        Picker senderBox = sender as Picker;

        if (senderBox != null && senderBox.SelectedIndex != -1)
        {
            LineworkViewModel vm3 = this.BindingContext as LineworkViewModel;
            vm3.SelectColorBasedOnLineType();
        }
    }

    #endregion
}
