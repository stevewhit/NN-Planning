SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' and name = 'GetRelativeStrengthIndex')
BEGIN
	PRINT 'Dropping "GetRelativeStrengthIndex" stored procedure...'
	DROP PROCEDURE GetRelativeStrengthIndex
END
GO

PRINT 'Creating "GetRelativeStrengthIndex" stored procedure...'
GO

-- =============================================
-- Author:		Steve Whitmire Jr.
-- Create date: 08-19-2019
-- Description:	Returns the Relative Strength Index (RSI) calculations for a given company.
-- =============================================
CREATE PROCEDURE [dbo].[GetRelativeStrengthIndex] 
	@companyId int,
	@startDate date,
	@endDate date,
	@rsiPeriod int
AS
BEGIN
	SET NOCOUNT ON;

	/*********************************************************************************************
		Table to hold RSI change calculations for the @rsiPeriod.
	*********************************************************************************************/
	DECLARE @changeCalcs TABLE
	(
		[QuoteId] INT, 
		[CompanyId] INT, 
		[Date] DATE, 
		[Close] DECIMAL(12, 4), 
		[Change] DECIMAL(12, 4)
	);

	/* Calculate the change values for each quote */
	INSERT INTO @changeCalcs 
	SELECT [Id] as [QuoteId], 
	       [CompanyId], 
	       [Date], 
	       [Close], 
	       [Close] - LAG([Close], 1) OVER (ORDER BY [Id]) as [Change]
	FROM StockMarketData.dbo.Quotes
	WHERE [CompanyId] = @companyId
	ORDER BY [Date], [Id]

	/*********************************************************************************************
		Table to hold RSI gain/loss calculations for the @rsiPeriod
	*********************************************************************************************/
	DECLARE @gainLossCalculations TABLE
	(
		[QuoteId] INT, 
		[Date] DATE,
		[CurrentGain] DECIMAL(12, 4), 
		[AverageGain] DECIMAL(12, 4), 
		[CurrentLoss] DECIMAL(12, 4), 
		[AverageLoss] DECIMAL(12, 4)
	);
	
	/* Calculate the average gain/loss over the @rsiPeriod for each quote */
	INSERT INTO @gainLossCalculations
	SELECT [QuoteId],
			[Date],
			CASE WHEN [Change] > 0 THEN [Change] ELSE 0 END as [CurrentGain],
			( SELECT AVG(CASE WHEN [Change] > 0 THEN [Change] ELSE 0 END) 
			  FROM (SELECT [Change] FROM @changeCalcs changeInner 
			  WHERE changeInner.QuoteId <= changeOuter.QuoteId AND changeInner.QuoteId >= (changeOuter.QuoteId - @rsiPeriod)) as AverageGainInner) as [AverageGain],
			CASE WHEN [Change] < 0 THEN ABS([Change]) ELSE 0 END  as [CurrentLoss],
			( SELECT AVG(ABS(CASE WHEN [Change] < 0 THEN [Change] ELSE 0 END)) 
			  FROM (SELECT [Change] FROM @changeCalcs changeInner 
			  WHERE changeInner.QuoteId <= changeOuter.QuoteId AND changeInner.QuoteId >= (changeOuter.QuoteId - @rsiPeriod)) as AverageLossInner) as [AverageLoss]
	FROM @changeCalcs changeOuter
	ORDER BY [Date]

	/*********************************************************************************************
		Table to hold the RS values over the @rsiPeriod for each quote. 
	*********************************************************************************************/
	DECLARE @rsiRSCalculations TABLE 
	(
		[QuoteId] INT,
		[Date] DATE,
		[RS] DECIMAL(12, 4)
	)

	INSERT INTO @rsiRSCalculations
	SELECT [QuoteId], 
		   [Date],
	       CASE WHEN AverageLoss = 0 AND LAG([AverageLoss], 1) OVER (ORDER BY [QuoteId]) = 0 
	       	    THEN 100000.00 
				ELSE (((LAG([AverageGain], 1) OVER (ORDER BY [QuoteId]) * (@rsiPeriod - 1)) + [CurrentGain]) / @rsiPeriod) / (((LAG([AverageLoss], 1) OVER (ORDER BY [QuoteId]) * (@rsiPeriod - 1)) + [CurrentLoss]) / @rsiPeriod) end as [RS]
	FROM @gainLossCalculations
	ORDER BY [Date]

	/*********************************************************************************************
		Table to hold the RSI values over the @rsiPeriod for each quote. 
	*********************************************************************************************/
	SELECT rsCalcs.QuoteId as [QuoteId], 
	       [CompanyId], 
	       rsCalcs.[Date], 
	    --   [Close], 
	    --   [Change], 
	    --   [CurrentGain], 
	    --   [AverageGain], 
		   --(LAG([AverageGain], 1) OVER (ORDER BY rsCalcs.[QuoteId]) * (@rsiPeriod - 1)) as [PreviousAverageGain],
	    --   [CurrentLoss], 
	    --   [AverageLoss], 
		   --(LAG([AverageLoss], 1) OVER (ORDER BY rsCalcs.[QuoteId]) * (@rsiPeriod - 1)) as [PreviousAverageLoss],
	    --   [RS],
	       100.00 - (100.00 / (1.00 + RS)) as RSI
	FROM @rsiRSCalculations rsCalcs
		INNER JOIN @changeCalcs changeCalcs ON rsCalcs.QuoteId = changeCalcs.QuoteId
		INNER JOIN @gainLossCalculations gainLossCalcs on rsCalcs.QuoteId = gainLossCalcs.QuoteId
	WHERE rsCalcs.[Date] >= @startDate AND rsCalcs.[Date] <= @endDate
END
GO


