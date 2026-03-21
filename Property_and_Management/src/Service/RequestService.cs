using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Property_and_Management.src.DTO;
using Property_and_Management.src.Interface;
using Property_and_Management.src.Model;
using Windows.Gaming.Input;
using static System.Net.Mime.MediaTypeNames;

namespace Property_and_Management.src.Service
{
    public enum CreateRequestError
    {
        OWNER_CANNOT_RENT_ERROR = -1,
        DATES_UNAVAILABLE_ERROR = -2,
        GAMEID_DOES_NOT_EXIST_ERROR = -3
    }
    public enum ApproveRequestError
    {
        UNAUTHORIZED_ERROR = -1,
        NOT_FOUND_ERROR = -2,
        TRANSACTION_FAILED_ERROR = -3
    }
    public enum DenyRequestError
    {
        UNAUTHORIZED_ERROR = -1,
        NOT_FOUND_ERROR = -2
    }
    public class RequestService
    {

        private IRequestRepository _requestRepository;
        private IRentalRepository _rentalRepository;
        private NotificationService _notificationService;
        private IGameRepository _gameRepository;
        // Db connection handling should be refactored to an interface later, removing it from this refactor since SQL attributes module is gone.

        public ImmutableList<RequestDTO> GetRequestsForRenter(int renterId)
        {
            return _requestRepository
                .GetRequestsByRenter(renterId)
                .Select(r => new RequestDTO(r))
                .ToImmutableList();
        }

        public ImmutableList<RequestDTO> GetRequestsForOwner(int ownerId)
        {
            return _requestRepository
                .GetRequestsByOwner(ownerId)
                .Select(r => new RequestDTO(r))
                .ToImmutableList();
        }

        public void SetRequestRepository(IRequestRepository newRequestRepository) =>
            _requestRepository = newRequestRepository;
        public void SetRentalRepository(IRentalRepository newRentalRepository) =>
            _rentalRepository = newRentalRepository;
        public void SetNotificationService(NotificationService newNotificationService) =>
            _notificationService = newNotificationService;

        //[BL-LFC-01] A new Request is created. We say it is PENDING while existing in the database.
        public int CreateRequest(int gameId, int renterId, int ownerId, DateTime startDate, DateTime endDate)
        {
            if (renterId == ownerId)
                return (int)CreateRequestError.OWNER_CANNOT_RENT_ERROR;

            //try { _gameRepository.Get(gameId);}
            //catch (KeyNotFoundException)
            //{ return (int)CreateRequestError.GAMEID_DOES_NOT_EXIST_ERROR; }

            if (!CheckAvailability(gameId, startDate, endDate))
                return (int)CreateRequestError.DATES_UNAVAILABLE_ERROR;

            var request = new Request(0, new Game { Id = gameId }, new User { Id = renterId }, new User { Id = ownerId }, startDate, endDate);
            _requestRepository.Add(request);

            return _requestRepository
                .GetRequestsByRenter(renterId)
                .Last(r => r.Game?.Id == gameId &&
                   r.StartDate == startDate &&
                   r.EndDate == endDate)
                .Id;
        }

        //[BL-LFC-02] When an Owner sends an ACCEPT signal for a Request:
        //(a) the system shall atomically create a new Rental entity from the Request's data;
        //(b) the system shall delete all other PENDING Requests for the same game_id whose date range overlaps with the accepted rental period (including its 48-hour buffer);
        //(c) the system shall send a notification to each Renter whose overlapping Request was deleted, stating the game is unavailable in their requested period and providing a link to the booking interface;
        //(d) the original Request entity shall be deleted.
        //All steps (a)–(d) must succeed atomically; if any fail, the transaction shall be rolled back.
        public int ApproveRequest(int requestId, int ownerId)
        {
            Request request;
            try { request = _requestRepository.Get(requestId); }
            catch (KeyNotFoundException)
            {
                return (int)ApproveRequestError.NOT_FOUND_ERROR;
            }

            if (request.Owner?.Id != ownerId)
                return (int)ApproveRequestError.UNAUTHORIZED_ERROR;

            var bufferedStart = request.StartDate.AddHours(-48);
            var bufferedEnd = request.EndDate.AddHours(48);

            try
            {
                var rental = new Rental(0, request.Game, request.Renter,
                                        request.Owner, request.StartDate, request.EndDate);

                _rentalRepository.Add(rental);

                var overlapping = _requestRepository
                    .GetRequestsByGame(request.Game?.Id ?? 0)
                    .Where(r => r.Id != requestId &&
                                r.StartDate < bufferedEnd &&
                                r.EndDate > bufferedStart)
                    .ToList();

                //foreach (var overlap in overlapping)
                //{
                //    _notificationService.SendNotification(
                //        userId: overlap.Renter?.Id ?? 0,
                //        message: $"Your request for game {overlap.Game?.Id} " +
                //                 $"({overlap.StartDate:d}–{overlap.EndDate:d}) is unavailable. " +
                //                 $"Book a new slot at: /booking/{overlap.Game?.Id}");

                //    _requestRepository.Delete(overlap.Id);
                //}

                _requestRepository.Delete(requestId);

                return rental.Id;
            }
            catch
            {
                return (int)ApproveRequestError.TRANSACTION_FAILED_ERROR;
            }
        }

        //[BL-LFC-03] When an Owner sends a DECLINE signal for a Request, the system shall delete the Request entity. No Rental is created. The user shall also be notified that their request has been declined.
        public int DenyRequest(int requestId, int ownerId, string reason)
        {
            Request request;
            try { request = _requestRepository.Get(requestId); }
            catch (KeyNotFoundException)
            { return (int)DenyRequestError.NOT_FOUND_ERROR; }

            if (request.Owner?.Id != ownerId)
                return (int)DenyRequestError.UNAUTHORIZED_ERROR;

            _requestRepository.Delete(requestId);

            //_notification_service.SendNotification(
            //    userId: request.Renter?.Id ?? 0,
            //    message: $"Your request for game {request.Game?.Id} " +
            //             $"({request.StartDate:d}–{request.EndDate:d}) was declined. " +
            //             $"Reason: {reason}");

            return requestId;
        }

        //[BL-LFC-04] A Renter may cancel (delete) their own PENDING Request at any time without requiring Owner approval.
        public void Cancelrequest(int requestId)
        {
            _requestRepository.Delete(requestId);
        }

        //[BL-LFC-05] Upon a game having its Active flag being set to FALSE, all requests to that game shall be declined and the respective users be notified of the decline.
        public void OnGameDeactivated(int gameId)
        {
            var pending = _requestRepository.GetRequestsByGame(gameId);

            //foreach (var request in pending)
            //{
            //    _requestRepository.Delete(request.Id);

            //    _notification_service.SendNotification(
            //        userId: request.Renter?.Id ?? 0,
            //        message: $"Your request for game {gameId} " +
            //                 $"({request.StartDate:d}–{request.EndDate:d}) has been declined " +
            //                 $"because the game is no longer available.");
            //}
        }

        //[API-GBD-04] The method shall return a list of objects. Each object shall contain:
        //StartDate (DateTime) — the start of the booked interval;
        //EndDate (DateTime) — the end of the booked interval, including the 48-hour buffer period
        //(i.e., rental end_date + 48 hours).
        //[API-GBD-05] The returned list shall be sorted by StartDate ascending.
        public ImmutableList<(DateTime, DateTime)> GetBookedDates(int gameId, int month = 0, int year = 0)
        {
            if (month == 0)
                month = DateTime.Now.Month;

            if (year == 0)
                year = DateTime.Now.Year;

            return _requestRepository
                .GetRequestsByGame(gameId)
                .Where(r => r.StartDate.Month == month && r.StartDate.Year == year)
                .OrderBy(r => r.StartDate)
                .Select(r => (r.StartDate, r.EndDate.AddDays(2)))
                .ToImmutableList();
        }

        //[API-CAV-04] IsAvailable shall be TRUE if and only if all of the following conditions hold:
        //(a) the requested [startDate, endDate] range does not overlap with any existing Rental interval
        //for the specified game_id;
        //(b) the requested startDate is not within the 48-hour buffer period of any existing Rental;
        //(c) the requested startDate is not more than one month in the future from the current date;
        //(d) the requested endDate is not more than one month in the future from the current date’
        //(e) the game_id corresponds to an existent and active game. 
        //If any condition is not met, IsAvailable shall be FALSE.

        public bool CheckAvailability(int gameId, DateTime startDate, DateTime endDate)
        {
            bool dateNotTooLate = endDate <= DateTime.Now.AddMonths(1);

            bool isTheGameActive = _gameRepository
                .GetAll()
                .Count(g => g.Id == gameId && g.IsActive) == 1;

            bool inAvailableTimeInterval = !_requestRepository
                .GetRequestsByGame(gameId)
                .Any(r => r.StartDate <= endDate && r.EndDate >= startDate);

            return dateNotTooLate && isTheGameActive && inAvailableTimeInterval;
        }
    }
}

