using System;
using Property_and_Management.src.Interface;

namespace Property_and_Management.src.Model
{
    public class Notification : IEntity
    {
        public int Id { get; set; }
        public User User { get; set; }
        public DateTime Timestamp { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }

        public Notification(int id, User user, DateTime timestamp, string title, string body)
        {
            Id = id;
            User = user;
            Timestamp = timestamp;
            Title = title;
            Body = body;
        }
    }
}
