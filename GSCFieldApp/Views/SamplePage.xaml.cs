using GSCFieldApp.Services;
using GSCFieldApp.ViewModel;
using System;

namespace GSCFieldApp.Views;

public partial class SamplePage : ContentPage
{
    public LocalizationResourceManager LocalizationResourceManager
        => LocalizationResourceManager.Instance; // Will be used for in code dynamic local strings

    public SamplePage(SampleViewModel vm)
	{
        InitializeComponent();
        BindingContext = vm;

        this.Loaded += SamplePage_Loaded;
    }

    private async void SamplePage_Loaded(object sender, EventArgs e)
    {
        //Conditional visual remainder for surficial to take duplicate or blank sample
        SampleViewModel vm3 = this.BindingContext as SampleViewModel;
        bool needReminder = await vm3.DuplicateReminder();

        if (needReminder)
        {
            await Shell.Current.DisplayAlert(LocalizationResourceManager["SamplePageDuplicateReminderTitle"].ToString(),
                    LocalizationResourceManager["SamplePageDuplicateReminderMessage"].ToString(),
                    LocalizationResourceManager["GenericButtonOk"].ToString());
        }
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        //After binding context is setup fill pickers
        SampleViewModel vm2 = this.BindingContext as SampleViewModel;
        await vm2.FillPickers();
        await vm2.Load(); //In case it is coming from an existing record in field notes

        ////Overide title 
        //if (vm2 != null && vm2.Earthmaterial != null && vm2.Earthmaterial.EarthMatName != string.Empty)
        //{
        //    this.Title = vm2.Earthmaterial.EarthMatName;
        //}
    }

}