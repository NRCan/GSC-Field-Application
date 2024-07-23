using GSCFieldApp.Services;
using GSCFieldApp.ViewModel;
using System;

namespace GSCFieldApp.Views;

public partial class StructurePage : ContentPage
{
    public LocalizationResourceManager LocalizationResourceManager
    => LocalizationResourceManager.Instance; // Will be used for in code dynamic local strings


    public StructurePage(StructureViewModel vm)
	{
        InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        //After binding context is setup fill pickers
        StructureViewModel vm2 = this.BindingContext as StructureViewModel;
        await vm2.FillPickers();
        await vm2.InitModel();
        await vm2.Load(); //In case it is coming from an existing record in field notes
    }
}