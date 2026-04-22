using Property_and_Management.Src.Interface;

namespace Property_and_Management.Src.Model
{
    public class Game : IEntity
    {
        public int Id { get; set; }
        public User Owner { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int MinimumPlayerNumber { get; set; }
        public int MaximumPlayerNumber { get; set; }
        public string Description { get; set; }
        public byte[] Image { get; set; }
        public bool IsActive { get; set; }

        public Game()
        {
        }

        public Game(int id, User gameOwner, string name, decimal price,
                    int minimumPlayerNumber, int maximumPlayerNumber,
                    string description, byte[] image, bool isActive)
        {
            this.Id = id;
            Owner = gameOwner;
            Name = name;
            Price = price;
            MinimumPlayerNumber = minimumPlayerNumber;
            MaximumPlayerNumber = maximumPlayerNumber;
            Description = description;
            Image = image;
            IsActive = isActive;
        }
    }
}