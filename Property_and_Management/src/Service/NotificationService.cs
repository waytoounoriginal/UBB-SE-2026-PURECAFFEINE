using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Composition;
using Property_and_Management.src.DTO;
using Property_and_Management.src.Interface;
using Property_and_Management.src.Model;
using Property_and_Management.src.Repository;
using Property_and_Management.src.Service.Listeners;
using ServerCommunication;
using Windows.UI.Notifications;

namespace Property_and_Management.src.Service
{
    public class NotificationService : INotificationService, IObserver<MessageBase>, IObservable<NotificationDTO>
    {
        private readonly NotificationRepository _notificationRepository;

        private IServerClient _serverClient;
        private List<IObserver<NotificationDTO>> _subscribers = [];

        public NotificationService(NotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
            _serverClient = new NotificationClient();

            _serverClient.Subscribe(this);
        }


        public NotificationDTO DeleteNotificationById(int id)
        {
            return (NotificationDTO)NotificationDTO.FromModel(_notificationRepository.Delete(id));
        }

        public NotificationDTO GetNotificationById(int id)
        {
            return (NotificationDTO)NotificationDTO.FromModel(_notificationRepository.Get(id));
        }

        public ImmutableList<NotificationDTO> GetNotificationsForUser(int userId)
        {
            return _notificationRepository
                .GetNotificationsByUser(userId)
                .Select(entity => (NotificationDTO)NotificationDTO.FromModel(entity))
                .ToImmutableList();
        }

        public void SendNotificationToUser(int userId, NotificationDTO notification)
        {
            _serverClient.SendNotification(userId, notification.Title, notification.Body);
        }

        public void UpdateNotificationById(int id, NotificationDTO notification)
        {
            _notificationRepository.Update(id, notification.ToModel());
        }

        public void StartListening()
        {
            _serverClient.ListenAsync();
        }

        public void StopListening()
        {
            _serverClient.StopListening();
        }

        public void OnCompleted()
        {
            // throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            // throw new NotImplementedException();
        }

        public void OnNext(MessageBase value)
        {
            // Notify all subscribers
            // only SendNotificationMessage is supported
            if( value is SendNotificationMessage message)
            {
                NotificationDTO notificationDTO = new NotificationDTO
                {
                    Timestamp = message.Timestamp,
                    Title = message.Title,
                    Body = message.Body,
                };

                foreach(var subscriber in _subscribers)
                {
                    subscriber.OnNext(notificationDTO);
                }

                // Display a system notification
                ShowWindowsNotification(message.Title, message.Body);
            }
        }

        public IDisposable Subscribe(IObserver<NotificationDTO> observer)
        {
            _subscribers.Add(observer);
            return null;
        }

        private void ShowWindowsNotification(string title, string body)
        {
            // Using Microsoft.Toolkit.Uwp.Notifications
            var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);

            var textNodes = toastXml.GetElementsByTagName("text");
            textNodes[0].AppendChild(toastXml.CreateTextNode(title));
            textNodes[1].AppendChild(toastXml.CreateTextNode(body));

            var toast = new ToastNotification(toastXml);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

        public void SubscribeToServer(int userId)
        {
            _serverClient.SubscribeToServer(userId);
        }
    }
}
