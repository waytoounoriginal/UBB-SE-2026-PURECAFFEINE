using System;
using Property_and_Management.Src.Interface;

namespace Property_and_Management.Src.Model
{
    public class Notification : IEntity
    {
        public int Id { get; set; }
        public User User { get; set; }
        public DateTime Timestamp { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public NotificationType Type { get; set; } = NotificationType.Informational;
        public int? RelatedRequestId { get; set; }

        public Notification()
        {
        }
        public Notification(int id, User recipientUser, DateTime timestamp, string title, string body,
                            NotificationType notificationType = NotificationType.Informational, int? relatedRequestId = null)
        {
            this.Id = id;
            User = recipientUser;
            Timestamp = timestamp;
            Title = title;
            Body = body;
            Type = notificationType;
            this.RelatedRequestId = relatedRequestId;
        }
    }
}