using Property_and_Management.Src.DataTransferObjects;
using Property_and_Management.Src.Interface;
using Property_and_Management.Src.Model;

namespace Property_and_Management.Src.Mapper
{
    public class RentalMapper : IMapper<Rental, RentalDTO>
    {
        private readonly IMapper<Game, GameDTO> rentalGameMapper;
        private readonly IMapper<User, UserDTO> rentalParticipantUserMapper;

        public RentalMapper(IMapper<Game, GameDTO> gameMapper, IMapper<User, UserDTO> userMapper)
        {
            this.rentalGameMapper = gameMapper;
            this.rentalParticipantUserMapper = userMapper;
        }

        public RentalDTO ToDTO(Rental rentalModel)
        {
            if (rentalModel == null)
            {
                return null;
            }

            return new RentalDTO
            {
                Id = rentalModel.Id,
                Game = rentalGameMapper.ToDTO(rentalModel.Game),
                Renter = rentalParticipantUserMapper.ToDTO(rentalModel.Renter),
                Owner = rentalParticipantUserMapper.ToDTO(rentalModel.Owner),
                StartDate = rentalModel.StartDate,
                EndDate = rentalModel.EndDate
            };
        }

        public Rental ToModel(RentalDTO rentalDto)
        {
            if (rentalDto == null)
            {
                return null;
            }

            return new Rental
            {
                Id = rentalDto.Id,
                Game = rentalGameMapper.ToModel(rentalDto.Game),
                Renter = rentalParticipantUserMapper.ToModel(rentalDto.Renter),
                Owner = rentalParticipantUserMapper.ToModel(rentalDto.Owner),
                StartDate = rentalDto.StartDate,
                EndDate = rentalDto.EndDate
            };
        }
    }
}