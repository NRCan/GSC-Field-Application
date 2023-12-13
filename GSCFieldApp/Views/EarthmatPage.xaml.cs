using GSCFieldApp.Models;
using GSCFieldApp.ViewModel;

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
        base.OnNavigatedTo(args);

        //After binding context is setup fill pickers
        EarthmatViewModel vm2 = this.BindingContext as EarthmatViewModel;
        await vm2.FillPickers();
        //await vm2.SetFieldVisibility();
        await vm2.Load(); //In case it is coming from an existing record in field notes
    }


    /// <summary>
    /// When a group item is selected, refine values in lith details
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void lihthoGroupSearchResults_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        //Cast
        ListView listView = sender as ListView;
        if (listView != null && listView.SelectedItem != null)
        {
            if (listView.SelectedItem.ToString() != string.Empty)
            {
                EarthmatViewModel vm2 = this.BindingContext as EarthmatViewModel;
                vm2.RefineDetailListFromGroup(listView.SelectedItem.ToString());
                vm2.SelectedLithoGroup = listView.SelectedItem.ToString();
                lithoSearchBar.Text = string.Empty;
                vm2.isLithoGroupListVisible = false;

            }
        }
    }


    /// <summary>
    /// When a lith detail is selected refine values in lith group/type
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void lihthoSearchResults_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        //Cast
        ListView listView = sender as ListView;
        if (listView != null && listView.SelectedItem != null)
        {
            if (listView.SelectedItem.ToString() != string.Empty)
            {
                EarthmatViewModel vm2 = this.BindingContext as EarthmatViewModel;
                vm2.RefineGroupListFromDetail(listView.SelectedItem.ToString());
                lithoSearchBar.Text = listView.SelectedItem.ToString();
                vm2.isLithoDetailListVisible = false;

            }
        }
    }
}