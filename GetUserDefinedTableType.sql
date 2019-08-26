USE Test

-- Create user-defined type for ??table value crossover??
IF NOT EXISTS (SELECT 1 from sys.types WHERE is_table_type = 1 AND name = 'DateValueCrossoversType')
BEGIN
	PRINT 'Creating Type "DateValueCrossoversType"...'
	CREATE TYPE DateValueCrossoversType AS TABLE
	(
		[Id] INT NOT NULL,
		[Date] DATE NOT NULL,
		[Value1] DECIMAL(12, 4) NOT NULL,
		[Value2] DECIMAL(12, 4) NOT NULL
	)
END
ELSE
	PRINT 'Type "DateValueCrossoversType" already exists...'
GO

DECLARE @rsis DateValueCrossoversType
INSERT INTO @rsis(Id, [Date], Value1, Value2) VALUES (0, '01-01-2018', 2, 1), (1, '01-02-2018', 1, 3), (2, '01-03-2018', 2, 3), (3, '01-04-2018', 3, 2), (4, '01-05-2018', 3, 2), (5, '01-06-2018', 3, 2), (6, '01-07-2018', 3, 2)

SELECT * FROM @rsis

SELECT * from [GetDateValueCrossovers](@rsis)






