using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ServerCommunication
{
    public class SubscribeToServerMessage : MessageBase
    {
        public int UserId { get; set; }
    }
}
