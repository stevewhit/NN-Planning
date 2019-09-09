SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' and name = 'GetTestingDataset1')
BEGIN
	PRINT 'Dropping "GetTestingDataset1" stored procedure...'
	DROP PROCEDURE GetTestingDataset1
END
GO

PRINT 'Creating "GetTestingDataset1" stored procedure...'
GO

-- =============================================
-- Author:		Steve Whitmire Jr.
-- Create date: 09-05-2019
-- Description:	Calculates and returns the future five day performance of a company from a specific date.
-- =============================================
CREATE PROCEDURE [dbo].[GetTestingDataset1] 
	@companyId int,
	@date date
AS
BEGIN
	SET NOCOUNT ON;

	/**************************************************************************************************************
		Values used in the final return block.
		---------
		Note: Declare this above the if/else blocks so that we can return an empty dataset if the @date 
			  is invalid.
	***************************************************************************************************************/
	DECLARE @futureFiveDayPerformance TABLE (
				[QuoteId] INT, 
				[CompanyId] INT, 
				[Date] DATE, 
				[TriggeredRiseFirst] BIT, 
				[TriggeredFallFirst] BIT);

	-- Verify there are at least 5 additional quotes after the @date otherwise the futureFiveDayPerformance SP will not perform the calculations correctly.
	IF (SELECT COUNT(*) FROM StockMarketData.dbo.Quotes WHERE [CompanyId] = @companyId AND [Date] > @date) >= 5
	BEGIN
		/***************************
			Future Five Day Performance
		****************************/
		DECLARE @riseMultiplier DECIMAL(8, 4) = (SELECT [PerformanceRiseMultiplier] FROM DatasetRetrievalMethods WHERE [Id] = 1)
		DECLARE @fallMultiplier DECIMAL(8, 4) = (SELECT [PerformanceFallMultiplier] FROM DatasetRetrievalMethods WHERE [Id] = 1)

		INSERT INTO @futureFiveDayPerformance 
		EXECUTE GetFutureFiveDayPerformance @companyId, @date, @date, @riseMultiplier, @fallMultiplier
	END
	ELSE
	BEGIN
		PRINT 'Cannot create testing dataset for company (' + convert(nvarchar, @companyId) + ') because the Quotes table needs atleast 5 additional quotes after the @date to perform calculations correctly.'
	END

	/***************************
		Return Values
	****************************/
	SELECT CASE WHEN [TriggeredRiseFirst] = 1 THEN 1 ELSE 0 END [Output_TriggeredRiseFirst],
		   CASE WHEN [TriggeredFallFirst] = 1 THEN 1 ELSE 0 END [Output_TriggeredFallFirst]
	FROM @futureFiveDayPerformance
	WHERE [CompanyId] = @companyId AND [Date] = @date
END
GO


