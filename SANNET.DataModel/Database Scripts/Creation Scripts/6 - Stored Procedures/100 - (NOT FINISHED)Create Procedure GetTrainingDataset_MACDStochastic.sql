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
			@performanceRiseMultiplier DECIMAL(8, 4) = 1.04,
			@performanceFallMultiplier DECIMAL(8, 4) = .98

	DECLARE	@quotesToSkipAtStart INT = (SELECT MAX(v) FROM (VALUES (@macdPeriodLong + @macdSignalPeriod - 1), (@stochasticPeriod + @stochasticSMAPeriod - 1)) as VALUE(v)),
			@quotesToSkipAtEnd INT = 5,
			@startDate DATE,
			@endDate DATE

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
	SELECT [Id], [CompanyQuoteNum], [Date], [CompanyId] FROM GetCompanyQuotes(@companyId)

	/****************************************************************
		Update start and end dates for the indicator calculations
	*****************************************************************/
	SET @startDate = (SELECT MIN([Date]) FROM @companyQuotes WHERE [QuoteId] <= @quoteId AND [CompanyQuoteNum] >= @quotesToSkipAtStart)
	SET @endDate = (SELECT MAX([Date]) FROM @companyQuotes WHERE [QuoteId] <= @quoteId AND [CompanyQuoteNum] <= (SELECT MAX([CompanyQuoteNum]) FROM @companyQuotes) - @quotesToSkipAtEnd)

	--/********************************
	--	MACD table
	--*********************************/
	--DECLARE @macdValues TABLE ([QuoteId] INT, [CompanyId] INT, [Date] DATE, [MACD] DECIMAL(12, 2), [MACDSignal] DECIMAL(12, 2), [MACDHistogram] DECIMAL(12, 2));
	--INSERT INTO @macdValues 
	--EXECUTE GetMovingAverageConvergenceDivergence @companyId, @startDate, @endDate, @macdPeriodShort, @macdPeriodLong, @macdSignalPeriod
	
	--/***********************************
	--	Stochastic table
	--************************************/
	--DECLARE @stochasticValues TABLE ([QuoteId] INT, [CompanyId] INT, [Date] DATE, [StochasticK] DECIMAL(12, 2), [StochasticD] DECIMAL(12, 2));
	--INSERT INTO @stochasticValues 
	--EXECUTE GetStochasticIndicator @companyId, @startDate, @endDate, @stochasticPeriod, @stochasticSMAPeriod 

	--/**********************************
	--	Future Five Day Performance
	--***********************************/
	--DECLARE @futureFiveDayPerformance TABLE ([QuoteId] INT, [CompanyId] INT, [Date] DATE, [TriggeredRiseFirst] BIT, [TriggeredFallFirst] BIT);
	--INSERT INTO @futureFiveDayPerformance 
	--EXECUTE GetFutureFiveDayPerformance @companyId, @startDate, @endDate, @performanceRiseMultiplier, @performanceFallMultiplier

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
	[TriggeredRiseFirst] BIT, 
	[TriggeredFallFirst] BIT);

	INSERT INTO @combinedIndicatorValues
	SELECT  macdValues.[QuoteId],
			[MACD],
			[MACDSignal],
			[MACDHistogram],
			[StochasticK],
			[StochasticD],
			[TriggeredRiseFirst],
			[TriggeredFallFirst]
	FROM (SELECT [QuoteId], [MACD], [MACDSignal], [MACDHistogram] FROM GetMovingAverageConvergenceDivergenceValues(@companyId, @startDate, @endDate, @macdPeriodShort, @macdPeriodLong, @macdSignalPeriod)) macdValues
			INNER JOIN (SELECT [QuoteId], [StochasticK], [StochasticD] FROM GetStochasticIndicatorValues(@companyId, @startDate, @endDate, @stochasticPeriod, @stochasticSMAPeriod)) stochasticValues ON macdValues.[QuoteId] = stochasticValues.[QuoteId]
			INNER JOIN (SELECT [QuoteId], [TriggeredRiseFirst], [TriggeredFallFirst] FROM GetFutureFiveDayPerformanceValues(@companyId, @startDate, @endDate, @performanceRiseMultiplier, @performanceFallMultiplier)) fiveDayPerformance ON macdValues.[QuoteId] = fiveDayPerformance.[QuoteId]

	SELECT * FROM @combinedIndicatorValues

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