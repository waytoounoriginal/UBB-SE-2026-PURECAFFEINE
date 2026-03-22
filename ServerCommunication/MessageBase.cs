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
        /// <summary>
        /// Provides the MessageWrapper for a given Message
        /// </summary>
        /// <returns>A MessageWrapper instance containing the payload of the object</returns>
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
