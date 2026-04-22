using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.UI.Dispatching;
using Property_and_Management.Src.DataTransferObjects;
using Property_and_Management.Src.Interface;

namespace Property_and_Management.Src.Viewmodels
{
    public class NotificationsViewModel : PagedViewModel<NotificationDTO>,
                                           IObserver<NotificationDTO>,
                                           IDisposable
    {
        private const int InvalidOrUnknownUserId = 0;
        private const int FallbackDefaultUserId = 1;

        private readonly INotificationService notificationLookupService;
        private readonly IDisposable notificationSubscription;

        private readonly DispatcherQueue? uiDispatcherQueue;

        public int CurrentUserId { get; private set; }

        public NotificationsViewModel(
            INotificationService notificationLookupService,
            ICurrentUserContext currentUserContext)
        {
            this.notificationLookupService = notificationLookupService;

            try
            {
                uiDispatcherQueue = DispatcherQueue.GetForCurrentThread();
            }
            catch (COMException)
            {
                uiDispatcherQueue = null;
            }

            LoadNotificationsForUser(currentUserContext.CurrentUserId);

            notificationSubscription = notificationLookupService.Subscribe(this);
        }

        public void LoadNotificationsForUser(int targetUserId)
        {
            CurrentUserId = targetUserId;
            Reload();
        }

        protected override void Reload()
        {
            var userNotificationsSortedByNewest = notificationLookupService
                .GetNotificationsForUser(CurrentUserId)
                .OrderByDescending(notification => notification.Id)
                .ToImmutableList();

            SetAllItems(userNotificationsSortedByNewest);
        }

        public void DeleteNotificationByIdentifier(int notificationIdToDelete)
        {
            try
            {
                notificationLookupService.DeleteNotificationByIdentifier(notificationIdToDelete);
            }
            catch (KeyNotFoundException)
            {
            }

            Reload();
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception observableError)
        {
            System.Diagnostics.Debug.WriteLine($"Notification observable error: {observableError.Message}");
        }

        public void OnNext(NotificationDTO incomingNotification)
        {
            var resolvedUserIdForReload = CurrentUserId == InvalidOrUnknownUserId
                ? FallbackDefaultUserId
                : CurrentUserId;

            if (uiDispatcherQueue != null && !uiDispatcherQueue.HasThreadAccess)
            {
                uiDispatcherQueue.TryEnqueue(() => LoadNotificationsForUser(resolvedUserIdForReload));
                return;
            }

            LoadNotificationsForUser(resolvedUserIdForReload);
        }

        public void Dispose() => notificationSubscription?.Dispose();
    }
}
