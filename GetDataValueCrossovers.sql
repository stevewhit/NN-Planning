SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'IF' and name = 'GetDateValueCrossovers')
BEGIN
	PRINT 'Dropping "GetDateValueCrossovers" function...'
	DROP FUNCTION GetDateValueCrossovers
END
GO

PRINT 'Creating "GetDateValueCrossovers" function...'
GO

-- =============================================
-- Author:		Steve Whitmire Jr.
-- Create date: 08-26-2019
-- Description:	Returns the following:
--					1. The number of consecutive days where Value1 is greater than Value2 for each Date.
--					2. The number of consecutive days where Value2 is greater than Value1 for each Date.
-- =============================================
CREATE FUNCTION [dbo].[GetDateValueCrossovers] 
(
	@valuesTable DateValueCrossoversType READONLY
)
RETURNS TABLE
AS
RETURN 
(
	SELECT
	[Id],
	CASE WHEN valuesOuter.Value1 < valuesOuter.Value2
			    THEN 0
				ELSE (SELECT COUNT(*)
					  FROM @valuesTable valuesInner 
					  WHERE valuesInner.[Date] < valuesOuter.[Date] AND 
							valuesInner.[Date] >= ISNULL((SELECT MAX(valuesInnerInner.[Date]) 
														  FROM @valuesTable valuesInnerInner
														  WHERE valuesInnerInner.[Date] < valuesOuter.[Date] AND valuesInnerInner.Value2 > valuesInnerInner.Value1), 
														 (SELECT MIN(valuesMin.[Date]) FROM @valuesTable valuesMin)))														  
				END as [ConsecutiveDaysValueOneAboveValueTwo],
	CASE WHEN valuesOuter.Value1 > valuesOuter.Value2
			    THEN 0
				ELSE (SELECT COUNT(*)
					  FROM @valuesTable valuesInner 
					  WHERE valuesInner.[Date] < valuesOuter.[Date] AND 
							valuesInner.[Date] >= ISNULL((SELECT MAX(valuesInnerInner.[Date]) 
														  FROM @valuesTable valuesInnerInner
														  WHERE valuesInnerInner.[Date] < valuesOuter.[Date] AND valuesInnerInner.Value2 < valuesInnerInner.Value1), 
														 (SELECT MIN(valuesMin.[Date]) FROM @valuesTable valuesMin)))
				END as [ConsecutiveDaysValueTwoAboveValueOne]
	FROM @valuesTable valuesOuter
)
GO
