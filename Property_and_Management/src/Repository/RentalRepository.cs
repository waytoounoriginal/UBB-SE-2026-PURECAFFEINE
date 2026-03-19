using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Property_and_Management.src.Interface;
using Property_and_Management.src.Model;
using Property_and_Management.src.SQL;

namespace Property_and_Management.src.Repository
{
    public class RentalRepository : DatabaseRepository<Rental>

    {
        public void Add(Rental entity, SqlConnection connection, SqlTransaction transaction)
        {
            using var command = new SqlCommand("", connection, transaction);
            string query = SqlQueryHelper<Rental>.CreateInsertQuery(command, entity);
            command.CommandText = query;
            command.ExecuteNonQuery();
        }

        public ImmutableList<Rental> GetRentalsByOwner(int ownerId)
        {
            return SelectWhere($"owner_id = {ownerId}");
        }

        public ImmutableList<Rental> GetRentalsByRenter(int renterId)
        {
            return SelectWhere($"renter_id = {renterId}");
        }

        public ImmutableList<Rental> GetRentalsByGame(int gameId)
        {
            return SelectWhere($"game_id = {gameId}");
        }
    }
}
