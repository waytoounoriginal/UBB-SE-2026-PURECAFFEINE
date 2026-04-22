using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using Property_and_Management.Src.Interface;

namespace Property_and_Management.Src.Service
{
    public class ToastNotificationService : IToastNotificationService
    {
        private const string NavigationKey = "navigate";
        private const string NotificationsPageKey = "NotificationsPage";

        public void Show(string notificationTitle, string notificationBody)
        {
            var notification = new AppNotificationBuilder()
                .AddArgument(NavigationKey, NotificationsPageKey)
                .AddText(notificationTitle)
                .AddText(notificationBody)
                .BuildNotification();

            AppNotificationManager.Default.Show(notification);
        }
    }
}