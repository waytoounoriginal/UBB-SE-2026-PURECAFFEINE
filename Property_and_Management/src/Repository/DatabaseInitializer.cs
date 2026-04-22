using System;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using Microsoft.Data.SqlClient;

namespace Property_and_Management.Src.Repository
{
    public static class DatabaseInitializer
    {
        private const string ConnectionStringName = "BoardRent";
        private const string MasterDatabase = "master";
        private const int EmptyRowCount = 0;

        public static void EnsureDatabaseInitialized()
        {
            var connectionString = ConfigurationManager.ConnectionStrings[ConnectionStringName]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                Debug.WriteLine($"DatabaseInitializer: connection string '{ConnectionStringName}' is missing.");
                return;
            }

            var originalBuilder = new SqlConnectionStringBuilder(connectionString);
            var targetDatabase = originalBuilder.InitialCatalog;
            if (string.IsNullOrWhiteSpace(targetDatabase))
            {
                Debug.WriteLine("DatabaseInitializer: connection string has no Initial Catalog.");
                return;
            }

            EnsureDatabaseExists(originalBuilder, targetDatabase);

            using var connection = new SqlConnection(connectionString);
            connection.Open();

            EnsureTablesExist(connection);
            EnsureOfferSystemColumnsMigrated(connection);
            SeedTestDataIfEmpty(connection);
        }

        private static void EnsureDatabaseExists(SqlConnectionStringBuilder originalBuilder, string targetDatabase)
        {
            var masterBuilder = new SqlConnectionStringBuilder(originalBuilder.ConnectionString)
            {
                InitialCatalog = MasterDatabase
            };

            using var connection = new SqlConnection(masterBuilder.ConnectionString);
            connection.Open();

            var escapedLiteral = targetDatabase.Replace("'", "''");
            var escapedIdentifier = targetDatabase.Replace("]", "]]");

            using var command = connection.CreateCommand();
            command.CommandText =
                $"IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = N'{escapedLiteral}') " +
                $"BEGIN CREATE DATABASE [{escapedIdentifier}]; END";
            command.ExecuteNonQuery();
        }

        private static void EnsureTablesExist(SqlConnection connection)
        {
            ExecuteBatch(connection, CreateUsersTableSql);
            ExecuteBatch(connection, CreateGamesTableSql);
            ExecuteBatch(connection, CreateRequestsTableSql);
            ExecuteBatch(connection, CreateRentalsTableSql);
            ExecuteBatch(connection, CreateNotificationsTableSql);
        }

        private static void EnsureOfferSystemColumnsMigrated(SqlConnection connection)
        {
            ExecuteBatch(connection, AddRequestsStatusColumnSql);
            ExecuteBatch(connection, AddRequestsOfferingUserColumnSql);
            ExecuteBatch(connection, AddNotificationsTypeColumnSql);
            ExecuteBatch(connection, AddNotificationsRelatedRequestColumnSql);
        }

        private static void SeedTestDataIfEmpty(SqlConnection connection)
        {
            using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
            {
                using (var checkCommand = connection.CreateCommand())
                {
                    checkCommand.Transaction = transaction;
                    checkCommand.CommandText = "SELECT COUNT(*) FROM Users";
                    var existingUserCount = Convert.ToInt32(checkCommand.ExecuteScalar());
                    if (existingUserCount != EmptyRowCount)
                    {
                        transaction.Rollback();
                        return;
                    }
                }

                ExecuteBatch(connection, SeedUsersSql, transaction);
                ExecuteBatch(connection, SeedGamesSql, transaction);
                ExecuteBatch(connection, SeedRequestsSql, transaction);
                ExecuteBatch(connection, SeedRentalsSql, transaction);
                ExecuteBatch(connection, SeedNotificationsSql, transaction);

                transaction.Commit();
            }

            ExecuteBatch(connection, ReseedIdentityColumnsSql);
        }

        private static void ExecuteBatch(SqlConnection connection, string sql, SqlTransaction? transaction = null)
        {
            using var command = connection.CreateCommand();
            if (transaction != null)
            {
                command.Transaction = transaction;
            }

            command.CommandText = sql;
            command.ExecuteNonQuery();
        }

        private const string CreateUsersTableSql = @"
IF OBJECT_ID(N'[dbo].[Users]', 'U') IS NULL
BEGIN
    CREATE TABLE Users (
        id INT IDENTITY(1,1) NOT NULL,
        display_name VARCHAR(50) NOT NULL DEFAULT 'Unknown User',
        CONSTRAINT PK_User PRIMARY KEY (id)
    );
END;";

        private const string CreateGamesTableSql = @"
IF OBJECT_ID(N'[dbo].[Games]', 'U') IS NULL
BEGIN
    CREATE TABLE Games (
        game_id INT IDENTITY(1,1) NOT NULL,
        owner_id INT NOT NULL,
        name VARCHAR(30) NOT NULL,
        price DECIMAL(5,2) NOT NULL,
        minimum_player_number INT NOT NULL,
        maximum_player_number INT NOT NULL,
        description VARCHAR(500) NOT NULL,
        image VARBINARY(MAX),
        is_active INT NOT NULL DEFAULT 1,
        CONSTRAINT PK_Games PRIMARY KEY (game_id),
        CONSTRAINT FK_Games_Owner FOREIGN KEY (owner_id) REFERENCES [Users](id),
        CONSTRAINT CHK_Game_Price CHECK (price > 0),
        CONSTRAINT CHK_Min_Players CHECK (minimum_player_number >= 1),
        CONSTRAINT CHK_Max_Players CHECK (maximum_player_number >= 1 AND maximum_player_number >= minimum_player_number)
    );
END;";

        private const string CreateRequestsTableSql = @"
IF OBJECT_ID(N'[dbo].[Requests]', 'U') IS NULL
BEGIN
    CREATE TABLE Requests (
        request_id INT IDENTITY(1,1) NOT NULL,
        game_id INT NOT NULL,
        renter_id INT NOT NULL,
        owner_id INT NOT NULL,
        start_date DATETIME NOT NULL,
        end_date DATETIME NOT NULL,
        status INT NOT NULL DEFAULT 0,
        offering_user_id INT NULL,
        CONSTRAINT PK_Request PRIMARY KEY (request_id),
        CONSTRAINT FK_Request_Game FOREIGN KEY (game_id) REFERENCES Games(game_id),
        CONSTRAINT FK_Request_Renter FOREIGN KEY (renter_id) REFERENCES [Users](id),
        CONSTRAINT FK_Request_Owner FOREIGN KEY (owner_id) REFERENCES [Users](id),
        CONSTRAINT FK_Request_OfferingUser FOREIGN KEY (offering_user_id) REFERENCES [Users](id),
        CONSTRAINT CHK_Request_DateRange CHECK (end_date >= start_date)
    );
END;";

        private const string CreateRentalsTableSql = @"
IF OBJECT_ID(N'[dbo].[Rentals]', 'U') IS NULL
BEGIN
    CREATE TABLE Rentals (
        rental_id INT IDENTITY(1,1) NOT NULL,
        game_id INT NOT NULL,
        renter_id INT NOT NULL,
        owner_id INT NOT NULL,
        start_date DATETIME NOT NULL,
        end_date DATETIME NOT NULL,
        CONSTRAINT PK_Rentals PRIMARY KEY (rental_id),
        CONSTRAINT FK_Rentals_Game FOREIGN KEY (game_id) REFERENCES Games(game_id),
        CONSTRAINT FK_Rentals_Renter FOREIGN KEY (renter_id) REFERENCES [Users](id),
        CONSTRAINT FK_Rentals_Owner FOREIGN KEY (owner_id) REFERENCES [Users](id),
        CONSTRAINT CHK_Rentals_DateRange CHECK (end_date >= start_date)
    );
END;";

        private const string CreateNotificationsTableSql = @"
IF OBJECT_ID(N'[dbo].[Notifications]', 'U') IS NULL
BEGIN
    CREATE TABLE Notifications (
        notification_id INT IDENTITY(1,1) NOT NULL,
        user_id INT NOT NULL,
        timestamp DATETIME NOT NULL,
        title VARCHAR(30) NOT NULL,
        body VARCHAR(500) NOT NULL,
        type INT NOT NULL DEFAULT 0,
        related_request_id INT NULL,
        CONSTRAINT PK_Notifications PRIMARY KEY (notification_id),
        CONSTRAINT FK_Notifications_User FOREIGN KEY (user_id) REFERENCES [Users](id),
        CONSTRAINT FK_Notification_Request FOREIGN KEY (related_request_id) REFERENCES Requests(request_id)
    );
END;";

        private const string AddRequestsStatusColumnSql = @"
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Requests') AND name = 'status')
BEGIN
    ALTER TABLE Requests ADD status INT NOT NULL DEFAULT 0;
END;";

        private const string AddRequestsOfferingUserColumnSql = @"
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Requests') AND name = 'offering_user_id')
BEGIN
    ALTER TABLE Requests ADD offering_user_id INT NULL;
    ALTER TABLE Requests ADD CONSTRAINT FK_Request_OfferingUser
        FOREIGN KEY (offering_user_id) REFERENCES [Users](id);
END;";

        private const string AddNotificationsTypeColumnSql = @"
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Notifications') AND name = 'type')
BEGIN
    ALTER TABLE Notifications ADD type INT NOT NULL DEFAULT 0;
END;";

        private const string AddNotificationsRelatedRequestColumnSql = @"
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Notifications') AND name = 'related_request_id')
BEGIN
    ALTER TABLE Notifications ADD related_request_id INT NULL;
    ALTER TABLE Notifications ADD CONSTRAINT FK_Notification_Request
        FOREIGN KEY (related_request_id) REFERENCES Requests(request_id);
END;";

        private const string SeedUsersSql = @"
SET IDENTITY_INSERT Users ON;
INSERT INTO Users (id, display_name) VALUES (1, 'Darius Turcu'), (2, 'Mihai Tira');
SET IDENTITY_INSERT Users OFF;";

        private const string SeedGamesSql = @"
SET IDENTITY_INSERT Games ON;
INSERT INTO Games (game_id, owner_id, name, price, minimum_player_number, maximum_player_number, description, is_active) VALUES
(1, 1, 'Catan Base Game', 15.50, 3, 4, 'Classic resource management and trading board game.', 1),
(2, 1, 'Ticket to Ride Europe', 12.00, 2, 5, 'Build train routes across Europe in this classic game.', 1),
(3, 1, 'Carcassonne', 10.00, 2, 5, 'Tile-placement game where you build medieval cities.', 1),
(4, 1, 'Pandemic', 18.00, 2, 4, 'Cooperative game where you save the world from diseases.', 1),
(5, 1, 'Splendor', 14.00, 2, 4, 'Engine-building game involving gem collecting and cards.', 1),
(6, 2, '7 Wonders', 20.00, 2, 7, 'Draft cards to build your ancient civilization.', 1),
(7, 2, 'Dominion', 16.00, 2, 4, 'The original deck-building card game.', 1),
(8, 2, 'Codenames', 8.50, 2, 8, 'Word association party game for two teams.', 1),
(9, 2, 'Wingspan', 22.00, 1, 5, 'Engine-building game based on bird watching.', 1),
(10, 2, 'Terraforming Mars', 25.00, 1, 5, 'Compete to make Mars habitable for humanity.', 1),
(11, 1, 'Twilight Imperium', 40.00, 3, 6, 'Epic space opera board game of galactic conquest.', 0);
SET IDENTITY_INSERT Games OFF;";

        private const string SeedRequestsSql = @"
SET IDENTITY_INSERT Requests ON;
INSERT INTO Requests (request_id, game_id, renter_id, owner_id, start_date, end_date, status, offering_user_id) VALUES
(1, 1, 2, 1, DATEADD(day, 5, GETDATE()), DATEADD(day, 7, GETDATE()), 0, NULL),
(2, 1, 2, 1, DATEADD(day, 6, GETDATE()), DATEADD(day, 8, GETDATE()), 0, NULL),
(3, 2, 2, 1, DATEADD(day, 10, GETDATE()), DATEADD(day, 12, GETDATE()), 0, NULL),
(4, 2, 2, 1, DATEADD(day, 20, GETDATE()), DATEADD(day, 22, GETDATE()), 0, NULL),
(5, 3, 2, 1, DATEADD(day, 1, GETDATE()), DATEADD(day, 3, GETDATE()), 0, NULL),
(6, 6, 1, 2, DATEADD(day, 5, GETDATE()), DATEADD(day, 7, GETDATE()), 0, NULL),
(7, 7, 1, 2, DATEADD(day, 15, GETDATE()), DATEADD(day, 18, GETDATE()), 1, 2),
(8, 8, 1, 2, DATEADD(day, 2, GETDATE()), DATEADD(day, 4, GETDATE()), 0, NULL),
(9, 9, 1, 2, DATEADD(day, 8, GETDATE()), DATEADD(day, 10, GETDATE()), 0, NULL),
(10, 10, 1, 2, DATEADD(day, 25, GETDATE()), DATEADD(day, 28, GETDATE()), 0, NULL),
(11, 10, 1, 2, DATEADD(day, 26, GETDATE()), DATEADD(day, 29, GETDATE()), 0, NULL);
SET IDENTITY_INSERT Requests OFF;";

        private const string SeedRentalsSql = @"
SET IDENTITY_INSERT Rentals ON;
INSERT INTO Rentals (rental_id, game_id, renter_id, owner_id, start_date, end_date) VALUES
(1, 4, 2, 1, DATEADD(day, -10, GETDATE()), DATEADD(day, -8, GETDATE())),
(2, 5, 2, 1, DATEADD(day, -5, GETDATE()), DATEADD(day, -3, GETDATE())),
(3, 1, 2, 1, DATEADD(day, -20, GETDATE()), DATEADD(day, -15, GETDATE())),
(4, 2, 2, 1, DATEADD(day, 1, GETDATE()), DATEADD(day, 4, GETDATE())),
(5, 3, 2, 1, DATEADD(day, 15, GETDATE()), DATEADD(day, 17, GETDATE())),
(6, 6, 1, 2, DATEADD(day, -12, GETDATE()), DATEADD(day, -10, GETDATE())),
(7, 7, 1, 2, DATEADD(day, -8, GETDATE()), DATEADD(day, -6, GETDATE())),
(8, 8, 1, 2, DATEADD(day, -2, GETDATE()), DATEADD(day, 2, GETDATE())),
(9, 9, 1, 2, DATEADD(day, 10, GETDATE()), DATEADD(day, 12, GETDATE())),
(10, 10, 1, 2, DATEADD(day, 20, GETDATE()), DATEADD(day, 23, GETDATE()));
SET IDENTITY_INSERT Rentals OFF;";

        private const string SeedNotificationsSql = @"
SET IDENTITY_INSERT Notifications ON;
INSERT INTO Notifications (notification_id, user_id, timestamp, title, body, type, related_request_id) VALUES
(1, 1, DATEADD(day, -5, GETDATE()), 'Upcoming Rental Reminder', 'Catan Base Game rental starts soon.', 0, NULL),
(2, 1, DATEADD(day, -4, GETDATE()), 'Rental Request Declined', 'Twilight Imperium request declined.', 0, NULL),
(3, 1, DATEADD(day, -3, GETDATE()), 'Booking Unavailable', 'Someone else booked 7 Wonders.', 0, NULL),
(4, 1, DATEADD(day, -2, GETDATE()), 'Game Offer Received', 'Mihai Tira is offering you Dominion for a rental.', 1, 7),
(5, 1, DATEADD(day, -1, GETDATE()), 'Offer Accepted', 'Mihai Tira accepted your offer for Codenames.', 2, NULL),
(6, 2, DATEADD(day, -5, GETDATE()), 'Upcoming Rental Reminder', '7 Wonders rental starts soon.', 0, NULL),
(7, 2, DATEADD(day, -4, GETDATE()), 'Rental Request Declined', 'Dominion request declined.', 0, NULL),
(8, 2, DATEADD(day, -3, GETDATE()), 'Offer Denied', 'Darius Turcu denied your offer for Catan.', 2, NULL),
(9, 2, DATEADD(day, -2, GETDATE()), 'Booking Unavailable', 'Someone else booked Catan Base Game.', 0, NULL),
(10, 2, DATEADD(day, -1, GETDATE()), 'Rental Confirmed', 'You accepted the offer for Codenames.', 2, NULL);
SET IDENTITY_INSERT Notifications OFF;";

        private const string ReseedIdentityColumnsSql = @"
DBCC CHECKIDENT ('Users', RESEED, 2);
DBCC CHECKIDENT ('Games', RESEED, 11);
DBCC CHECKIDENT ('Requests', RESEED, 11);
DBCC CHECKIDENT ('Rentals', RESEED, 10);
DBCC CHECKIDENT ('Notifications', RESEED, 10);";
    }
}