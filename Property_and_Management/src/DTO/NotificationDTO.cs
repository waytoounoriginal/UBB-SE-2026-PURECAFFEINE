using System;
using Property_and_Management.src.Interface;
using Property_and_Management.src.Model;

namespace Property_and_Management.src.DTO
{
    public class NotificationDTO : IDTO<Notification>
    {
        public int Id { get; set; }
        public User User { get; set; }
        public DateTime Timestamp { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }

        public NotificationDTO()
        {
        }

        public NotificationDTO(int id, User user, DateTime timestamp, string title, string body)
        {
            Id = id;
            User = user;
            Timestamp = timestamp;
            Title = title;
            Body = body;
        }

        public NotificationDTO(Notification model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            Id = model.Id;
            User = model.User;
            Timestamp = model.Timestamp;
            Title = model.Title;
            Body = model.Body;
        }

        public Notification ToModel()
        {
            return new Notification(Id, User, Timestamp, Title, Body);
        }

        public static IDTO<Notification> FromModel(Notification model)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            return new NotificationDTO(model);
        }
    }
}
