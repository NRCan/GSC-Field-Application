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
    private async void lihthoGroupSearchResults_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        //Cast
        ListView listView = sender as ListView;
        if (listView != null && listView.SelectedItem != null)
        {
            if (listView.SelectedItem.ToString() != string.Empty)
            {
                EarthmatViewModel vm2 = this.BindingContext as EarthmatViewModel;
                vm2.RefineDetailListFromGroup(listView.SelectedItem.ToString());

                //In theory we should reset detail value, but it gets annoying when clicking detail
                //Then clicking group type then reclicking detail because it disapeared

                //lithoSearchBar.Text = string.Empty; //Reset detail search bar because list will be refreshed with new values

                vm2.isLithoGroupListVisible = false;
                await vm2.Fill2ndRoundPickers();

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