using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Property_and_Management.src.Attributes;
using Property_and_Management.src.Interface;

namespace Property_and_Management.src.Model
{
    [SqlTableDefinition("Games")]
    public class Game : IEntity
    {
        [SqlTableFieldDefinition("game_id", IsPrimaryKey = true)]
        public int Id { get; set; }

        [SqlTableFieldDefinition("owner_id")]
        public int OwnerId { get; set; }

        [SqlTableFieldDefinition("name")]
        public string Name { get; set; }

        [SqlTableFieldDefinition("price")]
        public double Price { get; set; }

        [SqlTableFieldDefinition("minimum_player_number")]
        public int MinimumPlayerNumber { get; set; }

        [SqlTableFieldDefinition("maximum_player_number")]
        public int MaximumPlayerNumber { get; set; }

        [SqlTableFieldDefinition("description")]
        public string Description { get; set; }

        [SqlTableFieldDefinition("image")]
        public byte[] Image { get; set; }

        [SqlTableFieldDefinition("is_active")]
        public bool IsActive { get; set; }

        public Game(int id, int ownerId, string name, double price,
                    int minimumPlayerNumber, int maximumPlayerNumber,
                    string description, byte[] image, bool isActive)
        {
            Id = id;
            OwnerId = ownerId;
            Name = name;
            Price = price;
            MinimumPlayerNumber = minimumPlayerNumber;
            MaximumPlayerNumber = maximumPlayerNumber;
            Description = description;
            Image = image;
            IsActive = isActive;
        }

        public static IEntity BuildFromParameters(Dictionary<string, object> parameters)
        {
            return new Game(
                id: (int)parameters["game_id"],
                ownerId: (int)parameters["owner_id"],
                name: (string)parameters["name"],
                price: (double)parameters["price"],
                minimumPlayerNumber: (int)parameters["minimum_player_number"],
                maximumPlayerNumber: (int)parameters["maximum_player_number"],
                description: (string)parameters["description"],
                image: (byte[])parameters["image"],
                isActive: (bool)parameters["is_active"]
            );
        }
    }
}
