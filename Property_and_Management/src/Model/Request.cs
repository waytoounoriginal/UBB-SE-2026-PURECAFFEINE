using System;
using Property_and_Management.src.Interface;

namespace Property_and_Management.src.Model
{
    public class Request : IEntity
    {
        public int Id { get; set; }
        public Game Game { get; set; }
        public User Renter { get; set; }
        public User Owner { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }


        public Request() { }

        public Request(int id, Game game, User renter, User owner, DateTime startDate, DateTime endDate)
        {
            Id = id;
            Game = game;
            Renter = renter;
            Owner = owner;
            StartDate = startDate;
            EndDate = endDate;
        }
    }
}
