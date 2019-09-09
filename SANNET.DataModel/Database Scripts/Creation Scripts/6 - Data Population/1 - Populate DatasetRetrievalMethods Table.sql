IF NOT EXISTS (SELECT * FROM DatasetRetrievalMethods WHERE [PerformanceRiseMultiplier] = 1.04 AND [PerformanceFallMultiplier] = .98 AND [TrainingDatasetStoredProcedure] = 'GetTrainingDataset1' AND [TestingDatasetStoredProcedure] = 'GetTestingDataset1')
BEGIN
	PRINT 'Inserting DatasetRetrievalMethod (1.04, .98, GetTrainingDataset1, GetTestingDataset1) into "DatasetRetrievalMethods" table..'

	INSERT INTO [dbo].[DatasetRetrievalMethods]
           ([PerformanceRiseMultiplier]
           ,[PerformanceFallMultiplier]
           ,[TrainingDatasetStoredProcedure]
           ,[TestingDatasetStoredProcedure])
     VALUES
           (1.04, .98, 'GetTrainingDataset1', 'GetTestingDataset1')
END
ELSE
	PRINT 'DatasetRetrievalMethod (1.04, .98, GetTrainingDataset1, GetTestingDataset1) already exists in the "DatasetRetrievalMethods" table..'