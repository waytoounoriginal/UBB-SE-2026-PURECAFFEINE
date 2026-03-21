using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ServerCommunication.Interfaces
{
    public interface IMessage
    {
        string MessageType { get; }

        /// <summary>
        /// Provides the MessageWrapper for a given Message
        /// </summary>
        /// <returns>A MessageWrapper instance containing the payload of the object</returns>
        MessageWrraper ToMessage()
        {
            return new MessageWrraper
            {
                Type = MessageType,
                Payload = JsonSerializer.SerializeToUtf8Bytes(this)
            };
        }
    }
}
