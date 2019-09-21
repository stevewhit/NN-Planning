SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'GetTrainingDataset')
BEGIN
	PRINT 'Dropping "GetTrainingDataset" stored procedure...'
	DROP PROCEDURE GetTrainingDataset
END
GO

PRINT 'Creating "GetTrainingDataset" stored procedure...'
GO

-- =============================================
-- Author:		Steve Whitmire Jr.
-- Create date: 09-21-2019
-- Description:	Returns the training dataset for the Neural Network.
-- =============================================
CREATE PROCEDURE [dbo].[GetTrainingDataset] 
	@companyId int,
	@quoteId int
AS
BEGIN
	SELECT * 
	FROM GetDataset_AllIndicators(@companyId, @quoteId)
	WHERE [QuoteId] < @quoteId
END
GO