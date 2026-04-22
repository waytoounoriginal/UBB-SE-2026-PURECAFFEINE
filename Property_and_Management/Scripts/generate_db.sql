IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'BoardRent')
BEGIN
    CREATE DATABASE BoardRent;
END
GO

USE BoardRent;
GO
IF OBJECT_ID(N'[dbo].[Users]', 'U') IS NULL
BEGIN
    CREATE TABLE Users (
        id INT IDENTITY(1,1) NOT NULL,
        display_name VARCHAR(50) NOT NULL DEFAULT 'Unknown User',
        CONSTRAINT PK_User PRIMARY KEY (id)
    );
END;
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
END;
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
END;
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
END;
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
END;
