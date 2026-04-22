    USE BoardRent;
    GO
    IF OBJECT_ID('dbo.Notifications', 'U') IS NOT NULL DBCC CHECKIDENT ('Notifications', RESEED, 0);
    IF OBJECT_ID('dbo.Rentals', 'U') IS NOT NULL DBCC CHECKIDENT ('Rentals', RESEED, 0);
    IF OBJECT_ID('dbo.Requests', 'U') IS NOT NULL DBCC CHECKIDENT ('Requests', RESEED, 0);
    IF OBJECT_ID('dbo.Games', 'U') IS NOT NULL DBCC CHECKIDENT ('Games', RESEED, 0);
    IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DBCC CHECKIDENT ('Users', RESEED, 0);
    GO
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'display_name')
    BEGIN
        ALTER TABLE Users ADD display_name NVARCHAR(50) NOT NULL DEFAULT 'Unknown User';
    END;
    GO
    SET IDENTITY_INSERT Users ON;
    INSERT INTO Users (id, display_name) VALUES (1, 'Darius Turcu'), (2, 'Mihai Tira');
    SET IDENTITY_INSERT Users OFF;
    GO
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
    (11, 1, 'Twilight Imperium', 40.00, 3, 6, 'Epic space opera board game of galactic conquest.', 0); -- Inactive to test REQ-GAM-05
    SET IDENTITY_INSERT Games OFF;
    GO
    SET IDENTITY_INSERT Requests ON;
    INSERT INTO Requests (request_id, game_id, renter_id, owner_id, start_date, end_date, status, offering_user_id) VALUES
    (1, 1, 2, 1, DATEADD(day, 5, GETDATE()), DATEADD(day, 7, GETDATE()), 0, NULL),
    (2, 1, 2, 1, DATEADD(day, 6, GETDATE()), DATEADD(day, 8, GETDATE()), 0, NULL), -- Overlaps with 1
    (3, 2, 2, 1, DATEADD(day, 10, GETDATE()), DATEADD(day, 12, GETDATE()), 0, NULL),
    (4, 2, 2, 1, DATEADD(day, 20, GETDATE()), DATEADD(day, 22, GETDATE()), 0, NULL),
    (5, 3, 2, 1, DATEADD(day, 1, GETDATE()), DATEADD(day, 3, GETDATE()), 0, NULL),
    (6, 6, 1, 2, DATEADD(day, 5, GETDATE()), DATEADD(day, 7, GETDATE()), 0, NULL),
    (7, 7, 1, 2, DATEADD(day, 15, GETDATE()), DATEADD(day, 18, GETDATE()), 1, 2), -- OfferPending from user 2
    (8, 8, 1, 2, DATEADD(day, 2, GETDATE()), DATEADD(day, 4, GETDATE()), 0, NULL),
    (9, 9, 1, 2, DATEADD(day, 8, GETDATE()), DATEADD(day, 10, GETDATE()), 0, NULL),
    (10, 10, 1, 2, DATEADD(day, 25, GETDATE()), DATEADD(day, 28, GETDATE()), 0, NULL),
    (11, 10, 1, 2, DATEADD(day, 26, GETDATE()), DATEADD(day, 29, GETDATE()), 0, NULL); -- Overlaps with 10
    SET IDENTITY_INSERT Requests OFF;
    GO
    SET IDENTITY_INSERT Rentals ON;
    INSERT INTO Rentals (rental_id, game_id, renter_id, owner_id, start_date, end_date) VALUES
    (1, 4, 2, 1, DATEADD(day, -10, GETDATE()), DATEADD(day, -8, GETDATE())), -- Past rental
    (2, 5, 2, 1, DATEADD(day, -5, GETDATE()), DATEADD(day, -3, GETDATE())),  -- Past rental
    (3, 1, 2, 1, DATEADD(day, -20, GETDATE()), DATEADD(day, -15, GETDATE())), -- Past rental
    (4, 2, 2, 1, DATEADD(day, 1, GETDATE()), DATEADD(day, 4, GETDATE())),    -- Active/Future rental
    (5, 3, 2, 1, DATEADD(day, 15, GETDATE()), DATEADD(day, 17, GETDATE())),  -- Future rental
    (6, 6, 1, 2, DATEADD(day, -12, GETDATE()), DATEADD(day, -10, GETDATE())), -- Past rental
    (7, 7, 1, 2, DATEADD(day, -8, GETDATE()), DATEADD(day, -6, GETDATE())),   -- Past rental
    (8, 8, 1, 2, DATEADD(day, -2, GETDATE()), DATEADD(day, 2, GETDATE())),    -- Currently active rental
    (9, 9, 1, 2, DATEADD(day, 10, GETDATE()), DATEADD(day, 12, GETDATE())),   -- Future rental
    (10, 10, 1, 2, DATEADD(day, 20, GETDATE()), DATEADD(day, 23, GETDATE())); -- Future rental
    SET IDENTITY_INSERT Rentals OFF;
    GO
    SET IDENTITY_INSERT Notifications ON;
    INSERT INTO Notifications (notification_id, user_id, timestamp, title, body, type, related_request_id) VALUES
    (1, 1, DATEADD(day, -5, GETDATE()), 'Upcoming Rental Reminder', 'Catan Base Game rental starts soon.', 0, NULL),
    (2, 1, DATEADD(day, -4, GETDATE()), 'Rental Request Declined', 'Twilight Imperium request declined.', 0, NULL),
    (3, 1, DATEADD(day, -3, GETDATE()), 'Booking Unavailable', 'Someone else booked 7 Wonders.', 0, NULL),
    (4, 1, DATEADD(day, -2, GETDATE()), 'Game Offer Received', 'Mihai Tira is offering you Dominion for a rental.', 1, 7), -- Actionable offer for request 7
    (5, 1, DATEADD(day, -1, GETDATE()), 'Offer Accepted', 'Mihai Tira accepted your offer for Codenames.', 2, NULL),
    (6, 2, DATEADD(day, -5, GETDATE()), 'Upcoming Rental Reminder', '7 Wonders rental starts soon.', 0, NULL),
    (7, 2, DATEADD(day, -4, GETDATE()), 'Rental Request Declined', 'Dominion request declined.', 0, NULL),
    (8, 2, DATEADD(day, -3, GETDATE()), 'Offer Denied', 'Darius Turcu denied your offer for Catan.', 2, NULL),
    (9, 2, DATEADD(day, -2, GETDATE()), 'Booking Unavailable', 'Someone else booked Catan Base Game.', 0, NULL),
    (10, 2, DATEADD(day, -1, GETDATE()), 'Rental Confirmed', 'You accepted the offer for Codenames.', 2, NULL);
    SET IDENTITY_INSERT Notifications OFF;
    GO
    IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DBCC CHECKIDENT ('Users', RESEED, 2);
    IF OBJECT_ID('dbo.Games', 'U') IS NOT NULL DBCC CHECKIDENT ('Games', RESEED, 11);
    IF OBJECT_ID('dbo.Requests', 'U') IS NOT NULL DBCC CHECKIDENT ('Requests', RESEED, 11);
    IF OBJECT_ID('dbo.Rentals', 'U') IS NOT NULL DBCC CHECKIDENT ('Rentals', RESEED, 10);
    IF OBJECT_ID('dbo.Notifications', 'U') IS NOT NULL DBCC CHECKIDENT ('Notifications', RESEED, 10);
    GO
