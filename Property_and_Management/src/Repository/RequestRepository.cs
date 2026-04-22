using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using Microsoft.Data.SqlClient;
using Property_and_Management.Src.Interface;
using Property_and_Management.Src.Model;

namespace Property_and_Management.Src.Repository
{
    public class RequestRepository : IRequestRepository
    {
        private const int NewRequestId = 0;
        private const int MissingForeignKeyId = 0;
        private const string ConnectionStringName = "BoardRent";

        private readonly string boardRentConnectionString =
            System.Configuration.ConfigurationManager.ConnectionStrings[ConnectionStringName]?.ConnectionString ?? string.Empty;

        private const string BaseRequestSelectQuery =
            "SELECT r.*, renterUser.display_name AS renter_display_name, ownerUser.display_name AS owner_display_name, " +
            "g.name AS game_name, g.image AS game_image, " +
            "offeringUser.display_name AS offering_user_display_name " +
            "FROM Requests r " +
            "LEFT JOIN Users renterUser ON renterUser.Id = r.renter_id " +
            "LEFT JOIN Users ownerUser ON ownerUser.Id = r.owner_id " +
            "LEFT JOIN Games g ON g.game_id = r.game_id " +
            "LEFT JOIN Users offeringUser ON offeringUser.Id = r.offering_user_id";

        private static Request ReadFullRequestFromReader(SqlDataReader databaseReader)
        {
            var requestedGame = new Game
            {
                Id = (int)databaseReader["game_id"],
                Name = databaseReader["game_name"] as string ?? string.Empty,
                Image = databaseReader["game_image"] as byte[] ?? Array.Empty<byte>()
            };
            var renterUser = new User((int)databaseReader["renter_id"], databaseReader["renter_display_name"] as string ?? string.Empty);
            var ownerUser = new User((int)databaseReader["owner_id"], databaseReader["owner_display_name"] as string ?? string.Empty);
            var requestStatus = (RequestStatus)(int)databaseReader["status"];
            User? offeringUser = null;
            var offeringUserIdValue = databaseReader["offering_user_id"];
            if (offeringUserIdValue != DBNull.Value)
            {
                offeringUser = new User((int)offeringUserIdValue, databaseReader["offering_user_display_name"] as string ?? string.Empty);
            }
            return new Request((int)databaseReader["request_id"], requestedGame, renterUser, ownerUser,
                (DateTime)databaseReader["start_date"], (DateTime)databaseReader["end_date"], requestStatus, offeringUser);
        }

        public ImmutableList<Request> GetAll()
        {
            var allRetrievedRequests = new List<Request>();
            using (var connection = new SqlConnection(boardRentConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = BaseRequestSelectQuery;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            allRetrievedRequests.Add(ReadFullRequestFromReader(reader));
                        }
                    }
                }
            }
            return allRetrievedRequests.ToImmutableList();
        }

        public void Add(Request requestToInsert)
        {
            using (var connection = new SqlConnection(boardRentConnectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    AddRequestWithinTransaction(requestToInsert, connection, transaction);
                    transaction.Commit();
                }
            }
        }

        private static void AddRequestWithinTransaction(Request requestToInsert, SqlConnection connection, SqlTransaction transaction)
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText =
                "INSERT INTO Requests(game_id, renter_id, owner_id, start_date, end_date, status, offering_user_id) " +
                "VALUES(@game_id, @renter_id, @owner_id, @start_date, @end_date, @status, @offering_user_id); " +
                "SELECT SCOPE_IDENTITY();";
            command.Parameters.AddWithValue("@game_id", requestToInsert.Game?.Id ?? MissingForeignKeyId);
            command.Parameters.AddWithValue("@renter_id", requestToInsert.Renter?.Id ?? MissingForeignKeyId);
            command.Parameters.AddWithValue("@owner_id", requestToInsert.Owner?.Id ?? MissingForeignKeyId);
            command.Parameters.AddWithValue("@start_date", requestToInsert.StartDate);
            command.Parameters.AddWithValue("@end_date", requestToInsert.EndDate);
            command.Parameters.AddWithValue("@status", (int)requestToInsert.Status);
            command.Parameters.AddWithValue("@offering_user_id", requestToInsert.OfferingUser?.Id ?? (object)DBNull.Value);
            requestToInsert.Id = Convert.ToInt32(command.ExecuteScalar());
        }

        private static readonly string DeleteRequestWithOutputQuery =
            "DELETE r OUTPUT deleted.request_id, deleted.game_id, deleted.renter_id, deleted.owner_id, " +
            "deleted.start_date, deleted.end_date, deleted.status, deleted.offering_user_id, " +
            "renterUser.display_name AS renter_display_name, ownerUser.display_name AS owner_display_name, " +
            "g.name AS game_name, g.image AS game_image, " +
            "offeringUser.display_name AS offering_user_display_name " +
            "FROM Requests r " +
            "LEFT JOIN Users renterUser ON renterUser.Id = r.renter_id " +
            "LEFT JOIN Users ownerUser ON ownerUser.Id = r.owner_id " +
            "LEFT JOIN Games g ON g.game_id = r.game_id " +
            "LEFT JOIN Users offeringUser ON offeringUser.Id = r.offering_user_id " +
            "WHERE r.request_id = @id";

        public Request Delete(int requestIdToRemove)
        {
            using (var connection = new SqlConnection(boardRentConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = DeleteRequestWithOutputQuery;
                    command.Parameters.AddWithValue("@id", requestIdToRemove);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return ReadRequestFromDeleteOutputReader(reader);
                        }
                    }
                }
            }
            throw new KeyNotFoundException();
        }

        private static Request ReadRequestFromDeleteOutputReader(SqlDataReader deleteOutputReader)
        {
            var deletedRequestedGame = new Game
            {
                Id = (int)deleteOutputReader["game_id"],
                Name = deleteOutputReader["game_name"] as string ?? string.Empty,
                Image = deleteOutputReader["game_image"] as byte[] ?? Array.Empty<byte>()
            };
            var deletedRenterUser = new User((int)deleteOutputReader["renter_id"], deleteOutputReader["renter_display_name"] as string ?? string.Empty);
            var deletedOwnerUser = new User((int)deleteOutputReader["owner_id"], deleteOutputReader["owner_display_name"] as string ?? string.Empty);
            var deletedRequestStatus = (RequestStatus)(int)deleteOutputReader["status"];
            User? deletedOfferingUser = null;
            var deletedOfferingUserIdValue = deleteOutputReader["offering_user_id"];
            if (deletedOfferingUserIdValue != DBNull.Value)
            {
                deletedOfferingUser = new User((int)deletedOfferingUserIdValue, deleteOutputReader["offering_user_display_name"] as string ?? string.Empty);
            }

            return new Request((int)deleteOutputReader["request_id"], deletedRequestedGame, deletedRenterUser, deletedOwnerUser,
                (DateTime)deleteOutputReader["start_date"], (DateTime)deleteOutputReader["end_date"], deletedRequestStatus, deletedOfferingUser);
        }

        public void Update(int requestIdToUpdate, Request requestDataToUpdate)
        {
            using (var connection = new SqlConnection(boardRentConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "UPDATE Requests SET game_id = @game_id, renter_id = @renter_id, owner_id = @owner_id, " +
                        "start_date = @start_date, end_date = @end_date, status = @status, " +
                        "offering_user_id = @offering_user_id WHERE request_id = @id";
                    command.Parameters.AddWithValue("@id", requestIdToUpdate);
                    command.Parameters.AddWithValue("@game_id", requestDataToUpdate.Game?.Id ?? MissingForeignKeyId);
                    command.Parameters.AddWithValue("@renter_id", requestDataToUpdate.Renter?.Id ?? MissingForeignKeyId);
                    command.Parameters.AddWithValue("@owner_id", requestDataToUpdate.Owner?.Id ?? MissingForeignKeyId);
                    command.Parameters.AddWithValue("@start_date", requestDataToUpdate.StartDate);
                    command.Parameters.AddWithValue("@end_date", requestDataToUpdate.EndDate);
                    command.Parameters.AddWithValue("@status", (int)requestDataToUpdate.Status);
                    command.Parameters.AddWithValue("@offering_user_id", requestDataToUpdate.OfferingUser?.Id ?? (object)DBNull.Value);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void UpdateStatus(int requestIdToUpdateStatus, RequestStatus newRequestStatus, int? offeringUserId)
        {
            using (var connection = new SqlConnection(boardRentConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "UPDATE Requests SET status = @status, offering_user_id = @offering_user_id WHERE request_id = @id";
                    command.Parameters.AddWithValue("@id", requestIdToUpdateStatus);
                    command.Parameters.AddWithValue("@status", (int)newRequestStatus);
                    command.Parameters.AddWithValue("@offering_user_id", offeringUserId ?? (object)DBNull.Value);
                    command.ExecuteNonQuery();
                }
            }
        }

        public Request Get(int requestIdToFind)
        {
            using (var connection = new SqlConnection(boardRentConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = BaseRequestSelectQuery + " WHERE r.request_id = @id";
                    command.Parameters.AddWithValue("@id", requestIdToFind);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return ReadFullRequestFromReader(reader);
                        }
                    }
                }
            }
            throw new KeyNotFoundException();
        }

        public ImmutableList<Request> GetRequestsByOwner(int ownerUserId)
        {
            var ownerRequests = new List<Request>();
            using (var connection = new SqlConnection(boardRentConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = BaseRequestSelectQuery + " WHERE r.owner_id = @owner_id";
                    command.Parameters.AddWithValue("@owner_id", ownerUserId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ownerRequests.Add(ReadFullRequestFromReader(reader));
                        }
                    }
                }
            }
            return ownerRequests.ToImmutableList();
        }

        public ImmutableList<Request> GetRequestsByRenter(int renterUserId)
        {
            var renterRequests = new List<Request>();
            using (var connection = new SqlConnection(boardRentConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = BaseRequestSelectQuery + " WHERE r.renter_id = @renter_id";
                    command.Parameters.AddWithValue("@renter_id", renterUserId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            renterRequests.Add(ReadFullRequestFromReader(reader));
                        }
                    }
                }
            }
            return renterRequests.ToImmutableList();
        }

        public ImmutableList<Request> GetRequestsByGame(int requestedGameId)
        {
            var gameRequests = new List<Request>();
            using (var connection = new SqlConnection(boardRentConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = BaseRequestSelectQuery + " WHERE r.game_id = @game_id";
                    command.Parameters.AddWithValue("@game_id", requestedGameId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            gameRequests.Add(ReadFullRequestFromReader(reader));
                        }
                    }
                }
            }
            return gameRequests.ToImmutableList();
        }

        public ImmutableList<Request> GetOverlappingRequests(
            int gameIdForOverlapCheck,
            int requestIdToExclude,
            DateTime bufferedStartDate,
            DateTime bufferedEndDate)
        {
            using var connection = new SqlConnection(boardRentConnectionString);
            connection.Open();
            return QueryOverlappingRequestsWithinConnection(
                gameIdForOverlapCheck,
                requestIdToExclude,
                bufferedStartDate,
                bufferedEndDate,
                connection,
                transaction: null).ToImmutableList();
        }

        public int ApproveAtomically(
            Request requestToApprove,
            ImmutableList<Request> conflictingRequests)
        {
            using var connection = new SqlConnection(boardRentConnectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            try
            {
                foreach (var conflictingRequest in conflictingRequests)
                {
                    DeleteNotificationsLinkedToRequestWithinTransaction(conflictingRequest.Id, connection, transaction);
                }

                DeleteNotificationsLinkedToRequestWithinTransaction(requestToApprove.Id, connection, transaction);

                var newRentalIdentifier = InsertRentalFromApprovedRequest(requestToApprove, connection, transaction);

                foreach (var conflictingRequest in conflictingRequests)
                {
                    DeleteRequestWithinTransaction(conflictingRequest.Id, connection, transaction);
                }

                DeleteRequestWithinTransaction(requestToApprove.Id, connection, transaction);

                transaction.Commit();
                return newRentalIdentifier;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private static int InsertRentalFromApprovedRequest(Request approvedRequest, SqlConnection connection, SqlTransaction transaction)
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText =
                "INSERT INTO Rentals(game_id, renter_id, owner_id, start_date, end_date) " +
                "VALUES(@game_id, @renter_id, @owner_id, @start_date, @end_date); SELECT SCOPE_IDENTITY();";
            command.Parameters.AddWithValue("@game_id", approvedRequest.Game?.Id ?? MissingForeignKeyId);
            command.Parameters.AddWithValue("@renter_id", approvedRequest.Renter?.Id ?? MissingForeignKeyId);
            command.Parameters.AddWithValue("@owner_id", approvedRequest.Owner?.Id ?? MissingForeignKeyId);
            command.Parameters.AddWithValue("@start_date", approvedRequest.StartDate);
            command.Parameters.AddWithValue("@end_date", approvedRequest.EndDate);
            return Convert.ToInt32(command.ExecuteScalar());
        }

        private static List<Request> QueryOverlappingRequestsWithinConnection(
            int gameIdForOverlapCheck,
            int requestIdToExclude,
            DateTime bufferedStartDate,
            DateTime bufferedEndDate,
            SqlConnection connection,
            SqlTransaction? transaction)
        {
            var overlappingRequests = new List<Request>();
            using var command = connection.CreateCommand();
            if (transaction != null)
            {
                command.Transaction = transaction;
            }

            command.CommandText =
                "SELECT r.request_id, r.game_id, r.renter_id, r.owner_id, r.start_date, r.end_date, " +
                "renterUser.display_name AS renter_display_name, ownerUser.display_name AS owner_display_name, " +
                "g.name AS game_name, g.image AS game_image " +
                "FROM Requests r " +
                "LEFT JOIN Users renterUser ON renterUser.Id = r.renter_id " +
                "LEFT JOIN Users ownerUser ON ownerUser.Id = r.owner_id " +
                "LEFT JOIN Games g ON g.game_id = r.game_id " +
                "WHERE r.game_id = @game_id AND r.request_id != @exclude_id " +
                "AND r.start_date < @buffered_end AND r.end_date > @buffered_start";
            command.Parameters.AddWithValue("@game_id", gameIdForOverlapCheck);
            command.Parameters.AddWithValue("@exclude_id", requestIdToExclude);
            command.Parameters.AddWithValue("@buffered_end", bufferedEndDate);
            command.Parameters.AddWithValue("@buffered_start", bufferedStartDate);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var overlappingGame = new Game
                {
                    Id = (int)reader["game_id"],
                    Name = reader["game_name"] as string ?? string.Empty,
                    Image = reader["game_image"] as byte[] ?? Array.Empty<byte>()
                };
                var overlappingRenterUser = new User((int)reader["renter_id"], reader["renter_display_name"] as string ?? string.Empty);
                var overlappingOwnerUser = new User((int)reader["owner_id"], reader["owner_display_name"] as string ?? string.Empty);
                overlappingRequests.Add(new Request(
                    (int)reader["request_id"],
                    overlappingGame,
                    overlappingRenterUser,
                    overlappingOwnerUser,
                    (DateTime)reader["start_date"],
                    (DateTime)reader["end_date"]));
            }

            return overlappingRequests;
        }

        private static void DeleteNotificationsLinkedToRequestWithinTransaction(int linkedRequestId, SqlConnection connection, SqlTransaction transaction)
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "DELETE FROM Notifications WHERE related_request_id = @id";
            command.Parameters.AddWithValue("@id", linkedRequestId);
            command.ExecuteNonQuery();
        }

        private static void DeleteRequestWithinTransaction(int requestIdToDelete, SqlConnection connection, SqlTransaction transaction)
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "DELETE FROM Requests WHERE request_id = @id";
            command.Parameters.AddWithValue("@id", requestIdToDelete);
            command.ExecuteNonQuery();
        }
    }
}