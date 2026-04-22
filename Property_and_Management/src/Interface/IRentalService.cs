using System;
using System.Collections.Immutable;
using Property_and_Management.Src.DataTransferObjects;

namespace Property_and_Management.Src.Interface
{
    public interface IRentalService
    {
        ImmutableList<RentalDTO> GetRentalsForRenter(int renterUserId);

        ImmutableList<RentalDTO> GetRentalsForOwner(int ownerUserId);

        bool IsSlotAvailable(int gameId, DateTime requestedStartDate, DateTime requestedEndDate);

        void CreateConfirmedRental(int gameId, int renterUserId, int ownerUserId, DateTime startDate, DateTime endDate);
    }
}