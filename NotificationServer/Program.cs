namespace NotificationServer
{

    internal class Program
    {

        static async Task Main(string[] args)
        {
            using var cts = new CancellationTokenSource();

            // Handle Ctrl+C
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                Console.WriteLine("Stopping server...");
                cts.Cancel();
            };

            await UdpNotificationServer.ListenAsync(cts.Token);
        }

    }

}