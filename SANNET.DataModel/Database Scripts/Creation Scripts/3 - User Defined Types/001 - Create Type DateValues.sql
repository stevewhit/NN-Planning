IF NOT EXISTS (SELECT 1 from sys.types WHERE is_table_type = 1 AND name = 'DateValues')
BEGIN
	PRINT 'Creating Type "DateValues"...'
	CREATE TYPE [dbo].[DateValues] AS TABLE(
		[Id] [int] UNIQUE NOT NULL,
		[Date] [date] UNIQUE NULL,
		[Value] [decimal](9, 3) NULL
	)
END
ELSE
	PRINT 'Type "DateValues" already exists...'






