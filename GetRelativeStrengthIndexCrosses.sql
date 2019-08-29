SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' and name = 'GetRelativeStrengthIndexCrosses')
BEGIN
	PRINT 'Dropping "GetRelativeStrengthIndexCrosses" stored procedure...'
	DROP PROCEDURE GetRelativeStrengthIndexCrosses
END
GO

PRINT 'Creating "GetRelativeStrengthIndexCrosses" stored procedure...'
GO

-- =============================================
-- Author:		Steve Whitmire Jr.
-- Create date: 08-28-2019
-- Description:	Returns the RSI crossing calculations for a given company.
-- =============================================
CREATE PROCEDURE [dbo].[GetRelativeStrengthIndexCrosses]
	@companyId int,
	@startDate date,
	@endDate date,
	@rsiPeriodShort int,
	@rsiPeriodLong int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	/*********************************************************************************************
		Table to hold the short-term RSI values 
	*********************************************************************************************/
	DECLARE @rsiShort TABLE
	(
		[QuoteId] INT,
		[CompanyId] INT,
		[Date] DATE,
		[RSIShort] DECIMAL(12, 4)
	)

	INSERT INTO @rsiShort
	EXECUTE GetRelativeStrengthIndex @companyId, @startDate, @endDate, @rsiPeriodShort

	/*********************************************************************************************
		Table to hold the long-term RSI values 
	*********************************************************************************************/
	DECLARE @rsiLong TABLE
	(
		[QuoteId] INT,
		[CompanyId] INT,
		[Date] DATE,
		[RSILong] DECIMAL(12, 4)
	)
	
	INSERT INTO @rsiLong
	EXECUTE GetRelativeStrengthIndex @companyId, @startDate, @endDate, @rsiPeriodLong

	/*********************************************************************************************
		Table to hold the RSI long/short crossover data
	*********************************************************************************************/
	DECLARE @rsiCrossovers DateValueCrossoversType

	INSERT INTO @rsiCrossovers
	SELECT rsiShort.[QuoteId], 
		   rsiShort.[Date], 
		   rsiShort.[RSIShort], 
		   rsiLong.[RSILong] 
	FROM @rsiShort rsiShort 
			INNER JOIN @rsiLong rsiLong ON rsiShort.QuoteId = rsiLong.QuoteId
	ORDER BY rsiShort.[Date]

	/*********************************************************************************************
		Return dataset
	*********************************************************************************************/
	SELECT [Id] as [QuoteId],
		   [Date],
		   rsiCrossovers.[ConsecutiveDaysValueOneAboveValueTwo] as [ConsecutiveDaysShortAboveLong],
		   rsiCrossovers.[ConsecutiveDaysValueTwoAboveValueOne] as [ConsecutiveDaysLongAboveShort]
	FROM GetDateValueCrossovers(@rsiCrossovers) rsiCrossovers
	WHERE [Date] >= @startDate AND [Date] <= @endDate
	ORDER BY [Date]
END
GO


