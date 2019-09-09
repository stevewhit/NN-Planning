IF NOT EXISTS (SELECT * FROM sys.tables WHERE NAME = 'NetworkConfigurations')
BEGIN
	PRINT 'Creating table "NetworkConfigurations"..'

	CREATE TABLE [dbo].[NetworkConfigurations](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DatasetRetrievalMethodId] [int] NOT NULL,
	[NumHiddenLayers] [int] NOT NULL,
	[NumHiddenLayerNeurons] [int] NOT NULL,
	[NumTrainingMonths] [int] NOT NULL,
	 CONSTRAINT [PK_NetworkConfigurations] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[NetworkConfigurations] WITH CHECK ADD  CONSTRAINT [FK_NetworkConfigurations_DatasetRetrievalMethods] FOREIGN KEY([DatasetRetrievalMethodId]) REFERENCES [dbo].[DatasetRetrievalMethods] ([Id])
	ALTER TABLE [dbo].[NetworkConfigurations] CHECK CONSTRAINT [FK_NetworkConfigurations_DatasetRetrievalMethods]
END
ELSE
	PRINT 'The table "NetworkConfigurations" already exists.'
