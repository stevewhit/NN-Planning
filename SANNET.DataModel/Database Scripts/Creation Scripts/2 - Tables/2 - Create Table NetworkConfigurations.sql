IF NOT EXISTS (SELECT * FROM sys.tables WHERE NAME = 'NetworkConfigurations')
BEGIN
	PRINT 'Creating table "NetworkConfigurations"..'

	CREATE TABLE [dbo].[NetworkConfigurations](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[NumHiddenLayers] [int] NOT NULL,
	[NumHiddenLayerNeurons] [int] NOT NULL,
	 CONSTRAINT [PK_NetworkConfigurations] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

END
ELSE
	PRINT 'The table "NetworkConfigurations" already exists.'
