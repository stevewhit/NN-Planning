SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' and name = 'GetExponentialMovingAverage')
BEGIN
	PRINT 'Dropping "GetExponentialMovingAverage" stored procedure...'
	DROP PROCEDURE GetExponentialMovingAverage
END
GO

PRINT 'Creating "GetExponentialMovingAverage" stored procedure...'
GO

-- =============================================
-- Author:		Steve Whitmire Jr.
-- Create date: 09-11-2019
-- Description:	Returns the Exponential Moving Average (EMA) calculations for a given company.
-- =============================================
CREATE PROCEDURE [dbo].[GetExponentialMovingAverage] 
	@companyId int,
	@startDate date,
	@endDate date,
	@emaPeriod int
AS
BEGIN
	SET NOCOUNT ON;

	-- Verified NEVER --

	/*********************************************
		Table to hold numbered quotes.
	**********************************************/
	DECLARE @companyQuotes TABLE
	(
		[QuoteId] INT UNIQUE,
		[CompanyQuoteNum] INT,
		[CompanyId] INT,
        [Date] DATE,
        [Open] DECIMAL(10, 2),
        [High] DECIMAL(10, 2),
        [Low] DECIMAL(10, 2),
        [Close] DECIMAL(10, 2),
        [Volume] BIGINT
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
       [Open],
       [High],
       [Low],
       [Close],
       [Volume]
	FROM [StockMarketData].[dbo].[Quotes] quotesOuter
	WHERE [CompanyId] = @companyId
	ORDER BY [Date]	
	 
	/*********************************************************************************************
		Table for QuoteId && EMAvalues
	*********************************************************************************************/
	DECLARE @emaValues TABLE
	(
		[QuoteId] INT UNIQUE,
        [EMA] DECIMAL(10, 2)
	)

	DECLARE @previousEMA DECIMAL(10, 2),
			@currentEMA DECIMAL(10, 2),
			@currentClose DECIMAL(10, 2)

	DECLARE @minQuoteId INT = (SELECT MIN(QuoteId) FROM @companyQuotes)
	DECLARE @currentQuoteId INT = @minQuoteId
	DECLARE @maxQuoteId INT = (SELECT MAX(QuoteId) FROM @companyQuotes)
	DECLARE @smoothingConst DECIMAL(10, 2) = (2.0 / (1.0 + @emaPeriod))

	WHILE (@currentQuoteId <= @maxQuoteId)
	BEGIN
		SET @currentClose = (SELECT [Close] FROM @companyQuotes WHERE [QuoteId] = @currentQuoteId)
		SET @currentEMA = (CASE WHEN @currentQuoteId < @minQuoteId + @emaPeriod - 1
								THEN NULL
								ELSE (CASE WHEN @currentQuoteId = @minQuoteId + @emaPeriod - 1
								          THEN (SELECT AVG([CloseInner]) FROM (SELECT quotesInner.[Close] as [CloseInner] FROM @companyQuotes quotesInner WHERE quotesInner.[QuoteId] <= @currentQuoteId AND quotesInner.[QuoteId] >= (@currentQuoteId - @emaPeriod + 1)) as SMA)
										  ELSE ((@smoothingConst * (@currentClose - @previousEMA)) + @previousEMA)
										  END)
								END)

		INSERT INTO @emaValues ([QuoteId], [EMA]) VALUES (@currentQuoteId, @currentEMA)

		SET @previousEMA = @currentEMA
		SET @currentQuoteId = @currentQuoteId + 1
	END

	SELECT * FROM @emaValues
	--WHERE -startdate --> enddate


	-- Use while loop because we have to reach-back to the previous row's EMA value.




	-- First EMA = SMA(period)
	-- Every EMA afterwards: EMA = (SmoothingConst * (TodaysClose - EMAprevious)) + EMAprevious
	--							==> SmoothingConst = 2 / (period + 1)
END
GO
