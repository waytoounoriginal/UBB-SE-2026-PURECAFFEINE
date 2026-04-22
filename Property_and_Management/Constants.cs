using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Property_and_Management
{
    internal static class Constants
    {
        public const string AppTrayIconUri = "ms-appx:///Assets/tray_icon.ico";

        internal static class NotificationTitles
        {
            public const string UpcomingRentalReminder = "Upcoming Rental Reminder";
            public const string BookingUnavailable = "Booking Unavailable";
            public const string RentalRequestDeclined = "Rental request declined";
            public const string RentalRequestCancelled = "Rental request cancelled";
            public const string RentalRequestApproved = "Rental request approved";
            public const string OfferReceived = "Game Offer Received";
            public const string OfferAccepted = "Offer Accepted";
            public const string RentalConfirmed = "Rental Confirmed";
            public const string OfferDenied = "Offer Denied";
            public const string OfferDeclined = "Offer Declined";
        }

        internal static class DialogTitles
        {
            public const string ValidationError = "Validation Error";
            public const string RequestFailed = "Request Failed";
            public const string RentalFailed = "Rental Failed";
            public const string ApproveFailed = "Approve Failed";
            public const string DeclineFailed = "Decline Failed";
            public const string OfferFailed = "Offer Failed";
            public const string OfferGameConfirmation = "Offer Game?";
            public const string ApproveRequestConfirmation = "Approve Request?";
            public const string DeclineRequestConfirmation = "Decline Request?";
            public const string CancelRequestConfirmation = "Cancel Request?";
            public const string DeleteGameConfirmation = "Delete Game?";
            public const string GameRemoved = "Game Removed";
            public const string CannotDeleteGame = "Cannot Delete Game";
        }

        internal static class DialogButtons
        {
            public const string Ok = "OK";
            public const string Cancel = "Cancel";
            public const string GoBack = "Go Back";
            public const string Approve = "Approve";
            public const string Decline = "Decline";
            public const string Delete = "Delete";
            public const string CancelRequest = "Cancel Request";
            public const string Offer = "Offer";
        }

        internal static class DialogMessages
        {
            public const string UnexpectedErrorOccurred = "An unexpected error occurred.";
            public const string NoReasonProvided = "No reason provided.";
            public const string CreateRequestValidationError =
                "Please select a game and valid date range (start date must be before end date and not in the past).";
            public const string CreateRentalValidationError =
                "Please select a game, a renter, and a valid date range (start before end, not in the past).";
        }

        internal static class ValidationMessages
        {
            public static string NameLengthRange(int minimumLength, int maximumLength) =>
                $"Name must be between {minimumLength} and {maximumLength} characters.";

            public static string PriceMinimum(decimal minimumPrice) =>
                $"Price must be greater than or equal to {minimumPrice:0}.";

            public static string MinimumPlayerCount(int minimumPlayers) =>
                $"Minimum player count must be at least {minimumPlayers}.";

            public const string MaximumPlayerCountComparedToMinimum =
                "Maximum player count must be greater than or equal to minimum player count.";

            public static string DescriptionLengthRange(int minimumLength, int maximumLength) =>
                $"Description must be between {minimumLength} and {maximumLength} characters.";
        }

        internal static class GameValidation
        {
            public const int MinimumNameLength = 5;
            public const int MaximumNameLength = 30;
            public const decimal MinimumAllowedPrice = 1m;
            public const int MinimumPlayerCount = 1;
            public const int MinimumDescriptionLength = 10;
            public const int MaximumDescriptionLength = 500;
            public const int DefaultMinimumPlayers = 1;
            public const int DefaultMaximumPlayers = 4;
        }
    }
}