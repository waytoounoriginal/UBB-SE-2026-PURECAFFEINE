using System;
using System.Collections.Immutable;
using Property_and_Management.Src.DataTransferObjects;

namespace Property_and_Management.Src.Interface
{
    public interface IRequestService
    {
        ImmutableList<RequestDTO> GetRequestsForRenter(int renterUserId);

        ImmutableList<RequestDTO> GetRequestsForOwner(int ownerUserId);

        Result<int, CreateRequestError> CreateRequest(
            int gameId,
            int renterUserId,
            int ownerUserId,
            DateTime startDate,
            DateTime endDate);

        Result<int, ApproveRequestError> ApproveRequest(int requestId, int ownerUserId);

        Result<int, DenyRequestError> DenyRequest(int requestId, int ownerUserId, string declineReason);

        int CancelRequest(int requestId, int cancellingUserId);

        void OnGameDeactivated(int gameId);

        bool CheckAvailability(int gameId, DateTime startDate, DateTime endDate);

        ImmutableList<(DateTime StartDate, DateTime EndDate)> GetBookedDates(
            int gameId,
            int calendarMonth,
            int calendarYear);

        Result<int, OfferError> OfferGame(int requestId, int offeringOwnerUserId);
    }
}