using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCommunication
{
    public class MessageWrraper
    {
        public string Type { get; set; } = "";
        public byte[] Payload { get; set; } = Array.Empty<byte>();
    }
}
