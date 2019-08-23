
	DECLARE @companyId INT = 2
	DECLARE @startDate DATE = '01-01-2018'
	DECLARE @endDate DATE = '01-01-2019'
	DECLARE @rsiShortPeriod INT = 15
	DECLARE @rsiLongPeriod INT = 50
	DECLARE @cciShortPeriod INT = 15
	DECLARE @cciLongPeriod INT = 50

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
		[CCIConsecutiveDaysLongAboveShort] INT
	)

	--INSERT INTO @combinedDataset
	SELECT rsiShort.*,
		   rsiLong.[RSILong],
		   CASE WHEN rsiShort.RSIShort < rsiLong.RSILong
			    THEN 0
				ELSE (SELECT COUNT(*)+1 
					  FROM @rsiShort rsiShort1 
					  WHERE rsiShort1.Date < rsiShort.[Date] AND 
							rsiShort1.Date > (SELECT MAX(rsiLong2.[Date]) 
										      FROM @rsiShort rsiShort2 INNER JOIN @rsiLong rsiLong2 ON rsiShort2.QuoteId = rsiLong2.QuoteId 
										      WHERE rsiLong2.[Date] < rsiShort.[Date] AND rsiLong2.RSILong > rsiShort2.RSIShort))
				END as [RSIConsecutiveDaysShortAboveLong],
		   CASE WHEN rsiShort.RSIShort > rsiLong.RSILong
			    THEN 0
				ELSE (SELECT COUNT(*)+1 
					  FROM @rsiShort rsiShort1 
					  WHERE rsiShort1.Date < rsiShort.[Date] AND 
							rsiShort1.Date > (SELECT MAX(rsiLong2.[Date]) 
										      FROM @rsiShort rsiShort2 INNER JOIN @rsiLong rsiLong2 ON rsiShort2.QuoteId = rsiLong2.QuoteId 
										      WHERE rsiLong2.[Date] < rsiShort.[Date] AND rsiLong2.RSILong < rsiShort2.RSIShort))
				END as [RSIConsecutiveDaysLongAboveShort],
		   [CCIShort],
		   [CCILong],
		   CASE WHEN cciShort.CCIShort < cciLong.CCILong
			    THEN 0
				ELSE (SELECT COUNT(*)+1 
					  FROM @cciShort cciShort1 
					  WHERE cciShort1.Date < cciShort.[Date] AND 
							cciShort1.Date > (SELECT MAX(cciLong2.[Date]) 
										      FROM @cciShort cciShort2 INNER JOIN @cciLong cciLong2 ON cciShort2.QuoteId = cciLong2.QuoteId 
										      WHERE cciLong2.[Date] < cciShort.[Date] AND cciLong2.CCILong > cciShort2.CCIShort))
				END as [CCIConsecutiveDaysShortAboveLong],
		   CASE WHEN cciShort.CCIShort > cciLong.CCILong
			    THEN 0
				ELSE (SELECT COUNT(*)+1 
					  FROM @cciShort cciShort1 
					  WHERE cciShort1.Date < cciShort.[Date] AND 
							cciShort1.Date > (SELECT MAX(cciLong2.[Date]) 
										      FROM @cciShort cciShort2 INNER JOIN @cciLong cciLong2 ON cciShort2.QuoteId = cciLong2.QuoteId 
										      WHERE cciLong2.[Date] < cciShort.[Date] AND cciLong2.CCILong < cciShort2.CCIShort))
				END as [CCIConsecutiveDaysLongAboveShort]
	FROM @rsiShort rsiShort 
			INNER JOIN @rsiLong rsiLong ON rsiShort.QuoteId = rsiLong.QuoteId
			INNER JOIN @cciShort cciShort ON cciShort.QuoteId = rsiShort.QuoteId
			INNER JOIN @cciLong cciLong ON cciLong.QuoteId = rsiShort.QuoteId

	


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