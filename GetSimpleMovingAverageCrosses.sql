USE [SANNET]
GO

/****** Object:  StoredProcedure [dbo].[GetSimpleMovingAverageCrosses]    Script Date: 8/28/2019 10:27:41 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		Steve Whitmire Jr.
-- Create date: 08-28-2019
-- Description:	Returns the SMA crossing calculations for a given company.
-- =============================================
CREATE PROCEDURE [dbo].[GetSimpleMovingAverageCrosses]
	@companyId int,
	@startDate date,
	@endDate date,
	@smaPeriodShort int,
	@smaPeriodLong int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	/*********************************************************************************************
		Table to hold the short-term SMA values 
	*********************************************************************************************/
	DECLARE @smaShort TABLE
	(
		[QuoteId] INT,
		[CompanyId] INT,
		[Date] DATE,
		[SMAShort] DECIMAL(12, 4)
	)

	INSERT INTO @smaShort
	EXECUTE GetRelativeStrengthIndex @companyId, @startDate, @endDate, @smaPeriodShort

	/*********************************************************************************************
		Table to hold the long-term SMA values 
	*********************************************************************************************/
	DECLARE @smaLong TABLE
	(
		[QuoteId] INT,
		[CompanyId] INT,
		[Date] DATE,
		[SMALong] DECIMAL(12, 4)
	)
	
	INSERT INTO @smaLong
	EXECUTE GetRelativeStrengthIndex @companyId, @startDate, @endDate, @smaPeriodLong

	/*********************************************************************************************
		Table to hold the SMA long/short crossover data
	*********************************************************************************************/
	DECLARE @smaCrossovers DateValueCrossoversType

	INSERT INTO @smaCrossovers
	SELECT smaShort.[QuoteId], 
		   smaShort.[Date], 
		   smaShort.[SMAShort], 
		   smaLong.[SMALong] 
	FROM @smaShort smaShort 
			INNER JOIN @smaLong smaLong ON smaShort.QuoteId = smaLong.QuoteId
	ORDER BY smaShort.[Date]

	/*********************************************************************************************
		Return dataset
	*********************************************************************************************/
	SELECT [Id] as [QuoteId],
		   [Date],
		   smaCrossovers.[ConsecutiveDaysValueOneAboveValueTwo] as [ConsecutiveDaysShortAboveLong],
		   smaCrossovers.[ConsecutiveDaysValueTwoAboveValueOne] as [ConsecutiveDaysLongAboveShort]
	FROM GetDateValueCrossovers(@smaCrossovers) smaCrossovers
	WHERE [Date] >= @startDate AND [Date] <= @endDate
	ORDER BY [Date]
END
GO


