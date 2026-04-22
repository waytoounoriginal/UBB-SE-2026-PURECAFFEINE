using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Property_and_Management.Src.Viewmodels;

namespace Property_and_Management.Src.Views
{
    public sealed partial class RequestsToOthersPage : Page
    {
        public RequestsToOthersPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs navigationEventArgs)
        {
            base.OnNavigatedTo(navigationEventArgs);

            if (navigationEventArgs.Parameter is RequestsToOthersViewModel requestsToOthersViewModel)
            {
                DataContext = requestsToOthersViewModel;
                return;
            }

            if (DataContext is not RequestsToOthersViewModel)
            {
                DataContext = App.Services.GetRequiredService<RequestsToOthersViewModel>();
            }
        }

        private void CancelButton_Tapped(object sender, TappedRoutedEventArgs tappedRoutedEventArgs)
        {
            tappedRoutedEventArgs.Handled = true;
        }

        private async void CancelButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            if (sender is not Button clickedButton || clickedButton.Tag is not int requestId)
            {
                return;
            }

            var cancelConfirmationResult = await DialogHelper.ShowConfirmationAsync(
                this.XamlRoot,
                Constants.DialogTitles.CancelRequestConfirmation,
                "Are you sure you want to cancel this request?",
                Constants.DialogButtons.CancelRequest,
                Constants.DialogButtons.GoBack,
                ContentDialogButton.Close);

            if (cancelConfirmationResult != ContentDialogResult.Primary)
            {
                return;
            }

            var pageViewModel = DataContext as RequestsToOthersViewModel;
            var cancelErrorMessage = pageViewModel?.TryCancelRequest(requestId);
            if (cancelErrorMessage != null)
            {
                await DialogHelper.ShowMessageAsync(
                    this.XamlRoot,
                    Constants.DialogTitles.CancelRequestConfirmation,
                    cancelErrorMessage);
            }
        }

        private void CreateRequestButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            Frame?.Navigate(typeof(CreateRequestView));
        }

        private void Image_ImageFailed(object sender, ExceptionRoutedEventArgs exceptionRoutedEventArgs)
        {
            ImageFailureHandler.HandleFailure(sender as Image, Resources);
        }

        private void NextButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            (DataContext as RequestsToOthersViewModel)?.NextPage();
        }

        private void PrevButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            (DataContext as RequestsToOthersViewModel)?.PrevPage();
        }
    }
}