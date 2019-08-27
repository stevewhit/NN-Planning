SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Steve Whitmire Jr.
-- Create date: 08-26-2019
-- Description:	Returns a training dataset consisting of technical indicators
--				and other required signals.
-- =============================================
ALTER PROCEDURE [dbo].[GetTrainingDataset]
AS
BEGIN
	DECLARE @companyId INT = 1
	DECLARE @startDate DATE = '01-01-2018'
	DECLARE @endDate DATE = '01-01-2019'
	DECLARE @rsiShortPeriod INT = 15
	DECLARE @rsiLongPeriod INT = 50
	DECLARE @cciShortPeriod INT = 15
	DECLARE @cciLongPeriod INT = 50
	DECLARE @smaShortPeriod INT = 15
	DECLARE @smaLongPeriod INT = 50

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
	EXECUTE GetRelativeStrengthIndex @companyId, @startDate, @endDate, @rsiShortPeriod

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
	EXECUTE GetRelativeStrengthIndex @companyId, @startDate, @endDate, @rsiLongPeriod

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
	EXECUTE GetCommodityChannelIndex @companyId, @startDate, @endDate, @cciShortPeriod

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
	EXECUTE GetRelativeStrengthIndex @companyId, @startDate, @endDate, @cciLongPeriod

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
	EXECUTE GetSimpleMovingAverage @companyId, @startDate, @endDate, @smaShortPeriod

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
	EXECUTE GetSimpleMovingAverage @companyId, @startDate, @endDate, @smaLongPeriod

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

	/*********************************************************************************************
		Table to hold the weekly performance outputs
	*********************************************************************************************/
	DECLARE @weekPerformanceOutputs TABLE
	(
		[QuoteId] INT,
		[CompanyId] INT,
		[Date] DATE,
		[WeekOutcomeType] INT
	)

	INSERT INTO @weekPerformanceOutputs
	SELECT [Id] as [QuoteId],
		   [CompanyId],
		   [Date],
		   CASE WHEN LEAD([Close], 1) OVER (ORDER BY [Date]) > [Close] * 1.04 THEN 1
				ELSE CASE WHEN LEAD([Close], 1) OVER (ORDER BY [Date]) < [Close] * .98 THEN -1
					 ELSE CASE WHEN LEAD([Close], 2) OVER (ORDER BY [Date]) > [Close] * 1.04 THEN 1
						  ELSE CASE WHEN LEAD([Close], 2) OVER (ORDER BY [Date]) < [Close] * .98 THEN -1
							   ELSE CASE WHEN LEAD([Close], 3) OVER (ORDER BY [Date]) > [Close] * 1.04 THEN 1
								    ELSE CASE WHEN LEAD([Close], 3) OVER (ORDER BY [Date]) < [Close] * .98 THEN -1
									     ELSE CASE WHEN LEAD([Close], 4) OVER (ORDER BY [Date]) > [Close] * 1.04 THEN 1
											  ELSE CASE WHEN LEAD([Close], 4) OVER (ORDER BY [Date]) < [Close] * .98 THEN -1
												   ELSE CASE WHEN LEAD([Close], 5) OVER (ORDER BY [Date]) > [Close] * 1.04 THEN 1
													    ELSE CASE WHEN LEAD([Close], 5) OVER (ORDER BY [Date]) < [Close] * .98 THEN -1
															 ELSE 0
														END END END END END END END END END END as [WeekOutcomeType]
	FROM StockMarketData.dbo.Quotes

	/*********************************************************************************************
		Table to hold all non-normalized indicator values.
	*********************************************************************************************/
	DECLARE @combinedDataset TABLE
	(
		[QuoteId] INT,
		[CompanyId] INT,
		[Date] DATE,
		[RSIShort] DECIMAL(12, 4),
		[RSILong] DECIMAL(12, 4),
		[RSIConsecutiveDaysShortAboveLong] INT,
		[RSIConsecutiveDaysLongAboveShort] INT,
		[CCIShort] DECIMAL(12, 4),
		[CCILong] DECIMAL(12, 4),
		[CCIConsecutiveDaysShortAboveLong] INT,
		[CCIConsecutiveDaysLongAboveShort] INT,
		[SMAShort] DECIMAL(12, 4),
		[SMALong] DECIMAL(12, 4),
		[SMAConsecutiveDaysShortAboveLong] INT,
		[SMAConsecutiveDaysLongAboveShort] INT,
		[WeekDidStockRise4PercentFirst] BIT,
		[WeekDidStockFall2PercentFirst] BIT
	)

	INSERT INTO @combinedDataset
	SELECT rsiShort.*,
		   rsiLong.[RSILong],
		   rsiCrossovers.[ConsecutiveDaysValueOneAboveValueTwo] as [RSIConsecutiveDaysShortAboveLong],
		   rsiCrossovers.[ConsecutiveDaysValueTwoAboveValueOne] as [RSIConsecutiveDaysLongAboveShort],
		   [CCIShort],
		   [CCILong],
		   cciCrossovers.[ConsecutiveDaysValueOneAboveValueTwo] as [CCIConsecutiveDaysShortAboveLong],
		   cciCrossovers.[ConsecutiveDaysValueTwoAboveValueOne] as [CCIConsecutiveDaysLongAboveShort],
		   [SMAShort],
		   [SMALong],
		   smaCrossovers.[ConsecutiveDaysValueOneAboveValueTwo] as [SMAConsecutiveDaysShortAboveLong],
		   smaCrossovers.[ConsecutiveDaysValueTwoAboveValueOne] as [SMAConsecutiveDaysLongAboveShort],
		   CASE WHEN [WeekOutcomeType] = 1 
				THEN 1 ELSE 0 END as [WeekDidStockRise4PercentFirst],
		   CASE WHEN [WeekOutcomeType] = -1 
				THEN 1 ELSE 0 END as [WeekDidStockFall2PercentFirst]
	FROM @rsiShort rsiShort 
			INNER JOIN @rsiLong rsiLong ON rsiShort.QuoteId = rsiLong.QuoteId
			INNER JOIN GetDateValueCrossovers(@rsiCrossovers) rsiCrossovers ON rsiCrossovers.Id = rsiShort.QuoteId
			INNER JOIN @cciShort cciShort ON cciShort.QuoteId = rsiShort.QuoteId
			INNER JOIN @cciLong cciLong ON cciLong.QuoteId = rsiShort.QuoteId
			INNER JOIN GetDateValueCrossovers(@cciCrossovers) cciCrossovers ON cciCrossovers.Id = rsiShort.QuoteId
			INNER JOIN @smaShort smaShort ON smaShort.QuoteId = rsiShort.QuoteId
			INNER JOIN @smaLong smaLong ON smaLong.QuoteId = rsiShort.QuoteId
			INNER JOIN GetDateValueCrossovers(@smaCrossovers) smaCrossovers ON smaCrossovers.Id = rsiShort.QuoteId
			INNER JOIN @weekPerformanceOutputs outputs ON outputs.QuoteId = rsiShort.QuoteId

	-- TEMP
	SELECT * FROM @combinedDataset


	--DECLARE @combinedDatasetNormalized TABLE
	--(
	--	[QuoteId] INT,
	--	[CompanyId] INT,
	--	[Date] DATE,
	--	[RSIShort] DECIMAL(12, 4),
	--	[RSILong] DECIMAL(12, 4),
	--	[ConsecutiveDaysShortAboveLong] INT,
	--	[ConsecutiveDaysLongAboveShort] INT
	--)

	--INSERT INTO @combinedDatasetNormalized
	--SELECT [QuoteId],
	--	   [CompanyId],
	--	   [Date],
	--	   RSIShort / 100.0 as [RSIShort],
	--	   RSILong / 100.0 as [RSILong],
	--	   ConsecutiveDaysShortAboveLong / 10.0 as [ConsecutiveDaysShortAboveLong],
	--	   ConsecutiveDaysLongAboveShort / 10.0 as [ConsecutiveDaysLongAboveShort]
	--FROM @combinedDataset
END
GO


