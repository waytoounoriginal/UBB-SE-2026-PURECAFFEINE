using System.Collections.Immutable;
using Property_and_Management.src.Model;

namespace Property_and_Management.src.Interface
{
    public interface IRentalRepository : IRepository<Rental>
    {
        /// <summary>
        /// Gets rentals for which the specified user is the owner.
        /// </summary>
        /// <param name="ownerId">Owner user id.</param>
        /// <returns>Immutable list of matching rentals.</returns>
        ImmutableList<Rental> GetRentalsByOwner(int ownerId);

        /// <summary>
        /// Gets rentals created by the specified renter.
        /// </summary>
        /// <param name="renterId">Renter user id.</param>
        /// <returns>Immutable list of matching rentals.</returns>
        ImmutableList<Rental> GetRentalsByRenter(int renterId);

        /// <summary>
        /// Gets rentals for the specified game.
        /// </summary>
        /// <param name="gameId">Game id.</param>
        /// <returns>Immutable list of matching rentals.</returns>
        ImmutableList<Rental> GetRentalsByGame(int gameId);
    }
}
