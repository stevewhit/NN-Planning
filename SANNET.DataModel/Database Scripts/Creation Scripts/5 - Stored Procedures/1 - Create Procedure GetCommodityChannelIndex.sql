SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' and name = 'GetCommodityChannelIndex')
BEGIN
	PRINT 'Dropping "GetCommodityChannelIndex" stored procedure...'
	DROP PROCEDURE GetCommodityChannelIndex
END
GO

PRINT 'Creating "GetCommodityChannelIndex" stored procedure...'
GO

-- =============================================
-- Author:		Steve Whitmire Jr.
-- Create date: 08-20-2019
-- Description:	Returns the Commodity Channel Index (CCI) calculations for a given company.
-- =============================================
CREATE PROCEDURE [dbo].[GetCommodityChannelIndex] 
	@companyId int,
	@startDate date,
	@endDate date,
	@cciPeriod int
AS
BEGIN
	SET NOCOUNT ON;

	/*********************************************************************************************
		Table to hold CCI typical price calculations for the @cciPeriod.
	*********************************************************************************************/
	DECLARE @typicalPriceCalcs TABLE
	(
		[QuoteId] INT,
		[CompanyId] INT,
		[Date] DATE,
		[TypicalPrice] DECIMAL(12, 4)
	);

	INSERT INTO @typicalPriceCalcs
	SELECT [Id] as [QuoteId],
		   [CompanyId],
		   [Date],
		   ([High] + [Low] + [Close]) / 3.0 as [TypicalPrice]
	FROM StockMarketData.dbo.Quotes
	WHERE [CompanyId] = @companyId
	ORDER BY [Date]

	/*********************************************************************************************
		Table to hold CCI moving average calculations for the @cciPeriod.
	*********************************************************************************************/
	DECLARE @movingAvgCalcs TABLE
	(
		[QuoteId] INT,
		[Date] DATE,
		[TypicalPrice] DECIMAL(12, 4),
		[MovingAverage] DECIMAL(12, 4)
	);

	INSERT INTO @movingAvgCalcs
	SELECT [QuoteId],
		   [Date],
		   [TypicalPrice],
		   (SELECT AVG(TypicalPrice) FROM (SELECT [TypicalPrice] FROM @typicalPriceCalcs typicalPriceInner WHERE typicalPriceInner.QuoteId <= typicalPriceOuter.QuoteId AND typicalPriceInner.QuoteId >= (typicalPriceOuter.QuoteId - @cciPeriod)) as MovingAverageInner) as [MovingAverage]
	FROM @typicalPriceCalcs typicalPriceOuter
	ORDER BY [Date]

	/*********************************************************************************************
		Table to hold CCI mean deviation calculations for the @cciPeriod.
	*********************************************************************************************/
	DECLARE @cciMeanDeviationCalcs TABLE
	(
		[QuoteId] INT,
		[Date] DATE,
		[MeanDeviation] DECIMAL(12, 4)
	);

	INSERT INTO @cciMeanDeviationCalcs
	SELECT [QuoteId],
		   [Date],
		   (SELECT AVG(ABS(TypicalPrice - MovingAverage)) FROM (SELECT [TypicalPrice], [MovingAverage] FROM @movingAvgCalcs movingAverageInner WHERE movingAverageInner.QuoteId <= movingAverageOuter.QuoteId AND movingAverageInner.QuoteId >= (movingAverageOuter.QuoteId - @cciPeriod)) as MovingAverageInner) as [MovingAverage]
	FROM @movingAvgCalcs movingAverageOuter
	ORDER BY [Date]


	/*********************************************************************************************
		Table to hold the CCI values over the @cciPeriod for each quote. 
	*********************************************************************************************/
	SELECT typicalPriceCalcs.QuoteId as [QuoteId], 
	       typicalPriceCalcs.CompanyId as [CompanyId], 
	       typicalPriceCalcs.Date as [Date],
		   --[High],[Low],[Close],
		   --typicalPriceCalcs.TypicalPrice as [TypicalPrice],
		   --movingAvgCalcs.MovingAverage as [MovingAverage],
		   --meanDeviationCalcs.MeanDeviation as [MeanDeviation],
		   CASE WHEN meanDeviationCalcs.MeanDeviation = 0
			    THEN 10111.00
				ELSE (typicalPriceCalcs.TypicalPrice - movingAvgCalcs.MovingAverage) / (.015 * meanDeviationCalcs.MeanDeviation)
				END as CCI
	FROM @typicalPriceCalcs typicalPriceCalcs
		INNER JOIN @movingAvgCalcs movingAvgCalcs ON typicalPriceCalcs.QuoteId = movingAvgCalcs.QuoteId
		INNER JOIN @cciMeanDeviationCalcs meanDeviationCalcs on typicalPriceCalcs.QuoteId = meanDeviationCalcs.QuoteId
		--INNER JOIN StockMarketData.dbo.Quotes quotes on quotes.Id = typicalPriceCalcs.QuoteId
	WHERE typicalPriceCalcs.[Date] >= @startDate AND typicalPriceCalcs.[Date] <= @endDate
	ORDER BY typicalPriceCalcs.[Date]
END
GO


