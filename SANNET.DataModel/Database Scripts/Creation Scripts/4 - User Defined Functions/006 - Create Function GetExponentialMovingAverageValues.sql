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
	[CompanyId] INT,  
	[Date] DATE UNIQUE, 
	[EMA] DECIMAL(9, 4)
)
AS
BEGIN

	-- Verified 09-13-2019 --
	 
	 DECLARE @companyQuotes TABLE
	 (
		[Id] INT UNIQUE,
		[CompanyId] INT,
		[Date] DATE UNIQUE,
		[Close] DECIMAL(9, 3)
	 )

	 INSERT INTO @companyQuotes 
	 SELECT [Id], [CompanyId], [Date], [Close] from GetCompanyQuotes(@companyId)

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
			@currentClose DECIMAL(9, 3)

	DECLARE @minQuoteId INT = (SELECT MIN(Id) FROM @companyQuotes)
	DECLARE @currentQuoteId INT = @minQuoteId
	DECLARE @maxQuoteId INT = (SELECT MAX(Id) FROM @companyQuotes)
	DECLARE @smoothingConst DECIMAL(9, 4) = (2.0 / (1.0 + @emaPeriod))

	WHILE (@currentQuoteId <= @maxQuoteId)
	BEGIN
		SET @currentClose = (SELECT [Close] FROM @companyQuotes WHERE [Id] = @currentQuoteId)
		SET @currentEMA = (CASE WHEN @currentQuoteId < @minQuoteId + @emaPeriod - 1
								THEN NULL
								ELSE (CASE WHEN @currentQuoteId = @minQuoteId + @emaPeriod - 1
								          THEN (SELECT AVG([CloseInner]) FROM (SELECT quotesInner.[Close] as [CloseInner] FROM @companyQuotes quotesInner WHERE quotesInner.[Id] <= @currentQuoteId AND quotesInner.[Id] >= (@currentQuoteId - @emaPeriod + 1)) as SMA)
										  ELSE ((@smoothingConst * (@currentClose - @previousEMA)) + @previousEMA)
										  END)
								END)

		INSERT INTO @unfilteredEmaValues ([QuoteId], [EMA]) VALUES (@currentQuoteId, @currentEMA)

		SET @previousEMA = @currentEMA
		SET @currentQuoteId = @currentQuoteId + 1
	END

	/*********************************************************************************************
		Table to hold the EMA values over the @emaPeriod for each quote. 
	*********************************************************************************************/
	INSERT INTO @emaValues
	SELECT quotes.[Id], 
		   quotes.[CompanyId],
		   quotes.[Date],
		   ema.[EMA] 
	FROM @unfilteredEmaValues ema INNER JOIN @companyQuotes quotes ON ema.QuoteId = quotes.Id
	WHERE [Date] >= @startDate AND [Date] <= @endDate

	RETURN;
END

GO