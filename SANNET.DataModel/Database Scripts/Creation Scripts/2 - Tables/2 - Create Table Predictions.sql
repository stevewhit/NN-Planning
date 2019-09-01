IF NOT EXISTS (SELECT * FROM sys.tables WHERE NAME = 'Predictions')
BEGIN
	PRINT 'Creating table "Predictions"..'

	CREATE TABLE [dbo].[Predictions](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ConfigurationId] [int] NOT NULL,
	[CompanyId] INT NOT NULL,
	[QuoteId] INT NOT NULL,
	[TrainingStartDate] [date] NOT NULL,
	[TrainingEndDate] [date] NOT NULL,
	[PredictedOutcome] [nvarchar](100) NOT NULL,
	[PredictionConfidence] [decimal](5, 2) NOT NULL,
	[Outcome] [nvarchar](100) NULL,
	 CONSTRAINT [PK_Predictions] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]
	
	ALTER TABLE [dbo].[Predictions] WITH CHECK ADD  CONSTRAINT [FK_Predictions_NetworkConfigurations] FOREIGN KEY([ConfigurationId]) REFERENCES [dbo].[NetworkConfigurations] ([Id])
	ALTER TABLE [dbo].[Predictions] CHECK CONSTRAINT [FK_Predictions_NetworkConfigurations]
END
ELSE
	PRINT 'The table "Predictions" already exists.'
