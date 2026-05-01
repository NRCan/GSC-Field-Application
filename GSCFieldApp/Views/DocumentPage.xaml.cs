using GSCFieldApp.Services;
using GSCFieldApp.ViewModel;
using System.ComponentModel;

namespace GSCFieldApp.Views;

public partial class DocumentPage : ContentPage
{
    private Editor _captionEditor;

    public DocumentPage(DocumentViewModel vm)
    {
        try
        {
            InitializeComponent();
            BindingContext = vm;
            App.AppBecameActive += OnAppBecameActive;
            
            // Store reference to caption editor
            _captionEditor = this.FindByName<Editor>("DocumentPageCaptionEditor");
            
            // Subscribe to view model's copy event
            if (vm != null)
            {
                vm.CaptionFocusRequested += OnCaptionFocusRequested;
            }
        }
        catch (Exception e)
        {
            new ErrorToLogFile(e).WriteToFile();
        }
    }

    private async void OnAppBecameActive()
    {
        //Quick thumbnail validation
        DocumentViewModel vmDoc = this.BindingContext as DocumentViewModel;
        if (vmDoc?.IsImageTapped == true)
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
            if (!vm2.IsLoaded)
            {
                //After binding context is setup fill pickers
                if (vm2 != null && vm2.SaveCommand.ExecutionTask == null && vm2.SaveStayCommand.ExecutionTask == null)
                {
                    await vm2.FillPickers();
                    await vm2.InitModel();
                    await vm2.Load(); //In case it is coming from an existing record in field notes
                }
            }
        }
        catch (Exception e)
        {
            new ErrorToLogFile(e).WriteToFile();
        }
    }

    private void DocumentFileTypePicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        DocumentViewModel vm3 = this.BindingContext as DocumentViewModel;
        if (vm3 != null && !vm3.IsProcessingBatch)
        {
            vm3.CalculateFileName();
        }
    }

    private void DocumentPageFileFromEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        DocumentViewModel vm4 = this.BindingContext as DocumentViewModel;
        if (vm4 != null && !vm4.IsProcessingBatch)
        {
            vm4.CalculateFileName();
            vm4.CalculateFileNumberTo();
        }
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);
        App.AppBecameActive -= OnAppBecameActive;
        
        // Unsubscribe from view model events
        if (BindingContext is DocumentViewModel vm)
        {
            vm.CaptionFocusRequested -= OnCaptionFocusRequested;
        }
    }

    /// <summary>
    /// Event handler for caption focus request from view model
    /// </summary>
    private async void OnCaptionFocusRequested(object sender, EventArgs e)
    {
        await FocusAndSelectCaptionAsync();
    }

    /// <summary>
    /// Focus caption field and select all text
    /// </summary>
    public async Task FocusAndSelectCaptionAsync()
    {
        if (_captionEditor == null)
        {
            _captionEditor = this.FindByName<Editor>("DocumentPageCaptionEditor");
        }

        if (_captionEditor != null)
        {
            // Give the UI time to update before focusing
            await Task.Delay(100);
            
            _captionEditor.Focus();
            
            // Select all text if it exists
            if (!string.IsNullOrEmpty(_captionEditor.Text))
            {
                _captionEditor.CursorPosition = 0;
                _captionEditor.SelectionLength = _captionEditor.Text.Length;
            }
        }
    }
}