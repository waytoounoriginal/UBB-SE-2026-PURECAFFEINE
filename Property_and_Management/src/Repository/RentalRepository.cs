using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using Microsoft.Data.SqlClient;
using Property_and_Management.Src.Interface;
using Property_and_Management.Src.Model;

namespace Property_and_Management.Src.Repository
{
    public class RentalRepository : IRentalRepository
    {
        private const int MissingForeignKeyId = 0;
        private const string ConnectionStringName = "BoardRent";

        private readonly string boardRentConnectionString =
            System.Configuration.ConfigurationManager.ConnectionStrings[ConnectionStringName]?.ConnectionString ?? string.Empty;

        private const string SelectAllRentalsSql =
            "SELECT r.*, renterUser.display_name AS renter_display_name, ownerUser.display_name AS owner_display_name, " +
            "g.name AS game_name, g.image AS game_image " +
            "FROM Rentals r " +
            "LEFT JOIN Users renterUser ON renterUser.Id = r.renter_id " +
            "LEFT JOIN Users ownerUser ON ownerUser.Id = r.owner_id " +
            "LEFT JOIN Games g ON g.game_id = r.game_id";

        private static Rental ReadRentalFromReader(SqlDataReader databaseReader)
        {
            var rentalGame = new Game
            {
                Id = (int)databaseReader["game_id"],
                Name = databaseReader["game_name"] as string ?? string.Empty,
                Image = databaseReader["game_image"] as byte[] ?? Array.Empty<byte>()
            };
            var renterUser = new User((int)databaseReader["renter_id"], databaseReader["renter_display_name"] as string ?? string.Empty);
            var ownerUser = new User((int)databaseReader["owner_id"], databaseReader["owner_display_name"] as string ?? string.Empty);
            return new Rental((int)databaseReader["rental_id"], rentalGame, renterUser, ownerUser,
                (DateTime)databaseReader["start_date"], (DateTime)databaseReader["end_date"]);
        }

        public ImmutableList<Rental> GetAll()
        {
            var allRetrievedRentals = new List<Rental>();
            using (var connection = new SqlConnection(boardRentConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = SelectAllRentalsSql;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            allRetrievedRentals.Add(ReadRentalFromReader(reader));
                        }
                    }
                }
            }
            return allRetrievedRentals.ToImmutableList();
        }

        public void Add(Rental rentalToInsert)
        {
            using (var connection = new SqlConnection(boardRentConnectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    AddRentalWithinTransaction(rentalToInsert, connection, transaction);
                    transaction.Commit();
                }
            }
        }

        private static void AddRentalWithinTransaction(Rental rentalToInsert, SqlConnection connection, SqlTransaction transaction)
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText =
                "INSERT INTO Rentals(game_id, renter_id, owner_id, start_date, end_date) " +
                "VALUES(@game_id, @renter_id, @owner_id, @start_date, @end_date); SELECT SCOPE_IDENTITY();";
            command.Parameters.AddWithValue("@game_id", rentalToInsert.Game?.Id ?? MissingForeignKeyId);
            command.Parameters.AddWithValue("@renter_id", rentalToInsert.Renter?.Id ?? MissingForeignKeyId);
            command.Parameters.AddWithValue("@owner_id", rentalToInsert.Owner?.Id ?? MissingForeignKeyId);
            command.Parameters.AddWithValue("@start_date", rentalToInsert.StartDate);
            command.Parameters.AddWithValue("@end_date", rentalToInsert.EndDate);
            rentalToInsert.Id = Convert.ToInt32(command.ExecuteScalar());
        }

        public void AddConfirmed(Rental confirmedRentalToInsert)
        {
            using (var connection = new SqlConnection(boardRentConnectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    AddRentalWithinTransaction(confirmedRentalToInsert, connection, transaction);
                    transaction.Commit();
                }
            }
        }

        public ImmutableList<Rental> GetRentalsByOwner(int ownerUserId)
        {
            var ownerRentals = new List<Rental>();
            using (var connection = new SqlConnection(boardRentConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = SelectAllRentalsSql + " WHERE r.owner_id = @owner_id";
                    command.Parameters.AddWithValue("@owner_id", ownerUserId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ownerRentals.Add(ReadRentalFromReader(reader));
                        }
                    }
                }
            }
            return ownerRentals.ToImmutableList();
        }

        public ImmutableList<Rental> GetRentalsByRenter(int renterUserId)
        {
            var renterRentals = new List<Rental>();
            using (var connection = new SqlConnection(boardRentConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = SelectAllRentalsSql + " WHERE r.renter_id = @renter_id";
                    command.Parameters.AddWithValue("@renter_id", renterUserId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            renterRentals.Add(ReadRentalFromReader(reader));
                        }
                    }
                }
            }
            return renterRentals.ToImmutableList();
        }

        public ImmutableList<Rental> GetRentalsByGame(int rentalGameId)
        {
            var gameRentals = new List<Rental>();
            using (var connection = new SqlConnection(boardRentConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = SelectAllRentalsSql + " WHERE r.game_id = @game_id";
                    command.Parameters.AddWithValue("@game_id", rentalGameId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            gameRentals.Add(ReadRentalFromReader(reader));
                        }
                    }
                }
            }
            return gameRentals.ToImmutableList();
        }

        public Rental Delete(int rentalIdToRemove)
        {
            using (var connection = new SqlConnection(boardRentConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "DELETE r OUTPUT deleted.rental_id, deleted.game_id, deleted.renter_id, deleted.owner_id, " +
                        "deleted.start_date, deleted.end_date, " +
                        "renterUser.display_name AS renter_display_name, ownerUser.display_name AS owner_display_name, " +
                        "g.name AS game_name, g.image AS game_image " +
                        "FROM Rentals r " +
                        "LEFT JOIN Users renterUser ON renterUser.Id = r.renter_id " +
                        "LEFT JOIN Users ownerUser ON ownerUser.Id = r.owner_id " +
                        "LEFT JOIN Games g ON g.game_id = r.game_id " +
                        "WHERE r.rental_id = @id";
                    command.Parameters.AddWithValue("@id", rentalIdToRemove);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return ReadRentalFromReader(reader);
                        }
                    }
                }
            }
            throw new KeyNotFoundException();
        }

        public void Update(int rentalIdToUpdate, Rental rentalDataToUpdate)
        {
            using (var connection = new SqlConnection(boardRentConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "UPDATE Rentals SET game_id = @game_id, renter_id = @renter_id, owner_id = @owner_id, " +
                        "start_date = @start_date, end_date = @end_date WHERE rental_id = @id";
                    command.Parameters.AddWithValue("@id", rentalIdToUpdate);
                    command.Parameters.AddWithValue("@game_id", rentalDataToUpdate.Game?.Id ?? MissingForeignKeyId);
                    command.Parameters.AddWithValue("@renter_id", rentalDataToUpdate.Renter?.Id ?? MissingForeignKeyId);
                    command.Parameters.AddWithValue("@owner_id", rentalDataToUpdate.Owner?.Id ?? MissingForeignKeyId);
                    command.Parameters.AddWithValue("@start_date", rentalDataToUpdate.StartDate);
                    command.Parameters.AddWithValue("@end_date", rentalDataToUpdate.EndDate);
                    command.ExecuteNonQuery();
                }
            }
        }

        public Rental Get(int rentalIdToFind)
        {
            using (var connection = new SqlConnection(boardRentConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = SelectAllRentalsSql + " WHERE r.rental_id = @id";
                    command.Parameters.AddWithValue("@id", rentalIdToFind);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return ReadRentalFromReader(reader);
                        }
                    }
                }
            }
            throw new KeyNotFoundException();
        }
    }
}