SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' and name = 'GetCommodityChannelIndexCrosses')
BEGIN
	PRINT 'Dropping "GetCommodityChannelIndexCrosses" stored procedure...'
	DROP PROCEDURE GetCommodityChannelIndexCrosses
END
GO

PRINT 'Creating "GetCommodityChannelIndexCrosses" stored procedure...'
GO

-- =============================================
-- Author:		Steve Whitmire Jr.
-- Create date: 08-28-2019
-- Description:	Returns the CCI crossing calculations for a given company.
-- =============================================
CREATE PROCEDURE [dbo].[GetCommodityChannelIndexCrosses]
	@companyId int,
	@startDate date,
	@endDate date,
	@cciPeriodShort int,
	@cciPeriodLong int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	/*********************************************************************************************
		Table to hold the short-term CCI values 
	*********************************************************************************************/
	DECLARE @cciShort TABLE
	(
		[QuoteId] INT,
		[CompanyId] INT,
		[Date] DATE,
		[CCIShort] DECIMAL(12, 4)
	)

	INSERT INTO @cciShort
	EXECUTE GetRelativeStrengthIndex @companyId, @startDate, @endDate, @cciPeriodShort

	/*********************************************************************************************
		Table to hold the long-term CCI values 
	*********************************************************************************************/
	DECLARE @cciLong TABLE
	(
		[QuoteId] INT,
		[CompanyId] INT,
		[Date] DATE,
		[CCILong] DECIMAL(12, 4)
	)
	
	INSERT INTO @cciLong
	EXECUTE GetRelativeStrengthIndex @companyId, @startDate, @endDate, @cciPeriodLong

	/*********************************************************************************************
		Table to hold the CCI long/short crossover data
	*********************************************************************************************/
	DECLARE @cciCrossovers DateValueCrossoversType

	INSERT INTO @cciCrossovers
	SELECT cciShort.[QuoteId], 
		   cciShort.[Date], 
		   cciShort.[CCIShort], 
		   cciLong.[CCILong] 
	FROM @cciShort cciShort 
			INNER JOIN @cciLong cciLong ON cciShort.QuoteId = cciLong.QuoteId
	ORDER BY cciShort.[Date]

	/*********************************************************************************************
		Return dataset
	*********************************************************************************************/
	SELECT [Id] as [QuoteId],
		   [Date],
		   cciCrossovers.[ConsecutiveDaysValueOneAboveValueTwo] as [ConsecutiveDaysShortAboveLong],
		   cciCrossovers.[ConsecutiveDaysValueTwoAboveValueOne] as [ConsecutiveDaysLongAboveShort]
	FROM GetDateValueCrossovers(@cciCrossovers) cciCrossovers
	WHERE [Date] >= @startDate AND [Date] <= @endDate
	ORDER BY [Date]
END
GO


