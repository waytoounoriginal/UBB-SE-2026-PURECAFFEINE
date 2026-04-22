using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Property_and_Management;
using Property_and_Management.Src.DataTransferObjects;
using Property_and_Management.Src.Viewmodels;

namespace Property_and_Management.Src.Views
{
    public sealed partial class CreateRentalView : Page
    {
        public CreateRentalViewModel ViewModel { get; }

        public CreateRentalView()
        {
            ViewModel = App.Services.GetRequiredService<CreateRentalViewModel>();
            this.InitializeComponent();

            GamePicker.ItemsSource = ViewModel.OwnedActiveGames;
            RenterPicker.ItemsSource = ViewModel.AvailableRenters;
            StartDatePicker.MinDate = DateTimeOffset.Now;
            EndDatePicker.MinDate = DateTimeOffset.Now;
        }

        private void GamePicker_SelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            ViewModel.SelectedGameToRent = GamePicker.SelectedItem as GameDTO;
        }

        private void RenterPicker_SelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            ViewModel.SelectedRenter = RenterPicker.SelectedItem as UserDTO;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            ViewModel.StartDate = StartDatePicker.Date;
            ViewModel.EndDate = EndDatePicker.Date;

            var rentalCreationResult = ViewModel.CreateRental();
            if (rentalCreationResult.IsSuccess)
            {
                if (Frame.CanGoBack)
                {
                    Frame.GoBack();
                }
                return;
            }

            await DialogHelper.ShowMessageAsync(
                this.XamlRoot,
                rentalCreationResult.DialogTitle,
                rentalCreationResult.DialogMessage);
        }
    }
}