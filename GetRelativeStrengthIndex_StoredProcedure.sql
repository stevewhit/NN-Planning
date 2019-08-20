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

	/* Temp table to hold RSI change calculations for the @rsiPeriod*/
	IF OBJECT_ID('tempdb..#rsiChangeCalculations') IS NOT NULL DROP TABLE #rsiChangeCalculations
	CREATE TABLE #rsiChangeCalculations
	(
		[QuoteId] INT, 
		[CompanyId] INT, 
		[Date] DATE, 
		[Close] DECIMAL(12, 4), 
		[Change] DECIMAL(12, 4)
	);

	/* Calculate the change values for each date */
	INSERT INTO #rsiChangeCalculations 
	      --([QuoteId], [CompanyId], [Date], [Close], [Change])
	SELECT [QuoteId], 
	       [CompanyId], 
	       [Date], 
	       [Close], 
	       [Close] - LAG([Close], 1) OVER (ORDER BY Date) as Change
	FROM StockMarketData.dbo.Quotes
	WHERE CompanyId = @companyId
	ORDER BY Date

	/* Temp table to hold RSI gain/loss calculations for the @rsiPeriod*/
	IF OBJECT_ID('tempdb..#rsiGainLossCalculations') IS NOT NULL DROP TABLE #rsiGainLossCalculations
	CREATE TABLE #rsiGainLossCalculations
	(
		[QuoteId] INT, 
		[CurrentGain] DECIMAL(12, 4), 
		[AverageGain] DECIMAL(12, 4), 
		[CurrentLoss] DECIMAL(12, 4), 
		[AverageLoss] DECIMAL(12, 4)
	);
	
	/* Calculate the average gain/loss for each date */
	DECLARE @sql nvarchar(max) = 
		N'INSERT INTO #rsiGainLossCalculations
		  SELECT [QuoteId],
		  	 CASE WHEN [Change] > 0 THEN [Change] ELSE 0 END as [CurrentGain],
		  	 AVG(CASE WHEN [Change] > 0 THEN [Change] ELSE 0 END ) OVER (ORDER BY [Date] ROWS ' + convert(varchar, @rsiPeriod) +' PRECEDING) as [AverageGain], 
		  	 CASE WHEN [Change] < 0 THEN ABS([Change]) ELSE 0 END  as [CurrentLoss],
		  	 ABS(AVG(CASE WHEN [Change] < 0 THEN [Change] ELSE 0 END ) OVER (ORDER BY [Date] ROWS ' + convert(varchar, @rsiPeriod) +' PRECEDING)) as [AverageLoss]
		  FROM #rsiChangeCalculations
		  ORDER BY [Date]'
	EXEC sp_executesql @sql

	/* Holds the RS values */
	DECLARE @rsiRSCalculations TABLE 
	(
		[QuoteId] INT,  
		[RS] DECIMAL(12, 4)
	)

	INSERT INTO @rsiRSCalculations
	SELECT [QuoteId] 
	       CASE WHEN AverageLoss = 0 AND LAG([AverageLoss], 1) OVER (ORDER BY Date) = 0 
	       	    THEN 100000.00 
		    ELSE (((LAG([AverageGain], 1) OVER (ORDER BY Date) * (@rsiPeriod - 1)) + [CurrentGain]) / @rsiPeriod) / (((LAG([AverageLoss], 1) OVER (ORDER BY Date) * (@rsiPeriod - 1)) + [CurrentLoss]) / @rsiPeriod) end as [RS]
	FROM #rsiGainLossCalculations

	/* Drop temp tables before finishing /*
	DROP TABLE #rsiChangeCalculations
	DROP TABLE #rsiGainLossCalculations

	/* Calculate RSI values */
	SELECT [QuoteId], 
	       [CompanyId], 
	       [Date], 
	       [Close], 
	       [Change], 
	       [CurrentGain], 
	       [SumGain], 
	       [AverageGain], 
	       [PreviousAverageGain], 
	       [CurrentLoss], 
	       [SumLoss], 
	       [AverageLoss], 
	       [PreviousAverageLoss], 
	       [RS],
	       100.00 - (100.00 / (1.00 + RS)) as RSI
	FROM @rsiRSCalculations
	WHERE date >= @startDate AND date <= @endDate
END
GO


