using CommunityToolkit.Maui.Core.Views;
using GSCFieldApp.ViewModel;

namespace GSCFieldApp.Views;

public partial class FieldBooksPage : ContentPage
{
    private bool _isInitialized = false;

    public FieldBooksPage(FieldBooksViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
        this.Loaded += FieldBooksPage_Loaded;
	}

    private void FieldBooksPage_Loaded(object sender, EventArgs e)
    {
        _isInitialized = true;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        //Make sure to refresh field books in case something has changed
        if (_isInitialized)
        {
            FieldBooksViewModel fieldBooksViewModel = (FieldBooksViewModel)BindingContext;
            fieldBooksViewModel.FillBookCollectionAsync();
        }

    }

}