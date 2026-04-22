using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Property_and_Management.Src.Viewmodels;

namespace Property_and_Management.Src.Views
{
    public sealed partial class RentalsFromOthersPage : Page
    {
        public RentalsFromOthersPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs navigationEventArgs)
        {
            base.OnNavigatedTo(navigationEventArgs);

            if (navigationEventArgs.Parameter is RentalsFromOthersViewModel rentalsFromOthersViewModel)
            {
                DataContext = rentalsFromOthersViewModel;
                return;
            }

            if (DataContext is not RentalsFromOthersViewModel)
            {
                DataContext = App.Services.GetRequiredService<RentalsFromOthersViewModel>();
            }
        }

        private void Image_ImageFailed(object sender, ExceptionRoutedEventArgs exceptionRoutedEventArgs)
        {
            ImageFailureHandler.HandleFailure(sender as Image, Resources);
        }

        private void NextButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            (DataContext as RentalsFromOthersViewModel)?.NextPage();
        }

        private void PrevButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            (DataContext as RentalsFromOthersViewModel)?.PrevPage();
        }
    }
}