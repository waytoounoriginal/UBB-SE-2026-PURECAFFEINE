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

namespace NotificationServer
{
    internal class UdpNotificationServer
    {
        private const int DEFAULT_PORT = 4544;

        private static UdpClient? _udpClient;

        private static void HandleJsonPacket(string recivedJson)
        {

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
                    var result = await _udpClient.ReceiveAsync(cancellationToken);
                    string recivedJson = Encoding.UTF8.GetString(result.Buffer);

                    // Deserialize
                    var recivedObject = JsonSerializer.Deserialize<dynamic>(recivedJson);
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
