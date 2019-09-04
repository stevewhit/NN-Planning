SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' and name = 'GetTrainingDataset1')
BEGIN
	PRINT 'Dropping "GetTrainingDataset1" stored procedure...'
	DROP PROCEDURE GetTrainingDataset1
END
GO

PRINT 'Creating "GetTrainingDataset1" stored procedure...'
GO

-- =============================================
-- Author:		Steve Whitmire Jr.
-- Create date: 09-03-2019
-- Description:	Calculates and returns the training dataset using method 1 for the dates between @startDate and @endDate.
-- =============================================
CREATE PROCEDURE [dbo].[GetTrainingDataset1] 
	@companyId int,
	@startDate date,
	@endDate date
AS
BEGIN
	SET NOCOUNT ON;

	/***************************
		RSI tables
	****************************/
	DECLARE @rsiValuesShort TABLE ([QuoteId] INT, [CompanyId] INT, [Date] DATE, [RSI] DECIMAL(12, 2));
	INSERT INTO @rsiValuesShort 
	EXECUTE GetRelativeStrengthIndex @companyId, @startDate, @endDate, 7

	DECLARE @rsiValuesLong TABLE ([QuoteId] INT, [CompanyId] INT, [Date] DATE, [RSI] DECIMAL(12, 2));
	INSERT INTO @rsiValuesLong 
	EXECUTE GetRelativeStrengthIndex @companyId, @startDate, @endDate, 14

	----- 

	DECLARE @rsiValuesShortCross DateValues;
	INSERT INTO @rsiValuesShortCross SELECT [QuoteId], [Date], [RSI] FROM @rsiValuesShort

	DECLARE @rsiValuesLongCross DateValues;
	INSERT INTO @rsiValuesLongCross SELECT [QuoteId], [Date], [RSI] FROM @rsiValuesLong

	DECLARE @rsiValuesCross TABLE ([QuoteId] INT, [Date] DATE, [ConsecutiveDaysShortAboveLong] INT, [ConsecutiveDaysLongAboveShort] INT);
	INSERT INTO @rsiValuesCross 
	SELECT [Id] as [QuoteId],
		   [Date],
		   [ConsecutiveDaysValueOneAboveValueTwo] as [ConsecutiveDaysShortAboveLong],
		   [ConsecutiveDaysValueTwoAboveValueOne] as [ConsecutiveDaysLongAboveShort]
	FROM GetConsecutiveDayValueCrossovers( @rsiValuesShortCross, @rsiValuesLongCross)

	/***************************
		CCI tables
	****************************/
	DECLARE @cciValuesShort TABLE ([QuoteId] INT, [CompanyId] INT, [Date] DATE, [CCI] DECIMAL(12, 2));
	INSERT INTO @cciValuesShort 
	EXECUTE GetCommodityChannelIndex @companyId, @startDate, @endDate, 7

	DECLARE @cciValuesLong TABLE ([QuoteId] INT, [CompanyId] INT, [Date] DATE, [CCI] DECIMAL(12, 2));
	INSERT INTO @cciValuesLong 
	EXECUTE GetCommodityChannelIndex @companyId, @startDate, @endDate, 14

	/***************************
		SMA tables
	****************************/
	DECLARE @smaValuesShort TABLE ([QuoteId] INT, [CompanyId] INT, [Date] DATE, [Close] DECIMAL(12, 2), [SMA] DECIMAL(12, 2));
	INSERT INTO @smaValuesShort 
	EXECUTE GetSimpleMovingAverage @companyId, @startDate, @endDate, 7

	DECLARE @smaValuesLong TABLE ([QuoteId] INT, [CompanyId] INT, [Date] DATE, [Close] DECIMAL(12, 2), [SMA] DECIMAL(12, 2));
	INSERT INTO @smaValuesLong 
	EXECUTE GetSimpleMovingAverage @companyId, @startDate, @endDate, 14

	----- 

	DECLARE @smaValuesShortCross DateValues;
	INSERT INTO @smaValuesShortCross SELECT [QuoteId], [Date], [SMA] FROM @smaValuesShort

	DECLARE @smaValuesLongCross DateValues;
	INSERT INTO @smaValuesLongCross SELECT [QuoteId], [Date], [SMA] FROM @smaValuesLong

	DECLARE @smaValuesCross TABLE ([QuoteId] INT, [Date] DATE, [ConsecutiveDaysShortAboveLong] INT, [ConsecutiveDaysLongAboveShort] INT);
	INSERT INTO @smaValuesCross 
	SELECT [Id] as [QuoteId],
		   [Date],
		   [ConsecutiveDaysValueOneAboveValueTwo] as [ConsecutiveDaysShortAboveLong],
		   [ConsecutiveDaysValueTwoAboveValueOne] as [ConsecutiveDaysLongAboveShort]
	FROM GetConsecutiveDayValueCrossovers( @smaValuesShortCross, @smaValuesLongCross)

	/***************************
		Future Five Day Performance
	****************************/
	DECLARE @futureFiveDayPerformance TABLE ([QuoteId] INT, [CompanyId] INT, [Date] DATE, [TriggeredRiseFirst] BIT, [TriggeredFallFirst] BIT);
	INSERT INTO @futureFiveDayPerformance 
	EXECUTE GetFutureFiveDayPerformance @companyId, @startDate, @endDate, 1.04, .98

	/***************************
		All combined indicators
	****************************/
	DECLARE @combinedIndicatorValues TABLE(
		[Date] DATE, 
		[CompanyId] INT,
		[QuoteId] INT,
		[Close] DECIMAL(12, 2),
		--[Open] DECIMAL(12, 2),
		--[High] DECIMAL(12, 2),
		--[Low] DECIMAL(12, 2),
		[RSIShort] DECIMAL(12, 2), 
		[RSILong] DECIMAL(12, 2),  
		[RSIConsecutiveDaysShortAboveLong] INT,
		[RSIConsecutiveDaysLongAboveShort] INT,
		[CCIShort] DECIMAL(12, 2), 
		[CCILong] DECIMAL(12, 2), 
		[SMAShort] DECIMAL(12, 2), 
		[SMALong] DECIMAL(12, 2), 
		[SMAConsecutiveDaysShortAboveLong] INT,
		[SMAConsecutiveDaysLongAboveShort] INT,
		[Output_TriggeredRiseFirst] BIT,
		[Output_TriggeredFallFirst] BIT);

	INSERT INTO @combinedIndicatorValues
	SELECT rsiShort.[Date],
		   rsiShort.[CompanyId],
		   rsiShort.[QuoteId],
		   quotes.[Close],
		   rsiShort.RSI as [RSIShort],
		   rsiLong.RSI as [RSILong],
		   rsiCross.ConsecutiveDaysShortAboveLong as [RSIConsecutiveDaysShortAboveLong],
		   rsiCross.ConsecutiveDaysLongAboveShort as [RSIConsecutiveDaysLongAboveShort],
		   cciShort.CCI as [CCIShort],
		   cciLong.CCI as [CCILong],
		   smaShort.SMA as [SMAShort],
		   smaLong.SMA as [SMALong],
		   smaCross.ConsecutiveDaysShortAboveLong as [SMAConsecutiveDaysShortAboveLong],
		   smaCross.ConsecutiveDaysLongAboveShort as [SMAConsecutiveDaysLongAboveShort],
		   fiveDayPerformance.TriggeredRiseFirst as [Output_TriggeredRiseFirst],
		   fiveDayPerformance.TriggeredFallFirst as [Output_TriggeredFallFirst]
	FROM @rsiValuesShort rsiShort 
			INNER JOIN @rsiValuesLong rsiLong ON rsiShort.[QuoteId] = rsiLong.[QuoteId]
			INNER JOIN @rsiValuesCross rsiCross ON rsiShort.[QuoteId] = rsiCross.[QuoteId]
			INNER JOIN @cciValuesShort cciShort ON rsiShort.[QuoteId] = cciShort.[QuoteId]
			INNER JOIN @cciValuesLong cciLong ON rsiShort.[QuoteId] = cciLong.[QuoteId]
			INNER JOIN @smaValuesShort smaShort ON rsiShort.[QuoteId] = smaShort.[QuoteId]
			INNER JOIN @smaValuesLong smaLong ON rsiShort.[QuoteId] = smaLong.[QuoteId]
			INNER JOIN @smaValuesCross smaCross ON rsiShort.[QuoteId] = smaCross.[QuoteId]
			INNER JOIN StockMarketData.dbo.Quotes quotes ON rsiShort.QuoteId = quotes.Id
			INNER JOIN @futureFiveDayPerformance fiveDayPerformance ON rsiShort.QuoteId = fiveDayPerformance.QuoteId
	WHERE rsiShort.CompanyId = @companyId AND rsiShort.Date >= @startDate AND rsiShort.Date <= @endDate

	/***************************
		Normalized values
	****************************/
	SELECT [Date],
		   [CompanyId],

		   -- RSI Short
		   [RSIShort] / 100.0 as [RSIShortNormalized],
		   CASE WHEN [RSIShort] >= 70 THEN 1 ELSE 0 END [IsRSIShortOverBought],
		   CASE WHEN [RSIShort] <= 30 THEN 1 ELSE 0 END [IsRSIShortOverSold],
		   CASE WHEN [RSIShort] >= 70 AND (LAG([RSIShort], 1) OVER (ORDER BY [QuoteId])) < 70 THEN 1 ELSE 0 END [RSIShortJustCrossedIntoOverBought],
		   CASE WHEN [RSIShort] <= 30 AND (LAG([RSIShort], 1) OVER (ORDER BY [QuoteId])) > 30 THEN 1 ELSE 0 END [RSIShortJustCrossedIntoOverSold],

		   -- RSI Long
		   [RSILong] / 100.0 as [RSILongNormalized],
		   CASE WHEN [RSILong] >= 70 THEN 1 ELSE 0 END [IsRSILongOverBought],
		   CASE WHEN [RSILong] <= 30 THEN 1 ELSE 0 END [IsRSILongOverSold],
		   CASE WHEN [RSILong] >= 70 AND (LAG([RSILong], 1) OVER (ORDER BY [QuoteId])) < 70 THEN 1 ELSE 0 END [RSILongJustCrossedIntoOverBought],
		   CASE WHEN [RSILong] <= 30 AND (LAG([RSILong], 1) OVER (ORDER BY [QuoteId])) > 30 THEN 1 ELSE 0 END [RSILongJustCrossedIntoOverSold],

		   -- RSI Crosses
		   CASE WHEN [RSIConsecutiveDaysShortAboveLong] <= 3 AND [RSIConsecutiveDaysShortAboveLong] >= 1 THEN 1 ELSE 0 END [RSIShortJustCrossedOverLong],
		   CASE WHEN [RSIConsecutiveDaysShortAboveLong] > 3 THEN 1 ELSE 0 END [RSIShortGreaterThanLongForAwhile],
		   CASE WHEN [RSIConsecutiveDaysLongAboveShort] <= 3 AND [RSIConsecutiveDaysLongAboveShort] >= 1 THEN 1 ELSE 0 END [RSILongJustCrossedOverShort],
		   CASE WHEN [RSIConsecutiveDaysLongAboveShort] > 3 THEN 1 ELSE 0 END [RSILongGreaterThanShortForAwhile],

		   -- CCI Short
		   CASE WHEN [CCIShort] >= 0 AND (LAG([CCIShort], 1) OVER (ORDER BY [QuoteId])) < 0 THEN 1 ELSE 0 END [CCIShortJustCrossedAboveZero],
		   CASE WHEN [CCIShort] <= 0 AND (LAG([CCIShort], 1) OVER (ORDER BY [QuoteId])) > 0 THEN 1 ELSE 0 END [CCIShortJustCrossedBelowZero],

		   -- CCI Long
		   CASE WHEN [CCILong] >= 0 AND (LAG([CCILong], 1) OVER (ORDER BY [QuoteId])) < 0 THEN 1 ELSE 0 END [CCILongJustCrossedAboveZero],
		   CASE WHEN [CCILong] <= 0 AND (LAG([CCILong], 1) OVER (ORDER BY [QuoteId])) > 0 THEN 1 ELSE 0 END [CCILongJustCrossedBelowZero],

		   -- SMA Short
		   CASE WHEN [SMAShort] > [Close] THEN 1 ELSE 0 END [SMAShortAboveClose],

		   -- SMA Long
		   CASE WHEN [SMALong] > [Close] THEN 1 ELSE 0 END [SMALongAboveClose],

		   -- SMA Crosses
		   CASE WHEN [SMAConsecutiveDaysShortAboveLong] <= 3 AND [SMAConsecutiveDaysShortAboveLong] >= 1 THEN 1 ELSE 0 END [SMAShortJustCrossedOverLong],
		   CASE WHEN [SMAConsecutiveDaysShortAboveLong] > 3 THEN 1 ELSE 0 END [SMAShortGreaterThanLongForAwhile],
		   CASE WHEN [SMAConsecutiveDaysLongAboveShort] <= 3 AND [SMAConsecutiveDaysLongAboveShort] >= 1 THEN 1 ELSE 0 END [SMALongJustCrossedOverShort],
		   CASE WHEN [SMAConsecutiveDaysLongAboveShort] > 3 THEN 1 ELSE 0 END [SMALongGreaterThanShortForAwhile],

		   -- Outputs
		   CASE WHEN [Output_TriggeredRiseFirst] = 1 THEN 1 ELSE 0 END [Output_TriggeredRiseFirst],
		   CASE WHEN [Output_TriggeredFallFirst] = 1 THEN 1 ELSE 0 END [Output_TriggeredFallFirst]
	FROM @combinedIndicatorValues
END
GO


