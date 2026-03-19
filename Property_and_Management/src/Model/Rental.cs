using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Property_and_Management.src.Attributes;
using Property_and_Management.src.Interface;

namespace Property_and_Management.src.Model
{
    [SqlTableDefinition("Rentals")]
    public class Rental : IEntity
    {
        [SqlTableFieldDefinition("rental_id", IsPrimaryKey = true)]
        public int Id { get; set; }

        [SqlTableFieldDefinition("game_id")]

        public int GameId { get; set; }

        [SqlTableFieldDefinition("renter_id")]
        public int RenterId { get; set; }

        [SqlTableFieldDefinition("owner_id")]
        public int OwnerId { get; set; }

        [SqlTableFieldDefinition("start_date")]
        public DateTime StartDate { get; set; }

        [SqlTableFieldDefinition("end_date")]
        public DateTime EndDate { get; set; }

        public Rental(int id, int gameId, int renterId, int ownerId, DateTime startDate, DateTime endDate)
        {
            Id = id;
            GameId = gameId;
            RenterId = renterId;
            OwnerId = ownerId;
            StartDate = startDate;
            EndDate = endDate;
        }

        public static IEntity BuildFromParameters(Dictionary<string, object> parameters)
        {
            return new Rental(
                id: (int)parameters["rental_id"],
                gameId: (int)parameters["game_id"],
                renterId: (int)parameters["renter_id"],
                ownerId: (int)parameters["owner_id"],
                startDate: (DateTime)parameters["start_date"],
                endDate: (DateTime)parameters["end_date"]
                );

        }
    }
}
