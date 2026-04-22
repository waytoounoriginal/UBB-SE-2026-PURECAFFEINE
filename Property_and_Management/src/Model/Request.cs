using System;
using Property_and_Management.Src.Interface;

namespace Property_and_Management.Src.Model
{
    public class Request : IEntity
    {
        public int Id { get; set; }
        public Game Game { get; set; }
        public User Renter { get; set; }
        public User Owner { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public RequestStatus Status { get; set; } = RequestStatus.Open;
        public User? OfferingUser { get; set; }

        public Request()
        {
        }

        public Request(int id, Game requestedGame, User renterUser, User ownerUser, DateTime startDate, DateTime endDate,
                       RequestStatus status = RequestStatus.Open, User? offeringUser = null)
        {
            this.Id = id;
            Game = requestedGame;
            Renter = renterUser;
            Owner = ownerUser;
            StartDate = startDate;
            EndDate = endDate;
            Status = status;
            OfferingUser = offeringUser;
        }
    }
}