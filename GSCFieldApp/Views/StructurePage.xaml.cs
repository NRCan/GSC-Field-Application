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
}