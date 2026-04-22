using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Data.SqlClient;
using Property_and_Management.Src.Interface;
using Property_and_Management.Src.Model;

namespace Property_and_Management.Src.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly string boardRentConnectionString =
            System.Configuration.ConfigurationManager.ConnectionStrings["BoardRent"]?.ConnectionString ?? string.Empty;

        public ImmutableList<User> GetAll()
        {
            var allRetrievedUsers = new List<User>();
            using (var connection = new SqlConnection(boardRentConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM Users";
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var userId = (int)reader["id"];
                            var userDisplayName = reader["display_name"] as string ?? string.Empty;
                            allRetrievedUsers.Add(new User(userId, userDisplayName));
                        }
                    }
                }
            }
            return allRetrievedUsers.ToImmutableList();
        }

        public void Add(User userToInsert)
        {
            using (var connection = new SqlConnection(boardRentConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO Users (display_name) VALUES (@display_name); SELECT SCOPE_IDENTITY();";
                    command.Parameters.AddWithValue("@display_name", userToInsert.DisplayName ?? (object)DBNull.Value);
                    var newUserIdentifier = Convert.ToInt32(command.ExecuteScalar());
                    userToInsert.Id = newUserIdentifier;
                }
            }
        }

        public User Delete(int userIdToRemove)
        {
            using (var connection = new SqlConnection(boardRentConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM Users OUTPUT deleted.id, deleted.display_name WHERE id = @id";
                    command.Parameters.AddWithValue("@id", userIdToRemove);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var deletedUserId = (int)reader["id"];
                            var deletedUserDisplayName = reader["display_name"] as string ?? string.Empty;
                            return new User(deletedUserId, deletedUserDisplayName);
                        }
                    }
                }
            }
            throw new KeyNotFoundException();
        }

        public void Update(int userIdToUpdate, User userDataToUpdate)
        {
            if (userIdToUpdate != userDataToUpdate.Id)
            {
                throw new ArgumentException("Id mismatch");
            }

            using (var connection = new SqlConnection(boardRentConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "UPDATE Users SET display_name = @display_name WHERE id = @id";
                    command.Parameters.AddWithValue("@display_name", userDataToUpdate.DisplayName ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@id", userIdToUpdate);
                    command.ExecuteNonQuery();
                }
            }
        }

        public User Get(int userIdToFind)
        {
            using (var connection = new SqlConnection(boardRentConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM Users WHERE id = @id";
                    command.Parameters.AddWithValue("@id", userIdToFind);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var foundUserId = (int)reader["id"];
                            var foundUserDisplayName = reader["display_name"] as string ?? string.Empty;
                            return new User(foundUserId, foundUserDisplayName);
                        }
                    }
                }
            }

            throw new KeyNotFoundException();
        }
    }
}
