namespace NotificationServer
{
    internal class Program
    {
        private static async Task Main()
        {
            using var udpListenerLifetimeCancellationSource = new CancellationTokenSource();

            Console.CancelKeyPress += (_, cancelKeyPressEventArgs) =>
            {
                cancelKeyPressEventArgs.Cancel = true;
                Console.WriteLine("Stopping server...");
                udpListenerLifetimeCancellationSource.Cancel();
            };

            await UdpNotificationServer.ListenAsync(udpListenerLifetimeCancellationSource.Token);
        }
    }
}