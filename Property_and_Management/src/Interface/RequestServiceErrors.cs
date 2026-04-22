namespace Property_and_Management.Src.Interface
{
    internal static class CancelRequestErrorCodes
    {
        internal const int Unauthorized = -1;
        internal const int NotFound = -2;
    }

    public enum CreateRequestError
    {
        OwnerCannotRent,
        DatesUnavailable,
        GameDoesNotExist
    }

    public enum ApproveRequestError
    {
        Unauthorized,
        NotFound,
        TransactionFailed
    }

    public enum DenyRequestError
    {
        Unauthorized,
        NotFound
    }

    public enum OfferError
    {
        NotFound,
        NotOwner,
        RequestNotOpen,
        TransactionFailed
    }

    public enum CancelRequestError
    {
        Unauthorized = CancelRequestErrorCodes.Unauthorized,
        NotFound = CancelRequestErrorCodes.NotFound
    }
}