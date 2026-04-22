using System.Collections.Immutable;
using Property_and_Management.Src.Model;

namespace Property_and_Management.Src.Interface
{
    public interface IGameRepository : IRepository<Game>
    {
        ImmutableList<Game> GetGamesByOwner(int ownerUserId);
    }
}