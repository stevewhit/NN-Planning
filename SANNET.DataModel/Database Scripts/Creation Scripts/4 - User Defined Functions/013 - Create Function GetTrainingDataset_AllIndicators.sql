SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'TF' and name = 'GetDataset_AllIndicators')
BEGIN
	PRINT 'Dropping "GetDataset_AllIndicators" function...'
	DROP FUNCTION GetDataset_AllIndicators
END
GO

PRINT 'Creating "GetDataset_AllIndicators" function...'
GO

-- ==========================================================================================================
-- Author:		Steve Whitmire Jr.
-- Create date: 09-21-2019
-- Description:	Returns the training & testing dataset for the @quoteId, containing all technical indicator signals
-- ==========================================================================================================
CREATE FUNCTION [dbo].[GetDataset_AllIndicators] 
(
	@quoteId int
)
RETURNS @datasetValues TABLE
(
	[QuoteId] INT UNIQUE,
	[I_IsMACDAboveZeroLine] BIT NOT NULL, 
	[I_IsStochasticOverBought] BIT NOT NULL, 
	[I_IsStochasticOverSold] BIT NOT NULL, 
	[I_IsStochasticNeitherOverBoughtOrOverSold] BIT NOT NULL, 
	[I_IsRSIOverBought] BIT NOT NULL,
	[I_IsRSIOverSold] BIT NOT NULL,
	[I_IsRSINeitherOverBoughtOrOverSold] BIT NOT NULL, 
	[I_IsCCIOverBought] BIT NOT NULL,
	[I_IsCCIOverSold] BIT NOT NULL,
	[I_IsCCINeitherOverBoughtOrOverSold] BIT NOT NULL, 
	[I_IsSMAShortGreaterThanLong] BIT NOT NULL,
	[I_IsEMAShortGreaterThanLong] BIT NOT NULL,
	[O_TriggeredRiseFirst] BIT NOT NULL, 
	[O_TriggeredFallFirst] BIT NOT NULL
)
AS
BEGIN
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
			@minCloseTrendSlope DECIMAL(9, 4) = 0.05,
			@performanceRiseMultiplier DECIMAL(8, 4) = 1.02,
			@performanceFallMultiplier DECIMAL(8, 4) = .99

	DECLARE	@quotesToSkipAtStart INT = (SELECT MAX(v) FROM (VALUES (@macdPeriodLong + @macdSignalPeriod - 1), (@stochasticPeriod + @stochasticSMAPeriod - 1), (@closeTrendSlopePeriod)) as VALUE(v)),
			@quotesToSkipAtEnd INT = 5,
			@quoteDate DATE,
			@returnDatasetStartDate DATE,
			@returnDatasetEndDate DATE,
			@oldestDateToReturnDatasetFor DATE,
			@companyId INT = (SELECT [CompanyId] FROM StockMarketData.dbo.Quotes WHERE [Id] = @quoteId)

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
	SET @oldestDateToReturnDatasetFor = DATEADD(M, -6, GETDATE())
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
	
	/****************************************************************************************************
		All combined indicators
		---
		Note: This table is only populated when the @quoteId close trend slope is > @minCloseTrendSlope
	*****************************************************************************************************/
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
	[EMAShort] DECIMAL(9, 4),
	[EMALong] DECIMAL(9, 4),
	[CloseTrendSlope] DECIMAL(9, 4),
	[TriggeredRiseFirst] BIT NOT NULL, 
	[TriggeredFallFirst] BIT NOT NULL);

	-- Only return a populated dataset if the current quote closeTrendSlope is > @minCloseTrendSlope 
	-- and if the @quoteDate is > @oldestDateToReturnDatasetFor
	IF @quoteDate > @oldestDateToReturnDatasetFor AND (SELECT [CloseTrendSlope] FROM @closeTrendSlopes WHERE [QuoteId] = @quoteId) >= @minCloseTrendSlope
	BEGIN
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
				[EMAShort],
				[EMALong],
				[CloseTrendSlope],
				[TriggeredRiseFirst],
				[TriggeredFallFirst]
		FROM (SELECT [QuoteId], [MACD], [MACDSignal], [MACDHistogram] FROM GetMovingAverageConvergenceDivergenceValues(@companyId, @returnDatasetStartDate, @returnDatasetEndDate, @macdPeriodShort, @macdPeriodLong, @macdSignalPeriod)) macdValues
				INNER JOIN (SELECT [QuoteId], [StochasticK], [StochasticD] FROM GetStochasticIndicatorValues(@companyId, @returnDatasetStartDate, @returnDatasetEndDate, @stochasticPeriod, @stochasticSMAPeriod)) stochasticValues ON macdValues.[QuoteId] = stochasticValues.[QuoteId]
				INNER JOIN (SELECT [QuoteId], [RSI] FROM GetRelativeStrengthIndexValues(@companyId, @returnDatasetStartDate, @returnDatasetEndDate, @rsiPeriod)) rsiValues ON macdValues.[QuoteId] = rsiValues.[QuoteId]
				INNER JOIN (SELECT [QuoteId], [CCI] FROM GetCommodityChannelIndexValues(@companyId, @returnDatasetStartDate, @returnDatasetEndDate, @cciPeriod)) cciValues ON macdValues.[QuoteId] = cciValues.[QuoteId]
				INNER JOIN (SELECT [QuoteId], [SMA] as [SMAShort] FROM GetSimpleMovingAverageValues(@companyId, @returnDatasetStartDate, @returnDatasetEndDate, @smaPeriodShort)) smaShortValues ON macdValues.[QuoteId] = smaShortValues.[QuoteId]
				INNER JOIN (SELECT [QuoteId], [SMA] as [SMALong] FROM GetSimpleMovingAverageValues(@companyId, @returnDatasetStartDate, @returnDatasetEndDate, @smaPeriodLong)) smaLongValues ON macdValues.[QuoteId] = smaLongValues.[QuoteId]
				INNER JOIN (SELECT [QuoteId], [EMA] as [EMAShort] FROM GetExponentialMovingAverageValues(@companyId, @returnDatasetStartDate, @returnDatasetEndDate, @emaPeriod)) emaShortValues ON macdValues.[QuoteId] = emaShortValues.[QuoteId]
				INNER JOIN (SELECT [QuoteId], [EMA] as [EMALong] FROM GetExponentialMovingAverageValues(@companyId, @returnDatasetStartDate, @returnDatasetEndDate, @emaPeriod)) emaLongValues ON macdValues.[QuoteId] = emaLongValues.[QuoteId]
				INNER JOIN @closeTrendSlopes closeTrendSlopeValues ON macdValues.[QuoteId] = closeTrendSlopeValues.[QuoteId]
				INNER JOIN (SELECT [QuoteId], [TriggeredRiseFirst], [TriggeredFallFirst] FROM GetFutureFiveDayPerformanceValues(@companyId, @returnDatasetStartDate, @returnDatasetEndDate, @performanceRiseMultiplier, @performanceFallMultiplier)) fiveDayPerformance ON macdValues.[QuoteId] = fiveDayPerformance.[QuoteId]
		WHERE [CloseTrendSlope] >= @minCloseTrendSlope
	END 
	
	/***********************************
		Return dataset with binary values
	************************************/
	INSERT INTO @datasetValues
	SELECT quotes.[QuoteId],

		   -- MACD
		   CASE WHEN [MACD] > 0.0 THEN 1 ELSE 0 END [I_IsMACDAboveZeroLine],

		   -- Stochastic
		   CASE WHEN [StochasticK] >= 70 THEN 1 ELSE 0 END [I_IsStochasticOverBought],
		   CASE WHEN [StochasticK] <= 30 THEN 1 ELSE 0 END [I_IsStochasticOverSold],
		   CASE WHEN [StochasticK] < 70 AND [StochasticK] > 30 THEN 1 ELSE 0 END [I_IsStochasticNeitherOverBoughtOrOverSold],

		   -- RSI
		   CASE WHEN [RSI] >= 70 THEN 1 ELSE 0 END [I_IsRSIOverBought],
		   CASE WHEN [RSI] <= 30 THEN 1 ELSE 0 END [I_IsRSIOverSold],
		   CASE WHEN [RSI] < 70 AND [RSI] > 30 THEN 1 ELSE 0 END [I_IsRSINeitherOverBoughtOrOverSold],

		   -- CCI
		   CASE WHEN [CCI] >= 70 THEN 1 ELSE 0 END [I_IsCCIOverBought],
		   CASE WHEN [CCI] <= 30 THEN 1 ELSE 0 END [I_IsCCIOverSold],
		   CASE WHEN [CCI] < 70 AND [CCI] > 30 THEN 1 ELSE 0 END [I_IsCCINeitherOverBoughtOrOverSold],

		   -- SMA
		   CASE WHEN [SMAShort] > [SMALong] THEN 1 ELSE 0 END [I_IsSMAShortGreaterThanLong],

		   -- EMA
		   CASE WHEN [EMAShort] > [EMALong] THEN 1 ELSE 0 END [I_IsEMAShortGreaterThanLong],

			-- Outputs
		   CASE WHEN [TriggeredRiseFirst] = 1 THEN 1 ELSE 0 END [O_TriggeredRiseFirst],
		   CASE WHEN [TriggeredFallFirst] = 1 THEN 1 ELSE 0 END [O_TriggeredFallFirst]
	FROM @companyQuotes quotes INNER JOIN @combinedIndicatorValues indicatorValues ON quotes.[QuoteId] = indicatorValues.[QuoteId]
	WHERE [Date] >= @returnDatasetStartDate AND [Date] <= @returnDatasetEndDate
	ORDER BY [Date]

	RETURN;
END
GO