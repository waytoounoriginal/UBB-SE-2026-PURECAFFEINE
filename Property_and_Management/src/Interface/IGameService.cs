using System.Collections.Immutable;
using Property_and_Management.Src.DataTransferObjects;

namespace Property_and_Management.Src.Interface
{
    public interface IGameService
    {
        void AddGame(GameDTO gameDto);

        public void UpdateGameByIdentifier(int gameId, GameDTO updatedGameDTO);

        public GameDTO DeleteGameByIdentifier(int gameId);

        public GameDTO GetGameByIdentifier(int gameId);

        public ImmutableList<GameDTO> GetGamesForOwner(int ownerUserId);

        ImmutableList<GameDTO> GetAllGames();
    }
}