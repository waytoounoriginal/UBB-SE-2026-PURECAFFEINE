using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Configuration;
using System.Linq;
using Microsoft.Data.SqlClient;
using Property_and_Management.src.Interface;
using Property_and_Management.src.Model;

namespace Property_and_Management.src.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString =
            System.Configuration.ConfigurationManager.ConnectionStrings["BoardRent"]?.ConnectionString ?? string.Empty;

        public ImmutableList<User> GetAll()
        {
            var list = new List<User>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM Users";
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new User((int)reader["id"]));
                        }
                    }
                }
            }
            return list.ToImmutableList();
        }

        public void Add(User newEntity)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO Users DEFAULT VALUES; SELECT SCOPE_IDENTITY();";
                    var newId = Convert.ToInt32(command.ExecuteScalar());
                    newEntity.Id = newId;
                }
            }
        }

        public User Delete(int removedEntityId)
        {
            var entity = Get(removedEntityId);
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM Users WHERE id = @id";
                    command.Parameters.AddWithValue("@id", removedEntityId);
                    command.ExecuteNonQuery();
                }
            }
            return entity;
        }

        public void Update(int updatedEntityId, User newEntity)
        {
            // only Id column exists in Users table in DB script; nothing to update
            // keep method for interface compatibility
            if (updatedEntityId != newEntity.Id)
                throw new ArgumentException("Id mismatch");
        }

        public User Get(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM Users WHERE id = @id";
                    command.Parameters.AddWithValue("@id", id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User((int)reader["id"]);
                        }
                    }
                }
            }

            throw new KeyNotFoundException();
        }
    }
}
