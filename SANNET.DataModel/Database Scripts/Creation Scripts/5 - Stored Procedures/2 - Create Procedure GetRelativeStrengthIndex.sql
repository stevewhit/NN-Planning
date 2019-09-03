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

	-- Verified 09-03-2019 --

	/*********************************************************************************************
		Table to hold Change calculations for the @rsiPeriod.
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
	ORDER BY [Date]

	/*********************************************************************************************
		Table to hold current and average gain/loss calculations for the @rsiPeriod
	*********************************************************************************************/
	DECLARE @currentGainLossCalculations TABLE
	(
		[RowCount] INT,
		[QuoteId] INT, 
		[Date] DATE,
		[CurrentGain] DECIMAL(12, 4), 
		[CurrentLoss] DECIMAL(12, 4) 
	);

	INSERT INTO @currentGainLossCalculations
	SELECT  ROW_NUMBER() OVER(ORDER BY QuoteId) as [RowCount],
			[QuoteId],
			[Date],
			CASE WHEN [Change] > 0 THEN [Change] ELSE 0 END as [CurrentGain],
			CASE WHEN [Change] < 0 THEN ABS([Change]) ELSE 0 END  as [CurrentLoss]
	FROM @changeCalcs changeOuter
	ORDER BY [Date]

	------------------

	DECLARE @avgGainLossCalculations TABLE
	(
		[RowCount] INT,
		[QuoteId] INT, 
		[Date] DATE,
		[CurrentGain] DECIMAL(12, 4),
		[AverageGain] DECIMAL(12, 4), 
		[CurrentLoss] DECIMAL(12, 4),
		[AverageLoss] DECIMAL(12, 4)
	);
	
	INSERT INTO @avgGainLossCalculations
	SELECT [RowCount],
			[QuoteId],
			[Date],
			[CurrentGain],
			CASE WHEN [RowCount] <= @rsiPeriod
			     THEN NULL
		   	     ELSE CASE WHEN [RowCount] = @rsiPeriod + 1
						   THEN (SELECT AVG(CurrentGain) 
								 FROM (SELECT [CurrentGain]
									   FROM @currentGainLossCalculations currGainLossCalcsInner
									   WHERE currGainLossCalcsInner.QuoteId <= currGainLossCalcsOuter.QuoteId AND currGainLossCalcsInner.QuoteId >= (currGainLossCalcsOuter.QuoteId - @rsiPeriod + 1)) as currentGainLossCalcsInnerInner)
						   ELSE 0  
						   END
			     END as [AverageGain],
			[CurrentLoss],
			CASE WHEN [RowCount] <= @rsiPeriod
			     THEN NULL
		   	     ELSE CASE WHEN [RowCount] = @rsiPeriod + 1
						   THEN (SELECT AVG(CurrentLoss) 
							     FROM (SELECT [CurrentLoss]
									   FROM @currentGainLossCalculations currGainLossCalcsInner
									   WHERE currGainLossCalcsInner.QuoteId <= currGainLossCalcsOuter.QuoteId AND currGainLossCalcsInner.QuoteId >= (currGainLossCalcsOuter.QuoteId - @rsiPeriod + 1)) as currentGainLossCalcsInnerInner)
						   ELSE NULL
						   END
			     END as [AverageLoss]
	FROM @currentGainLossCalculations currGainLossCalcsOuter
	ORDER BY [Date]

	-------------------

	-- Declare local variables for RS and RSI computation --
	DECLARE @row_number INT = @rsiPeriod + 2,
			@total_rows INT = (SELECT COUNT(quoteId) FROM (SELECT [QuoteId] FROM @avgGainLossCalculations) InnerCount),
			@avg_gain_prior DECIMAL(12, 4),
			@avg_loss_prior DECIMAL(12, 4),
			@current_gain DECIMAL(12, 4),
			@current_loss DECIMAL(12, 4)

	-- Update each row's average gain/loss values for RowCount > @rsiPeriod
	WHILE @row_number > @rsiPeriod AND @row_number <= @total_rows
	BEGIN
		SET @avg_gain_prior = (SELECT [AverageGain] FROM @avgGainLossCalculations WHERE [RowCount] = (@row_number - 1))
		SET @avg_loss_prior = (SELECT [AverageLoss] FROM @avgGainLossCalculations WHERE [RowCount] = (@row_number - 1))
		SET @current_gain = (SELECT [CurrentGain] FROM @avgGainLossCalculations WHERE [RowCount] = @row_number)
		SET @current_loss = (SELECT [CurrentLoss] FROM @avgGainLossCalculations WHERE [RowCount] = @row_number)

		UPDATE @avgGainLossCalculations
				SET [AverageGain] = (((@avg_gain_prior * (@rsiPeriod - 1)) + @current_gain) / @rsiPeriod),
				    [AverageLoss] = (((@avg_loss_prior * (@rsiPeriod - 1)) + @current_loss) / @rsiPeriod)
		WHERE [RowCount] = @row_number 	

		SET @row_number = @row_number + 1
	END

	/*********************************************************************************************
		Table to hold the RSI values over the @rsiPeriod for each quote. 
	*********************************************************************************************/
	SELECT changeCalcs.[QuoteId], 
	       changeCalcs.[CompanyId], 
	       changeCalcs.[Date], 
	    --   changeCalcs.[Close], 
	    --   changeCalcs.[Change], 
		   --avgGainLossCalcs.[CurrentGain],
		   --avgGainLossCalcs.[CurrentLoss],
	    --   avgGainLossCalcs.[AverageGain], 
	    --   avgGainLossCalcs.[AverageLoss], 
		   CASE WHEN [AverageLoss] <= 0
	       	    THEN 100.00
				ELSE 100.00 - (100.00 / (1.00 + ([AverageGain] / [AverageLoss])))
				END as [RSI]
	FROM @changeCalcs changeCalcs 
			INNER JOIN @avgGainLossCalculations avgGainLossCalcs on changeCalcs.QuoteId = avgGainLossCalcs.QuoteId
	WHERE CompanyId = @companyId AND changeCalcs.[Date] >= @startDate AND changeCalcs.[Date] <= @endDate
END
GO


