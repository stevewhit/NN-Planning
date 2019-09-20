SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'ApplicationLogInsert')
BEGIN
	PRINT 'Dropping "ApplicationLogInsert" stored procedure...'
	DROP PROCEDURE ApplicationLogInsert
END
GO

PRINT 'Creating "ApplicationLogInsert" stored procedure...'
GO

-- =============================================
-- Author:		Steve Whitmire Jr.
-- Create date: 09-15-2019
-- Description:	Inserts a new application logs.
-- =============================================
CREATE proc [dbo].[ApplicationLogInsert]
	@logDate DateTime
	,@thread nvarchar(255)
	,@logLevel nvarchar(50)
	,@logger nvarchar(255)
	,@message nvarchar(max)
	,@exception nvarchar(max)
	,@location nvarchar(255)
	,@userId nvarchar(255)
AS
BEGIN
	INSERT INTO [dbo].[ApplicationLogs] ([Date],[Thread],[Level],[Logger],[Message],[Exception], [Location], [UserId] ) 
	VALUES (@logDate, @thread, @logLevel, @logger, @message, @exception, @location, @userId)
END
GO