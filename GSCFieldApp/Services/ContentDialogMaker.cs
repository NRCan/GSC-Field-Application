using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;

namespace GSCFieldApp.Services
{
    public static class ContentDialogMaker
    {
        public static async void CreateContentDialog(ContentDialog Dialog, bool awaitPreviousDialog) { await CreateDialog(Dialog, awaitPreviousDialog); }
        public static async Task<IAsyncOperation<ContentDialogResult>> CreateContentDialogAsync(ContentDialog Dialog, bool awaitPreviousDialog) { return await CreateDialog(Dialog, awaitPreviousDialog); }

        static Task<IAsyncOperation<ContentDialogResult>> CreateDialog(ContentDialog Dialog, bool awaitPreviousDialog)
        {
            if (ActiveDialog != null)
            {
                if (awaitPreviousDialog)
                {
                    ActiveDialog.Hide();
                }
                else
                {
                    switch (Info.Status)
                    {
                        case AsyncStatus.Canceled:
                            
                            break;
                        case AsyncStatus.Completed:
                            Info.Close();
                            break;
                        case AsyncStatus.Error:
                            break;
                        case AsyncStatus.Started:
                            Info.Cancel();
                            break;
                        default:
                            break;
                    }
                }
            }

            ActiveDialog = Dialog;
            ActiveDialog.Closing += ActiveDialog_Closing;
            Info = ActiveDialog.ShowAsync();
            return Task.FromResult(Info);


        }

        private static void ActiveDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            ActiveDialog = null;
        }

        public static IAsyncOperation<ContentDialogResult> Info;
       
        public static ContentDialog ActiveDialog;
    }
}
