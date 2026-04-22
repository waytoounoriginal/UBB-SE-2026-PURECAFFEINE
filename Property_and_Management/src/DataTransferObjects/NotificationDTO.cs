using System;
using Property_and_Management.Src.Interface;
using Property_and_Management.Src.Model;

namespace Property_and_Management.Src.DataTransferObjects
{
    public class NotificationDTO : IDTO<Notification>
    {
        private const string TimeDisplayFormat = "hh:mm tt";

        public int Id { get; set; }
        public UserDTO User { get; set; }
        public DateTime Timestamp { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }

        public NotificationType Type { get; set; } = NotificationType.Informational;
        public int? RelatedRequestId { get; set; }

        public string TimeDisplay => Timestamp.ToString(TimeDisplayFormat);

        public NotificationDTO()
        {
        }
    }
}