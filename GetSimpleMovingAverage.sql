USE [SANNET]
GO

/****** Object:  StoredProcedure [dbo].[GetSimpleMovingAverage]    Script Date: 8/28/2019 10:27:33 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
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

	/* Calculate the SMA over the @smaPeriod for each quote */
	DECLARE @sql nvarchar(max) = 
		N'SELECT [Id] as [QuoteId],
				 [CompanyId],
				 [Date],
				 AVG([Close]) OVER (ORDER BY [Id] ROWS ' + convert(varchar, @smaPeriod) +' PRECEDING) as [SMA]
		  FROM StockMarketData.dbo.Quotes
		  WHERE [CompanyId] = ' + convert(varchar, @companyId) + ' AND [Date] >= ''' + convert(varchar, @startDate) + ''' AND [Date] <= ''' + convert(varchar, @endDate) + ''' 
		  ORDER BY [Date]'
	EXEC sp_executesql @sql
END


GO


