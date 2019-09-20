SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'ApplicationLogSelect')
BEGIN
	PRINT 'Dropping "ApplicationLogSelect" stored procedure...'
	DROP PROCEDURE ApplicationLogSelect
END
GO

PRINT 'Creating "ApplicationLogSelect" stored procedure...'
GO

-- =============================================
-- Author:		Steve Whitmire Jr.
-- Create date: 09-15-2019
-- Description:	Returns the application logs.
-- =============================================
CREATE PROC [dbo].[ApplicationLogSelect]
AS
BEGIN
	SELECT [Id]
		,[Date]
		,[Thread]
		,[Level]
		,[Logger]
		,[Message]
		,[Exception]
		,[Location]
		,[UserId]
	FROM [dbo].[ApplicationLogs] logEntry
	ORDER BY logEntry.Date DESC
END
GO
