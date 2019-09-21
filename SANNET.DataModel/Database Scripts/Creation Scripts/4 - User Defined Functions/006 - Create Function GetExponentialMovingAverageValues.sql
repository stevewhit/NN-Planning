SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'TF' and name = 'GetExponentialMovingAverageValues')
BEGIN
	PRINT 'Dropping "GetExponentialMovingAverageValues" function...'
	DROP FUNCTION GetExponentialMovingAverageValues
END
GO

PRINT 'Creating "GetExponentialMovingAverageValues" function...'
GO

-- =============================================
-- Author:		Steve Whitmire Jr.
-- Create date: 09-11-2019
-- Description:	Returns the Exponential Moving Average (EMA) calculations for a given company.
-- =============================================
CREATE FUNCTION [dbo].[GetExponentialMovingAverageValues] 
(
	@companyId int,
	@startDate date,
	@endDate date,
	@emaPeriod int
)
RETURNS @emaValues TABLE
(
	[QuoteId] INT UNIQUE, 
	[EMA] DECIMAL(9, 4)
)
AS
BEGIN

	-- Verified 09-13-2019 --
	 
	 DECLARE @companyQuotes TABLE
	 (
		[QuoteId] INT UNIQUE,
		[CompanyQuoteNum] INT UNIQUE,
		[CompanyId] INT,
		[Date] DATE UNIQUE,
		[Close] DECIMAL(9, 3)
	 )

	 INSERT INTO @companyQuotes 
	 SELECT [QuoteId], [CompanyQuoteNum], [CompanyId], [Date], [Close] 
	 FROM GetCompanyQuotes(@companyId)
	 WHERE [Date] <= @endDate
	 ORDER BY [CompanyQuoteNum]

	/*********************************************************************************************
		Return dataset for EMA values.
		---
		EMA Calculations:
			1. Skip @emaPeriod rows
			2. Row @emaPeriod (first EMA) = SMA(@emaPeriod)
			3. Every following EMA = (SmoothingConst * (TodaysClose - EMAprevious)) + EMAprevious
						  SmoothingConst = 2 / (period + 1)
	*********************************************************************************************/
	DECLARE @unfilteredEmaValues TABLE
	(
		[QuoteId] INT UNIQUE,
        [EMA] DECIMAL(9, 3)
	)

	DECLARE @previousEMA DECIMAL(9, 3),
			@currentEMA DECIMAL(9, 3),
			@currentClose DECIMAL(9, 3),
			@currentQuoteId INT,
			@minQuoteRowNum INT = (SELECT MIN([CompanyQuoteNum]) FROM @companyQuotes),
			@maxQuoteRowNum INT = (SELECT MAX([CompanyQuoteNum]) FROM @companyQuotes),
			@smoothingConst DECIMAL(9, 4) = (2.0 / (1.0 + @emaPeriod))

	DECLARE @currentCompanyQuoteNum INT = @minQuoteRowNum

	WHILE (@currentCompanyQuoteNum <= @maxQuoteRowNum)
	BEGIN
		SET @currentQuoteId = (SELECT [QuoteId] FROM @companyQuotes WHERE [CompanyQuoteNum] = @currentCompanyQuoteNum)
		SET @currentClose = (SELECT [Close] FROM @companyQuotes WHERE [CompanyQuoteNum] = @currentCompanyQuoteNum)
		SET @currentEMA = (CASE WHEN @currentCompanyQuoteNum < @minQuoteRowNum + @emaPeriod - 1
								THEN NULL
								ELSE (CASE WHEN @currentCompanyQuoteNum = @minQuoteRowNum + @emaPeriod - 1
								          THEN (SELECT AVG([CloseInner]) FROM (SELECT quotesInner.[Close] as [CloseInner] FROM @companyQuotes quotesInner WHERE quotesInner.[CompanyQuoteNum] <= @currentCompanyQuoteNum AND quotesInner.[CompanyQuoteNum] >= (@currentCompanyQuoteNum - @emaPeriod + 1)) as SMA)
										  ELSE ((@smoothingConst * (@currentClose - @previousEMA)) + @previousEMA)
										  END)
								END)

		INSERT INTO @unfilteredEmaValues ([QuoteId], [EMA]) VALUES (@currentQuoteId, @currentEMA)

		SET @previousEMA = @currentEMA
		SET @currentCompanyQuoteNum = @currentCompanyQuoteNum + 1
	END

	/*********************************************************************************************
		Table to hold the EMA values over the @emaPeriod for each quote. 
	*********************************************************************************************/
	INSERT INTO @emaValues
	SELECT quotes.[QuoteId], 
		   [EMA] 
	FROM @unfilteredEmaValues ema INNER JOIN @companyQuotes quotes ON ema.[QuoteId] = quotes.[QuoteId]
	WHERE [Date] >= @startDate AND [Date] <= @endDate
	ORDER BY [Date]

	RETURN;
END

GO