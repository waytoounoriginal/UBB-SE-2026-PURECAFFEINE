using System.Collections.Generic;

namespace Property_and_Management.Src.Interface
{
    public interface IDismissedNotificationStore
    {
        HashSet<int> Load(int currentUserId);

        void Save(int currentUserId, IEnumerable<int> dismissedNotificationIdentifiers);
    }
}