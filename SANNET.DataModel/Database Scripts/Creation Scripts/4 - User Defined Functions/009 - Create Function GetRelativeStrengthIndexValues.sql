SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'TF' and name = 'GetRelativeStrengthIndexValues')
BEGIN
	PRINT 'Dropping "GetRelativeStrengthIndexValues" function...'
	DROP FUNCTION GetRelativeStrengthIndexValues
END
GO

PRINT 'Creating "GetRelativeStrengthIndexValues" function...'
GO

-- =============================================
-- Author:		Steve Whitmire Jr.
-- Create date: 08-19-2019
-- Description:	Returns the Relative Strength Index (RSI) calculations for a given company.
-- =============================================
CREATE FUNCTION [dbo].[GetRelativeStrengthIndexValues] 
(
	@companyId int,
	@startDate date,
	@endDate date,
	@rsiPeriod int
)
RETURNS @rsiValues TABLE
(
	[QuoteId] INT UNIQUE, 
	[RSI] DECIMAL(9, 4)
)
AS
BEGIN

	-- Verified 09-20-2019 --

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
	SELECT [QuoteId], [CompanyQuoteNum], [Date], [CompanyId], [Close]
	FROM GetCompanyQuotes(@companyId)
	WHERE [Date] <= @endDate
	ORDER BY [CompanyQuoteNum]

	/*********************************************************************************************
		Table to hold Change calculations for the @rsiPeriod.
	*********************************************************************************************/
	DECLARE @changeCalcs TABLE
	(
		[QuoteId] INT UNIQUE, 
		[CompanyQuoteNum] INT UNIQUE,
		[Change] DECIMAL(12, 4)
	);

	INSERT INTO @changeCalcs 
	SELECT [QuoteId], 
		   [CompanyQuoteNum],
	       [Close] - LAG([Close], 1) OVER (ORDER BY [QuoteId]) as [Change]
	FROM @companyQuotes

	/*********************************************************************************************
		Table to hold the current gain/loss calculations for the @rsiPeriod
	*********************************************************************************************/
	DECLARE @currentGainLossCalculations TABLE
	(
		[QuoteId] INT UNIQUE, 
		[CompanyQuoteNum] INT UNIQUE,
		[CurrentGain] DECIMAL(12, 4), 
		[CurrentLoss] DECIMAL(12, 4) 
	);

	INSERT INTO @currentGainLossCalculations
	SELECT  [QuoteId],
			[CompanyQuoteNum],
			CASE WHEN [Change] > 0 THEN [Change] ELSE 0 END as [CurrentGain],
			CASE WHEN [Change] < 0 THEN ABS([Change]) ELSE 0 END  as [CurrentLoss]
	FROM @changeCalcs changeOuter

	/*********************************************************************************************
		Table to hold the average gain/loss calculations for the @rsiPeriod
	*********************************************************************************************/
	DECLARE @avgGainLossCalculations TABLE
	(
		[QuoteId] INT UNIQUE, 
		[CompanyQuoteNum] INT UNIQUE,
		[AverageGain] DECIMAL(9, 4), 
		[AverageLoss] DECIMAL(9, 4)
	);

	DECLARE @previousAverageGain DECIMAL(9, 4),
			@previousAverageLoss DECIMAL(9, 4),
			@currentQuoteId INT,
			@currentAverageGain DECIMAL(9, 4),
			@currentAverageLoss DECIMAL(9, 4),
			@currentGain DECIMAL(9, 4),
			@currentLoss DECIMAL(9, 4),
			@minRowNum INT = (SELECT MIN([CompanyQuoteNum]) FROM @companyQuotes),
			@maxRowNum INT = (SELECT MAX([CompanyQuoteNum]) FROM @companyQuotes)
	DECLARE @currentRowNum INT = @minRowNum

	WHILE (@currentRowNum <= @maxRowNum)
	BEGIN
		SELECT @currentQuoteId = [QuoteId],
			   @currentGain = [CurrentGain],
			   @currentLoss = [CurrentLoss]
		FROM @currentGainLossCalculations
		WHERE [CompanyQuoteNum] = @currentRowNum

		-- First average gain = SMA(@rsiPeriod)
		-- Anything after that uses the @previousAverageGain/loss
		IF @currentRowNum = @rsiPeriod + 1
		BEGIN
			SELECT @currentAverageGain = AVG([CurrentGain]),
				   @currentAverageLoss = AVG([CurrentLoss])
			FROM @currentGainLossCalculations
			WHERE [CompanyQuoteNum] <= @currentRowNum AND [CompanyQuoteNum] >= [CompanyQuoteNum] - @rsiPeriod + 1
		END
		ELSE IF @currentRowNum > @rsiPeriod
		BEGIN
			SET @currentAverageGain = (((@previousAverageGain * (@rsiPeriod - 1)) + @currentGain) / @rsiPeriod)
			SET @currentAverageLoss = (((@previousAverageLoss * (@rsiPeriod - 1)) + @currentLoss) / @rsiPeriod)
		END
		ELSE
		BEGIN	
			SET @currentAverageGain = NULL
			SET @currentAverageLoss = NULL
		END

		INSERT INTO @avgGainLossCalculations ([QuoteId], [CompanyQuoteNum], [AverageGain], [AverageLoss]) 
		VALUES (@currentQuoteId, @currentRowNum, @currentAverageGain, @currentAverageLoss)

		SET @previousAverageGain = @currentAverageGain
		SET @previousAverageLoss = @currentAverageLoss
		SET @currentRowNum = @currentRowNum + 1
	END

	/*********************************************************************************************
		Table to hold the RSI values over the @rsiPeriod for each quote. 
	*********************************************************************************************/
	INSERT INTO @rsiValues
	SELECT quotes.[QuoteId], 
		   CASE WHEN [AverageLoss] <= 0
	       	    THEN 100.00
				ELSE 100.00 - (100.00 / (1.00 + ([AverageGain] / [AverageLoss])))
				END as [RSI]
	FROM @companyQuotes quotes 
			INNER JOIN @changeCalcs changeCalcs ON quotes.[QuoteId] = changeCalcs.[QuoteId]
			INNER JOIN @avgGainLossCalculations avgGainLossCalcs on quotes.[QuoteId] = avgGainLossCalcs.[QuoteId]
	WHERE quotes.[Date] >= @startDate AND quotes.[Date] <= @endDate
	ORDER BY quotes.[Date]

	RETURN;
END
GO