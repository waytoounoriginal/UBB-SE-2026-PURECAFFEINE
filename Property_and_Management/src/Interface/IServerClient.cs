using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ServerCommunication;

namespace Property_and_Management.src.Interface
{
    public interface IServerClient : IObservable<MessageBase>
    {
        IPEndPoint ServerEndpoint { get; }

        Task ListenAsync();
        void SubscribeToServer(int userId);
        void SendNotification(int userId, string title, string body);
        void StopListening();
    }
}
