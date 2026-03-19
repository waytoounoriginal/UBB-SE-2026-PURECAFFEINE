using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Property_and_Management.src.Interface;
using Property_and_Management.src.Model;

namespace Property_and_Management.src.DTO
{
    public class RequestDTO : IDTO<Request>
    {
        public int Id { get; set; }
        public Game Game { get; set; }
        public User Renter { get; set; }
        public User Owner { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public RequestDTO(int id, Game game, User renter, User owner, DateTime startDate, DateTime endDate)
        {
            Id = id;
            Game = game;
            Renter = renter;
            Owner = owner;
            StartDate = startDate;
            EndDate = endDate;
        }

        public RequestDTO(Request request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            Id = request.Id;
            Game = request.Game;
            Renter = request.Renter;
            Owner = request.Owner;
            StartDate = request.StartDate;
            EndDate = request.EndDate;
        }

        public Request ToModel()
        {
            return new Request(Id, Game, Renter, Owner, StartDate, EndDate);
        }

        public static IDTO<Request> FromModel(Request model)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            return new RequestDTO(model);
        }
    }
}
