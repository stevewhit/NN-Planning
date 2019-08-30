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






