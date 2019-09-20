SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'TF' and name = 'GetMovingAverageConvergenceDivergenceValues')
BEGIN
	PRINT 'Dropping "GetMovingAverageConvergenceDivergenceValues" function...'
	DROP FUNCTION GetMovingAverageConvergenceDivergenceValues
END
GO

PRINT 'Creating "GetMovingAverageConvergenceDivergenceValues" function...'
GO

-- =============================================
-- Author:		Steve Whitmire Jr.
-- Create date: 09-13-2019
-- Description:	Returns the Moving Average Convergence Divergence (MACD) calculations for a given company.
-- =============================================
CREATE FUNCTION [dbo].[GetMovingAverageConvergenceDivergenceValues] 
(
	@companyId int,
	@startDate date,
	@endDate date,
	@macdPeriodShort int,
	@macdPeriodLong int,
	@macdSignalPeriod int
)
RETURNS @macdReturnValues TABLE
(
	[QuoteId] INT UNIQUE, 
	[CompanyId] INT,  
	[Date] DATE UNIQUE, 
	[MACD] DECIMAL(9, 4),
	[MACDSignal] DECIMAL(9, 4),
	[MACDHistogram] DECIMAL(9, 4)
)
AS
BEGIN

	-- Verified 09-14-2019 --

	/*******************************************************
		Table to hold numbered quotes for a company.
	********************************************************/
	DECLARE @companyQuotes TABLE
	(
		[QuoteId] INT UNIQUE,
		[CompanyQuoteNum] INT UNIQUE,
		[CompanyId] INT,
        [Date] DATE UNIQUE,
        [Close] DECIMAL(9, 3)
	)

	INSERT INTO @companyQuotes
	SELECT [Id], [CompanyQuoteNum], [CompanyId], [Date], [Close] 
	FROM GetCompanyQuotes(@companyId)
	 
	/*********************************************************************************************
		Short-period MACD values
		---
		Note: MACD short-period is actually the EMA
	*********************************************************************************************/
	DECLARE @macdShortValues TABLE
	(
		[QuoteId] INT UNIQUE,
		[CompanyQuoteNum] INT UNIQUE,
        [MACDShort] DECIMAL(9, 4)
	)

	DECLARE @previousMACDShort DECIMAL(9, 4),
			@currentMACDShort DECIMAL(9, 4),
			@currentClose DECIMAL(9, 3),
			@currentQuoteId INT

	DECLARE @minRowNum INT = (SELECT MIN([CompanyQuoteNum]) FROM @companyQuotes)
	DECLARE @currentRowNum INT = @minRowNum
	DECLARE @maxRowNum INT = (SELECT MAX([CompanyQuoteNum]) FROM @companyQuotes)
	DECLARE @smoothingConst DECIMAL(9, 4) = (2.0 / (1.0 + @macdPeriodShort))

	WHILE (@currentRowNum <= @maxRowNum)
	BEGIN
		SET @currentQuoteId = (SELECT [QuoteId] FROM @companyQuotes WHERE [CompanyQuoteNum] = @currentRowNum)
		SET @currentClose = (SELECT [Close] FROM @companyQuotes WHERE [CompanyQuoteNum] = @currentRowNum)
		SET @currentMACDShort = (CASE WHEN @currentRowNum < @minRowNum + @macdPeriodShort - 1
								THEN NULL
								ELSE (CASE WHEN @currentRowNum = @minRowNum + @macdPeriodShort - 1
								          THEN (SELECT AVG([CloseInner]) FROM (SELECT quotesInner.[Close] as [CloseInner] FROM @companyQuotes quotesInner WHERE quotesInner.[CompanyQuoteNum] <= @currentRowNum AND quotesInner.[CompanyQuoteNum] >= (@currentRowNum - @macdPeriodShort + 1)) as SMA)
										  ELSE ((@smoothingConst * (@currentClose - @previousMACDShort)) + @previousMACDShort)
										  END)
								END)

		INSERT INTO @macdShortValues ([QuoteId], [CompanyQuoteNum], [MACDShort]) VALUES (@currentQuoteId, @currentRowNum, @currentMACDShort)

		SET @previousMACDShort = @currentMACDShort
		SET @currentRowNum = @currentRowNum + 1
	END

	/*********************************************************************************************
		Long-period MACD values
		---
		Note: MACD long-period is actually the EMA
	*********************************************************************************************/
	DECLARE @macdLongValues TABLE
	(
		[QuoteId] INT UNIQUE,
		[CompanyQuoteNum] INT UNIQUE,
        [MACDLong] DECIMAL(9, 4)
	)

	DECLARE @previousMACDLong DECIMAL(9, 4),
			@currentMACDLong DECIMAL(9, 4)

	SET @currentRowNum = @minRowNum
	SET @smoothingConst = (2.0 / (1.0 + @macdPeriodLong))

	WHILE (@currentRowNum <= @maxRowNum)
	BEGIN
		SET @currentQuoteId = (SELECT [QuoteId] FROM @companyQuotes WHERE [CompanyQuoteNum] = @currentRowNum)
		SET @currentClose = (SELECT [Close] FROM @companyQuotes WHERE [CompanyQuoteNum] = @currentRowNum)
		SET @currentMACDLong = (CASE WHEN @currentRowNum < @minRowNum + @macdPeriodLong - 1
								THEN NULL
								ELSE (CASE WHEN @currentRowNum = @minRowNum + @macdPeriodLong - 1
								          THEN (SELECT AVG([CloseInner]) FROM (SELECT quotesInner.[Close] as [CloseInner] FROM @companyQuotes quotesInner WHERE quotesInner.[CompanyQuoteNum] <= @currentRowNum AND quotesInner.[CompanyQuoteNum] >= (@currentRowNum - @macdPeriodLong + 1)) as SMA)
										  ELSE ((@smoothingConst * (@currentClose - @previousMACDLong)) + @previousMACDLong)
										  END)
								END)

		INSERT INTO @macdLongValues ([QuoteId], [CompanyQuoteNum], [MACDLong]) VALUES (@currentQuoteId, @currentRowNum, @currentMACDLong)

		SET @previousMACDLong = @currentMACDLong
		SET @currentRowNum = @currentRowNum + 1
	END

	/*********************************************************************************************
		Calculate MACD Values
		---
		Note: MACD = MACD(@shortPeriod) - MACD(@longPeriod)
	*********************************************************************************************/
	DECLARE @macdValues TABLE
	(
		[QuoteId] INT UNIQUE,
		[CompanyQuoteNum] INT UNIQUE,
        [MACDShort] DECIMAL(9, 4),
		[MACDLong] DECIMAL(9, 4),
		[MACD] DECIMAL(9, 4)
	)

	INSERT INTO @macdValues
	SELECT macdShort.[QuoteId],
		   macdShort.[CompanyQuoteNum],
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
        [MACDSignal] DECIMAL(9, 4)
	)

	DECLARE @previousMACDCombined DECIMAL(9, 4),
			@currentMACDCombined DECIMAL(9, 4),
			@currentMACD DECIMAL(9, 4)

	SET @currentRowNum = @minRowNum
	SET @smoothingConst = (2.0 / (1.0 + @macdSignalPeriod))

	WHILE (@currentRowNum <= @maxRowNum)
	BEGIN
		SET @currentQuoteId = (SELECT [QuoteId] FROM @companyQuotes WHERE [CompanyQuoteNum] = @currentRowNum)
		SET @currentMACD = (SELECT [MACD] FROM @macdValues WHERE [CompanyQuoteNum] = @currentRowNum)
		SET @currentMACDCombined = (CASE WHEN @currentRowNum < @minRowNum + @macdPeriodLong + @macdSignalPeriod - 2
										THEN NULL
										ELSE (CASE WHEN @currentRowNum = @minRowNum + @macdPeriodLong + @macdSignalPeriod - 2
												   THEN (SELECT AVG([MACDInner]) FROM (SELECT macdValuesInner.[MACD] as [MACDInner] FROM @macdValues macdValuesInner WHERE macdValuesInner.[CompanyQuoteNum] <= @currentRowNum AND macdValuesInner.[CompanyQuoteNum] >= (@currentRowNum - @macdSignalPeriod + 1)) as SMA)
												   ELSE ((@smoothingConst * (@currentMACD - @previousMACDCombined)) + @previousMACDCombined)
												   END)
										END)

		INSERT INTO @macdSignalLineValues ([QuoteId], [MACDSignal]) VALUES (@currentQuoteId, @currentMACDCombined)

		SET @previousMACDCombined = @currentMACDCombined
		SET @currentRowNum = @currentRowNum + 1
	END

	/*********************************************************************************************
		Return MACD results
	*********************************************************************************************/
	INSERT INTO @macdReturnValues
	SELECT quotes.[QuoteId],
		   quotes.[CompanyId],
		   quotes.[Date],
		   [MACD],
		   [MACDSignal],
		   [MACDShort] - [MACDLong] - [MACDSignal] as [MACDHistogram]
	FROM @companyQuotes quotes INNER JOIN @macdValues macdValues ON quotes.QuoteId = macdValues.QuoteId
							   INNER JOIN @macdSignalLineValues macdSignal ON quotes.QuoteId = macdSignal.QuoteId
	WHERE quotes.[Date] >= @startDate AND quotes.[Date] <= @endDate

	RETURN;
END
GO