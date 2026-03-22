using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Property_and_Management.src.Interface;
using ServerCommunication;

namespace Property_and_Management.src.Service.Listeners
{
    public class NotificationClient : IServerClient
    {

        private List<IObserver<MessageBase>> _subscribers = new();
        private UdpClient _udpClient;

        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private CancellationToken _cancellationToken=> _cancellationTokenSource.Token;

        public IPEndPoint ServerEndpoint => new IPEndPoint(IPAddress.Loopback, 4544);

        public NotificationClient()
        {
            _udpClient = new UdpClient(0); // OS will autoasign
        }

        private void HandleMessagePacket(MessageWrapper wrappedMessage)
        {
            try
            {
                switch (wrappedMessage.Type)
                {
                    case nameof(SendNotificationMessage):
                        // Deserialize the message
                        SendNotificationMessage? message = wrappedMessage.Deserialize<SendNotificationMessage>();

                        if (message == null)
                        {
                            throw new ArgumentNullException(nameof(message));
                        }

                        // Send the message to the subscribers
                        foreach (var subscriber in _subscribers)
                        {
                            subscriber.OnNext(message);
                        }
                        break;
                    default:
                        Console.WriteLine($"Message type cannot be handled: {wrappedMessage.Type}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception when handling message packet: {ex.Message}");
            }
        }

        public void StopListening() => _cancellationTokenSource.Cancel();

        public async Task ListenAsync()
        {
            try
            {
                while (!_cancellationToken.IsCancellationRequested)
                {
                    var result = await _udpClient.ReceiveAsync(_cancellationToken);
                    MessageWrapper? wrappedMessage = CommunicationHelper.GetMessageWrapper(result.Buffer);

                    if (wrappedMessage == null)
                    {
                        Console.WriteLine($"Recived bad json: {Encoding.UTF8.GetString(result.Buffer)}");
                    }

                    HandleMessagePacket(wrappedMessage);
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("UDP client cancelled");
            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine("UDP client socket closed");
            }
            finally
            {
                _udpClient?.Close();
            }
        }

        public IDisposable Subscribe(IObserver<MessageBase> observer)
        {
            _subscribers.Add(observer);
            return null;
        }

        public void SendNotification(int userId, string title, string body)
        {
            var sendNotificationMessage = new SendNotificationMessage
            {
                UserId = userId,
                Timestamp = DateTime.UtcNow,
                Title = title,
                Body = body
            };

            byte[] data = CommunicationHelper.SerializeMessage(sendNotificationMessage);
            _udpClient.Send(data, data.Length, ServerEndpoint);
        }

        public void SubscribeToServer(int userId)
        {
            var subscribeToServerMessage = new SubscribeToServerMessage
            {
                UserId = userId,
            };

            byte[] data = CommunicationHelper.SerializeMessage(subscribeToServerMessage);
            _udpClient.Send(data, data.Length, ServerEndpoint);
        }
    }
}
