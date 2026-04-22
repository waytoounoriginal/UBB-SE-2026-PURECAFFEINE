using Property_and_Management.Src.Interface;
using Property_and_Management.Src.Model;

namespace Property_and_Management.Src.DataTransferObjects
{
    public class GameDTO : IDTO<Game>
    {
        public int Id { get; set; }
        public UserDTO Owner { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int MinimumPlayerNumber { get; set; }
        public int MaximumPlayerNumber { get; set; }
        public string Description { get; set; }
        public byte[] Image { get; set; }
        public bool IsActive { get; set; }

        public GameDTO()
        {
        }
    }
}