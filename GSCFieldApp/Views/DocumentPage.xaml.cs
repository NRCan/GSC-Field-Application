using GSCFieldApp.Services;
using GSCFieldApp.ViewModel;

namespace GSCFieldApp.Views;

public partial class DocumentPage : ContentPage
{

    public DocumentPage(DocumentViewModel vm)
	{
        try
        {
            InitializeComponent();
            BindingContext = vm;

            this.Focused += DocumentPage_Focused;

        }
        catch (Exception e)
        {
            new ErrorToLogFile(e).WriteToFile();
        }

    }

    private void DocumentPage_Focused(object sender, FocusEventArgs e)
    {
        //Quick thumbnail validation
        DocumentViewModel vmDoc = this.BindingContext as DocumentViewModel;
        if (vmDoc.imageTapped)
        {
            vmDoc.UpdateThumbnail();
        }
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {

        try
        {
            base.OnNavigatedTo(args);

            DocumentViewModel vm2 = this.BindingContext as DocumentViewModel;
            //After binding context is setup fill pickers
            if (vm2 != null && vm2.SaveCommand.ExecutionTask == null && vm2.SaveStayCommand.ExecutionTask == null)
            {
                await Task.Run(async () => await vm2.FillPickers());
                await Task.Run(async () => await vm2.InitModel());
                await vm2.Load(); //In case it is coming from an existing record in field notes
            }

        }
        catch (Exception e)
        {
            new ErrorToLogFile(e).WriteToFile();
        }


    }

    /// <summary>
    /// Whenever the user selects a new document type, recalculate the file name
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void DocumentFileTypePicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        DocumentViewModel vm3 = this.BindingContext as DocumentViewModel;
        if (vm3 != null && !vm3.IsProcessingBatch)
        {
            vm3.CalculateFileName();
        }
        
    }

    /// <summary>
    /// Whenever user enters a new file from number, recalculate the file name
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void DocumentPageFileFromEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        DocumentViewModel vm4 = this.BindingContext as DocumentViewModel;
        if (vm4 != null && !vm4.IsProcessingBatch)
        {
            vm4.CalculateFileName();
            vm4.CalculateFileNumberTo(); //Make sure File number to fits the from number
        }

    }

}