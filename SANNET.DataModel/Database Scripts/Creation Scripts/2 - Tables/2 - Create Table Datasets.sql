IF NOT EXISTS (SELECT * FROM sys.tables WHERE NAME = 'DatasetRetrievalMethods')
BEGIN
	PRINT 'Creating table "DatasetRetrievalMethods"..'

	CREATE TABLE [dbo].[DatasetRetrievalMethods](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PerformanceRiseMultiplier] [decimal](8, 4) NOT NULL,
	[PerformanceFallMultiplier] [decimal](8, 4) NOT NULL,
	[TrainingDatasetStoredProcedure] [nvarchar](50) NULL,
	[TestingDatasetStoredProcedure] [nvarchar](50) NULL,
	 CONSTRAINT [PK_DatasetRetrievalMethods] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[DatasetRetrievalMethods] ADD  CONSTRAINT [DF_DatasetRetrievalMethods_PerformanceRiseMultiplier]  DEFAULT ((1.04)) FOR [PerformanceRiseMultiplier]
	ALTER TABLE [dbo].[DatasetRetrievalMethods] ADD  CONSTRAINT [DF_DatasetRetrievalMethods_PerformanceFallMultiplier]  DEFAULT ((0.98)) FOR [PerformanceFallMultiplier]
END
ELSE
	PRINT 'The table "DatasetRetrievalMethods" already exists.'
