using GSCFieldApp.ViewModel;
using System.Windows.Input;

namespace GSCFieldApp.Views;

public partial class FieldBookPage : ContentPage
{
    //Events
    public static event EventHandler newFieldBookSaved; //This event is triggered when a new field book has been saved on system

    public FieldBookPage(FieldBookViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
    }

    protected override void OnNavigatingFrom(NavigatingFromEventArgs args)
    {
        newFieldBookSaved?.Invoke(this, null);
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        
        FieldBookViewModel vm = (FieldBookViewModel)BindingContext;
        await vm.Load();
    }
}