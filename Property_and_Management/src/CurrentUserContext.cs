using Property_and_Management.Src.Interface;

namespace Property_and_Management.Src
{
    public sealed class CurrentUserContext : ICurrentUserContext
    {
        public int CurrentUserId { get; }

        public CurrentUserContext(int currentUserId)
        {
            this.CurrentUserId = currentUserId;
        }
    }
}
