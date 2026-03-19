using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Property_and_Management.src.DTO;
using Property_and_Management.src.Interface;
using Property_and_Management.src.Model;
using Property_and_Management.src.Repository;
using Property_and_Management.src.SQL.Attributes;

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

        private RequestRepository _requestRepository;
        private RentalRepository _rentalRepository;
        private NotificationService _notificationService;
        private GameRepository _gameRepository;
        private DatabaseConnection _db;

        public void SetDatabaseConnection(DatabaseConnection db) =>
        _db = db;

        public ImmutableList<RequestDTO> GetRequestsForRenter(int renterId)
        {
            return _requestRepository
                .GetRequestsByRenter(renterId)
                .Select(r => new RequestDTO(r.Id, r.GameId, r.RenterId, r.OwnerId, r.StartDate, r.EndDate))
                .ToImmutableList();
        }

        public ImmutableList<RequestDTO> GetRequestsForOwner(int ownerId)
        {
            return _requestRepository
                .GetRequestsByOwner(ownerId)
                .Select(r => new RequestDTO(r.Id, r.GameId, r.RenterId, r.OwnerId, r.StartDate, r.EndDate))
                .ToImmutableList();
        }

        public void SetRequestRepository(RequestRepository newRequestRepository) =>
            _requestRepository = newRequestRepository;
        public void SetRentalRepository(RentalRepository newRentalRepository) =>
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

            var request = new Request(0, gameId, renterId, ownerId, startDate, endDate);
            _requestRepository.Add(request);

            return _requestRepository
                .GetRequestsByRenter(renterId)
                .Last(r => r.GameId == gameId &&
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

            if (request.OwnerId != ownerId)
                return (int)ApproveRequestError.UNAUTHORIZED_ERROR;

            var bufferedStart = request.StartDate.AddHours(-48);
            var bufferedEnd = request.EndDate.AddHours(48);

            using var connection = _db.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction(System.Data.IsolationLevel.Serializable);

            try
            {
                var rental = new Rental(0, request.GameId, request.RenterId,
                                        request.OwnerId, request.StartDate, request.EndDate);

                _rentalRepository.Add(rental, connection, transaction);

                var overlapping = _requestRepository
                    .GetRequestsByGame(request.GameId)
                    .Where(r => r.Id != requestId &&
                                r.StartDate < bufferedEnd &&
                                r.EndDate > bufferedStart)
                    .ToList();

                //foreach (var overlap in overlapping)
                //{
                //    _notificationService.SendNotification(
                //        userId: overlap.RenterId,
                //        message: $"Your request for game {overlap.GameId} " +
                //                 $"({overlap.StartDate:d}–{overlap.EndDate:d}) is unavailable. " +
                //                 $"Book a new slot at: /booking/{overlap.GameId}");

                //    _requestRepository.Delete(overlap.Id, connection, transaction);
                //}

                _requestRepository.Delete(requestId, connection, transaction);

                transaction.Commit();
                return rental.Id;
            }
            catch
            {
                transaction.Rollback();
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

            if (request.OwnerId != ownerId)
                return (int)DenyRequestError.UNAUTHORIZED_ERROR;

            _requestRepository.Delete(requestId);

            //_notificationService.SendNotification(
            //    userId: request.RenterId,
            //    message: $"Your request for game {request.GameId} " +
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

            //    _notificationService.SendNotification(
            //        userId: request.RenterId,
            //        message: $"Your request for game {gameId} " +
            //                 $"({request.StartDate:d}–{request.EndDate:d}) has been declined " +
            //                 $"because the game is no longer available.");
            //}
        }

        public ImmutableList<(DateTime, DateTime)> GetBookedDates(int gameId, int month, int year)
        {
            return _requestRepository
                .GetRequestsByGame(gameId)
                .Where(r => r.StartDate.Month == month && r.StartDate.Year == year)
                .Select(r => (r.StartDate, r.EndDate))
                .ToImmutableList();
        }

        public bool CheckAvailability(int gameId, DateTime startDate, DateTime endDate)
        {
            return !_requestRepository
                .GetRequestsByGame(gameId)
                .Any(r => r.StartDate < endDate && r.EndDate > startDate);
        }
    }
}

