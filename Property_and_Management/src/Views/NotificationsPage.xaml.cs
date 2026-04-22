using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Property_and_Management.Src.DataTransferObjects;
using Property_and_Management.Src.Viewmodels;

namespace Property_and_Management.Src.Views
{
    public sealed partial class NotificationsPage : Page
    {
        public NotificationsPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs navigationEventArgs)
        {
            base.OnNavigatedTo(navigationEventArgs);

            if (navigationEventArgs.Parameter is NotificationsViewModel navigatedViewModel)
            {
                DataContext = navigatedViewModel;
                navigatedViewModel.LoadNotificationsForUser(navigatedViewModel.CurrentUserId);
                return;
            }

            if (DataContext is NotificationsViewModel existingViewModel)
            {
                existingViewModel.LoadNotificationsForUser(existingViewModel.CurrentUserId);
                return;
            }

            var resolvedViewModel = App.Services.GetRequiredService<NotificationsViewModel>();
            DataContext = resolvedViewModel;
            resolvedViewModel.LoadNotificationsForUser(resolvedViewModel.CurrentUserId);
        }

        private NotificationsViewModel? ResolveViewModel()
        {
            var pageRootElement = this.Content as FrameworkElement;
            return pageRootElement?.DataContext as NotificationsViewModel;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            var clickedButton = sender as Button;
            if (clickedButton?.DataContext is not NotificationDTO notificationToDelete)
            {
                Debug.WriteLine("DeleteButton_Click: notification Data Transfer Object not found");
                return;
            }

            var resolvedNotificationsViewModel = ResolveViewModel();
            if (resolvedNotificationsViewModel == null)
            {
                Debug.WriteLine("NotificationsPage: viewmodel not found on DeleteButton_Click");
                return;
            }

            resolvedNotificationsViewModel.DeleteNotificationByIdentifier(notificationToDelete.Id);
        }

        private void NextButton_Click(object sender, RoutedEventArgs routedEventArgs) => ResolveViewModel()?.NextPage();

        private void PrevButton_Click(object sender, RoutedEventArgs routedEventArgs) => ResolveViewModel()?.PrevPage();
    }
}