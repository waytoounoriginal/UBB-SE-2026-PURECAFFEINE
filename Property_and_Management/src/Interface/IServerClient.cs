using System;
using System.Threading.Tasks;
using Property_and_Management.Src.DataTransferObjects;

namespace Property_and_Management.Src.Interface
{
    public interface IServerClient : IObservable<IncomingNotification>
    {
        Task ListenAsync();
        void SubscribeToServer(int targetUserId);
        void SendNotification(int targetUserId, string notificationTitle, string notificationBody);
        void StopListening();
    }
}