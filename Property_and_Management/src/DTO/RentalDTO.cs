using System;
using Property_and_Management.src.Interface;
using Property_and_Management.src.Model;

namespace Property_and_Management.src.DTO
{
    public class RentalDTO : IDTO<Rental>
    {
        public int Id { get; set; }
        public Game Game { get; set; }
        public User Renter { get; set; }
        public User Owner { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public RentalDTO(int id, Game game, User renter, User owner, DateTime startDate, DateTime endDate)
        {
            Id = id;
            Game = game;
            Renter = renter;
            Owner = owner;
            StartDate = startDate;
            EndDate = endDate;
        }

        public RentalDTO(Rental rental)
        {
            if (rental == null) throw new ArgumentNullException(nameof(rental));

            Id = rental.Id;
            Game = rental.Game;
            Renter = rental.Renter;
            Owner = rental.Owner;
            StartDate = rental.StartDate;
            EndDate = rental.EndDate;
        }

        public Rental ToModel() => new Rental(Id, Game, Renter, Owner, StartDate, EndDate);

        public static IDTO<Rental> FromModel(Rental model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            return new RentalDTO(model);
        }
    }
}
