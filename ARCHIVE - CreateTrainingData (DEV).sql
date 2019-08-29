
	DECLARE @smd TABLE
	(
		[Id] INT,
		[CompanyId] INT,
		[Date] DATE,
		[Close] DECIMAL(10, 2),
		[Open] DECIMAL(10, 2)
	)

	INSERT INTO @smd values(0, 2, '01-01-2019', 70.00, 72.00);
	INSERT INTO @smd values(1, 2, '01-03-2019', 71.00, 73.00);
	INSERT INTO @smd values(2, 2, '01-04-2019', 67.00, 77.00);
	INSERT INTO @smd values(3, 2, '01-05-2019', 73.00, 73.00);
	INSERT INTO @smd values(4, 2, '01-06-2019', 74.00, 76.00);
	INSERT INTO @smd values(5, 2, '01-07-2019', 75.00, 71.00);
	INSERT INTO @smd values(6, 2, '01-08-2019', 76.00, 76.00);
	INSERT INTO @smd values(7, 2, '01-09-2019', 77.00, 73.00);
	INSERT INTO @smd values(8, 2, '01-10-2019', 78.00, 78.00);
	INSERT INTO @smd values(9, 2, '01-11-2019', 79.00, 79.00);
	INSERT INTO @smd values(10, 2, '01-12-2019', 80.00, 85.00);

	-- Dataset
	DECLARE @rsiShort TABLE
	(
		[QuoteId] INT,
		[CompanyId] INT,
		[Date] DATE,
		[RSIShort] DECIMAL(12, 4)
	)

	INSERT INTO @rsiShort values(0, 2, '01-01-2019', 1);
	INSERT INTO @rsiShort values(1, 2, '01-03-2019', 2);
	INSERT INTO @rsiShort values(2, 2, '01-04-2019', 3);
	INSERT INTO @rsiShort values(3, 2, '01-05-2019', 4);
	INSERT INTO @rsiShort values(4, 2, '01-06-2019', 5);
	INSERT INTO @rsiShort values(5, 2, '01-07-2019', 6);
	INSERT INTO @rsiShort values(6, 2, '01-08-2019', 5);
	INSERT INTO @rsiShort values(7, 2, '01-09-2019', 5);
	INSERT INTO @rsiShort values(8, 2, '01-10-2019', 5);
	INSERT INTO @rsiShort values(9, 2, '01-11-2019', 5);
	INSERT INTO @rsiShort values(10, 2, '01-12-2019', 5);

	DECLARE @rsiLong TABLE
	(
		[QuoteId] INT,
		[CompanyId] INT,
		[Date] DATE,
		[RSILong] DECIMAL(12, 4)
	)

	INSERT INTO @rsiLong values(0, 2, '01-01-2019', 2);
	INSERT INTO @rsiLong values(1, 2, '01-03-2019', 3);
	INSERT INTO @rsiLong values(2, 2, '01-04-2019', 4);
	INSERT INTO @rsiLong values(3, 2, '01-05-2019', 3);
	INSERT INTO @rsiLong values(4, 2, '01-06-2019', 6);
	INSERT INTO @rsiLong values(5, 2, '01-07-2019', 7);
	INSERT INTO @rsiLong values(6, 2, '01-08-2019', 8);
	INSERT INTO @rsiLong values(7, 2, '01-09-2019', 9);
	INSERT INTO @rsiLong values(8, 2, '01-10-2019', 4);
	INSERT INTO @rsiLong values(9, 2, '01-11-2019', 4);
	INSERT INTO @rsiLong values(10, 2, '01-12-2019', 4);

	DECLARE @rsiCrossovers DateValueCrossoversType

	INSERT INTO @rsiCrossovers
	SELECT short.QuoteId, short.[Date], short.RSIShort, long.RSILong 
	FROM @rsiShort short INNER JOIN @rsiLong long ON short.QuoteId = long.QuoteId
	
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
	FROM @smd

	DECLARE @combinedDataset TABLE
	(
		[QuoteId] INT,
		[CompanyId] INT,
		[Date] DATE,
		[RSIShort] DECIMAL(12, 4),
		[RSILong] DECIMAL(12, 4),
		[RSIConsecutiveDaysShortAboveLong] INT,
		[RSIConsecutiveDaysLongAboveShort] INT,
		--[CCIShort] DECIMAL(12, 4),
		--[CCILong] DECIMAL(12, 4),
		--[CCIConsecutiveDaysShortAboveLong] INT,
		--[CCIConsecutiveDaysLongAboveShort] INT
		[WeekDidStockRise4PercentFirst] BIT,
		[WeekDidStockFall2PercentFirst] BIT
	)

	INSERT INTO @combinedDataset
	SELECT rsiShort.*,
		   rsiLong.[RSILong],
		   rsiCrossovers.[ConsecutiveDaysValueOneAboveValueTwo] as [RSIConsecutiveDaysShortAboveLong],
		   rsiCrossovers.[ConsecutiveDaysValueTwoAboveValueOne] as [RSIConsecutiveDaysLongAboveShort],
		   CASE WHEN [WeekOutcomeType] = 1 
				THEN 1 ELSE 0 END as [WeekDidStockRise4PercentFirst],
		   CASE WHEN [WeekOutcomeType] = -1 
				THEN 1 ELSE 0 END as [WeekDidStockFall2PercentFirst]
	FROM @rsiShort rsiShort INNER JOIN @rsiLong rsiLong ON rsiShort.QuoteId = rsiLong.QuoteId
							INNER JOIN GetDateValueCrossovers(@rsiCrossovers) rsiCrossovers ON rsiCrossovers.Id = rsiShort.QuoteId
							INNER JOIN @weekPerformanceOutputs outputs ON outputs.QuoteId = rsiShort.QuoteId

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

	----INSERT INTO @combinedDatasetNormalized
	--SELECT [QuoteId],
	--	   [CompanyId],
	--	   [Date],
	--	   RSIShort / 100.0 as [RSIShort],
	--	   RSILong / 100.0 as [RSILong],
	--	   ConsecutiveDaysShortAboveLong / 10.0 as [ConsecutiveDaysShortAboveLong],
	--	   ConsecutiveDaysLongAboveShort / 10.0 as [ConsecutiveDaysLongAboveShort]
	--FROM @combinedDataset

