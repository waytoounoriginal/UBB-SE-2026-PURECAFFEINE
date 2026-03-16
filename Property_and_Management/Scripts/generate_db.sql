/*
This script should be run when setting up the project, so that you actually have access to the database.

Run in SSMS.
*/

-- 1. Create the Database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'BoardRent')
BEGIN
    CREATE DATABASE BoardRent;
END
GO

USE BoardRent;
GO

-- 2. Create the User table
IF OBJECT_ID(N'[dbo].[User]', 'U') IS NULL
BEGIN
    CREATE TABLE [User] (
        id INT IDENTITY(1,1) NOT NULL,
        CONSTRAINT PK_User PRIMARY KEY (id)
    );
END;

-- 3. Create the Games table
IF OBJECT_ID(N'[dbo].[Games]', 'U') IS NULL
BEGIN
    CREATE TABLE Games (
        game_id INT IDENTITY(1,1) NOT NULL,
        owner_id INT NOT NULL,
        name NVARCHAR(30) NOT NULL,
        price FLOAT NOT NULL,
        minimum_player_number INT NOT NULL,
        maximum_player_number INT NOT NULL,
        description NVARCHAR(500) NOT NULL,
        image VARBINARY(MAX),
        is_active BIT NOT NULL DEFAULT 1,
        
        CONSTRAINT PK_Games PRIMARY KEY (game_id),
        CONSTRAINT FK_Games_Owner FOREIGN KEY (owner_id) REFERENCES [User](id),
        
        -- Business Logic Constraints
        CONSTRAINT CHK_Game_Price CHECK (price > 0),
        CONSTRAINT CHK_Min_Players CHECK (minimum_player_number >= 1), 
        CONSTRAINT CHK_Max_Players CHECK (maximum_player_number >= 1 AND maximum_player_number >= minimum_player_number)
    );
END;

-- 4. Create the Request table (Pending Inquiries)
IF OBJECT_ID(N'[dbo].[Request]', 'U') IS NULL
BEGIN
    CREATE TABLE Request (
        request_id INT IDENTITY(1,1) NOT NULL,
        game_id INT NOT NULL,
        renter_id INT NOT NULL,
        owner_id INT NOT NULL,
        start_date DATETIME NOT NULL,
        end_date DATETIME NOT NULL,
        
        CONSTRAINT PK_Request PRIMARY KEY (request_id),
        CONSTRAINT FK_Request_Game FOREIGN KEY (game_id) REFERENCES Games(game_id),
        CONSTRAINT FK_Request_Renter FOREIGN KEY (renter_id) REFERENCES [User](id),
        CONSTRAINT FK_Request_Owner FOREIGN KEY (owner_id) REFERENCES [User](id),
        CONSTRAINT CHK_Request_DateRange CHECK (end_date >= start_date)
    );
END;

-- 5. Create the Rentals table (Confirmed Bookings)
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
        CONSTRAINT FK_Rentals_Renter FOREIGN KEY (renter_id) REFERENCES [User](id),
        CONSTRAINT FK_Rentals_Owner FOREIGN KEY (owner_id) REFERENCES [User](id),
        CONSTRAINT CHK_Rentals_DateRange CHECK (end_date >= start_date)
    );
END;

-- 6. Create the Notifications table
IF OBJECT_ID(N'[dbo].[Notifications]', 'U') IS NULL
BEGIN
    CREATE TABLE Notifications (
        notification_id INT IDENTITY(1,1) NOT NULL,
        user_id INT NOT NULL,
        timestamp DATETIME NOT NULL,
        title NVARCHAR(30) NOT NULL,
        body NVARCHAR(500) NOT NULL,
        
        CONSTRAINT PK_Notifications PRIMARY KEY (notification_id),
        CONSTRAINT FK_Notifications_User FOREIGN KEY (user_id) REFERENCES [User](id)
    );
END;
