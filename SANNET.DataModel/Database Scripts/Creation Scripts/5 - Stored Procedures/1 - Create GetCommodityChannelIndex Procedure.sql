SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' and name = 'GetCommodityChannelIndex')
BEGIN
	PRINT 'Dropping "GetCommodityChannelIndex" stored procedure...'
	DROP PROCEDURE GetCommodityChannelIndex
END
GO

PRINT 'Creating "GetCommodityChannelIndex" stored procedure...'
GO

-- =============================================
-- Author:		Steve Whitmire Jr.
-- Create date: 08-20-2019
-- Description:	Returns the Commodity Channel Index (CCI) calculations for a given company.
-- =============================================
CREATE PROCEDURE [dbo].[GetCommodityChannelIndex] 
	@companyId int,
	@startDate date,
	@endDate date,
	@cciPeriod int
AS
BEGIN
	SET NOCOUNT ON;

	/*********************************************************************************************
		Temp table to hold CCI typical price calculations for the @cciPeriod.
		---
		NOTE: A temp table is used here because the next dynamic sql statement (#cciMovingAverageCalculations)
		requires access to a table within its scope.
	*********************************************************************************************/
	IF OBJECT_ID('tempdb..#cciTypicalPriceCalculations') IS NOT NULL DROP TABLE #cciTypicalPriceCalculations
	CREATE TABLE #cciTypicalPriceCalculations
	(
		[QuoteId] INT,
		[CompanyId] INT,
		[Date] DATE,
		[TypicalPrice] DECIMAL(12, 4)
	);

	INSERT INTO #cciTypicalPriceCalculations
	SELECT [Id] as [QuoteId], 
		   [CompanyId],
		   [Date],	
	      ([High] + [Low] + [Close]) / 3.0 as [TypicalPrice]
	FROM StockMarketData.dbo.Quotes
	WHERE [CompanyId] = @companyId
	ORDER BY [Date]

	/*********************************************************************************************
		Temp table to hold CCI moving average calculations for the @cciPeriod.
		---
		NOTE: A temp table is used here because the query has to be dynamic in order
		to select @cciPeriod previous rows.
	*********************************************************************************************/
	IF OBJECT_ID('tempdb..#cciMovingAverageCalculations') IS NOT NULL DROP TABLE #cciMovingAverageCalculations
	CREATE TABLE #cciMovingAverageCalculations
	(
		[QuoteId] INT,
		[Date] DATE,
		[TypicalPrice] DECIMAL(12, 4),
		[MovingAverage] DECIMAL(12, 4)
	);

	/* Calculate the average gain/loss over the @rsiPeriod for each quote */
	DECLARE @sql nvarchar(max) = 
		N'INSERT INTO #cciMovingAverageCalculations
		  SELECT [QuoteId],
				 [Date],
				 [TypicalPrice],
				 AVG([TypicalPrice]) OVER (ORDER BY [QuoteId] ROWS ' + convert(varchar, @cciPeriod) +' PRECEDING) as [MovingAverage]
		  FROM #cciTypicalPriceCalculations
		  ORDER BY [Date]'
	EXEC sp_executesql @sql

	/*********************************************************************************************
		Temp table to hold CCI mean deviation calculations for the @cciPeriod.
		---
		NOTE: A temp table is used here because the query has to be dynamic in order
		to select @cciPeriod previous rows.
	*********************************************************************************************/
	IF OBJECT_ID('tempdb..#cciMeanDeviationCalculations') IS NOT NULL DROP TABLE #cciMeanDeviationCalculations
	CREATE TABLE #cciMeanDeviationCalculations
	(
		[QuoteId] INT,
		[Date] DATE,
		[MeanDeviation] DECIMAL(12, 4)
	);

	/* Calculate the average gain/loss over the @rsiPeriod for each quote */
	SET @sql = 
		N'INSERT INTO #cciMeanDeviationCalculations
		  SELECT [QuoteId],
				 [Date],
				 AVG(ABS([TypicalPrice] - [MovingAverage])) OVER (ORDER BY [QuoteId] ROWS ' + convert(varchar, @cciPeriod) +' PRECEDING) as [MeanDeviation]
		  FROM #cciMovingAverageCalculations
		  ORDER BY [Date]'
	EXEC sp_executesql @sql

	/*********************************************************************************************
		Table to hold the CCI values over the @cciPeriod for each quote. 
	*********************************************************************************************/
	SELECT typicalPriceCalcs.QuoteId as [QuoteId], 
	       typicalPriceCalcs.CompanyId as [CompanyId], 
	       typicalPriceCalcs.Date as [Date],
		   --[High],[Low],[Close],
		   --typicalPriceCalcs.TypicalPrice as [TypicalPrice],
		   --movingAvgCalcs.MovingAverage as [MovingAverage],
		   --meanDeviationCalcs.MeanDeviation as [MeanDeviation],
		   CASE WHEN meanDeviationCalcs.MeanDeviation = 0
			    THEN 10111.00
				ELSE (typicalPriceCalcs.TypicalPrice - movingAvgCalcs.MovingAverage) / (.015 * meanDeviationCalcs.MeanDeviation)
				END as CCI
	FROM #cciTypicalPriceCalculations typicalPriceCalcs
		INNER JOIN #cciMovingAverageCalculations movingAvgCalcs ON typicalPriceCalcs.QuoteId = movingAvgCalcs.QuoteId
		INNER JOIN #cciMeanDeviationCalculations meanDeviationCalcs on typicalPriceCalcs.QuoteId = meanDeviationCalcs.QuoteId
		--INNER JOIN StockMarketData.dbo.Quotes quotes on quotes.Id = typicalPriceCalcs.QuoteId
	WHERE typicalPriceCalcs.[Date] >= @startDate AND typicalPriceCalcs.[Date] <= @endDate
	ORDER BY typicalPriceCalcs.[Date]

	/* Drop temp tables before finishing */
	DROP TABLE #cciTypicalPriceCalculations
	DROP TABLE #cciMovingAverageCalculations
	DROP TABLE #cciMeanDeviationCalculations
END
GO


