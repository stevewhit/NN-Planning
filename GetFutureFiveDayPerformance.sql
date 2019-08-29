USE [SANNET]
GO

/****** Object:  StoredProcedure [dbo].[GetFutureFiveDayPerformance]    Script Date: 8/28/2019 10:26:55 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		Steve Whitmire Jr.
-- Create date: 08-28-2019
-- Description:	Returns the five day performance for a given company for the designated @date. 
--				The returned items will indicate whether the stock rose to the @riseMultiplierTrigger  
--				before falling to the @fallMultiplierTrigger and vice versa.
-- =============================================
CREATE PROCEDURE [dbo].[GetFutureFiveDayPerformance]
	@companyId int,
	@date date,
	@riseMultiplierTrigger decimal(7, 3) = 1.01,
	@fallMultiplierTrigger decimal(7, 3) = 1.01
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.
	SET NOCOUNT ON;

	-- End date is 5 quote-dates after the start date.
	DECLARE @endDate DATE = 
		(SELECT TOP 1 LEAD([Date], 5) OVER (ORDER BY [Date]) 
		 FROM StockMarketData.dbo.Quotes 
		 WHERE [CompanyId] = @companyId AND [Date] >= @date
		 ORDER BY [Date])

	/*********************************************************************************************
		Table to hold the performance outputs starting from the @startDate 
	*********************************************************************************************/
	DECLARE @fiveDayPerformance TABLE
	(
		[QuoteId] INT,
		[CompanyId] INT,
		[Date] DATE,
		[FiveDayOutcomeType] INT
	)

	INSERT INTO @fiveDayPerformance
	SELECT [Id] as [QuoteId],
		   [CompanyId],
		   [Date],
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
	WHERE [CompanyId] = @companyId AND [Date] >= @date
	ORDER BY [Date]
	
	/*********************************************************************************************
		Return dataset
	*********************************************************************************************/
	SELECT [QuoteId],
		   [CompanyId],
		   [Date],
		   CASE WHEN [FiveDayOutcomeType] = 1 THEN 1 ELSE 0 END as [TriggeredRiseFirst],
		   CASE WHEN [FiveDayOutcomeType] = -1 THEN 1 ELSE 0 END as [TriggeredFallFirst]
	FROM @fiveDayPerformance
	WHERE [CompanyId] = @companyId-- AND [Date] = @date--[Date] >= @date AND [Date] <= @endDate
	ORDER BY [Date]
END
GO


