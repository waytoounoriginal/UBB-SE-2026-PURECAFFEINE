using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerCommunication.Interfaces;

namespace ServerCommunication
{
    public class SubscribeToServerMessage : IMessage
    {
        public string MessageType => nameof(SubscribeToServerMessage);

        public int UserId { get; set; }
    }
}
