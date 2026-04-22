namespace Property_and_Management.Src.Model
{
    internal static class NotificationTypeValues
    {
        internal const int Informational = 0;
        internal const int OfferReceived = 1;
        internal const int OfferResult = 2;
    }

    public enum NotificationType
    {
        Informational = NotificationTypeValues.Informational,
        OfferReceived = NotificationTypeValues.OfferReceived,
        OfferResult = NotificationTypeValues.OfferResult
    }
}