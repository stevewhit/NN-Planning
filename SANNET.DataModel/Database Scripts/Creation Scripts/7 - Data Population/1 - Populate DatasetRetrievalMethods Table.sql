IF NOT EXISTS (SELECT * FROM DatasetRetrievalMethods WHERE [PerformanceRiseMultiplier] = 1.04 AND [PerformanceFallMultiplier] = .98 AND [TrainingDatasetStoredProcedure] = 'GetMethodDataset_1' AND [TestingDatasetStoredProcedure] = 'GetMethodDataset_1')
BEGIN
	PRINT 'Inserting DatasetRetrievalMethod (1.04, .98, GetMethodDataset_1, GetMethodDataset_1) into "DatasetRetrievalMethods" table..'

	INSERT INTO [dbo].[DatasetRetrievalMethods]
           ([PerformanceRiseMultiplier]
           ,[PerformanceFallMultiplier]
           ,[TrainingDatasetStoredProcedure]
           ,[TestingDatasetStoredProcedure])
     VALUES
           (1.04, .98, 'GetMethodDataset_1', 'GetMethodDataset_1')
END
ELSE
	PRINT 'DatasetRetrievalMethod (1.04, .98, GetMethodDataset_1, GetMethodDataset_1) already exists in the "DatasetRetrievalMethods" table..'