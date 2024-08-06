using GSCFieldApp.ViewModel;

namespace GSCFieldApp.Views;

public partial class PicklistPage : ContentPage
{
	public PicklistPage(PicklistViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}

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

    private void PicklistPageFieldsPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        //Cast and make sure something valid is selected
        Picker senderPicker = sender as Picker;
        if (senderPicker.SelectedIndex >= 0)
        {
            PicklistViewModel vm3 = (PicklistViewModel)BindingContext;
            vm3.FillFieldValuesPicklist();
        }
    }
}