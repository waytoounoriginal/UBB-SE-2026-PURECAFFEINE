namespace Property_and_Management.Src.Constants
{
    internal static class DialogTitles
    {
        public const string ValidationError = global::Property_and_Management.Constants.DialogTitles.ValidationError;
        public const string RequestFailed = global::Property_and_Management.Constants.DialogTitles.RequestFailed;
        public const string RentalFailed = global::Property_and_Management.Constants.DialogTitles.RentalFailed;
        public const string ApproveFailed = global::Property_and_Management.Constants.DialogTitles.ApproveFailed;
        public const string DeclineFailed = global::Property_and_Management.Constants.DialogTitles.DeclineFailed;
        public const string OfferFailed = global::Property_and_Management.Constants.DialogTitles.OfferFailed;
        public const string OfferGameConfirmation = global::Property_and_Management.Constants.DialogTitles.OfferGameConfirmation;
        public const string ApproveRequestConfirmation = global::Property_and_Management.Constants.DialogTitles.ApproveRequestConfirmation;
        public const string DeclineRequestConfirmation = global::Property_and_Management.Constants.DialogTitles.DeclineRequestConfirmation;
        public const string CancelRequestConfirmation = global::Property_and_Management.Constants.DialogTitles.CancelRequestConfirmation;
        public const string DeleteGameConfirmation = global::Property_and_Management.Constants.DialogTitles.DeleteGameConfirmation;
        public const string GameRemoved = global::Property_and_Management.Constants.DialogTitles.GameRemoved;
        public const string CannotDeleteGame = global::Property_and_Management.Constants.DialogTitles.CannotDeleteGame;
    }

    internal static class DialogButtons
    {
        public const string Ok = global::Property_and_Management.Constants.DialogButtons.Ok;
        public const string Cancel = global::Property_and_Management.Constants.DialogButtons.Cancel;
        public const string GoBack = global::Property_and_Management.Constants.DialogButtons.GoBack;
        public const string Approve = global::Property_and_Management.Constants.DialogButtons.Approve;
        public const string Decline = global::Property_and_Management.Constants.DialogButtons.Decline;
        public const string Delete = global::Property_and_Management.Constants.DialogButtons.Delete;
        public const string CancelRequest = global::Property_and_Management.Constants.DialogButtons.CancelRequest;
        public const string Offer = global::Property_and_Management.Constants.DialogButtons.Offer;
    }

    internal static class DialogMessages
    {
        public const string UnexpectedErrorOccurred = global::Property_and_Management.Constants.DialogMessages.UnexpectedErrorOccurred;
        public const string NoReasonProvided = global::Property_and_Management.Constants.DialogMessages.NoReasonProvided;
        public const string CreateRequestValidationError = global::Property_and_Management.Constants.DialogMessages.CreateRequestValidationError;
        public const string CreateRentalValidationError = global::Property_and_Management.Constants.DialogMessages.CreateRentalValidationError;
    }

    internal static class NotificationTitles
    {
        public const string UpcomingRentalReminder = global::Property_and_Management.Constants.NotificationTitles.UpcomingRentalReminder;
        public const string BookingUnavailable = global::Property_and_Management.Constants.NotificationTitles.BookingUnavailable;
        public const string RentalRequestDeclined = global::Property_and_Management.Constants.NotificationTitles.RentalRequestDeclined;
        public const string RentalRequestCancelled = global::Property_and_Management.Constants.NotificationTitles.RentalRequestCancelled;
        public const string RentalRequestApproved = global::Property_and_Management.Constants.NotificationTitles.RentalRequestApproved;
        public const string OfferReceived = global::Property_and_Management.Constants.NotificationTitles.OfferReceived;
        public const string OfferAccepted = global::Property_and_Management.Constants.NotificationTitles.OfferAccepted;
        public const string RentalConfirmed = global::Property_and_Management.Constants.NotificationTitles.RentalConfirmed;
        public const string OfferDenied = global::Property_and_Management.Constants.NotificationTitles.OfferDenied;
        public const string OfferDeclined = global::Property_and_Management.Constants.NotificationTitles.OfferDeclined;
    }

    internal static class ValidationMessages
    {
        public const string MaximumPlayerCountComparedToMinimum =
            global::Property_and_Management.Constants.ValidationMessages.MaximumPlayerCountComparedToMinimum;

        public static string NameLengthRange(int minimumLength, int maximumLength) =>
            global::Property_and_Management.Constants.ValidationMessages.NameLengthRange(minimumLength, maximumLength);

        public static string PriceMinimum(decimal minimumPrice) =>
            global::Property_and_Management.Constants.ValidationMessages.PriceMinimum(minimumPrice);

        public static string MinimumPlayerCount(int minimumPlayers) =>
            global::Property_and_Management.Constants.ValidationMessages.MinimumPlayerCount(minimumPlayers);

        public static string DescriptionLengthRange(int minimumLength, int maximumLength) =>
            global::Property_and_Management.Constants.ValidationMessages.DescriptionLengthRange(minimumLength, maximumLength);
    }

    internal static class GameValidation
    {
        public const int MinimumNameLength = global::Property_and_Management.Constants.GameValidation.MinimumNameLength;
        public const int MaximumNameLength = global::Property_and_Management.Constants.GameValidation.MaximumNameLength;
        public const decimal MinimumAllowedPrice = global::Property_and_Management.Constants.GameValidation.MinimumAllowedPrice;
        public const int MinimumPlayerCount = global::Property_and_Management.Constants.GameValidation.MinimumPlayerCount;
        public const int MinimumDescriptionLength = global::Property_and_Management.Constants.GameValidation.MinimumDescriptionLength;
        public const int MaximumDescriptionLength = global::Property_and_Management.Constants.GameValidation.MaximumDescriptionLength;
        public const int DefaultMinimumPlayers = global::Property_and_Management.Constants.GameValidation.DefaultMinimumPlayers;
        public const int DefaultMaximumPlayers = global::Property_and_Management.Constants.GameValidation.DefaultMaximumPlayers;
    }
}