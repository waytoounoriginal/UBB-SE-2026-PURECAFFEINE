using System;

namespace Property_and_Management.Src.DataTransferObjects
{
    public sealed class IncomingNotification
    {
        public int UserId { get; init; }
        public DateTime Timestamp { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Body { get; init; } = string.Empty;
    }
}
