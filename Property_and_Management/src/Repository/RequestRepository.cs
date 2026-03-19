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
    public class RequestRepository : DatabaseRepository<Request>
    {
        public void  Add(Request entity, SqlConnection connection, SqlTransaction transaction)
        {
            using var command = new SqlCommand("", connection, transaction);
            string query = SqlQueryHelper<Request>.CreateInsertQuery(command, entity);
            command.CommandText = query;
            command.ExecuteNonQuery();
        }

        public void Delete(int id, SqlConnection connection, SqlTransaction transaction)
        {
            using var command = new SqlCommand("", connection, transaction);
            command.CommandText = SqlQueryHelper<Request>.CreateDeleteQuery(id);
            command.ExecuteNonQuery();
        }
        public ImmutableList<Request> GetRequestsByOwner(int ownerId)
        {
            return SelectWhere($"owner_id = {ownerId}");
        }

        public ImmutableList<Request> GetRequestsByRenter(int renterId)
        {
            return SelectWhere($"renter_id = {renterId}");
        }

        public ImmutableList<Request> GetRequestsByGame(int gameId)
        {
            return SelectWhere($"game_id = {gameId}");
        }
    }
}
