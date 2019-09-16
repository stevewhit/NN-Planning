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
		[CompanyQuoteNum] INT UNIQUE,
		[QuoteId] INT UNIQUE,
		[CompanyId] INT,
        [Date] DATE UNIQUE
	)

	INSERT INTO @companyQuotes
	SELECT 
	   (SELECT [RowNum] 
	    FROM (SELECT Id, ROW_NUMBER() OVER(ORDER BY [Date]) as [RowNum] 
		      FROM [StockMarketData].dbo.Quotes quotes 
			  WHERE quotes.CompanyId = quotesOuter.CompanyId) rowNums 
	    WHERE rowNums.Id = quotesOuter.Id) as [CompanyQuoteNum],
		[Id] as [QuoteId], 
		[CompanyId], 
		[Date]
	FROM [StockMarketData].[dbo].[Quotes] quotesOuter
	WHERE [CompanyId] = @companyId
	ORDER BY [Date]

	/****************************************************************
		Update start and end dates for the indicator calculations
	*****************************************************************/
	SET @startDate = (SELECT MIN([Date]) FROM @companyQuotes WHERE [QuoteId] <= @quoteId AND [CompanyQuoteNum] >= @quotesToSkipAtStart)
	SET @endDate = (SELECT MAX([Date]) FROM @companyQuotes WHERE [QuoteId] <= @quoteId AND [CompanyQuoteNum] <= (SELECT MAX([CompanyQuoteNum]) FROM @companyQuotes) - @quotesToSkipAtEnd)

	/********************************
		MACD table
	*********************************/
	DECLARE @macdValues TABLE ([QuoteId] INT, [CompanyId] INT, [Date] DATE, [MACD] DECIMAL(12, 2), [MACDSignal] DECIMAL(12, 2), [MACDHistogram] DECIMAL(12, 2));
	INSERT INTO @macdValues 
	EXECUTE GetMovingAverageConvergenceDivergence @companyId, @startDate, @endDate, @macdPeriodShort, @macdPeriodLong, @macdSignalPeriod
	
	/***********************************
		Stochastic table
	************************************/
	DECLARE @stochasticValues TABLE ([QuoteId] INT, [CompanyId] INT, [Date] DATE, [StochasticK] DECIMAL(12, 2), [StochasticD] DECIMAL(12, 2));
	INSERT INTO @stochasticValues 
	EXECUTE GetStochasticIndicator @companyId, @startDate, @endDate, @stochasticPeriod, @stochasticSMAPeriod 

	/**********************************
		Future Five Day Performance
	***********************************/
	DECLARE @futureFiveDayPerformance TABLE ([QuoteId] INT, [CompanyId] INT, [Date] DATE, [TriggeredRiseFirst] BIT, [TriggeredFallFirst] BIT);
	INSERT INTO @futureFiveDayPerformance 
	EXECUTE GetFutureFiveDayPerformance @companyId, @startDate, @endDate, @performanceRiseMultiplier, @performanceFallMultiplier

	/***************************
		All combined indicators
	****************************/
	DECLARE @combinedIndicatorValues TABLE(
	[QuoteId] INT,
	[CompanyId] INT,
	[Date] DATE, 
	[MACD] DECIMAL(12, 2), 
	[MACDSignal] DECIMAL(12, 2), 
	[MACDHistogram] DECIMAL(12, 2), 
	[StochasticK] DECIMAL(12, 2), 
	[StochasticD] DECIMAL(12, 2), 
	[Output_TriggeredRiseFirst] DECIMAL(12, 2), 
	[Output_TriggeredFallFirst] DECIMAL(12, 2));

	INSERT INTO @combinedIndicatorValues
	SELECT companyQuotes.[QuoteId],
			companyQuotes.[CompanyId],
			companyQuotes.[Date],
			macdValues.[MACD],
			macdValues.[MACDSignal],
			macdValues.[MACDHistogram],
			stochasticValues.[StochasticK],
			stochasticValues.[StochasticD],
			fiveDayPerformance.TriggeredRiseFirst as [Output_TriggeredRiseFirst],
			fiveDayPerformance.TriggeredFallFirst as [Output_TriggeredFallFirst]
	FROM @companyQuotes companyQuotes
			INNER JOIN @macdValues macdValues ON companyQuotes.[QuoteId] = macdValues.[QuoteId]
			INNER JOIN @stochasticValues stochasticValues ON companyQuotes.[QuoteId] = stochasticValues.[QuoteId]
			INNER JOIN @futureFiveDayPerformance fiveDayPerformance ON companyQuotes.QuoteId = fiveDayPerformance.QuoteId

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
	SELECT  [QuoteId],
			[CompanyId],
			[Date],

			-- Buy signals
			--(not sure this is valid case) CASE WHEN [MACDSignal] >= 0 AND (LAG([MACDSignal], 1) OVER (ORDER BY [QuoteId])) < 0 THEN 1 ELSE 0 END [MACDSignalCrossedAboveZeroLine],
			-- SMA(longTerm) crosses over SMA(shortTerm)
			-- MACD crosses over zero
			-- MACD Crosses over MACDSignalLine
			-- RSI Crosses above 50 (when stock is trending up)
			
			-- Stochastic
			CASE WHEN [StochasticK] >= 70 THEN 1 ELSE 0 END [IsStochasticOverBought],
			CASE WHEN [StochasticK] <= 30 THEN 1 ELSE 0 END [IsStochasticOverSold],
			CASE WHEN [StochasticK] < 70 AND [StochasticK] > 30 THEN 1 ELSE 0 END [IsStochasticNeitherOverBoughtOrOverSold],

			-- Outputs
			CASE WHEN [Output_TriggeredRiseFirst] = 1 THEN 1 ELSE 0 END [Output_TriggeredRiseFirst],
			CASE WHEN [Output_TriggeredFallFirst] = 1 THEN 1 ELSE 0 END [Output_TriggeredFallFirst]
	FROM @combinedIndicatorValues

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