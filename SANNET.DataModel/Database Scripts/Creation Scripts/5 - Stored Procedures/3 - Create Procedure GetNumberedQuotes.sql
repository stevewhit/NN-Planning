SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' and name = 'GetNumberedQuotes')
BEGIN
	PRINT 'Dropping "GetNumberedQuotes" stored procedure...'
	DROP PROCEDURE GetNumberedQuotes
END
GO

PRINT 'Creating "GetNumberedQuotes" stored procedure...'
GO

-- =============================================
-- Author:		Steve Whitmire Jr.
-- Create date: 09-10-2019
-- Description:	Returns all quotes for a given company with an extra company quote row number column.
-- =============================================
CREATE PROCEDURE [dbo].[GetNumberedQuotes] 
	@companyId int
AS
BEGIN
	SET NOCOUNT ON;

	/*********************************************************
		Table to hold numbered quotes for a given company.
	**********************************************************/
	DECLARE @companyQuotes TABLE
	(
		[Id] INT UNIQUE,
		[CompanyQuoteNum] INT,
		[CompanyId] INT,
        [Date] DATE,
        [Open] DECIMAL(10, 2),
        [High] DECIMAL(10, 2),
        [Low] DECIMAL(10, 2),
        [Close] DECIMAL(10, 2),
        [Volume] BIGINT
	)

	INSERT INTO @companyQuotes
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

	/***************************************************
		Return table with/without filter applied.
	****************************************************/
	IF @companyId IS NOT NULL
		SELECT * FROM @companyQuotes WHERE [CompanyId] = @companyId ORDER BY [Date]
	ELSE
		SELECT * FROM @companyQuotes ORDER BY [Date]
END
GO


