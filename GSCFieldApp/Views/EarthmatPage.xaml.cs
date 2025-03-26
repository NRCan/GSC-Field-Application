using GSCFieldApp.Models;
using GSCFieldApp.Controls;
using GSCFieldApp.ViewModel;
using GSCFieldApp.Services;

namespace GSCFieldApp.Views;


public partial class EarthmatPage : ContentPage
{
	public EarthmatPage(EarthmatViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        try
        {
            base.OnNavigatedTo(args);

            //After binding context is setup fill pickers
            EarthmatViewModel vm2 = this.BindingContext as EarthmatViewModel;
            await Task.Run(async () => await vm2.FillPickers());
            await Task.Run(async () => await vm2.InitModel());
            await Task.Run(async () => await vm2.Load()); //In case it is coming from an existing record in field notes
        }
        catch (Exception e)
        {
            new ErrorToLogFile(e).WriteToFile();
        }

    }

    /// <summary>
    /// When a lith detail is selected refine values in lith group/type
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void lihthoSearchResults_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        try
        {
            //Cast
            if (e != null && e.SelectedItem != null)
            {

                EarthmatViewModel vm2 = this.BindingContext as EarthmatViewModel;
                vm2.RefineGroupListFromDetail(e.SelectedItem.ToString());

                lithoSearchBar.Text = e.SelectedItem.ToString();

            }
        }
        catch (Exception lithoSearchException)
        {
            new ErrorToLogFile(lithoSearchException).WriteToFile();
        }

    }

    private async void LithoGroupPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        //Cast
        Picker picker = sender as Picker;
        if (picker != null && picker.SelectedItem != null)
        {

            EarthmatViewModel vm2 = this.BindingContext as EarthmatViewModel;
            vm2.RefineDetailListFromGroup(picker.SelectedItem as ComboBoxItem);

            await vm2.Fill2ndRoundPickers();

        }
    }

    private void Entry_TextChanged(object sender, TextChangedEventArgs e)
    {
        //Cast and call method to calculate residual of all earth mat
        Entry senderBox = sender as Entry;
        if (senderBox != null && senderBox.Text != string.Empty)
        {
            EarthmatViewModel vm = this.BindingContext as EarthmatViewModel;
            vm.CalculateResidual(senderBox.Text);
        }
    }

    /// <summary>
    /// Will filter down values while using is typing text in, else
    /// they need to tap search icon in order to initiate the filtering
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void lithoSearchBar_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            SearchBar searchBar = sender as SearchBar;
            if (searchBar != null)
            {
                if (searchBar.Text != null && searchBar.Text != string.Empty)
                {
                    this.lithoSearchBar.SearchCommand.Execute(searchBar.Text);
                }

            }
        }
        catch (Exception searchBarException)
        {
            new ErrorToLogFile(searchBarException).WriteToFile();
        }

    }

}