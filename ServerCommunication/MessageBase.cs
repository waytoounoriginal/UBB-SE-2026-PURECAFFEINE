using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ServerCommunication
{
    public abstract class MessageBase
    {
        public MessageWrapper ToMessageWrapper()
        {
            return new MessageWrapper
            {
                Type = GetType().Name,
                Payload = JsonSerializer.SerializeToUtf8Bytes((object)this)
            };
        }
    }
}