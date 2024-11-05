using GSCFieldApp.Services;
using GSCFieldApp.ViewModel;

namespace GSCFieldApp.Views;

public partial class LineworkPage : ContentPage
{
    public LineworkPage(LineworkViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        //After binding context is setup fill pickers
        LineworkViewModel vm2 = this.BindingContext as LineworkViewModel;
        await vm2.FillPickers();
        await vm2.InitModel();
        await vm2.Load(); //In case it is coming from an existing record in field notes

    }
}
