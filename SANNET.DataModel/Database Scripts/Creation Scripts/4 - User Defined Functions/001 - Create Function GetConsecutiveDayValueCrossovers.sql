SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'IF' and name = 'GetConsecutiveDayValueCrossovers')
BEGIN
	PRINT 'Dropping "GetConsecutiveDayValueCrossovers" function...'
	DROP FUNCTION GetConsecutiveDayValueCrossovers
END
GO

PRINT 'Creating "GetConsecutiveDayValueCrossovers" function...'
GO

-- =============================================
-- Author:		Steve Whitmire Jr.
-- Create date: 08-26-2019
-- Description:	Returns the following:
--					1. The number of consecutive days where Value1 is greater than Value2 for each Date.
--					2. The number of consecutive days where Value2 is greater than Value1 for each Date.
-- =============================================
CREATE FUNCTION [dbo].[GetConsecutiveDayValueCrossovers] 
(
	@dateValuesFirst DateValues READONLY,
	@dateValuesSecond DateValues READONLY
)
RETURNS TABLE
AS
RETURN 
(
	SELECT valuesFirstOuter.[Id],
		   valuesFirstOuter.[Date],
		   valuesFirstOuter.Value as V1,
		   ValuesSecondOuter.Value as V2,
			CASE WHEN valuesFirstOuter.Value < valuesSecondOuter.Value
						THEN 0
						ELSE (SELECT COUNT(*)
							  FROM @dateValuesFirst valuesFirstInner
							  WHERE valuesFirstInner.[Date] < valuesFirstOuter.[Date] AND 
									valuesFirstInner.[Date] >= ISNULL((SELECT MAX(valuesFirstInnerInner.[Date]) 
																  FROM @dateValuesFirst valuesFirstInnerInner INNER JOIN @dateValuesSecond valuesSecondInnerInner ON valuesFirstInnerInner.Date = valuesSecondInnerInner.Date
																  WHERE valuesFirstInnerInner.[Date] < valuesFirstOuter.[Date] AND valuesSecondInnerInner.Value > valuesFirstInnerInner.Value), 
																 (SELECT MIN(valuesFirstMin.[Date]) FROM @dateValuesSecond valuesFirstMin)))														  
						END as [ConsecutiveDaysValueOneAboveValueTwo],
			CASE WHEN valuesFirstOuter.Value > valuesSecondOuter.Value
						THEN 0
						ELSE (SELECT COUNT(*)
							  FROM @dateValuesFirst valuesFirstInner
							  WHERE valuesFirstInner.[Date] < valuesFirstOuter.[Date] AND 
									valuesFirstInner.[Date] >= ISNULL((SELECT MAX(valuesFirstInnerInner.[Date]) 
																  FROM @dateValuesFirst valuesFirstInnerInner INNER JOIN @dateValuesSecond valuesSecondInnerInner ON valuesFirstInnerInner.Date = valuesSecondInnerInner.Date
																  WHERE valuesFirstInnerInner.[Date] < valuesFirstOuter.[Date] AND valuesSecondInnerInner.Value < valuesFirstInnerInner.Value), 
																 (SELECT MIN(valuesFirstMin.[Date]) FROM @dateValuesSecond valuesFirstMin)))	
						END as [ConsecutiveDaysValueTwoAboveValueOne]
	FROM @dateValuesFirst valuesFirstOuter INNER JOIN @dateValuesSecond valuesSecondOuter ON valuesFirstOuter.Date = valuesSecondOuter.Date
)
GO