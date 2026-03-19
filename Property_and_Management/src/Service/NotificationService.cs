using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Property_and_Management.src.DTO;
using Property_and_Management.src.Interface;
using Property_and_Management.src.Model;
using Property_and_Management.src.Repository;

namespace Property_and_Management.src.Service
{
    public class NotificationService : INotificationService
    {
        private readonly NotificationRepository _notificationRepository;

        public NotificationService(NotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
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
            throw new NotImplementedException();
        }

        public void UpdateNotificationById(int id, NotificationDTO notification)
        {
            _notificationRepository.Update(id, notification.ToModel());
        }
    }
}
