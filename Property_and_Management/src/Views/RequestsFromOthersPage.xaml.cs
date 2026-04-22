using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Property_and_Management.Src.DataTransferObjects;
using Property_and_Management.Src.Viewmodels;

namespace Property_and_Management.Src.Views
{
    public sealed partial class RequestsFromOthersPage : Page
    {
        private const double DenyReasonInputMinimumWidth = 360;
        private const double DenyDialogContentSpacing = 8;

        public RequestsFromOthersPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs navigationEventArgs)
        {
            base.OnNavigatedTo(navigationEventArgs);

            if (navigationEventArgs.Parameter is RequestsFromOthersViewModel requestsFromOthersViewModel)
            {
                DataContext = requestsFromOthersViewModel;
                return;
            }

            if (DataContext is not RequestsFromOthersViewModel)
            {
                DataContext = App.Services.GetRequiredService<RequestsFromOthersViewModel>();
            }
        }

        private void OfferButton_Tapped(object sender, TappedRoutedEventArgs tappedRoutedEventArgs)
        {
            tappedRoutedEventArgs.Handled = true;
        }

        private void DenyButton_Tapped(object sender, TappedRoutedEventArgs tappedRoutedEventArgs)
        {
            tappedRoutedEventArgs.Handled = true;
        }

        private async void OfferButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            if (sender is not Button clickedButton || clickedButton.Tag is not int requestId)
            {
                return;
            }

            var tappedRequestDto = clickedButton.DataContext as RequestDTO;
            var requestedGameName = tappedRequestDto?.Game?.Name ?? "this game";
            var requesterDisplayName = tappedRequestDto?.Renter?.DisplayName ?? "the requester";

            var offerConfirmationResult = await DialogHelper.ShowConfirmationAsync(
                this.XamlRoot,
                Constants.DialogTitles.OfferGameConfirmation,
                $"Offer {requestedGameName} to {requesterDisplayName} for {tappedRequestDto?.StartDateDisplayLong} - {tappedRequestDto?.EndDateDisplayLong}? This will approve the request and create the rental immediately.",
                Constants.DialogButtons.Offer,
                Constants.DialogButtons.Cancel,
                ContentDialogButton.Primary);

            if (offerConfirmationResult != ContentDialogResult.Primary)
            {
                return;
            }

            var pageViewModel = DataContext as RequestsFromOthersViewModel;
            var offerErrorMessage = pageViewModel?.TryOfferGame(requestId);
            if (offerErrorMessage != null)
            {
                await DialogHelper.ShowMessageAsync(this.XamlRoot, Constants.DialogTitles.OfferFailed, offerErrorMessage);
            }
        }

        private async void DenyButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            if (sender is not Button clickedButton || clickedButton.Tag is not int requestId)
            {
                return;
            }

            var tappedRequestDto = clickedButton.DataContext as RequestDTO;
            var requestedGameName = tappedRequestDto?.Game?.Name ?? "this game";
            var requesterDisplayName = tappedRequestDto?.Renter?.DisplayName ?? "the requester";

            var denyReasonTextBox = new TextBox
            {
                PlaceholderText = "Optional reason (e.g. unavailable in this period)",
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                MinWidth = DenyReasonInputMinimumWidth
            };

            var denyDialogContentPanel = new StackPanel { Spacing = DenyDialogContentSpacing };
            denyDialogContentPanel.Children.Add(new TextBlock
            {
                Text = $"Decline request for {requestedGameName} from {requesterDisplayName}?"
            });
            denyDialogContentPanel.Children.Add(denyReasonTextBox);

            var denyConfirmationResult = await DialogHelper.ShowConfirmationAsync(
                this.XamlRoot,
                Constants.DialogTitles.DeclineRequestConfirmation,
                denyDialogContentPanel,
                Constants.DialogButtons.Decline,
                Constants.DialogButtons.Cancel,
                ContentDialogButton.Primary);

            if (denyConfirmationResult != ContentDialogResult.Primary)
            {
                return;
            }

            var pageViewModel = DataContext as RequestsFromOthersViewModel;
            var denyErrorMessage = pageViewModel?.TryDenyRequest(requestId, denyReasonTextBox.Text);
            if (denyErrorMessage != null)
            {
                await DialogHelper.ShowMessageAsync(this.XamlRoot, Constants.DialogTitles.DeclineFailed, denyErrorMessage);
            }
        }

        private void Image_ImageFailed(object sender, ExceptionRoutedEventArgs exceptionRoutedEventArgs)
        {
            ImageFailureHandler.HandleFailure(sender as Image, Resources);
        }

        private void NextButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            (DataContext as RequestsFromOthersViewModel)?.NextPage();
        }

        private void PrevButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            (DataContext as RequestsFromOthersViewModel)?.PrevPage();
        }
    }
}