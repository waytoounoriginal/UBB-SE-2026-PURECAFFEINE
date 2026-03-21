using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerCommunication.Interfaces;

namespace ServerCommunication
{
    public class SendNotificationMessage : IMessage
    {
        public string MessageType => nameof(SendNotificationMessage);

        public int UserId { get; set; }
        public DateTime Timestamp { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
    }
}
