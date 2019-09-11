SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' and name = 'GetStochasticIndicator')
BEGIN
	PRINT 'Dropping "GetStochasticIndicator" stored procedure...'
	DROP PROCEDURE GetStochasticIndicator
END
GO

PRINT 'Creating "GetStochasticIndicator" stored procedure...'
GO

-- =============================================
-- Author:		Steve Whitmire Jr.
-- Create date: 09-10-2019
-- Description:	Returns the Stochastic Indicator calculations for a given company.
--				This indicator measures the momentum of price and often indicates
--				overbought and oversold values using 80 and 20 respectively.
-- =============================================
CREATE PROCEDURE [dbo].[GetStochasticIndicator] 
	@companyId int,
	@startDate date,
	@endDate date,
	@siPeriod int
AS
BEGIN
	SET NOCOUNT ON;

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
		Table to hold the highest/lowest values for the period for each quote.
	*********************************************************************************************/
	DECLARE @periodHighLows TABLE
	(
		[QuoteId] INT UNIQUE,
        [PeriodHigh] DECIMAL(10, 2),
        [PeriodLow] DECIMAL(10, 2)
	)

	INSERT INTO @periodHighLows
	SELECT [QuoteId],
		   CASE WHEN [CompanyQuoteNum] <= @siPeriod
			     THEN NULL
		   	     ELSE (SELECT MAX(High) 
					   FROM (SELECT [High]
						     FROM @companyQuotes companyQuotesInner
							 WHERE companyQuotesInner.CompanyQuoteNum <= companyQuotesOuter.CompanyQuoteNum AND companyQuotesInner.CompanyQuoteNum >= (companyQuotesOuter.CompanyQuoteNum - @siPeriod + 1)) as companyQuotesInnerInner)
			     END as [PeriodHigh],
		   CASE WHEN [CompanyQuoteNum] <= @siPeriod
			     THEN NULL
		   	     ELSE (SELECT MIN(High) 
					   FROM (SELECT [High]
						     FROM @companyQuotes companyQuotesInner
							 WHERE companyQuotesInner.CompanyQuoteNum <= companyQuotesOuter.CompanyQuoteNum AND companyQuotesInner.CompanyQuoteNum >= (companyQuotesOuter.CompanyQuoteNum - @siPeriod + 1)) as companyQuotesInnerInner)
			     END as [PeriodLow]
	FROM @companyQuotes companyQuotesOuter
	ORDER BY [Date]

	/*********************************************************************************************
		Result table that calculates the Stochastic values
	*********************************************************************************************/
	SELECT quotes.[QuoteId],
		   quotes.Date,
		   CASE WHEN quotes.Low = highLows.PeriodLow
				THEN 100.00
				ELSE (highLows.PeriodHigh - highLows.PeriodLow) / (quotes.Low - highLows.PeriodLow)
				END as [SI]
	FROM @companyQuotes quotes INNER JOIN @periodHighLows highLows ON quotes.QuoteId = highLows.QuoteId
	WHERE quotes.[Date] >= @startDate AND quotes.[Date] <= @endDate
END
GO