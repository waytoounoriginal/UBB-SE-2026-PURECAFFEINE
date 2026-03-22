using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ServerCommunication;

class Program
{
    static async Task<int> Main()
    {
        var serverEndpoint = new IPEndPoint(IPAddress.Loopback, 4544);

        // Bind the client so the server can reply to the same endpoint
        using var client = new UdpClient();
        client.Client.Bind(new IPEndPoint(IPAddress.Loopback, 0));

        int userId = 2;

        try
        {
            //// 1) Send SubscribeToServerMessage
            //MessageBase subscribe = new SubscribeToServerMessage { UserId = userId };
            //var subscribeJson = subscribe.ToMessageWrapper().Serialize();
            //Console.WriteLine(subscribeJson);
            //await client.SendAsync(subscribeJson, subscribeJson.Length, serverEndpoint);
            //Console.WriteLine("Sent subscribe");

            //// Give the server a moment to register the endpoint
            //await Task.Delay(200);

            // 2) Send SendNotificationMessage (server should forward to the registered endpoint)
            MessageBase notification = new SendNotificationMessage
            {
                UserId = userId,
                Timestamp = DateTime.UtcNow,
                Title = "Test",
                Body = "This is a test notification"
            };
            var notificationJson = notification.ToMessageWrapper().Serialize();
            Console.WriteLine($"Serialized notification: {Encoding.UTF8.GetString(notificationJson)}");
            await client.SendAsync(notificationJson, notificationJson.Length, serverEndpoint);
            Console.WriteLine("Sent notification request");

            // 3) Wait for forwarded message from server
            var receiveTask = client.ReceiveAsync();
            if (await Task.WhenAny(receiveTask, Task.Delay(500000)) == receiveTask)
            {
                var res = receiveTask.Result;
                Console.WriteLine($"Received forwarded packet from {res.RemoteEndPoint}:");

                string recivedJson = Encoding.UTF8.GetString(res.Buffer);
                Console.WriteLine($"Got {recivedJson}");
                // Console.WriteLine(JsonSerializer.Deserialize<MessageWrapper>(recivedJson).Payload.Count());
                var wrapper = JsonSerializer.Deserialize<MessageWrapper>(recivedJson);

                Console.WriteLine(wrapper);

                var deserializedMessage = JsonSerializer.Deserialize<SendNotificationMessage>(wrapper.Payload);
                Console.WriteLine(deserializedMessage.Title);
            }
            else
            {
                Console.WriteLine("No forwarded message received within timeout.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return 1;
        }

        return 0;
    }
}