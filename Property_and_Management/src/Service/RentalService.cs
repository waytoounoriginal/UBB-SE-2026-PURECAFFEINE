using System;
using System.Collections.Immutable;
using System.Linq;
using Property_and_Management.Src.Constants;
using Property_and_Management.Src.DataTransferObjects;
using Property_and_Management.Src.Interface;
using Property_and_Management.Src.Model;

namespace Property_and_Management.Src.Service
{
    public class RentalService : IRentalService
    {
        private readonly IRentalRepository rentalDataRepository;
        private readonly IGameRepository gameLookupRepository;
        private readonly IMapper<Rental, RentalDTO> rentalDtoMapper;

        private const int NewRentalId = 0;

        public RentalService(
            IRentalRepository rentalRepository,
            IGameRepository gameRepository,
            IMapper<Rental, RentalDTO> rentalMapper)
        {
            this.rentalDataRepository = rentalRepository;
            this.gameLookupRepository = gameRepository;
            this.rentalDtoMapper = rentalMapper;
        }

        public bool IsSlotAvailable(int gameId, DateTime proposedStartDate, DateTime proposedEndDate)
        {
            foreach (var existingRental in rentalDataRepository.GetRentalsByGame(gameId))
            {
                var bufferStart = existingRental.StartDate.AddHours(-DomainConstants.RentalBufferHours);
                var bufferEnd = existingRental.EndDate.AddHours(DomainConstants.RentalBufferHours);
                if (proposedStartDate < bufferEnd && proposedEndDate > bufferStart)
                {
                    return false;
                }
            }

            return true;
        }

        public void CreateConfirmedRental(int gameId, int renterUserId, int ownerUserId, DateTime rentalStartDate, DateTime rentalEndDate)
        {
            var gameToRent = gameLookupRepository.Get(gameId);
            if (gameToRent.Owner.Id != ownerUserId)
            {
                throw new InvalidOperationException("Seller ID must match Game Owner ID [ENT-REN-04].");
            }

            if (!IsSlotAvailable(gameId, rentalStartDate, rentalEndDate))
            {
                throw new InvalidOperationException(
                    $"Selected dates fall within the mandatory {DomainConstants.RentalBufferHours}-hour buffer of another rental.");
            }

            var confirmedRental = new Rental(
                id: NewRentalId,
                rentedGame: new Game { Id = gameId },
                renterUser: new User { Id = renterUserId },
                ownerUser: new User { Id = ownerUserId },
                startDate: rentalStartDate,
                endDate: rentalEndDate);

            rentalDataRepository.AddConfirmed(confirmedRental);
        }

        public ImmutableList<RentalDTO> GetRentalsForRenter(int renterUserId) =>
            rentalDataRepository
                .GetRentalsByRenter(renterUserId)
                .Select(rental => rentalDtoMapper.ToDTO(rental))
                .ToImmutableList();

        public ImmutableList<RentalDTO> GetRentalsForOwner(int ownerUserId) =>
            rentalDataRepository
                .GetRentalsByOwner(ownerUserId)
                .Select(rental => rentalDtoMapper.ToDTO(rental))
                .ToImmutableList();
    }
}