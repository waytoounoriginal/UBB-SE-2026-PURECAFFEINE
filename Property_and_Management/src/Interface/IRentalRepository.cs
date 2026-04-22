using System.Collections.Immutable;
using Property_and_Management.Src.Model;

namespace Property_and_Management.Src.Interface
{
    public interface IRentalRepository : IRepository<Rental>
    {
        void AddConfirmed(Rental confirmedRental);

        ImmutableList<Rental> GetRentalsByOwner(int ownerUserId);

        ImmutableList<Rental> GetRentalsByRenter(int renterUserId);

        ImmutableList<Rental> GetRentalsByGame(int gameId);
    }
}