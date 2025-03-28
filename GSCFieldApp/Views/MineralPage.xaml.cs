using GSCFieldApp.Dictionaries;
using GSCFieldApp.Services;
using GSCFieldApp.ViewModel;

namespace GSCFieldApp.Views;

public partial class MineralPage : ContentPage
{
	public MineralPage(MineralViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;

	}

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

		MineralViewModel viewModel = BindingContext as MineralViewModel;
		await viewModel.FillPickers();
        await viewModel.InitModel();
        await viewModel.Load();
    }

    /// <summary>
    /// From user selected mineral name in list view, apply item to search bar
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void mineralSearchList_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        //Cast
        if (e != null && e.SelectedItem != null)
        {
            mineralNameSearchBar.Text = e.SelectedItem.ToString();

            //Easter egg
            if (e.SelectedItem.ToString() == DatabaseLiterals.easterEggMineral)
            {
                this.mineralEasterEgg.IsVisible = true;

                //Wait 10 sec and remove
                Task.Delay(10000).ContinueWith(t => { this.mineralEasterEgg.IsVisible = false; });
            }
        }
    }

    /// <summary>
    /// Will filter down the mineral list while user is typing, 
    /// else they need to tap the search icon in order to initiate the filtering
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void mineralNameSearchBar_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            SearchBar searchBar = sender as SearchBar;
            if (searchBar != null)
            {
                if (searchBar.Text != null && searchBar.Text != string.Empty)
                {
                    this.mineralNameSearchBar.SearchCommand.Execute(searchBar.Text);
                }

            }
        }
        catch (Exception searchBarException)
        {
            new ErrorToLogFile(searchBarException).WriteToFile();
        }
    }
}