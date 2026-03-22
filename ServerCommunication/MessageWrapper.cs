using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ServerCommunication
{
    public class MessageWrapper
    {
        public string Type { get; set; } = "";
        public byte[] Payload { get; set; } = Array.Empty<byte>();

        public byte[] Serialize()
        {
            return JsonSerializer.SerializeToUtf8Bytes(this);
        }

        public T? Deserialize<T>() where T : MessageBase
        {
            return JsonSerializer.Deserialize<T>(Payload);
        }
    }
}
