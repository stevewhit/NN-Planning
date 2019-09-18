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
	[CompanyId] INT,  
	[Date] DATE UNIQUE, 
	[SMA] DECIMAL(9, 4)
)
AS
BEGIN

	-- Verified 09/02/2019 --

	DECLARE @companyQuotes TABLE
	(
		[Id] INT UNIQUE,
		[CompanyId] INT,
		[Date] DATE UNIQUE,
		[Close] DECIMAL(9, 3)
	)

	INSERT INTO @companyQuotes 
	SELECT [Id], [CompanyId], [Date], [Close] from GetCompanyQuotes(@companyId)

	INSERT INTO @smaReturnValues
	SELECT [QuoteId],
		   [CompanyId],
		   [Date],
		   [SMA]
	FROM (SELECT [Id] as [QuoteId],
				[CompanyId],
				[Date],
				[Close],
				(SELECT AVG(CloseInner) FROM (SELECT quotesInner.[Close] as [CloseInner] FROM @companyQuotes quotesInner WHERE quotesInner.[CompanyId] = @companyId AND quotesInner.[Id] <= quotesOuter.[Id] AND quotesInner.[Id] >= (quotesOuter.[Id] - @smaPeriod + 1)) as MovingAverageInner) as [SMA]
		  FROM @companyQuotes quotesOuter
		  WHERE [CompanyId] = @companyId) smaEntire
	WHERE [Date] >= @startDate AND [Date] <= @endDate 

	RETURN;
END
GO