using System.Collections.Immutable;
using System.Linq;
using Property_and_Management.Src.DataTransferObjects;
using Property_and_Management.Src.Interface;
using Property_and_Management.Src.Model;

namespace Property_and_Management.Src.Viewmodels
{
    public class RequestsFromOthersViewModel : PagedViewModel<RequestDTO>
    {
        private readonly IRequestService rentalRequestService;
        private readonly ICurrentUserContext currentUserContext;

        public int CurrentGameOwnerUserId { get; private set; }

        public RequestsFromOthersViewModel(IRequestService rentalRequestService, ICurrentUserContext currentUserContext)
        {
            this.rentalRequestService = rentalRequestService;
            this.currentUserContext = currentUserContext;
            Reload();
        }

        public override string ShowingText => $"Showing {DisplayedCount} of {TotalCount} requests";

        public void LoadRequests() => Reload();

        protected override void Reload()
        {
            CurrentGameOwnerUserId = currentUserContext.CurrentUserId;

            var openRequestsForOwnerSortedByNewest = rentalRequestService
                .GetRequestsForOwner(CurrentGameOwnerUserId)
                .Where(request => request.Status == RequestStatus.Open)
                .OrderByDescending(request => request.StartDate)
                .ToImmutableList();
            SetAllItems(openRequestsForOwnerSortedByNewest);
        }

        public string? TryApproveRequest(int requestIdToApprove)
        {
            var approvalResult = rentalRequestService.ApproveRequest(requestIdToApprove, CurrentGameOwnerUserId);
            if (approvalResult.IsSuccess)
            {
                Reload();
                return null;
            }

            return approvalResult.Error switch
            {
                ApproveRequestError.Unauthorized => "You are not authorized to approve this request.",
                ApproveRequestError.NotFound => "Request not found.",
                ApproveRequestError.TransactionFailed => "Could not approve the request. Please try again.",
                _ => Constants.DialogMessages.UnexpectedErrorOccurred
            };
        }

        public string? TryDenyRequest(int requestIdToDeny, string? rawDenialReason)
        {
            var trimmedDenialReason = (rawDenialReason ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(trimmedDenialReason))
            {
                trimmedDenialReason = Constants.DialogMessages.NoReasonProvided;
            }

            var denialResult = rentalRequestService.DenyRequest(requestIdToDeny, CurrentGameOwnerUserId, trimmedDenialReason);
            if (denialResult.IsSuccess)
            {
                Reload();
                return null;
            }

            return denialResult.Error switch
            {
                DenyRequestError.NotFound => "Request not found.",
                DenyRequestError.Unauthorized => "You are not authorized to deny this request.",
                _ => Constants.DialogMessages.UnexpectedErrorOccurred
            };
        }

        public string? TryOfferGame(int requestIdForGameOffer)
        {
            var gameOfferResult = rentalRequestService.OfferGame(requestIdForGameOffer, CurrentGameOwnerUserId);
            if (gameOfferResult.IsSuccess)
            {
                Reload();
                return null;
            }

            return gameOfferResult.Error switch
            {
                OfferError.NotFound => "Request not found.",
                OfferError.NotOwner => "You are not the owner of this game.",
                OfferError.RequestNotOpen => "This request is no longer open.",
                OfferError.TransactionFailed => "Could not approve the request. Please try again.",
                _ => Constants.DialogMessages.UnexpectedErrorOccurred
            };
        }
    }
}