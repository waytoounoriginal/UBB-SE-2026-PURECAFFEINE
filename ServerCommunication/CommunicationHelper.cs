using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ServerCommunication
{
    public static class CommunicationHelper
    {
        public static byte[] SerializeMessage(MessageBase message)
        {
            return JsonSerializer.SerializeToUtf8Bytes(message.ToMessageWrapper());
        }

        public static MessageWrapper? GetMessageWrapper(byte[] resultBytes)
        {
            string recivedJson = Encoding.UTF8.GetString(resultBytes);

            // Deserialize
            return JsonSerializer.Deserialize<MessageWrapper>(recivedJson);
        }

    }
}
