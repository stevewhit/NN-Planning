SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'GetTrainingDataset_MACDStochastic')
BEGIN
	PRINT 'Dropping "GetTrainingDataset_MACDStochastic" stored procedure...'
	DROP PROCEDURE GetTrainingDataset_MACDStochastic
END
GO

PRINT 'Creating "GetTrainingDataset_MACDStochastic" stored procedure...'
GO

-- =============================================
-- Author:		Steve Whitmire Jr.
-- Create date: 09-15-2019
-- Description:	Returns the training/testing dataset using the MACD & Stochastic indicators.
-- =============================================
CREATE PROCEDURE [dbo].[GetTrainingDataset_MACDStochastic] 
	@companyId int,
	@quoteId int
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @macdPeriodShort INT = 12,
			@macdPeriodLong INT = 26,
			@macdSignalPeriod INT = 9,
			@stochasticPeriod INT = 14,
			@stochasticSMAPeriod INT = 3,
			@rsiPeriod INT = 14,
			@cciPeriod INT = 14,
			@smaPeriodShort INT = 14,
			@smaPeriodLong INT = 50,
			@emaPeriod INT = 14,
			@closeTrendSlopePeriod INT = 60,
			@minCloseTrendSlope DECIMAL(9, 4) = 2.00,
			@performanceRiseMultiplier DECIMAL(8, 4) = 1.04,
			@performanceFallMultiplier DECIMAL(8, 4) = .98

	DECLARE	@quotesToSkipAtStart INT = (SELECT MAX(v) FROM (VALUES (@macdPeriodLong + @macdSignalPeriod - 1), (@stochasticPeriod + @stochasticSMAPeriod - 1), (@closeTrendSlopePeriod)) as VALUE(v)),
			@quotesToSkipAtEnd INT = 5,
			@quoteDate DATE,
			@returnDatasetStartDate DATE,
			@returnDatasetEndDate DATE

	/*********************************************
		Table to hold numbered quotes.
	**********************************************/
	DECLARE @companyQuotes TABLE
	(
		[QuoteId] INT UNIQUE,
		[CompanyQuoteNum] INT UNIQUE,
        [Date] DATE UNIQUE,
		[CompanyId] INT
	)

	INSERT INTO @companyQuotes
	SELECT [QuoteId], [CompanyQuoteNum], [Date], [CompanyId] 
	FROM GetCompanyQuotes(@companyId)
	ORDER BY [CompanyQuoteNum]

	/****************************************************************
		Update start and end dates for the indicator calculations
	*****************************************************************/
	SET @quoteDate = (SELECT [Date] FROM @companyQuotes WHERE [QuoteId] = @quoteId)
	SET @returnDatasetStartDate = (SELECT MIN([Date]) FROM @companyQuotes WHERE [Date] <= @quoteDate AND [CompanyQuoteNum] >= @quotesToSkipAtStart)
	SET @returnDatasetEndDate = (SELECT MAX([Date]) FROM @companyQuotes WHERE [Date] <= @quoteDate AND [CompanyQuoteNum] <= (SELECT MAX([CompanyQuoteNum]) FROM @companyQuotes) - @quotesToSkipAtEnd)

	/************************************************************************
		Table to hold the trend-slope values of the [Close] for each quote
		--
		Note: Calculated early so that the procedure can exit immediately
			  if the current-date slope is not > @minCloseTrendSlope
	*************************************************************************/
	DECLARE @closeTrendSlopes TABLE
	(
		[QuoteId] INT,
		[CloseTrendSlope] DECIMAL(9, 4)
	)

	INSERT INTO @closeTrendSlopes
	SELECT [QuoteId], [TrendSlope] as [CloseTrendSlope]
	FROM GetCloseTrendLineSlopeValues(@companyId, @returnDatasetStartDate, @returnDatasetEndDate, @closeTrendSlopePeriod)
	
	-- Only return a populated dataset if the current quote closeTrendSlope is > @minCloseTrendSlope --
	IF (SELECT [CloseTrendSlope] FROM @closeTrendSlopes WHERE [QuoteId] = @quoteId) > @minCloseTrendSlope
	BEGIN
		/***************************
			All combined indicators
		****************************/
		DECLARE @combinedIndicatorValues TABLE(
		[QuoteId] INT UNIQUE,
		[MACD] DECIMAL(9, 4), 
		[MACDSignal] DECIMAL(9, 4), 
		[MACDHistogram] DECIMAL(9, 4), 
		[StochasticK] DECIMAL(9, 4), 
		[StochasticD] DECIMAL(9, 4), 
		[RSI] DECIMAL(9, 4),
		[CCI] DECIMAL(9, 3),
		[SMAShort] DECIMAL(9, 4),
		[SMALong] DECIMAL(9, 4),
		[EMA] DECIMAL(9, 4),
		[CloseTrendSlope] DECIMAL(9, 4),
		[TriggeredRiseFirst] BIT, 
		[TriggeredFallFirst] BIT);

		INSERT INTO @combinedIndicatorValues
		SELECT  macdValues.[QuoteId],
				[MACD],
				[MACDSignal],
				[MACDHistogram],
				[StochasticK],
				[StochasticD],
				[RSI],
				[CCI],
				[SMAShort],
				[SMALong],
				[EMA],
				[CloseTrendSlope],
				[TriggeredRiseFirst],
				[TriggeredFallFirst]
		FROM (SELECT [QuoteId], [MACD], [MACDSignal], [MACDHistogram] FROM GetMovingAverageConvergenceDivergenceValues(@companyId, @returnDatasetStartDate, @returnDatasetEndDate, @macdPeriodShort, @macdPeriodLong, @macdSignalPeriod)) macdValues
				INNER JOIN (SELECT [QuoteId], [StochasticK], [StochasticD] FROM GetStochasticIndicatorValues(@companyId, @returnDatasetStartDate, @returnDatasetEndDate, @stochasticPeriod, @stochasticSMAPeriod)) stochasticValues ON macdValues.[QuoteId] = stochasticValues.[QuoteId]
				INNER JOIN (SELECT [QuoteId], [RSI] FROM GetRelativeStrengthIndexValues(@companyId, @returnDatasetStartDate, @returnDatasetEndDate, @rsiPeriod)) rsiValues ON macdValues.[QuoteId] = rsiValues.[QuoteId]
				INNER JOIN (SELECT [QuoteId], [CCI] FROM GetCommodityChannelIndexValues(@companyId, @returnDatasetStartDate, @returnDatasetEndDate, @cciPeriod)) cciValues ON macdValues.[QuoteId] = cciValues.[QuoteId]
				INNER JOIN (SELECT [QuoteId], [SMA] as [SMAShort] FROM GetSimpleMovingAverageValues(@companyId, @returnDatasetStartDate, @returnDatasetEndDate, @smaPeriodShort)) smaShortValues ON macdValues.[QuoteId] = smaShortValues.[QuoteId]
				INNER JOIN (SELECT [QuoteId], [SMA] as [SMALong] FROM GetSimpleMovingAverageValues(@companyId, @returnDatasetStartDate, @returnDatasetEndDate, @smaPeriodLong)) smaLongValues ON macdValues.[QuoteId] = smaLongValues.[QuoteId]
				INNER JOIN (SELECT [QuoteId], [EMA] FROM GetExponentialMovingAverageValues(@companyId, @returnDatasetStartDate, @returnDatasetEndDate, @emaPeriod)) emaValues ON macdValues.[QuoteId] = emaValues.[QuoteId]
				INNER JOIN @closeTrendSlopes closeTrendSlopeValues ON macdValues.[QuoteId] = closeTrendSlopeValues.[QuoteId]
				INNER JOIN (SELECT [QuoteId], [TriggeredRiseFirst], [TriggeredFallFirst] FROM GetFutureFiveDayPerformanceValues(@companyId, @returnDatasetStartDate, @returnDatasetEndDate, @performanceRiseMultiplier, @performanceFallMultiplier)) fiveDayPerformance ON macdValues.[QuoteId] = fiveDayPerformance.[QuoteId]

		SELECT * FROM @combinedIndicatorValues
	END 
	ELSE
	BEGIN
		SELECT '(TODO: Fix this) ===> CloseTrendSlope is not valid for @quoteId'
	END









	/****
		Next Steps
			- Filter @combinedIndicatorValues to only return dataset entries where the overall '[Close]' trendslope over the past 'x' dates, is positive (or greater than 1-2)

				- this requires that the trend-slope be calculated and added to each dataset entry.. hmmm maybe a while loop??
	*****/
















	/***********************************
		Normalized indicator values
	************************************/
	--DECLARE @normalizedIndicatorValues TABLE
	--(
	--	[QuoteId] INT,
	--	[CompanyId] INT,
	--	[Date] DATE, 
	--	[MACDSignalCrossedAboveZeroLine] INT, 
	--	[MACDSignalCrossedBelowZeroLine] INT, 
	--	[IsStochasticOverBought] INT, 
	--	[IsStochasticOverSold] INT, 
	--	[IsStochasticNeitherOverBoughtOrOverSold] INT, 
	--	[Output_TriggeredRiseFirst] INT, 
	--	[Output_TriggeredFallFirst] INT
	--)

	--INSERT INTO @normalizedIndicatorValues
	--SELECT  [QuoteId],
	--		[CompanyId],
	--		[Date],

	--		-- Buy signals
	--		--(not sure this is valid case) CASE WHEN [MACDSignal] >= 0 AND (LAG([MACDSignal], 1) OVER (ORDER BY [QuoteId])) < 0 THEN 1 ELSE 0 END [MACDSignalCrossedAboveZeroLine],
	--		-- SMA(longTerm) crosses over SMA(shortTerm)
	--		-- MACD crosses over zero
	--		-- MACD Crosses over MACDSignalLine
	--		-- RSI Crosses above 50 (when stock is trending up)
			
	--		-- Stochastic
	--		CASE WHEN [StochasticK] >= 70 THEN 1 ELSE 0 END [IsStochasticOverBought],
	--		CASE WHEN [StochasticK] <= 30 THEN 1 ELSE 0 END [IsStochasticOverSold],
	--		CASE WHEN [StochasticK] < 70 AND [StochasticK] > 30 THEN 1 ELSE 0 END [IsStochasticNeitherOverBoughtOrOverSold],

	--		-- Outputs
	--		CASE WHEN [Output_TriggeredRiseFirst] = 1 THEN 1 ELSE 0 END [Output_TriggeredRiseFirst],
	--		CASE WHEN [Output_TriggeredFallFirst] = 1 THEN 1 ELSE 0 END [Output_TriggeredFallFirst]
	--FROM @combinedIndicatorValues

	/******************************************
		Normalized indicator values, filtered
	*******************************************/
	--SELECT  [QuoteId],
	--		[CompanyId],
	--		[Date],

	--		-- MACD
	--		[MACDSignalCrossedAboveZeroLine],
	--		[MACDSignalCrossedBelowZeroLine],

	--		-- Stochastic
	--		[IsStochasticOverBought],
	--		[IsStochasticOverSold],
	--		[IsStochasticNeitherOverBoughtOrOverSold],
			
	--		-- Outputs
	--		[Output_TriggeredRiseFirst],
	--		[Output_TriggeredFallFirst]
	--FROM @normalizedIndicatorValues
	--WHERE ([MACDSignalCrossedAboveZeroLine] = 1 OR [MACDSignalCrossedBelowZeroLine] = 1)
END
GO