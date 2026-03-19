using Microsoft.Data.SqlClient;
using Property_and_Management.src.Interface;
using Property_and_Management.src.Model;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Property_and_Management.src.Repository
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly string _connectionString =
            System.Configuration.ConfigurationManager.ConnectionStrings["BoardRent"]?.ConnectionString ?? string.Empty;

        public ImmutableList<Notification> GetAll()
        {
            var list = new List<Notification>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT notification_id, user_id, timestamp, title, body FROM Notifications";
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var user = new User((int)reader["user_id"]);
                            var note = new Notification((int)reader["notification_id"], user, (DateTime)reader["timestamp"], (string)reader["title"], (string)reader["body"]);
                            list.Add(note);
                        }
                    }
                }
            }
            return list.ToImmutableList();
        }

        public void Add(Notification newEntity)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO Notifications(user_id, timestamp, title, body) VALUES(@user_id, @timestamp, @title, @body); SELECT SCOPE_IDENTITY();";
                    command.Parameters.AddWithValue("@user_id", newEntity.User?.Id ?? 0);
                    command.Parameters.AddWithValue("@timestamp", newEntity.Timestamp);
                    command.Parameters.AddWithValue("@title", newEntity.Title);
                    command.Parameters.AddWithValue("@body", newEntity.Body);
                    var newId = Convert.ToInt32(command.ExecuteScalar());
                    newEntity.Id = newId;
                }
            }
        }

        public Notification Delete(int removedEntityId)
        {
            var entity = Get(removedEntityId);
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM Notifications WHERE notification_id = @id";
                    command.Parameters.AddWithValue("@id", removedEntityId);
                    command.ExecuteNonQuery();
                }
            }
            return entity;
        }

        public void Update(int updatedEntityId, Notification newEntity)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "UPDATE Notifications SET user_id = @user_id, timestamp = @timestamp, title = @title, body = @body WHERE notification_id = @id";
                    command.Parameters.AddWithValue("@id", updatedEntityId);
                    command.Parameters.AddWithValue("@user_id", newEntity.User?.Id ?? 0);
                    command.Parameters.AddWithValue("@timestamp", newEntity.Timestamp);
                    command.Parameters.AddWithValue("@title", newEntity.Title);
                    command.Parameters.AddWithValue("@body", newEntity.Body);
                    command.ExecuteNonQuery();
                }
            }
        }

        public Notification Get(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT notification_id, user_id, timestamp, title, body FROM Notifications WHERE notification_id = @id";
                    command.Parameters.AddWithValue("@id", id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var user = new User((int)reader["user_id"]);
                            return new Notification((int)reader["notification_id"], user, (DateTime)reader["timestamp"], (string)reader["title"], (string)reader["body"]);
                        }
                    }
                }
            }
            throw new KeyNotFoundException();
        }

        public ImmutableList<Notification> GetNotificationsByUser(int userId)
        {
            var list = new List<Notification>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT notification_id, user_id, timestamp, title, body FROM Notifications WHERE user_id = @user_id";
                    command.Parameters.AddWithValue("@user_id", userId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var user = new User((int)reader["user_id"]);
                            var note = new Notification((int)reader["notification_id"], user, (DateTime)reader["timestamp"], (string)reader["title"], (string)reader["body"]);
                            list.Add(note);
                        }
                    }
                }
            }
            return list.ToImmutableList();
        }
    }
}
