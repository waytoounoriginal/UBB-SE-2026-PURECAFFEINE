using System;
using Property_and_Management.Src.Interface;

namespace Property_and_Management.Src.Model
{
    public class Rental : IEntity
    {
        public int Id { get; set; }
        public Game Game { get; set; }
        public User Renter { get; set; }
        public User Owner { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public Rental()
        {
        }

        public Rental(int id, Game rentedGame, User renterUser, User ownerUser, DateTime startDate, DateTime endDate)
        {
            this.Id = id;
            Game = rentedGame;
            Renter = renterUser;
            Owner = ownerUser;
            StartDate = startDate;
            EndDate = endDate;
        }
    }
}