IF NOT EXISTS (SELECT * FROM sys.tables WHERE NAME = 'Predictions')
BEGIN
	PRINT 'Creating table "Predictions"..'

	CREATE TABLE [dbo].[Predictions](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CompanySymbol] [nvarchar](20) NOT NULL,
	[Date] [date] NOT NULL,
	[TrainingStartDate] [date] NOT NULL,
	[TrainingEndDate] [date] NOT NULL,
	[TrainingParameters] [nvarchar](250) NULL,
	[Prediction] [nvarchar](100) NOT NULL,
	[Confidence] [decimal](5, 2) NOT NULL,
	[Outcome] [nvarchar](100) NULL,
	 CONSTRAINT [PK_Predictions] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]
END
ELSE
	PRINT 'The table "Predictions" already exists.'
