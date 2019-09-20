IF NOT EXISTS (SELECT 1 from sys.types WHERE is_table_type = 1 AND name = 'IdValues')
BEGIN
	PRINT 'Creating Type "IdValues"...'
	CREATE TYPE [dbo].[IdValues] AS TABLE(
		[Id] [int] UNIQUE NOT NULL,
		[Value] [decimal](9, 3) NULL
	)
END
ELSE
	PRINT 'Type "IdValues" already exists...'






