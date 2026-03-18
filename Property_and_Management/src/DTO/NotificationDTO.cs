using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Property_and_Management.src.Model;

namespace Property_and_Management.src.DTO
{
    public class NotificationDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime Timestamp { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }

        public NotificationDTO(int id, int userId, DateTime timestamp, string title, string body)
        {
            Id = id;
            UserId = userId;
            Timestamp = timestamp;
            Title = title;
            Body = body;
        }

        public NotificationDTO(Notification model)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            Id = model.Id;
            UserId = model.UserId;
            Timestamp = model.Timestamp;
            Title = model.Title;
            Body = model.Body;
        }

        public Notification ToModel()
        {
            return new Notification(Id, UserId, Timestamp, Title, Body);
        }

        public static NotificationDTO FromModel(Notification model)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            return new NotificationDTO(model);
        }
    }
}
