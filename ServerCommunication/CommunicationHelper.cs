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
        public static byte[] SerializeMessage(MessageBase messageToSerialize)
        {
            return JsonSerializer.SerializeToUtf8Bytes(messageToSerialize.ToMessageWrapper());
        }

        public static MessageWrapper? GetMessageWrapper(byte[] receivedPayloadBytes)
        {
            string receivedJsonPayload = Encoding.UTF8.GetString(receivedPayloadBytes);
            return JsonSerializer.Deserialize<MessageWrapper>(receivedJsonPayload);
        }
    }
}