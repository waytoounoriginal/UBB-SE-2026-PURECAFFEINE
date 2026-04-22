using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Data.SqlClient;
using Property_and_Management.Src.Interface;
using Property_and_Management.Src.Model;

namespace Property_and_Management.Src.Repository
{
    public class NotificationRepository : INotificationRepository
    {
        private const int MissingUserId = 0;

        private readonly string boardRentConnectionString =
            System.Configuration.ConfigurationManager.ConnectionStrings["BoardRent"]?.ConnectionString ?? string.Empty;

        private const string BaseNotificationSelectQuery =
            "SELECT n.*, u.display_name AS user_display_name FROM Notifications n LEFT JOIN Users u ON u.id = n.user_id";

        private static Notification ReadNotificationFromReader(SqlDataReader databaseReader)
        {
            var notificationOwner = new User((int)databaseReader["user_id"], databaseReader["user_display_name"] as string ?? string.Empty);
            var notificationType = (NotificationType)(int)databaseReader["type"];
            var relatedRequestIdValue = databaseReader["related_request_id"];
            return new Notification(
                (int)databaseReader["notification_id"], notificationOwner,
                (DateTime)databaseReader["timestamp"], (string)databaseReader["title"], (string)databaseReader["body"],
                notificationType, relatedRequestIdValue == DBNull.Value ? null : (int)relatedRequestIdValue);
        }

        public ImmutableList<Notification> GetAll()
        {
            var allRetrievedNotifications = new List<Notification>();
            using (var connection = new SqlConnection(boardRentConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = BaseNotificationSelectQuery;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            allRetrievedNotifications.Add(ReadNotificationFromReader(reader));
                        }
                    }
                }
            }
            return allRetrievedNotifications.ToImmutableList();
        }

        public void Add(Notification notificationToInsert)
        {
            using (var connection = new SqlConnection(boardRentConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO Notifications(user_id, timestamp, title, body, type, related_request_id) VALUES(@user_id, @timestamp, @title, @body, @type, @related_request_id); SELECT SCOPE_IDENTITY();";
                    command.Parameters.AddWithValue("@user_id", notificationToInsert.User?.Id ?? MissingUserId);
                    command.Parameters.AddWithValue("@timestamp", notificationToInsert.Timestamp);
                    command.Parameters.AddWithValue("@title", notificationToInsert.Title);
                    command.Parameters.AddWithValue("@body", notificationToInsert.Body);
                    command.Parameters.AddWithValue("@type", (int)notificationToInsert.Type);
                    command.Parameters.AddWithValue("@related_request_id", notificationToInsert.RelatedRequestId ?? (object)DBNull.Value);
                    var newNotificationIdentifier = Convert.ToInt32(command.ExecuteScalar());
                    notificationToInsert.Id = newNotificationIdentifier;
                }
            }
        }

        public Notification Delete(int notificationIdToRemove)
        {
            using (var connection = new SqlConnection(boardRentConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "DELETE n OUTPUT deleted.notification_id, deleted.user_id, deleted.timestamp, " +
                        "deleted.title, deleted.body, deleted.type, deleted.related_request_id, " +
                        "u.display_name AS user_display_name " +
                        "FROM Notifications n LEFT JOIN Users u ON u.id = n.user_id " +
                        "WHERE n.notification_id = @id";
                    command.Parameters.AddWithValue("@id", notificationIdToRemove);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return ReadNotificationFromReader(reader);
                        }
                    }
                }
            }
            throw new KeyNotFoundException();
        }

        public void Update(int notificationIdToUpdate, Notification notificationDataToUpdate)
        {
            using (var connection = new SqlConnection(boardRentConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "UPDATE Notifications SET user_id = @user_id, timestamp = @timestamp, title = @title, body = @body, type = @type, related_request_id = @related_request_id WHERE notification_id = @id";
                    command.Parameters.AddWithValue("@id", notificationIdToUpdate);
                    command.Parameters.AddWithValue("@user_id", notificationDataToUpdate.User?.Id ?? MissingUserId);
                    command.Parameters.AddWithValue("@timestamp", notificationDataToUpdate.Timestamp);
                    command.Parameters.AddWithValue("@title", notificationDataToUpdate.Title);
                    command.Parameters.AddWithValue("@body", notificationDataToUpdate.Body);
                    command.Parameters.AddWithValue("@type", (int)notificationDataToUpdate.Type);
                    command.Parameters.AddWithValue("@related_request_id", notificationDataToUpdate.RelatedRequestId ?? (object)DBNull.Value);
                    command.ExecuteNonQuery();
                }
            }
        }

        public Notification Get(int notificationIdToFind)
        {
            using (var connection = new SqlConnection(boardRentConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = BaseNotificationSelectQuery + " WHERE n.notification_id = @id";
                    command.Parameters.AddWithValue("@id", notificationIdToFind);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return ReadNotificationFromReader(reader);
                        }
                    }
                }
            }
            throw new KeyNotFoundException();
        }

        public ImmutableList<Notification> GetNotificationsByUser(int targetUserId)
        {
            var userNotifications = new List<Notification>();
            using (var connection = new SqlConnection(boardRentConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = BaseNotificationSelectQuery + " WHERE n.user_id = @user_id";
                    command.Parameters.AddWithValue("@user_id", targetUserId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            userNotifications.Add(ReadNotificationFromReader(reader));
                        }
                    }
                }
            }
            return userNotifications.ToImmutableList();
        }

        public void DeleteNotificationsLinkedToRequest(int linkedRequestId)
        {
            using (var connection = new SqlConnection(boardRentConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM Notifications WHERE related_request_id = @request_id";
                    command.Parameters.AddWithValue("@request_id", linkedRequestId);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
