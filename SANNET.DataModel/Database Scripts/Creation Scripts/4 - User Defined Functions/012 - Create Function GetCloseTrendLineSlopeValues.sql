SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'TF' and name = 'GetCloseTrendLineSlopeValues')
BEGIN
	PRINT 'Dropping "GetCloseTrendLineSlopeValues" function...'
	DROP FUNCTION GetCloseTrendLineSlopeValues
END
GO

PRINT 'Creating "GetCloseTrendLineSlopeValues" function...'
GO

-- =============================================
-- Author:		Steve Whitmire Jr.
-- Create date: 09-16-2019
-- Description:	Returns the slope of the trendline for each quote over the previous @slopePeriod values.
-- =============================================
CREATE FUNCTION [dbo].[GetCloseTrendLineSlopeValues] 
(
	@companyId INT,
	@startDate DATE, 
	@endDate DATE,
	@slopePeriod INT
)
RETURNS @slopeValues TABLE
(
	[QuoteId] INT UNIQUE,
	[TrendSlope] DECIMAL(9, 4)
)
AS
BEGIN

	-- Verified 09-19-2019 --

	/*********************************************
		Table to hold numbered quotes.
	**********************************************/
	DECLARE @companyQuotes TABLE
	(
		[QuoteId] INT UNIQUE,
		[CompanyQuoteNum] INT UNIQUE,
        [Date] DATE UNIQUE,
		[CompanyId] INT,
		[Close] DECIMAL(9, 3)
	)

	INSERT INTO @companyQuotes
	SELECT [Id], [CompanyQuoteNum], [Date], [CompanyId], [Close]
	FROM GetCompanyQuotes(@companyId)
	ORDER BY [CompanyQuoteNum]

	/*****************************************************
		Return slope values
	******************************************************/
	DECLARE @minCompanyQuoteNum INT = (SELECT MIN([CompanyQuoteNum]) FROM @companyQuotes WHERE [Date] >= @startDate AND [Date] <= @endDate),
			@maxCompanyQuoteNum INT = (SELECT MAX([CompanyQuoteNum]) FROM @companyQuotes WHERE [Date] >= @startDate AND [Date] <= @endDate)
	DECLARE	@currentCompanyQuoteNum INT = @minCompanyQuoteNum,
			@currentQuoteId INT,
			@currentSlope DECIMAL(9, 4),
			@periodCloseValues IdValues
	
	-- Foreach company quote between the start and end dates, calculate the 
	-- Trendline slope of the [Close] over the previous @slopePeriod values.
	WHILE @currentCompanyQuoteNum <= @maxCompanyQuoteNum
	BEGIN
		SET @currentQuoteId = (SELECT [QuoteId] FROM @companyQuotes WHERE [CompanyQuoteNum] = @currentCompanyQuoteNum)

		IF @currentCompanyQuoteNum >= @slopePeriod
		BEGIN
			DELETE FROM @periodCloseValues
			INSERT INTO @periodCloseValues
			SELECT [QuoteId] as [Id], [Close] as [Value]
			FROM @companyQuotes 
			WHERE [CompanyQuoteNum] <= @currentCompanyQuoteNum AND [CompanyQuoteNum] > (@currentCompanyQuoteNum - @slopePeriod)
		
			SET @currentSlope = dbo.GetTrendLineSlope(@periodCloseValues)
		END
		ELSE
			SET @currentSlope = NULL
		
		INSERT INTO @slopeValues VALUES (@currentQuoteId, @currentSlope)
		SET @currentCompanyQuoteNum = @currentCompanyQuoteNum + 1
	END

	RETURN;
END
GO