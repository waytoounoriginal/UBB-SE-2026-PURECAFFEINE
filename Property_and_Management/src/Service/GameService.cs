using System;
using System.Collections.Immutable;
using System.Linq;
using Property_and_Management.Src.DataTransferObjects;
using Property_and_Management.Src.Interface;
using Property_and_Management.Src.Model;

namespace Property_and_Management.Src.Service
{
    public class GameService : IGameService
    {
        private readonly IGameRepository gameListingRepository;
        private readonly IRentalRepository gameRentalRepository;
        private readonly IMapper<Game, GameDTO> gameDtoMapper;
        private readonly IRequestService rentalRequestService;
        private const int NoActiveOrUpcomingRentals = 0;
        private const int SingularRentalCount = 1;

        public GameService(
            IGameRepository gameRepository,
            IRentalRepository rentalRepository,
            IMapper<Game, GameDTO> gameMapper,
            IRequestService requestService)
        {
            this.gameListingRepository = gameRepository;
            this.gameRentalRepository = rentalRepository;
            this.gameDtoMapper = gameMapper;
            this.rentalRequestService = requestService;
        }

        public void AddGame(GameDTO gameToAdd)
        {
            gameListingRepository.Add(gameDtoMapper.ToModel(gameToAdd));
        }

        public void UpdateGameByIdentifier(int gameId, GameDTO updatedGameData)
        {
            gameListingRepository.Update(gameId, gameDtoMapper.ToModel(updatedGameData));
        }

        public GameDTO DeleteGameByIdentifier(int gameId)
        {
            var gameRentals = gameRentalRepository.GetRentalsByGame(gameId);
            var currentTime = DateTime.Now;
            var activeOrUpcomingRentalsCount = gameRentals.Count(rental => rental.EndDate >= currentTime);
            if (activeOrUpcomingRentalsCount > NoActiveOrUpcomingRentals)
            {
                var rentalWord = activeOrUpcomingRentalsCount == SingularRentalCount ? "rental" : "rentals";
                throw new InvalidOperationException(
                    $"There are {activeOrUpcomingRentalsCount} active {rentalWord} for this game and it cannot be removed now.");
            }

            foreach (var pastRental in gameRentals)
            {
                gameRentalRepository.Delete(pastRental.Id);
            }

            rentalRequestService.OnGameDeactivated(gameId);
            return gameDtoMapper.ToDTO(gameListingRepository.Delete(gameId));
        }

        public GameDTO GetGameByIdentifier(int gameId)
        {
            return gameDtoMapper.ToDTO(gameListingRepository.Get(gameId));
        }

        public ImmutableList<GameDTO> GetGamesForOwner(int ownerUserId)
        {
            return gameListingRepository
                .GetGamesByOwner(ownerUserId)
                .Select(game => gameDtoMapper.ToDTO(game))
                .ToImmutableList();
        }

        public ImmutableList<GameDTO> GetAllGames()
        {
            return gameListingRepository
                .GetAll()
                .Select(game => gameDtoMapper.ToDTO(game))
                .ToImmutableList();
        }
    }
}