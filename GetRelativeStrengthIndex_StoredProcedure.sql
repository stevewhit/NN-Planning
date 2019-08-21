SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Steve Whitmire Jr.
-- Create date: 08-19-2019
-- Description:	Returns the Relative Strength Index (RSI) calculations for a given company.
-- =============================================
CREATE PROCEDURE [dbo].[GetRelativeStrengthIndex] 
	@companyId int = 0, 
	@startDate date = null,
	@endDate date = null,
	@rsiPeriod int = 1
AS
BEGIN
	SET NOCOUNT ON;

	/*********************************************************************************************
		Temp table to hold RSI change calculations for the @rsiPeriod.
		---
		NOTE: A temp table is used here because the next dynamic sql statement (#rsiGainLossCalculations)
		requires access to a table within its scope.
	*********************************************************************************************/
	IF OBJECT_ID('tempdb..#rsiChangeCalculations') IS NOT NULL DROP TABLE #rsiChangeCalculations
	CREATE TABLE #rsiChangeCalculations
	(
		[QuoteId] INT, 
		[CompanyId] INT, 
		[Date] DATE, 
		[Close] DECIMAL(12, 4), 
		[Change] DECIMAL(12, 4)
	);

	/* Calculate the change values for each quote */
	INSERT INTO #rsiChangeCalculations 
	SELECT [Id] as [QuoteId], 
	       [CompanyId], 
	       [Date], 
	       [Close], 
	       [Close] - LAG([Close], 1) OVER (ORDER BY [Id]) as [Change]
	FROM StockMarketData.dbo.Quotes
	WHERE [CompanyId] = @companyId
	ORDER BY [QuoteId]

	/*********************************************************************************************
		Temp table to hold RSI gain/loss calculations for the @rsiPeriod
		---
		NOTE: A temp table is used here because the query has to be dynamic in order
		to select @rsiPeriod previous rows.
	*********************************************************************************************/
	IF OBJECT_ID('tempdb..#rsiGainLossCalculations') IS NOT NULL DROP TABLE #rsiGainLossCalculations
	CREATE TABLE #rsiGainLossCalculations
	(
		[QuoteId] INT, 
		[CurrentGain] DECIMAL(12, 4), 
		[AverageGain] DECIMAL(12, 4), 
		[CurrentLoss] DECIMAL(12, 4), 
		[AverageLoss] DECIMAL(12, 4)
	);
	
	/* Calculate the average gain/loss over the @rsiPeriod for each quote */
	DECLARE @sql nvarchar(max) = 
		N'INSERT INTO #rsiGainLossCalculations
		  SELECT [QuoteId],
		  	 CASE WHEN [Change] > 0 THEN [Change] ELSE 0 END as [CurrentGain],
		  	 AVG(CASE WHEN [Change] > 0 THEN [Change] ELSE 0 END ) OVER (ORDER BY [QuoteId] ROWS ' + convert(varchar, @rsiPeriod) +' PRECEDING) as [AverageGain], 
		  	 CASE WHEN [Change] < 0 THEN ABS([Change]) ELSE 0 END  as [CurrentLoss],
		  	 AVG(ABS(CASE WHEN [Change] < 0 THEN [Change] ELSE 0 END )) OVER (ORDER BY [QuoteId] ROWS ' + convert(varchar, @rsiPeriod) +' PRECEDING) as [AverageLoss]
		  FROM #rsiChangeCalculations
		  ORDER BY [QuoteId]'
	EXEC sp_executesql @sql

	/*********************************************************************************************
		Table to hold the RS values over the @rsiPeriod for each quote. 
		---
		NOTE: When AverageLoss and PreviousAverageLoss both equal 0, RSI by default goes to 100. 
	*********************************************************************************************/
	DECLARE @rsiRSCalculations TABLE 
	(
		[QuoteId] INT,
		[RS] DECIMAL(12, 4)
	)

	INSERT INTO @rsiRSCalculations
	SELECT [QuoteId], 
	       CASE WHEN AverageLoss = 0 AND LAG([AverageLoss], 1) OVER (ORDER BY [QuoteId]) = 0 
	       	    THEN 100000.00 
				ELSE (((LAG([AverageGain], 1) OVER (ORDER BY [QuoteId]) * (@rsiPeriod - 1)) + [CurrentGain]) / @rsiPeriod) / (((LAG([AverageLoss], 1) OVER (ORDER BY [QuoteId]) * (@rsiPeriod - 1)) + [CurrentLoss]) / @rsiPeriod) end as [RS]
	FROM #rsiGainLossCalculations

	/*********************************************************************************************
		Table to hold the RSI values over the @rsiPeriod for each quote. 
	*********************************************************************************************/
	SELECT rsCalcs.QuoteId as [QuoteId], 
	       [CompanyId], 
	       [Date], 
	       --[Close], 
	       --[Change], 
	       --[CurrentGain], 
	       --[AverageGain], 
		   --(LAG([AverageGain], 1) OVER (ORDER BY [QuoteId]) * (@rsiPeriod - 1)) as [PreviousAverageGain],
	       --[CurrentLoss], 
	       --[AverageLoss], 
		   --(LAG([AverageLoss], 1) OVER (ORDER BY [QuoteId]) * (@rsiPeriod - 1)) as [PreviousAverageLoss],
	       --[RS],
	       100.00 - (100.00 / (1.00 + RS)) as RSI
	FROM @rsiRSCalculations rsCalcs
		INNER JOIN #rsiChangeCalculations changeCalcs ON rsCalcs.QuoteId = changeCalcs.QuoteId
		INNER JOIN #rsiGainLossCalculations gainLossCalcs on rsCalcs.QuoteId = gainLossCalcs.QuoteId
	WHERE [Date] >= @startDate AND [Date] <= @endDate

	/* Drop temp tables before finishing */
	DROP TABLE #rsiChangeCalculations
	DROP TABLE #rsiGainLossCalculations
END


