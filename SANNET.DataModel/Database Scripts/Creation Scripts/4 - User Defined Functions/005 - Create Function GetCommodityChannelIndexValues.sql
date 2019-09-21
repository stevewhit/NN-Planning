SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'TF' and name = 'GetCommodityChannelIndexValues')
BEGIN
	PRINT 'Dropping "GetCommodityChannelIndexValues" function...'
	DROP FUNCTION GetCommodityChannelIndexValues
END
GO

PRINT 'Creating "GetCommodityChannelIndexValues" function...'
GO

-- =============================================
-- Author:		Steve Whitmire Jr.
-- Create date: 08-20-2019
-- Description:	Returns the Commodity Channel Index (CCI) calculations for a given company.
-- =============================================
CREATE FUNCTION [dbo].[GetCommodityChannelIndexValues] 
(
	@companyId int,
	@startDate date,
	@endDate date,
	@cciPeriod int
)
RETURNS @cciValues TABLE
(
	[QuoteId] INT UNIQUE, 
	[CCI] DECIMAL(9, 3)
)
AS
BEGIN

	-- Verified 09-20-2019 --

	/*********************************************
		Table to hold numbered quotes.
	**********************************************/
	DECLARE @companyQuotes TABLE
	(
		[QuoteId] INT UNIQUE,
		[CompanyQuoteNum] INT UNIQUE,
        [Date] DATE UNIQUE,
		[CompanyId] INT,
		[High] DECIMAL(9, 3),
		[Low] DECIMAL(9, 3),
		[Close] DECIMAL(9, 3)
	)

	INSERT INTO @companyQuotes
	SELECT [QuoteId], [CompanyQuoteNum], [Date], [CompanyId], [High], [Low], [Close]
	FROM GetCompanyQuotes(@companyId)
	WHERE [Date] <= @endDate
	ORDER BY [CompanyQuoteNum]

	/*********************************************************************************************
		Table to hold CCI typical price calculations for the @cciPeriod.
	*********************************************************************************************/
	DECLARE @typicalPriceCalcs TABLE
	(
		[QuoteId] INT UNIQUE,
		[CompanyQuoteNum] INT UNIQUE,
		[TypicalPrice] DECIMAL(9, 3)
	);

	INSERT INTO @typicalPriceCalcs
	SELECT [QuoteId],
		   [CompanyQuoteNum],
		   ([High] + [Low] + [Close]) / 3.0 as [TypicalPrice]
	FROM @companyQuotes

	/*********************************************************************************************
		Table to hold CCI moving average calculations for the @cciPeriod.
	*********************************************************************************************/
	DECLARE @movingAvgCalcs TABLE
	(
		[QuoteId] INT UNIQUE,
		[CompanyQuoteNum] INT UNIQUE,
		[TypicalPrice] DECIMAL(9, 3),
		[MovingAverage] DECIMAL(9, 3)
	);

	INSERT INTO @movingAvgCalcs
	SELECT [QuoteId],
		   [CompanyQuoteNum],
		   [TypicalPrice],
		   (CASE WHEN typicalPriceOuter.[CompanyQuoteNum] < @cciPeriod
				 THEN NULL
				 ELSE (SELECT AVG(TypicalPrice) 
					   FROM (SELECT [TypicalPrice] FROM @typicalPriceCalcs typicalPriceInner WHERE typicalPriceInner.[CompanyQuoteNum] <= typicalPriceOuter.[CompanyQuoteNum] AND typicalPriceInner.[CompanyQuoteNum] >= (typicalPriceOuter.[CompanyQuoteNum] - @cciPeriod + 1)) as MovingAverageInner)
				 END) as [MovingAverage]
	FROM @typicalPriceCalcs typicalPriceOuter

	/*********************************************************************************************
		Table to hold CCI mean deviation calculations for the @cciPeriod.
	*********************************************************************************************/
	DECLARE @cciMeanDeviationCalcs TABLE
	(
		[QuoteId] INT UNIQUE,
		[CompanyQuoteNum] INT UNIQUE,
		[MeanDeviation] DECIMAL(9, 5)
	);

	INSERT INTO @cciMeanDeviationCalcs
	SELECT [QuoteId],
		   [CompanyQuoteNum],
		   (SELECT AVG(ABS(TypicalPrice - MovingAverageOuter)) 
		    FROM (SELECT [TypicalPrice], 
						 movingAverageOuter.MovingAverage as [MovingAverageOuter]
			      FROM @movingAvgCalcs movingAverageInner 
				  WHERE movingAverageInner.[CompanyQuoteNum] <= movingAverageOuter.[CompanyQuoteNum] AND movingAverageInner.[CompanyQuoteNum] >= (movingAverageOuter.[CompanyQuoteNum] - @cciPeriod + 1)) as MovingAverageInner) as [MeanDeviation]
	FROM @movingAvgCalcs movingAverageOuter

	/*********************************************************************************************
		Table to hold the CCI values over the @cciPeriod for each quote. 
	*********************************************************************************************/
	INSERT INTO @cciValues
	SELECT quotes.[QuoteId], 
		   CASE WHEN [MeanDeviation] = 0
			    THEN 10111.00
				ELSE (typicalPriceCalcs.[TypicalPrice] - [MovingAverage]) / (.015 * [MeanDeviation])
				END as [CCI]
	FROM @companyQuotes quotes 
		 INNER JOIN @typicalPriceCalcs typicalPriceCalcs ON quotes.QuoteId = typicalPriceCalcs.QuoteId
		 INNER JOIN @movingAvgCalcs movingAvgCalcs ON quotes.QuoteId = movingAvgCalcs.QuoteId
		 INNER JOIN @cciMeanDeviationCalcs meanDeviationCalcs on quotes.QuoteId = meanDeviationCalcs.QuoteId
	WHERE quotes.[Date] >= @startDate AND quotes.[Date] <= @endDate
	ORDER BY quotes.[Date]

	RETURN;
END
GO