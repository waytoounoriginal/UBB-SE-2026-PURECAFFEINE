namespace Property_and_Management.Src.Model
{
    internal static class RequestStatusValues
    {
        internal const int Open = 0;
        internal const int OfferPending = 1;
        internal const int Accepted = 2;
        internal const int Cancelled = 3;
    }

    public enum RequestStatus
    {
        Open = RequestStatusValues.Open,
        OfferPending = RequestStatusValues.OfferPending,
        Accepted = RequestStatusValues.Accepted,
        Cancelled = RequestStatusValues.Cancelled
    }
}