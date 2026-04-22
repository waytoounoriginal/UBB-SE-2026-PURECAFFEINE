using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Property_and_Management;
using Property_and_Management.Src.Constants;
using Property_and_Management.Src.DataTransferObjects;
using Property_and_Management.Src.Interface;
using Property_and_Management.Src.Model;

namespace Property_and_Management.Src.Service
{
    public class RequestService : IRequestService
    {
        private const int NewRequestId = 0;
        private const int MissingUserId = 0;
        private const int MissingForeignKeyId = 0;
        private const int MissingOptionalDatePart = 0;
        private const int AvailabilityWindowMonths = 1;

        private readonly IRequestRepository requestDataRepository;
        private readonly IRentalRepository rentalConflictRepository;
        private readonly INotificationService requestNotificationService;
        private readonly IGameRepository gameValidationRepository;
        private readonly IMapper<Request, RequestDTO> requestDtoMapper;

        public RequestService(
            IRequestRepository requestRepository,
            IRentalRepository rentalRepository,
            IGameRepository gameRepository,
            INotificationService notificationService,
            IMapper<Request, RequestDTO> requestMapper)
        {
            this.requestDataRepository = requestRepository;
            this.rentalConflictRepository = rentalRepository;
            this.gameValidationRepository = gameRepository;
            this.requestNotificationService = notificationService;
            this.requestDtoMapper = requestMapper;
        }

        public ImmutableList<RequestDTO> GetRequestsForRenter(int renterUserId) =>
            requestDataRepository
                .GetRequestsByRenter(renterUserId)
                .Select(request => requestDtoMapper.ToDTO(request))
                .ToImmutableList();

        public ImmutableList<RequestDTO> GetRequestsForOwner(int ownerUserId) =>
            requestDataRepository
                .GetRequestsByOwner(ownerUserId)
                .Select(request => requestDtoMapper.ToDTO(request))
                .ToImmutableList();

        public Result<int, CreateRequestError> CreateRequest(
            int gameId,
            int renterUserId,
            int ownerUserId,
            DateTime proposedStartDate,
            DateTime proposedEndDate)
        {
            if (renterUserId == ownerUserId)
            {
                return Result<int, CreateRequestError>.Failure(CreateRequestError.OwnerCannotRent);
            }

            try
            {
                gameValidationRepository.Get(gameId);
            }
            catch (KeyNotFoundException)
            {
                return Result<int, CreateRequestError>.Failure(CreateRequestError.GameDoesNotExist);
            }

            if (!CheckAvailability(gameId, proposedStartDate, proposedEndDate))
            {
                return Result<int, CreateRequestError>.Failure(CreateRequestError.DatesUnavailable);
            }

            var newRentalRequest = new Request(
                id: NewRequestId,
                requestedGame: new Game { Id = gameId },
                renterUser: new User { Id = renterUserId },
                ownerUser: new User { Id = ownerUserId },
                startDate: proposedStartDate,
                endDate: proposedEndDate);

            requestDataRepository.Add(newRentalRequest);
            return Result<int, CreateRequestError>.Success(newRentalRequest.Id);
        }

        public Result<int, ApproveRequestError> ApproveRequest(int requestId, int approverOwnerId)
        {
            Request requestToApprove;
            try
            {
                requestToApprove = requestDataRepository.Get(requestId);
            }
            catch (KeyNotFoundException)
            {
                return Result<int, ApproveRequestError>.Failure(ApproveRequestError.NotFound);
            }

            if (requestToApprove.Owner?.Id != approverOwnerId)
            {
                return Result<int, ApproveRequestError>.Failure(ApproveRequestError.Unauthorized);
            }

            if (requestToApprove.Status != RequestStatus.Open)
            {
                return Result<int, ApproveRequestError>.Failure(ApproveRequestError.NotFound);
            }

            if (!TryApproveOpenRequestAndNotify(requestToApprove, out var createdRentalId))
            {
                return Result<int, ApproveRequestError>.Failure(ApproveRequestError.TransactionFailed);
            }

            return Result<int, ApproveRequestError>.Success(createdRentalId);
        }

        public Result<int, DenyRequestError> DenyRequest(int requestId, int denyingOwnerId, string denialReason)
        {
            Request requestToDeny;
            try
            {
                requestToDeny = requestDataRepository.Get(requestId);
            }
            catch (KeyNotFoundException)
            {
                return Result<int, DenyRequestError>.Failure(DenyRequestError.NotFound);
            }

            if (requestToDeny.Owner?.Id != denyingOwnerId)
            {
                return Result<int, DenyRequestError>.Failure(DenyRequestError.Unauthorized);
            }

            requestNotificationService.DeleteNotificationsLinkedToRequest(requestId);
            requestDataRepository.Delete(requestId);

            var deniedRenterId = requestToDeny.Renter?.Id ?? MissingUserId;
            var deniedGameName = requestToDeny.Game?.Name ?? "the selected game";
            SendNotificationToUser(
                deniedRenterId,
                Constants.NotificationTitles.RentalRequestDeclined,
                $"Your request for {deniedGameName} {FormatRequestPeriod(requestToDeny.StartDate, requestToDeny.EndDate)} was declined. Reason: {denialReason}");

            return Result<int, DenyRequestError>.Success(requestId);
        }

        public int CancelRequest(int requestId, int cancellingRenterUserId)
        {
            Request requestToCancel;
            try
            {
                requestToCancel = requestDataRepository.Get(requestId);
            }
            catch (KeyNotFoundException)
            {
                return (int)CancelRequestError.NotFound;
            }

            if (requestToCancel.Renter?.Id != cancellingRenterUserId)
            {
                return (int)CancelRequestError.Unauthorized;
            }

            requestNotificationService.DeleteNotificationsLinkedToRequest(requestId);
            try
            {
                requestDataRepository.Delete(requestId);
            }
            catch (KeyNotFoundException)
            {
                return (int)CancelRequestError.NotFound;
            }

            return requestId;
        }

        public void OnGameDeactivated(int deactivatedGameId)
        {
            var pendingRequestsForGame = requestDataRepository
                .GetRequestsByGame(deactivatedGameId)
                .Where(IsPendingForGameDeactivation)
                .ToImmutableList();

            foreach (var pendingRequest in pendingRequestsForGame)
            {
                requestNotificationService.DeleteNotificationsLinkedToRequest(pendingRequest.Id);
                requestDataRepository.Delete(pendingRequest.Id);

                var affectedRenterId = pendingRequest.Renter?.Id ?? MissingUserId;
                var affectedGameName = pendingRequest.Game?.Name ?? "the selected game";
                SendNotificationToUser(
                    affectedRenterId,
                    Constants.NotificationTitles.RentalRequestCancelled,
                    $"Your request for {affectedGameName} {FormatRequestPeriod(pendingRequest.StartDate, pendingRequest.EndDate)} has been cancelled because the game is no longer available.");
            }
        }

        private static bool IsPendingForGameDeactivation(Request request)
        {
            return request.Status == RequestStatus.Open ||
                   request.Status == RequestStatus.OfferPending;
        }

        public ImmutableList<(DateTime StartDate, DateTime EndDate)> GetBookedDates(
            int gameId,
            int month = MissingOptionalDatePart,
            int year = MissingOptionalDatePart)
        {
            if (month == MissingOptionalDatePart)
            {
                month = DateTime.UtcNow.Month;
            }

            if (year == MissingOptionalDatePart)
            {
                year = DateTime.UtcNow.Year;
            }

            return requestDataRepository
                .GetRequestsByGame(gameId)
                .Where(request => request.StartDate.Month == month && request.StartDate.Year == year)
                .OrderBy(request => request.StartDate)
                .Select(request => (request.StartDate, request.EndDate.AddHours(DomainConstants.RentalBufferHours)))
                .ToImmutableList();
        }

        public bool CheckAvailability(int gameId, DateTime proposedStartDate, DateTime proposedEndDate)
        {
            var oneMonthFromNow = DateTime.UtcNow.AddMonths(AvailabilityWindowMonths);
            if (proposedStartDate > oneMonthFromNow || proposedEndDate > oneMonthFromNow)
            {
                return false;
            }

            Game requestedGame;
            try
            {
                requestedGame = gameValidationRepository.Get(gameId);
            }
            catch (KeyNotFoundException)
            {
                return false;
            }

            if (!requestedGame.IsActive)
            {
                return false;
            }

            bool hasRentalConflict = rentalConflictRepository
                .GetRentalsByGame(gameId)
                .Any(rental => proposedStartDate < rental.EndDate.AddHours(DomainConstants.RentalBufferHours) &&
                               proposedEndDate > rental.StartDate.AddHours(-DomainConstants.RentalBufferHours));

            if (hasRentalConflict)
            {
                return false;
            }

            bool hasRequestConflict = requestDataRepository
                .GetRequestsByGame(gameId)
                .Any(request => request.StartDate.AddHours(-DomainConstants.RentalBufferHours) < proposedEndDate &&
                                request.EndDate.AddHours(DomainConstants.RentalBufferHours) > proposedStartDate);

            return !hasRequestConflict;
        }

        public Result<int, OfferError> OfferGame(int requestId, int offeringGameOwnerId)
        {
            Request requestToOffer;
            try
            {
                requestToOffer = requestDataRepository.Get(requestId);
            }
            catch (KeyNotFoundException)
            {
                return Result<int, OfferError>.Failure(OfferError.NotFound);
            }

            if (requestToOffer.Owner?.Id != offeringGameOwnerId)
            {
                return Result<int, OfferError>.Failure(OfferError.NotOwner);
            }

            if (requestToOffer.Status != RequestStatus.Open)
            {
                return Result<int, OfferError>.Failure(OfferError.RequestNotOpen);
            }

            if (!TryApproveOpenRequestAndNotify(requestToOffer, out var createdRentalId))
            {
                return Result<int, OfferError>.Failure(OfferError.TransactionFailed);
            }

            return Result<int, OfferError>.Success(createdRentalId);
        }

        private void NotifyOverlappingRequestsUnavailable(ImmutableList<Request> overlappingRequests, string unavailableGameName)
        {
            foreach (var overlappingRequest in overlappingRequests)
            {
                var affectedRenterId = overlappingRequest.Renter?.Id ?? MissingUserId;
                SendNotificationToUser(
                    affectedRenterId,
                    Constants.NotificationTitles.BookingUnavailable,
                    $"Your request for {unavailableGameName} {FormatRequestPeriod(overlappingRequest.StartDate, overlappingRequest.EndDate)} was declined because the game is no longer available in that period.");
            }
        }

        private void SendNotificationToUser(
            int recipientUserId,
            string notificationTitle,
            string notificationBody,
            NotificationType notificationType = default,
            int? relatedRequestId = null)
        {
            requestNotificationService.SendNotificationToUser(
                recipientUserId,
                BuildOutgoingNotification(recipientUserId, notificationTitle, notificationBody, notificationType, relatedRequestId));
        }

        private NotificationDTO BuildOutgoingNotification(
            int recipientUserId,
            string notificationTitle,
            string notificationBody,
            NotificationType notificationType,
            int? relatedRequestId)
        {
            return new NotificationDTO
            {
                Id = NewRequestId,
                User = new UserDTO { Id = recipientUserId },
                Timestamp = DateTime.UtcNow,
                Title = notificationTitle,
                Body = notificationBody,
                Type = notificationType,
                RelatedRequestId = relatedRequestId
            };
        }

        private bool TryApproveOpenRequestAndNotify(Request openRequestToApprove, out int createdRentalId)
        {
            var bufferedStartDate = openRequestToApprove.StartDate.AddHours(-DomainConstants.RentalBufferHours);
            var bufferedEndDate = openRequestToApprove.EndDate.AddHours(DomainConstants.RentalBufferHours);

            var conflictingRequests = requestDataRepository.GetOverlappingRequests(
                openRequestToApprove.Game?.Id ?? MissingForeignKeyId,
                openRequestToApprove.Id,
                bufferedStartDate,
                bufferedEndDate);

            try
            {
                createdRentalId = requestDataRepository.ApproveAtomically(openRequestToApprove, conflictingRequests);
            }
            catch
            {
                createdRentalId = MissingForeignKeyId;
                return false;
            }

            var approvedGameName = openRequestToApprove.Game?.Name ?? "the selected game";
            NotifyOverlappingRequestsUnavailable(conflictingRequests, approvedGameName);

            SendNotificationToUser(
                openRequestToApprove.Renter?.Id ?? MissingUserId,
                Constants.NotificationTitles.RentalRequestApproved,
                $"Your request for {approvedGameName} {FormatRequestPeriod(openRequestToApprove.StartDate, openRequestToApprove.EndDate)} was approved.");

            requestNotificationService.ScheduleUpcomingRentalReminder(
                openRequestToApprove.Renter?.Id ?? MissingUserId,
                openRequestToApprove.Owner?.Id ?? MissingUserId,
                openRequestToApprove.Game?.Name ?? "your game",
                openRequestToApprove.StartDate);

            return true;
        }

        private static string FormatRequestPeriod(DateTime periodStartDate, DateTime periodEndDate)
        {
            return $"({periodStartDate:d}-{periodEndDate:d})";
        }
    }
}
