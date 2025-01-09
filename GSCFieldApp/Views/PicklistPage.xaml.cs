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

    #endregion
}