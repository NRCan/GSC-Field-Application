using GSCFieldApp.ViewModel;

namespace GSCFieldApp.Views;

public partial class SamplePage : ContentPage
{
	public SamplePage(SampleViewModel vm)
	{
        InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        //After binding context is setup fill pickers
        SampleViewModel vm2 = this.BindingContext as SampleViewModel;
        await vm2.FillPickers();
        
        //await vm2.Load(); //In case it is coming from an existing record in field notes

        ////Overide title 
        //if (vm2 != null && vm2.Earthmaterial != null && vm2.Earthmaterial.EarthMatName != string.Empty)
        //{
        //    this.Title = vm2.Earthmaterial.EarthMatName;
        //}
    }
}