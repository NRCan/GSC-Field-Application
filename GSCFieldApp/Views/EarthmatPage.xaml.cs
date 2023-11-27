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
        //await vm2.FillPickers();
        //await vm2.SetFieldVisibility();
        //await vm2.Load(); //In case it is coming from an existing record in field notes
    }

    private void lihthoGroupSearchResults_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        //Cast
        ListView listView = sender as ListView;
        if (listView != null && listView.SelectedItem != null)
        {
            if (listView.SelectedItem.ToString() != string.Empty)
            {
                EarthmatViewModel vm2 = this.BindingContext as EarthmatViewModel;

                vm2.SelectedLithoGroup = listView.SelectedItem.ToString();
                vm2.isLithoGroupListVisible = false;
            }
        }
    }

    private void lihthoSearchResults_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        //Cast
        ListView listView = sender as ListView;
        if (listView != null && listView.SelectedItem != null)
        {
            if (listView.SelectedItem.ToString() != string.Empty)
            {
                EarthmatViewModel vm2 = this.BindingContext as EarthmatViewModel;

                vm2.SelectedLithoDetail = listView.SelectedItem.ToString();
                vm2.isLithoDetailListVisible = false;
            }
        }
    }
}