using Property_and_Management.Src.DataTransferObjects;
using Property_and_Management.Src.Interface;
using Property_and_Management.Src.Model;

namespace Property_and_Management.Src.Mapper
{
    public class UserMapper : IMapper<User, UserDTO>
    {
        public UserDTO ToDTO(User userModel)
        {
            if (userModel == null)
            {
                return null;
            }

            return new UserDTO
            {
                Id = userModel.Id,
                DisplayName = userModel.DisplayName
            };
        }

        public User ToModel(UserDTO userDto)
        {
            if (userDto == null)
            {
                return null;
            }

            return new User
            {
                Id = userDto.Id,
                DisplayName = userDto.DisplayName
            };
        }
    }
}