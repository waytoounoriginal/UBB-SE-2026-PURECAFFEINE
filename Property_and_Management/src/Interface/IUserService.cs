using System.Collections.Immutable;
using Property_and_Management.Src.DataTransferObjects;

namespace Property_and_Management.Src.Interface
{
    public interface IUserService
    {
        ImmutableList<UserDTO> GetUsersExcept(int excludeUserId);
    }
}