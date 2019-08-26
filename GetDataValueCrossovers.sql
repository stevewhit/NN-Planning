USE [Test]
GO
/****** Object:  UserDefinedFunction [dbo].[GetDateValueCrossoversType]    Script Date: 8/26/2019 10:59:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER FUNCTION [dbo].[GetDateValueCrossovers] (@valuesTable DateValueCrossoversType READONLY)
RETURNS TABLE
AS
RETURN
(
	/****************************************************************************************
	Returns the following:
		1. The number of consecutive days where Value1 is greater than Value2 for each Date.
		2. The number of consecutive days where Value2 is greater than Value1 for each Date.
	****************************************************************************************/
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
