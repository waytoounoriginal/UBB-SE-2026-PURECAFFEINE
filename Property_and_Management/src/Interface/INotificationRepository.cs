using System.Collections.Immutable;
using Property_and_Management.Src.Model;

namespace Property_and_Management.Src.Interface
{
    public interface INotificationRepository : IRepository<Notification>
    {
        ImmutableList<Notification> GetNotificationsByUser(int userId);

        void DeleteNotificationsLinkedToRequest(int relatedRequestId);
    }
}