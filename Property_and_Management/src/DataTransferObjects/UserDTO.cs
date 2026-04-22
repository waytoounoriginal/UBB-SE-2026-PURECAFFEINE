using Property_and_Management.Src.Interface;
using Property_and_Management.Src.Model;

namespace Property_and_Management.Src.DataTransferObjects
{
    public class UserDTO : IDTO<User>
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }

        public UserDTO()
        {
        }
    }
}