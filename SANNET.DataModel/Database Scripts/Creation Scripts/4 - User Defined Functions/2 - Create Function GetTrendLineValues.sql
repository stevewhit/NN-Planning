SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'TF' and name = 'GetTrendLineValues')
BEGIN
	PRINT 'Dropping "GetTrendLineValues" function...'
	DROP FUNCTION GetTrendLineValues
END
GO

PRINT 'Creating "GetTrendLineValues" function...'
GO

-- =============================================
-- Author:		Steve Whitmire Jr.
-- Create date: 09-16-2019
-- Description:	Returns the trendline values for each Id
-- =============================================
CREATE FUNCTION [dbo].[GetTrendLineValues] 
(
	@idValues IdValues READONLY
)
RETURNS @trendValues TABLE
(
	[Id] INT UNIQUE,
	[YTrend] DECIMAL(10, 4)
)
AS
BEGIN

	/**************************************************
		Table to hold @idValues but with row numbers
	***************************************************/
	DECLARE @numberedValues TABLE
	(
		[Id] INT UNIQUE,
		[X] INT,
		[Y] DECIMAL(10, 4)
	)

	INSERT INTO @numberedValues
	SELECT [Id],
		   ROW_NUMBER() OVER (ORDER BY [Id] ASC) as [X],
		   [Value] as [Y]
	FROM @idValues

	/***************************
		Trend calculations
	****************************/
	DECLARE @valuesCount INT = (SELECT COUNT(*) FROM @numberedValues)
	DECLARE @a DECIMAL(38, 3) = (SELECT @valuesCount * (SELECT SUM(CONVERT(DECIMAL(19, 4), [XY])) FROM (SELECT [X] * [Y] as [XY] FROM @numberedValues) as vals))
	DECLARE @b DECIMAL(38, 3) = (SELECT SUM([X]) * SUM([Y]) FROM (SELECT [X], [Y] FROM @numberedValues) as vals)
	DECLARE @c DECIMAL(38, 3) = @valuesCount * (SELECT SUM(CONVERT(BIGINT, xSquared)) FROM (SELECT [X] * [X] as [xSquared] FROM @numberedValues) as vals)
	DECLARE @d DECIMAL(38, 3) = (SELECT SUM(CONVERT(BIGINT, [X])) * SUM(CONVERT(BIGINT, [X])) FROM (SELECT [X] FROM @numberedValues) as vals)
	DECLARE @trendLineSlope DECIMAL(12, 4) = (@a - @b) / (@c - @d)
	DECLARE @e DECIMAL(38, 3) = (SELECT SUM([Y]) FROM (SELECT [Y] FROM @numberedValues) as vals)
	DECLARE @f DECIMAL(38, 3) = @trendLineSlope * (SELECT SUM([X]) FROM (SELECT [X] FROM @numberedValues) as vals)
	DECLARE @trendLineYIntercept DECIMAL(12, 4) = (@e - @f) / @valuesCount

	/*****************************************************
		Apply y = mx + b linear equation to each entry 
		in @idValues to get the Y value. 
	******************************************************/
	INSERT INTO @trendValues
	SELECT [Id], 
			(([X] * @trendLineSlope) + @trendLineYIntercept) as [YTrend]
	FROM @numberedValues
	ORDER BY [Id]

	RETURN;
END
GO
