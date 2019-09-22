SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'GetTestingDataset')
BEGIN
	PRINT 'Dropping "GetTestingDataset" stored procedure...'
	DROP PROCEDURE GetTestingDataset
END
GO

PRINT 'Creating "GetTestingDataset" stored procedure...'
GO

-- =============================================
-- Author:		Steve Whitmire Jr.
-- Create date: 09-21-2019
-- Description:	Returns the testing dataset for the Neural Network.
-- =============================================
CREATE PROCEDURE [dbo].[GetTestingDataset] 
	@quoteId int
AS
BEGIN
	SELECT * 
	FROM GetDataset_AllIndicators(@quoteId)
	WHERE [QuoteId] = @quoteId
END
GO