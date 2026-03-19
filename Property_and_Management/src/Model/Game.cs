using System;
using Property_and_Management.src.Interface;

namespace Property_and_Management.src.Model
{
    public class Game : IEntity
    {
        public int Id { get; set; }
        public User Owner { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public int MinimumPlayerNumber { get; set; }
        public int MaximumPlayerNumber { get; set; }
        public string Description { get; set; }
        public byte[] Image { get; set; }
        public bool IsActive { get; set; }

        public Game() { }

        public Game(int id, User owner, string name, double price,
                    int minimumPlayerNumber, int maximumPlayerNumber,
                    string description, byte[] image, bool isActive)
        {
            Id = id;
            Owner = owner;
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
