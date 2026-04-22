using System;
using System.Collections.Immutable;
using Property_and_Management.Src.DataTransferObjects;

namespace Property_and_Management.Src.Interface
{
    public interface INotificationService : IObservable<NotificationDTO>
    {
        NotificationDTO GetNotificationByIdentifier(int notificationId);

        NotificationDTO DeleteNotificationByIdentifier(int notificationId);

        void UpdateNotificationByIdentifier(int notificationId, NotificationDTO updatedNotificationDto);

        void SendNotificationToUser(int recipientUserId, NotificationDTO notificationDto);

        ImmutableList<NotificationDTO> GetNotificationsForUser(int userId);

        void SubscribeToServer(int targetUserId);

        void StartListening();

        void StopListening();

        void ScheduleUpcomingRentalReminder(int renterUserId, int ownerUserId, string gameName, DateTime rentalStartDate);

        void DeleteNotificationsLinkedToRequest(int relatedRequestId);
    }
}