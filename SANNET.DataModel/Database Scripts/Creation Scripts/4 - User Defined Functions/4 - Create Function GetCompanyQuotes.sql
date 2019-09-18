SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'IF' and name = 'GetCompanyQuotes')
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
RETURNS TABLE 
AS
RETURN 
(
	SELECT [Id],
		   (SELECT [RowNum] 
			FROM (SELECT Id, ROW_NUMBER() OVER(ORDER BY Id) as [RowNum] 
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
)
GO