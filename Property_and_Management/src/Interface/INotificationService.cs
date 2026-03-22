using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Property_and_Management.src.DTO;

namespace Property_and_Management.src.Interface
{
    interface INotificationService
    {
        /// <summary>Get a notification by its identifier.</summary>
        /// <param name="id">The identifier of the notification.</param>
        /// <returns>The <see cref="NotificationDTO"/> with the specified identifier.</returns>
        NotificationDTO GetNotificationById(int id);

        /// <summary>Delete a notification by its identifier and return the deleted item.</summary>
        /// <param name="id">The identifier of the notification.</param>
        /// <returns>The deleted <see cref="NotificationDTO"/>.</returns>
        NotificationDTO DeleteNotificationById(int id);

        /// <summary>Update an existing notification identified by id.</summary>
        /// <param name="id">The identifier of the notification to update.</param>
        /// <param name="notification">The updated notification data.</param>
        void UpdateNotificationById(int id, NotificationDTO notification);

        /// <summary>Send a notification to a specific user.</summary>
        /// <param name="userId">The identifier of the user.</param>
        /// <param name="notification">The notification to send.</param>
        void SendNotificationToUser(int userId, NotificationDTO notification);

        /// <summary>Return all notifications for a given user.</summary>
        /// <param name="userId">The identifier of the user.</param>
        /// <returns>A list of <see cref="NotificationDTO"/> objects for the specified user.</returns>
        ImmutableList<NotificationDTO> GetNotificationsForUser(int userId);

        /// <summary>
        /// Subscribes to recive notifications for the given userId
        /// </summary>
        /// <param name="userId"></param>
        void SubscribeToServer(int userId);

        /// <summary>
        /// Starts the listening on the client
        /// </summary>
        void StartListening();

        /// <summary>
        /// Stops the listening on the client
        /// </summary>
        void StopListening();
    }
}
