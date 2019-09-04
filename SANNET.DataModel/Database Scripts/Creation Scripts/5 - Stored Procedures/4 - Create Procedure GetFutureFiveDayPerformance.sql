SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' and name = 'GetFutureFiveDayPerformance')
BEGIN
	PRINT 'Dropping "GetFutureFiveDayPerformance" stored procedure...'
	DROP PROCEDURE GetFutureFiveDayPerformance
END
GO

PRINT 'Creating "GetFutureFiveDayPerformance" stored procedure...'
GO

-- =============================================
-- Author:		Steve Whitmire Jr.
-- Create date: 08-28-2019
-- Description:	Returns the five day performance for a given company from the @startDate to the @endDate. 
--				The returned items will indicate whether the stock rose to the @riseMultiplierTrigger  
--				before falling to the @fallMultiplierTrigger and vice versa for each day.
-- =============================================
CREATE PROCEDURE [dbo].[GetFutureFiveDayPerformance]
	@companyId int,
	@startDate date,
	@endDate date,
	@riseMultiplierTrigger decimal(7, 3) = 1.01,
	@fallMultiplierTrigger decimal(7, 3) = .99
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.
	SET NOCOUNT ON;

	/*********************************************************************************************
		Table to hold the performance outputs starting from the @startDate 
	*********************************************************************************************/
	DECLARE @fiveDayPerformance TABLE
	(
		[QuoteId] INT,
		[CompanyId] INT,
		[Date] DATE,
		[Close] DECIMAL(12, 2),
		[FiveDayOutcomeType] INT
	)

	INSERT INTO @fiveDayPerformance
	SELECT [Id] as [QuoteId],
		   [CompanyId],
		   [Date],
		   [Close],
		   CASE WHEN LEAD([Close], 1) OVER (ORDER BY [Date]) > [Close] * @riseMultiplierTrigger THEN 1
				ELSE CASE WHEN LEAD([Close], 1) OVER (ORDER BY [Date]) < [Close] * @fallMultiplierTrigger THEN -1
					 ELSE CASE WHEN LEAD([Close], 2) OVER (ORDER BY [Date]) > [Close] * @riseMultiplierTrigger THEN 1
						  ELSE CASE WHEN LEAD([Close], 2) OVER (ORDER BY [Date]) < [Close] * @fallMultiplierTrigger THEN -1
							   ELSE CASE WHEN LEAD([Close], 3) OVER (ORDER BY [Date]) > [Close] * @riseMultiplierTrigger THEN 1
								    ELSE CASE WHEN LEAD([Close], 3) OVER (ORDER BY [Date]) < [Close] * @fallMultiplierTrigger THEN -1
									     ELSE CASE WHEN LEAD([Close], 4) OVER (ORDER BY [Date]) > [Close] * @riseMultiplierTrigger THEN 1
											  ELSE CASE WHEN LEAD([Close], 4) OVER (ORDER BY [Date]) < [Close] * @fallMultiplierTrigger THEN -1
												   ELSE CASE WHEN LEAD([Close], 5) OVER (ORDER BY [Date]) > [Close] * @riseMultiplierTrigger THEN 1
													    ELSE CASE WHEN LEAD([Close], 5) OVER (ORDER BY [Date]) < [Close] * @fallMultiplierTrigger THEN -1
															 ELSE 0
														END END END END END END END END END END as [FiveDayOutcomeType]
	FROM StockMarketData.dbo.Quotes
	WHERE [CompanyId] = @companyId AND [Date] >= @startDate
	ORDER BY [Date]
	
	/*********************************************************************************************
		Return dataset
	*********************************************************************************************/
	SELECT [QuoteId],
		   [CompanyId],
		   [Date],
		   --[Close],
		   CASE WHEN [FiveDayOutcomeType] = 1 THEN 1 ELSE 0 END as [TriggeredRiseFirst],
		   CASE WHEN [FiveDayOutcomeType] = -1 THEN 1 ELSE 0 END as [TriggeredFallFirst]
	FROM @fiveDayPerformance
	WHERE [CompanyId] = @companyId AND [Date] >= @startDate AND [Date] <= @endDate
	ORDER BY [Date]
END
GO


