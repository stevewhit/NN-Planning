SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'TF' and name = 'GetSimpleMovingAverageValues')
BEGIN
	PRINT 'Dropping "GetSimpleMovingAverageValues" function...'
	DROP FUNCTION GetSimpleMovingAverageValues
END
GO

PRINT 'Creating "GetSimpleMovingAverageValues" function...'
GO

-- =============================================
-- Author:		Steve Whitmire Jr.
-- Create date: 08-20-2019
-- Description:	Returns the Simple Moving Average (SMA) calculations for a given company.
-- =============================================
CREATE FUNCTION [dbo].[GetSimpleMovingAverageValues] 
(
	@companyId int,
	@startDate date,
	@endDate date,
	@smaPeriod int
)
RETURNS @smaReturnValues TABLE
(
	[QuoteId] INT UNIQUE, 
	[SMA] DECIMAL(9, 4)
)
AS
BEGIN

	-- Verified 09/20/2019 --

	/*******************************************************
		Table to hold numbered quotes for a company.
	********************************************************/
	DECLARE @companyQuotes TABLE
	(
		[QuoteId] INT UNIQUE,
		[CompanyQuoteNum] INT UNIQUE,
		[Date] DATE UNIQUE,
		[CompanyId] INT,
		[Close] DECIMAL(9, 3)
	)

	INSERT INTO @companyQuotes 
	SELECT [QuoteId], [CompanyQuoteNum], [Date], [CompanyId], [Close] 
	FROM GetCompanyQuotes(@companyId)
	WHERE [Date] <= @endDate
	ORDER BY [CompanyQuoteNum]

	/*******************************************************
		Table to hold SMA values for a company
	********************************************************/
	DECLARE @quoteSMAs TABLE
	(
		[QuoteId] INT,
		[SMA] DECIMAL(9, 4)
	)

	INSERT INTO @quoteSMAs
	SELECT [QuoteId],
		   CASE WHEN [CompanyQuoteNum] < @smaPeriod
				THEN NULL
				ELSE (SELECT AVG(CloseInner) 
					  FROM (SELECT quotesInner.[Close] as [CloseInner] 
						    FROM @companyQuotes quotesInner 
						    WHERE quotesInner.[CompanyQuoteNum] <= quotesOuter.[CompanyQuoteNum] AND quotesInner.[CompanyQuoteNum] >= (quotesOuter.[CompanyQuoteNum] - @smaPeriod + 1)) as MovingAverageInner)
				END as [SMA]
	FROM @companyQuotes quotesOuter

	/*******************************************************
		Return dataset
	********************************************************/
	INSERT INTO @smaReturnValues
	SELECT quotes.[QuoteId],
		   [SMA]
	FROM @companyQuotes quotes INNER JOIN @quoteSMAs smas ON quotes.[QuoteId] = smas.[QuoteId]
	WHERE [Date] >= @startDate AND [Date] <= @endDate 
	ORDER BY [Date]

	RETURN;
END
GO