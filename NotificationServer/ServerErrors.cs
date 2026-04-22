namespace NotificationServer
{
    internal static class ServerExitCodes
    {
        internal const int Success = 0;
        internal const int InitializationFailure = -1;
    }

    internal enum ServerErrors
    {
        None = ServerExitCodes.Success,
        FailedToInitializeServer = ServerExitCodes.InitializationFailure,
    }
}