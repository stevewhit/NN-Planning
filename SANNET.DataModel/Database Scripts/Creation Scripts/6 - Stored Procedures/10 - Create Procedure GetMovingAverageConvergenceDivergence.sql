SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' and name = 'GetMovingAverageConvergenceDivergence')
BEGIN
	PRINT 'Dropping "GetMovingAverageConvergenceDivergence" stored procedure...'
	DROP PROCEDURE GetMovingAverageConvergenceDivergence
END
GO

PRINT 'Creating "GetMovingAverageConvergenceDivergence" stored procedure...'
GO

-- =============================================
-- Author:		Steve Whitmire Jr.
-- Create date: 09-13-2019
-- Description:	Returns the Moving Average Convergence Divergence (MACD) calculations for a given company.
-- =============================================
CREATE PROCEDURE [dbo].[GetMovingAverageConvergenceDivergence] 
	@companyId int,
	@startDate date,
	@endDate date,
	@macdPeriodShort int,
	@macdPeriodLong int,
	@macdSignalPeriod int
AS
BEGIN
	SET NOCOUNT ON;

	-- Verified 09-14-2019 --

	/*******************************************************
		Table to hold numbered quotes for a company.
	********************************************************/
	DECLARE @companyQuotes TABLE
	(
		[QuoteId] INT UNIQUE,
		[CompanyQuoteNum] INT,
		[CompanyId] INT,
        [Date] DATE,
        [Close] DECIMAL(10, 2)
	)

	INSERT INTO @companyQuotes
	SELECT [Id] as [QuoteId],
	   (SELECT [RowNum] 
	    FROM (SELECT Id, ROW_NUMBER() OVER(ORDER BY [Date]) as [RowNum] 
		      FROM [StockMarketData].dbo.Quotes quotes 
			  WHERE quotes.CompanyId = quotesOuter.CompanyId) rowNums 
	    WHERE rowNums.Id = quotesOuter.Id) as [CompanyQuoteNum],
       [CompanyId],
       [Date],
       [Close]
	FROM [StockMarketData].[dbo].[Quotes] quotesOuter
	WHERE [CompanyId] = @companyId
	ORDER BY [Date]	
	 
	/*********************************************************************************************
		Short-period MACD values
		---
		Note: MACD short-period is actually the EMA
	*********************************************************************************************/
	DECLARE @macdShortValues TABLE
	(
		[QuoteId] INT UNIQUE,
        [MACDShort] DECIMAL(10, 2)
	)

	DECLARE @previousMACDShort DECIMAL(10, 2),
			@currentMACDShort DECIMAL(10, 2),
			@currentClose DECIMAL(10, 2)

	DECLARE @minQuoteId INT = (SELECT MIN(QuoteId) FROM @companyQuotes)
	DECLARE @currentQuoteId INT = @minQuoteId
	DECLARE @maxQuoteId INT = (SELECT MAX(QuoteId) FROM @companyQuotes)
	DECLARE @smoothingConst DECIMAL(16, 6) = (2.0 / (1.0 + @macdPeriodShort))

	WHILE (@currentQuoteId <= @maxQuoteId)
	BEGIN
		SET @currentClose = (SELECT [Close] FROM @companyQuotes WHERE [QuoteId] = @currentQuoteId)
		SET @currentMACDShort = (CASE WHEN @currentQuoteId < @minQuoteId + @macdPeriodShort - 1
								THEN NULL
								ELSE (CASE WHEN @currentQuoteId = @minQuoteId + @macdPeriodShort - 1
								          THEN (SELECT AVG([CloseInner]) FROM (SELECT quotesInner.[Close] as [CloseInner] FROM @companyQuotes quotesInner WHERE quotesInner.[QuoteId] <= @currentQuoteId AND quotesInner.[QuoteId] >= (@currentQuoteId - @macdPeriodShort + 1)) as SMA)
										  ELSE ((@smoothingConst * (@currentClose - @previousMACDShort)) + @previousMACDShort)
										  END)
								END)

		INSERT INTO @macdShortValues ([QuoteId], [MACDShort]) VALUES (@currentQuoteId, @currentMACDShort)

		SET @previousMACDShort = @currentMACDShort
		SET @currentQuoteId = @currentQuoteId + 1
	END

	/*********************************************************************************************
		Long-period MACD values
		---
		Note: MACD long-period is actually the EMA
	*********************************************************************************************/
	DECLARE @macdLongValues TABLE
	(
		[QuoteId] INT UNIQUE,
        [MACDLong] DECIMAL(10, 2)
	)

	DECLARE @previousMACDLong DECIMAL(10, 2),
			@currentMACDLong DECIMAL(10, 2)

	SET @currentQuoteId = @minQuoteId
	SET @smoothingConst = (2.0 / (1.0 + @macdPeriodLong))

	WHILE (@currentQuoteId <= @maxQuoteId)
	BEGIN
		SET @currentClose = (SELECT [Close] FROM @companyQuotes WHERE [QuoteId] = @currentQuoteId)
		SET @currentMACDLong = (CASE WHEN @currentQuoteId < @minQuoteId + @macdPeriodLong - 1
								THEN NULL
								ELSE (CASE WHEN @currentQuoteId = @minQuoteId + @macdPeriodLong - 1
								          THEN (SELECT AVG([CloseInner]) FROM (SELECT quotesInner.[Close] as [CloseInner] FROM @companyQuotes quotesInner WHERE quotesInner.[QuoteId] <= @currentQuoteId AND quotesInner.[QuoteId] >= (@currentQuoteId - @macdPeriodLong + 1)) as SMA)
										  ELSE ((@smoothingConst * (@currentClose - @previousMACDLong)) + @previousMACDLong)
										  END)
								END)

		INSERT INTO @macdLongValues ([QuoteId], [MACDLong]) VALUES (@currentQuoteId, @currentMACDLong)

		SET @previousMACDLong = @currentMACDLong
		SET @currentQuoteId = @currentQuoteId + 1
	END

	/*********************************************************************************************
		Calculate MACD Values
		---
		Note: MACD = MACD(@shortPeriod) - MACD(@longPeriod)
	*********************************************************************************************/
	DECLARE @macdValues TABLE
	(
		[QuoteId] INT UNIQUE,
        [MACDShort] DECIMAL(10, 2),
		[MACDLong] DECIMAL(10, 2),
		[MACD] DECIMAL(10, 2)
	)

	INSERT INTO @macdValues
	SELECT macdShort.[QuoteId],
		   [MACDShort],
		   [MACDLong],
		   [MACDShort] - [MACDLong] as [MACD]
	FROM @macdShortValues macdShort INNER JOIN @macdLongValues macdLong ON macdShort.QuoteId = macdLong.QuoteId

	/*********************************************************************************************
		Calculate MACD Signal Line Values
		---
		Note: Signal line = EMA(@macdSignalPeriod) of the MACD
						  ==> EMA(@shortPeriod) - EMA(@longPeriod)
	*********************************************************************************************/
	DECLARE @macdSignalLineValues TABLE
	(
		[QuoteId] INT UNIQUE,
        [MACDSignal] DECIMAL(10, 2)
	)

	DECLARE @previousMACDCombined DECIMAL(10, 2),
			@currentMACDCombined DECIMAL(10, 2),
			@currentMACD DECIMAL(10, 2)

	SET @currentQuoteId = @minQuoteId
	SET @smoothingConst = (2.0 / (1.0 + @macdSignalPeriod))

	WHILE (@currentQuoteId <= @maxQuoteId)
	BEGIN
		SET @currentMACD = (SELECT [MACD] FROM @macdValues WHERE [QuoteId] = @currentQuoteId)
		SET @currentMACDCombined = (CASE WHEN @currentQuoteId < @minQuoteId + @macdPeriodLong + @macdSignalPeriod - 2
										THEN NULL
										ELSE (CASE WHEN @currentQuoteId = @minQuoteId + @macdPeriodLong + @macdSignalPeriod - 2
												   THEN (SELECT AVG([MACDInner]) FROM (SELECT macdValuesInner.[MACD] as [MACDInner] FROM @macdValues macdValuesInner WHERE macdValuesInner.[QuoteId] <= @currentQuoteId AND macdValuesInner.[QuoteId] >= (@currentQuoteId - @macdSignalPeriod + 1)) as SMA)
												   ELSE ((@smoothingConst * (@currentMACD - @previousMACDCombined)) + @previousMACDCombined)
												   END)
										END)

		INSERT INTO @macdSignalLineValues ([QuoteId], [MACDSignal]) VALUES (@currentQuoteId, @currentMACDCombined)

		SET @previousMACDCombined = @currentMACDCombined
		SET @currentQuoteId = @currentQuoteId + 1
	END

	/*********************************************************************************************
		Return MACD results
	*********************************************************************************************/
	SELECT quotes.[QuoteId],
		   quotes.[Date],
		   [MACD],
		   [MACDSignal],
		   [MACDShort] - [MACDLong] - [MACDSignal] as [MACDHistogram]
	FROM @companyQuotes quotes INNER JOIN @macdValues macdValues ON quotes.QuoteId = macdValues.QuoteId
							   INNER JOIN @macdSignalLineValues macdSignal ON quotes.QuoteId = macdSignal.QuoteId
	WHERE quotes.[Date] >= @startDate AND quotes.[Date] <= @endDate
END
GO