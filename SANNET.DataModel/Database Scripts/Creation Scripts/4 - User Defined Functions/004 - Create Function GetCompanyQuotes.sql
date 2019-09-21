SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'TF' and name = 'GetCompanyQuotes')
BEGIN
	PRINT 'Dropping "GetCompanyQuotes" function...'
	DROP FUNCTION GetCompanyQuotes
END
GO

PRINT 'Creating "GetCompanyQuotes" function...'
GO

-- =============================================
-- Author:		Steve Whitmire Jr.
-- Create date: 09-10-2019
-- Description:	Returns all quotes for a given company with an extra company quote row number column.
-- =============================================
CREATE FUNCTION [dbo].[GetCompanyQuotes] 
(	
	@companyId int
)
RETURNS @companyQuotes TABLE
(
	[QuoteId] INT UNIQUE,
	[CompanyQuoteNum] INT UNIQUE,
	[CompanyId] INT,
	[Date] DATE UNIQUE,
	[Open] DECIMAL(9, 3),
	[High] DECIMAL(9, 3),
	[Low] DECIMAL(9, 3),
	[Close] DECIMAL(9, 3),
	[Volume] BIGINT
)
AS
BEGIN

	/*********************************************
		Return ordered company quotes.
	**********************************************/
	INSERT INTO @companyQuotes
	SELECT [Id] as [QuoteId],
		   ROW_NUMBER() OVER(ORDER BY [Date]) as [CompanyQuoteNum],
		   [CompanyId],
		   [Date],
		   [Open],
		   [High],
		   [Low],
		   [Close],
		   [Volume]
	FROM [StockMarketData].[dbo].[Quotes] 
	WHERE [CompanyId] = @companyId
	ORDER BY [Date]

	RETURN;
END
GO