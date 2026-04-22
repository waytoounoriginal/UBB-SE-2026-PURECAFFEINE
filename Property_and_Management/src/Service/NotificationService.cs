using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Property_and_Management;
using Property_and_Management.Src.DataTransferObjects;
using Property_and_Management.Src.Interface;
using Property_and_Management.Src.Model;

namespace Property_and_Management.Src.Service
{
    public class NotificationService : INotificationService, IObserver<IncomingNotification>, IObservable<NotificationDTO>, IDisposable
    {
        private static readonly TimeSpan UpcomingRentalReminderLeadTime = TimeSpan.FromHours(24);
        private const int NewNotificationId = 0;
        private const int MissingUserId = 0;

        private bool isDisposed;
        private readonly CancellationTokenSource reminderScheduleCancellationSource = new();
        private readonly INotificationRepository notificationDataRepository;
        private readonly IMapper<Notification, NotificationDTO> notificationDtoMapper;
        private readonly IServerClient serverNotificationClient;
        private readonly ICurrentUserContext currentUserContext;
        private readonly IToastNotificationService toastAlertService;

        private readonly List<IObserver<NotificationDTO>> notificationSubscribers = new();
        private readonly object notificationSubscribersLock = new();

        public NotificationService(
            INotificationRepository notificationRepository,
            IMapper<Notification, NotificationDTO> notificationMapper,
            IServerClient serverClient,
            ICurrentUserContext currentUserContext,
            IToastNotificationService toastNotificationService)
        {
            this.notificationDataRepository = notificationRepository;
            this.notificationDtoMapper = notificationMapper;
            this.serverNotificationClient = serverClient;
            this.currentUserContext = currentUserContext;
            this.toastAlertService = toastNotificationService;
            serverNotificationClient.Subscribe(this);
        }

        public NotificationDTO DeleteNotificationByIdentifier(int notificationId) =>
            notificationDtoMapper.ToDTO(notificationDataRepository.Delete(notificationId));

        public NotificationDTO GetNotificationByIdentifier(int notificationId) =>
            notificationDtoMapper.ToDTO(notificationDataRepository.Get(notificationId));

        public ImmutableList<NotificationDTO> GetNotificationsForUser(int targetUserId) =>
            notificationDataRepository
                .GetNotificationsByUser(targetUserId)
                .Select(notification => notificationDtoMapper.ToDTO(notification))
                .ToImmutableList();

        public void SendNotificationToUser(int recipientUserId, NotificationDTO notificationToSend)
        {
            if (notificationToSend == null)
            {
                throw new ArgumentNullException(nameof(notificationToSend));
            }

            DateTime notificationTimestamp = notificationToSend.Timestamp == default ? DateTime.UtcNow : notificationToSend.Timestamp;

            var notificationModel = BuildNotificationDomainModel(
                recipientUserId,
                notificationTimestamp,
                notificationToSend.Title,
                notificationToSend.Body,
                notificationToSend.Type,
                notificationToSend.RelatedRequestId);

            notificationDataRepository.Add(notificationModel);

            if (currentUserContext.CurrentUserId == recipientUserId)
            {
                NotifyAllSubscribers(BuildNotificationDataTransferObject(
                    notificationModel.Id,
                    recipientUserId,
                    notificationTimestamp,
                    notificationToSend.Title,
                    notificationToSend.Body,
                    notificationToSend.Type,
                    notificationToSend.RelatedRequestId));
            }

            serverNotificationClient.SendNotification(recipientUserId, notificationToSend.Title, notificationToSend.Body);
        }

        public void DeleteNotificationsLinkedToRequest(int linkedRequestId)
        {
            notificationDataRepository.DeleteNotificationsLinkedToRequest(linkedRequestId);
        }

        public void UpdateNotificationByIdentifier(int notificationId, NotificationDTO updatedNotificationData)
        {
            notificationDataRepository.Update(notificationId, notificationDtoMapper.ToModel(updatedNotificationData));
        }

        public void StartListening()
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await serverNotificationClient.ListenAsync();
                }
                catch (Exception listenException)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"NotificationService: listen loop terminated - {listenException}");
                }
            });
        }

        public void StopListening() => serverNotificationClient.StopListening();
        public void OnCompleted()
        {
        }
        public void OnError(Exception observableError)
        {
        }

        public void OnNext(IncomingNotification receivedNotification)
        {
            var incomingNotificationDto = BuildNotificationDataTransferObject(
                NewNotificationId,
                receivedNotification.UserId,
                receivedNotification.Timestamp,
                receivedNotification.Title,
                receivedNotification.Body,
                default,
                null);

            NotifyAllSubscribers(incomingNotificationDto);
            toastAlertService.Show(receivedNotification.Title, receivedNotification.Body);
        }

        private void NotifyAllSubscribers(NotificationDTO outgoingNotificationDto)
        {
            IObserver<NotificationDTO>[] subscribersSnapshot;
            lock (notificationSubscribersLock)
            {
                subscribersSnapshot = notificationSubscribers.ToArray();
            }

            foreach (var subscriber in subscribersSnapshot)
            {
                subscriber.OnNext(outgoingNotificationDto);
            }
        }

        public IDisposable Subscribe(IObserver<NotificationDTO> newObserver)
        {
            lock (notificationSubscribersLock)
            {
                notificationSubscribers.Add(newObserver);
            }

            return new Unsubscriber(notificationSubscribers, notificationSubscribersLock, newObserver);
        }

        private sealed class Unsubscriber : IDisposable
        {
            private readonly List<IObserver<NotificationDTO>> subscribersList;
            private readonly object subscribersListLock;
            private readonly IObserver<NotificationDTO> subscriberToRemove;

            public Unsubscriber(
                List<IObserver<NotificationDTO>> subscribers,
                object subscribersLock,
                IObserver<NotificationDTO> observer)
            {
                this.subscribersList = subscribers;
                this.subscribersListLock = subscribersLock;
                this.subscriberToRemove = observer;
            }

            public void Dispose()
            {
                lock (subscribersListLock)
                {
                    subscribersList.Remove(subscriberToRemove);
                }
            }
        }

        public void SubscribeToServer(int userId) => serverNotificationClient.SubscribeToServer(userId);

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;

            reminderScheduleCancellationSource.Cancel();
            reminderScheduleCancellationSource.Dispose();
            StopListening();
            (serverNotificationClient as IDisposable)?.Dispose();
        }

        public void ScheduleUpcomingRentalReminder(int renterUserId, int ownerUserId, string rentalGameName, DateTime rentalStartDate)
        {
            var rentalStartUtc = rentalStartDate.ToUniversalTime();
            var currentUtcTime = DateTime.UtcNow;

            if (rentalStartUtc <= currentUtcTime)
            {
                return;
            }

            DateTime scheduledReminderTime = rentalStartUtc - UpcomingRentalReminderLeadTime;
            string reminderTitle = Constants.NotificationTitles.UpcomingRentalReminder;
            string reminderBody = BuildUpcomingRentalReminderBody(rentalGameName, rentalStartDate);

            ScheduleOrSendReminderForUser(renterUserId, reminderTitle, reminderBody, scheduledReminderTime);
            ScheduleOrSendReminderForUser(ownerUserId, reminderTitle, reminderBody, scheduledReminderTime);
        }

        private static string BuildUpcomingRentalReminderBody(string rentalGameName, DateTime rentalStartDate)
        {
            return $"Game: {rentalGameName}\nStart: {rentalStartDate:dd/MM/yyyy HH:mm}\n" +
                   "Delivery/Pick-up: Coordinate delivery/pick-up directly with the other party.";
        }

        private void ScheduleOrSendReminderForUser(int recipientUserId, string reminderTitle, string reminderBody, DateTime scheduledSendTime)
        {
            if (recipientUserId == MissingUserId)
            {
                return;
            }

            TimeSpan sendDelay = scheduledSendTime.ToUniversalTime() - DateTime.UtcNow;
            if (sendDelay <= TimeSpan.Zero)
            {
                SendReminderNotificationImmediately(recipientUserId, reminderTitle, reminderBody);
                return;
            }

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(sendDelay, reminderScheduleCancellationSource.Token);
                    SendReminderNotificationImmediately(recipientUserId, reminderTitle, reminderBody);
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception scheduledReminderException)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"NotificationService: scheduled reminder failed - {scheduledReminderException}");
                }
            });
        }

        private void SendReminderNotificationImmediately(int recipientUserId, string reminderTitle, string reminderBody)
        {
            var immediateReminderDto = BuildNotificationDataTransferObject(
                NewNotificationId,
                recipientUserId,
                DateTime.UtcNow,
                reminderTitle,
                reminderBody,
                default,
                null);

            SendNotificationToUser(recipientUserId, immediateReminderDto);
        }

        private static NotificationDTO BuildNotificationDataTransferObject(
            int notificationId,
            int recipientUserId,
            DateTime notificationTimestamp,
            string notificationTitle,
            string notificationBody,
            NotificationType notificationType,
            int? linkedRequestId)
        {
            return new NotificationDTO
            {
                Id = notificationId,
                User = new UserDTO { Id = recipientUserId },
                Timestamp = notificationTimestamp,
                Title = notificationTitle,
                Body = notificationBody,
                Type = notificationType,
                RelatedRequestId = linkedRequestId
            };
        }

        private static Notification BuildNotificationDomainModel(
            int recipientUserId,
            DateTime notificationTimestamp,
            string notificationTitle,
            string notificationBody,
            NotificationType notificationType = default,
            int? linkedRequestId = null)
        {
            return new Notification
            {
                Id = NewNotificationId,
                User = new User { Id = recipientUserId },
                Timestamp = notificationTimestamp,
                Title = notificationTitle,
                Body = notificationBody,
                Type = notificationType,
                RelatedRequestId = linkedRequestId
            };
        }
    }
}
