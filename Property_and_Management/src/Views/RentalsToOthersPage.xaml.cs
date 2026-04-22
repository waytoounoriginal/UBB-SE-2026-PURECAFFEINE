using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Property_and_Management.Src.Viewmodels;

namespace Property_and_Management.Src.Views
{
    public sealed partial class RentalsToOthersPage : Page
    {
        public RentalsToOthersPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs navigationEventArgs)
        {
            base.OnNavigatedTo(navigationEventArgs);

            if (navigationEventArgs.Parameter is RentalsToOthersViewModel rentalsToOthersViewModel)
            {
                DataContext = rentalsToOthersViewModel;
                return;
            }

            if (DataContext is not RentalsToOthersViewModel)
            {
                DataContext = App.Services.GetRequiredService<RentalsToOthersViewModel>();
            }
        }

        private void CreateRentalButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            Frame?.Navigate(typeof(CreateRentalView));
        }

        private void Image_ImageFailed(object sender, ExceptionRoutedEventArgs exceptionRoutedEventArgs)
        {
            ImageFailureHandler.HandleFailure(sender as Image, Resources);
        }

        private void NextButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            (DataContext as RentalsToOthersViewModel)?.NextPage();
        }

        private void PrevButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            (DataContext as RentalsToOthersViewModel)?.PrevPage();
        }
    }
}