using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Property_and_Management.Src.DataTransferObjects;
using Property_and_Management.Src.Viewmodels;

namespace Property_and_Management.Src.Views
{
    public sealed partial class ListingsPage : Page
    {
        public ListingsViewModel ViewModel { get; private set; }

        public ListingsPage()
        {
            this.InitializeComponent();
        }

        private void CreateGameButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            this.Frame.Navigate(typeof(CreateGameView));
        }

        private void EditGameButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            var clickedButton = sender as Button;
            var gameToEdit = clickedButton?.Tag as GameDTO;

            if (gameToEdit != null)
            {
                this.Frame.Navigate(typeof(EditGameView), gameToEdit.Id);
            }
        }

        private async void DeleteGameButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            var clickedButton = sender as Button;
            var gameToDelete = clickedButton?.Tag as GameDTO;

            if (gameToDelete == null)
            {
                return;
            }

            var deleteConfirmationResult = await DialogHelper.ShowConfirmationAsync(
                this.XamlRoot,
                Constants.DialogTitles.DeleteGameConfirmation,
                $"Are you sure you want to permanently delete '{gameToDelete.Name}'? Pending requests will be cancelled and notified. Deletion is blocked if active or upcoming rentals exist.",
                Constants.DialogButtons.Delete,
                Constants.DialogButtons.Cancel,
                ContentDialogButton.Close);

            if (deleteConfirmationResult == ContentDialogResult.Primary)
            {
                var gameDeletionResult = ViewModel.TryDeleteGame(gameToDelete);
                if (!string.IsNullOrWhiteSpace(gameDeletionResult.DialogMessage))
                {
                    await DialogHelper.ShowMessageAsync(
                        this.XamlRoot,
                        gameDeletionResult.DialogTitle,
                        gameDeletionResult.DialogMessage);
                }
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs navigationEventArgs)
        {
            base.OnNavigatedTo(navigationEventArgs);

            ViewModel = App.Services.GetRequiredService<ListingsViewModel>();
            this.DataContext = ViewModel;
        }

        private void PrevButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            ViewModel?.PrevPage();
        }

        private void NextButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            ViewModel?.NextPage();
        }

        private void Image_ImageFailed(object sender, ExceptionRoutedEventArgs exceptionRoutedEventArgs)
        {
            ImageFailureHandler.HandleFailure(sender as Image, Resources);
        }
    }
}