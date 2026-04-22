USE BoardRent;
GO
IF OBJECT_ID(N'[dbo].[Notifications]', 'U') IS NOT NULL 
    DROP TABLE [dbo].[Notifications];

IF OBJECT_ID(N'[dbo].[Rentals]', 'U') IS NOT NULL 
    DROP TABLE [dbo].[Rentals];

IF OBJECT_ID(N'[dbo].[Requests]', 'U') IS NOT NULL 
    DROP TABLE [dbo].[Requests];

IF OBJECT_ID(N'[dbo].[Games]', 'U') IS NOT NULL 
    DROP TABLE [dbo].[Games];

IF OBJECT_ID(N'[dbo].[Users]', 'U') IS NOT NULL 
    DROP TABLE [dbo].[Users];
GO
