using GSCFieldApp.Models;
using GSCFieldApp.ViewModel;

namespace GSCFieldApp.Views;

public partial class PicklistPage : ContentPage
{
	public PicklistPage(PicklistViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;

        //Special event to force a scroll to added new term if any
        vm.PicklistValues.CollectionChanged += PicklistValues_CollectionChanged;
	}

    #region EVENTS

    /// <summary>
    /// New value added, force refresh on collection view scroll so it falls at the right place
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void PicklistValues_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
        {
            int index = (BindingContext as PicklistViewModel).PicklistValues.IndexOf(e.NewItems[0] as Vocabularies);
            this.PicklistCollectionControl.ScrollTo(index);
        }
    }

    /// <summary>
    /// New table selection, force refresh on fields.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void PicklistPageTablesPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
		//Cast and make sure something valid is selected
		Picker senderPicker = sender as Picker;
		if (senderPicker.SelectedIndex >= 0)
		{
            PicklistViewModel vm2 = (PicklistViewModel)BindingContext;
            vm2.FillFieldsPicklist();
        }

    }

    /// <summary>
    /// New field selection, force refresh on values
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void PicklistPageFieldsPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        //Cast and make sure something valid is selected
        Picker senderPicker = sender as Picker;
        if (senderPicker.SelectedIndex >= 0)
        {
            PicklistViewModel vm3 = (PicklistViewModel)BindingContext;
            bool doesHaveParents = await vm3.FillFieldParentValuesPicklist();
            if (!doesHaveParents)
            {
                //Instead fill field values
                vm3.FillFieldValuesPicklist();
            }
            
        }
    }

    /// <summary>
    /// Will refill the picklist collection based on selected parent.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void PicklistPageParentValuePicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        //Cast and make sure something valid is selected
        Picker senderPicker = sender as Picker;
        if (senderPicker.SelectedIndex >= 0)
        {
            PicklistViewModel vm4 = (PicklistViewModel)BindingContext;
            vm4.FillFieldValuesPicklist();
        }
    }

    #endregion
}