using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Data.SqlClient;
using Property_and_Management.src.Interface;
using Property_and_Management.src.Model;

namespace Property_and_Management.src.Repository
{
    public class RequestRepository : IRequestRepository
    {
        private readonly string _connectionString =
            System.Configuration.ConfigurationManager.ConnectionStrings["BoardRent"]?.ConnectionString ?? string.Empty;

        public ImmutableList<Request> GetAll()
        {
            var list = new List<Request>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM Requests";
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var game = new Game { Id = (int)reader["game_id"] };
                            var renter = new User((int)reader["renter_id"]);
                            var owner = new User((int)reader["owner_id"]);
                            var req = new Request((int)reader["request_id"], game, renter, owner, (DateTime)reader["start_date"], (DateTime)reader["end_date"]);
                            list.Add(req);
                        }
                    }
                }
            }
            return list.ToImmutableList();
        }

        public void Add(Request entity)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    Add(entity, connection, transaction);
                    transaction.Commit();
                }
            }
        }

        public void Add(Request entity, SqlConnection connection, SqlTransaction transaction)
        {
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = "INSERT INTO Requests(game_id, renter_id, owner_id, start_date, end_date) VALUES(@game_id, @renter_id, @owner_id, @start_date, @end_date); SELECT SCOPE_IDENTITY();";
                command.Parameters.AddWithValue("@game_id", entity.Game?.Id ?? 0);
                command.Parameters.AddWithValue("@renter_id", entity.Renter?.Id ?? 0);
                command.Parameters.AddWithValue("@owner_id", entity.Owner?.Id ?? 0);
                command.Parameters.AddWithValue("@start_date", entity.StartDate);
                command.Parameters.AddWithValue("@end_date", entity.EndDate);
                var newId = Convert.ToInt32(command.ExecuteScalar());
                entity.Id = newId;
            }
        }

        public Request Delete(int removedEntityId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    var entity = Get(removedEntityId);
                    Delete(removedEntityId, connection, transaction);
                    transaction.Commit();
                    return entity;
                }
            }
        }

        public Request Delete(int id, SqlConnection connection, SqlTransaction transaction)
        {
            var entity = Get(id);
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = "DELETE FROM Requests WHERE request_id = @id";
                command.Parameters.AddWithValue("@id", id);
                command.ExecuteNonQuery();
            }
            return entity;
        }

        public void Update(int updatedEntityId, Request newEntity)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "UPDATE Requests SET game_id = @game_id, renter_id = @renter_id, owner_id = @owner_id, start_date = @start_date, end_date = @end_date WHERE request_id = @id";
                    command.Parameters.AddWithValue("@id", updatedEntityId);
                    command.Parameters.AddWithValue("@game_id", newEntity.Game?.Id ?? 0);
                    command.Parameters.AddWithValue("@renter_id", newEntity.Renter?.Id ?? 0);
                    command.Parameters.AddWithValue("@owner_id", newEntity.Owner?.Id ?? 0);
                    command.Parameters.AddWithValue("@start_date", newEntity.StartDate);
                    command.Parameters.AddWithValue("@end_date", newEntity.EndDate);
                    command.ExecuteNonQuery();
                }
            }
        }

        public Request Get(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM Requests WHERE request_id = @id";
                    command.Parameters.AddWithValue("@id", id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var game = new Game { Id = (int)reader["game_id"] };
                            var renter = new User((int)reader["renter_id"]);
                            var owner = new User((int)reader["owner_id"]);
                            return new Request((int)reader["request_id"], game, renter, owner, (DateTime)reader["start_date"], (DateTime)reader["end_date"]);
                        }
                    }
                }
            }
            throw new KeyNotFoundException();
        }

        public ImmutableList<Request> GetRequestsByOwner(int ownerId)
        {
            var list = new List<Request>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM Requests WHERE owner_id = @owner_id";
                    command.Parameters.AddWithValue("@owner_id", ownerId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var game = new Game { Id = (int)reader["game_id"] };
                            var renter = new User((int)reader["renter_id"]);
                            var owner = new User((int)reader["owner_id"]);
                            var req = new Request((int)reader["request_id"], game, renter, owner, (DateTime)reader["start_date"], (DateTime)reader["end_date"]);
                            list.Add(req);
                        }
                    }
                }
            }
            return list.ToImmutableList();
        }

        public ImmutableList<Request> GetRequestsByRenter(int renterId)
        {
            var list = new List<Request>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM Requests WHERE renter_id = @renter_id";
                    command.Parameters.AddWithValue("@renter_id", renterId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var game = new Game { Id = (int)reader["game_id"] };
                            var renter = new User((int)reader["renter_id"]);
                            var owner = new User((int)reader["owner_id"]);
                            var req = new Request((int)reader["request_id"], game, renter, owner, (DateTime)reader["start_date"], (DateTime)reader["end_date"]);
                            list.Add(req);
                        }
                    }
                }
            }
            return list.ToImmutableList();
        }

        public ImmutableList<Request> GetRequestsByGame(int gameId)
        {
            var list = new List<Request>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM Requests WHERE game_id = @game_id";
                    command.Parameters.AddWithValue("@game_id", gameId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var game = new Game { Id = (int)reader["game_id"] };
                            var renter = new User((int)reader["renter_id"]);
                            var owner = new User((int)reader["owner_id"]);
                            var req = new Request((int)reader["request_id"], game, renter, owner, (DateTime)reader["start_date"], (DateTime)reader["end_date"]);
                            list.Add(req);
                        }
                    }
                }
            }
            return list.ToImmutableList();
        }
    }
}
