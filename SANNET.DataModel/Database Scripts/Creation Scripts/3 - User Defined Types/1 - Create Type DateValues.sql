IF NOT EXISTS (SELECT 1 from sys.types WHERE is_table_type = 1 AND name = 'DateValues')
BEGIN
	PRINT 'Creating Type "DateValues"...'
	CREATE TYPE [dbo].[DateValues] AS TABLE(
		[Id] [int] NOT NULL,
		[Date] [date] NULL,
		[Value] [decimal](12, 2) NULL
	)
END
ELSE
	PRINT 'Type "DateValues" already exists...'






