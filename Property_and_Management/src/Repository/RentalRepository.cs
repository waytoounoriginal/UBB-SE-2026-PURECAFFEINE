using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Data.SqlClient;
using Property_and_Management.src.Interface;
using Property_and_Management.src.Model;

namespace Property_and_Management.src.Repository
{
    public class RentalRepository : IRentalRepository
    {
        private readonly string _connectionString =
            System.Configuration.ConfigurationManager.ConnectionStrings["BoardRent"]?.ConnectionString ?? string.Empty

        private Rental MapRowToRental(SqlDataReader reader)
        {
            return new Rental(
                (int)reader["rental_id"],
                new Game { Id = (int)reader["game_id"] },
                new User((int)reader["renter_id"]),
                new User((int)reader["owner_id"]),
                (DateTime)reader["start_date"],
                (DateTime)reader["end_date"]
            );
        }
        public ImmutableList<Rental> GetAll()
        {
            var list = new List<Rental>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM Rentals";
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read()) list.Add(MapRowToRental(reader));
                    }
                }
            }
            return list.ToImmutableList();
        }


        public void Add(Rental entity)
        {
            // [ENT-REN-02] The system shall enforce that end_date is strictly after start_date for every Rental.
            if (entity.EndDate <= entity.StartDate)
                throw new ArgumentException("End date must be strictly after start date [ENT-REN-02].");

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO Rentals(game_id, renter_id, owner_id, start_date, end_date) VALUES(@game_id, @renter_id, @owner_id, @start_date, @end_date); SELECT SCOPE_IDENTITY();";
                    command.Parameters.AddWithValue("@game_id", entity.Game?.Id ?? 0);
                    command.Parameters.AddWithValue("@renter_id", entity.Renter?.Id ?? 0);
                    command.Parameters.AddWithValue("@owner_id", entity.Owner?.Id ?? 0);
                    command.Parameters.AddWithValue("@start_date", entity.StartDate);
                    command.Parameters.AddWithValue("@end_date", entity.EndDate);
                    var newId = Convert.ToInt32(command.ExecuteScalar());
                    entity.Id = newId;
                }
            }
        }

        public ImmutableList<Rental> GetRentalsByOwner(int ownerId)
        {
            var list = new List<Rental>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM Rentals WHERE owner_id = @owner_id";
                    command.Parameters.AddWithValue("@owner_id", ownerId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var game = new Game { Id = (int)reader["game_id"] };
                            var renter = new User((int)reader["renter_id"]);
                            var owner = new User((int)reader["owner_id"]);
                            var rent = new Rental((int)reader["rental_id"], game, renter, owner, (DateTime)reader["start_date"], (DateTime)reader["end_date"]);
                            list.Add(rent);
                        }
                    }
                }
            }
            return list.ToImmutableList();
        }

        public ImmutableList<Rental> GetRentalsByRenter(int renterId)
        {
            var list = new List<Rental>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM Rentals WHERE renter_id = @renter_id";
                    command.Parameters.AddWithValue("@renter_id", renterId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var game = new Game { Id = (int)reader["game_id"] };
                            var renter = new User((int)reader["renter_id"]);
                            var owner = new User((int)reader["owner_id"]);
                            var rent = new Rental((int)reader["rental_id"], game, renter, owner, (DateTime)reader["start_date"], (DateTime)reader["end_date"]);
                            list.Add(rent);
                        }
                    }
                }
            }
            return list.ToImmutableList();
        }

        public ImmutableList<Rental> GetRentalsByGame(int gameId)
        {
            var list = new List<Rental>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM Rentals WHERE game_id = @game_id";
                    command.Parameters.AddWithValue("@game_id", gameId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var game = new Game { Id = (int)reader["game_id"] };
                            var renter = new User((int)reader["renter_id"]);
                            var owner = new User((int)reader["owner_id"]);
                            var rent = new Rental((int)reader["rental_id"], game, renter, owner, (DateTime)reader["start_date"], (DateTime)reader["end_date"]);
                            list.Add(rent);
                        }
                    }
                }
            }
            return list.ToImmutableList();
        }

        // [ENT-REN-05] Rental records shall never be deleted.
        public Rental Delete(int removedEntityId)
        {
            throw new NotSupportedException("Rental records constitute a historical log and cannot be deleted [ENT-REN-05].");
        }

        public void Update(int updatedEntityId, Rental newEntity)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "UPDATE Rentals SET game_id = @game_id, renter_id = @renter_id, owner_id = @owner_id, start_date = @start_date, end_date = @end_date WHERE rental_id = @id";
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

        public Rental Get(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM Rentals WHERE rental_id = @id";
                    command.Parameters.AddWithValue("@id", id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read()) return MapRowToRental(reader);
                        throw new KeyNotFoundException();
                    }
                }
            }
            throw new KeyNotFoundException();
        }
    }
}
