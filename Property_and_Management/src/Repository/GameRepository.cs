using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Data.SqlClient;
using Property_and_Management.Src.Interface;
using Property_and_Management.Src.Model;

namespace Property_and_Management.Src.Repository
{
    public class GameRepository : IGameRepository
    {
        private const int MissingForeignKeyId = 0;
        private const int VarBinaryMaxLength = -1;

        private readonly string boardRentConnectionString =
            System.Configuration.ConfigurationManager.ConnectionStrings["BoardRent"]?.ConnectionString ?? string.Empty;

        public ImmutableList<Game> GetAll()
        {
            var retrievedGames = new List<Game>();
            using (var connection = new SqlConnection(boardRentConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT g.*, u.display_name AS owner_display_name FROM Games g LEFT JOIN Users u ON u.id = g.owner_id";
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var ownerDisplayName = reader["owner_display_name"] as string ?? string.Empty;
                            var gameOwner = new User((int)reader["owner_id"], ownerDisplayName);
                            var mappedGame = new Game((int)reader["game_id"], gameOwner, (string)reader["name"], Convert.ToDecimal(reader["price"]), (int)reader["minimum_player_number"], (int)reader["maximum_player_number"], (string)reader["description"], reader["image"] as byte[], Convert.ToBoolean(reader["is_active"]));
                            retrievedGames.Add(mappedGame);
                        }
                    }
                }
            }
            return retrievedGames.ToImmutableList();
        }

        public void Add(Game gameToInsert)
        {
            using (var connection = new SqlConnection(boardRentConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO Games(owner_id, name, price, minimum_player_number, maximum_player_number, description, image, is_active) VALUES(@owner_id, @name, @price, @min_players, @max_players, @description, @image, @is_active); SELECT SCOPE_IDENTITY();";
                    command.Parameters.AddWithValue("@owner_id", gameToInsert.Owner?.Id ?? MissingForeignKeyId);
                    command.Parameters.AddWithValue("@name", gameToInsert.Name ?? string.Empty);
                    command.Parameters.AddWithValue("@price", gameToInsert.Price);
                    command.Parameters.AddWithValue("@min_players", gameToInsert.MinimumPlayerNumber);
                    command.Parameters.AddWithValue("@max_players", gameToInsert.MaximumPlayerNumber);
                    command.Parameters.AddWithValue("@description", gameToInsert.Description ?? string.Empty);
                    command.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@image", System.Data.SqlDbType.VarBinary, VarBinaryMaxLength)
                    {
                        Value = (object)gameToInsert.Image ?? DBNull.Value
                    });
                    command.Parameters.AddWithValue("@is_active", gameToInsert.IsActive);

                    gameToInsert.Id = Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public ImmutableList<Game> GetGamesByOwner(int ownerUserId)
        {
            var retrievedGames = new List<Game>();
            using (var connection = new SqlConnection(boardRentConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT g.*, u.display_name AS owner_display_name FROM Games g LEFT JOIN Users u ON u.id = g.owner_id WHERE g.owner_id = @owner_id";
                    command.Parameters.AddWithValue("@owner_id", ownerUserId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var ownerDisplayName = reader["owner_display_name"] as string ?? string.Empty;
                            var gameOwner = new User((int)reader["owner_id"], ownerDisplayName);
                            var mappedGame = new Game((int)reader["game_id"], gameOwner, (string)reader["name"], Convert.ToDecimal(reader["price"]), (int)reader["minimum_player_number"], (int)reader["maximum_player_number"], (string)reader["description"], reader["image"] as byte[], Convert.ToBoolean(reader["is_active"]));
                            retrievedGames.Add(mappedGame);
                        }
                    }
                }
            }
            return retrievedGames.ToImmutableList();
        }

        public void Update(int gameIdToUpdate, Game gameDataToUpdate)
        {
            using (var connection = new SqlConnection(boardRentConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "UPDATE Games SET owner_id = @owner_id, name = @name, price = @price, minimum_player_number = @min_players, maximum_player_number = @max_players, description = @description, image = @image, is_active = @is_active WHERE game_id = @id";
                    command.Parameters.AddWithValue("@id", gameIdToUpdate);
                    command.Parameters.AddWithValue("@owner_id", gameDataToUpdate.Owner?.Id ?? MissingForeignKeyId);
                    command.Parameters.AddWithValue("@name", gameDataToUpdate.Name ?? string.Empty);
                    command.Parameters.AddWithValue("@price", gameDataToUpdate.Price);
                    command.Parameters.AddWithValue("@min_players", gameDataToUpdate.MinimumPlayerNumber);
                    command.Parameters.AddWithValue("@max_players", gameDataToUpdate.MaximumPlayerNumber);
                    command.Parameters.AddWithValue("@description", gameDataToUpdate.Description ?? string.Empty);
                    command.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@image", System.Data.SqlDbType.VarBinary, VarBinaryMaxLength)
                    {
                        Value = (object)gameDataToUpdate.Image ?? DBNull.Value
                    });
                    command.Parameters.AddWithValue("@is_active", gameDataToUpdate.IsActive);
                    command.ExecuteNonQuery();
                }
            }
        }

        public Game Get(int gameIdToFind)
        {
            using (var connection = new SqlConnection(boardRentConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT g.*, u.display_name AS owner_display_name FROM Games g LEFT JOIN Users u ON u.id = g.owner_id WHERE g.game_id = @id";
                    command.Parameters.AddWithValue("@id", gameIdToFind);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var ownerDisplayName = reader["owner_display_name"] as string ?? string.Empty;
                            var gameOwner = new User((int)reader["owner_id"], ownerDisplayName);
                            return new Game((int)reader["game_id"], gameOwner, (string)reader["name"], Convert.ToDecimal(reader["price"]), (int)reader["minimum_player_number"], (int)reader["maximum_player_number"], (string)reader["description"], reader["image"] as byte[], Convert.ToBoolean(reader["is_active"]));
                        }
                    }
                }
            }
            throw new KeyNotFoundException();
        }

        public Game Delete(int gameIdToRemove)
        {
            using (var connection = new SqlConnection(boardRentConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "DELETE g OUTPUT deleted.game_id, deleted.owner_id, deleted.name, deleted.price, " +
                        "deleted.minimum_player_number, deleted.maximum_player_number, deleted.description, " +
                        "deleted.image, deleted.is_active, u.display_name AS owner_display_name " +
                        "FROM Games g LEFT JOIN Users u ON u.id = g.owner_id WHERE g.game_id = @id";
                    command.Parameters.AddWithValue("@id", gameIdToRemove);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var deletedGameOwner = new User((int)reader["owner_id"], reader["owner_display_name"] as string ?? string.Empty);
                            return new Game((int)reader["game_id"], deletedGameOwner, (string)reader["name"],
                                Convert.ToDecimal(reader["price"]), (int)reader["minimum_player_number"],
                                (int)reader["maximum_player_number"], (string)reader["description"],
                                reader["image"] as byte[], Convert.ToBoolean(reader["is_active"]));
                        }
                    }
                }
            }
            throw new KeyNotFoundException();
        }
    }
}
