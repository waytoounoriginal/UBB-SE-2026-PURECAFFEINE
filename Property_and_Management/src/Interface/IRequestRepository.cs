using System.Collections.Immutable;
using Microsoft.Data.SqlClient;
using Property_and_Management.src.Model;

namespace Property_and_Management.src.Interface
{
    public interface IRequestRepository : IRepository<Request>
    {
        /// <summary>
        /// Gets requests for which the specified user is the game owner.
        /// </summary>
        /// <param name="ownerId">Owner user id.</param>
        /// <returns>Immutable list of matching requests.</returns>
        ImmutableList<Request> GetRequestsByOwner(int ownerId);

        /// <summary>
        /// Gets requests created by the specified renter.
        /// </summary>
        /// <param name="renterId">Renter user id.</param>
        /// <returns>Immutable list of matching requests.</returns>
        ImmutableList<Request> GetRequestsByRenter(int renterId);

        /// <summary>
        /// Gets requests for the specified game.
        /// </summary>
        /// <param name="gameId">Game id.</param>
        /// <returns>Immutable list of matching requests.</returns>
        ImmutableList<Request> GetRequestsByGame(int gameId);

        // Transaction-capable overloads (requests only)

        /// <summary>
        /// Inserts the request using an existing connection and transaction.
        /// </summary>
        /// <param name="entity">Request to insert.</param>
        /// <param name="connection">Active SQL connection.</param>
        /// <param name="transaction">Active SQL transaction.</param>
        void Add(Request entity, SqlConnection connection, SqlTransaction transaction);

        /// <summary>
        /// Deletes the request using an existing connection and transaction and returns the deleted entity.
        /// </summary>
        /// <param name="id">Request id to delete.</param>
        /// <param name="connection">Active SQL connection.</param>
        /// <param name="transaction">Active SQL transaction.</param>
        /// <returns>The deleted <see cref="Request"/> instance.</returns>
        Request Delete(int id, SqlConnection connection, SqlTransaction transaction);
    }
}
