using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using ServerCommunication;

namespace NotificationServer
{
    internal class UdpNotificationServer
    {
        private const int DEFAULT_PORT = 4544;

        private static UdpClient? _udpClient;

        private static Dictionary<int, IPEndPoint> _userIpMap = [];

        private static async Task SendMessage(int userId, MessageBase unwrappedMessageToSend)
        {
            if (_udpClient == null)
            {
                throw new NullReferenceException(nameof(_udpClient));
            }

            if (!_userIpMap.TryGetValue(userId, out var endpoint))
            {
                throw new InvalidDataException("UserId was not present in the map");
            }

            byte[] data = CommunicationHelper.SerializeMessage(unwrappedMessageToSend);
            await _udpClient.SendAsync(data, data.Length, endpoint);
        }

        private static void HandleSubscribeToServerMessage(IPEndPoint recivedEndPoint, MessageWrapper recivedMessage)
        {
            SubscribeToServerMessage? message = recivedMessage.Deserialize<SubscribeToServerMessage>();

            if (message == null)
            {
                throw new InvalidCastException("Expected message was not " + nameof(SubscribeToServerMessage));
            }

            _userIpMap[message.UserId] = recivedEndPoint;
        }

        private static async Task HandleSendNotificationMessage(MessageWrapper recivedMessage)
        {
            SendNotificationMessage? unwrappedMessage = recivedMessage.Deserialize<SendNotificationMessage>();

            if (unwrappedMessage == null)
            {
                throw new InvalidCastException("Expected message was not " + nameof(SendNotificationMessage));
            }

            // Resend the UDP packet to the user id
            await SendMessage(unwrappedMessage.UserId, unwrappedMessage);
        }

        private static async Task HandleMessagePacket(IPEndPoint recivedEndPoint, MessageWrapper recivedMessageWrapper)
        {
            Console.WriteLine($"Got: {recivedMessageWrapper.Type}");
            try
            {
                switch (recivedMessageWrapper.Type)
                {
                    case nameof(SubscribeToServerMessage): HandleSubscribeToServerMessage(recivedEndPoint, recivedMessageWrapper); break;
                    case nameof(SendNotificationMessage): await HandleSendNotificationMessage(recivedMessageWrapper); break;
                    default: throw new InvalidDataException(recivedMessageWrapper.Type);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Recived exception while handling message: {exception.Message}");
            }

        }

        public static async Task ListenAsync(CancellationToken cancellationToken, int port = DEFAULT_PORT)
        {
            try
            {
                _udpClient = new UdpClient(port);
                Console.WriteLine($"UDP Server listening on  port {port}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR: {e.Message}");
                Environment.Exit((int)ServerErrors.SERVER_FAILED_INIT);
            }

            Console.WriteLine("Starting listener...");

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    // Recive the serialized object
                    UdpReceiveResult result = await _udpClient.ReceiveAsync(cancellationToken);

                    // Deserialize
                    MessageWrapper? recivedObject = CommunicationHelper.GetMessageWrapper(result.Buffer);

                    // If null drop the message, print a message
                    if (recivedObject == null)
                    {
                        Console.WriteLine($"Null message recived from json: {Encoding.UTF8.GetString(result.Buffer)}");
                        continue;
                    }

                    await HandleMessagePacket(result.RemoteEndPoint, recivedObject);
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("UDP server cancelled");
            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine("UDP server socket closed");
            }
            finally
            {
                _udpClient?.Close();
            }
        }

        public static void Stop()
        {
            _udpClient?.Close();
            _udpClient = null;
        }

    }
}
