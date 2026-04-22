

USE BoardRent;
GO
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Requests') AND name = 'status')
BEGIN
    ALTER TABLE Requests ADD status INT NOT NULL DEFAULT 0;
    PRINT 'Added status column to Requests';
END;
GO
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Requests') AND name = 'offering_user_id')
BEGIN
    ALTER TABLE Requests ADD offering_user_id INT NULL;
    ALTER TABLE Requests ADD CONSTRAINT FK_Request_OfferingUser
        FOREIGN KEY (offering_user_id) REFERENCES [Users](id);
    PRINT 'Added offering_user_id column to Requests';
END;
GO
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Notifications') AND name = 'type')
BEGIN
    ALTER TABLE Notifications ADD type INT NOT NULL DEFAULT 0;
    PRINT 'Added type column to Notifications';
END;
GO
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Notifications') AND name = 'related_request_id')
BEGIN
    ALTER TABLE Notifications ADD related_request_id INT NULL;
    ALTER TABLE Notifications ADD CONSTRAINT FK_Notification_Request
        FOREIGN KEY (related_request_id) REFERENCES Requests(request_id);
    PRINT 'Added related_request_id column to Notifications';
END;
GO

PRINT 'Offer system migration complete.';
GO
