USE [SANNET]
GO

/****** Object:  StoredProcedure [dbo].[GetRelativeStrengthIndex]    Script Date: 8/19/2019 11:40:15 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		Steve Whitmire Jr.
-- Create date: 08-19-2019
-- Description:	Returns the Relative Strength Index (RSI) calculations for a given company.
-- =============================================
CREATE PROCEDURE [dbo].[GetRelativeStrengthIndex] 
	@companyId int = 0, 
	@startDate date = null,
	@endDate date = null,
	@rsiPeriod int = 1
AS
BEGIN
	SET NOCOUNT ON;

	IF OBJECT_ID('tempdb..#rsiChangeCalculations') IS NOT NULL
	BEGIN
		DROP TABLE #rsiChangeCalculations
	END

	CREATE TABLE #rsiChangeCalculations
	(
		[CompanyId] INT, [Date] DATE, [Close] DECIMAL(12, 4), [Change] DECIMAL(12, 4)
	);

	IF OBJECT_ID('tempdb..#rsiGainLossCalculations') IS NOT NULL
	BEGIN
		DROP TABLE #rsiGainLossCalculations
	END

	CREATE TABLE #rsiGainLossCalculations
	(
		[CompanyId] INT, [Date] DATE, [Close] DECIMAL(12, 4), [Change] DECIMAL(12, 4), [CurrentGain] DECIMAL(12, 4), [SumGain] DECIMAL(12, 4), [AverageGain] DECIMAL(12, 4), [CurrentLoss] DECIMAL(12, 4), [SumLoss] DECIMAL(12, 4), [AverageLoss] DECIMAL(12, 4)
	);

	-- Calculate the change values for each date
	INSERT INTO #rsiChangeCalculations ([CompanyId], [Date], [Close], [Change])
	SELECT [CompanyId], [Date], [Close], [Close] - LAG([Close], 1) OVER (ORDER BY Date) as Change
	FROM StockMarketData.dbo.Quotes
	WHERE CompanyId = @companyId
	ORDER BY Date

	-- Calculate the average gain/loss for each date
	DECLARE @sql nvarchar(max) = N' INSERT INTO #rsiGainLossCalculations
									SELECT [CompanyId], [Date], [Close], [Change], 
									case when [Change] > 0 then [Change] else 0 end as CurrentGain,
									SUM(case when [Change] > 0 then [Change] else 0 end) OVER (ORDER BY [Date] ROWS ' + convert(varchar, @rsiPeriod) +' PRECEDING) as SumGain, 
									AVG(case when [Change] > 0 then [Change] else 0 end) OVER (ORDER BY [Date] ROWS ' + convert(varchar, @rsiPeriod) +' PRECEDING) as AverageGain, 
									case when [Change] < 0 then ABS([Change]) else 0 end as CurrentLoss,
									ABS(SUM(case when [Change] < 0 then [Change] else 0 end) OVER (ORDER BY [Date] ROWS ' + convert(varchar, @rsiPeriod) +' PRECEDING)) as SumLoss, 
									ABS(AVG(case when [Change] < 0 then [Change] else 0 end) OVER (ORDER BY [Date] ROWS ' + convert(varchar, @rsiPeriod) +' PRECEDING)) as AverageLoss
									FROM #rsiChangeCalculations
									ORDER BY Date'
	EXEC sp_executesql @sql

	-- Calculate the RS values
	DECLARE @rsiRSCalculations TABLE 
	(
		[CompanyId] INT, [Date] DATE, [Close] DECIMAL(12, 4), [Change] DECIMAL(12, 4), [CurrentGain] DECIMAL(12, 4), [SumGain] DECIMAL(12, 4), [AverageGain] DECIMAL(12, 4), [PreviousAverageGain] DECIMAL(12, 4), [CurrentLoss] DECIMAL(12, 4), [SumLoss] DECIMAL(12, 4), [AverageLoss] DECIMAL(12, 4), [PreviousAverageLoss] DECIMAL(12, 4), [RS] DECIMAL(12, 4)
	)

	INSERT INTO @rsiRSCalculations
	SELECT [CompanyId], [Date], [Close], [Change], 
	[CurrentGain], [SumGain], [AverageGain], 
	LAG([AverageGain], 1) OVER (ORDER BY Date) as [PreviousAverageGain],
	[CurrentLoss], [SumLoss],
	[AverageLoss],
	LAG([AverageLoss], 1) OVER (ORDER BY Date) as [PreviousAverageLoss],
	case when AverageLoss = 0 AND LAG([AverageLoss], 1) OVER (ORDER BY Date) = 0 
		 then 100000.00 
		 else (((LAG([AverageGain], 1) OVER (ORDER BY Date) * (@rsiPeriod - 1)) + [CurrentGain]) / @rsiPeriod) / (((LAG([AverageLoss], 1) OVER (ORDER BY Date) * (@rsiPeriod - 1)) + [CurrentLoss]) / @rsiPeriod) end as RS
	FROM #rsiGainLossCalculations

	DROP TABLE #rsiChangeCalculations
	DROP TABLE #rsiGainLossCalculations

	-- Calculate RSI values
	SELECT [CompanyId], [Date], [Close], [Change], [CurrentGain], [SumGain], [AverageGain], [PreviousAverageGain], [CurrentLoss], [SumLoss], [AverageLoss], [PreviousAverageLoss], [RS],
	100.00 - (100.00 / (1.00 + RS)) as RSI
	FROM @rsiRSCalculations
	WHERE date >= @startDate AND date <= @endDate
END
GO


