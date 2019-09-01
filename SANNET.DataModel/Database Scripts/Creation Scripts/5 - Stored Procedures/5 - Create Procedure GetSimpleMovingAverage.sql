SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' and name = 'GetSimpleMovingAverage')
BEGIN
	PRINT 'Dropping "GetSimpleMovingAverage" stored procedure...'
	DROP PROCEDURE GetSimpleMovingAverage
END
GO

PRINT 'Creating "GetSimpleMovingAverage" stored procedure...'
GO

-- =============================================
-- Author:		Steve Whitmire Jr.
-- Create date: 08-20-2019
-- Description:	Returns the Simple Moving Average (SMA) calculations for a given company.
-- =============================================
CREATE PROCEDURE [dbo].[GetSimpleMovingAverage] 
	@companyId int,
	@startDate date,
	@endDate date,
	@smaPeriod int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT [Id] as [QuoteId],
			[CompanyId],
			[Date],
			[Close],
			(SELECT AVG(CloseInner) FROM (SELECT quotesInner.[Close] as [CloseInner] FROM StockMarketData.dbo.Quotes quotesInner WHERE quotesInner.[CompanyId] = @companyId AND quotesInner.[Date] >= @startDate AND quotesInner.[Date] <= @endDate AND quotesInner.[Id] <= quotesOuter.[Id] AND quotesInner.[Id] >= (quotesOuter.[Id] - @smaPeriod)) as MovingAverageInner) as [SMA]
	FROM StockMarketData.dbo.Quotes quotesOuter
	WHERE [CompanyId] = @companyId AND [Date] >= @startDate AND [Date] <= @endDate 
	ORDER BY [Date]
END
GO


