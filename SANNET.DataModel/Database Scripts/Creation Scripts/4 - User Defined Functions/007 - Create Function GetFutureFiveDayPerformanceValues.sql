SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'TF' and name = 'GetFutureFiveDayPerformanceValues')
BEGIN
	PRINT 'Dropping "GetFutureFiveDayPerformanceValues" function...'
	DROP FUNCTION GetFutureFiveDayPerformanceValues
END
GO

PRINT 'Creating "GetFutureFiveDayPerformanceValues" function...'
GO

-- =============================================
-- Author:		Steve Whitmire Jr.
-- Create date: 08-28-2019
-- Description:	Returns the five day performance for a given company from the @startDate to the @endDate. 
--				The returned items will indicate whether the stock rose to the @riseMultiplierTrigger  
--				before falling to the @fallMultiplierTrigger and vice versa for each day.
-- =============================================
CREATE FUNCTION [dbo].[GetFutureFiveDayPerformanceValues] 
(
	@companyId int,
	@startDate date,
	@endDate date,
	@riseMultiplierTrigger decimal(7, 3) = 1.01,
	@fallMultiplierTrigger decimal(7, 3) = .99
)
RETURNS @fiveDayPerformanceValues TABLE
(
	[QuoteId] INT UNIQUE,
	[TriggeredRiseFirst] BIT,
	[TriggeredFallFirst] BIT
)
AS
BEGIN

	/*********************************************
		Table to hold numbered quotes.
	**********************************************/
	DECLARE @companyQuotes TABLE
	(
		[QuoteId] INT UNIQUE,
		[CompanyQuoteNum] INT UNIQUE,
        [Date] DATE UNIQUE,
		[CompanyId] INT,
		[Close] DECIMAL(9, 3)
	)

	INSERT INTO @companyQuotes
	SELECT [QuoteId], [CompanyQuoteNum], [Date], [CompanyId], [Close]
	FROM GetCompanyQuotes(@companyId)
	WHERE [Date] >= @startDate
	ORDER BY [CompanyQuoteNum]

	/*********************************************************************************************
		Table to hold the performance outputs
	*********************************************************************************************/
	DECLARE @fiveDayPerformance TABLE
	(
		[QuoteId] INT UNIQUE,
		[FiveDayOutcomeType] INT
	)

	INSERT INTO @fiveDayPerformance
	SELECT [QuoteId],
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
	FROM @companyQuotes
	
	/*********************************************************************************************
		Return dataset
	*********************************************************************************************/
	INSERT INTO @fiveDayPerformanceValues
	SELECT quotes.[QuoteId],
		   CASE WHEN [FiveDayOutcomeType] = 1 THEN 1 ELSE 0 END as [TriggeredRiseFirst],
		   CASE WHEN [FiveDayOutcomeType] = -1 THEN 1 ELSE 0 END as [TriggeredFallFirst]
	FROM @fiveDayPerformance fdp 
		INNER JOIN @companyQuotes quotes ON quotes.[QuoteId] = fdp.[QuoteId]
	WHERE [Date] >= @startDate AND [Date] <= @endDate
	ORDER BY [Date]

	RETURN;
END
GO