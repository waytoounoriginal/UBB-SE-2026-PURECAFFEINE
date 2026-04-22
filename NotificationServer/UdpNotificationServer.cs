using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ServerCommunication;

namespace NotificationServer
{
    internal class UdpNotificationServer
    {
        private const int DefaultServerPortNumber = 4544;
        private const string NotSubscribedEndpointDescription = "not subscribed";

        private static UdpClient? notificationServerUdpClient;

        private static readonly Dictionary<int, IPEndPoint> UserEndpointByIdentifierMap = [];

        private static async Task SendMessageToSubscribedUser(int destinationUserIdentifier, MessageBase notificationMessagePayload)
        {
            if (notificationServerUdpClient == null)
            {
                throw new NullReferenceException(nameof(notificationServerUdpClient));
            }

            if (!UserEndpointByIdentifierMap.TryGetValue(destinationUserIdentifier, out var destinationUserEndpoint))
            {
                throw new InvalidDataException("Target user id was not present in the endpoint map.");
            }

            byte[] serializedMessageBytes = CommunicationHelper.SerializeMessage(notificationMessagePayload);
            await notificationServerUdpClient.SendAsync(serializedMessageBytes, serializedMessageBytes.Length, destinationUserEndpoint);
        }

        private static void HandleSubscribeToServerMessagePacket(IPEndPoint receivedRemoteEndpoint, MessageWrapper receivedMessageWrapper)
        {
            SubscribeToServerMessage? userSubscriptionMessagePayload = receivedMessageWrapper.Deserialize<SubscribeToServerMessage>();

            if (userSubscriptionMessagePayload == null)
            {
                throw new InvalidCastException("Expected message was not " + nameof(SubscribeToServerMessage));
            }

            UserEndpointByIdentifierMap[userSubscriptionMessagePayload.UserId] = receivedRemoteEndpoint;
            Console.WriteLine($"{userSubscriptionMessagePayload.UserId} -> {receivedRemoteEndpoint.Address}:{receivedRemoteEndpoint.Port}");
        }

        private static async Task HandleSendNotificationMessagePacket(MessageWrapper receivedMessageWrapper)
        {
            SendNotificationMessage? outboundNotificationMessagePayload = receivedMessageWrapper.Deserialize<SendNotificationMessage>();

            if (outboundNotificationMessagePayload == null)
            {
                throw new InvalidCastException("Expected message was not " + nameof(SendNotificationMessage));
            }

            var deliveryEndpointDiagnosticDescription = UserEndpointByIdentifierMap.TryGetValue(outboundNotificationMessagePayload.UserId, out var subscribedUserEndpoint)
                ? subscribedUserEndpoint.ToString()
                : NotSubscribedEndpointDescription;

            Console.WriteLine(
                $"Sending notification to user: {outboundNotificationMessagePayload.UserId}({deliveryEndpointDiagnosticDescription}) " +
                $"[{outboundNotificationMessagePayload.Title} - {outboundNotificationMessagePayload.Body}]");

            await SendMessageToSubscribedUser(outboundNotificationMessagePayload.UserId, outboundNotificationMessagePayload);
        }

        private static async Task HandleIncomingMessagePacket(IPEndPoint receivedRemoteEndpoint, MessageWrapper receivedMessageWrapper)
        {
            Console.WriteLine($"Got: {receivedMessageWrapper.Type}");
            try
            {
                switch (receivedMessageWrapper.Type)
                {
                    case nameof(SubscribeToServerMessage):
                        HandleSubscribeToServerMessagePacket(receivedRemoteEndpoint, receivedMessageWrapper);
                        break;
                    case nameof(SendNotificationMessage):
                        await HandleSendNotificationMessagePacket(receivedMessageWrapper);
                        break;
                    default:
                        throw new InvalidDataException(receivedMessageWrapper.Type);
                }
            }
            catch (Exception caughtServerException)
            {
                Console.WriteLine($"Received exception while handling message: {caughtServerException.Message}");
            }
        }

        public static async Task ListenAsync(CancellationToken listenerShutdownCancellationToken, int udpListenPortNumber = DefaultServerPortNumber)
        {
            try
            {
                notificationServerUdpClient = new UdpClient(udpListenPortNumber);
                Console.WriteLine($"UDP server listening on port {udpListenPortNumber}");
            }
            catch (Exception caughtServerException)
            {
                Console.WriteLine($"ERROR: {caughtServerException.Message}");
                Environment.Exit((int)ServerErrors.FailedToInitializeServer);
            }

            Console.WriteLine("Starting listener...");

            try
            {
                while (!listenerShutdownCancellationToken.IsCancellationRequested)
                {
                    UdpReceiveResult receivedUdpPacketResult = await notificationServerUdpClient.ReceiveAsync(listenerShutdownCancellationToken);

                    MessageWrapper? receivedMessageWrapper = CommunicationHelper.GetMessageWrapper(receivedUdpPacketResult.Buffer);

                    if (receivedMessageWrapper == null)
                    {
                        Console.WriteLine($"Null message received from json: {Encoding.UTF8.GetString(receivedUdpPacketResult.Buffer)}");
                        continue;
                    }

                    await HandleIncomingMessagePacket(receivedUdpPacketResult.RemoteEndPoint, receivedMessageWrapper);
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
                notificationServerUdpClient?.Close();
            }
        }

        public static void Stop()
        {
            notificationServerUdpClient?.Close();
            notificationServerUdpClient = null;
        }
    }
}
