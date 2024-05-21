using GSCFieldApp.Models;
using GSCFieldApp.Themes;
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

        ////Overide title 
        //if (vm2 != null && vm2.Earthmaterial != null && vm2.Earthmaterial.EarthMatName != string.Empty)
        //{
        //    this.Title = vm2.Earthmaterial.EarthMatName;
        //}
    }

    /// <summary>
    /// When a lith detail is selected refine values in lith group/type
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void lihthoSearchResults_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        //Cast
        if (e != null && e.SelectedItem != null)
        {

            EarthmatViewModel vm2 = this.BindingContext as EarthmatViewModel;
            vm2.RefineGroupListFromDetail(e.SelectedItem.ToString());

            lithoSearchBar.Text = e.SelectedItem.ToString();

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


}